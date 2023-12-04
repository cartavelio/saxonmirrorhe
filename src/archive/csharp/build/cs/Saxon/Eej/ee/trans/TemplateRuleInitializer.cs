using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.ee.trans {
    public class TemplateRuleInitializer {
        private readonly Saxon.Hej.style.Compilation compilation;
        private readonly Saxon.Hej.style.XSLTemplate xslTemplate;
        private readonly Saxon.Hej.style.ComponentDeclaration decl;
        private readonly Saxon.Hej.om.StructuredQName modeName;
        private readonly Saxon.Eej.ee.trans.TemplateRuleEE templateRule;
        private bool firstTime = true;
        internal TemplateRuleInitializer(Saxon.Hej.style.Compilation comp, Saxon.Hej.style.ComponentDeclaration decl, Saxon.Eej.ee.trans.TemplateRuleEE templateRule)  {
            compilation = comp;
            this.decl = decl;
            this.templateRule = templateRule;
            this.modeName = templateRule.getMode().getModeName();
            xslTemplate = (Saxon.Hej.style.XSLTemplate)(decl.getSourceElement());
        }
        public void init() { lock(this) {
            if (firstTime) {
                try {
                    xslTemplate.jitCompile(compilation, decl);
                    Saxon.Hej.expr.parser.ExpressionVisitor visitor = new Saxon.Hej.expr.parser.ExpressionVisitor(compilation.getConfiguration());
                    visitor.setStaticContext(xslTemplate.getStaticContext());
                    Saxon.Hej.expr.parser.ContextItemStaticInfo cisi = new Saxon.Hej.expr.parser.ContextItemStaticInfo(xslTemplate.getContextItemTypeForTemplateRule(), false);
                    Saxon.Hej.trans.rules.RuleManager mgr = compilation.getPrincipalStylesheetModule().getRuleManager();
                    Saxon.Hej.trans.Mode m = mgr.obtainMode(modeName, false);
                    visitor.setOptimizeForStreaming(m.isDeclaredStreamable());
                    Saxon.Hej.expr.Expression body = templateRule.getBody().typeCheck(visitor, cisi);
                    templateRule.setBody(Saxon.Hej.expr.parser.ExpressionTool.optimizeComponentBody(body, compilation, visitor, cisi, false));
                    Saxon.Hej.trans.SimpleMode.allocateBindingSlotsRecursive(xslTemplate.getPackageData(), m, templateRule.getBody(), m.getDeclaringComponent().getComponentBindings());
                    if ((compilation.getCompilerInfo().getCodeInjector() != null)) {
                        compilation.getCompilerInfo().getCodeInjector().process(templateRule);
                    }
                    Saxon.Hej.expr.parser.ExpressionTool.allocateSlots(templateRule.getBody(), templateRule.getStackFrameMap().getNumberOfVariables(), templateRule.getStackFrameMap());
                    if (m.isDeclaredStreamable()) {
                        System.Collections.Generic.IList<string> reasons = new System.Collections.Generic.List<string>();
                        if (!(templateRule.isActuallyStreamable(reasons))) {
                            Saxon.Ejava.lang.StringBuilder message = new Saxon.Ejava.lang.StringBuilder("Template rule is not streamable");
                            foreach (string reason in reasons) {
                                message.append("\n  * ").append(reason);
                            }
                            throw new Saxon.Hej.trans.XPathException(message.ToString(), "XTSE3430", templateRule);
                        }
                        if ((Saxon.Eej.ee.stream.Streamability.getSweep(templateRule.getBody()) == Saxon.Eej.ee.stream.Sweep.CONSUMING)) {
                            templateRule.makeInversion();
                        }
                    }
                } catch (Saxon.Hej.trans.XPathException e) {
                    templateRule.setBody(new Saxon.Hej.expr.ErrorExpression(new Saxon.Hej.trans.XmlProcessingException(e)));
                    firstTime = false;
                    throw;
                }
                firstTime = false;
            }
        }}
    
    }
}