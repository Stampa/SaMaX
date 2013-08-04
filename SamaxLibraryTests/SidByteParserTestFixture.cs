namespace SamaxLibraryTests
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Text;
    using NUnit.Framework;
    using NUnit.Framework.Constraints;
    using SamaxLibrary.Sid;

    /* Notes:
     * DataByteCount in method names and whatnot might refer to either the dataByteCount
     * parameter or the AmountOfBytesLeft of the parser. These values are always the same anyway.
     * 
     * The HasBytesLeft property is checked in the tests for ReadInt32 but not the other methods.
     * 
     * Some tests rely on Encoding.ASCII to work "as expected".
     */

    /* TODO:
     * It's unclear exactly what kind of strings the string methods should accept.
     * Anything but alphanumeric characters? Nonprintable characters? Whitespace characters?
     * Non-ASCII (> 127)?!
     * Figure this out and add unit tests for them!
     */

    [TestFixture]
    public class SidByteParserTestFixture
    {
        private const string BoundaryCase = "Boundary case.";
        private const string TypicalCase = "Typical case.";
        private const string NonboundaryCase = "Nonboundary case (but not typical).";

        private const string OnlyTheFirstFourBytesShouldAffectTheResult =
            "Only the first four bytes should affect the result.";

        private const string OnlyTheFirstEightBytesShouldAffectTheResult =
            "Only the first eight bytes should affect the result.";

        private const string TheValueShouldBeReadInLittleEndian =
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

        [TestCase(0, Description = "Boundary case.")]
        [TestCase(1, Description = "Boundary case.")]
        [TestCase(20, Description = "Typical case.")]
        public void Constructor_ParserHasSpecifiedAmountOfBytesLeft(
            int dataByteCount)
        {
            SidByteParser parser = CreateSidByteParserWithRandomDataBytes(dataByteCount);
            Assert.That(parser.AmountOfBytesLeft, Is.EqualTo(dataByteCount));
        }

        [TestCase(0, Description = "Boundary case.")]
        [TestCase(1, Description = "Boundary case.")]
        [TestCase(20, Description = "Typical case´.")]
        public void Constructor_ParserHasBytesLeft_IffDataByteCountIsNot0(
            int dataByteCount)
        {
            SidByteParser parser = CreateSidByteParserWithRandomDataBytes(dataByteCount);
            IResolveConstraint fulfillsConstraint = dataByteCount != 0 ?
                (IResolveConstraint)Is.True : Is.False;
            Assert.That(parser.HasBytesLeft, fulfillsConstraint);
        }

        [TestCase(0, Description = BoundaryCase)]
        [TestCase(4 - 1, Description = BoundaryCase)]
        [TestCase(4, Description = BoundaryCase)]
        [TestCase(20, Description = TypicalCase)]
        public void ReadInt32_ThrowsSidByteParserException_IffDataByteCountIsLessThan4(
            int dataByteCount)
        {
            SidByteParser parser = CreateSidByteParserWithRandomDataBytes(dataByteCount);
            IResolveConstraint fulfillsConstraint = parser.AmountOfBytesLeft < 4 ?
                (IResolveConstraint)Throws.InstanceOf<SidByteParserException>() : Throws.Nothing;
            Assert.That(() => parser.ReadInt32(), fulfillsConstraint);
        }

        [TestCase(4, Description = BoundaryCase)]
        [TestCase(5, Description = BoundaryCase)]
        [TestCase(20, Description = BoundaryCase)]
        public void ReadInt32_ParserHasBytesLeft_IffDataByteCountIsNot4(
            int dataByteCount)
        {
            SidByteParser parser = CreateSidByteParserWithRandomDataBytes(dataByteCount);
            IResolveConstraint fulfillsConstraint = parser.AmountOfBytesLeft != 4 ?
                (IResolveConstraint)Is.True : Is.False;
            parser.ReadInt32();
            Assert.That(parser.HasBytesLeft, fulfillsConstraint);
        }

        [TestCase(4, Description = BoundaryCase)]
        [TestCase(20, Description = TypicalCase)]
        public void ReadInt32_DecreasesBytesLeftBy4(
            int dataByteCount)
        {
            SidByteParser parser = CreateSidByteParserWithRandomDataBytes(dataByteCount);
            int oldAmountOfBytesLeft = parser.AmountOfBytesLeft;
            parser.ReadInt32();
            Assert.That(parser.AmountOfBytesLeft, Is.EqualTo(oldAmountOfBytesLeft - 4));
        }

        [TestCase(
            1, 2, 3, 4,
            ExpectedResult = 1 + 256 * (2 + 256 * (3 + 256 * 4)),
            Description = TheValueShouldBeReadInLittleEndian)]
        [TestCase(
            0, 0, 0, 0x80,
            ExpectedResult = Int32.MinValue,
            Description = "It should be possible to read negative values.")]
        [TestCase(
            0, 0, 0, 0, (byte)0xFF,
            ExpectedResult = 0,
            Description = OnlyTheFirstFourBytesShouldAffectTheResult)]
        public Int32 ReadInt32( // Hehe, hidden method name here :)
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
            SidByteParser parser = CreateSidByteParserWithZeroedDataBytes(20);
            Assert.That(() => parser.ReadInt32AsEnum<Int64>(), Throws.ArgumentException);
        }

        [TestCase(0, Description = BoundaryCase)]
        [TestCase(4 - 1, Description = BoundaryCase)]
        [TestCase(4, Description = BoundaryCase)]
        [TestCase(20, Description = TypicalCase)]
        public void ReadInt32AsEnum_WithZeroedDataBytes_ThrowsSidByteParserException_IffDataByteCountIsLessThan4(
            int dataByteCount)
        {
            SidByteParser parser = CreateSidByteParserWithZeroedDataBytes(dataByteCount);
            IResolveConstraint fulfillsConstraint = parser.AmountOfBytesLeft < 4 ?
                (IResolveConstraint)Throws.InstanceOf<SidByteParserException>() : Throws.Nothing;
            Assert.That(() => parser.ReadInt32AsEnum<SidByteParserAndWriterTestEnum>(), fulfillsConstraint);
        }

        [Test]
        public void ReadInt32AsEnum_WhenReadValueDoesNotMatchAnyEnumMember_ThrowsSidByteParserException()
        {
            byte nonmatchingValue = (byte)(SidByteParserAndWriterTestEnum.NoMemberAtMePlusOne + 1);
            SidByteParser parser = CreateSidByteParserWithSpecifiedDataBytes(nonmatchingValue, 0, 0, 0);
            Assert.That(
                () => parser.ReadInt32AsEnum<SidByteParserAndWriterTestEnum>(),
                Throws.InstanceOf<SidByteParserException>());
        }

        [TestCase(4, Description = BoundaryCase)]
        [TestCase(20, Description = TypicalCase)]
        public void ReadInt32AsEnum_DecreasesBytesLeftBy4(
            int dataByteCount)
        {
            SidByteParser parser = CreateSidByteParserWithZeroedDataBytes(dataByteCount);
            int oldAmountOfBytesLeft = parser.AmountOfBytesLeft;
            parser.ReadInt32AsEnum<SidByteParserAndWriterTestEnum>();
            Assert.That(parser.AmountOfBytesLeft, Is.EqualTo(oldAmountOfBytesLeft - 4));
        }

        [TestCase(
            1, 2, 3, 4,
            ExpectedResult = SidByteParserAndWriterTestEnum.EndiannessTestMember,
            Description = TheValueShouldBeReadInLittleEndian)]
        [TestCase(
            0, 0, 0, 0, (byte)1,
            ExpectedResult = SidByteParserAndWriterTestEnum.Member0,
            Description = OnlyTheFirstFourBytesShouldAffectTheResult)]
        [TestCase(
            0, 0, 0, 0, (byte)0xFF,
            ExpectedResult = SidByteParserAndWriterTestEnum.Member0,
            Description = OnlyTheFirstFourBytesShouldAffectTheResult)]
        public SidByteParserAndWriterTestEnum ReadInt32AsEnum_WhenEnumTypeIsTestEnum(
            byte byte1,
            byte byte2,
            byte byte3,
            byte byte4,
            params byte[] remainingBytes)
        {
            List<byte> dataBytes = new List<byte> { byte1, byte2, byte3, byte4 };
            dataBytes.AddRange(remainingBytes);
            SidByteParser parser = CreateSidByteParserWithSpecifiedDataBytes(dataBytes.ToArray());
            return parser.ReadInt32AsEnum<SidByteParserAndWriterTestEnum>();
        }

        [TestCase(0, Description = BoundaryCase)]
        [TestCase(8 - 1, Description = BoundaryCase)]
        [TestCase(8, Description = BoundaryCase)]
        [TestCase(20, Description = TypicalCase)]
        public void ReadUInt64_ThrowsSidByteParserException_IffDataByteCountIsLessThan8(
            int dataByteCount)
        {
            SidByteParser parser = CreateSidByteParserWithRandomDataBytes(dataByteCount);
            IResolveConstraint fulfillsConstraint = parser.AmountOfBytesLeft < 8 ?
                (IResolveConstraint)Throws.InstanceOf<SidByteParserException>() : Throws.Nothing;
            Assert.That(() => parser.ReadUInt64(), fulfillsConstraint);
        }

        [TestCase(8, Description = BoundaryCase)]
        [TestCase(20, Description = TypicalCase)]
        public void ReadUInt64_DecreasesBytesLeftBy8(
            int dataByteCount)
        {
            SidByteParser parser = CreateSidByteParserWithRandomDataBytes(dataByteCount);
            int oldAmountOfBytesLeft = parser.AmountOfBytesLeft;
            parser.ReadUInt64();
            Assert.That(parser.AmountOfBytesLeft, Is.EqualTo(oldAmountOfBytesLeft - 8));
        }

        [TestCase(
            1, 2, 3, 4, 5, 6, 7, 8,
            ExpectedResult = 0x0807060504030201U,
            Description = TheValueShouldBeReadInLittleEndian)]
        [TestCase(
            0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF,
            ExpectedResult = UInt64.MaxValue,
            Description = "It should be possible to read UInt64 values that cannot be stored in" +
                "a signed 64-bit integer.")]
        [TestCase(
            0, 0, 0, 0, 0, 0, 0, 0, (byte)0xFF,
            ExpectedResult = 0,
            Description = OnlyTheFirstEightBytesShouldAffectTheResult)]
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

        [TestCase(-20, Description = NonboundaryCase)]
        [TestCase(-1, Description = BoundaryCase)]
        [TestCase(0, Description = BoundaryCase)]
        [TestCase(1, Description = BoundaryCase)]
        [TestCase(20, Description = TypicalCase)]
        public void ReadByteArray_ThrowsArgumentOutOfRangeException_IffCountIsLessThan0(
            int count)
        {
            SidByteParser parser = CreateSidByteParserWithRandomDataBytes(50);
            IResolveConstraint fulfillsConstraint = count < 0 ?
                (IResolveConstraint)Throws.TypeOf<ArgumentOutOfRangeException>() : Throws.Nothing;
            Assert.That(() => parser.ReadByteArray(count), fulfillsConstraint);
        }

        [TestCase(40, 20, Description = TypicalCase)]
        public void ReadByteArray_ThrowsSidByteParserException_IffCountIsGreaterThanDataByteCount(
            [Values(0, 1, 20)] int dataByteCount,
            [Values(0, 1, 20)] int count)
        {
            SidByteParser parser = CreateSidByteParserWithRandomDataBytes(dataByteCount);
            IResolveConstraint fulfillsConstraint = count > parser.AmountOfBytesLeft ?
                (IResolveConstraint)Throws.InstanceOf<SidByteParserException>() : Throws.Nothing;
            Assert.That(() => parser.ReadByteArray(count), fulfillsConstraint);
        }

        // TODO: Using Combinatorial (default) and checking for invalid combinations in the method
        // could be better than using Sequential and carefully pairing up the arguments.
        [SuppressMessage("Microsoft.StyleCop.CSharp.SpacingRules", "SA1025:CodeMustNotContainMultipleWhitespaceInARow", Justification = "Proper alignment improves the readability.")]
        [TestCase(40, 20, Description = TypicalCase)]
        [Sequential]
        public void ReadByteArray_DecreasesBytesLeftByCount(
            [Values(0, 1, 1, 20, 20, 20)] int dataByteCount,
            [Values(0, 0, 1,  0,  1, 20)]  int count)
        {
            SidByteParser parser = CreateSidByteParserWithRandomDataBytes(dataByteCount);
            int oldAmountOfBytesLeft = parser.AmountOfBytesLeft;
            parser.ReadByteArray(count);
            Assert.That(parser.AmountOfBytesLeft, Is.EqualTo(oldAmountOfBytesLeft - count));
        }

        private IEnumerable<TestCaseData> ReadByteArrayTestSource()
        {
            const string ReturnValueShouldBeEmptyArrayWhenCountIs0 =
                "Return value should be empty array when count is 0.";
            byte[] emptyArray = new byte[0];
            yield return new TestCaseData(GetRandomByteArray(0), 0)
                .Returns(emptyArray)
                .SetDescription(ReturnValueShouldBeEmptyArrayWhenCountIs0);
            yield return new TestCaseData(GetRandomByteArray(1), 0)
                .Returns(emptyArray)
                .SetDescription(ReturnValueShouldBeEmptyArrayWhenCountIs0);
            yield return new TestCaseData(GetRandomByteArray(20), 0)
                .Returns(emptyArray)
                .SetDescription(ReturnValueShouldBeEmptyArrayWhenCountIs0);

            const string ReturnValueShouldBeEntireArrayWhenCountIsDataByteCount =
                "Return value should be entire array when count is data byte count.";
            byte[] dataBytes = GetRandomByteArray(0);
            yield return new TestCaseData(dataBytes, 0)
                .Returns(dataBytes)
                .SetDescription(ReturnValueShouldBeEntireArrayWhenCountIsDataByteCount);
            dataBytes = GetRandomByteArray(1);
            yield return new TestCaseData(dataBytes, 1)
                .Returns(dataBytes)
                .SetDescription(ReturnValueShouldBeEntireArrayWhenCountIsDataByteCount);
            dataBytes = GetRandomByteArray(20);
            yield return new TestCaseData(dataBytes, 20)
                .Returns(dataBytes)
                .SetDescription(ReturnValueShouldBeEntireArrayWhenCountIsDataByteCount);

            dataBytes = GetRandomByteArray(20);
            yield return new TestCaseData(dataBytes, 5)
                .Returns(new byte[5] { dataBytes[0], dataBytes[1], dataBytes[2], dataBytes[3], dataBytes[4] })
                .SetDescription(TypicalCase);
        }

        [TestCaseSource("ReadByteArrayTestSource")]
        public byte[] ReadByteArray(byte[] dataBytes, int count)
        {
            SidByteParser parser = CreateSidByteParserWithSpecifiedDataBytes(dataBytes);
            return parser.ReadByteArray(count);
        }

        // This could be expressed as a series of TestCases just as well;
        // all the input and descriptions are array creation and constant expressions.
        private IEnumerable<TestCaseData> ReadAsciiStringTestSource1()
        {
            yield return new TestCaseData(new byte[0])
                .SetDescription(BoundaryCase);
            yield return new TestCaseData(new byte[1] { 1 })
                .SetDescription(BoundaryCase);
            yield return new TestCaseData(new byte[] { 65, 66, 67, 68, 69 })
                .SetDescription(NonboundaryCase);
            yield return new TestCaseData(new byte[1] { 0 })
                .SetDescription(BoundaryCase);
            yield return new TestCaseData(new byte[] { 0, 70, 71, 72, 73 })
                .SetDescription("Null terminator at the very beginning.");
            yield return new TestCaseData(new byte[] { 65, 65, 65, 65, 65, 0 })
                .SetDescription("Null terminator at the very end.");
            yield return new TestCaseData(new byte[] { 65, 66, 0, 192, 168 })
                .SetDescription(TypicalCase);
            yield return new TestCaseData(new byte[] { 65, 0, 75, 0, 85 })
                .SetDescription("Multiple null terminators.");
        }

        [TestCaseSource("ReadAsciiStringTestSource1")]
        public void ReadAsciiString_ThrowsSidByteParserException_IffNoNullTerminatorExists(
            byte[] dataBytes)
        {
            SidByteParser parser = CreateSidByteParserWithSpecifiedDataBytes(dataBytes);
            IResolveConstraint fulfillsConstraint = Array.IndexOf<byte>(dataBytes, 0) == -1 ?
                (IResolveConstraint)Throws.InstanceOf<SidByteParserException>() : Throws.Nothing;
            Assert.That(() => parser.ReadAsciiString(), fulfillsConstraint);
        }

        private IEnumerable<TestCaseData> ReadAsciiStringTestSource2()
        {
            const string NullTerminatorAtFirstIndex = "Null terminator at the very beginning.";
            yield return new TestCaseData(new byte[1] { 0 })
                .SetDescription(NullTerminatorAtFirstIndex);
            yield return new TestCaseData(new byte[2] { 0, 0 })
                .SetDescription(NullTerminatorAtFirstIndex);
            yield return new TestCaseData(new byte[2] { 97, 0 })
                .SetDescription(NullTerminatorAtFirstIndex);
            yield return new TestCaseData(new byte[5])
                .SetDescription(NullTerminatorAtFirstIndex);

            // Using an Encoding might be a little crazy ...
            Encoding ascii = Encoding.ASCII;
            yield return new TestCaseData(ascii.GetBytes("Hello\0"))
                .SetDescription("Null terminator at the very end.");
            yield return new TestCaseData(ascii.GetBytes("Hello\0World\0"))
                .SetDescription("Multiple null terminators.");
        }

        [TestCaseSource("ReadAsciiStringTestSource2")]
        public void ReadAsciiString_DecreasesBytesLeftByStringLengthPlusOne(
            byte[] dataBytes)
        {
            SidByteParser parser = CreateSidByteParserWithSpecifiedDataBytes(dataBytes);
            int oldAmountOfBytesLeft = parser.AmountOfBytesLeft;
            string str = parser.ReadAsciiString();
            Assert.That(parser.AmountOfBytesLeft, Is.EqualTo(oldAmountOfBytesLeft - (str.Length + 1)));
        }

        private IEnumerable<TestCaseData> ReadAsciiStringTestSource3()
        {
            const string ShouldReturnEmptyStringWhenNullTerminatorAtFirstIndex =
                "An empty string should be returned when a null terminator is at the first index.";
            yield return new TestCaseData(new byte[1])
                .Returns(String.Empty)
                .SetDescription(ShouldReturnEmptyStringWhenNullTerminatorAtFirstIndex);
            yield return new TestCaseData(new byte[2])
                .Returns(String.Empty)
                .SetDescription(ShouldReturnEmptyStringWhenNullTerminatorAtFirstIndex);
            yield return new TestCaseData(new byte[20])
                .Returns(String.Empty)
                .SetDescription(ShouldReturnEmptyStringWhenNullTerminatorAtFirstIndex);

            Encoding ascii = Encoding.ASCII;
            yield return new TestCaseData(ascii.GetBytes("Toodles\0"))
                .Returns("Toodles")
                .SetDescription("Null terminator at the very end.");
            yield return new TestCaseData(ascii.GetBytes("Toodles\0World\0"))
                .Returns("Toodles")
                .SetDescription("Multiple null terminators.");
        }

        [TestCaseSource("ReadAsciiStringTestSource3")]
        public string ReadAsciiString(
            byte[] dataBytes)
        {
            SidByteParser parser = CreateSidByteParserWithSpecifiedDataBytes(dataBytes);
            return parser.ReadAsciiString();
        }

        [Test]
        public void ReadDwordString_ThrowsSidByteParserException_IffDataByteCountIsLessThan4(
            [Values(0, 1, 3, 4, 20)] int dataByteCount)
        {
            SidByteParser parser = CreateSidByteParserWithRandomDataBytes(dataByteCount);
            IResolveConstraint fulfillsConstraint = parser.AmountOfBytesLeft < 4 ?
                (IResolveConstraint)Throws.InstanceOf<SidByteParserException>() : Throws.Nothing;
            Assert.That(() => parser.ReadDwordString(), fulfillsConstraint);
        }

        [Test]
        public void ReadDwordString_DecreasesBytesLeftBy4(
            [Values(4, 5, 20)] int dataByteCount)
        {
            SidByteParser parser = CreateSidByteParserWithRandomDataBytes(dataByteCount);
            int oldAmountOfBytesLeft = parser.AmountOfBytesLeft;
            parser.ReadDwordString();
            Assert.That(parser.AmountOfBytesLeft, Is.EqualTo(oldAmountOfBytesLeft - 4));
        }

        private IEnumerable<TestCaseData> ReadDwordStringTestSource()
        {
            yield return new TestCaseData(new byte[] { 0x5A, 0x7A, 0x7A, 0x5A })
                .Returns("ZzzZ")
                .SetDescription("The case should be preserved.");
            yield return new TestCaseData(new byte[] { 0x41, 0x61, 0x42, 0x62 })
                .Returns("bBaA")
                .SetDescription(TheValueShouldBeReadInLittleEndian);
        }

        [TestCaseSource("ReadDwordStringTestSource")]
        public string ReadDwordString(byte[] dataBytes)
        {
            SidByteParser parser = CreateSidByteParserWithSpecifiedDataBytes(dataBytes);
            return parser.ReadDwordString();
        }

        private byte[] GetRandomByteArray(int count)
        {
            byte[] bytes = new byte[count];
            new Random(0).NextBytes(bytes);
            return bytes;
        }

        private SidByteParser CreateSidByteParser(byte[] bytesToParse)
        {
            return new SidByteParser(bytesToParse);
        }

        private SidByteParser CreateSidByteParserWithZeroedDataBytes(int dataByteCount)
        {
            byte[] dataBytes = new byte[dataByteCount];
            return CreateSidByteParserWithSpecifiedDataBytes(dataBytes);
        }

        private SidByteParser CreateSidByteParserWithRandomDataBytes(int dataByteCount)
        {
            byte[] dataBytes = GetRandomByteArray(dataByteCount);
            return CreateSidByteParserWithSpecifiedDataBytes(dataBytes);
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