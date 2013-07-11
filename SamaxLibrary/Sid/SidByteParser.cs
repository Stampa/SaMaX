namespace SamaxLibrary.Sid
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using MiscUtil.Conversion;

    /// <summary>
    /// This class is used to parse the array of bytes that composes the data of a SID message.
    /// </summary>
    /// <remarks>
    /// <para>
    ///     In SID messages
    ///     <list type="bullet">
    ///         <item>strings are encoded in ASCII and are null-terminated, and</item>
    ///         <item>most chunks larger than one byte are encoded in little-endian.</item>
    ///     </list>
    /// </para>
    /// <para>
    ///     In the context of SID messages, dword strings are 4-byte (4-character) ASCII strings
    ///     without null terminator that are treated as an entire chunk and encoded in
    ///     little-endian.
    /// </para>
    /// </remarks>
    /// <seealso cref="SidByteWriter"/>
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
        /// <param name="bytesToParse">The array of bytes to parse, <em>including</em> the SID
        /// header.</param>
        /// <exception cref="ArgumentNullException"><paramref name="bytesToParse"/> is <c>null</c>.
        /// </exception>
        /// <exception cref="ArgumentException"><paramref name="bytesToParse"/> is too small to
        /// contain a SID header (see <see cref="SidHeader.HeaderLength"/>).</exception>
        public SidByteParser(byte[] bytesToParse)
        {
            if (bytesToParse == null)
            {
                throw new ArgumentNullException("bytesToParse");
            }

            if (bytesToParse.Length < SidHeader.HeaderLength)
            {
                throw new ArgumentException(
                    String.Format(
                        "The length of the byte array ({0}) is too small to contain a SID header",
                        bytesToParse.Length));
            }

            this.bytes = bytesToParse;
            this.index = SidHeader.HeaderLength;
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
                throw new SidByteParserException(
                    String.Format(
                        "The integer value ({0}) does not match any constant of the enumeration ({1})",
                        value,
                        enumType));
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
        /// Reads a byte array of the specified amount of elements.
        /// </summary>
        /// <param name="count">The amount of bytes to read.</param>
        /// <returns>The array of bytes that was read.</returns>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="count"/> is less than
        /// zero.</exception>
        /// <exception cref="SidByteParserException">There are fewer than <paramref name="count"/>
        /// bytes left in the array of bytes to parse.</exception>
        public byte[] ReadByteArray(int count)
        {
            if (count < 0)
            {
                throw new ArgumentOutOfRangeException("count");
            }

            if (count > this.AmountOfBytesLeft)
            {
                throw new SidByteParserException(
                    String.Format(
                        "There are too few bytes left ({0}) to read {1} bytes.",
                        this.AmountOfBytesLeft,
                        count));
            }

            byte[] returnValue = new byte[count];
            Array.Copy(this.bytes, this.index, returnValue, 0, count);

            this.index += count;

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
            //// TODO: Make sure that the array does not contain any non-ASCII symbol?
            //// Can the GetString method throw any exception?
            string returnValue = Encoding.ASCII.GetString(stringBytes);
            
            this.index += amountOfBytesInNullTerminatedString;
            
            return returnValue;
        }

        /// <summary>
        /// Reads a dword string.
        /// </summary>
        /// <returns>The dword string that was read.</returns>
        /// <exception cref="SidByteParserException">There are fewer than 4 bytes left in the array
        /// of bytes to parse.</exception>
        public string ReadDwordString()
        {
            const int AmountOfBytesToRead = 4;
            if (this.AmountOfBytesLeft < AmountOfBytesToRead)
            {
                throw new SidByteParserException(
                    String.Format(
                        "There are too few bytes left ({0}) to read a dword string.",
                        this.AmountOfBytesLeft));
            }

            byte[] stringBytes = this.ReadByteArray(AmountOfBytesToRead); // TODO: Obvious but magic number
            Array.Reverse(stringBytes); // Go from big-endian to little-endian
            //// TODO: Make sure that the array does not contain any non-ASCII symbol?
            //// Can the GetString method throw any exception?
            string returnValue = Encoding.ASCII.GetString(stringBytes);

            return returnValue;
        }

        /// <summary>
        /// Reads a dword string and parses it as an enumeration value.
        /// </summary>
        /// <typeparam name="T">The enumeration type as which to parse the dword string that is
        /// read.</typeparam>
        /// <returns>The enumeration constant that the dword string that was read represents.</returns>
        /// <exception cref="ArgumentException"><typeparamref name="T"/> is not an enumeration
        /// type.</exception>
        /// <exception cref="SidByteParserException">There are fewer than 4 bytes left in the
        /// array of bytes to parse, or there is no constant in the enumeration whose string
        /// representation is the dword string that is read from the array of bytes.</exception>
        public T ReadDwordStringAsEnum<T>()
            where T : struct, IComparable, IConvertible, IFormattable
        {
            Type enumType = typeof(T);

            if (!enumType.IsEnum)
            {
                throw new ArgumentException("The type parameter is not an enumeration type.", "T");
            }

            string value = this.ReadDwordString();

            try
            {
                T enumValue = (T)Enum.Parse(enumType, value, ignoreCase: true);
                return enumValue;
            }
            catch (ArgumentException ex)
            {
                throw new SidByteParserException(
                    String.Format(
                        "The dword string that was read ({0}) does not represent any constant of the enumeration ({1}).", // Add {1}
                        value,
                        enumType),
                    ex);
            }
        }
    }
}
