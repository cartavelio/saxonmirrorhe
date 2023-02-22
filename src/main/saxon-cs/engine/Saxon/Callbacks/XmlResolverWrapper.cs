using System;
using System.IO;
using System.Xml;
using Org.XmlResolver;
using Saxon.Api;
using Saxon.Ejavax.xml.transform;
using Saxon.Ejavax.xml.transform.stream;

namespace Saxon.Callbacks {
    public class XmlResolverWrapper : CommonResourceResolver {

        private System.Xml.XmlResolver xmlResolver;

        public XmlResolverWrapper(System.Xml.XmlResolver xmlResolver) {
            this.xmlResolver = xmlResolver;
        }
        public override CommonResourceResolver NewInstance() {
            XmlResolverWrapper newResolver = new XmlResolverWrapper(xmlResolver);
            newResolver.CopyFrom(this);
            return newResolver;
        }

        public override IResource DereferenceLocation(ResourceRequest request) {
            Uri baseUri = request.BaseUri ?? request.Uri;
            string href = request.RelativeUri ?? request.Uri.ToString();
            Uri abs = xmlResolver.ResolveUri(baseUri, href);
            object obj = xmlResolver.GetEntity(abs, null, typeof(System.IO.Stream));
            if ((obj is Stream stream)) {
                return new TextResource(stream, baseUri);
            }
            else {
                throw new Saxon.Hej.trans.XPathException(
                    "XmlResolver returned type " + obj.GetType().Name + " (expected Stream)");
            }
        }

        public override IResource DereferenceNamespace(string namespaceUri, ResourceRequest request) {
            throw new NotImplementedException();
        }

        public override object GetEntity(Uri absoluteUri, string role, Type ofObjectToReturn) {
            return xmlResolver.GetEntity(absoluteUri, role, ofObjectToReturn);
        }

        public override Uri ResolveUri(Uri baseUri, string relativeUri) {
            return xmlResolver.ResolveUri(baseUri, relativeUri);
        }
    }
}