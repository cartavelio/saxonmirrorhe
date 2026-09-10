package com.saxonica.ee.xtupdate;

import com.saxonica.ee.update.InsertExpression;
import com.saxonica.ee.update.InsertPosition;
import net.sf.saxon.Configuration;
import net.sf.saxon.expr.ContextItemExpression;
import net.sf.saxon.expr.Expression;
import net.sf.saxon.lib.Validation;
import net.sf.saxon.om.AttributeInfo;
import net.sf.saxon.om.NodeName;
import net.sf.saxon.style.Compilation;
import net.sf.saxon.style.ComponentDeclaration;
import net.sf.saxon.style.ExtensionInstruction;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.value.Whitespace;

/**
 * saxon:insert is an XSLT version of the XQuery Update "insert node" expression.
 */

public class SaxonInsert extends ExtensionInstruction {

    private Expression select;
    private String positionAtt;

    /**
     * Determine whether this type of element is allowed to contain a template-body
     *
     * @return true: yes, it may contain a template-body
     */

    @Override
    protected boolean mayContainSequenceConstructor() {
        return true;
    }

    @Override
    protected void prepareAttributes() {
        String selectAtt = null;

        for (AttributeInfo att : attributes()) {
            NodeName attName = att.getNodeName();
            String value = att.getValue();
            String f = attName.getDisplayName();
            switch (f) {
                case "select":
                    selectAtt = value;
                    select = makeExpression(selectAtt, att);
                    break;
                case "position":
                    String val = Whitespace.trim(value);
                    if (!(val.equals("after") || val.equals("before") || val.equals("first") || val.equals("last"))) {
                        invalidAttribute("position", "after|before|first|last");
                    }
                    positionAtt = val;
                    break;
                default:
                    checkUnknownAttribute(attName);
                    break;
            }
        }

        if (selectAtt == null) {
            select = new ContextItemExpression();
            select.setLocation(allocateLocation());
        }

        if (positionAtt == null) {
            positionAtt = "last";
        }

    }

    @Override
    public void validate(ComponentDeclaration decl) throws XPathException {
        getConfiguration().checkLicensedFeature(Configuration.LicenseFeature.ENTERPRISE_XSLT,
                                                "saxon:insert",
                                                getPackageData().getLocalLicenseId());

        select = typeCheck("select", select);
        SaxonUpdate.checkContainment(this);
    }

    @Override
    public Expression compile(Compilation exec, ComponentDeclaration decl) throws XPathException {
        Expression content = compileSequenceConstructor(exec, decl, false);
        InsertPosition pos = interpretPosition(positionAtt);
        return new InsertExpression(content, select, pos, Validation.PRESERVE, true, true);
    }

    public static InsertPosition interpretPosition(String p) throws XPathException {
        switch (p) {
            case "first": return InsertPosition.FIRST;
            case "last": return InsertPosition.LAST;
            case "before": return InsertPosition.BEFORE;
            case "after": return InsertPosition.AFTER;
            default: throw new XPathException("Unknown insert position: " + p);
        }
    }

    public static String encodePosition(InsertPosition p) {
        switch (p) {
            case FIRST:
                return "first";
            case LAST:
                return "last";
            case BEFORE:
                return "before";
            case AFTER:
                return "after";
            case INTO:
                return "into";
            default:
                return "??";
        }
    }

}

// Copyright (c) 2018-2023 Saxonica Limited

