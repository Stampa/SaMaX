namespace SamaxLibrary.Clients
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    /// <summary>
    /// Defines the trace event identifiers used by <see cref="D2xpSidClient"/>.
    /// </summary>
    public enum TraceEventID
    {
        /// <summary>
        /// The identifier for the event that a message is sent.
        /// </summary>
        MessageSent,

        /// <summary>
        /// The identifier for the event that a packet is received.
        /// </summary>
        PacketReceived
    }
}
