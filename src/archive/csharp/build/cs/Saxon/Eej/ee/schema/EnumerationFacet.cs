using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.ee.schema {
    public class EnumerationFacet : Saxon.Eej.ee.schema.Facet {
        private Saxon.Hej.str.UnicodeString value;
        private Saxon.Hej.om.AtomicSequence typedValue;
        internal Saxon.Hej.om.NamespaceResolver namespaceContext = null;
        public override string getName() {
            return "enumeration";
        }
        public override Saxon.Hej.str.UnicodeString getValue() {
            return value;
        }
        public Saxon.Hej.om.NamespaceResolver getNamespaceContext() {
            return namespaceContext;
        }
        public void setValue(string value) {
            this.value = Saxon.Hej.str.StringView.of(value).tidy();
        }
        public Saxon.Hej.om.AtomicSequence getTypedValue(Saxon.Hej.type.SimpleType type, Saxon.Hej.lib.ConversionRules rules) {
            if ((typedValue != null)) {
                return typedValue;
            }
            try {
                typedValue = type.getTypedValue(getValue(), namespaceContext, rules);
                return typedValue;
            } catch (Saxon.Hej.trans.XPathException e) {
                throw new Saxon.Hej.type.SchemaException(e);
            }
        }
        public override void checkFacetRestriction(Saxon.Eej.ee.schema.UserSimpleType type, Saxon.Hej.type.SimpleType @base, Saxon.Eej.ee.schema.SchemaCompiler compiler) {
            Saxon.Hej.lib.ConversionRules rules = type.getConfiguration().getConversionRules();
            rules = rules.copy();
            rules.setNotationSet(compiler.getPreparedSchema());
            if ((@base is Saxon.Eej.ee.schema.UserAtomicType)) {
                getTypedValue(@base, rules);
            } else {
                if (((namespaceContext == null) && @base.isNamespaceSensitive())) {
                    throw new Saxon.Hej.type.SchemaException("No namespace context available for checking enumeration QName");
                }
                Saxon.Hej.type.ValidationFailure err = @base.validateContent((getValue()), namespaceContext, rules);
                if ((err != null)) {
                    throw new Saxon.Hej.type.SchemaException(((((("Enumeration value "+Saxon.Hej.trans.Err.wrap(value))+" is not permitted by the base type ")+@base.getDescription())+": ")+err.getMessage()));
                }
            }
        }
        public void setNamespaceContext(Saxon.Hej.om.NamespaceResolver map) {
            namespaceContext = map;
        }
        public override bool testAtomicValue(Saxon.Hej.value.AtomicValue value) {
            return true;
        }
        public override Saxon.Hej.om.Function getFacetAsFunctionItem() {
            throw new Saxon.Ejava.lang.UnsupportedOperationException();
        }
    
    }
}