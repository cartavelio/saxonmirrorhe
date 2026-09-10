using System;
using System.IO;
using Saxon.Ejava.util;
using Saxon.Hej;
using Saxon.Hej.serialize;
using Saxon.Hej.z;
using SysCollGen = System.Collections.Generic;
using System.Collections.Generic;

namespace Saxon.Api {
    /// <summary>
    /// A <c>Serializer</c> takes a tree representation of XML and turns
    /// it into lexical XML markup.
    /// </summary>
    /// <remarks>
    /// Note that this is serialization in the sense of the W3C XSLT and XQuery specifications.
    /// Unlike the class <c>System.Xml.Serialization.XmlSerializer</c>, this object does not
    /// serialize arbitrary CLI objects.
    /// </remarks>

    public class Serializer : IDestination
    {

        private readonly Saxon.Hej.s9api.Serializer jSerializer;
        private readonly Properties props = new();

        /// <summary>QName identifying the serialization parameter "method". If the value
        /// is a user-defined method, then it is given as a QName in Clark notation, that is
        /// "{uri}local".</summary>

        public static readonly QName METHOD =
            new QName("", "method");

        /// <summary>QName identifying the serialization parameter "byte-order-mark".
        /// The value is the string "yes" or "no".</summary>

        public static readonly QName BYTE_ORDER_MARK =
            new QName("", "byte-order-mark");

        /// <summary>QName identifying the serialization parameter "cdata-section-elements".
        /// The value of this parameter is given as a space-separated list of expanded QNames in
        /// Clark notation, that is "{uri}local".</summary>

        public static readonly QName CDATA_SECTION_ELEMENTS =
            new QName("", "cdata-section-elements");

        /// <summary>QName identifying the serialization parameter "doctype-public"</summary>

        public static readonly QName DOCTYPE_PUBLIC =
            new QName("", "doctype-public");

        /// <summary>QName identifying the serialization parameter "doctype-system"</summary>

        public static readonly QName DOCTYPE_SYSTEM =
            new QName("", "doctype-system");

        /// <summary>QName identifying the serialization parameter "encoding"</summary>

        public static readonly QName ENCODING =
            new QName("", "encoding");

        /// <summary>QName identifying the serialization parameter "escape-uri-attributes".
        /// The value is the string "yes" or "no".</summary>

        public static readonly QName ESCAPE_URI_ATTRIBUTES =
            new QName("", "escape-uri-attributes");


        /// <summary>QName identifying the serialization parameter "include-content-type".
        /// The value is the string "yes" or "no".</summary>

        public static readonly QName INCLUDE_CONTENT_TYPE =
            new QName("", "include-content-type");

        /// <summary>QName identifying the serialization parameter "indent".
        /// The value is the string "yes" or "no".</summary>

        public static readonly QName INDENT =
            new QName("", "indent");

        /// <summary>QName identifying the serialization parameter "media-type".</summary>

        public static readonly QName MEDIA_TYPE =
            new QName("", "media-type");

        /// <summary>QName identifying the serialization parameter "normalization-form"</summary>

        public static readonly QName NORMALIZATION_FORM =
            new QName("", "normalization-form");

        /// <summary>
        /// Set to a string used to separate adjacent items in an XQuery result sequence
        /// </summary>
        public static readonly QName ITEM_SEPARATOR =
            new QName("", "item-separator");


        /// <summary>
        /// HTML version number
        /// </summary>
        public static readonly QName HTML_VERSION =
            new QName("" , "html-version");


        /// <summary>
        /// Build-tree option (XSLT only)
        /// </summary>
        public static readonly QName BUILD_TREE =
            new QName("", "build-tree");

        /// <summary>QName identifying the serialization parameter "omit-xml-declaration".
        /// The value is the string "yes" or "no".</summary>

        public static readonly QName OMIT_XML_DECLARATION =
            new QName("", "omit-xml-declaration");

        /// <summary>QName identifying the serialization parameter "standalone".
        /// The value is the string "yes" or "no" or "omit".</summary>

        public static readonly QName STANDALONE =
            new QName("", "standalone");

        /// <summary>QName identifying the serialization parameter "suppress-indentation"
        /// (introduced in XSLT/XQuery 3.0). Previously available as "saxon:suppress-indentation".
        /// The value is the string "yes" or "no".</summary>

