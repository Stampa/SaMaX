namespace SamaxLibrary.Sid
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    /// <summary>
    /// This class represents a SID_PING message sent from the server to the client.
    /// </summary>
    public class PingServerToClientSidMessage : SidMessage
    {
        /// <summary>
        /// The SID message type of the SID message that the
        /// <see cref="PingServerToClientSidMessage"/> class represents.
        /// </summary>
        public new const SidMessageType MessageType = SidMessageType.Ping;

        /// <summary>
        /// Gets the ping value, which the client is supposed to immediately return in a
        /// client-to-server ping message.
        /// </summary>
        public Int32 PingValue { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="PingServerToClientSidMessage"/> class.
        /// </summary>
        /// <param name="messageBytes">An array of bytes that composes the message.</param>
        /// <exception cref="ArgumentNullException"><paramref name="messageBytes"/> is <c>null</c>.
        /// </exception>
        /// <exception cref="ArgumentException"><paramref name="messageBytes"/> does not represent
        /// a valid server-to-client SID_PING message.</exception>
        public PingServerToClientSidMessage(byte[] messageBytes)
            : base(messageBytes, MessageType)
        {
            if (messageBytes == null)
            {
                throw new ArgumentNullException("messageBytes");
            }

            SidByteParser parser = new SidByteParser(messageBytes);

            try
            {
                this.PingValue = parser.ReadInt32();
            }
            catch (SidByteParserException ex)
            {
                throw new ArgumentException(
                    String.Format("The bytes could not be parsed successfully: {0}", ex.Message),
                    ex);
            }

            if (parser.HasBytesLeft)
            {
                throw new ArgumentException("There were unexpected bytes at the end of the message.");
            }
        }
    }
}
