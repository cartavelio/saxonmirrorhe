using System;
using System.IO;

namespace SaxonCS_NUnit
{
    public class ConfigTest {
        
        public static string repo = Environment.GetEnvironmentVariable("SAXONDEV_DIR_13") 
                                    ?? Environment.GetEnvironmentVariable("SAXONDEV_DIR");
        public static string extraTests = Environment.GetEnvironmentVariable("SAXON_EXTRA_TEST_SUITE_DIR");
        
        
        public static string NUNIT_DATA_DIR = Path.Combine(repo, "src/test/nunit/testdata/");
		public static string DATA_DIR = Path.Combine(repo, "src/test/testdata/");
        public static string SANDPIT_DIR = Path.Combine(DATA_DIR, "sandpit/");
        public static string XMARK_DIR = Path.Combine(extraTests, "data/xmark/");
        public static string RESOURCES_DIR = NUNIT_DATA_DIR;
        public static string XSLT3TS_DIR =  Path.Combine(Environment.GetEnvironmentVariable("W3C_XSLT40_TEST_SUITE_DIR"), "tests");

        public static Uri RepoUri = new Uri(repo);
        public static Uri DataDirUri = new Uri(RepoUri, DATA_DIR);
        public static Uri SandpitDirUri = new Uri(DataDirUri, "sandpit/");
    }
}
