namespace SamaxLibraryTests
{
    using System;
    using System.Collections.Generic;
    using NUnit.Framework;
    using NUnit.Framework.Constraints;
    using SamaxLibrary.Sid;

    /* Notes:
     * DataByteCount in method names and whatnot might refer to either the dataByteCount
     * parameter or the AmountOfBytesLeft of the parser. These values are always the same anyway.
     * 
     * The HasBytesLeft property is checked in the tests for ReadInt32 but not the other methods.
     */

    [TestFixture]
    public class SidByteParserTestFixture
    {
        private const string OnlyTheFirstFourBytesShouldAffectTheResultDescription =
            "Only the first four bytes should affect the result.";

        private const string OnlyTheFirstEightBytesShouldAffectTheResultDescription =
            "Only the first eight bytes should affect the result.";

        private const string TheValueShouldBeReadInLittleEndianDescription =
            "The value should be read in little-endian.";

        private const string BoundaryCase = "Boundary case.";
        private const string TypicalCase = "Typical case.";

        [Test]
        public void Constructor_WhenBytesIsNull_ThrowsArgumentNullException()
        {
            AssertThat_Constructor_ThrowsException<ArgumentNullException>(null);
        }

        [Test]
        public void Constructor_WhenBytesIsEmpty_ThrowsArgumentException()
        {
            byte[] bytes = new byte[0];
            AssertThat_Constructor_ThrowsException<ArgumentException>(bytes);
        }

        [Test]
        public void Constructor_WhenBytesIsJustTooSmall_ThrowsArgumentException()
        {
            byte[] bytes = new byte[SidHeader.HeaderLength - 1];
            AssertThat_Constructor_ThrowsException<ArgumentException>(bytes);
        }

        [TestCase(0, Description = "Boundary case.")]
        [TestCase(1, Description = "Boundary case.")]
        [TestCase(20, Description = "Typical case.")]
        public void Constructor_WhenParserHasSpecifiedDataByteCount_ParserHasSpecifiedAmountOfBytesLeft(
            int dataByteCount)
        {
            SidByteParser parser = CreateSidByteParserWithSpecifiedDataByteCount(dataByteCount);
            Assert.That(parser.AmountOfBytesLeft, Is.EqualTo(dataByteCount));
        }

        [TestCase(0, Description = "Boundary case.")]
        [TestCase(1, Description = "Boundary case.")]
        [TestCase(20, Description = "Typical case´.")]
        public void Constructor_WhenParserHasSpecifiedDataByteCount_ParserHasBytesLeftIffDataByteCountIsNot0(
            int dataByteCount)
        {
            SidByteParser parser = CreateSidByteParserWithSpecifiedDataByteCount(dataByteCount);
            IResolveConstraint fulfillsConstraint = dataByteCount != 0 ?
                (IResolveConstraint)Is.True : Is.False;
            Assert.That(parser.HasBytesLeft, fulfillsConstraint);
        }

        [TestCase(0, Description = BoundaryCase)]
        [TestCase(4 - 1, Description = BoundaryCase)]
        [TestCase(4, Description = BoundaryCase)]
        [TestCase(20, Description = TypicalCase)]
        public void ReadInt32_WhenParserHasSpecifiedDataByteCount_ThrowsSidByteParserExceptionIffDataByteCountIsLessThan4(
            int dataByteCount)
        {
            SidByteParser parser = CreateSidByteParserWithSpecifiedDataByteCount(dataByteCount);
            IResolveConstraint fulfillsConstraint = parser.AmountOfBytesLeft < 4 ?
                (IResolveConstraint)Throws.InstanceOf<SidByteParserException>() : Throws.Nothing;
            Assert.That(() => parser.ReadInt32(), fulfillsConstraint);
        }

        [TestCase(4, Description = BoundaryCase)]
        [TestCase(5, Description = BoundaryCase)]
        [TestCase(20, Description = BoundaryCase)]
        public void ReadInt32_WhenParserHasSpecifiedDataByteCount_ParserHasBytesLeftIffDataByteCountIsNot4(
            int dataByteCount)
        {
            SidByteParser parser = CreateSidByteParserWithSpecifiedDataByteCount(dataByteCount);
            IResolveConstraint fulfillsConstraint = parser.AmountOfBytesLeft != 4 ?
                (IResolveConstraint)Is.True : Is.False;
            parser.ReadInt32();
            Assert.That(parser.HasBytesLeft, fulfillsConstraint);
        }

