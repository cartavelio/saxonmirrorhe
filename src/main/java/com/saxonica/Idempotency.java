// Copyright (c) 2018-2023 Saxonica Limited

/* A program to check for idempotency in self-compilation of the XSLT-in-XSLT compiler running under Java.
*
* It starts by compiling & running the compiler in SaxonEE, consuming its own source to produce an initial export,
* then successively uses the exported compilers compiler.sef, compiler[12].sef to export the next in sequence.
* These are all generated in <repos>/src/xslt/test
*
* The acid test should be that compile2.sef and compiler3.sef are identical, save for
* a differing @buildTimeDate attribute on the top level package element. (Oxygen compare is good to check this.)
*
* NOTE - at present you will probably have to suppress import checksum testing (StylesheetModule#240)
*
* Different options are possible for debugging purposes
*
* The IntelliJ command to run this is:
*  Main class:         com.saxonica.Idempotency
*  working directory:  <repos>/src/xslt
* */

package com.saxonica;

import java.util.ArrayList;

public class Idempotency {
    private static final String compilerSource = "XSLT-SEF-compiler.xsl";
    private static final String[] common = new String[]{
            "-s:"+compilerSource,
            "-im:compile-complete",
            "-l:on",
            "-opt:0",
            /*"-TP:profile.html",*/
            "--generateByteCode:off",
            "-relocate:on"
           };

    private static final String completeOpts =    "?options=map{'traceProgress':false(),'timing':true(),'justStatic':false(),'justNormalize':false(),'noXPath':false(),'noBindings':false()}";
    private static final String partialOpts =    "?options=map{'traceProgress':false(),'timing':true(),'justStatic':false(),'justNormalize':false(),'noXPath':true(),'noBindings':false()}";

    private static void run(String xsl, String sef) throws Exception {
        run(xsl,sef,completeOpts);
    }
    private static void run(String xsl, String sef, String opts) throws Exception {
        ArrayList<String> p0 = new ArrayList<String>();

       System.err.println("Stylesheet: "+xsl);
        p0.add("-xsl:" + xsl);
        p0.add("-o:" + sef);
        for (String a : common) {
            p0.add(a);
        }
        p0.add(opts);
        net.sf.saxon.Transform.main(p0.toArray(new String[0]));
    }

    public static void main(String[] args)
            throws java.lang.Exception {
        //run("XSLT-SEF-compiler.xsl","test/compiler.sef");
        run(compilerSource,"test/compiler.sef");
        //run("test/compiler.sef","test/compiler2Static.sef",partialOpts);
        run("test/compiler.sef","test/compiler2.sef");
        run("test/compiler2.sef","test/compiler3.sef");
        run("test/compiler3.sef","test/compiler4.sef");



    }
}
