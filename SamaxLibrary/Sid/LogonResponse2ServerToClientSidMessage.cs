namespace SamaxLibrary.Sid
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    /// <summary>
    /// This class represents a SID_LOGONRESPONSE2 message sent from the server to the client.
    /// </summary>
    public class LogonResponse2ServerToClientSidMessage : SidMessage
    {
        /// <summary>
        /// The SID message type of the SID message that the
        /// <see cref="LogonResponse2ServerToClientSidMessage"/> class represents.
        /// </summary>
        public new const SidMessageType MessageType = SidMessageType.LogonResponse2;

        /// <summary>
        /// Gets the logon response.
        /// </summary>
        public LogonResponse Status { get; private set; }

        /// <summary>
        /// Gets a string with additional information for the result.
        /// </summary>
        /// <remarks>This value is <see langword="null"/> rather than an empty string whenever
        /// <see cref="Status"/> is not <see cref="LogonResponse.AccountClosed"/>.</remarks>
        public string AdditionalInformation { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="LogonResponse2ServerToClientSidMessage"/> class.
        /// </summary>
        /// <param name="messageBytes">An array of bytes that composes the message.</param>
        /// <exception cref="ArgumentNullException"><paramref name="messageBytes"/> is <c>null</c>.
        /// </exception>
        /// <exception cref="ArgumentException"><paramref name="messageBytes"/> does not represent
        /// a valid server-to-client SID_LOGONRESPONSE2 message.</exception>
        public LogonResponse2ServerToClientSidMessage(byte[] messageBytes)
            : base(messageBytes, MessageType)
        {
            if (messageBytes == null)
            {
                throw new ArgumentNullException("messageBytes");
            }

            SidByteParser parser = new SidByteParser(messageBytes);

            try
            {
                this.Status = parser.ReadInt32AsEnum<LogonResponse>();
                if (this.Status == LogonResponse.AccountClosed)
                {
                    this.AdditionalInformation = parser.ReadAsciiString();
                }
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
