using System;
using System.IO;
using javax.xml.transform.stream;

namespace Saxon.Api {
    /// <summary>
    /// Internal class that wraps a (.NET) <c>IQueryResolver</c> to create a (Java) <c>ModuleURIResolver</c>.
    /// <para>A <c>ModuleURIResolver</c> is used when resolving references to
    /// query modules. It takes as input a URI that identifies the module to be loaded, and a set of
    /// location hints, and returns one or more <c>StreamSource</c> obects containing the queries
    /// to be imported.</para>
    /// </summary>

    internal class DotNetModuleURIResolver : net.sf.saxon.lib.ModuleURIResolver
    {

        private IQueryResolver resolver;

        /// <summary>
        /// Initializes a new instance of the <see cref="Saxon.Api.DotNetModuleURIResolver"/> class.
        /// </summary>
        /// <param name="resolver">Resolver.</param>
        public DotNetModuleURIResolver(IQueryResolver resolver)
        {
            this.resolver = resolver;
        }


        /// <summary>
        /// Resolve a module URI and associated location hints.
        /// </summary>
        /// <param name="moduleURI">ModuleURI. The module namespace URI of the module to be imported; or null when
        /// loading a non-library module.</param>
        /// <param name="baseURI">BaseURI. The base URI of the module containing the "import module" declaration;
        /// null if no base URI is known</param>
        /// <param name="locations">Locations. The set of URIs specified in the "at" clause of "import module",
        /// which serve as location hints for the module</param>
        /// <returns>an array of StreamSource objects each identifying the contents of a module to be
        /// imported. Each StreamSource must contain a
        /// non-null absolute System ID which will be used as the base URI of the imported module,
        /// and either an InputSource or a Reader representing the text of the module. The method
        /// may also return null, in which case the system attempts to resolve the URI using the
        /// standard module URI resolver.</returns>
        public StreamSource[] resolve(String moduleURI, String baseURI, String[] locations)
        {
            Uri baseU = (baseURI == null ? null : new Uri(baseURI));
            Uri[] modules = resolver.GetModules(moduleURI, baseU, locations);
            StreamSource[] ss = new StreamSource[modules.Length];
            for (int i = 0; i < ss.Length; i++)
            {
                ss[i] = new StreamSource();
                ss[i].setSystemId(modules[i].ToString());
                Object query = resolver.GetEntity(modules[i]);
                if (query is Stream)
                {
                    ss[i].setInputStream(new JDotNetInputStream((Stream)query));
                }
                else if (query is String)
                {
                    ss[i].setReader(new JDotNetReader(new StringReader((String)query)));
                }
                else
                {
                    throw new ArgumentException("Invalid response from GetEntity()");
                }
            }
            return ss;
        }
    }
}