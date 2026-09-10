package com.saxonica.ptree;

import com.saxonica.config.ProfessionalConfiguration;
import net.sf.saxon.Configuration;
import net.sf.saxon.expr.parser.ExplicitLocation;
import net.sf.saxon.lib.ParseOptions;
import net.sf.saxon.om.*;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.tree.tiny.*;
import net.sf.saxon.type.SchemaType;
import net.sf.saxon.type.SimpleType;
import net.sf.saxon.type.Type;
import net.sf.saxon.z.IntToIntHashMap;
import net.sf.saxon.z.IntToIntMap;
import org.xml.sax.SAXParseException;

import javax.xml.transform.Source;
import javax.xml.transform.stream.StreamSource;
import java.io.*;
import java.util.ArrayList;
import java.util.HashMap;
import java.util.List;


/**
 * This class serializes a TinyTree to a binary file on disk, in a proprietary format
 * designed for speed of loading back into a new TinyTree.
 * <p>The format stored on disk is not dependent on the NamePool. Although it still uses
 * name codes, it also contains a mapping of namecodes to the actual QNames.</p>
 * <p>The base URIs of nodes are not retained.
 * Line numbers (if they were present in the original tree) are not retained.</p>
 */
public class PTreeWriter {

    private Configuration config;
    private NamePool pool;
    private List<String[]> names;
    private IntToIntMap nameCodeMap;
    private int version = 1;

    public PTreeWriter(ProfessionalConfiguration config) {
        this.config = config;
        config.checkLicensedFeature(Configuration.LicenseFeature.PROFESSIONAL_EDITION, "PTree writer", -1);
        names = new ArrayList<>(500);
        nameCodeMap = new IntToIntHashMap(500);
        nameCodeMap.setDefaultValue(-1);
    }

    /**
     * Set the PTree version to be written.
     * <ul>
     *     <li>Version 0 was written by all releases prior to 9.9</li>
     *     <li>Version 1 was introduced in Saxon 9.9; it extends the format with textual element nodes.</li>
     * </ul>
     * <p>Saxon 9.9 can read both version 0 and version 1, but always writes version 1, which cannot be read
     * by earlier releases.</p>
     *
     * @param version the version to be written. Must be 1.
     */

    public void setPTreeVersion(int version) {
        if (version != 1) {
            throw new IllegalArgumentException("version must be 1");
        }
        this.version = version;
    }

    /**
     * Serialize a TinyTree in PTree format to a supplied output stream
     *
     * @param tree the tree to be serialized
     * @param out  the output stream. Responsibility for closing this stream rests with the caller
     * @throws IOException if any error occurs writing to the output stream.
     */

