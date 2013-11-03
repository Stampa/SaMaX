namespace SamaxLibrary.Sid
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Text;
    using System.Threading.Tasks;

    /// <summary>
    /// This class represents a SID_QUERYREALMS2 message sent from the server to the client.
    /// </summary>
    public class QueryRealms2ServerToClientSidMessage : SidMessage
    {
        /// <summary>
        /// The SID message type of the SID message that the
        /// <see cref="QueryRealms2ServerToClientSidMessage"/> class represents.
        /// </summary>
        public new const SidMessageType MessageType = SidMessageType.QueryRealms2;

        /// <summary>
        /// Gets a value whose purpose is unknown.
        /// </summary>
        /// TODO: Figure out what this value is.
        public Int32 Unknown { get; private set; }

        /// <summary>
        /// Gets the amount of realms in this message.
        /// </summary>
        public Int32 RealmCount { get; private set; }

        /// <summary>
        /// Gets an <see cref="IReadOnlyList{T}"/> containing the available realms.
        /// </summary>
        public IReadOnlyList<Realm> Realms { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="QueryRealms2ServerToClientSidMessage"/> class.
        /// </summary>
        /// <param name="messageBytes">An array of bytes that composes the message.</param>
        /// <exception cref="ArgumentNullException"><paramref name="messageBytes"/> is <c>null</c>.
        /// </exception>
        /// <exception cref="ArgumentException"><paramref name="messageBytes"/> does not represent
        /// a valid server-to-client SID_QUERYREALMS2 message.</exception>
        public QueryRealms2ServerToClientSidMessage(byte[] messageBytes)
            : base(messageBytes, MessageType)
        {
            if (messageBytes == null)
            {
                throw new ArgumentNullException("messageBytes");
            }

            SidByteParser parser = new SidByteParser(messageBytes);

            try
            {
                this.Unknown = parser.ReadInt32();
                this.RealmCount = parser.ReadInt32();

                var realmBuilder = new ReadOnlyCollectionBuilder<Realm>();
                for (int i = 0; i < this.RealmCount; ++i)
                {
                    Int32 unknown = parser.ReadInt32();
                    string title = parser.ReadAsciiString();
                    string description = parser.ReadAsciiString();
                    var realm = new Realm(unknown, title, description);
                    realmBuilder.Add(realm);
                }

                this.Realms = realmBuilder.ToReadOnlyCollection();
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
