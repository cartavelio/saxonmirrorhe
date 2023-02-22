using System;
using Saxon.Hej.s9api;

namespace Saxon.Api {
    /// <summary>
    /// An <c>XdmDestination</c> is a <c>Destination</c> in which an <c>XdmNode</c> 
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
    public class XdmDestination : IDestination {
        private readonly Saxon.Hej.s9api.XdmDestination jDestination;
        //internal Action<XdmNode> closeAction;

        /// <summary>Construct an <c>XdmDestination</c></summary>
        public XdmDestination() {
            jDestination = new Saxon.Hej.s9api.XdmDestination();
        }

        ///<summary>
        /// The Tree Model implementation to be used for the constructed document. By default
        /// the <c>TinyTree</c> is used. The main reason for using the <c>LinkedTree</c> alternative is if
        /// updating is required (the <c>TinyTree</c> is not updateable).
        ///</summary>

        public TreeModel TreeModel {
            get => (TreeModel) jDestination.getTreeModel().getSymbolicValue();
            set => jDestination.setTreeModel(Saxon.Hej.om.TreeModel.getTreeModel((int) value));
        }

        /// <summary>This property determines the base URI of the constructed <c>XdmNode</c>. 
        /// If the <c>BaseUri</c> property of the <c>XdmDestination</c> is set before the destination is written to,
        /// then the constructed <c>XdmNode</c> will have this base URI. Setting this property after constructing the node
        /// has no effect.
        /// </summary>

        public Uri BaseUri {
            get => jDestination.getBaseURI();
            set => jDestination.setBaseURI(value);
        }


        /// <summary>Reset the state of the <c>XdmDestination</c> so that it can be used to hold
        /// the result of another query or transformation.</summary>
        public void Reset() {
            jDestination.reset();
        }

        /// <summary>After construction, retrieve the constructed document node.</summary>
        /// <remarks>
        /// <para>The value of the property will be null if no data has been written to the
        /// <c>XdmDestination</c>, either because the process that writes to the destination has not
        /// yet been run, or because the process produced no output.</para>
        /// </remarks>

        public XdmNode XdmNode => jDestination.getXdmNode();


        /// <summary>
        /// Get the underlying Saxon <c>Destination</c> object from the <c>XdmDestination</c>.
        /// This method is for internal use but is provided for the benefit of applications that need to mix
        /// use of the Saxon .NET API with direct use of the underlying objects
        /// and methods offered by the Java implementation.
        /// </summary>
        public Destination UnderlyingDestination {
            get { return jDestination; }
        }

        /// <summary>
        /// Set the base URI of the resource being written to this destination
        /// </summary>
        /// <param name="uri">the base URI to be used</param>
        public void setDestinationBaseURI(Uri uri) {
            jDestination.setDestinationBaseURI(uri);
        }

        /// <summary>
        /// Get the base URI of the resource being written to this destination
        /// </summary>
        /// <returns>the base URI, or null if none is known</returns>
        public Uri getDestinationBaseURI() {
            return jDestination.getDestinationBaseURI();
        }


        /// <summary>
        /// Close the destination, allowing resources to be released. Saxon calls this
        /// method when it has finished writing to the destination.
        /// </summary>
        public void Close() {
            jDestination.close();
        }

        /// <summary>
        /// Define an action to be performed when writing to the Destination is complete.
        /// </summary>
        /// <param name="action">The action to be performed.</param>
        public void OnClose(System.Action action) {
            jDestination.onClose(action.Invoke);
        }
        
    }
}

// Copyright (c) 2021 Saxonica Limited