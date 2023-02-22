using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.ee.schema.sdoc {
    public class XSDInclude : Saxon.Eej.ee.schema.sdoc.AnnotationParent {
        private Saxon.Eej.ee.schema.PreparedSchema includedSchema;
        public XSDInclude()  {
        }
        protected internal override void prepareAttributes() {
            Saxon.Hej.om.AttributeMap atts = attributes();
            string[] allowed = {"id", "schemaLocation"};
            allowAttributes(atts, allowed);
            processId();
            string schemaLocation = atts.getValue("", "schemaLocation");
            if ((schemaLocation == null)) {
                missingAttribute("schemaLocation");
                return ;
            }
            Saxon.Eej.ee.schema.SchemaCompiler compiler = getXSDSchema().getSchemaCompiler();
            string documentBase = getBaseURI();
            try {
                string location = Saxon.Hej.functions.ResolveURI.makeAbsolute(schemaLocation, documentBase).ToString();
                Saxon.Eej.ee.schema.sdoc.XSDSchema existing = compiler.getExistingSchemaDocument(location, getXSDSchema().getTargetNamespace());
                if ((existing != null)) {
                    includedSchema = existing.getSchema();
                    return ;
                }
            } catch (System.UriFormatException e) {
            }
            Saxon.Ejavax.xml.transform.Source source = null;
            try {
                source = /*134*/Saxon.Eej.ee.schema.sdoc.SchemaReader.getSource(getBaseURI(), schemaLocation, compiler, null);
            } catch (Saxon.Hej.type.SchemaException e) {
                handleSchemaException(schemaLocation, e);
            }
            if ((source == null)) {
                warning(("Failed to locate included schema document "+schemaLocation));
                includedSchema = new Saxon.Eej.ee.schema.SingleNamespaceSchema((Saxon.Eej.config.EnterpriseConfiguration)(getConfiguration()), "");
            } else {
                if (compiler.isBeingRead(source.getSystemId())) {
                    warning((("Module "+source.getSystemId())+" includes itself recursively"));
                    return ;
                }
                try {
                    Saxon.Hej.@event.PipelineConfiguration pipe = getSchemaNodeFactory().getPipelineConfiguration();
                    Saxon.Eej.ee.schema.sdoc.SchemaDocument inc = /*134*/Saxon.Eej.ee.schema.sdoc.SchemaReader.read(source, compiler, pipe, this);
                    Saxon.Eej.ee.schema.sdoc.XSDSchema includedXSDSchema = inc.getXSDSchema();
                    includedXSDSchema.setReferrer(this);
                    includedSchema = includedXSDSchema.getSchema();
                    compiler.markAsRead(source.getSystemId(), includedXSDSchema);
                } catch (Saxon.Hej.type.SchemaException e) {
                    handleSchemaException(schemaLocation, e);
                }
            }
        }
        private void handleSchemaException(string schemaLocation, Saxon.Hej.type.SchemaException e) {
            System.Exception cause = e.getException();
            if (isFileNotFoundException(cause)) {
                warning((((((("Included schema document "+schemaLocation)+" cannot be located: ")+cause.GetType().Name)+" (")+cause.Message)+")"));
                includedSchema = new Saxon.Eej.ee.schema.SingleNamespaceSchema((Saxon.Eej.config.EnterpriseConfiguration)(getConfiguration()), "");
            } else {
                error(("Failed to process included schema document "+schemaLocation));
                throw e;
            }
        }
        public static bool isFileNotFoundException(System.Exception cause) {
            return cause.Message.StartsWith("Unable to retrieve");
        }
        public Saxon.Eej.ee.schema.PreparedSchema getIncludedSchema() {
            return includedSchema;
        }
    
    }
}