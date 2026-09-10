package com.saxonica.functions.extfn;

import net.sf.saxon.Query;
import net.sf.saxon.event.PipelineConfiguration;
import net.sf.saxon.event.Receiver;
import net.sf.saxon.event.ReceiverOption;
import net.sf.saxon.expr.ErrorExpression;
import net.sf.saxon.expr.Expression;
import net.sf.saxon.expr.Literal;
import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.functions.SystemFunction;
import net.sf.saxon.functions.URIQueryParameters;
import net.sf.saxon.functions.UnparsedTextFunction;
import net.sf.saxon.functions.registry.BuiltInFunctionSet;
import net.sf.saxon.lib.Feature;
import net.sf.saxon.lib.SerializerFactory;
import net.sf.saxon.om.*;
import net.sf.saxon.pattern.NodeKindTest;
import net.sf.saxon.serialize.SerializationParamsHandler;
import net.sf.saxon.serialize.SerializationProperties;
import net.sf.saxon.serialize.charcode.XMLCharacterData;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.trans.XmlProcessingException;
import net.sf.saxon.transpile.CSharp;
import net.sf.saxon.transpile.CSharpReplaceBody;
import net.sf.saxon.tree.iter.TextLinesIterator;
import net.sf.saxon.type.AnyItemType;
import net.sf.saxon.type.BuiltInAtomicType;
import net.sf.saxon.value.*;
import net.sf.saxon.z.IntPredicateLambda;

import javax.xml.transform.Result;
import javax.xml.transform.stream.StreamResult;
import java.io.*;
import java.math.BigDecimal;
import java.net.URI;
import java.net.URISyntaxException;
import java.nio.charset.*;
import java.nio.file.Files;
import java.nio.file.StandardCopyOption;
import java.util.ArrayList;
import java.util.List;
import java.util.Objects;
import java.util.Properties;


/**
 * Implementation of the file:base-dir function in the EXPath file library.
 * Other functions in this library are implemented as reflexive extension
 * functions, but this one can't be implemented this way because it needs
 * access to the static context.
 */
public class EXPathFileFunctionSet extends BuiltInFunctionSet {

    private static final EXPathFileFunctionSet THE_INSTANCE = new EXPathFileFunctionSet();

    public static EXPathFileFunctionSet getInstance() {
        return THE_INSTANCE;
    }

    private EXPathFileFunctionSet() {
        init();
    }

