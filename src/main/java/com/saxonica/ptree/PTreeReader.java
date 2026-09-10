package com.saxonica.ptree;

import net.sf.saxon.Configuration;
import net.sf.saxon.event.Receiver;
import net.sf.saxon.event.ReceiverOptions;
import net.sf.saxon.expr.parser.ExplicitLocation;
import net.sf.saxon.expr.parser.Location;
import net.sf.saxon.lib.SerializerFactory;
import net.sf.saxon.om.*;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.tree.tiny.CompressedWhitespace;
import net.sf.saxon.tree.tiny.Statistics;
import net.sf.saxon.tree.tiny.TinyBuilder;
import net.sf.saxon.tree.tiny.TinyDocumentImpl;
import net.sf.saxon.tree.util.FastStringBuffer;
import net.sf.saxon.type.*;
import net.sf.saxon.z.IntHashMap;
import net.sf.saxon.z.IntToIntHashMap;
import net.sf.saxon.z.IntToIntMap;

import javax.xml.transform.stream.StreamResult;
import java.io.*;
import java.util.ArrayList;
import java.util.List;


/**
 * This class loads a TinyTree held as a serialized binary file on disk, in a proprietary format
 * designed for speed of loading back into a new TinyTree.
 * <p>The format stored on disk is not dependent on the NamePool. Although it still uses
 * name codes, it also contains a mapping of namecodes to the actual QNames.</p>
 * <p>The base URIs of nodes are not retained.
 * Line numbers (if they were present in the original tree) are not retained.</p>
 */
public class PTreeReader {

    private IntToIntMap names = new IntToIntHashMap(500);
    private IntHashMap<String> prefixMap = new IntHashMap<String>(500);
    private static final Location loc = ExplicitLocation.UNKNOWN_LOCATION;

    /**
     * Read an input stream containing an XML document in PTree format, returning an in-memory
     * representation of the document
     *
     * @param in       the input stream holding the PTree document. The method will consume the stream;
     *                 the caller is responsible for closing the stream after use.
     * @param systemId the base URI to be used for the document associated with this input stream
     * @param config   the Saxon Configuration
     * @return the root node of the constructed in-memory document
     * @throws IOException    if an error occurs reading the input stream
     * @throws XPathException if an error occurs building the in-memory document
     */

    public TreeInfo readTree(DataInputStream in, String systemId, Configuration config)
            throws IOException, XPathException {

        TinyBuilder builder = new TinyBuilder(config.makePipelineConfiguration());
        builder.setStatistics(config.getTreeStatistics().SOURCE_DOCUMENT_STATISTICS);
        builder.setSystemId(systemId);
        readTree(in, systemId, config, builder);

        TinyDocumentImpl doc = (TinyDocumentImpl) builder.getCurrentRoot();
        //doc.setSystemId(systemId);
        return doc.getTreeInfo();
    }

    /**
     * Read an input stream containing an XML document in PTree format, passing the events
     * associated with the document to a Saxon Receiver
     *
     * @param in       the input stream holding the PTree document. The method will consume the stream;
     *                 the caller is responsible for closing the stream after use.
     * @param systemId the base URI to be used for the document associated with this input stream
     * @param config   the Saxon Configuration
     * @param out      the Receiver to receive the document events
     * @throws IOException    if an error occurs reading the input stream
     * @throws XPathException if an error occurs building the in-memory document
     */

