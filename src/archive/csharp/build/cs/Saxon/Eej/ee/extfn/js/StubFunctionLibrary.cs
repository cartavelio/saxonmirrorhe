using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.ee.extfn.js {
    public class StubFunctionLibrary : Saxon.Hej.functions.FunctionLibrary {
        private readonly System.Collections.Generic.IDictionary<Saxon.Hej.trans.SymbolicName.F,Saxon.Hej.type.SpecificFunctionType> signatures = new System.Collections.Generic.Dictionary<Saxon.Hej.trans.SymbolicName.F, Saxon.Hej.type.SpecificFunctionType>();
        private readonly Saxon.Hej.Configuration config;
        public StubFunctionLibrary(Saxon.Hej.Configuration config)  {
            this.config = config;
        }
        public void loadSignatures(Saxon.Ejavax.xml.transform.stream.StreamSource input) {
            Saxon.Hej.expr.XPathContext context = config.getConversionContext();
            Saxon.Hej.str.UnicodeString content;
            try {
                System.IO.TextReader reader = Saxon.Hej.lib.StandardUnparsedTextResolver.getReaderFromStreamSource(input, "utf-8", config, config.isTiming());
                content = Saxon.Hej.functions.UnparsedTextFunction.readFile(Saxon.Hej.z.IntPredicateLambda.of((ch) => true), reader);
            } catch (System.IO.IOException ioErr) {
                throw new Saxon.Hej.trans.XPathException("Failed to read function signatures", ioErr);
            }
            Saxon.Hej.om.Item result = Saxon.Hej.ma.json.ParseJsonFn.parse(content.ToString(), Saxon.Hej.ma.json.ParseJsonFn.OPTION_DETAILS.getDefaultOptions(), context);
            if ((result is Saxon.Hej.ma.arrays.ArrayItem)) {
                foreach (Saxon.Hej.om.GroundedValue member in ((Saxon.Hej.ma.arrays.ArrayItem)(result)).members()) {
                    if ((member is Saxon.Hej.ma.map.MapItem)) {
                        loadSignatureMap((Saxon.Hej.ma.map.MapItem)(member));
                    } else {
                        badInput("Top level is an array but contents are not maps");
                    }
                }
            } else {
                if ((result is Saxon.Hej.ma.map.MapItem)) {
                    loadSignatureMap((Saxon.Hej.ma.map.MapItem)(result));
                } else {
                    badInput("Neither an array nor a map");
                }
            }
        }
        private readonly static Saxon.Hej.value.StringValue NAMESPACE = new Saxon.Hej.value.StringValue("namespace");
        private readonly static Saxon.Hej.value.StringValue SIGNATURES = new Saxon.Hej.value.StringValue("signatures");
        private readonly static Saxon.Hej.value.StringValue AS = new Saxon.Hej.value.StringValue("as");
        private readonly static Saxon.Hej.value.StringValue PARAM = new Saxon.Hej.value.StringValue("param");
        private readonly static Saxon.Hej.value.StringValue ARITY = new Saxon.Hej.value.StringValue("arity");
        private void loadSignatureMap(Saxon.Hej.ma.map.MapItem map) {
            string @namespace = map.get(NAMESPACE).getStringValue();
            Saxon.Hej.om.GroundedValue sigs = map.get(SIGNATURES);
            Saxon.Hej.expr.StaticContext env = new Saxon.Hej.sxpath.IndependentContext(config);
            if ((sigs is Saxon.Hej.ma.map.MapItem)) {
                foreach (Saxon.Hej.ma.map.KeyValuePair kvp in ((Saxon.Hej.ma.map.MapItem)(sigs)).keyValuePairs()) {
                    Saxon.Hej.om.StructuredQName name = new Saxon.Hej.om.StructuredQName("", @namespace, kvp.key.ToString());
                    expect((kvp.value is Saxon.Hej.ma.map.MapItem), (("Value of "+name)+" must be a map"));
                    Saxon.Hej.ma.map.MapItem sigValue = ((Saxon.Hej.ma.map.MapItem)(kvp.value));
                    Saxon.Hej.om.GroundedValue asValue = sigValue.get(AS);
                    expect((asValue is Saxon.Hej.value.StringValue), (("In "+name)+", as must exist and be a string"));
                    Saxon.Hej.value.SequenceType resultType = parseSequenceType(asValue.getStringValue(), env);
                    System.Collections.Generic.IList<Saxon.Hej.value.SequenceType> paramTypes = new System.Collections.Generic.List<Saxon.Hej.value.SequenceType>();
                    Saxon.Hej.om.GroundedValue @params = sigValue.get(PARAM);
                    expect((@params is Saxon.Hej.ma.arrays.ArrayItem), (("In "+name)+", param must exist and be an array"));
                    foreach (Saxon.Hej.om.GroundedValue paramDetails in ((Saxon.Hej.ma.arrays.ArrayItem)(@params)).members()) {
                        expect((paramDetails is Saxon.Hej.value.StringValue), (("In "+name)+", param members must be strings"));
                        Saxon.Hej.value.SequenceType paramType = parseSequenceType(paramDetails.getStringValue(), env);
                        paramTypes.Add(paramType);
                    }
                    Saxon.Hej.om.GroundedValue arityValue = sigValue.get(ARITY);
                    expect((arityValue is Saxon.Hej.ma.arrays.ArrayItem), (("In "+name)+", as must exist and be an array"));
                    foreach (Saxon.Hej.om.GroundedValue arity in ((Saxon.Hej.ma.arrays.ArrayItem)(arityValue)).members()) {
                        expect((arity is Saxon.Hej.value.NumericValue), (("In "+name)+", arity must be a number"));
                        int arityInt = (int)(((Saxon.Hej.value.NumericValue)(arity)).getDoubleValue());
                        expect((arityInt>=0), (("In "+name)+" arity is less than zero"));
                        Saxon.Hej.trans.SymbolicName.F functionName = new Saxon.Hej.trans.SymbolicName.F(name, arityInt);
                        expect((arityInt<=paramTypes.Count), (("In "+name)+" arity exceeds number of params"));
                        Saxon.Hej.value.SequenceType[] st = new Saxon.Hej.value.SequenceType[arityInt];
                        for (int i = 0; (i<arityInt); i++) {
                            st[i] = paramTypes[i];
                        }
                        Saxon.Hej.type.SpecificFunctionType sig = new Saxon.Hej.type.SpecificFunctionType(st, resultType);
                        Saxon.Impl.Helpers.MapUtils.Put(signatures, functionName, sig);
                    }
                }
            }
        }
        private static Saxon.Hej.value.SequenceType parseSequenceType(string designator, Saxon.Hej.expr.StaticContext env) {
            Saxon.Hej.expr.parser.XPathParser p = new Saxon.Hej.expr.parser.XPathParser(env);
            return p.parseSequenceType(designator, env);
        }
        private static void expect(bool condition, string description) {
            if (!(condition)) {
                throw new Saxon.Hej.trans.XPathException(("Invalid function signature file: "+description));
            }
        }
        private static void badInput(string message) {
            throw new Saxon.Hej.trans.XPathException("Invalid content in function signatures file", message);
        }
        public bool isAvailable(Saxon.Hej.trans.SymbolicName.F functionName, int languageLevel) {
            return signatures.ContainsKey(functionName);
        }
        public Saxon.Hej.expr.Expression bind(Saxon.Hej.trans.SymbolicName.F functionName, Saxon.Hej.expr.Expression[] staticArgs, System.Collections.Generic.IDictionary<Saxon.Hej.om.StructuredQName,int> keywords, Saxon.Hej.expr.StaticContext env, System.Collections.Generic.IList<string> reasons) {
            if (signatures.ContainsKey(functionName)) {
                Saxon.Hej.type.SpecificFunctionType type = Saxon.Impl.Helpers.MapUtils.Get(signatures, functionName);
                Saxon.Hej.lib.ExtensionFunctionDefinition defn = new Saxon.Eej.ee.extfn.js.StubFunctionLibrary.StubExtensionDefinition(functionName.getComponentName(), type);
                Saxon.Hej.lib.ExtensionFunctionCall f = defn.makeCallExpression();
                f.setDefinition(defn);
                Saxon.Hej.functions.IntegratedFunctionCall fc = new Saxon.Hej.functions.IntegratedFunctionCall(defn.getFunctionQName(), f);
                fc.setArguments(staticArgs);
                return fc;
            }
            return null;
        }
        public Saxon.Hej.functions.FunctionLibrary copy() {
            return this;
        }
        public Saxon.Hej.om.FunctionItem getFunctionItem(Saxon.Hej.trans.SymbolicName.F functionName, Saxon.Hej.expr.StaticContext staticContext) {
            if (signatures.ContainsKey(functionName)) {
                Saxon.Hej.type.SpecificFunctionType type = Saxon.Impl.Helpers.MapUtils.Get(signatures, functionName);
                Saxon.Hej.lib.ExtensionFunctionDefinition defn = new Saxon.Eej.ee.extfn.js.StubFunctionLibrary.StubExtensionDefinition(functionName.getComponentName(), type);
                try {
                    return defn.asFunction(functionName.getArity());
                } catch (System.Exception err) {
                    throw new Saxon.Hej.trans.XPathException(("Failed to create call to extension function "+functionName.getComponentName().getDisplayName()), err);
                }
            }
            return null;
        }
        private class StubExtensionDefinition : Saxon.Hej.lib.ExtensionFunctionDefinition {
            private readonly Saxon.Hej.om.StructuredQName functionName;
            private readonly Saxon.Hej.type.SpecificFunctionType functionType;
            public StubExtensionDefinition(Saxon.Hej.om.StructuredQName name, Saxon.Hej.type.SpecificFunctionType type)  {
                this.functionName = name;
                this.functionType = type;
            }
            public override Saxon.Hej.om.StructuredQName getFunctionQName() {
                return functionName;
            }
            public override Saxon.Hej.value.SequenceType[] getArgumentTypes() {
                return functionType.getArgumentTypes();
            }
            public override Saxon.Hej.value.SequenceType getResultType(Saxon.Hej.value.SequenceType[] suppliedArgumentTypes) {
                return functionType.getResultType();
            }
            public override Saxon.Hej.lib.ExtensionFunctionCall makeCallExpression() {
                return new Saxon.Eej.ee.extfn.js.StubFunctionLibrary.StubFunctionCall();
            }
        
        }
        private class StubFunctionCall : Saxon.Hej.lib.ExtensionFunctionCall {
            public override Saxon.Hej.om.Sequence call(Saxon.Hej.expr.XPathContext context, Saxon.Hej.om.Sequence[] arguments) {
                throw new Saxon.Hej.trans.XPathException((("Function "+getDefinition().getFunctionQName().getEQName())+" has no implementation"));
            }
        
        }
    
        /* Expanding default method setConfiguration from interface FunctionLibrary */
        public virtual void setConfiguration(Saxon.Hej.Configuration config) {
        }

    }
}