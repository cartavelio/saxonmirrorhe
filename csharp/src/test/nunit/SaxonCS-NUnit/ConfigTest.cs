using System;
using System.IO;

namespace SaxonCS_NUnit
{
    public class ConfigTest {
        // These tests are run from .../saxondev/csharp/{he,ee,pe} and the relative path reflects that
        public static string repo = Path.GetFullPath(
            Path.Combine(Environment.CurrentDirectory, "../../../../../.."));
        public static string extraTests = Path.GetFullPath(Path.Combine(repo, "submodules/extra"));
        
        public static string NUNIT_DATA_DIR = Path.Combine(repo, "csharp/src/test/nunit/testdata/");
	public static string DATA_DIR = Path.Combine(repo, "src/test/testdata/");
        public static string SANDPIT_DIR = Path.Combine(DATA_DIR, "sandpit/");
        public static string XMARK_DIR = Path.Combine(extraTests, "data/xmark/");
        public static string RESOURCES_DIR = NUNIT_DATA_DIR;
        public static string XSLT3TS_DIR =  Path.Combine(repo, "submodules/xslt40/tests");

        public static Uri RepoUri = new Uri(repo);
        public static Uri DataDirUri = new Uri(RepoUri, DATA_DIR);
        public static Uri SandpitDirUri = new Uri(DataDirUri, "sandpit/");
    }
}
