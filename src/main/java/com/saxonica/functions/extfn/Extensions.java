package com.saxonica.functions.extfn;

import net.sf.saxon.Configuration;
import net.sf.saxon.Controller;
import net.sf.saxon.expr.Component;
import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.expr.instruct.ParameterSet;
import net.sf.saxon.expr.number.NamedTimeZone;
import net.sf.saxon.lib.FeatureKeys;
import net.sf.saxon.lib.NamespaceConstant;
import net.sf.saxon.lib.StandardDiagnostics;
import net.sf.saxon.lib.StandardLogger;
import net.sf.saxon.ma.map.HashTrieMap;
import net.sf.saxon.ma.map.MapItem;
import net.sf.saxon.om.*;
import net.sf.saxon.resource.Loader;
import net.sf.saxon.serialize.charcode.UTF16CharacterSet;
import net.sf.saxon.serialize.charcode.UTF8CharacterSet;
import net.sf.saxon.str.StringTool;
import net.sf.saxon.str.UnicodeString;
import net.sf.saxon.trans.*;
import net.sf.saxon.tree.iter.EmptyIterator;
import net.sf.saxon.tree.tiny.TinyAttributeImpl;
import net.sf.saxon.tree.util.Navigator;
import net.sf.saxon.tree.util.Orphan;
import net.sf.saxon.tree.util.ProcInstParser;
import net.sf.saxon.type.*;
import net.sf.saxon.value.*;

import java.io.*;
import java.net.URL;
import java.time.format.DateTimeFormatter;
import java.time.format.DateTimeFormatterBuilder;
import java.time.format.DateTimeParseException;
import java.time.format.ResolverStyle;
import java.time.temporal.ChronoField;
import java.time.temporal.TemporalAccessor;
import java.util.ArrayList;
import java.util.Arrays;
import java.util.Iterator;
import java.util.List;
import java.util.function.IntPredicate;

/**
 * This class implements functions that are supplied as standard with SAXON,
 * but which are not defined in the XSLT or XPath specifications.
 * <p>To invoke these functions, use a function call of the form <code>prefix:name()</code> where
 * name is the method name, and prefix maps to the URI <code>http://saxon.sf.net/</code></p>
 */


public class Extensions {

    // The class is never instantiated
    private Extensions() {
    }

    /**
     * Switch tracing off. Only works if tracing was enabled at compile time.
     *
     * @param c the XPath dynamic context
     */

    public static void pauseTracing(XPathContext c) {
        c.getController().pauseTracing(true);
    }

    /**
     * Resume tracing. Only works if tracing was originally enabled
     * but is currently paused.
     *
     * @param c the XPath dynamic context
     */

    public static void resumeTracing(XPathContext c) {
        c.getController().pauseTracing(false);
    }

    /**
     * Return the system identifier of the context node
     *
     * @param c the XPath dynamic context
     * @return the system ID
     */

    public static One<StringItem> systemId(XPathContext c) throws XPathException {
        Item item = c.getContextItem();
        if (item == null) {
            XPathException e = new XPathException("The context item for saxon:systemId() is absent");
            e.setXPathContext(c);
            throw e;
        }
        if (item instanceof NodeInfo) {
            return One.string(((NodeInfo) item).getSystemId());
        } else {
            return One.string("");
        }
    }

    /**
     * Return the line number of the context node.
     *
     * @param c the XPath dynamic context
     * @return the line number, or -1 if not available
     */
    public static One<IntegerValue> lineNumber(XPathContext c) {
        Item item = c.getContextItem();
        if (item instanceof NodeInfo) {
            return One.integer(((NodeInfo) item).getLineNumber());
        } else {
            return One.integer(-1);
        }
    }

    /**
     * Return the line number of the specified node.
     *
     * @param node the node whose line number is required
     * @return the line number of the node. This is only available if line numbering was switched on.
     */
    public static One<IntegerValue> lineNumber(ZeroOrOne<NodeInfo> node) {
        NodeInfo n = node.head();
        if (n == null) {
            return One.integer(-1);
        }
        return One.integer(n.getLineNumber());
    }

    /**
     * Return the column number of the context node. This is only
     * available if line numbering has been enabled for the containing tree
     *
     * @param c the XPath dynamic context
     * @return the column number, or -1 if not available
     */
    public static One<IntegerValue> columnNumber(XPathContext c) {
        Item item = c.getCurrentIterator().current();
        if (item instanceof NodeInfo) {
            return One.integer(((NodeInfo) item).getColumnNumber());
        } else {
            return One.integer(-1);
        }
    }

