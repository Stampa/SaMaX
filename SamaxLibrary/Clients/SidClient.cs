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

    /// <summary>
    /// This class represents a client for the SID protocol.
    /// </summary>
    /// <seealso cref="SamaxLibrary.Sid"/>
    public class SidClient
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
        /// Initializes a new instance of the <see cref="SidClient"/> class.
        /// </summary>
        public SidClient()
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
        /// <param name="productID">The product ID.</param>
        /// <param name="version">The version.</param>
        /// <param name="localIPAddress">The local IP address to state.</param>
        /// <exception cref="InvalidOperationException">The client is not connected.</exception>
        /// <exception cref="ClientException">An error occurred.</exception>
        /// TODO: This method is not very flexible in terms of the order of messages ...
        public void Authenticate(ProductID productID, int version, byte[] localIPAddress)
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

            this.SendCsAuthInfo(productID, version, localIPAddress);
            var scPingMessage = this.ReceiveScPing(buffer);
            this.SendCsPing(scPingMessage.PingValue);
        }

        /// <summary>
        /// Sends a client-to-server authentication info message.
        /// </summary>
        /// <param name="productID">The product ID</param>
        /// <param name="version">The version.</param>
        /// <param name="localIPAddress">The local IP address.</param>
        private void SendCsAuthInfo(ProductID productID, int version, byte[] localIPAddress)
        {
            var csAuthInfomessage = AuthInfoClientToServerSidMessage.CreateFromHighLevelData(
                productID,
                version,
                localIPAddress);
            this.stream.Write(csAuthInfomessage.Bytes, 0, csAuthInfomessage.Bytes.Length);
        }

        /// <summary>
        /// Receives a server-to-client ping message.
        /// </summary>
        /// <param name="buffer">The buffer used to receive the message bytes.</param>
        /// <returns>The received server-to-client ping message</returns>
        private PingServerToClientSidMessage ReceiveScPing(byte[] buffer)
        {
            int count = this.stream.Read(buffer, 0, buffer.Length);
            byte[] messageBytes = buffer.Take(count).ToArray();
            SidMessage message = SidMessageFactory.CreateServerToClientMessageFromBytes(messageBytes);

            if (message.MessageType != SidMessageType.Ping)
            {
                throw new ClientException(
                    String.Format(
                        "The message type ({0}) was not the ping type.",
                        message.MessageType));
            }

            return (PingServerToClientSidMessage)message;
        }

        /// <summary>
        /// Sends a client-to-server ping message. 
        /// </summary>
        /// <param name="pingValue">The ping value received from the server earlier.</param>
        private void SendCsPing(Int32 pingValue)
        {
            var csPingMessage = PingClientToServerSidMessage.CreateFromHighLevelData(pingValue);
            this.stream.Write(csPingMessage.Bytes, 0, csPingMessage.Bytes.Length);
        }
    }
}
