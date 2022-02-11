using System;
using System.Reflection.Metadata;
using net.sf.saxon.@event;
using net.sf.saxon.om;
using net.sf.saxon.s9api;
using net.sf.saxon.str;
using net.sf.saxon.trans;
using net.sf.saxon.type;
using Action = net.sf.saxon.s9api.Action;

namespace Saxon.Api {
    /// <summary>
    /// An <c>XdmDestination</c> is an <c>XmlDestination</c> in which an <c>XdmNode</c> 
    /// is constructed to hold the output of a query or transformation: 
    /// that is, a tree using Saxon's implementation of the XDM data model.
    /// </summary>
    /// <remarks>
    /// <para>No data needs to be supplied to the <c>XdmDestination</c> object. The query or transformation
    /// populates an <c>XmlNode</c>, which may then be retrieved as the value of the <c>XmlNode</c>
    /// property.</para>
    /// <para>An <c>XdmDestination</c> can be reused to hold the results of a second transformation only
    /// if the <c>Reset</c> method is first called to reset its state.</para>
    /// </remarks>
    public class XdmDestination : XmlDestination {
        internal net.sf.saxon.s9api.XdmDestination destination;
        internal Action<XdmNode> closeAction;

        /// <summary>Construct an <c>XdmDestination</c></summary>
        public XdmDestination() {
            destination = new net.sf.saxon.s9api.XdmDestination();
        }

        ///<summary>
        /// The Tree Model implementation to be used for the constructed document. By default
        /// the <c>TinyTree</c> is used. The main reason for using the <c>LinkedTree</c> alternative is if
        /// updating is required (the <c>TinyTree</c> is not updateable).
        ///</summary>

        public TreeModel TreeModel {
            get => (TreeModel) destination.getTreeModel().getSymbolicValue();
            set => destination.setTreeModel(net.sf.saxon.om.TreeModel.getTreeModel((int) value));
        }

        /// <summary>This property determines the base URI of the constructed <c>XdmNode</c>. 
        /// If the <c>BaseUri</c> property of the <c>XdmDestination</c> is set before the destination is written to,
        /// then the constructed <c>XdmNode</c> will have this base URI. Setting this property after constructing the node
        /// has no effect.
        /// </summary>

        public Uri BaseUri {
            get => destination.getBaseURI();
            set => destination.setBaseURI(value);
        }


        /// <summary>Reset the state of the <c>XdmDestination</c> so that it can be used to hold
        /// the result of another query or transformation.</summary>
        public void Reset() {
            destination.reset();
        }

        /// <summary>After construction, retrieve the constructed document node.</summary>
        /// <remarks>
        /// <para>The value of the property will be null if no data has been written to the
        /// <c>XdmDestination</c>, either because the process that writes to the destination has not
        /// yet been run, or because the process produced no output.</para>
        /// </remarks>

        public XdmNode XdmNode => destination.getXdmNode();


        /// <summary>
        /// Get the underlying Saxon <c>Destination</c> object from the <c>XdmDestination</c>.
        /// This method is for internal use but is provided for the benefit of applications that need to mix
        /// use of the Saxon .NET API with direct use of the underlying objects
        /// and methods offered by the Java implementation.
        /// </summary>
        public Destination GetUnderlyingDestination() {
            return destination;
        }

        /// <summary>
        /// Set the base URI of the resource being written to this destination
        /// </summary>
        /// <param name="uri">the base URI to be used</param>
        public void setDestinationBaseURI(Uri uri) {
            destination.setDestinationBaseURI(uri);
        }

        /// <summary>
        /// Get the base URI of the resource being written to this destination
        /// </summary>
        /// <returns>the base URI, or null if none is known</returns>
        public Uri getDestinationBaseURI() {
            return destination.getDestinationBaseURI();
        }


        /// <summary>
        /// Close the destination, allowing resources to be released. Saxon calls this
        /// method when it has finished writing to the destination.
        /// </summary>
        public void Close() {
            destination.close();
        }

        /// <summary>
        /// Define an action to be performed when writing to the Destination is complete.
        /// </summary>
        /// <param name="action">The action to be performed.</param>
        public void OnClose(System.Action action) {
            destination.onClose(action.Invoke);
        }