    private void init() {

        register("append", 2, e -> e.populate(FileAppend::new, AnyItemType.getInstance(), STAR, LATE|SIDE)
                .arg(0, BuiltInAtomicType.STRING, ONE, null)
                .arg(1, AnyItemType.getInstance(), STAR, null));
        register("append", 3, e -> e.populate(FileAppend::new, AnyItemType.getInstance(), STAR, LATE | SIDE)
                .arg(0, BuiltInAtomicType.STRING, ONE, null)
                .arg(1, AnyItemType.getInstance(), STAR, null)
                .arg(2, NodeKindTest.ELEMENT, ONE, null));
        register("append-binary", 2, e -> e.populate(FileAppendBinary::new, AnyItemType.getInstance(), STAR, LATE | SIDE)
                .arg(0, BuiltInAtomicType.STRING, ONE, null)
                .arg(1, BuiltInAtomicType.BASE64_BINARY, ONE, null));
        register("append-text", 2, e -> e.populate(FileAppendText::new, AnyItemType.getInstance(), STAR, LATE | SIDE)
                .arg(0, BuiltInAtomicType.STRING, ONE, null)
                .arg(1, BuiltInAtomicType.STRING, ONE, null));
        register("append-text", 3, e -> e.populate(FileAppendText::new, AnyItemType.getInstance(), STAR, LATE | SIDE)
                .arg(0, BuiltInAtomicType.STRING, ONE, null)
                .arg(1, BuiltInAtomicType.STRING, ONE, null)
                .arg(2, BuiltInAtomicType.STRING, ONE, null));
        register("append-text-lines", 2, e -> e.populate(FileAppendTextLines::new, AnyItemType.getInstance(), STAR, LATE | SIDE)
                .arg(0, BuiltInAtomicType.STRING, ONE, null)
                .arg(1, BuiltInAtomicType.STRING, STAR, null));
        register("append-text-lines", 3, e -> e.populate(FileAppendTextLines::new, AnyItemType.getInstance(), STAR, LATE | SIDE)
                .arg(0, BuiltInAtomicType.STRING, ONE, null)
                .arg(1, BuiltInAtomicType.STRING, STAR, null)
                .arg(2, BuiltInAtomicType.STRING, ONE, null));
        register("base-dir", 0, e -> e.populate(FileBaseDir::new, AnyItemType.getInstance(), OPT, 0));
        register("children", 1, e -> e.populate(FileChildren::new, BuiltInAtomicType.STRING, STAR, LATE)
                .arg(0, BuiltInAtomicType.STRING, ONE, null));
        register("copy", 2, e -> e.populate(FileCopy::new, AnyItemType.getInstance(), STAR, LATE | SIDE)
                .arg(0, BuiltInAtomicType.STRING, ONE, null)
                .arg(1, BuiltInAtomicType.STRING, ONE, null));
        register("create-dir", 1, e -> e.populate(FileCreateDir::new, AnyItemType.getInstance(), STAR, LATE | SIDE)
                .arg(0, BuiltInAtomicType.STRING, ONE, null));
        register("create-temp-dir", 2, e -> e.populate(FileCreateTempDir::new, AnyItemType.getInstance(), STAR, LATE | SIDE)
                .arg(0, BuiltInAtomicType.STRING, ONE, null)
                .arg(1, BuiltInAtomicType.STRING, ONE, null));
        register("create-temp-dir", 3, e -> e.populate(FileCreateTempDir::new, AnyItemType.getInstance(), STAR, LATE | SIDE)
                .arg(0, BuiltInAtomicType.STRING, ONE, null)
                .arg(1, BuiltInAtomicType.STRING, ONE, null)
                .arg(2, BuiltInAtomicType.STRING, ONE, null));
        register("create-temp-file", 2, e -> e.populate(FileCreateTempFile::new, AnyItemType.getInstance(), STAR, LATE | SIDE)
                .arg(0, BuiltInAtomicType.STRING, ONE, null)
                .arg(1, BuiltInAtomicType.STRING, ONE, null));
        register("create-temp-file", 3, e -> e.populate(FileCreateTempFile::new, AnyItemType.getInstance(), STAR, LATE | SIDE)
                .arg(0, BuiltInAtomicType.STRING, ONE, null)
                .arg(1, BuiltInAtomicType.STRING, ONE, null)
                .arg(2, BuiltInAtomicType.STRING, ONE, null));
        register("current-dir", 0, e -> e.populate(FileCurrentDir::new, BuiltInAtomicType.STRING, ONE, 0));
        register("delete", 1, e -> e.populate(FileDelete::new, AnyItemType.getInstance(), STAR, LATE | SIDE)
                .arg(0, BuiltInAtomicType.STRING, ONE, null));
        register("delete", 2, e -> e.populate(FileDelete::new, AnyItemType.getInstance(), STAR, LATE | SIDE)
                .arg(0, BuiltInAtomicType.STRING, ONE, null)
                .arg(1, BuiltInAtomicType.BOOLEAN, ONE, null));
        register("dir-separator", 0, e -> e.populate(FileDirSeparator::new, BuiltInAtomicType.STRING, ONE, 0));
        register("exists", 1, e -> e.populate(FileExists::new, BuiltInAtomicType.BOOLEAN, ONE, LATE)
                .arg(0, BuiltInAtomicType.STRING, ONE, null));
        register("is-dir", 1, e -> e.populate(FileIsDir::new, BuiltInAtomicType.BOOLEAN, ONE, LATE)
                .arg(0, BuiltInAtomicType.STRING, ONE, null));
        register("is-file", 1, e -> e.populate(FileIsFile::new, BuiltInAtomicType.BOOLEAN, ONE, LATE)
                .arg(0, BuiltInAtomicType.STRING, ONE, null));
        register("last-modified", 1, e -> e.populate(FileLastModified::new, BuiltInAtomicType.DATE_TIME_STAMP, ONE, LATE)
                .arg(0, BuiltInAtomicType.STRING, ONE, null));
        register("line-separator", 0, e -> e.populate(FileLineSeparator::new, BuiltInAtomicType.STRING, ONE, 0));
        register("list", 1, e -> e.populate(FileList::new, BuiltInAtomicType.STRING, STAR, LATE)
                .arg(0, BuiltInAtomicType.STRING, ONE, null));
        register("list", 2, e -> e.populate(FileList::new, BuiltInAtomicType.STRING, STAR, LATE)
                .arg(0, BuiltInAtomicType.STRING, ONE, null)
                .arg(1, BuiltInAtomicType.BOOLEAN, ONE, null));
        register("list", 3, e -> e.populate(FileList::new, BuiltInAtomicType.STRING, STAR, LATE)
                .arg(0, BuiltInAtomicType.STRING, ONE, null)
                .arg(1, BuiltInAtomicType.BOOLEAN, ONE, null)
                .arg(2, BuiltInAtomicType.STRING, ONE, null));
        register("move", 2, e -> e.populate(FileMove::new, AnyItemType.getInstance(), STAR, LATE | SIDE)
                .arg(0, BuiltInAtomicType.STRING, ONE, null)
                .arg(1, BuiltInAtomicType.STRING, ONE, null));
        register("name", 1, e -> e.populate(FileName::new, BuiltInAtomicType.STRING, ONE, LATE)
                .arg(0, BuiltInAtomicType.STRING, ONE, null));
        register("parent", 1, e -> e.populate(FileParent::new, BuiltInAtomicType.STRING, OPT, LATE)
                .arg(0, BuiltInAtomicType.STRING, ONE, null));
        register("path-separator", 0, e -> e.populate(FilePathSeparator::new, BuiltInAtomicType.STRING, ONE, 0));
        register("path-to-native", 1, e -> e.populate(FilePathToNative::new, BuiltInAtomicType.STRING, ONE, LATE)
                .arg(0, BuiltInAtomicType.STRING, ONE, null));
        register("path-to-uri", 1, e -> e.populate(FilePathToUri::new, BuiltInAtomicType.ANY_URI, ONE, LATE)
                .arg(0, BuiltInAtomicType.STRING, ONE, null));
        register("read-binary", 1, e -> e.populate(FileReadBinary::new, BuiltInAtomicType.BASE64_BINARY, ONE, LATE)
                .arg(0, BuiltInAtomicType.STRING, ONE, null));
        register("read-binary", 2, e -> e.populate(FileReadBinary::new, BuiltInAtomicType.BASE64_BINARY, ONE, LATE)
                .arg(0, BuiltInAtomicType.STRING, ONE, null)
                .arg(1, BuiltInAtomicType.INTEGER, ONE, null));
        register("read-binary", 3, e -> e.populate(FileReadBinary::new, BuiltInAtomicType.BASE64_BINARY, ONE, LATE)
                .arg(0, BuiltInAtomicType.STRING, ONE, null)
                .arg(1, BuiltInAtomicType.INTEGER, ONE, null)
                .arg(2, BuiltInAtomicType.INTEGER, ONE, null));
        register("read-text", 1, e -> e.populate(FileReadText::new, BuiltInAtomicType.STRING, ONE, LATE)
                .arg(0, BuiltInAtomicType.STRING, ONE, null));
        register("read-text", 2, e -> e.populate(FileReadText::new, BuiltInAtomicType.STRING, ONE, LATE)
                .arg(0, BuiltInAtomicType.STRING, ONE, null)
                .arg(1, BuiltInAtomicType.STRING, ONE, null));
        register("read-text-lines", 1, e -> e.populate(FileReadTextLines::new, BuiltInAtomicType.STRING, STAR, LATE)
                .arg(0, BuiltInAtomicType.STRING, ONE, null));
        register("read-text-lines", 2, e -> e.populate(FileReadTextLines::new, BuiltInAtomicType.STRING, STAR, LATE)
                .arg(0, BuiltInAtomicType.STRING, ONE, null)
                .arg(1, BuiltInAtomicType.STRING, ONE, null));
        register("resolve-path", 1, e -> e.populate(FileResolvePath::new, BuiltInAtomicType.STRING, ONE, LATE)
                .arg(0, BuiltInAtomicType.STRING, ONE, null));
        register("size", 1, e -> e.populate(FileSize::new, BuiltInAtomicType.INTEGER, ONE, LATE)
                .arg(0, BuiltInAtomicType.STRING, ONE, null));
        register("temp-dir", 0, e -> e.populate(FileTempDir::new, BuiltInAtomicType.STRING, ONE, LATE));
        register("write", 2, e -> e.populate(FileWrite::new, AnyItemType.getInstance(), STAR, LATE)
                .arg(0, BuiltInAtomicType.STRING, ONE, null)
                .arg(1, AnyItemType.getInstance(), STAR, null));
        register("write", 3, e -> e.populate(FileWrite::new, AnyItemType.getInstance(), STAR, LATE)
                .arg(0, BuiltInAtomicType.STRING, ONE, null)
                .arg(1, AnyItemType.getInstance(), STAR, null)
                .arg(2, NodeKindTest.ELEMENT, ONE, null));
        register("write-binary", 2, e -> e.populate(FileWriteBinary::new, AnyItemType.getInstance(), STAR, LATE | SIDE)
                .arg(0, BuiltInAtomicType.STRING, ONE, null)
                .arg(1, BuiltInAtomicType.BASE64_BINARY, ONE, null));
        register("write-binary", 3, e -> e.populate(FileWriteBinary::new, AnyItemType.getInstance(), STAR, LATE | SIDE)
                .arg(0, BuiltInAtomicType.STRING, ONE, null)
                .arg(1, BuiltInAtomicType.BASE64_BINARY, ONE, null)
                .arg(2, BuiltInAtomicType.INTEGER, ONE, null));
        register("write-text", 2, e -> e.populate(FileWriteText::new, AnyItemType.getInstance(), STAR, LATE | SIDE)
                .arg(0, BuiltInAtomicType.STRING, ONE, null)
                .arg(1, BuiltInAtomicType.STRING, ONE, null));
        register("write-text", 3, e -> e.populate(FileWriteText::new, AnyItemType.getInstance(), STAR, LATE | SIDE)
                .arg(0, BuiltInAtomicType.STRING, ONE, null)
                .arg(1, BuiltInAtomicType.STRING, ONE, null)
                .arg(2, BuiltInAtomicType.STRING, ONE, null));
        register("write-text-lines", 2, e -> e.populate(FileWriteTextLines::new, AnyItemType.getInstance(), STAR, LATE | SIDE)
                .arg(0, BuiltInAtomicType.STRING, ONE, null)
                .arg(1, BuiltInAtomicType.STRING, STAR, null));
        register("write-text-lines", 3, e -> e.populate(FileWriteTextLines::new, AnyItemType.getInstance(), STAR, LATE | SIDE)
                .arg(0, BuiltInAtomicType.STRING, ONE, null)
                .arg(1, BuiltInAtomicType.STRING, STAR, null)
                .arg(2, BuiltInAtomicType.STRING, ONE, null));

    }

