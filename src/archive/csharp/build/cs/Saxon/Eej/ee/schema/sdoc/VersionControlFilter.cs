using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.ee.schema.sdoc {
    public class VersionControlFilter : Saxon.Hej.@event.ProxyReceiver {
        private readonly Saxon.Eej.ee.schema.SchemaCompiler compiler;
        private int depthOfHole = 0;
        private bool emptySchemaElement = false;
        private readonly Saxon.Hej.value.BigDecimalValue thisVersion;
        private readonly bool isXSD11;
        private bool foundErrors;
        public VersionControlFilter(Saxon.Eej.ee.schema.SchemaCompiler compiler, Saxon.Hej.@event.Receiver next) : base (next) {
            this.compiler = compiler;
            int xsdVersion = compiler.getLanguageVersion();
            thisVersion = new Saxon.Hej.value.BigDecimalValue(Singulink.Numerics.BigDecimal.Parse(((xsdVersion == Saxon.Hej.Configuration.XSD10) ? "1.0" : "1.1"), System.Globalization.NumberStyles.Number, System.Globalization.CultureInfo.InvariantCulture));
            isXSD11 = (xsdVersion == Saxon.Hej.Configuration.XSD11);
        }
        public override void open() {
            nextReceiver.open();
        }
        public override void startElement(Saxon.Hej.om.NodeName elemName, Saxon.Hej.type.SchemaType type, Saxon.Hej.om.AttributeMap attributes, Saxon.Hej.om.NamespaceMap namespaces, Saxon.Hej.s9api.Location location, int properties) {
            if (emptySchemaElement) {
                depthOfHole++;
                return ;
            }
            if ((depthOfHole == 0)) {
                bool ignore = false;
                bool doneAttributes = false;
                foreach (Saxon.Hej.om.AttributeInfo att in attributes) {
                    Saxon.Hej.om.NodeName attName = att.getNodeName();
                    if (attName.hasURI(Saxon.Hej.om.NamespaceUri.SCHEMA_VERSIONING)) {
                        string localName = attName.getLocalPart();
                        try {
                            switch (localName) {
                              case "minVersion":
                                {
                                    Saxon.Hej.value.BigDecimalValue dv = parseDecimalValue(att.getValue());
                                    if ((thisVersion.compareTo(dv)<0)) {
                                        ignore = true;
                                        doneAttributes = true;
                                    }
                                    break ;
                                }
                              case "maxVersion":
                                {
                                    Saxon.Hej.value.BigDecimalValue dv = parseDecimalValue(att.getValue());
                                    if ((thisVersion.compareTo(dv)>=0)) {
                                        ignore = true;
                                        doneAttributes = true;
                                    }
                                    break ;
                                }
                              case "typeAvailable":
                                System.Collections.Generic.ISet<Saxon.Hej.om.StructuredQName> typesAvailable = parseListOfQNames(att.getValue(), namespaces);
                                foreach (Saxon.Hej.om.StructuredQName name in typesAvailable) {
                                    if (!(isAutomaticallyKnownTypeName(name))) {
                                        ignore = true;
                                        doneAttributes = true;
                                        break ;
                                    }
                                }
                                break ;
                              case "typeUnavailable":
                                System.Collections.Generic.ISet<Saxon.Hej.om.StructuredQName> typesUnavailable = parseListOfQNames(att.getValue(), namespaces);
                                ignore = true;
                                foreach (Saxon.Hej.om.StructuredQName name in typesUnavailable) {
                                    if (!(isAutomaticallyKnownTypeName(name))) {
                                        ignore = false;
                                        doneAttributes = true;
                                        break ;
                                    }
                                }
                                break ;
                              case "facetAvailable":
                                System.Collections.Generic.ISet<Saxon.Hej.om.StructuredQName> facetsAvailable = parseListOfQNames(att.getValue(), namespaces);
                                foreach (Saxon.Hej.om.StructuredQName name in facetsAvailable) {
                                    if (!(isKnownFacetName(name))) {
                                        ignore = true;
                                        doneAttributes = true;
                                        break ;
                                    }
                                }
                                break ;
                              case "facetUnavailable":
                                System.Collections.Generic.ISet<Saxon.Hej.om.StructuredQName> facetsUnavailable = parseListOfQNames(att.getValue(), namespaces);
                                ignore = true;
                                foreach (Saxon.Hej.om.StructuredQName name in facetsUnavailable) {
                                    if (!(isKnownFacetName(name))) {
                                        ignore = false;
                                        doneAttributes = true;
                                        break ;
                                    }
                                }
                                break ;
                              default:
                                compiler.warning(("Unrecognized attribute name "+attName), Saxon.Hej.trans.SaxonErrorCode.SXSD1014, location);
                                break ;
                            }
                            if (doneAttributes) {
                                break ;
                            }
                        } catch (Saxon.Hej.trans.XPathException e) {
                            compiler.error(((("Invalid value for attribute "+attName)+": ")+e.Message), location);
                            foundErrors = true;
                        }
                    }
                }
                if (ignore) {
                    if (elemName.Equals(XS_SCHEMA)) {
                        emptySchemaElement = true;
                    } else {
                        depthOfHole = 1;
                        return ;
                    }
                }
                nextReceiver.startElement(elemName, type, attributes, namespaces, location, properties);
            } else {
                depthOfHole++;
            }
        }
        private static readonly Saxon.Hej.om.FingerprintedQName XS_SCHEMA = new Saxon.Hej.om.FingerprintedQName("xs", Saxon.Hej.om.NamespaceUri.SCHEMA, "schema", (Saxon.Hej.om.StandardNames.XS_SCHEMA&Saxon.Hej.om.NamePool.FP_MASK));
        public override void endElement() {
            if ((depthOfHole>0)) {
                depthOfHole--;
            } else {
                nextReceiver.endElement();
            }
        }
        public override void characters(Saxon.Hej.str.UnicodeString chars, Saxon.Hej.s9api.Location locationId, int properties) {
            if ((depthOfHole == 0)) {
                nextReceiver.characters(chars, locationId, properties);
            }
        }
        public override void processingInstruction(string target, Saxon.Hej.str.UnicodeString data, Saxon.Hej.s9api.Location locationId, int properties) {
        }
        public override void comment(Saxon.Hej.str.UnicodeString chars, Saxon.Hej.s9api.Location locationId, int properties) {
        }
        public override void endDocument() {
            base.endDocument();
            if (foundErrors) {
                throw new Saxon.Hej.trans.XPathException("One or more errors were found in the content of schema versioning attributes");
            }
        }
        private Saxon.Hej.value.BigDecimalValue parseDecimalValue(string value) {
            return (Saxon.Hej.value.BigDecimalValue)(Saxon.Hej.value.BigDecimalValue.makeDecimalValue(value, true).asAtomic());
        }
        private System.Collections.Generic.ISet<Saxon.Hej.om.StructuredQName> parseListOfQNames(string value, Saxon.Hej.om.NamespaceResolver resolver) {
            System.Collections.Generic.ISet<Saxon.Hej.om.StructuredQName> result = new System.Collections.Generic.HashSet<Saxon.Hej.om.StructuredQName>(4);
            Saxon.Hej.value.Whitespace.Tokenizer tokenizer = new Saxon.Hej.value.Whitespace.Tokenizer(Saxon.Hej.str.StringView.tidy(value));
            Saxon.Hej.value.StringValue tok;
            while (((tok = ((/*1430*/Saxon.Hej.value.StringValue)tokenizer.next())) != null)) {
                Saxon.Hej.om.StructuredQName val = Saxon.Hej.om.StructuredQName.fromLexicalQName(tok.getStringValue(), true, false, resolver);
                result.Add(val);
            }
            return result;
        }
        private bool isKnownFacetName(Saxon.Hej.om.StructuredQName name) {
            string localName = name.getLocalPart();
            return (((name.hasURI(Saxon.Hej.om.NamespaceUri.SCHEMA) && /*134*/Saxon.Eej.ee.schema.Facet.isFacetName(localName, (isXSD11 ? Saxon.Hej.Configuration.XSD11 : Saxon.Hej.Configuration.XSD10)))) || ((name.hasURI(Saxon.Hej.om.NamespaceUri.SAXON) && localName.Equals("preprocess"))));
        }
        private bool isAutomaticallyKnownTypeName(Saxon.Hej.om.StructuredQName name) {
            int fp = Saxon.Hej.om.StandardNames.getFingerprint(name.getNamespaceUri(), name.getLocalPart());
            return (((fp != -1) && (Saxon.Hej.type.BuiltInType.getSchemaType(fp) != null)) && ((isXSD11 || (fp != Saxon.Hej.om.StandardNames.XS_ERROR))));
        }
    
    }
}