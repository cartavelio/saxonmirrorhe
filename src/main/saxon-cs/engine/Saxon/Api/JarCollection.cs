using System;

namespace Saxon.Api {
    /// <summary>
    /// A JarCollection represents a collection of resources held in a JAR or ZIP archive, accessess typically
    /// using a URI using the "jar" URI scheme, or simply a "file" URI where the target file is a JAR or ZIP file.
    /// </summary>
    public class JarCollection : ResourceCollection
    {

        public JarCollection(DynamicContext context, String collectionURI, URIQueryParameters parameters)
        {
            throw new NotImplementedException();
            //resourceCollection = new net.sf.saxon.resource.JarCollection(context.context, collectionURI, parameters.Implementation());

        }

    }
}