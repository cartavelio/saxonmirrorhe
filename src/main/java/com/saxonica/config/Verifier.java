package com.saxonica.config;

import net.sf.saxon.Configuration;
import net.sf.saxon.Version;
import net.sf.saxon.lib.Feature;
import net.sf.saxon.str.BMPString;
import net.sf.saxon.str.StringView;
import net.sf.saxon.trans.LicenseException;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.transpile.CSharpModifiers;
import net.sf.saxon.transpile.CSharpReplaceBody;
import net.sf.saxon.value.Base64BinaryValue;
import net.sf.saxon.value.DateValue;
import net.sf.saxon.value.DayTimeDurationValue;
import net.sf.saxon.z.IntHashMap;

import java.io.*;
import java.net.URL;
import java.nio.charset.StandardCharsets;
import java.util.Properties;

/**
 * A helper class for EnterpriseConfiguration that handles license keys as issued for the Java platform.
 */
public abstract class Verifier {

    private final static boolean DISABLED = false;

    private static final int MS_A_DAY = 24 * 3600 * 1000;
    private License primaryLicense = null;
    private final IntHashMap<License> secondaryLicenses = new IntHashMap<>();

    protected int status = UNREAD;

    private static final int UNREAD = 0;
    private static final int AVAILABLE = 1;
    private static final int UNAVAILABLE = 2;


    protected final static String LICENSE_FILE_NAME = "saxon-license.lic";


    Verifier() { }

    protected void setPrimaryLicense(License license) {
        primaryLicense = license;
    }

    protected License getPrimaryLicense() {
        return primaryLicense;
    }

    /**
     * Display a message about the license status
     *
     * @param config the Configuration
     */
    
    void displayLicenseMessage(Configuration config) {
        loadPrimaryLicense(config);
        if (config.isTiming()) {
            config.getLogger().info("Using license serial number " + getSerialNumber());
        }
        if (!config.getConfigurationProperty(Feature.SUPPRESS_EVALUATION_EXPIRY_WARNING)) {
            int left = primaryLicense.daysLeft();
            if (left == 1) {
                config.getLogger().info("****** Saxon license expires tomorrow! ******");
            } else if (left > 0 && left < 30) {
                config.getLogger().info("Saxon license expires in " + left + " days");
            }
        }
    }

    public String getFeature(String name, Configuration config) {
        loadPrimaryLicense(config);
        return primaryLicense.getFeature(name);
    }

    @CSharpModifiers(code = {"internal"})
    final void loadPrimaryLicense(Configuration config) {
        if (DISABLED || status==UNAVAILABLE) {
            throw new LicenseException("Saxon license disabled", LicenseException.NOT_FOUND);
        }
        if (status == UNREAD) {
            try {
                loadLicense(config);
                status = AVAILABLE;
            } catch (LicenseException e) {
                primaryLicense = null;
                status = UNAVAILABLE;
                throw e;
            }
        }
    }


    /**
     * Locate the license file, read it, and verify it
     * @param config the Saxon configuration
     * @throws LicenseException if, for example, a license is found and is invalid
     */
    protected abstract void loadLicense(Configuration config);


    /**
     * Read the primary license file directly from a specified file
     *
     * @param filename the name or URI of the license file
     */

    @CSharpModifiers(code={"internal"})
    synchronized void loadLicense(String filename) {
        InputStream is;
        boolean exists = false;
        try {
            if (filename.startsWith("file:")) {
                is = connectToURI(filename);
            } else {
                exists = new File(filename).exists();
                is = new FileInputStream(filename);
            }
            primaryLicense = readLicenseFile(is);
            status = AVAILABLE;
            is.close();
        } catch (IOException e) {
            primaryLicense = null;
            status = UNAVAILABLE;
            throw new LicenseException("Failed to read license file " + filename,
                    exists ? LicenseException.NOT_FOUND : LicenseException.CANNOT_READ);
        }
    }

    @CSharpReplaceBody(code="return new System.Net.WebClient().OpenRead(uri);")
    protected InputStream connectToURI(String uri) throws IOException {
        return new URL(uri).openStream();
    }

    /**
     * Register a license held locally within a compiled stylesheet or schema
     * @param dmk the binary representation of the local license
     * @return an integer key which can be used to identify this license among all the registered
     * local licenses
     */

    @CSharpModifiers(code = {"internal"})
    synchronized int registerSecondaryLicense(String dmk) {
        try {
            Base64BinaryValue b64 = new Base64BinaryValue(StringView.of(dmk).tidy());
            byte[] binary = b64.getBinaryValue();
            ByteArrayInputStream is = new ByteArrayInputStream(binary);
            License license = configure(new InputStreamReader(is, StandardCharsets.UTF_8), true);
            int id = dmk.hashCode() & 0x3ffff;
            while (secondaryLicenses.get(id) != null) {
                id++;
            }
            secondaryLicenses.put(id, license);
            return id;
        } catch (XPathException err) {
            throw new LicenseException(err.getMessage(), LicenseException.INVALID);
        }
    }

