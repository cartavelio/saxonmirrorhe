using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.style {
    public class XSLImportSchema : Saxon.Hej.style.StyleElement {
        public override bool isDeclaration() {
            return true;
        }
        protected internal override void prepareAttributes() {
            string @namespace = null;
            foreach (Saxon.Hej.om.AttributeInfo att in attributes()) {
                Saxon.Hej.om.NodeName attName = att.getNodeName();
                string value = att.getValue();
                string f = attName.getDisplayName();
                if (f.Equals("schema-location")) {
                } else {
                    if (f.Equals("namespace")) {
                        @namespace = Saxon.Hej.value.Whitespace.trim(value);
                    } else {
                        checkUnknownAttribute(attName);
                    }
                }
            }
            if ("".Equals(@namespace)) {
                compileError(("The zero-length string is not a valid namespace URI. "+"For a schema with no namespace, omit the namespace attribute"));
            }
        }
        public override void validate(Saxon.Hej.style.ComponentDeclaration decl) {
            checkTopLevel("XTSE0010", false);
        }
        public override void index(Saxon.Hej.style.ComponentDeclaration decl, Saxon.Hej.style.PrincipalStylesheetModule top) {
        }
        public void readSchema() {
            try {
                string schemaLoc = Saxon.Hej.value.Whitespace.trim(getAttributeValue(Saxon.Hej.om.NamespaceUri.NULL, "schema-location"));
                string @namespace = Saxon.Hej.value.Whitespace.trim(getAttributeValue(Saxon.Hej.om.NamespaceUri.NULL, "namespace"));
                if ((@namespace == null)) {
                    @namespace = "";
                } else {
                    @namespace = @namespace.Trim();
                }
                Saxon.Hej.Configuration config = getConfiguration();
                try {
                    config.checkLicensedFeature(Saxon.Hej.Configuration.LicenseFeature.ENTERPRISE_XSLT, "xsl:import-schema", getPackageData().getLocalLicenseId());
                } catch (Saxon.Hej.trans.LicenseException err) {
                    Saxon.Hej.trans.XPathException xe = new Saxon.Hej.trans.XPathException(err);
                    xe.setErrorCode("XTSE1650");
                    xe.setLocator(this);
                    throw xe;
                }
                Saxon.Hej.om.NodeInfo inlineSchema = null;
                Saxon.Hej.om.NamespaceUri targetNamespace = null;
                foreach (Saxon.Hej.om.NodeInfo child in children()) {
                    if ((inlineSchema != null)) {
                        compileError((getDisplayName()+" must not have more than one child element"));
                    }
                    inlineSchema = child;
                    if ((inlineSchema.getFingerprint() != Saxon.Hej.om.StandardNames.XS_SCHEMA)) {
                        compileError((("The only child element permitted for "+getDisplayName())+" is xs:schema"));
                    }
                    if ((schemaLoc != null)) {
                        compileError("The schema-location attribute must be absent if an inline schema is present", "XTSE0215");
                    }
                    if (@namespace.Length==0) {
                        @namespace = inlineSchema.getAttributeValue(Saxon.Hej.om.NamespaceUri.NULL, "targetNamespace");
                        if ((@namespace == null)) {
                            @namespace = "";
                        }
                    }
                    targetNamespace = Saxon.Hej.om.NamespaceUri.of(@namespace);
                    targetNamespace = config.readInlineSchema(inlineSchema, targetNamespace, getCompilation().getCompilerInfo().getErrorReporter());
                    getPrincipalStylesheetModule().addImportedSchema(targetNamespace);
                }
                if ((inlineSchema != null)) {
                    return ;
                }
                if (((@namespace.Equals(Saxon.Hej.lib.NamespaceConstant.XML) || @namespace.Equals(Saxon.Hej.lib.NamespaceConstant.FN)) || @namespace.Equals(Saxon.Hej.lib.NamespaceConstant.SCHEMA_INSTANCE))) {
                    targetNamespace = Saxon.Hej.om.NamespaceUri.of(@namespace);
                    config.addSchemaForBuiltInNamespace(targetNamespace);
                    getPrincipalStylesheetModule().addImportedSchema(targetNamespace);
                    return ;
                }
                targetNamespace = Saxon.Hej.om.NamespaceUri.of(@namespace);
                bool namespaceKnown = config.isSchemaAvailable(targetNamespace);
                if (((schemaLoc == null) && !(namespaceKnown))) {
                    issueWarning(("No schema for this namespace is known, "+"and no schema-location was supplied, so no schema has been imported"), Saxon.Hej.trans.SaxonErrorCode.SXWN9006);
                    return ;
                }
                if ((namespaceKnown && !(config.getBooleanProperty(Saxon.Hej.lib.Feature<bool>.MULTIPLE_SCHEMA_IMPORTS)))) {
                    if ((schemaLoc != null)) {
                        issueWarning((("The schema document at "+schemaLoc)+" is ignored because a schema for this namespace is already loaded"), Saxon.Hej.trans.SaxonErrorCode.SXWN9006);
                    }
                }
                if (!(namespaceKnown)) {
                    Saxon.Hej.@event.PipelineConfiguration pipe = config.makePipelineConfiguration();
                    pipe.setErrorReporter(getCompilation().getCompilerInfo().getErrorReporter());
                    targetNamespace = config.readSchema(pipe, getBaseURI(), schemaLoc, targetNamespace);
                }
                getPrincipalStylesheetModule().addImportedSchema(targetNamespace);
            } catch (Saxon.Hej.type.SchemaException err) {
                string errorCode = ((err.getErrorCodeLocalPart() == null) ? "XTSE0220" : err.getErrorCodeLocalPart());
                compileError(err.Message, errorCode);
            }
        }
        public override void compileDeclaration(Saxon.Hej.style.Compilation compilation, Saxon.Hej.style.ComponentDeclaration decl) {
        }
    
    }
}