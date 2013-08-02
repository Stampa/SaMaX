namespace SamaxLibrary.Sid
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    /// <summary>
    /// This class represents a SID_AUTH_INFO message sent from the server to the client.
    /// </summary>
    public class AuthInfoServerToClientSidMessage : SidMessage
    {
        /// <summary>
        /// The SID message type of the SID message that the
        /// <see cref="AuthInfoServerToClientSidMessage"/> class represents.
        /// </summary>
        public new const SidMessageType MessageType = SidMessageType.AuthInfo;

        /// <summary>
        /// Gets the logon type that the server expects.
        /// </summary>
        /// <remarks>For StarCraft (including the expansion) and Diablo 2 (also including the
        /// expansion), <see cref="SamaxLibrary.Sid.LogonType.BrokenSha1"/> is used.</remarks>
        public LogonType LogonType { get; private set; }

        /// <summary>
        /// Gets the server token.
        /// </summary>
        /// <remarks>This value is used in subsequent message in the logon process.</remarks>
        public Int32 ServerToken { get; private set; }

        /// <summary>
        /// Gets the UDP value.
        /// </summary>
        /// <remarks>TODO: Add information here (it seems to be unclear what this value is though)
        /// </remarks>
        public Int32 UdpValue { get; private set; }

        /// <summary>
        /// Gets the MPQ file time.
        /// TODO: Clarify what this is
        /// </summary>
        public DateTime MpqFileTime { get; private set; }

        /// <summary>
        /// Gets the MPQ file name.
        /// TODO: The name of what MPQ file? The IX86-ver thing?
        /// </summary>
        public string MpqFileName { get; private set; }

        /// <summary>
        /// Gets the value string.
        /// TODO: Clarify what the value string is.
        /// </summary>
        public string ValueString { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="AuthInfoServerToClientSidMessage"/> class
        /// from an array of bytes.
        /// </summary>
        /// <param name="messageBytes">An array of bytes that composes the message.</param>
        /// <exception cref="ArgumentNullException"><paramref name="messageBytes"/> is <c>null</c>.
        /// </exception>
        /// <exception cref="ArgumentException"><paramref name="messageBytes"/> does not represent
        /// a valid server-to-client SID_AUTH_INFO message.</exception>
        public AuthInfoServerToClientSidMessage(byte[] messageBytes)
            : base(messageBytes, MessageType)
        {
            if (messageBytes == null)
            {
                throw new ArgumentNullException("messageBytes");
            }

            SidByteParser parser = new SidByteParser(messageBytes);

            try
            {
                this.LogonType = parser.ReadInt32AsEnum<LogonType>();
                this.ServerToken = parser.ReadInt32();
                this.UdpValue = parser.ReadInt32();

                UInt64 fileTime = parser.ReadUInt64();
                Int64 signedFileTime = (Int64)fileTime; // TODO: What happens if fileTime is large?

                try
                {
                    this.MpqFileTime = DateTime.FromFileTime(signedFileTime); // TODO: FromFileTimeUtc?
                }
                catch (ArgumentOutOfRangeException ex)
                {
                    throw new ArgumentException(
                        "The parsed MPQ file time could not be converted to a DateTime", ex);
                }

                this.MpqFileName = parser.ReadAsciiString();
                this.ValueString = parser.ReadAsciiString();

                // TODO: Parse the 128-bit server signature for Warcraft 3 here
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
