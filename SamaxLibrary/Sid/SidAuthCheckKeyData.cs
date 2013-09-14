namespace SamaxLibrary.Sid
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    /// <summary>
    /// This immutable class represents the data of one CD key in a SID_AUTH_CHECK message.
    /// </summary>
    public class SidAuthCheckKeyData
    {
        /// <summary>
        /// The length of the key data in bytes.
        /// </summary>
        public const Int32 LengthInBytes = (4 * 4) + BrokenSha1Hash.HashSize;

        /// <summary>
        /// Gets the length of the CD key.
        /// </summary>
        /// TODO: UInt32?
        public Int32 KeyLength { get; private set; }

        /// <summary>
        /// Gets the product value of the CD key.
        /// </summary>
        public Int32 ProductValue { get; private set; }

        /// <summary>
        /// Gets the public value of the CD key.
        /// </summary>
        public Int32 PublicValue { get; private set; }

        /// <summary>
        /// Gets a value whose purpose is unknown.
        /// </summary>
        /// <remarks>This value appears to be 0 at all times.</remarks>
        public Int32 Unknown { get; private set; }

        /// <summary>
        /// Gets a hash of
        /// <list type="number">
        ///     <item><description>A client token</description></item>
        ///     <item><description>A server token</description></item>
        ///     <item><description><see cref="ProductValue"/></description></item>
        ///     <item><description><see cref="PublicValue"/></description></item>
        ///     <item><description><see cref="Unknown"/> (?)</description></item>
        ///     <item><description>The private value of the CD key</description></item>
        /// </list>
        /// </summary>
        /// TODO: More descriptive name?
        public BrokenSha1Hash Hash { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="SidAuthCheckKeyData"/> class.
        /// </summary>
        /// <param name="keyDataBytes">An array of bytes that composes the key data.</param>
        /// <exception cref="ArgumentNullException"><paramref name="keyDataBytes"/> is <c>null</c>.
        /// </exception>
        public SidAuthCheckKeyData(byte[] keyDataBytes)
        {
            //// TODO: Is it okay to use SidByteParser here? Hell no!
            SidByteParser parser = new SidByteParser(keyDataBytes);
        }
    }
}
