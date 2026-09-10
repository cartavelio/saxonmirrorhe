package com.saxonica.ee.trans;

import com.saxonica.config.EnterpriseConfiguration;
import com.saxonica.ee.optim.GeneralEqualityEE;
import com.saxonica.ee.optim.IndexedFilterExpression;
import com.saxonica.ee.optim.SwitchExpression;
import com.saxonica.ee.stream.StreamInstr;
import com.saxonica.ee.stream.StreamableUserFunction;
import com.saxonica.ee.update.*;
import com.saxonica.ee.xtupdate.SaxonInsert;
import com.saxonica.trans.PackageLoaderPE;
import net.sf.saxon.expr.*;
import net.sf.saxon.expr.instruct.Block;
import net.sf.saxon.expr.instruct.UserFunction;
import net.sf.saxon.expr.parser.ExpressionTool;
import net.sf.saxon.expr.parser.OperatorSymbol;
import net.sf.saxon.expr.sort.AtomicComparer;
import net.sf.saxon.lib.ParseOptions;
import net.sf.saxon.lib.Validation;
import net.sf.saxon.om.NamespaceUri;
import net.sf.saxon.om.NodeInfo;
import net.sf.saxon.om.StructuredQName;
import net.sf.saxon.pattern.NodeSelector;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.type.SchemaType;
import net.sf.saxon.value.AtomicValue;

/**
 * Saxon-EE version of PackageLoader
 */
public class PackageLoaderEE extends PackageLoaderPE {

    public PackageLoaderEE(EnterpriseConfiguration config) {
        super(config);
    }



