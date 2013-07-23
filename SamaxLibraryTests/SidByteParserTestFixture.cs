namespace SamaxLibraryTests
{
    using System;
    using System.Collections.Generic;
    using NUnit.Framework;
    using NUnit.Framework.Constraints;
    using SamaxLibrary.Sid;

    [TestFixture]
    public class SidByteParserTestFixture
    {
        private const string OnlyTheFirstFourBytesShouldAffectTheResultDescription =
            "Only the first four bytes should affect the result.";

        private const string TheValueShouldBeReadInLittleEndianDescription =
            "The value should be read in little-endian.";

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

        [Test]
        public void Constructor_WhenBytesIsJustLargeEnough_ParserHasNoDataBytesToParse()
        {
            AssertThat_Constructor_WithSpecifiedDataByteCount_HasOrHasNotDataBytesToParse(0, false);
        }

        [Test]
        public void Constructor_WhenBytesIsJustLargeEnoughPlusOne_ParserHasDataBytesToParse()
        {
            AssertThat_Constructor_WithSpecifiedDataByteCount_HasOrHasNotDataBytesToParse(1, true);
        }

        [Test]
        public void Constructor_WhenBytesIsVeryLarge_ParserHasDataBytesToParse()
        {
            AssertThat_Constructor_WithSpecifiedDataByteCount_HasOrHasNotDataBytesToParse(10000, true);
        }

        [Test]
        public void ReadInt32_WhenNoDataBytes_ThrowsSidByteParserException()
        {
            AssertThat_ReadInt32_WhenParserHasSpecifiedDataByteCount_ThrowsException<SidByteParserException>(0);
        }

        [Test]
        public void ReadInt32_WhenJustTooFewDataBytes_ThrowsSidByteParserException()
        {
            AssertThat_ReadInt32_WhenParserHasSpecifiedDataByteCount_ThrowsException<SidByteParserException>(3);
        }

        [Test]
        public void ReadInt32_WhenJustEnoughDataBytes_ParserHasNoDataBytesToParse()
        {
            AssertThat_ReadInt32_WhenParserHasSpecifiedDataByteCount_HasOrHasNotDataBytesToParse(4, false);
        }

        [Test]
        public void ReadInt32_WhenJustEnoughDataBytesPlusOne_ParserHasDataBytesToParse()
        {
            AssertThat_ReadInt32_WhenParserHasSpecifiedDataByteCount_HasOrHasNotDataBytesToParse(5, true);
        }

        [Test]
        public void ReadInt32_WhenLotsOfDataBytes_ParserHasDataBytesToParse()
        {
            AssertThat_ReadInt32_WhenParserHasSpecifiedDataByteCount_HasOrHasNotDataBytesToParse(100000, true);
        }

        [TestCase(
            1, 2, 3, 4,
            ExpectedResult = 1 + 256 * (2 + 256 * (3 + 256 * 4)),
            Description = TheValueShouldBeReadInLittleEndianDescription)]
        [TestCase(
            0, 0, 0, 0x80,
            ExpectedResult = Int32.MinValue,
            Description = "It should be possible to read the negative number of the greatest magnitude.")]
        [TestCase(
            0, 0, 0, 0, (byte)1,
            ExpectedResult = 0,
            Description = OnlyTheFirstFourBytesShouldAffectTheResultDescription)]
        public Int32 ReadInt32(
            byte firstByte, byte secondByte, byte thirdByte, byte fourthByte, params Byte[] remainingBytes)
        {
            List<byte> dataBytes = new List<byte> { firstByte, secondByte, thirdByte, fourthByte };
            dataBytes.AddRange(remainingBytes);
            SidByteParser parser = CreateSidByteParserWithSpecifiedDataBytes(dataBytes.ToArray());
            return parser.ReadInt32();
        }

        [Test]
        public void ReadInt32AsEnum_WhenTypeParameterIsNotAnEnumerationType_ThrowsArgumentException()
        {
            SidByteParser parser = CreateSidByteParserWithSpecifiedDataByteCount(4);
            Assert.That(() => parser.ReadInt32AsEnum<Int64>(), Throws.ArgumentException);
        }

        [Test]
        public void ReadInt32AsEnum_WhenNoDataBytes_ThrowsSidByteParserException()
        {
            AssertThat_ReadInt32AsEnum_WhenEnumTypeIsTestEnumAndParserHasSpecifiedDataByteCount_ThrowsException<SidByteParserException>(0);
        }

        [Test]
        public void ReadInt32AsEnum_WhenJustTooFewDataBytes_ThrowsSidByteParserException()
        {
            AssertThat_ReadInt32AsEnum_WhenEnumTypeIsTestEnumAndParserHasSpecifiedDataByteCount_ThrowsException<SidByteParserException>(3);
        }

        [Test]
        public void ReadInt32AsEnum_WhenReadValueDoesNotMatchAnyEnumMember_ThrowsSidByteParserException()
        {
            SidByteParser parser = CreateSidByteParserWithSpecifiedDataBytes(2, 0, 0, 0);
            Assert.That(
                () => parser.ReadInt32AsEnum<SidByteParserTestEnum>(),
                Throws.InstanceOf<SidByteParserException>());
        }

        [Test]
        public void ReadInt32AsEnum_WhenJustEnoughDataBytes_ParserHasNoDataBytesToParse()
        {
            AssertThat_ReadInt32AsEnum_WhenEnumTypeIsTestEnumAndParserHasSpecifiedDataByteCount_HasOrHasNotDataBytesToParse(4, false);
        }

        [Test]
        public void ReadInt32AsEnum_WhenJustEnoughDataBytesPlusOne_ParserHasDataBytesToParse()
        {
            AssertThat_ReadInt32AsEnum_WhenEnumTypeIsTestEnumAndParserHasSpecifiedDataByteCount_HasOrHasNotDataBytesToParse(5, true);
        }

        public void ReadInt32AsEnum_WhenLotsOfDataBytes_ParserHasDataBytesToParse()
        {
            AssertThat_ReadInt32AsEnum_WhenEnumTypeIsTestEnumAndParserHasSpecifiedDataByteCount_HasOrHasNotDataBytesToParse(4, false);
        }

        [TestCase(
            1, 2, 3, 4,
            ExpectedResult = SidByteParserTestEnum.EndiannessTestMember,
            Description = TheValueShouldBeReadInLittleEndianDescription)]
        [TestCase(
            0, 0, 0, 0, (byte)1,
            ExpectedResult = SidByteParserTestEnum.Member0,
            Description = OnlyTheFirstFourBytesShouldAffectTheResultDescription)]
        public SidByteParserTestEnum ReadInt32AsEnum_WhenEnumTypeIsTestEnum(
            byte firstByte, byte secondByte, byte thirdByte, byte fourthByte, params byte[] remainingBytes)
        {
            List<byte> dataBytes = new List<byte> { firstByte, secondByte, thirdByte, fourthByte };
            dataBytes.AddRange(remainingBytes);
            SidByteParser parser = CreateSidByteParserWithSpecifiedDataBytes(dataBytes.ToArray());
            return parser.ReadInt32AsEnum<SidByteParserTestEnum>();
        }

        private SidByteParser CreateSidByteParser(byte[] bytesToParse)
        {
            return new SidByteParser(bytesToParse);
        }

        private SidByteParser CreateSidByteParserWithSpecifiedDataByteCount(int dataByteCount)
        {
            byte[] bytes = new byte[SidHeader.HeaderLength + dataByteCount];
            return CreateSidByteParser(bytes);
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

        private void AssertThat_Constructor_WithSpecifiedDataByteCount_HasOrHasNotDataBytesToParse(
            int dataByteCount, bool shouldHaveDataBytesToParse)
        {
            SidByteParser parser = CreateSidByteParserWithSpecifiedDataByteCount(dataByteCount);
            IResolveConstraint constraint = shouldHaveDataBytesToParse ? (IResolveConstraint)Is.True : Is.False;
            Assert.That(parser.HasBytesToParse, constraint);
        }

        private void AssertThat_ReadInt32_ThrowsException<T>(SidByteParser parser) where T : Exception
        {
            Assert.That(() => parser.ReadInt32(), Throws.InstanceOf<T>());
        }

        private void AssertThat_ReadInt32_WhenParserHasSpecifiedDataByteCount_ThrowsException<T>(
            int dataByteCount)
            where T : Exception
        {
            SidByteParser parser = CreateSidByteParserWithSpecifiedDataByteCount(dataByteCount);
            AssertThat_ReadInt32_ThrowsException<T>(parser);
        }

        private void AssertThat_ReadInt32_WhenParserHasSpecifiedDataByteCount_HasOrHasNotDataBytesToParse(
            int dataByteCount, bool shouldHaveDataBytesToParse)
        {
            SidByteParser parser = CreateSidByteParserWithSpecifiedDataByteCount(dataByteCount);
            parser.ReadInt32();
            IResolveConstraint constraint = shouldHaveDataBytesToParse ? (IResolveConstraint)Is.True : Is.False;
            Assert.That(parser.HasBytesToParse, constraint);
        }

        private void AssertThat_ReadInt32AsEnum_ThrowsException<TEnum, TException>(SidByteParser parser)
            where TEnum : struct, IComparable, IConvertible, IFormattable
            where TException : Exception
        {
            Assert.That(() => parser.ReadInt32AsEnum<TEnum>(), Throws.InstanceOf<TException>());
        }

        private void AssertThat_ReadInt32AsEnum_WhenEnumTypeIsTestEnum_ThrowsException<T>(
            SidByteParser parser)
            where T : Exception
        {
            AssertThat_ReadInt32AsEnum_ThrowsException<SidByteParserTestEnum, T>(parser);
        }

        private void AssertThat_ReadInt32AsEnum_WhenEnumTypeIsTestEnumAndParserHasSpecifiedDataByteCount_ThrowsException<T>(
            int dataByteCount)
            where T : Exception
        {
            SidByteParser parser = CreateSidByteParserWithSpecifiedDataByteCount(dataByteCount);
            AssertThat_ReadInt32AsEnum_WhenEnumTypeIsTestEnum_ThrowsException<T>(parser);
        }

        private void AssertThat_ReadInt32AsEnum_WhenEnumTypeIsTestEnumAndParserHasSpecifiedDataByteCount_HasOrHasNotDataBytesToParse(
            int dataByteCount, bool shouldHaveDataBytesToParse)
        {
            // Explicitness about the data bytes to make sure that there is an enum member matching the read value
            byte[] dataBytes = new byte[dataByteCount];
            SidByteParser parser = CreateSidByteParserWithSpecifiedDataBytes(dataBytes);
            parser.ReadInt32AsEnum<SidByteParserTestEnum>();
            IResolveConstraint constraint = shouldHaveDataBytesToParse ? (IResolveConstraint)Is.True : Is.False;
            Assert.That(parser.HasBytesToParse, constraint);
        }
    }
}