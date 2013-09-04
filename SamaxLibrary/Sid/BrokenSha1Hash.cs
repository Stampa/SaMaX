namespace SamaxLibrary.Sid
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Text;
    using System.Threading.Tasks;
    using MiscUtil.Conversion;

    /// <summary>
    /// This immutable class represents a hash obtained using Blizzard's broken SHA-1 algorithm.
    /// </summary>
    public class BrokenSha1Hash
    {
        /// <summary>
        /// The size, in bytes, of a broken SHA-1 hash.
        /// </summary>
        public const int HashSize = 4 * 5;

        /// <summary>
        /// The bytes that compose the hash code.
        /// </summary>
        private readonly IReadOnlyList<byte> bytes;

        /// <summary>
        /// Gets an array of bytes that compose the hash code.
        /// </summary>
        public byte[] Bytes
        {
            get
            {
                return this.bytes.ToArray();
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BrokenSha1Hash"/> class.
        /// </summary>
        /// <param name="hashBytes">The bytes that compose the hash code.</param>
        /// <exception cref="ArgumentNullException"><paramref name="hashBytes"/> is <c>null</c>.
        /// </exception>
        /// <exception cref="ArgumentException"><paramref name="hashBytes"/> is not of length
        /// <see cref="HashSize"/>.</exception>
        public BrokenSha1Hash(byte[] hashBytes)
        {
            if (hashBytes == null)
            {
                throw new ArgumentNullException("hashBytes");
            }

            if (hashBytes.Length != HashSize)
            {
                throw new ArgumentException(
                    String.Format(
                        "The length of the hash bytes ({0}) is not {1}.",
                        hashBytes.Length,
                        HashSize));
            }

            this.bytes = new ReadOnlyCollectionBuilder<byte>(hashBytes).ToReadOnlyCollection();
        }

        /// <summary>
        /// Determines whether this instance and a specified object has the same value.
        /// </summary>
        /// <param name="obj">The object to compare to this instance.</param>
        /// <returns><c>true</c> if <paramref name="obj"/> is a <see cref="BrokenSha1Hash"/> with
        /// the same value as this instance; otherwise, <c>false</c>.</returns>
        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }

            var otherHash = obj as BrokenSha1Hash;
            if (object.ReferenceEquals(otherHash, null))
            {
                return false;
            }

            return this.Equals(otherHash);
        }

        /// <summary>
        /// Determines whether this instance and another specified <see cref="BrokenSha1Hash"/>
        /// object has the same value.
        /// </summary>
        /// <param name="otherHash">The <see cref="BrokenSha1Hash"/> object to compare to this
        /// instance.</param>
        /// <returns><c>true</c> if <paramref name="otherHash"/> has the same value as this
        /// instance; otherwise, <c>false</c>.</returns>
        public bool Equals(BrokenSha1Hash otherHash)
        {
            if (otherHash == null)
            {
                return false;
            }

            return this.bytes.SequenceEqual(otherHash.bytes);
        }

        /// <summary>
        /// Returns the hash code for this <see cref="BrokenSha1Hash"/> instance.
        /// </summary>
        /// <returns>A 32-bit signed integer hash code.</returns>
        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = 17;
                foreach (byte hashByte in this.bytes)
                {
                    hashCode = (hashCode * 29) + hashByte;
                }

                return hashCode;
            }
        }
    }
}
