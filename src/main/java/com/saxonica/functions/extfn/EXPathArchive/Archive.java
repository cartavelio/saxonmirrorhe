package com.saxonica.functions.extfn.EXPathArchive;

import net.sf.saxon.event.Outputter;
import net.sf.saxon.event.ReceiverOption;
import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.expr.parser.Loc;
import net.sf.saxon.ma.map.HashTrieMap;
import net.sf.saxon.ma.map.MapItem;
import net.sf.saxon.om.*;
import net.sf.saxon.s9api.QName;
import net.sf.saxon.sapling.SaplingElement;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.tree.iter.AtomicIterator;
import net.sf.saxon.type.BuiltInAtomicType;
import net.sf.saxon.type.Untyped;
import net.sf.saxon.value.*;

import java.io.ByteArrayInputStream;
import java.io.ByteArrayOutputStream;
import java.io.IOException;
import java.math.BigDecimal;
import java.net.URISyntaxException;
import java.nio.ByteBuffer;
import java.nio.CharBuffer;
import java.nio.charset.*;
import java.util.ArrayList;
import java.util.HashMap;
import java.util.List;
import java.util.zip.*;

/**
 * Implementation of extension functions in the EXPath Archive package, see
 * http://expath.org/spec/archive
 */

@SuppressWarnings("unused")
public class Archive {
    public static final BigDecimalValue VERSION = new BigDecimalValue(new BigDecimal("0.1"));

    public static final NamespaceUri NAMESPACE = NamespaceUri.of("http://expath.org/ns/archive");
    public static final String PREFIX = "arch";
    public static final NamespaceUri ERROR_NAMESPACE = NAMESPACE;
    public static final String ERROR_PREFIX = PREFIX;

    /* Generate information attached to elements, rather than as attributes */
    private static final boolean USE_ELEMENTS = false;

    private static final String ERROR_UNKNOWN_ENTRY = "unknown-entry";
    private static final String ERROR_ENTRY_DATA_MISMATCH = "entry-data-mismatch";
    private static final String ERROR_UNKNOWN_ENCODING = "unknown-encoding";
    private static final String ERROR_DECODING = "decoding-error";
    private static final String ERROR_OPTION = "option-error";


    // arch:version()
    // as xs:decimal
    public static BigDecimalValue version() {
        return VERSION;
    }

    /**
     * Throw an error
     *
     * @param message the error message
     * @param code    the error code to be used
     * @throws net.sf.saxon.trans.XPathException always
     */

    private static void error(String message, String code)
            throws XPathException {
        XPathException e = new XPathException(message);
        e.setErrorCodeQName(new StructuredQName(ERROR_PREFIX, ERROR_NAMESPACE, code));
        throw e;
    }

    public enum ArchiveType {
        UNKNOWN, ZIP, GZIP
    }

    private static final HashMap<ArchiveType, String> archiveTypes = new HashMap<ArchiveType, String>() {
        {
            put(ArchiveType.ZIP, "zip");
            put(ArchiveType.GZIP, "gzip");
            put(ArchiveType.UNKNOWN, "unknown");
        }
    };

    public enum CompressionType {
        STORED, DEFLATE, UNKNOWN, MIXED
    }

    private static final HashMap<CompressionType, String> compressionTypes = new HashMap<CompressionType, String>() {
        {
            put(CompressionType.STORED, "stored");
            put(CompressionType.DEFLATE, "deflate");
            put(CompressionType.UNKNOWN, "unknown");
            put(CompressionType.MIXED, "mixed");
        }
    };

    private static CompressionType stringToCompressionType(String s) {
        for (CompressionType c : compressionTypes.keySet()) {
            if (compressionTypes.get(c).equals(s)) {
                return c;
            }
        }
        return null;
    }

    private static CompressionType zipCompressions(int c) {
        if (c == ZipEntry.DEFLATED) {
            return CompressionType.DEFLATE;
        }
        if (c == ZipEntry.STORED) {
            return CompressionType.STORED;
        }
        return CompressionType.UNKNOWN;
    }


    /**
     * Generate an xs:base64Binary result from the byte sequence
     *
     * @param result - byte sequence
     * @return suitably encapsulated Base64BinaryValue
     */
    private static One<Base64BinaryValue> one(byte[] result) {
        return new One<>(new Base64BinaryValue(result));
    }

