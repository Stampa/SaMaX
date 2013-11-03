namespace SamaxLibrary.Sid
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    /// <summary>
    /// This immutable class represents a realm in a SID_QUERYREALMS2 message.
    /// </summary>
    /// <seealso cref="QueryRealms2ServerToClientSidMessage"/>
    public class Realm
    {
        /// <summary>
        /// Gets a value whose purpose is unknown.
        /// </summary>
        /// TODO: Figure out what this value represents.
        public Int32 Unknown { get; private set; }

        /// <summary>
        /// Gets the title of the realm.
        /// </summary>
        public string Title { get; private set; }

        /// <summary>
        /// Gets the description of the realm.
        /// </summary>
        public string Description { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Realm"/> class.
        /// </summary>
        /// <param name="unknown">TODO: The purpose of this value is unknown.</param>
        /// <param name="title">The title of the realm.</param>
        /// <param name="description">The description of the realm.</param>
        public Realm(Int32 unknown, string title, string description)
        {
            this.Unknown = unknown;
            this.Title = title;
            this.Description = description;
        }
    }
}
