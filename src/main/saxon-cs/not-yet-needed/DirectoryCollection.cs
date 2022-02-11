using System;

namespace Saxon.Api {
    /// <summary>
    /// This class represents a resource collection containing all, or selected, files within a filestore directory
    /// </summary>
    public class DirectoryCollection : ResourceCollection
    {
       
        /// <summary>
        /// Create a directory collection
        /// </summary>
        /// <param name="proc">Processor object for configuration properties</param>
        /// <param name="collectionURI">the collection URI</param>
        /// <param name="directoryName">the directory containing the files</param>
        /// <param name="parameters">query parameter supplied as part of the URI</param>
        public DirectoryCollection(Processor proc, String collectionURI, String directoryName, URIQueryParameters parameters)
        {
            throw new NotImplementedException();
            //resourceCollection = new net.sf.saxon.resource.DirectoryCollection(proc.Implementation, collectionURI, new  java.io.File(directoryName), parameters.Implementation());
            
        }

     
    }
}