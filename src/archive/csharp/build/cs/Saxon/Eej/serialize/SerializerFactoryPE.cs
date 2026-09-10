using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.serialize {
    public class SerializerFactoryPE : Saxon.Hej.lib.SerializerFactory {
        public SerializerFactoryPE(Saxon.Eej.config.ProfessionalConfiguration config) : base (config) {
        }
        protected internal override Saxon.Hej.@event.SequenceReceiver createSaxonSerializationMethod(string method, Saxon.Hej.serialize.SerializationProperties serProps, Saxon.Hej.@event.PipelineConfiguration pipe, Saxon.Hej.serialize.CharacterMapExpander characterMapExpander, Saxon.Hej.@event.ProxyReceiver normalizer, Saxon.Hej.serialize.ExpandedStreamResult expandedResult, Saxon.Ejavax.xml.transform.Result result) {
            Saxon.Ejava.util.Properties props = serProps.getProperties();
            pipe.getConfiguration().checkLicensedFeature(Saxon.Hej.Configuration.LicenseFeature.PROFESSIONAL_EDITION, "custom serialization", -1);
            if (Saxon.Hej.lib.SaxonOutputKeys.SAXON_XQUERY_METHOD.Equals(method)) {
                Saxon.Hej.serialize.XMLEmitter emitter = new Saxon.Eej.serialize.XQueryEmitter();
                emitter.setPipelineConfiguration(pipe);
                if ((result is Saxon.Hej.serialize.UnicodeWriterResult)) {
                    emitter.setUnicodeWriter(((Saxon.Hej.serialize.UnicodeWriterResult)(result)).getUnicodeWriter());
                } else {
                    emitter.setUnicodeWriter(expandedResult.obtainUnicodeWriter());
                }
                props.setProperty(Saxon.Ejavax.xml.transform.OutputKeys.OMIT_XML_DECLARATION, "yes");
                emitter.setOutputProperties(props);
                return createXMLSerializer(emitter, serProps);
            } else {
                if (Saxon.Hej.lib.SaxonOutputKeys.SAXON_BASE64_BINARY_METHOD.Equals(method)) {
                    Saxon.Eej.serialize.BinaryEmitter emitter = new Saxon.Eej.serialize.Base64BinaryEmitter((Saxon.Ejavax.xml.transform.stream.StreamResult)(result), pipe);
                    props.setProperty(Saxon.Ejavax.xml.transform.OutputKeys.OMIT_XML_DECLARATION, "yes");
                    emitter.setOutputProperties(props);
                    return emitter;
                } else {
                    if (Saxon.Hej.lib.SaxonOutputKeys.SAXON_HEX_BINARY_METHOD.Equals(method)) {
                        Saxon.Hej.serialize.Emitter emitter = new Saxon.Eej.serialize.HexBinaryEmitter((Saxon.Ejavax.xml.transform.stream.StreamResult)(result), pipe);
                        emitter.setPipelineConfiguration(pipe);
                        props.setProperty(Saxon.Ejavax.xml.transform.OutputKeys.OMIT_XML_DECLARATION, "yes");
                        emitter.setOutputProperties(props);
                        return emitter;
                    } else {
                        if (Saxon.Hej.lib.SaxonOutputKeys.SAXON_XML_TO_JSON_METHOD.Equals(method)) {
                            Saxon.Hej.serialize.Emitter emitter = new Saxon.Hej.serialize.TEXTEmitter();
                            emitter.setPipelineConfiguration(pipe);
                            if ((result is Saxon.Hej.serialize.UnicodeWriterResult)) {
                                emitter.setUnicodeWriter(((Saxon.Hej.serialize.UnicodeWriterResult)(result)).getUnicodeWriter());
                            } else {
                                emitter.setUnicodeWriter(expandedResult.obtainUnicodeWriter());
                            }
                            props.setProperty(Saxon.Ejavax.xml.transform.OutputKeys.OMIT_XML_DECLARATION, "yes");
                            emitter.setOutputProperties(props);
                            return emitter;
                        } else {
                            Saxon.Hej.trans.XPathException err = new Saxon.Hej.trans.XPathException(("Unknown serialization method "+method));
                            err.setErrorCode(Saxon.Hej.trans.SaxonErrorCode.SXCH0004);
                            throw err;
                        }
                    }
                }
            }
        }
        protected internal override Saxon.Hej.@event.ProxyReceiver newHTMLIndenter(Saxon.Hej.@event.Receiver next, Saxon.Ejava.util.Properties outputProperties) {
            Saxon.Eej.serialize.HTMLIndenterPE r = new Saxon.Eej.serialize.HTMLIndenterPE(next, "html");
            r.setOutputProperties(outputProperties);
            return r;
        }
        protected internal override Saxon.Hej.@event.ProxyReceiver newXHTMLIndenter(Saxon.Hej.@event.Receiver next, Saxon.Ejava.util.Properties outputProperties) {
            string method = "xhtml";
            string htmlVersion = outputProperties.getProperty("html-version");
            if (((htmlVersion != null) && htmlVersion.StartsWith("5"))) {
                method = "xhtml5";
            }
            Saxon.Eej.serialize.HTMLIndenterPE r = new Saxon.Eej.serialize.HTMLIndenterPE(next, method);
            r.setOutputProperties(outputProperties);
            return r;
        }
        protected internal override Saxon.Hej.@event.ProxyReceiver newXMLIndenter(Saxon.Hej.serialize.XMLEmitter next, Saxon.Ejava.util.Properties outputProperties) {
            if ("yes".Equals(outputProperties.getProperty(Saxon.Hej.lib.SaxonOutputKeys.CANONICAL))) {
                return new Saxon.Eej.serialize.OuterNewlineInserter(next);
            } else {
                Saxon.Eej.serialize.XMLIndenterPE r = new Saxon.Eej.serialize.XMLIndenterPE(next);
                r.setOutputProperties(outputProperties);
                return r;
            }
        }
        protected internal override Saxon.Hej.serialize.Emitter newXMLEmitter(Saxon.Ejava.util.Properties properties) {
            Saxon.Hej.serialize.XMLEmitter emitter = new Saxon.Hej.serialize.XMLEmitter();
            string rep = properties.getProperty(Saxon.Hej.lib.SaxonOutputKeys.CHARACTER_REPRESENTATION);
            rep = Saxon.Hej.value.Whitespace.trim(rep);
            if ("decimal".Equals(rep)) {
                emitter.setCharacterReferenceGenerator(Saxon.Eej.serialize.DecimalCharacterReferenceGenerator.THE_INSTANCE);
            }
            return emitter;
        }
        protected internal override Saxon.Hej.serialize.Emitter newHTMLEmitter(Saxon.Ejava.util.Properties properties) {
            Saxon.Hej.serialize.HTMLEmitter emitter;
            if (Saxon.Hej.lib.SaxonOutputKeys.isHtmlVersion5(properties)) {
                emitter = new Saxon.Hej.serialize.HTML50Emitter();
            } else {
                emitter = new Saxon.Hej.serialize.HTML40Emitter();
            }
            setCharacterRepresention(emitter, properties);
            return emitter;
        }
        protected internal override Saxon.Hej.@event.ProxyReceiver newAttributeSorter(Saxon.Hej.@event.Receiver next, Saxon.Ejava.util.Properties outputProperties) {
            Saxon.Eej.serialize.AttributeSorter r = new Saxon.Eej.serialize.AttributeSorter(next);
            r.setOutputProperties(outputProperties);
            return r;
        }
        protected internal override Saxon.Hej.@event.Receiver newNamespaceSorter(Saxon.Hej.@event.Receiver next, Saxon.Ejava.util.Properties outputProperties) {
            return next;
        }
        private void setCharacterRepresention(Saxon.Hej.serialize.XMLEmitter emitter, Saxon.Ejava.util.Properties properties) {
            string rep = properties.getProperty(Saxon.Hej.lib.SaxonOutputKeys.CHARACTER_REPRESENTATION);
            if ((rep != null)) {
                string nonASCIIrep;
                string excludedRep;
                int semi = rep.IndexOf(';');
                if ((semi<0)) {
                    nonASCIIrep = Saxon.Hej.value.Whitespace.trim(rep);
                    excludedRep = nonASCIIrep;
                } else {
                    nonASCIIrep = Saxon.Hej.value.Whitespace.trim(rep.Substring(0, semi - 0));
                    excludedRep = Saxon.Hej.value.Whitespace.trim(rep.Substring((semi+1)));
                }
                if ("native".Equals(excludedRep)) {
                    excludedRep = "entity";
                }
                if (!("native".Equals(nonASCIIrep))) {
                    emitter.setEscapeNonAscii(true);
                    if ("entity".Equals(nonASCIIrep)) {
                        if ("decimal".Equals(excludedRep)) {
                            emitter.setCharacterReferenceGenerator(Saxon.Eej.serialize.HTMLEntityReferenceGenerator.DECIMAL);
                        } else {
                            emitter.setCharacterReferenceGenerator(Saxon.Eej.serialize.HTMLEntityReferenceGenerator.HEX);
                        }
                    } else {
                        if ("decimal".Equals(nonASCIIrep)) {
                            emitter.setCharacterReferenceGenerator(Saxon.Eej.serialize.DecimalCharacterReferenceGenerator.THE_INSTANCE);
                        } else {
                            emitter.setCharacterReferenceGenerator(Saxon.Hej.serialize.HexCharacterReferenceGenerator.THE_INSTANCE);
                        }
                    }
                }
            }
        }
        protected internal override Saxon.Hej.serialize.Emitter newXHTMLEmitter(Saxon.Ejava.util.Properties properties) {
            bool is5 = Saxon.Hej.lib.SaxonOutputKeys.isXhtmlHtmlVersion5(properties);
            Saxon.Hej.serialize.XMLEmitter emitter = (is5 ? new Saxon.Hej.serialize.XHTML5Emitter() : new Saxon.Hej.serialize.XHTML1Emitter());
            setCharacterRepresention(emitter, properties);
            return emitter;
        }
        public override Saxon.Hej.@event.Receiver addTextOutputFilter(Saxon.Hej.@event.Receiver next, Saxon.Ejava.util.Properties properties) {
            bool recognize = "yes".Equals(properties.getProperty(Saxon.Hej.lib.SaxonOutputKeys.RECOGNIZE_BINARY));
            if (recognize) {
                return new Saxon.Hej.serialize.BinaryTextDecoder(next, properties);
            } else {
                return next;
            }
        }
        public override Saxon.Hej.@event.SequenceReceiver prepareNextStylesheet(Saxon.Hej.@event.PipelineConfiguration pipe, string href, string baseURI, Saxon.Ejavax.xml.transform.Result result) {
            Saxon.Hej.Configuration config = pipe.getConfiguration();
            config.checkLicensedFeature(Saxon.Hej.Configuration.LicenseFeature.PROFESSIONAL_EDITION, "saxon:next-in-chain", -1);
            Saxon.Hej.PreparedStylesheet thisStylesheet = (Saxon.Hej.PreparedStylesheet)(pipe.getController().getExecutable());
            Saxon.Hej.PreparedStylesheet next = thisStylesheet.getCachedStylesheet(href, baseURI);
            if ((next == null)) {
                Saxon.Ejavax.xml.transform.Source source = null;
                try {
                    if ((pipe.getURIResolver() != null)) {
                        source = pipe.getURIResolver().resolve(href, baseURI);
                    }
                    if ((source == null)) {
                        source = pipe.getController().getStandardURIResolver().resolve(href, baseURI);
                    }
                } catch (Saxon.Ejavax.xml.transform.TransformerException e) {
                    throw Saxon.Hej.trans.XPathException.makeXPathException(e);
                }
                Saxon.Hej.trans.CompilerInfo compilerInfo = new Saxon.Hej.trans.CompilerInfo(config.getDefaultXsltCompilerInfo());
                compilerInfo.setGenerateByteCode(config.isGenerateByteCode(Saxon.Api.HostLanguage.XSLT));
                compilerInfo.setTargetEdition(config.getEditionCode());
                compilerInfo.setJustInTimeCompilation(config.isJITEnabled());
                next = Saxon.Hej.style.Compilation.compileSingletonPackage(config, compilerInfo, source);
                thisStylesheet.putCachedStylesheet(href, baseURI, next);
            }
            Saxon.Hej.@event.Receiver destination = next.getConfiguration().getSerializerFactory().getReceiver(result, next.getDeclaredSerializationProperties(), pipe);
            Saxon.Hej.trans.XsltController nextController = next.newController();
            Saxon.Hej.@event.TransformerReceiver nextTransformer = new Saxon.Hej.@event.TransformerReceiver(nextController);
            nextController.initializeController(new Saxon.Hej.expr.instruct.GlobalParameterSet());
            nextTransformer.setSystemId(pipe.getController().getBaseOutputURI());
            nextTransformer.setPipelineConfiguration(new Saxon.Hej.@event.PipelineConfiguration(pipe));
            nextTransformer.setDestination(destination);
            return new Saxon.Hej.@event.SequenceNormalizerWithSpaceSeparator(nextTransformer);
        }
        public override Saxon.Hej.query.SequenceWrapper newSequenceWrapper(Saxon.Hej.@event.Receiver destination) {
            return new Saxon.Eej.serialize.SequenceWrapperPE(destination);
        }
        public override string checkOutputProperty(string key, string value) {
            if (key.StartsWith("{http://saxon.sf.net/}")) {
                switch (key) {
                  case Saxon.Hej.lib.SaxonOutputKeys.STYLESHEET_VERSION:
                    break ;
                  case Saxon.Hej.lib.SaxonOutputKeys.PARAMETER_DOCUMENT_BASE_URI:
                    break ;
                  case Saxon.Hej.lib.SaxonOutputKeys.SUPPLY_SOURCE_LOCATOR:
                  case Saxon.Hej.lib.SaxonOutputKeys.UNFAILING:
                    if ((value != null)) {
                        value = checkYesOrNo(key, value);
                    }
                    break ;
                  case Saxon.Hej.lib.SaxonOutputKeys.NEWLINE:
                  case Saxon.Hej.lib.SaxonOutputKeys.NEXT_IN_CHAIN:
                  case Saxon.Hej.lib.SaxonOutputKeys.CHARACTER_REPRESENTATION:
                  case Saxon.Hej.lib.SaxonOutputKeys.NEXT_IN_CHAIN_BASE_URI:
                  case Saxon.Hej.lib.SaxonOutputKeys.PROPERTY_ORDER:
                  case Saxon.Hej.lib.SaxonOutputKeys.INTERNAL_DTD_SUBSET:
                    if ((value != null)) {
                        checkExtensions(key);
                    }
                    break ;
                  case Saxon.Hej.lib.SaxonOutputKeys.ATTRIBUTE_ORDER:
                    if ((value != null)) {
                        checkExtensions(key);
                        value = checkListOfEQNamesAllowingStar(key, value);
                    }
                    break ;
                  case Saxon.Hej.lib.SaxonOutputKeys.CANONICAL:
                  case Saxon.Hej.lib.SaxonOutputKeys.REQUIRE_WELL_FORMED:
                  case Saxon.Hej.lib.SaxonOutputKeys.RECOGNIZE_BINARY:
                  case Saxon.Hej.lib.SaxonOutputKeys.WRAP:
                  case Saxon.Hej.lib.SaxonOutputKeys.SINGLE_QUOTES:
                    if ((value != null)) {
                        checkExtensions(key);
                        value = checkYesOrNo(key, value);
                    }
                    break ;
                  case Saxon.Hej.lib.SaxonOutputKeys.INDENT_SPACES:
                  case Saxon.Hej.lib.SaxonOutputKeys.LINE_LENGTH:
                    if ((value != null)) {
                        checkExtensions(key);
                        checkNonNegativeInteger(key, value);
                    }
                    break ;
                  case Saxon.Hej.lib.SaxonOutputKeys.DOUBLE_SPACE:
                    if ((value != null)) {
                        checkExtensions(key);
                        checkListOfEQNames(key, value);
                    }
                    break ;
                  default:
                    throw new Saxon.Hej.trans.XPathException(("Unknown serialization parameter "+Saxon.Hej.trans.Err.wrap(key)), "XQST0109");
                }
            } else {
                value = base.checkOutputProperty(key, value);
            }
            return value;
        }
        protected internal override void checkExtensions(string key) {
            getConfiguration().checkLicensedFeature(Saxon.Hej.Configuration.LicenseFeature.PROFESSIONAL_EDITION, ("custom serialization "+key), -1);
        }
        protected internal override System.Collections.Generic.Comparer<Saxon.Hej.value.AtomicValue> getPropertySorter(string propertyOrder) {
            int MIDDLE_GROUND = 10000000;
            System.Collections.Generic.IDictionary<string,int> knownKeys = new System.Collections.Generic.Dictionary<string, int>();
            int pos = 0;
            Saxon.Ejava.util.StringTokenizer st2 = new Saxon.Ejava.util.StringTokenizer(propertyOrder, " \t\n\r", false);
            while (st2.hasMoreTokens()) {
                string key = st2.nextToken();
                if (key.Equals("*")) {
                    pos += MIDDLE_GROUND;
                } else {
                    Saxon.Impl.Helpers.MapUtils.Put(knownKeys, key, pos++);
                }
            }
            return new Saxon.Eej.serialize.SerializerFactoryPE.PropertyComparator(knownKeys);
        }
        private class PropertyComparator : System.Collections.Generic.Comparer<Saxon.Hej.value.AtomicValue> {
            public const int MIDDLE_GROUND = 10000000;
            private readonly System.Collections.Generic.IDictionary<string,int> knownKeys;
            public PropertyComparator(System.Collections.Generic.IDictionary<string,int> knownKeys)  {
                this.knownKeys = knownKeys;
            }
            public int compare(Saxon.Hej.value.AtomicValue n0, Saxon.Hej.value.AtomicValue n1) {
                int r0 = Saxon.Impl.Helpers.MapUtils.GetOrDefault(knownKeys, n0.getStringValue(), -1);
                int r1 = Saxon.Impl.Helpers.MapUtils.GetOrDefault(knownKeys, n1.getStringValue(), -1);
                if ((r0 != -1)) {
                    if ((r1 == -1)) {
                        return r0.CompareTo(MIDDLE_GROUND);
                    } else {
                        return r0.CompareTo(r1);
                    }
                } else {
                    if ((r1 != -1)) {
                        return MIDDLE_GROUND.CompareTo(r1);
                    } else {
                        return System.String.Compare(n0.getStringValue(), n1.getStringValue(), System.StringComparison.Ordinal);
                    }
                }
            }
            // Extra method to make the class a C# Comparer
            public override int Compare(Saxon.Hej.value.AtomicValue a, Saxon.Hej.value.AtomicValue b) {
                return compare(a, b);
            }
        
        }
    
    }
}