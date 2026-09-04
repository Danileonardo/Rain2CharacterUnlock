using System;
using System.Collections.Generic;
using System.Text;

using R2API.Networking;
using R2API.Networking.Interfaces;

using UnityEngine.Networking;


namespace UniversalSurvivorUnlocks
{
    /*
     * =============================================================
     * SESSION MISSION CHUNK TRANSPORT
     * =============================================================
     *
     * Transporte seguro para:
     *
     * - LobbySnapshot
     * - RunSnapshot
     *
     * MECÁNICA
     * --------
     *
     * HOST:
     *
     * JSON
     *   ↓ UTF-8
     * byte[]
     *   ↓
     * chunks de 512 bytes
     *   ↓ Base64
     * SessionMissionSyncMessage x N
     *
     *
     * CLIENTE:
     *
     * chunks
     *   ↓
     * reensamblado por SnapshotId
     *   ↓
     * byte[]
     *   ↓ UTF-8
     * JSON
     *   ↓
     * SessionMissionRegistry.ReceiveHostSnapshot(...)
     *
     *
     * POR QUÉ BASE64
     * --------------
     *
     * El mensaje de R2API ya sabe transportar strings.
     *
     * Base64 permite cortar el byte[] UTF-8 sin riesgo de:
     *
     * - romper caracteres acentuados,
     * - cortar pares surrogate,
     * - depender de cuántos bytes ocupa cada carácter.
     *
     *
     * TAMAÑO
     * ------
     *
     * Raw chunk:
     *     512 bytes
     *
     * Base64:
     *     ~684 bytes
     *
     * Incluso sumando metadata queda ampliamente por debajo
     * del límite observado de 1100 bytes del canal.
     * =============================================================
     */
    public static class SessionMissionChunkTransport
    {
        // ---------------------------------------------------------
        // LÍMITES
        // ---------------------------------------------------------

        private const int ChunkByteSize =
            512;


        private const int MaxSnapshotBytes =
            1024 * 1024;


        private const int MaxChunkCount =
            2048;


        private const int MaxPendingSnapshots =
            8;


        // ---------------------------------------------------------
        // HOST
        // ---------------------------------------------------------

        private static readonly string
            HostTransportSessionId =
                Guid.NewGuid()
                    .ToString(
                        "N"
                    );


        private static int
            nextSnapshotSequence;


        // ---------------------------------------------------------
        // CLIENTE
        // ---------------------------------------------------------

        private static string
            activeRemoteHostTransportSessionId =
                "";


        private static int
            lastAppliedLobbySequence;


        private static int
            lastAppliedRunSequence;


        private static readonly Dictionary<
            string,
            PendingSnapshot
        > PendingSnapshots =
            new Dictionary<
                string,
                PendingSnapshot
            >(
                StringComparer.Ordinal
            );


        // =========================================================
        // HOST SEND
        // =========================================================

        public static int SendSnapshotToClients(
            string snapshotJson,
            bool isRunSnapshot
        )
        {
            if (!NetworkServer.active)
            {
                return 0;
            }


            string safeJson =
                snapshotJson ?? "";


            byte[] payload =
                Encoding.UTF8
                    .GetBytes(
                        safeJson
                    );


            if (
                payload.Length >
                MaxSnapshotBytes
            )
            {
                throw new InvalidOperationException(
                    "Mission snapshot excede el límite interno de " +
                    $"{MaxSnapshotBytes} bytes."
                );
            }


            int chunkCount =
                Math.Max(
                    1,
                    (
                        payload.Length +
                        ChunkByteSize -
                        1
                    ) /
                    ChunkByteSize
                );


            if (
                chunkCount >
                MaxChunkCount
            )
            {
                throw new InvalidOperationException(
                    "Mission snapshot requiere demasiados fragmentos: " +
                    chunkCount
                );
            }


            int sequence =
                unchecked(
                    ++nextSnapshotSequence
                );


            if (sequence <= 0)
            {
                nextSnapshotSequence =
                    1;

                sequence =
                    1;
            }


            string snapshotId =
                Guid.NewGuid()
                    .ToString(
                        "N"
                    );


            for (
                int chunkIndex = 0;
                chunkIndex < chunkCount;
                chunkIndex++
            )
            {
                int offset =
                    chunkIndex *
                    ChunkByteSize;


                int remaining =
                    payload.Length -
                    offset;


                int length =
                    Math.Max(
                        0,
                        Math.Min(
                            ChunkByteSize,
                            remaining
                        )
                    );


                byte[] chunk =
                    new byte[
                        length
                    ];


                if (length > 0)
                {
                    Buffer.BlockCopy(
                        payload,
                        offset,
                        chunk,
                        0,
                        length
                    );
                }


                string chunkBase64 =
                    Convert.ToBase64String(
                        chunk
                    );


                new SessionMissionSyncMessage(
                    isRunSnapshot,
                    HostTransportSessionId,
                    sequence,
                    snapshotId,
                    chunkIndex,
                    chunkCount,
                    payload.Length,
                    chunkBase64
                )
                .Send(
                    NetworkDestination.Clients
                );
            }


            return chunkCount;
        }


