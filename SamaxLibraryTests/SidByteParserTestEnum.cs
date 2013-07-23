namespace SamaxLibraryTests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    public enum SidByteParserTestEnum : int
    {
        Member0,
        Member1,
        EndiannessTestMember = 1 + 2 * 256 + 3 * 256 * 256 + 4 * 256 * 256 * 256
    }
}