    public void writeTree(TinyTree tree, DataOutputStream out) throws IOException, XPathException {
        if (tree.containsGraftedSubtrees()) {
            TinyBuilder builder = new TinyBuilder(config.makePipelineConfiguration());
            tree.getRootNode().copy(builder, CopyOptions.ALL_NAMESPACES, ExplicitLocation.UNKNOWN_LOCATION);
            writeTree(builder.getTree(), out);
            return;
        }
        int numberOfNodes = tree.getNumberOfNodes();
        int[] nameCodes = tree.getNameCodeArray();
        byte[] nodeKind = tree.getNodeKindArray();
        short[] depth = tree.getNodeDepthArray();
        SchemaType[] typeArray = tree.getTypeArray();
        int[] typeCodes = null;
        if (typeArray != null) {
            for (int i=0; i<typeArray.length; i++) {
                int t = typeArray[i].getFingerprint();
                if (tree.isNilled(i)) {
                    t |= NodeInfo.IS_NILLED;
                }
                typeCodes[i] = t;
            }
        }
        int[] alpha = tree.getAlphaArray();
        int[] beta = tree.getBetaArray();
        CharSequence text = tree.getCharacterBuffer();
        CharSequence comments = tree.getCommentBuffer();
        int numberOfAttributes = tree.getNumberOfAttributes();
        int[] attributeNameCodes = tree.getAttributeNameCodeArray();
        int[] attributeParentPointers = tree.getAttributeParentArray();
        SimpleType[] attributeTypes = tree.getAttributeTypeArray();
        int[] attributeTypeCodes = null;
        if (attributeTypes != null) {
            attributeTypeCodes = new int[attributeTypes.length];
            for (int i = 0; i < attributeTypes.length; i++) {
                int t = attributeTypes[i].getFingerprint();
                if (tree.isIdrefAttribute(i)) {
                    t |= TinyTree.TYPECODE_IDREF;
                }
                typeCodes[i] = t;
            }
        }
        CharSequence[] attributeValues = tree.getAttributeValueArray();
        int numberOfNamespaces = tree.getNumberOfNamespaces();
        NamespaceBinding[] namespaceCodes = tree.getNamespaceBindings();
        int[] namespaceParents = tree.getNamespaceParentArray();

        pool = tree.getNamePool();

        out.writeInt(0xBEE9AC1D);       // magic word for recognition purposes
        out.writeInt(version);          // version number, always zero

        out.writeInt(0);                // options and flags for future use
        out.writeInt(numberOfNodes);
        out.writeInt(numberOfAttributes);
        out.writeInt(numberOfNamespaces);
        out.writeInt(text.length());

        handleNames(tree.getPrefixPool(), out, nameCodes, typeCodes, attributeNameCodes, attributeTypeCodes);

        for (int i = 0; i < numberOfNodes; i++) {
            int depthCode;
            if (i == 0 || depth[i] == depth[i - 1]) {
                depthCode = 0;
            } else if (depth[i] == depth[i - 1] + 1) {
                depthCode = 1;
            } else if (depth[i] <= 255) {
                depthCode = 2;
            } else {
                depthCode = 3;
            }
            out.writeByte(nodeKind[i] | (depthCode << 6));
            if (depthCode == 2) {
                out.writeByte(depth[i]);
            } else if (depthCode == 3) {
                out.writeInt(depth[i]);
            }
            switch (nodeKind[i]) {
                case Type.DOCUMENT:
                    break;

                case Type.ELEMENT: {
                    int nc = allocateNameCode(tree.getPrefixPool(), nameCodes[i]);
                    int nameCodeSize = integerLength(nc);
                    int options = (nameCodeSize - 1) << 6;
                    options |= alpha[i] >= 0 ? 1 << 5 : 0;
                    options |= beta[i] >= 0 ? 1 << 4 : 0;
                    int typeCodeSize = 0;
                    int tc = 0;
                    if (typeCodes != null && typeCodes[i] >= 0) {
                        tc = allocateNameCode(tree.getPrefixPool(), typeCodes[i]);
                        typeCodeSize = integerLength(tc);
                        options |= (typeCodeSize & 0x3) << 2;
                    }

                    out.writeByte(options);

                    writeVariableInt(out, nc, nameCodeSize);

                    if (typeCodeSize > 0) {
                        writeVariableInt(out, tc, typeCodeSize);
                    }

                    if (beta[i] >= 0) {
                        int ns = beta[i];
                        int nss = 0;
                        while (ns < numberOfNamespaces && namespaceParents[ns++] == i) {
                            nss++;
                        }
                        out.writeShort(nss);
                        for (int a = 0; a < nss; a++) {
                            NamespaceBinding nscode = namespaceCodes[beta[i] + a];
                            out.writeUTF(nscode.getPrefix());
                            out.writeUTF(nscode.getURI());
                        }
                    }

                    if (alpha[i] >= 0) {
                        int att = alpha[i];
                        int atts = 0;
                        while (att < numberOfAttributes && attributeParentPointers[att++] == i) {
                            atts++;
                        }
                        out.writeShort(atts);
                        for (int a = 0; a < atts; a++) {
                            int ai = alpha[i] + a;
                            int newTypeCode = -1;
                            boolean isId = tree.isIdAttribute(ai);
                            boolean isIdref = tree.isIdrefAttribute(ai);
                            if (attributeTypeCodes != null && attributeTypeCodes[ai] >= 0) {
                                int fp = attributeTypeCodes[ai] & ~TinyTree.TYPECODE_IDREF;
                                newTypeCode = allocateNameCode(tree.getPrefixPool(), fp);
                            }
                            writeAttribute(out,
                                           allocateNameCode(tree.getPrefixPool(), attributeNameCodes[alpha[i] + a]),
                                           newTypeCode,
                                           isId,
                                           isIdref,
                                           attributeValues[alpha[i] + a].toString());
                        }
                    }
                    break;
                }
                case Type.TEXTUAL_ELEMENT: {
                    int nc = allocateNameCode(tree.getPrefixPool(), nameCodes[i]);
                    int nameCodeSize = integerLength(nc);
                    int options = (nameCodeSize - 1) << 6;
                    options |= alpha[i] >= 0 ? 1 << 5 : 0;
                    options |= beta[i] >= 0 ? 1 << 4 : 0;
                    int typeCodeSize = 0;
                    int tc = 0;
                    if (typeCodes != null && typeCodes[i] >= 0) {
                        tc = allocateNameCode(tree.getPrefixPool(), typeCodes[i]);
                        typeCodeSize = integerLength(tc);
                        options |= (typeCodeSize & 0x3) << 2;
                    }

                    out.writeByte(options);

                    writeVariableInt(out, nc, nameCodeSize);

                    if (typeCodeSize > 0) {
                        writeVariableInt(out, tc, typeCodeSize);
                    }
                    String value = TinyTextImpl.getStringValue(tree, i).toString();
                    writeString(value, out, "");
                    break;
                }
                case Type.TEXT: {
                    String value = TinyTextImpl.getStringValue(tree, i).toString();
                    writeString(value, out, "");
                    break;
                }
                case Type.WHITESPACE_TEXT: {
                    final long val = WhitespaceTextImpl.getLongValue(tree, i);
                    byte count = 0;
                    for (int s = 56; s >= 0; s -= 8) {
                        byte b = (byte) ((val >>> s) & 0xff);
                        if (b == 0) {
                            break;
                        } else {
                            count++;
                        }
                    }
                    out.writeByte(count);
                    for (int s = 56; s >= 0; s -= 8) {
                        byte b = (byte) ((val >>> s) & 0xff);
                        if (b == 0) {
                            break;
                        } else {
                            out.writeByte(b);
                        }
                    }
                    break;
                }
                case Type.COMMENT:
                    String comment = comments.subSequence(alpha[i], alpha[i] + beta[i]).toString();
                    writeString(comment, out, "--");
                    break;

                case Type.PROCESSING_INSTRUCTION:
                    String name = pool.getLocalName(nameCodes[i]);
                    out.writeUTF(name);
                    String data = comments.subSequence(alpha[i], alpha[i] + beta[i]).toString();
                    writeString(data, out, "?>");
                    break;

                case Type.PARENT_POINTER:
                    break;

                case Type.EXTERNAL_NODE_REFERENCE:
                    // Grafted trees have been dealt with by copying to a regular (ungrafted) tree
                    throw new AssertionError();

            }
        }
        out.flush();
    }

