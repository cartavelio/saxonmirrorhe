using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.functions {
    public class EnvironmentVariable : Saxon.Hej.functions.SystemFunction {
        public override Saxon.Hej.om.Sequence call(Saxon.Hej.expr.XPathContext context, Saxon.Hej.om.Sequence[] arguments) {
            Saxon.Ejava.util.Optional<Saxon.Hej.value.StringValue> value = getVariable((Saxon.Hej.value.StringValue)(arguments[0].head()), context);
            if (value.isPresent()) {
                return value.get();
            } else {
                return Saxon.Hej.value.EmptySequence.getInstance();
            }
        }
        private static Saxon.Ejava.util.Optional<Saxon.Hej.value.StringValue> getVariable(Saxon.Hej.value.StringValue environVar, Saxon.Hej.expr.XPathContext context) {
            Saxon.Hej.lib.EnvironmentVariableResolver resolver = context.getConfiguration().getConfigurationProperty(Saxon.Hej.lib.Feature<Saxon.Hej.lib.EnvironmentVariableResolver>.ENVIRONMENT_VARIABLE_RESOLVER);
            string environVarName = environVar.getStringValue();
            string environValue = "";
            if (context.getConfiguration().getBooleanProperty(Saxon.Hej.lib.Feature<bool>.ALLOW_EXTERNAL_FUNCTIONS)) {
                try {
                    environValue = resolver.getEnvironmentVariable(environVarName);
                    if ((environValue == null)) {
                        return Saxon.Ejava.util.Optional<Saxon.Hej.value.StringValue>.empty();
                    }
                } catch (Exception e) when (e is Saxon.Ejava.lang.SecurityException || e is Saxon.Ejava.lang.NullPointerException) {
                }
            }
            return Saxon.Ejava.util.Optional<Saxon.Hej.value.StringValue>.of(new Saxon.Hej.value.StringValue(environValue));
        }
    
    }
}