    public void readTree(DataInputStream in, String systemId, Configuration config, Receiver out)
            throws IOException, XPathException {

        NamePool pool = config.getNamePool();

        // read version heading
        int signature = in.readInt();
        if (signature != 0xBEE9AC1D) {
            throw new PTreeException("Input file is not a Saxon PTree");
        }
        int version = in.readInt();
        if (version != 0 && version != 1) {
            throw new PTreeException("Input PTree is an unsupported version (" + version + ")");
        }
        int options = in.readInt();
        int numberOfNodes = in.readInt();
        int numberOfAttributes = in.readInt();
        int numberOfNamespaces = in.readInt();
        int textLength = in.readInt();

        if (out instanceof TinyBuilder) {
            Statistics stats = new Statistics(numberOfNodes, numberOfAttributes, numberOfNamespaces, textLength);
            ((TinyBuilder) out).setStatistics(stats);
        }

        out.open();

        handleNames(in, pool);

        int currentDepth = 0;
        boolean previousElement = false;
        for (int i = 0; i < numberOfNodes; i++) {
            int b = in.readUnsignedByte();
            int depthCode = (b >> 6) & 0x3;
            int depth = 0;
            if (depthCode == 0) {
                depth = currentDepth;
            } else if (depthCode == 1) {
                depth = currentDepth + 1;
            } else if (depthCode == 2) {
                depth = in.readUnsignedByte();
            } else if (depthCode == 3) {
                depth = in.readInt();
            }
            int nodeKind = b & 0x3f;
            if (depth < currentDepth + 1) {
                if (previousElement) {
                    out.endElement();
                }
                for (int d = 0; d < currentDepth - depth; d++) {
                    out.endElement();
                }
            }
            currentDepth = depth;
            previousElement = false;

            switch (nodeKind) {
                case Type.DOCUMENT:
                    out.startDocument(0);
                    break;

                case Type.ELEMENT: {
                    int opt = in.readUnsignedByte();
                    int nameCodeSize = ((opt >> 6) & 0x3) + 1;
                    boolean hasAttributes = ((opt >> 5) & 1) != 0;
                    boolean hasNamespaces = ((opt >> 4) & 1) != 0;
                    int typeCodeSize = (opt >> 2) & 0x3;

                    int nc = readVariableInt(in, nameCodeSize);
                    int tc = -1;
                    if (typeCodeSize > 0) {
                        tc = readVariableInt(in, typeCodeSize);
                    }

                    SchemaType newType = null;
                    boolean nilled = false;
                    int newtc = allocate(tc);
                    if (newtc == -1) {
                        newType = Untyped.getInstance();
                    } else {
                        newtc = newtc & NamePool.FP_MASK;
                        StructuredQName newName = config.getNamePool().getStructuredQName(newtc);
                        newType = config.getSchemaType(newName);
                        nilled = (tc & (1 << 29)) != 0;
                    }
                    // The typecode stored in the PTree might no longer exist, for example if the schema
                    // has changed, or if anonymous types have been given a different name. Recover from this
                    // by declaring them as xs:anyType

                    if (newType == null) {
                        newType = AnyType.getInstance();
                    }
                    out.startElement(allocateNodeName(nc, config), newType, loc,
                                     nilled ? ReceiverOptions.NILLED_ELEMENT : 0);

                    if (hasNamespaces) {
                        int nrOfNamespaces = in.readUnsignedShort();
                        for (int n = 0; n < nrOfNamespaces; n++) {
                            String prefix = in.readUTF();
                            String uri = in.readUTF();
                            out.namespace(new NamespaceBinding(prefix, uri), 0);
                        }
                    }

                    if (hasAttributes) {
                        int nrOfAttributes = in.readUnsignedShort();
                        for (int a = 0; a < nrOfAttributes; a++) {
                            readAttribute(in, out, config);
                        }
                    }
                    if (nodeKind == Type.TEXTUAL_ELEMENT) {
                        CharSequence value = readString(in, "");
                        out.characters(value, loc, ReceiverOptions.WHOLE_TEXT_NODE);
                    } else {
                        previousElement = true;
                    }
                    break;
                }
                case Type.TEXTUAL_ELEMENT: {
                    int opt = in.readUnsignedByte();
                    int nameCodeSize = ((opt >> 6) & 0x3) + 1;
                    boolean hasAttributes = ((opt >> 5) & 1) != 0;
                    boolean hasNamespaces = ((opt >> 4) & 1) != 0;
                    int typeCodeSize = (opt >> 2) & 0x3;

                    int nc = readVariableInt(in, nameCodeSize);
                    int tc = -1;
                    if (typeCodeSize > 0) {
                        tc = readVariableInt(in, typeCodeSize);
                    }

                    SchemaType newType = null;
                    boolean nilled = false;
                    int newtc = allocate(tc);
                    if (newtc == -1) {
                        newType = Untyped.getInstance();
                    } else {
                        newtc = newtc & NamePool.FP_MASK;
                        StructuredQName newName = config.getNamePool().getStructuredQName(newtc);
                        newType = config.getSchemaType(newName);
                        nilled = (tc & (1 << 29)) != 0;
                    }
                    // The typecode stored in the PTree might no longer exist, for example if the schema
                    // has changed, or if anonymous types have been given a different name. Recover from this
                    // by declaring them as xs:anyType

                    if (newType == null) {
                        newType = AnyType.getInstance();
                    }
                    out.startElement(allocateNodeName(nc, config), newType, loc,
                                     nilled ? ReceiverOptions.NILLED_ELEMENT : 0);
                    CharSequence value = readString(in, "");
                    out.characters(value, loc, ReceiverOptions.WHOLE_TEXT_NODE);
                    out.endElement();
                    break;
                }
                case Type.TEXT:
                    CharSequence value = readString(in, "");
                    out.characters(value, loc, ReceiverOptions.WHOLE_TEXT_NODE);
                    break;

                case Type.WHITESPACE_TEXT:
                    byte count = in.readByte();
                    long lval = 0;
                    for (int wb = 0; wb < count; wb++) {
                        byte bval = in.readByte();
                        lval = (lval << 8) | ((long) bval & 0xff);
                    }
                    lval = lval << (8 * (8 - count));
                    CompressedWhitespace cws = new CompressedWhitespace(lval);
                    out.characters(cws, loc, ReceiverOptions.WHOLE_TEXT_NODE);
                    break;

                case Type.COMMENT:
                    CharSequence comment = readString(in, "--");
                    out.comment(comment, loc, 0);
                    break;

                case Type.PROCESSING_INSTRUCTION:
                    String piname = in.readUTF();
                    CharSequence pidata = readString(in, "?>");
                    out.processingInstruction(piname, pidata, loc, 0);
                    break;

                case Type.PARENT_POINTER:
                case Type.STOPPER:
                    break;

                default:
                    throw new PTreeException("Unknown node kind " + nodeKind + " in PTree");
            }
        }

        for (int i = 0; i < currentDepth; i++) {
            out.endElement();
        }

        out.endDocument();
        out.close();
    }

