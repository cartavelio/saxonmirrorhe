using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.s9api {
    internal class Serializer : Saxon.Hej.s9api.AbstractDestination {

        static Serializer() {
            var propertyValues = Saxon.Hej.s9api.Serializer.Property.values();
            foreach (Saxon.Hej.s9api.Serializer.Property p in propertyValues) {
                Saxon.Impl.Helpers.MapUtils.Put(standardProperties, p.ToString(), p);
            }
        }
        private Saxon.Hej.s9api.Processor processor;
        private readonly System.Collections.Generic.IDictionary<Saxon.Hej.om.StructuredQName,string> properties = new System.Collections.Generic.Dictionary<Saxon.Hej.om.StructuredQName, string>(10);
        private readonly Saxon.Ejavax.xml.transform.stream.StreamResult result = new Saxon.Ejavax.xml.transform.stream.StreamResult();
        private Saxon.Hej.serialize.CharacterMapIndex characterMapIndex = null;
        private bool mustClose = false;
        private static readonly System.Collections.Generic.IDictionary<string,Saxon.Hej.s9api.Serializer.Property> standardProperties = new System.Collections.Generic.Dictionary<string, Saxon.Hej.s9api.Serializer.Property>();
        public class  Property  {
            public static readonly Property METHOD = new Property("METHOD", InnerEnum.METHOD, Saxon.Ejavax.xml.transform.OutputKeys.METHOD);
            public static readonly Property VERSION = new Property("VERSION", InnerEnum.VERSION, Saxon.Ejavax.xml.transform.OutputKeys.VERSION);
            public static readonly Property ENCODING = new Property("ENCODING", InnerEnum.ENCODING, Saxon.Ejavax.xml.transform.OutputKeys.ENCODING);
            public static readonly Property OMIT_XML_DECLARATION = new Property("OMIT_XML_DECLARATION", InnerEnum.OMIT_XML_DECLARATION, Saxon.Ejavax.xml.transform.OutputKeys.OMIT_XML_DECLARATION);
            public static readonly Property STANDALONE = new Property("STANDALONE", InnerEnum.STANDALONE, Saxon.Ejavax.xml.transform.OutputKeys.STANDALONE);
            public static readonly Property DOCTYPE_PUBLIC = new Property("DOCTYPE_PUBLIC", InnerEnum.DOCTYPE_PUBLIC, Saxon.Ejavax.xml.transform.OutputKeys.DOCTYPE_PUBLIC);
            public static readonly Property DOCTYPE_SYSTEM = new Property("DOCTYPE_SYSTEM", InnerEnum.DOCTYPE_SYSTEM, Saxon.Ejavax.xml.transform.OutputKeys.DOCTYPE_SYSTEM);
            public static readonly Property CDATA_SECTION_ELEMENTS = new Property("CDATA_SECTION_ELEMENTS", InnerEnum.CDATA_SECTION_ELEMENTS, Saxon.Ejavax.xml.transform.OutputKeys.CDATA_SECTION_ELEMENTS);
            public static readonly Property INDENT = new Property("INDENT", InnerEnum.INDENT, Saxon.Ejavax.xml.transform.OutputKeys.INDENT);
            public static readonly Property MEDIA_TYPE = new Property("MEDIA_TYPE", InnerEnum.MEDIA_TYPE, Saxon.Ejavax.xml.transform.OutputKeys.MEDIA_TYPE);
            public static readonly Property USE_CHARACTER_MAPS = new Property("USE_CHARACTER_MAPS", InnerEnum.USE_CHARACTER_MAPS, Saxon.Hej.lib.SaxonOutputKeys.USE_CHARACTER_MAPS);
            public static readonly Property INCLUDE_CONTENT_TYPE = new Property("INCLUDE_CONTENT_TYPE", InnerEnum.INCLUDE_CONTENT_TYPE, Saxon.Hej.lib.SaxonOutputKeys.INCLUDE_CONTENT_TYPE);
            public static readonly Property UNDECLARE_PREFIXES = new Property("UNDECLARE_PREFIXES", InnerEnum.UNDECLARE_PREFIXES, Saxon.Hej.lib.SaxonOutputKeys.UNDECLARE_PREFIXES);
            public static readonly Property ESCAPE_URI_ATTRIBUTES = new Property("ESCAPE_URI_ATTRIBUTES", InnerEnum.ESCAPE_URI_ATTRIBUTES, Saxon.Hej.lib.SaxonOutputKeys.ESCAPE_URI_ATTRIBUTES);
            public static readonly Property BYTE_ORDER_MARK = new Property("BYTE_ORDER_MARK", InnerEnum.BYTE_ORDER_MARK, Saxon.Hej.lib.SaxonOutputKeys.BYTE_ORDER_MARK);
            public static readonly Property NORMALIZATION_FORM = new Property("NORMALIZATION_FORM", InnerEnum.NORMALIZATION_FORM, Saxon.Hej.lib.SaxonOutputKeys.NORMALIZATION_FORM);
            public static readonly Property ITEM_SEPARATOR = new Property("ITEM_SEPARATOR", InnerEnum.ITEM_SEPARATOR, Saxon.Hej.lib.SaxonOutputKeys.ITEM_SEPARATOR);
            public static readonly Property HTML_VERSION = new Property("HTML_VERSION", InnerEnum.HTML_VERSION, Saxon.Hej.lib.SaxonOutputKeys.HTML_VERSION);
            public static readonly Property BUILD_TREE = new Property("BUILD_TREE", InnerEnum.BUILD_TREE, Saxon.Hej.lib.SaxonOutputKeys.BUILD_TREE);
            public static readonly Property SAXON_INDENT_SPACES = new Property("SAXON_INDENT_SPACES", InnerEnum.SAXON_INDENT_SPACES, Saxon.Hej.lib.SaxonOutputKeys.INDENT_SPACES);
            public static readonly Property SAXON_INTERNAL_DTD_SUBSET = new Property("SAXON_INTERNAL_DTD_SUBSET", InnerEnum.SAXON_INTERNAL_DTD_SUBSET, Saxon.Hej.lib.SaxonOutputKeys.INTERNAL_DTD_SUBSET);
            public static readonly Property SAXON_LINE_LENGTH = new Property("SAXON_LINE_LENGTH", InnerEnum.SAXON_LINE_LENGTH, Saxon.Hej.lib.SaxonOutputKeys.LINE_LENGTH);
            public static readonly Property SAXON_ATTRIBUTE_ORDER = new Property("SAXON_ATTRIBUTE_ORDER", InnerEnum.SAXON_ATTRIBUTE_ORDER, Saxon.Hej.lib.SaxonOutputKeys.ATTRIBUTE_ORDER);
            public static readonly Property SAXON_CANONICAL = new Property("SAXON_CANONICAL", InnerEnum.SAXON_CANONICAL, Saxon.Hej.lib.SaxonOutputKeys.CANONICAL);
            public static readonly Property SAXON_NEWLINE = new Property("SAXON_NEWLINE", InnerEnum.SAXON_NEWLINE, Saxon.Hej.lib.SaxonOutputKeys.NEWLINE);
            public static readonly Property SAXON_SUPPRESS_INDENTATION = new Property("SAXON_SUPPRESS_INDENTATION", InnerEnum.SAXON_SUPPRESS_INDENTATION, Saxon.Hej.lib.SaxonOutputKeys.SUPPRESS_INDENTATION);
            public static readonly Property SAXON_DOUBLE_SPACE = new Property("SAXON_DOUBLE_SPACE", InnerEnum.SAXON_DOUBLE_SPACE, Saxon.Hej.lib.SaxonOutputKeys.DOUBLE_SPACE);
            public static readonly Property SAXON_STYLESHEET_VERSION = new Property("SAXON_STYLESHEET_VERSION", InnerEnum.SAXON_STYLESHEET_VERSION, Saxon.Hej.lib.SaxonOutputKeys.STYLESHEET_VERSION);
            public static readonly Property SAXON_CHARACTER_REPRESENTATION = new Property("SAXON_CHARACTER_REPRESENTATION", InnerEnum.SAXON_CHARACTER_REPRESENTATION, Saxon.Hej.lib.SaxonOutputKeys.CHARACTER_REPRESENTATION);
            public static readonly Property SAXON_RECOGNIZE_BINARY = new Property("SAXON_RECOGNIZE_BINARY", InnerEnum.SAXON_RECOGNIZE_BINARY, Saxon.Hej.lib.SaxonOutputKeys.RECOGNIZE_BINARY);
            public static readonly Property SAXON_REQUIRE_WELL_FORMED = new Property("SAXON_REQUIRE_WELL_FORMED", InnerEnum.SAXON_REQUIRE_WELL_FORMED, Saxon.Hej.lib.SaxonOutputKeys.REQUIRE_WELL_FORMED);
            public static readonly Property SAXON_WRAP = new Property("SAXON_WRAP", InnerEnum.SAXON_WRAP, Saxon.Hej.lib.SaxonOutputKeys.WRAP);
            public static readonly Property SAXON_SUPPLY_SOURCE_LOCATOR = new Property("SAXON_SUPPLY_SOURCE_LOCATOR", InnerEnum.SAXON_SUPPLY_SOURCE_LOCATOR, Saxon.Hej.lib.SaxonOutputKeys.SUPPLY_SOURCE_LOCATOR);
            private static readonly List<Property> valueList = new List<Property>();
            static Property() {
                valueList.Add(METHOD);
                valueList.Add(VERSION);
                valueList.Add(ENCODING);
                valueList.Add(OMIT_XML_DECLARATION);
                valueList.Add(STANDALONE);
                valueList.Add(DOCTYPE_PUBLIC);
                valueList.Add(DOCTYPE_SYSTEM);
                valueList.Add(CDATA_SECTION_ELEMENTS);
                valueList.Add(INDENT);
                valueList.Add(MEDIA_TYPE);
                valueList.Add(USE_CHARACTER_MAPS);
                valueList.Add(INCLUDE_CONTENT_TYPE);
                valueList.Add(UNDECLARE_PREFIXES);
                valueList.Add(ESCAPE_URI_ATTRIBUTES);
                valueList.Add(BYTE_ORDER_MARK);
                valueList.Add(NORMALIZATION_FORM);
                valueList.Add(ITEM_SEPARATOR);
                valueList.Add(HTML_VERSION);
                valueList.Add(BUILD_TREE);
                valueList.Add(SAXON_INDENT_SPACES);
                valueList.Add(SAXON_INTERNAL_DTD_SUBSET);
                valueList.Add(SAXON_LINE_LENGTH);
                valueList.Add(SAXON_ATTRIBUTE_ORDER);
                valueList.Add(SAXON_CANONICAL);
                valueList.Add(SAXON_NEWLINE);
                valueList.Add(SAXON_SUPPRESS_INDENTATION);
                valueList.Add(SAXON_DOUBLE_SPACE);
                valueList.Add(SAXON_STYLESHEET_VERSION);
                valueList.Add(SAXON_CHARACTER_REPRESENTATION);
                valueList.Add(SAXON_RECOGNIZE_BINARY);
                valueList.Add(SAXON_REQUIRE_WELL_FORMED);
                valueList.Add(SAXON_WRAP);
                valueList.Add(SAXON_SUPPLY_SOURCE_LOCATOR);
            }
            public static IList<Property> values() {
                return valueList;
            }
            public enum InnerEnum {
                METHOD,
                VERSION,
                ENCODING,
                OMIT_XML_DECLARATION,
                STANDALONE,
                DOCTYPE_PUBLIC,
                DOCTYPE_SYSTEM,
                CDATA_SECTION_ELEMENTS,
                INDENT,
                MEDIA_TYPE,
                USE_CHARACTER_MAPS,
                INCLUDE_CONTENT_TYPE,
                UNDECLARE_PREFIXES,
                ESCAPE_URI_ATTRIBUTES,
                BYTE_ORDER_MARK,
                NORMALIZATION_FORM,
                ITEM_SEPARATOR,
                HTML_VERSION,
                BUILD_TREE,
                SAXON_INDENT_SPACES,
                SAXON_INTERNAL_DTD_SUBSET,
                SAXON_LINE_LENGTH,
                SAXON_ATTRIBUTE_ORDER,
                SAXON_CANONICAL,
                SAXON_NEWLINE,
                SAXON_SUPPRESS_INDENTATION,
                SAXON_DOUBLE_SPACE,
                SAXON_STYLESHEET_VERSION,
                SAXON_CHARACTER_REPRESENTATION,
                SAXON_RECOGNIZE_BINARY,
                SAXON_REQUIRE_WELL_FORMED,
                SAXON_WRAP,
                SAXON_SUPPLY_SOURCE_LOCATOR,
            }
            public readonly InnerEnum innerEnumValue;
            public readonly string nameValue;
            public readonly int ordinalValue;
            public static int nextOrdinal = 0;
            internal Property(string name, InnerEnum innerEnum, string propertyName) { 
                this.propertyName = propertyName;
                nameValue = name;
                ordinalValue = nextOrdinal++;
                innerEnumValue = innerEnum;
        this.name = propertyName;
            } 
            private string propertyName;
            private readonly string name;
            public override  /* REPLACING public */ string ToString() {
                return name;
            }
            public Saxon.Api.QName getQName() {
                return Saxon.Api.QName.FromClarkName(name);
            }
            public static Saxon.Hej.s9api.Serializer.Property get(string s) {
                foreach (Saxon.Hej.s9api.Serializer.Property p in System.Enum.GetValues(typeof(Saxon.Hej.s9api.Serializer.Property))) {
                    if (p.name.Equals(s)) {
                        return p;
                    }
                }
                return null;
            }
        }
        protected internal Serializer(Saxon.Hej.s9api.Processor processor)  {
            setProcessor(processor);
        }
        public void setProcessor(Saxon.Hej.s9api.Processor processor) {
            this.processor = Saxon.Ejava.util.Objects.requireNonNull(processor);
        }
        public Saxon.Hej.s9api.Processor getProcessor() {
            return processor;
        }
        public void setOutputProperties(Saxon.Ejava.util.Properties suppliedProperties) {
            foreach (string name in suppliedProperties.stringPropertyNames()) {
                Saxon.Impl.Helpers.MapUtils.Put(properties, Saxon.Hej.om.StructuredQName.fromClarkName(name), suppliedProperties.getProperty(name));
            }
        }
        public void setOutputProperties(Saxon.Hej.serialize.SerializationProperties suppliedProperties) {
            setOutputProperties(suppliedProperties.getProperties());
            setCharacterMap(suppliedProperties.getCharacterMapIndex());
        }
        public void setCloseOnCompletion(bool value) {
            mustClose = value;
        }
        public void setCharacterMap(Saxon.Hej.serialize.CharacterMapIndex characterMap) {
            Saxon.Hej.serialize.CharacterMapIndex existingIndex = this.characterMapIndex;
            if (((existingIndex == null) || existingIndex.isEmpty())) {
                existingIndex = characterMap;
            } else {
                if ((((characterMap != null) && !(characterMap.isEmpty())) && (existingIndex != characterMap))) {
                    existingIndex = existingIndex.copy();
                    foreach (Saxon.Hej.serialize.CharacterMap map in characterMap) {
                        existingIndex.putCharacterMap(map.getName(), map);
                    }
                }
            }
            this.characterMapIndex = existingIndex;
        }
        public Saxon.Hej.serialize.CharacterMapIndex getCharacterMapIndex() {
            if ((characterMapIndex == null)) {
                characterMapIndex = new Saxon.Hej.serialize.CharacterMapIndex();
            }
            return characterMapIndex;
        }
        public void setOutputProperty(Saxon.Hej.s9api.Serializer.Property property, string value) {
            Saxon.Hej.lib.SerializerFactory sf = processor.getUnderlyingConfiguration().getSerializerFactory();
            try {
                value = sf.checkOutputProperty(property.ToString(), value);
            } catch (Saxon.Hej.trans.XPathException e) {
                throw new Saxon.Ejava.lang.IllegalArgumentException(e.Message);
            }
            if ((value == null)) {
                properties.Remove(property.getQName().GetStructuredQName());
            } else {
                Saxon.Impl.Helpers.MapUtils.Put(properties, property.getQName().GetStructuredQName(), value);
            }
        }
        public string getOutputProperty(Saxon.Hej.s9api.Serializer.Property property) {
            return Saxon.Impl.Helpers.MapUtils.Get(properties, property.getQName().GetStructuredQName());
        }
        public void setOutputProperty(Saxon.Api.QName property, string value) {
            Saxon.Hej.lib.SerializerFactory sf = processor.getUnderlyingConfiguration().getSerializerFactory();
            Saxon.Hej.om.NamespaceUri uri = property.NamespaceUri;
            if ((uri.isEmpty() || uri.Equals(Saxon.Hej.om.NamespaceUri.SAXON))) {
                try {
                    value = sf.checkOutputProperty(property.ClarkName, value);
                } catch (Saxon.Hej.trans.XPathException e) {
                    throw new Saxon.Ejava.lang.IllegalArgumentException(e.Message);
                }
                if ((uri.Equals(Saxon.Hej.om.NamespaceUri.SAXON) && property.LocalName.Equals("next-in-chain"))) {
                    throw new Saxon.Ejava.lang.IllegalArgumentException("saxon:next-in-chain is not a valid serialization property");
                }
            }
            if ((value == null)) {
                properties.Remove(property.GetStructuredQName());
            } else {
                Saxon.Impl.Helpers.MapUtils.Put(properties, property.GetStructuredQName(), value);
            }
        }
        public string getOutputProperty(Saxon.Api.QName property) {
            return Saxon.Impl.Helpers.MapUtils.Get(properties, property.GetStructuredQName());
        }
        public void setOutputWriter(System.IO.TextWriter writer) {
            result.setOutputStream(null);
            result.setSystemId((string)(null));
            result.setWriter(writer);
            mustClose = false;
        }
        public void setOutputStream(System.IO.Stream stream) {
            result.setWriter(null);
            result.setSystemId((string)(null));
            result.setOutputStream(stream);
            mustClose = false;
        }
        public void setOutputFile(Saxon.Ejava.io.File file) {
            result.setOutputStream(null);
            result.setWriter(null);
            result.setSystemId(file);
            setDestinationBaseURI(file.toURI());
            mustClose = true;
        }
        public void serializeNode(Saxon.Api.XdmNode node) {
            Saxon.Ejavax.xml.transform.stream.StreamResult res = result;
            if ((((res.getOutputStream() == null) && (res.getWriter() == null)) && (res.getSystemId() == null))) {
                throw new Saxon.Ejava.lang.IllegalStateException("Either an outputStream, or a Writer, or a File must be supplied");
            }
            serializeNodeToResult(node, res);
        }
        public void serializeXdmValue(Saxon.Api.XdmValue value) {
            if ((value is Saxon.Api.XdmNode)) {
                serializeNode((Saxon.Api.XdmNode)(value));
            } else {
                try {
                    Saxon.Hej.serialize.SerializationProperties properties = new Saxon.Hej.serialize.SerializationProperties(getLocallyDefinedProperties(), characterMapIndex);
                    Saxon.Hej.query.QueryResult.serializeSequence(((Saxon.Hej.om.GroundedValue)value.Unwrap()).iterate(), processor.getUnderlyingConfiguration(), result, properties);
                } catch (Saxon.Hej.trans.XPathException e) {
                    throw new Saxon.Api.SaxonApiException(e);
                }
            }
            closeAndNotify();
        }
        public void serialize(Saxon.Ejavax.xml.transform.Source source) {
            try {
                Saxon.Hej.lib.SerializerFactory sf = processor.getUnderlyingConfiguration().getSerializerFactory();
                Saxon.Hej.@event.Receiver tr = sf.getReceiver(result, new Saxon.Hej.serialize.SerializationProperties(getLocallyDefinedProperties()));
                Saxon.Hej.@event.Sender.send(source, tr, processor.getUnderlyingConfiguration().getParseOptions());
                closeAndNotify();
            } catch (Saxon.Hej.trans.XPathException e) {
                throw new Saxon.Api.SaxonApiException(e);
            }
        }
        public string serializeToString(Saxon.Ejavax.xml.transform.Source source) {
            try {
                Saxon.Hej.lib.SerializerFactory sf = processor.getUnderlyingConfiguration().getSerializerFactory();
                System.IO.StringWriter sw = new System.IO.StringWriter();
                Saxon.Hej.@event.Receiver tr = sf.getReceiver(new Saxon.Ejavax.xml.transform.stream.StreamResult(sw), new Saxon.Hej.serialize.SerializationProperties(getLocallyDefinedProperties()));
                Saxon.Hej.@event.Sender.send(source, tr, processor.getUnderlyingConfiguration().getParseOptions());
                closeAndNotify();
                return sw.ToString();
            } catch (Saxon.Hej.trans.XPathException e) {
                throw new Saxon.Api.SaxonApiException(e);
            }
        }
        public string serializeNodeToString(Saxon.Api.XdmNode node) {
            System.IO.StringWriter sw = new System.IO.StringWriter();
            Saxon.Ejavax.xml.transform.stream.StreamResult sr = new Saxon.Ejavax.xml.transform.stream.StreamResult(sw);
            serializeNodeToResult(node, sr);
            return sw.ToString();
        }
        private void serializeNodeToResult(Saxon.Api.XdmNode node, Saxon.Ejavax.xml.transform.Result res) {
            try {
                Saxon.Hej.query.QueryResult.serialize(((Saxon.Hej.om.NodeInfo)node.Unwrap()), res, getLocallyDefinedProperties());
            } catch (Saxon.Hej.trans.XPathException e) {
                throw new Saxon.Api.SaxonApiException(e);
            }
        }
        public System.Object getOutputDestination() {
            if ((result.getOutputStream() != null)) {
                return result.getOutputStream();
            }
            if ((result.getWriter() != null)) {
                return result.getWriter();
            }
            string systemId = result.getSystemId();
            if ((systemId != null)) {
                try {
                    return new Saxon.Ejava.io.File(new System.Uri(systemId, System.UriKind.RelativeOrAbsolute));
                } catch (System.UriFormatException e) {
                    return null;
                }
            } else {
                return null;
            }
        }
        public override Saxon.Hej.@event.Receiver getReceiver(Saxon.Hej.@event.PipelineConfiguration pipe, Saxon.Hej.serialize.SerializationProperties @params) {
            try {
                Saxon.Hej.lib.SerializerFactory sf = pipe.getConfiguration().getSerializerFactory();
                Saxon.Hej.serialize.SerializationProperties mergedParams = getSerializationProperties().combineWith(@params);
                Saxon.Hej.@event.Receiver target = sf.getReceiver(result, mergedParams, pipe);
                if ((helper.getListeners() != null)) {
                    if ((target is Saxon.Hej.@event.SequenceNormalizer)) {
                        ((Saxon.Hej.@event.SequenceNormalizer)(target)).onClose(helper.getListeners());
                    } else {
                        target = new Saxon.Hej.@event.CloseNotifier(target, helper.getListeners());
                    }
                }
                if (((target.getSystemId() == null) && (getDestinationBaseURI() != null))) {
                    target.setSystemId(getDestinationBaseURI().OriginalString);
                }
                return target;
            } catch (Saxon.Hej.trans.XPathException e) {
                throw new Saxon.Api.SaxonApiException(e);
            }
        }
        public Saxon.Ejava.util.Properties getCombinedOutputProperties(Saxon.Ejava.util.Properties defaultOutputProperties) {
            Saxon.Ejava.util.Properties props = ((defaultOutputProperties == null) ? new Saxon.Ejava.util.Properties() : new Saxon.Ejava.util.Properties(defaultOutputProperties));
            foreach (Saxon.Hej.om.StructuredQName p in Saxon.Impl.Helpers.MapUtils.KeySet(properties)) {
                string value = Saxon.Impl.Helpers.MapUtils.Get(properties, p);
                props.setProperty(p.getClarkName(), value);
            }
            return props;
        }
        protected internal Saxon.Ejava.util.Properties getLocallyDefinedProperties() {
            Saxon.Ejava.util.Properties props = new Saxon.Ejava.util.Properties();
            foreach (Saxon.Hej.om.StructuredQName p in Saxon.Impl.Helpers.MapUtils.KeySet(properties)) {
                string value = Saxon.Impl.Helpers.MapUtils.Get(properties, p);
                props.setProperty(p.getClarkName(), value);
            }
            return props;
        }
        public Saxon.Hej.serialize.SerializationProperties getSerializationProperties() {
            return new Saxon.Hej.serialize.SerializationProperties(getLocallyDefinedProperties(), characterMapIndex);
        }
        protected internal Saxon.Ejavax.xml.transform.Result getResult() {
            return result;
        }
        public override void close() {
            if (mustClose) {
                System.IO.Stream stream = result.getOutputStream();
                if ((stream != null)) {
                    try {
                        stream.Close();
                    } catch (System.IO.IOException err) {
                        throw new Saxon.Api.SaxonApiException("Failed while closing output file", err);
                    }
                }
                System.IO.TextWriter writer = result.getWriter();
                if ((writer != null)) {
                    try {
                        writer.Close();
                    } catch (System.IO.IOException err) {
                        throw new Saxon.Api.SaxonApiException("Failed while closing output file", err);
                    }
                }
            }
        }
        public static Saxon.Hej.s9api.Serializer.Property getProperty(Saxon.Api.QName name) {
            string clarkName = name.ClarkName;
            Saxon.Hej.s9api.Serializer.Property prop = Saxon.Impl.Helpers.MapUtils.Get(standardProperties, clarkName);
            if ((prop != null)) {
                return prop;
            } else {
                throw new Saxon.Ejava.lang.IllegalArgumentException(("Unknown serialization property "+clarkName));
            }
        }
        public bool isMustCloseAfterUse() {
            return mustClose;
        }
    
    }
}