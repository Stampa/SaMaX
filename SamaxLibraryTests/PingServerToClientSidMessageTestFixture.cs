namespace SamaxLibraryTests
{
    using System;
    using System.Collections.Generic;
    using NUnit.Framework;
    using SamaxLibrary.Sid;

    [TestFixture]
    public class PingServerToClientSidMessageTestFixture
    {
        private byte[] validMessageBytes = new byte[] { 0xff, 0x25, 0x08, 0x00, 0x35, 0x90, 0xAD, 0xB4 };

        [Test]
        public void MessageType()
        {
            Assert.That(PingServerToClientSidMessage.MessageType, Is.EqualTo(SidMessageType.Ping));
        }

        [Test]
        public void Constructor_WhenMessageBytesIsNull_ThrowsArgumentNullException()
        {
            Assert.That(() => CreateMessage(null), Throws.TypeOf<ArgumentNullException>());
        }

        [Test]
        public void Constructor_WhenMessageBytesIsTooShort_ThrowsArgumentException()
        {
            byte[] messageBytes = new byte[] { 0xFF, 0x25, 0x04, 0x00 };
            Assert.That(() => CreateMessage(messageBytes), Throws.ArgumentException);
        }

        [Test]
        public void Constructor_WhenMessageBytesHasTrailingBytes_ThrowsArgumentException()
        {
            List<byte> messageBytes = new List<byte>(validMessageBytes);
            messageBytes.Add(0);
            Assert.That(() => CreateMessage(messageBytes.ToArray()), Throws.ArgumentException);
        }

        [Test]
        public void Constructor_WhenMessageBytesIsValid_SetsPropertiesProperly()
        {
            var message = CreateMessage(validMessageBytes);
            Assert.That(message.Bytes, Is.EqualTo(validMessageBytes));
            Assert.That(message.PingValue, Is.EqualTo(unchecked((int)0xB4AD9035)));
        }

        private PingServerToClientSidMessage CreateMessage(byte[] messageBytes)
        {
            return new PingServerToClientSidMessage(messageBytes);
        }
    }
}
