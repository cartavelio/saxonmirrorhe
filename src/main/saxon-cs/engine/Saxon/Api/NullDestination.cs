using System;
using net.sf.saxon.s9api;

namespace Saxon.Api {
    /// <summary>
    /// A <c>NullDestination</c> is an <c>XmlDestination</c> that discards all its output.
    /// </summary>

    public class NullDestination : IDestination
    {

        net.sf.saxon.s9api.NullDestination destination;
        /// <summary>Construct a <c>NullDestination</c></summary>

        public NullDestination()
        {
            destination = new net.sf.saxon.s9api.NullDestination();
        }

        /// <summary>This property determines the base Uri of the constructed <c>Serializer</c>. 
        /// </summary>

        public Uri BaseUri
        {
            get { return destination.getDestinationBaseURI(); }
            set { destination.setDestinationBaseURI(value); }
        }


        /// <summary>
        /// The underlying <c>Destination</c> object in the Saxon implementation, which in the NullDestination is null.
        /// </summary>
        /// <returns>returns null</returns>
        public Destination GetUnderlyingDestination()
        {
            return destination;
        }
        
        /// <summary>
        /// Define an action to be performed when writing to the Destination is complete.
        /// </summary>
        /// <param name="action">The action to be performed.</param>
        public void OnClose(System.Action action) {
            destination.onClose(action.Invoke);
        }


    }
}