        [TestCase(4, Description = BoundaryCase)]
        [TestCase(20, Description = TypicalCase)]
        public void ReadInt32_WhenParserHasSpecifiedDataByteCount_DecreasesBytesLeftBy4(
            int dataByteCount)
        {
            SidByteParser parser = CreateSidByteParserWithSpecifiedDataByteCount(dataByteCount);
            int oldAmountOfBytesLeft = parser.AmountOfBytesLeft;
            parser.ReadInt32();
            Assert.That(parser.AmountOfBytesLeft, Is.EqualTo(oldAmountOfBytesLeft - 4));
        }

        [TestCase(
            1, 2, 3, 4,
            ExpectedResult = 1 + 256 * (2 + 256 * (3 + 256 * 4)),
            Description = TheValueShouldBeReadInLittleEndianDescription)]
        [TestCase(
            0, 0, 0, 0x80,
            ExpectedResult = Int32.MinValue,
            Description = "It should be possible to read negative values.")]
        [TestCase(
            0, 0, 0, 0, (byte)0xFF,
            ExpectedResult = 0,
            Description = OnlyTheFirstFourBytesShouldAffectTheResultDescription)]
        public Int32 ReadInt32(
            byte byte1,
            byte byte2,
            byte byte3,
            byte byte4,
            params byte[] remainingBytes)
        {
            List<byte> dataBytes = new List<byte> { byte1, byte2, byte3, byte4 };
            dataBytes.AddRange(remainingBytes);
            SidByteParser parser = CreateSidByteParserWithSpecifiedDataBytes(dataBytes.ToArray());
            return parser.ReadInt32();
        }

        [Test]
        public void ReadInt32AsEnum_WhenTypeParameterIsNotAnEnumerationType_ThrowsArgumentException()
        {
            SidByteParser parser = CreateSidByteParserWithSpecifiedCountOfZeroedDataBytes(20);
            Assert.That(() => parser.ReadInt32AsEnum<Int64>(), Throws.ArgumentException);
        }

        [TestCase(0, Description = BoundaryCase)]
        [TestCase(4 - 1, Description = BoundaryCase)]
        [TestCase(4, Description = BoundaryCase)]
        [TestCase(20, Description = TypicalCase)]
        public void ReadInt32AsEnum_WhenParserHasSpecifiedCountOfZeroedDataBytes_ThrowsSidByteParserExceptionIffDataByteCountIsLessThan4(
            int dataByteCount)
        {
            SidByteParser parser = CreateSidByteParserWithSpecifiedCountOfZeroedDataBytes(dataByteCount);
            IResolveConstraint fulfillsConstraint = parser.AmountOfBytesLeft < 4 ?
                (IResolveConstraint)Throws.InstanceOf<SidByteParserException>() : Throws.Nothing;
            Assert.That(() => parser.ReadInt32AsEnum<SidByteParserTestEnum>(), fulfillsConstraint);
        }

        [Test]
        public void ReadInt32AsEnum_WhenReadValueDoesNotMatchAnyEnumMember_ThrowsSidByteParserException()
        {
            SidByteParser parser = CreateSidByteParserWithSpecifiedDataBytes(2, 0, 0, 0);
            Assert.That(
                () => parser.ReadInt32AsEnum<SidByteParserTestEnum>(),
                Throws.InstanceOf<SidByteParserException>());
        }

        [TestCase(4, Description = BoundaryCase)]
        [TestCase(20, Description = TypicalCase)]
        public void ReadInt32AsEnum_WhenParserHasSpecifiedCountOfZeroedDataBytes_DecreasesBytesLeftBy4(
            int dataByteCount)
        {
            SidByteParser parser = CreateSidByteParserWithSpecifiedCountOfZeroedDataBytes(dataByteCount);
            int oldAmountOfBytesLeft = parser.AmountOfBytesLeft;
            parser.ReadInt32AsEnum<SidByteParserTestEnum>();
            Assert.That(parser.AmountOfBytesLeft, Is.EqualTo(oldAmountOfBytesLeft - 4));
        }

