namespace SamaxLibrary.Clients
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    /// <summary>
    /// The exception that is thrown when some generic error occurs in one of the client classes.
    /// </summary>
    /// TODO: Rename this exception!
    public class ClientException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ClientException"/> class.
        /// </summary>
        public ClientException()
            : this(null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ClientException"/> class with a specified
        /// error message.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.
        /// </param>
        public ClientException(string message)
            : this(message, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ClientException"/> class with a specified
        /// error message and a reference to the inner exception that is the cause of this
        /// exception.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.
        /// </param>
        /// <param name="innerException">The exception that is the cause of the current exception,
        /// or a null reference if no inner exception is specified.</param>
        public ClientException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
