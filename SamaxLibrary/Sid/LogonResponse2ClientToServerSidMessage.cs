﻿namespace SamaxLibrary.Sid
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    /// <summary>
    /// This class represents a SID_LOGONRESPONSE2 message sent from the client to the server.
    /// This message is sent when logging onto an account.
    /// </summary>
    public class LogonResponse2ClientToServerSidMessage : SidMessage
    {
        /// <summary>
        /// The SID message type of the SID message that the
        /// <see cref="LogonResponse2ClientToServerSidMessage"/> class represents.
        /// </summary>
        public new const SidMessageType MessageType = SidMessageType.LogonResponse2;

        /// <summary>
        /// Gets the client token.
        /// </summary>
        /// TODO: Mention in the remarks what message the client token is received in.
        public Int32 ClientToken { get; private set; }

        /// <summary>
        /// Gets the server token.
        /// </summary>
        /// TODO: Mention in the remarks what message the server token is received in.
        public Int32 ServerToken { get; private set; }

        /// <summary>
        /// Gets a "tokenized" hash of the password.
        /// </summary>
        /// <remarks>
        /// <para>This value is the result of the broken SHA-1 algorithm applied twice in a way
        /// that involves the client token and the server token as well as the password itself.
        /// </para>
        /// <para>
        /// See <see cref="BrokenSha1.ComputeTokenizedHash"/> for more information.
        /// </para>
        /// </remarks>
        public BrokenSha1Hash TokenizedPasswordHash { get; private set; }

        /// <summary>
        /// Gets the account name.
        /// </summary>
        /// TODO: Does "account name" make sense for games other than Diablo 2?
        public string AccountName { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="LogonResponse2ClientToServerSidMessage"/>
        /// class.
        /// </summary>
        /// <param name="messageBytes">An array of bytes that composes the message.</param>
        /// <exception cref="ArgumentNullException"><paramref name="messageBytes"/> is <c>null</c>.
        /// </exception>
        /// <exception cref="ArgumentException"><paramref name="messageBytes"/> does not represent
        /// a valid client-to-server SID_LOGONRESPONSE2 message.</exception>
        public LogonResponse2ClientToServerSidMessage(byte[] messageBytes)
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
                this.ServerToken = parser.ReadInt32();
                this.TokenizedPasswordHash = parser.ReadBrokenSha1Hash();
                this.AccountName = parser.ReadAsciiString();
                //// TODO: Validate the account name
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
        /// Creates an instance of the <see cref="LogonResponse2ClientToServerSidMessage"/> class from
        /// high-level data.
        /// </summary>
        /// <param name="serverToken">The server token, which is received in the SID_AUTH_INFO
        /// message sent from the server (see <see cref="AuthInfoServerToClientSidMessage"/>).</param>
        /// <param name="accountName">The name of the account to which to log in.</param>
        /// <param name="password">The password of the account to which to log in.</param>
        /// <returns>An instance of the <see cref="LogonResponse2ClientToServerSidMessage"/> class
        /// with the specified data.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="accountName"/> or
        /// <paramref name="password"/> is <c>null</c>.</exception>
        public static LogonResponse2ClientToServerSidMessage CreateFromHighLevelData(
            Int32 serverToken,
            string accountName,
            string password)
        {
            if (accountName == null)
            {
                throw new ArgumentNullException("accountName");
            }

            if (password == null)
            {
                throw new ArgumentNullException("password");
            }

            //// TODO: Validate the account name and the password.
            //// Make sure that the account name is not too long for SidMessage.GetMessageBytes to fail.

            // Note that the bytes do not include any null terminator. (What's with this comment?)
            int clientToken = new Random().Next();
            BrokenSha1Hash passwordHash = BrokenSha1.ComputeTokenizedHash(clientToken, serverToken, password);

            SidByteWriter writer = new SidByteWriter();
            writer.AppendInt32(clientToken);
            writer.AppendInt32(serverToken);
            writer.AppendBrokenSha1Hash(passwordHash);
            writer.AppendAsciiString(accountName);

            byte[] dataBytes = writer.Bytes;
            byte[] messageBytes = SidMessage.GetMessageBytes(dataBytes, MessageType);

            return new LogonResponse2ClientToServerSidMessage(messageBytes);
        }
    }
}
