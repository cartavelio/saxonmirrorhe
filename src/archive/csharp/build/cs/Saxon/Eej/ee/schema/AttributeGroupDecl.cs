using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.ee.schema {
    public sealed class AttributeGroupDecl : Saxon.Eej.ee.schema.SchemaStructure, Saxon.Eej.ee.schema.UserSchemaComponent, Saxon.Eej.ee.schema.SerializableSchemaComponent {
        private readonly System.Collections.Generic.IList<Saxon.Eej.ee.schema.AttributeUse> attributes;
        private readonly System.Collections.Generic.IList<Saxon.Eej.ee.schema.AttributeGroupReference> references;
        private Saxon.Eej.ee.schema.AttributeWildcard anyAttribute = null;
        private Saxon.Hej.om.StructuredQName attributeGroupName;
        private Saxon.Hej.z.IntHashMap<Saxon.Eej.ee.schema.AttributeUse> declaredAttributes = null;
        private Saxon.Hej.z.IntToIntMap counterMap;
        private Saxon.Eej.ee.schema.AttributeGroupDecl redefinedAttributeGroup;
        public AttributeGroupDecl(Saxon.Eej.config.EnterpriseConfiguration config)  {
            setConfiguration(config);
            attributes = new System.Collections.Generic.List<Saxon.Eej.ee.schema.AttributeUse>(10);
            references = new System.Collections.Generic.List<Saxon.Eej.ee.schema.AttributeGroupReference>(10);
        }
        public Saxon.Hej.om.StructuredQName getAttributeGroupName() {
            return attributeGroupName;
        }
        public string getName() {
            return attributeGroupName.getLocalPart();
        }
        public string getTargetNamespace() {
            return attributeGroupName.getURI();
        }
        public void addAttributeUse(Saxon.Eej.ee.schema.AttributeUse use) {
            if ((use == null)) {
                return ;
            }
            attributes.Add(use);
        }
        public void addAttributeGroupReference(Saxon.Eej.ee.schema.AttributeGroupReference attrGroup) {
            if ((attrGroup == null)) {
                return ;
            }
            references.Add(attrGroup);
        }
        public void setRedefinedAttributeGroupDecl(Saxon.Eej.ee.schema.AttributeGroupDecl other) {
            redefinedAttributeGroup = other;
        }
        public System.Collections.Generic.IList<Saxon.Eej.ee.schema.AttributeUse> getLocalAttributes() {
            return attributes;
        }
        public System.Collections.Generic.IList<Saxon.Eej.ee.schema.AttributeGroupReference> getLocalAttributeGroupReferences() {
            return references;
        }
        public override void lookForCycles(System.Collections.Generic.Stack<Saxon.Hej.type.SchemaComponent> references, Saxon.Eej.ee.schema.SchemaCompiler compiler) {
            if ((compiler.getLanguageVersion() == Saxon.Hej.Configuration.XSD11)) {
                return ;
            }
            if (references.Contains(this)) {
                throw new Saxon.Hej.type.SchemaException((("The definition of attribute group "+Saxon.Hej.trans.Err.wrap(getAttributeGroupName().getDisplayName()))+" is circular"), this);
            }
            references.Push(this);
            foreach (Saxon.Eej.ee.schema.AttributeGroupReference group in getLocalAttributeGroupReferences()) {
                group.lookForCycles(references, compiler);
            }
            references.Pop();
        }
        public Saxon.Eej.ee.schema.AttributeWildcard getLocalAttributeWildcard() {
            return anyAttribute;
        }
        public Saxon.Eej.ee.schema.AttributeWildcard getAttributeWildcard(Saxon.Eej.ee.schema.SchemaCompiler compiler) {
            return getAttributeWildcard(compiler, new System.Collections.Generic.Stack<Saxon.Eej.ee.schema.AttributeGroupDecl>());
        }
        internal Saxon.Eej.ee.schema.AttributeWildcard getAttributeWildcard(Saxon.Eej.ee.schema.SchemaCompiler compiler, System.Collections.Generic.Stack<Saxon.Eej.ee.schema.AttributeGroupDecl> stack) {
            Saxon.Eej.ee.schema.AttributeWildcard awc = getLocalAttributeWildcard();
            Saxon.Eej.ee.schema.Wildcard wat = ((awc == null) ? null : awc.getWildcard());
            if (stack.Contains(this)) {
                return null;
            }
            stack.Push(this);
            foreach (Saxon.Eej.ee.schema.AttributeGroupReference @ref in getLocalAttributeGroupReferences()) {
                Saxon.Eej.ee.schema.AttributeWildcard gawc = @ref.getAttributeWildcard(compiler, stack);
                Saxon.Eej.ee.schema.Wildcard gat = ((gawc == null) ? null : gawc.getWildcard());
                if ((gat != null)) {
                    if ((wat == null)) {
                        wat = gat;
                    } else {
                        wat = /*134*/Saxon.Eej.ee.schema.Wildcard.makeIntersection(wat, gat, getConfiguration().getNamePool());
                    }
                }
            }
            stack.Pop();
            return ((wat == null) ? null : new Saxon.Eej.ee.schema.AttributeWildcard(wat));
        }
        public System.Collections.Generic.IList<Saxon.Eej.ee.schema.AttributeUse> getAttributeUses() {
            System.Collections.Generic.IList<Saxon.Eej.ee.schema.AttributeUse> list = new System.Collections.Generic.List<Saxon.Eej.ee.schema.AttributeUse>(20);
            gatherAttributeUses(list);
            return list;
        }
        private void gatherAttributeUses(System.Collections.Generic.IList<Saxon.Eej.ee.schema.AttributeUse> list) {
            gatherAttributeUses(list, new System.Collections.Generic.Stack<Saxon.Eej.ee.schema.AttributeGroupDecl>());
        }
        private void gatherAttributeUses(System.Collections.Generic.IList<Saxon.Eej.ee.schema.AttributeUse> list, System.Collections.Generic.Stack<Saxon.Eej.ee.schema.AttributeGroupDecl> stack) {
            Saxon.Impl.Helpers.ListUtils.AddAll(list, attributes);
            foreach (Saxon.Eej.ee.schema.AttributeGroupReference reference in references) {
                Saxon.Eej.ee.schema.AttributeGroupDecl decl = (Saxon.Eej.ee.schema.AttributeGroupDecl)(reference.getTarget());
                stack.Push(this);
                if (!(stack.Contains(decl))) {
                    decl.gatherAttributeUses(list, stack);
                }
                stack.Pop();
            }
        }
        public Saxon.Hej.z.IntToIntMap getCounterMap() {
            return counterMap;
        }
        public string getDisplayName() {
            return attributeGroupName.getDisplayName();
        }
        public void setAnyAttribute(Saxon.Eej.ee.schema.AttributeWildcard wildcard) {
            anyAttribute = wildcard;
        }
        public override bool fixup(Saxon.Eej.ee.schema.SchemaCompiler compiler) {
            if ((getFixupStatus() == /*explicit enum*/ Saxon.Hej.type.SchemaValidationStatus.UNVALIDATED)) {
                setFixupStatus(/*explicit enum*/ Saxon.Hej.type.SchemaValidationStatus.VALIDATING);
                foreach (Saxon.Eej.ee.schema.AttributeUse use in attributes) {
                    Saxon.Eej.ee.schema.AttributeDecl decl = (Saxon.Eej.ee.schema.AttributeDecl)(/*134*/Saxon.Eej.ee.schema.PreparedSchema.validateReference(use, compiler, false));
                    if ((decl == null)) {
                        setFixupStatus(/*explicit enum*/ Saxon.Hej.type.SchemaValidationStatus.INVALID);
                        continue ;
                    }
                    bool b = decl.fixup(compiler);
                    if (!(b)) {
                        setFixupStatus(/*explicit enum*/ Saxon.Hej.type.SchemaValidationStatus.INVALID);
                    }
                }
                foreach (Saxon.Eej.ee.schema.AttributeGroupReference @ref in references) {
                    Saxon.Eej.ee.schema.AttributeGroupDecl decl = (Saxon.Eej.ee.schema.AttributeGroupDecl)(/*134*/Saxon.Eej.ee.schema.PreparedSchema.validateReference(@ref, compiler, false));
                    if ((decl == null)) {
                        @ref.setFixupStatus(/*explicit enum*/ Saxon.Hej.type.SchemaValidationStatus.INVALID);
                        setFixupStatus(/*explicit enum*/ Saxon.Hej.type.SchemaValidationStatus.INVALID);
                        continue ;
                    }
                    bool b = decl.fixup(compiler);
                    if (!(b)) {
                        setFixupStatus(/*explicit enum*/ Saxon.Hej.type.SchemaValidationStatus.INVALID);
                    }
                }
                if ((getFixupStatus() == /*explicit enum*/ Saxon.Hej.type.SchemaValidationStatus.INVALID)) {
                    return false;
                }
                lookForCycles(new System.Collections.Generic.Stack<Saxon.Hej.type.SchemaComponent>(), compiler);
            }
            setFixupStatus(/*explicit enum*/ Saxon.Hej.type.SchemaValidationStatus.VALIDATED);
            return true;
        }
        public override bool validate(Saxon.Eej.ee.schema.SchemaCompiler compiler) {
            bool result = true;
            if ((getFixupStatus() == /*explicit enum*/ Saxon.Hej.type.SchemaValidationStatus.UNVALIDATED)) {
                result = fixup(compiler);
            }
            if ((getFixupStatus() == /*explicit enum*/ Saxon.Hej.type.SchemaValidationStatus.INVALID)) {
                setValidationStatus(/*explicit enum*/ Saxon.Hej.type.SchemaValidationStatus.INVALID);
                return false;
            }
            switch (getValidationStatus()) {
              case Saxon.Hej.type.SchemaValidationStatus.VALIDATED:
              case Saxon.Hej.type.SchemaValidationStatus.INVALID:
                return true;
              case Saxon.Hej.type.SchemaValidationStatus.VALIDATING:
                compiler.error((("The definition of attribute group "+Saxon.Hej.trans.Err.wrap(getAttributeGroupName().getDisplayName()))+" is circular"), this);
                return false;
              case Saxon.Hej.type.SchemaValidationStatus.UNVALIDATED:
                break ;
            }
            setValidationStatus(/*explicit enum*/ Saxon.Hej.type.SchemaValidationStatus.VALIDATING);
            foreach (Saxon.Eej.ee.schema.AttributeUse att in attributes) {
                result &= att.validate(compiler);
            }
            foreach (Saxon.Eej.ee.schema.AttributeGroupReference @ref in references) {
                result &= @ref.validate(compiler);
            }
            if (!(result)) {
                setValidationStatus(/*explicit enum*/ Saxon.Hej.type.SchemaValidationStatus.INVALID);
                return false;
            }
            Saxon.Eej.ee.schema.AttributeWildcard wild = getAttributeWildcard(compiler);
            if ((wild != null)) {
                result = wild.validate(compiler);
            }
            if (result) {
                string foundID = null;
                System.Collections.Generic.IList<Saxon.Eej.ee.schema.AttributeUse> attributeList = new System.Collections.Generic.List<Saxon.Eej.ee.schema.AttributeUse>();
                gatherAttributeUses(attributeList);
                System.Collections.Generic.ISet<Saxon.Hej.om.StructuredQName> fingerprintSet = new System.Collections.Generic.HashSet<Saxon.Hej.om.StructuredQName>((attributeList.Count*2));
                Saxon.Hej.type.TypeHierarchy th = compiler.getConfiguration().getTypeHierarchy();
                foreach (Saxon.Eej.ee.schema.AttributeUse att in attributeList) {
                    result &= att.validate(compiler);
                    if (result) {
                        Saxon.Eej.ee.schema.AttributeDecl decl = (Saxon.Eej.ee.schema.AttributeDecl)(att.getTarget());
                        Saxon.Hej.type.SimpleType stype = decl.getSimpleType();
                        if (((stype.isAtomicType() && !(((stype.getFingerprint() == Saxon.Hej.om.StandardNames.XS_ANY_SIMPLE_TYPE)))) && th.isSubType((Saxon.Hej.type.AtomicType)(stype), Saxon.Hej.type.BuiltInAtomicType.ID))) {
                            if (((foundID != null) && (compiler.getLanguageVersion() == Saxon.Hej.Configuration.XSD10))) {
                                compiler.error((((("Attribute group contains more than one ID attribute ("+foundID)+", ")+att.getDisplayName())+')'), this);
                                result = false;
                            }
                            foundID = att.getDisplayName();
                        }
                        Saxon.Hej.om.StructuredQName key = att.getTargetComponentName();
                        if (fingerprintSet.Contains(key)) {
                            compiler.error((("Attribute "+Saxon.Hej.trans.Err.wrap(key.getDisplayName(), Saxon.Hej.trans.Err.ATTRIBUTE))+" appears more than once in attribute group"), this);
                            result = false;
                        }
                        fingerprintSet.Add(key);
                    }
                }
                buildCounterMap();
            }
            if ((redefinedAttributeGroup != null)) {
                result &= isValidRestriction(redefinedAttributeGroup, compiler);
            }
            getDeclaredAttributes();
            setValidationStatus(/*explicit enum*/ Saxon.Hej.type.SchemaValidationStatus.VALIDATED);
            return result;
        }
        public void buildCounterMap() {
            System.Collections.Generic.IList<Saxon.Eej.ee.schema.AttributeUse> attributeList = new System.Collections.Generic.List<Saxon.Eej.ee.schema.AttributeUse>();
            gatherAttributeUses(attributeList);
            int size = attributeList.Count;
            if ((size<8)) {
                counterMap = new Saxon.Hej.z.IntToIntArrayMap((size+2));
            } else {
                counterMap = new Saxon.Hej.z.IntToIntHashMap((size+2));
            }
            int counterIndex = 0;
            foreach (Saxon.Eej.ee.schema.AttributeUse att in attributeList) {
                int key = att.getAttributeDeclaration().getFingerprint();
                counterMap.put(key, counterIndex++);
            }
        }
        public bool isValidRestriction(Saxon.Eej.ee.schema.AttributeGroupDecl @base, Saxon.Eej.ee.schema.SchemaCompiler compiler) {
            bool result = true;
            foreach (Saxon.Eej.ee.schema.AttributeUse att in getAttributeUses()) {
                if (att.isProhibited()) {
                    continue ;
                }
                Saxon.Hej.om.StructuredQName targetName = att.getTargetComponentName();
                Saxon.Eej.ee.schema.AttributeUse baseAtt = @base.getAttributeUse(targetName);
                if (((baseAtt == null) || baseAtt.isProhibited())) {
                    Saxon.Eej.ee.schema.AttributeWildcard baseWildCard = @base.getAttributeWildcard(compiler);
                    if ((baseWildCard == null)) {
                        compiler.error((((("Invalid restriction: attribute "+Saxon.Hej.trans.Err.wrap(att.getDisplayName(), Saxon.Hej.trans.Err.ATTRIBUTE))+" is ")+(((baseAtt == null) ? "not defined" : "prohibited")))+" in the base type"), this);
                        result = false;
                    } else {
                        if (!(baseWildCard.getWildcard().matches(att.getTargetComponentName(), false, getConfiguration(), null))) {
                            compiler.error((((("Invalid restriction: attribute "+Saxon.Hej.trans.Err.wrap(att.getDisplayName(), Saxon.Hej.trans.Err.ATTRIBUTE))+" is ")+(((baseAtt == null) ? "not defined" : "prohibited")))+" in the base type"), this);
                            result = false;
                        }
                    }
                } else {
                    if ((baseAtt.isRequired() && !(att.isRequired()))) {
                        compiler.error((("Invalid restriction: attribute "+Saxon.Hej.trans.Err.wrap(att.getDisplayName(), Saxon.Hej.trans.Err.ATTRIBUTE))+" is optional but the base attribute is required"), this);
                        result = false;
                    }
                    if ((baseAtt.isInheritable() != att.isInheritable())) {
                        if (att.isInheritable()) {
                            compiler.error((("Invalid restriction: attribute "+Saxon.Hej.trans.Err.wrap(att.getDisplayName(), Saxon.Hej.trans.Err.ATTRIBUTE))+" is inheritable, but the corresponding attribute in the base type is not"), this);
                        } else {
                            compiler.error((("Invalid restriction: attribute "+Saxon.Hej.trans.Err.wrap(att.getDisplayName(), Saxon.Hej.trans.Err.ATTRIBUTE))+" is not inheritable, but the corresponding attribute in the base type is"), this);
                        }
                        result = false;
                    }
                    Saxon.Eej.ee.schema.AttributeDecl decl = (Saxon.Eej.ee.schema.AttributeDecl)(att.getTarget());
                    Saxon.Eej.ee.schema.AttributeDecl baseDecl = (Saxon.Eej.ee.schema.AttributeDecl)(baseAtt.getTarget());
                    string reason = /*134*/Saxon.Eej.ee.schema.UserSimpleType.isTypeDerivationOK(decl.getSimpleType(), baseDecl.getSimpleType(), 0);
                    if ((reason != null)) {
                        compiler.error(((("Invalid restriction: attribute "+Saxon.Hej.trans.Err.wrap(att.getDisplayName(), Saxon.Hej.trans.Err.ATTRIBUTE))+" is inconsistent with the base type. ")+reason), this);
                        result = false;
                    }
                    Saxon.Hej.om.AtomicSequence baseFixed = baseAtt.getFixedValue();
                    if ((baseFixed == null)) {
                        baseFixed = baseDecl.getFixedValue();
                    }
                    if ((baseFixed != null)) {
                        if ((att.getUnderlyingDefaultValue() != null)) {
                            compiler.error((("Invalid restriction: attribute "+Saxon.Hej.trans.Err.wrap(att.getDisplayName(), Saxon.Hej.trans.Err.ATTRIBUTE))+" has a default value but the base attribute is fixed"), this);
                            result = false;
                        }
                        Saxon.Hej.om.AtomicSequence attFixed = att.getUnderlyingFixedValue();
                        if (((attFixed != null) && !(Saxon.Hej.expr.sort.SimpleTypeComparison.getInstance().equal(attFixed, baseFixed)))) {
                            compiler.error((("Invalid restriction: attribute "+Saxon.Hej.trans.Err.wrap(att.getDisplayName(), Saxon.Hej.trans.Err.ATTRIBUTE))+" has a fixed value different from that of the base attribute"), this);
                            result = false;
                        } else {
                            if ((attFixed == null)) {
                                compiler.error((("Invalid restriction: attribute "+Saxon.Hej.trans.Err.wrap(att.getDisplayName(), Saxon.Hej.trans.Err.ATTRIBUTE))+" must have the same fixed value as the base attribute"), this);
                                result = false;
                            }
                        }
                    }
                }
            }
            foreach (Saxon.Eej.ee.schema.AttributeUse baseAtt in @base.getAttributeUses()) {
                if (baseAtt.isRequired()) {
                    Saxon.Eej.ee.schema.AttributeUse thisAtt = getAttributeUse(baseAtt.getTargetComponentName());
                    if ((thisAtt == null)) {
                        compiler.error(("Invalid restriction: base type has a required attribute "+Saxon.Hej.trans.Err.wrap(baseAtt.getDisplayName(), Saxon.Hej.trans.Err.ATTRIBUTE)), this);
                        result = false;
                    }
                }
            }
            Saxon.Eej.ee.schema.AttributeWildcard wild = getAttributeWildcard(compiler);
            if ((wild != null)) {
                Saxon.Eej.ee.schema.AttributeWildcard baseWild = @base.getAttributeWildcard(compiler);
                if ((baseWild == null)) {
                    compiler.error(("Invalid restriction: this type has an <xs:anyAttribute> "+"but the base type does not"), this);
                    return false;
                }
                if (!(wild.getWildcard().isSubset(baseWild.getWildcard(), compiler.getNamePool()))) {
                    compiler.error(("Invalid restriction: this type has an <anyAttribute> wildcard which allows"+" attribute names that are not permitted by the <anyAttribute> on the base type"), this);
                    return false;
                }
                if ((baseWild.getWildcard().compareStrength(wild.getWildcard())>0)) {
                    compiler.error(("Invalid <anyAttribute> restriction: "+"this type has a weaker processContents than the base type"), this);
                    return false;
                }
            }
            return result;
        }
        public bool isSameDeclaration(Saxon.Eej.ee.schema.AttributeGroupDecl other) {
            return ((this == other) || ((getAttributeGroupName().Equals(other.getAttributeGroupName()) && hasSameLocation(other))));
        }
        public void setAttributeGroupName(Saxon.Hej.om.StructuredQName name) {
            this.attributeGroupName = name;
        }
        public Saxon.Hej.z.IntHashMap<Saxon.Eej.ee.schema.AttributeUse> getDeclaredAttributes() {
            if ((declaredAttributes == null)) {
                Saxon.Hej.z.IntHashMap<Saxon.Eej.ee.schema.AttributeUse> map = new Saxon.Hej.z.IntHashMap<Saxon.Eej.ee.schema.AttributeUse>(10);
                foreach (Saxon.Eej.ee.schema.AttributeUse att in getAttributeUses()) {
                    map.put(att.getAttributeDeclaration().getFingerprint(), att);
                }
                declaredAttributes = map;
            }
            return declaredAttributes;
        }
        public Saxon.Eej.ee.schema.AttributeUse getAttributeUse(Saxon.Hej.om.StructuredQName attributeName) {
            foreach (Saxon.Eej.ee.schema.AttributeUse att in getAttributeUses()) {
                if (att.getTargetComponentName().Equals(attributeName)) {
                    return att;
                }
            }
            return null;
        }
        public void serialize(Saxon.Eej.ee.schema.SchemaModelSerializer serializer) {
            string id = serializer.getId(this, true);
            serializer.startElement("attributeGroup");
            serializer.emitAttribute("id", id);
            serializer.emitAttribute("name", getName());
            if (((getTargetNamespace() != null) && !(getTargetNamespace().Length==0))) {
                serializer.emitAttribute("targetNamespace", getTargetNamespace());
            }
            serializeContents(serializer);
            serializer.endElement();
        }
        public void serializeContents(Saxon.Eej.ee.schema.SchemaModelSerializer serializer) {
            foreach (Saxon.Eej.ee.schema.AttributeUse att in getAttributeUses()) {
                att.serialize(serializer);
            }
            if ((getAttributeWildcard(null) != null)) {
                getAttributeWildcard(null).serialize(serializer);
            }
        }
        public override void elaborate(Saxon.Eej.ee.schema.SchemaCompiler compiler) {
            buildCounterMap();
        }
        public override Saxon.Hej.om.Function getComponentAsFunction() {
            Saxon.Eej.ee.schema.AttributeGroupDecl thisDecl = this;
            Saxon.Hej.expr.CallableDelegate callable = new Saxon.Hej.expr.CallableDelegate((context, arguments) => {
                string key = arguments[0].head().getStringValue();
                switch (key) {
                  case "class":
                    return Saxon.Hej.value.StringValue.bmp("Attribute Group Definition");
                  case "implementation":
                    return new Saxon.Hej.value.ObjectValue<Saxon.Eej.ee.schema.AttributeGroupDecl>(thisDecl);
                  case "name":
                    return new Saxon.Hej.value.StringValue(getName(), Saxon.Hej.type.BuiltInAtomicType.NCNAME);
                  case "target namespace":
                    return new Saxon.Hej.value.AnyURIValue(getTargetNamespace());
                  case "attribute uses":
                    System.Collections.Generic.IList<Saxon.Hej.om.Item> uses = new System.Collections.Generic.List<Saxon.Hej.om.Item>();
                    foreach (Saxon.Eej.ee.schema.AttributeUse use in getAttributeUses()) {
                        uses.Add(use.getComponentAsFunction());
                    }
                    return Saxon.Hej.value.SequenceExtent.makeSequenceExtent(uses);
                  case "attribute wildcard":
                    if ((getLocalAttributeWildcard() != null)) {
                        return getLocalAttributeWildcard().getComponentAsFunction();
                    } else {
                        return Saxon.Hej.value.EmptySequence.getInstance();
                    }
                  default:
                    return Saxon.Hej.value.EmptySequence.getInstance();
                }
            } );
            return new Saxon.Hej.functions.CallableFunction(1, callable, Saxon.Hej.type.SpecificFunctionType.COMPONENT_FUNCTION_TYPE);
        }
    
    }
}