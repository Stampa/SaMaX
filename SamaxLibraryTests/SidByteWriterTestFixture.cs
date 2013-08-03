namespace SamaxLibraryTests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using NUnit.Framework;
    using NUnit.Framework.Constraints;
    using SamaxLibrary.Sid;

    /* Notes:
     * Most helper methods for creating a SidByteWriter relies on SidByteWriter.AppendByteArray to
     * work properly.
     * 
     * Some tests rely on Encoding.ASCII to work "as expected".
     */

    /* TODO:
     * It's unclear exactly what kind of strings the string methods should accept.
     * Anything but alphanumeric characters? Nonprintable characters? Whitespace characters?
     * Null terminators (probably not)? Non-ASCII (> 255, > 127)?
     * Figure this out and add unit tests for them!
     */

    [TestFixture]
    public class SidByteWriterTestFixture
    {
        private const string BoundaryCase = "Boundary case.";
        private const string TypicalCase = "Typical case.";
        private const string NonboundaryCase = "Nonboundary case (but not typical).";

        private const string TheValueShouldBeWrittenInLittleEndian =
            "The value should be written in little-endian.";

        [Test]
        public void Constructor_BytesIsEmpty()
        {
            SidByteWriter writer = CreateSidByteWriter();
            Assert.That(writer.Bytes.Length, Is.EqualTo(0));
        }

        [TestCase(0, Description = BoundaryCase)]
        [TestCase(1, Description = BoundaryCase)]
        [TestCase(20, Description = BoundaryCase)]
        public void AppendInt32_IncreasesDataByteCountBy4(int dataByteCount)
        {
            SidByteWriter writer = CreateSidByteWriterWithRandomDataBytes(dataByteCount);
            int oldAmountOfBytes = writer.Bytes.Length;
            writer.AppendInt32(0);
            Assert.That(writer.Bytes.Length, Is.EqualTo(oldAmountOfBytes + 4));
        }

        [TestCase(0, Description = BoundaryCase)]
        [TestCase(1, Description = BoundaryCase)]
        [TestCase(20, Description = TypicalCase)]
        public void AppendInt32_DoesNotAffectOriginalDataBytes(int dataByteCount)
        {
            byte[] dataBytes = GetRandomByteArray(dataByteCount);
            SidByteWriter writer = CreateSidByteWriterWithSpecifiedDataBytes(dataBytes);
            writer.AppendInt32(0);
            Assert.That(writer.Bytes.Take(dataByteCount), Is.EqualTo(dataBytes));
        }

        [TestCase(
            4 + 256 * (3 + 256 * (2 + 256 * 1)),
            new byte[4] { 4, 3, 2, 1 },
            Description = TheValueShouldBeWrittenInLittleEndian)]
        [TestCase(
            Int32.MinValue,
            new byte[4] { 0, 0, 0, 0x80 },
            Description = "It should be possible to write negative values.")]
        public void AppendInt32_AppendsSpecifiedBytes(Int32 value, params byte[] bytes)
        {
            SidByteWriter writer = CreateSidByteWriter();
            writer.AppendInt32(value);
            Assert.That(writer.Bytes.Skip(0), Is.EqualTo(bytes));
        }

        [Test]
        public void AppendDwordString_WhenStringIsNull_ThrowsArgumentNullException()
        {
            SidByteWriter writer = CreateSidByteWriter();
            Assert.That(() => writer.AppendDwordString(null), Throws.TypeOf<ArgumentNullException>());
        }

        [TestCase("", Description = BoundaryCase)]
        [TestCase("Abc", Description = BoundaryCase)]
        [TestCase("1234", Description = BoundaryCase)]
        [TestCase("Qwert", Description = BoundaryCase)]
        [TestCase("The raven flew down the avenue.", Description = NonboundaryCase)]
        public void AppendDwordString_ThrowsArgumentException_IffStringLengthIsNot4(string dwordString)
        {
            SidByteWriter writer = CreateSidByteWriter();
            IResolveConstraint fulfillsConstraint = dwordString.Length != 4 ?
                (IResolveConstraint)Throws.ArgumentException : Throws.Nothing;
            Assert.That(() => writer.AppendDwordString(dwordString), fulfillsConstraint);
        }

        [TestCase(0, Description = BoundaryCase)]
        [TestCase(1, Description = BoundaryCase)]
        [TestCase(20, Description = TypicalCase)]
        public void AppendDwordString_IncreasesDataByteCountBy4(int dataByteCount)
        {
            SidByteWriter writer = CreateSidByteWriterWithRandomDataBytes(dataByteCount);
            int oldAmountOfBytes = writer.Bytes.Length;
            writer.AppendDwordString("Ab3W");
            Assert.That(writer.Bytes.Length, Is.EqualTo(oldAmountOfBytes + 4));
        }

        [TestCase(
            "ABCD",
            new byte[] { 0x44, 0x43, 0x42, 0x41 },
            Description = TheValueShouldBeWrittenInLittleEndian)]
        public void AppendDwordString_AppendsSpecifiedBytes(string dwordString, params byte[] bytes)
        {
            SidByteWriter writer = CreateSidByteWriter();
            writer.AppendDwordString(dwordString);
            Assert.That(writer.Bytes.Skip(0), Is.EqualTo(bytes));
        }

        [Test]
        public void AppendEnumAsDwordString_WhenEnumValueIsNull_ThrowsArgumentNullException()
        {
            SidByteWriter writer = CreateSidByteWriter();
            Assert.That(
                () => writer.AppendEnumAsDwordString(null),
                Throws.TypeOf<ArgumentNullException>());
        }

        [Test]
        public void AppendEnumAsDwordString_WhenEnumValueDoesNotMatchAnyEnumMember_ThrowsArgumentException()
        {
            SidByteWriter writer = CreateSidByteWriter();
            SidByteParserAndWriterTestEnum value = SidByteParserAndWriterTestEnum.NoMemberAtMePlusOne + 1;
            Assert.That(() => writer.AppendEnumAsDwordString(value), Throws.ArgumentException);
        }

        [TestCase(SidByteParserAndWriterTestEnum.X, Description = NonboundaryCase)]
        [TestCase(SidByteParserAndWriterTestEnum.Pan, Description = BoundaryCase)]
        [TestCase(SidByteParserAndWriterTestEnum.Ha1O, Description = BoundaryCase)]
        [TestCase(SidByteParserAndWriterTestEnum.Samax, Description = BoundaryCase)]
        [TestCase(SidByteParserAndWriterTestEnum.Member0, Description = NonboundaryCase)]
        public void AppendEnumAsDwordString_WhenEnumValueMatchesEnumMember_ThrowsArgumentException_IffStringRepresentationIsNotOfLength4(
            SidByteParserAndWriterTestEnum enumValue)
        {
            SidByteWriter writer = CreateSidByteWriter();
            IResolveConstraint fulfillsConstraint = enumValue.ToString().Length != 4 ?
                (IResolveConstraint)Throws.ArgumentException : Throws.Nothing;
            Assert.That(() => writer.AppendEnumAsDwordString(enumValue), fulfillsConstraint);
        }

        [TestCase(0, Description = BoundaryCase)]
        [TestCase(1, Description = BoundaryCase)]
        [TestCase(20, Description = TypicalCase)]
        public void AppendEnumAsDwordString_IncreasesDataByteCountBy4(int dataByteCount)
        {
            SidByteWriter writer = CreateSidByteWriterWithRandomDataBytes(dataByteCount);
            int oldAmountOfBytes = writer.Bytes.Length;
            writer.AppendEnumAsDwordString(SidByteParserAndWriterTestEnum.Ha1O);
            Assert.That(writer.Bytes.Length, Is.EqualTo(oldAmountOfBytes + 4));
        }

        private IEnumerable<TestCaseData> AppendEnumAsDwordStringTestSource()
        {
            // Relying on this to work "as expected" might be bad.
            Encoding ascii = Encoding.ASCII;

            yield return new TestCaseData(
                SidByteParserAndWriterTestEnum.Abba,
                ascii.GetBytes("ABBA"))
                .SetDescription("The value should be written in upper case.");
            yield return new TestCaseData(
                SidByteParserAndWriterTestEnum.Pals,
                ascii.GetBytes("SLAP"))
                .SetDescription(TheValueShouldBeWrittenInLittleEndian);
            yield return new TestCaseData(
                SidByteParserAndWriterTestEnum.Ha1O,
                ascii.GetBytes("O1AH"))
                .SetDescription("It should be possible to write digits.");
        }

        [TestCaseSource("AppendEnumAsDwordStringTestSource")]
        public void AppendEnumAsDwordString_AppendsSpecifiedBytes(
            SidByteParserAndWriterTestEnum enumValue, params byte[] bytes)
        {
            SidByteWriter writer = CreateSidByteWriter();
            writer.AppendEnumAsDwordString(enumValue);
            Assert.That(writer.Bytes.Skip(0), Is.EqualTo(bytes));
        }

        [Test]
        public void AppendAsciiString_WhenStringIsNull_ThrowsArgumentNullException()
        {
            SidByteWriter writer = CreateSidByteWriter();
            Assert.That(() => writer.AppendAsciiString(null), Throws.TypeOf<ArgumentNullException>());
        }

        [Test]
        public void AppendAsciiString_IncreasesDataByteCountByStringLengthPlusOne(
            [Values(0, 1, 20)] int dataByteCount,
            [Values("", "S", "laughter")] string asciiString)
        {
            SidByteWriter writer = CreateSidByteWriterWithRandomDataBytes(dataByteCount);
            int oldAmountOfBytes = writer.Bytes.Length;
            writer.AppendAsciiString(asciiString);
            Assert.That(writer.Bytes.Length, Is.EqualTo(oldAmountOfBytes + asciiString.Length + 1));
        }

        private IEnumerable<TestCaseData> AppendAsciiStringDataSource()
        {
            yield return new TestCaseData(String.Empty, new byte[] { 0 })
                .SetDescription(BoundaryCase);
            yield return new TestCaseData("J", new byte[] { 0x4A, 0 })
                .SetDescription(BoundaryCase);
            
            Encoding ascii = Encoding.ASCII;
            string asciiString = "Onomatopoeia";
            List<byte> byteList = ascii.GetBytes(asciiString).ToList();
            byteList.Add(0);
            yield return new TestCaseData(asciiString, byteList.ToArray())
                .SetDescription(TypicalCase);
        }

        [TestCaseSource("AppendAsciiStringDataSource")]
        public void AppendAsciiString_AppendsSpecifiedBytes(
            string asciiString,
            params byte[] bytes)
        {
            SidByteWriter writer = CreateSidByteWriter();
            writer.AppendAsciiString(asciiString);
            Assert.That(writer.Bytes.Skip(0), Is.EqualTo(bytes));
        }

        private byte[] GetRandomByteArray(int count)
        {
            byte[] bytes = new byte[count];
            new Random(0).NextBytes(bytes);
            return bytes;
        }

        private SidByteWriter CreateSidByteWriter()
        {
            return new SidByteWriter();
        }

        private SidByteWriter CreateSidByteWriterWithRandomDataBytes(int dataByteCount)
        {
            byte[] dataBytes = GetRandomByteArray(dataByteCount);
            return CreateSidByteWriterWithSpecifiedDataBytes(dataBytes);
        }

        private SidByteWriter CreateSidByteWriterWithSpecifiedDataBytes(params byte[] dataBytes)
        {
            SidByteWriter writer = new SidByteWriter();
            writer.AppendByteArray(dataBytes);
            return writer;
        }
    }
}