    /**
     * Create a no-namespace QName
     *
     * @param s the local name
     * @return a no-namespace NodeName for the given name
     */

    private static NodeName qname(String s) {
        return new NoNamespaceName(s);
    }

    /**
     * Create a QName in the http://expath.org/ns/archive namespace
     *
     * @param s the local name
     * @return a namespaced NodeName for the given name
     */

    private static QName qnameNS(String s) {
        return new QName(PREFIX, NAMESPACE.toString(), s);
    }

    private static final HashMap<String, CharsetDecoder> decoders = new HashMap<>();

    static {
        CharsetDecoder decoder = StandardCharsets.UTF_8.newDecoder();
        decoder.onMalformedInput(CodingErrorAction.REPORT);
        decoders.put("UTF-8", decoder);

        decoder = StandardCharsets.UTF_16.newDecoder();
        decoder.onMalformedInput(CodingErrorAction.REPORT);
        decoders.put("UTF-16", decoder);

        decoder = StandardCharsets.US_ASCII.newDecoder();
        decoder.onMalformedInput(CodingErrorAction.REPORT);
        decoders.put("US-ASCII", decoder);

        decoder = StandardCharsets.US_ASCII.newDecoder();
        decoder.onMalformedInput(CodingErrorAction.REPORT);
        decoders.put("ASCII", decoder);

        decoder = StandardCharsets.ISO_8859_1.newDecoder();
        decoder.onMalformedInput(CodingErrorAction.REPORT);
        decoders.put("ISO-8859-1", decoder);
    }

    /**
     * Create a string-valued attribute
     *
     * @param r     the receiver to which the attribute is written
     * @param name  the name of the attribute
     * @param value the value of the attribute
     * @throws XPathException if any failure occurs
     */

    private static void attribute(Outputter r, NodeName name,
                                  String value) throws XPathException {
        r.attribute(name, BuiltInAtomicType.UNTYPED_ATOMIC, value,
                    Loc.NONE, ReceiverOption.NONE);
    }

    /**
     * Create an integer-valued attribute
     *
     * @param r     the receiver to which the attribute is written
     * @param name  the name of the attribute
     * @param value the value of the attribute
     * @throws XPathException if any failure occurs
     */

    private static void attribute(Outputter r, NodeName name, long value)
            throws XPathException {
        r.attribute(name, BuiltInAtomicType.UNTYPED_ATOMIC, Long.valueOf(value).toString(),
                    Loc.NONE, ReceiverOption.NONE);
    }

    /**
     * Write an element start tag
     *
     * @param r    the receiver to which the attribute is written
     * @param name the name of the element
     * @throws XPathException if any failure occurs
     */

    private static void startElement(Outputter r, NodeName name)
            throws XPathException {
        r.startElement(name, Untyped.getInstance(), Loc.NONE, ReceiverOption.NONE);
    }

    private static ArrayList<Entry> toExtractionEntries(Sequence entries) {
        return toExtractionEntries(entries, null);
    }

    /**
     * Generate a list of extraction entries, from a sequence either of string
     * names or a map
     *
     * @param entries      Required entries as a sequence
     * @param entryDefault A default entry to copy properties such as encoding. Could be
     *                     null
     * @return The list of entries required, each having a name recorded
     * internally
     */
    private static ArrayList<Entry> toExtractionEntries(Sequence entries, Entry entryDefault) {
        ArrayList<Entry> requested = new ArrayList<>();
        if (entries == null) {
            return requested;
        }
        SequenceIterator iter = entries.iterate();
        Item item;
        while ((item = iter.next()) != null) {
            if (item instanceof MapItem) {
                AtomicIterator ui = ((MapItem) item).keys();
                AtomicValue av;
                while ((av = ui.next()) != null) {
                    requested.add(new Entry(entryDefault, av.getStringValue()));
                }
            } else {
                requested.add(new Entry(entryDefault, item.getStringValue()));
            }

        }
        return requested;
    }

