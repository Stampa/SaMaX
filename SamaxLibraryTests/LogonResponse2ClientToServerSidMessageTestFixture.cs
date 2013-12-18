namespace SamaxLibraryTests
{
    using System;
    using System.Collections.Generic;
    using NUnit.Framework;
    using NUnit.Framework.Constraints;
    using SamaxLibrary.Sid;

    [TestFixture]
    public class LogonResponse2ClientToServerSidMessageTestFixture
    {
        private byte[] validMessageBytes = new byte[]
        {
            0xff, 0x3a, 0x29, 0x00, 0x97, 0x4a, 0x22, 0x00,
            0x4c, 0x4b, 0x8b, 0x0f, 0x0d, 0xc9, 0xe8, 0x79,
            0x89, 0x6d, 0x96, 0x6a, 0x2e, 0xaf, 0xf1, 0xf2,
            0xa0, 0x73, 0xd6, 0x4f, 0x98, 0xca, 0x72, 0x56,
            0x53, 0x61, 0x6d, 0x61, 0x78, 0x41, 0x63, 0x63,
            0x00
        };

        private const Int32 ValidMessageBytesClientToken = 0x00224a97;
        private const Int32 ValidMessageBytesServerToken = 0x0f8b4b4c;
        private const string ValidMessageBytesAccountName = "SamaxAcc";
        private const string ValidMessageBytesPassword = "SamaxPass";
        private BrokenSha1Hash validMessageBytesPasswordHash = new BrokenSha1Hash(
            new byte[20]
            {
                0x0d, 0xc9, 0xe8, 0x79, 0x89, 0x6d, 0x96, 0x6a,
                0x2e, 0xaf, 0xf1, 0xf2, 0xa0, 0x73, 0xd6, 0x4f,
                0x98, 0xca, 0x72, 0x56
            });

        private const Int32 ValidClientToken = 0; // How are these tokens valid?
        private const Int32 ValidServerToken = 0;
        private const string ValidAccountName = "ValidAcc";
        private const string ValidPassword = "Password";

        [Test]
        public void MessageType()
        {
            Assert.That(LogonResponse2ClientToServerSidMessage.MessageType, Is.EqualTo(SidMessageType.LogonResponse2));
        }

        [Test]
        public void Constructor_WhenMessageBytesNull_ThrowsArgumentNullException()
        {
            Assert.That(() => CreateMessage(null), Throws.TypeOf<ArgumentNullException>());
        }

        [Test]
        public void Constructor_WhenMessageBytesIsInvalid_ThrowsArgumentException()
        {
            byte[] invalidMessageBytes = new byte[] { 0xBA, 0xAD };
            Assert.That(() => CreateMessage(invalidMessageBytes), Throws.ArgumentException);
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
            Assert.That(message.ClientToken, Is.EqualTo(ValidMessageBytesClientToken));
            Assert.That(message.ServerToken, Is.EqualTo(ValidMessageBytesServerToken));
            Assert.That(message.AccountName, Is.EqualTo(ValidMessageBytesAccountName));
            Assert.That(message.TokenizedPasswordHash, Is.EqualTo(validMessageBytesPasswordHash));
        }

        [Test]
        public void CreateFromHighLevelData_ThrowsArgumentNullException_IffAccountNameOrPasswordIsNull(
            [Values(null, ValidAccountName)] string accountName,
            [Values(null, ValidPassword)] string password)
        {
            IResolveConstraint fulfillsConstraint = accountName == null || password == null ?
                (IResolveConstraint)Throws.TypeOf<ArgumentNullException>() : Throws.Nothing;

            Assert.That(
                () => LogonResponse2ClientToServerSidMessage.CreateFromHighLevelData(
                    ValidServerToken,
                    accountName,
                    password),
                fulfillsConstraint);
        }

        private IEnumerable<TestCaseData> CreateFromHighLevelDataTestSource()
        {
            yield return new TestCaseData(
                ValidMessageBytesServerToken,
                ValidMessageBytesPassword)
                .Returns(validMessageBytesPasswordHash);
        }

        [TestCaseSource("CreateFromHighLevelDataTestSource")]
        public BrokenSha1Hash CreateFromHighLevelData_GeneratesCorrectPasswordHash(
            Int32 serverToken,
            string password)
        {
            const string AccountName = "Few";
            var message = LogonResponse2ClientToServerSidMessage.CreateFromHighLevelData(
                serverToken,
                AccountName,
                password);

            return message.TokenizedPasswordHash;
        }

        //// TODO: More CreateFromHighLevelData tests!

        private LogonResponse2ClientToServerSidMessage CreateMessage(byte[] messageBytes)
        {
            return new LogonResponse2ClientToServerSidMessage(messageBytes);
        }
    }
}
