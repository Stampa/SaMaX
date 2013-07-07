namespace SamaxLibrary.Sid
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Text;
    using MiscUtil.Conversion;

    /// <summary>
    /// This class represents the header of a SID message, which contains information about the
    /// message type and the message length.
    /// </summary>
    /// <remarks>
    /// SID headers are 4 bytes long and of the following form:
    /// <list type="table">
    ///     <listheader>
    ///         <term>Byte 0</term>
    ///         <term>Byte 1</term>
    ///         <term>Byte 2-3</term>
    ///     </listheader>
    ///     <item>
    ///         <term>0xFF</term>
    ///         <term>Message type</term>
    ///         <term>Message length (in bytes, including this header)</term>
    ///     </item>
    /// </list>
    /// The message length is encoded in little-endian.
    /// </remarks>
    public class SidHeader
    {
        /// <summary>
        /// The index of the dummy value in the header.
        /// </summary>
        /// TODO: Is "dummy value" the right term?
        private const int DummyIndex = 0;

        /// <summary>
        /// The dummy value.
        /// </summary>
        private const byte DummyValue = 0xFF;

        /// <summary>
        /// The index of the message type in the header.
        /// </summary>
        private const int MessageTypeIndex = 1;

        /// <summary>
        /// The index of the first byte of the message length in the header.
        /// </summary>
        /// <remarks>The message length is encoded in little-endian, so this field specifies the
        /// least significant byte of the message length.</remarks>
        private const int MessageLengthIndex = 2;

        /// <summary>
        /// Gets the header length of SID messages, which is always 4.
        /// </summary>
        public static int HeaderLength
        {
            get
            {
                return 4;
            }
        }

        /// <summary>
        /// The bytes that compose the SID header.
        /// </summary>
        private readonly ReadOnlyCollection<byte> headerBytes;

        /// <summary>
        /// Gets the bytes that compose the SID header.
        /// </summary>
        public byte[] HeaderBytes
        {
            get
            {
                return this.headerBytes.ToArray();
            }
        }

        /// <summary>
        /// The SID message type.
        /// </summary>
        private readonly SidMessageType messageType;

        /// <summary>
        /// Gets the SID message type.
        /// </summary>
        public SidMessageType MessageType
        {
            get
            {
                return this.messageType;
            }
        }

        /// <summary>
        /// The message length, in bytes.
        /// </summary>
        private readonly UInt16 messageLength;

        /// <summary>
        /// Gets the message length, in bytes.
        /// </summary>
        public UInt16 MessageLength
        {
            get
            {
                return this.messageLength;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SidHeader"/> class from the bytes that
        /// compose the entire SID message, including the header.
        /// </summary>
        /// <param name="messageBytes">An array of bytes that composes the SID message whose header
        /// to construct.</param>
        /// <exception cref="ArgumentNullException"><paramref name="messageBytes"/> is <c>null</c>.
        /// </exception>
        /// <exception cref="ArgumentException"><paramref name="messageBytes"/> is too small to
        /// contain the SID header, or the SID header is invalid for the specified message.
        /// </exception>
        public SidHeader(byte[] messageBytes)
        {
            if (messageBytes == null)
            {
                throw new ArgumentNullException("messageBytes");
            }

            if (messageBytes.Length < HeaderLength)
            {
                throw new ArgumentException(
                    String.Format(
                        "The length of the array ({0}) is too small to contain the SID header.",
                        messageBytes.Length),
                    "messageBytes");
            }

            if (messageBytes[DummyIndex] != DummyValue)
            {
                throw new ArgumentException(
                    String.Format(
                        "The dummy value (0x{0:X}) is not 0x{1:X}.",
                        messageBytes[DummyIndex],
                        DummyValue));
            }

            SidMessageType messageType = (SidMessageType)messageBytes[MessageTypeIndex];
            if (!Enum.IsDefined(typeof(SidMessageType), messageType))
            {
                throw new ArgumentException(
                    String.Format(
                        "The byte corresponding to the SID message type ({0}) does not represent a valid SID message type.",
                        messageType),
                    "messageBytes");
            }

            this.messageType = messageType;

            LittleEndianBitConverter converter = new LittleEndianBitConverter();
            UInt16 supposedMessageLength = converter.ToUInt16(messageBytes, MessageLengthIndex);
            int actualMessageLength = messageBytes.Length;
            if (supposedMessageLength != actualMessageLength)
            {
                throw new ArgumentException(
                    String.Format(
                        "The message length as specified in the SID header ({0}) is different from the actual length ({1}).",
                        supposedMessageLength,
                        actualMessageLength));
            }

            this.messageLength = supposedMessageLength;

            byte[] headerBytes = new byte[HeaderLength];
            Array.Copy(messageBytes, headerBytes, HeaderLength);
            this.headerBytes = new ReadOnlyCollection<byte>(headerBytes);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SidHeader"/> class from the bytes that
        /// compose the data of a SID message of the specified SID message type.
        /// </summary>
        /// <param name="dataBytes">The data bytes of the SID message whose header to get.</param>
        /// <param name="messageType">The SID message type of the SID message.</param>
        /// <exception cref="ArgumentNullException"><paramref name="dataBytes"/> is <c>null</c>.
        /// </exception>
        /// <exception cref="ArgumentException"><paramref name="messageType"/> is not a constant of
        /// the <see cref="SidMessageType"/> enumeration, or the length of message (i.e. the length
        /// of the header plus the length of the data), in bytes, is greater than
        /// <see cref="UInt16.MaxValue"/>.</exception>
        public SidHeader(byte[] dataBytes, SidMessageType messageType)
        {
            if (!Enum.IsDefined(typeof(SidMessageType), messageType))
            {
                throw new ArgumentException(
                    String.Format(
                        "The message type ({0}) is not a valid SID message type.",
                        messageType));
            }

            this.messageType = messageType;

            int messageLength = dataBytes.Length + HeaderLength; // TODO: What if dataBytes.Length is Int32.MaxValue?
            if (messageLength > UInt16.MaxValue)
            {
                throw new ArgumentException(
                    String.Format(
                        "The length of the header and the data bytes ({0}) is greater than {1}.",
                        messageLength,
                        UInt16.MaxValue));
            }

            UInt16 shortMessageLength = (UInt16)messageLength;
            this.messageLength = shortMessageLength;

            this.headerBytes = GetHeaderBytes(messageType, shortMessageLength);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SidHeader"/> class from high-level data.
        /// </summary>
        /// <param name="messageType">The SID message type of the message whose header to
        /// construct.</param>
        /// <param name="messageLength">The length, in bytes, of the message whose header to
        /// construct.</param>
        /// <exception cref="ArgumentException"><paramref name="messageType"/> is not a constant of
        /// the <see cref="SidMessageType"/> enumeration.</exception>
        public SidHeader(SidMessageType messageType, UInt16 messageLength)
        {
            if (!Enum.IsDefined(typeof(SidMessageType), messageType))
            {
                throw new ArgumentException(
                    String.Format(
                        "The message type ({0}) is not a valid SID message type.",
                        messageType));
            }

            this.messageType = messageType;
            this.messageLength = messageLength;
            this.headerBytes = GetHeaderBytes(messageType, messageLength);
        }

        /// <summary>
        /// Returns a <see cref="ReadOnlyCollection{T}"/> containing the bytes of a header with the
        /// specified SID message type and message length.
        /// </summary>
        /// <param name="messageType">The SID message type.</param>
        /// <param name="messageLength">The message length, including the header.
        /// </param>
        /// <returns>A <see cref="ReadOnlyCollection{T}"/> containing the bytes of the header.
        /// </returns>
        private static ReadOnlyCollection<byte> GetHeaderBytes(
            SidMessageType messageType,
            UInt16 messageLength)
        {
            LittleEndianBitConverter converter = new LittleEndianBitConverter();
            byte[] headerBytes = new byte[HeaderLength];
            headerBytes[DummyIndex] = DummyValue;
            headerBytes[MessageTypeIndex] = (byte)messageType;
            converter.CopyBytes(messageLength, headerBytes, MessageLengthIndex);
            return new ReadOnlyCollection<byte>(headerBytes);
        }
    }
}
