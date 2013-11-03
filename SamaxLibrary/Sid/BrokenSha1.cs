namespace SamaxLibrary.Sid
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using MiscUtil.Conversion;

    /* TODO
     * Make sure that little endian is the default endianness
     *      Done!
     * 
     * Take endianness in the constructor
     *      This doesn't make sense? The input is a _byte_ array!
     * 
     * Subclass HashAlgorithm
     *      This would likely need the class to be made non-static (which is a good thing)
     * 
     * Magic numbers
     * 
     * In the ComputeHash method, consider putting h0 through h4 into an array and looping
     * 
     * Consider making incremental updates of the input bytes like the Python code
     *      This would never be of use though?
     */

    /* Notes:
     * Replace BrokenRotateLeft with RotateLeft and uncomment code tagged with "Unbroken code:"
     * (commenting out the corresponding broken code of course) to get the regular, unbroken SHA-1
     * 
     * For powers of 2 (of the divisor), modulo can be implemented using bitwise AND.
     */

    /// <summary>
    /// This class implements Blizzard's broken variant of the SHA-1 algorithm.
    /// </summary>
    public static class BrokenSha1
    {
        /// <summary>
        /// Computes the hash value for the specified byte array.
        /// </summary>
        /// <param name="buffer">The input to compute the hash code for.</param>
        /// <returns>The computed hash code.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="buffer"/> is <c>null</c>.
        /// </exception>
        public static byte[] ComputeHash(byte[] buffer)
        {
            if (buffer == null)
            {
                throw new ArgumentNullException("buffer");
            }

            buffer = BrokenSha1.PreProcessBuffer(buffer);

            UInt32[][] chunkedBuffer = BrokenSha1.ChunkBuffer(buffer);

            uint h0 = 0x67452301;
            uint h1 = 0xefcdab89;
            uint h2 = 0x98badcfe;
            uint h3 = 0x10325476;
            uint h4 = 0xc3d2e1f0;

            foreach (UInt32[] chunk in chunkedBuffer)
            {
                for (int i = 16; i < chunk.Length; i++)
                {
                    chunk[i] = (chunk[i - 3] ^ chunk[i - 8] ^ chunk[i - 14] ^ chunk[i - 16]).BrokenRotateLeft(1);
                }

                uint a = h0;
                uint b = h1;
                uint c = h2;
                uint d = h3;
                uint e = h4;
                uint f;
                uint k;

                for (int i = 0; i < chunk.Length; i++)
                {
                    if (i < 20)
                    {
                        f = (b & c) | (~b & d);
                        k = 0x5a827999;
                    }
                    else if (i < 40)
                    {
                        f = b ^ c ^ d;
                        k = 0x6ed9eba1;
                    }
                    else if (i < 60)
                    {
                        f = (b & c) | (b & d) | (c & d);
                        k = 0x8f1bbcdc;
                    }
                    else
                    {
                        f = b ^ c ^ d;
                        k = 0xca62c1d6;
                    }

                    uint temp = a.RotateLeft(5) + f + e + k + chunk[i];
                    e = d;
                    d = c;
                    c = b.RotateLeft(30);
                    b = a;
                    a = temp;
                }

                h0 = h0 + a;
                h1 = h1 + b;
                h2 = h2 + c;
                h3 = h3 + d;
                h4 = h4 + e;
            }

            byte[] hash = new byte[20];

            // Unbroken code:
            ////BigEndianBitConverter converter = new BigEndianBitConverter();
            LittleEndianBitConverter converter = new LittleEndianBitConverter();
            Array.Copy(converter.GetBytes(h0), 0, hash, 0, 4);
            Array.Copy(converter.GetBytes(h1), 0, hash, 4, 4);
            Array.Copy(converter.GetBytes(h2), 0, hash, 8, 4);
            Array.Copy(converter.GetBytes(h3), 0, hash, 12, 4);
            Array.Copy(converter.GetBytes(h4), 0, hash, 16, 4);

            return hash;
        }

        /// <summary>
        /// Computes a "tokenized" hash of a password.
        /// </summary>
        /// <param name="clientToken">The client token.</param>
        /// <param name="serverToken">The server token.</param>
        /// <param name="password">The password.</param>
        /// <returns>The "tokenized" password hash for the specified client token, server token,
        /// and password.</returns>
        /// <remarks>Informally, the password hash is computed as follows:
        /// <code>
        ///     hash = Bsha(<paramref name="clientToken"/>, <paramref name="serverToken"/>,
        ///     Bsha(<paramref name="password"/>))
        /// </code>
        /// </remarks>
        public static BrokenSha1Hash ComputeTokenizedHash(
            Int32 clientToken,
            Int32 serverToken,
            string password)
        {
            byte[] firstPassBytes = BrokenSha1.ComputeHashOfAsciiString(password);

            ByteWriter writer = new ByteWriter(true);
            writer.AppendInt32(clientToken);
            writer.AppendInt32(serverToken);
            writer.AppendByteArray(firstPassBytes);
            byte[] secondPassBytes = BrokenSha1.ComputeHash(writer.Bytes);

            return new BrokenSha1Hash(secondPassBytes);
        }

        /// <summary>
        /// Computes the hash value for an ASCII string.
        /// </summary>
        /// <param name="asciiString">The ASCII string for which to compute the hash code.</param>
        /// <returns>The computed hash code.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="asciiString"/> is <c>null</c>.
        /// </exception>
        /// <remarks>This method is case-insensitive.</remarks>
        public static byte[] ComputeHashOfAsciiString(string asciiString)
        {
            if (asciiString == null)
            {
                throw new ArgumentNullException("asciiString");
            }

            byte[] stringBytes = Encoding.ASCII.GetBytes(asciiString.ToLower());
            return BrokenSha1.ComputeHash(stringBytes);
        }

        /// <summary>
        /// Pre-processes the buffer by appending a one, possibly several zeros, and the size of
        /// the buffer in bits.
        /// </summary>
        /// <param name="buffer">The buffer to pre-process.</param>
        /// <returns>The pre-processed buffer.</returns>
        /// <remarks>The broken variant appends only zeros.</remarks>
        private static byte[] PreProcessBuffer(byte[] buffer)
        {
            // This buffer size is in bytes
            int bufferSizeAfterAppendingFirstByte = buffer.Length + 1;
            int additionalAmountOfBytesToAdd = (-bufferSizeAfterAppendingFirstByte).Mod(64);
            int totalAmountOfBytesToAdd = additionalAmountOfBytesToAdd + 1;
            byte[] preProcessedBuffer = new byte[buffer.Length + totalAmountOfBytesToAdd];

            // TODO: Use .CopyTo instead? Probably in other places as well!
            Array.Copy(buffer, 0, preProcessedBuffer, 0, buffer.Length);

            // Unbroken code:
            ////preProcessedBuffer[buffer.Length] = 0x80;
            
            ////Int64 bufferSizeInBits = buffer.Length * 8;

            ////BigEndianBitConverter converter = new BigEndianBitConverter();
            ////byte[] bufferSizeBytes = converter.GetBytes(bufferSizeInBits);

            ////bufferSizeBytes.CopyTo(preProcessedBuffer, preProcessedBuffer.Length - bufferSizeBytes.Length);
            
            return preProcessedBuffer;
        }

        /// <summary>
        /// Chunks the specified buffer into chunks of 64 bytes each.
        /// </summary>
        /// <param name="buffer">The buffer to chunk.</param>
        /// <returns>An array of chunks of 64 bytes (16 dwords) of the buffer.</returns>
        /// <exception cref="ArgumentException">The length of <paramref name="buffer"/> is not a
        /// multiple of 64.</exception>
        private static UInt32[][] ChunkBuffer(byte[] buffer)
        {
            const int ChunkSizeInBytes = 64;
            const int ChunkSizeInDwords = ChunkSizeInBytes / 4;
            if (buffer.Length % ChunkSizeInBytes != 0)
            {
                throw new ArgumentException(String.Format("Invalid buffer size ({0}).", buffer.Length));
            }

            int amountOfChunks = buffer.Length / ChunkSizeInBytes;
            UInt32[][] chunkedBuffer = new UInt32[amountOfChunks][];

            // Unbroken code:
            ////BigEndianBitConverter converter = new BigEndianBitConverter();
            LittleEndianBitConverter converter = new LittleEndianBitConverter();

            for (int indexOfChunk = 0; indexOfChunk < chunkedBuffer.Length; indexOfChunk++)
            {
                const int ExtendedChunkSizeInDwords = 80;
                chunkedBuffer[indexOfChunk] = new UInt32[ExtendedChunkSizeInDwords];
                for (int indexOfChunkDword = 0; indexOfChunkDword < ChunkSizeInDwords; indexOfChunkDword++)
                {
                    int bufferIndexOfBytes = (indexOfChunk * ChunkSizeInBytes) + (32 / 8 * indexOfChunkDword);
                    
                    chunkedBuffer[indexOfChunk][indexOfChunkDword] = converter.ToUInt32(buffer, bufferIndexOfBytes);
                }
            }

            return chunkedBuffer;
        }

        /// <summary>
        /// Rotates a <see cref="UInt32"/> value to the left. That is, the value is circularly
        /// shifted to the left.
        /// </summary>
        /// <param name="value">The value to rotate to the left.</param>
        /// <param name="count">The amount of bits to rotate.</param>
        /// <returns><paramref name="value"/> rotated <paramref name="count"/> bits to the left.
        /// </returns>
        private static UInt32 RotateLeft(this UInt32 value, int count)
        {
            return (value << count) | (value >> (32 - count));
        }

        /// <summary>
        /// Blizzard's broken left-rotation, which is a normal left-rotation but with swapped parameters. 
        /// </summary>
        /// <param name="value">The value that should be rotated to the left, but which is
        /// interpreted as the amount of bits to rotate.</param>
        /// <param name="count">The amount of bits that should be rotated, but which is interpreted
        /// as the value that should be rotated.</param>
        /// <returns>The result of the broken left-rotation of <paramref name="value"/> by
        /// <paramref name="count"/> bits.</returns>
        /// TODO: The behavior when count is negative (reinterpreting the Int32 as a UInt32) might
        /// not be good, but count is 1 in all method calls as of this writing.
        private static UInt32 BrokenRotateLeft(this UInt32 value, int count)
        {
            int actualCount = (int)value.Mod(32);
            uint actualValue = (uint)count;
            return actualValue.RotateLeft(actualCount);
        }

        /// <summary>
        /// Calculates the least non-negative remainder of a value modulo another value.
        /// </summary>
        /// <param name="a">The dividend.</param>
        /// <param name="n">The divisor.</param>
        /// <returns><paramref name="a"/> mod <paramref name="n"/>. In other words, the
        /// least non-negative remainder of <paramref name="a"/> mod <paramref name="n"/></returns>
        /// <exception cref="DivideByZeroException"><paramref name="n"/> is <c>0</c>.</exception>
        private static int Mod(this int a, int n)
        {
            if (n == 0)
            {
                throw new DivideByZeroException();
            }

            int b = a % n;
            if (b < 0)
            {
                b = b + n;
            }

            return b;
        }

        /// <summary>
        /// Calculates the least remainder of an unsigned value modulo another unsigned value.
        /// </summary>
        /// <param name="a">The dividend.</param>
        /// <param name="n">The divisor.</param>
        /// <returns><paramref name="a"/> mod <paramref name="n"/>. In other words, the least
        /// remainder of <paramref name="a"/> mod <paramref name="n"/>.</returns>
        /// <exception cref="DivideByZeroException"><paramref name="n"/> is <c>0</c>.</exception>
        private static uint Mod(this uint a, uint n)
        {
            if (n == 0)
            {
                throw new DivideByZeroException();
            }

            uint b = a % n;
            return b;
        }
    }
}