    private static ArrayList<Entry> toInsertionEntries(ZeroOrMore<StringValue> entries, ZeroOrMore<Base64BinaryValue> values)
            throws XPathException {
        ArrayList<Entry> m = new ArrayList<>();
        SequenceIterator entryIter = entries.iterate();
        SequenceIterator valueIter = values.iterate();
        while (true) {
            StringValue entry = (StringValue) entryIter.next();
            Base64BinaryValue value = (Base64BinaryValue) valueIter.next();
            if ((entry == null) != (value == null)) {
                error("Entry name and value sequences have different lengths", ERROR_ENTRY_DATA_MISMATCH);
            }
            if (entry == null) {
                break;
            }
            m.add(new Entry(entry.getStringValue(), value));
        }
        return m;
    }

    private static ArrayList<Entry> toInsertionEntries(MapItem entries, Entry defaultEntry) throws XPathException {
        AtomicIterator ui = entries.keys();
        ArrayList<Entry> result = new ArrayList<>();
        Entry first = null;
        Sequence s;
        AtomicValue av;
        StringValue content = StringValue.bmp("content");
        StringValue compressionKey = StringValue.bmp("compression");
        StringValue positionKey = StringValue.bmp("position");
        while ((av = ui.next()) != null) {
            MapItem entry = (MapItem) entries.get(av).iterate().next();
            Entry e = new Entry(av.getStringValue(),
                                (Base64BinaryValue) entry.get(content).iterate().next());
            e.setCompression(defaultEntry.compression);
            if ((s = entry.get(compressionKey)) != null) {
                String compressionValue = s.iterate().next().getStringValue();
                e.setCompression(stringToCompressionType(compressionValue));
            }
            if ((s = entry.get(positionKey)) != null) {
                long position;
                try {
                    position = ((Int64Value) s.iterate().next()).longValue();
                } catch (Exception ex) {
                    error("The value of 'position' property must be an integer", ERROR_OPTION);
                }
                if (first != null) {
                    error("Only one entry can be declared as position:1", ERROR_OPTION);
                }
                first = e;
            } else {
                result.add(e);
            }
        }
        if (first != null) {
            result.add(0, first);
        }
        return result;
    }

    public static ArchiveType archiveType(Base64BinaryValue in)
            throws XPathException {
        byte[] supplied = in.getBinaryValue();

        byte[] zipID = new byte[]{0x50, 0x4b, 0x3, 0x4};
        if (arraysEqual(supplied, 0, 4, zipID, 0, 4)) {
            return ArchiveType.ZIP;
        }

        byte[] gzipID = new byte[]{0x1f, (byte) 0x8b};
        if (arraysEqual(supplied, 0, 2, gzipID, 0, 2)) {
            return ArchiveType.GZIP;
        }

        return ArchiveType.UNKNOWN;
    }

    @SuppressWarnings("SameParameterValue")
    private static boolean arraysEqual(byte[] a, int aFrom, int aTo, byte[] b, int bFrom, int bTo) {
        // TODO: from JDK 9, use Arrays.equal(a, aFrom, aTo, b, bFrom, bTo)
        if (aTo - aFrom != bTo - bFrom) {
            return false;
        }
        for (int i=aFrom, j=bFrom; i<aTo; ) {
            if (a[i++] != b[j++]) {
                return false;
            }
        }
        return true;
    }


    public static CompressionType compressionType(Base64BinaryValue in)
            throws XPathException {
        return CompressionType.UNKNOWN;
    }

    /* Helper functions to add to immutable maps */
    private static HashTrieMap add(HashTrieMap map, String key, HashTrieMap val) {
        return map.addEntry(new StringValue(key), val);
    }

    /* the following can be used externally */
    public static HashTrieMap add(HashTrieMap map, String key, AtomicValue val) throws XPathException {
        return map.addEntry(new StringValue(key), val);
    }

    public static HashTrieMap add(HashTrieMap map, String key, String val) throws XPathException {
        return add(map, key, new StringValue(val));
    }

    public static HashTrieMap add(HashTrieMap map, String key, long val) throws XPathException {
        return add(map, key, new Int64Value(val));
    }

    // Returns entries in archive order
    private static ArrayList<ZipEntry> getEntries(ZipInputStream in) throws IOException {
        ArrayList<ZipEntry> entries = new ArrayList<>();
        ZipEntry next;
        while ((next = in.getNextEntry()) != null) {
            entries.add(next);
            in.closeEntry();
        }
        in.close();
        return entries;
    }

