namespace SamaxLibrary.Sid
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Text;
    using System.Threading.Tasks;

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
        /// TODO: Mention in the remarks where this value is received.
        public Int32 ClientToken { get; private set; }

        /// <summary>
        /// Gets the version of the executable.
        /// </summary>
        /// TODO: Clarify what executable (Game.exe for Diablo 2 probably)
        public Int32 ExecutableVersion { get; private set; }

        /// <summary>
        /// Gets the hash of the executable.
        /// </summary>
        /// TODO: If it is a hash, why is it a single dword?
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
                this.SpawnCDKey = parser.ReadInt32();

                var keyDataBuilder = new ReadOnlyCollectionBuilder<SidAuthCheckKeyData>();
                for (int i = 0; i < this.AmountOfCDKeys; i++)
                {
                    // UNDONE:   
                }
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
    }
}
