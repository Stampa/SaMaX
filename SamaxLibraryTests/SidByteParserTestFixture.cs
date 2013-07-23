namespace SamaxLibraryTests
{
    using System;
    using NUnit.Framework;
    using SamaxLibrary.Sid;

    [TestFixture]
    public class SidByteParserTestFixture
    {
        [Test]
        public void Constructor_WhenBytesIsNull_ThrowsArgumentNullException()
        {
            AssertThatConstructorThrowsException<ArgumentNullException>(null);
        }

        [Test]
        public void Constructor_WhenBytesIsEmpty_ThrowsArgumentException()
        {
            byte[] bytes = new byte[0];
            AssertThatConstructorThrowsException<ArgumentException>(bytes);
        }

        [Test]
        public void Constructor_WhenBytesIsJustTooSmall_ThrowsArgumentException()
        {
            byte[] bytes = new byte[SidHeader.HeaderLength - 1];
            AssertThatConstructorThrowsException<ArgumentException>(bytes);
        }

        [Test]
        public void Constructor_WhenBytesIsJustLargeEnough_ParserHasNoDataBytesToParse()
        {
            SidByteParser parser = CreateSidByteParserWithSpecifiedDataByteCount(0);
            Assert.That(parser.HasBytesToParse, Is.False);
        }

        [Test]
        public void Constructor_WhenBytesIsJustLargeEnoughPlusOne_ParserHasDataBytesToParse()
        {
            SidByteParser parser = CreateSidByteParserWithSpecifiedDataByteCount(1);
            Assert.That(parser.HasBytesToParse, Is.True);
        }

        [Test]
        public void Constructor_WhenBytesIsVeryLarge_ParserHasDataBytesToParse()
        {
            SidByteParser parser = CreateSidByteParserWithSpecifiedDataByteCount(100000);
            Assert.That(parser.HasBytesToParse, Is.True);
        }

        [Test]
        public void ReadInt32_WhenNoDataBytes_ThrowsSidByteParserException()
        {
            SidByteParser parser = CreateSidByteParserWithSpecifiedDataByteCount(0);
            AssertThatReadInt32ThrowsException<SidByteParserException>(parser);
        }

        [Test]
        public void ReadInt32_WhenJustTooFewDataBytes_ThrowsSidByteParserException()
        {
            SidByteParser parser = CreateSidByteParserWithSpecifiedDataByteCount(3);
            AssertThatReadInt32ThrowsException<SidByteParserException>(parser);
        }

        [Test]
        public void ReadInt32_WhenJustEnoughDataBytes_ParserHasNoDataBytesToParse()
        {
            SidByteParser parser = CreateSidByteParserWithSpecifiedDataByteCount(4);
            parser.ReadInt32();
            Assert.That(parser.HasBytesToParse, Is.False);
        }

        [Test]
        public void ReadInt32_WhenJustEnoughDataBytesPlusOne_ParserHasDataBytesToParse()
        {
            SidByteParser parser = CreateSidByteParserWithSpecifiedDataByteCount(5);
            parser.ReadInt32();
            Assert.That(parser.HasBytesToParse, Is.True);
        }

        //// TODO: Consider having a "WhenLotsOfDataBytes_ParserHasDataBytesToParse()

        [TestCase(
            1, 2, 3, 4,
            ExpectedResult = 1 + 256 * (2 + 256 * (3 + 256 * 4)),
            Description = "The integer should be read in little-endian.")]
        [TestCase(
            0, 0, 0, 0x80,
            ExpectedResult = Int32.MinValue,
            Description = "It should be possible to read the negative number of the greatest magnitude.")]
        public Int32 ReadInt32(byte firstByte, byte secondByte, byte thirdByte, byte fourthByte)
        {
            byte[] dataBytes = { firstByte, secondByte, thirdByte, fourthByte };
            SidByteParser parser = CreateSidByteParserWithSpecifiedDataBytes(dataBytes);
            return parser.ReadInt32();
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

        private SidByteParser CreateSidByteParserWithSpecifiedDataBytes(byte[] dataBytes)
        {
            var bytes = new System.Collections.Generic.List<byte>() { 0, 0, 0, 0 };
            bytes.AddRange(dataBytes);
            return CreateSidByteParser(bytes.ToArray());
        }

        private void AssertThatConstructorThrowsException<T>(byte[] bytes) where T : Exception
        {
            Assert.That(() => CreateSidByteParser(bytes), Throws.InstanceOf<T>());
        }

        private void AssertThatReadInt32ThrowsException<T>(SidByteParser parser) where T : Exception
        {
            Assert.That(() => parser.ReadInt32(), Throws.InstanceOf<T>());
        }
    }
}