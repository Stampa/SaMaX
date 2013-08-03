namespace SamaxLibraryTests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    public enum SidByteParserAndWriterTestEnum : int
    {
        Member0 = 0,
        NoMemberAtMePlusOne = 1,
        Pals = 12,
        Abba,
        Ha1O,
        Samax,
        Pan,
        X,
        EndiannessTestMember = 1 + 2 * 256 + 3 * 256 * 256 + 4 * 256 * 256 * 256
    }
}
