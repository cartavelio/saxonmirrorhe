using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.functions {
    public class OptionsParameter {
        private readonly System.Collections.Generic.IDictionary<string,Saxon.Hej.value.SequenceType> allowedOptions = new System.Collections.Generic.Dictionary<string, Saxon.Hej.value.SequenceType>(8);
        private readonly System.Collections.Generic.IDictionary<string,Saxon.Hej.om.Sequence> defaultValues = new System.Collections.Generic.Dictionary<string, Saxon.Hej.om.Sequence>(8);
        private readonly System.Collections.Generic.ISet<string> requiredOptions = new System.Collections.Generic.HashSet<string>(4);
        private readonly System.Collections.Generic.IDictionary<string,System.Collections.Generic.ISet<string>> allowedValues = new System.Collections.Generic.Dictionary<string, System.Collections.Generic.ISet<string>>(8);
        private string errorCodeForDisallowedValue;
        private string errorCodeForAbsentValue = "SXJE9999";
        private bool allowCastFromString = false;
        public OptionsParameter()  {
        }
        public void addAllowedOption(string name, Saxon.Hej.value.SequenceType type) {
            Saxon.Impl.Helpers.MapUtils.Put(allowedOptions, name, type);
        }
        public void addRequiredOption(string name, Saxon.Hej.value.SequenceType type) {
            Saxon.Impl.Helpers.MapUtils.Put(allowedOptions, name, type);
            requiredOptions.Add(name);
        }
        public void addAllowedOption(string name, Saxon.Hej.value.SequenceType type, Saxon.Hej.om.Sequence defaultValue) {
            Saxon.Impl.Helpers.MapUtils.Put(allowedOptions, name, type);
            if ((defaultValue != null)) {
                Saxon.Impl.Helpers.MapUtils.Put(defaultValues, name, defaultValue);
            }
        }
        public void setAllowedValues(string name, string errorCode, params string[]  values) {
            System.Collections.Generic.HashSet<string> valueSet = new System.Collections.Generic.HashSet<string>(Saxon.Ejava.util.Arrays.asList(values));
            Saxon.Impl.Helpers.MapUtils.Put(allowedValues, name, valueSet);
            errorCodeForDisallowedValue = errorCode;
        }
        public System.Collections.Generic.IDictionary<string,Saxon.Hej.om.Sequence> processSuppliedOptions(Saxon.Hej.ma.map.MapItem supplied, Saxon.Hej.expr.XPathContext context) {
            System.Collections.Generic.IDictionary<string,Saxon.Hej.om.Sequence> result = new System.Collections.Generic.Dictionary<string, Saxon.Hej.om.Sequence>();
            Saxon.Hej.type.TypeHierarchy th = context.getConfiguration().getTypeHierarchy();
            foreach (string req in requiredOptions) {
                if ((supplied.get(new Saxon.Hej.value.StringValue(req)) == null)) {
                    throw new Saxon.Hej.trans.XPathException(("No value supplied for required option: "+req), errorCodeForAbsentValue);
                }
            }
            foreach (System.Collections.Generic.KeyValuePair<string,Saxon.Hej.value.SequenceType> allowed in allowedOptions) {
                string nominalKey = allowed.Key;
                Saxon.Hej.value.AtomicValue actualKey;
                if (nominalKey.StartsWith("Q{")) {
                    actualKey = new Saxon.Hej.value.QNameValue(Saxon.Hej.om.StructuredQName.fromEQName((nominalKey)), Saxon.Hej.type.BuiltInAtomicType.QNAME);
                } else {
                    actualKey = new Saxon.Hej.value.StringValue(nominalKey);
                }
                Saxon.Hej.value.SequenceType required = allowed.Value;
                Saxon.Hej.om.GroundedValue actual = supplied.get(actualKey);
                if ((actual != null)) {
                    if (!(required.matches(actual, th))) {
                        bool ok = false;
                        if ((((actual is Saxon.Hej.value.StringValue) && allowCastFromString) && (required.getPrimaryType() is Saxon.Hej.type.AtomicType))) {
                            try {
                                Saxon.Hej.lib.ConversionRules rules = context.getConfiguration().getConversionRules();
                                actual = Saxon.Hej.type.Converter.convert((Saxon.Hej.value.StringValue)(actual), (Saxon.Hej.type.AtomicType)(required.getPrimaryType()), rules);
                                ok = true;
                            } catch (Saxon.Hej.trans.XPathException err) {
                                ok = false;
                            }
                        }
                        if (!(ok)) {
                            Saxon.Hej.expr.parser.RoleDiagnostic role = new Saxon.Hej.expr.parser.RoleDiagnostic(Saxon.Hej.expr.parser.RoleDiagnostic.OPTION, nominalKey, 0);
                            role.setErrorCode("XPTY0004");
                            actual = th.applyFunctionConversionRules(actual, required, role, Saxon.Hej.expr.parser.Loc.NONE);
                        }
                    }
                    actual = actual.materialize();
                    System.Collections.Generic.ISet<string> permitted = Saxon.Impl.Helpers.MapUtils.Get(allowedValues, nominalKey);
                    if ((permitted != null)) {
                        if ((!(((actual is Saxon.Hej.value.AtomicValue))) || !(permitted.Contains(((Saxon.Hej.value.AtomicValue)(actual)).getStringValue())))) {
                            Saxon.Ejava.lang.StringBuilder message = new Saxon.Ejava.lang.StringBuilder((((("Invalid option "+nominalKey)+"=")+Saxon.Hej.trans.Err.depictSequence(actual))+". Valid values are:"));
                            int i = 0;
                            foreach (string v in permitted) {
                                message.append(((i++ == 0) ? " " : ", ")).append(v);
                            }
                            throw new Saxon.Hej.trans.XPathException(message.ToString(), errorCodeForDisallowedValue);
                        }
                    }
                    Saxon.Impl.Helpers.MapUtils.Put(result, nominalKey, actual);
                } else {
                    Saxon.Hej.om.Sequence def = Saxon.Impl.Helpers.MapUtils.Get(defaultValues, nominalKey);
                    if ((def != null)) {
                        Saxon.Impl.Helpers.MapUtils.Put(result, nominalKey, def);
                    }
                }
            }
            return result;
        }
        public System.Collections.Generic.IDictionary<string,Saxon.Hej.om.Sequence> getDefaultOptions() {
            System.Collections.Generic.IDictionary<string,Saxon.Hej.om.Sequence> result = new System.Collections.Generic.Dictionary<string, Saxon.Hej.om.Sequence>();
            foreach (System.Collections.Generic.KeyValuePair<string,Saxon.Hej.om.Sequence> entry in defaultValues) {
                Saxon.Impl.Helpers.MapUtils.Put(result, entry.Key, entry.Value);
            }
            return result;
        }
        public string getErrorCodeForAbsentValue() {
            return errorCodeForAbsentValue;
        }
        public void setErrorCodeForAbsentValue(string errorCodeForAbsentValue) {
            this.errorCodeForAbsentValue = errorCodeForAbsentValue;
        }
        public bool isAllowCastFromString() {
            return allowCastFromString;
        }
        public void setAllowCastFromString(bool allowCastFromString) {
            this.allowCastFromString = allowCastFromString;
        }
    
    }
}