    private static CompressionType compressionMethods(Base64BinaryValue in) throws IOException {
        CompressionType ct = CompressionType.UNKNOWN;
        for (ZipEntry ze : getEntries(new ZipInputStream(
                new ByteArrayInputStream(in.getBinaryValue())))) {
            CompressionType t = zipCompressions(ze.getMethod());
            if (ct == CompressionType.UNKNOWN) {
                ct = t;
            } else if (ct != t) {
                return CompressionType.MIXED;
            }
        }
        return ct;
    }

    // arch:entry-names($in as xs:base64Binary) as xs:string*
    public static ZeroOrMore<? extends StringValue> entryNames(Base64BinaryValue in)
            throws IOException, XPathException, URISyntaxException {
        List<StringValue> names = new ArrayList<>();
        final ZipInputStream stream = new ZipInputStream(new ByteArrayInputStream(in.getBinaryValue()));
        for (ZipEntry ze : getEntries(stream)) {
            names.add(new StringValue(ze.getName()));
        }
        return new ZeroOrMore<>(names);
    }

    // arch:entries($in as xs:base64Binary) as element(arch:entry)*
    public static ArrayList<NodeInfo> entries(XPathContext context, Base64BinaryValue in)
            throws IOException, XPathException, URISyntaxException {

        QName archEntry = qnameNS("entry");
        QName archDir = qnameNS("dir");

        ZipInputStream zi = new ZipInputStream(new ByteArrayInputStream(in.getBinaryValue()));
        ArrayList<ZipEntry> entries = getEntries(zi);
        ArrayList<NodeInfo> results = new ArrayList<>();
        long val;
        for (ZipEntry ze : entries) {
            SaplingElement elem = new SaplingElement(ze.isDirectory() ? archDir : archEntry);
            if ((val = ze.getSize()) != -1) {
                elem = elem.withAttr("size", ""+val);
            }
            if ((val = ze.getCompressedSize()) != -1) {
                elem = elem.withAttr("compressed-size", val+"");
            }
            if ((val = ze.getTime()) != -1) {
                elem = elem.withAttr("last-modified", DateTimeValue.fromJavaTime(val).getStringValue());
            }
            elem = elem.withAttr("compression", compressionTypes.get(zipCompressions(ze.getMethod())));
            elem = elem.withText(ze.getName());
            results.add(elem.toNodeInfo(context.getConfiguration()));
        }
        return results;
    }

    // archM:entries($in as xs:base64Binary) as map(xs:string, map(xs:string,item()))
    public static MapItem entriesMap(XPathContext context, Base64BinaryValue in)
            throws IOException, XPathException {
        return entriesMap(context, in, BooleanValue.FALSE);
    }

    // archM:entries($in as xs:base64Binary, $return-content as xs:boolean) as map(xs:string, map(xs:string,item()))
    public static MapItem entriesMap(XPathContext context, Base64BinaryValue in, BooleanValue returnContent)
            throws IOException, XPathException {
        HashTrieMap result = new HashTrieMap();
        ZipInputStream zi = new ZipInputStream(new ByteArrayInputStream(
                in.getBinaryValue()));
        byte[] buffer = new byte[4096];
        ByteArrayOutputStream output = new ByteArrayOutputStream();
        ZipEntry next;
        long val;
        int position = 1;
        while ((next = zi.getNextEntry()) != null) {

            HashTrieMap properties = new HashTrieMap();
            properties = add(properties, "position", position++);
            if ((val = next.getSize()) != -1) {
                properties = add(properties, "size", val);
            }
            if ((val = next.getCompressedSize()) != -1) {
                properties = add(properties, "compressed-size", val);
            }
            if ((val = next.getTime()) != -1) {
                properties = add(properties, "last-modified", DateTimeValue.fromJavaTime(val)
                        .getStringValue());
            }
            properties = add(properties, "compression", compressionTypes.get(zipCompressions(next.getMethod())));
            if (returnContent.getBooleanValue()) {
                int len;
                while ((len = zi.read(buffer)) > 0) {
                    output.write(buffer, 0, len);
                }
                properties = add(properties, "content", new Base64BinaryValue(
                        output.toByteArray()));
                output.reset();
            }
            result = add(result, next.getName(), properties);
            zi.closeEntry();
        }
        zi.close();
        return result;
    }

