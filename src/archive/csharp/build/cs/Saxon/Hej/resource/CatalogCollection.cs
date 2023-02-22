using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.resource {
    public class CatalogCollection : Saxon.Hej.resource.AbstractResourceCollection {
        private bool stable;
        private Saxon.Hej.om.SpaceStrippingRule whitespaceRules;
        public CatalogCollection(Saxon.Hej.Configuration config, string collectionURI) : base (config) {
            this.collectionURI = collectionURI;
        }
        public override System.Collections.Generic.IEnumerator<string> getResourceURIs(Saxon.Hej.expr.XPathContext context) {
            Saxon.Hej.resource.AbstractResourceCollection.checkNotNull(collectionURI, context);
            return catalogContents(collectionURI, context);
        }
        public override System.Collections.Generic.IEnumerator<Saxon.Hej.lib.Resource> getResources(Saxon.Hej.expr.XPathContext context) {
            Saxon.Hej.resource.AbstractResourceCollection.checkNotNull(collectionURI, context);
            System.Collections.Generic.IEnumerator<string> resourceURIs = getResourceURIs(context);
            return new Saxon.Impl.Overrides.MappingEnumerator<string,Saxon.Hej.lib.Resource>(resourceURIs, input => {
                string uri = input;
                try {
                    if (uri.StartsWith("data:")) {
                        try {
                            Saxon.Hej.lib.Resource basicResource = Saxon.Hej.resource.DataURIScheme.decode(new System.Uri(uri, System.UriKind.RelativeOrAbsolute));
                            return makeTypedResource(context, basicResource);
                        } catch (Exception e) when (e is System.UriFormatException || e is Saxon.Ejava.lang.IllegalArgumentException) {
                            throw new Saxon.Hej.trans.XPathException(e);
                        }
                    } else {
                        Saxon.Hej.resource.AbstractResourceCollection.InputDetails id = getInputDetails(uri);
                        id.parseOptions = new Saxon.Hej.lib.ParseOptions(context.getConfiguration().getParseOptions());
                        id.parseOptions.setSpaceStrippingRule(whitespaceRules);
                        id.resourceUri = uri;
                        return makeResource(context, id);
                    }
                } catch (Saxon.Hej.trans.XPathException e) {
                    Saxon.Ejava.util.Optional<int> onError = Saxon.Ejava.util.Optional<int>.of(Saxon.Hej.functions.URIQueryParameters.ON_ERROR_FAIL);
                    if ((@params != null)) {
                        onError = @params.getOnError();
                    }
                    if ((onError.isPresent() && (onError.get() == Saxon.Hej.functions.URIQueryParameters.ON_ERROR_FAIL))) {
                        return new Saxon.Hej.resource.FailedResource(uri, e);
                    } else {
                        if ((onError.isPresent() && (onError.get() == Saxon.Hej.functions.URIQueryParameters.ON_ERROR_WARNING))) {
                            context.getController().warning(((("collection(): failed to parse "+uri)+": ")+e.Message), e.getErrorCodeLocalPart(), null);
                            return null;
                        } else {
                            return null;
                        }
                    }
                }
            } );
        }
        public override bool isStable(Saxon.Hej.expr.XPathContext context) {
            return stable;
        }
        public static string makeStringFromStream(System.IO.Stream input, string encoding) {
            return new System.IO.StreamReader(input, System.Text.Encoding.GetEncoding(encoding)).ReadToEnd();
        }
        protected internal System.Collections.Generic.IEnumerator<string> catalogContents(string href, Saxon.Hej.expr.XPathContext context) {
            Saxon.Ejavax.xml.transform.Source source = Saxon.Hej.functions.DocumentFn.resolveURI(href, null, null, context);
            Saxon.Hej.lib.ParseOptions options = new Saxon.Hej.lib.ParseOptions();
            options.setSchemaValidationMode(Saxon.Hej.lib.Validation.SKIP);
            options.setDTDValidationMode(Saxon.Hej.lib.Validation.SKIP);
            Saxon.Hej.om.TreeInfo catalog = context.getConfiguration().buildDocumentTree(source, options);
            if ((catalog == null)) {
                Saxon.Hej.trans.XPathException err = new Saxon.Hej.trans.XPathException(("Failed to load collection catalog "+href));
                err.setErrorCode("FODC0004");
                err.setXPathContext(context);
                throw err;
            }
            Saxon.Hej.tree.iter.AxisIterator iter = catalog.getRootNode().iterateAxis(Saxon.Hej.om.AxisInfo.CHILD, Saxon.Hej.pattern.NodeKindTest.ELEMENT);
            Saxon.Hej.om.NodeInfo top = ((/*1430*/Saxon.Hej.om.NodeInfo)iter.next());
            if (((top == null) || !((("collection".Equals(top.getLocalPart()) && top.getURI().Length==0))))) {
                string message;
                if ((top == null)) {
                    message = "No outermost element found in collection catalog";
                } else {
                    message = ((((("Outermost element of collection catalog should be Q{}collection "+"(found Q{")+top.getURI())+"}")+top.getLocalPart())+")");
                }
                Saxon.Hej.trans.XPathException err = new Saxon.Hej.trans.XPathException(message);
                err.setErrorCode("FODC0004");
                err.setXPathContext(context);
                throw err;
            }
            iter.close();
            string stableAtt = top.getAttributeValue("", "stable");
            if ((stableAtt != null)) {
                if ("true".Equals(stableAtt)) {
                    stable = true;
                } else {
                    if ("false".Equals(stableAtt)) {
                        stable = false;
                    } else {
                        Saxon.Hej.trans.XPathException err = new Saxon.Hej.trans.XPathException("The 'stable' attribute of element <collection> must be true or false");
                        err.setErrorCode("FODC0004");
                        err.setXPathContext(context);
                        throw err;
                    }
                }
            }
            Saxon.Hej.tree.iter.AxisIterator documents = top.iterateAxis(Saxon.Hej.om.AxisInfo.CHILD, Saxon.Hej.pattern.NodeKindTest.ELEMENT);
            System.Collections.Generic.IList<string> result = new System.Collections.Generic.List<string>();
            Saxon.Hej.om.NodeInfo item;
            while (((item = ((/*1430*/Saxon.Hej.om.NodeInfo)documents.next())) != null)) {
                if (!((("doc".Equals(item.getLocalPart()) && item.getURI().Length==0)))) {
                    Saxon.Hej.trans.XPathException err = new Saxon.Hej.trans.XPathException("Children of <collection> element must be <doc> elements");
                    err.setErrorCode("FODC0004");
                    err.setXPathContext(context);
                    throw err;
                }
                string hrefAtt = item.getAttributeValue("", "href");
                if ((hrefAtt == null)) {
                    Saxon.Hej.trans.XPathException err = new Saxon.Hej.trans.XPathException("A <doc> element in the collection catalog has no @href attribute");
                    err.setErrorCode("FODC0004");
                    err.setXPathContext(context);
                    throw err;
                }
                string uri;
                try {
                    uri = Saxon.Hej.functions.ResolveURI.makeAbsolute(hrefAtt, item.getBaseURI()).ToString();
                } catch (System.UriFormatException e) {
                    Saxon.Hej.trans.XPathException err = new Saxon.Hej.trans.XPathException((((("Invalid base URI or href URI in collection catalog: ("+item.getBaseURI())+", ")+hrefAtt)+")"));
                    err.setErrorCode("FODC0004");
                    err.setXPathContext(context);
                    throw err;
                }
                result.Add(uri);
            }
            return result.GetEnumerator();
        }
    
    }
}