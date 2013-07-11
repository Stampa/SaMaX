namespace SamaxLibraryTests
{
    using System;
    using NUnit.Framework;
    using SamaxLibrary.Sid;

    [TestFixture]
    public class SidByteParserTestFixture
    {
        [Test]
        public void Constructor_WhenBytesNull_ThrowsArgumentNullException()
        {
            AssertConstructorThrowsException<ArgumentNullException>(null);
        }

        [Test]
        public void Constructor_WhenBytesEmpty_ThrowsArgumentException()
        {
            byte[] bytes = new byte[0];
            AssertConstructorThrowsException<ArgumentException>(bytes);
        }

        [Test]
        public void Constructor_WhenBytesJustTooSmall_ThrowsArgumentException()
        {
            byte[] bytes = new byte[SidHeader.HeaderLength - 1];
            AssertConstructorThrowsException<ArgumentException>(bytes);
        }

        [Test]
        public void Constructor_WhenBytesJustLargeEnough_ParserHasNoDataBytesToParse()
        {
            SidByteParser parser = CreateSidByteParserWithSpecifiedDataByteCount(0);
            Assert.That(parser.HasBytesToParse, Is.False);
        }

        [Test]
        public void Constructor_WhenBytesJustLargeEnoughPlusOne_ParserHasDataBytesToParse()
        {
            SidByteParser parser = CreateSidByteParserWithSpecifiedDataByteCount(1);
            Assert.That(parser.HasBytesToParse, Is.True);
        }

        [Test]
        public void Constructor_WhenBytesVeryLarge_ParserHasDataBytesToParse()
        {
            SidByteParser parser = CreateSidByteParserWithSpecifiedDataByteCount(100000);
            Assert.That(parser.HasBytesToParse, Is.True);
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

        private void AssertConstructorThrowsException<T>(byte[] bytes) where T : Exception
        {
            Assert.Throws<T>(() => CreateSidByteParser(bytes));
        }
    }
}