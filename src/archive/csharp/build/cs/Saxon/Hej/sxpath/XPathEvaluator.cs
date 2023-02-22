using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.sxpath {
    public class XPathEvaluator {
        private Saxon.Hej.sxpath.XPathStaticContext staticContext;
        public XPathEvaluator(Saxon.Hej.Configuration config)  {
            staticContext = new Saxon.Hej.sxpath.IndependentContext(config);
        }
        public Saxon.Hej.Configuration getConfiguration() {
            return staticContext.getConfiguration();
        }
        public void setStaticContext(Saxon.Hej.sxpath.XPathStaticContext context) {
            staticContext = context;
        }
        public Saxon.Hej.sxpath.XPathStaticContext getStaticContext() {
            return staticContext;
        }
        public Saxon.Hej.sxpath.XPathExpression createExpression(string expression) {
            Saxon.Hej.Configuration config = getConfiguration();
            Saxon.Hej.expr.instruct.Executable exec = new Saxon.Hej.expr.instruct.Executable(config);
            exec.setTopLevelPackage(staticContext.getPackageData());
            exec.setSchemaAware(staticContext.getPackageData().isSchemaAware());
            exec.setHostLanguage(Saxon.Api.HostLanguage.XPATH);
            Saxon.Hej.functions.FunctionLibrary userlib = exec.getFunctionLibrary();
            Saxon.Hej.functions.FunctionLibraryList lib = new Saxon.Hej.functions.FunctionLibraryList();
            lib.addFunctionLibrary(Saxon.Hej.functions.registry.XPath31FunctionSet.getInstance());
            lib.addFunctionLibrary(config.getBuiltInExtensionLibraryList(31));
            lib.addFunctionLibrary(new Saxon.Hej.functions.registry.ConstructorFunctionLibrary(config));
            lib.addFunctionLibrary(config.getIntegratedFunctionLibrary());
            config.addExtensionBinders(lib);
            if ((userlib != null)) {
                lib.addFunctionLibrary(userlib);
            }
            exec.setFunctionLibrary(lib);
            Saxon.Hej.expr.parser.Optimizer opt = config.obtainOptimizer();
            Saxon.Hej.expr.Expression exp = Saxon.Hej.expr.parser.ExpressionTool.make(expression, staticContext, 0, -1, null);
            Saxon.Hej.expr.parser.RetainedStaticContext rsc = staticContext.makeRetainedStaticContext();
            exp.setRetainedStaticContext(rsc);
            Saxon.Hej.expr.parser.ExpressionVisitor visitor = Saxon.Hej.expr.parser.ExpressionVisitor.make(staticContext);
            Saxon.Hej.type.ItemType contextItemType = staticContext.getRequiredContextItemType();
            Saxon.Hej.expr.parser.ContextItemStaticInfo cit = config.makeContextItemStaticInfo(contextItemType, true);
            cit.setParentless(staticContext.isContextItemParentless());
            exp = exp.typeCheck(visitor, cit);
            if (opt.isOptionSet(Saxon.Hej.expr.parser.OptimizerOptions.MISCELLANEOUS)) {
                exp = exp.optimize(visitor, cit);
            }
            if (opt.isOptionSet(Saxon.Hej.expr.parser.OptimizerOptions.LOOP_LIFTING)) {
                exp.setParentExpression(null);
                exp = Saxon.Hej.expr.parser.LoopLifter.process(exp, visitor, cit);
            }
            exp = postProcess(exp, visitor, cit);
            exp.setRetainedStaticContext(rsc);
            Saxon.Hej.expr.instruct.SlotManager map = staticContext.getStackFrameMap();
            int numberOfExternalVariables = map.getNumberOfVariables();
            Saxon.Hej.expr.parser.ExpressionTool.allocateSlots(exp, numberOfExternalVariables, map);
            Saxon.Hej.sxpath.XPathExpression xpe = new Saxon.Hej.sxpath.XPathExpression(staticContext, exp, exec);
            xpe.setStackFrameMap(map, numberOfExternalVariables);
            return xpe;
        }
        protected internal virtual Saxon.Hej.expr.Expression postProcess(Saxon.Hej.expr.Expression exp, Saxon.Hej.expr.parser.ExpressionVisitor visitor, Saxon.Hej.expr.parser.ContextItemStaticInfo cit) {
            return exp;
        }
        public Saxon.Hej.sxpath.XPathExpression createPattern(string pattern) {
            Saxon.Hej.Configuration config = getConfiguration();
            Saxon.Hej.expr.instruct.Executable exec = new Saxon.Hej.expr.instruct.Executable(config);
            Saxon.Hej.pattern.Pattern pat = Saxon.Hej.pattern.Pattern.make(pattern, staticContext, new Saxon.Hej.expr.PackageData(config));
            Saxon.Hej.expr.parser.ExpressionVisitor visitor = Saxon.Hej.expr.parser.ExpressionVisitor.make(staticContext);
            pat.typeCheck(visitor, config.makeContextItemStaticInfo(Saxon.Hej.type.Type.NODE_TYPE, true));
            Saxon.Hej.expr.instruct.SlotManager map = staticContext.getStackFrameMap();
            int slots = map.getNumberOfVariables();
            slots = pat.allocateSlots(map, slots);
            Saxon.Hej.sxpath.XPathExpression xpe = new Saxon.Hej.sxpath.XPathExpression(staticContext, pat, exec);
            xpe.setStackFrameMap(map, slots);
            return xpe;
        }
    
    }
}