    private void writeVariableInt(DataOutputStream out, int value, int bytes) throws IOException {
        if (bytes == 4) {
            out.writeInt(value);
        } else if (bytes == 3) {
            out.writeByte(value >>> 16);
            out.writeByte(value >>> 8);
            out.writeByte(value);
        } else if (bytes == 2) {
            out.writeByte(value >>> 8);
            out.writeByte(value);
        } else if (bytes == 1) {
            out.writeByte(value);
        }
    }

    private void handleNames(PrefixPool prefixPool, DataOutputStream out, int[] enames, int[] etypes, int[] anames, int[] atypes)
            throws IOException {

        indexNames(prefixPool, enames, 0xffffffff);
        indexNames(prefixPool, etypes, 0xffffffff);
        indexNames(prefixPool, anames, 0xffffffff);
        indexNames(prefixPool, atypes, ~TinyTree.TYPECODE_IDREF);

        // Now output the relevant part of the NamePool

        out.writeInt(names.size());

        HashMap<String, Short> uris = new HashMap<>(16);
        List<String> uriList = new ArrayList<>(16);
        HashMap<String, Short> prefixes = new HashMap<>(16);
        List<String> prefixList = new ArrayList<>(16);
        for (Object name : names) {
            String[] triple = (String[]) name;
            Short prefixCode = prefixes.get(triple[0]);
            if (prefixCode == null) {
                prefixCode = (short) prefixes.size();
                prefixes.put(triple[0], prefixCode);
                prefixList.add(triple[0]);
            }
            Short uriCode = uris.get(triple[1]);
            if (uriCode == null) {
                uriCode = (short) uris.size();
                uris.put(triple[1], uriCode);
                uriList.add(triple[1]);
            }
            out.writeShort(prefixCode);
            out.writeShort(uriCode);
            if (triple[2] == null) {
                triple[2] = "";
            }
            out.writeUTF(triple[2]);
        }
        out.writeInt(prefixes.size());
        for (String prefix : prefixList) {
            out.writeUTF(prefix);
        }
        out.writeInt(uris.size());
        for (String uri : uriList) {
            out.writeUTF(uri);
        }
    }

