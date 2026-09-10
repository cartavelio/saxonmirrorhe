using System;

namespace Saxon.Api {
    /// <summary>
    /// The <c>SchemaResolver</c> is a user-supplied class used for resolving references to
    /// schema documents. It applies to references from one schema document to another
    /// appearing in <c>xs:import</c>, <c>xs:include</c>, and <c>xs:redefine</c>; to
    /// references from an instance document to a schema in <c>xsi:schemaLocation</c> and
    /// <c>xsi:noNamespaceSchemaLocation</c>, to <c>xsl:import-schema</c> in XSLT, and to
    /// the <c>import schema</c> declaration in XQuery.
    /// </summary>
    /// <param name="targetNamespace">The target namespace of the required schema components</param>
    /// <param name="baseUri">The base URI of the module containing the reference to a schema document
    /// declaration</param>
    /// <param name="locationHint"Xslt>The URI (if any) provided as a location hint.
    /// Note: the <c>import schema</c> declaration in XQuery permits several location hints; if
    /// there is more than one, all but the first are ignored</param>
    /// <returns>An XdmNode representing the root document node of the schema document to be loaded.
    /// </returns>


    public delegate XdmNode SchemaResolver(string targetNamespace, Uri baseUri, Uri locationHint);
    

    
}

// Copyright (c) 2021 Saxonica Limited