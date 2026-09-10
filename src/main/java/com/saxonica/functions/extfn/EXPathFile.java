package com.saxonica.functions.extfn;

import net.sf.saxon.Query;
import net.sf.saxon.event.PipelineConfiguration;
import net.sf.saxon.event.Receiver;
import net.sf.saxon.event.ReceiverOption;
import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.functions.URIQueryParameters;
import net.sf.saxon.functions.UnparsedTextFunction;
import net.sf.saxon.lib.Feature;
import net.sf.saxon.lib.SerializerFactory;
import net.sf.saxon.om.*;
import net.sf.saxon.serialize.SerializationParamsHandler;
import net.sf.saxon.serialize.SerializationProperties;
import net.sf.saxon.serialize.charcode.XMLCharacterData;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.tree.iter.TextLinesIterator;
import net.sf.saxon.tree.iter.UnfailingIterator;
import net.sf.saxon.value.*;

import javax.xml.transform.Result;
import javax.xml.transform.stream.StreamResult;
import java.io.*;
import java.math.BigDecimal;
import java.net.URI;
import java.net.URISyntaxException;
import java.nio.channels.FileChannel;
import java.nio.charset.*;
import java.util.ArrayList;
import java.util.List;
import java.util.Properties;
import java.util.function.IntPredicate;

/**
 * Implementation of extension functions in the EXPath File package, see
 * http://expath.org/spec/file
 */
public class EXPathFile {
    public static final BigDecimalValue VERSION = new BigDecimalValue(new BigDecimal("0.1"));

    public static final String NAMESPACE = "http://expath.org/ns/file";
    public static final String PREFIX = "file";
    public static final String ERROR_NAMESPACE = NAMESPACE;
    public static final String ERROR_PREFIX = PREFIX;

    private static final String ERROR_PATH_NOT_EXIST = "not-found";    //FILE0001
    private static final String ERROR_PATH_EXISTS = "exists";  //FILE0002
    private static final String ERROR_PATH_NOT_DIRECTORY = "no-dir"; //FILE0003
    private static final String ERROR_PATH_IS_DIRECTORY = "is-dir";  //FILE0004
    private static final String ERROR_UNKNOWN_ENCODING = "unknown-encoding";  //FILE0005
    public static final String ERROR_INDEX_OUT_OF_BOUNDS = "out-of-range";  //FILE0006
    public static final String ERROR_IO = "io-error";   //FILE9999

    private static final One<StringItem> UTF8 = new One<StringItem>(StringItem.bmp("UTF-8"));

    public static final String NEWLINE = System.getProperty("line.separator");

    // file:version()
    // as xs:decimal
    public static BigDecimalValue version() {
        return VERSION;
    }
    //////////////////////////////////////////////////////////////////////////////////////////////////////

//    3.1 file:exists($path as xs:string)
//        as xs:boolean

    public static One<BooleanValue> exists(One<StringItem> path) throws XPathException {
        File file = toFile(path.head().getStringValue());
        return One.bool(file.exists());
    }

//    3.2 file:is-dir($path as xs:string)
//        as xs:boolean

    public static One<BooleanValue> isDir(One<StringItem> path) throws XPathException {
        File file = toFile(path.head().getStringValue());
        return One.bool(file.exists() && file.isDirectory());
    }

//    3.3 file:is-file($path as xs:string)
//        as xs:boolean

    public static One<BooleanValue> isFile(One<StringItem> path) throws XPathException {
        File file = toFile(path.head().getStringValue());
        return One.bool(file.exists() && file.isFile());
    }

//    3.4 file:last-modified($path as xs:string)
//        as xs:dateTime

    public static One<DateTimeValue> lastModified(One<StringItem> path) throws XPathException {
        File file = toFile(path.head().getStringValue());
        if (!file.exists()) {
            error("File " + path + " does not exist", ERROR_PATH_NOT_EXIST);
        }
        long dt = file.lastModified();
        return new One<DateTimeValue>(DateTimeValue.EPOCH.add(DayTimeDurationValue.fromMilliseconds(dt)));
    }

//    3.5 file:size($file as xs:string)
//        as xs:integer

    public static One<IntegerValue> size(One<StringItem> path) throws XPathException {
        File file = toExistingFile(path.head().getStringValue());
        return One.integer(file.isDirectory() ? 0 : file.length());
    }


//    4.1 file:append($file as xs:string,
//        $items as item()*)
//        as empty-sequence()

    public static void append(XPathContext context, One<StringItem> file, ZeroOrMore<Item> contents) throws XPathException {
        Properties props = new Properties();
        props.setProperty("method", "xml");
        serialize(context, file.head().getStringValue(), true, contents, new SerializationProperties(props));
    }

    //////////////////////////////////////////////////////////////////////////////////////////////////////
//
//    4.1 file:append($file as xs:string,
//        $items as item()*,
//        $params element(output:serialization-parameters))
//        as empty-sequence()

    public static void append(XPathContext context, One<StringItem> file, ZeroOrMore<Item> contents, One<NodeInfo> params) throws XPathException {
        SerializationParamsHandler sph = new SerializationParamsHandler();
        sph.setSerializationParams(params.head());
        SerializationProperties props = sph.getSerializationProperties();
        serialize(context, file.head().getStringValue(), true, contents, props);
    }

