namespace SamaxLibraryTests
{
    using System;
    using System.Collections.Generic;
    using NUnit.Framework;
    using NUnit.Framework.Constraints;
    using SamaxLibrary.Sid;
    using PlatformID = SamaxLibrary.Sid.PlatformID;
    
    /* TODO:
     * Add tests where the constructor fails because of invalid values for the individual fields.
     */

    [TestFixture]
    public class AuthInfoClientToServerSidMessageTestFixture
    {
        private const string BoundaryCase = "Boundary case.";
        private const string TypicalCase = "Typical case.";
        private const string NonboundaryCase = "Nonboundary case (but not typical).";

        private byte[] validMessageBytes = new byte[] { 0xff, 0x50, 0x33, 0x00, 0x00, 0x00, 0x00, 0x00, 0x36, 0x38, 0x58, 0x49, 0x50, 0x58, 0x32, 0x44, 0x0d, 0x00, 0x00, 0x00, 0x53, 0x55, 0x6e, 0x65, 0xc0, 0xa8, 0x01, 0x02, 0x88, 0xff, 0xff, 0xff, 0x1d, 0x04, 0x00, 0x00, 0x1d, 0x04, 0x00, 0x00, 0x53, 0x57, 0x45, 0x00, 0x53, 0x77, 0x65, 0x64, 0x65, 0x6e, 0x00 };
        
        private const ProductID ValidProductID = ProductID.D2xp;
        private const ProductID NonmatchingProductID = (ProductID)(-17);
        private const Int32 ValidVersion = 0xD;
        private byte[] validLocalIPAddress = new byte[] { 192, 168, 1, 2 };

        [Test]
        public void MessageType()
        {
            Assert.That(AuthInfoClientToServerSidMessage.MessageType, Is.EqualTo(SidMessageType.AuthInfo));
        }

        [Test]
        public void Constructor_WhenMessageBytesIsNull_ThrowsArgumentNullException()
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

        [Test]
        public void CreateFromHighLevelData_WhenLocalIPAddressIsNull_ThrowsArgumentNullException()
        {
            Assert.That(
                () => AuthInfoClientToServerSidMessage.CreateFromHighLevelData(
                    ValidProductID,
                    ValidVersion,
                    null),
                Throws.TypeOf<ArgumentNullException>());
        }

        [Test]
        public void CreateFromHighLevelData_WhenProductIDValueIsNotAValidProductID_ThrowsArgumentException()
        {
            Assert.That(
                () => AuthInfoClientToServerSidMessage.CreateFromHighLevelData(
                    NonmatchingProductID,
                    ValidVersion,
                    validLocalIPAddress),
                Throws.ArgumentException);
        }

        [TestCase(new byte[0], Description = BoundaryCase)]
        [TestCase(new byte[1] { 78 }, Description = BoundaryCase)]
        [TestCase(new byte[3] { 0xC0, 0xA8, 0 }, Description = BoundaryCase)]
        [TestCase(new byte[4] { 192, 168, 1, 1 }, Description = BoundaryCase)]
        [TestCase(new byte[5] { 192, 168, 1, 1, 1 }, Description = BoundaryCase)]
        [TestCase(new byte[12] { 192, 168, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 }, Description = NonboundaryCase)]
        public void CreateFromHighLevelData_WhenTheRestIsValid_ThrowsArgumentException_IffLocalIPAddressIsNotOfLength4(
            byte[] localIPAddress)
        {
            IResolveConstraint fulfillsConstraint = localIPAddress.Length != 4 ?
                (IResolveConstraint)Throws.ArgumentException : Throws.Nothing;
            Assert.That(
                () => AuthInfoClientToServerSidMessage.CreateFromHighLevelData(
                    ValidProductID,
                    ValidVersion,
                    localIPAddress),
                fulfillsConstraint);
        }

        [Test]
        public void CreateFromHighLevelData_WithValidArguments_ReturnsMessageWithProperProperties()
        {
            var message = AuthInfoClientToServerSidMessage.CreateFromHighLevelData(
                ValidProductID,
                ValidVersion,
                validLocalIPAddress);
            Assert.That(message.LocalIpAddress, Is.EqualTo(validLocalIPAddress));
            Assert.That(message.ProductID, Is.EqualTo(ValidProductID));
            Assert.That(message.Version, Is.EqualTo(ValidVersion));

            // TODO: Assert more things here?
        }

        private AuthInfoClientToServerSidMessage CreateMessage(byte[] messageBytes)
        {
            return new AuthInfoClientToServerSidMessage(messageBytes);
        }
    }
}
