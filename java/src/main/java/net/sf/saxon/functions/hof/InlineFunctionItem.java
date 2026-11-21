////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) 2020 Saxonica Limited
// This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
// If a copy of the MPL was not distributed with this file, You can obtain one at http://mozilla.org/MPL/2.0/.
// This Source Code Form is "Incompatible With Secondary Licenses", as defined by the Mozilla Public License, v. 2.0.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

package net.sf.saxon.functions.hof;

import net.sf.saxon.Configuration;
import net.sf.saxon.expr.Expression;
import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.expr.XPathContextMajor;
import net.sf.saxon.expr.elab.PullEvaluator;
import net.sf.saxon.expr.instruct.SlotManager;
import net.sf.saxon.expr.instruct.UserFunctionParameter;
import net.sf.saxon.expr.parser.*;
import net.sf.saxon.functions.AbstractFunction;
import net.sf.saxon.functions.DeepEqual;
import net.sf.saxon.om.FunctionItem;
import net.sf.saxon.om.LazySequence;
import net.sf.saxon.om.Sequence;
import net.sf.saxon.om.StructuredQName;
import net.sf.saxon.query.AnnotationList;
import net.sf.saxon.query.QueryModule;
import net.sf.saxon.s9api.Location;
import net.sf.saxon.trans.UncheckedXPathException;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.type.FunctionItemType;
import net.sf.saxon.type.SpecificFunctionType;
import net.sf.saxon.value.SequenceType;

import java.util.Stack;
import java.util.function.Supplier;

/**
 * An InlineFunctionItem is a function item derived directly or indirectly by parsing
 * an inline function expression. If the inline function as written includes references
 * to variables declared outside the function body, these will be represented as additional
 * parameters to the function, which must be supplied by the caller (this is achieved by
 * wrapping the InlineFunctionItem in a CurriedFunction which supplies the extra variables).
 *
 * <p>An InlineFunctionItem contains an Expression in its body, but this Expression is
 * disconnected from the main expression tree. To ensure that the code of an InlineFunctionItem
 * is processed through all the various phases of compilation, it is necessary to register
 * all InlineFunctionItems with the containing executable code (e.g. a Query module or a
 * stylesheet package.)</p>
 *
 * <p>Construction of a stack frame map and allocation of slot numbers is done on first
 * execution of the inline function item.</p>
 */

public class InlineFunctionItem extends AbstractFunction {

    private final AnnotationList annotationList;
    private final UserFunctionParameter[] parameters;
    private SequenceType declaredResultType;
    private Expression body;
    private volatile PullEvaluator bodyEvaluator;
    private SlotManager stackFrameMap;

    public InlineFunctionItem(AnnotationList annotationList,
                              UserFunctionParameter[] parameters,
                              SequenceType declaredResultType,
                              Expression body) {
        this.annotationList = annotationList == null ? AnnotationList.EMPTY : annotationList;
        this.parameters = parameters;
        this.declaredResultType = declaredResultType;
        this.body = body;
    }

    public void makeStackFrameMap(Configuration config) {
        stackFrameMap = config.makeSlotManager();
        for (int i = 0; i < parameters.length; i++) {
            parameters[i].setSlotNumber(i);
            stackFrameMap.allocateSlotNumber(parameters[i].getVariableQName(), parameters[i]);
        }
        ExpressionTool.allocateSlots(body, parameters.length, stackFrameMap);
    }

    @Override
    public FunctionItemType getFunctionItemType() {
        SequenceType[] argTypes = new SequenceType[parameters.length];
        for (int i = 0; i < parameters.length; i++) {
            argTypes[i] = parameters[i].getRequiredType();
        }
        return new SpecificFunctionType(argTypes, declaredResultType, annotationList);
    }

    public UserFunctionParameter[] getParameters() {
        return parameters;
    }

    @Override
    public StructuredQName getFunctionName() {
        return null;
    }

    @Override
    public AnnotationList getAnnotations() {
        return annotationList;
    }

    public Expression getBody() {
        return body;
    }

    @Override
    public int getArity() {
        return parameters.length;
    }