    public static final BigDecimalValue VERSION = new BigDecimalValue(new BigDecimal("1.0"));
    public static final NamespaceUri NAMESPACE = NamespaceUri.EXPATH_FILE;
    public static final String PREFIX = "file";
    public static final NamespaceUri ERROR_NAMESPACE = NAMESPACE;
    public static final String ERROR_PREFIX = PREFIX;

    private static final String ERROR_PATH_NOT_EXIST = "not-found";    //FILE0001
    private static final String ERROR_PATH_EXISTS = "exists";  //FILE0002
    private static final String ERROR_PATH_NOT_DIRECTORY = "no-dir"; //FILE0003
    private static final String ERROR_PATH_IS_DIRECTORY = "is-dir";  //FILE0004
    private static final String ERROR_UNKNOWN_ENCODING = "unknown-encoding";  //FILE0005
    public static final String ERROR_INDEX_OUT_OF_BOUNDS = "out-of-range";  //FILE0006
    public static final String ERROR_IO = "io-error";   //FILE9999

    
    public static final String NEWLINE = System.getProperty("line.separator");


    @Override
    public NamespaceUri getNamespace() {
        return NamespaceUri.EXPATH_FILE;
    }

    @Override
    public String getConventionalPrefix() {
        return PREFIX;
    }

    public static class FileBaseDir extends SystemFunction {

        /**
         * Make an expression that either calls this function, or that is equivalent to a call
         * on this function
         *
         * @param arguments the supplied arguments to the function call
         * @return either a function call on this function, or an expression that delivers
         * the same result
         */
        @Override
        public Expression makeFunctionCall(Expression... arguments) {
            if (getRetainedStaticContext().getPackageData().isRelocatable()) {
                return super.makeFunctionCall(arguments);
            } else {
                try {
                    return Literal.makeLiteral(makeResult(getStaticBaseUriString()));
                } catch (XPathException e) {
                    return new ErrorExpression(new XmlProcessingException(e));
                }
            }
        }

        /**
         * Invoke the function
         *
         * @param context the XPath dynamic evaluation context
         * @param args    the actual arguments to be supplied
         * @return the result of invoking the function
         * @throws XPathException if a dynamic error occurs within the function
         */
        @Override
        public Sequence call(XPathContext context, Sequence[] args) throws XPathException {
            return makeResult(getStaticBaseUriString());
        }

        private static GroundedValue makeResult(String baseURI) throws XPathException {
            if (baseURI == null) {
                return EmptySequence.getInstance();
            }
            String parent = _parent(baseURI);
            if (parent == null) {
                return EmptySequence.getInstance();
            }
            return StringValue.makeStringValue(parent);
        }

    }

    public static class FileAppend extends SystemFunction {
        @Override
        public Sequence call(XPathContext context, Sequence[] arguments) throws XPathException {
            String path = arguments[0].head().getStringValue();
            SerializationProperties params;
            if (arguments.length > 2) {
                SerializationParamsHandler sph = new SerializationParamsHandler();
                sph.setSerializationParams((NodeInfo)arguments[2].head());
                params = sph.getSerializationProperties();
            } else {
                Properties props = new Properties();
                props.setProperty("method", "xml");
                params = new SerializationProperties(props);
            }
            serialize(context, path, true, arguments[1], params);
            return EmptySequence.getInstance();
        }
    }

    public static class FileAppendBinary extends SystemFunction {
        @Override
        public Sequence call(XPathContext context, Sequence[] arguments) throws XPathException {
            String path = arguments[0].head().getStringValue();
            try {
                File file = toFile(path);
                if (file.isDirectory()) {
                    throw error("Path " + path + " is a directory", ERROR_PATH_IS_DIRECTORY);
                }
                FileOutputStream stream = makeFileAppendOutputStream(file);
                Base64BinaryValue contents = (Base64BinaryValue) arguments[1].head();
                stream.write(contents.getBinaryValue());
                stream.close();
            } catch (FileNotFoundException e) {
                throw error("Failed to create file", e, ERROR_PATH_NOT_DIRECTORY);
            } catch (IOException e) {
                throw error("Failed to write to file " + path, e, ERROR_IO);
            }
            return EmptySequence.getInstance();
        }
    }

    @CSharpReplaceBody(code="return file.makeAppendOutputStream();")
    private static FileOutputStream makeFileAppendOutputStream(File file) throws FileNotFoundException {
        // Separate method to enable C# conversion
        return new FileOutputStream(file, true);
    }


    public static class FileAppendText extends SystemFunction {
        @Override
        //@CSharpReplaceException(from = "java.io.UnsupportedEncodingException", to = "System.ArgumentException")
        public Sequence call(XPathContext context, Sequence[] arguments) throws XPathException {
            String path = arguments[0].head().getStringValue();
            String contents = arguments[1].head().getStringValue();
            String encoding = "UTF-8";
            if (arguments.length > 2) {
                encoding = arguments[2].head().getStringValue();
            }
            File file = null;
            Writer writer;
            try {
                file = toFile(path);
                if (file.isDirectory()) {
                    throw error("Path " + path + " is a directory", ERROR_PATH_IS_DIRECTORY);
                }
                writer = Objects.requireNonNull(
                        newOutputStreamWriter(makeFileAppendOutputStream(file), encoding));
                writer.write(contents);
                writer.close();
            } catch (UnsupportedEncodingException e) {
                throw error("Unsupported encoding " + encoding, ERROR_UNKNOWN_ENCODING);
            } catch (FileNotFoundException e) {
                throw error("Failed to create file", e, ERROR_PATH_NOT_DIRECTORY);
            } catch (IOException e) {
                throw error("Failed to write to file " + file.getAbsolutePath(), e, ERROR_IO);
            }
            return EmptySequence.getInstance();
        }
    }

