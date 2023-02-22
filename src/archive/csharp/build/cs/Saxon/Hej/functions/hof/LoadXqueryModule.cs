using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.functions.hof {
    public class LoadXqueryModule : Saxon.Hej.functions.SystemFunction, Saxon.Hej.expr.Callable {
        public static Saxon.Hej.functions.OptionsParameter makeOptionsParameter() {
            Saxon.Hej.functions.OptionsParameter op = new Saxon.Hej.functions.OptionsParameter();
            op.addAllowedOption("xquery-version", Saxon.Hej.value.SequenceType.SINGLE_DECIMAL);
            op.addAllowedOption("location-hints", Saxon.Hej.value.SequenceType.STRING_SEQUENCE);
            op.addAllowedOption("context-item", Saxon.Hej.value.SequenceType.OPTIONAL_ITEM);
            op.addAllowedOption("variables", Saxon.Hej.value.SequenceType.makeSequenceType(new Saxon.Hej.ma.map.MapType(Saxon.Hej.type.BuiltInAtomicType.QNAME, Saxon.Hej.value.SequenceType.ANY_SEQUENCE), Saxon.Hej.expr.StaticProperty.EXACTLY_ONE));
            op.addAllowedOption("vendor-options", Saxon.Hej.value.SequenceType.makeSequenceType(new Saxon.Hej.ma.map.MapType(Saxon.Hej.type.BuiltInAtomicType.QNAME, Saxon.Hej.value.SequenceType.ANY_SEQUENCE), Saxon.Hej.expr.StaticProperty.EXACTLY_ONE));
            return op;
        }
        public override Saxon.Hej.expr.XPathContext makeNewContext(Saxon.Hej.expr.XPathContext callingContext, Saxon.Hej.expr.ContextOriginator originator) {
            return callingContext;
        }
        public override Saxon.Hej.ma.map.MapItem call(Saxon.Hej.expr.XPathContext context, Saxon.Hej.om.Sequence[] args) {
            Saxon.Hej.om.Sequence xqueryVersionOption = null;
            Saxon.Hej.om.Sequence locationHintsOption = null;
            Saxon.Hej.om.Sequence variablesOption = null;
            Saxon.Hej.om.Sequence contextItemOption = null;
            Saxon.Hej.om.Sequence vendorOptionsOption = null;
            if ((args.Length == 2)) {
                Saxon.Hej.ma.map.MapItem suppliedOptions = (Saxon.Hej.ma.map.MapItem)(args[1].head());
                System.Collections.Generic.IDictionary<string,Saxon.Hej.om.Sequence> checkedOptions = getDetails().optionDetails.processSuppliedOptions(suppliedOptions, context);
                xqueryVersionOption = Saxon.Impl.Helpers.MapUtils.Get(checkedOptions, "xquery-version");
                if ((xqueryVersionOption != null)) {
                    double vn = ((Saxon.Hej.value.DecimalValue)(xqueryVersionOption.head())).getDoubleValue();
                    if (((vn*10)>31)) {
                        throw new Saxon.Hej.trans.XPathException((("No XQuery version "+vn)+" processor is available"), "FOQM0006");
                    }
                }
                locationHintsOption = Saxon.Impl.Helpers.MapUtils.Get(checkedOptions, "location-hints");
                variablesOption = Saxon.Impl.Helpers.MapUtils.Get(checkedOptions, "variables");
                contextItemOption = Saxon.Impl.Helpers.MapUtils.Get(checkedOptions, "context-item");
                vendorOptionsOption = Saxon.Impl.Helpers.MapUtils.Get(checkedOptions, "vendor-options");
            }
            int qv = 31;
            if ((xqueryVersionOption != null)) {
                Singulink.Numerics.BigDecimal decimalVn = ((Saxon.Hej.value.DecimalValue)(xqueryVersionOption.head())).getDecimalValue();
                if (((decimalVn.Equals(Singulink.Numerics.BigDecimal.Parse("1.0", System.Globalization.NumberStyles.Number, System.Globalization.CultureInfo.InvariantCulture)) || decimalVn.Equals(Singulink.Numerics.BigDecimal.Parse("3.0", System.Globalization.NumberStyles.Number, System.Globalization.CultureInfo.InvariantCulture))) || decimalVn.Equals(Singulink.Numerics.BigDecimal.Parse("3.1", System.Globalization.NumberStyles.Number, System.Globalization.CultureInfo.InvariantCulture)))) {
                    qv = ((int)(decimalVn*(Singulink.Numerics.BigDecimal)10));
                } else {
                    throw new Saxon.Hej.trans.XPathException(("Unsupported XQuery version "+decimalVn), "FOQM0006");
                }
            }
            Saxon.Hej.om.NamespaceUri moduleUri = Saxon.Hej.om.NamespaceUri.of(args[0].head().getStringValue());
            if (moduleUri.isEmpty()) {
                throw new Saxon.Hej.trans.XPathException("First argument of fn:load-xquery-module() must not be a zero length string", "FOQM0001");
            }
            System.Collections.Generic.IList<string> locationHints = new System.Collections.Generic.List<string>();
            if ((locationHintsOption != null)) {
                Saxon.Hej.om.SequenceIterator iterator = locationHintsOption.iterate();
                Saxon.Hej.om.Item hint;
                while (((hint = iterator.next()) != null)) {
                    locationHints.Add(hint.getStringValue());
                }
            }
            Saxon.Hej.Configuration config = context.getConfiguration();
            Saxon.Hej.query.StaticQueryContext staticQueryContext = config.newStaticQueryContext();
            Saxon.Hej.lib.ModuleURIResolver moduleURIResolver = config.getModuleURIResolver();
            if ((moduleURIResolver == null)) {
                moduleURIResolver = new Saxon.Hej.lib.StandardModuleURIResolver(config);
            }
            staticQueryContext.setModuleURIResolver(moduleURIResolver);
            string baseURI = getRetainedStaticContext().getStaticBaseUriString();
            staticQueryContext.setBaseURI(baseURI);
            Saxon.Ejavax.xml.transform.stream.StreamSource[] streamSources;
            try {
                string[] hints = locationHints.ToArray();
                streamSources = staticQueryContext.getModuleURIResolver().resolve(moduleUri.ToString(), baseURI, hints);
                if ((streamSources == null)) {
                    streamSources = new Saxon.Hej.lib.StandardModuleURIResolver(config).resolve(moduleUri.ToString(), baseURI, hints);
                }
            } catch (Saxon.Hej.trans.XPathException e) {
                e.maybeSetErrorCode("FOQM0002");
                throw;
            }
            if ((streamSources.Length == 0)) {
                throw new Saxon.Hej.trans.XPathException(("No library module found with specified target namespace "+moduleUri), "FOQM0002");
            }
            try {
                string sourceQuery = Saxon.Hej.query.QueryReader.readSourceQuery(streamSources[0], config.getValidCharacterChecker());
                staticQueryContext.compileLibrary(sourceQuery);
            } catch (Saxon.Hej.trans.XPathException e) {
                throw new Saxon.Hej.trans.XPathException(e.Message, "FOQM0003");
            }
            Saxon.Hej.query.QueryLibrary lib = staticQueryContext.getCompiledLibrary(moduleUri);
            if ((lib == null)) {
                throw new Saxon.Hej.trans.XPathException(("The library module located does not have the expected namespace "+moduleUri), "FOQM0002");
            }
            Saxon.Hej.query.QueryModule main = new Saxon.Hej.query.QueryModule(staticQueryContext);
            main.setPackageData(lib.getPackageData());
            main.setExecutable(lib.getExecutable());
            lib.link(main);
            Saxon.Hej.query.XQueryExpression xqe = new Saxon.Hej.query.XQueryExpression(new Saxon.Hej.expr.ContextItemExpression(), main, false);
            Saxon.Hej.query.DynamicQueryContext dqc = new Saxon.Hej.query.DynamicQueryContext(context.getConfiguration());
            if ((variablesOption != null)) {
                Saxon.Hej.ma.map.MapItem extVariables = (Saxon.Hej.ma.map.MapItem)(variablesOption.head());
                Saxon.Hej.tree.iter.AtomicIterator iterator = extVariables.keys();
                Saxon.Hej.value.AtomicValue key;
                while (((key = ((/*1430*/Saxon.Hej.value.AtomicValue)iterator.next())) != null)) {
                    dqc.setParameter(((Saxon.Hej.value.QNameValue)(key)).getStructuredQName(), ((Saxon.Hej.om.Sequence)(extVariables.get(key))).materialize());
                }
            }
            if ((contextItemOption != null)) {
                Saxon.Hej.om.Item contextItem = contextItemOption.head();
                Saxon.Hej.expr.instruct.GlobalContextRequirement gcr = main.getExecutable().getGlobalContextRequirement();
                if ((gcr != null)) {
                    Saxon.Hej.type.ItemType req = gcr.getRequiredItemType();
                    if (((req != null) && !(req.matches(contextItem, config.getTypeHierarchy())))) {
                        throw new Saxon.Hej.trans.XPathException(("Required context item type is "+req), "FOQM0005");
                    }
                }
                dqc.setContextItem(contextItemOption.head());
            }
            Saxon.Hej.Controller newController = xqe.newController(dqc);
            Saxon.Hej.expr.XPathContext newContext = newController.newXPathContext();
            Saxon.Hej.ma.map.MapItem variablesMap = new Saxon.Impl.DataModel.ImmMap();
            foreach (Saxon.Hej.expr.instruct.GlobalVariable var in lib.getImportedGlobalVariables()) {
                Saxon.Hej.om.GroundedValue value;
                Saxon.Hej.value.QNameValue qNameValue = new Saxon.Hej.value.QNameValue(var.getVariableQName(), Saxon.Hej.type.BuiltInAtomicType.QNAME);
                if ((qNameValue.getNamespaceURI().Equals(moduleUri) && !(var.isPrivate()))) {
                    try {
                        value = ((/*1430*/Saxon.Hej.om.GroundedValue)var.evaluateVariable(newContext));
                    } catch (Saxon.Hej.trans.XPathException e) {
                        e.setIsGlobalError(false);
                        if (e.getErrorCodeLocalPart().Equals("XPTY0004")) {
                            throw new Saxon.Hej.trans.XPathException(e.Message, "FOQM0005");
                        } else {
                            throw;
                        }
                    }
                    variablesMap = variablesMap.addEntry(qNameValue, value);
                }
            }
            Saxon.Hej.ma.map.MapItem functionsMap = new Saxon.Impl.DataModel.ImmMap();
            Saxon.Hej.query.XQueryFunctionLibrary functionLib = lib.getGlobalFunctionLibrary();
            Saxon.Hej.expr.ExportAgent agent = new Inner_ExportAgent_36(this);
            foreach (Saxon.Hej.query.XQueryFunction function in functionLib.getFunctionDefinitions()) {
                Saxon.Hej.ma.map.MapItem newMap;
                Saxon.Hej.value.QNameValue functionQName = new Saxon.Hej.value.QNameValue(function.getFunctionName(), Saxon.Hej.type.BuiltInAtomicType.QNAME);
                if ((functionQName.getNamespaceURI().Equals(moduleUri) && !(function.isPrivate()))) {
                    Saxon.Hej.expr.instruct.UserFunction userFunction = function.getUserFunction();
                    Saxon.Hej.functions.hof.UserFunctionReference.BoundUserFunction buf = new Saxon.Hej.functions.hof.UserFunctionReference.BoundUserFunction(userFunction, userFunction.getArity(), null, agent, newController);
                    if ((functionsMap.get(functionQName) != null)) {
                        newMap = ((Saxon.Hej.ma.map.MapItem)(functionsMap.get(functionQName))).addEntry(new Saxon.Hej.value.Int64Value(function.getNumberOfParameters()), buf);
                    } else {
                        newMap = new Saxon.Hej.ma.map.SingleEntryMap(Saxon.Hej.value.Int64Value.makeIntegerValue(function.getNumberOfParameters()), buf);
                    }
                    functionsMap = functionsMap.addEntry(functionQName, newMap);
                }
            }
            Saxon.Hej.ma.map.DictionaryMap map = new Saxon.Hej.ma.map.DictionaryMap();
            map.initialPut("variables", variablesMap);
            map.initialPut("functions", functionsMap);
            return map;
        }
    
        private class Inner_ExportAgent_36 : Saxon.Hej.expr.ExportAgent {
            private readonly LoadXqueryModule outerInstance;
            public Inner_ExportAgent_36(LoadXqueryModule outerInstance) {
                this.outerInstance = outerInstance;
            }
            public void export(Saxon.Hej.trace.ExpressionPresenter @out) {
                Saxon.Hej.trans.XPathException err = new Saxon.Hej.trans.XPathException("Cannot export a stylesheet that statically incorporates XQuery functions", Saxon.Hej.trans.SaxonErrorCode.SXST0069);
                /*localvar*/err.setIsStaticError(true);
                throw /*localvar*/err;
            }
        }

    }
}