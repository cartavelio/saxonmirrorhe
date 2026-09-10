using System.Xml;
using Saxon.Hej.@event;
using Saxon.Hej.s9api;
using Saxon.Hej.serialize;
using Saxon.Impl.Helpers;

namespace Saxon.Api {
    /// <summary>
    /// An <c>XmlWriterDestination</c> is an implementation of <c>XmlDestination</c> that wraps
    /// an instance of <c>XmlWriter</c>.
    /// </summary>
    /// <remarks>
    /// <para>The class was previously named <c>TextWriterDestination</c>, but this was a misnomer.</para>
    /// <para>Note that when an <c>XmlWriterDestination</c> is used to process the output of a stylesheet
    /// or query, the output format depends only on the way the underlying <c>XmlWriter</c>
    /// is configured; serialization parameters present in the stylesheet or query are ignored.
    /// The XSLT <c>disable-output-escaping</c> option is also ignored. If serialization
    /// is to be controlled from the stylesheet or query, use a <c>Serializer</c> as the
    /// <c>Destination</c>.</para>
    /// </remarks>
	public class XmlWriterDestination : Saxon.Hej.s9api.AbstractDestination, IDestination {
        private readonly XmlWriter writer;
        private bool closeAfterUse = true;

        /// <summary>Construct a <c>XmlWriterDestination</c></summary>
        /// <param name="writer">The <c>XmlWriter</c> that is to be notified of the events
        /// representing the XML document.</param>
        public XmlWriterDestination(XmlWriter writer) {
            this.writer = writer;
        }

        /// <summary>
        /// The <c>CloseAfterUse</c> property indicates whether the underlying <c>XmlWriter</c> is closed
        /// (by calling its <c>Close()</c> method) when Saxon has finished writing to it. The default
        /// value is true, in which case <c>Close()</c> is called. If the property is set to <c>false</c>,
        /// Saxon will refrain from calling the <c>Close()</c> method, and merely call <c>Flush()</c>,
        /// which can be useful if further output is to be written to the <c>XmlWriter</c> by the application.
        /// </summary>

        public bool CloseAfterUse {
            get => closeAfterUse;
            set => closeAfterUse = value;
        }

        /// <summary>
        /// Close any resources associated with this destination. Note that this does <b>not</b>
        /// close any user-supplied OutputStream or Writer; those must be closed explicitly
        /// by the calling application.
        /// </summary>
        public override void close() {
            writer.Close();
        }

        /// <summary>
        /// Define an action to be performed when writing to the Destination is complete.
        /// </summary>
        /// <param name="action">The action to be performed.</param>
        public void OnClose(System.Action action) {
            onClose(action.Invoke);
        }


        /// <summary>
        /// Return a <c>Receiver</c>. Saxon calls this method to obtain a <c>Receiver</c>, to which it then sends
        /// a sequence of events representing the content of an XML document. The method is intended
        /// primarily for internal use, and may give poor diagnostics if used incorrectly.
        /// </summary>
        /// <returns>The receiver</returns>
        /// <param name="pipe">The Saxon configuration as a <c>JPipelineConfiguration</c>. 
        /// This is supplied so that the destination can
        /// use information from the configuration (for example, a reference to the name pool)
        /// to construct or configure the returned <c>Receiver</c>.</param>
        /// <param name="params1"></param>
        private Receiver GetReceiver(PipelineConfiguration pipe, SerializationProperties params1) {
            XmlWriterReceiver dnr = new (writer, pipe);
            dnr.setPipelineConfiguration(pipe);
            dnr.setCloseAfterUse(closeAfterUse);
            NamespaceDifferencer delta = new NamespaceDifferencer(dnr, params1.getProperties());
            return params1.makeSequenceNormalizer(delta);
        }

        /// <summary>
        /// Return a <c>Receiver</c>. Saxon calls this method to obtain a Java <c>Receiver</c>, to which it then sends
        /// a sequence of events representing the content of an XML document. The method is intended
        /// primarily for internal use, and may give poor diagnostics if used incorrectly.
        /// </summary>
        /// <returns>The receiver</returns>
        /// <param name="pipe">The Saxon configuration as a <c>JPipelineConfiguration</c>. 
        /// This is supplied so that the destination can
        /// use information from the configuration (for example, a reference to the name pool)
        /// to construct or configure the returned <c>Receiver</c>.</param>
        /// <param name="value2">Serialization parameters known to the caller of the method; typically, output
        ///  properties defined in a stylesheet or query. These will mainly be of interest if the destination is performing serialization, but
        ///  some properties (such as <c>item-separator</c>) are also used in other situations.</param>
		public override Saxon.Hej.@event.Receiver getReceiver(PipelineConfiguration pipe, SerializationProperties value2) {
            return GetReceiver(pipe, value2);
        }

        /// <summary>
        /// The underlying <c>Destination</c> object in the Saxon implementation
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
        public Saxon.Hej.s9api.Destination UnderlyingDestination {
            get { return this; }
        }
    }
}

// Copyright (c) 2021 Saxonica Limited