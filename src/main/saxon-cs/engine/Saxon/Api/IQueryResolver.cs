using System;

namespace Saxon.Api {
    /// <summary>
    /// Interface defining a user-supplied class used to retrieve XQuery library modules listed
    /// in an <c>import module</c> declaration in the query prolog.
    /// </summary>


    public interface IQueryResolver
    {

        /// <summary>
        /// Given a module URI and a set of location hints, return a set of query modules.
        /// </summary>
        /// <param name="moduleUri">The URI of the required library module as written in the
        /// <c>import module</c> declaration</param>
        /// <param name="baseUri">The base URI of the module containing the <c>import module</c>
        /// declaration</param>
        /// <param name="locationHints">The sequence of URIs (if any) listed as location hints
        /// in the <c>import module</c> declaration in the query prolog.</param>
        /// <returns>A set of absolute URIs identifying the query modules to be loaded. There is no requirement
        /// that these correspond one-to-one with the URIs defined in the <c>locationHints</c>. The 
        /// returned URIs will be de-referenced by calling the <c>GetEntity</c> method.
        /// </returns>

        Uri[] GetModules(string moduleUri, Uri baseUri, string[] locationHints);

        /// <summary>
        /// Dereference a URI returned by <c>GetModules</c> to retrieve a <c>Stream</c> containing
        /// the actual query text.
        /// </summary>
        /// <param name="absoluteUri">A URI returned by the <code>GetModules</code> method.</param>
        /// <returns>Either a <c>Stream</c> or a <c>String</c> containing the query text. 
        /// The supplied URI will be used as the base URI of the query module.</returns>

        object GetEntity(Uri absoluteUri);

    }
}