using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.ee.schema.sdoc {
    public class XSDImport : Saxon.Eej.ee.schema.sdoc.AnnotationParent {
        private Saxon.Eej.ee.schema.PreparedSchema importedSchema;
        public XSDImport()  {
        }
        protected internal override void prepareAttributes() {
            Saxon.Hej.om.AttributeMap atts = attributes();
            string[] allowed = {"id", "namespace", "schemaLocation"};
            allowAttributes(atts, allowed);
            processId();
            string schemaLocation = atts.getValue("", "schemaLocation");
            string @namespace = atts.getValue("", "namespace");
            if ((((schemaLocation == null)) && ((@namespace == null)))) {
                return ;
            }
            Saxon.Eej.ee.schema.sdoc.XSDSchema thisSchemaDocument = getXSDSchema();
            Saxon.Eej.config.EnterpriseConfiguration config = (Saxon.Eej.config.EnterpriseConfiguration)(getConfiguration());
            Saxon.Eej.ee.schema.SchemaCompiler compiler = thisSchemaDocument.getSchemaCompiler();
            if ((@namespace == null)) {
                @namespace = "";
            }
            string documentBase = getBaseURI();
            if ((schemaLocation != null)) {
                try {
                    string location = Saxon.Hej.functions.ResolveURI.makeAbsolute(schemaLocation, documentBase).ToString();
                    Saxon.Eej.ee.schema.sdoc.XSDSchema existing = compiler.getExistingSchemaDocument(location, @namespace);
                    if ((existing != null)) {
                        importedSchema = existing.getSchema();
                        return ;
                    }
                } catch (System.UriFormatException e) {
                }
            }
            if (@namespace.Equals(Saxon.Hej.lib.NamespaceConstant.XML)) {
                if ((((schemaLocation == null) || schemaLocation.Equals("http://www.w3.org/2001/xml.xsd")) || schemaLocation.Equals("http://www.w3.org/2001/03/xml.xsd"))) {
                    if ((config.getNamespaceStatus(@namespace) != Saxon.Eej.ee.schema.PreparedSchema.NAMESPACE_KNOWN)) {
                        importedSchema = /*134*/Saxon.Eej.ee.schema.sdoc.XMLNamespaceSchema.buildSchema(config);
                        config.addSchema(importedSchema);
                    }
                    return ;
                }
            }
            if (@namespace.Equals(thisSchemaDocument.getTargetNamespace())) {
                error("The namespace attribute in the xs:import element cannot be the same as the targetNamespace of the containing schema");
                return ;
            }
            Saxon.Ejavax.xml.transform.Source source = null;
            if ((schemaLocation != null)) {
                try {
                    source = /*134*/Saxon.Eej.ee.schema.sdoc.SchemaReader.getSource(documentBase, schemaLocation, compiler, @namespace);
                } catch (Saxon.Hej.type.SchemaException e) {
                    processSchemaException(schemaLocation, @namespace, e);
                    return ;
                }
            }
            if ((source != null)) {
                if (compiler.isBeingRead(source.getSystemId())) {
                    return ;
                }
            }
            int status = config.getNamespaceStatus(@namespace);
            if ((status == Saxon.Eej.ee.schema.PreparedSchema.NAMESPACE_UNDER_CONSTRUCTION)) {
                if (((source == null) || ((!(compiler.isBeingRead(source.getSystemId())) && !(config.getBooleanProperty(Saxon.Hej.lib.Feature<bool>.MULTIPLE_SCHEMA_IMPORTS)))))) {
                    if ((schemaLocation != null)) {
                        string message = (("The schema document at "+schemaLocation)+" is not being read because schema components for this namespace are already being processed");
                        if (!(@namespace.Equals(Saxon.Hej.lib.NamespaceConstant.XML))) {
                            message += ". To force the schema document to be read, set --multipleSchemaImports:on";
                        }
                        warning(message);
                    }
                    return ;
                }
            } else {
                if ((status == Saxon.Eej.ee.schema.PreparedSchema.NAMESPACE_SEALED)) {
                    return ;
                } else {
                    if (((status == Saxon.Eej.ee.schema.PreparedSchema.NAMESPACE_KNOWN) && !(config.getBooleanProperty(Saxon.Hej.lib.Feature<bool>.MULTIPLE_SCHEMA_IMPORTS)))) {
                        if ((schemaLocation != null)) {
                            string message = (("The schema document at "+schemaLocation)+" is not being read because schema components for this namespace are already available");
                            if (!(@namespace.Equals(Saxon.Hej.lib.NamespaceConstant.XML))) {
                                message += ". To force the schema document to be read, set --multipleSchemaImports:on";
                            }
                            warning(message);
                        }
                        return ;
                    }
                }
            }
            if (((source == null) && (schemaLocation != null))) {
                warning(("Failed to locate imported schema document: "+schemaLocation));
                importedSchema = new Saxon.Eej.ee.schema.SingleNamespaceSchema((Saxon.Eej.config.EnterpriseConfiguration)(getConfiguration()), @namespace);
                return ;
            }
            if ((schemaLocation == null)) {
                if (@namespace.Equals(Saxon.Hej.lib.NamespaceConstant.SCHEMA_INSTANCE)) {
                    importedSchema = /*134*/Saxon.Eej.ee.schema.sdoc.XSINamespaceSchema.buildSchema(config);
                    config.addSchema(importedSchema);
                    return ;
                } else {
                    if (@namespace.Equals(Saxon.Hej.lib.NamespaceConstant.FN)) {
                        if (!(config.isSchemaAvailable(Saxon.Hej.lib.NamespaceConstant.FN))) {
                            System.Collections.Generic.IList<string> messages = new System.Collections.Generic.List<string>();
                            System.IO.Stream @in = Saxon.Hej.Version.platform.locateResource("xpath-functions.xsd", messages);
                            if (config.isTiming()) {
                                config.getLogger().info(("Loading schema from resources for: "+Saxon.Hej.lib.NamespaceConstant.FN));
                            }
                            source = new Saxon.Ejavax.xml.transform.stream.StreamSource(@in, "http://www.w3.org/2005/xpath-functions.scm");
                        }
                    } else {
                        Saxon.Hej.lib.SchemaURIResolver resolver = compiler.getSchemaURIResolver();
                        if ((resolver != null)) {
                            try {
                                Saxon.Ejavax.xml.transform.Source[] sources = resolver.resolve(@namespace, getBaseURI(), new string[0]);
                                if (((sources == null) || (sources.Length == 0))) {
                                    warning(("Cannot locate a schema document for namespace "+@namespace));
                                    importedSchema = new Saxon.Eej.ee.schema.SingleNamespaceSchema((Saxon.Eej.config.EnterpriseConfiguration)(getConfiguration()), @namespace);
                                    return ;
                                } else {
                                    if ((sources.Length>1)) {
                                        error("SchemaURIResolver returned more than one schema document");
                                    } else {
                                        source = sources[0];
                                    }
                                }
                            } catch (Saxon.Hej.trans.XPathException err) {
                                throw new Saxon.Hej.type.SchemaException(err);
                            }
                        }
                    }
                }
            }
            Saxon.Eej.ee.schema.sdoc.SchemaDocument importee = null;
            int errors;
            try {
                Saxon.Hej.@event.PipelineConfiguration pipe = getSchemaNodeFactory().getPipelineConfiguration();
                importee = /*134*/Saxon.Eej.ee.schema.sdoc.SchemaReader.read(source, compiler, pipe, null);
                Saxon.Eej.ee.schema.sdoc.XSDSchema s = importee.getXSDSchema();
                errors = importee.getXSDSchema().getErrorCount();
                importedSchema = s.getSchema();
            } catch (Saxon.Hej.type.SchemaException e) {
                processSchemaException(schemaLocation, @namespace, e);
                return ;
            }
            bool valid = ((errors == 0));
            config.removeSchema(@namespace);
            if (!(valid)) {
                throw new Saxon.Hej.type.SchemaException("Imported schema is invalid");
            }
            string importedNamespace = importee.getTargetNamespace();
            if (thisSchemaDocument.getTargetNamespace().Equals(importedNamespace)) {
                error(("The targetNamespace of the imported schema must not be the same as "+"the targetNamespace of the importing schema"));
                return ;
            }
            if (!(@namespace.Equals(importedNamespace))) {
                error("The namespace attribute in xs:import differs from the targetNamespace of the imported schema");
                return ;
            }
            config.addSchema(importedSchema);
        }
        private void processSchemaException(string schemaLocation, string @namespace, Saxon.Hej.type.SchemaException e) {
            System.Exception cause = e.getException();
            if (/*134*/Saxon.Eej.ee.schema.sdoc.XSDInclude.isFileNotFoundException(cause)) {
                warning((((((("Imported schema document "+schemaLocation)+" cannot be located: ")+cause.GetType().Name)+" (")+cause.Message)+")"));
            } else {
                error(("Failed to process imported schema document: "+e.Message));
            }
            importedSchema = new Saxon.Eej.ee.schema.SingleNamespaceSchema((Saxon.Eej.config.EnterpriseConfiguration)(getConfiguration()), @namespace);
            return ;
        }
        public Saxon.Eej.ee.schema.PreparedSchema getImportedSchema() {
            return importedSchema;
        }
        public string getTargetNamespace() {
            string uri = attributes().getValue("", "namespace");
            if ((uri == null)) {
                return "";
            } else {
                return uri;
            }
        }
    
    }
}