    @CSharpReplaceBody(code="return Saxon.Impl.Helpers.IOUtils.newOutputStreamWriter(stream, encoding);")
    private static Writer newOutputStreamWriter(OutputStream stream, String encoding) throws XPathException {
        // Separate method to allow C# conversion
        try {
            return new OutputStreamWriter(stream, encoding);
        } catch (UnsupportedEncodingException e) {
            throw handleIOError(e);
        }
    }

    public static class FileAppendTextLines extends SystemFunction {
        @Override
        public Sequence call(XPathContext context, Sequence[] arguments) throws XPathException {
            String path = arguments[0].head().getStringValue();
            String encoding = "UTF-8";
            if (arguments.length > 2) {
                encoding = arguments[2].head().getStringValue();
            }
            File file;
            try {
                file = toFile(path);
                if (file.isDirectory()) {
                    throw error("Path " + path + " is a directory", ERROR_PATH_IS_DIRECTORY);
                }
                Writer writer = newOutputStreamWriter(makeFileAppendOutputStream(file), encoding);
                SequenceIterator iter = arguments[1].iterate();
                for (Item item; (item = iter.next()) != null; ) {
                    writer.write(item.getStringValue());
                    writer.write(NEWLINE);
                }
            } catch (UnsupportedEncodingException e) {
                throw error("Unsupported encoding " + encoding, ERROR_UNKNOWN_ENCODING);
            } catch (FileNotFoundException e) {
                throw error("Failed to create file", e, ERROR_PATH_NOT_DIRECTORY);
            } catch (IOException e) {
                throw error("Failed to write to file " + path, e, ERROR_IO);
            }
            return EmptySequence.getInstance();
        }
    }

    public static class FileChildren extends SystemFunction {
        @Override
        public Sequence call(XPathContext context, Sequence[] arguments) throws XPathException {
            List<StringValue> result = new ArrayList<>();
            String dir = arguments[0].head().getStringValue();
            File fd = toExistingDirectory(dir);
            if (!fd.isDirectory()) {
                throw error("File " + dir + " is not a directory", ERROR_PATH_NOT_DIRECTORY);
            }
            for (File file : Objects.requireNonNull(fd.listFiles())) {
                result.add(new StringValue(file.getAbsolutePath() + (file.isDirectory() ? File.separator : "")));
            }
            return SequenceExtent.makeSequenceExtent(result);
        }
    }

    public static class FileCopy extends SystemFunction {
        @SuppressWarnings("ResultOfMethodCallIgnored")
        @Override
        public Sequence call(XPathContext context, Sequence[] arguments) throws XPathException {
            File from = toFile(arguments[0].head().getStringValue());
            File to = toFile(arguments[1].head().getStringValue());
            if (from.exists() && from.isDirectory()) {
                if (to.exists()) {
                    if (to.isDirectory()) {
                        to = new File(to, from.getName());     // the source directory goes INSIDE the target
                        to.mkdir();
                        copyDirectory(from, to);
                    } else {
                        throw error("Source is a directory but target is a file", ERROR_PATH_EXISTS);
                    }
                } else {
                    to.mkdir();
                    copyDirectory(from, to);
                }
            } else {
                if (to.exists() && to.isDirectory()) {
                    to = new File(to, from.getName());
                }
                copyFile(from, to);
            }
            return EmptySequence.getInstance();
        }
    }

    public static class FileCreateDir extends SystemFunction {
        @Override
        public Sequence call(XPathContext context, Sequence[] arguments) throws XPathException {
            String path = arguments[0].head().getStringValue();
            File file = toFile(path);
            if (file.exists()) {
                if (!file.isDirectory()) {
                    throw error("Path " + path + " already exists and is not a directory", ERROR_PATH_EXISTS);
                }
            } else {
                boolean result = file.mkdirs();
                if (!result) {
                    throw error("Failed to create directory " + path, ERROR_PATH_EXISTS);
                }
            }
            return EmptySequence.getInstance();
        }
    }

    public static class FileCreateTempDir extends SystemFunction {
        @SuppressWarnings("ResultOfMethodCallIgnored")
        @Override
        public Sequence call(XPathContext context, Sequence[] arguments) throws XPathException {
            try {
                String prefix = arguments[0].head().getStringValue();
                String suffix = arguments[1].head().getStringValue();
                File test;
                if (arguments.length == 2) {
                    test = File.createTempFile(prefix, suffix);
                } else {
                    String dir = arguments[2].head().getStringValue();
                    test = File.createTempFile(prefix, suffix, toFile(dir));

                }
                String path = test.getCanonicalPath();
                test.delete();
                File temp = new File(path);
                temp.mkdirs();
                deleteTempFile(context, temp);
                return new StringValue(temp.getCanonicalPath());
            } catch (IOException e) {
                throw handleIOError(e);
            }
        }
    }


    public static class FileCreateTempFile extends SystemFunction {
        @Override
        public Sequence call(XPathContext context, Sequence[] arguments) throws XPathException {
            try {
                String prefix = arguments[0].head().getStringValue();
                String suffix = arguments[1].head().getStringValue();
                File temp;
                if (arguments.length == 2) {
                    temp = File.createTempFile(prefix, suffix);
                } else {
                    String dir = arguments[2].head().getStringValue();
                    temp = File.createTempFile(prefix, suffix, toFile(dir));
                }
                deleteTempFile(context, temp);
                return new StringValue(temp.getCanonicalPath());
            } catch (IOException e) {
                throw handleIOError(e);
            }
        }
    }

    public static class FileCurrentDir extends SystemFunction {
        @Override
        public Sequence call(XPathContext context, Sequence[] arguments) throws XPathException {
            File file = toFile(".");
            return new StringValue(file.getAbsolutePath() + (file.isDirectory() ? File.separator : ""));
        }
    }

    public static class FileDelete extends SystemFunction {
        @Override
        public Sequence call(XPathContext context, Sequence[] arguments) throws XPathException {
            String path = arguments[0].head().getStringValue();
            File file = toExistingFile(path);
            boolean recurse = false;
            if (arguments.length == 2) {
                recurse = ((BooleanValue)arguments[1].head()).getBooleanValue();
            }
            if (!recurse) {
                //noinspection ConstantConditions
                if (file.isDirectory() && file.list().length != 0) {
                    throw error("Cannot delete non-empty directory " + file.getAbsolutePath(), ERROR_PATH_IS_DIRECTORY);
                }
                boolean result = file.delete();
                if (!result) {
                    throw error("Failed to delete file " + file.getAbsolutePath(), ERROR_IO);
                }
            } else {
                try {
                    deleteRecursive(file);
                } catch (IOException e) {
                    throw error("Failed to delete " + path, e, ERROR_IO);
                }
            }
            return EmptySequence.getInstance();
        }
    }

