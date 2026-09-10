using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.style {
    public class XSLSourceDocument : Saxon.Hej.style.StyleElement {
        private Saxon.Hej.expr.Expression href = null;
        private System.Collections.Generic.ISet<Saxon.Hej.expr.accum.Accumulator> accumulators = new System.Collections.Generic.HashSet<Saxon.Hej.expr.accum.Accumulator>();
        private bool streaming = false;
        private Saxon.Hej.lib.ParseOptions parseOptions;
        public override bool isInstruction() {
            return true;
        }
        protected internal override bool mayContainSequenceConstructor() {
            return true;
        }
        protected internal override bool isWithinDeclaredStreamableConstruct() {
            return true;
        }
        protected internal override void prepareAttributes() {
            parseOptions = getConfiguration().getParseOptions();
            string hrefAtt = null;
            string validationAtt = null;
            string typeAtt = null;
            string useAccumulatorsAtt = null;
            foreach (Saxon.Hej.om.AttributeInfo att in attributes()) {
                Saxon.Hej.om.NodeName attName = att.getNodeName();
                Saxon.Hej.om.StructuredQName name = attName.getStructuredQName();
                string value = att.getValue();
                string f = name.getClarkName();
                if (f.Equals("href")) {
                    hrefAtt = value;
                    href = makeAttributeValueTemplate(hrefAtt, att);
                } else {
                    if (f.Equals("validation")) {
                        validationAtt = Saxon.Hej.value.Whitespace.trim(value);
                    } else {
                        if (f.Equals("type")) {
                            typeAtt = Saxon.Hej.value.Whitespace.trim(value);
                        } else {
                            if (f.Equals("use-accumulators")) {
                                useAccumulatorsAtt = Saxon.Hej.value.Whitespace.trim(value);
                            } else {
                                if (f.Equals("streamable")) {
                                    streaming = processStreamableAtt(value);
                                } else {
                                    if (attName.hasURI(Saxon.Hej.om.NamespaceUri.SAXON)) {
                                        isExtensionAttributeAllowed(attName.getDisplayName());
                                        string local = attName.getLocalPart();
                                        switch (local) {
                                          case "dtd-validation":
                                            parseOptions = parseOptions.withDTDValidationMode((processBooleanAttribute(f, value) ? Saxon.Hej.lib.Validation.STRICT : Saxon.Hej.lib.Validation.SKIP));
                                            break ;
                                          case "expand-attribute-defaults":
                                            parseOptions = parseOptions.withExpandAttributeDefaults(processBooleanAttribute(f, value));
                                            break ;
                                          case "line-numbering":
                                            parseOptions = parseOptions.withLineNumbering(processBooleanAttribute(f, value));
                                            break ;
                                          case "xinclude":
                                            parseOptions = parseOptions.withXIncludeAware(processBooleanAttribute(f, value));
                                            break ;
                                          case "validation-params":
                                            break ;
                                          case "strip-space":
                                            switch (Saxon.Hej.value.Whitespace.normalizeWhitespace(Saxon.Hej.str.StringView.of(value)).ToString()) {
                                              case "#all":
                                                parseOptions = parseOptions.withSpaceStrippingRule(Saxon.Hej.om.AllElementsSpaceStrippingRule.getInstance());
                                                break ;
                                              case "#none":
                                                parseOptions = parseOptions.withSpaceStrippingRule(Saxon.Hej.om.NoElementsSpaceStrippingRule.getInstance());
                                                break ;
                                              case "#ignorable":
                                                parseOptions = parseOptions.withSpaceStrippingRule(Saxon.Hej.om.IgnorableSpaceStrippingRule.getInstance());
                                                break ;
                                              case "#default":
                                                parseOptions = parseOptions.withSpaceStrippingRule(null);
                                                break ;
                                              default:
                                                invalidAttribute("saxon:strip-space", "#all|#none|#ignorable|#default");
                                                break ;
                                            }
                                            break ;
                                          default:
                                            checkUnknownAttribute(attName);
                                            break ;
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
            if ((hrefAtt == null)) {
                reportAbsence("href");
            }
            if ((validationAtt != null)) {
                int validation = validateValidationAttribute(validationAtt);
                parseOptions = parseOptions.withSchemaValidationMode(validation);
            }
            if ((typeAtt != null)) {
                if (!(isSchemaAware())) {
                    compileError("The @type attribute is available only with a schema-aware XSLT processor", "XTSE1660");
                }
                parseOptions = parseOptions.withSchemaValidationMode(Saxon.Hej.lib.Validation.BY_TYPE);
                parseOptions = parseOptions.withTopLevelType(getSchemaType(typeAtt));
            }
            if (((typeAtt != null) && (validationAtt != null))) {
                compileError("The @validation and @type attributes are mutually exclusive", "XTSE1505");
            }
            if ((useAccumulatorsAtt == null)) {
                useAccumulatorsAtt = "";
            }
            Saxon.Hej.expr.accum.AccumulatorRegistry registry = getPrincipalStylesheetModule().getStylesheetPackage().getAccumulatorRegistry();
            accumulators = registry.getUsedAccumulators(useAccumulatorsAtt, this);
        }
        public override void validate(Saxon.Hej.style.ComponentDeclaration decl) {
            href = typeCheck("select", href);
            if (!(hasChildNodes())) {
                issueWarning("An empty xsl:source-document instruction has no effect", Saxon.Hej.trans.SaxonErrorCode.SXWN9009);
            }
        }
        public override Saxon.Hej.expr.Expression compile(Saxon.Hej.style.Compilation exec, Saxon.Hej.style.ComponentDeclaration decl) {
            Saxon.Hej.Configuration config = getConfiguration();
            if ((parseOptions.getSpaceStrippingRule() == null)) {
                parseOptions = parseOptions.withSpaceStrippingRule(getPackageData().getSpaceStrippingRule());
            }
            parseOptions = parseOptions.withApplicableAccumulators(accumulators);
            Saxon.Hej.expr.Expression action = compileSequenceConstructor(exec, decl, false);
            if ((action == null)) {
                return Saxon.Hej.expr.Literal.makeEmptySequence();
            }
            try {
                Saxon.Hej.expr.parser.ExpressionVisitor visitor = makeExpressionVisitor();
                action = action.simplify();
                action = action.typeCheck(visitor, config.makeContextItemStaticInfo(Saxon.Hej.pattern.NodeKindTest.DOCUMENT, false));
                return config.makeStreamInstruction(href, action, streaming, parseOptions, null, allocateLocation(), makeRetainedStaticContext());
            } catch (Saxon.Hej.trans.XPathException err) {
                compileError(err);
                return null;
            }
        }
    
    }
}