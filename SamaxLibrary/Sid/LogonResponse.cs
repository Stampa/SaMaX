namespace SamaxLibrary.Sid
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    /// <summary>
    /// Defines the logon responses in a SID_LOGONRESPONSE2 message.
    /// </summary>
    /// <seealso cref="LogonResponse2ServerToClientSidMessage"/>
    public enum LogonResponse : int
    {
        /// <summary>
        /// Indicates that the login process was successful.
        /// </summary>
        Success = 0,

        /// <summary>
        /// Indicates that the account does not exist.
        /// </summary>
        AccountDoesNotExist = 1,

        /// <summary>
        /// Indicates that the password is invalid for the specified account.
        /// </summary>
        InvalidPassword = 2,

        /// <summary>
        /// Indicates that the account is closed.
        /// </summary>
        AccountClosed = 6
    }
}
