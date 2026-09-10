package com.saxonica.config;

// Concrete subclass of Verifier for the Java platform

import net.sf.saxon.Configuration;
import net.sf.saxon.Version;
import net.sf.saxon.trans.LicenseException;

import java.io.FileInputStream;
import java.io.FileNotFoundException;
import java.io.IOException;
import java.io.InputStream;
import java.net.URL;
import java.nio.charset.StandardCharsets;
import java.security.*;
import java.security.spec.X509EncodedKeySpec;
import java.util.ArrayList;
import java.util.List;

public class JavaVerifier extends Verifier {

    // Public key created by the KeyManager for Saxon. This key is in X509 format, that is, in DER-encoded ASN.1

    private static final String PUBLIC_KEY = "308201B73082012C06072A8648CE3804013082011F02818100FD7F53811D75122952DF4A9" +
            "C2EECE4E7F611B7523CEF4400C31E3F80B6512669455D402251FB593D8D58FABFC5F5BA30F6CB9B556CD7813B801D346FF26660B76B9950" +
            "A5A49F9FE8047B1022C24FBBA9D7FEB7C61BF83B57E7C6A8A6150F04FB83F6D3C51EC3023554135A169132F675F3AE2B61D72AEFF222031" +
            "99DD14801C70215009760508F15230BCCB292B982A2EB840BF0581CF502818100F7E1A085D69B3DDECBBCAB5C36B857B97994AFBBFA3AEA" +
            "82F9574C0B3D0782675159578EBAD4594FE67107108180B449167123E84C281613B7CF09328CC8A6E13C167A8B547C8D28E0A3AE1E2BB3A" +
            "675916EA37F0BFA213562F1FB627A01243BCCA4F1BEA8519089A883DFE15AE59F06928B665E807B552564014C3BFECF492A038184000281" +
            "806CA067E469754A2F98D08965D35AB41252974F69FCD260152CFB58F94D8B956D6A1A91CC213A11107301DD37C85B383DDA409EC067D2A" +
            "8C4BE6651020A69AFD533630172D6F96F928667D7705D46E56D364E8E967002A7D864BACB02225F52B271BA1F6522D98FF8299C6273B24B" +
            "C41202C8857897E46B40FD7624A9CB0CC9E8";


    private static final byte[] ENCODED_PUBLIC_KEY = convertHexToBinary(PUBLIC_KEY);

    /**
     * Locate the license file, read it, and verify it
     *
     * @param config the Saxon configuration
     * @throws LicenseException if, for example, a license is found and is invalid
     */

    protected void loadLicense(Configuration config) {
        InputStream is = null;
        try {
            List<String> attempts = new ArrayList<>(4);
            is = lookInCodeLocation(attempts);
            if (is == null) {
                is = lookInInstallationDirectory(config, attempts);
            }
            if (is == null) {
                is = lookInClassPath();
            }
            if (is == null) {
                attempts.add("classpath");
                StringBuilder message = new StringBuilder("License file " + LICENSE_FILE_NAME + " not found. Tried in ");
                for (int i = 0; i < attempts.size(); i++) {
                    message.append(attempts.get(i));
                    if (i < attempts.size() - 2) {
                        message.append(", ");
                    } else if (i == attempts.size() - 2) {
                        message.append(", and ");
                    }
                }
                throw new LicenseException(message.toString(), LicenseException.NOT_FOUND);
            }
            License lic = readLicenseFile(is);
            setPrimaryLicense(lic);
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

    /**
     * This method validates license data based on the signature and the public key.
     *
     * @param data             the license data to be verified
     * @param signature        the digital signature, in X509 format (ASN.1 DER encoding)
     * @return a boolean indicating whether or not the license is valid.
     * @throws LicenseException if a failure occurs in the verification process
     */
    protected boolean verify(String data, String signature)
            throws LicenseException {
        try {
            // Create an object to hold the signature
            Signature sig = Signature.getInstance("DSA");
            // Create an object to hold the public key, and populate it with the known key
            PublicKey key = KeyFactory.getInstance("DSA").generatePublic(new X509EncodedKeySpec(ENCODED_PUBLIC_KEY));
            // Supply the public key to the Signature object
            sig.initVerify(key);
            // Supply the data to be verified to the Signature object
            sig.update(data.getBytes(StandardCharsets.UTF_8));
            // Verify the data against the signature
            return sig.verify(convertHexToBinary(signature));
        } catch (GeneralSecurityException e) {
            throw new LicenseException("Failed to verify license signature: " + e.getMessage(), LicenseException.INVALID);
        }
    }


    private static InputStream lookInClassPath() {
        ClassLoader cl = Thread.currentThread().getContextClassLoader();
        if (cl == null) {
            cl = ClassLoader.getSystemClassLoader();
        }
        return cl.getResourceAsStream(LICENSE_FILE_NAME);
    }

    private static InputStream lookInInstallationDirectory(Configuration config, List<String> attempts) {
        String home = Version.platform.getInstallationDirectory(Version.softwareEdition, config);
        String path;
        InputStream is = null;
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

    private InputStream lookInCodeLocation(List<String> attempts) {
        try {
            CodeSource codeSource = getClass().getProtectionDomain().getCodeSource();
            if (codeSource != null) {
                URL sourceLoc = codeSource.getLocation();
                if (sourceLoc != null) {
                    URL licenseLoc = new URL(sourceLoc, LICENSE_FILE_NAME);
                    attempts.add(licenseLoc.toString());
                    return licenseLoc.openStream();
                }
            }
        } catch (Exception e) {
            // try elsewhere
        }
        return null;
    }
    
}

// Copyright (c) 2010-2023 Saxonica Limited