    /**
     * Return the column number of the specified node.
     *
     * @param node the node whose column number is required
     * @return the column number of the node. This is only available if line numbering was switched on.
     */
    public static One<IntegerValue> columnNumber(ZeroOrOne<NodeInfo> node) {
        NodeInfo n = node.head();
        if (n == null) {
            return One.integer(-1);
        }
        return One.integer(n.getColumnNumber());
    }

    /**
     * Get the values of all tunnel parameters (whether declared in the current template or not).
     * Returns a value of type {@code map(xs:QName, item()*)}.
     */

    public static One<MapItem> tunnelParams(XPathContext context) throws XPathException {
        ParameterSet params = context.getTunnelParameters();
        params.materializeValues();
        StructuredQName[] names = params.getParameterNames();
        HashTrieMap map = new HashTrieMap();
        for (int i=0; i<params.size(); i++) {
            map.initialPut(new QNameValue(names[i], BuiltInAtomicType.QNAME), params.getValue(i).materialize());
        }
        return new One<>(map);
    }

    /**
     * Remove a document from the document pool. The effect is that the document becomes eligible for
     * garbage collection, allowing memory to be released when processing of the document has finished.
     * The downside is that a subsequent call on document() with the same URI causes the document to be
     * reloaded and reparsed, and the new nodes will have different node identity from the old.
     *
     * @param context the evaluation context (supplied implicitly by the call mechanism)
     * @param document     the document to be released from the document pool
     * @return the document that was released. This allows a call such as
     *         select="saxon:discard-document(document('a.xml'))"
     */

    public static ZeroOrOne<NodeInfo> discardDocument(XPathContext context, ZeroOrOne<NodeInfo> document) {
        NodeInfo doc = document.head();
        if (doc == null) {
            return new ZeroOrOne<>(null);
        }
        Controller c = context.getController();
        String uri = c.getDocumentPool().getDocumentURI(doc);
        if (uri != null && c instanceof XsltController) {
            ((XsltController)c).removeUnavailableOutputDestination(new DocumentKey(uri));
        }
        c.getDocumentPool().discard(doc.getTreeInfo());
        return new ZeroOrOne<>(doc);
    }

    /**
     * Determine whether two node sequence contain the same sequences of nodes, in the same order, compared by identity
     *
     * @param p1 The first node-set. The iterator must be correctly ordered.
     * @param p2 The second node-set. The iterator must be correctly ordered.
     * @return true if p1 and p2 contain the same set of nodes
     */

    public static One<BooleanValue> hasSameNodes(ZeroOrMore<NodeInfo> p1, ZeroOrMore<NodeInfo> p2) throws XPathException {
        SequenceIterator e1 = p1.iterate();
        SequenceIterator e2 = p2.iterate();

        if (e1 == null) {
            e1 = EmptyIterator.getInstance();
        }

        if (e2 == null) {
            e2 = EmptyIterator.getInstance();
        }

        while (true) {
            NodeInfo n1 = (NodeInfo)e1.next();
            NodeInfo n2 = (NodeInfo)e2.next();
            if (n1 == null || n2 == null) {
                return One.bool(n1 == n2);
            }
            if (!n1.equals(n2)) {
                return One.bool(false);
            }
        }
    }

    /**
     * Return an XPath expression that identifies a specified node
     *
     * @param node the node whose path is required
     * @return a path expression giving a path from the root of the tree to the specified node
     */

    public static One<StringItem> path(XPathContext context, One<NodeInfo> node)  {
        return One.string(Navigator.getPath(node.head(), context));
    }


    /**
     * Return an XPath expression that identifies the current node
     *
     * @param c the XPath dynamic context
     * @return a path expression giving a path from the root of the tree to the context node
     */

    public static One<StringItem> path(XPathContext c) throws XPathException {
        Item item = c.getContextItem();
        if (item == null) {
            XPathException e = new XPathException("The context item for saxon:path() is not set");
            e.setXPathContext(c);
            throw e;
        }
        if (item instanceof NodeInfo) {
            return One.string(Navigator.getPath((NodeInfo) item, c));
        } else {
            return One.string("");
        }
    }

