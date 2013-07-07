namespace SamaxLibrary
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    /// <summary>
    /// Defines the direction of a message, which is either from the client to the server or from
    /// the server to the client.
    /// </summary>
    public enum MessageDirection
    {
        /// <summary>
        /// The direction from the client to the server.
        /// </summary>
        ClientToServer,

        /// <summary>
        /// The direction from the server to the client.
        /// </summary>
        ServerToClient
    }
}
