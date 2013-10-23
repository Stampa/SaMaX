namespace SamaxLibrary.Sid
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    /// <summary>
    /// This class represents a SID_PING message sent from the client to the server.
    /// It is sent in response to a SID_PING message sent from the server.
    /// </summary>
    public class PingClientToServerSidMessage : SidMessage
    {
        /// <summary>
        /// The SID message type of the SID message that the
        /// <see cref="PingClientToServerSidMessage"/> class represents.
        /// </summary>
        public new const SidMessageType MessageType = SidMessageType.Ping;

        /// <summary>
        /// Gets the ping value, which is obtained from the server-to-client ping message.
        /// </summary>
        /// <seealso cref="PingServerToClientSidMessage.PingValue"/>
        public Int32 PingValue { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="PingClientToServerSidMessage"/> class.
        /// </summary>
        /// <param name="messageBytes">An array of bytes that composes the message.</param>
        /// <exception cref="ArgumentNullException"><paramref name="messageBytes"/> is <c>null</c>.
        /// </exception>
        /// <exception cref="ArgumentException"><paramref name="messageBytes"/> does not represent
        /// a valid server-to-client SID_PING message.</exception>
        public PingClientToServerSidMessage(byte[] messageBytes)
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

        /// <summary>
        /// Creates an instance of the <see cref="PingClientToServerSidMessage"/> class from
        /// high-level data.
        /// </summary>
        /// <param name="pingValue">The ping value (typically received from the server).</param>
        /// <returns>An instance of the <see cref="PingClientToServerSidMessage"/> class with the
        /// specified data.</returns>
        public static PingClientToServerSidMessage CreateFromHighLevelData(Int32 pingValue)
        {
            SidByteWriter writer = new SidByteWriter();
            writer.AppendInt32(pingValue);

            byte[] dataBytes = writer.Bytes;
            byte[] messageBytes = SidMessage.GetMessageBytes(dataBytes, MessageType);

            return new PingClientToServerSidMessage(messageBytes);
        }
    }
}