    public static class FileDirSeparator extends SystemFunction {
        @Override
        public Sequence call(XPathContext context, Sequence[] arguments) throws XPathException {
            return new StringValue(File.separator);
        }
    }

    public static class FileExists extends SystemFunction {
        @Override
        public Sequence call(XPathContext context, Sequence[] arguments) throws XPathException {
            File file = toFile(arguments[0].head().getStringValue());
            return BooleanValue.get(file.exists());
        }
    }

    public static class FileIsDir extends SystemFunction {
        @Override
        public Sequence call(XPathContext context, Sequence[] arguments) throws XPathException {
            File file = toFile(arguments[0].head().getStringValue());
            return BooleanValue.get(file.exists() && file.isDirectory());
        }
    }

    public static class FileIsFile extends SystemFunction {
        @Override
        public Sequence call(XPathContext context, Sequence[] arguments) throws XPathException {
            File file = toFile(arguments[0].head().getStringValue());
            return BooleanValue.get(file.exists() && file.isFile());
        }
    }

    public static class FileLastModified extends SystemFunction {
        @Override
        public Sequence call(XPathContext context, Sequence[] arguments) throws XPathException {
            File file = toExistingFile(arguments[0].head().getStringValue());
            long dt = file.lastModified();
            return DateTimeValue.EPOCH.add(DayTimeDurationValue.fromMilliseconds(dt));
        }
    }

    public static class FileLineSeparator extends SystemFunction {
        @Override
        public Sequence call(XPathContext context, Sequence[] arguments) throws XPathException {
            return new StringValue(NEWLINE);
        }
    }

    public static class FileList extends SystemFunction {
        @Override
        public Sequence call(XPathContext context, Sequence[] arguments) throws XPathException {
            String path = arguments[0].head().getStringValue();
            File file = toExistingDirectory(path);
            if (!file.isDirectory()) {
                throw error("File " + path + " is not a directory", ERROR_PATH_NOT_DIRECTORY);
            }
            boolean recursive = false;
            if (arguments.length > 1) {
                recursive = ((BooleanValue)arguments[1].head()).getBooleanValue();
            }
            List<StringValue> result = new ArrayList<>();
            FilenameFilter filter;
            if (arguments.length > 2) {
                filter = URIQueryParameters.makeGlobFilter(arguments[2].head().getStringValue());
            } else {
                filter = null;  // means all files are accepted
            }
            gatherDirectoryContents(file, "", recursive, filter, result);
            return SequenceExtent.makeSequenceExtent(result);
        }

        private static void gatherDirectoryContents(File dir, String prefix, boolean recurse, FilenameFilter filter, List<StringValue> result) {
            URIQueryParameters.RegexFilter filter2 =
                    filter instanceof URIQueryParameters.RegexFilter ? (URIQueryParameters.RegexFilter) filter : null;
            for (File file : Objects.requireNonNull(dir.listFiles(filter))) {
                if (filter2 == null || !file.isDirectory() || filter2.matches(file.getName())) {
                    String name =
                            prefix.isEmpty() ? file.getName() : prefix + File.separator + file.getName();
                    if (file.isDirectory()) {
                        name += File.separator;
                    }
                    result.add(new StringValue(name));
                }
            }
            if (recurse) {
                for (File file : Objects.requireNonNull(dir.listFiles())) {
                    if (file.isDirectory()) {
                        String name =
                                prefix.isEmpty() ? file.getName() : prefix + File.separator + file.getName();
                        gatherDirectoryContents(file, name, true, filter, result);
                    }
                }
            }
        }
    }

    public static class FileMove extends SystemFunction {
        @Override
        public Sequence call(XPathContext context, Sequence[] arguments) throws XPathException {
            File from = toExistingFile(arguments[0].head().getStringValue());
            File to = toFile(arguments[1].head().getStringValue());
            if (from.isDirectory()) {
                if (to.exists()) {
                    if (to.isDirectory()) {
                        boolean success = from.renameTo(new File(to, from.getName())); 
                        // the directory gets moved INSIDE the target
                        if (!success) {
                            throw error("Renaming of " + from.getName() + " failed", ERROR_IO);
                        }
                    } else {
                        throw error("Target of directory move exists and is not a directory", ERROR_PATH_EXISTS);
                    }
                } else {
                    boolean created = to.mkdirs();
                    if (!created) {
                        throw error("Creation of " + to + " failed", ERROR_IO);
                    }
                    for (File f : Objects.requireNonNull(from.listFiles())) {
                        boolean success = f.renameTo(new File(to, f.getName()));
                        if (!success) {
                            throw error("Renaming of " + f + " failed", ERROR_IO);
                        }
                    }
                }
            } else {
                if (to.isDirectory()) {
                    to = new File(to, from.getName());
                }
                //TODO - move sideways to rollback if something fails.
                if (to.exists()) {
                    boolean success = to.delete();
                    if (!success) {
                        throw error("Deletion of existing file " + to + " failed", ERROR_IO);
                    }
                }
                boolean result = from.renameTo(to);
                if (!result) {
                    throw error("Moving " + from + " to " + to + " failed ", ERROR_IO);
                }
                //
            }
            return EmptySequence.getInstance();
        }
    }

    public static class FileName extends SystemFunction {
        @Override
        public Sequence call(XPathContext context, Sequence[] arguments) throws XPathException {
            String path = arguments[0].head().getStringValue();
            if (path.equals("/") || path.equals("")) {  
                return StringValue.EMPTY_STRING;
            }
            File file = toFile(path);
            return new StringValue(file.getName());
        }
    }

    public static class FileParent extends SystemFunction {
        @Override
        public Sequence call(XPathContext context, Sequence[] arguments) throws XPathException {
            String path = arguments[0].head().getStringValue();
            if (path.equals("/")) {
                return EmptySequence.getInstance();
            }
            File file = toFile(path);
            try {
                File parent = new File(file.getParent());
                return new StringValue(parent.getCanonicalPath() + File.separator);
            } catch (IOException e) {
                throw handleIOError(e);
            }
        }
    }

    public static class FilePathSeparator extends SystemFunction {
        @Override
        public Sequence call(XPathContext context, Sequence[] arguments) throws XPathException {
            return new StringValue(File.pathSeparator);
        }
    }

    public static class FilePathToNative extends SystemFunction {
        @Override
        public Sequence call(XPathContext context, Sequence[] arguments) throws XPathException {
            final String path = arguments[0].head().getStringValue();
            File file = null;
            if (path.startsWith("file:/")) {
                try {
                    URI uri = new URI(path);
                    file = new File(uri);
                    if (!(file.exists())) {
                        throw error(("No file exists at " + path), ERROR_PATH_NOT_EXIST);
                    }
                } catch (URISyntaxException err) {
                    // no action
                }
            }
            if (file == null) {
                file = toExistingFile(path);
            }
            try {
                return new StringValue(file.getCanonicalPath());
            } catch (IOException e) {
                throw handleIOError(e);
            }
        }
    }

