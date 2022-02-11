using System;
using Saxon.Hej.s9api;

namespace Saxon.Api {
    /// <summary>
    /// A <c>NullDestination</c> is an <c>XmlDestination</c> that discards all its output.
    /// </summary>

    public class NullDestination : IDestination
    {
        readonly Saxon.Hej.s9api.NullDestination destination;
        
        /// <summary>Construct a <c>NullDestination</c></summary>

        public NullDestination()
        {
            destination = new Saxon.Hej.s9api.NullDestination();
        }

        /// <summary>This property determines the base Uri of the constructed <c>Serializer</c>. 
        /// </summary>

        public Uri BaseUri
        {
            get => destination.getDestinationBaseURI();
            set => destination.setDestinationBaseURI(value);
        }


        /// <summary>
        /// The underlying <c>Destination</c> object in the Saxon implementation, which in the NullDestination is null.
        /// </summary>
        /// <value>returns null</value>
        public Destination UnderlyingDestination {
            get { return destination; }
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

// Copyright (c) 2021 Saxonica Limited