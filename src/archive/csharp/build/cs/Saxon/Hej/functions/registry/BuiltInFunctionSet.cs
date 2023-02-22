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
                throw new Saxon.Ejava.lang.IllegalArgumentException(importee.getNamespace().ToString());
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
        public Saxon.Hej.expr.Expression bind(Saxon.Hej.trans.SymbolicName.F symbolicName, Saxon.Hej.expr.Expression[] staticArgs, System.Collections.Generic.IDictionary<Saxon.Hej.om.StructuredQName,int> keywords, Saxon.Hej.expr.StaticContext env, System.Collections.Generic.IList<string> reasons) {
            Saxon.Hej.om.StructuredQName functionName = symbolicName.getComponentName();
            int arity = symbolicName.getArity();
            string localName = functionName.getLocalPart();
            Saxon.Hej.functions.registry.BuiltInFunctionSet.Entry entry = getFunctionDetails(localName, arity);
            if ((functionName.hasURI(getNamespace()) && (entry != null))) {
                entry.ensurePopulated();
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
                                    int pos = i;
                                    Saxon.Ejava.util.function.Supplier<Saxon.Hej.expr.parser.RoleDiagnostic> role = () => new Saxon.Hej.expr.parser.RoleDiagnostic(Saxon.Hej.expr.parser.RoleDiagnostic.FUNCTION, "concat", pos);
                                    a2[i] = Saxon.Hej.expr.CardinalityChecker.makeCardinalityChecker(staticArgs[i], Saxon.Hej.expr.StaticProperty.ALLOWS_ZERO_OR_ONE, role);
                                    a2[i].setRetainedStaticContext(env.makeRetainedStaticContext());
                                }
                            }
                            staticArgs = a2;
                        }
                    }
                    int declaredArity = entry.maxArity;
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
                } else {
                    if (((((keywords != null) && !(keywords.Count==0))) || (staticArgs.Length<entry.maxArity))) {
                        staticArgs = Saxon.Hej.expr.instruct.UserFunction.makeExpandedArgumentArray(staticArgs, keywords, entry);
                    }
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
                string diagName = (getNamespace().Equals(Saxon.Hej.om.NamespaceUri.FN) ? ("System function "+name) : ((("Function Q{"+getNamespace())+"}")+name));
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
            entry.ensurePopulated();
            Saxon.Hej.functions.SystemFunction f = entry.implementationFactory();
            f.setDetails(entry);
            f.setArity(arity);
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
        public bool isAvailable(Saxon.Hej.trans.SymbolicName.F symbolicName, int languageLevel) {
            Saxon.Hej.om.StructuredQName qn = symbolicName.getComponentName();
            if (!(qn.hasURI(getNamespace()))) {
                return false;
            }
            Saxon.Hej.functions.registry.BuiltInFunctionSet.Entry entry = getFunctionDetails(qn.getLocalPart(), symbolicName.getArity());
            if ((entry == null)) {
                return false;
            }
            if ((((languageLevel<40) && symbolicName.getComponentName().getLocalPart().Equals("concat")) && (symbolicName.getArity()<2))) {
                return false;
            }
            return true;
        }
        public Saxon.Hej.functions.FunctionLibrary copy() {
            return this;
        }
        public Saxon.Hej.om.FunctionItem getFunctionItem(Saxon.Hej.trans.SymbolicName.F symbolicName, Saxon.Hej.expr.StaticContext staticContext) {
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
        protected internal Saxon.Hej.functions.registry.BuiltInFunctionSet.Entry register(string name, int arity, System.Func<Saxon.Hej.functions.registry.BuiltInFunctionSet.Entry,Saxon.Hej.functions.registry.BuiltInFunctionSet.Entry> populator) {
            Saxon.Hej.functions.registry.BuiltInFunctionSet.Entry e = new Saxon.Hej.functions.registry.BuiltInFunctionSet.Entry();
            e.name = new Saxon.Hej.om.StructuredQName(getConventionalPrefix(), getNamespace(), name);
            e.minArity = arity;
            e.maxArity = arity;
            e.populator = populator;
            e.functionSet = this;
            Saxon.Impl.Helpers.MapUtils.Put(functionTable, ((name+"#")+arity), e);
            return e;
        }
        protected internal Saxon.Hej.functions.registry.BuiltInFunctionSet.Entry register(string name, int minArity, int maxArity, System.Func<Saxon.Hej.functions.registry.BuiltInFunctionSet.Entry,Saxon.Hej.functions.registry.BuiltInFunctionSet.Entry> populator) {
            Saxon.Hej.functions.registry.BuiltInFunctionSet.Entry e = new Saxon.Hej.functions.registry.BuiltInFunctionSet.Entry();
            e.name = new Saxon.Hej.om.StructuredQName(getConventionalPrefix(), getNamespace(), name);
            e.minArity = minArity;
            e.maxArity = maxArity;
            e.populator = populator;
            e.functionSet = this;
            for (int a = minArity; (a<=maxArity); a++) {
                Saxon.Impl.Helpers.MapUtils.Put(functionTable, ((name+"#")+a), e);
            }
            return e;
        }
        protected internal Saxon.Hej.functions.registry.BuiltInFunctionSet.Entry registerVariadic(string name, int arity, System.Func<Saxon.Hej.functions.registry.BuiltInFunctionSet.Entry,Saxon.Hej.functions.registry.BuiltInFunctionSet.Entry> populator) {
            Saxon.Hej.functions.registry.BuiltInFunctionSet.Entry e = register(name, arity, populator);
            Saxon.Impl.Helpers.MapUtils.Put(sequenceVariadicFunctions, name, (arity-1));
            return e;
        }
        public virtual Saxon.Hej.om.NamespaceUri getNamespace() {
            return Saxon.Hej.om.NamespaceUri.FN;
        }
        public virtual string getConventionalPrefix() {
            return "fn";
        }
        public class Entry : Saxon.Hej.functions.registry.FunctionDefinition {
            public Saxon.Hej.om.StructuredQName name;
            public Saxon.Ejava.util.function.Supplier<Saxon.Hej.functions.SystemFunction> implementationFactory;
            public System.Func<Saxon.Hej.functions.registry.BuiltInFunctionSet.Entry,Saxon.Hej.functions.registry.BuiltInFunctionSet.Entry> populator;
            public Saxon.Hej.functions.registry.BuiltInFunctionSet functionSet;
            public int maxArity;
            public int minArity;
            public Saxon.Hej.type.ItemType itemType;
            public int cardinality;
            public Saxon.Hej.expr.OperandUsage[] usage;
            public string[] paramNames;
            public Saxon.Hej.value.SequenceType[] paramTypes;
            public Saxon.Hej.om.Sequence[] resultIfEmpty;
            public Saxon.Hej.z.IntHashMap<Saxon.Hej.expr.Expression> defaultValueExpressions;
            public int properties;
            public Saxon.Hej.functions.OptionsParameter optionDetails;
            public void ensurePopulated() { lock(this) {
                if ((implementationFactory == null)) {
                    populator(this);
                }
            }}
            public Saxon.Hej.functions.registry.BuiltInFunctionSet.Entry populate(Saxon.Ejava.util.function.Supplier<Saxon.Hej.functions.SystemFunction> functionFactory, Saxon.Hej.type.ItemType itemType, int cardinality, int properties) {
                this.implementationFactory = functionFactory;
                this.itemType = itemType;
                this.cardinality = cardinality;
                this.properties = properties;
                if ((this.maxArity == -1)) {
                    this.paramTypes = new Saxon.Hej.value.SequenceType[1];
                    this.resultIfEmpty = new Saxon.Hej.value.AtomicValue[1];
                    this.usage = new Saxon.Hej.expr.OperandUsage[1];
                } else {
                    this.paramTypes = new Saxon.Hej.value.SequenceType[maxArity];
                    this.resultIfEmpty = new Saxon.Hej.om.Sequence[maxArity];
                    this.usage = new Saxon.Hej.expr.OperandUsage[maxArity];
                }
                if ((((properties&SEQV)) != 0)) {
                    Saxon.Impl.Helpers.MapUtils.Put(this.functionSet.sequenceVariadicFunctions, name.getLocalPart(), (this.maxArity-1));
                }
                Saxon.Hej.om.NamespaceUri ns = name.getNamespaceUri();
                System.Collections.Generic.IDictionary<string,string> paramNameMap;
                if ((ns == Saxon.Hej.om.NamespaceUri.FN)) {
                    paramNameMap = Saxon.Hej.functions.registry.ParamKeywords.fnParamNames;
                } else {
                    if ((ns == Saxon.Hej.om.NamespaceUri.MAP_FUNCTIONS)) {
                        paramNameMap = Saxon.Hej.functions.registry.ParamKeywords.mapParamNames;
                    } else {
                        if ((ns == Saxon.Hej.om.NamespaceUri.ARRAY_FUNCTIONS)) {
                            paramNameMap = Saxon.Hej.functions.registry.ParamKeywords.arrayParamNames;
                        } else {
                            if ((ns == Saxon.Hej.om.NamespaceUri.MATH)) {
                                paramNameMap = Saxon.Hej.functions.registry.ParamKeywords.mathParamNames;
                            } else {
                                paramNameMap = new System.Collections.Generic.Dictionary<string, string>();
                            }
                        }
                    }
                }
                string keywords = Saxon.Impl.Helpers.MapUtils.Get(paramNameMap, name.getLocalPart());
                if ((keywords == null)) {
                    keywords = Saxon.Impl.Helpers.MapUtils.Get(paramNameMap, ((name.getLocalPart()+"#")+maxArity));
                }
                if ((keywords == null)) {
                    keywords = "a|b|c|d|e|f";
                }
                this.paramNames = System.Text.RegularExpressions.Regex.Split(keywords, "\\|");
                return this;
            }
            public Saxon.Hej.functions.registry.BuiltInFunctionSet.Entry arg(int a, Saxon.Hej.type.ItemType type, int options, Saxon.Hej.om.Sequence resultIfEmpty) {
                return arg(a, type, options, resultIfEmpty, null);
            }
            public Saxon.Hej.functions.registry.BuiltInFunctionSet.Entry arg(int a, Saxon.Hej.type.ItemType type, int options, Saxon.Hej.om.Sequence resultIfEmpty, Saxon.Hej.expr.Expression defaultValue) {
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
                    this.paramTypes[a] = Saxon.Hej.value.SequenceType.makeSequenceType(type, cardinality);
                    this.resultIfEmpty[a] = resultIfEmpty;
                    this.usage[a] = usage;
                    if ((defaultValue != null)) {
                        withDefault(a, defaultValue);
                    }
                } catch (System.IndexOutOfRangeException err) {
                    System.Console.Error.WriteLine(((("Internal Saxon error: Can't set argument "+a)+" of ")+name));
                }
                return this;
            }
            public Saxon.Hej.functions.registry.BuiltInFunctionSet.Entry withDefault(int arg, Saxon.Hej.expr.Expression defaultValue) {
                if ((this.defaultValueExpressions == null)) {
                    this.defaultValueExpressions = new Saxon.Hej.z.IntHashMap<Saxon.Hej.expr.Expression>(getNumberOfParameters());
                }
                this.defaultValueExpressions.put(arg, defaultValue);
                return this;
            }
            public Saxon.Hej.functions.registry.BuiltInFunctionSet.Entry setOptionDetails(Saxon.Hej.functions.OptionsParameter details) {
                this.optionDetails = details;
                return this;
            }
            public Saxon.Hej.om.StructuredQName getFunctionName() {
                return name;
            }
            public int getNumberOfParameters() {
                return maxArity;
            }
            public int getMinimumArity() {
                return minArity;
            }
            public Saxon.Hej.om.StructuredQName getParameterName(int i) {
                return new Saxon.Hej.om.StructuredQName("", "", paramNames[i]);
            }
            public Saxon.Hej.expr.Expression getDefaultValueExpression(int i) {
                if ((defaultValueExpressions != null)) {
                    return defaultValueExpressions.get(i);
                } else {
                    return null;
                }
            }
            public int getPositionOfParameter(Saxon.Hej.om.StructuredQName name) {
                for (int i = 0; (i<maxArity); i++) {
                    if (paramNames[i].Equals(name.getLocalPart())) {
                        return i;
                    }
                }
                return -1;
            }
        
        }
    
        /* Expanding default method setConfiguration from interface FunctionLibrary */
        public virtual void setConfiguration(Saxon.Hej.Configuration config) {
        }

    }
}