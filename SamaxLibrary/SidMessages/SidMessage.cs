// -----------------------------------------------------------------------
// <copyright file="SidMessage.cs" company="TODO">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

namespace SamaxLibrary.SidMessages
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    /// <summary>
    /// This class represents a message of the SID protocol used in communication with some of
    /// the servers of Battle.Net.
    /// </summary>
    /// <remarks>
    /// <para>Strings are encoded in ASCII</para>
    /// <para>Most chunks larger than one byte are encoded in little-endian.</para>
    /// </remarks>
    public abstract class SidMessage : Message
    {
        /// <summary>
        /// The SID message type of this message.
        /// </summary>
        private readonly SidMessageType messageType;

        /// <summary>
        /// Gets the SID message type of this message.
        /// </summary>
        public SidMessageType MessageType
        {
            get { return this.messageType; }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SidMessage"/> class.
        /// </summary>
        /// <param name="messageBytes">The bytes that compose the SID message.</param>
        /// <param name="messageType">The SID message type of the SID message.</param>
        public SidMessage(byte[] messageBytes, SidMessageType messageType) : base(messageBytes)
        {
            this.messageType = messageType;
        }
    }
}
