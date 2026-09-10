using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.ee.schema {
    public class UserListType : Saxon.Eej.ee.schema.UserSimpleType, Saxon.Hej.type.ListType {
        private Saxon.Eej.ee.schema.TypeReference itemType = null;
        public UserListType(Saxon.Eej.config.EnterpriseConfiguration config)  {
            setConfiguration(config);
        }
        public override bool isAtomicType() {
            return false;
        }
        public override bool isIdType() {
            return getItemType().isIdType();
        }
        public override bool isIdRefType() {
            return getItemType().isIdRefType();
        }
        public override bool isBuiltInType() {
            return false;
        }
        public override bool isListType() {
            return true;
        }
        private System.Func<Saxon.Hej.str.UnicodeString,Saxon.Hej.tree.iter.AtomicIterator> tokenizerFactory = str => new Saxon.Hej.value.Whitespace.Tokenizer(str);
        protected internal override System.Collections.Generic.IList<Saxon.Eej.ee.schema.Facet> addInheritedFacets(System.Collections.Generic.IList<Saxon.Eej.ee.schema.Facet> localFacets) {
            localFacets = base.addInheritedFacets(localFacets);
            for (int i = 0; (i<localFacets.Count); i++) {
                Saxon.Eej.ee.schema.Facet f = localFacets[i];
                if (((f is Saxon.Eej.ee.schema.LengthFacet) || (f is Saxon.Eej.ee.schema.MinLengthFacet))) {
                    return localFacets;
                }
            }
            Saxon.Hej.type.SchemaType @base = this;
            while ((@base != null)) {
                @base = @base.getBaseType();
                if ((@base is Saxon.Hej.type.BuiltInListType)) {
                    System.Collections.Generic.IList<Saxon.Eej.ee.schema.Facet> extendedFacets = new System.Collections.Generic.List<Saxon.Eej.ee.schema.Facet>(localFacets);
                    Saxon.Eej.ee.schema.MinLengthFacet minLength = new Saxon.Eej.ee.schema.MinLengthFacet();
                    minLength.setNumericValue(1);
                    extendedFacets.Add(minLength);
                    return extendedFacets;
                }
            }
            return localFacets;
        }
        public override void elaborate(Saxon.Eej.ee.schema.SchemaCompiler compiler) {
            Saxon.Hej.type.SimpleType st = (Saxon.Hej.type.SimpleType)(itemType.getTarget());
            if ((st is Saxon.Eej.ee.schema.UserSimpleType)) {
                ((Saxon.Eej.ee.schema.UserSimpleType)(st)).elaborate(compiler);
            }
            base.elaborate(compiler);
        }
        public Saxon.Eej.ee.schema.TypeReference getItemTypeReference() {
            return itemType;
        }
        public Saxon.Hej.type.SimpleType getItemType() {
            return (Saxon.Hej.type.SimpleType)(itemType.getTarget());
        }
        public void setItemTypeReference(Saxon.Eej.ee.schema.TypeReference type) {
            itemType = type;
        }
        public void setTokenSeparator(Saxon.Hej.regex.RegularExpression regex) {
            if ((regex != null)) {
                tokenizerFactory = value => regex.tokenize(value);
            }
        }
        public void inheritSeparator(Saxon.Eej.ee.schema.UserListType @base) {
            tokenizerFactory = @base.tokenizerFactory;
        }
        public override Saxon.Hej.type.ValidationFailure validateContent(Saxon.Hej.str.UnicodeString value, Saxon.Hej.om.NamespaceResolver nsResolver, Saxon.Hej.lib.ConversionRules rules) {
            if (value.isEmpty()) {
                return checkAgainstListFacets(value, nsResolver, rules, 0);
            } else {
                Saxon.Hej.type.SimpleType itemType = null;
                try {
                    itemType = getItemType();
                } catch (Saxon.Hej.type.MissingComponentException e) {
                    return new Saxon.Hej.type.ValidationFailure(e.Message);
                }
                Saxon.Hej.tree.iter.AtomicIterator iter = tokenizerFactory(value);
                int count = 0;
                Saxon.Hej.value.StringValue val;
                while (((val = (Saxon.Hej.value.StringValue)(((/*1430*/Saxon.Hej.value.AtomicValue)iter.next()))) != null)) {
                    Saxon.Hej.type.ValidationFailure err = itemType.validateContent(val.getUnicodeStringValue(), nsResolver, rules);
                    if ((err != null)) {
                        return err;
                    }
                    count++;
                }
                return checkAgainstListFacets(value, nsResolver, rules, count);
            }
        }
        public Saxon.Hej.type.ValidationFailure checkAgainstListFacets(Saxon.Hej.str.UnicodeString value, Saxon.Hej.om.NamespaceResolver nsResolver, Saxon.Hej.lib.ConversionRules rules, int count) {
            Saxon.Hej.om.AtomicSequence typedValue = null;
            foreach (Saxon.Eej.ee.schema.Facet f in getExtendedFacetList()) {
                if ((f is Saxon.Eej.ee.schema.PatternFacet)) {
                    bool match = f.testAtomicValue(new Saxon.Hej.value.StringValue(Saxon.Hej.value.Whitespace.collapseWhitespace(value)));
                    if (!(match)) {
                        Saxon.Hej.type.ValidationFailure vf = new Saxon.Hej.type.ValidationFailure(((("List "+Saxon.Hej.trans.Err.wrap(value))+" contravenes the pattern facet ")+Saxon.Hej.trans.Err.wrap(f.getValue())));
                        vf.setConstraintReference(2, "cvc-pattern-valid", "1");
                        return vf;
                    }
                } else {
                    if ((f is Saxon.Eej.ee.schema.EnumerationFacetSet)) {
                        try {
                            if ((typedValue == null)) {
                                typedValue = ((Saxon.Hej.type.SimpleType)(getBaseType())).getTypedValue(value, nsResolver, rules);
                            }
                            bool match = f.testListValue(typedValue);
                            if (!(match)) {
                                string message = ((("List "+Saxon.Hej.trans.Err.wrap(value))+" does not match any of the values in the enumeration facet ")+Saxon.Hej.trans.Err.wrap(f.getValue()));
                                if ((f.getMessage() != null)) {
                                    message += (". "+f.getMessage());
                                }
                                Saxon.Hej.type.ValidationFailure vf = new Saxon.Hej.type.ValidationFailure(message);
                                vf.setConstraintReference(2, "cvc-enumeration-valid", "1");
                                return vf;
                            }
                        } catch (Saxon.Hej.trans.XPathException e) {
                            Saxon.Hej.type.ValidationFailure vf = new Saxon.Hej.type.ValidationFailure(((((("List "+Saxon.Hej.trans.Err.wrap(value))+" cannot be compared against the enumeration facet ")+Saxon.Hej.trans.Err.wrap(f.getValue()))+": ")+e.Message));
                            vf.setConstraintReference(2, "cvc-enumeration-valid", "1");
                            return vf;
                        }
                    } else {
                        if (((f is Saxon.Eej.ee.schema.DistinctFacet) || (f is Saxon.Eej.ee.schema.OrderFacet))) {
                            try {
                                if ((typedValue == null)) {
                                    typedValue = ((Saxon.Hej.type.SimpleType)(getBaseType())).getTypedValue(value, nsResolver, rules);
                                }
                            } catch (Saxon.Hej.type.ValidationException e) {
                                Saxon.Hej.type.ValidationFailure vf = new Saxon.Hej.type.ValidationFailure(((((((("List "+Saxon.Hej.trans.Err.wrap(value))+" cannot be compared against the ")+f.getName())+" facet ")+Saxon.Hej.trans.Err.wrap(f.getValue()))+": ")+e.Message));
                                vf.setConstraintReference(2, "cvc-facet-valid", "1");
                                return vf;
                            }
                            bool match = f.testListValue(typedValue);
                            if (!(match)) {
                                Saxon.Hej.type.ValidationFailure vf = new Saxon.Hej.type.ValidationFailure((((("List "+Saxon.Hej.trans.Err.wrap(value))+" does not satisfy the saxon:")+f.getName())+" facet"));
                                vf.setConstraintReference(2, "cvc-facet-valid", "1");
                                return vf;
                            }
                        } else {
                            if ((f is Saxon.Eej.ee.schema.AssertionFacet)) {
                                try {
                                    if ((typedValue == null)) {
                                        typedValue = ((Saxon.Hej.type.SimpleType)(getBaseType())).getTypedValue(value, nsResolver, rules);
                                    }
                                    bool match = ((Saxon.Eej.ee.schema.AssertionFacet)(f)).testListValue(typedValue, rules);
                                    if (!(match)) {
                                        string message = f.getMessage();
                                        if ((message == null)) {
                                            message = ((("List "+Saxon.Hej.trans.Err.wrap(value))+" does not satisfy the assertion ")+Saxon.Hej.trans.Err.wrap(f.getValue()));
                                        }
                                        Saxon.Hej.type.ValidationFailure vf = new Saxon.Hej.type.ValidationFailure(message);
                                        vf.setConstraintReference(2, "cvc-assertions-valid", "1");
                                        return vf;
                                    }
                                } catch (Saxon.Hej.trans.XPathException e) {
                                    Saxon.Hej.type.ValidationFailure vf = new Saxon.Hej.type.ValidationFailure(((((("List "+Saxon.Hej.trans.Err.wrap(value))+" cannot be compared against the assertion facet ")+Saxon.Hej.trans.Err.wrap(f.getValue()))+": ")+e.Message));
                                    vf.setConstraintReference(2, "cvc-assertions-valid", "1");
                                    return vf;
                                }
                            }
                        }
                    }
                }
                if (!(f.testLength(count))) {
                    Saxon.Hej.type.ValidationFailure vf = new Saxon.Hej.type.ValidationFailure(((((("Length of list "+Saxon.Hej.trans.Err.wrap((count+"")))+" contravenes the ")+f.getName())+" facet ")+Saxon.Hej.trans.Err.wrap(f.getValue())));
                    vf.setConstraintReference(2, f.getConstraintName(), "1");
                    return vf;
                }
            }
            return null;
        }
        public override Saxon.Hej.om.AtomicSequence getTypedValue(Saxon.Hej.str.UnicodeString value, Saxon.Hej.om.NamespaceResolver resolver, Saxon.Hej.lib.ConversionRules rules) {
            if (value.isEmpty()) {
                return Saxon.Hej.om.EmptyAtomicSequence.getInstance();
            }
            Saxon.Hej.tree.iter.AtomicIterator iter = tokenizerFactory(value);
            Saxon.Hej.type.SimpleType theItemType = getItemType();
            try {
                return new Saxon.Hej.om.AtomicArray(Saxon.Hej.expr.MappingIterator.map(iter, item => theItemType.getTypedValue(item.getUnicodeStringValue(), resolver, rules).iterate()));
            } catch (Saxon.Hej.trans.XPathException err) {
                throw new Saxon.Hej.type.ValidationException(err);
            }
        }
        public override bool isNamespaceSensitive() {
            try {
                return getItemType().isNamespaceSensitive();
            } catch (Saxon.Hej.type.MissingComponentException e) {
                return true;
            }
        }
        protected internal override void serializeVariety(Saxon.Eej.ee.schema.SchemaModelSerializer serializer) {
            serializer.emitAttribute("variety", "list");
            serializer.emitAttribute("itemType", serializer.getTypeLink(getItemType()));
        }
        public override string ToString() {
            return getTypeName().getEQName();
        }
    
    }
}