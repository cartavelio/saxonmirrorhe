using System;


namespace SaxonCS_NUnit
{
    public class ConfigTest {
        
        public static string repo = "/Users/mike/GitHub/saxon2020/";
        public static string qt4cg = "/Users/mike/GitHub/qt4cg/";
        
        
        public static string NUNIT_DATA_DIR = repo + "src/test/nunit/testdata/";
		public static string DATA_DIR = repo + "src/test/testdata/";
        public static string SANDPIT_DIR = DATA_DIR + "sandpit/";
        public static string XMARK_DIR = repo + "src/test/xmark/";
        public static string RESOURCES_DIR = NUNIT_DATA_DIR;
        public static string XSLT3TS_DIR =  qt4cg + "xslt40-test/tests/";

        public static Uri RepoUri = new Uri(repo);
        public static Uri Qt4cgUri = new Uri(qt4cg);
        public static Uri DataDirUri = new Uri(RepoUri, DATA_DIR);
        public static Uri BugsDirUri = new Uri(RepoUri, "src/test/bugs/");
        public static Uri NUnitDataDirUri = new Uri(RepoUri, "src/test/nunit/testdata/");
        public static Uri SandpitDirUri = new Uri(DataDirUri, "sandpit/");
    }
}
