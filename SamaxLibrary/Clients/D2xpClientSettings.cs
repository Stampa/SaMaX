namespace SamaxLibrary.Clients
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Text;
    using System.Threading.Tasks;

    /// <summary>
    /// This class contains all the settings necessary for the <see cref="D2xpSidClient"/> class.
    /// </summary>
    public class D2xpClientSettings
    {
        /// <summary>
        /// Gets the IP address of the SID server to which to connect.
        /// </summary>
        public IPAddress IPAddress { get; private set; }

        /// <summary>
        /// Gets the port of the SID server to which to connect.
        /// </summary>
        public int Port { get; private set; }

        /// <summary>
        /// Gets the version.
        /// </summary>
        /// <remarks>0xD was a valid version in 2013.</remarks>
        /// TODO: Clarify the version of what!
        public int Version { get; private set; }

        /// <summary>
        /// Gets the local IP address of the client.
        /// </summary>
        public IPAddress LocalIPAddress { get; private set; }

        /// <summary>
        /// Gets the file triple for D2XP.
        /// </summary>
        public D2xpFileTriple FileTriple { get; private set; }

        /// <summary>
        /// Gets the first (non-expansion) CD key.
        /// </summary>
        /// TODO: Does the order actually matter?
        public string CDKey1 { get; private set; }

        /// <summary>
        /// Gets the second (expansion) CD key.
        /// </summary>
        public string CDKey2 { get; private set; }

        /// <summary>
        /// Gets the name of the CD key owner.
        /// </summary>
        public string CDKeyOwner { get; private set; }
    }
}