    private void readAttribute(DataInputStream in, Receiver builder, Configuration config) throws IOException, XPathException {
        int opt = in.readUnsignedByte();
        boolean isId = (opt & 1) != 0;
        boolean isIdref = (opt & 2) != 0;
        int nameCodeSize = ((opt >> 6) & 3) + 1;
        int typeCodeSize = (opt >> 4) & 3;
        int nameCode = readVariableInt(in, nameCodeSize);
        int typeCode = -1;
        if (typeCodeSize > 0) {
            typeCode = readVariableInt(in, typeCodeSize) & NamePool.FP_MASK;
        }
        boolean valueExists = ((opt >> 3) & 1) != 0;
        CharSequence value = "";
        if (valueExists) {
            value = readString(in, "");
        }

        int newtc = allocate(typeCode);
        if (newtc == -1) {
            newtc = StandardNames.XS_UNTYPED_ATOMIC;
        }
        StructuredQName newName = config.getNamePool().getStructuredQName(newtc);
        SimpleType newType = (SimpleType) config.getSchemaType(newName);
        // The typecode stored in the PTree might no longer exist, for example if the schema
        // has changed, or if anonymous types have been given a different name. Recover from this
        // by declaring them as xs:untypedAtomic

        if (newType == null) {
            newType = BuiltInAtomicType.UNTYPED_ATOMIC;
        }
        int builderOptions = 0;
        if (isId) {
            builderOptions |= ReceiverOptions.IS_ID;
        }
        if (isIdref) {
            builderOptions |= ReceiverOptions.IS_IDREF;
        }
        builder.attribute(allocateNodeName(nameCode, config), newType, value, loc, builderOptions);
    }

