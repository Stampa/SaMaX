namespace SamaxLibrary.Sid
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    /// <summary>
    /// This exception is thrown by the <see cref="SidByteParser"/> class when the byte array could
    /// not be parsed properly.
    /// </summary>
    /// TODO: Consider renaming this class.
    public class SidByteParserException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SidByteParserException"/> class.
        /// </summary>
        public SidByteParserException()
            : this(null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SidByteParserException"/> class with a
        /// specified error message.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.
        /// </param>
        public SidByteParserException(string message)
            : this(message, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SidByteParserException"/> class with a
        /// specified error message and a reference to the inner exception that is the cause of
        /// this exception.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.
        /// </param>
        /// <param name="innerException">The exception that is the cause of the current exception,
        /// or a null reference if no inner exception is specified.</param>
        public SidByteParserException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
