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
        public Saxon.Hej.om.Function asFunction() {
            return new Saxon.Hej.lib.ExtensionFunctionDefinition.ExtensionFunction(this);
        }
        private class ExtensionFunction : Saxon.Hej.functions.AbstractFunction {
            private readonly Saxon.Hej.lib.ExtensionFunctionDefinition definition;
            public ExtensionFunction(Saxon.Hej.lib.ExtensionFunctionDefinition definition)  {
                this.definition = definition;
            }
            public override Saxon.Hej.om.Sequence call(Saxon.Hej.expr.XPathContext context, Saxon.Hej.om.Sequence[] args) {
                return definition.makeCallExpression().call(context, args);
            }
            public override Saxon.Hej.type.FunctionItemType getFunctionItemType() {
                return new Saxon.Hej.type.SpecificFunctionType(definition.getArgumentTypes(), definition.getResultType(definition.getArgumentTypes()));
            }
            public override Saxon.Hej.om.StructuredQName getFunctionName() {
                return definition.getFunctionQName();
            }
            public override int getArity() {
                return definition.getArgumentTypes().Length;
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