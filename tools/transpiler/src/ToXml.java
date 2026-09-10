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
import java.util.*;

public class ToXml {

    private final static String suffix = "java";
    private static XsltExecutable xmlToCSharp;

    private boolean DEBUG = false;

    /**
     * List of classes that are renamed (by prefixing with "JJ_") to make them less visible as public classes,
     * because they otherwise have the same local name as a class in the public API. The list needs to be
     * kept aligned with the list in functions.xsl
     */

    private static List<String> internalClasses = new ArrayList<>();
    static {
        internalClasses.add("ResourceResolver.cs");
        internalClasses.add("ResourceRequest.cs");
    }

    /**
     * Main program with three arguments.
     *
     * With the arguments -j:dir and -xml:dir, the program processes an input directory of Java source
     * code, recursively, and outputs a corresponding directory containing XML files with the abstract
     * syntax tree of the input. The -h:dir argument identifies a directory containing an older version
     * of the Java input, allowing the conversion to be skipped if the input has not changed.
     *
     * With the arguments -xml:dir and -cs:dir, the program processes the directory of XML files, and
     * converts each file to C#. The -digest argument identifies a digest file containing details of the
     * classes and methods, primarily so that C# "virtual" and "override" modifiers can be attached to
     * methods where required.
     * @throws Exception on failure
     */

    public static void main(String[] args) throws Exception {
        System.err.println("Transpiler ToXML v2022-02-11");
        new ToXml().go(args);
    }

    
    private static File repoDir = null;
    private static String repoDirName = null;
    private static String jarVersion = null;

    private static String edition = null;
    private static String icu4jJar = null; // The location of the icu4j jar to read
    private static String vavrJar = null; // The location of the VAVR jar to read
    private static String vavrMatchJar = null;
    private File singleInputFile = null;
    private File historicInputDir = null; // directory holding the Java prior to preprocessing
    private File inputDir = null;
    private File srcDir = null;
    private File xmlDir = null;
    private File digest = null;
    private File csDir = null;
    private boolean forceRebuild = false;

    private net.sf.saxon.s9api.Processor processor = new Processor(false);
    private XsltCompiler compiler = processor.newXsltCompiler();

