using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.ee.schema.sdoc {
    public class SchemaDocumentTransformer {
        private Saxon.Hej.PreparedStylesheet pss;
        public SchemaDocumentTransformer(Saxon.Hej.Configuration config, string filename)  {
            compileStylesheet(config, filename);
        }
        private void compileStylesheet(Saxon.Hej.Configuration config, string filename) {
            try {
                System.IO.Stream @in = Saxon.Hej.Version.platform.locateResource(filename, new System.Collections.Generic.List<string>());
                Saxon.Ejavax.xml.transform.stream.StreamSource ss = new Saxon.Ejavax.xml.transform.stream.StreamSource(@in, filename);
                Saxon.Hej.trans.CompilerInfo info = config.getDefaultXsltCompilerInfo();
                pss = Saxon.Hej.style.Compilation.compileSingletonPackage(config, info, ss);
            } catch (Saxon.Hej.trans.XPathException e) {
                throw new Saxon.Hej.type.SchemaException(e);
            }
        }
        public Saxon.Hej.om.TreeInfo transform(Saxon.Eej.ee.schema.SchemaCompiler compiler, Saxon.Hej.@event.PipelineConfiguration pipe, Saxon.Eej.ee.schema.sdoc.SchemaElement referrer, Saxon.Hej.om.NodeInfo input, Saxon.Hej.expr.instruct.GlobalParameterSet @params) {
            try {
                Saxon.Hej.trans.XsltController transformer = pss.newController();
                transformer.initializeController(@params);
                Saxon.Hej.tree.linked.LinkedTreeBuilder builder = new Saxon.Hej.tree.linked.LinkedTreeBuilder(pipe, Saxon.Hej.om.Durability.LASTING);
                builder.setNodeFactory(new Saxon.Eej.ee.schema.sdoc.SchemaNodeFactory(compiler, pipe, referrer));
                transformer.setBaseOutputURI(input.getSystemId());
                transformer.setGlobalContextItem(input);
                transformer.applyTemplates(input, new Saxon.Hej.@event.TreeReceiver(builder));
                return builder.getCurrentRoot().getTreeInfo();
            } catch (Saxon.Ejavax.xml.transform.TransformerException e) {
                throw new Saxon.Hej.type.SchemaException(e);
            }
        }
    
    }
}