    private static void serialize(XPathContext context, String path, boolean append, ZeroOrMore<Item> contents, SerializationProperties properties) throws XPathException {
        SerializerFactory factory = context.getConfiguration().getSerializerFactory();
        PipelineConfiguration pipe = context.getController().makePipelineConfiguration();
        pipe.setXPathContext(context);
        File file = toFile(path);
        if (file.isDirectory()) {
            error("Path " + path + " is a directory", ERROR_PATH_IS_DIRECTORY);
        }
        OutputStream stream = null;
        try {
            stream = new FileOutputStream(file, append);
        } catch (FileNotFoundException e) {
            error("Failed to write to file " + path, e, ERROR_UNKNOWN_ENCODING);
        }
        Result result = new StreamResult(stream);
        Receiver sr = factory.getReceiver(result, properties, pipe);
        sr.open();
        sr.startDocument(ReceiverOption.NONE);
        for (Item item : contents) {
            sr.append(item);
        }
        sr.endDocument();
        sr.close();
        try {
            if (stream != null) {
                stream.close();
            }
        } catch (IOException e) {
            // no action
        }
    }

//    4.3 file:append-text($file as xs:string,
//        $contents as xs:string)
//        as empty-sequence()

    public static void appendText(One<StringItem> path, One<StringItem> contents) throws XPathException {

        appendText(path, contents, UTF8);
    }
//
//    4.3 file:append-text($file as xs:string,
//        $contents as xs:string,
//        $encoding as xs:string)
//        as empty-sequence()

    public static void appendText(One<StringItem> path, One<StringItem> contents, One<StringItem> encoding) throws XPathException {
        File file = null;
        Writer writer;
        try {
            file = toFile(path.head().getStringValue());
            if (file.isDirectory()) {
                error("Path " + path + " is a directory", ERROR_PATH_IS_DIRECTORY);
            }
            writer = new OutputStreamWriter(new FileOutputStream(file, true), encoding.head().getStringValue());
            writer.write(contents.head().getStringValue());
            writer.close();
        } catch (UnsupportedEncodingException e) {
            error("Unsupported encoding " + encoding, ERROR_UNKNOWN_ENCODING);
        } catch (FileNotFoundException e) {
            error("Failed to create file", e, ERROR_PATH_NOT_DIRECTORY);
        } catch (IOException e) {
            error("Failed to write to file " + file.getAbsolutePath(), e, ERROR_IO);
        }
    }

//    4.4 file:append-text-lines($file as xs:string,
//        $contents as xs:string*)
//        as empty-sequence()

    public static void appendTextLines(One<StringItem> path, ZeroOrMore<StringItem> contents) throws XPathException {
        appendTextLines(path, contents, UTF8);
    }
//
//   4.4 file:append-text-lines($file as xs:string,
//        $contents as xs:string*,
//        $encoding as xs:string)
//        as empty-sequence()

    public static void appendTextLines(One<StringItem> path, ZeroOrMore<StringItem> contents, One<StringItem> encoding) throws XPathException {
        File file = null;
        try {
            file = toFile(path.head().getStringValue());
            if (file.isDirectory()) {
                error("Path " + path + " is a directory", ERROR_PATH_IS_DIRECTORY);
            }
            Writer writer = new OutputStreamWriter(new FileOutputStream(file, true), encoding.head().getStringValue());
            for (StringItem sv : contents) {
                writer.write(sv.getStringValue());
                writer.write(NEWLINE);
            }
        } catch (UnsupportedEncodingException e) {
            error("Unsupported encoding " + encoding, ERROR_UNKNOWN_ENCODING);
        } catch (FileNotFoundException e) {
            error("Failed to create file", e, ERROR_PATH_NOT_DIRECTORY);
        } catch (IOException e) {
            error("Failed to write to file " + (file == null ? path : file.getAbsolutePath()), e, ERROR_IO);
        }
    }

//    4.2 file:append-binary($file as xs:string,
//          $contents as xs:base64Binary*)
//          as empty-sequence()

    public static void appendBinary(One<StringItem> path, ZeroOrMore<Base64BinaryValue> contents) throws XPathException {
        try {
            File file = toFile(path.head().getStringValue());
            if (file.isDirectory()) {
                error("Path " + path + " is a directory", ERROR_PATH_IS_DIRECTORY);
            }
            FileOutputStream stream = new FileOutputStream(file, true);
            for (Base64BinaryValue b64 : contents) {
                stream.write(b64.getBinaryValue());
            }
            stream.close();
        } catch (FileNotFoundException e) {
            error("Failed to create file", e, ERROR_PATH_NOT_DIRECTORY);
        } catch (IOException e) {
            error("Failed to write to file " + path, e, ERROR_IO);
        }
    }

//    4.5 file:copy($source as xs:string,
//          $target as xs:string)
//          as empty-sequence()

