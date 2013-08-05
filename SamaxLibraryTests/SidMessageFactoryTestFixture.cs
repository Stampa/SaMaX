namespace SamaxLibraryTests
{
    using System;
    using NUnit.Framework;
    using SamaxLibrary.Sid;

    [TestFixture]
    public class SidMessageFactoryTestFixture
    {
        private const byte InvalidSidMessageType = 0xFF;

        [Test]
        public void CreateClientToServerMessageFromBytes_WhenMessageBytesIsNull_ThrowsArgmuentNullException()
        {
            Assert.That(
                () => SidMessageFactory.CreateClientToServerMessageFromBytes(null),
                Throws.TypeOf<ArgumentNullException>());
        }

        [Test]
        public void CreateClientToServerMessageFromBytes_WhenMessageTypeIsInvalid_ThrowsArgumentException()
        {
            byte[] messageBytes = new byte[4] { 0xFF, InvalidSidMessageType, 0, 0 };
            Assert.That(
                () => SidMessageFactory.CreateClientToServerMessageFromBytes(messageBytes),
                Throws.ArgumentException);
        }
    }
}
