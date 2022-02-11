using System;
using Org.XmlResolver.Features;
using Saxon.Ejavax.xml.transform;

namespace Saxon.Callbacks {
    public abstract class CommonResourceResolver : System.Xml.XmlResolver, Org.XmlResolver.INamespaceResolver {
        protected System.Xml.XmlResolver nextXmlResolver = null;
        protected Org.XmlResolver.INamespaceResolver nextNamespaceResolver = null;



        public abstract CommonResourceResolver NewInstance();

        internal void CopyFrom(CommonResourceResolver resolver) {
            nextXmlResolver = resolver.nextXmlResolver;
            nextNamespaceResolver = resolver.nextNamespaceResolver;
        }

        public virtual CommonResourceResolver chain(System.Xml.XmlResolver resolver) {
            nextXmlResolver = resolver;
            return this;
        }

        public virtual CommonResourceResolver chain(Org.XmlResolver.INamespaceResolver resolver) {
            nextNamespaceResolver = resolver;
            return this;
        }
        
        public abstract object GetEntity(Uri absoluteUri, string nature, string purpose);

        public abstract object GetEntity(string href, Uri baseUri, string nature, string purpose);
        
        public abstract void SetFeature(ResolverFeature feature, object value);

        public abstract object GetFeature(ResolverFeature resolverFeature);

        
    }
}