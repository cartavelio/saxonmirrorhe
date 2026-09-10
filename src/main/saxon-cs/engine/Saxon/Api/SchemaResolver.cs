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
    /// <remarks>
    /// <para>If the <c>SchemaResolver</c> returns null, then the system attempts to locate
    /// a schema using the common resource resolver associated with the Saxon <c>Configuration</c>.
    /// It first attempts to resolve the location hint (if available), then the target namespace
    /// (if present and non-empty). If both attempts fail, the system then attempts to use
    /// standard URI resolution mechanisms to resolve the location hint (if available); it does
    /// not attempt to resolve the target namespace.
    /// </para>
    /// </remarks>
    /// <param name="targetNamespace">The target namespace of the required schema components.
    /// In the case of <c>xs:include</c>, <c>xs:redefine</c>, and <c>xs:override</c> (where
    /// no target namespace is identified), the parameter is set to null. In the case of <c>xs:import</c>,
    /// the parameter is set to the value of the <c>namespace</c> attribute if present, or to
    /// the zero-length string otherwise.</param>
    /// <param name="baseUri">The base URI of the module containing the reference to a schema document
    /// declaration</param>
    /// <param name="locationHint"Xslt>The URI (if any) provided as a location hint: typically,
    /// the value of the <c>schemaLocation</c> attribute on <c>xs:include</c> or <c>xs:import</c>.
    /// Note: the <c>import schema</c> declaration in XQuery permits several location hints; if
    /// there is more than one, all but the first are ignored</param>
    /// <returns>An <c>IResource</c> representing the schema document to be loaded.
    /// </returns>


    public delegate IResource SchemaResolver(string targetNamespace, Uri baseUri, Uri locationHint);
    

    
}

// Copyright (c) 2021 Saxonica Limited