    /**
     * Determine the value of the type annotation of a node or an atomic value
     *
     * @param context the XPath dynamic context
     * @param value    the node or atomic value whose type annotation is required
     * @return the type annotation or type label as a QName; null in the case of text nodes,
     *         comments, processing instructions
     */

    public static ZeroOrOne<QNameValue> typeAnnotation(XPathContext context, ZeroOrOne<Item> value) {
        Item item = value.head();
        if (item == null) {
            return ZeroOrOne.empty();
        } else if (item instanceof NodeInfo) {
            NodeInfo node = (NodeInfo) item;
            SchemaType type = node.getSchemaType();
            if (type == null) {
                return ZeroOrOne.empty();
            }
            String uri = type.getTargetNamespace();
            String prefix = uri.equals(NamespaceConstant.SCHEMA) ? "xs" : "";
            return new ZeroOrOne<>(new QNameValue(prefix, uri, type.getName()));
        } else if (item instanceof AtomicValue) {
            AtomicType type = ((AtomicValue) item).getItemType();
            String uri = type.getTargetNamespace();
            String prefix = uri.equals(NamespaceConstant.SCHEMA) ? "xs" : "";
            return new ZeroOrOne<>(new QNameValue(prefix, uri, type.getName()));
        } else if (item instanceof ExternalObject) {
            return new ZeroOrOne<>(new QNameValue(JavaExternalObjectType.classNameToQName(
                    ((ExternalObject) item).getObject().getClass().getName()), BuiltInAtomicType.QNAME));
        } else {
            return ZeroOrOne.empty();
        }
    }

//#ifdefined SCHEMA

    /**
     * Determine the type annotation of a node or an atomic value in the form of a
     * function item representing the type definition as a schema component, allowing
     * navigation to other related schema components.
     *
     * @param value the node or atomic value whose type annotation is required
     * @return the type annotation or type label as a QName; null in the case of text nodes,
     *         comments, processing instructions
     */

    public static ZeroOrOne<Function> type(ZeroOrOne<Item> value) {
        Item item = value.head();
        if (item == null) {
            return ZeroOrOne.empty();
        } else if (item instanceof NodeInfo) {
            NodeInfo node = (NodeInfo) item;
            SchemaType type = node.getSchemaType();
            if (type == null) {
                return ZeroOrOne.empty();
            }
            return new ZeroOrOne<>(type.getComponentAsFunction());
        } else if (item instanceof AtomicValue) {
            AtomicType type = ((AtomicValue) item).getItemType();
            return new ZeroOrOne<>(type.getComponentAsFunction());
        } else {
            return ZeroOrOne.empty();
        }
    }
//#endif


    /**
     * Return the XPathContext object
     *
     * @param c the context object
     * @return the context object (this looks crazy, but it works given that the function is called from an XPath
     *         environment where the context is supplied as an implicit argument)
     */

    public static XPathContext getContext(XPathContext c) {
        return c;
    }

    /**
     * Return the Controller object
     *
     * @param c the XPath dynamic context
     * @return the Controller
     */

    public static Controller getController(XPathContext c) {
        return c.getController();
    }

    /**
     * Return the Configuration object
     *
     * @param c the XPath dynamic context
     * @return the Saxon configuration
     */

    public static Configuration getConfiguration(XPathContext c) {
        return c.getConfiguration();
    }

    /**
     * Return a string containing a diagnostic print of the current execution stack
     *
     * @param c the XPath dynamic context
     * @return a diagnostic stack print
     */

    public static One<StringItem> printStack(XPathContext c) {
        ByteArrayOutputStream baos = new ByteArrayOutputStream(1000);
        StandardLogger logger = new StandardLogger();
        logger.setPrintStream(new PrintStream(baos));
        new StandardDiagnostics().logStackTrace(c, logger, 2);
        return One.string(baos.toString().replace("\r", ""));
    }

    /**
     * Force eager evaluation of the expression supplied as the argument.
     * @param value any value
     * @return the supplied value, but in a form that is guaranteed to be "grounded",
     * that is, evaluated in memory with no context dependencies and no possibilities
     * of dynamic evaluation errors when the value is used.
     */

    public static ZeroOrMore<Item> eager(ZeroOrMore<Item> value) {
        // the work of eagerly evaluating the argument expression is done by the function calling mechanism
        return value;
    }