    private void indexNames(PrefixPool prefixPool, int[] codes, int mask) {
        if (codes != null) {
            for (int code : codes) {
                if (code > 0) {
                    allocateNameCode(prefixPool, code & mask);
                }
            }
        }
    }

    private int allocateNameCode(PrefixPool prefixPool, int oldNameCode) {
        int nc = nameCodeMap.get(oldNameCode);
        if (nc == -1) {
            String prefix = prefixPool.getPrefix(oldNameCode >> 20);
            String uri = pool.getURI(oldNameCode);
            String localName = pool.getLocalName(oldNameCode);
            String[] triple = {prefix, uri, localName};
            int newNameCode = names.size();
            names.add(triple);
            nameCodeMap.put(oldNameCode, newNameCode);
            return newNameCode;
        } else {
            return nc;
        }
    }

    private int integerLength(int i) {
        if (i < 0) {
            return 4;
        } else if (i < 255) {
            return 1;
        } else if (i < 65535) {
            return 2;
        } else if (i < (1 << 24)) {
            return 3;
        } else {
            return 4;
        }
    }

    private void writeAttribute(DataOutputStream out, int nc, int typeCode, boolean isId, boolean isIdref, String value) throws IOException {
        int nameCodeSize = integerLength(nc);
        int options = (nameCodeSize - 1) << 6;
        int typeCodeSize = integerLength(typeCode);
        if (typeCode >= 0) {
            options |= (typeCodeSize & 0x3) << 4;
        }
        options |= isId ? 1 : 0;
        options |= isIdref ? 2 : 0;
        options |= value.isEmpty() ? 0 : 1 << 3;
        out.writeByte(options);

        writeVariableInt(out, nc, nameCodeSize);

        if (typeCode >= 0) {
            writeVariableInt(out, typeCode, typeCodeSize);
        }

        if (!value.isEmpty()) {
            writeString(value, out, "");
        }
    }

    /**
     * Write a string
     * @param value the string to be written
     * @param out the destination
     * @param signal a string that will never occur as an actual value, and which can therefore be
     *               used as a signal that the "long representation" of strings is being used
     * @throws IOException if writing to the output fails
     */
    private void writeString(String value, DataOutputStream out, String signal) throws IOException {
        try {
            out.writeUTF(value);
        } catch (UTFDataFormatException err) {
            out.writeUTF(signal);
            writeLongString(value, out, signal);
        }
    }

