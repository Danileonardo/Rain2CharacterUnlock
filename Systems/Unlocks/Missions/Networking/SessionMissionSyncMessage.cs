using R2API.Networking.Interfaces;
using UnityEngine.Networking;

namespace UniversalSurvivorUnlocks
{
    /*
     * =============================================================
     * HOST -> CLIENTES
     * SNAPSHOT CHUNK
     * =============================================================
     *
     * Antes este mensaje intentaba transportar el snapshot completo.
     *
     * En multiplayer real encontramos:
     *
     * Snapshot:
     *     ~5950 bytes
     *
     * Límite del canal:
     *     1100 bytes
     *
     * Ahora CADA instancia de este mensaje representa únicamente
     * un fragmento seguro del snapshot.
     *
     * El reensamblado vive en:
     *
     * SessionMissionChunkTransport
     *
     * El cliente NUNCA escribe estos datos en Survivors.json.
     * =============================================================
     */
    public sealed class SessionMissionSyncMessage :
        INetMessage
    {
        public bool IsRunSnapshot;

        // Identifica el proceso/sesión de transporte del host.
        public string HostTransportSessionId;

        // Orden lógico de snapshots producidos por ese host.
        public int SnapshotSequence;

        // Identificador único de ESTE snapshot.
        public string SnapshotId;

        // Índice 0-based del fragmento.
        public int ChunkIndex;

        // Cantidad total de fragmentos.
        public int ChunkCount;

        // Longitud real del JSON completo codificado como UTF-8.
        public int TotalByteLength;

        // Fragmento binario convertido a Base64.
        public string ChunkBase64;


        public SessionMissionSyncMessage()
        {
        }


        public SessionMissionSyncMessage(
            bool isRunSnapshot,
            string hostTransportSessionId,
            int snapshotSequence,
            string snapshotId,
            int chunkIndex,
            int chunkCount,
            int totalByteLength,
            string chunkBase64
        )
        {
            IsRunSnapshot =
                isRunSnapshot;

            HostTransportSessionId =
                hostTransportSessionId ?? "";

            SnapshotSequence =
                snapshotSequence;

            SnapshotId =
                snapshotId ?? "";

            ChunkIndex =
                chunkIndex;

            ChunkCount =
                chunkCount;

            TotalByteLength =
                totalByteLength;

            ChunkBase64 =
                chunkBase64 ?? "";
        }


        public void Serialize(
            NetworkWriter writer
        )
        {
            writer.Write(
                IsRunSnapshot
            );

            writer.Write(
                HostTransportSessionId ?? ""
            );

            writer.Write(
                SnapshotSequence
            );

            writer.Write(
                SnapshotId ?? ""
            );

            writer.Write(
                ChunkIndex
            );

            writer.Write(
                ChunkCount
            );

            writer.Write(
                TotalByteLength
            );

            writer.Write(
                ChunkBase64 ?? ""
            );
        }


        public void Deserialize(
            NetworkReader reader
        )
        {
            IsRunSnapshot =
                reader.ReadBoolean();

            HostTransportSessionId =
                reader.ReadString();

            SnapshotSequence =
                reader.ReadInt32();

            SnapshotId =
                reader.ReadString();

            ChunkIndex =
                reader.ReadInt32();

            ChunkCount =
                reader.ReadInt32();

            TotalByteLength =
                reader.ReadInt32();

            ChunkBase64 =
                reader.ReadString();
        }


        public void OnReceived()
        {
            /*
             * Este flujo es host -> cliente.
             *
             * El host nunca debe aplicar sus propios fragmentos.
             */
            if (NetworkServer.active)
            {
                return;
            }


            SessionMissionChunkTransport
                .ReceiveChunk(
                    this
                );
        }
    }
}