    /**
     * Get a pseudo-attribute of a processing instruction. Return an empty string
     * if the pseudo-attribute is not present.
     * Character references and built-in entity references are expanded
     *
     * @param c    the XPath dynamic context. The context item should be a processing instruction,
     *             though it doesn't matter if it isn't: the function will look at the string-value of the context item
     *             whatever it is.
     * @param name the name of the required pseudo-attribute
     * @return the value of the pseudo-attribute if it is present
     */

    public static ZeroOrOne<StringItem> getPseudoAttribute(XPathContext c, One<StringItem> name)
            throws XPathException {
        Item pi = c.getContextItem();
        if (pi == null) {
            XPathException e = new XPathException("The context item for saxon:getPseudoAttribute() is not set");
            e.setXPathContext(c);
            throw e;
        }
        // we'll assume it's a PI, it doesn't matter if it isn't...
        final String value = ProcInstParser.getPseudoAttribute(pi.getStringValue(), name.head().getStringValue());
        return new ZeroOrOne<>(value==null ? null : new StringItem(value));
    }

    /**
     * Get the UTF-8 encoding of a string
     *
     * @param in the supplied string
     * @return a sequence of integers, each in the range 0-255, representing the octets of the UTF-8
     *         encoding of the given string
     */

    public static ZeroOrMore<IntegerValue> stringToUtf8(One<StringItem> in) {
        String s = in.head().getStringValue();
        ArrayList<IntegerValue> list = new ArrayList<>(s.length() * 2);
        byte[] octets = new byte[4];
        for (int i = 0; i < s.length(); i++) {
            int used = UTF8CharacterSet.getUTF8Encoding(
                    s.charAt(i), i + 1 < s.length() ? s.charAt(i + 1) : (char) 0, octets);
            for (int j = 0; j < used; j++) {
                list.add(new Int64Value(255 & (int) octets[j]));
            }
        }
        return new ZeroOrMore<>(list);
    }

    /**
     * Convert a sequence of integers in the range 0-255, representing a sequence of octets,
     * to a base64Binary value
     *
     * @param in the input array of bytes (octets)
     * @return the corresponding base64Binary value
     */

    public static ZeroOrOne<Base64BinaryValue> octetsToBase64Binary(ZeroOrMore<IntegerValue> in) throws XPathException {
        if (in == null) {
            return ZeroOrOne.empty();
        }
        return new ZeroOrOne<>(new Base64BinaryValue(toByteArray(in)));
    }

    private static byte[] toByteArray(ZeroOrMore<IntegerValue> in) throws XPathException {
        byte[] octets = new byte[in.getLength()];
        for (int i=0; i<octets.length; i++) {
            long l = in.itemAt(i).longValue();
            if (l < 0L || l > 255L) {
                throw new XPathException("Integers in input sequence must be in range 0-255", "SXJX0001");
            }
            octets[i] = (byte)l;
        }
        return octets;
    }

    /**
     * Convert a sequence of integers in the range 0-255, representing a sequence of octets,
     * to a hexBinary value
     *
     * @param in the input array of bytes (octets)
     * @return the corresponding HexBinary value
     */

    public static ZeroOrOne<HexBinaryValue> octetsToHexBinary(ZeroOrMore<IntegerValue> in) throws XPathException {
        if (in == null) {
            return ZeroOrOne.empty();
        }
        return new ZeroOrOne<>(new HexBinaryValue(toByteArray(in)));
    }

    /**
     * Convert a base64Binary value to a sequence of integers representing the octets contained in the value
     *
     * @param in the supplied base64Binary value
     * @return the corresponding array of integers, representing the octet values
     */

    public static byte[] base64BinaryToOctets(One<Base64BinaryValue> in) {
        return in.head().getBinaryValue();
    }

    /**
     * Convert a hexBinary value to a sequence of integers representing the octets contained in the value
     *
     * @param in the input hexBinary value
     * @return the corresponding array of integers, representing the octet values
     */

    public static byte[] hexBinaryToOctets(One<HexBinaryValue> in) {
        return in.head().getBinaryValue();
    }

    /**
     * Convert a base64Binary value to a String, assuming a particular encoding
     *
     * @param context  the XPath dynamic context
     * @param in       the supplied base64Binary value
     * @param encoding the character encoding
     * @return the string that results from treating the base64binary value as a sequence of octets
     *         that encode a string in the given encoding
     * @throws XPathException for example if the encoding is not recognized or if the binary value is not valid in this
     * encoding.
     */

