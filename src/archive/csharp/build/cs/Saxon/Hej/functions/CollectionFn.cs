using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.functions {
    public class CollectionFn : Saxon.Hej.functions.SystemFunction, Saxon.Hej.expr.Callable {
        public static string EMPTY_COLLECTION_URI = "http://saxon.sf.net/collection/empty";
        public readonly static Saxon.Hej.lib.ResourceCollection EMPTY_COLLECTION = new Saxon.Hej.functions.CollectionFn.EmptyCollection(EMPTY_COLLECTION_URI);
        private class EmptyCollection : Saxon.Hej.lib.ResourceCollection {
            private readonly string collectionUri;
            internal EmptyCollection(string cUri)  {
                collectionUri = cUri;
            }
            public string getCollectionURI() {
                return collectionUri;
            }
            public System.Collections.Generic.IEnumerator<string> getResourceURIs(Saxon.Hej.expr.XPathContext context) {
                return new System.Collections.Generic.List<string>().GetEnumerator();;
            }
            public System.Collections.Generic.IEnumerator<Saxon.Hej.lib.Resource> getResources(Saxon.Hej.expr.XPathContext context) {
                return new System.Collections.Generic.List<Saxon.Hej.lib.Resource>().GetEnumerator();;
            }
            public bool isStable(Saxon.Hej.expr.XPathContext context) {
                return true;
            }
        
        }
        public override int getSpecialProperties(Saxon.Hej.expr.Expression[] arguments) {
            return (((base.getSpecialProperties(arguments)&~(Saxon.Hej.expr.StaticProperty.NO_NODES_NEWLY_CREATED)))|Saxon.Hej.expr.StaticProperty.PEER_NODESET);
        }
        private string getAbsoluteCollectionURI(string href, Saxon.Hej.expr.XPathContext context) {
            string absoluteURI;
            if ((href == null)) {
                absoluteURI = context.getConfiguration().getDefaultCollection();
            } else {
                System.Uri uri;
                try {
                    uri = new System.Uri(href, System.UriKind.RelativeOrAbsolute);
                } catch (System.UriFormatException e) {
                    href = Saxon.Hej.functions.IriToUri.iriToUri(Saxon.Hej.str.StringView.tidy(href)).ToString();
                    try {
                        uri = new System.Uri(href, System.UriKind.RelativeOrAbsolute);
                    } catch (System.UriFormatException e2) {
                        throw new Saxon.Hej.trans.XPathException(e2.Message, "FODC0004");
                    }
                }
                try {
                    if (uri.IsAbsoluteUri) {
                        absoluteURI = uri.ToString();
                    } else {
                        string @base = getRetainedStaticContext().getStaticBaseUriString();
                        if ((@base != null)) {
                            absoluteURI = Saxon.Hej.functions.ResolveURI.makeAbsolute(href, @base).ToString();
                        } else {
                            throw new Saxon.Hej.trans.XPathException("Relative collection URI cannot be resolved: no base URI available", "FODC0002");
                        }
                    }
                } catch (System.UriFormatException e) {
                    throw new Saxon.Hej.trans.XPathException(e.Message, "FODC0004");
                }
            }
            return absoluteURI;
        }
        private Saxon.Hej.om.SequenceIterator getSequenceIterator(Saxon.Hej.lib.ResourceCollection collection, Saxon.Hej.expr.XPathContext context) {
            System.Collections.Generic.IEnumerator<Saxon.Hej.lib.Resource> sources = collection.getResources(context);
            return new Saxon.Hej.functions.CollectionFn.CollectionIterator(sources, context);
        }
        private class CollectionIterator : Saxon.Hej.om.SequenceIterator {
    // Injecting C# code
        ~CollectionIterator(){sources?.Dispose();} // finalizer
    // End of injected C# code
            private readonly System.Collections.Generic.IEnumerator<Saxon.Hej.lib.Resource> sources;
            public CollectionIterator(System.Collections.Generic.IEnumerator<Saxon.Hej.lib.Resource> sources, Saxon.Hej.expr.XPathContext context)  {
                this.sources = sources;
            }
            public Saxon.Hej.om.Item next() {
                if (sources.MoveNext() /*UNSAFE CONVERSION*/) {
                    return new Saxon.Hej.value.ObjectValue<Saxon.Hej.lib.Resource>(sources.Current);
                } else {
                    return null;
                }
            }
            public void close() {
                sources?.Dispose();
            }
        
        }
        public override Saxon.Hej.om.Sequence call(Saxon.Hej.expr.XPathContext context, Saxon.Hej.om.Sequence[] arguments) {
            string href;
            if ((getArity() == 0)) {
                href = context.getController().getDefaultCollection();
            } else {
                Saxon.Hej.om.Item arg = arguments[0].head();
                if ((arg == null)) {
                    href = context.getController().getDefaultCollection();
                } else {
                    href = arg.getStringValue();
                }
            }
            if ((href == null)) {
                throw new Saxon.Hej.trans.XPathException("No default collection has been defined", "FODC0002");
            }
            string absoluteURI = getAbsoluteCollectionURI(href, context);
            Saxon.Hej.expr.PackageData packageData = getRetainedStaticContext().getPackageData();
            Saxon.Hej.om.SpaceStrippingRule whitespaceRule = Saxon.Hej.om.NoElementsSpaceStrippingRule.getInstance();
            string collectionKey = absoluteURI;
            if (packageData.isXSLT()) {
                whitespaceRule = ((Saxon.Hej.style.StylesheetPackage)(packageData)).getSpaceStrippingRule();
                if ((whitespaceRule != Saxon.Hej.om.NoElementsSpaceStrippingRule.getInstance())) {
                    collectionKey = (((((Saxon.Hej.style.StylesheetPackage)(packageData)).getPackageName()+((Saxon.Hej.style.StylesheetPackage)(packageData)).getPackageVersion())+" ")+absoluteURI);
                }
            }
            Saxon.Hej.om.GroundedValue cachedCollection = (Saxon.Hej.om.GroundedValue)(context.getController().getUserData("saxon:collections", collectionKey));
            if ((cachedCollection != null)) {
                return cachedCollection;
            }
            Saxon.Hej.lib.ResourceCollection collection = context.getConfiguration().getRegisteredCollection(absoluteURI);
            if ((collection == null)) {
                Saxon.Hej.lib.CollectionFinder collectionFinder = context.getController().getCollectionFinder();
                if ((collectionFinder != null)) {
                    collection = /*Delegate.findCollection()*/collectionFinder(context, absoluteURI);
                }
            }
            if ((collection == null)) {
                collection = new Saxon.Hej.functions.CollectionFn.EmptyCollection(EMPTY_COLLECTION_URI);
            }
            if (((packageData is Saxon.Hej.style.StylesheetPackage) && (whitespaceRule != Saxon.Hej.om.NoElementsSpaceStrippingRule.getInstance()))) {
                if ((collection is Saxon.Hej.resource.AbstractResourceCollection)) {
                    bool alreadyStripped = ((Saxon.Hej.resource.AbstractResourceCollection)(collection)).stripWhitespace(whitespaceRule);
                    if (alreadyStripped) {
                        whitespaceRule = null;
                    }
                }
            }
            Saxon.Hej.om.SequenceIterator sourceSeq = getSequenceIterator(collection, context);
            Saxon.Hej.om.SequenceIterator result = context.getConfiguration().getMultithreadedItemMappingIterator(sourceSeq, Saxon.Hej.expr.ItemMapper.of(it => ((Saxon.Hej.value.ObjectValue<Saxon.Hej.lib.Resource>)(it)).getObject().getItem()));
            if ((whitespaceRule != null)) {
                Saxon.Hej.om.SpaceStrippingRule rule = whitespaceRule;
                result = Saxon.Hej.expr.ItemMappingIterator.map(result, item => {
                    if (((item is Saxon.Hej.om.NodeInfo) && (((Saxon.Hej.om.NodeInfo)(item)).getNodeKind() == Saxon.Hej.type.Type.DOCUMENT))) {
                        Saxon.Hej.om.TreeInfo treeInfo = ((Saxon.Hej.om.NodeInfo)(item)).getTreeInfo();
                        if ((treeInfo.getSpaceStrippingRule() != rule)) {
                            return new Saxon.Hej.tree.wrapper.SpaceStrippedDocument(treeInfo, rule).getRootNode();
                        }
                    }
                    return item;
                } );
            }
            if ((collection.isStable(context) || context.getConfiguration().getBooleanProperty(Saxon.Hej.lib.Feature<bool>.STABLE_COLLECTION_URI))) {
                Saxon.Hej.Controller controller = context.getController();
                Saxon.Hej.om.DocumentPool docPool = controller.getDocumentPool();
                cachedCollection = Saxon.Hej.om.SequenceTool.toGroundedValue(result);
                Saxon.Hej.om.SequenceIterator iter = cachedCollection.iterate();
                for (Saxon.Hej.om.Item item; ((item = iter.next()) != null); ) {
                    if (((item is Saxon.Hej.om.NodeInfo) && (((Saxon.Hej.om.NodeInfo)(item)).getNodeKind() == Saxon.Hej.type.Type.DOCUMENT))) {
                        string docUri = ((Saxon.Hej.om.NodeInfo)(item)).getSystemId();
                        Saxon.Hej.om.DocumentKey docKey;
                        if ((packageData is Saxon.Hej.style.StylesheetPackage)) {
                            Saxon.Hej.style.StylesheetPackage pack = (Saxon.Hej.style.StylesheetPackage)(packageData);
                            docKey = new Saxon.Hej.om.DocumentKey(docUri, pack.getPackageName(), pack.getPackageVersion());
                        } else {
                            docKey = new Saxon.Hej.om.DocumentKey(docUri);
                        }
                        Saxon.Hej.om.TreeInfo info = ((item is Saxon.Hej.om.TreeInfo) ? (Saxon.Hej.om.TreeInfo)(item) : new Saxon.Hej.om.GenericTreeInfo(controller.getConfiguration(), (Saxon.Hej.om.NodeInfo)(item)));
                        docPool.add(info, docKey);
                    }
                }
                context.getController().setUserData("saxon:collections", collectionKey, cachedCollection);
                return cachedCollection;
            }
            return new Saxon.Hej.om.LazySequence(result);
        }
    
    }
}