    @Override
    public void simplify() throws XPathException {
        body = body.simplify();
    }

    @Override
    public void typeCheck(ExpressionVisitor visitor, ContextItemStaticInfo contextItemType) throws XPathException {

        if (visitor.getStaticContext() instanceof QueryModule qm) {
            for (UserFunctionParameter param : parameters) {
                SequenceType type = param.getRequiredType();
                SequenceType resolved = type.resolveAliases(new Stack<>());
                param.setRequiredType(resolved);
            }
            declaredResultType = declaredResultType.resolveAliases(new Stack<>());
        }

        body = body.typeCheck(visitor, ContextItemStaticInfo.ABSENT);
        final TypeChecker tc = visitor.getConfiguration().getTypeChecker(false);
        Supplier<RoleDiagnostic> role = () -> new RoleDiagnostic(RoleDiagnostic.FUNCTION_RESULT, getDescription(), 0);
        body = tc.staticTypeCheck(body, declaredResultType, role, visitor);


    }

    public void optimize(ExpressionVisitor visitor, ContextItemStaticInfo contextItemType) throws XPathException {
        body = body.optimize(visitor, ContextItemStaticInfo.ABSENT);
    }

    @Override
    public String getDescription() {
        StringBuilder sb = new StringBuilder();
        sb.append("fn(");
        boolean first = true;
        for (UserFunctionParameter parameter : parameters) {
            if (first) {
                first = false;
            } else {
                sb.append(", ");
            }
            sb.append('$');
            sb.append(parameter.getVariableQName().getLocalPart());
        }
        sb.append(") {");
        String bodyStr = body.toShortString();
        if (bodyStr.length() > 50) {
            bodyStr = bodyStr.substring(0, 50) + "...";
        }
        sb.append(bodyStr);
        sb.append("}");
        return sb.toString();
    }

    @Override
    public String toString() {
        return getDescription();
    }

    @Override
    public Sequence call(XPathContext context, Sequence[] arguments) throws XPathException {
        if (bodyEvaluator == null) {
            synchronized(this) {
                if (bodyEvaluator == null) {
                    // first time through
                    makeStackFrameMap(context.getConfiguration());
                    bodyEvaluator = body.makeElaborator().elaborateForPull();
                }
            }
        }

        XPathContextMajor c2 = context.newCleanContext();
        c2.setCurrentComponent(context.getCurrentComponent());
        c2.setStackFrame(stackFrameMap, arguments);
        Sequence result;
        try {
            result = new LazySequence(bodyEvaluator.iterate(c2));
        } catch (XPathException err) {
            throw err.maybeWithLocation(body.getLocation()).maybeWithContext(c2);
        } catch (UncheckedXPathException uxe) {
            throw uxe.getXPathException().maybeWithLocation(body.getLocation()).maybeWithContext(c2);
        } catch (Exception err2) {
            Location loc = body.getLocation();
            String message = "Internal error evaluating function "
                    + getDescription()
                    + (loc.getLineNumber() > 0 ? " at line " + loc.getLineNumber() : "")
                    + (loc.getSystemId() != null ? " in module " + loc.getSystemId() : "");
            throw new RuntimeException(message, err2);
        }
        return result;
    }

    @Override
    public boolean deepEqual40(FunctionItem other, XPathContext context, DeepEqual.DeepEqualOptions options) throws XPathException {
        if (other instanceof InlineFunctionItem ifi) {
            if (this.getArity() != ifi.getArity()) {
                return false;
            }
            for (int i = 0; i < getArity(); i++) {
                SequenceType thisType = parameters[i].getRequiredType();
                SequenceType otherType = ifi.parameters[i].getRequiredType();
                if (!thisType.equals(otherType)) {
                    return false;
                }
            }
            if (!declaredResultType.equals(ifi.declaredResultType)) {
                return false;
            }
            if (!body.equals(ifi.body)) {
                return false;
            }
            return true;
        } else {
            return super.deepEqual40(other, context, options);
        }
    }
}

// Copyright (c) 2018-2026 Saxonica Limited