    // archM:options($in as xs:base64Binary) as map(xs:string,item())
    public static MapItem optionsMap(XPathContext context, Base64BinaryValue in)
            throws IOException, XPathException {
        HashTrieMap result = new HashTrieMap();
        result = add(result, "format", archiveTypes.get(archiveType(in)));
        result = add(result, "compression", compressionTypes.get(compressionMethods(in)));
        return result;
    }

    // arch:options($in as xs:base64Binary) as element(arch:options)*
    public static NodeInfo options(XPathContext context, Base64BinaryValue in)
            throws XPathException {
        SaplingElement elem = new SaplingElement(qnameNS("options"));
        if (USE_ELEMENTS) {
            elem = elem.withChild(
                new SaplingElement(qnameNS("format")).withAttr("value", archiveTypes.get(archiveType(in))),
                new SaplingElement(qnameNS("compression")).withAttr("value", compressionTypes.get(compressionType(in))));

        } else {
            elem = elem
                    .withAttr("format", archiveTypes.get(archiveType(in)))
                    .withAttr("compression", compressionTypes.get(compressionType(in)));
        }
        return elem.toNodeInfo(context.getConfiguration());
    }

    /*
     * Reports on any remaining entries in an entry list - normally from unfound
     * conditions
     */
    private static void checkUnfound(ArrayList<Entry> entries)
            throws XPathException {
        if (!entries.isEmpty()) {
            StringBuilder e = new StringBuilder("No entr").append(entries.size() == 1 ? "y" : "ies");
            for (int i = 0; i < entries.size(); i++) {
                e.append(" ").append(entries.get(i).name);
                if (i != entries.size() - 1) {
                    e.append(",");
                }
            }
            e.append(" found in archive");
            error(e.toString(), ERROR_UNKNOWN_ENTRY);
        }
    }

    private static ArrayList<Entry> extractEntries(Base64BinaryValue in, ArrayList<Entry> entries)
            throws IOException, XPathException {
        ArrayList<Entry> requested = (ArrayList<Entry>) entries.clone();
        ArrayList<Entry> result = new ArrayList<>();
        byte[] buffer = new byte[4096];
        ByteArrayOutputStream output = new ByteArrayOutputStream();
        ZipInputStream zi = new ZipInputStream(new ByteArrayInputStream(
                in.getBinaryValue()));
        ZipEntry next;
        HashMap<String, Entry> found = new HashMap<>();
        while ((next = zi.getNextEntry()) != null && !entries.isEmpty()) {
            String name = next.getName();
            Entry e = new Entry(name);
            if (entries.contains(e)) {
                int len;
                while ((len = zi.read(buffer)) > 0) {
                    output.write(buffer, 0, len);
                }
                found.put(name, new Entry(entries.get(entries.indexOf(e)),
                        new Base64BinaryValue(output.toByteArray())));
                //result.add(new Entry(entries.get(entries.indexOf(e)),
                //		new Base64BinaryValue(output.toByteArray())));
                output.reset();
                while (entries.remove(e)) {
                    // no action
                }
            }
            zi.closeEntry();
        }
        zi.close();
        checkUnfound(entries);
        for (Entry e : requested) {
            result.add(found.get(e.name));
        }
        return result;
    }

    // arch:extract-binary($in as xs:base64Binary, $entries as xs:string*) as xs:base64Binary*
    public static ZeroOrMore<Base64BinaryValue> extractBinary(One<Base64BinaryValue> in, ZeroOrMore<StringValue> entries) throws IOException,
            XPathException {
        List<Base64BinaryValue> result = new ArrayList<>();
        for (Entry e : extractEntries(in.head(), toExtractionEntries(entries))) {
            result.add(e.content);
        }
        return new ZeroOrMore<>(result);
    }

    // arch:extract-binary-map($in as xs:base64Binary, $entries as map(xs:string,item())) as xs:base64Binary*
    public static ZeroOrMore<Base64BinaryValue> extractBinaryMap(One<Base64BinaryValue> in, One<MapItem> entries) throws IOException,
            XPathException {
        List<Base64BinaryValue> result = new ArrayList<>();
        for (Entry e : extractEntries(in.head(), toExtractionEntries(entries.head()))) {
            result.add(e.content);
        }
        return new ZeroOrMore<>(result);
    }