    void disableLicensing() {
        primaryLicense = null;
        status = UNAVAILABLE;
    }

    @CSharpModifiers(code = {"internal"})
    synchronized boolean isFeatureAllowedBySecondaryLicense(int license, int feature) {
        License lic = secondaryLicenses.get(license);
        if (lic == null) {
            return false;
        }
        switch (feature) {
            case Configuration.LicenseFeature.PROFESSIONAL_EDITION:
                return true;
            case Configuration.LicenseFeature.ENTERPRISE_XSLT:
                return "yes".equals(lic.getFeature("SAT"));
            case Configuration.LicenseFeature.ENTERPRISE_XQUERY:
                return "yes".equals(lic.getFeature("SAQ"));
            case Configuration.LicenseFeature.SCHEMA_VALIDATION:
                return "yes".equals(lic.getFeature("SAV"));
            default:
                return false;
        }
    }

    protected License readLicenseFile(InputStream is) {
        return configure(new InputStreamReader(is, StandardCharsets.UTF_8), false);
    }

    /**
     * This method allows the contents of a license key to be supplied programmatically.
     * This must be a license file as issued to authorized OEM distributors of Saxon-EE.
     *
     * @param reader A reader that will be used to read the contents of the license file
     *               (for example, a StringReader)
     * @throws IllegalArgumentException if the key supplied is not a valid OEM license key
     */

    @CSharpModifiers(code={"internal"})
    synchronized void supplyLicenseKey(Reader reader) {
        License license = configure(reader, false);
        String value = license.getFeature("Company");
        if (value == null || !value.startsWith("OEM")) {
            primaryLicense = null;
            status = UNAVAILABLE;
            throw new IllegalArgumentException("License key supplied is not a valid OEM license key");
        }
        primaryLicense = license;
        status = AVAILABLE;
    }


    private License configure(Reader reader, boolean embedded) {
        License license;
        Reader lineReader = makeLineReader(reader);
        try {
            String line;
            String name;
            String value;
            StringBuilder buffer = new StringBuilder(80);
            Properties features = new Properties();
            license = new License(features);
            String signature = "";
            while ((line = readLine(lineReader)) != null) {
                if (line.trim().indexOf('#') == 0 || line.trim().indexOf('!') == 0) {
                    continue;
                }
                int index = line.indexOf('=');
                if (index > 0) {
                    name = line.substring(0, index).trim();
                    value = line.substring(++index).trim();
                } else {
                    name = line.trim();
                    value = "";
                }

                if (!"".equals(name)) {
                    features.setProperty(name, value);

                    if ("Signature".equals(name)) {
                        signature = value;
                    } else {
                        buffer.append(name).append('=').append(value).append('\n');
                    }
                }
            }
            boolean valid = verify(buffer.toString(), signature);
            String licenseDesc = embedded ? "embedded license" : "license file";
            if (!valid) {
                throw new LicenseException("Invalid " + licenseDesc + " found", LicenseException.INVALID);
            }
            String serial = license.getFeature("Serial");
            licenseDesc += " (serial number " + serial + ")";
            int daysLeft = license.daysLeft();
            if (daysLeft < 0) {
                if ("yes".equals(features.getProperty("Evaluation")) || daysLeft < -45) {
                    license.clearFeatures();
                    throw new LicenseException("The " + licenseDesc + " has expired",
                                               LicenseException.EXPIRED);
                } else if (daysLeft < -15) {
                    System.err.println("Saxon " + licenseDesc + " has expired. \n*** You are using the software without lawful authority. ***"
                                               + "\nYou must obtain a new license immediately.");

                } else {
                    System.err.println("Saxon " + licenseDesc + " has expired. \n*** You are using the software without lawful authority. ***"
                                               + "\nYou must obtain a new license immediately; meanwhile a delay of " + Math.abs(daysLeft) * 2 +
                                               " seconds will be imposed. This delay will increase every day.");
                    try {
                        Thread.sleep(Math.abs(daysLeft) * 2000);
                    } catch (InterruptedException e) {
                        throw new LicenseException("Saxon license " + licenseDesc + " has expired",
                                                   LicenseException.EXPIRED);
                    }
                }
            }
            DateValue licenseIssued = new DateValue(BMPString.of(features.getProperty("Issued").substring(0, 10) + 'Z'));
            // some licenses were wrongly issued with YYYY-MM-DDTHH:MM:SS format
            DateValue majorVersionIssued = new DateValue(BMPString.of(Version.getMajorReleaseDate() + 'Z'));
            String upgradeDays = features.getProperty("UpgradeDays");
            int udays = 366;
            if (upgradeDays != null) {
                udays = Integer.parseInt(upgradeDays.trim());
            }
            DayTimeDurationValue sinceMajorRelease = majorVersionIssued.subtract(licenseIssued, null);
            if (sinceMajorRelease.getLengthInSeconds() > 60 * 60 * 24 * udays) {
                throw new LicenseException("The " + licenseDesc + " does not cover upgrade to this Saxon version",
                        LicenseException.EXPIRED);
            }
            DateValue minorVersionIssued = new DateValue(BMPString.of(Version.getReleaseDate() + 'Z'));
            String maintenanceDays = features.getProperty("MaintenanceDays");
            int mdays = udays;
            if (maintenanceDays != null) {
                mdays = Integer.parseInt(maintenanceDays.trim());
            }
            DayTimeDurationValue sinceMinorRelease = minorVersionIssued.subtract(licenseIssued, null);
            if (sinceMinorRelease.getLengthInSeconds() > 60 * 60 * 24 * mdays) {
                throw new LicenseException("The " + licenseDesc + " does not cover this Saxon maintenance release",
                        LicenseException.EXPIRED);
            }
        } catch (UnsupportedEncodingException e) {
            throw new LicenseException("License file uses windows-1252, which is an unsupported encoding",
                    LicenseException.INVALID);
        } catch (java.io.IOException e) {
            throw new LicenseException(e.getMessage(), LicenseException.NOT_FOUND);
        } catch (XPathException e) {
            throw new LicenseException("Invalid date in license file", LicenseException.INVALID);
        }
        return license;
    }

