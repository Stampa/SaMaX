namespace SamaxLibrary.Sid
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Text;
    using System.Threading.Tasks;

    /* TODO:
     * Handle nulls and other invalid values in the collections gracefully.
     * IDisposable!
     */

    /// <summary>
    /// This class represents a tuple of three files used in the revision check of client-to-server
    /// SID_AUTH_CHECK messages.
    /// </summary>
    /// <remarks>The order of the files matter.</remarks>
    /// <seealso cref="AuthCheckClientToServerSidMessage"/>
    public class FileTriple
    {
        /// <summary>
        /// Gets a triple of paths to the files.
        /// </summary>
        public IReadOnlyList<string> PathTriple { get; private set; }

        /// <summary>
        /// Gets the path to the executable file of the triple.
        /// </summary>
        public string ExePath
        {
            get
            {
                return this.PathTriple.First();
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FileTriple"/> class with paths to the
        /// three files of the triplet.
        /// </summary>
        /// <param name="pathTriple">An <see cref="IEnumerable{T}"/> with paths to the three files
        /// that compose the file triple.</param>
        /// <exception cref="ArgumentNullException"><paramref name="pathTriple"/> is
        /// <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException"><paramref name="pathTriple"/> is not a triple; that
        /// is, there are not exactly three paths in the collection.</exception>
        public FileTriple(IEnumerable<string> pathTriple)
        {
            if (pathTriple == null)
            {
                throw new ArgumentNullException("pathTriple");
            }

            if (pathTriple.Count() != 3)
            {
                throw new ArgumentException(
                    String.Format(
                        "There were {0} files in the collection, not {1}.",
                        pathTriple.Count(),
                        3));
            }

            var builder = new ReadOnlyCollectionBuilder<string>(pathTriple);
            this.PathTriple = builder.ToReadOnlyCollection();
        }

        /// <summary>
        /// Gets an <see cref="IEnumerable{T}"/> containing streams to the files of the triple.
        /// </summary>
        /// <returns>An <see cref="IEnumerable{T}"/> containing streams to the files of the triple.
        /// </returns>
        public IList<Stream> GetStreams()
        {
            // "Unroll" Linq statement or what have you for debugability
            List<Stream> streams = new List<Stream>();
            foreach (string path in this.PathTriple)
            {
                // TODO: Document all the exceptions that this may throw.
                // Note that File.OpenRead works as well (as seen in BNSharp)
                var stream = new FileStream(path, FileMode.Open);
                streams.Add(stream);
            }

            return streams;
        }
    }
}
