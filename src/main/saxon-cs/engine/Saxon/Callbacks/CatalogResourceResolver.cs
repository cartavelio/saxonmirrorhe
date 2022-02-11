using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Org.XmlResolver;
using Org.XmlResolver.Features;
using Saxon.Ejavax.xml.transform;
using Saxon.Hej.expr.parser;

namespace Saxon.Callbacks {
    public class CatalogResourceResolver : CommonResourceResolver {
        private readonly Org.XmlResolver.Resolver resolver;

        public CatalogResourceResolver() {
            resolver = new(new XmlResolverConfiguration(null, new List<string>()));
        }

        public CatalogResourceResolver(Resolver resolver) {
            this.resolver = resolver;
        }
        
        public override CommonResourceResolver NewInstance() {
            CommonResourceResolver newResolver = new CatalogResourceResolver(resolver);
            newResolver.CopyFrom(this);
            return newResolver;
        }
        
        public override CommonResourceResolver chain(System.Xml.XmlResolver resolver) {
            nextXmlResolver = resolver;
            return this;
        }

        public override object GetEntity(System.Uri absoluteUri, string role, System.Type ofObjectToReturn) {
            object result = null;
            if (resolver != null) {
                result = resolver.GetEntity(absoluteUri, role, ofObjectToReturn);
            }
            if (result == null && nextXmlResolver != null) {
                result = nextXmlResolver.GetEntity(absoluteUri, role, ofObjectToReturn);
            }
            return result;
        }
        
        public override Uri ResolveUri(Uri baseUri, string relativeUri) {
            Uri result = null;
            if (resolver != null) {
                result = resolver.ResolveUri(baseUri, relativeUri);
            }
            if (result == null && nextXmlResolver != null) {
                result = nextXmlResolver.ResolveUri(baseUri, relativeUri);
            }
            return result;
        }

        public override object GetEntity(Uri absoluteUri, string nature, string purpose) {
            object result = null;
            if (resolver != null) {
                result = resolver.GetEntity(absoluteUri, nature, purpose);
            }
            if (result == null && nextNamespaceResolver != null) {
                result = nextNamespaceResolver.GetEntity(absoluteUri, nature, purpose);
            }

            return result;
        }

        public override object GetEntity(string href, Uri baseUri, string nature, string purpose) {
            Uri abs = ResolveUri(baseUri, href);
            return GetEntity(abs, nature, purpose);
        }

        public override void SetFeature(ResolverFeature feature, object value) {
            if (resolver == null) {
                throw new NullReferenceException();
            }
            resolver.GetConfiguration().SetFeature(feature, value);
        }

        public override object GetFeature(ResolverFeature resolverFeature) {
            return resolver?.GetConfiguration().GetFeature(resolverFeature);
        }

    }
}