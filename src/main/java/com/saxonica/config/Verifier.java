package com.saxonica.config;

import net.sf.saxon.Configuration;
import net.sf.saxon.Version;
import net.sf.saxon.lib.Feature;
import net.sf.saxon.trans.LicenseException;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.value.Base64BinaryValue;
import net.sf.saxon.value.DateValue;
import net.sf.saxon.value.DayTimeDurationValue;
import net.sf.saxon.z.IntHashMap;

import java.io.*;
import java.net.URL;
import java.nio.charset.StandardCharsets;
import java.security.*;
import java.security.spec.X509EncodedKeySpec;
import java.util.ArrayList;
import java.util.Enumeration;
import java.util.List;
import java.util.Properties;

/**
 * A helper class for EnterpriseConfiguration that handles license keys as issued for the Java platform.
 * From Saxon 9.2 the same license keys are also used on the .NET platform, though the option of using older
 * .NET license keys is retained for the time being.
 */
public class Verifier {

    private final static boolean DISABLED = false;

    private static final int MS_A_DAY = 24 * 3600 * 1000;
    private License primaryLicense = null;
    private IntHashMap<License> secondaryLicenses = new IntHashMap<License>();

    protected int status = UNREAD;

    public static final int UNREAD = 0;
    public static final int AVAILABLE = 1;
    public static final int UNAVAILABLE = 2;

    // beware: access to this Properties object is synchronized, so it needs to be infrequent

    public final static String LICENSE_FILE_NAME = "saxon-license.lic";

    // Public key created by the KeyManager for Saxon.

    private static final String PUBLIC_KEY = "308201B73082012C06072A8648CE3804013082011F02818100FD7F53811D75122952DF4A9" +
            "C2EECE4E7F611B7523CEF4400C31E3F80B6512669455D402251FB593D8D58FABFC5F5BA30F6CB9B556CD7813B801D346FF26660B76B9950" +
            "A5A49F9FE8047B1022C24FBBA9D7FEB7C61BF83B57E7C6A8A6150F04FB83F6D3C51EC3023554135A169132F675F3AE2B61D72AEFF222031" +
            "99DD14801C70215009760508F15230BCCB292B982A2EB840BF0581CF502818100F7E1A085D69B3DDECBBCAB5C36B857B97994AFBBFA3AEA" +
            "82F9574C0B3D0782675159578EBAD4594FE67107108180B449167123E84C281613B7CF09328CC8A6E13C167A8B547C8D28E0A3AE1E2BB3A" +
            "675916EA37F0BFA213562F1FB627A01243BCCA4F1BEA8519089A883DFE15AE59F06928B665E807B552564014C3BFECF492A038184000281" +
            "806CA067E469754A2F98D08965D35AB41252974F69FCD260152CFB58F94D8B956D6A1A91CC213A11107301DD37C85B383DDA409EC067D2A" +
            "8C4BE6651020A69AFD533630172D6F96F928667D7705D46E56D364E8E967002A7D864BACB02225F52B271BA1F6522D98FF8299C6273B24B" +
            "C41202C8857897E46B40FD7624A9CB0CC9E8";


    private static byte[] key = convertHexToBinary(PUBLIC_KEY);

    public Verifier() {
    }



    /**
     * Display a message about the license status
     *
     * @param config the Configuration
     */

    public void displayLicenseMessage(Configuration config) {
        loadPrimaryLicense(config);
        if (config.isTiming()) {
            config.getStandardErrorOutput().println("Using license serial number " + getSerialNumber());
        }
        if (!config.getConfigurationProperty(Feature.SUPPRESS_EVALUATION_EXPIRY_WARNING)) {
            int left = primaryLicense.daysLeft();
            if (left == 1) {
                config.getStandardErrorOutput().println("****** Saxon evaluation license expires tomorrow! ******");
            } else if (left > 0 && left < 15) {
                config.getStandardErrorOutput().println("Saxon evaluation license expires in " + left + " days");
            }
        }
    }

    public String getFeature(String name, Configuration config) {
        loadPrimaryLicense(config);
        return primaryLicense.getFeature(name);
    }

    protected final void loadPrimaryLicense(Configuration config) {
        if (DISABLED || status==UNAVAILABLE) {
            throw new LicenseException("Saxon license disabled", LicenseException.NOT_FOUND);
        }
        if (status == UNREAD) {
            try {
                primaryLicense = loadLicense(config);
                status = AVAILABLE;
            } catch (LicenseException e) {
                primaryLicense = null;
                status = UNAVAILABLE;
                throw e;
            }
        }
    }


