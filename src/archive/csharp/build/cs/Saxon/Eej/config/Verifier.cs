using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.config {
    public abstract class Verifier {
        private const bool DISABLED = false;
        private const int MS_A_DAY = ((24*3600)*1000);
        private Saxon.Eej.config.Verifier.License primaryLicense = null;
        private readonly Saxon.Hej.z.IntHashMap<Saxon.Eej.config.Verifier.License> secondaryLicenses = new Saxon.Hej.z.IntHashMap<Saxon.Eej.config.Verifier.License>();
        protected internal int status = UNREAD;
        private const int UNREAD = 0;
        private const int AVAILABLE = 1;
        private const int UNAVAILABLE = 2;
        protected internal const string LICENSE_FILE_NAME = "saxon-license.lic";
        internal Verifier()  {
        }
        protected internal void setPrimaryLicense(Saxon.Eej.config.Verifier.License license) {
            primaryLicense = license;
        }
        protected internal Saxon.Eej.config.Verifier.License getPrimaryLicense() {
            return primaryLicense;
        }
        internal void displayLicenseMessage(Saxon.Hej.Configuration config) {
            loadPrimaryLicense(config);
            if (config.isTiming()) {
                config.getLogger().info(("Using license serial number "+getSerialNumber()));
            }
            if (!(config.getConfigurationProperty(Saxon.Hej.lib.Feature<bool>.SUPPRESS_EVALUATION_EXPIRY_WARNING))) {
                int left = primaryLicense.daysLeft();
                if ((left == 1)) {
                    config.getLogger().info("****** Saxon license expires tomorrow! ******");
                } else {
                    if (((left>0) && (left<30))) {
                        config.getLogger().info((("Saxon license expires in "+left)+" days"));
                    }
                }
            }
        }
        public string getFeature(string name, Saxon.Hej.Configuration config) {
            loadPrimaryLicense(config);
            return primaryLicense.getFeature(name);
        }
        internal  /* REPLACING */ void loadPrimaryLicense(Saxon.Hej.Configuration config) {
            if ((DISABLED || (status == UNAVAILABLE))) {
                throw new Saxon.Hej.trans.LicenseException("Saxon license disabled", Saxon.Hej.trans.LicenseException.NOT_FOUND);
            }
            if ((status == UNREAD)) {
                try {
                    loadLicense(config);
                    status = AVAILABLE;
                } catch (Saxon.Hej.trans.LicenseException e) {
                    primaryLicense = null;
                    status = UNAVAILABLE;
                    throw;
                }
            }
        }
        protected internal abstract void loadLicense(Saxon.Hej.Configuration config);
        internal  /* REPLACING */ void loadLicense(string filename) { lock(this) {
            System.IO.Stream @is;
            bool exists = false;
            try {
                if (filename.StartsWith("file:")) {
                    @is = connectToURI(filename);
                } else {
                    exists = new Saxon.Ejava.io.File(filename).exists();
                    @is = System.IO.File.OpenRead(filename);
                }
                primaryLicense = readLicenseFile(@is);
                status = AVAILABLE;
                @is.Close();
            } catch (System.IO.IOException e) {
                primaryLicense = null;
                status = UNAVAILABLE;
                throw new Saxon.Hej.trans.LicenseException(("Failed to read license file "+filename), (exists ? Saxon.Hej.trans.LicenseException.NOT_FOUND : Saxon.Hej.trans.LicenseException.CANNOT_READ));
            }
        }}
        protected internal System.IO.Stream connectToURI(string uri) {
            return new System.Net.WebClient().OpenRead(uri);
        }
        internal  /* REPLACING */ int registerSecondaryLicense(string dmk) { lock(this) {
            try {
                Saxon.Hej.value.Base64BinaryValue b64 = new Saxon.Hej.value.Base64BinaryValue(Saxon.Hej.str.StringView.of(dmk).tidy());
                byte[] binary = b64.getBinaryValue();
                System.IO.MemoryStream @is = new System.IO.MemoryStream(binary);
                Saxon.Eej.config.Verifier.License license = configure(new System.IO.StreamReader(@is, System.Text.Encoding.UTF8), true);
                int id = (dmk.GetHashCode()&0x3ffff);
                while ((secondaryLicenses.get(id) != null)) {
                    id++;
                }
                secondaryLicenses.put(id, license);
                return id;
            } catch (Saxon.Hej.trans.XPathException err) {
                throw new Saxon.Hej.trans.LicenseException(err.Message, Saxon.Hej.trans.LicenseException.INVALID);
            }
        }}
        internal void disableLicensing() {
            primaryLicense = null;
            status = UNAVAILABLE;
        }
        internal  /* REPLACING */ bool isFeatureAllowedBySecondaryLicense(int license, int feature) { lock(this) {
            Saxon.Eej.config.Verifier.License lic = secondaryLicenses.get(license);
            if ((lic == null)) {
                return false;
            }
            switch (feature) {
              case Saxon.Hej.Configuration.LicenseFeature.PROFESSIONAL_EDITION:
                return true;
              case Saxon.Hej.Configuration.LicenseFeature.ENTERPRISE_XSLT:
                return "yes".Equals(lic.getFeature("SAT"));
              case Saxon.Hej.Configuration.LicenseFeature.ENTERPRISE_XQUERY:
                return "yes".Equals(lic.getFeature("SAQ"));
              case Saxon.Hej.Configuration.LicenseFeature.SCHEMA_VALIDATION:
                return "yes".Equals(lic.getFeature("SAV"));
              default:
                return false;
            }
        }}
        protected internal Saxon.Eej.config.Verifier.License readLicenseFile(System.IO.Stream @is) {
            return configure(new System.IO.StreamReader(@is, System.Text.Encoding.UTF8), false);
        }
        internal  /* REPLACING */ void supplyLicenseKey(System.IO.TextReader reader) { lock(this) {
            Saxon.Eej.config.Verifier.License license = configure(reader, false);
            string value = license.getFeature("Company");
            if (((value == null) || !(value.StartsWith("OEM")))) {
                primaryLicense = null;
                status = UNAVAILABLE;
                throw new Saxon.Ejava.lang.IllegalArgumentException("License key supplied is not a valid OEM license key");
            }
            primaryLicense = license;
            status = AVAILABLE;
        }}
        private Saxon.Eej.config.Verifier.License configure(System.IO.TextReader reader, bool embedded) {
            Saxon.Eej.config.Verifier.License license;
            System.IO.TextReader lineReader = makeLineReader(reader);
            try {
                string line;
                string name;
                string value;
                Saxon.Ejava.lang.StringBuilder buffer = new Saxon.Ejava.lang.StringBuilder(80);
                Saxon.Ejava.util.Properties features = new Saxon.Ejava.util.Properties();
                license = new Saxon.Eej.config.Verifier.License(features);
                string signature = "";
                while (((line = readLine(lineReader)) != null)) {
                    if (((line.Trim().IndexOf('#') == 0) || (line.Trim().IndexOf('!') == 0))) {
                        continue ;
                    }
                    int index = line.IndexOf('=');
                    if ((index>0)) {
                        name = line.Substring(0, index - 0).Trim();
                        value = line.Substring(++index).Trim();
                    } else {
                        name = line.Trim();
                        value = "";
                    }
                    if (!("".Equals(name))) {
                        features.setProperty(name, value);
                        if ("Signature".Equals(name)) {
                            signature = value;
                        } else {
                            buffer.append(name).append('=').append(value).append('\n');
                        }
                    }
                }
                bool valid = verify(buffer.ToString(), signature);
                string licenseDesc = (embedded ? "embedded license" : "license file");
                if (!(valid)) {
                    throw new Saxon.Hej.trans.LicenseException((("Invalid "+licenseDesc)+" found"), Saxon.Hej.trans.LicenseException.INVALID);
                }
                string serial = license.getFeature("Serial");
                licenseDesc += ((" (serial number "+serial)+")");
                int daysLeft = license.daysLeft();
                if ((daysLeft<0)) {
                    if (("yes".Equals(features.getProperty("Evaluation")) || (daysLeft<-30))) {
                        license.clearFeatures();
                        throw new Saxon.Hej.trans.LicenseException((("The "+licenseDesc)+" has expired"), Saxon.Hej.trans.LicenseException.EXPIRED);
                    } else {
                        System.Console.Error.WriteLine(((((("Saxon "+licenseDesc)+" has expired. \n*** You are using the software without legal authority. ***")+"\nYou must obtain a new license immediately; meanwhile a delay of ")+(System.Math.Abs(daysLeft)*2))+" seconds will be imposed."));
                        try {
                            Saxon.Ejava.lang.Thread.sleep((System.Math.Abs(daysLeft)*2000));
                        } catch (System.Threading.ThreadInterruptedException e) {
                            throw new Saxon.Hej.trans.LicenseException((("Saxon license "+licenseDesc)+" has expired"), Saxon.Hej.trans.LicenseException.EXPIRED);
                        }
                    }
                }
                Saxon.Hej.value.DateValue licenseIssued = new Saxon.Hej.value.DateValue(Saxon.Hej.str.BMPString.of((features.getProperty("Issued").Substring(0, 10 - 0)+'Z')));
                Saxon.Hej.value.DateValue majorVersionIssued = new Saxon.Hej.value.DateValue(Saxon.Hej.str.BMPString.of((Saxon.Hej.Version.getMajorReleaseDate()+'Z')));
                string upgradeDays = features.getProperty("UpgradeDays");
                int udays = 366;
                if ((upgradeDays != null)) {
                    udays = System.Int32.Parse(upgradeDays.Trim());
                }
                Saxon.Hej.value.DayTimeDurationValue sinceMajorRelease = majorVersionIssued.subtract(licenseIssued, null);
                if ((sinceMajorRelease.getLengthInSeconds()>(((60*60)*24)*udays))) {
                    throw new Saxon.Hej.trans.LicenseException((("The "+licenseDesc)+" does not cover upgrade to this Saxon version"), Saxon.Hej.trans.LicenseException.EXPIRED);
                }
                Saxon.Hej.value.DateValue minorVersionIssued = new Saxon.Hej.value.DateValue(Saxon.Hej.str.BMPString.of((Saxon.Hej.Version.getReleaseDate()+'Z')));
                string maintenanceDays = features.getProperty("MaintenanceDays");
                int mdays = udays;
                if ((maintenanceDays != null)) {
                    mdays = System.Int32.Parse(maintenanceDays.Trim());
                }
                Saxon.Hej.value.DayTimeDurationValue sinceMinorRelease = minorVersionIssued.subtract(licenseIssued, null);
                if ((sinceMinorRelease.getLengthInSeconds()>(((60*60)*24)*mdays))) {
                    throw new Saxon.Hej.trans.LicenseException((("The "+licenseDesc)+" does not cover this Saxon maintenance release"), Saxon.Hej.trans.LicenseException.EXPIRED);
                }
            } catch (Saxon.Ejava.io.UnsupportedEncodingException e) {
                throw new Saxon.Hej.trans.LicenseException("License file uses windows-1252, which is an unsupported encoding", Saxon.Hej.trans.LicenseException.INVALID);
            } catch (System.IO.IOException e) {
                throw new Saxon.Hej.trans.LicenseException(e.Message, Saxon.Hej.trans.LicenseException.NOT_FOUND);
            } catch (Saxon.Hej.trans.XPathException e) {
                throw new Saxon.Hej.trans.LicenseException("Invalid date in license file", Saxon.Hej.trans.LicenseException.INVALID);
            }
            return license;
        }
        private System.IO.TextReader makeLineReader(System.IO.TextReader reader) {
            return reader;
        }
        private static string readLine(System.IO.TextReader reader) {
            return reader.ReadLine();
        }
        internal string getSerialNumber() {
            return primaryLicense.getFeature("Serial");
        }
        protected internal static byte[] convertHexToBinary(string hex) {
            return System.Convert.FromHexString(hex);
        }
        protected internal abstract bool verify(string data, string signature);
        protected internal class License {
            protected internal readonly Saxon.Ejava.util.Properties features;
            public License(Saxon.Ejava.util.Properties features)  {
                this.features = features;
            }
            internal int daysLeft() {
                string company = features.getProperty("Company");
                if (((company != null) && company.StartsWith("OEM"))) {
                    return 0;
                }
                string expiration = features.getProperty("Expiration");
                if ((expiration == null)) {
                    return -1;
                } else {
                    if ((expiration.Trim().Length==0 || expiration.Contains("never"))) {
                        return 0;
                    }
                }
                try {
                    string evaluating = features.getProperty("Evaluation");
                    Saxon.Hej.value.DateValue expiryDate = new Saxon.Hej.value.DateValue(Saxon.Hej.str.BMPString.of(expiration));
                    long expiryTimeMillis = (((long)expiryDate.toDateTime().secondsSinceEpoch())*1000);
                    long time = (expiryTimeMillis-Saxon.Ejava.lang.System.currentTimeMillis());
                    if (("yes".Equals(evaluating) && ((((time/300000))%151) == 0))) {
                        clearFeatures();
                        throw new Saxon.Hej.trans.LicenseException("Evaluation license temporarily suspended: please try again later", Saxon.Hej.trans.LicenseException.EXPIRED);
                    }
                    return (1+(int)(((time/MS_A_DAY))));
                } catch (Saxon.Hej.trans.XPathException err) {
                    clearFeatures();
                    throw new Saxon.Hej.trans.LicenseException((("Invalid expiry date found in license (serial "+getFeature("Serial"))+")"), Saxon.Hej.trans.LicenseException.INVALID);
                }
            }
            public string getFeature(string name) { lock(this) {
                return features.getProperty(name);
            }}
            public  /* REPLACING private */ void clearFeatures() {
                if ((features != null)) {
                    features.clear();
                }
            }
        
        }
    
    }
}