    // arch:extract-text($in as xs:base64Binary, $entries as xs:string*) as xs:string*
    public static ZeroOrMore<? extends StringValue> extractText(One<Base64BinaryValue> in, ZeroOrMore<StringValue> entries) throws IOException,
            XPathException, URISyntaxException {
        return extractText(in, entries, new One<>(StringValue.bmp("UTF-8")));
    }

    // arch:extract-text($in as xs:base64Binary, $entries as xs:string*, $encoding as xs:string) as xs:string*
    public static ZeroOrMore<? extends StringValue> extractText(
            One<Base64BinaryValue> in, ZeroOrMore<StringValue> entries, One<StringValue> encoding)
            throws IOException, XPathException, URISyntaxException {
        Entry def = new Entry();
        def.setEncoding(encoding.head().getStringValue().toUpperCase());
        return extractText(in.head(), toExtractionEntries(entries, def));
    }

    private static ZeroOrMore<? extends StringValue> extractText(Base64BinaryValue in, ArrayList<Entry> entries)
            throws IOException, XPathException {
        CharsetDecoder decoder;

        List<StringValue> result = new ArrayList<>();
        for (Entry b : extractEntries(in, entries)) {
            decoder = decoders.get(b.encoding);
            if (decoder == null) {
                error("Unsupported encoding in text encode/decode:"
                        + b.encoding, ERROR_UNKNOWN_ENCODING);
            }
            decoder.reset();
            byte[] bytes = b.content.getBinaryValue();
            ByteBuffer bb = ByteBuffer.wrap(bytes);
            char[] outChars = new char[bytes.length];
            CharBuffer out = CharBuffer.wrap(outChars);
            CoderResult res = decoder.decode(bb, out, true);
            if (res.isError()) {
                if (res.isMalformed()) {
                    error("Malformed input in decoding", ERROR_DECODING);
                }
                if (res.isUnmappable()) {
                    error("Unmappable input in decoding", ERROR_DECODING);
                }
                error("Other error in decoding", ERROR_DECODING);
            }
            char[] resChars = new char[out.position()];
            System.arraycopy(outChars, 0, resChars, 0, out.position());
            result.add(new StringValue(new String(resChars)));
        }
        return new ZeroOrMore<>(result);
    }

    // arch:extract-map($in as xs:base64Binary, map(xs:string,map(xs:string,item()))) as xs:anyAtomicType*
    public static AtomicValue[] extractMap(One<Base64BinaryValue> in, One<MapItem> entries) throws IOException,
            XPathException {
        return _extractMap(in.head(), toExtractionEntries(entries.head()));
    }

    // This might be able to subsume extractText()
    private static AtomicValue[] _extractMap(Base64BinaryValue in,
                                           ArrayList<Entry> entries) throws IOException, XPathException {
        CharsetDecoder decoder;

        ArrayList<AtomicValue> result = new ArrayList<>();
        for (Entry b : extractEntries(in, entries)) {
            if (b.encoding != null) {
                decoder = decoders.get(b.encoding);
                if (decoder == null) {
                    error("Unsupported encoding in text encode/decode:"
                            + b.encoding, ERROR_UNKNOWN_ENCODING);
                }
                decoder.reset();
                byte[] bytes = b.content.getBinaryValue();
                ByteBuffer bb = ByteBuffer.wrap(bytes);
                char[] outChars = new char[bytes.length];
                CharBuffer out = CharBuffer.wrap(outChars);
                CoderResult res = decoder.decode(bb, out, true);
                if (res.isError()) {
                    if (res.isMalformed()) {
                        error("Malformed input in decoding", ERROR_DECODING);
                    }
                    if (res.isUnmappable()) {
                        error("Unmappable input in decoding", ERROR_DECODING);
                    }
                    error("Other error in decoding", ERROR_DECODING);
                }
                char[] resChars = new char[out.position()];
                System.arraycopy(outChars, 0, resChars, 0, out.position());
                result.add(new StringValue(new String(resChars)));
            } else {
                result.add(b.content);
            }
        }
        return result.toArray(new AtomicValue[0]);
    }