    public static void copy(One<StringItem> source, One<StringItem> target) throws XPathException {
        File from = toFile(source.head().getStringValue());
        if (from.exists() && from.isDirectory()) {
            File to = toFile(target.head().getStringValue());
            if (to.exists()) {
                if (to.isDirectory()) {
                    to = new File(to, from.getName());     // the source directory goes INSIDE the target
                    to.mkdir();
                    copyDirectory(from, to);
                } else {
                    error("Source is a directory but target is a file", ERROR_PATH_EXISTS);
                }
            } else {
                to.mkdir();
                copyDirectory(from, to);
            }
        } else {
            File to = toFile(target.head().getStringValue());
            if (to.exists()) {
                if (to.isDirectory()) {
                    to = new File(to, from.getName());
                    copyFile(from, to);
                } else {
                    copyFile(from, to);
                }
            } else {
                copyFile(from, to);
            }
        }
    }

    private static void copyDirectory(File from, File to) throws XPathException {
        for (File child : from.listFiles()) {
            File target = new File(to, child.getName());
            if (child.isDirectory()) {
                if (target.exists()) {
                    if (target.isDirectory()) {
                        copyDirectory(child, target);
                    } else {
                        target.delete();
                        target.mkdir();
                        copyDirectory(child, target);
                    }
                } else {
                    target.mkdir();
                    copyDirectory(child, target);
                }
            } else {
                if (target.exists() && target.isDirectory()) {
                    error("Cannot copy " + child.getAbsolutePath() + " because directory " + target.getAbsolutePath() + " is in the way", ERROR_PATH_IS_DIRECTORY);
                } else {
                    copyFile(child, target);
                }
            }
        }
    }

    private static void copyFile(File sourceFile, File destFile) throws XPathException {
        try {
            Query.createFileIfNecessary(destFile);

            FileChannel source = null;
            FileChannel destination = null;

            try {
                source = new FileInputStream(sourceFile).getChannel();
                destination = new FileOutputStream(destFile).getChannel();
                destination.transferFrom(source, 0, source.size());
            } finally {
                if (source != null) {
                    source.close();
                }
                if (destination != null) {
                    destination.close();
                }
            }
        } catch (IOException e) {
            error("Failed to copy file " + sourceFile + " to " + destFile, e, ERROR_IO);
        }
    }


//    4.6 file:create-dir($dir as xs:string)
//          as empty-sequence()

    public static void createDir(One<StringItem> dir) throws XPathException {
        File file = toFile(dir.head().getStringValue());
        if (file.exists()) {
            if (!file.isDirectory()) {
                error("Path " + dir + " already exists and is not a directory", ERROR_PATH_EXISTS);
            }
        } else {
            boolean result = file.mkdirs();
            if (!result) {
                error("Failed to create directory " + dir, ERROR_PATH_EXISTS);
            }
        }
    }

    private static void deleteTempFile(XPathContext context, File temp) {
        Boolean d = context.getConfiguration().getConfigurationProperty(Feature.EXPATH_FILE_DELETE_TEMPORARY_FILES);
        // System.err.println("===============================DTF:" + d);
        if (d) {
            temp.deleteOnExit();
        }
    }


    //    4.7 file:create-temp-dir($prefix as xs:string, $suffix as xs:string)
//          as xs:string
    public static One<StringItem> createTempDir(XPathContext context, One<StringItem> prefix, One<StringItem> suffix) throws XPathException, IOException {
        File test = File.createTempFile(prefix.head().getStringValue(), suffix.head().getStringValue());
        String path = test.getCanonicalPath();
        test.delete();
        File temp = new File(path);
        temp.mkdirs();
        deleteTempFile(context, temp);
        return One.string(temp.getCanonicalPath());
    }

    //    4.7 file:create-temp-dir($prefix as xs:string, $suffix as xs:string, $dir as xs:string)
//          as xs:string
    public static One<StringItem> createTempDir(XPathContext context, One<StringItem> prefix, One<StringItem> suffix, One<StringItem> dir) throws XPathException, IOException {
        File file = toFile(dir.head().getStringValue());
        File test = File.createTempFile(prefix.head().getStringValue(), suffix.head().getStringValue(), file);
        String path = test.getCanonicalPath();
        test.delete();
        File temp = new File(path);
        temp.mkdirs();
        deleteTempFile(context, temp);
        return One.string(temp.getCanonicalPath());
    }

    //    4.8 file:create-temp-file($prefix as xs:string, $suffix as xs:string)
//          as xs:string
    public static One<StringItem> createTempFile(XPathContext context, One<StringItem> prefix, One<StringItem> suffix) throws XPathException, IOException {
        File temp = File.createTempFile(prefix.head().getStringValue(), suffix.head().getStringValue());
        deleteTempFile(context, temp);
        return One.string(temp.getCanonicalPath());
    }

    //    4.8 file:create-temp-file($prefix as xs:string, $suffix as xs:string, $dir as xs:string)
//          as xs:string
    public static One<StringItem> createTempFile(XPathContext context, One<StringItem> prefix, One<StringItem> suffix, One<StringItem> dir) throws XPathException, IOException {
        File file = toFile(dir.head().getStringValue());
        File temp = File.createTempFile(prefix.head().getStringValue(), suffix.head().getStringValue(), file);
        deleteTempFile(context, temp);
        return One.string(temp.getCanonicalPath());
    }

//    4.9 file:delete($path as xs:string)
//          as empty-sequence()

