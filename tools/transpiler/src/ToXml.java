import com.github.javaparser.ParseProblemException;
import com.github.javaparser.StaticJavaParser;
import com.github.javaparser.ast.CompilationUnit;
import com.github.javaparser.symbolsolver.JavaSymbolSolver;
import com.github.javaparser.symbolsolver.resolution.typesolvers.CombinedTypeSolver;
import com.github.javaparser.symbolsolver.resolution.typesolvers.JarTypeSolver;
import com.github.javaparser.symbolsolver.resolution.typesolvers.ReflectionTypeSolver;
import net.sf.saxon.s9api.*;

import javax.xml.transform.stream.StreamSource;
import java.io.*;
import java.util.HashMap;
import java.util.Map;

public class ToXml {

    private final static String suffix = "java";
    private static XsltExecutable xmlToCSharp;

    private boolean DEBUG = false;

    /**
     * Main program with three arguments. The program processes an input directory of Java source
     * code, recursively, and outputs a corresponding directory containing XML files with the abstract
     * syntax tree of the input. It then converts these XML files back to Java to check that everything
     * is working.
     *
     * @param args 0:input directory 1:output XML directory 2:output Java/C# directory
     * @throws Exception on failure
     */

    public static void main(String[] args) throws Exception {
        new ToXml().go(args);
    }

    private File singleInputFile = null;
    private File inputDir = null;
    private File xmlDir = null;
    private File digest = null;
    private File csDir = null;

    private net.sf.saxon.s9api.Processor processor = new Processor(false);
    private XsltCompiler compiler = processor.newXsltCompiler();

    private void go(String[] args) throws Exception {
        for (String arg : args) {
            if (arg.startsWith("-f:")) {
                singleInputFile = new File(arg.substring(3));
                if (singleInputFile.isDirectory()) {
                    throw new IllegalArgumentException(singleInputFile + " is a directory");
                }
            } else if (arg.startsWith("-j:")) {
                inputDir = new File(arg.substring(3));
                if (!inputDir.isDirectory()) {
                    throw new IllegalArgumentException(inputDir + " is not a directory");
                }
            } else if (arg.startsWith("-xml:")) {
                xmlDir = new File(arg.substring(5));
                if (!xmlDir.isDirectory()) {
                    throw new IllegalArgumentException(xmlDir + " is not a directory");
                }
            } else if (arg.startsWith("-digest:")) {
                digest = new File(arg.substring(8));
                if (digest.isDirectory()) {
                    throw new IllegalArgumentException(digest + " is a directory");
                }
            } else if (arg.startsWith("-cs:")) {
                csDir = new File(arg.substring(4));
                if (!csDir.isDirectory()) {
                    throw new IllegalArgumentException(xmlDir + " is not a directory");
                }
            } else if (arg.equals("-debug")) {
                DEBUG = true;
                compiler.setCompileWithTracing(true);
                compiler.setAssertionsEnabled(true);
            }
        }


        if (inputDir == null && singleInputFile == null && xmlDir == null) {
            throw new IllegalArgumentException("No input supplied (-f, -xml, -j)");
        }

        if (singleInputFile != null) {
//            if (digest == null) {
//                throw new IllegalArgumentException("No -digest supplied");
//            }
            processSingleFile(singleInputFile, digest);
        } else {
            if (inputDir != null) {
                if (xmlDir == null) {
                    throw new IllegalArgumentException("No -xml directory supplied");
                }
                processJavaDirToXml(inputDir, xmlDir);
            }
            if (xmlDir != null && digest != null && csDir == null) {
                processXmlToDigest(xmlDir, digest);
            }
            if (csDir != null) {
                if (digest == null) {
                    throw new IllegalArgumentException("No -digest supplied");
                }
                processXmlDirToCSharp(xmlDir, digest, csDir);
            }
        }
    }

    public XdmNode parseDigest(File digestFile) throws SaxonApiException {
        DocumentBuilder builder = processor.newDocumentBuilder();
        builder.setWhitespaceStrippingPolicy(WhitespaceStrippingPolicy.ALL);
        return builder.build(digestFile);
    }

