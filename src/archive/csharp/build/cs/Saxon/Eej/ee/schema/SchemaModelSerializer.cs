using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.ee.schema {
    public class SchemaModelSerializer {
        private readonly Saxon.Eej.config.EnterpriseConfiguration config;
        private readonly Saxon.Hej.@event.Receiver receiver;
        private readonly Saxon.Hej.@event.ComplexContentOutputter cco;
        private readonly System.Collections.Generic.LinkedList<Saxon.Hej.type.SchemaComponent> pendingComponents = new System.Collections.Generic.LinkedList<Saxon.Hej.type.SchemaComponent>();
        private readonly Saxon.Ejava.util.IdentityHashMap<Saxon.Hej.type.SchemaComponent,string> componentToIdMap = new Saxon.Ejava.util.IdentityHashMap<Saxon.Hej.type.SchemaComponent, string>(200);
        private readonly Saxon.Ejava.util.IdentityHashMap<Saxon.Eej.ee.schema.SerializableSchemaComponent,string> serializedComponents = new Saxon.Ejava.util.IdentityHashMap<Saxon.Eej.ee.schema.SerializableSchemaComponent, string>(200);
        private int nextComponent = 0;
        private Saxon.Hej.om.NamespaceMap namespaceMap = Saxon.Hej.om.NamespaceMap.emptyMap();
        private bool inStartTag = false;
        public SchemaModelSerializer(Saxon.Hej.Configuration config, Saxon.Hej.@event.Receiver @out)  {
            config.checkLicensedFeature(Saxon.Hej.Configuration.LicenseFeature.SCHEMA_VALIDATION, "schema processing", -1);
            this.config = (Saxon.Eej.config.EnterpriseConfiguration)(config);
            Saxon.Hej.@event.NamespaceReducer reducer = new Saxon.Hej.@event.NamespaceReducer(@out);
            this.receiver = new Saxon.Hej.@event.CheckSumFilter(reducer);
            this.cco = new Saxon.Hej.@event.ComplexContentOutputter(this.receiver);
        }
        public void serialize() {
            cco.open();
            cco.startDocument(Saxon.Hej.@event.ReceiverOption.NONE);
            startElement("schema");
            emitAttribute("generatedAt", Saxon.Hej.value.DateTimeValue.getCurrentDateTime(null).getStringValue());
            emitAttribute("xsdVersion", config.getConfigurationProperty(Saxon.Hej.lib.Feature<string>.XSD_VERSION));
            string dmk = config.getLicenseFeature("DMK");
            if ((dmk != null)) {
                emitAttribute("dmk", dmk);
            }
            Saxon.Eej.ee.schema.PreparedSchema schema = config.getSuperSchema();
            foreach (Saxon.Hej.type.SchemaType type in schema.getAllTypes()) {
                Saxon.Hej.type.SchemaType sType = type;
                if ((sType is Saxon.Eej.ee.schema.sdoc.SimpleTypeDefinition)) {
                    sType = ((Saxon.Eej.ee.schema.sdoc.SimpleTypeDefinition)(type)).getWorkingType();
                }
                if ((sType is Saxon.Eej.ee.schema.SerializableSchemaComponent)) {
                    serializeComponent((Saxon.Eej.ee.schema.SerializableSchemaComponent)(sType));
                }
            }
            foreach (Saxon.Eej.ee.schema.ElementDecl decl in schema.getElementDeclarations()) {
                serializeComponent(decl);
            }
            foreach (Saxon.Eej.ee.schema.AttributeDecl decl in schema.getAttributeDeclarations()) {
                serializeComponent(decl);
            }
            foreach (Saxon.Eej.ee.schema.ModelGroupDefinition decl in schema.getAllModelGroups()) {
                serializeComponent(decl);
            }
            foreach (Saxon.Eej.ee.schema.AttributeGroupDecl group in schema.getAllAttributeGroups()) {
                serializeComponent(group);
            }
            foreach (Saxon.Eej.ee.schema.Notation notation in schema.getAllNotations()) {
                serializeComponent(notation);
            }
            while (!(pendingComponents.Count==0)) {
                Saxon.Eej.ee.schema.SerializableSchemaComponent component = (Saxon.Eej.ee.schema.SerializableSchemaComponent)(pendingComponents.First.Value);
                serializeComponent(component);
                pendingComponents.RemoveFirst();
            }
            endElement();
            if ((receiver is Saxon.Hej.@event.CheckSumFilter)) {
                int c = ((Saxon.Hej.@event.CheckSumFilter)(receiver)).getChecksum();
                cco.processingInstruction(Saxon.Hej.@event.CheckSumFilter.SIGMA, Saxon.Hej.str.BMPString.of(Saxon.Impl.Helpers.Utils.intToHexString(c)), Saxon.Hej.expr.parser.Loc.NONE, Saxon.Hej.@event.ReceiverOption.NONE);
            }
            cco.endDocument();
            cco.close();
        }
        public void startElement(string elementName) {
            if (inStartTag) {
                cco.@namespace("", "", Saxon.Hej.@event.ReceiverOption.NONE);
                cco.startContent();
            }
            inStartTag = true;
            Saxon.Hej.om.NodeName nc = new Saxon.Hej.om.FingerprintedQName("scm", Saxon.Hej.lib.NamespaceConstant.SCM, elementName);
            cco.startElement(nc, Saxon.Hej.type.Untyped.getInstance(), Saxon.Hej.expr.parser.Loc.NONE, Saxon.Hej.@event.ReceiverOption.NONE);
        }
        public void endElement() {
            if (inStartTag) {
                cco.startContent();
                inStartTag = false;
            }
            receiver.endElement();
        }
        public void emitNamespaceContext(Saxon.Hej.om.NamespaceResolver resolver) {
            for (System.Collections.Generic.IEnumerator<string> iter = resolver.iteratePrefixes(); iter.MoveNext() /*UNSAFE CONVERSION*/; ) {
                string prefix = iter.Current;
                string uri = resolver.getURIForPrefix(prefix, false);
                namespaceMap = namespaceMap.put(prefix, uri);
            }
        }
        public void declareNamespace(string prefix, string uri) {
            namespaceMap = namespaceMap.put(prefix, uri);
        }
        public void emitAttribute(string attributeName, string value) {
            if (!(inStartTag)) {
                throw new Saxon.Ejava.lang.IllegalStateException("Attribute emitted when not in start tag");
            }
            Saxon.Hej.om.NodeName nc;
            if (attributeName.Equals("xml:base")) {
                nc = XML_BASE;
            } else {
                nc = new Saxon.Hej.om.NoNamespaceName(attributeName);
            }
            cco.attribute(nc, Saxon.Hej.type.BuiltInAtomicType.UNTYPED_ATOMIC, value, Saxon.Hej.expr.parser.Loc.NONE, Saxon.Hej.@event.ReceiverOption.NONE);
        }
        private static readonly Saxon.Hej.om.FingerprintedQName XML_BASE = new Saxon.Hej.om.FingerprintedQName("xml", Saxon.Hej.lib.NamespaceConstant.XML, "base", Saxon.Hej.om.StandardNames.XML_BASE);
        public void setIsSerialized(Saxon.Eej.ee.schema.SerializableSchemaComponent component) {
            Saxon.Impl.Helpers.MapUtils.Put(serializedComponents, component, "");
        }
        public string getId(Saxon.Hej.type.SchemaComponent component, bool forSelf) {
            if ((component is Saxon.Eej.ee.schema.sdoc.SimpleTypeDefinition)) {
                throw new Saxon.Ejava.lang.IllegalStateException("Unresolved SimpleTypeDefinition found");
            }
            if ((component is Saxon.Eej.ee.schema.AttributeUse)) {
                throw new Saxon.Ejava.lang.IllegalStateException("AttributeUse has no ID");
            }
            if ((component is Saxon.Eej.ee.schema.SerializableSchemaComponent)) {
                string existing = Saxon.Impl.Helpers.MapUtils.Get(componentToIdMap, component);
                if ((existing != null)) {
                    return existing;
                } else {
                    string id = ("C"+nextComponent++);
                    Saxon.Impl.Helpers.MapUtils.Put(componentToIdMap, component, id);
                    if (!(forSelf)) {
                        pendingComponents.AddLast(component);
                    }
                    return id;
                }
            } else {
                throw new Saxon.Ejava.lang.UnsupportedOperationException(component.GetType().Name);
            }
        }
        private void serializeComponent(Saxon.Eej.ee.schema.SerializableSchemaComponent component) {
            if ((Saxon.Impl.Helpers.MapUtils.Get(serializedComponents, component) == null)) {
                component.serialize(this);
                Saxon.Impl.Helpers.MapUtils.Put(serializedComponents, component, "");
            }
        }
        public void emitTypedValue(Saxon.Hej.om.AtomicSequence value) {
            foreach (Saxon.Hej.value.AtomicValue atom in value) {
                startElement("item");
                Saxon.Hej.type.AtomicType type = atom.getPrimitiveType();
                emitAttribute("type", ("#"+type.getName()));
                emitAttribute("value", atom.getUnicodeStringValue().ToString());
                if ((atom is Saxon.Hej.value.QualifiedNameValue)) {
                    emitAttribute("namespace", ((Saxon.Hej.value.QualifiedNameValue)(atom)).getNamespaceURI());
                }
                endElement();
            }
        }
        public string getTypeLink(Saxon.Hej.type.SchemaType type) {
            if (((type is Saxon.Eej.ee.schema.UserSimpleType) || (type is Saxon.Eej.ee.schema.UserComplexType))) {
                return getId(type, false);
            } else {
                return ("#"+type.getName());
            }
        }
    
    }
}