    public static class FilePathToUri extends SystemFunction {
        @Override
        public Sequence call(XPathContext context, Sequence[] arguments) throws XPathException {
            String filePath = arguments[0].head().getStringValue();
            URI uri;
            if (File.separator.equals("\\") && (filePath.startsWith("\\\\") || filePath.startsWith("//"))) {
                // See bug 3745. This is specifically to handle UNC paths, working around
                // https://bugs.java.com/view_bug.do?bug_id=4723726.
                uri = new File(filePath).toPath().toUri();
            } else {
                uri = toFile(filePath).toURI().normalize();
            }
            return new AnyURIValue(escapeCS(uri.toString()));
        }

        @CSharpReplaceBody(code="return System.Uri.EscapeUriString(s);")
        private static String escapeCS(String s) {
            return s;
        }
    }



    public static class FileReadBinary extends SystemFunction {
        @Override
        public Sequence call(XPathContext context, Sequence[] arguments) throws XPathException {
            String path = arguments[0].head().getStringValue();
            File file = checkReadSource(path);
            long offset = 0;
            if (arguments.length > 1) {
                offset = ((NumericValue)arguments[1].head()).longValue();
            }
            long length = file.length() - offset;
            if (arguments.length > 2) {
                length = ((NumericValue) arguments[2].head()).longValue();
            }
            return readBinary(file, path, offset, length);
        }

        private static Base64BinaryValue readBinary(File file, String path, long offset, long length) throws XPathException {
            try {
                long fileLength = file.length();
                if (offset < 0L) {
                    throw error("Negative offset in binary read:" + offset, ERROR_INDEX_OUT_OF_BOUNDS);
                }
                if (length < 0L) {
                    throw error("Negative length in binary read:" + length, ERROR_INDEX_OUT_OF_BOUNDS);
                }
                if (offset + length > fileLength) {
                    throw error("Length exceeds file length in binary read:" + (offset + length), ERROR_INDEX_OUT_OF_BOUNDS);
                }
                byte[] content = new byte[(int) length];
                FileInputStream in = new FileInputStream(file);
                if (offset > 0) {
                    long skipped = in.skip(offset);
                    if (skipped != offset) {
                        throw error("Error skipping content at start of binary: " + skipped + " bytes skipped, but requested " + offset, ERROR_IO);
                    }
                }
                int bytes = in.read(content, 0, (int) length);
                if (bytes != content.length) {
                    in.close();
                    throw error("Number of bytes read does not match file length", ERROR_IO);
                }
                in.close(); // ensure input stream is closed
                return new Base64BinaryValue(content);
            } catch (IOException e) {
                throw error("Failed to read file " + path, e, ERROR_IO);
            }
        }
    }

    public static class FileReadText extends SystemFunction {
        @Override
        public Sequence call(XPathContext context, Sequence[] arguments) throws XPathException {
            String path = arguments[0].head().getStringValue();
            File file = checkReadSource(path);
            String encoding = "UTF-8";
            if (arguments.length > 1) {
                encoding = arguments[1].head().getStringValue();
            }

            Reader reader = Objects.requireNonNull(getInputStreamReader(file, encoding));
            try {
                String result = UnparsedTextFunction.readFile(
                        context.getConfiguration().getValidCharacterChecker(), reader).toString();
                reader.close();
                return new StringValue(result);
            } catch (XPathException e) {
                try {
                    reader.close();
                } catch (IOException e1) {
                    throw error("Failed to close file " + file.getAbsolutePath(), e, ERROR_IO);
                }
                throw error(e.getMessage(), ERROR_IO);
            } catch (IOException e) {
                try {
                    reader.close();
                } catch (IOException e1) {
                    throw error("Failed to close file " + file.getAbsolutePath(), e, ERROR_IO);
                }
                throw error("Failed to read file " + path, e, ERROR_IO);
            }
        }


    }

    public static class FileReadTextLines extends SystemFunction {
        @Override
        public Sequence call(XPathContext context, Sequence[] arguments) throws XPathException {
            String path = arguments[0].head().getStringValue();
            File file = checkReadSource(path);
            String encoding = "UTF-8";
            if (arguments.length > 1) {
                encoding = arguments[1].head().getStringValue();
            }
            return new LazySequence(
                    new TextFileLinesIterator(file, encoding));

        }
    }

    public static class FileResolvePath extends SystemFunction {
        @Override
        public Sequence call(XPathContext context, Sequence[] arguments) throws XPathException {
            File file = toFile(arguments[0].head().getStringValue());
            return new StringValue(file.getAbsolutePath() + (file.isDirectory() ? File.separator : ""));
        }
    }

    public static class FileSize extends SystemFunction {
        @Override
        public Sequence call(XPathContext context, Sequence[] arguments) throws XPathException {
            File file = toExistingFile(arguments[0].head().getStringValue());
            return new Int64Value(file.isDirectory() ? 0 : file.length());
        }
    }

    public static class FileTempDir extends SystemFunction {
        @Override
        public Sequence call(XPathContext context, Sequence[] arguments) throws XPathException {
            return new StringValue(System.getProperty("java.io.tmpdir"));
        }
    }

    public static class FileWrite extends SystemFunction {
        @Override
        public Sequence call(XPathContext context, Sequence[] arguments) throws XPathException {
            String path = arguments[0].head().getStringValue();
            Sequence contents = arguments[1];
            SerializationProperties params;
            if (arguments.length > 2) {
                SerializationParamsHandler sph = new SerializationParamsHandler();
                sph.setSerializationParams((NodeInfo) arguments[2].head());
                params = sph.getSerializationProperties();
            } else {
                params = context.getConfiguration().obtainDefaultSerializationProperties();
            }
            serialize(context, path, false, contents, params);
            return EmptySequence.getInstance();
        }
    }

    public static class FileWriteBinary extends SystemFunction {
        @Override
        public Sequence call(XPathContext context, Sequence[] arguments) throws XPathException {
            String path = arguments[0].head().getStringValue();
            File file = checkWriteDestination(path);
            byte[] content = ((Base64BinaryValue) arguments[1].head()).getBinaryValue();
            if (arguments.length == 2) {
                try {

                    FileOutputStream stream = new FileOutputStream(file);
                    stream.write(content);
                    stream.close();
                } catch (IOException e) {
                    throw error("Failed to write to file " + path, e, ERROR_IO);
                }
            } else {
                try {
                    long offset = ((NumericValue)arguments[2].head()).longValue();
                    if (offset < 0L) {
                        throw error("Negative offset in binary write:" + offset, ERROR_INDEX_OUT_OF_BOUNDS);
                    }
                    int lb = content.length;
                    byte[] data;
                    if (file.exists() && offset > 0) {
                        long fileLength = file.length();
                        // Comment the test out for permitting extending the file
                        if (offset > fileLength) {
                            throw error("Offset exceeds file length in binary write:" + offset, ERROR_INDEX_OUT_OF_BOUNDS);
                        }
                        data = new byte[Math.max((int) fileLength, (int) offset + lb)];
                        FileInputStream in = new FileInputStream(file);
                        int bytes = in.read(data);
                        if (bytes != fileLength) {
                            in.close();
                            throw error("Number of bytes read does not match file length", ERROR_IO);
                        }
                        in.close();
                    } else {
                        data = new byte[(int) offset + lb];
                    }
                    System.arraycopy(content, 0, data, (int) offset, lb);
                    FileOutputStream stream = new FileOutputStream(file);
                    stream.write(data);
                    stream.close();
                } catch (IOException e) {
                    throw error("Failed to write to file " + path, e, ERROR_IO);
                }
            }
            return EmptySequence.getInstance();
        }
    }

