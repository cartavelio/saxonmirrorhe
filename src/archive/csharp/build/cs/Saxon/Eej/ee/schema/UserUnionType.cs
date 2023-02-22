using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.ee.schema {
    public class UserUnionType : Saxon.Eej.ee.schema.UserSimpleType, Saxon.Hej.type.PlainType, Saxon.Hej.type.UnionType {
        public Saxon.Hej.om.Genre getGenre() {
            return Saxon.Hej.om.Genre.ATOMIC;
        }
        public override Saxon.Hej.om.StructuredQName getStructuredQName() {
            return getTypeName();
        }
        private System.Collections.Generic.IList<Saxon.Eej.ee.schema.TypeReference> memberTypeReferences = null;
        private int isPlainUnion = -1;
        public UserUnionType(Saxon.Eej.config.EnterpriseConfiguration config)  {
            setConfiguration(config);
            memberTypeReferences = new System.Collections.Generic.List<Saxon.Eej.ee.schema.TypeReference>(3);
        }
        public void setMemberTypeReferences(System.Collections.Generic.IList<Saxon.Eej.ee.schema.TypeReference> members) {
            memberTypeReferences = members;
        }
        public void addMemberTypeReference(Saxon.Eej.ee.schema.TypeReference member) {
            if ((memberTypeReferences == null)) {
                memberTypeReferences = new System.Collections.Generic.List<Saxon.Eej.ee.schema.TypeReference>(5);
            }
            memberTypeReferences.Add(member);
        }
        public System.Collections.Generic.IList<Saxon.Eej.ee.schema.TypeReference> getMemberTypeReferences() {
            if ((memberTypeReferences == null)) {
                if ((getBaseType() is Saxon.Eej.ee.schema.UserUnionType)) {
                    memberTypeReferences = ((Saxon.Eej.ee.schema.UserUnionType)(getBaseType())).getMemberTypeReferences();
                } else {
                    memberTypeReferences = new System.Collections.Generic.List<Saxon.Eej.ee.schema.TypeReference>();
                }
            }
            return memberTypeReferences;
        }
        public override bool isAtomicType() {
            return false;
        }
        public bool containsListType() {
            foreach (Saxon.Eej.ee.schema.TypeReference @ref in getMemberTypeReferences()) {
                Saxon.Hej.type.SimpleType type = (Saxon.Hej.type.SimpleType)(@ref.getTarget());
                if (type.isListType()) {
                    return true;
                } else {
                    if (type.isUnionType()) {
                        return ((Saxon.Eej.ee.schema.UserUnionType)(type)).containsListType();
                    }
                }
            }
            return false;
        }
        public bool isPlainType() {
            if ((isPlainUnion == -1)) {
                if (((Saxon.Hej.type.SimpleType)(getBaseType())).isUnionType()) {
                    isPlainUnion = 0;
                    return false;
                }
                foreach (Saxon.Eej.ee.schema.TypeReference @ref in getMemberTypeReferences()) {
                    try {
                        Saxon.Hej.type.SimpleType st = (Saxon.Hej.type.SimpleType)(@ref.getTarget());
                        if (!((((st is Saxon.Hej.type.ItemType) && ((Saxon.Hej.type.ItemType)(st)).isPlainType())))) {
                            isPlainUnion = 0;
                            return false;
                        }
                    } catch (Saxon.Hej.type.MissingComponentException e) {
                        return false;
                    }
                }
                isPlainUnion = 1;
            }
            return (isPlainUnion == 1);
        }
        public Saxon.Hej.value.SequenceType getResultTypeOfCast() {
            try {
                if (isPlainType()) {
                    return Saxon.Hej.value.SequenceType.makeSequenceType(this, Saxon.Hej.expr.StaticProperty.ALLOWS_ZERO_OR_ONE);
                } else {
                    if (containsListType()) {
                        return Saxon.Hej.value.SequenceType.ATOMIC_SEQUENCE;
                    } else {
                        return ((Saxon.Eej.ee.schema.UserUnionType)(getBaseType())).getResultTypeOfCast();
                    }
                }
            } catch (Saxon.Hej.type.MissingComponentException e) {
                return Saxon.Hej.value.SequenceType.ATOMIC_SEQUENCE;
            }
        }
        public override bool isIdType() {
            foreach (Saxon.Eej.ee.schema.TypeReference @ref in getMemberTypeReferences()) {
                Saxon.Hej.type.SimpleType st = (Saxon.Hej.type.SimpleType)(@ref.getTarget());
                if (st.isIdType()) {
                    return true;
                }
            }
            return false;
        }
        public override bool isIdRefType() {
            foreach (Saxon.Eej.ee.schema.TypeReference @ref in getMemberTypeReferences()) {
                Saxon.Hej.type.SimpleType st = (Saxon.Hej.type.SimpleType)(@ref.getTarget());
                if (st.isIdRefType()) {
                    return true;
                }
            }
            return false;
        }
        public override bool isBuiltInType() {
            return false;
        }
        public override bool isListType() {
            return false;
        }
        public override bool isUnionType() {
            return true;
        }
        public Saxon.Hej.type.UType getUType() {
            Saxon.Hej.type.UType u = Saxon.Hej.type.UType.VOID;
            foreach (Saxon.Eej.ee.schema.TypeReference @ref in getMemberTypeReferences()) {
                try {
                    Saxon.Hej.type.SimpleType st = (Saxon.Hej.type.SimpleType)(@ref.getTarget());
                    if ((st is Saxon.Hej.type.PlainType)) {
                        u = u.union(((Saxon.Hej.type.PlainType)(st)).getUType());
                    } else {
                        throw new Saxon.Ejava.lang.UnsupportedOperationException("Union type is not a plain type");
                    }
                } catch (Saxon.Hej.type.MissingComponentException e) {
                    throw new Saxon.Ejava.lang.UnsupportedOperationException(("Union type is not a plain type. "+e.Message));
                }
            }
            return u;
        }
        public string getBasicAlphaCode() {
            return "A";
        }
        public override bool isNamespaceSensitive() {
            try {
                foreach (Saxon.Eej.ee.schema.TypeReference @ref in getMemberTypeReferences()) {
                    Saxon.Hej.type.SimpleType st = (Saxon.Hej.type.SimpleType)(@ref.getTarget());
                    if (st.isNamespaceSensitive()) {
                        return true;
                    }
                }
            } catch (Saxon.Hej.type.MissingComponentException e) {
                return true;
            }
            return false;
        }
        public override bool validate(Saxon.Eej.ee.schema.SchemaCompiler compiler) {
            bool result = true;
            switch (getValidationStatus()) {
              case Saxon.Hej.type.SchemaValidationStatus.VALIDATED:
              case Saxon.Hej.type.SchemaValidationStatus.INVALID:
                return true;
              case Saxon.Hej.type.SchemaValidationStatus.VALIDATING:
                compiler.error((("The definition of the union type "+getDescription())+" is circular"), this);
                return false;
              case Saxon.Hej.type.SchemaValidationStatus.UNVALIDATED:
                break ;
            }
            base.validate(compiler);
            setValidationStatus(/*explicit enum*/ Saxon.Hej.type.SchemaValidationStatus.VALIDATING);
            getMemberTypeReferences();
            lookForCycles(new System.Collections.Generic.Stack<Saxon.Hej.type.SchemaComponent>(), compiler);
            setValidationStatus(/*explicit enum*/ Saxon.Hej.type.SchemaValidationStatus.VALIDATED);
            return result;
        }
        public override void elaborate(Saxon.Eej.ee.schema.SchemaCompiler compiler) {
            foreach (Saxon.Eej.ee.schema.TypeReference @ref in getMemberTypeReferences()) {
                Saxon.Hej.type.SimpleType member = (Saxon.Hej.type.SimpleType)(@ref.getTarget());
                if ((member is Saxon.Eej.ee.schema.UserSimpleType)) {
                    ((Saxon.Eej.ee.schema.UserSimpleType)(member)).elaborate(compiler);
                }
            }
            base.elaborate(compiler);
        }
        public override void lookForCycles(System.Collections.Generic.Stack<Saxon.Hej.type.SchemaComponent> references, Saxon.Eej.ee.schema.SchemaCompiler compiler) {
            foreach (System.Object reference in references) {
                if (((Saxon.Hej.type.SchemaType)(reference)).isSameType(this)) {
                    compiler.error((("The definition of the union type "+getDescription())+" is circular"), this);
                    throw new Saxon.Hej.type.SchemaException("Circular definition found");
                }
            }
            references.Push(this);
            foreach (Saxon.Eej.ee.schema.TypeReference @ref in getMemberTypeReferences()) {
                @ref.tryToResolve(compiler, false);
                if (@ref.isResolved()) {
                    Saxon.Hej.type.SchemaType st = (Saxon.Hej.type.SchemaType)(@ref.getTarget());
                    if ((st is Saxon.Eej.ee.schema.sdoc.SimpleTypeDefinition)) {
                        ((Saxon.Eej.ee.schema.sdoc.SimpleTypeDefinition)(st)).fixup(compiler);
                        st = ((Saxon.Eej.ee.schema.sdoc.SimpleTypeDefinition)(st)).getWorkingType();
                    }
                    if ((st is Saxon.Eej.ee.schema.UserUnionType)) {
                        ((Saxon.Eej.ee.schema.UserUnionType)(st)).lookForCycles(references, compiler);
                        while (true) {
                            Saxon.Hej.type.SchemaType stb = st.getBaseType();
                            if ((stb is Saxon.Eej.ee.schema.UserUnionType)) {
                                ((Saxon.Eej.ee.schema.UserUnionType)(stb)).lookForCycles(references, compiler);
                                st = stb;
                            } else {
                                break ;
                            }
                        }
                    } else {
                        if ((st is Saxon.Eej.ee.schema.UserListType)) {
                            Saxon.Hej.type.SimpleType it = ((Saxon.Eej.ee.schema.UserListType)(st)).getItemType();
                            if ((it is Saxon.Eej.ee.schema.sdoc.SimpleTypeDefinition)) {
                                ((Saxon.Eej.ee.schema.sdoc.SimpleTypeDefinition)(it)).fixup(compiler);
                                it = ((Saxon.Eej.ee.schema.sdoc.SimpleTypeDefinition)(it)).getWorkingType();
                            }
                            if (it.isSameType(this)) {
                                compiler.error((("The definition of the union type "+getDescription())+" is circular"), this);
                                throw new Saxon.Hej.type.SchemaException("Circular definition found");
                            } else {
                                if ((it is Saxon.Eej.ee.schema.UserUnionType)) {
                                    ((Saxon.Eej.ee.schema.UserUnionType)(it)).lookForCycles(references, compiler);
                                    while (true) {
                                        Saxon.Hej.type.SchemaType stb = it.getBaseType();
                                        if ((stb is Saxon.Eej.ee.schema.UserUnionType)) {
                                            ((Saxon.Eej.ee.schema.UserUnionType)(stb)).lookForCycles(references, compiler);
                                            it = (Saxon.Eej.ee.schema.UserUnionType)(stb);
                                        } else {
                                            break ;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            references.Pop();
        }
        public override Saxon.Hej.type.ValidationFailure validateContent(Saxon.Hej.str.UnicodeString value, Saxon.Hej.om.NamespaceResolver nsResolver, Saxon.Hej.lib.ConversionRules rules) {
            Saxon.Hej.type.ValidationFailure result = null;
            Saxon.Hej.type.SimpleType matchingType = null;
            foreach (Saxon.Eej.ee.schema.TypeReference @ref in getMemberTypeReferences()) {
                Saxon.Hej.type.SimpleType type = null;
                try {
                    type = (Saxon.Hej.type.SimpleType)(@ref.getTarget());
                } catch (Saxon.Hej.type.MissingComponentException e) {
                    result = new Saxon.Hej.type.ValidationFailure(("Cannot validate against union type because the schema is incomplete. "+e.Message));
                }
                if ((type != null)) {
                    Saxon.Hej.type.ValidationFailure err = type.validateContent(value, nsResolver, rules);
                    if ((err == null)) {
                        matchingType = type;
                        break ;
                    }
                }
            }
            if ((matchingType != null)) {
                Saxon.Hej.type.SchemaType @base = getBaseType();
                if ((@base is Saxon.Eej.ee.schema.UserUnionType)) {
                    try {
                        int action = matchingType.getWhitespaceAction();
                        value = Saxon.Hej.value.Whitespace.applyWhitespaceNormalization(action, value);
                        Saxon.Hej.om.AtomicSequence typedValue = matchingType.getTypedValue(value, nsResolver, rules);
                        result = checkAgainstFacets(typedValue, value, getExtendedFacetList(), rules);
                    } catch (Saxon.Hej.trans.XPathException e) {
                        result = new Saxon.Hej.type.ValidationFailure(((("Value "+Saxon.Hej.trans.Err.wrap(value, Saxon.Hej.trans.Err.VALUE))+" does not match base type of union type ")+getDescription()));
                    }
                } else {
                    result = null;
                }
            } else {
                if ((result == null)) {
                    result = new Saxon.Hej.type.ValidationFailure(((("Value "+Saxon.Hej.trans.Err.wrap(value, Saxon.Hej.trans.Err.VALUE))+" does not match any member of union type ")+getDescription()));
                }
            }
            return result;
        }
        public Saxon.Hej.type.ValidationFailure checkAgainstFacets(Saxon.Hej.value.AtomicValue value, Saxon.Hej.lib.ConversionRules rules) {
            return checkAgainstFacets(value, value.getCanonicalLexicalRepresentation(), getExtendedFacetList(), rules);
        }
        private Saxon.Hej.type.SimpleType getMatchingMemberType(Saxon.Hej.str.UnicodeString value, Saxon.Hej.om.NamespaceResolver resolver, Saxon.Hej.lib.ConversionRules rules) {
            foreach (Saxon.Eej.ee.schema.TypeReference @ref in getMemberTypeReferences()) {
                try {
                    Saxon.Hej.type.SimpleType type = (Saxon.Hej.type.SimpleType)(@ref.getTarget());
                    Saxon.Hej.type.ValidationFailure err = type.validateContent(value, resolver, rules);
                    if ((err == null)) {
                        return type;
                    }
                } catch (Saxon.Hej.type.MissingComponentException e) {
                }
            }
            return null;
        }
        public override Saxon.Hej.om.AtomicSequence getTypedValue(Saxon.Hej.str.UnicodeString value, Saxon.Hej.om.NamespaceResolver resolver, Saxon.Hej.lib.ConversionRules rules) {
            Saxon.Hej.om.AtomicSequence result = null;
            foreach (Saxon.Eej.ee.schema.TypeReference @ref in getMemberTypeReferences()) {
                Saxon.Hej.type.SimpleType type = null;
                try {
                    type = (Saxon.Hej.type.SimpleType)(@ref.getTarget());
                } catch (Saxon.Hej.type.MissingComponentException e) {
                    Saxon.Hej.type.ValidationFailure ve = new Saxon.Hej.type.ValidationFailure(("Cannot validate against union type because the schema is incomplete. "+e.Message));
                    ve.setSchemaType(this);
                    throw ve.makeException();
                }
                try {
                    Saxon.Hej.type.ValidationFailure err = type.validateContent(value, resolver, rules);
                    if ((err == null)) {
                        result = type.getTypedValue(value, resolver, rules);
                        break ;
                    }
                } catch (Saxon.Hej.type.ValidationException err) {
                }
            }
            if ((result != null)) {
                Saxon.Hej.type.SchemaType @base = getBaseType();
                if ((@base is Saxon.Eej.ee.schema.UserUnionType)) {
                    Saxon.Hej.om.AtomicSequence typedValue = ((Saxon.Eej.ee.schema.UserUnionType)(@base)).getTypedValue(value, resolver, rules);
                    Saxon.Hej.type.ValidationFailure failure = checkAgainstFacets(typedValue, value, getExtendedFacetList(), rules);
                    if ((failure != null)) {
                        throw failure.makeException();
                    }
                }
                return result;
            } else {
                Saxon.Hej.type.ValidationFailure ve = new Saxon.Hej.type.ValidationFailure(((("Value "+Saxon.Hej.trans.Err.wrap(value, Saxon.Hej.trans.Err.VALUE))+" does not match any member of union type ")+getDescription()));
                ve.setSchemaType(this);
                throw ve.makeException();
            }
        }
        protected internal override void serializeVariety(Saxon.Eej.ee.schema.SchemaModelSerializer serializer) {
            serializer.emitAttribute("variety", "union");
            Saxon.Ejava.lang.StringBuilder sb = new Saxon.Ejava.lang.StringBuilder(64);
            foreach (Saxon.Eej.ee.schema.TypeReference @ref in getMemberTypeReferences()) {
                Saxon.Hej.type.SimpleType type = (Saxon.Hej.type.SimpleType)(@ref.getTarget());
                if ((sb.length() != 0)) {
                    sb.append(" ");
                }
                sb.append(serializer.getTypeLink(type));
            }
            serializer.emitAttribute("memberTypes", sb.ToString());
        }
        public bool matches(Saxon.Hej.om.Item item, Saxon.Hej.type.TypeHierarchy th) {
            if (!(((item is Saxon.Hej.value.AtomicValue)))) {
                return false;
            }
            foreach (Saxon.Eej.ee.schema.TypeReference @ref in getMemberTypeReferences()) {
                try {
                    Saxon.Hej.type.PlainType pt = (Saxon.Hej.type.PlainType)(@ref.getTarget());
                    if (pt.matches(item, th)) {
                        return true;
                    }
                } catch (Saxon.Hej.type.MissingComponentException e) {
                    return false;
                }
            }
            return false;
        }
        public Saxon.Hej.type.ItemType getPrimitiveItemType() {
            return Saxon.Hej.type.BuiltInAtomicType.ANY_ATOMIC;
        }
        public int getPrimitiveType() {
            return Saxon.Hej.om.StandardNames.XS_ANY_ATOMIC_TYPE;
        }
        public Saxon.Hej.type.PlainType getAtomizedItemType() {
            return this;
        }
        public bool isAtomizable(Saxon.Hej.type.TypeHierarchy th) {
            return true;
        }
        public System.Collections.Generic.IList<Saxon.Hej.type.PlainType> getPlainMemberTypes() {
            System.Collections.Generic.IList<Saxon.Hej.type.PlainType> s = new System.Collections.Generic.List<Saxon.Hej.type.PlainType>(10);
            gatherPlainMemberTypes(s);
            return s;
        }
        private void gatherPlainMemberTypes(System.Collections.Generic.IList<Saxon.Hej.type.PlainType> s) {
            foreach (Saxon.Eej.ee.schema.TypeReference @ref in getMemberTypeReferences()) {
                Saxon.Hej.type.SimpleType pt = (Saxon.Hej.type.SimpleType)(@ref.getTarget());
                if ((pt is Saxon.Hej.type.PlainType)) {
                    s.Add((Saxon.Hej.type.PlainType)(pt));
                }
                if ((pt is Saxon.Eej.ee.schema.UserUnionType)) {
                    ((Saxon.Eej.ee.schema.UserUnionType)(pt)).gatherPlainMemberTypes(s);
                }
            }
        }
        public double getDefaultPriority() {
            try {
                double result = 1;
                foreach (Saxon.Hej.type.PlainType t in getPlainMemberTypes()) {
                    result *= t.getDefaultPriority();
                }
                return result;
            } catch (Saxon.Hej.type.MissingComponentException e) {
                return -1;
            }
        }
        public override string ToString() {
            if (isAnonymousType()) {
                Saxon.Ejava.lang.StringBuilder fsb = new Saxon.Ejava.lang.StringBuilder(256);
                fsb.append("union(");
                foreach (Saxon.Eej.ee.schema.TypeReference @ref in getMemberTypeReferences()) {
                    string member = @ref.getTargetComponentName().getDisplayName();
                    fsb.append(member);
                    fsb.append(", ");
                }
                fsb.setLength((fsb.length()-2));
                fsb.append(")");
                return fsb.ToString();
            } else {
                return getDisplayName();
            }
        }
        public string toExportString() {
            if (isAnonymousType()) {
                Saxon.Ejava.lang.StringBuilder fsb = new Saxon.Ejava.lang.StringBuilder(256);
                fsb.append("union(");
                foreach (Saxon.Eej.ee.schema.TypeReference @ref in getMemberTypeReferences()) {
                    try {
                        Saxon.Hej.type.PlainType member = (Saxon.Hej.type.PlainType)(@ref.getTarget());
                        fsb.append(member.toExportString());
                    } catch (Saxon.Hej.type.MissingComponentException err) {
                        fsb.append("xs:anyAtomicType");
                    }
                    fsb.append(", ");
                }
                fsb.setLength((fsb.length()-2));
                fsb.append(")");
                return fsb.ToString();
            } else {
                return getEQName();
            }
        }
        public override string getDescription() {
            if (isAnonymousType()) {
                return ToString();
            } else {
                return base.getDescription();
            }
        }
    
        /* Expanding default method getNormalizedDefaultPriority from interface ItemType */
        public virtual double getNormalizedDefaultPriority() {
            return (((getDefaultPriority()+1))/2);
        }

        /* Expanding default method getFullAlphaCode from interface ItemType */
        public virtual string getFullAlphaCode() {
            return getBasicAlphaCode();
        }

        /* Expanding default method explainMismatch from interface UnionType */
        public virtual Saxon.Ejava.util.Optional<string> explainMismatch(Saxon.Hej.om.Item item, Saxon.Hej.type.TypeHierarchy th) {
            if ((item.getGenre() == /*explicit enum*/ Saxon.Hej.om.Genre.ATOMIC)) {
                Saxon.Ejava.lang.StringBuilder message = new Saxon.Ejava.lang.StringBuilder(256);
                message.append("The required type is a union type allowing any of ");
                string punctuation = "(";
                try {
                    foreach (Saxon.Hej.type.PlainType member in getPlainMemberTypes()) {
                        message.append(punctuation);
                        punctuation = ", ";
                        message.append(member.getTypeName().getDisplayName());
                    }
                } catch (Saxon.Hej.type.MissingComponentException e) {
                    message.append("*member types unobtainable*");
                }
                message.append("), but the supplied type ");
                message.append(((Saxon.Hej.value.AtomicValue)(item)).getItemType().getDisplayName());
                message.append(" is not any of these");
                return Saxon.Ejava.util.Optional<string>.of(message.ToString());
            } else {
                return Saxon.Ejava.util.Optional<string>.empty();
            }
        }

    }
}