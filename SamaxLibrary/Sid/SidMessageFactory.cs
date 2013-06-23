// -----------------------------------------------------------------------
// <copyright file="SidMessageFactory.cs" company="TODO">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

namespace SamaxLibrary.Sid
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Text;

    /// <summary>
    /// This class is used to create instances of the <see cref="SidMessage"/> class. In other
    /// words, it is used to create messages of the SID protocol.
    /// </summary>
    public static class SidMessageFactory
    {
        /// <summary>
        /// Creates and returns an instance of the <see cref="SidMessage"/> class from the bytes
        /// that compose the message.
        /// </summary>
        /// <param name="messageBytes">The bytes that compose the message to create.</param>
        /// <param name="messageDirection">The direction of the message.</param>
        /// <returns>An instance of the <see cref="SidMessage"/> class that contains the specified
        /// bytes.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="messageBytes"/> is <c>null</c>.
        /// </exception>
        /// <exception cref="ArgumentException"><paramref name="messageDirection"/> is not a
        /// constant of the <see cref="MessageDirection"/> enumeration, or
        /// <paramref name="messageBytes"/> does not compose a valid SID message in the specified
        /// direction.</exception>
        public static SidMessage CreateMessageFromBytes(byte[] messageBytes, MessageDirection messageDirection)
        {
            if (messageBytes == null)
            {
                throw new ArgumentNullException("messageBytes");
            }

            if (!Enum.IsDefined(typeof(MessageDirection), messageDirection))
            {
                throw new ArgumentException(
                    String.Format("The message direction ({0}) is invalid.", messageDirection),
                    "messageDirection");
            }

            if (messageDirection == MessageDirection.ClientToServer)
            {
                return SidMessageFactory.CreateClientToServerMessageFromBytes(messageBytes);
            }
            else
            {
                return SidMessageFactory.CreateServerToClientMessageFromBytes(messageBytes);
            }
        }

        /// <summary>
        /// Creates and returns an instance of the <see cref="SidMessage"/> class representing a
        /// client-to-server SID message from the bytes that compose said message.
        /// </summary>
        /// <param name="messageBytes">The bytes that compose the message to create.</param>
        /// <returns>An instance of the <see cref="SidMessage"/> class that contains the specified
        /// bytes.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="messageBytes"/> is <c>null</c>.
        /// </exception>
        /// <exception cref="ArgumentException"><paramref name="messageBytes"/> does not compose a
        /// valid client-to-server SID message.</exception>
        public static SidMessage CreateClientToServerMessageFromBytes(byte[] messageBytes)
        {
            SidMessageType messageType = SidMessage.GetSidMessageType(messageBytes);

            SidMessage message;
            switch (messageType)
            {
                case SidMessageType.AuthInfo:
                    message = new AuthInfoClientToServerSidMessage(messageBytes);
                    break;
                default:
                    Debug.Fail(String.Format("Invalid SID message type for client-to-server message: {0}", messageType));
                    message = null;
                    break;
            }

            return message;
        }

        /// <summary>
        /// Creates and returns an instance of the <see cref="SidMessage"/> class representing a
        /// server-to-client SID message from the bytes that compose said message.
        /// </summary>
        /// <param name="messageBytes">The bytes that compose the message to create.</param>
        /// <returns>An instance of the <see cref="SidMessage"/> class that contains the specified
        /// bytes.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="messageBytes"/> is <c>null</c>.
        /// </exception>
        /// <exception cref="ArgumentException"><paramref name="messageBytes"/> does not compose a
        /// valid server-to-client SID message.</exception>
        public static SidMessage CreateServerToClientMessageFromBytes(byte[] messageBytes)
        {
            SidMessageType messageType = SidMessage.GetSidMessageType(messageBytes);

            SidMessage message;
            switch (messageType)
            {
                case SidMessageType.AuthInfo:
                    message = new AuthInfoServerToClientSidMessage(messageBytes);
                    break;
                default:
                    Debug.Fail(String.Format("Invalid SID message type for server-to-client message: {0}", messageType));
                    message = null;
                    break;
            }

            return message;
        }
    }
}
