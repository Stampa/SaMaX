﻿namespace SamaxLibrary.Sid
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Text;
    using System.Threading.Tasks;
    using BNSharp.BattleNet.Core;

    /// <summary>
    /// This class represents a SID_AUTH_CHECK message sent from the client to the server.
    /// </summary>
    public class AuthCheckClientToServerSidMessage : SidMessage
    {
        /// <summary>
        /// The SID message type of the SID message that the
        /// <see cref="AuthCheckClientToServerSidMessage"/> class represents.
        /// </summary>
        public new const SidMessageType MessageType = SidMessageType.AuthCheck;

        /// <summary>
        /// Gets the client token.
        /// </summary>
        /// <remarks>The client token is a random value generated by the client.</remarks>
        public Int32 ClientToken { get; private set; }

        /// <summary>
        /// Gets the version of the executable.
        /// </summary>
        /// TODO: Clarify what executable (Game.exe for Diablo 2 probably)
        public Int32 ExecutableVersion { get; private set; }

        /// <summary>
        /// Gets the the executable hash as returned by
        /// <see cref="CheckRevision.DoCheckRevision"/>.
        /// </summary>
        public Int32 ExecutableHash { get; private set; }

        /// <summary>
        /// Gets the amount of CD keys whose data this message contains.
        /// </summary>
        /// TODO: Should this be a UInt32?
        public Int32 AmountOfCDKeys { get; private set; }

        /// <summary>
        /// Gets a value indicating something.
        /// </summary>
        /// <remarks>This value should supposedly be FALSE (0) for all games but Starcraft, Japan Starcraft
        /// and Warcraft II.</remarks>
        /// TODO: Consider making this into a bool and make Boolean functions for the
        /// SID parser and writer classes.
        public Int32 SpawnCDKey { get; private set; }

        /// <summary>
        /// Gets the data for all CD keys in this message.
        /// </summary>
        public IReadOnlyList<SidAuthCheckKeyData> KeyData { get; private set; }

        /// <summary>
        /// Gets a string containing the following values separated by one space:
        /// <list type="number">
        ///     <item><description>Executable name</description></item>
        ///     <item><description>Last modified date (MM/DD/YY?)</description></item>
        ///     <item><description>Last modified time (hh:mm:ss)</description></item>
        ///     <item><description>Executable size in bytes</description></item>
        /// </list>
        /// </summary>
        [SuppressMessage(
            "Microsoft.StyleCop.CSharp.DocumentationRules",
            "SA1650:ElementDocumentationMustBeSpelledCorrectly",
            Justification = "Don't be hating on my time format (find a better solution than a suppression).")]
        public string ExecutableInformation { get; private set; }

        /// <summary>
        /// Gets the owner of the CD key.
        /// </summary>
        /// <remarks>If this value is greater than 15 bytes, it will supposedly be trimmed.
        /// </remarks>
        public string CDKeyOwner { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="AuthCheckClientToServerSidMessage"/>
        /// class.
        /// </summary>
        /// <param name="messageBytes">An array of bytes that composes the message.</param>
        /// <exception cref="ArgumentNullException"><paramref name="messageBytes"/> is <c>null</c>.
        /// </exception>
        /// <exception cref="ArgumentException"><paramref name="messageBytes"/> does not represent
        /// a valid client-to-server SID_AUTH_CHECK message.</exception>
        public AuthCheckClientToServerSidMessage(byte[] messageBytes)
            : base(messageBytes, MessageType)
        {
            if (messageBytes == null)
            {
                throw new ArgumentNullException("messageBytes");
            }

            SidByteParser parser = new SidByteParser(messageBytes);

            try
            {
                this.ClientToken = parser.ReadInt32();
                this.ExecutableVersion = parser.ReadInt32();
                this.ExecutableHash = parser.ReadInt32();
                this.AmountOfCDKeys = parser.ReadInt32();
                this.SpawnCDKey = parser.ReadInt32(); // ReadBoolean?

                var keyDataBuilder = new ReadOnlyCollectionBuilder<SidAuthCheckKeyData>();
                for (int i = 0; i < this.AmountOfCDKeys; i++)
                {
                    byte[] keyDataBytes = parser.ReadByteArray(SidAuthCheckKeyData.LengthInBytes);
                    SidAuthCheckKeyData keyData = new SidAuthCheckKeyData(keyDataBytes);
                    keyDataBuilder.Add(keyData);
                }

                this.KeyData = keyDataBuilder.ToReadOnlyCollection();

                this.ExecutableInformation = parser.ReadAsciiString();
                this.CDKeyOwner = parser.ReadAsciiString();
            }
            catch (SidByteParserException ex)
            {
                throw new ArgumentException(
                    String.Format("The bytes could not be parsed successfully: {0}", ex.Message),
                    ex);
            }

            if (parser.HasBytesLeft)
            {
                throw new ArgumentException("There were unexpected bytes at the end of the message.");
            }
        }

        /// <summary>
        /// Creates an instance of the <see cref="AuthCheckClientToServerSidMessage"/> from
        /// high-level data.
        /// </summary>
        /// <param name="fileTriple">A file triple used in the revision check. See
        /// <see cref="CheckRevision.DoCheckRevision"/> for more information.</param>
        /// <param name="productID">The product ID.</param>
        /// <param name="cdKey1">The first CD key.</param>
        /// <param name="cdKey2">The second CD key, or <see langword="null"/> if the specified
        /// product uses only a single key.</param>
        /// <param name="cdKeyOwner">The owner of the CD key.</param>
        /// TODO: Which CD key?
        /// <param name="mpqFileName">The MPQ file name received in the server-to-client
        /// SID_AUTH_INFO message.</param>
        /// <param name="valueString">The value string received in the server-to-client
        /// SID_AUTH_INFO message.</param>
        /// <param name="serverToken">The server token received in the server-to-client
        /// SID_AUTH_INFO message.</param>
        /// <returns>An instance of the <see cref="AuthCheckClientToServerSidMessage"/> class with
        /// the specified data.</returns>
        public static AuthCheckClientToServerSidMessage CreateFromHighLevelData(
            FileTriple fileTriple,
            ProductID productID,
            string cdKey1,
            string cdKey2,
            string cdKeyOwner,
            string mpqFileName,
            string valueString,
            Int32 serverToken)
        {
            bool usingLockdown = mpqFileName.StartsWith("LOCKDOWN", StringComparison.OrdinalIgnoreCase);
            if (usingLockdown)
            {
                throw new NotImplementedException("Revision check using lockdown has not been implemented.");
            }

            int mpqNumber = CheckRevision.ExtractMPQNumber(mpqFileName);

            Int32 clientToken = new Random().Next();
            string exeInformation;
            Int32 exeVersion = CheckRevision.GetExeInfo(fileTriple.ExePath, out exeInformation);
            Int32 exeHash = CheckRevision.DoCheckRevision(valueString, fileTriple.GetStreams(), mpqNumber);
            Int32 numberOfCDKeys = GetNumberOfKeysForProduct(productID);
            Int32 spawn = 0; // TODO: Hardcoding to 0 is bad and it should be a type of its own (SID boolean)

            CdKey key1 = new CdKey(cdKey1);
            CdKey key2 = (numberOfCDKeys == 2) ? new CdKey(cdKey2) : null;

            SidByteWriter writer = new SidByteWriter();
            writer.AppendInt32(clientToken);
            writer.AppendInt32(exeVersion);
            writer.AppendInt32(exeHash);
            writer.AppendInt32(numberOfCDKeys);
            writer.AppendInt32(spawn);

            AppendKeyToWriter(writer, key1, clientToken, serverToken);
            if (numberOfCDKeys == 2)
            {
                AppendKeyToWriter(writer, key2, clientToken, serverToken);
            }

            writer.AppendAsciiString(exeInformation);
            writer.AppendAsciiString(cdKeyOwner);

            byte[] dataBytes = writer.Bytes;
            byte[] messageBytes = SidMessage.GetMessageBytes(dataBytes, MessageType);

            return new AuthCheckClientToServerSidMessage(messageBytes);
        }

        /// <summary>
        /// Appends a CD key (<see cref="CdKey"/>) to the specified writer.
        /// </summary>
        /// <param name="writer">The SID byte writer to which to append the CD key.</param>
        /// <param name="key">The CD key to append.</param>
        /// <param name="clientToken">A client token to use.</param>
        /// <param name="serverToken">The server token received in the server-to-client
        /// SID_AUTH_INFO message.</param>
        private static void AppendKeyToWriter(SidByteWriter writer, CdKey key, int clientToken, int serverToken)
        {
            writer.AppendInt32(key.Key.Length);
            writer.AppendInt32(key.Product);
            writer.AppendInt32(key.Value1);
            writer.AppendInt32(0);
            byte[] keyHash = key.GetHash(clientToken, serverToken);
            writer.AppendByteArray(keyHash);
        }

        /// <summary>
        /// Returns the number of keys used for a specified product. 
        /// </summary>
        /// <param name="productID">The ID of the product.</param>
        /// <returns>The number of keys used for the specified product.</returns>
        private static int GetNumberOfKeysForProduct(ProductID productID)
        {
            switch (productID)
            {
                case ProductID.D2xp:
                    return 2;
                default:
                    throw new Exception("Switch failed."); // TODO
            }
        }
    }
}