    public static ZeroOrOne<StringItem> base64BinaryToString(XPathContext context, ZeroOrOne<Base64BinaryValue> in, One<StringItem> encoding)
    throws XPathException {
        try {
            if (in.head() == null) {
                return ZeroOrOne.empty();
            }
            byte[] bytes = in.head().getBinaryValue();
            ByteArrayInputStream stream = new ByteArrayInputStream(bytes);
            InputStreamReader reader = new InputStreamReader(stream, encoding.head().getStringValue());
            char[] array = new char[bytes.length];
            int used = reader.read(array, 0, array.length);
            checkBytes(array, 0, used, context.getConfiguration().getValidCharacterChecker());
            return new ZeroOrOne<>(new StringItem(new String(array, 0, used)));
        } catch (IOException e) {
            throw new XPathException(e);
        }
    }

    /**
     * Convert a string to a base64Binary value in a given encoding
     *
     * @param in       the input string
     * @param encoding the desired encoding
     * @return the base64Binary value that results from encoding the string as a sequence of octets in the
     *         given encoding.
     */

    public static ZeroOrOne<Base64BinaryValue> stringToBase64Binary(ZeroOrOne<StringItem> in, One<StringItem> encoding)
            throws IOException {
        if (in.head() == null) {
            return ZeroOrOne.empty();
        }
        ByteArrayOutputStream stream = new ByteArrayOutputStream(in.head().length32());
        OutputStreamWriter writer = new OutputStreamWriter(stream, encoding.head().getStringValue());
        writer.write(in.head().getStringValue());
        writer.close();
        byte[] bytes = stream.toByteArray();
        return new ZeroOrOne<>(new Base64BinaryValue(bytes));
    }

    /**
     * Convert a hexBinary value to a String, assuming a particular encoding
     *
     * @param context  the XPath dynamic context
     * @param in       the supplied hexBinary value
     * @param encoding the character encoding
     * @return the string that results from treating the hexBinary value as a sequence of octets
     *         that encode a string in the given encoding
     * @throws XPathException if the encoding is not recognized or if the binary value is not valid in this encoding.
     */

    public static ZeroOrOne<StringItem> hexBinaryToString(XPathContext context, ZeroOrOne<HexBinaryValue> in, One<StringItem> encoding) throws XPathException {
        try {
            if (in.head() == null) {
                return ZeroOrOne.empty();
            }
            byte[] bytes = in.head().getBinaryValue();
            ByteArrayInputStream stream = new ByteArrayInputStream(bytes);
            InputStreamReader reader = new InputStreamReader(stream, encoding.head().getStringValue());
            char[] array = new char[bytes.length];
            int used = reader.read(array, 0, array.length);
            checkBytes(array, 0, used, context.getConfiguration().getValidCharacterChecker());
            return new ZeroOrOne<>(new StringItem(new String(array, 0, used)));
        } catch (IOException e) {
            throw new XPathException(e);
        }
    }

    /**
     * Read the contents of a binary resource identified by a URI
     * @param uri the URI of the resource to be read
     * @return the contents of the resource identified by the URI, as a Base64Binary value
     * @throws XPathException if an I/O error occurs, for example if the resource cannot be read
     */

    public static One<Base64BinaryValue> readBinaryResource(One<StringItem> uri) throws XPathException {
        try {
            String arg = uri.head().getStringValue();
            URL url = new URL(arg);
            InputStream stream;
            try {
                stream = Loader.urlStream(url);
            } catch (IllegalArgumentException e) {
                // Not well documented, but this happens when given an invalid URL
                throw new XPathException("saxon:read-binary-resource - invalid URL " + arg, SaxonErrorCode.SXJE0053);
            }
            List<byte[]> buffers = new ArrayList<>();
            int len = 0;
            while(true) {
                byte[] buffer = new byte[4096];
                int b = stream.read(buffer);
                if (b < 0) {
                    break;
                }
                if (b < 4096) {
                    buffer = Arrays.copyOf(buffer, b);
                }
                buffers.add(buffer);
                len += b;
            }
            byte[] result = new byte[len];
            int offset = 0;
            for (byte[] buffer : buffers) {
                System.arraycopy(buffer, 0, result, offset, buffer.length);
                offset += buffer.length;
            }
            return new One<>(new Base64BinaryValue(result));
        } catch (IOException e) {
            throw new XPathException("saxon:read-binary-resource: I/O error: " + e.getMessage(), SaxonErrorCode.SXJE0053);
        }
    }

