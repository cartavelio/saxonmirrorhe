using System;
using Saxon.Hej.value;

namespace Saxon.Api {
    /// <summary inherits="XdmValue">
    /// The class <c>XdmEmptySequence</c> represents an empty sequence in the XDM Data Model.
    /// </summary>
    /// <remarks>
    /// <para>An empty sequence <i>may</i> also be represented by an <c>XdmValue</c> whose length
    /// happens to be zero. Applications should therefore not test to see whether an object
    /// is an instance of this class in order to decide whether it is empty.</para>
    /// </remarks>

    [Serializable]
    public sealed class XdmEmptySequence : XdmValue
    {

        ///<summary>The singular instance of this class</summary>

        public static readonly XdmEmptySequence Instance = new ();

        private XdmEmptySequence() : base(EmptySequence.getInstance())
        { }
    }
}

// Copyright (c) 2021 Saxonica Limited