using System;
using System.Reflection;
using RoR2;

namespace UniversalSurvivorUnlocks
{
    /*
     * =============================================================
     * IDENTIDAD DE JUGADOR PARA PROGRESO DE MISIÓN
     * =============================================================
     *
     * NO almacenamos NetworkUser, CharacterMaster ni CharacterBody
     * como clave permanente del progreso.
     *
     * Esos objetos pueden desaparecer al:
     * - morir,
     * - cambiar de cuerpo,
     * - desconectarse.
     *
     * Intentamos obtener un identificador estable del NetworkUser
     * mediante reflexión para no depender de una propiedad concreta
     * de una versión específica de RoR2.
     *
     * Si no existe un ID aprovechable:
     *     usamos nickname como fallback.
     *
     * El MissionProgressRegistry además puede recuperar progreso
     * por nickname cuando la identidad estable cambie al reconectar,
     * siempre que no exista ambigüedad.
     * =============================================================
     */
    public sealed class MissionPlayerIdentity
    {
        private static readonly string[]
            StableIdMemberCandidates =
            {
                "platformUserId",
                "PlatformUserId",
                "steamId",
                "SteamId",
                "steamId64",
                "SteamId64",
                "userId",
                "UserId",
                "networkUserId",
                "NetworkUserId",
                "id",
                "Id"
            };


        public string Key
        {
            get;
            private set;
        }


        public string StableId
        {
            get;
            private set;
        }


        public string Nickname
        {
            get;
            private set;
        }


        public string IdentitySource
        {
            get;
            private set;
        }


        public bool HasStableId =>
            !string.IsNullOrWhiteSpace(
                StableId
            );


        private MissionPlayerIdentity()
        {
        }


        // =========================================================
        // NETWORK USER
        // =========================================================

        public static bool TryCreate(
            NetworkUser networkUser,
            out MissionPlayerIdentity identity
        )
        {
            identity =
                null;


            if (networkUser == null)
            {
                return false;
            }


            string nickname =
                networkUser.userName ?? "";


            string stableId =
                TryResolveStableId(
                    networkUser,
                    out string source
                );


            if (
                !string.IsNullOrWhiteSpace(
                    stableId
                )
            )
            {
                identity =
                    new MissionPlayerIdentity
                    {
                        StableId =
                            stableId,

                        Nickname =
                            nickname,

                        IdentitySource =
                            source,

                        Key =
                            "id:" +
                            NormalizeKeyPart(
                                stableId
                            )
                    };


                return true;
            }


            if (
                string.IsNullOrWhiteSpace(
                    nickname
                )
            )
            {
                return false;
            }


            identity =
                new MissionPlayerIdentity
                {
                    StableId =
                        "",

                    Nickname =
                        nickname,

                    IdentitySource =
                        "NicknameFallback",

                    Key =
                        "nick:" +
                        NormalizeKeyPart(
                            nickname
                        )
                };


            return true;
        }


        // =========================================================
        // CHARACTER MASTER
        // =========================================================

        public static bool TryCreate(
            CharacterMaster master,
            out MissionPlayerIdentity identity
        )
        {
            identity =
                null;


            NetworkUser networkUser =
                GetNetworkUser(
                    master
                );


            return TryCreate(
                networkUser,
                out identity
            );
        }


        // =========================================================
        // CHARACTER BODY
        // =========================================================

        public static bool TryCreate(
            CharacterBody body,
            out MissionPlayerIdentity identity
        )
        {
            identity =
                null;


            if (
                body == null ||
                body.master == null
            )
            {
                return false;
            }


            return TryCreate(
                body.master,
                out identity
            );
        }


        // =========================================================
        // OBTENER NETWORK USER
        // =========================================================

        public static NetworkUser GetNetworkUser(
            CharacterMaster master
        )
        {
            if (
                master == null ||
                master
                    .playerCharacterMasterController == null
            )
            {
                return null;
            }


            return master
                .playerCharacterMasterController
                .networkUser;
        }


        // =========================================================
        // ID ESTABLE POR REFLEXIÓN
        // =========================================================

        private static string TryResolveStableId(
            NetworkUser networkUser,
            out string source
        )
        {
            source =
                "";


            if (networkUser == null)
            {
                return "";
            }


            Type type =
                networkUser.GetType();


            const BindingFlags flags =
                BindingFlags.Instance |
                BindingFlags.Public |
                BindingFlags.NonPublic;


            foreach (
                string candidate
                in StableIdMemberCandidates
            )
            {
                // ---------------------------------------------
                // PROPIEDAD
                // ---------------------------------------------
                PropertyInfo property =
                    type.GetProperty(
                        candidate,
                        flags
                    );


                if (
                    property != null &&
                    property.GetIndexParameters().Length == 0
                )
                {
                    try
                    {
                        object value =
                            property.GetValue(
                                networkUser,
                                null
                            );


                        string text =
                            ConvertIdentityValueToString(
                                value
                            );


                        if (
                            IsUsableIdentityValue(
                                text
                            )
                        )
                        {
                            source =
                                "Property:" +
                                candidate;


                            return text;
                        }
                    }
                    catch
                    {
                        /*
                         * Seguimos con el siguiente candidato.
                         * Resolver identidad nunca debe romper una run.
                         */
                    }
                }


                // ---------------------------------------------
                // CAMPO
                // ---------------------------------------------
                FieldInfo field =
                    type.GetField(
                        candidate,
                        flags
                    );


                if (field != null)
                {
                    try
                    {
                        object value =
                            field.GetValue(
                                networkUser
                            );


                        string text =
                            ConvertIdentityValueToString(
                                value
                            );


                        if (
                            IsUsableIdentityValue(
                                text
                            )
                        )
                        {
                            source =
                                "Field:" +
                                candidate;


                            return text;
                        }
                    }
                    catch
                    {
                    }
                }
            }


            return "";
        }


        private static string
            ConvertIdentityValueToString(
                object value
            )
        {
            if (value == null)
            {
                return "";
            }


            string text =
                value.ToString();


            return
                text != null
                    ? text.Trim()
                    : "";
        }


        private static bool IsUsableIdentityValue(
            string value
        )
        {
            if (
                string.IsNullOrWhiteSpace(
                    value
                )
            )
            {
                return false;
            }


            if (
                value == "0" ||
                value == "null" ||
                value == "None"
            )
            {
                return false;
            }


            return true;
        }


        public static string NormalizeNickname(
            string nickname
        )
        {
            return NormalizeKeyPart(
                nickname
            );
        }


        private static string NormalizeKeyPart(
            string value
        )
        {
            if (value == null)
            {
                return "";
            }


            return value
                .Trim()
                .ToLowerInvariant();
        }


        // =========================================================
        // COPIA
        // =========================================================

        public MissionPlayerIdentity Clone()
        {
            return new MissionPlayerIdentity
            {
                Key =
                    Key,

                StableId =
                    StableId,

                Nickname =
                    Nickname,

                IdentitySource =
                    IdentitySource
            };
        }
    }
}
