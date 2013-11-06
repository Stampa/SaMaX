namespace SamaxLibrary.Clients
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
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
        /// A trace source for writing sent and received packets to a log file.
        /// </summary>
        private static TraceSource traceSource;

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
        /// The part after the authentication check messages should probably be placed elsewhere
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
            Int32 serverToken = scAuthInfoMessage.ServerToken;
            var csAuthCheckMessage = this.SendCsAuthCheck(scAuthInfoMessage.MpqFileName, scAuthInfoMessage.ValueString, scAuthInfoMessage.ServerToken);
            Int32 clientToken = csAuthCheckMessage.ClientToken;
            this.ReceiveScAuthCheck(buffer);
            this.SendCsLogonResponse2(clientToken, serverToken);
            this.ReceiveScLogonResponse2(buffer);
            this.SendCsQueryRealms2();
            var scQueryRealms2Message = this.ReceiveScQueryRealms2(buffer);
            this.SendCsLogonRealmEx(clientToken, serverToken, scQueryRealms2Message.Realms[0].Title);
            var scLogonRealmExMessage = this.ReceiveScLogonRealmEx(buffer);
        }

        /// <summary>
        /// Reads the bytes of the next message from the specified stream.
        /// </summary>
        /// <param name="stream">The stream from which to read.</param>
        /// <param name="buffer">The buffer used to receive the bytes.</param>
        /// <returns>The bytes composing the next message in the stream.</returns>
        private byte[] GetMessageBytes(NetworkStream stream, byte[] buffer)
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

            byte[] messageBytes = buffer.Take(amountOfBytesRead).ToArray();
            traceSource.TraceData(TraceEventType.Verbose, 0, messageBytes);
            return messageBytes;
        }

        /// <summary>
        /// Sends a client-to-server authentication info message.
        /// </summary>
        private void SendCsAuthInfo()
        {
            var csAuthInfomessage = SidMessageFactory.CreateClientToServerMessageFromHighLevelData(
                SidMessageType.AuthInfo,
                new object[] { ProductID.D2xp, this.settings.Version, this.settings.LocalIPAddress.GetAddressBytes() });
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
            byte[] messageBytes = this.GetMessageBytes(this.stream, buffer);
            SidMessage message = SidMessageFactory.CreateServerToClientMessageFromBytes(messageBytes);
            this.ValidateScPing(message);
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
        /// <exception cref="ClientException">The received message contains unexpected data.
        /// </exception>
        private AuthInfoServerToClientSidMessage ReceiveScAuthInfo(byte[] buffer)
        {
            byte[] messageBytes = this.GetMessageBytes(this.stream, buffer);
            SidMessage message = SidMessageFactory.CreateServerToClientMessageFromBytes(messageBytes);
            this.ValidateScAuthInfo(message);
            return (AuthInfoServerToClientSidMessage)message;
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
        /// <returns>The authentication check message that was sent.</returns>
        private AuthCheckClientToServerSidMessage SendCsAuthCheck(
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
            return csAuthCheckMessage;
        }

        /// <summary>
        /// Receives a server-to-client authentication check message.
        /// </summary>
        /// <param name="buffer">The buffer used to receive the message bytes.</param>
        /// <returns>The received server-to-client authentication info message.</returns>
        /// <exception cref="ClientException">The received message contains unexpected data.
        /// </exception>
        private AuthCheckServerToClientSidMessage ReceiveScAuthCheck(byte[] buffer)
        {
            byte[] messageBytes = this.GetMessageBytes(this.stream, buffer);
            SidMessage message = SidMessageFactory.CreateServerToClientMessageFromBytes(messageBytes);
            this.ValidateScAuthCheck(message);
            return (AuthCheckServerToClientSidMessage)message;
        }

        /// <summary>
        /// Sends a client-to-server logon response (2) message.
        /// </summary>
        /// <param name="clientToken">The client token generated for the SID_AUTH_CHECK message.
        /// </param>
        /// <param name="serverToken">The server token received in the SID_AUTH_INFO message from
        /// the server.</param>
        private void SendCsLogonResponse2(Int32 clientToken, Int32 serverToken)
        {
            var message = LogonResponse2ClientToServerSidMessage.CreateFromHighLevelData(
                clientToken,
                serverToken,
                this.settings.AccountName,
                this.settings.Password);
            this.stream.Write(message.Bytes, 0, message.Bytes.Length);
        }

        /// <summary>
        /// Receives a server-to-client logon response (2) message.
        /// </summary>
        /// <param name="buffer">The buffer used to receive the message bytes.</param>
        /// <returns>The received server-to-client logon response (2) message.</returns>
        /// <exception cref="ClientException">The received message contains unexpected data.
        /// </exception>
        private LogonResponse2ServerToClientSidMessage ReceiveScLogonResponse2(byte[] buffer)
        {
            byte[] messageBytes = this.GetMessageBytes(this.stream, buffer);
            SidMessage message = SidMessageFactory.CreateServerToClientMessageFromBytes(messageBytes);
            this.ValidateScLogonResponse2(message);
            return (LogonResponse2ServerToClientSidMessage)message;
        }

        /// <summary>
        /// Sends a client-to-server query realms (2) message.
        /// </summary>
        private void SendCsQueryRealms2()
        {
            var message = QueryRealms2ClientToServerSidMessage.CreateFromHighLevelData();
            this.stream.Write(message.Bytes, 0, message.Bytes.Length);
        }

        /// <summary>
        /// Receives a server-to-client query realms (2) message.
        /// </summary>
        /// <param name="buffer">The buffer used to receive the message bytes.</param>
        /// <returns>The received server-to-client query realms (2) message.</returns>
        /// <exception cref="ClientException">The received message contains unexpected data.
        /// </exception>
        private QueryRealms2ServerToClientSidMessage ReceiveScQueryRealms2(byte[] buffer)
        {
            byte[] messageBytes = this.GetMessageBytes(this.stream, buffer);
            SidMessage message = SidMessageFactory.CreateServerToClientMessageFromBytes(messageBytes);
            this.ValidateScQueryRealms2(message);
            return (QueryRealms2ServerToClientSidMessage)message;
        }

        /// <summary>
        /// Sends a client-to-server logon realm ex message.
        /// </summary>
        /// <param name="clientToken">The client token generated for the SID_AUTH_CHECK message.
        /// </param>
        /// <param name="serverToken">The server token received in the SID_AUTH_INFO message from
        /// the server.</param>
        /// <param name="realmTitle">The realm title as received in the SID_QUERYREALMS2 message
        /// from the server.</param>
        private void SendCsLogonRealmEx(Int32 clientToken, Int32 serverToken, string realmTitle)
        {
            var message = LogonRealmExClientToServerSidMessage.CreateFromHighLevelData(
                clientToken,
                serverToken,
                realmTitle);
            this.stream.Write(message.Bytes, 0, message.Bytes.Length);
        }

        /// <summary>
        /// Receives a server-to-client logon realm ex message.
        /// </summary>
        /// <param name="buffer">The buffer used to receive the message bytes.</param>
        /// <returns>The received server-to-client logon realm ex message.</returns>
        /// <exception cref="ClientException">The received message contains unexpected data.
        /// </exception>
        private LogonRealmExServerToClientSidMessage ReceiveScLogonRealmEx(byte[] buffer)
        {
            byte[] messageBytes = this.GetMessageBytes(this.stream, buffer);
            SidMessage message = SidMessageFactory.CreateServerToClientMessageFromBytes(messageBytes);
            this.ValidateScLogonRealmEx(message);
            return (LogonRealmExServerToClientSidMessage)message;
        }

        /// <summary>
        /// Validates a server-to-client ping message by throwing exceptions if it contains
        /// unexpected data.
        /// </summary>
        /// <param name="message">The message to validate.</param>
        /// <exception cref="ClientException">The message contains unexpected data.</exception>
        private void ValidateScPing(SidMessage message)
        {
            if (message.MessageType != SidMessageType.Ping)
            {
                throw new ClientException(
                    String.Format(
                        "The message type ({0}) was not {1}.",
                        message.MessageType,
                        SidMessageType.Ping));
            }
        }

        /// <summary>
        /// Validates a server-to-client authentication info message by throwing exceptions if it
        /// contains unexpected data.
        /// </summary>
        /// <param name="message">The authentication info message to validate.</param>
        /// <exception cref="ClientException">The logon type is not broken SHA-1.</exception>
        private void ValidateScAuthInfo(SidMessage message)
        {
            if (message.MessageType != SidMessageType.AuthInfo)
            {
                throw new ClientException(
                    String.Format(
                        "The message type ({0}) was not {1}.",
                        message.MessageType,
                        SidMessageType.AuthInfo));
            }

            var csAuthInfoMessage = (AuthInfoServerToClientSidMessage)message;
            if (csAuthInfoMessage.LogonType != LogonType.BrokenSha1)
            {
                throw new ClientException(
                    String.Format(
                        "Received logon type was {0}, not {1}.",
                        csAuthInfoMessage.LogonType,
                        LogonType.BrokenSha1));
            }
        }

        /// <summary>
        /// Validates a server-to-client authentication check message by throwing exceptions if it
        /// contains unexpected data.
        /// </summary>
        /// <param name="message">The authentication check message to validate.</param>
        /// <exception cref="ClientException">The result is not 0.</exception>
        private void ValidateScAuthCheck(SidMessage message)
        {
            if (message.MessageType != SidMessageType.AuthCheck)
            {
                throw new ClientException(
                    String.Format(
                        "The message type ({0}) was not {1}.",
                        message.MessageType,
                        SidMessageType.AuthInfo));
            }

            var scAuthCheckMessage = (AuthCheckServerToClientSidMessage)message;
            if (scAuthCheckMessage.Result != 0)
            {
                throw new ClientException(
                    String.Format(
                        "The authentication was not successful. The result was {0}, not {1}.",
                        scAuthCheckMessage.Result,
                        0));
            }
        }

        /// <summary>
        /// Validates a server-to-client logon response (2) message by throwing exceptions if it
        /// contains unexpected data.
        /// </summary>
        /// <param name="message">The logon response (2) message to validate.</param>
        /// <exception cref="ClientException">The status is not "success".</exception>
        private void ValidateScLogonResponse2(SidMessage message)
        {
            if (message.MessageType != SidMessageType.LogonResponse2)
            {
                throw new ClientException(
                    String.Format(
                        "The message type ({0}) was not {1}.",
                        message.MessageType,
                        SidMessageType.LogonResponse2));
            }

            var scAuthCheckMessage = (LogonResponse2ServerToClientSidMessage)message;
            if (scAuthCheckMessage.Status != LogonResponse.Success)
            {
                throw new ClientException(
                    String.Format(
                        "Could not log onto the account. The response was {0}, not {1}.",
                        scAuthCheckMessage.Status,
                        LogonResponse.Success));
            }
        }

        /// <summary>
        /// Validates a server-to-client query realms (2) message by throwing exceptions if it
        /// contains unexpected data.
        /// </summary>
        /// <param name="message">The query realms (2) message to validate.</param>
        /// <exception cref="ClientException">There is not exactly one realm.</exception>
        private void ValidateScQueryRealms2(SidMessage message)
        {
            if (message.MessageType != SidMessageType.QueryRealms2)
            {
                throw new ClientException(
                    String.Format(
                        "The message type ({0}) was not {1}.",
                        message.MessageType,
                        SidMessageType.QueryRealms2));
            }

            var scQueryRealms2Message = (QueryRealms2ServerToClientSidMessage)message;
            if (scQueryRealms2Message.RealmCount != 1)
            {
                throw new ClientException(
                    String.Format(
                        "There were {0} realm(s) to which to connect, not {1}.",
                        scQueryRealms2Message.RealmCount,
                        1));
            }
        }

        /// <summary>
        /// Validates a server-to-client logon realm ex message by throwing exceptions if it
        /// contains unexpected data.
        /// </summary>
        /// <param name="message">The logon realm ex message to validate.</param>
        private void ValidateScLogonRealmEx(SidMessage message)
        {
            if (message.MessageType != SidMessageType.LogonRealmEx)
            {
                throw new ClientException(
                    String.Format(
                        "The message type ({0}) was not {1}.",
                        message.MessageType,
                        SidMessageType.LogonRealmEx));
            }

            var scLogonRealmExMessage = (LogonRealmExServerToClientSidMessage)message;
            if (!scLogonRealmExMessage.LogonWasSuccessful)
            {
                throw new ClientException(
                    String.Format(
                        "The logon was unsuccessful. The status code is {0}.",
                        scLogonRealmExMessage.McpStatus));
            }
        }
    }
}