    public void processSingleFile(File filename, File digest) throws Exception {

        xmlToCSharp = compiler.compile(new StreamSource(new File("/Users/mike/GitHub/saxon2020/tools/jparser/xslt/xml-to-cs.xsl")));

//        // Generate XML representation
//        CombinedTypeSolver combinedTypeSolver = new CombinedTypeSolver();
////        combinedTypeSolver.add(new ReflectionTypeSolver(false));
//        combinedTypeSolver.add(new ReflectionTypeSolver() {
//            // TEMP workaround for Javaparser bug #3080
//            // Note: using new ReflectionTypeSolver(false) includes the Saxon JAR file in the reflection search,
//            // which is a problem if it's not the latest Saxon.
//            protected boolean filterName(String name) {
//                return super.filterName(name) || name.startsWith("org.");
//            }
//        });
//        // This tells the JavaParser to use the Saxon 10.3 JAR file to resolve references to classes and methods
//        combinedTypeSolver.add(new JarTypeSolver("/Users/mike/GitHub/saxon2020/build/libs/saxon-he-11.0.jar"));
//        JavaSymbolSolver symbolSolver = new JavaSymbolSolver(combinedTypeSolver);
//        StaticJavaParser.getConfiguration().setSymbolResolver(symbolSolver);
//        StaticJavaParser.getConfiguration().setAttributeComments(false);

        CompilationUnit cu = StaticJavaParser.parse(filename);
        System.err.println("Processing " + filename);

        XmlPrinterMHK printer = new XmlPrinterMHK(symbolSolver, true);
        //System.err.println(printer.output(cu));
        File xmlOut = new File(xmlDir, filename.toString().replace(".java", ".xml"));
        System.err.println("Writing to " + xmlOut);
        PrintStream p = new PrintStream(xmlOut);
        p.println(printer.output(cu));
        //xmlDir.mkdirs();
//        PrintWriter writer = new PrintWriter(new FileWriter(xmlOut));
//        writer.write(printer.output(cu));
//        writer.close();

//        File xmlOut = new File(xmlDir, filename.replace(".java", ".xml"));
//        xmlDir.mkdirs();
//        PrintWriter writer = new PrintWriter(
//                new FileWriter(xmlOut));
//        writer.write(printer.output(cu));
//        writer.close();

//        Xslt30Transformer trans = xmlToCSharp.load30();
//        if (DEBUG) {
//            trans.setTraceListener(new XSLTTraceListener());
//        }
//        Map<QName, XdmValue> params = new HashMap<>();
//        params.put(new QName("digest"), parseDigest(digest));
//        trans.setStylesheetParameters(params);
//        trans.applyTemplates(new StreamSource(new StringReader(xmlOut.toString())), xmlToCSharp.getProcessor().newSerializer(System.out));
    }

    public void processJavaDirToXml(File inputDir, File xmlDir) throws Exception {
        String[] files = inputDir.list();
        if (files != null) {
            for (String file : files) {
                processJavaFileToXml(inputDir, file, xmlDir);
            }
        }
    }

    public void processJavaFileToXml(File directory, String filename, File xmlDir) throws Exception {
        File file = new File(directory, filename);
        if (file.isDirectory()) {
            processJavaDirToXml(file, new File(xmlDir, filename));
        } else if (filename.endsWith(".java") && file.canRead()) {
            try {

                System.err.println("Processing " + filename);
                CompilationUnit cu;
                try {
                    cu = StaticJavaParser.parse(file);
                } catch (ParseProblemException pe) {
                    System.err.println("Failed to parse " + filename +  " - " + pe.getMessage());
                    pe.printStackTrace();
                    return;
                }
                // Generate XML representation

                XmlPrinterMHK printer = new XmlPrinterMHK(symbolSolver, true);
                File xmlOut = new File(xmlDir, filename.replace(".java", ".xml"));
                xmlDir.mkdirs();
                PrintWriter writer = new PrintWriter(new FileWriter(xmlOut));
                writer.write(printer.output(cu));
                writer.close();
//
////                // Now convert back to Java
////                File javaOut = new File(outDir, filename);
////                Xslt30Transformer trans = xmlToJava.load30();
////                trans.applyTemplates(new StreamSource(xmlOut), xmlToJava.getProcessor().newSerializer(javaOut));
//                // Now convert to C#
//                File csOut = new File(outDir, filename.replace(".java", ".cs"));
//                Xslt30Transformer trans = xmlToCSharp.load30();
//                trans.applyTemplates(new StreamSource(xmlOut), xmlToCSharp.getProcessor().newSerializer(csOut));
            } catch (IOException e) {
                System.err.println(e.getMessage());
            }
        }
    }

