﻿namespace SamaxLibrary.Sid
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Diagnostics.Contracts;
    using System.Linq;
    using System.Text;
    using MiscUtil.Conversion;

    /// <summary>
    /// This class is used to construct an array of bytes that composes the data of a SID
    /// message. The array is constructed from high-level data.
    /// </summary>
    /// <seealso cref="SidByteParser"/>
    public class SidByteWriter
    {
        /// <summary>
        /// The underlying writer.
        /// </summary>
        private ByteWriter writer;

        /// <summary>
        /// Gets the array of bytes that have been written.
        /// </summary>
        public byte[] Bytes
        {
            get
            {
                return this.writer.Bytes;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SidByteWriter"/> class.
        /// </summary>
        public SidByteWriter()
        {
            this.writer = new ByteWriter(true);
        }

        /// <summary>
        /// Appends a signed 32-bit integer to the bytes.
        /// </summary>
        /// <param name="value">The signed 32-bit integer to append to the bytes.</param>
        /// <remarks>The value is encoded in little-endian.</remarks>
        public void AppendInt32(Int32 value)
        {
            this.EnsuresSpecifiedAmountOfBytesAreWritten(4);

            this.writer.AppendInt32(value);
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
            this.EnsuresSpecifiedAmountOfBytesAreWritten(4);

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
            Debug.Assert(
                bytesToAppend.Length == 4,
                "Encoded 4-character ASCII string into a byte array that was not of length 4.");
            Array.Reverse(bytesToAppend); // Go from big-endian to little-endian
            this.writer.AppendByteArray(bytesToAppend);
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
            this.EnsuresSpecifiedAmountOfBytesAreWritten(4);

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
        /// Appends an ASCII string to the bytes.
        /// </summary>
        /// <param name="value">The string to append to the bytes.</param>
        /// <exception cref="ArgumentNullException"><paramref name="value"/> is <c>null</c>.
        /// </exception>
        /// <remarks>A null terminator is appended as well, even if the string already has a null
        /// terminator.</remarks>
        public void AppendAsciiString(string value)
        {
            this.EnsuresSpecifiedAmountOfBytesAreWritten(value.Length + 1);

            if (value == null)
            {
                throw new ArgumentNullException("value");
            }

            this.writer.AppendNullTerminatedAsciiString(value);
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

            this.writer.AppendByteArray(value);
        }

        /// <summary>
        /// Appends a BSHA-1 hash to the bytes.
        /// </summary>
        /// <param name="value">The BSHA-1 hash to append to the bytes.</param>
        /// <exception cref="ArgumentNullException"><paramref name="value"/> is <c>null</c>.
        /// </exception>
        public void AppendBrokenSha1Hash(BrokenSha1Hash value)
        {
            this.EnsuresSpecifiedAmountOfBytesAreWritten(BrokenSha1Hash.HashSize);

            if (value == null)
            {
                throw new ArgumentNullException("value");
            }

            this.AppendByteArray(value.Bytes);
        }

        /// <summary>
        /// Contains the object invariants for the <see cref="SidByteWriter"/> class.
        /// </summary>
        [ContractInvariantMethod]
        private void SidByteWriterInvariants()
        {
            Contract.Invariant(this.writer != null);
        }

        /// <summary>
        /// Ensures that the specified amount of bytes are written.
        /// </summary>
        /// <param name="count">The amount of bytes that are written.</param>
        [ContractAbbreviator]
        private void EnsuresSpecifiedAmountOfBytesAreWritten(int count)
        {
            Contract.Ensures(this.Bytes.Length == Contract.OldValue<int>(this.Bytes.Length) + count);
        }
    }
}