    /**
     * Check that characters are valid XML characters (UTF-16 encoded)
     *
     * @param array   the array of characters
     * @param start   the position of the first significant character in the array
     * @param end     the position after the last significant character in the array
     * @param checker the NameChecker to be used (for XML 1.0 or XML 1.1 rules)
     * @throws XPathException if the string contains characters that are invalid in XML
     */

    private static void checkBytes(char[] array, int start, int end, IntPredicate checker) throws XPathException {
        int ch32 = UTF16CharacterSet.firstInvalidChar(
                StringTool.codePoints(new String(array, start, end - start)), checker);
        if (ch32 != -1) {
            XPathException err = new XPathException("The byte sequence contains a character not allowed by XML (hex " +
                    Integer.toHexString(ch32) + ')');
            err.setErrorCode("XTDE1180");
            throw err;
        }
    }

    /**
     * Convert a string to a hexBinary value in a given encoding
     *
     * @param in       the input string
     * @param encoding the desired encoding
     * @return the hexBinary value that results from encoding the string as a sequence of octets in the
     *         given encoding.
     * @throws XPathException in the event of a dynamic error, for example if the encoding is invalid or the
     * binary value is not valid in this encoding.
     */

    public static ZeroOrOne<HexBinaryValue> stringToHexBinary(ZeroOrOne<StringItem> in, One<StringItem> encoding) throws XPathException {
        try {
            if (in.head() == null) {
                return ZeroOrOne.empty();
            }
            ByteArrayOutputStream stream = new ByteArrayOutputStream(in.head().length32());
            OutputStreamWriter writer = new OutputStreamWriter(stream, encoding.head().getStringValue());
            writer.write(in.head().getStringValue());
            writer.close();
            byte[] bytes = stream.toByteArray();
            return new ZeroOrOne<>(new HexBinaryValue(bytes));
        } catch (IOException e) {
            throw new XPathException(e);
        }
    }

    /**
     * Test whether a given integer is the codepoint of a valid XML character
     *
     * @param c  the XPath dynamic context
     * @param in the character to be tested
     * @return true if and only if the character is valid in (the relevant version of) XML
     */

    public static boolean validCharacter(XPathContext c, int in) {
        return c.getConfiguration().getValidCharacterChecker().test(in);
    }
    
    /**
     * Create a parentless namespace node. This function is useful in XQuery when namespaces need to be created
     * dynamically. The effect is the same as that of the xsl:namespace instruction in XSLT.
     *
     * @param context the dynamic evaluation context
     * @param prefix  the name of the namespace node
     * @param uri     the string value of the namespace node
     * @return the newly constructed namespace node
     * @throws XPathException for example if the prefix is not a valid NCName, or if the URI is empty.
     */

    public static One<NodeInfo> namespaceNode(XPathContext context, One<StringItem> prefix, One<StringItem> uri) throws XPathException {
        String prefix0 = prefix.head().getStringValue();
        UnicodeString uri0 = uri.head().getUnicodeStringValue();
        if (!(prefix0.isEmpty() || NameChecker.isValidNCName(StringTool.codePoints(prefix0)))) {
            throw new XPathException("Namespace prefix " + Err.wrap(prefix0) + " is not a valid NCName");
        }
        if (uri0.isEmpty()) {
            throw new XPathException("URI of namespace node must not be empty");
        }
        Orphan node = new Orphan(context.getConfiguration());
        node.setNodeKind(Type.NAMESPACE);
        node.setNodeName(new NoNamespaceName(prefix0));
        node.setStringValue(uri0);
        return new One<>(node);
    }

    /**
     * Get a list of the names of the unparsed entities in a document
     *
     * @param doc the document node of the document whose unparsed entities are required
     * @return a sequence of strings containing the names of the unparsed entities
     */

    public static ZeroOrMore<StringItem> unparsedEntities(One<NodeInfo> doc) {
        Iterator<String> names = doc.head().getTreeInfo().getUnparsedEntityNames();
        List<StringItem> result = new ArrayList<>();
        while (names.hasNext()) {
            result.add(new StringItem(names.next()));
        }
        return new ZeroOrMore<>(result);
    }

