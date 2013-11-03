namespace SamaxLibrary.Sid
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    /// <summary>
    /// This class represents a SID_LOGONREALMEX message sent from the server to the client.
    /// </summary>
    public class LogonRealmExServerToClientSidMessage : SidMessage
    {
        /// <summary>
        /// The SID message type of the SID message that the
        /// <see cref="LogonRealmExServerToClientSidMessage"/> class represents.
        /// </summary>
        public new const SidMessageType MessageType = SidMessageType.LogonRealmEx;

        /// <summary>
        /// Gets a value indicating whether the logon was successful.
        /// </summary>
        /// <remarks>This value does not correspond to any byte of the message but rather is
        /// encoded implicitly. If the length of the message (in bytes, including the header) is
        /// greater than 12, the logon was successful.</remarks>
        public bool LogonWasSuccessful { get; private set; }

        /// <summary>
        /// Gets the MCP cookie.
        /// </summary>
        /// <remarks>This value is the client token that was generated for the client-to-server
        /// SID_LOGONREALMEX message.</remarks>
        /// <seealso cref="LogonRealmExClientToServerSidMessage.ClientToken"/>
        public Int32 McpCookie { get; private set; }

        /// <summary>
        /// Gets the MCP status indicating whether the MCP server was logged onto successfully.
        /// </summary>
        /// <remarks>If the logon was unsuccessful, this field should be interpreted as an error
        /// code as follows:
        /// <list type="table">
        ///     <listheader>
        ///         <term>Value</term>
        ///         <description>Meaning</description>
        ///     </listheader>
        ///     <item>
        ///         <term>0x80000001</term>
        ///         <description>The realm is unavailable.</description>
        ///     </item>
        ///     <item>
        ///         <term>0x80000002</term>
        ///         <description>The realm logon failed.
        ///         TODO: Does this mean that the realm password was wrong?</description>
        ///     </item>
        /// </list>
        /// </remarks>
        public Int32 McpStatus { get; private set; }

        /// <summary>
        /// Gets the first MCP chunk, which contains 2 dwords.
        /// </summary>
        /// <remarks>If the logon was unsuccessful, this value is <see langword="null"/>.</remarks>
        public Int32[] McpChunk1 { get; private set; }

        /// <summary>
        /// Gets the IP address of the MCP server.
        /// </summary>
        /// <remarks>If the logon was unsuccessful, this value is <see langword="null"/>.</remarks>
        public byte[] IPAddress { get; private set; }

        /// <summary>
        /// Gets the port of the MCP server to which to connect.
        /// </summary>
        /// <remarks>If the logon was unsuccessful, this value is <see langword="null"/>.</remarks>
        public Int32? Port { get; private set; }

        /// <summary>
        /// Gets the second MCP chunk, which contains 12 dwords.
        /// </summary>
        /// <remarks>If the logon was unsuccessful, this value is <see langword="null"/>.</remarks>
        public Int32[] McpChunk2 { get; private set; }

        /// <summary>
        /// Gets the unique name.
        /// </summary>
        /// <remarks>If the logon was unsuccessful, this value is <see langword="null"/>.</remarks>
        /// TODO: Clarify what this is.
        public string UniqueName { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="LogonRealmExServerToClientSidMessage"/> class.
        /// </summary>
        /// <param name="messageBytes">An array of bytes that composes the message.</param>
        /// <exception cref="ArgumentNullException"><paramref name="messageBytes"/> is <c>null</c>.
        /// </exception>
        /// <exception cref="ArgumentException"><paramref name="messageBytes"/> does not represent
        /// a valid server-to-client SID_LOGONREALMEX message.</exception>
        public LogonRealmExServerToClientSidMessage(byte[] messageBytes)
            : base(messageBytes, MessageType)
        {
            if (messageBytes == null)
            {
                throw new ArgumentNullException("messageBytes");
            }

            SidByteParser parser = new SidByteParser(messageBytes);

            try
            {
                this.LogonWasSuccessful = messageBytes.Length > 12;
                this.McpCookie = parser.ReadInt32();
                this.McpStatus = parser.ReadInt32();

                if (this.LogonWasSuccessful)
                {
                    this.McpChunk1 = parser.ReadInt32Array(2);
                    this.IPAddress = parser.ReadByteArray(4);
                    this.Port = parser.ReadInt32();
                    this.McpChunk2 = parser.ReadInt32Array(12);
                    this.UniqueName = parser.ReadAsciiString();
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
