namespace SamaxLibrary.Sid
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Text;
    using MiscUtil.Conversion;

    /// <summary>
    /// This class is used to construct an array of bytes that composes the data of a SID
    /// message. The array is constructed from high-level data.
    /// </summary>
    /// <see cref="SidByteParser"/>
    public class SidByteWriter
    {
        /// <summary>
        /// A converter that is used in the writing.
        /// </summary>
        private LittleEndianBitConverter converter;

        /// <summary>
        /// The underlying list of bytes to which bytes are written.
        /// </summary>
        private List<byte> bytes;

        /// <summary>
        /// Gets the array of bytes that have been written.
        /// </summary>
        public byte[] Bytes
        {
            get
            {
                return this.bytes.ToArray();
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SidByteWriter"/> class.
        /// </summary>
        public SidByteWriter()
        {
            this.bytes = new List<byte>();
            this.converter = new LittleEndianBitConverter();
        }

        /// <summary>
        /// Appends a signed 32-bit integer to the bytes.
        /// </summary>
        /// <param name="value">The signed 32-bit integer to append to the bytes.</param>
        /// <remarks>The value is encoded in little-endian.</remarks>
        public void AppendInt32(Int32 value)
        {
            byte[] bytesToAppend = this.converter.GetBytes(value);
            this.bytes.AddRange(bytesToAppend);
        }

        /// <summary>
        /// Appends an enumeration value interpreted as a dword string to the bytes.
        /// </summary>
        /// <param name="value">The enumeration value to append to the bytes.</param>
        /// <exception cref="ArgumentNullException"><paramref name="value"/> is <c>null</c>.
        /// </exception>
        /// <exception cref="ArgumentException"><paramref name="value"/> is not a constant in its
        /// enumeration, or the string representation of <paramref name="value"/> is not of length
        /// 4.</exception>
        public void AppendEnumAsDwordString(Enum value)
        {
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }

            Type enumType = value.GetType();
            if (!Enum.IsDefined(enumType, value))
            {
                throw new ArgumentException(
                    String.Format(
                        "The enumeration value to append ({0}) is not a constant of the enumeration ({1}).",
                        value,
                        enumType));
            }

            string valueString = value.ToString().ToUpper();
            this.AppendDwordString(valueString);
        }

        /// <summary>
        /// Appends a dword string to the bytes.
        /// </summary>
        /// <param name="value">The string to append to the bytes.</param>
        /// <exception cref="ArgumentNullException"><paramref name="value"/> is <c>null</c>.
        /// </exception>
        /// <exception cref="ArgumentException"><paramref name="value"/> is not of length 4.
        /// </exception>
        public void AppendDwordString(string value)
        {
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }

            if (value.Length != 4)
            {
                throw new ArgumentException(
                    String.Format(
                        "The string '{0}' is of length {1}, not 4.",
                        value,
                        value.Length),
                    "value");
            }

            // TODO: Validate that it contains only ASCII symbols?
            // Non-ASCII symbols seem to turn into question marks (value 63)
            // Validate that it consists of only 4 bytes (with an exception, not just an assert)?
            byte[] bytesToAppend = Encoding.ASCII.GetBytes(value);
            Array.Reverse(bytesToAppend); // Go from big-endian to little-endian
            Debug.Assert(bytesToAppend.Length == 4, "Dword strings are of length 4.");
            this.bytes.AddRange(bytesToAppend);
        }

        /// <summary>
        /// Appends a string to the bytes.
        /// </summary>
        /// <param name="value">The string to append to the bytes.</param>
        /// <exception cref="ArgumentNullException"><paramref name="value"/> is <c>null</c>.
        /// </exception>
        public void AppendString(string value)
        {
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }

            //// TODO: Validate that the string contains only ASCII symbols?
            //// Non-ASCII symbols seem to turn into question marks (value 63)
            string nullTerminatedValue = value + '\0';
            byte[] bytesToAppend = Encoding.ASCII.GetBytes(nullTerminatedValue);
            this.bytes.AddRange(bytesToAppend);
        }

        /// <summary>
        /// Appends an array of bytes to the bytes.
        /// </summary>
        /// <param name="value">The array of bytes to append to the bytes.</param>
        /// <exception cref="ArgumentNullException"><paramref name="value"/> is <c>null</c>.
        /// </exception>
        public void AppendByteArray(byte[] value)
        {
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }

            this.bytes.AddRange(value);
        }
    }
}
