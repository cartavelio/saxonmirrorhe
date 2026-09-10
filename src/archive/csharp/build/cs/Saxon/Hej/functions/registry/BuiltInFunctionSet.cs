using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.functions.registry {
    public abstract class BuiltInFunctionSet : Saxon.Hej.functions.FunctionLibrary {
        public static Saxon.Hej.om.Sequence EMPTY = Saxon.Hej.value.EmptySequence.getInstance();
        public static readonly int ONE = Saxon.Hej.expr.StaticProperty.ALLOWS_ONE;
        public static readonly int OPT = Saxon.Hej.expr.StaticProperty.ALLOWS_ZERO_OR_ONE;
        public static readonly int STAR = Saxon.Hej.expr.StaticProperty.ALLOWS_ZERO_OR_MORE;
        public static readonly int PLUS = Saxon.Hej.expr.StaticProperty.ALLOWS_ONE_OR_MORE;
        public const int AS_ARG0 = 1;
        public const int AS_PRIM_ARG0 = 2;
        public const int CITEM = 4;
        public const int BASE = 8;
        public const int NS = 16;
        public const int DCOLL = 32;
        public const int DLANG = 64;
        public const int FILTER = 256;
        public const int LATE = 512;
        public const int UO = 1024;
        public const int POSN = (1024*2);
        public const int LAST = (1024*4);
        public const int SIDE = (1024*8);
        public const int CDOC = (1024*16);
        public const int CARD0 = (1024*32);
        public const int NEW = (1024*64);
        public const int CTRL = (1024*128);
        public const int SEQV = (1024*256);
        public const int DEPENDS_ON_STATIC_CONTEXT = ((BASE|NS)|DCOLL);
        public const int FOCUS = (((CITEM|POSN)|LAST)|CDOC);
        protected internal const int INS = (1<<24);
        protected internal const int ABS = (1<<25);
        protected internal const int TRA = (1<<26);
        protected internal const int NAV = (1<<27);
        private readonly System.Collections.Generic.Dictionary<string,Saxon.Hej.functions.registry.BuiltInFunctionSet.Entry> functionTable = new System.Collections.Generic.Dictionary<string, Saxon.Hej.functions.registry.BuiltInFunctionSet.Entry>(200);
        private readonly System.Collections.Generic.Dictionary<string,int> sequenceVariadicFunctions = new System.Collections.Generic.Dictionary<string, int>(10);
        public void importFunctionSet(Saxon.Hej.functions.registry.BuiltInFunctionSet importee) {
            if (!(importee.getNamespace().Equals(getNamespace()))) {
                throw new Saxon.Ejava.lang.IllegalArgumentException(importee.getNamespace());
            }
            Saxon.Impl.Helpers.MapUtils.PutAll(functionTable, importee.functionTable);
            Saxon.Impl.Helpers.MapUtils.PutAll(sequenceVariadicFunctions, importee.sequenceVariadicFunctions);
        }
        public Saxon.Hej.functions.registry.BuiltInFunctionSet.Entry getFunctionDetails(string name, int arity) {
            if ((arity == -1)) {
                for (int i = 0; (i<20); i++) {
                    Saxon.Hej.functions.registry.BuiltInFunctionSet.Entry found = getFunctionDetails(name, i);
                    if ((found != null)) {
                        return found;
                    }
                }
                return null;
            }
            string key = ((name+"#")+arity);
            Saxon.Hej.functions.registry.BuiltInFunctionSet.Entry entry = Saxon.Impl.Helpers.MapUtils.Get(functionTable, key);
            if ((entry != null)) {
                return entry;
            }
            int minArity = Saxon.Impl.Helpers.MapUtils.GetOrDefault(sequenceVariadicFunctions, name, -1);
            if (((minArity != -1) && (arity>=minArity))) {
                key = ((name+"#")+((minArity+1)));
                return Saxon.Impl.Helpers.MapUtils.Get(functionTable, key);
            }
            return null;
        }
        public Saxon.Hej.expr.Expression bind(Saxon.Hej.trans.SymbolicName.F symbolicName, Saxon.Hej.expr.Expression[] staticArgs, Saxon.Hej.expr.StaticContext env, System.Collections.Generic.IList<string> reasons) {
            Saxon.Hej.om.StructuredQName functionName = symbolicName.getComponentName();
            int arity = symbolicName.getArity();
            string localName = functionName.getLocalPart();
            Saxon.Hej.functions.registry.BuiltInFunctionSet.Entry entry = getFunctionDetails(localName, arity);
            if ((functionName.hasURI(getNamespace()) && (entry != null))) {
                if ((((entry.properties&SEQV)) != 0)) {
                    if ((env.getXPathVersion()<40)) {
                        if (localName.Equals("concat")) {
                            if ((staticArgs.Length<2)) {
                                reasons.Add("concat() prior to XPath 4.0 requires at least two arguments");
                                return null;
                            }
                            Saxon.Hej.expr.Expression[] a2 = new Saxon.Hej.expr.Expression[staticArgs.Length];
                            for (int i = 0; (i<staticArgs.Length); i++) {
                                if ((staticArgs[i] is Saxon.Hej.expr.StringLiteral)) {
                                    a2[i] = staticArgs[i];
                                } else {
                                    Saxon.Hej.expr.parser.RoleDiagnostic role = new Saxon.Hej.expr.parser.RoleDiagnostic(Saxon.Hej.expr.parser.RoleDiagnostic.FUNCTION, "concat", i);
                                    a2[i] = Saxon.Hej.expr.CardinalityChecker.makeCardinalityChecker(staticArgs[i], Saxon.Hej.expr.StaticProperty.ALLOWS_ZERO_OR_ONE, role);
                                    a2[i].setRetainedStaticContext(env.makeRetainedStaticContext());
                                }
                            }
                            staticArgs = a2;
                        }
                    }
                    int declaredArity = entry.arity;
                    Saxon.Hej.expr.Expression[] newArgs = Saxon.Ejava.util.Arrays.copyOf(staticArgs, declaredArity);
                    if ((declaredArity>staticArgs.Length)) {
                        newArgs[(declaredArity-1)] = Saxon.Hej.expr.Literal.makeEmptySequence();
                    } else {
                        if ((declaredArity<staticArgs.Length)) {
                            Saxon.Hej.expr.Expression block = new Saxon.Hej.expr.instruct.Block(Saxon.Ejava.util.Arrays.copyOfRange(staticArgs, (declaredArity-1), staticArgs.Length));
                            Saxon.Hej.expr.parser.ExpressionTool.copyLocationInfo(staticArgs[0], block);
                            newArgs[(declaredArity-1)] = block;
                        }
                    }
                    staticArgs = newArgs;
                }
                Saxon.Hej.expr.parser.RetainedStaticContext rsc = new Saxon.Hej.expr.parser.RetainedStaticContext(env);
                try {
                    Saxon.Hej.functions.SystemFunction fn = makeFunction(localName, staticArgs.Length);
                    fn.setRetainedStaticContext(rsc);
                    Saxon.Hej.expr.Expression f = fn.makeFunctionCall(staticArgs);
                    f.setRetainedStaticContext(rsc);
                    return f;
                } catch (Saxon.Hej.trans.XPathException e) {
                    reasons.Add(e.Message);
                    return null;
                }
            } else {
                return null;
            }
        }
        public Saxon.Hej.functions.SystemFunction makeFunction(string name, int arity) {
            Saxon.Hej.functions.registry.BuiltInFunctionSet.Entry entry = getFunctionDetails(name, arity);
            if ((entry == null)) {
                string diagName = (getNamespace().Equals(Saxon.Hej.lib.NamespaceConstant.FN) ? ("System function "+name) : ((("Function Q{"+getNamespace())+"}")+name));
                if ((getFunctionDetails(name, -1) == null)) {
                    Saxon.Hej.trans.XPathException err = new Saxon.Hej.trans.XPathException((diagName+"() does not exist or is not available in this environment"));
                    err.setErrorCode("XPST0017");
                    err.setIsStaticError(true);
                    throw err;
                } else {
                    Saxon.Hej.trans.XPathException err = new Saxon.Hej.trans.XPathException(((diagName+"() cannot be called with ")+pluralArguments(arity)));
                    err.setErrorCode("XPST0017");
                    err.setIsStaticError(true);
                    throw err;
                }
            }
            System.Type functionClass = entry.implementationClass;
            Saxon.Hej.functions.SystemFunction f = instantiate(name, functionClass);
            f.setDetails(entry);
            f.setArity(arity);
            return f;
        }
        private Saxon.Hej.functions.SystemFunction instantiate(string name, System.Type functionClass) {
            Saxon.Hej.functions.SystemFunction f;
            try {
                f = (Saxon.Hej.functions.SystemFunction)((Saxon.Hej.functions.SystemFunction)System.Activator.CreateInstance(functionClass));
            } catch (System.Exception err) {
                System.Console.Error.WriteLine(err);
                throw new Saxon.Ejava.lang.AssertionError(((("Failed to instantiate system function "+name)+" - ")+err.Message));
            }
            return f;
        }
        private static string pluralArguments(int num) {
            if ((num == 0)) {
                return "zero arguments";
            }
            if ((num == 1)) {
                return "one argument";
            }
            return (num+" arguments");
        }
        public bool isAvailable(Saxon.Hej.trans.SymbolicName.F symbolicName) {
            Saxon.Hej.om.StructuredQName qn = symbolicName.getComponentName();
            return (qn.hasURI(getNamespace()) && (getFunctionDetails(qn.getLocalPart(), symbolicName.getArity()) != null));
        }
        public Saxon.Hej.functions.FunctionLibrary copy() {
            return this;
        }
        public Saxon.Hej.om.Function getFunctionItem(Saxon.Hej.trans.SymbolicName.F symbolicName, Saxon.Hej.expr.StaticContext staticContext) {
            Saxon.Hej.om.StructuredQName functionName = symbolicName.getComponentName();
            int arity = symbolicName.getArity();
            if ((functionName.hasURI(getNamespace()) && (getFunctionDetails(functionName.getLocalPart(), arity) != null))) {
                Saxon.Hej.expr.parser.RetainedStaticContext rsc = staticContext.makeRetainedStaticContext();
                Saxon.Hej.functions.SystemFunction fn = makeFunction(functionName.getLocalPart(), arity);
                if ((((staticContext.getXPathVersion()<40) && (fn is Saxon.Hej.functions.Concat)) && (arity<2))) {
                    return null;
                }
                fn.setRetainedStaticContext(rsc);
                return fn;
            } else {
                return null;
            }
        }
        protected internal Saxon.Hej.functions.registry.BuiltInFunctionSet.Entry register(string name, int arity, System.Type implementationClass, Saxon.Hej.type.ItemType itemType, int cardinality, int properties) {
            Saxon.Hej.functions.registry.BuiltInFunctionSet.Entry e = new Saxon.Hej.functions.registry.BuiltInFunctionSet.Entry();
            e.name = new Saxon.Hej.om.StructuredQName(getConventionalPrefix(), getNamespace(), name);
            e.arity = arity;
            e.implementationClass = implementationClass;
            e.functionSet = this;
            e.itemType = itemType;
            e.cardinality = cardinality;
            e.properties = properties;
            if ((e.arity == -1)) {
                e.argumentTypes = new Saxon.Hej.value.SequenceType[1];
                e.resultIfEmpty = new Saxon.Hej.value.AtomicValue[1];
                e.usage = new Saxon.Hej.expr.OperandUsage[1];
            } else {
                e.argumentTypes = new Saxon.Hej.value.SequenceType[arity];
                e.resultIfEmpty = new Saxon.Hej.om.Sequence[arity];
                e.usage = new Saxon.Hej.expr.OperandUsage[arity];
            }
            Saxon.Impl.Helpers.MapUtils.Put(functionTable, ((name+"#")+arity), e);
            if ((((properties&SEQV)) != 0)) {
                Saxon.Impl.Helpers.MapUtils.Put(sequenceVariadicFunctions, name, (arity-1));
            }
            return e;
        }
        protected internal void registerReducedArityVariants(string key, int min, int max) {
            Saxon.Hej.functions.registry.BuiltInFunctionSet.Entry master = Saxon.Impl.Helpers.MapUtils.Get(functionTable, key);
            for (int arity = min; (arity<=max); arity++) {
                Saxon.Hej.functions.registry.BuiltInFunctionSet.Entry e = new Saxon.Hej.functions.registry.BuiltInFunctionSet.Entry();
                e.name = master.name;
                e.arity = arity;
                e.implementationClass = master.implementationClass;
                e.itemType = master.itemType;
                e.cardinality = master.cardinality;
                e.properties = master.properties;
                e.argumentTypes = new Saxon.Hej.value.SequenceType[arity];
                e.resultIfEmpty = new Saxon.Hej.om.Sequence[arity];
                e.usage = new Saxon.Hej.expr.OperandUsage[arity];
                for (int i = 0; (i<arity); i++) {
                    e.argumentTypes[i] = master.argumentTypes[i];
                    e.resultIfEmpty[i] = master.resultIfEmpty[i];
                    e.usage[i] = master.usage[i];
                }
                Saxon.Impl.Helpers.MapUtils.Put(functionTable, ((e.name.getLocalPart()+"#")+arity), e);
            }
        }
        public virtual string getNamespace() {
            return Saxon.Hej.lib.NamespaceConstant.FN;
        }
        public virtual string getConventionalPrefix() {
            return "fn";
        }
        public class Entry {
            public Saxon.Hej.om.StructuredQName name;
            public System.Type implementationClass;
            public Saxon.Hej.functions.registry.BuiltInFunctionSet functionSet;
            public int arity;
            public Saxon.Hej.type.ItemType itemType;
            public int cardinality;
            public Saxon.Hej.expr.OperandUsage[] usage;
            public Saxon.Hej.value.SequenceType[] argumentTypes;
            public Saxon.Hej.om.Sequence[] resultIfEmpty;
            public int properties;
            public Saxon.Hej.functions.OptionsParameter optionDetails;
            public Saxon.Hej.functions.registry.BuiltInFunctionSet.Entry arg(int a, Saxon.Hej.type.ItemType type, int options, Saxon.Hej.om.Sequence resultIfEmpty) {
                int cardinality = (options&Saxon.Hej.expr.StaticProperty.CARDINALITY_MASK);
                Saxon.Hej.expr.OperandUsage usage = Saxon.Hej.expr.OperandUsage.NAVIGATION;
                if ((((options&ABS)) != 0)) {
                    usage = Saxon.Hej.expr.OperandUsage.ABSORPTION;
                } else {
                    if ((((options&TRA)) != 0)) {
                        usage = Saxon.Hej.expr.OperandUsage.TRANSMISSION;
                    } else {
                        if ((((options&INS)) != 0)) {
                            usage = Saxon.Hej.expr.OperandUsage.INSPECTION;
                        } else {
                            if ((type is Saxon.Hej.type.PlainType)) {
                                usage = Saxon.Hej.expr.OperandUsage.ABSORPTION;
                            }
                        }
                    }
                }
                try {
                    this.argumentTypes[a] = Saxon.Hej.value.SequenceType.makeSequenceType(type, cardinality);
                    this.resultIfEmpty[a] = resultIfEmpty;
                    this.usage[a] = usage;
                } catch (System.IndexOutOfRangeException err) {
                    System.Console.Error.WriteLine(((("Internal Saxon error: Can't set argument "+a)+" of ")+name));
                }
                return this;
            }
            public Saxon.Hej.functions.registry.BuiltInFunctionSet.Entry setOptionDetails(Saxon.Hej.functions.OptionsParameter details) {
                this.optionDetails = details;
                return this;
            }
        
        }
    
        /* Expanding default method setConfiguration from interface FunctionLibrary */
        public virtual void setConfiguration(Saxon.Hej.Configuration config) {
        }

    }
}