    /**
     * Determine whether a given date/time is in summer time (daylight savings time)
     * in a given region. This relies on the Java database of changes to daylight savings time.
     * Since summer time changes are set by civil authorities the information is not necessarily
     * reliable when applied to dates in the future.
     *
     * @param context used to get the implicit timezone in the event that the supplied date/time
     *                has no timezone
     * @param date    the date/time in question. This should preferably include a timezone offset.
     *                If it does not, the implicit timezone from the dynamic context is used.
     * @param region  either the two-letter ISO country code, or an Olsen timezone name such as
     *                "America/New_York" or "Europe/Lisbon". If the country code denotes a country spanning several
     *                timezones, such as the US, then one of them is chosen arbitrarily.
     * @return true if the date/time is known to be in summer time in the relevant country;
     *         false if it is known not to be in summer time; null if an empty sequence was supplied
     *         for the dateTime argument, or if no information is available.
     */

    public static ZeroOrOne<BooleanValue> inSummerTime(XPathContext context, One<DateTimeValue> date, One<StringItem> region) {
        DateTimeValue dt = date.head();
        if (!dt.hasTimezone()) {
            dt = dt.adjustTimezone(context.getImplicitTimezone());
        }
        Boolean b = NamedTimeZone.inSummerTime(dt, region.head().getStringValue());
        if (b == null) {
            return ZeroOrOne.empty();
        }
        return new ZeroOrOne<>(BooleanValue.get(b));
    }

    /**
     * Parse a date/time value in a non-standard format
     * @param input the input string
     * @param format defines the input format, using the notation of the Java DateTimeFormatter class.
     *               For example "MM/dd/yyyy" for the popular US format.
     * @return the date time value represented
     * @throws XPathException if parsing fails
     */

    public static ZeroOrOne<CalendarValue> parseDateTime(ZeroOrOne<StringItem> input, One<StringItem> format) throws XPathException {
        DateTimeFormatter formatter;
        try {
            formatter = new DateTimeFormatterBuilder()
                    .parseCaseInsensitive()
                    .appendPattern(format.getStringValue())
                    .toFormatter()
                    .withResolverStyle(ResolverStyle.SMART);
        } catch (IllegalArgumentException e) {
            throw new XPathException("Invalid date/time pattern " + format.getUnicodeStringValue() + ": " + e.getMessage());
        }
        StringItem in = input.head();
        if (in == null) {
            return ZeroOrOne.empty();
        } else {
            TemporalAccessor ta;
            try {
                ta = formatter.parse(in.getStringValue());
            } catch (DateTimeParseException e) {
                throw new XPathException("Supplied date/time value " + in.getUnicodeStringValue() + " cannot be parsed using pattern " + format.getUnicodeStringValue() + ": " + e.getMessage());

            }
            boolean hasYear = ta.isSupported(ChronoField.YEAR);
            boolean hasMonth = ta.isSupported(ChronoField.MONTH_OF_YEAR);
            boolean hasDay = ta.isSupported(ChronoField.DAY_OF_MONTH);
            boolean hasHour = ta.isSupported(ChronoField.HOUR_OF_DAY);
            boolean hasMinute = ta.isSupported(ChronoField.MINUTE_OF_HOUR);
            boolean hasSecond = ta.isSupported(ChronoField.SECOND_OF_MINUTE);
            boolean hasNano = ta.isSupported(ChronoField.NANO_OF_SECOND);
            boolean hasTZ = ta.isSupported(ChronoField.OFFSET_SECONDS);
            int tz = hasTZ ? ta.get(ChronoField.OFFSET_SECONDS) / 60 : CalendarValue.NO_TIMEZONE;
            int year = hasYear ? ta.get(ChronoField.YEAR) : 0;
            byte month = hasMonth ? (byte)ta.get(ChronoField.MONTH_OF_YEAR) : 0;
            byte day = hasDay ? (byte)ta.get(ChronoField.DAY_OF_MONTH) : 0;
            byte hour = hasHour ? (byte)ta.get(ChronoField.HOUR_OF_DAY) : 0;
            byte minute = hasMinute ? (byte)ta.get(ChronoField.MINUTE_OF_HOUR) : 0;
            byte second = hasSecond ? (byte)ta.get(ChronoField.SECOND_OF_MINUTE) : 0;
            int nano = hasNano ? ta.get(ChronoField.NANO_OF_SECOND) : 0;
            CalendarValue result = null;
            if (hasYear && hasMonth && hasDay) {
                if (hasHour && hasMinute) {
                    result = new DateTimeValue(year, month, day, hour, minute, second, nano, tz);
                } else {
                    result = new DateValue(year, month, day, tz, false);
                }
            } else if (hasHour && hasMinute) {
                result = new TimeValue(hour, minute, second, nano, tz, "");
            } else if (hasYear && hasMonth) {
                result = new GYearMonthValue(year, month, tz, false);
            } else if (hasYear) {
                result = new GYearValue(year, tz, false);
            } else if (hasMonth && hasDay) {
                result = new GMonthDayValue(month, day, tz);
            } else if (hasMonth) {
                result = new GMonthValue(month, tz);
            } else if (hasDay) {
                result = new GDayValue(day, tz);
            }

            return new ZeroOrOne<>(result);
        }
    }

