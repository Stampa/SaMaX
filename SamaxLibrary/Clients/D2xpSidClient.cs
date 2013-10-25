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
     *      
     * Make this class disposable to dispose of the stream properly
     */

    /// <summary>
    /// This class represents a client for the SID protocol.
    /// </summary>
    /// <seealso cref="SamaxLibrary.Sid"/>
    public class D2xpSidClient
    {
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
        public D2xpSidClient()
        {
            this.IsConnected = false;
        }

        /// <summary>
        /// Connects the client to the specified SID server.
        /// </summary>
        /// <param name="address">The IP address of the SID server to which to connect.</param>
        /// <param name="port">The port of the SID server to which to connect.</param>
        /// <exception cref="ArgumentNullException"><paramref name="address"/> is
        /// <see langword="null"/>.</exception>
        /// <exception cref="ArgumentOutOfRangeException">The <paramref name="port"/> parameter is
        /// outside the valid range of ports.</exception>
        /// <exception cref="InvalidOperationException">The client is already connected.
        /// </exception>
        /// <exception cref="ClientException">An error occurred.</exception>
        /// TODO: Make a wrapper for System.Net.IPAddress so that consumers do not need to deal
        /// with that namespace too?
        public void Connect(IPAddress address, int port = 6112)
        {
            if (address == null) 
            {
                throw new ArgumentNullException("address");
            }

            if (this.IsConnected)
            {
                throw new InvalidOperationException("The client is already connected.");
            }

            try
            {
                TcpClient client = new TcpClient();
                client.Connect(address, port);
                this.stream = client.GetStream();
            }
            catch (SocketException ex)
            {
                throw new ClientException(
                    String.Format("Could not connect to {0}:{1}.", address, port),
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
        /// <param name="version">The version.</param>
        /// <param name="localIPAddress">The local IP address to state.</param>
        /// <exception cref="InvalidOperationException">The client is not connected.</exception>
        /// <exception cref="ClientException">An error occurred.</exception>
        /// TODO: This method is not very flexible in terms of the order of messages ...
        public void Authenticate(int version, byte[] localIPAddress)
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

            this.SendCsAuthInfo(version, localIPAddress);
            var scPingMessage = this.ReceiveScPing(buffer);
            this.SendCsPing(scPingMessage.PingValue);
            var scAuthInfoMessage = this.ReceiveScAuthInfo(buffer);
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
        /// <param name="version">The version.</param>
        /// <param name="localIPAddress">The local IP address.</param>
        private void SendCsAuthInfo(int version, byte[] localIPAddress)
        {
            var csAuthInfomessage = SidMessageFactory.CreateClientToServerMessageFromHighLevelData(
                SidMessageType.AuthInfo,
                new object[] { ProductID.D2xp, version, localIPAddress });
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
    }
}
