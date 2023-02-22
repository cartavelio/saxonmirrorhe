using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.lib {
    public class SerializerFactory {
        internal Saxon.Hej.Configuration config;
        internal Saxon.Hej.@event.PipelineConfiguration pipe;
        public SerializerFactory(Saxon.Hej.Configuration config)  {
            this.config = config;
        }
        public SerializerFactory(Saxon.Hej.@event.PipelineConfiguration pipe)  {
            this.pipe = pipe;
            this.config = pipe.getConfiguration();
        }
        public Saxon.Hej.Configuration getConfiguration() {
            return config;
        }
        public Saxon.Hej.@event.Receiver getReceiver(Saxon.Ejavax.xml.transform.Result result, Saxon.Hej.@event.PipelineConfiguration pipe, Saxon.Ejava.util.Properties props) {
            return getReceiver(result, new Saxon.Hej.serialize.SerializationProperties(props), pipe);
        }
        public Saxon.Hej.@event.Receiver getReceiver(Saxon.Ejavax.xml.transform.Result result) {
            return getReceiver(result, new Saxon.Hej.serialize.SerializationProperties(), config.makePipelineConfiguration());
        }
        public Saxon.Hej.@event.Receiver getReceiver(Saxon.Ejavax.xml.transform.Result result, Saxon.Hej.serialize.SerializationProperties @params) {
            return getReceiver(result, @params, config.makePipelineConfiguration());
        }
        public virtual  /* REPLACING public */ Saxon.Hej.@event.Receiver getReceiver(Saxon.Ejavax.xml.transform.Result result, Saxon.Hej.serialize.SerializationProperties @params, Saxon.Hej.@event.PipelineConfiguration pipe) {
            Saxon.Ejava.util.Objects.requireNonNull(result);
            Saxon.Ejava.util.Objects.requireNonNull(@params);
            Saxon.Ejava.util.Objects.requireNonNull(pipe);
            Saxon.Ejava.util.Properties props = @params.getProperties();
            Saxon.Hej.serialize.CharacterMapIndex charMapIndex = @params.getCharacterMapIndex();
            if ((charMapIndex == null)) {
                charMapIndex = new Saxon.Hej.serialize.CharacterMapIndex();
            }
            string nextInChain = props.getProperty(Saxon.Hej.lib.SaxonOutputKeys.NEXT_IN_CHAIN);
            if (((nextInChain != null) && !(nextInChain.Length==0))) {
                string href = props.getProperty(Saxon.Hej.lib.SaxonOutputKeys.NEXT_IN_CHAIN);
                string @base = props.getProperty(Saxon.Hej.lib.SaxonOutputKeys.NEXT_IN_CHAIN_BASE_URI);
                if ((@base == null)) {
                    @base = "";
                }
                Saxon.Ejava.util.Properties sansNext = new Saxon.Ejava.util.Properties(props);
                sansNext.setProperty(Saxon.Hej.lib.SaxonOutputKeys.NEXT_IN_CHAIN, "");
                return prepareNextStylesheet(pipe, href, @base, result);
            }
            string paramDoc = props.getProperty(Saxon.Hej.lib.SaxonOutputKeys.PARAMETER_DOCUMENT);
            if (((paramDoc != null) && !(paramDoc.Length==0))) {
                string @base = props.getProperty(Saxon.Hej.lib.SaxonOutputKeys.PARAMETER_DOCUMENT_BASE_URI);
                if ((@base == null)) {
                    @base = result.getSystemId();
                }
                Saxon.Ejava.util.Properties props2 = new Saxon.Ejava.util.Properties(props);
                props2.setProperty(Saxon.Hej.lib.SaxonOutputKeys.PARAMETER_DOCUMENT, "");
                Saxon.Hej.lib.JJ_ResourceRequest rr = new Saxon.Hej.lib.JJ_ResourceRequest();
                rr.relativeUri = paramDoc;
                rr.baseUri = @base;
                try {
                    rr.uri = Saxon.Hej.functions.ResolveURI.makeAbsolute(paramDoc, @base).ToString();
                } catch (System.UriFormatException err) {
                    throw Saxon.Hej.trans.XPathException.makeXPathException(err);
                }
                rr.nature = Saxon.Hej.lib.NamespaceConstant.OUTPUT;
                rr.purpose = "serialization";
                Saxon.Ejavax.xml.transform.Source source = rr.resolve(config.getResourceResolver(), new Saxon.Callbacks.DirectResourceResolver(config));
                Saxon.Hej.lib.ParseOptions options = new Saxon.Hej.lib.ParseOptions();
                options.setSchemaValidationMode(Saxon.Hej.lib.Validation.LAX);
                options.setDTDValidationMode(Saxon.Hej.lib.Validation.SKIP);
                Saxon.Hej.om.TreeInfo doc = config.buildDocumentTree(source);
                Saxon.Hej.serialize.SerializationParamsHandler ph = new Saxon.Hej.serialize.SerializationParamsHandler();
                ph.setSerializationParams(doc.getRootNode());
                Saxon.Ejava.util.Properties paramDocProps = ph.getSerializationProperties().getProperties();
                foreach (string name in paramDocProps.stringPropertyNames()) {
                    string value = paramDocProps.getProperty(name);
                    props2.setProperty(name, value);
                }
                Saxon.Hej.serialize.CharacterMap charMap = ph.getCharacterMap();
                if ((charMap != null)) {
                    props2.setProperty(Saxon.Hej.lib.SaxonOutputKeys.USE_CHARACTER_MAPS, charMap.getName().getClarkName());
                    charMapIndex.putCharacterMap(charMap.getName(), charMap);
                }
                props = props2;
                @params = new Saxon.Hej.serialize.SerializationProperties(props2, charMapIndex);
            }
            Saxon.Hej.str.UnicodeWriter uWriter = null;
            Saxon.Hej.serialize.ExpandedStreamResult expandedResult = null;
            if ((result is Saxon.Hej.serialize.UnicodeWriterResult)) {
                uWriter = ((Saxon.Hej.serialize.UnicodeWriterResult)(result)).getUnicodeWriter();
            } else {
                if ((result is Saxon.Ejavax.xml.transform.stream.StreamResult)) {
                    expandedResult = new Saxon.Hej.serialize.ExpandedStreamResult(getConfiguration(), (Saxon.Ejavax.xml.transform.stream.StreamResult)(result), props);
                }
            }
            if (((result is Saxon.Ejavax.xml.transform.stream.StreamResult) || (result is Saxon.Hej.serialize.UnicodeWriterResult))) {
                Saxon.Hej.@event.SequenceReceiver target;
                string method = props.getProperty(Saxon.Ejavax.xml.transform.OutputKeys.METHOD);
                if ((method == null)) {
                    return newUncommittedSerializer(result, new Saxon.Hej.@event.Sink(pipe), @params);
                }
                Saxon.Hej.serialize.Emitter emitter = null;
                switch (method) {
                  case "html":
                    {
                        emitter = newHTMLEmitter(props);
                        emitter.setPipelineConfiguration(pipe);
                        if ((uWriter == null)) {
                            uWriter = expandedResult.obtainUnicodeWriter();
                        }
                        emitter.setUnicodeWriter(uWriter);
                        target = createHTMLSerializer(emitter, @params, pipe);
                        break ;
                    }
                  case "xml":
                    {
                        emitter = newXMLEmitter(props);
                        emitter.setPipelineConfiguration(pipe);
                        if ((uWriter == null)) {
                            System.Diagnostics.Debug.Assert((expandedResult != null));
                            uWriter = expandedResult.obtainUnicodeWriter();
                        }
                        emitter.setUnicodeWriter(uWriter);
                        target = createXMLSerializer((Saxon.Hej.serialize.XMLEmitter)(emitter), @params);
                        break ;
                    }
                  case "xhtml":
                    {
                        emitter = newXHTMLEmitter(props);
                        emitter.setPipelineConfiguration(pipe);
                        if ((uWriter == null)) {
                            System.Diagnostics.Debug.Assert((expandedResult != null));
                            uWriter = expandedResult.obtainUnicodeWriter();
                        }
                        emitter.setUnicodeWriter(uWriter);
                        target = createXHTMLSerializer(emitter, @params, pipe);
                        break ;
                    }
                  case "text":
                    {
                        emitter = newTEXTEmitter();
                        emitter.setPipelineConfiguration(pipe);
                        if ((uWriter == null)) {
                            System.Diagnostics.Debug.Assert((expandedResult != null));
                            uWriter = expandedResult.obtainUnicodeWriter();
                        }
                        emitter.setUnicodeWriter(uWriter);
                        target = createTextSerializer(emitter, @params);
                        break ;
                    }
                  case "json":
                    {
                        props.setProperty(Saxon.Ejavax.xml.transform.OutputKeys.OMIT_XML_DECLARATION, "yes");
                        if ((uWriter == null)) {
                            uWriter = expandedResult.obtainUnicodeWriter();
                        }
                        Saxon.Hej.serialize.JSONEmitter je = new Saxon.Hej.serialize.JSONEmitter(pipe, uWriter, props);
                        Saxon.Hej.serialize.JSONSerializer js = new Saxon.Hej.serialize.JSONSerializer(pipe, je, props);
                        string sortOrder = props.getProperty(Saxon.Hej.lib.SaxonOutputKeys.PROPERTY_ORDER);
                        if ((sortOrder != null)) {
                            js.setPropertySorter(getPropertySorter(sortOrder));
                        }
                        Saxon.Hej.serialize.CharacterMapExpander characterMapExpander = makeCharacterMapExpander(pipe, props, charMapIndex);
                        Saxon.Hej.@event.ProxyReceiver normalizer = makeUnicodeNormalizer(pipe, props);
                        return customizeJSONSerializer(js, props, characterMapExpander, normalizer);
                    }
                  case "adaptive":
                    {
                        if ((uWriter == null)) {
                            System.Diagnostics.Debug.Assert((expandedResult != null));
                            uWriter = expandedResult.obtainUnicodeWriter();
                        }
                        Saxon.Hej.serialize.AdaptiveEmitter je = new Saxon.Hej.serialize.AdaptiveEmitter(pipe, uWriter);
                        je.setOutputProperties(props);
                        Saxon.Hej.serialize.CharacterMapExpander characterMapExpander = makeCharacterMapExpander(pipe, props, charMapIndex);
                        Saxon.Hej.@event.ProxyReceiver normalizer = makeUnicodeNormalizer(pipe, props);
                        return customizeAdaptiveSerializer(je, props, characterMapExpander, normalizer);
                    }
                  default:
                    {
                        if (method.StartsWith("{")) {
                            method = ("Q"+method);
                        }
                        if (method.StartsWith((("Q{"+Saxon.Hej.lib.NamespaceConstant.SAXON)+"}"))) {
                            Saxon.Hej.serialize.CharacterMapExpander characterMapExpander = makeCharacterMapExpander(pipe, props, charMapIndex);
                            Saxon.Hej.@event.ProxyReceiver normalizer = makeUnicodeNormalizer(pipe, props);
                            target = createSaxonSerializationMethod(method, @params, pipe, characterMapExpander, normalizer, expandedResult, result);
                            if ((target is Saxon.Hej.serialize.Emitter)) {
                                emitter = (Saxon.Hej.serialize.Emitter)(target);
                            }
                        } else {
                            Saxon.Hej.@event.Receiver userReceiver;
                            userReceiver = createUserDefinedOutputMethod(method, props, pipe);
                            if ((userReceiver is Saxon.Hej.serialize.Emitter)) {
                                emitter = (Saxon.Hej.serialize.Emitter)(userReceiver);
                                if ((uWriter == null)) {
                                    System.Diagnostics.Debug.Assert((expandedResult != null));
                                    uWriter = expandedResult.obtainUnicodeWriter();
                                }
                                emitter.setUnicodeWriter(uWriter);
                                target = @params.makeSequenceNormalizer(emitter);
                            } else {
                                return @params.makeSequenceNormalizer(userReceiver);
                            }
                        }
                    }
                    break ;
                }
                if ((emitter != null)) {
                    emitter.setOutputProperties(props);
                }
                target.setSystemId(result.getSystemId());
                return target;
            } else {
                return getReceiverForNonSerializedResult(result, props, pipe);
            }
        }
        private Saxon.Hej.@event.ProxyReceiver makeUnicodeNormalizer(Saxon.Hej.@event.PipelineConfiguration pipe, Saxon.Ejava.util.Properties props) {
            string normForm = props.getProperty(Saxon.Hej.lib.SaxonOutputKeys.NORMALIZATION_FORM);
            if (((normForm != null) && !(normForm.Equals("none")))) {
                return newUnicodeNormalizer(new Saxon.Hej.@event.Sink(pipe), props);
            }
            return null;
        }
        private Saxon.Hej.serialize.CharacterMapExpander makeCharacterMapExpander(Saxon.Hej.@event.PipelineConfiguration pipe, Saxon.Ejava.util.Properties props, Saxon.Hej.serialize.CharacterMapIndex charMapIndex) {
            string useMaps = props.getProperty(Saxon.Hej.lib.SaxonOutputKeys.USE_CHARACTER_MAPS);
            if ((useMaps != null)) {
                return charMapIndex.makeCharacterMapExpander(useMaps, new Saxon.Hej.@event.Sink(pipe), this);
            }
            return null;
        }
        private Saxon.Hej.@event.Receiver getReceiverForNonSerializedResult(Saxon.Ejavax.xml.transform.Result result, Saxon.Ejava.util.Properties props, Saxon.Hej.@event.PipelineConfiguration pipe) {
            if ((result is Saxon.Hej.serialize.Emitter)) {
                if ((((Saxon.Hej.serialize.Emitter)(result)).getOutputProperties() == null)) {
                    ((Saxon.Hej.serialize.Emitter)(result)).setOutputProperties(props);
                }
                return (Saxon.Hej.serialize.Emitter)(result);
            } else {
                if ((result is Saxon.Hej.serialize.JSONSerializer)) {
                    if ((((Saxon.Hej.serialize.JSONSerializer)(result)).getOutputProperties() == null)) {
                        ((Saxon.Hej.serialize.JSONSerializer)(result)).setOutputProperties(props);
                    }
                    return (Saxon.Hej.serialize.JSONSerializer)(result);
                } else {
                    if ((result is Saxon.Hej.serialize.AdaptiveEmitter)) {
                        if ((((Saxon.Hej.serialize.AdaptiveEmitter)(result)).getOutputProperties() == null)) {
                            ((Saxon.Hej.serialize.AdaptiveEmitter)(result)).setOutputProperties(props);
                        }
                        return (Saxon.Hej.serialize.AdaptiveEmitter)(result);
                    } else {
                        if ((result is Saxon.Hej.@event.Receiver)) {
                            Saxon.Hej.@event.Receiver receiver = (Saxon.Hej.@event.Receiver)(result);
                            receiver.setSystemId(result.getSystemId());
                            receiver.setPipelineConfiguration(pipe);
                            if ((((Saxon.Hej.@event.Receiver)(result)).handlesAppend() && "no".Equals(props.getProperty(Saxon.Hej.lib.SaxonOutputKeys.BUILD_TREE)))) {
                                return receiver;
                            } else {
                                return new Saxon.Hej.@event.TreeReceiver(receiver);
                            }
                        } else {
                            if ((pipe != null)) {
                            }
                        }
                    }
                }
            }
            throw new Saxon.Ejava.lang.IllegalArgumentException(("Unknown type of result: "+result.GetType()));
        }
        public Saxon.Hej.@event.SequenceReceiver makeSequenceNormalizer(Saxon.Hej.@event.Receiver receiver, Saxon.Ejava.util.Properties properties) {
            string method = properties.getProperty(Saxon.Ejavax.xml.transform.OutputKeys.METHOD);
            if (("json".Equals(method) || "adaptive".Equals(method))) {
                return ((receiver is Saxon.Hej.@event.SequenceReceiver) ? (Saxon.Hej.@event.SequenceReceiver)(receiver) : new Saxon.Hej.@event.TreeReceiver(receiver));
            } else {
                Saxon.Hej.@event.PipelineConfiguration pipe = receiver.getPipelineConfiguration();
                Saxon.Hej.@event.SequenceReceiver result;
                string separator = properties.getProperty(Saxon.Hej.lib.SaxonOutputKeys.ITEM_SEPARATOR);
                if (((separator == null) || "#absent".Equals(separator))) {
                    result = new Saxon.Hej.@event.SequenceNormalizerWithSpaceSeparator(receiver);
                } else {
                    result = new Saxon.Hej.@event.SequenceNormalizerWithItemSeparator(receiver, Saxon.Hej.str.StringView.of(separator));
                }
                result.setPipelineConfiguration(pipe);
                return result;
            }
        }
        protected internal Saxon.Hej.@event.SequenceReceiver createHTMLSerializer(Saxon.Hej.serialize.Emitter emitter, Saxon.Hej.serialize.SerializationProperties @params, Saxon.Hej.@event.PipelineConfiguration pipe) {
            Saxon.Hej.@event.Receiver target;
            target = emitter;
            Saxon.Ejava.util.Properties props = @params.getProperties();
            if (!("no".Equals(props.getProperty(Saxon.Ejavax.xml.transform.OutputKeys.INDENT)))) {
                target = newHTMLIndenter(target, props);
            }
            target = new Saxon.Hej.@event.NamespaceDifferencer(target, props);
            target = injectUnicodeNormalizer(@params, target);
            target = injectCharacterMapExpander(@params, target, true);
            string cdataElements = props.getProperty(Saxon.Ejavax.xml.transform.OutputKeys.CDATA_SECTION_ELEMENTS);
            if (((cdataElements != null) && !(cdataElements.Length==0))) {
                target = newCDATAFilter(target, props);
            }
            if (Saxon.Hej.lib.SaxonOutputKeys.isHtmlVersion5(props)) {
                target = addHtml5Component(target, props);
            }
            if (!("no".Equals(props.getProperty(Saxon.Hej.lib.SaxonOutputKeys.ESCAPE_URI_ATTRIBUTES)))) {
                target = newHTMLURIEscaper(target, props);
            }
            if (!("no".Equals(props.getProperty(Saxon.Hej.lib.SaxonOutputKeys.INCLUDE_CONTENT_TYPE)))) {
                target = newHTMLMetaTagAdjuster(target, props);
            }
            string attributeOrder = props.getProperty(Saxon.Hej.lib.SaxonOutputKeys.ATTRIBUTE_ORDER);
            if (((attributeOrder != null) && !(attributeOrder.Length==0))) {
                target = newAttributeSorter(target, props);
            }
            Saxon.Hej.@event.FilterFactory validationFactory = @params.getValidationFactory();
            if ((validationFactory != null)) {
                target = /*Delegate.makeFilter()*/validationFactory(target);
            }
            return makeSequenceNormalizer(target, props);
        }
        protected internal Saxon.Hej.@event.SequenceReceiver createTextSerializer(Saxon.Hej.serialize.Emitter emitter, Saxon.Hej.serialize.SerializationProperties @params) {
            Saxon.Ejava.util.Properties props = @params.getProperties();
            Saxon.Hej.@event.Receiver target;
            target = injectUnicodeNormalizer(@params, emitter);
            target = injectCharacterMapExpander(@params, target, false);
            target = addTextOutputFilter(target, props);
            Saxon.Hej.@event.FilterFactory validationFactory = @params.getValidationFactory();
            if ((validationFactory != null)) {
                target = /*Delegate.makeFilter()*/validationFactory(target);
            }
            return makeSequenceNormalizer(target, props);
        }
        protected internal Saxon.Hej.@event.SequenceReceiver customizeJSONSerializer(Saxon.Hej.serialize.JSONSerializer emitter, Saxon.Ejava.util.Properties props, Saxon.Hej.serialize.CharacterMapExpander characterMapExpander, Saxon.Hej.@event.ProxyReceiver normalizer) {
            if ((normalizer is Saxon.Hej.serialize.UnicodeNormalizer)) {
                emitter.setNormalizationForm(((Saxon.Hej.serialize.UnicodeNormalizer)(normalizer)).getNormalizationForm());
            }
            if ((characterMapExpander != null)) {
                emitter.setCharacterMap(characterMapExpander.getCharacterMap());
            }
            return emitter;
        }
        protected internal Saxon.Hej.@event.SequenceReceiver customizeAdaptiveSerializer(Saxon.Hej.serialize.AdaptiveEmitter emitter, Saxon.Ejava.util.Properties props, Saxon.Hej.serialize.CharacterMapExpander characterMapExpander, Saxon.Hej.@event.ProxyReceiver normalizer) {
            if ((normalizer is Saxon.Hej.serialize.UnicodeNormalizer)) {
                emitter.setNormalizationForm(((Saxon.Hej.serialize.UnicodeNormalizer)(normalizer)).getNormalizationForm());
            }
            if ((characterMapExpander != null)) {
                emitter.setCharacterMap(characterMapExpander.getCharacterMap());
            }
            return emitter;
        }
        protected internal Saxon.Hej.@event.SequenceReceiver createXHTMLSerializer(Saxon.Hej.serialize.Emitter emitter, Saxon.Hej.serialize.SerializationProperties @params, Saxon.Hej.@event.PipelineConfiguration pipe) {
            Saxon.Hej.@event.Receiver target = emitter;
            Saxon.Ejava.util.Properties props = @params.getProperties();
            if (!("no".Equals(props.getProperty(Saxon.Ejavax.xml.transform.OutputKeys.INDENT)))) {
                target = newXHTMLIndenter(target, props);
            }
            target = new Saxon.Hej.@event.NamespaceDifferencer(target, props);
            target = injectUnicodeNormalizer(@params, target);
            target = injectCharacterMapExpander(@params, target, true);
            string cdataElements = props.getProperty(Saxon.Ejavax.xml.transform.OutputKeys.CDATA_SECTION_ELEMENTS);
            if (((cdataElements != null) && !(cdataElements.Length==0))) {
                target = newCDATAFilter(target, props);
            }
            if (Saxon.Hej.lib.SaxonOutputKeys.isXhtmlHtmlVersion5(props)) {
                target = addHtml5Component(target, props);
            }
            if (!("no".Equals(props.getProperty(Saxon.Hej.lib.SaxonOutputKeys.ESCAPE_URI_ATTRIBUTES)))) {
                target = newXHTMLURIEscaper(target, props);
            }
            if (!("no".Equals(props.getProperty(Saxon.Hej.lib.SaxonOutputKeys.INCLUDE_CONTENT_TYPE)))) {
                target = newXHTMLMetaTagAdjuster(target, props);
            }
            string attributeOrder = props.getProperty(Saxon.Hej.lib.SaxonOutputKeys.ATTRIBUTE_ORDER);
            if (((attributeOrder != null) && !(attributeOrder.Length==0))) {
                target = newAttributeSorter(target, props);
            }
            if ((@params.getValidationFactory() != null)) {
                target = /*Delegate.makeFilter()*/@params.getValidationFactory()(target);
            }
            return makeSequenceNormalizer(target, props);
        }
        public Saxon.Hej.@event.Receiver addHtml5Component(Saxon.Hej.@event.Receiver target, Saxon.Ejava.util.Properties outputProperties) {
            target = new Saxon.Hej.@event.NamespaceReducer(target);
            target = new Saxon.Hej.serialize.XHTMLPrefixRemover(target);
            return target;
        }
        protected internal Saxon.Hej.@event.SequenceReceiver createXMLSerializer(Saxon.Hej.serialize.XMLEmitter emitter, Saxon.Hej.serialize.SerializationProperties @params) {
            Saxon.Hej.@event.Receiver target;
            Saxon.Ejava.util.Properties props = @params.getProperties();
            bool canonical = "yes".Equals(props.getProperty(Saxon.Hej.lib.SaxonOutputKeys.CANONICAL));
            if (("yes".Equals(props.getProperty(Saxon.Ejavax.xml.transform.OutputKeys.INDENT)) || canonical)) {
                target = newXMLIndenter(emitter, props);
            } else {
                target = emitter;
            }
            target = new Saxon.Hej.@event.NamespaceDifferencer(target, props);
            if (("1.0".Equals(props.getProperty(Saxon.Ejavax.xml.transform.OutputKeys.VERSION)) && (config.getXMLVersion() == Saxon.Hej.Configuration.XML11))) {
                target = newXML10ContentChecker(target, props);
            }
            target = injectUnicodeNormalizer(@params, target);
            if (!(canonical)) {
                target = injectCharacterMapExpander(@params, target, true);
            }
            string cdataElements = props.getProperty(Saxon.Ejavax.xml.transform.OutputKeys.CDATA_SECTION_ELEMENTS);
            if ((((cdataElements != null) && !(cdataElements.Length==0)) && !(canonical))) {
                target = newCDATAFilter(target, props);
            }
            if (canonical) {
                target = newAttributeSorter(target, props);
                target = newNamespaceSorter(target, props);
            } else {
                string attributeOrder = props.getProperty(Saxon.Hej.lib.SaxonOutputKeys.ATTRIBUTE_ORDER);
                if (((attributeOrder != null) && !(attributeOrder.Length==0))) {
                    target = newAttributeSorter(target, props);
                }
            }
            if ((@params.getValidationFactory() != null)) {
                target = /*Delegate.makeFilter()*/@params.getValidationFactory()(target);
            }
            return makeSequenceNormalizer(target, props);
        }
        protected internal virtual Saxon.Hej.@event.SequenceReceiver createSaxonSerializationMethod(string method, Saxon.Hej.serialize.SerializationProperties @params, Saxon.Hej.@event.PipelineConfiguration pipe, Saxon.Hej.serialize.CharacterMapExpander characterMapExpander, Saxon.Hej.@event.ProxyReceiver normalizer, Saxon.Hej.serialize.ExpandedStreamResult expandedResult, Saxon.Ejavax.xml.transform.Result result) {
            throw new Saxon.Hej.trans.XPathException("Saxon serialization methods require Saxon-PE to be enabled");
        }
        protected internal Saxon.Hej.@event.SequenceReceiver createUserDefinedOutputMethod(string method, Saxon.Ejava.util.Properties props, Saxon.Hej.@event.PipelineConfiguration pipe) {
            Saxon.Hej.@event.Receiver userReceiver;
            userReceiver = pipe.getConfiguration().makeEmitter(method, props);
            userReceiver.setPipelineConfiguration(pipe);
            return ((userReceiver is Saxon.Hej.@event.SequenceReceiver) ? (Saxon.Hej.@event.SequenceReceiver)(userReceiver) : new Saxon.Hej.@event.TreeReceiver(userReceiver));
        }
        protected internal Saxon.Hej.@event.Receiver injectCharacterMapExpander(Saxon.Hej.serialize.SerializationProperties @params, Saxon.Hej.@event.Receiver @out, bool useNullMarkers) {
            Saxon.Hej.serialize.CharacterMapIndex charMapIndex = @params.getCharacterMapIndex();
            if ((charMapIndex != null)) {
                string useMaps = @params.getProperties().getProperty(Saxon.Hej.lib.SaxonOutputKeys.USE_CHARACTER_MAPS);
                if ((useMaps != null)) {
                    Saxon.Hej.serialize.CharacterMapExpander expander = charMapIndex.makeCharacterMapExpander(useMaps, @out, this);
                    expander.setUseNullMarkers(useNullMarkers);
                    return expander;
                }
            }
            return @out;
        }
        protected internal Saxon.Hej.@event.Receiver injectUnicodeNormalizer(Saxon.Hej.serialize.SerializationProperties @params, Saxon.Hej.@event.Receiver @out) {
            Saxon.Ejava.util.Properties props = @params.getProperties();
            string normForm = props.getProperty(Saxon.Hej.lib.SaxonOutputKeys.NORMALIZATION_FORM);
            if (((normForm != null) && !(normForm.Equals("none")))) {
                return newUnicodeNormalizer(@out, props);
            }
            return @out;
        }
        protected internal Saxon.Hej.serialize.UncommittedSerializer newUncommittedSerializer(Saxon.Ejavax.xml.transform.Result result, Saxon.Hej.@event.Receiver next, Saxon.Hej.serialize.SerializationProperties @params) {
            return new Saxon.Hej.serialize.UncommittedSerializer(result, next, @params);
        }
        protected internal virtual Saxon.Hej.serialize.Emitter newXMLEmitter(Saxon.Ejava.util.Properties properties) {
            return new Saxon.Hej.serialize.XMLEmitter();
        }
        protected internal virtual Saxon.Hej.serialize.Emitter newHTMLEmitter(Saxon.Ejava.util.Properties properties) {
            Saxon.Hej.serialize.HTMLEmitter emitter;
            if (Saxon.Hej.lib.SaxonOutputKeys.isHtmlVersion5(properties)) {
                emitter = new Saxon.Hej.serialize.HTML50Emitter();
            } else {
                emitter = new Saxon.Hej.serialize.HTML40Emitter();
            }
            return emitter;
        }
        protected internal virtual Saxon.Hej.serialize.Emitter newXHTMLEmitter(Saxon.Ejava.util.Properties properties) {
            bool is5 = Saxon.Hej.lib.SaxonOutputKeys.isXhtmlHtmlVersion5(properties);
            if (is5) {
                return new Saxon.Hej.serialize.XHTML5Emitter();
            } else {
                return new Saxon.Hej.serialize.XHTML1Emitter();
            }
        }
        public virtual Saxon.Hej.@event.Receiver addTextOutputFilter(Saxon.Hej.@event.Receiver next, Saxon.Ejava.util.Properties properties) {
            return next;
        }
        protected internal Saxon.Hej.serialize.Emitter newTEXTEmitter() {
            return new Saxon.Hej.serialize.TEXTEmitter();
        }
        protected internal virtual Saxon.Hej.@event.ProxyReceiver newXMLIndenter(Saxon.Hej.serialize.XMLEmitter next, Saxon.Ejava.util.Properties outputProperties) {
            Saxon.Hej.serialize.XMLIndenter r = new Saxon.Hej.serialize.XMLIndenter(next);
            r.setOutputProperties(outputProperties);
            return r;
        }
        protected internal virtual Saxon.Hej.@event.ProxyReceiver newHTMLIndenter(Saxon.Hej.@event.Receiver next, Saxon.Ejava.util.Properties outputProperties) {
            Saxon.Hej.serialize.HTMLIndenter r = new Saxon.Hej.serialize.HTMLIndenter(next, "html");
            r.setOutputProperties(outputProperties);
            return r;
        }
        protected internal virtual Saxon.Hej.@event.ProxyReceiver newXHTMLIndenter(Saxon.Hej.@event.Receiver next, Saxon.Ejava.util.Properties outputProperties) {
            string method = "xhtml";
            string htmlVersion = outputProperties.getProperty("html-version");
            if (((htmlVersion != null) && htmlVersion.StartsWith("5"))) {
                method = "xhtml5";
            }
            Saxon.Hej.serialize.HTMLIndenter r = new Saxon.Hej.serialize.HTMLIndenter(next, method);
            r.setOutputProperties(outputProperties);
            return r;
        }
        protected internal Saxon.Hej.serialize.MetaTagAdjuster newXHTMLMetaTagAdjuster(Saxon.Hej.@event.Receiver next, Saxon.Ejava.util.Properties outputProperties) {
            Saxon.Hej.serialize.MetaTagAdjuster r = new Saxon.Hej.serialize.MetaTagAdjuster(next);
            r.setIsXHTML(true);
            r.setOutputProperties(outputProperties);
            return r;
        }
        protected internal Saxon.Hej.serialize.MetaTagAdjuster newHTMLMetaTagAdjuster(Saxon.Hej.@event.Receiver next, Saxon.Ejava.util.Properties outputProperties) {
            Saxon.Hej.serialize.MetaTagAdjuster r = new Saxon.Hej.serialize.MetaTagAdjuster(next);
            r.setIsXHTML(false);
            r.setOutputProperties(outputProperties);
            return r;
        }
        protected internal Saxon.Hej.@event.ProxyReceiver newHTMLURIEscaper(Saxon.Hej.@event.Receiver next, Saxon.Ejava.util.Properties outputProperties) {
            return new Saxon.Hej.serialize.HTMLURIEscaper(next);
        }
        protected internal Saxon.Hej.@event.ProxyReceiver newXHTMLURIEscaper(Saxon.Hej.@event.Receiver next, Saxon.Ejava.util.Properties outputProperties) {
            return new Saxon.Hej.serialize.XHTMLURIEscaper(next);
        }
        protected internal Saxon.Hej.@event.ProxyReceiver newCDATAFilter(Saxon.Hej.@event.Receiver next, Saxon.Ejava.util.Properties outputProperties) {
            Saxon.Hej.serialize.CDATAFilter r = new Saxon.Hej.serialize.CDATAFilter(next);
            r.setOutputProperties(outputProperties);
            return r;
        }
        protected internal virtual Saxon.Hej.@event.Receiver newAttributeSorter(Saxon.Hej.@event.Receiver next, Saxon.Ejava.util.Properties outputProperties) {
            return next;
        }
        protected internal virtual Saxon.Hej.@event.Receiver newNamespaceSorter(Saxon.Hej.@event.Receiver next, Saxon.Ejava.util.Properties outputProperties) {
            return next;
        }
        protected internal Saxon.Hej.@event.ProxyReceiver newXML10ContentChecker(Saxon.Hej.@event.Receiver next, Saxon.Ejava.util.Properties outputProperties) {
            return new Saxon.Hej.serialize.XML10ContentChecker(next);
        }
        protected internal Saxon.Hej.@event.ProxyReceiver newUnicodeNormalizer(Saxon.Hej.@event.Receiver next, Saxon.Ejava.util.Properties outputProperties) {
            string normForm = outputProperties.getProperty(Saxon.Hej.lib.SaxonOutputKeys.NORMALIZATION_FORM);
            return new Saxon.Hej.serialize.UnicodeNormalizer(normForm, next);
        }
        public Saxon.Hej.serialize.CharacterMapExpander newCharacterMapExpander(Saxon.Hej.@event.Receiver next) {
            return new Saxon.Hej.serialize.CharacterMapExpander(next);
        }
        public virtual Saxon.Hej.@event.SequenceReceiver prepareNextStylesheet(Saxon.Hej.@event.PipelineConfiguration pipe, string href, string baseURI, Saxon.Ejavax.xml.transform.Result result) {
            pipe.getConfiguration().checkLicensedFeature(Saxon.Hej.Configuration.LicenseFeature.PROFESSIONAL_EDITION, "saxon:next-in-chain", -1);
            return null;
        }
        public Saxon.Hej.query.SequenceWrapper newSequenceWrapper(Saxon.Hej.@event.Receiver destination) {
            return new Saxon.Hej.query.SequenceWrapper(destination);
        }
        public virtual string checkOutputProperty(string key, string value) {
            if (!(key.StartsWith("{"))) {
                switch (key) {
                  case Saxon.Hej.lib.SaxonOutputKeys.ALLOW_DUPLICATE_NAMES:
                  case Saxon.Hej.lib.SaxonOutputKeys.ESCAPE_URI_ATTRIBUTES:
                  case Saxon.Hej.lib.SaxonOutputKeys.INCLUDE_CONTENT_TYPE:
                  case Saxon.Ejavax.xml.transform.OutputKeys.INDENT:
                  case Saxon.Ejavax.xml.transform.OutputKeys.OMIT_XML_DECLARATION:
                  case Saxon.Hej.lib.SaxonOutputKeys.UNDECLARE_PREFIXES:
                    if ((value != null)) {
                        value = checkYesOrNo(key, value);
                    }
                    break ;
                  case Saxon.Hej.lib.SaxonOutputKeys.BUILD_TREE:
                    if ((value != null)) {
                        value = checkYesOrNo(key, value);
                    }
                    break ;
                  case Saxon.Hej.lib.SaxonOutputKeys.BYTE_ORDER_MARK:
                    if ((value != null)) {
                        value = checkYesOrNo(key, value);
                    }
                    break ;
                  case Saxon.Ejavax.xml.transform.OutputKeys.CDATA_SECTION_ELEMENTS:
                  case Saxon.Hej.lib.SaxonOutputKeys.SUPPRESS_INDENTATION:
                  case Saxon.Hej.lib.SaxonOutputKeys.USE_CHARACTER_MAPS:
                    if ((value != null)) {
                        value = checkListOfEQNames(key, value);
                    }
                    break ;
                  case Saxon.Ejavax.xml.transform.OutputKeys.DOCTYPE_PUBLIC:
                    if ((value != null)) {
                        checkPublicIdentifier(value);
                    }
                    break ;
                  case Saxon.Ejavax.xml.transform.OutputKeys.DOCTYPE_SYSTEM:
                    if ((value != null)) {
                        checkSystemIdentifier(value);
                    }
                    break ;
                  case Saxon.Ejavax.xml.transform.OutputKeys.ENCODING:
                    break ;
                  case Saxon.Hej.lib.SaxonOutputKeys.HTML_VERSION:
                    if ((value != null)) {
                        checkDecimal(key, value);
                    }
                    break ;
                  case Saxon.Hej.lib.SaxonOutputKeys.ITEM_SEPARATOR:
                    break ;
                  case Saxon.Ejavax.xml.transform.OutputKeys.METHOD:
                  case Saxon.Hej.lib.SaxonOutputKeys.JSON_NODE_OUTPUT_METHOD:
                    if ((value != null)) {
                        value = checkMethod(key, value);
                    }
                    break ;
                  case Saxon.Ejavax.xml.transform.OutputKeys.MEDIA_TYPE:
                    break ;
                  case Saxon.Hej.lib.SaxonOutputKeys.NORMALIZATION_FORM:
                    if ((value != null)) {
                        checkNormalizationForm(value);
                    }
                    break ;
                  case Saxon.Hej.lib.SaxonOutputKeys.PARAMETER_DOCUMENT:
                    break ;
                  case Saxon.Ejavax.xml.transform.OutputKeys.STANDALONE:
                    if (((value != null) && !(value.Equals("omit")))) {
                        value = checkYesOrNo(key, value);
                    }
                    break ;
                  case Saxon.Ejavax.xml.transform.OutputKeys.VERSION:
                    break ;
                  default:
                    throw new Saxon.Hej.trans.XPathException(("Unknown serialization parameter "+Saxon.Hej.trans.Err.wrap(key)), "XQST0109");
                }
            } else {
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
                      default:
                        throw new Saxon.Hej.trans.XPathException((("Serialization parameter "+Saxon.Hej.trans.Err.wrap(key, Saxon.Hej.trans.Err.EQNAME))+" is not available in Saxon-HE"), "XQST0109");
                    }
                } else {
                }
            }
            return value;
        }
        protected internal static string checkYesOrNo(string key, string value) {
            if ((("yes".Equals(value) || "true".Equals(value)) || "1".Equals(value))) {
                return "yes";
            } else {
                if ((("no".Equals(value) || "false".Equals(value)) || "0".Equals(value))) {
                    return "no";
                } else {
                    throw new Saxon.Hej.trans.XPathException((("Serialization parameter "+Saxon.Hej.trans.Err.wrap(key))+" must have the value yes|no, true|false, or 1|0"), "SEPM0016");
                }
            }
        }
        private string checkMethod(string key, string value) {
            if ((((!("xml".Equals(value)) && !("html".Equals(value))) && !("xhtml".Equals(value))) && !("text".Equals(value)))) {
                if ((!(Saxon.Hej.lib.SaxonOutputKeys.JSON_NODE_OUTPUT_METHOD.Equals(key)) && (("json".Equals(value) || "adaptive".Equals(value))))) {
                    return value;
                }
                if (value.StartsWith("{")) {
                    value = ("Q"+value);
                }
                if (isValidEQName(value)) {
                    checkExtensions(value);
                } else {
                    throw new Saxon.Hej.trans.XPathException(((("Invalid value ("+value)+") for serialization method: ")+"must be xml|html|xhtml|text|json|adaptive, or a QName in 'Q{uri}local' form"), "SEPM0016");
                }
            }
            return value;
        }
        private static void checkNormalizationForm(string value) {
            if (!(Saxon.Hej.om.NameChecker.isValidNmtoken(Saxon.Hej.str.StringView.of(value)))) {
                throw new Saxon.Hej.trans.XPathException(("Invalid value for normalization-form: "+"must be NFC, NFD, NFKC, NFKD, fully-normalized, or none"), "SEPM0016");
            }
        }
        private static bool isValidEQName(string value) {
            Saxon.Ejava.util.Objects.requireNonNull(value);
            if ((value.Length==0 || !(value.StartsWith("Q{")))) {
                return false;
            }
            int closer = value.IndexOf('}', 2);
            return (((closer>=2) && (closer != (value.Length-1))) && Saxon.Hej.om.NameChecker.isValidNCName(value.Substring((closer+1))));
        }
        private static bool isValidClarkName(string value) {
            if (value.StartsWith("{")) {
                return isValidEQName(("Q"+value));
            } else {
                return isValidEQName(("Q{}"+value));
            }
        }
        protected internal static void checkNonNegativeInteger(string key, string value) {
            try {
                int n = Saxon.Impl.Helpers.Utils.ParseInt(value);
                if ((n<0)) {
                    throw new Saxon.Hej.trans.XPathException((("Value of "+Saxon.Hej.trans.Err.wrap(key))+" must be a non-negative integer"), "SEPM0016");
                }
            } catch (Saxon.Ejava.lang.NumberFormatException err) {
                throw new Saxon.Hej.trans.XPathException((("Value of "+Saxon.Hej.trans.Err.wrap(key))+" must be a non-negative integer"), "SEPM0016");
            }
        }
        private static void checkDecimal(string key, string value) {
            if (!(Saxon.Hej.value.BigDecimalValue.castableAsDecimal(value))) {
                throw new Saxon.Hej.trans.XPathException((("Value of "+Saxon.Hej.trans.Err.wrap(key))+" must be a decimal number"), "SEPM0016");
            }
        }
        protected internal static string checkListOfEQNames(string key, string value) {
            Saxon.Hej.value.Whitespace.Tokenizer tokenizer = new Saxon.Hej.value.Whitespace.Tokenizer(Saxon.Hej.str.StringView.of(value).tidy());
            Saxon.Ejava.lang.StringBuilder builder = new Saxon.Ejava.lang.StringBuilder();
            Saxon.Hej.value.StringValue tok;
            while (((tok = ((/*1430*/Saxon.Hej.value.StringValue)tokenizer.next())) != null)) {
                string s = tok.getStringValue();
                if ((isValidEQName(s) || Saxon.Hej.om.NameChecker.isValidNCName(tok.codePoints()))) {
                    builder.append(s);
                } else {
                    if (isValidClarkName(s)) {
                        if (s.StartsWith("{")) {
                            builder.append("Q").append(s);
                        } else {
                            builder.append("Q{}").append(s);
                        }
                    } else {
                        throw new Saxon.Hej.trans.XPathException((("Value of "+Saxon.Hej.trans.Err.wrap(key))+" must be a list of QNames in 'Q{uri}local' notation"), "SEPM0016");
                    }
                }
                builder.append(" ");
            }
            return builder.ToString();
        }
        protected internal static string checkListOfEQNamesAllowingStar(string key, string value) {
            Saxon.Hej.value.Whitespace.Tokenizer tokenizer = new Saxon.Hej.value.Whitespace.Tokenizer(Saxon.Hej.str.StringView.of(value).tidy());
            Saxon.Ejava.lang.StringBuilder builder = new Saxon.Ejava.lang.StringBuilder();
            Saxon.Hej.value.StringValue tok;
            while (((tok = ((/*1430*/Saxon.Hej.value.StringValue)tokenizer.next())) != null)) {
                string s = tok.getStringValue();
                if ((("*".Equals(s) || isValidEQName(s)) || Saxon.Hej.om.NameChecker.isValidNCName(s))) {
                    builder.append(s);
                } else {
                    if (isValidClarkName(s)) {
                        if (s.StartsWith("{")) {
                            builder.append("Q").append(s);
                        } else {
                            builder.append("Q{}").append(s);
                        }
                    } else {
                        throw new Saxon.Hej.trans.XPathException((("Value of "+Saxon.Hej.trans.Err.wrap(key))+" must be a list of QNames in 'Q{uri}local' notation"), "SEPM0016");
                    }
                }
                builder.append(" ");
            }
            return builder.ToString().Trim();
        }
        private static readonly Saxon.Ejava.util.regex.Pattern publicIdPattern = Saxon.Ejava.util.regex.Pattern.compile("^[\\s\\r\\na-zA-Z0-9\\-'()+,./:=?;!*#@$_%]*$");
        private static void checkPublicIdentifier(string value) {
            if (!(publicIdPattern.matcher(value).matches())) {
                throw new Saxon.Hej.trans.XPathException("Invalid character in doctype-public parameter", "SEPM0016");
            }
        }
        private static void checkSystemIdentifier(string value) {
            if ((value.Contains("'") && value.Contains("\""))) {
                throw new Saxon.Hej.trans.XPathException("The doctype-system parameter must not contain both an apostrophe and a quotation mark", "SEPM0016");
            }
        }
        protected internal virtual void checkExtensions(string key) {
            throw new Saxon.Hej.trans.XPathException((("Serialization property "+Saxon.Hej.trans.Err.wrap(key, Saxon.Hej.trans.Err.EQNAME))+" is not available in Saxon-HE"));
        }
        protected internal virtual System.Collections.Generic.Comparer<Saxon.Hej.value.AtomicValue> getPropertySorter(string sortSpecification) {
            throw new Saxon.Hej.trans.XPathException("Serialization property saxon:property-order is not available in Saxon-HE");
        }
    
    }
}