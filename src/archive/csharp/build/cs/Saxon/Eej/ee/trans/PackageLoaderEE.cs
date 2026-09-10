using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.ee.trans {
    public class PackageLoaderEE : Saxon.Eej.trans.PackageLoaderPE {

        static PackageLoaderEE() {
            Saxon.Impl.Helpers.MapUtils.Put(eMap, "copyModify", (loader, element) => {
    int validation = Saxon.Hej.lib.Validation.getCode(element.getAttributeValue("", "val"));
    Saxon.Hej.expr.Expression bindings = loader.getExpressionWithRole(element, "bindings");
    Saxon.Hej.expr.Expression update = loader.getExpressionWithRole(element, "modify");
    Saxon.Hej.expr.Expression returnExp = loader.getExpressionWithRole(element, "return");
    Saxon.Eej.ee.update.CopyModifyExpression c = new Saxon.Eej.ee.update.CopyModifyExpression(update, returnExp, validation);
    c.setCopyBindings(bindings);
    return c;
} );
            Saxon.Impl.Helpers.MapUtils.Put(eMap, "delete", (loader, element) => {
    Saxon.Hej.expr.Expression select = loader.getFirstChildExpression(element);
    return new Saxon.Eej.ee.update.DeleteExpression(select);
} );
            Saxon.Impl.Helpers.MapUtils.Put(eMap, "gcEE", (loader, element) => {
    string opAtt = element.getAttributeValue("", "op");
    int op = getOperator(opAtt);
    Saxon.Hej.expr.Expression lhs = loader.getFirstChildExpression(element);
    Saxon.Hej.expr.Expression rhs = loader.getSecondChildExpression(element);
    string compAtt = element.getAttributeValue("", "comp");
    Saxon.Hej.expr.sort.AtomicComparer comp = loader.makeAtomicComparer(compAtt, element);
    Saxon.Hej.expr.GeneralComparison gc = new Saxon.Eej.ee.optim.GeneralComparisonEE(lhs, op, rhs);
    gc.setAtomicComparer(comp);
    return gc;
} );
            Saxon.Impl.Helpers.MapUtils.Put(eMap, "indexedFilter", (loader, element) => {
    Saxon.Hej.expr.Expression lhs = loader.getFirstChildExpression(element);
    Saxon.Hej.expr.Expression rhs = loader.getSecondChildExpression(element);
    string useAtt = element.getAttributeValue("", "use");
    return new Saxon.Eej.ee.optim.IndexedFilterExpression((Saxon.Hej.expr.VariableReference)(lhs), (Saxon.Hej.expr.ComparisonExpression)(rhs), "left".Equals(useAtt));
} );
            Saxon.Impl.Helpers.MapUtils.Put(eMap, "indexedFilter2", (loader, element) => {
    Saxon.Hej.expr.Expression @base = loader.getExpressionWithRole(element, "base");
    Saxon.Hej.expr.Expression use = loader.getExpressionWithRole(element, "use");
    Saxon.Hej.expr.Expression search = loader.getExpressionWithRole(element, "search");
    string compAtt = element.getAttributeValue("", "comp");
    Saxon.Hej.expr.sort.AtomicComparer comp = loader.makeAtomicComparer(compAtt, element);
    string flags = element.getAttributeValue("", "flags");
    bool convertUntyped = ((flags != null) && flags.Contains("u"));
    return new Saxon.Eej.ee.optim.IndexedFilterExpression(@base, use, search, comp, convertUntyped);
} );
            Saxon.Impl.Helpers.MapUtils.Put(eMap, "indexedLookup", (loader, element) => {
    throw new Saxon.Hej.trans.XPathException("indexedLookup expression is no longer supported: please recompile stylesheet");
} );
            Saxon.Impl.Helpers.MapUtils.Put(eMap, "insert", (loader, element) => {
    int constructionMode = loader.getIntegerAttribute(element, "constructionMode");
    int position = /*134*/Saxon.Eej.ee.xtupdate.SaxonInsert.interpretPosition(element.getAttributeValue("", "position"));
    string flags = element.getAttributeValue("", "flags");
    Saxon.Hej.expr.Expression source = loader.getFirstChildExpression(element);
    Saxon.Hej.expr.Expression target = loader.getSecondChildExpression(element);
    return new Saxon.Eej.ee.update.InsertExpression(source, target, position, constructionMode, flags.Contains("i"), flags.Contains("p"));
} );
            Saxon.Impl.Helpers.MapUtils.Put(eMap, "rename", (loader, element) => {
    Saxon.Hej.expr.Expression target = loader.getFirstChildExpression(element);
    Saxon.Hej.expr.Expression namer = loader.getSecondChildExpression(element);
    string flags = element.getAttributeValue("", "flags");
    return new Saxon.Eej.ee.update.RenameExpression(target, namer, null, null, ((flags != null) && flags.Contains("i")));
} );
            Saxon.Impl.Helpers.MapUtils.Put(eMap, "replaceNodes", (loader, element) => {
    int constructionMode = Saxon.Hej.lib.Validation.getCode(element.getAttributeValue("", "constructionMode"));
    Saxon.Hej.expr.Expression target = loader.getFirstChildExpression(element);
    Saxon.Hej.expr.Expression replacement = loader.getSecondChildExpression(element);
    string flags = element.getAttributeValue("", "flags");
    return new Saxon.Eej.ee.update.ReplaceNodeExpression(target, replacement, constructionMode, ((flags != null) && flags.Contains("i")));
} );
            Saxon.Impl.Helpers.MapUtils.Put(eMap, "replaceValue", (loader, element) => {
    Saxon.Hej.expr.Expression target = loader.getFirstChildExpression(element);
    Saxon.Hej.expr.Expression replacement = loader.getSecondChildExpression(element);
    return new Saxon.Eej.ee.update.ReplaceValueExpression(target, replacement);
} );
            Saxon.Impl.Helpers.MapUtils.Put(eMap, "stream", (loader, element) => {
    loader.needsEELicense("streaming");
    int valSpecified = loader.getIntegerAttribute(element, "validation");
    int validation = ((valSpecified == System.Int32.MinValue) ? Saxon.Hej.lib.Validation.SKIP : valSpecified);
    Saxon.Hej.type.SchemaType schemaType = null;
    Saxon.Hej.om.StructuredQName typeAtt = loader.getQNameAttribute(element, "schemaType");
    if ((typeAtt != null)) {
        schemaType = loader.getConfiguration().getSchemaType(typeAtt);
        validation = Saxon.Hej.lib.Validation.BY_TYPE;
    }
    Saxon.Hej.lib.ParseOptions options = new Saxon.Hej.lib.ParseOptions(loader.getConfiguration().getParseOptions());
    options.setSchemaValidationMode(validation);
    options.setTopLevelType(schemaType);
    string flags = element.getAttributeValue("", "flags");
    if (((flags != null) && flags.Contains("s"))) {
        loader.addCompletionAction(() => options.setSpaceStrippingRule(loader.getTopLevelPackage().getSpaceStrippingRule()));
    }
    Saxon.Hej.expr.Expression body = loader.getExpressionWithRole(element, "body");
    Saxon.Hej.expr.Expression href = loader.getExpressionWithRole(element, "href");
    Saxon.Eej.ee.stream.StreamInstr inst = new Saxon.Eej.ee.stream.StreamInstr(href, body, options);
    string accumulatorNames = element.getAttributeValue("", "accum");
    processAccumulatorList(loader, inst, accumulatorNames);
    Saxon.Hej.expr.instruct.Block block = new Saxon.Hej.expr.instruct.Block(new Saxon.Hej.expr.Expression[]{inst});
    loader.addCompletionAction(() => {
        Saxon.Hej.expr.Expression e2 = inst.prepareForStreaming(loader.getPackStack().Peek());
        Saxon.Hej.expr.Expression parent = block.getParentExpression();
        if ((parent != null)) {
            Saxon.Hej.expr.Operand op = Saxon.Hej.expr.parser.ExpressionTool.findOperand(parent, block);
            if ((op != null)) {
                op.setChildExpression(e2);
            }
        } else {
            if ((e2 != inst)) {
                block.getOperanda()[0].setChildExpression(e2);
            }
        }
    } );
    return block;
} );
            Saxon.Impl.Helpers.MapUtils.Put(eMap, "switch", (loader, element) => {
    Saxon.Eej.ee.optim.SwitchExpression.SwitchCaseInfo info = new Saxon.Eej.ee.optim.SwitchExpression.SwitchCaseInfo();
    info.collation = loader.getConfiguration().getCollation(element.getAttributeValue("", "coll"));
    info.commonlhs = loader.getExpressionWithRole(element, "value");
    info.defaultAction = loader.getExpressionWithRole(element, "default");
    foreach (Saxon.Hej.om.NodeInfo aCase in element.children(Saxon.Hej.pattern.NodeSelector.of(n => n.getLocalPart().Equals("case")))) {
        Saxon.Hej.expr.Expression val = loader.getFirstChildExpression(aCase);
        Saxon.Hej.value.AtomicValue av = (Saxon.Hej.value.AtomicValue)(((Saxon.Hej.expr.Literal)(val)).getGroundedValue());
        info.values.Add(av);
        info.actions.Add(loader.getSecondChildExpression(aCase));
    }
    info.makeIndex();
    return new Saxon.Eej.ee.optim.SwitchExpression(info);
} );
        }
        public PackageLoaderEE(Saxon.Eej.config.EnterpriseConfiguration config) : base (config) {
        }
        protected internal override Saxon.Hej.expr.instruct.UserFunction makeFunction(string flags) {
            if (System.Text.RegularExpressions.Regex.IsMatch(flags, ".*[AIFSDC].*")) {
                return new Saxon.Eej.ee.stream.StreamableUserFunction();
            } else {
                return base.makeFunction(flags);
            }
        }
    
    }
}