        public static readonly QName SUPPRESS_INDENTATION =
            new QName("", "suppress-indentation");

        /// <summary>QName identifying the serialization parameter "undeclare-prefixes".
        /// The value is the string "yes" or "no".</summary>

        public static readonly QName UNDECLARE_PREFIXES =
            new QName("", "undeclare-prefixes");

        /// <summary>QName identifying the serialization parameter "use-character-maps".
        /// The value of the parameter is a list of expanded QNames
        /// in Clark notation giving the names of character maps defined in the XSLT stylesheet,
        /// or supplied programmatically.</summary>

        public static readonly QName USE_CHARACTER_MAPS =
            new QName("", "use-character-maps");

        /// <summary>QName identifying the serialization parameter "version"</summary>

        public static readonly QName VERSION =
            new QName("", "version");

        private static readonly string SAXON = NamespaceConstant.SAXON;


        /// <summary>QName identifying the serialization parameter "saxon:character-representation"</summary>


        public static readonly QName SAXON_CHARACTER_REPRESENTATION =
            new QName(SAXON, "saxon:character-representation");


        /// <summary>
        /// Saxon extension for use when writing to the text output method; this option causes the processing
        /// instructions hex and b64 to be recognized containing hexBinary or base64 data respectively.
        /// </summary>
        public static readonly QName SAXON_RECOGNIZE_BINARY =
            new QName("", "saxon:recognize-binary");

        /// <summary>QName identifying the serialization parameter "saxon:indent-spaces". The value
        /// is an integer (represented as a string) indicating the amount of indentation required.
        /// If specified, this parameter overrides indent="no".</summary>

        public static readonly QName SAXON_INDENT_SPACES =
            new QName(SAXON, "saxon:indent-spaces");


        /// <summary>
        /// Saxon extension: set to an integer (represented as a string) giving the desired maximum
        /// length of lines when indenting.Default is 80.
        /// </summary>
        public static readonly QName SAXON_LINE_LENGTH =
            new QName("", "saxon:line-length");


        /// <summary>
        /// Saxon extension: set to a space-separated list of attribute names, in Clark notation,
        /// indicating that attributes present in the list should be serialized in the order
        /// indicated, followed by attributes not present in the list(these are sorted first
        /// by namespace, then by local name).
        /// </summary>
        public static readonly QName SAXON_ATTRIBUTE_ORDER =
            new QName("", "saxon:attribute-order");

        /// <summary>
        ///  Saxon extension: request canonical XML output.
        /// </summary>
        public static readonly QName SAXON_CANONICAL =
            new QName("", "saxon:canonical");

        /// <summary>
        /// Saxon extension: set to any string. Indicates the sequence of characters used to represent
        /// a newline in the text output method, and in newlines used for indentation in any output
        /// methods that use indentation.
        /// </summary>
        public static readonly QName SAXON_NEWLINE =
            new QName("", "saxon:newline");


        /// <summary>
        /// Saxon extension for internal use: used in XSLT to tell the serializer whether the
        /// stylesheet used version="1.0" or version = "2.0"
        /// </summary>
        public static readonly QName SAXON_STYLESHEET_VERSION =
            new QName("", "saxon:stylesheet-version");

        /// <summary>QName identifying the serialization parameter "saxon:double-space". The value of this 
        /// parameter is given as a space-separated list of expanded QNames in Clark notation, that is 
        /// "{uri}local"; each QName identifies an element that should be preceded by an extra blank line within
        /// indented output.</summary>

        public static readonly QName SAXON_DOUBLE_SPACE =
            new QName(SAXON, "saxon:double-space");

        /// <summary>QName identifying the serialization parameter "suppress-indentation". Retained
        /// as a synonym of SUPPRESS_INDENTATION for backwards compatibility.</summary>

        public static readonly QName SAXON_SUPPRESS_INDENTATION = SUPPRESS_INDENTATION;

        /// <summary>QName identifying the serialization parameter "saxon:next-in-chain". This
        /// is available only with XSLT, and identifies the URI of a stylesheet that is to be used to
        /// process the results before passing them to their final destination.</summary>

        public static readonly QName NEXT_IN_CHAIN =
            new QName(SAXON, "saxon:next-in-chain");

