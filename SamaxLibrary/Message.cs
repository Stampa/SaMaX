namespace SamaxLibrary
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Text;

    /// <summary>
    /// This class represents a message that can be sent through a network.
    /// </summary>
    public abstract class Message
    {
        /// <summary>
        /// The bytes that compose this message.
        /// </summary>
        private readonly ReadOnlyCollection<byte> bytes;

        /// <summary>
        /// Gets the bytes that compose this message.
        /// </summary>
        public byte[] Bytes
        {
            get { return this.bytes.ToArray(); }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Message"/> class.
        /// </summary>
        /// <param name="messageBytes">The bytes that compose the message.</param>
        /// <exception cref="ArgumentNullException"><paramref name="messageBytes"/> is <c>null</c>.
        /// </exception>
        public Message(byte[] messageBytes)
        {
            if (messageBytes == null)
            {
                throw new ArgumentNullException("messageBytes");
            }

            this.bytes = new ReadOnlyCollection<byte>(messageBytes);
        }
    }
}
