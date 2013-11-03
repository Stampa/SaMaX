namespace SamaxLibrary.Sid
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    /// <summary>
    /// This class represents a SID_LOGONREALMEX message sent from the client to the server.
    /// </summary>
    public class LogonRealmExClientToServerSidMessage : SidMessage
    {
        /// <summary>
        /// The SID message type of the SID message that the
        /// <see cref="LogonRealmExClientToServerSidMessage"/> class represents.
        /// </summary>
        public new const SidMessageType MessageType = SidMessageType.LogonRealmEx;

        /// <summary>
        /// Gets the client token. This value is not to be confused with the client token of the
        /// client-to-server SID_AUTH_CHECK message.
        /// </summary>
        /// <seealso cref="AuthCheckClientToServerSidMessage"/>
        public Int32 ClientToken { get; private set; }

        /// <summary>
        /// Gets the tokenized hash of the realm password, which is always "password".
        /// </summary>
        public BrokenSha1Hash TokenizedRealmPasswordHash { get; private set; }

        /// <summary>
        /// Gets the title of the realm to which to log on.
        /// </summary>
        public string RealmTitle { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="LogonRealmExClientToServerSidMessage"/> class.
        /// </summary>
        /// <param name="messageBytes">An array of bytes that composes the message.</param>
        /// <exception cref="ArgumentNullException"><paramref name="messageBytes"/> is <c>null</c>.
        /// </exception>
        /// <exception cref="ArgumentException"><paramref name="messageBytes"/> does not represent
        /// a valid client-to-server SID_LOGONREALMEX message.</exception>
        public LogonRealmExClientToServerSidMessage(byte[] messageBytes)
            : base(messageBytes, MessageType)
        {
            if (messageBytes == null)
            {
                throw new ArgumentNullException("messageBytes");
            }

            SidByteParser parser = new SidByteParser(messageBytes);

            try
            {
                this.ClientToken = parser.ReadInt32();
                this.TokenizedRealmPasswordHash = parser.ReadBrokenSha1Hash();
                this.RealmTitle = parser.ReadAsciiString();
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
        /// Creates an instance of the <see cref="LogonRealmExClientToServerSidMessage"/> class
        /// from high-level data.
        /// </summary>
        /// <param name="clientToken">The client token, which was generated for the
        /// client-to-server SID_AUTH_CHECK message.</param>
        /// <param name="serverToken">The server token, which was received in the server-to-client
        /// SID_AUTH_INFO message.</param>
        /// <param name="realmTitle">The realm title, which was received in the server-to-client
        /// SID_QUERYREALMS2 message.</param>
        /// <returns>An instance of the <see cref="LogonRealmExClientToServerSidMessage"/> class
        /// with the specified data.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="realmTitle"/> is
        /// <see langword="null"/>.</exception>
        public static LogonRealmExClientToServerSidMessage CreateFromHighLevelData(
            Int32 clientToken,
            Int32 serverToken,
            string realmTitle)
        {
            if (realmTitle == null)
            {
                throw new ArgumentNullException("realmTitle");
            }

            SidByteWriter writer = new SidByteWriter();
            writer.AppendInt32(clientToken);

            BrokenSha1Hash tokenizedRealmPasswordHash =
                BrokenSha1.ComputeTokenizedHash(clientToken, serverToken, "password");
            writer.AppendBrokenSha1Hash(tokenizedRealmPasswordHash);

            writer.AppendAsciiString(realmTitle);

            byte[] dataBytes = writer.Bytes;
            byte[] messageBytes = SidMessage.GetMessageBytes(dataBytes, MessageType);

            return new LogonRealmExClientToServerSidMessage(messageBytes);
        }
    }
}
