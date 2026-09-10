package com.saxonica.xsltextn.style;

import com.saxonica.xsltextn.instruct.Assign;
import net.sf.saxon.Configuration;
import net.sf.saxon.expr.Expression;
import net.sf.saxon.expr.instruct.GlobalVariable;
import net.sf.saxon.lib.Feature;
import net.sf.saxon.om.StructuredQName;
import net.sf.saxon.style.*;
import net.sf.saxon.trans.SaxonErrorCode;
import net.sf.saxon.trans.XPathException;

import java.util.EnumSet;

/**
 * saxon:assign element in stylesheet.
 * The saxon:assign element has mandatory attribute name and optional attribute select.
 * It also allows xsl:extension-element-prefixes etc.
 */

public class SaxonAssign extends XSLGeneralVariable {

    private Assign instruction;

    private final static EnumSet<SourceBinding.BindingProperty> allowedAttributes =
            EnumSet.of(SourceBinding.BindingProperty.SELECT);

    public SaxonAssign() {
    }

    @Override
    protected void prepareAttributes() {
        sourceBinding.prepareAttributes(allowedAttributes);
    }

    /**
     * Determine whether this node is an instruction.
     *
     * @return true - it is an instruction
     */

    @Override
    public boolean isInstruction() {
        return true;
    }

    /**
     * Ask whether the element is in the XSLT namespace
     *
     * @return true if the element is in the XSLT namespace
     */
    @Override
    public boolean isInXsltNamespace() {
        return false;
    }

    @Override
    public void validate(ComponentDeclaration decl) throws XPathException {
        super.validate(decl);
        instruction = new Assign(makeExpression("()", null));
        getConfiguration().checkLicensedFeature(Configuration.LicenseFeature.PROFESSIONAL_EDITION,
                                                "saxon:assign",
                                                getPackageData().getLocalLicenseId());
        if (getConfiguration().getBooleanProperty(Feature.ALLOW_MULTITHREADING)) {
            issueWarning("saxon:assign should not be used in a configuration " +
                                   "that allows multi-threading: see FeatureKeys.ALLOW_MULTITHREADING", SaxonErrorCode.SXWN9016);
        }
        StructuredQName name = getSourceBinding().getVariableQName();
        SourceBinding globalVariable;
        try {
            globalVariable = bindVariable(name);
            if (globalVariable == null) {
                throw new XPathException("Variable " + name.getDisplayName() +
                        " has not been declared")
                        .withErrorCode("XPST0008")
                        .asStaticError();
            }
            globalVariable.registerReference(instruction);
        } catch (XPathException err) {
            // variable not declared
            compileError(err.getMessage());
            return;
        }
        if (!globalVariable.hasProperty(SourceBinding.BindingProperty.GLOBAL)) {
            compileError("Variable " + name.getDisplayName() + " is not global");
            return;
        }
        if (!globalVariable.hasProperty(SourceBinding.BindingProperty.ASSIGNABLE)) {
            compileError("Variable " + name.getDisplayName() + " is not marked as assignable");
        }
        // Fix for bug 3871
        XSLGlobalVariable sourceVar = (XSLGlobalVariable) globalVariable.getSourceElement();
        GlobalVariable compiledVar = sourceVar.getCompiledVariable();
        if (compiledVar != null) {
            instruction.fixup(sourceVar.getCompiledVariable());
        }
    }

    @Override
    public Expression compile(Compilation exec, ComponentDeclaration decl) throws XPathException {
        //initializeBinding(exec, decl, instruction);
        if (sourceBinding.getSelectExpression() != null) {
            instruction.setSelectExpr(sourceBinding.getSelectExpression());
        } else {
            instruction.setSelectExpr(compileSequenceConstructor(exec, decl, false));
        }
        return instruction;
    }

}

// Copyright (c) 2004-2023 Saxonica Limited