        // =========================================================
        // CLIENT RECEIVE
        // =========================================================

        public static void ReceiveChunk(
            SessionMissionSyncMessage message
        )
        {
            if (
                message == null ||
                NetworkServer.active
            )
            {
                return;
            }


            if (
                string.IsNullOrWhiteSpace(
                    message.HostTransportSessionId
                ) ||
                string.IsNullOrWhiteSpace(
                    message.SnapshotId
                )
            )
            {
                return;
            }


            if (
                message.ChunkCount <= 0 ||
                message.ChunkCount >
                    MaxChunkCount ||
                message.ChunkIndex < 0 ||
                message.ChunkIndex >=
                    message.ChunkCount ||
                message.TotalByteLength < 0 ||
                message.TotalByteLength >
                    MaxSnapshotBytes
            )
            {
                return;
            }


            // -----------------------------------------------------
            // NUEVO HOST / NUEVA SESIÓN DE TRANSPORTE
            // -----------------------------------------------------
            //
            // Esto permite:
            //
            // Cliente entra al Host A
            // ↓
            // desconecta
            // ↓
            // entra al Host B sin reiniciar el juego
            //
            // El Sequence del Host B puede volver a empezar desde 1.
            // -----------------------------------------------------

            if (
                !string.Equals(
                    activeRemoteHostTransportSessionId,
                    message.HostTransportSessionId,
                    StringComparison.Ordinal
                )
            )
            {
                activeRemoteHostTransportSessionId =
                    message.HostTransportSessionId;

                lastAppliedLobbySequence =
                    0;

                lastAppliedRunSequence =
                    0;

                PendingSnapshots.Clear();
            }


            // -----------------------------------------------------
            // SNAPSHOT YA SUPERADO
            // -----------------------------------------------------

            int lastAppliedSequence =
                message.IsRunSnapshot
                    ? lastAppliedRunSequence
                    : lastAppliedLobbySequence;


            if (
                message.SnapshotSequence <=
                    lastAppliedSequence
            )
            {
                return;
            }


            // -----------------------------------------------------
            // PENDING
            // -----------------------------------------------------

            if (
                !PendingSnapshots
                    .TryGetValue(
                        message.SnapshotId,
                        out PendingSnapshot pending
                    )
            )
            {
                TrimPendingSnapshotsIfNeeded();


                pending =
                    new PendingSnapshot(
                        message.HostTransportSessionId,
                        message.SnapshotSequence,
                        message.SnapshotId,
                        message.IsRunSnapshot,
                        message.ChunkCount,
                        message.TotalByteLength
                    );


                PendingSnapshots[
                    message.SnapshotId
                ] =
                    pending;
            }


            // -----------------------------------------------------
            // METADATA CONSISTENCY
            // -----------------------------------------------------

            if (
                !pending.Matches(
                    message
                )
            )
            {
                PendingSnapshots.Remove(
                    message.SnapshotId
                );

                return;
            }


            // -----------------------------------------------------
            // CHUNK DATA
            // -----------------------------------------------------

            string incomingChunk =
                message.ChunkBase64 ?? "";


            string existingChunk =
                pending.Chunks[
                    message.ChunkIndex
                ];


            if (existingChunk == null)
            {
                pending.Chunks[
                    message.ChunkIndex
                ] =
                    incomingChunk;

                pending.ReceivedCount++;
            }
            else if (
                !string.Equals(
                    existingChunk,
                    incomingChunk,
                    StringComparison.Ordinal
                )
            )
            {
                /*
                 * Mismo SnapshotId + ChunkIndex con contenido
                 * diferente = transporte inconsistente.
                 */
                PendingSnapshots.Remove(
                    message.SnapshotId
                );

                return;
            }


            if (
                pending.ReceivedCount <
                    pending.ChunkCount
            )
            {
                return;
            }


            // -----------------------------------------------------
            // REASSEMBLE
            // -----------------------------------------------------

            if (
                !TryReassemble(
                    pending,
                    out string json
                )
            )
            {
                PendingSnapshots.Remove(
                    message.SnapshotId
                );

                return;
            }


            PendingSnapshots.Remove(
                message.SnapshotId
            );


            /*
             * Mientras llegaban los chunks pudo haberse aplicado
             * una versión más nueva.
             */
            lastAppliedSequence =
                pending.IsRunSnapshot
                    ? lastAppliedRunSequence
                    : lastAppliedLobbySequence;


            if (
                pending.Sequence <=
                    lastAppliedSequence
            )
            {
                return;
            }


            // -----------------------------------------------------
            // APPLY
            // -----------------------------------------------------

            SessionMissionRegistry
                .ReceiveHostSnapshot(
                    json,
                    pending.IsRunSnapshot
                );


            if (pending.IsRunSnapshot)
            {
                lastAppliedRunSequence =
                    pending.Sequence;
            }
            else
            {
                lastAppliedLobbySequence =
                    pending.Sequence;
            }


            RemoveOlderPendingSnapshots(
                pending
            );
        }


        // =========================================================
        // REASSEMBLY
        // =========================================================