    public static void delete(One<StringItem> path) throws XPathException {
        File file = toExistingFile(path.head().getStringValue());
//        System.err.println("Can write parent:" + path + " = " + file.getParentFile().canWrite());
//        System.err.println("Can write:" + path + " = " + file.canWrite());
        if (file.isDirectory() && file.list().length != 0) {
            error("Cannot delete non-empty directory " + file.getAbsolutePath(), ERROR_PATH_IS_DIRECTORY);
        }
        boolean result = file.delete();
        if (!result) {
            error("Failed to delete file " + file.getAbsolutePath(), ERROR_IO);
        }
    }
//
//    4.9 file:delete($path as xs:string,
//          $recursive as xs:boolean)
//          as empty-sequence()

    public static void delete(One<StringItem> path, boolean recurse) throws XPathException {
        if (!recurse) {
            delete(path);
        } else {
            File file = toExistingFile(path.head().getStringValue());
            try {
                deleteRecursive(file);
            } catch (IOException e) {
                error("Failed to delete " + path, e, ERROR_IO);
            }
        }
    }

    private static void deleteRecursive(File file) throws IOException {
        if (file.isDirectory()) {
            for (File child : file.listFiles()) {
                deleteRecursive(child);
            }
            file.delete();
        } else {
            file.delete();
        }
    }

//    4.10 file:list($dir as xs:string)
//        as xs:string*

    public static ZeroOrMore<StringItem> list(One<StringItem> dir) throws XPathException {
        return list(dir, new One<BooleanValue>(BooleanValue.FALSE));
    }

//
//    4.10 file:list($dir as xs:string,
//        $recursive as xs:boolean)
//        as xs:string*

    public static ZeroOrMore<StringItem> list(One<StringItem> dir, One<BooleanValue> recursive) throws XPathException {
        File file = toExistingDirectory(dir.head().getStringValue());
        if (!file.isDirectory()) {
            error("File " + dir + " is not a directory", ERROR_PATH_NOT_DIRECTORY);
        }
        FilenameFilter all = new FilenameFilter() {
            @Override
            public boolean accept(File dir, String name) {
                return true;
            }
        };
        List<StringItem> result = new ArrayList<StringItem>();
        gatherDirectoryContents(file, "", recursive.head().getBooleanValue(), all, result);
        return new ZeroOrMore<StringItem>(result);
    }

//
//    4.10 file:list($dir as xs:string,
//        $recursive as xs:boolean,
//        $pattern as xs:string)
//        as xs:string*

    public static ZeroOrMore<StringItem> list(One<StringItem> dir, One<BooleanValue> recursive, One<StringItem> pattern) throws XPathException {
        //File file = toFile(dir);
        File file = toExistingDirectory(dir.head().getStringValue());
        if (!file.isDirectory()) {
            error("File " + dir + " is not a directory", ERROR_PATH_NOT_DIRECTORY);
        }
        List<StringItem> result = new ArrayList<StringItem>();
        FilenameFilter filter = URIQueryParameters.makeGlobFilter(pattern.head().getStringValue());
        gatherDirectoryContents(file, "", recursive.head().getBooleanValue(), filter, result);
        return new ZeroOrMore(result);
    }

    private static void gatherDirectoryContents(File dir, String prefix, boolean recurse, FilenameFilter filter, List<StringItem> result) {
        URIQueryParameters.RegexFilter filter2 =
                filter instanceof URIQueryParameters.RegexFilter ? (URIQueryParameters.RegexFilter) filter : null;
        for (File file : dir.listFiles(filter)) {
            if (filter2 == null || !file.isDirectory() || filter2.matches(file.getName())) {
                String name =
                        prefix.isEmpty() ? file.getName() : prefix + File.separator + file.getName();
                if (file.isDirectory()) {
                    name += File.separator;
                }
                result.add(new StringItem(name));
            }
        }
        if (recurse) {
            for (File file : dir.listFiles()) {
                if (file.isDirectory()) {
                    String name =
                            prefix.isEmpty() ? file.getName() : prefix + File.separator + file.getName();
                    gatherDirectoryContents(file, name, recurse, filter, result);
                }
            }
        }
    }

//    4.11 file:move($source as xs:string,
//        $target as xs:string)
//        as empty-sequence()

    @SuppressWarnings({"ResultOfMethodCallIgnored"})
    public static void move(One<StringItem> source, One<StringItem> target) throws XPathException {
        File from = toExistingFile(source.head().getStringValue());
        File to = toFile(target.head().getStringValue());
        if (from.isDirectory()) {
            if (to.exists()) {
                if (to.isDirectory()) {
                    from.renameTo(new File(to, from.getName())); // the dirctory gets moved INSIDE the target
                    /*for (File f : from.listFiles()) {
                        f.renameTo(new File(to, f.getName()));
                    }*/
                } else {
                    error("Target of directory move exists and is not a directory", ERROR_PATH_EXISTS);
                }
            } else {
                to.mkdirs();
                for (File f : from.listFiles()) {
                    f.renameTo(new File(to, f.getName()));
                }
            }
        } else {
            if (to.isDirectory()) {
                to = new File(to, from.getName());
            }
            //TODO - move sideways to rollback if something fails.
            if (to.exists()) {
                to.delete();
            }
            boolean result = from.renameTo(to);
            if (!result) {
                throw new XPathException("Renaming " + source + " to " + target + " failed ");
            }
            //
        }
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
            error("Path " + path + " is a directory", ERROR_PATH_IS_DIRECTORY);
        }
        if (!file.exists()) {
            error("File not found: " + path, ERROR_PATH_NOT_EXIST);
        }
        return file;
    }

