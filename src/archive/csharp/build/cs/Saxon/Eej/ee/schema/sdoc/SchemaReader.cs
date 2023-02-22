using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.ee.schema.sdoc {
    public abstract class SchemaReader {
        private SchemaReader()  {
        }
        public static Saxon.Ejavax.xml.transform.Source getSource(string baseURI, string relativeURI, Saxon.Hej.@event.PipelineConfiguration pipe, string targetNamespace) {
            try {
                string[] locations = {relativeURI};
                Saxon.Ejavax.xml.transform.Source[] sources = null;
                Saxon.Hej.lib.SchemaURIResolver r = pipe.getSchemaURIResolver();
                if ((r != null)) {
                    sources = r.resolve(targetNamespace, baseURI, locations);
                }
                if ((sources == null)) {
                    r = new Saxon.Eej.ee.config.StandardSchemaResolver((Saxon.Eej.config.EnterpriseConfiguration)(pipe.getConfiguration()));
                    sources = r.resolve(targetNamespace, baseURI, locations);
                }
                if ((sources.Length == 0)) {
                    return null;
                } else {
                    if ((sources.Length == 1)) {
                        return sources[0];
                    } else {
                        throw new Saxon.Ejava.lang.UnsupportedOperationException("SchemaURIResolver is currently restricted to return one source for each schemaLocation");
                    }
                }
            } catch (Saxon.Hej.trans.XPathException e) {
                throw new Saxon.Hej.type.SchemaException(e);
            }
        }
        public static Saxon.Ejavax.xml.transform.Source[] getSources(string baseURI, string[] relativeURIs, Saxon.Hej.@event.PipelineConfiguration pipe, string targetNamespace) {
            try {
                Saxon.Ejavax.xml.transform.Source[] sources = pipe.getSchemaURIResolver().resolve(targetNamespace, baseURI, relativeURIs);
                if ((sources == null)) {
                    Saxon.Hej.lib.SchemaURIResolver r = new Saxon.Eej.ee.config.StandardSchemaResolver((Saxon.Eej.config.EnterpriseConfiguration)(pipe.getConfiguration()));
                    sources = r.resolve(targetNamespace, baseURI, relativeURIs);
                }
                return sources;
            } catch (Saxon.Hej.trans.XPathException e) {
                throw new Saxon.Hej.type.SchemaException(e);
            }
        }
        public static Saxon.Eej.ee.schema.sdoc.SchemaDocument read(Saxon.Ejavax.xml.transform.Source source, Saxon.Eej.ee.schema.SchemaCompiler compiler, Saxon.Hej.@event.PipelineConfiguration pipe, Saxon.Eej.ee.schema.sdoc.SchemaElement referrer) {
            bool tracing = pipe.getConfiguration().isTiming();
            string docId = source.getSystemId();
            if ((docId == null)) {
                docId = "(unknown URI)";
            }
            if (tracing) {
                pipe.getConfiguration().getLogger().info(("Loading schema document "+docId));
            }
            Saxon.Eej.ee.schema.sdoc.SchemaDocument schemaDocument = buildSchemaDocument(source, compiler, pipe, referrer);
            Saxon.Hej.tree.iter.AxisIterator children = schemaDocument.getDocument().getRootNode().iterateAxis(Saxon.Hej.om.AxisInfo.CHILD, Saxon.Hej.pattern.NodeKindTest.makeNodeKindTest(Saxon.Hej.type.Type.ELEMENT));
            Saxon.Hej.om.NodeInfo top = ((/*1430*/Saxon.Hej.om.NodeInfo)children.next());
            if (top.getURI().Equals(Saxon.Hej.lib.NamespaceConstant.SCM)) {
                if (tracing) {
                    pipe.getConfiguration().getLogger().info(("Loaded compiled schema document "+docId));
                }
                return schemaDocument;
            }
            if (!(((top is Saxon.Eej.ee.schema.sdoc.XSDSchema)))) {
                Saxon.Hej.type.SchemaException err = new Saxon.Hej.type.SchemaException("Outermost element of schema document must be xs:schema");
                Saxon.Hej.expr.parser.Loc loc = new Saxon.Hej.expr.parser.Loc(source.getSystemId(), ((top == null) ? 1 : top.getLineNumber()), -1);
                err.setLocator(loc);
                pipe.getErrorReporter().report(new Saxon.Hej.trans.XmlProcessingException(err));
                throw err;
            }
            compiler.setPipelineConfiguration(pipe);
            compiler.setBeingRead(source.getSystemId());
            ((Saxon.Eej.ee.schema.sdoc.XSDSchema)(top)).processAllAttributes();
            ((Saxon.Eej.ee.schema.sdoc.XSDSchema)(top)).validateSubtree(compiler);
            compiler.markAsRead(source.getSystemId(), (Saxon.Eej.ee.schema.sdoc.XSDSchema)(top));
            int errors = ((Saxon.Eej.ee.schema.sdoc.XSDSchema)(top)).getErrorCount();
            if ((errors>0)) {
                string msg = ((errors == 1) ? "One error was" : (errors+" errors were"));
                throw new Saxon.Hej.type.SchemaException((msg+" found while processing the schema"));
            }
            if (tracing) {
                pipe.getConfiguration().getLogger().info(("Finished loading schema document "+source.getSystemId()));
            }
            return schemaDocument;
        }
        public static Saxon.Eej.ee.schema.sdoc.SchemaDocument buildSchemaDocument(Saxon.Ejavax.xml.transform.Source source, Saxon.Eej.ee.schema.SchemaCompiler compiler, Saxon.Hej.@event.PipelineConfiguration pipe, Saxon.Eej.ee.schema.sdoc.SchemaElement referrer) {
            Saxon.Eej.ee.schema.sdoc.SchemaNodeFactory nodeFactory = new Saxon.Eej.ee.schema.sdoc.SchemaNodeFactory(compiler, pipe, referrer);
            Saxon.Hej.tree.linked.LinkedTreeBuilder schemaBuilder = new Saxon.Hej.tree.linked.LinkedTreeBuilder(pipe);
            schemaBuilder.setSystemId(source.getSystemId());
            schemaBuilder.setNodeFactory(nodeFactory);
            schemaBuilder.setLineNumbering(true);
            Saxon.Hej.@event.Stripper stripper = new Saxon.Hej.@event.Stripper(Saxon.Hej.om.AllElementsSpaceStrippingRule.getInstance(), schemaBuilder);
            Saxon.Eej.ee.schema.sdoc.VersionControlFilter vcFilter = new Saxon.Eej.ee.schema.sdoc.VersionControlFilter(compiler, stripper);
            Saxon.Hej.@event.CommentStripper commentStripper = new Saxon.Hej.@event.CommentStripper(vcFilter);
            if ((source is Saxon.Hej.om.NodeInfo)) {
                string @base = ((Saxon.Hej.om.NodeInfo)(source)).getBaseURI();
                if (!(@base.Equals(source.getSystemId()))) {
                    schemaBuilder.setSystemId(@base);
                }
                Saxon.Hej.@event.LocationCopier copier = new Saxon.Hej.@event.LocationCopier((((Saxon.Hej.om.NodeInfo)(source)).getNodeKind() == Saxon.Hej.type.Type.DOCUMENT));
                pipe.setCopyInformee(element => copier.notifyElementNode(element));
            }
            Saxon.Hej.tree.tiny.TinyBuilder scmBuilder = new Saxon.Hej.tree.tiny.TinyBuilder(pipe);
            scmBuilder.setSystemId(source.getSystemId());
            Saxon.Hej.@event.Valve valve = new Saxon.Hej.@event.Valve(Saxon.Hej.lib.NamespaceConstant.SCM, commentStripper, scmBuilder);
            Saxon.Hej.om.TreeInfo tree;
            Saxon.Hej.Configuration config = pipe.getConfiguration();
            Saxon.Hej.lib.ParseOptions options = new Saxon.Hej.lib.ParseOptions(config.getParseOptions());
            options.setSchemaValidationMode(Saxon.Hej.lib.Validation.PRESERVE);
            options.setErrorReporter(compiler.getErrorReporter());
            try {
                commentStripper.setPipelineConfiguration(pipe);
                sendSchemaSource(source, compiler, pipe, valve, config, options);
                if (valve.wasDiverted()) {
                    tree = scmBuilder.getTree();
                } else {
                    tree = schemaBuilder.getCurrentRoot().getTreeInfo();
                }
                schemaBuilder.reset();
            } catch (Saxon.Hej.trans.XPathException err) {
                System.Exception cause = err.getException();
                if ((cause != null)) {
                    if (isSAXException(cause)) {
                        throw new Saxon.Hej.type.SchemaException("Failed to parse schema document");
                    } else {
                        if ((cause is Saxon.Hej.type.SchemaException)) {
                            throw (Saxon.Hej.type.SchemaException)(cause);
                        } else {
                            throw new Saxon.Hej.type.SchemaException(cause);
                        }
                    }
                }
                throw new Saxon.Hej.type.SchemaException(err);
            } finally {
                if (options.isPleaseCloseAfterUse()) {
                    Saxon.Hej.lib.ParseOptions.close(source);
                }
            }
            Saxon.Hej.om.NodeInfo doc = tree.getRootNode();
            if ((((/*1430*/Saxon.Hej.om.NodeInfo)doc.iterateAxis(Saxon.Hej.om.AxisInfo.CHILD, Saxon.Hej.pattern.NodeKindTest.ELEMENT).next()) == null)) {
                throw new Saxon.Hej.type.SchemaException("Schema document is empty or absent");
            }
            return new Saxon.Eej.ee.schema.sdoc.SchemaDocument(tree);
        }
        private static bool isSAXException(System.Exception e) {
            return false;
        }
        private static void sendSchemaSource(Saxon.Ejavax.xml.transform.Source source, Saxon.Eej.ee.schema.SchemaCompiler compiler, Saxon.Hej.@event.PipelineConfiguration pipe, Saxon.Hej.@event.Valve valve, Saxon.Hej.Configuration config, Saxon.Hej.lib.ParseOptions options) {
            Saxon.Hej.@event.Sender.send(source, valve, options);
        }
    
    }
}