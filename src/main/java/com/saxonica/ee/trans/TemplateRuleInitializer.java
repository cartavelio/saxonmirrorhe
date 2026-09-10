package com.saxonica.ee.trans;


import com.saxonica.ee.stream.Streamability;
import com.saxonica.ee.stream.Sweep;
import net.sf.saxon.expr.ErrorExpression;
import net.sf.saxon.expr.Expression;
import net.sf.saxon.expr.instruct.TemplateRule;
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

    private Compilation compilation;
    private XSLTemplate xslTemplate;
    private ComponentDeclaration decl;
    private StructuredQName modeName;
    private boolean firstTime = true;

    public TemplateRuleInitializer(Compilation comp, ComponentDeclaration decl, StructuredQName modeName) {
        compilation = comp;
        this.decl = decl;
        this.modeName = modeName;
        xslTemplate = (XSLTemplate) decl.getSourceElement();
    }

    public synchronized void init() throws XPathException {
        if (firstTime) {
            TemplateRule rule = xslTemplate.getTemplateRulesByMode().get(modeName);
            try {
                xslTemplate.jitCompile(compilation, decl);

                ExpressionVisitor visitor = new ExpressionVisitor(compilation.getConfiguration());
                visitor.setStaticContext(xslTemplate.getStaticContext());
                ContextItemStaticInfo cisi = new ContextItemStaticInfo(
                        xslTemplate.getContextItemTypeForTemplateRule(), false);

                RuleManager mgr = compilation.getPrincipalStylesheetModule().getRuleManager();
                Mode m = mgr.obtainMode(modeName, false);
                visitor.setOptimizeForStreaming(m.isDeclaredStreamable());
                Expression body = rule.getBody().typeCheck(visitor, cisi);
                rule.setBody(ExpressionTool.optimizeComponentBody(
                        body, compilation, visitor, cisi, false));
                SimpleMode.allocateBindingSlotsRecursive(xslTemplate.getPackageData(), m, rule.getBody(), m.getDeclaringComponent().getComponentBindings());
                ExpressionTool.allocateSlots(rule.getBody(), rule.getStackFrameMap().getNumberOfVariables(), rule.getStackFrameMap());

                if (m.isDeclaredStreamable()) {
                    List<String> reasons = new ArrayList<String>();
                    if (!((TemplateRuleEE) rule).isActuallyStreamable(reasons)) {
                        String message = "Template rule is not streamable";
                        for (String reason : reasons) {
                            message += "\n  * " + reason;
                        }
                        throw new XPathException(message, "XTSE3430", rule);
                    }
                    if (Streamability.getSweep(rule.getBody()) == Sweep.CONSUMING) {
                        ((TemplateRuleEE) rule).makeInversion();
                    }
                }
            } catch (XPathException e) {
                rule.setBody(new ErrorExpression(e));
                firstTime = false;
                throw e;
            }
            firstTime = false;
        }
    }

}

// Copyright (c) 2018 Saxonica Limited.