//    4.12 file:read-binary($file as xs:string)
//        as xs:base64Binary

    public static One<Base64BinaryValue> readBinary(One<StringItem> path) throws XPathException {
        File file = checkReadSource(path.head().getStringValue());
        return new One<Base64BinaryValue>(
                readBinary(file, path.head().getStringValue(), 0, (int) file.length()));
    }

//    4.12 file:read-binary($file as xs:string, $offset as xs:integer)
//        as xs:base64Binary

    public static One<Base64BinaryValue> readBinary(One<StringItem> path, One<IntegerValue> offset) throws XPathException {
        File file = checkReadSource(path.head().getStringValue());
        return new One<Base64BinaryValue>(
                readBinary(file, path.head().getStringValue(),
                        offset.head().longValue(), file.length() - offset.head().longValue()));
    }

//    4.12 file:read-binary($file as xs:string, $offset as xs:integer, $length as xs:integer)
//        as xs:base64Binary

    public static One<Base64BinaryValue> readBinary(One<StringItem> path, One<IntegerValue> offset, One<IntegerValue> length) throws XPathException {
        File file = checkReadSource(path.head().getStringValue());
        return new One<Base64BinaryValue>(
                readBinary(file, path.head().getStringValue(), offset.head().longValue(), length.head().longValue()));
    }

    //    4.13 file:read-text($file as xs:string)
//        as xs:string

    public static One<StringItem> readText(XPathContext context, One<StringItem> file) throws XPathException {
        return readText(context, file, UTF8);
    }
//
//    4.13 file:read-text($file as xs:string,
//        $encoding as xs:string)
//        as xs:string

    public static One<StringItem> readText(XPathContext context, One<StringItem> path, One<StringItem> encoding) throws XPathException {
        File file = checkReadSource(path.head().getStringValue());
        /*File file = toExistingFile(path);
        if (file.isDirectory()) {
            error("File " + path + " is a directory", ERROR_PATH_IS_DIRECTORY);
        }
        if (!file.exists()) {
            error("File not found: " + file.getAbsolutePath(), ERROR_PATH_NOT_EXIST);
        }*/
        Reader reader = getInputStreamReader(file, encoding.head().getStringValue());
        try {
            String result = UnparsedTextFunction.readFile(
                    context.getConfiguration().getValidCharacterChecker(), reader).toString();
            reader.close();
            //in.close();
            return One.string(result);
        } catch (XPathException e) {
            try {
                reader.close();
            } catch (IOException e1) {
                error("Failed to close file " + file.getAbsolutePath(), e, ERROR_IO);
            }
            error(e.getMessage(), ERROR_IO);
        } catch (IOException e) {
            try {
                reader.close();
            } catch (IOException e1) {
                error("Failed to close file " + file.getAbsolutePath(), e, ERROR_IO);
            }
            error("Failed to read file " + path, e, ERROR_IO);
        }
        return null;
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

    private static Reader getInputStreamReader(File file, String encoding) throws XPathException {
        InputStream in = null;
        try {
            in = new FileInputStream(file);
            CharsetDecoder decoder = Charset.forName(encoding).newDecoder();
            decoder.onMalformedInput(CodingErrorAction.REPORT);
            return new InputStreamReader(in, decoder);
        } catch (FileNotFoundException e) {
            EXPathFile.error("File not found: " + file.getAbsolutePath(), ERROR_PATH_NOT_EXIST);
        } catch (UnsupportedCharsetException e) {
            if (in != null) {
                try {
                    in.close();
                } catch (IOException e1) {
                    error("Failed to close file " + file.getAbsolutePath(), e, ERROR_IO);
                }
            }
            EXPathFile.error("Unsupported encoding " + encoding, ERROR_UNKNOWN_ENCODING);
        } catch (IllegalCharsetNameException e) {
            if (in != null) {
                try {
                    in.close();
                } catch (IOException e1) {
                    error("Failed to close file " + file.getAbsolutePath(), e, ERROR_IO);
                }
            }
            EXPathFile.error("Invalid encoding name " + encoding, ERROR_UNKNOWN_ENCODING);
        }
        return null;
    }

//    4.14 file:read-text-lines($file as xs:string)
//        as xs:string*

    public static ZeroOrMore<StringItem> readTextLines(One<StringItem> path) throws XPathException {
        return readTextLines(path, UTF8);
    }
//
//    4.14 file:read-text-lines($file as xs:string,
//        $encoding as xs:string)
//        as xs:string*


    public static ZeroOrMore<StringItem> readTextLines(One<StringItem> path, One<StringItem> encoding) throws XPathException {
        File file = checkReadSource(path.head().getStringValue());
        /*File file = toExistingFile(path);
        if (file.isDirectory()) {
            error("Path " + file.getAbsolutePath() + " is a directory", ERROR_PATH_IS_DIRECTORY);
        }
        if (!file.exists()) {
            error("File not found: " + file.getAbsolutePath(), ERROR_PATH_NOT_EXIST);
        }*/
        return new ZeroOrMore<StringItem>(new TextFileLinesIterator(file, encoding.head().getStringValue()));

    }

    public static Base64BinaryValue readBinary(File file, String path, long offset, long length) throws XPathException {
        try {
            long fileLength = file.length();
            if (offset < 0L) {
                error("Negative offset in binary read:" + offset, ERROR_INDEX_OUT_OF_BOUNDS);
            }
            if (length < 0L) {
                error("Negative length in binary read:" + length, ERROR_INDEX_OUT_OF_BOUNDS);
            }
            if (offset + length > fileLength) {
                error("Length exceeds file length in binary read:" + (offset + length), ERROR_INDEX_OUT_OF_BOUNDS);
            }
            byte[] content = new byte[(int) length];
            FileInputStream in = new FileInputStream(file);
            if (offset > 0) {
                long skipped = in.skip(offset);
                if (skipped != offset) {
                    error("Error skipping content at start of binary: " + skipped + " bytes skipped, but requested " + offset, ERROR_IO);
                }
            }
            int bytes = in.read(content, 0, (int) length);
            if (bytes != content.length) {
                in.close();
                error("Number of bytes read does not match file length", ERROR_IO);
            }
            in.close(); // ensure input stream is closed
            return new Base64BinaryValue(content);
        } catch (IOException e) {
            error("Failed to read file " + path, e, ERROR_IO);
        }
        return null;
    }

    private static class TextFileLinesIterator extends TextLinesIterator {

        File file;
        String encoding;

        public TextFileLinesIterator(File file, String encoding) throws XPathException {
            this.encoding = encoding;
            this.file = file;
            reader = new LineNumberReader(getInputStreamReader(file, encoding));
            uri = file.toURI();
            checker = new IntPredicate() {
                @Override
                public boolean test(int value) {
                    return XMLCharacterData.isValid11(value);
                }
            };
        }

        @Override
        public StringItem next() throws XPathException {
            StringItem result = null;
            try {
                result = super.next();
            } catch (XPathException e) {
                error("Error in reading text lines", e, ERROR_IO);
            }
            return result;
        }

    }

//    4.15 file:write($file as xs:string,
//        $contents as item()*)
//        as empty-sequence()

    public static void write(XPathContext context, One<StringItem> file, ZeroOrMore<Item> contents) throws XPathException {
        serialize(context, file.head().getStringValue(), false, contents, context.getConfiguration().obtainDefaultSerializationProperties());
    }
//
//    4.15 file:write($file as xs:string,
//        $contents as item()*,
//        $params as element(output:serialization-parameters))
//        as empty-sequence()

    public static void write(XPathContext context, One<StringItem> file, ZeroOrMore<Item> contents, One<NodeInfo> params) throws XPathException {
        SerializationParamsHandler sph = new SerializationParamsHandler();
        sph.setSerializationParams(params.head());
        SerializationProperties props = sph.getSerializationProperties();
        serialize(context, file.head().getStringValue(), false, contents, props);
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
            error("Path " + path + " is a directory", ERROR_PATH_IS_DIRECTORY);
        }
        if (!file.getParentFile().exists()) {
            error("Parent directory does not exist: " + path, ERROR_PATH_NOT_DIRECTORY);
        }
        return file;
    }

