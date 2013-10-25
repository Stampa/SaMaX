namespace SamaxLibrary.Clients
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Net.Sockets;
    using System.Text;
    using System.Threading.Tasks;
    using SamaxLibrary.Sid;

    /* TODO:
     * Beware of ArgumentExceptions from the message factory!
     *      The SidHeader class as well!
     *      ... and FCL classes
     *      
     * Make this class disposable to dispose of the stream properly
     * 
     * Put all the settings needed in some settings class
     */

    /// <summary>
    /// This class represents a client for the SID protocol.
    /// </summary>
    /// <seealso cref="SamaxLibrary.Sid"/>
    public class D2xpSidClient
    {
        /// <summary>
        /// The settings for the client.
        /// </summary>
        private D2xpClientSettings settings;

        /// <summary>
        /// The underlying network stream.
        /// </summary>
        private NetworkStream stream;

        /// <summary>
        /// Gets a value indicating whether the client is connected to a SID server.
        /// </summary>
        public bool IsConnected { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="D2xpSidClient"/> class.
        /// </summary>
        /// <param name="settings">The settings for the client.</param>
        /// <exception cref="ArgumentNullException"><paramref name="settings"/> is
        /// <see langword="null"/>.</exception>
        public D2xpSidClient(D2xpClientSettings settings)
        {
            if (settings == null)
            {
                throw new ArgumentNullException("settings");
            }

            this.settings = settings;

            this.IsConnected = false;
        }

        /// <summary>
        /// Connects the client to the specified SID server.
        /// </summary>
        /// <exception cref="InvalidOperationException">The client is already connected.
        /// </exception>
        /// <exception cref="ClientException">An error occurred.</exception>
        /// TODO: Make a wrapper for System.Net.IPAddress so that consumers do not need to deal
        /// with that namespace too?
        public void Connect()
        {
            if (this.IsConnected)
            {
                throw new InvalidOperationException("The client is already connected.");
            }

            try
            {
                TcpClient client = new TcpClient();
                client.Connect(this.settings.IPAddress, this.settings.Port);
                this.stream = client.GetStream();
            }
            catch (SocketException ex)
            {
                throw new ClientException(
                    String.Format(
                        "Could not connect to {0}:{1}.",
                        this.settings.IPAddress,
                        this.settings.Port),
                    ex);
            }

            try
            {
                // TODO: Is it really called a "protocol bit"?
                byte[] protocolBitBuffer = { 1 };
                this.stream.Write(protocolBitBuffer, 0, protocolBitBuffer.Length);
            }
            catch (IOException ex)
            {
                throw new ClientException("Could not send protocol bit.", ex);
            }

            this.IsConnected = true;
        }

        /// <summary>
        /// Authenticates the client.
        /// </summary>
        /// <exception cref="InvalidOperationException">The client is not connected.</exception>
        /// <exception cref="ClientException">An error occurred.</exception>
        /// TODO: This method is not very flexible in terms of the order of messages ...
        public void Authenticate()
        {
            if (!this.IsConnected)
            {
                throw new InvalidOperationException("The client is not connected.");
            }

            // TODO: Arbitrary size, and what happens if more than this is sent?
            // Exception could _but needs not_ be thrown by SID message classes
            // An easy fix would be allow at most BufferSize-1 bytes per message and
            // crash if the entire buffer is filled
            const int BufferSize = 1000;
            byte[] buffer = new byte[BufferSize];

            this.SendCsAuthInfo();
            var scPingMessage = this.ReceiveScPing(buffer);
            this.SendCsPing(scPingMessage.PingValue);
            var scAuthInfoMessage = this.ReceiveScAuthInfo(buffer);
            this.SendCsAuthCheck(scAuthInfoMessage.MpqFileName, scAuthInfoMessage.ValueString, scAuthInfoMessage.ServerToken);
        }

        /// <summary>
        /// Reads the bytes of the next message from the specified stream.
        /// </summary>
        /// <param name="stream">The stream from which to read.</param>
        /// <param name="buffer">The buffer used to receive the bytes.</param>
        /// <returns>The bytes composing the next message in the stream.</returns>
        private byte[] ReadMessageBytes(NetworkStream stream, byte[] buffer)
        {
            // Read header bytes first to determine how much more to read
            int amountOfBytesRead = stream.Read(buffer, 0, SidHeader.HeaderLength);
            if (amountOfBytesRead < SidHeader.HeaderLength)
            {
                throw new ClientException(
                    String.Format(
                        "Read too few bytes ({0}) to compose a SID header ({1} required).",
                        amountOfBytesRead,
                        SidHeader.HeaderLength));
            }

            byte[] headerBytes = buffer.Take(SidHeader.HeaderLength).ToArray();
            SidHeader header = new SidHeader(headerBytes);
            amountOfBytesRead += stream.Read(buffer, SidHeader.HeaderLength, header.MessageLength - SidHeader.HeaderLength);
            if (amountOfBytesRead != header.MessageLength)
            {
                throw new ClientException(
                    String.Format(
                        "Read too few bytes ({0}) to compose the entire message ({1} required).",
                        amountOfBytesRead,
                        header.MessageLength));
            }

            return buffer.Take(amountOfBytesRead).ToArray();
        }

        /// <summary>
        /// Sends a client-to-server authentication info message.
        /// </summary>
        private void SendCsAuthInfo()
        {
            var csAuthInfomessage = SidMessageFactory.CreateClientToServerMessageFromHighLevelData(
                SidMessageType.AuthInfo,
                new object[] { ProductID.D2xp, this.settings.Version, this.settings.LocalIPAddress });
            this.stream.Write(csAuthInfomessage.Bytes, 0, csAuthInfomessage.Bytes.Length);
        }

        /// <summary>
        /// Receives a server-to-client ping message.
        /// </summary>
        /// <param name="buffer">The buffer used to receive the message bytes.</param>
        /// <returns>The received server-to-client ping message</returns>
        /// <exception cref="ClientException">The message received is not of type
        /// <see cref="SidMessageType.Ping"/></exception>
        private PingServerToClientSidMessage ReceiveScPing(byte[] buffer)
        {
            byte[] messageBytes = this.ReadMessageBytes(this.stream, buffer);
            SidMessage message = SidMessageFactory.CreateServerToClientMessageFromBytes(messageBytes);

            if (message.MessageType != SidMessageType.Ping)
            {
                throw new ClientException(
                    String.Format(
                        "The message type ({0}) was not {1}.",
                        message.MessageType,
                        SidMessageType.Ping));
            }

            return (PingServerToClientSidMessage)message;
        }

        /// <summary>
        /// Sends a client-to-server ping message. 
        /// </summary>
        /// <param name="pingValue">The ping value received from the server earlier.</param>
        private void SendCsPing(Int32 pingValue)
        {
            var csPingMessage = SidMessageFactory.CreateClientToServerMessageFromHighLevelData(
                SidMessageType.Ping,
                new object[] { pingValue });
            this.stream.Write(csPingMessage.Bytes, 0, csPingMessage.Bytes.Length);
        }

        /// <summary>
        /// Receives a server-to-client authentication info message.
        /// </summary>
        /// <param name="buffer">The buffer used to receive the message bytes.</param>
        /// <returns>The received server-to-client authentication info message.</returns>
        /// <exception cref="ClientException">The message received is not of type
        /// <see cref="SidMessageType.AuthInfo"/></exception>
        private AuthInfoServerToClientSidMessage ReceiveScAuthInfo(byte[] buffer)
        {
            byte[] messageBytes = this.ReadMessageBytes(this.stream, buffer);
            SidMessage message = SidMessageFactory.CreateServerToClientMessageFromBytes(messageBytes);
            if (message.MessageType != SidMessageType.AuthInfo)
            {
                throw new ClientException(
                    String.Format(
                        "The message type ({0}) was not {1}.",
                        message.MessageType,
                        SidMessageType.AuthInfo));
            }

            var scAuthInfoMessage = (AuthInfoServerToClientSidMessage)message;
            this.ValidateScAuthInfo(scAuthInfoMessage);
            return scAuthInfoMessage;
        }

        /// <summary>
        /// Validates a server-to-client authentication info message by throwing exceptions if it
        /// is invalid.
        /// </summary>
        /// <param name="message">The authentication info message to validate.</param>
        /// <exception cref="ClientException">The logon type is not broken SHA-1.</exception>
        private void ValidateScAuthInfo(AuthInfoServerToClientSidMessage message)
        {
            if (message.LogonType != LogonType.BrokenSha1)
            {
                throw new ClientException(
                    String.Format(
                        "Received logon type was {0}, not {1}.",
                        message.LogonType,
                        LogonType.BrokenSha1));
            }
        }

        /// <summary>
        /// Sends a client-to-server authentication check message.
        /// </summary>
        /// <param name="mpqFileName">The MPQ file name received in the SID_AUTH_INFO message from
        /// the server.</param>
        /// <param name="valueString">The value string received in the SID_AUTH_INFO message from
        /// the server.
        /// </param>
        /// <param name="serverToken">The server token received in the SID_AUTH_INFO message from
        /// the server.</param>
        private void SendCsAuthCheck(
            string mpqFileName,
            string valueString,
            Int32 serverToken)
        {
            var csAuthCheckMessage = AuthCheckClientToServerSidMessage.CreateFromHighLevelData(
                this.settings.FileTriple,
                ProductID.D2xp,
                this.settings.CDKey1,
                this.settings.CDKey2,
                this.settings.CDKeyOwner,
                mpqFileName,
                valueString,
                serverToken);
            this.stream.Write(csAuthCheckMessage.Bytes, 0, csAuthCheckMessage.Bytes.Length);
        }
    }
}
