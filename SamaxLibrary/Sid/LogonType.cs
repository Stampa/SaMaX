// -----------------------------------------------------------------------
// <copyright file="LogonType.cs" company="TODO">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

namespace SamaxLibrary.Sid
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    /// <summary>
    /// Defines the logon types.
    /// TODO: Logon types for what? Battle.Net v. 1?
    /// </summary>
    public enum LogonType : uint
    {
        /// <summary>
        /// Indicates that Blizzard's broken version of the SHA-1 algorithm is used when logging on.
        /// </summary>
        /// <remarks>This logon type is supposedly used for StarCraft (including the expansion) and
        /// Diablo 2 (including the expansion).</remarks>
        BrokenSha1 = 0,
    }
}