    public static class FileWriteText extends SystemFunction {
        @Override
        public Sequence call(XPathContext context, Sequence[] arguments) throws XPathException {
            String path = arguments[0].head().getStringValue();
            File file = checkWriteDestination(path);
            String content = arguments[1].head().getStringValue();
            String encoding = "UTF-8";
            if (arguments.length > 2) {
                encoding = arguments[2].head().getStringValue();
            }
            try {
                FileOutputStream stream = new FileOutputStream(file);
                Writer writer = newOutputStreamWriter(stream, encoding);
                writer.write(content);
                writer.close();
            } catch (UnsupportedEncodingException e) {
                throw error("Unsupported encoding " + encoding, ERROR_UNKNOWN_ENCODING);
            } catch (FileNotFoundException e) {
                throw error("Failed to create file", e, ERROR_PATH_NOT_DIRECTORY);
            } catch (IOException e) {
                throw error("Failed to write to file " + file.getAbsolutePath(), e, ERROR_IO);
            }
            return EmptySequence.getInstance();
        }
    }

    public static class FileWriteTextLines extends SystemFunction {
        @Override
        public Sequence call(XPathContext context, Sequence[] arguments) throws XPathException {
            String path = arguments[0].head().getStringValue();
            File file = checkWriteDestination(path);
            SequenceIterator contents = arguments[1].iterate();
            String encoding = "UTF-8";
            if (arguments.length > 2) {
                encoding = arguments[2].head().getStringValue();
            }
            try {
                FileOutputStream stream = new FileOutputStream(file);
                Writer writer = newOutputStreamWriter(stream, encoding);
                Item i;
                while ((i = contents.next()) != null) {
                    writer.write(i.getStringValue());
                    writer.write(NEWLINE);
                }
                writer.close();
            } catch (UnsupportedEncodingException e) {
                throw error("Unsupported encoding " + encoding, ERROR_UNKNOWN_ENCODING);
            } catch (FileNotFoundException e) {
                throw error("Failed to create file", e, ERROR_PATH_NOT_DIRECTORY);
            } catch (IOException e) {
                throw error("Failed to write to file " + file.getAbsolutePath(), e, ERROR_IO);
            }
            return EmptySequence.getInstance();
        }
    }

    /**
     * Get the file corresponding to a given path
     *
     * @param path either a file name, or a URI starting with "file:///" or "file:/".
     *             If a relative file name is supplied, it is resolved relative to the directory named in the
     *             system property "expath.base.directory" if it exists, or the current working directory otherwise.
     * @return the corresponding File object
     */

    private static File toFile(String path) throws XPathException {
        File file;
        try {
            file = new File(path);
            if (file.isAbsolute()) {
                return file;
            }
            boolean maybeURI = path.startsWith("file:");
            if (maybeURI) {
                URI uri = new URI(path.replace("\\", "/"));
                return new File(uri.getPath());
            } else {
                String dir = System.getProperty("expath.base.directory");
                if (dir != null) {
                    return new File(dir, path).getCanonicalFile();
                } else {
                    return file.getCanonicalFile(); // see bug 3745
                }
            }
        } catch (URISyntaxException e) {
            throw new XPathException("URISyntax error in file path:" + path);
        } catch (IOException e) {
            throw new XPathException("IO error in file path:" + path);
        }
        // return null;
    }


    /**
     * Get the file corresponding to a given path
     *
     * @param path either a file name, or a URI starting with "file:///" or "file:/".
     *             If a relative file name is supplied, it is resolved relative to the directory named in the
     *             system property "expath.base.directory" if it exists, or the current working directory otherwise.
     * @return the corresponding File object if the file exists (either as a real file, or as a directory)
     * @throws XPathException if the file does not exist
     */

    private static File toExistingFile(String path) throws XPathException {
        File file = toFile(path);
        if (!file.exists()) {
            throw error("No file exists at " + path, ERROR_PATH_NOT_EXIST);
        }
        return file;
    }

    /**
     * Check that a source path is i) not a directory and ii)  exists
     *
     * @param path The path for the reading
     * @return The file structure for reading.
     * @throws XPathException if for example the path is non-existent, or not a directory
     */
    private static File checkReadSource(String path) throws XPathException {
        File file = toExistingFile(path);
        if (file.isDirectory()) {
            throw error("Path " + path + " is a directory", ERROR_PATH_IS_DIRECTORY);
        }
        if (!file.exists()) {
            throw error("File not found: " + path, ERROR_PATH_NOT_EXIST);
        }
        return file;
    }

    /**
     * Check that a destination path is i) not a directory and ii) the parent directory exists
     *
     * @param path The path for the writing
     * @return The file structure for writing.
     * @throws XPathException if for example the path is not a directory
     */
    private static File checkWriteDestination(String path) throws XPathException {
        File file = toFile(path);
        if (file.isDirectory()) {
            throw error("Path " + path + " is a directory", ERROR_PATH_IS_DIRECTORY);
        }
        if (!file.getParentFile().exists()) {
            throw error("Parent directory does not exist: " + path, ERROR_PATH_NOT_DIRECTORY);
        }
        return file;
    }


    /**
     * Get the directory corresponding to a given path
     *
     * @param path either a file name, or a URI starting with "file:///" or "file:/".
     *             If a relative file name is supplied, it is resolved relative to the directory named in the
     *             system property "expath.base.directory" if it exists, or the current working directory otherwise.
     * @return the corresponding File object if the file exists (either as a real file, or as a directory)
     * @throws XPathException if the file does not exist
     */

    private static File toExistingDirectory(String path) throws XPathException {
        File file = toFile(path);
        if (!file.exists()) {
            throw error("No directory exists at " + path, ERROR_PATH_NOT_DIRECTORY);
        }
        return file;
    }

    /**
     * Throw an error
     *
     * @param message the error message
     * @param code    the error code to be used
     * @return XPathException always
     */

    public static XPathException error(String message, String code)  {
        XPathException e = new XPathException(message);
        e.setErrorCodeQName(new StructuredQName(ERROR_PREFIX, ERROR_NAMESPACE, code));
        return e;
    }

    /**
     * Throw an error
     *
     * @param message the error message
     * @param cause   the underlying exception, usually an IOException
     * @param code    the error code to be used
     * @return XPathException always
     */