//    4.16 file:write-binary($file as xs:string,
//        $contents as base64Binary)
//        as empty-sequence()

    public static void writeBinary(One<StringItem> path, One<Base64BinaryValue> contents) throws XPathException {
        try {
            File file = checkWriteDestination(path.head().getStringValue());
            FileOutputStream stream = new FileOutputStream(file);
            stream.write(contents.head().getBinaryValue());
            stream.close();
        } catch (IOException e) {
            error("Failed to write to file " + path, e, ERROR_IO);
        }
    }

//    4.16 file:write-binary($file as xs:string,
//        $contents as base64Binary, $offset as xs:integer)
//        as empty-sequence()

    public static void writeBinary(One<StringItem> path, One<Base64BinaryValue> contents, One<IntegerValue> offset) throws XPathException {
        try {
            File file = checkWriteDestination(path.head().getStringValue());
            long offset0 = offset.head().longValue();
            if (offset0 < 0L) {
                error("Negative offset in binary write:" + offset, ERROR_INDEX_OUT_OF_BOUNDS);
            }
            int lb = contents.head().getLengthInOctets();
            byte[] data;
            if (file.exists() && offset0 > 0) {
                long fileLength = file.length();
                // Comment the test out for permitting extending the file
                if (offset0 > fileLength) {
                    error("Offset exceeds file length in binary write:" + offset, ERROR_INDEX_OUT_OF_BOUNDS);
                }
                data = new byte[Math.max((int) fileLength, (int) offset0 + lb)];
                FileInputStream in = new FileInputStream(file);
                int bytes = in.read(data);
                if (bytes != fileLength) {
                    in.close();
                    error("Number of bytes read does not match file length", ERROR_IO);
                }
                in.close();
            } else {
                data = new byte[(int) offset0 + lb];
            }
            System.arraycopy(contents.head().getBinaryValue(), 0, data, (int) offset0, lb);
            FileOutputStream stream = new FileOutputStream(file);
            stream.write(data);
            stream.close();
        } catch (IOException e) {
            error("Failed to write to file " + path, e, ERROR_IO);
        }
    }