        /// <summary>QName identifying the serialization parameter "saxon:require-well-formed". The
        /// value is the string "yes" or "no". If set to "yes", the output must be a well-formed
        /// document, or an error will be reported. ("Well-formed" here means that the document node
        /// must have exactly one element child, and no text node children other than whitespace-only
        /// text nodes).</summary>

        public static readonly QName SAXON_REQUIRE_WELL_FORMED =
            new QName(SAXON, "saxon:require-well-formed");


        /// <summary>
        /// Saxon extension for interfacing with debuggers; indicates that the location information is
        /// available for events in this output stream
        /// </summary>
        public static readonly QName SUPPLY_SOURCE_LOCATOR =
            new QName("", "supply-source-locator");


        /// <summary>
        /// Saxon extension, indicates that the output of a query is to be wrapped before serialization,
        /// such that each item in the result sequence is enclosed in an element indicating its type
        /// </summary>
        public static readonly QName SAXON_WRAP =
            new QName("", "", "wrap-result-sequence");

        /// <summary>Create a Serializer</summary>

        internal Serializer(Processor processor, Saxon.Hej.s9api.Serializer s) {
            this.Processor = processor;
            jSerializer = s;
        }

        /// <summary>The Processor from which this <c>Serializer</c> was created.</summary>

        public Processor Processor { get; private set; } = null;

        /// <summary>Set a serialization property</summary>
        /// <remarks>In the case of XSLT, properties set within the serializer override
        /// any properties set in <c>xsl:output</c> declarations in the stylesheet.
        /// Similarly, with XQuery, they override any properties set in the Query
        /// prolog using <c>declare option output</c>.</remarks>
        /// <example>
        ///   <code>
        ///     Serializer qout = processor.NewSerializer();
        ///     qout.SetOutputProperty(Serializer.METHOD, "xml");
        ///     qout.SetOutputProperty(Serializer.INDENT, "yes");
        ///     qout.SetOutputProperty(Serializer.SAXON_INDENT_SPACES, "1");
        ///   </code>
        /// </example> 
        /// <param name="name">The name of the serialization property to be set</param>
        /// <param name="value">The value to be set for the serialization property. May be null
        /// to unset the property (that is, to set it back to the default value).</param>

        public void SetOutputProperty(QName name, string value)
        {
            props.setProperty(name.ClarkName, value);
            jSerializer.setOutputProperty(Saxon.Hej.s9api.Serializer.getProperty(name), value);
        }

        /// <summary>
        /// Get the character map with a given name
        /// </summary>
        /// <param name="name">The name of the character map, as a QName</param>
        /// <returns>A dictionary that maps integer Unicode codepoint values to strings</returns>
        
        public IDictionary<int, string> GetCharacterMap(QName name) {
            Saxon.Hej.serialize.CharacterMap map = 
                    jSerializer.getCharacterMapIndex().getCharacterMap(name.GetStructuredQName());
            if (map != null) {
                IntHashMap<string> content = map.getMap();
                SysCollGen.Dictionary<int, string> result = new();
                IntIterator iter = content.keyIterator();
                while (iter.hasNext()) {
                    int key = iter.next();
                    result.Add(key, content.get(key));
                }
                return result;
            }
            return null;
        }

        /// <summary>
        /// Set a character map to be used
        /// </summary>
        /// <remarks>
        /// <para>Note that the character map is used by the serializer only if the
        /// <c>USE_CHARACTER_MAPS</c> output property includes the name of this character map.
        /// </para>
        /// </remarks>
        /// <param name="name">the name of the character map</param>
        /// <param name="charMap">the content of the character map, supplied as
        /// a dictionary that maps integer Unicode codepoint values to strings</param>

        public void SetCharacterMap(QName name, IDictionary<int, string> charMap) {
            IntHashMap<string> content = new IntHashMap<string>();
            foreach(SysCollGen.KeyValuePair<int, string> pair in charMap) {
                content.put(pair.Key, pair.Value);
            }

            CharacterMap map = new CharacterMap(name.GetStructuredQName(), content);
            jSerializer.getCharacterMapIndex().putCharacterMap(name.GetStructuredQName(), map);
        }


