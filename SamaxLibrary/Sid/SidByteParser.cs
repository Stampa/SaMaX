// -----------------------------------------------------------------------
// <copyright file="SidByteParser.cs" company="TODO">
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
    /// This class is used to parse the array of bytes that compose a SID message.
    /// </summary>
    public class SidByteParser
    {
        /// <summary>
        /// The array of bytes to parse.
        /// </summary>
        private readonly byte[] bytes;

        /// <summary>
        /// The index in the array of bytes from which to start reading the next piece of data.
        /// </summary>
        private int index;

        /// <summary>
        /// A converter that is used in the parsing.
        /// </summary>
        private LittleEndianBitConverter converter;

        /// <summary>
        /// Gets a value indicating whether there are any bytes left to parse.
        /// </summary>
        public bool HasBytesToParse
        {
            get
            {
                return this.AmountOfBytesLeft != 0;
            }
        }

        /// <summary>
        /// Gets the amount of bytes left to parse.
        /// </summary>
        private int AmountOfBytesLeft
        {
            get
            {
                return this.bytes.Length - this.index;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SidByteParser"/> class.
        /// </summary>
        /// <param name="bytesToParse">The array of bytes to parse.</param>
        public SidByteParser(byte[] bytesToParse)
        {
            this.bytes = bytesToParse;
            this.index = 4; // TODO: This would appear to be a magic number
            this.converter = new LittleEndianBitConverter();
        }

        /// <summary>
        /// Reads a signed 32-bit integer.
        /// </summary>
        /// <returns>The signed 32-bit integer that was read.</returns>
        /// <exception cref="SidByteParserException">There are fewer than 4 bytes left in the
        /// array of bytes to parse.</exception>
        public Int32 ReadInt32()
        {
            const int AmountOfBytesToRead = 4;
            if (this.AmountOfBytesLeft < AmountOfBytesToRead)
            {
                throw new SidByteParserException(
                    String.Format(
                        "There are too few bytes left ({0}) to read a 32-bit integer.",
                        this.AmountOfBytesLeft));
            }

            Int32 returnValue = this.converter.ToInt32(this.bytes, this.index);
            this.index += AmountOfBytesToRead;
            return returnValue;
        }

        /// <summary>
        /// Reads a signed 32-bit integer and interprets it as an enumeration value.
        /// </summary>
        /// <typeparam name="T">The enumeration type as which to interpret the integer value that
        /// is read.</typeparam>
        /// <returns>The enumeration constant corresponding to the signed 32-bit integer value that
        /// was read.</returns>
        /// <exception cref="ArgumentException"><typeparamref name="T"/> is not an enumeration
        /// type.</exception>
        /// <exception cref="SidByteParserException">There are fewer than 4 bytes left in the
        /// array of bytes to parse, or there is no constant in the enumeration with the signed
        /// 32-bit integer value that is read from the array of bytes.</exception>
        public T ReadInt32AsEnum<T>()
            where T : struct, IComparable, IConvertible, IFormattable
        {
            Type enumType = typeof(T);

            if (!enumType.IsEnum)
            {
                throw new ArgumentException("The type parameter is not an enumeration type.", "T");
            }

            // TODO: What if there is more than one constant for the value?
            Int32 value = this.ReadInt32();
            T enumValue = (T)Enum.ToObject(enumType, value);

            if (!Enum.IsDefined(enumType, enumValue))
            {
                throw new SidByteParserException("The integer value does not match any constant of the enumeration");
            }

            return enumValue;
        }

        /// <summary>
        /// Reads an unsigned 64-bit integer.
        /// </summary>
        /// <returns>The unsigned 64-bit integer that was read.</returns>
        /// <exception cref="SidByteParserException">There are fewer than 8 bytes left in the array
        /// of bytes to parse.</exception>
        public UInt64 ReadUInt64()
        {
            const int AmountOfBytesToRead = 8;
            if (this.AmountOfBytesLeft < AmountOfBytesToRead)
            {
                throw new SidByteParserException(
                    String.Format(
                        "There are too few bytes left ({0}) to read a 64-bit integer.",
                        this.AmountOfBytesLeft));
            }

            UInt64 returnValue = this.converter.ToUInt64(this.bytes, this.index);
            this.index += AmountOfBytesToRead;
            return returnValue;
        }

        /// <summary>
        /// Reads a null-terminated ASCII string.
        /// </summary>
        /// <returns>The string that was read.</returns>
        /// <remarks>The null terminator is not included in the string that is returned.</remarks>
        public string ReadAsciiString()
        {
            const byte NullTerminatorValue = 0;
            int indexOfTerminator = Array.IndexOf<byte>(this.bytes, NullTerminatorValue, this.index);
            if (indexOfTerminator == -1)
            {
                throw new SidByteParserException("The null terminator could not be found in the string.");
            }

            int amountOfBytesInNullTerminatedString = indexOfTerminator - this.index + 1;
            int amountOfBytesToTake = amountOfBytesInNullTerminatedString - 1;

            byte[] stringBytes = new byte[amountOfBytesToTake];
            Array.Copy(this.bytes, this.index, stringBytes, 0, amountOfBytesToTake);
            string returnValue = Encoding.ASCII.GetString(stringBytes); // TODO: Can exceptions be thrown here?
            
            this.index += amountOfBytesInNullTerminatedString;
            
            return returnValue;
        }
    }
}