    // arch:extract-text-map($in as xs:base64Binary, map(xs:string,item())) as xs:string*
    public static ZeroOrMore<? extends StringValue> extractTextMap(One<Base64BinaryValue> in, One<MapItem> entries) throws IOException,
            XPathException {
        return extractTextMap(in, entries, One.string("UTF-8"));
    }

    // arch:extract-text-map($in as xs:base64Binary, map(xs:string,item()), $encoding as xs:string) as xs:string*
    public static ZeroOrMore<? extends StringValue> extractTextMap(One<Base64BinaryValue> in, One<MapItem> entries, One<StringValue> encoding)
            throws IOException, XPathException {
        Entry def = new Entry();
        def.setEncoding(encoding.head().getStringValue().toUpperCase());
        return extractText(in.head(), toExtractionEntries(entries, def));
    }

    // arch:delete($in as xs:base64Binary, $entries as xs:string*)
    // as xs:base64Binary
    public static One<Base64BinaryValue> delete(One<Base64BinaryValue> in, ZeroOrMore<StringValue> entries) throws IOException,
            XPathException {
        if (entries.head() == null) {
            return in;
        }
        return delete(in, toExtractionEntries(entries));
    }

    // arch:delete-map($in as xs:base64Binary, $entries as map(xs:string,item())) as xs:base64Binary
    public static One<Base64BinaryValue> deleteMap(One<Base64BinaryValue> in, One<MapItem> entries) throws IOException,
            XPathException {
        if (entries.head() == null) {
            return in;
        }
        return delete(in, toExtractionEntries(entries));
    }

    private static One<Base64BinaryValue> delete(One<Base64BinaryValue> in,
                                                 ArrayList<Entry> entries) throws IOException, XPathException {
        byte[] buffer = new byte[4096];
        ByteArrayOutputStream output = new ByteArrayOutputStream();
        ByteArrayOutputStream full = new ByteArrayOutputStream();
        ZipOutputStream zo = new ZipOutputStream(full);
        ZipInputStream zi = new ZipInputStream(new ByteArrayInputStream(
                in.head().getBinaryValue()));
        ZipEntry next;
        while ((next = zi.getNextEntry()) != null) {
            String name = next.getName();
            Entry e = new Entry(name);
            if (!entries.contains(e)) {
                int len;
                while ((len = zi.read(buffer)) > 0) {
                    output.write(buffer, 0, len);
                }
                ZipEntry nze = new ZipEntry(next.getName());
                nze.setMethod(next.getMethod());
                nze.setSize(next.getSize());
                nze.setCrc(next.getCrc());
                nze.setTime(next.getTime());
                zo.putNextEntry(nze);
                zo.write(output.toByteArray());
                zo.closeEntry();
                output.reset();
            } else {
                entries.remove(e);
            }
            zi.closeEntry();
        }
        zi.close();
        try {
            zo.close();
        } catch (ZipException e) {
            if (!e.getMessage().equals("ZIP file must have at least one entry")) {
                // This failure occurs on a couple of delete tests when using JDK 1.6. It doesn't occur on JDK 1.8,
                // and appears to be ignorable. See bug 2542.
                throw e;
            }
        }
        checkUnfound(entries);
        return one(full.toByteArray());
    }

    // arch:update($in as xs:base64Binary, $entries as xs:string*, $new as xs:base64Binary*) as xs:base64Binary
    public static Base64BinaryValue update(One<Base64BinaryValue> in, ZeroOrMore<StringValue> entries, ZeroOrMore<Base64BinaryValue> values)
            throws IOException, XPathException {
        return updateFromEntries(in.head(), toInsertionEntries(entries, values));
    }

    // archM:update($in as xs:base64Binary, $entries as map(xs:string,map(xs:string,item()))) as xs:base64Binary
    public static Base64BinaryValue updateMap(One<Base64BinaryValue> in, One<MapItem> entries) throws IOException,
            XPathException {
        return updateFromEntries(in.head(), toInsertionEntries(entries.head(), new Entry()));
    }

