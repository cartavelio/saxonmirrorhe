using System;
using System.IO;
using System.Xml;
using Org.XmlResolver;
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

        public override StreamSource DereferenceLocation(Uri baseUri, string href, string nature, string purpose) {
            Uri abs = xmlResolver.ResolveUri(baseUri, href);
            object obj = xmlResolver.GetEntity(abs, null, typeof(System.IO.Stream));
            if ((obj is Stream stream)) {
                Saxon.Ejavax.xml.transform.stream.StreamSource source = new(stream, abs.ToString());
                return source;
                // source.setSystemId(abs.ToString());
                // Saxon.Hej.lib.AugmentedSource @as =
                //     Saxon.Hej.lib.AugmentedSource.makeAugmentedSource(source);
                // @as.setPleaseCloseAfterUse(true);
                // return @as;
            }
            else if ((obj is TextReader reader)) {
                Saxon.Ejavax.xml.transform.stream.StreamSource source = new(reader);
                source.setSystemId(abs.ToString());
                return source;
            }
            else if ((obj == null)) {
                throw new Saxon.Hej.trans.XPathException("XmlResolver returned null");
            }
            else {
                throw new Saxon.Hej.trans.XPathException(
                    ("Unrecognized object returned by XmlResolver (type " + obj.GetType().Name));
            }
        }

        public override StreamSource DereferenceNamespace(string namespaceUri, string nature, string purpose) {
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