//   4.17 file:write-text($file as xs:string,
//        $contents as xs:string)
//        as empty-sequence()

    public static void writeText(One<StringItem> path, One<StringItem> contents) throws XPathException {
        writeText(path, contents, UTF8);
    }
//
//    4.17 file:write-text($file as xs:string,
//        $contents as xs:string,
//        $encoding as xs:string)
//        as empty-sequence()

    public static void writeText(One<StringItem> path, One<StringItem> contents, One<StringItem> encoding) throws XPathException {
        File file = null;
        try {
            file = checkWriteDestination(path.head().getStringValue());
            //file = toFile(path);

            FileOutputStream stream = new FileOutputStream(file);
            Writer writer = new OutputStreamWriter(stream, encoding.head().getStringValue());
            writer.write(contents.head().getStringValue());
            writer.close();
        } catch (UnsupportedEncodingException e) {
            error("Unsupported encoding " + encoding, ERROR_UNKNOWN_ENCODING);
        } catch (FileNotFoundException e) {
            error("Failed to create file", e, ERROR_PATH_NOT_DIRECTORY);
        } catch (IOException e) {
            error("Failed to write to file " + file.getAbsolutePath(), e, ERROR_IO);
        }
    }

//   4.18 file:write-text-lines($file as xs:string,
//        $contents as xs:string*)
//        as empty-sequence()

    public static void writeTextLines(One<StringItem> path, ZeroOrMore<StringItem> contents) throws XPathException {
        writeTextLines(path, contents, UTF8);
    }

//    4.18 file:write-text-lines($file as xs:string,
//        $contents as xs:string*,
//        $encoding as xs:string)
//        as empty-sequence()

    public static void writeTextLines(One<StringItem> path, ZeroOrMore<StringItem> contents, One<StringItem> encoding) throws XPathException {
        File file = null;
        try {
            file = checkWriteDestination(path.head().getStringValue());
            FileOutputStream stream = new FileOutputStream(file);
            Writer writer = new OutputStreamWriter(stream, encoding.head().getStringValue());
            UnfailingIterator ui = contents.iterate();
            Item i = ui.next();
            while (i != null) {
                writer.write(i.getUnicodeStringValue() + NEWLINE);
                i = ui.next();
            }
            writer.close();
        } catch (UnsupportedEncodingException e) {
            error("Unsupported encoding " + encoding, ERROR_UNKNOWN_ENCODING);
        } catch (FileNotFoundException e) {
            error("Failed to create file", e, ERROR_PATH_NOT_DIRECTORY);
        } catch (IOException e) {
            error("Failed to write to file " + file.getAbsolutePath(), e, ERROR_IO);
        }
    }


//////////////////////////////////////////////////////////////////////////////////////////////////////

//    5.1 file:name($path as xs:string)
//        as xs:string
    /*
    The semantics of file:name() in the spec are pretty illogical and somewhat
     at odds with the Version 1.0 spec. In particular the 'absence of directory separators'
     is somewhat quirky.
     */

    public static One<StringItem> name(String path) throws XPathException {
        if (path.equals("/") || path.equals("")) {  // || !path.contains(File.separator)
            return One.string("");
        }
        File file = toFile(path);
        return One.string(file.getName());
    }

//    5.2 file:parent($path as xs:string)
//        as xs:string?

    public static ZeroOrOne<StringItem> parent(String path) throws XPathException {
        String p = _parent(path);
        return new ZeroOrOne<StringItem>(p == null ? null : new StringItem(p));
    }

    public static String _parent(String path) throws XPathException {
        if (path.equals("/")) {
            return null;
        }
        File file = toFile(path);
        try {
            File parent = new File(file.getParent());
            return parent.getCanonicalPath() + File.separatorChar;
        } catch (IOException e) {
            //
        }
        return null;
    }

//    5.3 file:children($dir as xs:string)
//        as xs:string*

    public static ZeroOrMore<StringItem> children(One<StringItem> dir) throws XPathException {
        List<StringItem> result = new ArrayList<StringItem>();
        File fd = toExistingDirectory(dir.head().getStringValue());
        if (!fd.isDirectory()) {
            error("File " + dir + " is not a directory", ERROR_PATH_NOT_DIRECTORY);
        }
        for (File file : fd.listFiles()){
             result.add(new StringItem(file.getAbsolutePath()+(file.isDirectory()?File.separator:"")));
        }
        return new ZeroOrMore(result);
    }
/*

//    4.1 file:base-name($path as xs:string)
//        as xs:string

    public static String baseName(String path) {
        if (path.length() == 0) {
            return ".";
        }
        File file = toFile(path);
        return file.getName();
    }
//
//    file:base-name($path as xs:string,
//        $suffix as xs:string)
//        as xs:string

    public static String baseName(String path, String suffix) {
        File file = toFile(path);
        String name = file.getName();
        // TODO: spec slightly unclear here as to how the "." is handled
        if (name.endsWith(suffix)) {
            return name.substring(0, name.length() - suffix.length());
        } else {
            return name;
        }
    }

//    4.2 file:dir-name($path as xs:string)
//        as xs:string

    public static String dirName(String path) throws XPathException, IOException {
        File f = new File(path);
        if (path.startsWith("file:/")) {
            try {
                f = new File(new URI(path));
            } catch (URISyntaxException e) {
                e.printStackTrace();  //To change body of catch statement use File | Settings | File Templates.
            }
            System.err.println("FILE:" + path + " " + f.getAbsolutePath());
        }
        String pathNative = f.getCanonicalPath();
        // The rules in the spec/tests are so obtuse it's easier to just enumerate the short cases.
        if (path.equals("/")) {
            return "" + File.separatorChar;
        }
        if (path.length() == 0 || path.equals(".") || path.equals("..")
                || (pathNative.indexOf(File.separatorChar) < 0)) {
            // Many of these are totally illogical, especially ".."
            return "." + File.separatorChar;
        }
        File file = toFile(path).getParentFile();
        return (file == null ? "" : file.getName() + File.separatorChar);
    }
*/

