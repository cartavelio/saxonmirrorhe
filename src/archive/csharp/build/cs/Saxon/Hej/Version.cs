using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej {
    public sealed class Version {

        static Version() {
            softwareEdition = "HE";
            softwareEdition = "EE";
              platform = new Saxon.Impl.Helpers.DotNetPlatform();
        }
        private const int MAJOR_VERSION = 12;
        private const int MINOR_VERSION = 5;
        private const int PATCH_VERSION = 0;
        private const int BUILD = 32818;
        private const string MAJOR_RELEASE_DATE = "2023-01-12";
        private const string MINOR_RELEASE_DATE = "2024-06-18";
        private Version()  {
        }
        public static string getProductName() {
            return "SAXON";
        }
        public static string getProductVendor() {
            return "Saxonica";
        }
        public static string getProductVariantAndVersion(string edition) {
            return ((edition+" ")+getProductVersion());
        }
        public static string getProductVersion() {
            if ((PATCH_VERSION == 0)) {
                return ((MAJOR_VERSION+".")+MINOR_VERSION);
            }
            return ((((MAJOR_VERSION+".")+MINOR_VERSION)+".")+PATCH_VERSION);
        }
        public static int[] getStructuredVersionNumber() {
            return new int[]{MAJOR_VERSION, MINOR_VERSION, BUILD, PATCH_VERSION};
        }
        public static string getReleaseDate() {
            return MINOR_RELEASE_DATE;
        }
        public static string getMajorReleaseDate() {
            return MAJOR_RELEASE_DATE;
        }
        public static string getProductTitle() {
            return (((((((getProductName()+'-')+getSoftwarePlatform())+'-')+softwareEdition)+' ')+getProductVersion())+" from Saxonica");
        }
        public static string getSoftwarePlatform() {
            return "CS";
        }
        public static string getWebSiteAddress() {
            return "http://www.saxonica.com/";
        }
        public static void main(string[] args) {
            System.Console.Error.WriteLine((((getProductTitle()+" (build ")+BUILD)+')'));
            if (((args.Length>0) && args[0].Equals("-resources"))) {
                platform.showEmbeddedResources();
            }
        }
        public static string softwareEdition;
        public static Saxon.Hej.Platform platform;
    
    }
}