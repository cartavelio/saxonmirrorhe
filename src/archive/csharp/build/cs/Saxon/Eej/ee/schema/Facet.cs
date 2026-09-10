using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.ee.schema {
    public abstract class Facet {
        private bool @fixed = false;
        private string message = null;
        public void setFixed(bool @fixed) {
            this.@fixed = @fixed;
        }
        public bool isFixed() {
            return @fixed;
        }
        public virtual bool isConstraining() {
            return true;
        }
        public void setMessage(string message) {
            this.message = message;
        }
        public string getMessage() {
            return message;
        }
        public abstract string getName();
        public virtual string getConstraintName() {
            return (("cvc-"+getName())+"-valid");
        }
        public abstract Saxon.Hej.str.UnicodeString getValue();
        public virtual long toLong() {
            return System.Int64.Parse(getValue().ToString());
        }
        public virtual void checkFacetRestriction(Saxon.Eej.ee.schema.UserSimpleType type, Saxon.Hej.type.SimpleType @base, Saxon.Eej.ee.schema.SchemaCompiler compiler) {
        }
        public bool isNewlyIntroduced(Saxon.Hej.type.SimpleType @base) {
            System.Type thisClass = this.GetType();
            if ((@base is Saxon.Eej.ee.schema.UserSimpleType)) {
                Saxon.Eej.ee.schema.Facet f = ((Saxon.Eej.ee.schema.UserSimpleType)(@base)).getFacet(thisClass);
                return ((f == null) || !(getValue().Equals(f.getValue())));
            }
            return true;
        }
        public abstract bool testAtomicValue(Saxon.Hej.value.AtomicValue value);
        public virtual bool testListValue(Saxon.Hej.om.AtomicSequence value) {
            throw new Saxon.Ejava.lang.UnsupportedOperationException((("The "+getName())+" facet is not applicable to list values"));
        }
        public virtual bool testLength(int count) {
            return true;
        }
        public virtual int getWhitespaceAction() {
            return Saxon.Hej.value.Whitespace.PRESERVE;
        }
        public virtual void serializeFacet(Saxon.Eej.ee.schema.SchemaModelSerializer serializer) {
            serializer.startElement(getName());
            if ((getValue() == null)) {
                throw new Saxon.Ejava.lang.NullPointerException((("*** Facet "+getName())+" has no value"));
            }
            serializer.emitAttribute("value", getValue().ToString());
            if (isFixed()) {
                serializer.emitAttribute("fixed", "true");
            }
            if ((message != null)) {
                serializer.emitAttribute("message", message);
            }
            if ((this is Saxon.Eej.ee.schema.PatternFacet)) {
                string flags = ((Saxon.Eej.ee.schema.PatternFacet)(this)).getFlags();
                if (((flags != null) && !(flags.Length==0))) {
                    serializer.emitAttribute("flags", flags);
                }
            }
            serializer.endElement();
        }
        public static bool isFacetName(string name, int xsdVersion) {
            return ((((((((((((((((("assertion".Equals(name) && (xsdVersion == Saxon.Hej.Configuration.XSD11))) || "enumeration".Equals(name)) || "length".Equals(name)) || "pattern".Equals(name)) || "maxExclusive".Equals(name)) || "minExclusive".Equals(name)) || "maxInclusive".Equals(name)) || "minInclusive".Equals(name)) || "maxLength".Equals(name)) || "minLength".Equals(name)) || "maxScale".Equals(name)) || "minScale".Equals(name)) || "whiteSpace".Equals(name)) || "totalDigits".Equals(name)) || "fractionDigits".Equals(name)) || (("explicitTimezone".Equals(name) && (xsdVersion == Saxon.Hej.Configuration.XSD11))));
        }
        public abstract Saxon.Hej.om.FunctionItem getFacetAsFunctionItem();
        public static Saxon.Eej.ee.schema.Facet makeFacet(string name, string value, Saxon.Eej.config.EnterpriseConfiguration config, Saxon.Hej.om.NamespaceResolver resolver) {
            switch (name) {
              case "pattern":
                {
                    Saxon.Eej.ee.schema.PatternFacet lf = new Saxon.Eej.ee.schema.PatternFacet(config);
                    lf.setValue(value);
                    return lf;
                }
              case "enumeration":
                {
                    Saxon.Eej.ee.schema.EnumerationFacet ef = new Saxon.Eej.ee.schema.EnumerationFacet();
                    ef.setValue(value);
                    ef.setNamespaceContext(resolver);
                    return ef;
                }
              case "length":
                {
                    Saxon.Eej.ee.schema.LengthFacet lf = new Saxon.Eej.ee.schema.LengthFacet();
                    lf.setValue(value);
                    return lf;
                }
              case "minLength":
                {
                    Saxon.Eej.ee.schema.MinLengthFacet lf = new Saxon.Eej.ee.schema.MinLengthFacet();
                    lf.setValue(value);
                    return lf;
                }
              case "maxLength":
                {
                    Saxon.Eej.ee.schema.MaxLengthFacet lf = new Saxon.Eej.ee.schema.MaxLengthFacet();
                    lf.setValue(value);
                    return lf;
                }
              case "minInclusive":
                {
                    Saxon.Eej.ee.schema.ValueRangeFacet vrf = new Saxon.Eej.ee.schema.MinInclusiveFacet();
                    vrf.setValue(value);
                    return vrf;
                }
              case "maxInclusive":
                {
                    Saxon.Eej.ee.schema.ValueRangeFacet vrf = new Saxon.Eej.ee.schema.MaxInclusiveFacet();
                    vrf.setValue(value);
                    return vrf;
                }
              case "minExclusive":
                {
                    Saxon.Eej.ee.schema.ValueRangeFacet vrf = new Saxon.Eej.ee.schema.MinExclusiveFacet();
                    vrf.setValue(value);
                    return vrf;
                }
              case "maxExclusive":
                {
                    Saxon.Eej.ee.schema.ValueRangeFacet vrf = new Saxon.Eej.ee.schema.MaxExclusiveFacet();
                    vrf.setValue(value);
                    return vrf;
                }
              case "totalDigits":
                {
                    Saxon.Eej.ee.schema.DigitsFacet df = new Saxon.Eej.ee.schema.TotalDigitsFacet();
                    df.setValue(value);
                    return df;
                }
              case "fractionDigits":
                {
                    Saxon.Eej.ee.schema.DigitsFacet df = new Saxon.Eej.ee.schema.FractionDigitsFacet();
                    df.setValue(value);
                    return df;
                }
              case "explicitTimezone":
                {
                    Saxon.Eej.ee.schema.ExplicitTimezoneFacet tf = new Saxon.Eej.ee.schema.ExplicitTimezoneFacet();
                    tf.setValue(value);
                    return tf;
                }
              case "whiteSpace":
                {
                    Saxon.Eej.ee.schema.WhitespaceFacet wf = new Saxon.Eej.ee.schema.WhitespaceFacet();
                    wf.setValue(value);
                    return wf;
                }
              case "order":
                {
                    Saxon.Eej.ee.schema.OrderFacet of = new Saxon.Eej.ee.schema.OrderFacet();
                    of.setValue(value);
                    return of;
                }
              case "distinct":
                {
                    Saxon.Eej.ee.schema.DistinctFacet df = new Saxon.Eej.ee.schema.DistinctFacet();
                    df.setValue(value);
                    return df;
                }
              default:
                throw new Saxon.Hej.type.SchemaException(("Unknown facet: "+name));
            }
        }
    
    }
}