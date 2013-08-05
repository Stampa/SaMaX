namespace SamaxLibraryTests
{
    using System;
    using System.Collections.Generic;
    using NUnit.Framework;
    using NUnit.Framework.Constraints;
    using SamaxLibrary.Sid;

    [TestFixture]
    public class AuthInfoServerToClientSidMessageTestFixture
    {
        private byte[] validMessageBytes = new byte[] { 0xff, 0x50, 0x66, 0x00, 0x00, 0x00, 0x00, 0x00, 0x3e, 0x4a, 0x63, 0x5d, 0x18, 0x08, 0x05, 0x00, 0x00, 0x8b, 0x51, 0x03, 0x70, 0x5f, 0xc7, 0x01, 0x76, 0x65, 0x72, 0x2d, 0x49, 0x58, 0x38, 0x36, 0x2d, 0x36, 0x2e, 0x6d, 0x70, 0x71, 0x00, 0x41, 0x3d, 0x33, 0x32, 0x37, 0x34, 0x31, 0x34, 0x36, 0x32, 0x37, 0x32, 0x20, 0x42, 0x3d, 0x32, 0x36, 0x38, 0x31, 0x33, 0x30, 0x30, 0x30, 0x30, 0x20, 0x43, 0x3d, 0x33, 0x39, 0x34, 0x34, 0x35, 0x35, 0x39, 0x36, 0x32, 0x20, 0x34, 0x20, 0x41, 0x3d, 0x41, 0x2b, 0x53, 0x20, 0x42, 0x3d, 0x42, 0x2b, 0x43, 0x20, 0x43, 0x3d, 0x43, 0x2d, 0x41, 0x20, 0x41, 0x3d, 0x41, 0x2d, 0x42, 0x00 };

        [Test]
        public void MessageType()
        {
            Assert.That(AuthInfoServerToClientSidMessage.MessageType, Is.EqualTo(SidMessageType.AuthInfo));
        }

        [Test]
        public void Constructor_WhenMessageBytesIsNull_ThrowsArgumentNullException()
        {
            Assert.That(() => CreateMessage(null), Throws.TypeOf<ArgumentNullException>());
        }

        [Test]
        public void Constructor_WhenMessageBytesIsInvalid_ThrowsArgumentException()
        {
            byte[] messageBytes = new byte[] { 1, 3, 6, 10, 15 };
            Assert.That(() => CreateMessage(messageBytes), Throws.ArgumentException);
        }

        [Test]
        public void Constructor_WhenMessageBytesHasTrailingBytes_ThrowsArgumentException()
        {
            List<byte> messageBytes = new List<byte>(validMessageBytes);
            messageBytes.Add(0);
            Assert.That(() => CreateMessage(messageBytes.ToArray()), Throws.ArgumentException);
        }

        [TestCase(
            new byte[] { 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF }, true,
            Description = "Negative value.")]
        [TestCase(
            new byte[] { 0xFF, 0x3F, 0xC0, 0xD1, 0x5E, 0x5A, 0xC8, 0x24 }, false,
            Description = "Maximum allowed value.")]
        [TestCase(
            new byte[] { 0x00, 0x40, 0xC0, 0xD1, 0x5E, 0x5A, 0xC8, 0x24 }, true,
            Description = "Smallest disallowed (positive) value.")]
        public void Constructor_WhenMpqFileTimeIsInvalid_ThrowsArgumentException(
            byte[] mpqFileTimeBytes,
            bool mpqFileTimeIsInvalid)
        {
            byte[] messageBytes = validMessageBytes;
            Array.Copy(mpqFileTimeBytes, 0, messageBytes, 16, 8);
            IResolveConstraint fulfillsConstraint = mpqFileTimeIsInvalid ?
                (IResolveConstraint)Throws.ArgumentException : Throws.Nothing;
            Assert.That(() => CreateMessage(messageBytes), fulfillsConstraint);
        }

        [Test]
        public void Constructor_WhenMessageBytesIsValid_SetsPropertiesProperly()
        {
            var message = CreateMessage(validMessageBytes);
            Assert.That(message.Bytes, Is.EqualTo(validMessageBytes));
            Assert.That(message.LogonType, Is.EqualTo(LogonType.BrokenSha1));
            Assert.That(message.MpqFileName, Is.EqualTo("ver-IX86-6.mpq"));

            // This is not known to be correct
            Assert.That(message.MpqFileTime, Is.EqualTo(new DateTime(2007, 03, 05, 22, 48, 30)));
            Assert.That(message.ServerToken, Is.EqualTo(0x5D634A3E));
            Assert.That(message.UdpValue, Is.EqualTo(0x50818));
            Assert.That(message.ValueString, Is.EqualTo("A=3274146272 B=268130000 C=394455962 4 A=A+S B=B+C C=C-A A=A-B"));
        }

        private AuthInfoServerToClientSidMessage CreateMessage(byte[] messageBytes)
        {
            return new AuthInfoServerToClientSidMessage(messageBytes);
        }
    }
}