    /**
     * Write a string that is too long to be represented using the default writeUTF format (which
     * has a limit of 65535 characters)
     * @param chars the string to be written
     * @param out the destination
     * @param signal a string that will never occur as an actual value, and which can therefore be
     *               used as a signal that the "long representation" of strings is being used
     * @throws IOException if writing to the output fails
     */
    private void writeLongString(/*@Nullable*/ CharSequence chars, DataOutputStream out, String signal) throws IOException {
        // the writeUTF() method has an undocumented limit of 65535 bytes
        if (chars == null) {
            chars = signal;
        }
        int length = chars.length();
        int globs = length / 10000;
        if (length % 10000 != 0) {
            globs++;
        }
        int start = 0;
        out.writeInt(globs);
        for (int i = 0; i < globs; i++) {
            out.writeUTF(chars.subSequence(start, Math.min(start + 10000, length)).toString());
            start += 10000;
        }
    }

    private static TreeInfo build(File in, boolean stripSpace) throws XPathException {
        Source source = new StreamSource(in.toURI().toString());
        ParseOptions options = new ParseOptions();
        if (stripSpace) {
            options.setSpaceStrippingRule(AllElementsSpaceStrippingRule.getInstance());
        }
        ProfessionalConfiguration config = new ProfessionalConfiguration();
        try {
            return config.buildDocumentTree(source, options);
        } catch (XPathException err) {
            Throwable cause = err.getException();
            if (cause instanceof SAXParseException) {
                // This generally means the error was already reported.
                // But if a RuntimeException occurs in Saxon during a callback from
                // the Crimson parser, Crimson wraps this in a SAXParseException without
                // reporting it further.
                SAXParseException spe = (SAXParseException) cause;
                cause = spe.getException();
                if (cause instanceof RuntimeException) {
                    config.reportFatalError(err);
                }
            } else {
                while (err.getException() instanceof XPathException) {
                    err = (XPathException) err.getException();
                }
                config.reportFatalError(err);
            }
            throw err;
        }
    }

    /**
     * Static utility method for writing a Ptree
     *
     * @param in      the Source representing the document to be written as a PTree
     * @param options the options to be used when processing the input source
     * @param out     the OutputStream to which the PTree is written. The responsibility for closing
     *                this stream rests with the caller.
     * @throws XPathException if any failure occurs processing the input
     * @throws IOException    if any failure occurs writing the output
     * @since 9.3
     */

    public static void copyToPTree(Source in, ParseOptions options, OutputStream out) throws XPathException, IOException {
        DataOutputStream outs = new DataOutputStream(out);
        ProfessionalConfiguration config = new ProfessionalConfiguration();
        ParseOptions opt2 = new ParseOptions(options);
        opt2.setModel(TreeModel.TINY_TREE);
        TinyDocumentImpl doc = (TinyDocumentImpl) config.buildDocumentTree(in, opt2).getRootNode();
        new PTreeWriter(config).writeTree(doc.getTree(), outs);

    }

    /**
     * Command line interface for writing a Ptree
     *
     * @param args [-strip] source.xml out.tree
     * @throws Exception if the operation fails
     */

    public static void main(String[] args) throws Exception {
        if (args.length < 2) {
            System.err.println("Format: java com.saxonica.PTreeWriter [-strip] source.xml out.tree");
            System.exit(2);
        }
        int a = 0;
        boolean stripSpace = false;
        int version = 1; // Version 0 used in all releases up to 9.9; version 1 introduced in 9.9
        if (args[a].equals("-strip")) {
            stripSpace = true;
            a++;
        }
        File in = new File(args[a++]);
        TreeInfo doc = build(in, stripSpace);

        File out = new File(args[a]);
        DataOutputStream outs = new DataOutputStream(new FileOutputStream(out));
        TinyTree tree = (TinyTree) doc;
        PTreeWriter writer = new PTreeWriter((ProfessionalConfiguration) doc.getConfiguration());
        writer.setPTreeVersion(version);
        writer.writeTree(tree, outs);
        outs.close();
    }
}

// Copyright (c) 2005-2011 Saxonica Limited.
