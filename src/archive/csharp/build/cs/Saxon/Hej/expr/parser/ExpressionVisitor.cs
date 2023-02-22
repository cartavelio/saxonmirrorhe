using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.expr.parser {
    public class ExpressionVisitor {
        private Saxon.Hej.expr.StaticContext staticContext;
        private bool optimizeForStreaming = false;
        private bool optimizeForPatternMatching = false;
        private readonly Saxon.Hej.Configuration config;
        private Saxon.Hej.expr.parser.Optimizer optimizer;
        private int depth = 0;
        private bool inliningFunctions = false;
        private bool suppressWarnings = false;
        private const int MAX_DEPTH = 500;
        public ExpressionVisitor(Saxon.Hej.Configuration config)  {
            this.config = config;
        }
        public Saxon.Hej.Configuration getConfiguration() {
            return config;
        }
        public Saxon.Hej.expr.StaticContext getStaticContext() {
            return staticContext;
        }
        public void setStaticContext(Saxon.Hej.expr.StaticContext staticContext) {
            this.staticContext = staticContext;
        }
        public static Saxon.Hej.expr.parser.ExpressionVisitor make(Saxon.Hej.expr.StaticContext env) {
            Saxon.Hej.expr.parser.ExpressionVisitor visitor = new Saxon.Hej.expr.parser.ExpressionVisitor(env.getConfiguration());
            visitor.setStaticContext(env);
            return visitor;
        }
        public void issueWarning(string message, string errorCode, Saxon.Hej.s9api.Location locator) {
            if (!(isSuppressWarnings())) {
                staticContext.issueWarning(message, errorCode, locator);
            }
        }
        public Saxon.Hej.expr.XPathContext makeDynamicContext() {
            return staticContext.makeEarlyEvaluationContext();
        }
        public Saxon.Hej.expr.parser.Optimizer obtainOptimizer() {
            if ((optimizer == null)) {
                optimizer = config.obtainOptimizer(staticContext.getOptimizerOptions());
            }
            return optimizer;
        }
        public void setOptimizeForStreaming(bool option) {
            optimizeForStreaming = option;
        }
        public bool isOptimizeForStreaming() {
            return optimizeForStreaming;
        }
        public void setOptimizeForPatternMatching(bool option) {
            optimizeForPatternMatching = option;
        }
        public bool isOptimizeForPatternMatching() {
            return optimizeForPatternMatching;
        }
        public string getTargetEdition() {
            return staticContext.getPackageData().getTargetEdition();
        }
        public bool incrementAndTestDepth() {
            return (depth++<MAX_DEPTH);
        }
        public void decrementDepth() {
            depth--;
        }
        public bool isSuppressWarnings() {
            return suppressWarnings;
        }
        public void setSuppressWarnings(bool suppressWarnings) {
            this.suppressWarnings = suppressWarnings;
        }
        public bool isInliningFunctions() {
            return inliningFunctions;
        }
        public void setInliningFunctions(bool inliningFunctions) {
            this.inliningFunctions = inliningFunctions;
        }
    
    }
}