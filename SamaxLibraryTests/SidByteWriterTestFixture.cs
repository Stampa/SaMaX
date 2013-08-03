namespace SamaxLibraryTests
{
    using System;
    using NUnit.Framework;
    using SamaxLibrary.Sid;

    [TestFixture]
    public class SidByteWriterTestFixture
    {
        [Test]
        public void Constructor_BytesIsEmpty()
        {
            SidByteWriter writer = CreateSidByteWriter();
            Assert.That(writer.Bytes.Length, Is.EqualTo(0));
        }

        private SidByteWriter CreateSidByteWriter()
        {
            return new SidByteWriter();
        }
    }
}