    static {
        eMap.put("copyModify", (loader, element) -> {
            int validation = Validation.getCode(element.getAttributeValue(NamespaceUri.NULL, "val"));
            Expression bindings = loader.getExpressionWithRole(element, "bindings");
            Expression update = loader.getExpressionWithRole(element, "modify");
            Expression returnExp = loader.getExpressionWithRole(element, "return");
            CopyModifyExpression c = new CopyModifyExpression(update, returnExp, validation);
            c.setCopyBindings(bindings);
            return c;
        });

        eMap.put("delete", (loader, element) -> {
            Expression select = loader.getFirstChildExpression(element);
            return new DeleteExpression(select);
        });

        eMap.put("gcEE", (loader, element) -> {
            String opAtt = element.getAttributeValue(NamespaceUri.NULL, "op");
            OperatorSymbol op = getOperator(opAtt);
            Expression lhs = loader.getFirstChildExpression(element);
            Expression rhs = loader.getSecondChildExpression(element);
            String compAtt = element.getAttributeValue(NamespaceUri.NULL, "comp");
            AtomicComparer comp = loader.makeAtomicComparer(compAtt, element);
            GeneralComparison gc = new GeneralEqualityEE(lhs, op, rhs);
            gc.setAtomicComparer(comp);
            return gc;
        });

        eMap.put("indexedFilter", (loader, element) -> {
            // Dropped in 9.8, but found in 9.7 export files
            Expression lhs = loader.getFirstChildExpression(element);
            Expression rhs = loader.getSecondChildExpression(element);
            String useAtt = element.getAttributeValue(NamespaceUri.NULL, "use");
            return new IndexedFilterExpression((VariableReference) lhs, (ComparisonExpression) rhs, "left".equals(useAtt));
        });

        eMap.put("indexedFilter2", (loader, element) -> {
            // New in 9.8
            Expression base = loader.getExpressionWithRole(element, "base");
            Expression use = loader.getExpressionWithRole(element, "use");
            Expression search = loader.getExpressionWithRole(element, "search");
            String compAtt = element.getAttributeValue(NamespaceUri.NULL, "comp");
            AtomicComparer comp = loader.makeAtomicComparer(compAtt, element);
            String flags = element.getAttributeValue(NamespaceUri.NULL, "flags");
            boolean convertUntyped = flags != null && flags.contains("u");
            return new IndexedFilterExpression(base, use, search, comp, convertUntyped);
        });

        eMap.put("indexedLookup", (loader, element) -> {
            throw new XPathException("indexedLookup expression is no longer supported: please recompile stylesheet");
        });

        eMap.put("insert", (loader, element) -> {
            int constructionMode = loader.getIntegerAttribute(element, "constructionMode");
            InsertPosition position =
                    SaxonInsert.interpretPosition(element.getAttributeValue(NamespaceUri.NULL, "position"));
            String flags = element.getAttributeValue(NamespaceUri.NULL, "flags");
            Expression source = loader.getFirstChildExpression(element);
            Expression target = loader.getSecondChildExpression(element);
            return new InsertExpression(source, target, position, constructionMode, flags.contains("i"), flags.contains("p"));
        });

        eMap.put("rename", (loader, element) -> {
            Expression target = loader.getFirstChildExpression(element);
            Expression namer = loader.getSecondChildExpression(element);
            String flags = element.getAttributeValue(NamespaceUri.NULL, "flags");
            return new RenameExpression(target, namer, null, null, flags != null && flags.contains("i"));
        });

        eMap.put("replaceNodes", (loader, element) -> {
            int constructionMode = Validation.getCode(element.getAttributeValue(NamespaceUri.NULL, "constructionMode"));
            Expression target = loader.getFirstChildExpression(element);
            Expression replacement = loader.getSecondChildExpression(element);
            String flags = element.getAttributeValue(NamespaceUri.NULL, "flags");
            return new ReplaceNodeExpression(target, replacement, constructionMode, flags != null && flags.contains("i"));
        });

        eMap.put("replaceValue", (loader, element) -> {
            Expression target = loader.getFirstChildExpression(element);
            Expression replacement = loader.getSecondChildExpression(element);
            return new ReplaceValueExpression(target, replacement);
        });

        eMap.put("stream", (loader, element) -> {
            loader.needsEELicense("streaming");
            int valSpecified = loader.getIntegerAttribute(element, "validation");
            int validation = valSpecified == Integer.MIN_VALUE ? Validation.SKIP : valSpecified;
            SchemaType schemaType = null;
            StructuredQName typeAtt = loader.getQNameAttribute(element, "schemaType");
            if (typeAtt != null) {
                schemaType = loader.getConfiguration().getSchemaType(typeAtt);
                validation = Validation.BY_TYPE;
            }
            final ParseOptions options = loader.getConfiguration().getParseOptions()
                .withSchemaValidationMode(validation)
                .withTopLevelType(schemaType);

            Expression body = loader.getExpressionWithRole(element, "body");
            Expression href = loader.getExpressionWithRole(element, "href");

            final StreamInstr inst = new StreamInstr(href, body, options);

            String flags = element.getAttributeValue(NamespaceUri.NULL, "flags");
            if (flags != null && flags.contains("s")) {
                loader.addCompletionAction(() -> inst.setSpaceStrippingRule(loader.getTopLevelPackage().getSpaceStrippingRule()));
            }

            String accumulatorNames = element.getAttributeValue(NamespaceUri.NULL, "accum");
            processAccumulatorList(loader, inst, accumulatorNames);

            // The xsl:stream instruction might be converted to an xsl:source-document instruction when we
            // come to analyse its streamability. To enable the replacement, we insert an artificial
            // single-instruction Block in the tree, whose child expression can be changed

            final Block block = new Block(new Expression[]{inst});

            loader.addCompletionAction(() -> {
                Expression e2 = inst.prepareForStreaming(loader.getPackStack().peek());
                Expression parent = block.getParentExpression();
                if (parent != null) {
                    // Eliminate the redundant Block if we can
                    // (this doesn't work if it's the outermost instruction in a function or template)
                    Operand op = ExpressionTool.findOperand(parent, block);
                    if (op != null) {
                        op.setChildExpression(e2);
                    }
                } else if (e2 != inst) {
                    block.getOperanda()[0].setChildExpression(e2);
                }
            });
            return block;
        });

        eMap.put("switch", (loader, element) -> {
            SwitchExpression.SwitchCaseInfo info = new SwitchExpression.SwitchCaseInfo();
            info.collation = loader.getConfiguration().getCollation(element.getAttributeValue(NamespaceUri.NULL, "coll"));
            info.commonlhs = loader.getExpressionWithRole(element, "value");
            info.defaultAction = loader.getExpressionWithRole(element, "default");
            for (NodeInfo aCase : element.children(NodeSelector.of(n -> n.getLocalPart().equals("case")))) {
                Expression val = loader.getFirstChildExpression(aCase);
                AtomicValue av = (AtomicValue) ((Literal) val).getGroundedValue();
                info.values.add(av);
                info.actions.add(loader.getSecondChildExpression(aCase));
            }
            info.makeIndex();
            return new SwitchExpression(info);
        });


    }

    @Override
    protected UserFunction makeFunction(String flags) {
        if (flags.matches(".*[AIFSDC].*")) {
            return new StreamableUserFunction();
        } else {
            return super.makeFunction(flags);
        }
    }
}

// Copyright (c) 2018-2023 Saxonica Limited