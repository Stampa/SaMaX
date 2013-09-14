namespace SamaxLibrary
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.Contracts;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using MiscUtil.Conversion;

    /// <summary>
    /// This class is used to construct an array of bytes composing high-level data.
    /// </summary>
    public class ByteWriter
    {
        /// <summary>
        /// The underlying list of bytes to which bytes are written.
        /// </summary>
        private readonly List<byte> bytes;

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
        /// A converter that is used in the writing.
        /// </summary>
        private EndianBitConverter converter;

        /// <summary>
        /// Initializes a new instance of the <see cref="ByteWriter"/> class.
        /// </summary>
        /// <param name="writerShouldBeLittleEndian">Determines whether the byte writer should
        /// write chunks in little-endian.</param>
        public ByteWriter(bool writerShouldBeLittleEndian)
        {
            this.bytes = new List<byte>();
            this.converter = writerShouldBeLittleEndian ?
                (EndianBitConverter)new LittleEndianBitConverter() : new BigEndianBitConverter();
        }

        /// <summary>
        /// Appends a signed 32-bit integer to the bytes.
        /// </summary>
        /// <param name="value">The signed 32-bit integer to append to the bytes.</param>
        public void AppendInt32(Int32 value)
        {
            this.EnsuresSpecifiedAmountOfBytesAreWritten(4);

            byte[] bytesToAppend = this.converter.GetBytes(value);
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
            this.EnsuresSpecifiedAmountOfBytesAreWritten(value.Length);

            if (value == null)
            {
                throw new ArgumentNullException("value");
            }

            this.bytes.AddRange(value);
        }

        /// <summary>
        /// Appends an ASCII string to the bytes.
        /// </summary>
        /// <param name="value">The string whose bytes, when interpreted as a null-terminated ASCII
        /// string, to append to the bytes.</param>
        /// <exception cref="ArgumentNullException"><paramref name="value"/> is <c>null</c>.
        /// </exception>
        public void AppendNullTerminatedAsciiString(string value)
        {
            this.EnsuresSpecifiedAmountOfBytesAreWritten(value.Length + 1);

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
        /// Contains the object invariants for the <see cref="ByteWriter"/> class.
        /// </summary>
        [ContractInvariantMethod]
        private void ByteWriterInvariants()
        {
            Contract.Invariant(this.bytes != null);
            Contract.Invariant(this.converter != null);
        }

        /// <summary>
        /// Ensures that the specified amount of bytes are written.
        /// </summary>
        /// <param name="count">The amount of bytes that are written.</param>
        [ContractAbbreviator]
        private void EnsuresSpecifiedAmountOfBytesAreWritten(int count)
        {
            Contract.Ensures(this.bytes.Count == Contract.OldValue<int>(this.bytes.Count) + count);
            Contract.Ensures(this.Bytes.Length == Contract.OldValue<int>(this.Bytes.Length) + count);
        }
    }
}