        [TestCase(
            1, 2, 3, 4,
            ExpectedResult = SidByteParserTestEnum.EndiannessTestMember,
            Description = TheValueShouldBeReadInLittleEndianDescription)]
        [TestCase(
            0, 0, 0, 0, (byte)1,
            ExpectedResult = SidByteParserTestEnum.Member0,
            Description = OnlyTheFirstFourBytesShouldAffectTheResultDescription)]
        [TestCase(
            0, 0, 0, 0, (byte)0xFF,
            ExpectedResult = SidByteParserTestEnum.Member0,
            Description = OnlyTheFirstFourBytesShouldAffectTheResultDescription)]
        public SidByteParserTestEnum ReadInt32AsEnum_WhenEnumTypeIsTestEnum(
            byte byte1,
            byte byte2,
            byte byte3,
            byte byte4,
            params byte[] remainingBytes)
        {
            List<byte> dataBytes = new List<byte> { byte1, byte2, byte3, byte4 };
            dataBytes.AddRange(remainingBytes);
            SidByteParser parser = CreateSidByteParserWithSpecifiedDataBytes(dataBytes.ToArray());
            return parser.ReadInt32AsEnum<SidByteParserTestEnum>();
        }

        [TestCase(0, Description = BoundaryCase)]
        [TestCase(8 - 1, Description = BoundaryCase)]
        [TestCase(8, Description = BoundaryCase)]
        [TestCase(20, Description = TypicalCase)]
        public void ReadUInt64_WhenParserHasSpecifiedDataByteCount_ThrowsSidByteParserExceptionIffDataByteCountIsLessThan8(
            int dataByteCount)
        {
            SidByteParser parser = CreateSidByteParserWithSpecifiedDataByteCount(dataByteCount);
            IResolveConstraint fulfillsConstraint = parser.AmountOfBytesLeft < 8 ?
                (IResolveConstraint)Throws.InstanceOf<SidByteParserException>() : Throws.Nothing;
            Assert.That(() => parser.ReadUInt64(), fulfillsConstraint);
        }

        [TestCase(8, Description = BoundaryCase)]
        [TestCase(20, Description = TypicalCase)]
        public void ReadUInt64_WhenParserHasSpecifiedDataByteCount_DecreasesBytesLeftBy8(
            int dataByteCount)
        {
            SidByteParser parser = CreateSidByteParserWithSpecifiedDataByteCount(dataByteCount);
            int oldAmountOfBytesLeft = parser.AmountOfBytesLeft;
            parser.ReadUInt64();
            Assert.That(parser.AmountOfBytesLeft, Is.EqualTo(oldAmountOfBytesLeft - 8));
        }

        [TestCase(
            1, 2, 3, 4, 5, 6, 7, 8,
            ExpectedResult = 0x0807060504030201U,
            Description = TheValueShouldBeReadInLittleEndianDescription)]
        [TestCase(
            0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF,
            ExpectedResult = UInt64.MaxValue,
            Description = "It should be possible to read values that cannot be stored in a signed 64-bit integer.")]
        [TestCase(
            0, 0, 0, 0, 0, 0, 0, 0, (byte)0xFF,
            ExpectedResult = 0,
            Description = OnlyTheFirstEightBytesShouldAffectTheResultDescription)]
        public UInt64 ReadUInt64(
            byte byte1,
            byte byte2,
            byte byte3,
            byte byte4,
            byte byte5,
            byte byte6,
            byte byte7,
            byte byte8,
            params byte[] remainingBytes)
        {
            List<byte> dataBytes = new List<byte>() { byte1, byte2, byte3, byte4, byte5, byte6, byte7, byte8 };
            dataBytes.AddRange(remainingBytes);
            SidByteParser parser = CreateSidByteParserWithSpecifiedDataBytes(dataBytes.ToArray());
            return parser.ReadUInt64();
        }

        private SidByteParser CreateSidByteParser(byte[] bytesToParse)
        {
            return new SidByteParser(bytesToParse);
        }

        private SidByteParser CreateSidByteParserWithSpecifiedDataByteCount(int dataByteCount)
        {
            return CreateSidByteParserWithSpecifiedCountOfZeroedDataBytes(dataByteCount);
        }

        private SidByteParser CreateSidByteParserWithSpecifiedCountOfZeroedDataBytes(int dataByteCount)
        {
            byte[] bytes = new byte[dataByteCount];
            return CreateSidByteParserWithSpecifiedDataBytes(bytes);
        }

        private SidByteParser CreateSidByteParserWithSpecifiedDataBytes(params byte[] dataBytes)
        {
            var bytes = new System.Collections.Generic.List<byte>() { 0, 0, 0, 0 };
            bytes.AddRange(dataBytes);
            return CreateSidByteParser(bytes.ToArray());
        }

        private void AssertThat_Constructor_ThrowsException<T>(byte[] bytes) where T : Exception
        {
            Assert.That(() => CreateSidByteParser(bytes), Throws.InstanceOf<T>());
        }
    }
}