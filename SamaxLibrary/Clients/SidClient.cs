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
    }
}
