using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.style {
    public class XSLResultDocument : Saxon.Hej.style.StyleElement {

        static XSLResultDocument() {
            fans.Add("allow-duplicate-names");
            fans.Add("build-tree");
            fans.Add("byte-order-mark");
            fans.Add("cdata-section-elements");
            fans.Add("doctype-public");
            fans.Add("doctype-system");
            fans.Add("encoding");
            fans.Add("escape-uri-attributes");
            fans.Add("html-version");
            fans.Add("include-content-type");
            fans.Add("indent");
            fans.Add("item-separator");
            fans.Add("json-node-output-method");
            fans.Add("media-type");
            fans.Add("method");
            fans.Add("normalization-form");
            fans.Add("omit-xml-declaration");
            fans.Add("output-version");
            fans.Add("parameter-document");
            fans.Add("standalone");
            fans.Add("suppress-indentation");
            fans.Add("undeclare-prefixes");
            fans.Add(Saxon.Hej.lib.SaxonOutputKeys.ATTRIBUTE_ORDER);
            fans.Add(Saxon.Hej.lib.SaxonOutputKeys.CANONICAL);
            fans.Add(Saxon.Hej.lib.SaxonOutputKeys.CHARACTER_REPRESENTATION);
            fans.Add(Saxon.Hej.lib.SaxonOutputKeys.DOUBLE_SPACE);
            fans.Add(Saxon.Hej.lib.SaxonOutputKeys.INDENT_SPACES);
            fans.Add(Saxon.Hej.lib.SaxonOutputKeys.INTERNAL_DTD_SUBSET);
            fans.Add(Saxon.Hej.lib.SaxonOutputKeys.LINE_LENGTH);
            fans.Add(Saxon.Hej.lib.SaxonOutputKeys.NEWLINE);
            fans.Add(Saxon.Hej.lib.SaxonOutputKeys.NEXT_IN_CHAIN);
            fans.Add(Saxon.Hej.lib.SaxonOutputKeys.RECOGNIZE_BINARY);
            fans.Add(Saxon.Hej.lib.SaxonOutputKeys.REQUIRE_WELL_FORMED);
            fans.Add(Saxon.Hej.lib.SaxonOutputKeys.PROPERTY_ORDER);
            fans.Add(Saxon.Hej.lib.SaxonOutputKeys.SINGLE_QUOTES);
            fans.Add(Saxon.Hej.lib.SaxonOutputKeys.SUPPLY_SOURCE_LOCATOR);
        }
        public static readonly System.Collections.Generic.HashSet<string> fans = new System.Collections.Generic.HashSet<string>(40);
        private Saxon.Hej.expr.Expression href;
        private Saxon.Hej.om.StructuredQName formatQName;
        private Saxon.Hej.expr.Expression formatExpression;
        private int validationAction = Saxon.Hej.lib.Validation.STRIP;
        private Saxon.Hej.type.SchemaType schemaType = null;
        private readonly System.Collections.Generic.IDictionary<Saxon.Hej.om.StructuredQName,Saxon.Hej.expr.Expression> serializationAttributes = new System.Collections.Generic.Dictionary<Saxon.Hej.om.StructuredQName, Saxon.Hej.expr.Expression>(10);
        private bool async = true;
        public override bool isInstruction() {
            return true;
        }
        protected internal override bool mayContainSequenceConstructor() {
            return true;
        }
        protected internal override void prepareAttributes() {
            string formatAttribute = null;
            string hrefAttribute = null;
            string validationAtt = null;
            string typeAtt = null;
            string useCharacterMapsAtt = null;
            foreach (Saxon.Hej.om.AttributeInfo att in attributes()) {
                Saxon.Hej.om.NodeName attName = att.getNodeName();
                Saxon.Hej.om.StructuredQName name = attName.getStructuredQName();
                string value = att.getValue();
                string f = name.getClarkName();
                if (f.Equals("format")) {
                    formatAttribute = Saxon.Hej.value.Whitespace.trim(value);
                    formatExpression = makeAttributeValueTemplate(formatAttribute, att);
                } else {
                    if (f.Equals("href")) {
                        hrefAttribute = Saxon.Hej.value.Whitespace.trim(value);
                        href = makeAttributeValueTemplate(hrefAttribute, att);
                    } else {
                        if (f.Equals("validation")) {
                            validationAtt = Saxon.Hej.value.Whitespace.trim(value);
                        } else {
                            if (f.Equals("type")) {
                                typeAtt = Saxon.Hej.value.Whitespace.trim(value);
                            } else {
                                if (f.Equals("use-character-maps")) {
                                    useCharacterMapsAtt = Saxon.Hej.value.Whitespace.trim(value);
                                } else {
                                    if ((fans.Contains(f) || ((f.StartsWith("{") && !(Saxon.Hej.om.StandardNames.SAXON_ASYCHRONOUS.Equals(f)))))) {
                                        string val = value;
                                        if ((!(f.Equals(Saxon.Hej.lib.SaxonOutputKeys.ITEM_SEPARATOR)) && !(f.Equals(Saxon.Hej.lib.SaxonOutputKeys.NEWLINE)))) {
                                            val = Saxon.Hej.value.Whitespace.trim(value);
                                        }
                                        Saxon.Hej.expr.Expression exp = makeAttributeValueTemplate(val, att);
                                        Saxon.Impl.Helpers.MapUtils.Put(serializationAttributes, name, exp);
                                    } else {
                                        if ((name.getLocalPart().Equals("asynchronous") && name.hasURI(Saxon.Hej.om.NamespaceUri.SAXON))) {
                                            async = processBooleanAttribute("saxon:asynchronous", value);
                                            if (getCompilation().getCompilerInfo().isCompileWithTracing()) {
                                                async = false;
                                            } else {
                                                if (!("EE".Equals(getConfiguration().getEditionCode()))) {
                                                    issueWarning("saxon:asynchronous - ignored when not running Saxon-EE", Saxon.Hej.trans.SaxonErrorCode.SXWN9013);
                                                    async = false;
                                                }
                                            }
                                        } else {
                                            checkUnknownAttribute(attName);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            if ((formatAttribute != null)) {
                if ((formatExpression is Saxon.Hej.expr.StringLiteral)) {
                    formatQName = makeQName(((Saxon.Hej.expr.StringLiteral)(formatExpression)).stringify(), "XTDE1460", "format");
                    formatExpression = null;
                } else {
                    getPrincipalStylesheetModule().setNeedsDynamicOutputProperties(true);
                }
            }
            if ((validationAtt == null)) {
                validationAction = getDefaultValidation();
            } else {
                validationAction = validateValidationAttribute(validationAtt);
            }
            if ((typeAtt != null)) {
                if (!(isSchemaAware())) {
                    compileError("The @type attribute is available only with a schema-aware XSLT processor", "XTSE1660");
                }
                schemaType = getSchemaType(typeAtt);
                validationAction = Saxon.Hej.lib.Validation.BY_TYPE;
            }
            if (((typeAtt != null) && (validationAtt != null))) {
                compileError("The @validation and @type attributes are mutually exclusive", "XTSE1505");
            }
            if ((useCharacterMapsAtt != null)) {
                string s = Saxon.Hej.style.XSLOutput.prepareCharacterMaps(this, useCharacterMapsAtt, new Saxon.Ejava.util.Properties());
                Saxon.Impl.Helpers.MapUtils.Put(serializationAttributes, new Saxon.Hej.om.StructuredQName("", Saxon.Hej.om.NamespaceUri.NULL, "use-character-maps"), new Saxon.Hej.expr.StringLiteral(s));
            }
        }
        public override void validate(Saxon.Hej.style.ComponentDeclaration decl) {
            if (((href != null) && !(getConfiguration().getBooleanProperty(Saxon.Hej.lib.Feature<bool>.ALLOW_EXTERNAL_FUNCTIONS)))) {
                compileError("xsl:result-document is disabled when extension functions are disabled");
            }
            href = typeCheck("href", href);
            formatExpression = typeCheck("format", formatExpression);
            foreach (Saxon.Hej.om.StructuredQName prop in Saxon.Impl.Helpers.MapUtils.KeySet(serializationAttributes)) {
                Saxon.Hej.expr.Expression exp1 = Saxon.Impl.Helpers.MapUtils.Get(serializationAttributes, prop);
                Saxon.Hej.expr.Expression exp2 = typeCheck(prop.getDisplayName(), exp1);
                if ((exp1 != exp2)) {
                    Saxon.Impl.Helpers.MapUtils.Put(serializationAttributes, prop, exp2);
                }
            }
            getContainingPackage().setCreatesSecondaryResultDocuments(true);
        }
        public static Saxon.Hej.om.StructuredQName METHOD = new Saxon.Hej.om.StructuredQName("", Saxon.Hej.om.NamespaceUri.NULL, "method");
        public static Saxon.Hej.om.StructuredQName BUILD_TREE = new Saxon.Hej.om.StructuredQName("", Saxon.Hej.om.NamespaceUri.NULL, "build-tree");
        public override Saxon.Hej.expr.Expression compile(Saxon.Hej.style.Compilation exec, Saxon.Hej.style.ComponentDeclaration decl) {
            Saxon.Hej.tree.iter.AxisIterator ai = iterateAxis(Saxon.Hej.om.AxisInfo.ANCESTOR);
            Saxon.Hej.om.NodeInfo node;
            while (((node = ((/*1430*/Saxon.Hej.om.NodeInfo)ai.next())) != null)) {
                if (((node is Saxon.Hej.style.XSLGeneralVariable) || (node is Saxon.Hej.style.XSLFunction))) {
                    issueWarning((("An xsl:result-document instruction inside "+node.getDisplayName())+" will always fail at run-time"), "XTDE1480");
                    return new Saxon.Hej.expr.ErrorExpression("Call to xsl:result-document while in temporary output state", "XTDE1480", false);
                }
            }
            Saxon.Ejava.util.Properties globalProps;
            if ((formatExpression == null)) {
                try {
                    globalProps = getPrincipalStylesheetModule().gatherOutputProperties(formatQName);
                } catch (Saxon.Hej.trans.XPathException err) {
                    compileError("Named output format has not been defined", "XTDE1460");
                    return null;
                }
            } else {
                globalProps = new Saxon.Ejava.util.Properties();
                getPrincipalStylesheetModule().setNeedsDynamicOutputProperties(true);
            }
            string method = null;
            if ((((formatExpression == null) && (globalProps.getProperty("method") == null)) && (Saxon.Impl.Helpers.MapUtils.Get(serializationAttributes, METHOD) == null))) {
                Saxon.Hej.tree.iter.AxisIterator kids = iterateAxis(Saxon.Hej.om.AxisInfo.CHILD);
                Saxon.Hej.om.NodeInfo first = ((/*1430*/Saxon.Hej.om.NodeInfo)kids.next());
                if ((first is Saxon.Hej.style.LiteralResultElement)) {
                    if ((first.getNamespaceUri().Equals(Saxon.Hej.om.NamespaceUri.XHTML) && first.getLocalPart().Equals("html"))) {
                        method = "xhtml";
                    } else {
                        if ((Saxon.Impl.Helpers.StringUtils.stringEqualsIgnoreCase(first.getLocalPart(), "html") && first.getNamespaceUri().isEmpty())) {
                            method = "html";
                        } else {
                            method = "xml";
                        }
                    }
                    globalProps.setProperty("method", method);
                }
            }
            Saxon.Ejava.util.Properties localProps = new Saxon.Ejava.util.Properties();
            System.Collections.Generic.HashSet<Saxon.Hej.om.StructuredQName> @fixed = new System.Collections.Generic.HashSet<Saxon.Hej.om.StructuredQName>(10);
            Saxon.Hej.om.NamespaceResolver namespaceResolver = getStaticContext().getNamespaceResolver();
            foreach (Saxon.Hej.om.StructuredQName property in Saxon.Impl.Helpers.MapUtils.KeySet(serializationAttributes)) {
                Saxon.Hej.expr.Expression exp = Saxon.Impl.Helpers.MapUtils.Get(serializationAttributes, property);
                if ((exp is Saxon.Hej.expr.StringLiteral)) {
                    string s = ((Saxon.Hej.expr.StringLiteral)(exp)).stringify();
                    string lname = property.getLocalPart();
                    Saxon.Hej.om.NamespaceUri uri = property.getNamespaceUri();
                    try {
                        Saxon.Hej.expr.instruct.ResultDocument.setSerializationProperty(localProps, uri, lname, s, namespaceResolver, false, exec.getConfiguration());
                        @fixed.Add(property);
                        if (property.Equals(METHOD)) {
                            method = s;
                        }
                    } catch (Saxon.Hej.trans.XPathException e) {
                        if (Saxon.Hej.om.NamespaceUri.SAXON.Equals(e.getErrorCodeNamespace())) {
                            compileWarning(e.Message, e.getErrorCodeQName());
                        } else {
                            e.setErrorCode("XTSE0020");
                            compileError(e);
                        }
                    }
                }
            }
            foreach (Saxon.Hej.om.StructuredQName p in @fixed) {
                serializationAttributes.Remove(p);
            }
            Saxon.Hej.expr.instruct.ResultDocument inst = new Saxon.Hej.expr.instruct.ResultDocument(globalProps, localProps, href, formatExpression, validationAction, schemaType, serializationAttributes, getContainingPackage().getCharacterMapIndex());
            Saxon.Hej.expr.Expression content = compileSequenceConstructor(exec, decl, true);
            if ((content == null)) {
                content = Saxon.Hej.expr.Literal.makeLiteral(Saxon.Hej.value.EmptySequence.getInstance());
            }
            inst.setContentExpression(content);
            inst.setAsynchronous(async);
            return inst;
        }
    
    }
}