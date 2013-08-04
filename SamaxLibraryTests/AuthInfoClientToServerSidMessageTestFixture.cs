namespace SamaxLibraryTests
{
    using System;
    using System.Collections.Generic;
    using NUnit.Framework;
    using SamaxLibrary.Sid;
    using PlatformID = SamaxLibrary.Sid.PlatformID;
    
    /* TODO:
     * Add tests where the constructor fails because of invalid values for the individual fields.
     */

    [TestFixture]
    public class AuthInfoClientToServerSidMessageTestFixture
    {
        private byte[] validMessageBytes = new byte[] { 0xff, 0x50, 0x33, 0x00, 0x00, 0x00, 0x00, 0x00, 0x36, 0x38, 0x58, 0x49, 0x50, 0x58, 0x32, 0x44, 0x0d, 0x00, 0x00, 0x00, 0x53, 0x55, 0x6e, 0x65, 0xc0, 0xa8, 0x01, 0x02, 0x88, 0xff, 0xff, 0xff, 0x1d, 0x04, 0x00, 0x00, 0x1d, 0x04, 0x00, 0x00, 0x53, 0x57, 0x45, 0x00, 0x53, 0x77, 0x65, 0x64, 0x65, 0x6e, 0x00 };

        [Test]
        public void Constructor_WhenMessageBytesNull_ThrowsArgumentNullException()
        {
            Assert.That(() => CreateMessage(null), Throws.TypeOf<ArgumentNullException>());
        }

        [Test]
        public void Constructor_WhenMessageBytesIsInvalid_ThrowsArgumentException()
        {
            byte[] messageBytes = new byte[] { 0xB1, 0x6B, 0xAD, 0xBE, 0xEF };
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
            AuthInfoClientToServerSidMessage message = CreateMessage(validMessageBytes);
            Assert.That(message.Bytes, Is.EqualTo(validMessageBytes));
            Assert.That(message.Country, Is.EqualTo("Sweden"));
            Assert.That(message.CountryAbbreviation, Is.EqualTo("SWE"));
            Assert.That(message.LanguageID, Is.EqualTo(0x041D));
            Assert.That(message.LocaleID, Is.EqualTo(0x041D));
            Assert.That(message.LocalIpAddress, Is.EqualTo(new byte[] { 192, 168, 1, 2 }));
            Assert.That(message.PlatformID, Is.EqualTo(PlatformID.Ix86));
            Assert.That(message.ProductID, Is.EqualTo(ProductID.D2xp));
            Assert.That(message.ProtocolID, Is.EqualTo(0));
            Assert.That(message.ProductLanguage, Is.EqualTo("enUS"));
            Assert.That(message.TimeZoneBiasInMinutes, Is.EqualTo(-120));
            Assert.That(message.Version, Is.EqualTo(0xD));
        }

        private AuthInfoClientToServerSidMessage CreateMessage(byte[] messageBytes)
        {
            return new AuthInfoClientToServerSidMessage(messageBytes);
        }
    }
}
