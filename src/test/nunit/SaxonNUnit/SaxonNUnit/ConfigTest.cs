using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SaxonNUnit
{
    class ConfigTest
    {
        public static String REPO = "C:/Users/mike/GitHub/saxon2020/";
        public static String NUNIT_DATA_DIR = REPO + "src/test/testdata/";
        public static String DATA_DIR = NUNIT_DATA_DIR;
        public static String DATA_DIR2 = "file:///" + DATA_DIR;
        public static String XMARK_DIR = REPO + "src/test/xmark/";
        public static String RESOURCES_DIR = NUNIT_DATA_DIR;
        public static String XSLT3TS_DIR =  REPO + "src/test/xmark/";
    }
}