    private static JavaSymbolSolver symbolSolver = getJavaSymbolSolver();
    private static JavaSymbolSolver getJavaSymbolSolver() {
        try {
            CombinedTypeSolver combinedTypeSolver = new CombinedTypeSolver();
            combinedTypeSolver.add(new ReflectionTypeSolver() {
                // TEMP workaround for Javaparser bug #3080
                // Note: using new ReflectionTypeSolver(false) includes the Saxon JAR file in the reflection search,
                // which is a problem if it's not the latest Saxon.
                protected boolean filterName(String name) {
                    return super.filterName(name) || name.startsWith("org.");
                }
            });
            // This tells the JavaParser to use the Saxon 10.3 JAR file to resolve references to classes and methods
            combinedTypeSolver.add(new JarTypeSolver("/Users/mike/GitHub/saxon2020/build/libs/saxon-he-11.0.jar"));
            JavaSymbolSolver symbolSolver = new JavaSymbolSolver(combinedTypeSolver);
            StaticJavaParser.getConfiguration().setSymbolResolver(symbolSolver);
            StaticJavaParser.getConfiguration().setAttributeComments(false);
            return symbolSolver;
        } catch (IOException e) {
            throw new AssertionError(e);
        }
    }

    public void processXmlToDigest(File xmlDir, File digest) throws Exception {
        XsltExecutable digester = compiler.compile(new StreamSource(new File("/Users/mike/GitHub/saxon2020/tools/jparser/xslt/xml-to-digest.xsl")));
        Xslt30Transformer trans = digester.load30();
        Map<QName, XdmValue> params = new HashMap<>();
        params.put(new QName("xmlDir"), new XdmAtomicValue(xmlDir.toURI()));
        trans.setStylesheetParameters(params);
        trans.callTemplate(null, processor.newSerializer(digest));
    }

    public void processXmlDirToCSharp(File xmlDir, File digestFile, File csDir) throws Exception {
        XdmNode digest = parseDigest(digestFile);
        xmlToCSharp = compiler.compile(new StreamSource(new File("/Users/mike/GitHub/saxon2020/tools/jparser/xslt/xml-to-cs.xsl")));
        processXmlFileToCSharp(xmlDir, csDir, digest, xmlToCSharp);
    }

    public void processXmlFileToCSharp(File xmlDir, File csDir, XdmNode digest, XsltExecutable xmlToCSharp) throws Exception {
        String[] files = xmlDir.list();
        if (files != null) {
            for (String file : files) {
                processXmlFileToCSharp(xmlDir, file, csDir, digest, xmlToCSharp);
            }
        }
    }

    public void processXmlFileToCSharp(File directory, String filename, File csDir, XdmNode digest, XsltExecutable xmlToCSharp) throws Exception {
        File file = new File(directory, filename);
        if (file.isDirectory()) {
            System.err.println("Processing directory " + file);
            processXmlFileToCSharp(file, new File(csDir, filename), digest, xmlToCSharp);
        } else if (filename.endsWith(".xml") && file.canRead()) {
            try {
                System.err.println("Processing file " + file);
                File csOut = new File(csDir, filename.replace(".xml", ".cs"));
                csDir.mkdirs();
                Xslt30Transformer trans = xmlToCSharp.load30();
                Map<QName, XdmValue> params = new HashMap<>();
                params.put(new QName("digest"), digest);
                trans.setStylesheetParameters(params);
                trans.setAssertionsEnabled(true);
                trans.transform(new StreamSource(file), processor.newSerializer(csOut));
////                trans.applyTemplates(new StreamSource(xmlOut), xmlToJava.getProcessor().newSerializer(javaOut));
//                // Now convert to C#
//                File csOut = new File(outDir, filename.replace(".java", ".cs"));
//                Xslt30Transformer trans = xmlToCSharp.load30();
//                trans.applyTemplates(new StreamSource(xmlOut), xmlToCSharp.getProcessor().newSerializer(csOut));
            } catch (Exception e) {
                System.err.println(e.getMessage());
                throw e;
            }
        }
    }

}