        private static bool TryReassemble(
            PendingSnapshot pending,
            out string json
        )
        {
            json =
                "";


            if (
                pending == null ||
                pending.ReceivedCount !=
                    pending.ChunkCount
            )
            {
                return false;
            }


            try
            {
                byte[] payload =
                    new byte[
                        pending.TotalByteLength
                    ];


                int writeOffset =
                    0;


                for (
                    int chunkIndex = 0;
                    chunkIndex <
                        pending.ChunkCount;
                    chunkIndex++
                )
                {
                    string encoded =
                        pending.Chunks[
                            chunkIndex
                        ];


                    if (encoded == null)
                    {
                        return false;
                    }


                    byte[] chunk =
                        Convert.FromBase64String(
                            encoded
                        );


                    if (
                        writeOffset +
                        chunk.Length >
                        payload.Length
                    )
                    {
                        return false;
                    }


                    if (chunk.Length > 0)
                    {
                        Buffer.BlockCopy(
                            chunk,
                            0,
                            payload,
                            writeOffset,
                            chunk.Length
                        );
                    }


                    writeOffset +=
                        chunk.Length;
                }


                if (
                    writeOffset !=
                        payload.Length
                )
                {
                    return false;
                }


                json =
                    Encoding.UTF8
                        .GetString(
                            payload
                        );


                return true;
            }
            catch
            {
                json =
                    "";

                return false;
            }
        }


        // =========================================================
        // CLEANUP
        // =========================================================

        private static void TrimPendingSnapshotsIfNeeded()
        {
            while (
                PendingSnapshots.Count >=
                    MaxPendingSnapshots
            )
            {
                string oldestKey =
                    null;

                int oldestSequence =
                    int.MaxValue;


                foreach (
                    KeyValuePair<
                        string,
                        PendingSnapshot
                    > pair
                    in PendingSnapshots
                )
                {
                    if (
                        pair.Value == null
                    )
                    {
                        oldestKey =
                            pair.Key;

                        break;
                    }


                    if (
                        pair.Value.Sequence <
                            oldestSequence
                    )
                    {
                        oldestSequence =
                            pair.Value.Sequence;

                        oldestKey =
                            pair.Key;
                    }
                }


                if (
                    string.IsNullOrWhiteSpace(
                        oldestKey
                    )
                )
                {
                    PendingSnapshots.Clear();

                    return;
                }


                PendingSnapshots.Remove(
                    oldestKey
                );
            }
        }


        private static void RemoveOlderPendingSnapshots(
            PendingSnapshot applied
        )
        {
            if (applied == null)
            {
                return;
            }


            List<string> remove =
                new List<string>();


            foreach (
                KeyValuePair<
                    string,
                    PendingSnapshot
                > pair
                in PendingSnapshots
            )
            {
                PendingSnapshot candidate =
                    pair.Value;


                if (candidate == null)
                {
                    remove.Add(
                        pair.Key
                    );

                    continue;
                }


                if (
                    candidate.IsRunSnapshot ==
                        applied.IsRunSnapshot &&
                    string.Equals(
                        candidate.HostTransportSessionId,
                        applied.HostTransportSessionId,
                        StringComparison.Ordinal
                    ) &&
                    candidate.Sequence <=
                        applied.Sequence
                )
                {
                    remove.Add(
                        pair.Key
                    );
                }
            }


            for (
                int i = 0;
                i < remove.Count;
                i++
            )
            {
                PendingSnapshots.Remove(
                    remove[i]
                );
            }
        }


        // =========================================================
        // PENDING MODEL
        // =========================================================

        private sealed class PendingSnapshot
        {
            public readonly string
                HostTransportSessionId;


            public readonly int
                Sequence;


            public readonly string
                SnapshotId;


            public readonly bool
                IsRunSnapshot;


            public readonly int
                ChunkCount;


            public readonly int
                TotalByteLength;


            public readonly string[]
                Chunks;


            public int
                ReceivedCount;


            public PendingSnapshot(
                string hostTransportSessionId,
                int sequence,
                string snapshotId,
                bool isRunSnapshot,
                int chunkCount,
                int totalByteLength
            )
            {
                HostTransportSessionId =
                    hostTransportSessionId ?? "";

                Sequence =
                    sequence;

                SnapshotId =
                    snapshotId ?? "";

                IsRunSnapshot =
                    isRunSnapshot;

                ChunkCount =
                    chunkCount;

                TotalByteLength =
                    totalByteLength;

                Chunks =
                    new string[
                        chunkCount
                    ];
            }


            public bool Matches(
                SessionMissionSyncMessage message
            )
            {
                if (message == null)
                {
                    return false;
                }


                return
                    string.Equals(
                        HostTransportSessionId,
                        message.HostTransportSessionId,
                        StringComparison.Ordinal
                    ) &&
                    Sequence ==
                        message.SnapshotSequence &&
                    string.Equals(
                        SnapshotId,
                        message.SnapshotId,
                        StringComparison.Ordinal
                    ) &&
                    IsRunSnapshot ==
                        message.IsRunSnapshot &&
                    ChunkCount ==
                        message.ChunkCount &&
                    TotalByteLength ==
                        message.TotalByteLength;
            }
        }
    }
}
