using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.lib {
    public abstract class ExtensionFunctionDefinition {
        public abstract Saxon.Hej.om.StructuredQName getFunctionQName();
        public virtual int getMinimumNumberOfArguments() {
            return getArgumentTypes().Length;
        }
        public virtual int getMaximumNumberOfArguments() {
            return getMinimumNumberOfArguments();
        }
        public abstract Saxon.Hej.value.SequenceType[] getArgumentTypes();
        public abstract Saxon.Hej.value.SequenceType getResultType(Saxon.Hej.value.SequenceType[] suppliedArgumentTypes);
        public bool trustResultType() {
            return false;
        }
        public bool dependsOnFocus() {
            return false;
        }
        public bool hasSideEffects() {
            return false;
        }
        public abstract Saxon.Hej.lib.ExtensionFunctionCall makeCallExpression();
        public Saxon.Hej.om.FunctionItem asFunction(int arity) {
            return new Saxon.Hej.lib.ExtensionFunctionDefinition.ExtensionFunction(this, arity);
        }
        private class ExtensionFunction : Saxon.Hej.functions.AbstractFunction {
            private readonly Saxon.Hej.lib.ExtensionFunctionDefinition definition;
            private readonly int arity;
            public ExtensionFunction(Saxon.Hej.lib.ExtensionFunctionDefinition definition, int arity)  {
                this.definition = definition;
                this.arity = arity;
            }
            public override Saxon.Hej.om.Sequence call(Saxon.Hej.expr.XPathContext context, Saxon.Hej.om.Sequence[] args) {
                if ((args.Length != arity)) {
                    throw new Saxon.Hej.trans.XPathException(("Wrong number of arguments in call to "+definition.getFunctionQName().getDisplayName()), "XPTY0004");
                }
                return definition.makeCallExpression().call(context, args);
            }
            public override Saxon.Hej.type.FunctionItemType getFunctionItemType() {
                return new Saxon.Hej.type.SpecificFunctionType(definition.getArgumentTypes(), definition.getResultType(definition.getArgumentTypes()));
            }
            public override Saxon.Hej.om.StructuredQName getFunctionName() {
                return definition.getFunctionQName();
            }
            public override int getArity() {
                return this.arity;
            }
            public override string getDescription() {
                return definition.getFunctionQName().getDisplayName();
            }
            public override bool isTrustedResultType() {
                return definition.trustResultType();
            }
        
        }
    
    }
}