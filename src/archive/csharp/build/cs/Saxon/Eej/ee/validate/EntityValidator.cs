using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.ee.validate {
    public class EntityValidator : Saxon.Eej.ee.validate.ValidatingFilter {
        private readonly System.Collections.Generic.ISet<string> declaredEntities = new System.Collections.Generic.HashSet<string>(10);
        private Saxon.Eej.ee.validate.EntityValidator.EntityChecker currentElementChecker = null;
        private readonly Saxon.Hej.str.UnicodeBuilder buffer = new Saxon.Hej.str.UnicodeBuilder();
        private Saxon.Hej.s9api.Location textLocationId = Saxon.Hej.expr.parser.Loc.NONE;
        private readonly System.Collections.Generic.IDictionary<Saxon.Hej.type.SchemaType,Saxon.Eej.ee.validate.EntityValidator.EntityChecker> checkerMap = new System.Collections.Generic.Dictionary<Saxon.Hej.type.SchemaType, Saxon.Eej.ee.validate.EntityValidator.EntityChecker>(10);
        public EntityValidator(Saxon.Hej.@event.Receiver next) : base (next) {
        }
        public override void setUnparsedEntity(string name, string uri, string publicId) {
            declaredEntities.Add(name);
        }
        public override void startElement(Saxon.Hej.om.NodeName elemName, Saxon.Hej.type.SchemaType type, Saxon.Hej.om.AttributeMap attributes, Saxon.Hej.om.NamespaceMap namespaces, Saxon.Hej.s9api.Location location, int properties) {
            currentElementChecker = Saxon.Impl.Helpers.MapUtils.Get(checkerMap, type);
            if ((currentElementChecker == null)) {
                currentElementChecker = allocateChecker(type);
                Saxon.Impl.Helpers.MapUtils.Put(checkerMap, type, currentElementChecker);
            }
            if ((currentElementChecker == NonEntityChecker)) {
                currentElementChecker = null;
            }
            foreach (Saxon.Hej.om.AttributeInfo att in attributes) {
                Saxon.Hej.type.SimpleType typeCode = att.getType();
                Saxon.Eej.ee.validate.EntityValidator.EntityChecker checker = Saxon.Impl.Helpers.MapUtils.Get(checkerMap, typeCode);
                if ((checker == null)) {
                    checker = allocateChecker(typeCode);
                    Saxon.Impl.Helpers.MapUtils.Put(checkerMap, typeCode, checker);
                }
                /*Delegate.checkValue()*/checker(this, Saxon.Hej.str.StringView.of(att.getValue()).tidy(), att.getLocation());
            }
            nextReceiver.startElement(elemName, type, attributes, namespaces, location, properties);
        }
        public override void characters(Saxon.Hej.str.UnicodeString chars, Saxon.Hej.s9api.Location locationId, int properties) {
            if ((currentElementChecker != null)) {
                buffer.accept(chars);
            }
            nextReceiver.characters(chars, locationId, properties);
            textLocationId = locationId.saveLocation();
        }
        public override void endElement() {
            if ((currentElementChecker != null)) {
                /*Delegate.checkValue()*/currentElementChecker(this, buffer.toUnicodeString(), textLocationId);
            }
            buffer.clear();
            currentElementChecker = null;
            nextReceiver.endElement();
        }
        private Saxon.Eej.ee.validate.EntityValidator.EntityChecker allocateChecker(Saxon.Hej.type.SchemaType type) {
            int fingerprint = type.getFingerprint();
            switch (fingerprint) {
              case Saxon.Hej.om.StandardNames.XS_ENTITY:
                return SimpleEntityChecker;
              case Saxon.Hej.om.StandardNames.XS_ENTITIES:
                return EntityListChecker;
              default:
                if ((fingerprint<1023)) {
                    return NonEntityChecker;
                }
                Saxon.Hej.Configuration config = getConfiguration();
                Saxon.Hej.type.TypeHierarchy th = config.getTypeHierarchy();
                if (type.isComplexType()) {
                    return NonEntityChecker;
                }
                Saxon.Hej.type.SimpleType stype = (Saxon.Hej.type.SimpleType)(type);
                if (type.isAtomicType()) {
                    if (th.isSubType((Saxon.Hej.type.AtomicType)(type), Saxon.Hej.type.BuiltInAtomicType.ENTITY)) {
                        return SimpleEntityChecker;
                    } else {
                        return NonEntityChecker;
                    }
                } else {
                    if (stype.isListType()) {
                        if (isEntityType(stype, th)) {
                            return EntityListChecker;
                        } else {
                            return NonEntityChecker;
                        }
                    } else {
                        if (isEntityType(stype, th)) {
                            return getUnionChecker(stype);
                        } else {
                            return NonEntityChecker;
                        }
                    }
                }
            }
        }
        private static bool isEntityType(Saxon.Hej.type.SimpleType type, Saxon.Hej.type.TypeHierarchy th) {
            if (type.isAtomicType()) {
                return th.isSubType((Saxon.Hej.type.AtomicType)(type), Saxon.Hej.type.BuiltInAtomicType.ENTITY);
            } else {
                if (type.isListType()) {
                    return isEntityType(((Saxon.Hej.type.ListType)(type)).getItemType(), th);
                } else {
                    if (type.isUnionType()) {
                        Saxon.Eej.ee.schema.UserUnionType union = (Saxon.Eej.ee.schema.UserUnionType)(type);
                        foreach (Saxon.Eej.ee.schema.TypeReference @ref in union.getMemberTypeReferences()) {
                            Saxon.Hej.type.SimpleType mem = (Saxon.Hej.type.SimpleType)(@ref.getTarget());
                            if (isEntityType(mem, th)) {
                                return true;
                            }
                        }
                        return false;
                    } else {
                        return false;
                    }
                }
            }
        }
        public override bool usesTypeAnnotations() {
            return true;
        }
        private void processEntityRef(string eref, Saxon.Hej.s9api.Location locationId) {
            if (!(declaredEntities.Contains(eref))) {
                Saxon.Hej.type.ValidationFailure ve = new Saxon.Hej.type.ValidationFailure((("Value '"+eref)+"' is not declared in the DTD as an unparsed entity name"));
                ve.setConstraintReference(1, "cvc-simple-type", "3");
                reportValidationError(ve, false, locationId);
            }
        }
        private delegate void EntityChecker (Saxon.Eej.ee.validate.EntityValidator validator, Saxon.Hej.str.UnicodeString value, Saxon.Hej.s9api.Location locationId);
        private readonly static Saxon.Eej.ee.validate.EntityValidator.EntityChecker SimpleEntityChecker = (validator, value, locationId) => {
            string id = Saxon.Hej.value.Whitespace.trim(value).ToString();
            validator.processEntityRef(id, locationId);
        } ;
        private readonly static Saxon.Eej.ee.validate.EntityValidator.EntityChecker EntityListChecker = (validator, value, locationId) => {
            string ids = value.ToString();
            Saxon.Ejava.util.StringTokenizer tok = new Saxon.Ejava.util.StringTokenizer(ids, " \t\n\r", false);
            while (tok.hasMoreTokens()) {
                string id = tok.nextToken();
                validator.processEntityRef(id, locationId);
            }
        } ;
        private Saxon.Eej.ee.validate.EntityValidator.EntityChecker getUnionChecker(Saxon.Hej.type.SimpleType type) {
            return (validator, value, locationId) => {
                Saxon.Hej.Configuration config = getConfiguration();
                Saxon.Hej.type.TypeHierarchy th = config.getTypeHierarchy();
                Saxon.Hej.om.SequenceIterator iter = type.getTypedValue(value, Saxon.Hej.expr.instruct.DummyNamespaceResolver.getInstance(), config.getConversionRules()).iterate();
                Saxon.Hej.value.AtomicValue val;
                while (((val = (Saxon.Hej.value.AtomicValue)(iter.next())) != null)) {
                    Saxon.Hej.type.AtomicType itemType = val.getItemType();
                    if (isEntityType(itemType, th)) {
                        validator.processEntityRef(val.getStringValue(), locationId);
                    }
                }
            } ;
        }
        private static readonly Saxon.Eej.ee.validate.EntityValidator.EntityChecker NonEntityChecker = (validator, value, locationId) => {
        } ;
    
    }
}