    private void handleNames(DataInputStream in, NamePool pool) throws IOException {
        int numberOfNames = in.readInt();
        List<Object[]> tempMap = new ArrayList<Object[]>(numberOfNames);
        for (int i = 0; i < numberOfNames; i++) {
            int prefixCode = in.readUnsignedShort();
            int uriCode = in.readUnsignedShort();
            String localName = in.readUTF();
            Object[] triple = {prefixCode, uriCode, localName};
            tempMap.add(triple);
        }
        int numberOfPrefixes = in.readInt();
        List<String> prefixes = new ArrayList<String>(numberOfPrefixes);
        for (int p = 0; p < numberOfPrefixes; p++) {
            prefixes.add(in.readUTF());
        }
        prefixes.add("");

        int numberOfURIs = in.readInt();
        List<String> uris = new ArrayList<String>(numberOfURIs);
        for (int u = 0; u < numberOfURIs; u++) {
            uris.add(in.readUTF());
        }
        uris.add("");

        for (int i = 0; i < numberOfNames; i++) {
            Object[] triple = tempMap.get(i);
            String prefix = prefixes.get((Integer) triple[0]);
            String uri = uris.get((Integer) triple[1]);
            String local = (String) triple[2];
            int nameCode = pool.allocateFingerprint(uri, local);
            names.put(i, nameCode);
            if (!prefix.isEmpty()) {
                prefixMap.put(i, prefix);
            }
        }
    }

    private int allocate(int oldName) {
        if (oldName == -1) {
            return -1;
        } else {
            return names.get(oldName);
        }
    }

    private NodeName allocateNodeName(int code, Configuration config) {
        if (code == -1) {
            return null;
        } else {
            int fingerprint = names.get(code);
            String prefix = prefixMap.get(code);
            if (prefix == null) {
                prefix = "";
            }
            return new CodedName(fingerprint, prefix, config.getNamePool());
        }
    }

    private int readVariableInt(DataInputStream in, int bytes) throws IOException {
        if (bytes == 4) {
            return in.readInt();
        } else if (bytes == 3) {
            return in.readUnsignedByte() << 16 | in.readUnsignedShort();
        } else if (bytes == 2) {
            return in.readUnsignedShort();
        } else if (bytes == 1) {
            return in.readUnsignedByte();
        } else {
            return -1;
        }
    }

    public static class PTreeException extends XPathException {
        public PTreeException(String s) {
            super(s);
        }
    }

    private CharSequence readString(DataInputStream in, String signal) throws IOException {
        CharSequence value = in.readUTF();
        if (value.equals(signal)) {
            value = readLongString(in, signal);
        }
        return value;
    }

    private FastStringBuffer readLongString(DataInputStream in, String signal) throws IOException {
        int globs = in.readInt();
        FastStringBuffer sb = new FastStringBuffer(globs * 10000);
        for (int i = 0; i < globs; i++) {
            sb.append(in.readUTF());
        }
        return sb;
    }

    /**
     * Command line interface to read a PTree document and convert it to lexical XML
     *
     * @param args [-t] source.ptree out.xml
     * @throws Exception
     */

    public static void main(String[] args) throws Exception {
        if (args.length < 2) {
            System.err.println("Format: java com.saxonica.ptree.PTreeReader [-t] source.ptree out.xml");
            System.exit(2);
        }
        boolean timing = false;
        TreeInfo doc = null;

        int a = 0;
        if (args[a].equals("-t")) {
            a++;
            timing = true;
        }
        int times = 1;
        if (args[a].equals("-3")) {
            a++;
            times = 3;
        }
        Configuration config = new Configuration();
        File infile = new File(args[a++]);
        long startTime = 0;
        for (int tt = 0; tt < times; tt++) {
            startTime = System.currentTimeMillis();
            DataInputStream in = new DataInputStream(
                    new BufferedInputStream(
                            new FileInputStream(infile)));

            doc = new PTreeReader().readTree(in, infile.toURI().toString(), config);

            if (timing) {
                long endTime = System.currentTimeMillis();
                System.err.println("Tree loaded in " + (endTime - startTime) + "ms");
                startTime = endTime;
            }
        }

        File outfile = new File(args[a]);
        SerializerFactory sf = config.getSerializerFactory();
        Receiver out = sf.getReceiver(new StreamResult(outfile));
        doc.getRootNode().copy(out, CopyOptions.ALL_NAMESPACES | CopyOptions.TYPE_ANNOTATIONS, loc);
        out.close();

        if (timing) {
            long endTime = System.currentTimeMillis();
            System.err.println("Tree serialized in " + (endTime - startTime) + "ms");
            startTime = endTime;
        }
    }

}

// Copyright (c) 2005 Saxonica Limited.
