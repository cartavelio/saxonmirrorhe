using System;
using Saxon.Hej.s9api;

namespace Saxon.Api {
    /// <summary>
    /// A <c>RawDestination</c> is an <c>Destination</c> that accepts a sequence output
    /// by a stylesheet or query and returns it directly as an <c>XdmValue</c>, without
    /// constructing an XML tree, and without serialization. It corresponds to the serialization
    /// option <code>build-tree="no"</code>.
    /// </summary>
    public class RawDestination : IDestination {
        private readonly Saxon.Hej.s9api.RawDestination destination;

        /// <summary>Construct a <c>RawDestination</c></summary>
        public RawDestination() {
            destination = new Saxon.Hej.s9api.RawDestination();
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
        /// <value>returns the underlying Destination object</value>
        public Destination UnderlyingDestination {
            get { return destination; }
        }

        /// <summary>
        /// Close the destination and notify all registered listeners that it has been closed.
        /// This method is intended for internal use by Saxon. The method first calls
        /// <see cref="Close"/> to close the destination, then it calls <c>Saxon.Ejava.util.function.Consumer.accept()</c>
        /// on each of the listeners in turn to notify the fact that it has been closed.
        /// </summary>
        public void CloseAndNotify() {
            destination.closeAndNotify();
        }
        
        /// <summary>
        /// Define an action to be performed when writing to the Destination is complete.
        /// </summary>
        /// <param name="action">The action to be performed.</param>
        public void OnClose(System.Action action) {
            destination.onClose(action.Invoke);
        }



        /// <summary>This property determines the base URI of the constructed <c>XdmNode</c>. 
        /// If the <c>BaseUri</c> property of the <c>XdmDestination</c> is set before the destination is written to,
        /// then the constructed <c>XdmNode</c> will have this base URI. Setting this property after constructing the node
        /// has no effect.
        /// </summary>

        public Uri BaseUri {
            get => destination.getDestinationBaseURI();
            set => destination.setDestinationBaseURI(value);
        }


        /// <summary>
        /// Close any resources associated with this destination. Note that this does <b>not</b>
        /// close any user-supplied OutputStream or Writer; those must be closed explicitly
        /// by the calling application.
        /// </summary>
        public void Close() {
            destination.close();
        }

        /// <summary>After construction, retrieve the constructed document node.</summary>
        /// <remarks>
        /// <para>The value of the property will be null if no data has been written to the
        /// <c>RawDestination</c>, either because the process that writes to the destination has not
        /// yet been run, or because the process produced no output.</para>
        /// </remarks>

        public XdmValue XdmValue => destination.getXdmValue();
    }
}

// Copyright (c) 2021 Saxonica Limited