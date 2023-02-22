using JDotNetObjectModel = net.sf.saxon.dotnet.DotNetObjectModel;
using JSequence = net.sf.saxon.om.Sequence;
using JGroundedValue = net.sf.saxon.om.GroundedValue;
using FingerprintedQName = net.sf.saxon.om.FingerprintedQName;
using JValidation = net.sf.saxon.lib.Validation;
// using JDotNetWriter = net.sf.saxon.dotnet.DotNetWriter;
// using JDotNetInputStream = net.sf.saxon.dotnet.DotNetInputStream;
// using JDotNetURIResolver = net.sf.saxon.dotnet.DotNetURIResolver;
// using JDotNetEnumerableCollection = net.sf.saxon.dotnet.DotNetEnumerableCollection;
// using JDotNetPullProvider = net.sf.saxon.dotnet.DotNetPullProvider;
// using JDotNetReader = net.sf.saxon.dotnet.DotNetReader;
// using JDotNetComparator = net.sf.saxon.dotnet.DotNetComparator;
using java.util.function;

namespace Saxon.Api
{
    /// <summary>
    /// Enumeration identifying the different tree model implementations
    /// </summary>
    /// 
    public enum TreeModel
    {
        /// <summary>
        /// Saxon <c>LinkedTree</c>. This tree model is primarily useful when using XQuery Update, since it is the
        /// only standard tree model to support updates.
        /// </summary>
        LinkedTree,
        /// <summary>
		/// Saxon <c>TinyTree</c>. This is the default model and is suitable for most purposes.
        /// </summary>
        TinyTree,
        /// <summary>
		/// Saxon Condensed <c>TinyTree</c>. This is a variant of the <c>TinyTree</c> that shares storage for 
        /// duplicated text and attribute nodes. It gives a further saving in space occupied, at the cost
        /// of some increase in the time taken for tree construction.
        /// </summary>
        TinyTreeCondensed,
        /// <summary>
        /// Unspecified tree model. This value is used to indicate that there is no preference for any specific
        /// tree model, which allows the choice to fall back to other interfaces.
        /// </summary>
        Unspecified = -1
    }

    internal class DotNetObjectModelDefinition : JDotNetObjectModel
    {

        public override bool isXdmValue(object obj)
        {
            return obj is XdmValue;
        }

        public override bool isXdmAtomicValueType(System.Type type)
        {
            return typeof(XdmAtomicValue).IsAssignableFrom(type);
        }

        public override bool isXdmValueType(System.Type type)
        {
            return typeof(XdmValue).IsAssignableFrom(type);
        }

        public override JGroundedValue unwrapXdmValue(object obj)
        {
            return ((XdmValue)obj).Unwrap();
        }

        public override object wrapAsXdmValue(JSequence value)
        {
            return XdmValue.Wrap(value);
        }

        public override bool isXmlNodeType(System.Type type)
        {
            return typeof(System.Xml.XmlNode).IsAssignableFrom(type);
        }

    }

}

////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) 2020 Saxonica Limited.
// This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
// If a copy of the MPL was not distributed with this file, You can obtain one at http://mozilla.org/MPL/2.0/.
// This Source Code Form is "Incompatible With Secondary Licenses", as defined by the Mozilla Public License, v. 2.0.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////