using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.functions {
    public class DocumentFn : Saxon.Hej.functions.SystemFunction, Saxon.Hej.expr.Callable {
        private Saxon.Hej.s9api.Location location;
        public override int getCardinality(Saxon.Hej.expr.Expression[] arguments) {
            Saxon.Hej.expr.Expression expression = arguments[0];
            if (Saxon.Hej.value.Cardinality.allowsMany(expression.getCardinality())) {
                return Saxon.Hej.expr.StaticProperty.ALLOWS_ZERO_OR_MORE;
            } else {
                return Saxon.Hej.expr.StaticProperty.ALLOWS_ZERO_OR_ONE;
            }
        }
        public override int getSpecialProperties(Saxon.Hej.expr.Expression[] arguments) {
            return ((Saxon.Hej.expr.StaticProperty.ORDERED_NODESET|Saxon.Hej.expr.StaticProperty.PEER_NODESET)|Saxon.Hej.expr.StaticProperty.NO_NODES_NEWLY_CREATED);
        }
        public override Saxon.Hej.expr.Expression makeFunctionCall(params Saxon.Hej.expr.Expression[]  arguments) {
            location = arguments[0].getLocation();
            Saxon.Hej.expr.Expression expr = Saxon.Hej.functions.Doc.maybePreEvaluate(this, arguments);
            return ((expr == null) ? base.makeFunctionCall(arguments) : expr);
        }
        public static bool sourceIsTree(Saxon.Ejavax.xml.transform.Source source) {
            if ((source is Saxon.Hej.om.NodeSource)) {
                return true;
            }
            return false;
        }
        public override Saxon.Hej.om.Sequence call(Saxon.Hej.expr.XPathContext context, Saxon.Hej.om.Sequence[] arguments) {
            int numArgs = getArity();
            Saxon.Hej.om.SequenceIterator hrefSequence = arguments[0].iterate();
            string baseURI = null;
            if ((numArgs == 2)) {
                Saxon.Hej.om.NodeInfo @base = (Saxon.Hej.om.NodeInfo)(arguments[1].head());
                baseURI = @base.getBaseURI();
                if ((baseURI == null)) {
                    throw new Saxon.Hej.trans.XPathException("The second argument to document() is a node with no base URI", "XTDE1162");
                }
            }
            Saxon.Hej.functions.DocumentFn.DocumentMappingFunction map = new Saxon.Hej.functions.DocumentFn.DocumentMappingFunction(context);
            map.baseURI = baseURI;
            map.stylesheetURI = getStaticBaseUriString();
            map.packageData = getRetainedStaticContext().getPackageData();
            map.locator = location;
            Saxon.Hej.expr.ItemMappingIterator iter = new Saxon.Hej.expr.ItemMappingIterator(hrefSequence, map);
            return Saxon.Hej.om.SequenceTool.toLazySequence(new Saxon.Hej.expr.sort.DocumentOrderIterator(iter, Saxon.Hej.expr.sort.GlobalOrderComparer.getInstance()));
        }
        private class DocumentMappingFunction : Saxon.Hej.expr.ItemMappingFunction {
            public string baseURI;
            public string stylesheetURI;
            public Saxon.Hej.s9api.Location locator;
            public Saxon.Hej.expr.PackageData packageData;
            public Saxon.Hej.expr.XPathContext context;
            public DocumentMappingFunction(Saxon.Hej.expr.XPathContext context)  {
                this.context = context;
            }
            public Saxon.Hej.om.Item mapItem(Saxon.Hej.om.Item item) {
                string b = baseURI;
                if ((b == null)) {
                    if ((item is Saxon.Hej.om.NodeInfo)) {
                        b = ((Saxon.Hej.om.NodeInfo)(item)).getBaseURI();
                    } else {
                        b = stylesheetURI;
                    }
                }
                return makeDoc(item.getStringValue(), b, packageData, null, context, locator, false);
            }
        
        }
        public static Saxon.Hej.om.NodeInfo makeDoc(string href, string baseURI, Saxon.Hej.expr.PackageData packageData, Saxon.Hej.lib.ParseOptions options, Saxon.Hej.expr.XPathContext c, Saxon.Hej.s9api.Location locator, bool silent) {
            Saxon.Hej.Configuration config = c.getConfiguration();
            int hash = href.IndexOf('#');
            string fragmentId = null;
            if ((hash>=0)) {
                if ((hash == (href.Length-1))) {
                    href = href.Substring(0, hash - 0);
                } else {
                    fragmentId = href.Substring((hash+1));
                    href = href.Substring(0, hash - 0);
                    if (!(Saxon.Hej.om.NameChecker.isValidNCName(fragmentId))) {
                        Saxon.Hej.trans.XPathException de = new Saxon.Hej.trans.XPathException((("The fragment identifier "+Saxon.Hej.trans.Err.wrap(fragmentId))+" is not a valid NCName"));
                        de.setErrorCode("XTDE1160");
                        de.setXPathContext(c);
                        de.setLocator(locator);
                        throw de;
                    }
                }
            }
            Saxon.Hej.Controller controller = c.getController();
            if ((controller == null)) {
                throw new Saxon.Hej.trans.XPathException("doc() function is not available in this environment");
            }
            Saxon.Hej.om.DocumentKey documentKey = computeDocumentKey(href, baseURI, packageData, c);
            Saxon.Hej.om.TreeInfo doc = config.getGlobalDocumentPool().find(documentKey);
            if ((doc != null)) {
                return doc.getRootNode();
            }
            Saxon.Hej.om.DocumentPool pool = controller.getDocumentPool();
            lock (controller) {
                doc = pool.find(documentKey);
                if ((doc != null)) {
                    return getFragment(doc, fragmentId, c, locator);
                }
                if (((controller is Saxon.Hej.trans.XsltController) && !(((Saxon.Hej.trans.XsltController)(controller)).checkUniqueOutputDestination(documentKey)))) {
                    pool.markUnavailable(documentKey);
                    Saxon.Hej.trans.XPathException err = new Saxon.Hej.trans.XPathException(("Cannot read a document that was written during the same transformation: "+documentKey));
                    err.setXPathContext(c);
                    err.setErrorCode("XTRE1500");
                    err.setLocator(locator);
                    throw err;
                }
                if (pool.isMarkedUnavailable(documentKey)) {
                    Saxon.Hej.trans.XPathException err = new Saxon.Hej.trans.XPathException(("Document has been marked not available: "+documentKey));
                    err.setXPathContext(c);
                    err.setErrorCode("FODC0002");
                    err.setLocator(locator);
                    throw err;
                }
            }
            try {
                Saxon.Ejavax.xml.transform.Source source = resolveURI(href, baseURI, documentKey.ToString(), c);
                if ((source is Saxon.Hej.lib.EmptySource)) {
                    return null;
                }
                source = config.getSourceResolver().resolveSource(source, config);
                Saxon.Hej.om.TreeInfo newdoc;
                if (sourceIsTree(source)) {
                    Saxon.Hej.om.NodeInfo startNode = controller.prepareInputTree(source);
                    newdoc = startNode.getTreeInfo();
                } else {
                    Saxon.Hej.@event.Builder b = controller.makeBuilder();
                    b.setUseEventLocation(true);
                    if ((b is Saxon.Hej.tree.tiny.TinyBuilder)) {
                        ((Saxon.Hej.tree.tiny.TinyBuilder)(b)).setStatistics(config.getTreeStatistics().SOURCE_DOCUMENT_STATISTICS);
                    }
                    Saxon.Hej.@event.Receiver s = b;
                    if ((options == null)) {
                        options = new Saxon.Hej.lib.ParseOptions(b.getPipelineConfiguration().getParseOptions());
                        if ((packageData is Saxon.Hej.style.StylesheetPackage)) {
                            Saxon.Hej.om.SpaceStrippingRule rule = ((Saxon.Hej.style.StylesheetPackage)(packageData)).getSpaceStrippingRule();
                            if ((rule != Saxon.Hej.om.NoElementsSpaceStrippingRule.getInstance())) {
                                options.setSpaceStrippingRule(rule);
                            }
                        }
                        options.setSchemaValidationMode(controller.getSchemaValidationMode());
                    }
                    b.getPipelineConfiguration().setParseOptions(options);
                    if (options.isLineNumbering()) {
                        b.setLineNumbering(true);
                    }
                    if (((packageData is Saxon.Hej.style.StylesheetPackage) && ((Saxon.Hej.style.StylesheetPackage)(packageData)).isStripsTypeAnnotations())) {
                        s = config.getAnnotationStripper(s);
                    }
                    Saxon.Hej.expr.parser.PathMap map = controller.getPathMapForDocumentProjection();
                    if ((map != null)) {
                        Saxon.Hej.expr.parser.PathMap.PathMapRoot pathRoot = map.getRootForDocument(documentKey.ToString());
                        if ((((pathRoot != null) && !(pathRoot.isReturnable())) && !(pathRoot.hasUnknownDependencies()))) {
                            options.addFilter(config.makeDocumentProjector(pathRoot));
                        }
                    }
                    s.setPipelineConfiguration(b.getPipelineConfiguration());
                    try {
                        Saxon.Hej.@event.Sender.send(source, s, options);
                        newdoc = b.getCurrentRoot().getTreeInfo();
                        b.reset();
                    } catch (Saxon.Hej.trans.XPathException err) {
                        if (((err.getErrorCodeLocalPart() == null) || err.getErrorCodeLocalPart().Equals("SXXP0003"))) {
                            err.setErrorCode("FODC0002");
                        }
                        throw;
                    } finally {
                        if (options.isPleaseCloseAfterUse()) {
                            Saxon.Hej.lib.ParseOptions.close(source);
                        }
                    }
                }
                lock (controller) {
                    doc = pool.find(documentKey);
                    if ((doc != null)) {
                        return getFragment(doc, fragmentId, c, locator);
                    }
                    controller.registerDocument(newdoc, documentKey);
                    if ((controller is Saxon.Hej.trans.XsltController)) {
                        ((Saxon.Hej.trans.XsltController)(controller)).addUnavailableOutputDestination(documentKey);
                    }
                }
                return getFragment(newdoc, fragmentId, c, locator);
            } catch (Saxon.Ejavax.xml.transform.TransformerException err) {
                pool.markUnavailable(documentKey);
                Saxon.Hej.trans.XPathException xerr = Saxon.Hej.trans.XPathException.makeXPathException(err);
                xerr.maybeSetLocation(locator);
                string code = (((err.getException() is System.UriFormatException)) ? "FODC0005" : "FODC0002");
                xerr.maybeSetErrorCode(code);
                throw xerr;
            }
        }
        public static Saxon.Ejavax.xml.transform.Source resolveURI(string href, string baseURI, string documentKey, Saxon.Hej.expr.XPathContext context) {
            Saxon.Ejavax.xml.transform.URIResolver resolver = context.getURIResolver();
            Saxon.Ejavax.xml.transform.Source source;
            if ((baseURI == null)) {
                try {
                    System.Uri uri = new System.Uri(href, System.UriKind.RelativeOrAbsolute);
                    if (!(uri.IsAbsoluteUri)) {
                        throw new Saxon.Hej.trans.XPathException((("Relative URI passed to document() function ("+href)+"); but no base URI is available"), "XTDE1162");
                    }
                } catch (System.UriFormatException e) {
                    throw new Saxon.Hej.trans.XPathException(("Invalid URI passed to document() function: "+href), "FODC0005");
                }
            }
            try {
                if (((resolver is Saxon.Hej.lib.RelativeURIResolver) && (documentKey != null))) {
                    source = ((Saxon.Hej.lib.RelativeURIResolver)(resolver)).dereference(documentKey);
                } else {
                    source = resolver.resolve(href, baseURI);
                }
            } catch (System.Exception ex) {
                Saxon.Hej.trans.XPathException de = new Saxon.Hej.trans.XPathException((((("Exception thrown by URIResolver resolving `"+href)+"` against `")+baseURI)+"'"), ex);
                if (context.getConfiguration().getBooleanProperty(Saxon.Hej.lib.Feature<bool>.TRACE_EXTERNAL_FUNCTIONS)) {
                    System.Console.Error.WriteLine(ex);
                }
                throw de;
            }
            if ((((source is Saxon.Ejavax.xml.transform.stream.StreamSource) && (((Saxon.Ejavax.xml.transform.stream.StreamSource)(source)).getInputStream() == null)) && (((Saxon.Ejavax.xml.transform.stream.StreamSource)(source)).getReader() == null))) {
                string uri = source.getSystemId();
                if ((uri != null)) {
                    resolver = context.getController().getStandardURIResolver();
                    try {
                        source = resolver.resolve(uri, "");
                    } catch (Saxon.Ejavax.xml.transform.TransformerException ex) {
                        throw Saxon.Hej.trans.XPathException.makeXPathException(ex);
                    }
                } else {
                    source = null;
                }
            }
            if (((source == null) && !(((resolver is Saxon.Hej.trans.NonDelegatingURIResolver))))) {
                resolver = context.getController().getStandardURIResolver();
                try {
                    if (((resolver is Saxon.Hej.lib.RelativeURIResolver) && (documentKey != null))) {
                        source = ((Saxon.Hej.lib.RelativeURIResolver)(resolver)).dereference(documentKey);
                    } else {
                        source = resolver.resolve(href, baseURI);
                    }
                } catch (Saxon.Ejavax.xml.transform.TransformerException ex) {
                    throw Saxon.Hej.trans.XPathException.makeXPathException(ex);
                }
            }
            return source;
        }
        protected internal static Saxon.Hej.om.DocumentKey computeDocumentKey(string href, string baseURI, Saxon.Hej.expr.PackageData packageData, Saxon.Hej.expr.XPathContext c) {
            Saxon.Hej.Controller controller = c.getController();
            Saxon.Ejavax.xml.transform.URIResolver resolver = controller.getURIResolver();
            if ((resolver == null)) {
                resolver = controller.getStandardURIResolver();
            }
            return computeDocumentKey(href, baseURI, packageData, resolver, true);
        }
        public static Saxon.Hej.om.DocumentKey computeDocumentKey(string href, string baseURI, Saxon.Hej.expr.PackageData packageData, Saxon.Ejavax.xml.transform.URIResolver resolver, bool strip) {
            string absURI;
            if ((resolver is Saxon.Hej.lib.RelativeURIResolver)) {
                try {
                    absURI = ((Saxon.Hej.lib.RelativeURIResolver)(resolver)).makeAbsolute(href, baseURI);
                } catch (Saxon.Ejavax.xml.transform.TransformerException e) {
                    absURI = ('/'+href);
                }
            } else {
                href = Saxon.Hej.functions.ResolveURI.escapeSpaces(href);
                if ((baseURI == null)) {
                    try {
                        absURI = new System.Uri(href, System.UriKind.RelativeOrAbsolute).ToString();
                    } catch (System.UriFormatException err) {
                        absURI = ('/'+href);
                    }
                } else {
                    if (href.Length==0) {
                        absURI = baseURI;
                    } else {
                        try {
                            absURI = Saxon.Hej.functions.ResolveURI.makeAbsolute(href, baseURI).ToString();
                        } catch (Exception err) when (err is System.UriFormatException || err is Saxon.Ejava.lang.IllegalArgumentException) {
                            absURI = ((baseURI+"/../")+href);
                        }
                    }
                }
            }
            if (((strip && (packageData is Saxon.Hej.style.StylesheetPackage)) && (((Saxon.Hej.style.StylesheetPackage)(packageData)).getSpaceStrippingRule() != Saxon.Hej.om.NoElementsSpaceStrippingRule.getInstance()))) {
                string name = ((Saxon.Hej.style.StylesheetPackage)(packageData)).getPackageName();
                if ((name != null)) {
                    return new Saxon.Hej.om.DocumentKey(absURI, name, ((Saxon.Hej.style.StylesheetPackage)(packageData)).getPackageVersion());
                }
            }
            return new Saxon.Hej.om.DocumentKey(absURI);
        }
        public static Saxon.Hej.om.NodeInfo preLoadDoc(string href, string baseURI, Saxon.Hej.Configuration config, Saxon.Ejavax.xml.transform.SourceLocator locator) {
            int hash = href.IndexOf('#');
            if ((hash>=0)) {
                throw new Saxon.Hej.trans.XPathException("Fragment identifier not supported for preloaded documents");
            }
            string documentKey;
            Saxon.Ejavax.xml.transform.URIResolver resolver = config.getURIResolver();
            if ((resolver is Saxon.Hej.lib.RelativeURIResolver)) {
                try {
                    documentKey = ((Saxon.Hej.lib.RelativeURIResolver)(resolver)).makeAbsolute(href, baseURI);
                } catch (Saxon.Ejavax.xml.transform.TransformerException e) {
                    documentKey = ('/'+href);
                    baseURI = "";
                }
            } else {
                if ((baseURI == null)) {
                    try {
                        documentKey = new System.Uri(href, System.UriKind.RelativeOrAbsolute).ToString();
                    } catch (System.UriFormatException err) {
                        documentKey = ('/'+href);
                        baseURI = "";
                    }
                } else {
                    if (href.Length==0) {
                        documentKey = baseURI;
                    } else {
                        try {
                            documentKey = Saxon.Hej.functions.ResolveURI.makeAbsolute(href, baseURI).ToString();
                        } catch (Exception err) when (err is System.UriFormatException || err is Saxon.Ejava.lang.IllegalArgumentException) {
                            documentKey = ((baseURI+"/../")+href);
                        }
                    }
                }
            }
            Saxon.Hej.om.TreeInfo doc = config.getGlobalDocumentPool().find(documentKey);
            if ((doc != null)) {
                return doc.getRootNode();
            }
            try {
                Saxon.Ejavax.xml.transform.URIResolver r = resolver;
                Saxon.Ejavax.xml.transform.Source source = null;
                if ((r != null)) {
                    try {
                        source = r.resolve(href, baseURI);
                    } catch (System.Exception ex) {
                        Saxon.Hej.trans.XPathException de = new Saxon.Hej.trans.XPathException("Exception thrown by URIResolver", ex);
                        if (config.getBooleanProperty(Saxon.Hej.lib.Feature<bool>.TRACE_EXTERNAL_FUNCTIONS)) {
                            System.Console.Error.WriteLine(ex);
                        }
                        de.setLocator(locator);
                        throw de;
                    }
                }
                if (((source == null) && !(((r is Saxon.Hej.trans.NonDelegatingURIResolver))))) {
                    r = config.getSystemURIResolver();
                    source = r.resolve(href, baseURI);
                }
                source = config.getSourceResolver().resolveSource(source, config);
                Saxon.Hej.om.TreeInfo newdoc = config.buildDocumentTree(source);
                config.getGlobalDocumentPool().add(newdoc, documentKey);
                return newdoc.getRootNode();
            } catch (Saxon.Ejavax.xml.transform.TransformerException err) {
                Saxon.Hej.trans.XPathException xerr = Saxon.Hej.trans.XPathException.makeXPathException(err);
                xerr.setLocator(locator);
                xerr.setErrorCode("FODC0002");
                throw new Saxon.Hej.trans.XPathException(err);
            }
        }
        public static void sendDoc(string href, string baseURL, Saxon.Hej.expr.XPathContext c, Saxon.Hej.s9api.Location locator, Saxon.Hej.@event.Receiver @out, Saxon.Hej.lib.ParseOptions parseOptions) {
            Saxon.Hej.@event.PipelineConfiguration pipe = @out.getPipelineConfiguration();
            if ((pipe == null)) {
                pipe = c.getController().makePipelineConfiguration();
                pipe.setXPathContext(c);
                @out.setPipelineConfiguration(pipe);
            }
            string documentKey;
            if ((baseURL == null)) {
                try {
                    documentKey = new System.Uri(href, System.UriKind.RelativeOrAbsolute).ToString();
                } catch (System.UriFormatException err) {
                    documentKey = ('/'+href);
                    baseURL = "";
                }
            } else {
                if (href.Length==0) {
                    documentKey = baseURL;
                } else {
                    try {
                        documentKey = Saxon.Hej.functions.ResolveURI.makeAbsolute(href, baseURL).ToString();
                    } catch (Exception err) when (err is System.UriFormatException || err is Saxon.Ejava.lang.IllegalArgumentException) {
                        documentKey = ((baseURL+"/../")+href);
                    }
                }
            }
            Saxon.Hej.Controller controller = c.getController();
            Saxon.Hej.om.TreeInfo doc = controller.getDocumentPool().find(documentKey);
            Saxon.Ejavax.xml.transform.Source source = null;
            if ((doc != null)) {
                source = doc.getRootNode().asActiveSource();
            } else {
                try {
                    Saxon.Ejavax.xml.transform.URIResolver r = controller.getURIResolver();
                    if ((r != null)) {
                        source = r.resolve(href, baseURL);
                    }
                    if ((source == null)) {
                        r = controller.getStandardURIResolver();
                        source = r.resolve(href, baseURL);
                    }
                    if (sourceIsTree(source)) {
                        Saxon.Hej.om.NodeInfo startNode = controller.prepareInputTree(source);
                        source = startNode.getRoot().asActiveSource();
                    }
                } catch (Saxon.Ejavax.xml.transform.TransformerException err) {
                    Saxon.Hej.trans.XPathException xerr = Saxon.Hej.trans.XPathException.makeXPathException(err);
                    xerr.setLocator(locator);
                    xerr.maybeSetErrorCode("FODC0005");
                    throw xerr;
                }
            }
            if (controller.getConfiguration().isTiming()) {
                controller.getConfiguration().getLogger().info(("Streaming input document "+source.getSystemId()));
            }
            @out.setPipelineConfiguration(pipe);
            try {
                Saxon.Hej.@event.Sender.send(source, @out, parseOptions);
            } catch (Saxon.Hej.trans.XPathException e) {
                e.maybeSetLocation(locator);
                e.maybeSetErrorCode("FODC0002");
                throw;
            }
        }
        private static Saxon.Hej.om.NodeInfo getFragment(Saxon.Hej.om.TreeInfo doc, string fragmentId, Saxon.Hej.expr.XPathContext context, Saxon.Hej.s9api.Location locator) {
            if ((fragmentId == null)) {
                return doc.getRootNode();
            }
            if (!(Saxon.Hej.om.NameChecker.isValidNCName(fragmentId))) {
                context.getController().warning("Invalid fragment identifier in URI", "XTDE1160", locator);
                return doc.getRootNode();
            }
            return doc.selectID(fragmentId, false);
        }
    
    }
}