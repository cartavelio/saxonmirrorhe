using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.ee.extfn.js {
    public class IXSLFunctionSet : Saxon.Hej.functions.registry.BuiltInFunctionSet {
        private static readonly Saxon.Eej.ee.extfn.js.IXSLFunctionSet THE_INSTANCE = new Saxon.Eej.ee.extfn.js.IXSLFunctionSet();
        public static Saxon.Eej.ee.extfn.js.IXSLFunctionSet getInstance() {
            return THE_INSTANCE;
        }
        private IXSLFunctionSet()  {
            init();
        }
        private void init() {
            register("page", 0, typeof(Saxon.Eej.ee.extfn.js.IXSLFunctionSet.Stub), Saxon.Hej.pattern.NodeKindTest.DOCUMENT, ONE, (SIDE|LATE));
            register("source", 0, typeof(Saxon.Eej.ee.extfn.js.IXSLFunctionSet.Source), Saxon.Hej.pattern.NodeKindTest.DOCUMENT, OPT, LATE);
            register("window", 0, typeof(Saxon.Eej.ee.extfn.js.IXSLFunctionSet.Stub), Saxon.Hej.type.AnyItemType.getInstance(), ONE, LATE);
            register("query-params", 0, typeof(Saxon.Eej.ee.extfn.js.IXSLFunctionSet.Stub), Saxon.Hej.ma.map.MapType.ANY_MAP_TYPE, ONE, (SIDE|LATE));
            register("event", 0, typeof(Saxon.Eej.ee.extfn.js.IXSLFunctionSet.Stub), Saxon.Hej.type.AnyItemType.getInstance(), OPT, (SIDE|LATE));
            register("location", 0, typeof(Saxon.Eej.ee.extfn.js.IXSLFunctionSet.Stub), Saxon.Hej.type.BuiltInAtomicType.STRING, ONE, (SIDE|LATE));
            register("style", 1, typeof(Saxon.Eej.ee.extfn.js.IXSLFunctionSet.Stub), Saxon.Hej.ma.map.MapType.ANY_MAP_TYPE, ONE, (SIDE|LATE)).arg(0, Saxon.Hej.type.AnyItemType.getInstance(), OPT, null);
            register("get", 2, typeof(Saxon.Eej.ee.extfn.js.IXSLFunctionSet.Stub), Saxon.Hej.type.AnyItemType.getInstance(), STAR, (SIDE|LATE)).arg(0, Saxon.Hej.type.AnyItemType.getInstance(), ONE, null).arg(1, Saxon.Hej.type.BuiltInAtomicType.STRING, ONE, null);
            register("call", 3, typeof(Saxon.Eej.ee.extfn.js.IXSLFunctionSet.Call), Saxon.Hej.type.AnyItemType.getInstance(), STAR, (SIDE|LATE)).arg(0, Saxon.Hej.type.AnyItemType.getInstance(), ONE, null).arg(1, Saxon.Hej.type.BuiltInAtomicType.STRING, ONE, null).arg(2, Saxon.Hej.ma.arrays.ArrayItemType.getInstance(), ONE, null);
            register("contains", 2, typeof(Saxon.Eej.ee.extfn.js.IXSLFunctionSet.Stub), Saxon.Hej.type.BuiltInAtomicType.BOOLEAN, ONE, (SIDE|LATE)).arg(0, Saxon.Hej.type.AnyItemType.getInstance(), ONE, null).arg(1, Saxon.Hej.type.BuiltInAtomicType.STRING, ONE, null);
            register("eval", 1, typeof(Saxon.Eej.ee.extfn.js.IXSLFunctionSet.Eval), Saxon.Hej.type.AnyItemType.getInstance(), STAR, (SIDE|LATE)).arg(0, Saxon.Hej.type.BuiltInAtomicType.STRING, ONE, null);
            register("apply", 2, typeof(Saxon.Eej.ee.extfn.js.IXSLFunctionSet.Stub), Saxon.Hej.type.AnyItemType.getInstance(), STAR, (SIDE|LATE)).arg(0, Saxon.Hej.type.AnyItemType.getInstance(), ONE, null).arg(1, Saxon.Hej.ma.arrays.ArrayItemType.getInstance(), ONE, null);
            register("schedule-action", 3, typeof(Saxon.Eej.ee.extfn.js.IXSLFunctionSet.Stub), Saxon.Hej.type.AnyItemType.getInstance(), STAR, (SIDE|LATE)).arg(0, Saxon.Hej.type.BuiltInAtomicType.INTEGER, ONE, null).arg(1, Saxon.Hej.type.BuiltInAtomicType.STRING, OPT, null).arg(2, Saxon.Hej.type.AnyItemType.getInstance(), STAR, null);
            register("schedule-action", 4, typeof(Saxon.Eej.ee.extfn.js.IXSLFunctionSet.Stub), Saxon.Hej.type.AnyItemType.getInstance(), STAR, (SIDE|LATE)).arg(0, Saxon.Hej.type.BuiltInAtomicType.INTEGER, ONE, null).arg(1, Saxon.Hej.type.BuiltInAtomicType.STRING, OPT, null).arg(2, Saxon.Hej.type.AnyItemType.getInstance(), STAR, null).arg(3, Saxon.Hej.ma.map.MapType.ANY_MAP_TYPE, OPT, null);
            register("set-attribute", 3, typeof(Saxon.Eej.ee.extfn.js.IXSLFunctionSet.Stub), Saxon.Hej.type.AnyItemType.getInstance(), OPT, ((SIDE|FOCUS)|LATE)).arg(0, Saxon.Hej.type.BuiltInAtomicType.STRING, ONE, null).arg(1, Saxon.Hej.type.BuiltInAtomicType.STRING, ONE, null).arg(2, Saxon.Hej.type.AnyItemType.getInstance(), ONE, null);
            register("remove-attribute", 2, typeof(Saxon.Eej.ee.extfn.js.IXSLFunctionSet.Stub), Saxon.Hej.type.AnyItemType.getInstance(), OPT, ((SIDE|FOCUS)|LATE)).arg(0, Saxon.Hej.type.BuiltInAtomicType.STRING, ONE, null).arg(1, Saxon.Hej.type.AnyItemType.getInstance(), ONE, null);
            register("set-property", 3, typeof(Saxon.Eej.ee.extfn.js.IXSLFunctionSet.Stub), Saxon.Hej.type.AnyItemType.getInstance(), OPT, ((SIDE|FOCUS)|LATE)).arg(0, Saxon.Hej.type.BuiltInAtomicType.STRING, ONE, null).arg(1, Saxon.Hej.type.AnyItemType.getInstance(), OPT, null).arg(2, Saxon.Hej.type.AnyItemType.getInstance(), ONE, null);
            register("remove-property", 2, typeof(Saxon.Eej.ee.extfn.js.IXSLFunctionSet.Stub), Saxon.Hej.type.AnyItemType.getInstance(), OPT, ((SIDE|FOCUS)|LATE)).arg(0, Saxon.Hej.type.BuiltInAtomicType.STRING, ONE, null).arg(1, Saxon.Hej.type.AnyItemType.getInstance(), ONE, null);
            register("set-style", 3, typeof(Saxon.Eej.ee.extfn.js.IXSLFunctionSet.Stub), Saxon.Hej.type.AnyItemType.getInstance(), OPT, ((SIDE|FOCUS)|LATE)).arg(0, Saxon.Hej.type.BuiltInAtomicType.STRING, ONE, null).arg(1, Saxon.Hej.type.BuiltInAtomicType.STRING, OPT, null).arg(2, Saxon.Hej.type.AnyItemType.getInstance(), ONE, null);
        }
        public override string getNamespace() {
            return Saxon.Hej.lib.NamespaceConstant.IXSL;
        }
        public override string getConventionalPrefix() {
            return "ixsl";
        }
        public class Stub : Saxon.Hej.functions.SystemFunction {
            public override Saxon.Hej.om.Sequence call(Saxon.Hej.expr.XPathContext context, Saxon.Hej.om.Sequence[] args) {
                throw new Saxon.Hej.trans.XPathException((("ixsl:"+getDetails().name.getLocalPart())+" is available only in Saxon-JS"));
            }
        
        }
        public class Source : Saxon.Hej.functions.SystemFunction {
            public override Saxon.Hej.om.Sequence call(Saxon.Hej.expr.XPathContext context, Saxon.Hej.om.Sequence[] arguments) {
                return context.getController().getGlobalContextItem();
            }
        
        }
        public class ScheduleActionFn : Saxon.Hej.functions.SystemFunction {
            internal Saxon.Hej.s9api.Location location;
            public override Saxon.Hej.expr.Expression makeFunctionCall(params Saxon.Hej.expr.Expression[]  arguments) {
                Saxon.Hej.expr.Expression[] copiedArgs = Saxon.Ejava.util.Arrays.copyOf(arguments, arguments.Length);
                copiedArgs[2] = Saxon.Hej.expr.Literal.makeLiteral(new Saxon.Hej.value.ObjectValue<Saxon.Hej.expr.Expression>(arguments[2]));
                Saxon.Hej.expr.parser.ExpressionTool.copyLocationInfo(arguments[2], copiedArgs[2]);
                location = arguments[0].getLocation();
                return base.makeFunctionCall(copiedArgs);
            }
            public override Saxon.Hej.om.Sequence call(Saxon.Hej.expr.XPathContext context, Saxon.Hej.om.Sequence[] arguments) {
                Saxon.Hej.value.IntegerValue wait = (Saxon.Hej.value.IntegerValue)(arguments[0].head());
                Saxon.Hej.value.StringValue documents = (Saxon.Hej.value.StringValue)(arguments[1].head());
                Saxon.Hej.value.ObjectValue<Saxon.Hej.expr.Expression> wrappedActionObject = (Saxon.Hej.value.ObjectValue<Saxon.Hej.expr.Expression>)(arguments[2].head());
                Saxon.Hej.expr.Expression action = wrappedActionObject.getObject();
                if ((documents != null)) {
                    Saxon.Hej.tree.iter.AtomicIterator tokenIter = new Saxon.Hej.value.Whitespace.Tokenizer(documents.getUnicodeStringValue());
                    string baseURI = getRetainedStaticContext().getStaticBaseUriString();
                    Saxon.Hej.expr.PackageData pd = getRetainedStaticContext().getPackageData();
                    Saxon.Hej.lib.ParseOptions options = context.getConfiguration().getParseOptions();
                    Saxon.Hej.expr.ItemMappingFunction mapper = Saxon.Hej.expr.ItemMapper.of(token => Saxon.Hej.functions.DocumentFn.makeDoc(token.getStringValue(), baseURI, pd, options, context, location, false));
                    Saxon.Hej.om.SequenceIterator mapped = context.getConfiguration().getMultithreadedItemMappingIterator(tokenIter, mapper);
                    Saxon.Hej.om.SequenceTool.toGroundedValue(mapped);
                    return new Saxon.Hej.om.LazySequence(action.iterate(context));
                } else {
                    return Saxon.Hej.value.EmptySequence.getInstance();
                }
            }
        
        }
        public class Call : Saxon.Hej.functions.SystemFunction {
            public override Saxon.Hej.om.Sequence call(Saxon.Hej.expr.XPathContext context, Saxon.Hej.om.Sequence[] arguments) {
                return Saxon.Hej.value.EmptySequence.getInstance();
            }
        
        }
        public class Eval : Saxon.Hej.functions.SystemFunction {
            public override Saxon.Hej.om.Sequence call(Saxon.Hej.expr.XPathContext context, Saxon.Hej.om.Sequence[] arguments) {
                return Saxon.Hej.value.EmptySequence.getInstance();
            }
        
        }
    
    }
}