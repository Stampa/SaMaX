namespace SamaxLibrary.Sid
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    /// <summary>
    /// Defines the SID message types.
    /// </summary>
    public enum SidMessageType
    {
        /// <summary>
        /// The type corresponding to SID_PING messages.
        /// </summary>
        Ping = 0x25,

        /// <summary>
        /// The type corresponding to SID_LOGONRESPONSE2 messages.
        /// </summary>
        LogonResponse2 = 0x3a,

        /// <summary>
        /// The type corresponding to SID_LOGONREALMEX messages.
        /// </summary>
        LogonRealmEx = 0x3e,

        /// <summary>
        /// The type corresponding to SID_QUERYREALMS2 messages.
        /// </summary>
        QueryRealms2 = 0x40,

        /// <summary>
        /// The type corresponding to SID_AUTH_INFO messages.
        /// </summary>
        AuthInfo = 0x50,

        /// <summary>
        /// The type corresponding to SID_AUTH_CHECK messages.
        /// </summary>
        AuthCheck = 0x51
    }
}
