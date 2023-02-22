using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.ee.validate {
    public class IdValidator : Saxon.Eej.ee.validate.ValidatingFilter {
        private readonly Saxon.Hej.tree.util.IndexedStack<System.Collections.Generic.ISet<string>> stack = new Saxon.Hej.tree.util.IndexedStack<System.Collections.Generic.ISet<string>>();
        private int level = 0;
        private readonly System.Collections.Generic.HashSet<string> idValues = new System.Collections.Generic.HashSet<string>(100);
        private readonly System.Collections.Generic.HashSet<string> unresolvedRefs = new System.Collections.Generic.HashSet<string>(100);
        private Saxon.Eej.ee.validate.IdValidator.IdValueChecker currentElementChecker = null;
        private readonly Saxon.Hej.str.UnicodeBuilder buffer = new Saxon.Hej.str.UnicodeBuilder(32);
        private Saxon.Hej.s9api.Location textLocationId = Saxon.Hej.expr.parser.Loc.NONE;
        private readonly System.Collections.Generic.IDictionary<Saxon.Hej.type.SchemaType,Saxon.Eej.ee.validate.IdValidator.IdValueChecker> checkerMap = new System.Collections.Generic.Dictionary<Saxon.Hej.type.SchemaType, Saxon.Eej.ee.validate.IdValidator.IdValueChecker>(10);
        public IdValidator(Saxon.Hej.@event.Receiver next) : base (next) {
        }
        public override void startElement(Saxon.Hej.om.NodeName elemName, Saxon.Hej.type.SchemaType typeCode, Saxon.Hej.om.AttributeMap attributes, Saxon.Hej.om.NamespaceMap namespaces, Saxon.Hej.s9api.Location location, int properties) {
            currentElementChecker = Saxon.Impl.Helpers.MapUtils.Get(checkerMap, typeCode);
            if ((currentElementChecker == null)) {
                currentElementChecker = allocateChecker(typeCode);
                Saxon.Impl.Helpers.MapUtils.Put(checkerMap, typeCode, currentElementChecker);
            }
            if ((currentElementChecker == NonIdChecker)) {
                currentElementChecker = null;
            }
            stack.push(new System.Collections.Generic.HashSet<string>(4));
            level++;
            foreach (Saxon.Hej.om.AttributeInfo att in attributes) {
                Saxon.Hej.type.SimpleType type = att.getType();
                Saxon.Eej.ee.validate.IdValidator.IdValueChecker checker = Saxon.Impl.Helpers.MapUtils.Get(checkerMap, type);
                if ((checker == null)) {
                    checker = allocateChecker(type);
                    Saxon.Impl.Helpers.MapUtils.Put(checkerMap, type, checker);
                }
                /*Delegate.checkValue()*/checker(this, Saxon.Hej.str.StringView.of(att.getValue()).tidy(), att.getLocation());
            }
            nextReceiver.startElement(elemName, typeCode, attributes, namespaces, location, properties);
        }
        public override void characters(Saxon.Hej.str.UnicodeString chars, Saxon.Hej.s9api.Location locationId, int properties) {
            if ((currentElementChecker != null)) {
                buffer.accept(chars);
            }
            nextReceiver.characters(chars, locationId, properties);
            textLocationId = locationId;
        }
        public override void endElement() {
            level--;
            stack.pop();
            if ((currentElementChecker != null)) {
                /*Delegate.checkValue()*/currentElementChecker(this, buffer.toUnicodeString(), textLocationId);
            }
            buffer.clear();
            currentElementChecker = null;
            nextReceiver.endElement();
        }
        private Saxon.Eej.ee.validate.IdValidator.IdValueChecker allocateChecker(Saxon.Hej.type.SchemaType type) {
            int fingerprint = type.getFingerprint();
            switch (fingerprint) {
              case Saxon.Hej.om.StandardNames.XS_ID:
                return SimpleIdChecker;
              case Saxon.Hej.om.StandardNames.XS_IDREF:
                return SimpleIdRefChecker;
              case Saxon.Hej.om.StandardNames.XS_IDREFS:
                return IdRefListChecker;
              default:
                if ((fingerprint<1023)) {
                    return NonIdChecker;
                }
                Saxon.Hej.Configuration config = getConfiguration();
                Saxon.Hej.type.TypeHierarchy th = config.getTypeHierarchy();
                Saxon.Hej.type.SimpleType stype;
                if (type.isComplexType()) {
                    if (((Saxon.Hej.type.ComplexType)(type)).isSimpleContent()) {
                        stype = ((Saxon.Hej.type.ComplexType)(type)).getSimpleContentType();
                        System.Diagnostics.Debug.Assert((stype != null));
                    } else {
                        return NonIdChecker;
                    }
                } else {
                    stype = (Saxon.Hej.type.SimpleType)(type);
                }
                if (type.isAtomicType()) {
                    if (type.isIdType()) {
                        return SimpleIdChecker;
                    } else {
                        if (type.isIdRefType()) {
                            return SimpleIdRefChecker;
                        } else {
                            return NonIdChecker;
                        }
                    }
                } else {
                    if (stype.isListType()) {
                        Saxon.Hej.type.SimpleType componentType = ((Saxon.Hej.type.ListType)(stype)).getItemType();
                        if (((componentType is Saxon.Hej.type.AtomicType) && th.isSubType((Saxon.Hej.type.AtomicType)(componentType), Saxon.Hej.type.BuiltInAtomicType.ID))) {
                            return IdListChecker;
                        } else {
                            if (((componentType is Saxon.Hej.type.AtomicType) && th.isSubType((Saxon.Hej.type.AtomicType)(componentType), Saxon.Hej.type.BuiltInAtomicType.IDREF))) {
                                return IdRefListChecker;
                            } else {
                                if ((type.isIdType() || type.isIdRefType())) {
                                    return getUnionChecker(stype);
                                } else {
                                    return NonIdChecker;
                                }
                            }
                        }
                    } else {
                        if ((type.isIdType() || type.isIdRefType())) {
                            return getUnionChecker(stype);
                        } else {
                            return NonIdChecker;
                        }
                    }
                }
            }
        }
        public override void endDocument() {
            checkUnresolvedRefs();
            nextReceiver.endDocument();
        }
        public override void close() {
            nextReceiver.close();
        }
        private void checkUnresolvedRefs() {
            if (!(unresolvedRefs.Count==0)) {
                int count = unresolvedRefs.Count;
                string howMany;
                string value;
                if ((count == 1)) {
                    howMany = " is one";
                    value = "value";
                } else {
                    Saxon.Hej.expr.number.Numberer_en num = new Saxon.Hej.expr.number.Numberer_en();
                    num.setTensUnitsSeparatorCardinal("-");
                    howMany = (" are "+num.toWords("", count, Saxon.Hej.expr.number.AbstractNumberer.LOWER_CASE));
                    value = "values";
                }
                Saxon.Ejava.lang.StringBuilder err = new Saxon.Ejava.lang.StringBuilder((((("There"+howMany)+" IDREF ")+value)+" with no corresponding ID"));
                if ((count>10)) {
                    err.append(". First ten:");
                } else {
                    err.append(':');
                }
                int i = 0;
                foreach (string id in unresolvedRefs) {
                    err.append("\n    ");
                    err.append(id);
                    if ((i++>9)) {
                        break ;
                    }
                }
                Saxon.Hej.type.ValidationFailure ve = new Saxon.Hej.type.ValidationFailure(err.ToString());
                ve.setErrorCode((getPipelineConfiguration().isXSLT() ? "XTTE1555" : "XQDY0027"));
                ve.setConstraintReference(1, "cvc-id", "1");
                reportValidationError(ve, Saxon.Hej.expr.parser.Loc.NONE);
            }
        }
        public override bool usesTypeAnnotations() {
            return true;
        }
        protected internal void reportValidationError(Saxon.Hej.type.ValidationFailure err, Saxon.Hej.s9api.Location locationId) {
            Saxon.Hej.@event.PipelineConfiguration pipe = getPipelineConfiguration();
            err.setErrorCode((pipe.isXSLT() ? "XTTE1555" : "XQDY0027"));
            base.reportValidationError(err, false, locationId);
        }
        private void processId(string id, Saxon.Hej.s9api.Location locationId) {
            if ((level == 0)) {
                Saxon.Hej.type.ValidationFailure ve = new Saxon.Hej.type.ValidationFailure("An element of type xs:ID must have a parent element within the tree being validated");
                ve.setErrorCode((getPipelineConfiguration().isXSLT() ? "XTTE1555" : "XQDY0027"));
                ve.setConstraintReference(1, "cvc-id", "2");
                reportValidationError(ve, locationId);
                throw ve.makeException();
            }
            if (idValues.Contains(id)) {
                System.Collections.Generic.ISet<string> idsForElement = stack.get((level-1));
                if (((getConfiguration().getXsdVersion() == Saxon.Hej.Configuration.XSD10) || !(idsForElement.Contains(id)))) {
                    Saxon.Hej.type.ValidationFailure ve = new Saxon.Hej.type.ValidationFailure((("ID value '"+id)+"' is not unique"));
                    ve.setErrorCode((getPipelineConfiguration().isXSLT() ? "XTTE1555" : "XQDY0027"));
                    ve.setConstraintReference(1, "cvc-id", "2");
                    reportValidationError(ve, locationId);
                }
            } else {
                idValues.Add(id);
                if ((level>=1)) {
                    stack.get((level-1)).Add(id);
                }
            }
            unresolvedRefs.Remove(id);
        }
        private delegate void IdValueChecker (Saxon.Eej.ee.validate.IdValidator validator, Saxon.Hej.str.UnicodeString value, Saxon.Hej.s9api.Location locationId);
        private readonly static Saxon.Eej.ee.validate.IdValidator.IdValueChecker SimpleIdChecker = (validator, value, locationId) => validator.processId(Saxon.Hej.value.Whitespace.trim(value).ToString(), locationId);
        private readonly static Saxon.Eej.ee.validate.IdValidator.IdValueChecker SimpleIdRefChecker = (validator, value, locationId) => {
            string id = Saxon.Hej.value.Whitespace.trim(value).ToString();
            if ((!(id.Length==0) && !(validator.idValues.Contains(id)))) {
                validator.unresolvedRefs.Add(id);
            }
        } ;
        private readonly static Saxon.Eej.ee.validate.IdValidator.IdValueChecker IdListChecker = (validator, value, locationId) => {
            string ids = value.ToString();
            Saxon.Ejava.util.StringTokenizer tok = new Saxon.Ejava.util.StringTokenizer(ids, " \t\n\r", false);
            while (tok.hasMoreTokens()) {
                string id = tok.nextToken();
                validator.processId(id, locationId);
            }
        } ;
        private readonly static Saxon.Eej.ee.validate.IdValidator.IdValueChecker IdRefListChecker = (validator, value, locationId) => {
            string ids = value.ToString();
            Saxon.Ejava.util.StringTokenizer tok = new Saxon.Ejava.util.StringTokenizer(ids, " \t\n\r", false);
            while (tok.hasMoreTokens()) {
                string id = tok.nextToken();
                if (!(validator.idValues.Contains(id))) {
                    validator.unresolvedRefs.Add(id);
                }
            }
        } ;
        private Saxon.Eej.ee.validate.IdValidator.IdValueChecker getUnionChecker(Saxon.Hej.type.SimpleType type) {
            return (validator, value, locationId) => {
                Saxon.Hej.Configuration config = getConfiguration();
                Saxon.Hej.type.TypeHierarchy th = config.getTypeHierarchy();
                Saxon.Hej.om.AtomicSequence typedValue = type.getTypedValue(value, Saxon.Hej.expr.instruct.DummyNamespaceResolver.getInstance(), config.getConversionRules());
                foreach (Saxon.Hej.value.AtomicValue val in typedValue) {
                    Saxon.Hej.type.AtomicType itemType = val.getItemType();
                    if (th.isSubType(itemType, Saxon.Hej.type.BuiltInAtomicType.ID)) {
                        processId(val.getStringValue(), locationId);
                    } else {
                        if (th.isSubType(itemType, Saxon.Hej.type.BuiltInAtomicType.IDREF)) {
                            string idref = val.getStringValue();
                            if (!(idValues.Contains(idref))) {
                                unresolvedRefs.Add(idref);
                            }
                        }
                    }
                }
            } ;
        }
        private static readonly Saxon.Eej.ee.validate.IdValidator.IdValueChecker NonIdChecker = (validator, value, locationId) => {
        } ;
    
    }
}