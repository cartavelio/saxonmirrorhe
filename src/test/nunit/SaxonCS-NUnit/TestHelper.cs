using System;
using System.IO;

namespace SaxonCS_NUnit;

public class TestHelper
{
    public static string DataDirPath(string path)
    {
        return Path.Combine(ConfigTest.DATA_DIR, path);
    }

    public static Uri DataDirUri(string path)
    {
        return new Uri(ConfigTest.DataDirUri, path);
    }

    public static string XsltTestSuitePath(string path)
    {
        return Path.Combine(ConfigTest.XSLT3TS_DIR, path);
    }

    public static string RepoPath(string path)
    {
        return Path.Combine(ConfigTest.repo, path);
    }
}