    protected final License loadLicense(Configuration config) {
        InputStream is = null;
        try {
            List<String> attempts = new ArrayList<String>(4);
            is = lookInCodeLocation(is, attempts);
            if (is == null) {
                is = lookInInstallationDirectory(config, is, attempts);
            }
            if (is == null) {
                is = lookInClassPath(is);
            }
            if (is == null) {
                attempts.add("classpath");
                String message = "License file " + LICENSE_FILE_NAME + " not found. Tried in ";
                for (int i = 0; i < attempts.size(); i++) {
                    message += attempts.get(i);
                    if (i < attempts.size() - 2) {
                        message += ", ";
                    } else if (i == attempts.size() - 2) {
                        message += ", and ";
                    }
                }
                throw new LicenseException(message, LicenseException.NOT_FOUND);
            }
            return readLicenseFile(is);
        } finally {
            try {
                if (is != null) {
                    is.close();
                }
            } catch (IOException e) {
                //
            }
        }

    }

    private static InputStream lookInClassPath(InputStream is) {
        ClassLoader cl = Thread.currentThread().getContextClassLoader();
        if (cl == null) {
            cl = ClassLoader.getSystemClassLoader();
        }
        is = cl.getResourceAsStream(LICENSE_FILE_NAME);
        return is;
    }

    private static InputStream lookInInstallationDirectory(Configuration config, InputStream is, List<String> attempts) {
        String home = Version.platform.getInstallationDirectory(Version.softwareEdition, config);
        String path;
        if (home != null) {
            // For backwards compatibility (/bin/ no longer exists)
            if (home.endsWith("/") || home.endsWith("\\")) {
                path = home + "bin/" + LICENSE_FILE_NAME;
            } else {
                path = home + "/bin/" + LICENSE_FILE_NAME;
            }
            attempts.add(path);
            try {
                is = new FileInputStream(path);
            } catch (FileNotFoundException e) {
                if (home.endsWith("/") || home.endsWith("\\")) {
                    path = home + LICENSE_FILE_NAME;
                } else {
                    path = home + "/" + LICENSE_FILE_NAME;
                }
                attempts.add(path);
                try {
                    is = new FileInputStream(path);
                } catch (FileNotFoundException e2) {
                    //
                }
            }
        }
        return is;
    }

    private InputStream lookInCodeLocation(InputStream is, List<String> attempts) {
        try {
            CodeSource codeSource = getClass().getProtectionDomain().getCodeSource();
            if (codeSource != null) {
                URL sourceLoc = codeSource.getLocation();
                if (sourceLoc != null) {
                    URL licenseLoc = new URL(sourceLoc, LICENSE_FILE_NAME);
                    attempts.add(licenseLoc.toString());
                    is = licenseLoc.openStream();
                }
            }
        } catch (Exception e) {
            // try elsewhere
        }
        return is;
    }

    /**
     * Read the primary license file directly from a specified file
     *
     * @param filename the name or URI of the license file
     */