    public static XPathException error(String message, Exception cause, String code) throws XPathException {
        XPathException e = new XPathException(message, cause);
        e.setErrorCodeQName(new StructuredQName(ERROR_PREFIX, ERROR_NAMESPACE, code));
        return e;
    }

    private static void serialize(XPathContext context, String path, boolean append, Sequence contents, SerializationProperties properties) throws XPathException {
        SerializerFactory factory = context.getConfiguration().getSerializerFactory();
        PipelineConfiguration pipe = context.getController().makePipelineConfiguration();
        pipe.setXPathContext(context);
        File file = toFile(path);
        if (file.isDirectory()) {
            throw error("Path " + path + " is a directory", ERROR_PATH_IS_DIRECTORY);
        }
        OutputStream stream;
        try {
            if (append) {
                stream = makeFileAppendOutputStream(file);
            } else {
                stream = new FileOutputStream(file);
            }
        } catch (FileNotFoundException e) {
            throw error("Failed to write to file " + path, e, ERROR_UNKNOWN_ENCODING);
        }
        Result result = new StreamResult(stream);
        Receiver sr = factory.getReceiver(result, properties, pipe);
        sr.open();
        sr.startDocument(ReceiverOption.NONE);

        SequenceIterator contentIter = contents.iterate();
        Item item;
        while ((item = contentIter.next()) != null) {
            sr.append(item);
        }
        sr.endDocument();
        sr.close();
        try {
            stream.close();
        } catch (IOException e) {
            // no action
        }
    }

    private static void copyDirectory(File from, File to) throws XPathException {
        for (File child : Objects.requireNonNull(from.listFiles())) {
            File target = new File(to, child.getName());
            if (child.isDirectory()) {
                if (target.exists()) {
                    if (!target.isDirectory()) {
                        boolean deleteOK = target.delete();
                        if (!deleteOK) {
                            throw error("Failed to delete existing file " + target, ERROR_IO);
                        }
                        boolean mkOK = target.mkdir();
                        if (!mkOK) {
                            throw error("Failed to create new directory " + target, ERROR_IO);
                        }
                    }
                } else {
                    boolean mkOK = target.mkdir();
                    if (!mkOK) {
                        throw error("Failed to create new directory " + target, ERROR_IO);
                    }
                }
                copyDirectory(child, target);
            } else {
                if (target.exists() && target.isDirectory()) {
                    throw error("Cannot copy " + child.getAbsolutePath() + " because directory " + target.getAbsolutePath() + " is in the way", ERROR_PATH_IS_DIRECTORY);
                } else {
                    copyFile(child, target);
                }
            }
        }
    }

    @CSharpReplaceBody(code="System.IO.File.Copy(sourceFile.getPath(), destFile.getPath(), true);")
    private static void copyFile(File sourceFile, File destFile) throws XPathException {
        try {
            Query.createFileIfNecessary(destFile);
            Files.copy(sourceFile.toPath(), destFile.toPath(), StandardCopyOption.REPLACE_EXISTING, StandardCopyOption.COPY_ATTRIBUTES);
        } catch (IOException e) {
            throw handleIOError(e);
        }
    }

    private static void deleteTempFile(XPathContext context, File temp) {
        Boolean d = context.getConfiguration().getConfigurationProperty(Feature.EXPATH_FILE_DELETE_TEMPORARY_FILES);
        // System.err.println("===============================DTF:" + d);
        if (d) {
            temp.deleteOnExit();
        }
    }

    @SuppressWarnings("ResultOfMethodCallIgnored")
    private static void deleteRecursive(File file) throws IOException {
        if (file.isDirectory()) {
            for (File child : Objects.requireNonNull(file.listFiles())) {
                deleteRecursive(child);
            }
        }
        file.delete();
    }

    private static XPathException handleIOError(IOException err) throws XPathException {
        XPathException xe = new XPathException(err);
        String localName = err instanceof UnsupportedEncodingException ? ERROR_UNKNOWN_ENCODING : ERROR_IO;
        xe.setErrorCodeQName(new StructuredQName(ERROR_PREFIX, ERROR_NAMESPACE, localName));
        throw xe;
    }

    private static class TextFileLinesIterator extends TextLinesIterator {

        File file;
        String encoding;

        public TextFileLinesIterator(File file, String encoding) throws XPathException {
            this.encoding = encoding;
            this.file = file;
            reader = new LineNumberReader(
                    Objects.requireNonNull(
                            getInputStreamReader(file, encoding)));
            uri = file.toURI();
            checker = IntPredicateLambda.of(CSharp.methodRef(XMLCharacterData::isValid11));
        }

    }

    /**
     * Get a Reader to read a file with a given encoding, setting options such that it throws an exception
     * if there is an encoding error, rather than taking the default option of returning a substitute character
     *
     * @param file     the input file
     * @param encoding the (alleged) encoding of the input stream
     * @return a Reader suitable for decoding the input stream
     * @throws java.nio.charset.UnsupportedCharsetException if the encoding name is not supported by the implementation
     * @throws XPathException                               if the file cannot be read
     */

    @CSharpReplaceBody(code="try {\n"
            + "                return file.getInputStreamReader(encoding);\n"
            + "            }\n"
            + "            catch (Saxon.Ejava.io.IOException) {\n"
            + "                throw error(\"Unsupported encoding \" + encoding, ERROR_UNKNOWN_ENCODING);\n"
            + "            }")
    private static Reader getInputStreamReader(File file, String encoding) throws XPathException {
        InputStream in = null;
        try {
            in = new FileInputStream(file);
            CharsetDecoder decoder = Charset.forName(encoding).newDecoder();
            decoder.onMalformedInput(CodingErrorAction.REPORT);
            return new InputStreamReader(in, decoder);
        } catch (FileNotFoundException e) {
            throw error("File not found: " + file.getAbsolutePath(), ERROR_PATH_NOT_EXIST);
        } catch (UnsupportedCharsetException e) {
            if (in != null) {
                try {
                    in.close();
                } catch (IOException e1) {
                    throw error("Failed to close file " + file.getAbsolutePath(), e, ERROR_IO);
                }
            }
            throw error("Unsupported encoding " + encoding, ERROR_UNKNOWN_ENCODING);
        } catch (IllegalCharsetNameException e) {
            if (in != null) {
                try {
                    in.close();
                } catch (IOException e1) {
                    throw error("Failed to close file " + file.getAbsolutePath(), e, ERROR_IO);
                }
            }
            throw error("Invalid encoding name " + encoding, ERROR_UNKNOWN_ENCODING);
        }
    }

    public static String _parent(String path) throws XPathException {
        if (path.equals("/")) {
            return null;
        }
        File file = toFile(path);
        try {
            File parent = new File(file.getParent());
            return parent.getCanonicalPath() + File.separator;
        } catch (IOException e) {
            //
        }
        return null;
    }


}

// Copyright (c) 2018-2022 Saxonica Limited