    @CSharpReplaceBody(code="return reader;")
    private Reader makeLineReader(Reader reader) {
        return new BufferedReader(reader);
    }

    @CSharpReplaceBody(code = "return reader.ReadLine();")
    private static String readLine(Reader reader) throws IOException {
        return ((BufferedReader)reader).readLine();
    }

    /**
     * Get the serial number of the license
     *
     * @return the serial number
     */

    String getSerialNumber() {
        return primaryLicense.getFeature("Serial");
    }

    /**
     * Convert a Hex String into the corresponding byte array by encoding
     * each two hexadecimal digits as a byte.
     *
     * @param hex Hexadecimal digits representation
     * @return byte Byte array
     */
    @CSharpReplaceBody(code="return System.Convert.FromHexString(hex);")
    protected static byte[] convertHexToBinary(String hex) {
        int len = hex.length() / 2;
        byte[] out = new byte[len];
        for (int i = 0; i < len; i++) {
            char c1 = hex.charAt(i * 2);
            char c2 = hex.charAt(i * 2 + 1);
            if ((c1 >= '0') && (c1 <= '9')) {
                out[i] = (byte) ((c1 - '0') * 16);
            } else if ((c1 >= 'A') && (c1 <= 'F')) {
                out[i] = (byte) ((c1 - 'A' + 10) * 16);
            } else {
                throw new IllegalArgumentException();
            }
            if ((c2 >= '0') && (c2 <= '9')) {
                out[i] += c2 - '0';
            } else if ((c2 >= 'A') && (c2 <= 'F')) {
                out[i] += c2 - 'A' + 10;
            } else {
                throw new IllegalArgumentException();
            }
        }
        return out;
    }

    /**
     * This method validates license data based on the signature and the public key.
     *
     * @param data             the license data.
     * @param signature        the digital signature
     * @return a boolean whether or the license is valid.
     * @throws LicenseException if a failure occurs in the verification process
     */
    protected abstract boolean verify(String data, String signature)
            throws LicenseException;



    protected static class License {

        protected final Properties features;

        public License(Properties features) {
            this.features = features;
        }

        /**
         * It returns how many days are left for the license.
         * The value 0 is valid, it may indicate a never expire license.
         *
         * @return how many days are left for the license.
         */

        int daysLeft() {
            String company = features.getProperty("Company");
            if (company != null && company.startsWith("OEM")) {
                return 0;
            }
            String expiration = features.getProperty("Expiration");
            if (expiration == null) {
                return -1;
            } else if (expiration.trim().isEmpty() || expiration.contains("never")) {
                return 0;
            }
            try {
                String evaluating = features.getProperty("Evaluation");
                DateValue expiryDate = new DateValue(BMPString.of(expiration));
                long expiryTimeMillis = expiryDate.toDateTime().secondsSinceEpoch().longValue()*1000;
                long time = expiryTimeMillis - System.currentTimeMillis();
                if ("yes".equals(evaluating) && (time / 300000) % 151 == 0) {
                    // this will put the software out of action for 5 minutes every 11 hours
                    clearFeatures();
                    throw new LicenseException("Evaluation license temporarily suspended: please try again later",
                                               LicenseException.EXPIRED);
                }
                return 1 + (int) (time / MS_A_DAY);
            } catch (XPathException err) {
                clearFeatures();
                throw new LicenseException("Invalid expiry date found in license (serial " +
                                                   getFeature("Serial") + ")", LicenseException.INVALID);
            }
        }

        /**
         * This method is called to get the feature of the license.
         *
         * @param name   the name of the required feature
         * @return the feature value of the license.
         */
        public synchronized String getFeature(String name) {
            return features.getProperty(name);
        }

        @CSharpModifiers(code={"public"})
        private void clearFeatures() {
            if (features != null) {
                features.clear();
            }
        }

    }


}

// Copyright (c) 2004-2022 Saxonica Limited
