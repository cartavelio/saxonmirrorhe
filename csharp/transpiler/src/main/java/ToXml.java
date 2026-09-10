import com.github.javaparser.ParseProblemException;
import com.github.javaparser.JavaParser;
import com.github.javaparser.StaticJavaParser;
import com.github.javaparser.ParserConfiguration;
import com.github.javaparser.ast.CompilationUnit;
import com.github.javaparser.symbolsolver.JavaSymbolSolver;
import com.github.javaparser.symbolsolver.resolution.typesolvers.CombinedTypeSolver;
import com.github.javaparser.symbolsolver.resolution.typesolvers.JarTypeSolver;
import com.github.javaparser.symbolsolver.resolution.typesolvers.ReflectionTypeSolver;
import net.sf.saxon.s9api.*;
import net.sf.saxon.trace.XSLTTraceListener;
import org.xml.sax.InputSource;

import javax.xml.transform.sax.SAXSource;
import javax.xml.transform.stream.StreamSource;
import java.io.*;
import java.util.*;
import java.util.function.Consumer;

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
                    throw new IllegalArgumentException(repoDir + " is not a directory");
                }
                if (!new File(repoDir, "src").exists()) {
                    throw new IllegalArgumentException(repoDir + " is not the Saxon repository");
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

        StaticJavaParser.getConfiguration().setLanguageLevel(ParserConfiguration.LanguageLevel.JAVA_17);
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
                FilesToProcess toBeProcessed = new FilesToProcess();
                findJavaFiles(inputDir, xmlDir, toBeProcessed);
                processJavaFiles(toBeProcessed, symbolSolver);
                //processJavaDirToXml(inputDir, xmlDir, srcDir, symbolSolver);
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
        if (filename.getAbsolutePath().endsWith(".java")) {
            CompilationUnit cu = StaticJavaParser.parse(filename);
            System.err.println("Processing " + filename);

            // Hack; assume the filename contains build/tmp/java and we can work out the relative location from that
            String fn = filename.getAbsolutePath();
            int pos = fn.indexOf("build/tmp/java/");
            if (pos < 0) {
                throw new IllegalArgumentException("Cannot find build/tmp in filename: " + fn);
            }
            fn = fn.substring(pos+15).replace(".java", ".xml");

            XmlPrinterMHK printer = new XmlPrinterMHK(symbolSolver, true);
            File xmlOut = new File(xmlDir, fn);
            System.err.println("Writing to " + xmlOut);

            // Parse and re-serialize so that we can get indented output...
            String constructedXml = printer.output(cu);
            DocumentBuilder builder = processor.newDocumentBuilder();
            StringReader reader = new StringReader(constructedXml);
            SAXSource source =  new SAXSource(new InputSource(reader));
            XdmDestination destination = new XdmDestination();
            builder.parse(source, destination);
            Serializer serializer = processor.newSerializer(xmlOut);
            serializer.setOutputProperty(Serializer.Property.INDENT, "yes");
            serializer.serializeNode(destination.getXdmNode());
            serializer.close();
            return;
        }

        // It wasn't java, assume it was XML
        System.err.println("Processing " + filename);
        xmlToCSharp = compiler.compile(new StreamSource(new File(repoDir, "tools/xslt/xml-to-cs.xsl")));
        Xslt30Transformer trans = xmlToCSharp.load30();
        if (DEBUG) {
            trans.setTraceListener(new XSLTTraceListener());
        }
        Map<QName, XdmValue> params = new HashMap<>();
        params.put(new QName("digest"), parseDigest(digest));
        trans.setStylesheetParameters(params);
        trans.applyTemplates(new StreamSource(new FileReader(filename)), xmlToCSharp.getProcessor().newSerializer(System.out));
    }

    private void findJavaFiles(File inputDir, File xmlDir, FilesToProcess toBeProcessed) {
        String[] files = inputDir.list();
        if (files != null) {
            for (String file : files) {
                findJavaFiles(inputDir, file, xmlDir, toBeProcessed);
            }
        }
    }

    private void findJavaFiles(File directory, String filename, File xmlDir, FilesToProcess toBeProcessed) {
        File file = new File(directory, filename);
        if (file.isDirectory()) {
            findJavaFiles(file, new File(xmlDir, filename), toBeProcessed);
        } else if (filename.endsWith(".java") && file.canRead()) {
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
                toBeProcessed.skipCount++;
            } else {
                toBeProcessed.files.put(file, xmlOut);
            }
        }
    }

    private void processJavaFiles(FilesToProcess toBeProcessed, JavaSymbolSolver symbolSolver) {
        System.err.println("Processing " + toBeProcessed.files.size() + " files; " + toBeProcessed.skipCount + " skipped.");

        try {
            int count = 0;
            int totalCount = toBeProcessed.files.size();

            for (File javaFile : toBeProcessed.files.keySet()) {
                File xmlOut = toBeProcessed.files.get(javaFile);
                File parent = xmlOut.getParentFile();
                if (!parent.exists()) {
                    parent.mkdirs();
                }

                count++;
                int perc = count * 100 / totalCount;

                System.err.print("\r                                                                        \r");
                System.err.print("Processing " + perc + "% " + javaFile.getName());

                CompilationUnit cu;
                try {
                    cu = StaticJavaParser.parse(javaFile);
                } catch (ParseProblemException pe) {
                    System.err.println("Failed to parse " + javaFile + " - " + pe.getMessage());
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
            System.err.println();
            System.err.println(e.getMessage());
        }

        System.err.println("\r                                                                        ");
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
            //combinedTypeSolver.add(new JarTypeSolver(String.format("%s/build/libs/saxon-%s-%s.jar", repoDirName, edition, jarVersion)));
            combinedTypeSolver.add(new JarTypeSolver(jarVersion));
            combinedTypeSolver.add(new JarTypeSolver(icu4jJar));
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
        Serializer serializer = processor.newSerializer(digest);
        serializer.setOutputProperty(Serializer.Property.INDENT, "yes");
        trans.callTemplate(null, serializer);
    }

    
    public void processXmlDirToCSharp(File xmlDir, File digestFile, File csDir) throws Exception {
        XdmNode digest = parseDigest(digestFile);
        xmlToCSharp = compiler.compile(new StreamSource(new File(repoDir, "tools/xslt/xml-to-cs.xsl")));
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
        //System.err.println("Processing dir " + xmlDir + " to " + csDir);
        String[] files = xmlDir.list();
        if (files != null) {
            for (String file : files) {
                processXmlFileToCSharp(xmlDir, file, csDir, digest, xmlToCSharp);
            }
        }
    }

    public void processXmlFileToCSharp(File directory, String filename, File csDir, XdmNode digest, XsltExecutable xmlToCSharp) throws Exception {
        File file = new File(directory, filename);
        String fn = file.getAbsolutePath();
        int pos = fn.lastIndexOf("build/tmp/");
        if (pos > 0) {
            fn = fn.substring(pos+9);
            while (fn.length() > 57) {
                pos = fn.indexOf("/");
                fn = fn.substring(pos+1);
            }
            fn = "..." + fn;
        }

        if (file.isDirectory()) {
            //System.err.println("Processing subdirectory " + fn);
            processXmlSubDirectoryToCSharp(file, new File(csDir, filename), digest, xmlToCSharp);
        } else if (filename.endsWith(".xml") && file.canRead()) {
            try {
                System.err.print("                                                                        \r");
                System.err.print("Processing " + fn + "\r");
                String csFileName = filename.replace(".xml", ".cs");
                if (internalClasses.contains(csFileName)) {
                    csFileName = "JJ_" + csFileName;
                }
                File csOut = new File(csDir, csFileName);
                csDir.mkdirs();
                Xslt30Transformer trans = xmlToCSharp.load30();
                Map<QName, XdmValue> params = new HashMap<>();
                params.put(new QName("digest"), digest);

                MessageHandler handler = new MessageHandler();
                trans.setMessageHandler(handler);

                trans.setStylesheetParameters(params);
                trans.setAssertionsEnabled(true);
                trans.transform(new StreamSource(file), processor.newSerializer(csOut));

                if (handler.hasMessages()) {
                    System.out.println();
                    handler.printMessages();
                }

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

    private static class MessageHandler implements Consumer<Message> {
        List<Message> messages = new ArrayList<>();

        @Override
        public void accept(Message message) {
            messages.add(message);
        }

        public boolean hasMessages() {
            return !messages.isEmpty();
        }

        public void printMessages() {
            for (Message message : messages) {
                System.out.println(message.getStringValue());
            }
            System.out.println();
            clearMessages();
        }

        public void clearMessages() {
            messages.clear();
        }
    }

    private static class FilesToProcess {
        int skipCount = 0;
        Map<File, File> files = new HashMap<>();
    }
}
