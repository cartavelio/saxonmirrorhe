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
    /// <para>In interfaces that expect an <c>XdmItem</c>, an empty sequence is represented
    /// by a CLI <c>null</c> value.</para> 
    /// </remarks>

    [Serializable]
    public sealed class XdmEmptySequence : XdmValue
    {

        ///<summary>The singular instance of this class</summary>

        public static XdmEmptySequence INSTANCE = new ();

        private XdmEmptySequence() : base(EmptySequence.getInstance())
        { }
    }
}