//    4.3 file:path-to-native($path as xs:string)
//        as xs:string

    public static One<StringItem> pathToNative(One<StringItem> path) throws XPathException {
        File file = toFile(path.head().getStringValue());

        try {
            if (!file.exists()) { // Version 1.0 - the path must exist.
                error("Path does not exist:" + file.getAbsolutePath(), ERROR_PATH_NOT_EXIST);
            }
            return One.string(file.getCanonicalPath());
        } catch (IOException e) {
            error("Failed to get canonical path", e, ERROR_IO);
        }
        return null;
    }

//    4.4 file:path-to-uri($path as xs:string)
//        as xs:anyURI

    public static One<AnyURIValue> pathToUri(One<StringItem> path) throws XPathException {
        String filePath = path.head().getStringValue();
        URI uri = null;
        if (File.separator.equals("\\") && (filePath.startsWith("\\\\") || filePath.startsWith("//"))) {
            // See bug 3745. This is specifically to handle UNC paths, working around
            // https://bugs.java.com/view_bug.do?bug_id=4723726.
            uri = new File(filePath).toPath().toUri();
        } else {
            uri = toFile(filePath).toURI().normalize();
        }
        return new One<AnyURIValue>(new AnyURIValue(uri.toString()));
    }

//    5.6 file:resolve-path($path as xs:string)
//        as xs:string

    public static One<StringItem> resolvePath(One<StringItem> path) throws XPathException {
        File file = toFile(path.head().getStringValue());
        return One.string(file.getAbsolutePath() + (file.isDirectory() ? File.separator : ""));
    }


    //////////////////////////////////////////////////////////////////////////////////////////////////////

//  6.1 file:dir-separator() as xs:string

    public static One<StringItem> dirSeparator() {
        return One.string(File.separator);
    }


//  6.2 file:line-separator() as xs:string

    public static One<StringItem> lineSeparator() {
        return One.string(NEWLINE);
    }

//  6.3 file:path-separator() as xs:string

    public static One<StringItem> pathSeparator() {
        return One.string(File.pathSeparator);
    }

    //   6.4 file:temp-dir()
//          as xs:string
    public static One<StringItem> tempDir() {
        return One.string(System.getProperty("java.io.tmpdir"));
    }

//   6.5 file:base-dir()
//        as xs:string
    /*
    The code for file:base-dir() is in EXPathFileBaseDir, as it requires special treatment for static context
     */

   /* public static ZeroOrOne<StringValue> baseDir2(XPathContext context) throws XPathException {
       URI baseURI = null;
        //RetainedStaticContext rsc = getRetainedStaticContext();
        Component currentComponent = context.getCurrentComponent();
        baseURI = currentComponent.getCode().getRetainedStaticContext().getStaticBaseUri();
        if (baseURI != null) {
            File file = toFile(".");
            return One.string(file.getAbsolutePath() + (file.isDirectory() ? File.separator : ""));
        } else {
            return new ZeroOrOne<StringValue>(null);
        }
    }*/
//   6.6 file:current-dir()
//        as xs:string

    public static One<StringItem> currentDir() throws XPathException {
        File file = toFile(".");
        return One.string(file.getAbsolutePath() + (file.isDirectory() ? File.separator : ""));
    }

    //////////////////////////////////////////////////////////////////////////////////////////////////////////

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
                URI uri = new URI(path.replaceAll("\\\\", "/"));
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
            e.printStackTrace();
            throw new XPathException("URISyntax error in file path:" + path);
        } catch (IOException e) {
            e.printStackTrace();
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
            error("No file exists at " + path, ERROR_PATH_NOT_EXIST);
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
            error("No directory exists at " + path, ERROR_PATH_NOT_DIRECTORY);
        }
        return file;
    }

    /**
     * Throw an error
     *
     * @param message the error message
     * @param code    the error code to be used
     * @throws XPathException always
     */

    public static void error(String message, String code) throws XPathException {
        XPathException e = new XPathException(message);
        e.setErrorCodeQName(new StructuredQName(ERROR_PREFIX, ERROR_NAMESPACE, code));
        throw e;
    }

    /**
     * Throw an error
     *
     * @param message the error message
     * @param cause   the underlying exception, usually an IOException
     * @param code    the error code to be used
     * @throws XPathException always
     */

    public static void error(String message, Exception cause, String code) throws XPathException {
        XPathException e = new XPathException(message, cause);
        e.setErrorCodeQName(new StructuredQName(ERROR_PREFIX, ERROR_NAMESPACE, code));
        throw e;
    }

}

// Copyright (c) 2014-2020 Saxonica Limited

