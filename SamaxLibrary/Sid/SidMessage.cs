// -----------------------------------------------------------------------
// <copyright file="SidMessage.cs" company="TODO">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

namespace SamaxLibrary.Sid
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    /// <summary>
    /// This class represents a message of the SID protocol used in communication with some of
    /// the servers of Battle.Net.
    /// </summary>
    /// <remarks>See <see cref="SidByteParser"/> for information about the byte-level
    /// representation of SID messages.</remarks>
    /// <seealso cref="SidByteParser"/>
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

        /// <summary>
        /// Gets the SID message type from the bytes that compose a SID message.
        /// </summary>
        /// <param name="messageBytes">The bytes that compose the SID message whose message type to
        /// get.</param>
        /// <returns>The SID message type of the SID message that the specified byte array
        /// composes.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="messageBytes"/> is <c>null</c>.
        /// </exception>
        /// <exception cref="ArgumentException"><paramref name="messageBytes"/> is too small to
        /// contain the SID message type, or the byte in <paramref name="messageBytes"/> that
        /// corresponds to the SID message type does not represent a valid SID message type.
        /// </exception>
        internal static SidMessageType GetSidMessageType(byte[] messageBytes)
        {
            if (messageBytes == null)
            {
                throw new ArgumentNullException("messageBytes");
            }

            //// TODO: Validate the entire header (not just the message bytes) here

            const int MessageTypeIndex = 1;
            if (messageBytes.Length <= MessageTypeIndex)
            {
                throw new ArgumentException(
                    String.Format(
                        "The length of the array ({0}) is too small to contain the SID message type.",
                        messageBytes.Length),
                    "messageBytes");
            }

            SidMessageType messageType = (SidMessageType)messageBytes[MessageTypeIndex];
            if (!Enum.IsDefined(typeof(SidMessageType), messageType))
            {
                throw new ArgumentException(
                    String.Format(
                        "The byte corresponding to the SID message type ({0}) does not represent a valid SID message type.",
                        messageType),
                    "messageBytes");
            }

            return messageType;
        }
    }
}
