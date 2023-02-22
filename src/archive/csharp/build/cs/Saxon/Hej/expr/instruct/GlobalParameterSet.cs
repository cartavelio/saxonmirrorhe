using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.expr.instruct {
    public class GlobalParameterSet {
        private System.Collections.Generic.Dictionary<Saxon.Hej.om.StructuredQName,Saxon.Hej.om.GroundedValue> @params = new System.Collections.Generic.Dictionary<Saxon.Hej.om.StructuredQName, Saxon.Hej.om.GroundedValue>(10);
        public GlobalParameterSet()  {
        }
        public GlobalParameterSet(Saxon.Hej.expr.instruct.GlobalParameterSet parameterSet)  {
            this.@params = new System.Collections.Generic.Dictionary<Saxon.Hej.om.StructuredQName,Saxon.Hej.om.GroundedValue>(parameterSet.@params);
        }
        public void put(Saxon.Hej.om.StructuredQName qName, Saxon.Hej.om.GroundedValue value) {
            if ((value == null)) {
                @params.Remove(qName);
            } else {
                Saxon.Impl.Helpers.MapUtils.Put(@params, qName, value);
            }
        }
        public Saxon.Hej.om.GroundedValue get(Saxon.Hej.om.StructuredQName qName) {
            return Saxon.Impl.Helpers.MapUtils.Get(@params, qName);
        }
        public bool containsKey(Saxon.Hej.om.StructuredQName qName) {
            return @params.ContainsKey(qName);
        }
        public Saxon.Hej.om.GroundedValue convertParameterValue(Saxon.Hej.om.StructuredQName qName, Saxon.Hej.value.SequenceType requiredType, bool convert, Saxon.Hej.expr.XPathContext context) {
            Saxon.Hej.om.Sequence val = get(qName);
            if ((val == null)) {
                return null;
            }
            if ((requiredType != null)) {
                if (convert) {
                    Saxon.Hej.expr.parser.RoleDiagnostic role = new Saxon.Hej.expr.parser.RoleDiagnostic(Saxon.Hej.expr.parser.RoleDiagnostic.VARIABLE, qName.getDisplayName(), -1);
                    Saxon.Hej.Configuration config = context.getConfiguration();
                    val = config.getTypeHierarchy().applyFunctionConversionRules(val, requiredType, role, Saxon.Hej.expr.parser.Loc.NONE);
                } else {
                    Saxon.Hej.trans.XPathException err = Saxon.Hej.expr.parser.TypeChecker.testConformance(val, requiredType, context);
                    if ((err != null)) {
                        throw err;
                    }
                }
            }
            return val.materialize();
        }
        public void clear() {
            @params.Clear();
        }
        public System.Collections.Generic.ICollection<Saxon.Hej.om.StructuredQName> getKeys() {
            return Saxon.Impl.Helpers.MapUtils.KeySet(@params);
        }
        public int getNumberOfKeys() {
            return @params.Count;
        }
    
    }
}