    public synchronized void loadLicense(String filename) {
        InputStream is;
        boolean exists = false;
        try {
            if (filename.startsWith("file:")) {
                is = new URL(filename).openStream();
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

    /**
     * Register a license held locally within a compiled stylesheet or schema
     * @param dmk the binary representation of the local license
     * @return an integer key which can be used to identify this license among all the registered
     * local licenses
     */

    public synchronized int registerSecondaryLicense(String dmk) {
        try {
            Base64BinaryValue b64 = new Base64BinaryValue(dmk);
            byte[] binary = b64.getBinaryValue();
            ByteArrayInputStream is = new ByteArrayInputStream(binary);
            BufferedReader reader = new BufferedReader(new InputStreamReader(is, "utf8"));
            License license = configure(reader, true);
            int id = dmk.hashCode() & 0x3ffff;
            while (secondaryLicenses.get(id) != null) {
                id++;
            }
            secondaryLicenses.put(id, license);
            return id;
        } catch (XPathException err) {
            throw new LicenseException(err.getMessage(), LicenseException.INVALID);
        } catch (IOException err) {
            throw new LicenseException(err.getMessage(), LicenseException.INVALID);
        }
    }

    public void disableLicensing() {
        primaryLicense = null;
        status = UNAVAILABLE;
    }

    public synchronized boolean isFeatureAllowedBySecondaryLicense(int license, int feature) {
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

    private License readLicenseFile(InputStream is) {
        BufferedReader reader;
        try {
            reader = new BufferedReader(new InputStreamReader(is, "UTF-8"));
        } catch (UnsupportedEncodingException e) {
            throw new LicenseException("License file uses UTF-8, which is an unsupported encoding",
                    LicenseException.INVALID);
        } 
        return configure(reader, false);
    }

    /**
     * This method allows the contents of a license key to be supplied programmatically.
     * This must be a license file as issued to authorized OEM distributors of Saxon-EE.
     *
     * @param reader A reader that will be used to read the contents of the license file
     *               (for example, a StringReader)
     * @throws IllegalArgumentException if the key supplied is not a valid OEM license key
     */

    public synchronized void supplyLicenseKey(BufferedReader reader) {
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


    private static License configure(BufferedReader reader, boolean embedded) {
        License license;
        try {
            String line;
            String name;
            String value;
            StringBuilder buffer = new StringBuilder(80);
            Properties features = new Properties();
            license = new License(features);
            String signature = "";
            while ((line = reader.readLine()) != null) {
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
            boolean valid = verify(buffer.toString(), convertHexToBinary(signature), key);
            String licenseDesc = embedded ? "license embedded in the stylesheet export file" : "license file";
            if (!valid) {
                throw new LicenseException("Invalid " + licenseDesc + " found", LicenseException.INVALID);
            }
            String serial = license.getFeature("Serial");
            licenseDesc += " (serial number " + serial + ")";
            if (license.daysLeft() < 0) {

                license.clearFeatures();
                throw new LicenseException("The " + licenseDesc + " has expired",
                        LicenseException.EXPIRED);
            }
            DateValue licenseIssued = new DateValue(features.getProperty("Issued").substring(0, 10) + 'Z');
            // some licenses were wrongly issued with YYYY-MM-DDTHH:MM:SS format
            DateValue majorVersionIssued = new DateValue(Version.getMajorReleaseDate() + 'Z');
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
            DateValue minorVersionIssued = new DateValue(Version.getReleaseDate() + 'Z');
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
        } catch (GeneralSecurityException e) {
            throw new LicenseException(e.getMessage(), LicenseException.INVALID);
        } catch (XPathException e) {
            throw new LicenseException("Invalid date in license file", LicenseException.INVALID);
        }
        return license;
    }



    /**
     * Get the serial number of the license
     *
     * @return the serial number
     */

    public String getSerialNumber() {
        return primaryLicense.getFeature("Serial");
    }

    /**
     * Convert a Hex String into the corresponding byte array by encoding
     * each two hexadecimal digits as a byte.
     *
     * @param hex Hexadecimal digits representation
     * @return byte Byte array
     */
    private static byte[] convertHexToBinary(String hex) {
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
     * @param signature        the digital sugnature
     * @param encodedPublicKey the public key.
     * @return a boolean whether or the license is valid.
     * @throws java.io.UnsupportedEncodingException
     *          if the encoding of the license file is not recognized
     * @throws java.security.GeneralSecurityException
     *          if a failure occurs in the verification process
     */
    private static boolean verify(String data, byte[] signature, byte[] encodedPublicKey)
            throws GeneralSecurityException, UnsupportedEncodingException {
        Signature sig = Signature.getInstance("DSA");
        PublicKey key = KeyFactory.getInstance("DSA").generatePublic(new X509EncodedKeySpec(encodedPublicKey));
        sig.initVerify(key);
        sig.update(data.getBytes(StandardCharsets.UTF_8));
        return sig.verify(signature);
    }

    /**
     * Test program to verify that a license contained in a specific file is valid
     *
     * @param args Command line arguments. First argument is the filename of the license
     *             file to be tested.
     * @throws Exception if any failure occurs
     */

    public static void main(String[] args) throws Exception {
        File licenseFile = new File(args[0]);
        Verifier v = new Verifier();
        License license = v.readLicenseFile(new FileInputStream(licenseFile));
        System.err.println("License OK");
        for (Enumeration en = license.features.propertyNames(); en.hasMoreElements(); ) {
            String s = (String) en.nextElement();
            System.err.println("Feature " + s + " = " + license.features.getProperty(s));
        }
    }

    private static class License {

        private Properties features;

        private License(Properties features) {
            this.features = features;
        }

        /**
         * It returns how many days are left for the license.
         * The value 0 is valid, it may indicate a never expire license.
         *
         * @return how many days are left for the license.
         */

        protected int daysLeft() {
//        if (features == null) {
//            loadLicense(false);
//        }
            String expiration = features.getProperty("Expiration");
            if (expiration == null) {
                return -1;
            } else if (expiration.trim().isEmpty() || expiration.contains("never")) {
                return 0;
            }
            try {
                String evaluating = features.getProperty("Evaluation");
                DateValue expiryDate = new DateValue(expiration);
                long expiryTimeMillis = expiryDate.getCalendar().getTime().getTime();
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

        private void clearFeatures() {
            if (features != null) {
                features.clear();
            }
        }

    }


}

// Copyright (C) 2004-2015 Saxonica Limited.
// Reproduction, distribution, or modification of this code is prohibited.