        // /// <summary>
        // /// <c>TreeProtector</c> is a filter that ensures that the events reaching the <c>Builder</c> constitute a single
        // /// tree rooted at an element or document node (because anything else will crash the builder)
        // /// </summary>
        // public class TreeProtector : ProxyReceiver {
        //     private int level = 0;
        //     private bool ended = false;
        //
        //     /// <summary>
        //     /// Constructor
        //     /// </summary>
        //     /// <param name="next">Set the underlying receiver</param>
        //     public TreeProtector(Receiver next)
        //         : base(next) {
        //     }
        //
        //     /// <summary>
        //     /// Start of a document node
        //     /// </summary>
        //     /// <param name="properties"></param>
        //     public override void startDocument(int properties) {
        //         if (ended) {
        //             XPathException e = new XPathException("Only a single document can be written to an XdmDestination");
        //             throw new DynamicError(e);
        //         }
        //
        //         base.startDocument(properties);
        //         level++;
        //     }
        //
        //     /// <summary>
        //     /// Notify the end of a document node
        //     /// </summary>
        //     public override void endDocument() {
        //         base.endDocument();
        //         level--;
        //         if (level == 0) {
        //             ended = true;
        //         }
        //     }
        //
        //     /// <summary>
        //     /// Notify the start of an element
        //     /// </summary>
        //     /// <param name="nameCode"></param>
        //     /// <param name="typeCode"></param>
        //     /// <param name="attributes"></param>
        //     /// <param name="namespaces"></param>
        //     /// <param name="location"></param>
        //     /// <param name="properties"></param>
        //     public override void startElement(NodeName nameCode, SchemaType typeCode, AttributeMap attributes,
        //         NamespaceMap namespaces, Location location, int properties) {
        //         if (ended) {
        //             XPathException e =
        //                 new XPathException("Only a single root node can be written to an XdmDestination");
        //             throw new DynamicError(e);
        //         }
        //
        //         base.startElement(nameCode, typeCode, attributes, namespaces, location, properties);
        //         level++;
        //     }
        //
        //     /// <summary>
        //     /// End of element
        //     /// </summary>
        //     public override void endElement() {
        //         base.endElement();
        //         level--;
        //         if (level == 0) {
        //             ended = true;
        //         }
        //     }
        //
        //     /// <summary>
        //     /// Character data
        //     /// </summary>
        //     /// <param name="chars">Character data as input</param>
        //     /// <param name="location">Provides information such as line number and system ID</param>
        //     /// <param name="properties">Bit significant value. The following bits are defined</param>
        //     public override void characters(CodePoints chars, Location location, int properties) {
        //         if (level == 0) {
        //             XPathException e = new XPathException(
        //                 "When writing to an XdmDestination, text nodes are only allowed within a document or element node");
        //             throw new DynamicError(e);
        //         }
        //
        //         base.characters(chars, location, properties);
        //     }
        //
        //     /// <summary>
        //     /// Processing instruction
        //     /// </summary>
        //     /// <param name="target">The PI name. This must be a legal name (it will not be checked)</param>
        //     /// <param name="data">The data portion of the processing instruction</param>
        //     /// <param name="location">provides information about the PI</param>
        //     /// <param name="properties">Additional information about the PI</param>
        //     public override void processingInstruction(String target, CodePoints data, Location location,
        //         int properties) {
        //         if (level == 0) {
        //             XPathException e = new XPathException(
        //                 "When writing to an XdmDestination, processing instructions are only allowed within a document or element node");
        //             throw new DynamicError(e);
        //         }
        //
        //         base.processingInstruction(target, data, location, properties);
        //     }
        //
        //     /// <summary>
        //     /// Output a comment
        //     /// </summary>
        //     /// <param name="chars">The content of the comment</param>
        //     /// <param name="location">provides information such as line number and system ID</param>
        //     /// <param name="properties">Additional information about the comment</param>
        //     public override void comment(CodePoints chars, Location location, int properties) {
        //         if (level == 0) {
        //             XPathException e = new XPathException(
        //                 "When writing to an XdmDestination, comment nodes are only allowed within a document or element node");
        //         }
        //
        //         base.comment(chars, location, properties);
        //     }
        //
        //
        //     /// <summary>
        //     /// Append an  arbitrary item (node or atomic value) to the output
        //     /// </summary>
        //     /// <param name="item">the item to be appended</param>
        //     /// <param name="location">the location of the calling instruction, for diagnostics</param>
        //     /// <param name="copyNamespaces">if the item is an element node, this indicates whether its namespace need to be copied. 
        //     /// 0x80000 means ALL_NAMESPACE, 0x40000 means LOCAL_NAMESPACE and 0 means no namespace</param>
        //     public override void append(Item item, Location location, int copyNamespaces) {
        //         if (level == 0) {
        //             XPathException e = new XPathException(
        //                 "When writing to an XdmDestination, atomic values are only allowed within a document or element node");
        //         }
        //
        //         base.append(item, location, copyNamespaces);
        //     }
        // }
    }
}