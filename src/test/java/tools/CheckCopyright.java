////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) 2018-2022 Saxonica Limited
// This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
// If a copy of the MPL was not distributed with this file, You can obtain one at http://mozilla.org/MPL/2.0/.
// This Source Code Form is “Incompatible With Secondary Licenses”, as defined by the Mozilla Public License, v. 2.0.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

package tools;


import java.io.File;
import java.io.FileReader;
import java.io.LineNumberReader;

// Check copyright statements in Java code modules

public class CheckCopyright {

    private final int totalLines = 0;
    private final int totalNonCommentLines = 0;
    private int totalFiles = 0;
    private static String suffix = ".java";
    private static String dir = "/Users/mike/GitHub/saxon2020/src/main/java";
    private static String currentYear = "2022";

    public static void main(String[] args) throws Exception {
        if (args.length >= 1) {
            dir = args[0];
        }
        if (args.length >= 2) {
            suffix = args[1];
        }
        new CheckCopyright().processDir(new File(dir), true);

    }

    public void processDir(File directory, boolean print) throws Exception {
        String[] files = directory.list();
        //System.out.println("<dir name='" + directory + "'>");
        for (String file : files) {
            processFile(directory, file);
        }
//        System.out.println("</dir>");
//        if (print) {
//            System.err.println("Total number of files: " + totalFiles);
//            System.err.println("Total number of lines: " + totalLines);
//            System.err.println("Total number of non-comment lines: " + totalNonCommentLines);
//        }
    }

    public String fileName(File file) {
        String path = file.getAbsolutePath();
        int i = path.indexOf("src/main/java");
        return path.substring(i+13);
    }

    public void processFile(File directory, String filename) throws Exception {
        int lines = 0;
        int nonCommentLines = 0;

        File file = new File(directory, filename);
        if (file.isDirectory()) {
            processDir(file, false);
        } else if (filename.endsWith(suffix) && file.canRead()) {
            //System.err.println("Processing " + filename);
            totalFiles++;
            LineNumberReader r = new LineNumberReader(new FileReader(file));
            boolean isSaxonicaPackage = false;
            boolean containsSaxonicaCopyright = false;
            boolean containsMozillaLicense = false;
            boolean containsCurrentYear = false;
            String line = r.readLine();
            while (line!=null) {
                if (line.contains("package com.saxonica") &&
                        !(line.contains("package com.saxonica.xqj") && !line.contains("package com.saxonica.xqj.pull")) &&
                        !line.contains("com.saxonica.testdriver")) {
                    isSaxonicaPackage = true;
                }
                if (line.contains("Copyright") && line.contains("Saxonica")) {
                    containsSaxonicaCopyright = true;
                    containsCurrentYear = line.contains(currentYear);
                }
                if (line.contains("This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.")) {
                    containsMozillaLicense = true;
                }
                lines++;
                line = line.trim();
                if (!line.isEmpty() && !(line.charAt(0)=='*' || line.charAt(0)=='/')) {
                    nonCommentLines++;
                }
                line = r.readLine();
            }
            if (isSaxonicaPackage) {
                if (containsMozillaLicense) {
                    System.err.println("File " + fileName(file) + " contains Mozilla license");
                }
                if (!containsSaxonicaCopyright) {
                    System.err.println("File " + fileName(file) + " contains no Saxonica copyright");
                } else if (!containsCurrentYear) {
                    System.err.println("File " + fileName(file) + " contains Saxonica copyright without current year");
                }
            } else {
                if (!containsMozillaLicense) {
                    System.err.println("File " + fileName(file) + " contains no Mozilla license");
                }
            }
//            System.out.println("<file name='" + file.getName() + "' lines='" + lines + "' non-comment-lines='" + nonCommentLines + "'/>");
//            totalLines += lines;
//            totalNonCommentLines += nonCommentLines;
        }
    }
}

