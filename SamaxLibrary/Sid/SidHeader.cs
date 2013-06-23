// -----------------------------------------------------------------------
// <copyright file="SidHeader.cs" company="TODO">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

namespace SamaxLibrary.Sid
{
    using System;
    using System.Collections.Generic;
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
        /// Gets the SID message type.
        /// </summary>
        public SidMessageType MessageType { get; private set; }

        /// <summary>
        /// Gets the message length.
        /// </summary>
        public UInt16 MessageLength { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="SidHeader"/> class.
        /// </summary>
        /// <param name="messageBytes">An array of bytes that composes the SID message whose header
        /// to construct.</param>
        /// <exception cref="ArgumentNullException"><paramref name="messageBytes"/> is <c>null</c>.
        /// </exception>
        /// <exception cref="ArgumentException"><paramref name="messageBytes"/> is too small to
        /// contain the SID header, or the SID header is invalid.</exception>
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

            if (messageBytes[0] != 0xFF)
            {
                throw new ArgumentException(
                    String.Format("The first byte of the header (0x{0:X}) is not 0xFF.", messageBytes[0]));
            }

            const int MessageTypeIndex = 1;
            SidMessageType messageType = (SidMessageType)messageBytes[MessageTypeIndex];
            if (!Enum.IsDefined(typeof(SidMessageType), messageType))
            {
                throw new ArgumentException(
                    String.Format(
                        "The byte corresponding to the SID message type ({0}) does not represent a valid SID message type.",
                        messageType),
                    "messageBytes");
            }

            this.MessageType = messageType;

            const int MessageLengthIndex = 2;
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

            this.MessageLength = supposedMessageLength;
        }
    }
}
