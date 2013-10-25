namespace SamaxLibrary.Sid
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    /// <summary>
    /// This class represents a SID_AUTH_CHECK message sent from the server to the client.
    /// </summary>
    public class AuthCheckServerToClientSidMessage : SidMessage
    {
        /// <summary>
        /// The SID message type of the SID message that the
        /// <see cref="AuthCheckServerToClientSidMessage"/> class represents.
        /// </summary>
        public new const SidMessageType MessageType = SidMessageType.AuthCheck;

        /// <summary>
        /// Gets the result of the authentication check.
        /// </summary>
        /// <remarks>
        /// 0x000 indicates success.
        /// </remarks>
        /// TODO: There's a lot of possible return values to document.
        public Int32 Result { get; private set; }

        /// <summary>
        /// Gets a string with additional information for the result.
        /// </summary>
        public string AdditionalInformation { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="AuthCheckServerToClientSidMessage"/> class.
        /// </summary>
        /// <param name="messageBytes">An array of bytes that composes the message.</param>
        /// <exception cref="ArgumentNullException"><paramref name="messageBytes"/> is <c>null</c>.
        /// </exception>
        /// <exception cref="ArgumentException"><paramref name="messageBytes"/> does not represent
        /// a valid server-to-client SID_AUTH_CHECK message.</exception>
        public AuthCheckServerToClientSidMessage(byte[] messageBytes)
            : base(messageBytes, MessageType)
        {
            if (messageBytes == null)
            {
                throw new ArgumentNullException("messageBytes");
            }

            SidByteParser parser = new SidByteParser(messageBytes);

            try
            {
                this.Result = parser.ReadInt32();
                this.AdditionalInformation = parser.ReadAsciiString();
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