    private void go(String[] args) throws Exception {
        for (String arg : args) {
            if (arg.startsWith("-repo:")) {
                repoDirName = arg.substring(6);
                System.err.println("Repository name: " + repoDirName);
                if (!repoDirName.endsWith("/")) {
                    repoDirName = repoDirName + "/";
                }
                repoDir = new File(repoDirName);
                if (!repoDir.isDirectory()) {
                    throw new IllegalArgumentException(singleInputFile + " is not a directory");
                }
                if (!new File(repoDir, "src").exists()) {
                    throw new IllegalArgumentException(singleInputFile + " is not the Saxon repository");
                }
            } else if (arg.startsWith("-f:")) {
                singleInputFile = new File(arg.substring(3));
                if (singleInputFile.isDirectory()) {
                    throw new IllegalArgumentException(singleInputFile + " is a directory");
                }
            } else if (arg.startsWith("-j:")) {
                inputDir = new File(arg.substring(3));
                if (!inputDir.isDirectory()) {
                    throw new IllegalArgumentException(inputDir + " is not a directory");
                }
            } else if (arg.startsWith("-h:")) {
                historicInputDir = new File(arg.substring(3));
                if (!historicInputDir.isDirectory()) {
                    throw new IllegalArgumentException(historicInputDir + " is not a directory");
                }
            } else if (arg.startsWith("-src:")) {
                srcDir = new File(arg.substring(5));
                if (!srcDir.isDirectory()) {
                    throw new IllegalArgumentException(srcDir + " is not a directory");
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
            } else if (arg.startsWith("-ee:")) {
                edition = "ee";
                jarVersion = arg.substring(4);
            } else if (arg.startsWith("-he:")) {
                edition = "he";
                jarVersion = arg.substring(4);
            } else if (arg.startsWith("-icu4j:")) {
                icu4jJar = arg.substring(7);
            } else if (arg.startsWith("-vavr:")) {
                vavrJar = arg.substring(6);
                System.err.println("vavrJar set to " + vavrJar);
            } else if (arg.startsWith("-vavrMatch:")) {
                vavrMatchJar = arg.substring(11);
                System.err.println("vavrMatchJar set to " + vavrMatchJar);
            } else if (arg.equals("-debug")) {
                DEBUG = true;
                compiler.setCompileWithTracing(true);
                compiler.setAssertionsEnabled(true);
            } else if (arg.equals("-force")) {
                forceRebuild = true;
            } else {
                throw new IllegalArgumentException("Unknown option " + arg);
            }
        }

        if (jarVersion == null) {
            throw new IllegalArgumentException("You must specify the EE jar version: -ee:11.2 for example");
        }


        if (inputDir == null && singleInputFile == null && xmlDir == null) {
            throw new IllegalArgumentException("No input supplied (-f, -xml, -j)");
        }

        JavaSymbolSolver symbolSolver = getJavaSymbolSolver();

        if (singleInputFile != null) {
//            if (digest == null) {
//                throw new IllegalArgumentException("No -digest supplied");
//            }
            processSingleFile(singleInputFile, digest, symbolSolver);
        } else {
            if (inputDir != null) {
                if (xmlDir == null) {
                    throw new IllegalArgumentException("No -xml directory supplied");
                }
                processJavaDirToXml(inputDir, xmlDir, srcDir, symbolSolver);
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

    /**
     * From a file containing preprocessed Java, get the file containing the original Java
     * @param suppliedFile - contains the preprocessed Java
     * @return originalFile - contains the original Java
     */

    public File getOriginalJavaFile(File suppliedFile) {
        String path = suppliedFile.getAbsolutePath().substring(inputDir.getAbsolutePath().length());
        String previousPath = historicInputDir + path;
        return new File(previousPath);
    }

    public XdmNode parseDigest(File digestFile) throws SaxonApiException {
        DocumentBuilder builder = processor.newDocumentBuilder();
        builder.setWhitespaceStrippingPolicy(WhitespaceStrippingPolicy.ALL);
        return builder.build(digestFile);
    }

    public void processSingleFile(File filename, File digest, JavaSymbolSolver symbolSolver) throws Exception {

        xmlToCSharp = compiler.compile(new StreamSource(new File("tools/transpiler/xslt/xml-to-cs.xsl")));

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
        /*List<NameExpr>  nes = cu.findAll(NameExpr .class);
        nes.forEach(ne -> {
            System.out.println(( ne.calculateResolvedType().describe()));
        }); */

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

    public void processJavaDirToXml(File inputDir, File xmlDir, File srcDir, JavaSymbolSolver symbolSolver) throws Exception {
        String[] files = inputDir.list();
        if (files != null) {
            for (String file : files) {
                processJavaFileToXml(inputDir, file, xmlDir, srcDir, symbolSolver);
            }
        }
    }

    public void processJavaFileToXml(File directory, String filename, File xmlDir, File srcDir, JavaSymbolSolver symbolSolver) throws Exception {
        File file = new File(directory, filename);
        if (file.isDirectory()) {
            processJavaDirToXml(file, new File(xmlDir, filename), srcDir, symbolSolver);
        } else if (filename.endsWith(".java") && file.canRead()) {
            try {
                xmlDir.mkdirs();
                boolean skip = false;
                File xmlOut = new File(xmlDir, filename.replace(".java", ".xml"));
                if (xmlOut.exists() && historicInputDir != null && !forceRebuild) {
                    File historicSrcFile = getOriginalJavaFile(file);
                    if (historicSrcFile.exists()) {
                        skip = xmlOut.lastModified() > historicSrcFile.lastModified();
                        if (file.lastModified() < historicSrcFile.lastModified()) {
                            throw new IllegalStateException("Failed to run preprocessor on " + file);
                        }
                    }
                }
                if (skip) {
                    //System.err.println("Unchanged " + filename);
                } else {
                    System.err.println("Processing " + filename);
                    CompilationUnit cu;
                    try {
                        cu = StaticJavaParser.parse(file);
                    } catch (ParseProblemException pe) {
                        System.err.println("Failed to parse " + filename + " - " + pe.getMessage());
                        pe.printStackTrace();
                        return;
                    }
                    // Generate XML representation

                    XmlPrinterMHK printer = new XmlPrinterMHK(symbolSolver, true);
                    PrintWriter writer = new PrintWriter(new FileWriter(xmlOut));
                    writer.write(printer.output(cu));
                    writer.close();
                }
            } catch (IOException e) {
                System.err.println(e.getMessage());
            }
        }
    }

    //private static JavaSymbolSolver symbolSolver = getJavaSymbolSolver();

    private static JavaSymbolSolver getJavaSymbolSolver() {
        try {
            CombinedTypeSolver combinedTypeSolver = new CombinedTypeSolver();
            combinedTypeSolver.add(new ReflectionTypeSolver() {
                // TEMP workaround for Javaparser bug #3080
                // Note: using new ReflectionTypeSolver(false) includes the Saxon JAR file in the reflection search,
                // which is a problem if it's not the latest Saxon.
                protected boolean filterName(String name) {
                    return super.filterName(name) || name.startsWith("org.") || name.startsWith("io.");
                }
            });

            // This tells the JavaParser to use the Saxon 10.3 JAR file to resolve references to classes and methods
            combinedTypeSolver.add(new JarTypeSolver(String.format("%s/build/libs/saxon-%s-%s.jar", repoDirName, edition, jarVersion)));
            combinedTypeSolver.add(new JarTypeSolver(icu4jJar));
            // The gradle script is currently supplying locations for the VAVR jar files in $repo/lib, as using
            // the versions in .gradle cache didn't work for some reason
            combinedTypeSolver.add(new JarTypeSolver(vavrJar));
            combinedTypeSolver.add(new JarTypeSolver(vavrMatchJar));
            JavaSymbolSolver symbolSolver = new JavaSymbolSolver(combinedTypeSolver);
            StaticJavaParser.getConfiguration().setSymbolResolver(symbolSolver);
            StaticJavaParser.getConfiguration().setAttributeComments(false);
            return symbolSolver;
        } catch (IOException e) {
            throw new AssertionError(e);
        }
    }

    public void processXmlToDigest(File xmlDir, File digest) throws Exception {
        XsltExecutable digester = compiler.compile(new StreamSource(new File(repoDir, "tools/transpiler/xslt/xml-to-digest.xsl")));
        Xslt30Transformer trans = digester.load30();
        Map<QName, XdmValue> params = new HashMap<>();
        params.put(new QName("xmlDir"), new XdmAtomicValue(xmlDir.toURI()));
        trans.setStylesheetParameters(params);
        trans.callTemplate(null, processor.newSerializer(digest));
    }

    
    public void processXmlDirToCSharp(File xmlDir, File digestFile, File csDir) throws Exception {
        XdmNode digest = parseDigest(digestFile);
        xmlToCSharp = compiler.compile(new StreamSource(new File(repoDir, "tools/transpiler/xslt/xml-to-cs.xsl")));
        if(edition.equals("ee")) {
            File xmlDirEE = new File(new File(xmlDir, "com"), "saxonica");
            File csDirEE = new File(new File(csDir, "Saxon"), "Eej");
            processXmlSubDirectoryToCSharp(xmlDirEE, csDirEE, digest, xmlToCSharp);
            File xmlDirHE = new File(new File(new File(xmlDir, "net"), "sf"), "saxon");
            File csDirHE = new File(new File(csDir, "Saxon"), "Hej");
            processXmlSubDirectoryToCSharp(xmlDirHE, csDirHE, digest, xmlToCSharp);
        } else {
            File xmlDirHE = new File(new File(new File(xmlDir, "net"), "sf"), "saxon");
            File csDirHE = new File(new File(csDir, "Saxon"), "Hej");
            processXmlSubDirectoryToCSharp(xmlDirHE, csDirHE, digest, xmlToCSharp);
        }
    }

    public void processXmlSubDirectoryToCSharp(File xmlDir, File csDir, XdmNode digest, XsltExecutable xmlToCSharp) throws Exception {
        System.err.println("Processing dir " + xmlDir + " to " + csDir);
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
            System.err.println("Processing subdirectory " + file);
            processXmlSubDirectoryToCSharp(file, new File(csDir, filename), digest, xmlToCSharp);
        } else if (filename.endsWith(".xml") && file.canRead()) {
            try {
                System.err.println("Processing file " + file);
                String csFileName = filename.replace(".xml", ".cs");
                if (internalClasses.contains(csFileName)) {
                    csFileName = "JJ_" + csFileName;
                }
                File csOut = new File(csDir, csFileName);
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
