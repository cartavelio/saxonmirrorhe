using System;

namespace Saxon.Callbacks {
    
    /// <summary>
    /// An XmlResolver that chains together two supplied XmlResolvers. If the <c>GetEntity</c>
    /// method of the first one returns null, the second is called.
    /// </summary>
    /// <remarks>
    /// <para>The <c>make()</c> method allows an arbitrary long chain of resolvers to be constructed.</para>
    /// <para>The <c>ResolveUri</c> method cannot return null, so URI absolutization is
    /// always done using the first <c>XmlResolver</c>; the <c>ResolveUri</c> of the second
    /// and subsequent resolvers are never called.
    /// </para>
    /// </remarks>
    /// 
    public class ChainedXmlResolver : System.Xml.XmlResolver {

        private readonly System.Xml.XmlResolver _first;
        private readonly System.Xml.XmlResolver _second;

        public ChainedXmlResolver(System.Xml.XmlResolver first, System.Xml.XmlResolver second) {
            _first = first;
            _second = second;
        }
        public override Uri ResolveUri(Uri baseUri, string relativeUri) {
            return _first.ResolveUri(baseUri, relativeUri);
        }

        // Maps a URI to an Object containing the actual resource.
        public override object GetEntity(Uri absoluteUri, string role, Type ofObjectToReturn) {
            object result = _first.GetEntity(absoluteUri, role, ofObjectToReturn);
            if (result == null) {
                result = _second.GetEntity(absoluteUri, role, ofObjectToReturn);
            }

            return result;
        }
        
        /// <summary>
        /// Construct a chain of resolvers
        /// </summary>
        /// <param name="resolvers">The resolvers to be included in the chain</param>
        /// <returns>A composite <c>XmlResolver</c>. Returns null if the supplied array is empty.</returns>

        public static System.Xml.XmlResolver make(params System.Xml.XmlResolver[] resolvers) {
            if (resolvers.Length == 0) {
                return null;
            }

            System.Xml.XmlResolver result = resolvers[0];
            for (int i = 1; i < resolvers.Length; i++) {
                result = new ChainedXmlResolver(result, resolvers[i]);
            }

            return result;
        }
        
        
    }
}

// Copyright (c) 2022 - 2023 Saxonica Limited