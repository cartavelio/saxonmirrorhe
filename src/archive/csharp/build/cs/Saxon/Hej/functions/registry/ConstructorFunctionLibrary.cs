using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.functions.registry {
    public class ConstructorFunctionLibrary : Saxon.Hej.functions.FunctionLibrary {
        private readonly Saxon.Hej.Configuration config;
        public ConstructorFunctionLibrary(Saxon.Hej.Configuration config)  {
            this.config = config;
        }
        public Saxon.Hej.om.Function getFunctionItem(Saxon.Hej.trans.SymbolicName.F functionName, Saxon.Hej.expr.StaticContext staticContext) {
            if ((functionName.getArity() != 1)) {
                return null;
            }
            string uri = functionName.getComponentName().getURI();
            string localName = functionName.getComponentName().getLocalPart();
            Saxon.Hej.type.SchemaType type = config.getSchemaType(new Saxon.Hej.om.StructuredQName("", uri, localName));
            if (((type == null) || type.isComplexType())) {
                return null;
            }
            Saxon.Hej.om.NamespaceResolver resolver = (((Saxon.Hej.type.SimpleType)(type)).isNamespaceSensitive() ? staticContext.getNamespaceResolver() : null);
            if ((type is Saxon.Hej.type.AtomicType)) {
                return new Saxon.Hej.functions.hof.AtomicConstructorFunction((Saxon.Hej.type.AtomicType)(type), resolver);
            } else {
                if ((type is Saxon.Hej.type.ListType)) {
                    return new Saxon.Hej.expr.ListConstructorFunction((Saxon.Hej.type.ListType)(type), resolver, true);
                } else {
                    Saxon.Hej.expr.Callable callable = new Saxon.Hej.expr.CallableDelegate((context, arguments) => {
                        Saxon.Hej.value.AtomicValue value = (Saxon.Hej.value.AtomicValue)(arguments[0].head());
                        if ((value == null)) {
                            return Saxon.Hej.value.EmptySequence.getInstance();
                        }
                        return Saxon.Hej.expr.UnionConstructorFunction.cast(value, (Saxon.Hej.type.UnionType)(type), resolver, context.getConfiguration().getConversionRules());
                    } );
                    Saxon.Hej.value.SequenceType returnType = ((Saxon.Hej.type.UnionType)(type)).getResultTypeOfCast();
                    return new Saxon.Hej.functions.CallableFunction(1, callable, new Saxon.Hej.type.SpecificFunctionType(new Saxon.Hej.value.SequenceType[]{Saxon.Hej.value.SequenceType.OPTIONAL_ATOMIC}, returnType));
                }
            }
        }
        public bool isAvailable(Saxon.Hej.trans.SymbolicName.F functionName, int languageLevel) {
            if ((functionName.getArity() != 1)) {
                return false;
            }
            string uri = functionName.getComponentName().getURI();
            string localName = functionName.getComponentName().getLocalPart();
            Saxon.Hej.type.SchemaType type = config.getSchemaType(new Saxon.Hej.om.StructuredQName("", uri, localName));
            if (((type == null) || type.isComplexType())) {
                return false;
            }
            if ((type.isAtomicType() && ((Saxon.Hej.type.AtomicType)(type)).isAbstract())) {
                return false;
            }
            return (type != Saxon.Hej.type.AnySimpleType.getInstance());
        }
        public Saxon.Hej.expr.Expression bind(Saxon.Hej.trans.SymbolicName.F functionName, Saxon.Hej.expr.Expression[] arguments, Saxon.Hej.expr.StaticContext env, System.Collections.Generic.IList<string> reasons) {
            string uri = functionName.getComponentName().getURI();
            string localName = functionName.getComponentName().getLocalPart();
            bool builtInNamespace = uri.Equals(Saxon.Hej.lib.NamespaceConstant.SCHEMA);
            if (builtInNamespace) {
                if ((functionName.getArity() != 1)) {
                    reasons.Add("A constructor function must have exactly one argument");
                    return null;
                }
                Saxon.Hej.type.SimpleType type = Saxon.Hej.type.Type.getBuiltInSimpleType(uri, localName);
                if ((type != null)) {
                    if (type.isAtomicType()) {
                        if (((Saxon.Hej.type.AtomicType)(type)).isAbstract()) {
                            reasons.Add(((("Abstract type used in constructor function: {"+uri)+'}')+localName));
                            return null;
                        } else {
                            Saxon.Hej.expr.CastExpression cast = new Saxon.Hej.expr.CastExpression(arguments[0], (Saxon.Hej.type.AtomicType)(type), true);
                            if ((arguments[0] is Saxon.Hej.expr.StringLiteral)) {
                                cast.setOperandIsStringLiteral(true);
                            }
                            return cast;
                        }
                    } else {
                        if (type.isUnionType()) {
                            Saxon.Hej.om.NamespaceResolver resolver = env.getNamespaceResolver();
                            Saxon.Hej.expr.UnionConstructorFunction ucf = new Saxon.Hej.expr.UnionConstructorFunction((Saxon.Hej.type.UnionType)(type), resolver, true);
                            return new Saxon.Hej.expr.StaticFunctionCall(ucf, arguments);
                        } else {
                            Saxon.Hej.om.NamespaceResolver resolver = env.getNamespaceResolver();
                            try {
                                Saxon.Hej.expr.ListConstructorFunction lcf = new Saxon.Hej.expr.ListConstructorFunction((Saxon.Hej.type.ListType)(type), resolver, true);
                                return new Saxon.Hej.expr.StaticFunctionCall(lcf, arguments);
                            } catch (Saxon.Hej.type.MissingComponentException e) {
                                reasons.Add(("Missing schema component: "+e.Message));
                                return null;
                            }
                        }
                    }
                } else {
                    reasons.Add(((("Unknown constructor function: {"+uri)+'}')+localName));
                    return null;
                }
            }
            if ((arguments.Length == 1)) {
                Saxon.Hej.type.SchemaType st = config.getSchemaType(new Saxon.Hej.om.StructuredQName("", uri, localName));
                if ((st is Saxon.Hej.type.SimpleType)) {
                    if ((st is Saxon.Hej.type.AtomicType)) {
                        return new Saxon.Hej.expr.CastExpression(arguments[0], (Saxon.Hej.type.AtomicType)(st), true);
                    } else {
                        if (((st is Saxon.Hej.type.ListType) && (env.getXPathVersion()>=30))) {
                            Saxon.Hej.om.NamespaceResolver resolver = env.getNamespaceResolver();
                            try {
                                Saxon.Hej.expr.ListConstructorFunction lcf = new Saxon.Hej.expr.ListConstructorFunction((Saxon.Hej.type.ListType)(st), resolver, true);
                                return new Saxon.Hej.expr.StaticFunctionCall(lcf, arguments);
                            } catch (Saxon.Hej.type.MissingComponentException e) {
                                reasons.Add(("Missing schema component: "+e.Message));
                                return null;
                            }
                        } else {
                            if ((((Saxon.Hej.type.SimpleType)(st)).isUnionType() && (env.getXPathVersion()>=30))) {
                                Saxon.Hej.om.NamespaceResolver resolver = env.getNamespaceResolver();
                                Saxon.Hej.expr.UnionConstructorFunction ucf = new Saxon.Hej.expr.UnionConstructorFunction((Saxon.Hej.type.UnionType)(st), resolver, true);
                                return new Saxon.Hej.expr.StaticFunctionCall(ucf, arguments);
                            }
                        }
                    }
                }
            }
            return null;
        }
        public Saxon.Hej.functions.FunctionLibrary copy() {
            return this;
        }
    
        /* Expanding default method setConfiguration from interface FunctionLibrary */
        public virtual void setConfiguration(Saxon.Hej.Configuration config) {
        }

    }
}