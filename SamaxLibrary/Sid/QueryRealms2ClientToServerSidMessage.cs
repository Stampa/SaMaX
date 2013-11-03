namespace SamaxLibrary.Sid
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    /// <summary>
    /// This class represents a SID_QUERYREALMS2 message sent from the client to the server.
    /// </summary>
    public class QueryRealms2ClientToServerSidMessage : SidMessage
    {
        /// <summary>
        /// The SID message type of the SID message that the
        /// <see cref="QueryRealms2ClientToServerSidMessage"/> class represents.
        /// </summary>
        public new const SidMessageType MessageType = SidMessageType.QueryRealms2;

        /// <summary>
        /// Initializes a new instance of the <see cref="QueryRealms2ClientToServerSidMessage"/> class.
        /// </summary>
        /// <param name="messageBytes">An array of bytes that composes the message.</param>
        /// <exception cref="ArgumentNullException"><paramref name="messageBytes"/> is <c>null</c>.
        /// </exception>
        /// <exception cref="ArgumentException"><paramref name="messageBytes"/> does not represent
        /// a valid client-to-server SID_QUERYREALMS2 message.</exception>
        public QueryRealms2ClientToServerSidMessage(byte[] messageBytes)
            : base(messageBytes, MessageType)
        {
            if (messageBytes == null)
            {
                throw new ArgumentNullException("messageBytes");
            }

            SidByteParser parser = new SidByteParser(messageBytes);

            if (parser.HasBytesLeft)
            {
                throw new ArgumentException("There were unexpected bytes at the end of the message.");
            }
        }

        /// <summary>
        /// Creates an instance of the <see cref="QueryRealms2ClientToServerSidMessage"/> class from
        /// high-level data.
        /// </summary>
        /// <returns>An instance of the <see cref="QueryRealms2ClientToServerSidMessage"/> class.
        /// </returns>
        public static QueryRealms2ClientToServerSidMessage CreateFromHighLevelData()
        {
            SidByteWriter writer = new SidByteWriter();

            byte[] dataBytes = writer.Bytes;
            byte[] messageBytes = SidMessage.GetMessageBytes(dataBytes, MessageType);

            return new QueryRealms2ClientToServerSidMessage(messageBytes);
        }
    }
}
