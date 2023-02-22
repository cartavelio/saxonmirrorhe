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

// Count the number of lines of Java code in a directory and its subdirectories

public class CountLines {

    private int totalLines = 0;
    private int totalNonCommentLines = 0;
    private int totalFiles = 0;
    private static String suffix = ".java";

    public static void main(String[] args) throws Exception {
        if (args.length == 2) {
            suffix = args[1];
        }
        new CountLines().processDir(new File(args[0]), true);

    }

    public void processDir(File directory, boolean print) throws Exception {
        String[] files = directory.list();
        System.out.println("<dir name='" + directory + "'>");
        for (String file : files) {
            processFile(directory, file);
        }
        System.out.println("</dir>");
        if (print) {
            System.err.println("Total number of files: " + totalFiles);
            System.err.println("Total number of lines: " + totalLines);
            System.err.println("Total number of non-comment lines: " + totalNonCommentLines);
        }
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
            String line = r.readLine();
            while (line!=null) {
                lines++;
                line = line.trim();
                if (line.length()>0 && !(line.charAt(0)=='*' || line.charAt(0)=='/')) {
                    nonCommentLines++;
                }
                line = r.readLine();
            }
            System.out.println("<file name='" + file.getName() + "' lines='" + lines + "' non-comment-lines='" + nonCommentLines + "'/>");
            totalLines += lines;
            totalNonCommentLines += nonCommentLines;
        }
    }
}

