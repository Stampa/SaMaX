namespace SamaxLibraryTests
{
    using System;
    using System.Linq;
    using NUnit.Framework;
    using SamaxLibrary.Sid;

    /* Notes:
     * Most helper methods for creating a SidByteWriter relies on SidByteWriter.AppendByteArray to
     * work properly.
     */

    [TestFixture]
    public class SidByteWriterTestFixture
    {
        private const string BoundaryCase = "Boundary case.";
        private const string TypicalCase = "Typical case.";

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
            SidByteWriter writer = CreateSidByteWriterWithZeroedDataBytes(dataByteCount);
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
        public void AppendInt32_AppendsSpecifiedBytes(
            Int32 value, params byte[] bytes)
        {
            SidByteWriter writer = CreateSidByteWriter();
            writer.AppendInt32(value);
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

        private SidByteWriter CreateSidByteWriterWithZeroedDataBytes(int dataByteCount)
        {
            byte[] dataBytes = new byte[dataByteCount];
            return CreateSidByteWriterWithSpecifiedDataBytes(dataBytes);
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
