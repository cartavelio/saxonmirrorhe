using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.ee.schema {
    public class ValueConstraint {
        public class  Variety  {
            public static readonly Variety DEFAULT = new Variety("DEFAULT", InnerEnum.DEFAULT);
            public static readonly Variety FIXED = new Variety("FIXED", InnerEnum.FIXED);
            private static readonly List<Variety> valueList = new List<Variety>();
            static Variety() {
                valueList.Add(DEFAULT);
                valueList.Add(FIXED);
            }
            public static IList<Variety> values() {
                return valueList;
            }
            public enum InnerEnum {
                DEFAULT,
                FIXED,
            }
            public readonly InnerEnum innerEnumValue;
            public readonly string nameValue;
            public readonly int ordinalValue;
            public static int nextOrdinal = 0;
            internal Variety(string name, InnerEnum innerEnum) { 
                nameValue = name;
                ordinalValue = nextOrdinal++;
                innerEnumValue = innerEnum;
            } 
        }
        private readonly Saxon.Eej.ee.schema.ValueConstraint.Variety variety;
        private Saxon.Hej.str.UnicodeString lexicalForm;
        private Saxon.Hej.om.AtomicSequence typedValue;
        private int comparisonHashCode;
        public ValueConstraint(Saxon.Eej.ee.schema.ValueConstraint.Variety variety, Saxon.Hej.str.UnicodeString lexicalForm)  {
            this.variety = variety;
            this.lexicalForm = lexicalForm;
            if ((lexicalForm != null)) {
                this.typedValue = new Saxon.Hej.value.StringValue(lexicalForm);
            }
        }
        public Saxon.Eej.ee.schema.ValueConstraint.Variety getVariety() {
            return variety;
        }
        public Saxon.Hej.str.UnicodeString getLexicalForm() {
            return lexicalForm;
        }
        internal void setLexicalForm(Saxon.Hej.str.UnicodeString value) {
            lexicalForm = value;
        }
        public Saxon.Hej.om.AtomicSequence getValue() {
            return typedValue;
        }
        internal void setValue(Saxon.Hej.om.AtomicSequence value) {
            this.typedValue = value;
            this.comparisonHashCode = Saxon.Hej.expr.sort.SimpleTypeComparison.getInstance().hash(value);
        }
        public bool matches(Saxon.Hej.om.AtomicSequence supplied) {
            Saxon.Hej.expr.sort.SimpleTypeComparison comp = Saxon.Hej.expr.sort.SimpleTypeComparison.getInstance();
            return ((comp.hash(supplied) == this.comparisonHashCode) && comp.equal(this.typedValue, supplied));
        }
        public Saxon.Hej.om.Sequence asFunction() {
            Saxon.Hej.expr.CallableDelegate constraintFunction = new Saxon.Hej.expr.CallableDelegate((context, arguments) => {
                string key = arguments[0].head().getStringValue();
                switch (key) {
                  case "class":
                    return Saxon.Hej.value.StringValue.bmp("Value Constraint");
                  case "variety":
                    return Saxon.Hej.value.StringValue.bmp(((variety == /*explicit enum*/ Saxon.Eej.ee.schema.ValueConstraint.Variety.FIXED) ? "fixed" : "default"));
                  case "value":
                    return ((getValue() == null) ? new Saxon.Hej.value.StringValue(getLexicalForm()) : getValue());
                  case "lexical form":
                    return new Saxon.Hej.value.StringValue(getLexicalForm());
                  default:
                    return Saxon.Hej.value.EmptySequence.getInstance();
                }
            } );
            return new Saxon.Hej.functions.CallableFunction(1, constraintFunction, Saxon.Hej.type.SpecificFunctionType.COMPONENT_FUNCTION_TYPE);
        }
        internal bool makeTypedValue(Saxon.Eej.ee.schema.SchemaCompiler compiler, Saxon.Hej.type.SimpleType stype, Saxon.Hej.om.NamespaceResolver resolver, Saxon.Hej.s9api.Location loc) {
            lexicalForm = Saxon.Hej.value.Whitespace.applyWhitespaceNormalization(stype.getWhitespaceAction(), lexicalForm);
            Saxon.Hej.lib.ConversionRules rules = compiler.getConfiguration().getConversionRules();
            Saxon.Hej.type.ValidationFailure err = stype.validateContent(lexicalForm, resolver, rules);
            if ((err != null)) {
                string val = Saxon.Hej.trans.Err.wrap(lexicalForm, Saxon.Hej.trans.Err.VALUE);
                compiler.error((((((((variety == /*explicit enum*/ Saxon.Eej.ee.schema.ValueConstraint.Variety.FIXED) ? "Fixed" : "Default"))+" value ")+Saxon.Hej.trans.Err.wrap(val))+" does not not conform to the declared type: ")+err.getMessage()), loc);
                return false;
            }
            try {
                if (stype.isNamespaceSensitive()) {
                    setValue(stype.getTypedValue(lexicalForm, resolver, rules));
                } else {
                    if (stype.isAtomicType()) {
                        setValue(Saxon.Hej.type.Converter.convert(new Saxon.Hej.value.StringValue(lexicalForm), (Saxon.Hej.type.AtomicType)(stype), rules));
                    } else {
                        if ((stype is Saxon.Hej.type.AnySimpleType)) {
                            setValue(new Saxon.Hej.value.StringValue(lexicalForm));
                        } else {
                            if ((getValue().getLength() == 1)) {
                                setValue(stype.getTypedValue(lexicalForm, null, rules));
                            }
                        }
                    }
                }
                return true;
            } catch (Saxon.Hej.trans.XPathException e) {
                compiler.error(e.Message, loc);
                return false;
            }
        }
        public bool testFixedValue(Saxon.Hej.Configuration config, Saxon.Hej.str.UnicodeString value, Saxon.Hej.type.SimpleType type, Saxon.Hej.om.NamespaceResolver resolver) {
            if ((variety != /*explicit enum*/ Saxon.Eej.ee.schema.ValueConstraint.Variety.FIXED)) {
                return true;
            }
            Saxon.Hej.om.AtomicSequence actual = type.getTypedValue(value, resolver, config.getConversionRules());
            Saxon.Hej.expr.sort.SimpleTypeComparison comp = Saxon.Hej.expr.sort.SimpleTypeComparison.getInstance();
            return ((comp.hash(actual) == comparisonHashCode) && comp.equal(actual, getValue()));
        }
        public void serialize(Saxon.Eej.ee.schema.SchemaModelSerializer serializer) {
            serializer.startElement(((variety == /*explicit enum*/ Saxon.Eej.ee.schema.ValueConstraint.Variety.FIXED) ? "fixed" : "default"));
            serializer.emitAttribute("lexicalForm", getLexicalForm().ToString());
            serializer.emitTypedValue(getValue());
            serializer.endElement();
        }
    
    }
}