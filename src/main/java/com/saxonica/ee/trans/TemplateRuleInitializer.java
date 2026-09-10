package com.saxonica.ee.trans;


import com.saxonica.ee.stream.Streamability;
import com.saxonica.ee.stream.Sweep;
import net.sf.saxon.expr.ErrorExpression;
import net.sf.saxon.expr.Expression;
import net.sf.saxon.expr.parser.ContextItemStaticInfo;
import net.sf.saxon.expr.parser.ExpressionTool;
import net.sf.saxon.expr.parser.ExpressionVisitor;
import net.sf.saxon.om.StructuredQName;
import net.sf.saxon.style.Compilation;
import net.sf.saxon.style.ComponentDeclaration;
import net.sf.saxon.style.XSLTemplate;
import net.sf.saxon.trans.Mode;
import net.sf.saxon.trans.SimpleMode;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.trans.XmlProcessingException;
import net.sf.saxon.trans.rules.RuleManager;

import java.util.ArrayList;
import java.util.List;

/**
 * A TemplateRuleInitializer is used to prepare a template rule for execution the first
 * time that its match pattern is matched, in cases where JIT template rule compilation
 * is in force. The class is responsible for compiling the template rule body to an
 * expression tree, performing static type checking and optimization on the template rule,
 * analyzing its streamability and constructing an inversion for streamed evaluation,
 * and allocating slot numbers for local variables and external component references.
 */

public class TemplateRuleInitializer {

    private final Compilation compilation;
    private final XSLTemplate xslTemplate;
    private final ComponentDeclaration decl;
    private final StructuredQName modeName;
    private final TemplateRuleEE templateRule;
    private boolean firstTime = true;

    TemplateRuleInitializer(Compilation comp, ComponentDeclaration decl, TemplateRuleEE templateRule) {
        compilation = comp;
        this.decl = decl;
        this.templateRule = templateRule;
        this.modeName = templateRule.getMode().getModeName();
        xslTemplate = (XSLTemplate) decl.getSourceElement();
    }

    public synchronized void init() throws XPathException {
        if (firstTime) {
            try {
                xslTemplate.jitCompile(compilation, decl);

                ExpressionVisitor visitor = new ExpressionVisitor(compilation.getConfiguration());
                visitor.setStaticContext(xslTemplate.getStaticContext());
                ContextItemStaticInfo cisi = new ContextItemStaticInfo(
                        xslTemplate.getContextItemTypeForTemplateRule(), false);

                RuleManager mgr = compilation.getPrincipalStylesheetModule().getRuleManager();
                Mode m = mgr.obtainMode(modeName, false);
                visitor.setOptimizeForStreaming(m.isDeclaredStreamable());
                Expression body = templateRule.getBody().typeCheck(visitor, cisi);
                templateRule.setBody(ExpressionTool.optimizeComponentBody(
                        body, compilation, visitor, cisi, false));
                SimpleMode.allocateBindingSlotsRecursive(
                        xslTemplate.getPackageData(), m, templateRule.getBody(),
                        m.getDeclaringComponent().getComponentBindings());
                if (compilation.getCompilerInfo().getCodeInjector() != null) {
                    compilation.getCompilerInfo().getCodeInjector().process(templateRule);
                }
                ExpressionTool.allocateSlots(
                        templateRule.getBody(),
                        templateRule.getStackFrameMap().getNumberOfVariables(),
                        templateRule.getStackFrameMap());

                if (m.isDeclaredStreamable()) {
                    List<String> reasons = new ArrayList<>();
                    if (!templateRule.isActuallyStreamable(reasons)) {
                        StringBuilder message = new StringBuilder("Template rule is not streamable");
                        for (String reason : reasons) {
                            message.append("\n  * ").append(reason);
                        }
                        throw new XPathException(message.toString(), "XTSE3430", templateRule);
                    }
                    if (Streamability.getSweep(templateRule.getBody()) == Sweep.CONSUMING) {
                        templateRule.makeInversion();
                    }
                }
            } catch (XPathException e) {
                templateRule.setBody(new ErrorExpression(new XmlProcessingException(e)));
                firstTime = false;
                throw e;
            }
            firstTime = false;
        }
    }

}

// Copyright (c) 2018-2023 Saxonica Limited
