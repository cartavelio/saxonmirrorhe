using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.functions {
    public class UriCollection : Saxon.Hej.functions.SystemFunction {
        private Saxon.Hej.om.SequenceIterator getUris(string absoluteURI, Saxon.Hej.expr.XPathContext context) {
            Saxon.Hej.lib.ResourceCollection collection = context.getConfiguration().getRegisteredCollection(absoluteURI);
            if ((collection == null)) {
                Saxon.Hej.lib.CollectionFinder collectionFinder = context.getController().getCollectionFinder();
                if ((collectionFinder != null)) {
                    collection = /*Delegate.findCollection()*/collectionFinder(context, absoluteURI);
                }
            }
            if ((collection == null)) {
                Saxon.Hej.trans.XPathException err = new Saxon.Hej.trans.XPathException(("No collection has been defined for href: "+(((absoluteURI == null) ? "" : absoluteURI))));
                err.setErrorCode("FODC0002");
                err.setXPathContext(context);
                throw err;
            }
            System.Collections.Generic.IEnumerator<string> sources = collection.getResourceURIs(context);
            System.Collections.Generic.IEnumerator<Saxon.Hej.value.AnyURIValue> uris = new Saxon.Impl.Overrides.MappingEnumerator<string,Saxon.Hej.value.AnyURIValue>(sources, s => new Saxon.Hej.value.AnyURIValue(s));
            return new Saxon.Hej.om.IteratorWrapper(uris);
        }
        private Saxon.Hej.om.Sequence getDefaultUriCollection(Saxon.Hej.expr.XPathContext context) {
            string href = context.getConfiguration().getDefaultCollection();
            if ((href == null)) {
                throw new Saxon.Hej.trans.XPathException("No default collection has been defined", "FODC0002");
            } else {
                return new Saxon.Hej.om.LazySequence(getUris(href, context));
            }
        }
        public override Saxon.Hej.om.Sequence call(Saxon.Hej.expr.XPathContext context, Saxon.Hej.om.Sequence[] arguments) {
            if ((arguments.Length == 0)) {
                return getDefaultUriCollection(context);
            } else {
                Saxon.Hej.om.Item arg = arguments[0].head();
                if ((arg == null)) {
                    return getDefaultUriCollection(context);
                }
                string href = arg.getStringValue();
                System.Uri hrefURI;
                try {
                    hrefURI = new System.Uri(href, System.UriKind.RelativeOrAbsolute);
                } catch (System.UriFormatException e) {
                    throw new Saxon.Hej.trans.XPathException(("Invalid URI passed to uri-collection: "+href), "FODC0004");
                }
                if (!(hrefURI.IsAbsoluteUri)) {
                    System.Uri staticBaseUri = getRetainedStaticContext().getStaticBaseUri();
                    if ((staticBaseUri == null)) {
                        throw new Saxon.Hej.trans.XPathException("No base URI available for uri-collection", "FODC0002");
                    }
                    hrefURI = new System.Uri(staticBaseUri, hrefURI);
                }
                return new Saxon.Hej.om.LazySequence(getUris(hrefURI.ToString(), context));
            }
        }
    
    }
}