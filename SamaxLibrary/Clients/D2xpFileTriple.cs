namespace SamaxLibrary.Clients
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using SamaxLibrary.Sid;

    /// <summary>
    /// This class represents a file triple used for authentication with Diablo II: Lord of
    /// destruction.
    /// </summary>
    public class D2xpFileTriple : FileTriple
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="D2xpFileTriple"/> class.
        /// </summary>
        /// <param name="gameExePath">The path to Game.exe.</param>
        /// <param name="bnClientDllPath">The path to Bnclient.dll.</param>
        /// <param name="d2ClientDllPath">The path to D2Client.dll.</param>
        /// TODO: Document exceptions
        [SuppressMessage(
        "Microsoft.StyleCop.CSharp.DocumentationRules",
        "SA1650:ElementDocumentationMustBeSpelledCorrectly",
        Justification = "Don't be hating on my time format (find a better solution than a suppression).")]
        public D2xpFileTriple(string gameExePath, string bnClientDllPath, string d2ClientDllPath)
            : base(new string[] { gameExePath, bnClientDllPath, d2ClientDllPath })
        {
        }
    }
}
