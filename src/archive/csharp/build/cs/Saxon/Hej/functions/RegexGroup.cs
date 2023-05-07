using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.functions {
    public class RegexGroup : Saxon.Hej.functions.ContextAccessorFunction {
        public override Saxon.Hej.om.FunctionItem bindContext(Saxon.Hej.expr.XPathContext context) {
            Saxon.Hej.om.FunctionItem alwaysEmptyFunction = new Saxon.Hej.functions.CallableFunction(1, new Saxon.Hej.functions.RegexGroup.AlwaysEmpty(), new Saxon.Hej.type.SpecificFunctionType(new Saxon.Hej.value.SequenceType[]{Saxon.Hej.value.SequenceType.SINGLE_INTEGER}, Saxon.Hej.value.SequenceType.SINGLE_STRING));
            if ((getRetainedStaticContext().getPackageData().getHostLanguageVersion()<40)) {
                return alwaysEmptyFunction;
            }
            Saxon.Hej.regex.RegexIterator ri = context.getCurrentRegexIterator();
            if ((ri == null)) {
                throw new Saxon.Hej.trans.XPathException("There is no current group", "XTDE1061");
            }
            int groups = ri.getNumberOfGroups();
            Saxon.Hej.ma.map.MapItem map = new Saxon.Impl.DataModel.ImmMap();
            for (int i = 0; (i<=groups); i++) {
                map = map.addEntry(Saxon.Hej.value.Int64Value.makeIntegerValue(i), new Saxon.Hej.value.StringValue(ri.getRegexGroup(i)));
            }
            Saxon.Hej.om.StructuredQName mapGetName = new Saxon.Hej.om.StructuredQName("map", Saxon.Hej.om.NamespaceUri.MAP_FUNCTIONS, "get");
            Saxon.Hej.functions.registry.BuiltInFunctionSet lib = Saxon.Hej.ma.map.MapFunctionSet.getInstance(40);
            Saxon.Hej.trans.SymbolicName.F symbolicName = new Saxon.Hej.trans.SymbolicName.F(mapGetName, 3);
            Saxon.Hej.om.FunctionItem mapGet3 = lib.getFunctionItem(symbolicName, new Saxon.Hej.sxpath.IndependentContext(context.getConfiguration()));
            return new Saxon.Hej.functions.hof.CurriedFunction(mapGet3, new Saxon.Hej.om.Sequence[]{map, null, alwaysEmptyFunction});
        }
        public override Saxon.Hej.value.StringValue call(Saxon.Hej.expr.XPathContext context, Saxon.Hej.om.Sequence[] arguments) {
            Saxon.Hej.regex.RegexIterator iter = context.getCurrentRegexIterator();
            if ((iter == null)) {
                return Saxon.Hej.value.StringValue.EMPTY_STRING;
            }
            Saxon.Hej.value.NumericValue gp0 = (Saxon.Hej.value.NumericValue)(arguments[0].head());
            Saxon.Hej.str.UnicodeString s = iter.getRegexGroup((int)(gp0.longValue()));
            return Saxon.Hej.value.StringValue.makeUStringValue(s);
        }
        private class AlwaysEmpty : Saxon.Hej.expr.Callable {
            public Saxon.Hej.om.Sequence call(Saxon.Hej.expr.XPathContext context, Saxon.Hej.om.Sequence[] arguments) {
                return Saxon.Hej.value.StringValue.EMPTY_STRING;
            }
        
        }
    
    }
}