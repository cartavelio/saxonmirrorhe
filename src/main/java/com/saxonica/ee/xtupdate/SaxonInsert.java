package com.saxonica.ee.xtupdate;

import com.saxonica.ee.update.InsertExpression;
import net.sf.saxon.Configuration;
import net.sf.saxon.expr.ContextItemExpression;
import net.sf.saxon.expr.Expression;
import net.sf.saxon.expr.parser.Token;
import net.sf.saxon.lib.Validation;
import net.sf.saxon.om.AttributeCollection;
import net.sf.saxon.style.Compilation;
import net.sf.saxon.style.ComponentDeclaration;
import net.sf.saxon.style.StyleElement;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.value.Whitespace;

/**
 * saxon:insert is an XSLT version of the XQuery Update "insert node" expression.
 */

public class SaxonInsert extends StyleElement {

    private Expression select;
    private String positionAtt;

    /**
     * Determine whether this node is an instruction.
     *
     * @return true - it is an instruction
     */

    public boolean isInstruction() {
        return true;
    }

    /**
     * Determine whether this type of element is allowed to contain a template-body
     *
     * @return true: yes, it may contain a template-body
     */

    public boolean mayContainSequenceConstructor() {
        return true;
    }

    public void prepareAttributes() throws XPathException {
        String selectAtt = null;
        String contentAtt = null;

        AttributeCollection atts = getAttributeList();

        for (int a = 0; a < atts.getLength(); a++) {
            String f = atts.getQName(a);
            switch (f) {
                case "select":
                    selectAtt = atts.getValue(a);
                    select = makeExpression(selectAtt, a);
                    break;
                case "position":
                    String val = Whitespace.trim(atts.getValue(a));
                    if (!(val.equals("after") || val.equals("before") || val.equals("first") || val.equals("last"))) {
                        invalidAttribute("position", "after|before|first|last");
                    }
                    positionAtt = val;
                    break;
                default:
                    checkUnknownAttribute(atts.getNodeName(a));
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

    public void validate(ComponentDeclaration decl) throws XPathException {
        getConfiguration().checkLicensedFeature(Configuration.LicenseFeature.ENTERPRISE_XSLT,
                                                "saxon:insert",
                                                getPackageData().getLocalLicenseId());

        select = typeCheck("select", select);
        SaxonUpdate.checkContainment(this);
    }

    public Expression compile(Compilation exec, ComponentDeclaration decl) throws XPathException {
        Expression content = compileSequenceConstructor(exec, decl, false);
        int pos = interpretPosition(positionAtt);
        return new InsertExpression(content, select, pos, Validation.PRESERVE, true, true);
    }

    private int interpretPosition(String p) {
        switch (p) {
            case "first": return Token.FIRST_INTO;
            case "last": return Token.LAST_INTO;
            case "before": return Token.BEFORE;
            case "after": return Token.AFTER;
            default: return -1;
        }
    }

}

// Copyright (c) 2018 Saxonica Ltd

