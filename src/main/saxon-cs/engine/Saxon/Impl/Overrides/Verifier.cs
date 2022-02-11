using System;

namespace Saxon.Impl.Overrides {
    public class Verifier {

        internal Verifier()  {
        }
        internal void displayLicenseMessage(net.sf.saxon.Configuration config) {
            Console.Error.WriteLine("Using built-in temporary license");
        }
        public string getFeature(string name, net.sf.saxon.Configuration config) {
            return null;
        }
        internal void loadPrimaryLicense(net.sf.saxon.Configuration config) {
            
        }
        internal void loadLicense(net.sf.saxon.Configuration config) {
            
        }
        private static System.IO.Stream lookInClassPath() {
            return null;
        }
        private static System.IO.Stream lookInInstallationDirectory(net.sf.saxon.Configuration config, System.Collections.Generic.IList<string> attempts) {
            return null;
        }
        private System.IO.Stream lookInCodeLocation(System.Collections.Generic.IList<string> attempts) {
            return null;
        }
        internal void loadLicense(string filename) { lock(this) {
            return;
        }}
        internal int registerSecondaryLicense(string dmk) { lock(this) {
            return -1;
        }}
        internal void disableLicensing() {
            
        }
        internal bool isFeatureAllowedBySecondaryLicense(int license, int feature) { lock(this) {
            return true;
        }}
        
        internal void supplyLicenseKey(System.IO.TextReader reader) { lock(this) {
            
        }}
        
        
        private static bool verify(string data, byte[] signature, byte[] encodedPublicKey) {
            return true;
        }

        public string getSerialNumber() {
            return "XX9999";
        }
    }
    
}