    /**
     * Adjust a given date/time to a specified civil time zone, taking account of summer time
     * (daylight savings time) changes, as obtained from the Java database of changes to daylight
     * savings time.
     * Since summer time changes are set by civil authorities the information is not necessarily
     * reliable when applied to dates in the future.
     *
     * @param context used to get the implicit timezone in the event that the supplied date/time
     *                has no timezone
     * @param date    the date/time in question. This should preferably include a timezone offset.
     *                If it does not, the implicit timezone from the dynamic context is used.
     * @param region  An Olsen timezone name such as "America/New_York" or "Europe/Lisbon".
     * @return The date/time adjusted to the timezone offset applicable to the place denoted by the
     *         Olsen timezone name. For example:
     *         <p><code>adjust-to-civil-time(xs:dateTime('2008-01-10T12:00:00Z', 'America/New_York')</code>
     *         returns <code>2008-01-10T07:00:00-05:00</code></p>
     *         <p><code>adjust-to-civil-time(xs:dateTime('2008-07-10T12:00:00Z', 'America/New_York')</code>
     *         returns <code>2008-07-10T08:00:00-04:00</code></p>
     */

    public static ZeroOrOne<DateTimeValue> adjustToCivilTime(XPathContext context, ZeroOrOne<DateTimeValue> date, One<StringItem> region) {
        DateTimeValue dt = date.head();
        if (dt == null) {
            return ZeroOrOne.empty();
        }
        if (!dt.hasTimezone()) {
            dt = dt.adjustTimezone(context.getImplicitTimezone());
        }
        int ms = NamedTimeZone.civilTimeOffset(dt, region.head().getStringValue());
        return new ZeroOrOne<>(dt.adjustTimezone(ms / 60000));
    }


    /**
     * Get the current mode. Returns the name of the current mode if there is one.
     * Returns null (delivered as an empty sequence) if there is no current mode or if the
     * current mode is the unnamed (default) mode.
     * @param context the dynamic context
     * @return the current mode name, or empty if there is none
     */

    /*@Nullable*/
    public static ZeroOrOne<QNameValue> currentModeName(XPathContext context) {
        Component.M mode = context.getCurrentMode();
        if (mode == null) {
            return ZeroOrOne.empty();
        }
        StructuredQName name = mode.getActor().getModeName();
        if (name == null || name.equals(Mode.UNNAMED_MODE_NAME)) {
            return ZeroOrOne.empty();
        }
        return new ZeroOrOne<>(new QNameValue(name, BuiltInAtomicType.QNAME));
    }

    /**
     * Ask whether an attribute node exists as a result of the expansion of
     * default and fixed values defined in a schema. Note that this property will only be set if both the configuration properties
     * {@link FeatureKeys#EXPAND_ATTRIBUTE_DEFAULTS} and {@link FeatureKeys#MARK_DEFAULTED_ATTRIBUTES}
     * are set. The property is currently maintained only for attribute nodes in a TinyTree that results from schema
     * validation, or where DTD-defined defaults are notified by the XML parser.
     *
     * @param arg the attribute node in question, or null
     * @return null if the argument is null;
     * true if the node is an attribute node that exists as a result of the expansion of default and fixed
     * values defined in a schema; provided that configuration options are defined to maintain this property. In all
     * other cases, false.
     */

    /*@Nullable*/
    public static ZeroOrOne<BooleanValue> isDefaulted(ZeroOrOne<NodeInfo> arg) {
        NodeInfo node = arg.head();
        if (node == null) {
            return ZeroOrOne.empty();
        }
        boolean result = node instanceof TinyAttributeImpl && ((TinyAttributeImpl)node).isDefaultedAttribute();
        return new One<>(BooleanValue.get(result));
    }


}

// Copyright (c) 2009-2020 Saxonica Limited