    private static Base64BinaryValue updateFromEntries(Base64BinaryValue in,
                                                       ArrayList<Entry> entries) throws IOException {
        byte[] buffer = new byte[4096];
        ByteArrayOutputStream output = new ByteArrayOutputStream();
        ByteArrayOutputStream full = new ByteArrayOutputStream();
        ZipOutputStream zo = new ZipOutputStream(full);
        ZipInputStream zi = new ZipInputStream(new ByteArrayInputStream(
                in.getBinaryValue()));
        ZipEntry next;
        ZipEntry nze;
        CRC32 crc = new CRC32();
        while ((next = zi.getNextEntry()) != null) {
            String name = next.getName();
            Entry e = new Entry(name);
            nze = new ZipEntry(name);
            nze.setMethod(next.getMethod());
            if (entries.contains(e)) {
                Base64BinaryValue b = entries.get(entries.indexOf(e)).content;
                nze.setSize(b.getLengthInOctets());
                crc.reset();
                crc.update(b.getBinaryValue());
                nze.setCrc(crc.getValue());
                zo.putNextEntry(nze);
                zo.write(b.getBinaryValue());
                zo.closeEntry();
                entries.remove(e);
            } else {
                int len;
                while ((len = zi.read(buffer)) > 0) {
                    output.write(buffer, 0, len);
                }
                nze.setSize(next.getSize());
                nze.setCrc(next.getCrc());
                nze.setTime(next.getTime());
                zo.putNextEntry(nze);
                zo.write(output.toByteArray());
                zo.closeEntry();
                output.reset();
            }
            zi.closeEntry();
        }
        zi.close();
        for (Entry e : entries) {
            nze = new ZipEntry(e.name);
            Base64BinaryValue b = e.content;
            nze.setSize(b.getLengthInOctets());
            crc.reset();
            crc.update(b.getBinaryValue());
            nze.setCrc(crc.getValue());
            zo.putNextEntry(nze);
            zo.write(b.getBinaryValue());
            zo.closeEntry();
        }
        zo.close();
        return new Base64BinaryValue(full.toByteArray());
    }

    // arch:create($entries as xs:string*, $new as xs:base64Binary*) as xs:base64Binary
    public static Base64BinaryValue create(ZeroOrMore<StringValue> entries, ZeroOrMore<Base64BinaryValue> values)
            throws IOException, XPathException {
        return createFromEntries(toInsertionEntries(entries, values));
    }

    // archM:create($entries as map(xs:string,map(xs:string,item())))
    // as xs:base64Binary
    public static Base64BinaryValue createMap(One<MapItem> entries) throws IOException, XPathException {
        return createFromEntries(toInsertionEntries(entries.head(), new Entry()));
    }

    // archM:create($entries as map(xs:string,map(xs:string,item())),$options as map(xs:string,item()))
    // as xs:base64Binary
    public static Base64BinaryValue createMap(One<MapItem> entries, One<MapItem> options) throws IOException, XPathException {
        Entry defaultEntry = new Entry();
        Sequence s = options.head().get(StringValue.bmp("compression"));
        if (s != null) {
            defaultEntry.setCompression(stringToCompressionType((s.iterate().next()).getStringValue()));
        }
        return createFromEntries(toInsertionEntries(entries.head(), defaultEntry));
    }

    private static Base64BinaryValue createFromEntries(ArrayList<Entry> entries)
            throws IOException {
        ByteArrayOutputStream full = new ByteArrayOutputStream();
        ZipOutputStream zo = new ZipOutputStream(full);
        ZipEntry nze;
        CRC32 crc = new CRC32();
        for (Entry e : entries) {
            nze = new ZipEntry(e.name);
            Base64BinaryValue b = e.content;
            if (e.isDir()) {   // Ignore any content for directories
                nze.setSize(0);
                nze.setCrc(0);
                zo.putNextEntry(nze);
            } else {
                nze.setMethod(e.compression == CompressionType.STORED ? ZipEntry.STORED : ZipEntry.DEFLATED);
                nze.setSize(b.getLengthInOctets());
                crc.reset();
                crc.update(b.getBinaryValue());
                nze.setCrc(crc.getValue());
                zo.putNextEntry(nze);
                zo.write(b.getBinaryValue());
            }
            zo.closeEntry();
        }
        zo.close();
        return new Base64BinaryValue(full.toByteArray());
    }

}

// Copyright (c) 2018-2022 Saxonica Limited
