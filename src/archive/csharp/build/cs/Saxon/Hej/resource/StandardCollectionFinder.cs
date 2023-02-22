using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.resource {
    public class StandardCollectionFinder {
        public Saxon.Hej.lib.ResourceCollection findCollection(Saxon.Hej.expr.XPathContext context, string collectionURI) {
            Saxon.Hej.resource.AbstractResourceCollection.checkNotNull(collectionURI, context);
            Saxon.Hej.functions.URIQueryParameters @params = null;
            string query = null;
            System.Uri relativeURI;
            try {
                relativeURI = new System.Uri(Saxon.Hej.functions.ResolveURI.escapeSpaces(collectionURI), System.UriKind.RelativeOrAbsolute);
                query = Saxon.Impl.Helpers.UriUtils.GetQuery(relativeURI);
                if ((query != null)) {
                    int q = collectionURI.IndexOf('?');
                    @params = new Saxon.Hej.functions.URIQueryParameters(query, context.getConfiguration());
                    collectionURI = Saxon.Hej.functions.ResolveURI.escapeSpaces(collectionURI.Substring(0, q - 0));
                }
            } catch (System.UriFormatException e) {
                Saxon.Hej.trans.XPathException err = new Saxon.Hej.trans.XPathException((("Invalid relative URI "+Saxon.Hej.trans.Err.wrap(collectionURI, Saxon.Hej.trans.Err.VALUE))+" passed to collection() function"));
                err.setErrorCode("FODC0004");
                err.setXPathContext(context);
                throw err;
            }
            System.Uri resolvedURI;
            try {
                resolvedURI = new System.Uri(collectionURI, System.UriKind.RelativeOrAbsolute);
            } catch (System.UriFormatException e) {
                throw new Saxon.Hej.trans.XPathException(e);
            }
            if ("file".Equals(resolvedURI.Scheme)) {
                Saxon.Ejava.io.File file = new Saxon.Ejava.io.File(resolvedURI);
                checkFileExists(file, resolvedURI, context);
                if (file.isDirectory()) {
                    return new Saxon.Hej.resource.DirectoryCollection(context.getConfiguration(), collectionURI, file, @params);
                }
            }
            return new Saxon.Hej.resource.CatalogCollection(context.getConfiguration(), collectionURI);
        }
        protected internal bool isJarFileURI(string collectionURI) {
            return (((collectionURI.EndsWith(".jar") || collectionURI.EndsWith(".zip")) || collectionURI.EndsWith(".docx")) || collectionURI.StartsWith("jar:"));
        }
        public static void checkFileExists(Saxon.Ejava.io.File file, System.Uri resolvedURI, Saxon.Hej.expr.XPathContext context) {
            if (!(file.exists())) {
                Saxon.Hej.trans.XPathException err = new Saxon.Hej.trans.XPathException((("The file or directory "+resolvedURI)+" does not exist"));
                err.setErrorCode("FODC0002");
                err.setXPathContext(context);
                throw err;
            }
        }
    
    }
}