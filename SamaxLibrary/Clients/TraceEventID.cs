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
        /// The identifier for the event that the bytes of a message is about to be sent.
        /// </summary>
        MessageBytesToBeSent,

        /// <summary>
        /// The identifier for the event that the bytes of a message is received.
        /// </summary>
        MessageBytesReceived,

        /// <summary>
        /// The identifier for the event that a message is about to be sent.
        /// </summary>
        MessageToBeSent,

        /// <summary>
        /// The identifier for the event that a message is received.
        /// </summary>
        MessageReceived
    }
}
