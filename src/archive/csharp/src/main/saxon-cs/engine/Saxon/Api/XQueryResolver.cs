using System;

namespace Saxon.Api {
    
    /// <summary>
    /// Given a module URI and a set of location hints, return a set of query modules.
    /// </summary>
    /// <param name="moduleUri">The URI of the required library module as written in the
    /// <c>import module</c> declaration. Although use of a URI is recommended, the
    /// specifications do not require it to be a valid URI, so it is passed as a string.</param>
    /// <param name="baseUri">The base URI of the module containing the <c>import module</c>
    /// declaration. This will be null if no base URI for the containing module is known.</param>
    /// <param name="locationHints">The sequence of URIs (if any) listed as location hints
    /// in the <c>import module</c> declaration in the query prolog. These are supplied
    /// as written in the query; an error will typically be reported (before calling the
    /// <c>XQueryResolver</c> if any of the location hints is not a valid absolute or
    /// relative URI.</param>
    /// <returns>A set of <c>XQueryModuleSource</c> objects identifying the query modules to be loaded. There is no requirement
    /// that these correspond one-to-one with the URIs defined in the <c>locationHints</c>. If the result is an empty array,
    /// no query module is imported, which may or may not cause a compilation error in the importing module.
    /// If the result is null, Saxon attempts to resolve the <c>import module</c> declaration using its default rules,
    /// as if no <c>XQueryResolver</c> had been supplied.
    /// </returns>
    /// <example>
    /// <code>
	/// xqueryCompiler.XQueryResolver = (moduleUri, baseUri, locations) => {
    ///     if (moduleUri == "test.xq") {
    ///         const string q = "module namespace t='test.xq'; declare variable $t:x := 'x';";
    ///         return new [] {new XQueryModuleSource() {Content = q}};
    ///     }
    ///     return null;
    /// };
    /// </code>
    /// </example>
    /// 
    public delegate XQueryModuleSource[] XQueryResolver(string moduleUri, Uri baseUri, Uri[] locationHints);

}

// Copyright (c) 2021 Saxonica Limited