        /// <summary>Specify the destination of the serialized output, in the
        /// form of a file name</summary>
        /// <value>
        ///     The name of the file to receive the serialized output. This
        ///     method sets the destination base URI to the URI corresponding to the name of the supplied file.
        /// </value>
        /// <exception cref="SaxonApiException">Throws a <c>SaxonApiException</c> if it is not possible to 
        /// create an output stream to write to this file, for example, if the filename is in a
        /// directory that does not exist.</exception>
        public string OutputFile {
            set {
                try {
                    jSerializer.setOutputFile(new Saxon.Ejava.io.File(value));
                }
                catch (Saxon.Ejava.io.IOException err) {
                    throw new SaxonApiException(err);
                }
            }
        }


        /// <summary>Specify the destination of the serialized output, in the
        /// form of a <c>Stream</c>. This must be a stream that allows writing.</summary>
        /// <remarks>Saxon will not close the stream on completion; this is the
        /// caller's responsibility.</remarks>

        public Stream OutputStream {
            set => jSerializer.setOutputStream(value);
        }

        /// <summary>Specify the destination of the serialized output, in the
        /// form of a <c>TextWriter</c>.</summary>
        /// <remarks>
        /// <para>Note that when writing to a <c>TextWriter</c>, character encoding is
        /// the responsibility of the <c>TextWriter</c>, not the <c>Serializer</c>. This
        /// means that the encoding requested in the output properties is ignored; it also
        /// means that characters that cannot be represented in the target encoding will
        /// use whatever fallback representation the <c>TextWriter</c> defines, rather than
        /// being represented as XML character references.</para>
        /// <para>
        /// Saxon will not close the
        /// <c>TextWriter</c> on completion; this is the caller's responsibility.
        /// </para>
        /// </remarks>

        public TextWriter OutputWriter {
            set => jSerializer.setOutputWriter(value);
        }


        /// <summary>
        /// Serialize an <c>XdmNode</c> to the selected output destination using this serializer.
        /// </summary>
        /// <param name="node">The node to be serialized</param>
        /// <remarks>since 9.8</remarks>
        public void SerializeXdmNode(XdmNode node)
        {
            jSerializer.serializeNode(node);
        }


        /// <summary>
        /// Serialize an arbitrary <c>XdmValue</c> to the selected output destination using this serializer.
        /// The supplied sequence is first wrapped in a document node according to the rules given in section 2
        /// (Sequence Normalization) of the XSLT/XQuery serialization specification; the resulting document node 
        /// is then serialized using the serialization parameters defined in this serializer. A call on this 
        /// method will close the writer or output stream internally.
        /// </summary>
        /// <param name="value"> The value to be serialized</param>
        /// <remarks>since 9.8</remarks>
        public void SerializeXdmValue(XdmValue value) {
            if (value is XdmNode node)
            {
                SerializeXdmNode(node);  
            }
            else {
                jSerializer.serializeXdmValue(value);
            }

        }

        
        /// <summary>
        /// Close any resources associated with this destination. Note that this does <b>not</b>
        /// close any user-supplied OutputStream or Writer; those must be closed explicitly
        /// by the calling application.
        /// </summary>
        public void Close()
        {
            jSerializer.close();
        }
        
        /// <summary>
        /// Define an action to be performed when writing to the Destination is complete.
        /// </summary>
        /// <param name="action">The action to be performed.</param>
        public void OnClose(System.Action action) {
            jSerializer.onClose(action.Invoke);
        }


        /// <summary>
        /// The underlying <c>Saxon.Hej.s9api.Destination</c> object in the Saxon implementation
        /// </summary>
        /// <remarks>
        /// <para>This property provides access to internal methods in the Saxon engine that are
        /// not specifically exposed in the .NET API. In general these methods should be
        /// considered to be less stable than the classes in the Saxon.Api namespace.</para> 
        /// <para>The internal methods follow
        /// Java naming conventions rather than .NET conventions.</para>
        /// <para>Information about the returned <see cref="Saxon.Hej.s9api.Destination"/> object 
        /// (and the objects it provides access to) is included in the Saxon JavaDoc documentation.
        /// </para>
        /// </remarks>
        /// <value>The underlying Destination object</value>
        public Saxon.Hej.s9api.Destination UnderlyingDestination => jSerializer;


        /// <summary>This property determines the base Uri of the constructed <c>Serializer</c>. 
        /// </summary>

        public Uri BaseUri
        {
            get => new Uri(jSerializer.getDestinationBaseURI().OriginalString);
            set => jSerializer.setDestinationBaseURI(new Uri(value.OriginalString));
        }



    }
}

// Copyright (c) 2021 - 2023 Saxonica Limited