using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.ma.json {
    public class JsonHandlerXML : Saxon.Hej.ma.json.JsonHandler {
        private readonly Saxon.Hej.@event.Outputter @out;
        private readonly Saxon.Hej.@event.Builder builder;
        private System.Collections.Generic.Stack<string> keys;
        private readonly System.Collections.Generic.Stack<bool> inMap = new System.Collections.Generic.Stack<bool>();
        private bool allowAnyTopLevel;
        public bool validate;
        private bool checkForDuplicates;
        private const string SCHEMA_URI = "http://www.w3.org/2005/xpath-functions.xsd";
        private static readonly string JSON_NS = Saxon.Hej.lib.NamespaceConstant.FN;
        public const string PREFIX = "";
        private Saxon.Hej.om.NamePool namePool;
        private Saxon.Hej.om.FingerprintedQName mapQN;
        private Saxon.Hej.om.FingerprintedQName arrayQN;
        private Saxon.Hej.om.FingerprintedQName stringQN;
        private Saxon.Hej.om.FingerprintedQName numberQN;
        private Saxon.Hej.om.FingerprintedQName booleanQN;
        private Saxon.Hej.om.FingerprintedQName nullQN;
        private Saxon.Hej.om.FingerprintedQName keyQN;
        private Saxon.Hej.om.FingerprintedQName escapedQN;
        private Saxon.Hej.om.FingerprintedQName escapedKeyQN;
        private static readonly Saxon.Hej.type.SimpleType SIMPLE_TYPE = Saxon.Hej.type.AnySimpleType.getInstance();
        private static readonly Saxon.Hej.type.SimpleType BOOLEAN_TYPE = Saxon.Hej.type.BuiltInAtomicType.BOOLEAN;
        private static readonly Saxon.Hej.type.SimpleType STRING_TYPE = Saxon.Hej.type.BuiltInAtomicType.STRING;
        public System.Collections.Generic.Dictionary<string,Saxon.Hej.type.SchemaType> types;
        private readonly System.Collections.Generic.Stack<System.Collections.Generic.HashSet<string>> mapKeys = new System.Collections.Generic.Stack<System.Collections.Generic.HashSet<string>>();
        private Saxon.Hej.om.FingerprintedQName qname(string s) {
            Saxon.Hej.om.FingerprintedQName fp = new Saxon.Hej.om.FingerprintedQName("", "", s);
            fp.obtainFingerprint(namePool);
            return fp;
        }
        private Saxon.Hej.om.FingerprintedQName qnameNS(string s) {
            Saxon.Hej.om.FingerprintedQName fp = new Saxon.Hej.om.FingerprintedQName(PREFIX, JSON_NS, s);
            fp.obtainFingerprint(namePool);
            return fp;
        }
        public JsonHandlerXML(Saxon.Hej.expr.XPathContext context, string staticBaseUri, int flags)  {
            init(context, flags);
            builder = context.getController().makeBuilder();
            builder.setSystemId(staticBaseUri);
            builder.setTiming(false);
            @out = new Saxon.Hej.@event.ComplexContentOutputter(builder);
            @out.open();
            @out.startDocument(Saxon.Hej.@event.ReceiverOption.NONE);
        }
        private void init(Saxon.Hej.expr.XPathContext context, int flags) {
            keys = new System.Collections.Generic.Stack<string>();
            setContext(context);
            charChecker = context.getConfiguration().getValidCharacterChecker();
            escape = (((flags&Saxon.Hej.ma.json.JsonParser.ESCAPE)) != 0);
            allowAnyTopLevel = (((flags&Saxon.Hej.ma.json.JsonParser.ALLOW_ANY_TOP_LEVEL)) != 0);
            validate = (((flags&Saxon.Hej.ma.json.JsonParser.VALIDATE)) != 0);
            checkForDuplicates = (validate || (((flags&Saxon.Hej.ma.json.JsonParser.DUPLICATES_RETAINED)) == 0));
            types = new System.Collections.Generic.Dictionary<string, Saxon.Hej.type.SchemaType>();
            namePool = context.getConfiguration().getNamePool();
            mapQN = qnameNS("map");
            arrayQN = qnameNS("array");
            stringQN = qnameNS("string");
            numberQN = qnameNS("number");
            booleanQN = qnameNS("boolean");
            nullQN = qnameNS("null");
            keyQN = qname("key");
            escapedQN = qname("escaped");
            escapedKeyQN = qname("escaped-key");
            if (validate) {
                try {
                    Saxon.Hej.Configuration config = context.getConfiguration();
                    lock (config) {
                        config.checkLicensedFeature(Saxon.Hej.Configuration.LicenseFeature.SCHEMA_VALIDATION, "validation", -1);
                        loadSchema(config);
                    }
                    string[] typeNames = {"mapType", "arrayType", "stringType", "numberType", "booleanType", "nullType", "mapWithinMapType", "arrayWithinMapType", "stringWithinMapType", "numberWithinMapType", "booleanWithinMapType", "nullWithinMapType"};
                    foreach (string t in typeNames) {
                        setType(t, config.getSchemaType(new Saxon.Hej.om.StructuredQName(PREFIX, JSON_NS, t)));
                    }
                } catch (Saxon.Hej.type.SchemaException e) {
                    throw new Saxon.Hej.trans.XPathException(e);
                }
            }
        }
        private void loadSchema(Saxon.Hej.Configuration config) {
            if (!(config.isSchemaAvailable(JSON_NS))) {
                System.Collections.Generic.IList<string> messages = new System.Collections.Generic.List<string>();
                System.IO.Stream stream = Saxon.Hej.Version.platform.locateResource("xpath-functions.scm", messages);
                if (config.isTiming()) {
                    config.getLogger().info(("Loading schema for: "+JSON_NS));
                }
                config.addSchemaSource(new Saxon.Ejavax.xml.transform.stream.StreamSource(stream, "classpath:xpath-functions.xsd"));
            }
        }
        public void setType(string name, Saxon.Hej.type.SchemaType st) {
            Saxon.Impl.Helpers.MapUtils.Put(types, name, st);
        }
        public override bool setKey(string unEscaped, string reEscaped) {
            this.keys.Push(unEscaped);
            return (checkForDuplicates && !(mapKeys.Peek().Add(reEscaped)));
        }
        public override Saxon.Hej.om.Item getResult() {
            @out.endDocument();
            @out.close();
            return builder.getCurrentRoot();
        }
        private bool containsEscape(string literal) {
            return (literal.IndexOf('\\')>=0);
        }
        private bool isInMap() {
            return (!((inMap.Count==0)) && inMap.Peek());
        }
        private void startElement(Saxon.Hej.om.FingerprintedQName qn, string typeName) {
            startElement(qn, Saxon.Impl.Helpers.MapUtils.Get(types, typeName));
        }
        private void startElement(Saxon.Hej.om.FingerprintedQName qn, Saxon.Hej.type.SchemaType st) {
            @out.startElement(qn, ((validate && (st != null)) ? st : Saxon.Hej.type.Untyped.getInstance()), Saxon.Hej.expr.parser.Loc.NONE, Saxon.Hej.@event.ReceiverOption.NONE);
            if (isInMap()) {
                string k = keys.Pop();
                string uk = reEscape(k);
                if (escape) {
                    markAsEscaped(uk, true);
                }
                @out.attribute(keyQN, (validate ? STRING_TYPE : SIMPLE_TYPE), uk, Saxon.Hej.expr.parser.Loc.NONE, Saxon.Hej.@event.ReceiverOption.NONE);
            }
        }
        private void startContent() {
            @out.startContent();
        }
        private void characters(string s) {
            @out.characters(Saxon.Hej.str.StringView.of(s), Saxon.Hej.expr.parser.Loc.NONE, Saxon.Hej.@event.ReceiverOption.NONE);
        }
        private void endElement() {
            @out.endElement();
        }
        public override void startArray() {
            startElement(arrayQN, (isInMap() ? "arrayWithinMapType" : "arrayType"));
            inMap.Push(false);
            startContent();
        }
        public override void endArray() {
            inMap.Pop();
            endElement();
        }
        public override void startMap() {
            startElement(mapQN, (isInMap() ? "mapWithinMapType" : "mapType"));
            if (checkForDuplicates) {
                mapKeys.Push(new System.Collections.Generic.HashSet<string>());
            }
            inMap.Push(true);
            startContent();
        }
        public override void endMap() {
            inMap.Pop();
            if (checkForDuplicates) {
                mapKeys.Pop();
            }
            endElement();
        }
        public override void writeNumeric(string asString, Saxon.Hej.value.AtomicValue parsedValue) {
            startElement(numberQN, (isInMap() ? "numberWithinMapType" : "numberType"));
            startContent();
            characters(asString);
            endElement();
        }
        public override void writeString(string val) {
            startElement(stringQN, (isInMap() ? "stringWithinMapType" : "stringType"));
            string escaped = reEscape(val);
            if (escape) {
                markAsEscaped(escaped, false);
            }
            startContent();
            characters(escaped);
            endElement();
        }
        protected internal override void markAsEscaped(string escaped, bool isKey) {
            if ((containsEscape(escaped) && escape)) {
                Saxon.Hej.om.NodeName name = (isKey ? escapedKeyQN : escapedQN);
                @out.attribute(name, (validate ? BOOLEAN_TYPE : SIMPLE_TYPE), "true", Saxon.Hej.expr.parser.Loc.NONE, Saxon.Hej.@event.ReceiverOption.NONE);
            }
        }
        public override void writeBoolean(bool value) {
            startElement(booleanQN, (isInMap() ? "booleanWithinMapType" : "booleanType"));
            startContent();
            characters((value ? "true" : "false"));
            endElement();
        }
        public override void writeNull() {
            startElement(nullQN, (isInMap() ? "nullWithinMapType" : "nullType"));
            startContent();
            endElement();
        }
    
    }
}