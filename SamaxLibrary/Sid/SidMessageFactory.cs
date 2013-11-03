namespace SamaxLibrary.Sid
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Reflection;
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
            if (messageBytes == null)
            {
                throw new ArgumentNullException("messageBytes");
            }

            SidMessageType messageType = SidMessage.GetSidMessageType(messageBytes);

            SidMessage message;
            switch (messageType)
            {
                case SidMessageType.AuthInfo:
                    message = new AuthInfoClientToServerSidMessage(messageBytes);
                    break;
                default:
                    Debug.Fail(
                        String.Format(
                            "Invalid SID message type for creating client-to-server message from bytes: {0}",
                            messageType));
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
                case SidMessageType.Ping:
                    message = new PingServerToClientSidMessage(messageBytes);
                    break;
                case SidMessageType.AuthInfo:
                    message = new AuthInfoServerToClientSidMessage(messageBytes);
                    break;
                case SidMessageType.AuthCheck:
                    message = new AuthCheckServerToClientSidMessage(messageBytes);
                    break;
                case SidMessageType.LogonResponse2:
                    message = new LogonResponse2ServerToClientSidMessage(messageBytes);
                    break;
                case SidMessageType.QueryRealms2:
                    message = new QueryRealms2ServerToClientSidMessage(messageBytes);
                    break;
                default:
                    Debug.Fail(
                        String.Format(
                            "Invalid SID message type for creating server-to-client message from bytes: {0}",
                            messageType));
                    message = null;
                    break;
            }

            return message;
        }

        /// <summary>
        /// Creates and returns an instance of the <see cref="SidMessage"/> class representing a
        /// client-to-server SID message of the specified type with the specified data.
        /// </summary>
        /// <param name="messageType">The SID message type of the SID message to create.</param>
        /// <param name="highLevelData">The high-level data from which to construct the SID
        /// message, or <c>null</c> if there is no high-level data.</param>
        /// <returns>An instance of the <see cref="SidMessage"/> class with the specified type and
        /// data.</returns>
        /// <exception cref="ArgumentException"><paramref name="messageType"/> is not a member of
        /// the <see cref="SidMessageType"/> enumeration, or <paramref name="highLevelData"/> does
        /// not contain valid high-level data for messages of the specified SID message type.
        /// </exception>
        public static SidMessage CreateClientToServerMessageFromHighLevelData(
            SidMessageType messageType,
            object[] highLevelData)
        {
            if (!Enum.IsDefined(typeof(SidMessageType), messageType))
            {
                throw new ArgumentException(
                    String.Format(
                        "The specified message type ({0}) is not a constant of the SidMessageType enumeration.",
                        messageType));
            }

            Type sidMessageClassType;
            switch (messageType)
            {
                case SidMessageType.AuthInfo:
                    sidMessageClassType = typeof(AuthInfoClientToServerSidMessage);
                    break;
                case SidMessageType.Ping:
                    sidMessageClassType = typeof(PingClientToServerSidMessage);
                    break;
                default:
                    Debug.Fail(
                        String.Format(
                            "Invalid SID message type for creating client-to-server message from high-level data: {0}",
                            messageType));
                    sidMessageClassType = null;
                    break;
            }

            MethodInfo createMethod = sidMessageClassType.GetMethod("CreateFromHighLevelData");

            Debug.Assert(
                createMethod != null,
                String.Format("Could not find the method Create in the type {0}.", sidMessageClassType));

            try
            {
                SidMessage message = (SidMessage)createMethod.Invoke(null, highLevelData);
                return message;
            }
            catch (TargetParameterCountException ex)
            {
                throw new ArgumentException(
                    String.Format(
                        "The array of high-level data contains an invalid amount of data ({0}).",
                        highLevelData.Length),
                    ex);
            }
            catch (ArgumentException ex)
            {
                //// TODO: Figure out how to make String.Format list all the types
                //// Possibly even list the expected types using reflection!
                throw new ArgumentException(
                    String.Format("The elements of the array of high-level data are of invalid type"),
                    ex);
            }
            catch (TargetInvocationException ex)
            {
                //// TODO: Is it safe to assume that ex.InnerException is the exception thrown by
                //// the SID message class?
                throw new ArgumentException(
                    String.Format(
                        "The SID message constructor threw an exception: {0}",
                        ex.InnerException.Message),
                    ex);
            }
        }
    }
}
