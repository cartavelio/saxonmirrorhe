using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.trans {
    public class PackageLoaderHE : Saxon.Hej.trans.packages.IPackageLoader {

        static PackageLoaderHE() {
            Saxon.Impl.Helpers.MapUtils.Put(licensableConstructs, "gcEE", "EE");
            Saxon.Impl.Helpers.MapUtils.Put(licensableConstructs, "indexedFilter", "EE");
            Saxon.Impl.Helpers.MapUtils.Put(licensableConstructs, "indexedFilter2", "EE");
            Saxon.Impl.Helpers.MapUtils.Put(licensableConstructs, "indexedLookup", "EE");
            Saxon.Impl.Helpers.MapUtils.Put(licensableConstructs, "stream", "EE");
            Saxon.Impl.Helpers.MapUtils.Put(licensableConstructs, "switch", "EE");
            Saxon.Impl.Helpers.MapUtils.Put(licensableConstructs, "acFnRef", "PE");
            Saxon.Impl.Helpers.MapUtils.Put(licensableConstructs, "assign", "PE");
            Saxon.Impl.Helpers.MapUtils.Put(licensableConstructs, "do", "PE");
            Saxon.Impl.Helpers.MapUtils.Put(licensableConstructs, "javaCall", "PE");
            Saxon.Impl.Helpers.MapUtils.Put(licensableConstructs, "while", "PE");
            Saxon.Impl.Helpers.MapUtils.Put(eMap, "among", (loader, element) => {
    Saxon.Hej.expr.Expression lhs = loader.getFirstChildExpression(element);
    Saxon.Hej.expr.Expression rhs = loader.getSecondChildExpression(element);
    return new Saxon.Hej.expr.SingletonIntersectExpression(lhs, Saxon.Hej.expr.parser.Token.INTERSECT, rhs);
} );
            Saxon.Impl.Helpers.MapUtils.Put(eMap, "analyzeString", (loader, element) => {
    Saxon.Hej.expr.Expression select = loader.getExpressionWithRole(element, "select");
    Saxon.Hej.expr.Expression regex = loader.getExpressionWithRole(element, "regex");
    Saxon.Hej.expr.Expression flags = loader.getExpressionWithRole(element, "flags");
    Saxon.Hej.expr.Expression matching = loader.getExpressionWithRole(element, "matching");
    Saxon.Hej.expr.Expression nonMatching = loader.getExpressionWithRole(element, "nonMatching");
    Saxon.Hej.expr.instruct.AnalyzeString instr = new Saxon.Hej.expr.instruct.AnalyzeString(select, regex, flags, matching, nonMatching, null);
    instr.precomputeRegex(loader.getConfiguration(), null);
    return instr;
} );
            Saxon.Impl.Helpers.MapUtils.Put(eMap, "and", (loader, element) => {
    Saxon.Hej.expr.Expression lhs = loader.getFirstChildExpression(element);
    Saxon.Hej.expr.Expression rhs = loader.getSecondChildExpression(element);
    return new Saxon.Hej.expr.AndExpression(lhs, rhs);
} );
            Saxon.Impl.Helpers.MapUtils.Put(eMap, "applyImports", (loader, element) => {
    Saxon.Hej.expr.instruct.ApplyImports inst = new Saxon.Hej.expr.instruct.ApplyImports();
    Saxon.Hej.expr.instruct.WithParam[] actuals = loader.loadWithParams(element, inst, false);
    Saxon.Hej.expr.instruct.WithParam[] tunnels = loader.loadWithParams(element, inst, true);
    inst.setActualParams(actuals);
    inst.setTunnelParams(tunnels);
    return inst;
} );
            Saxon.Impl.Helpers.MapUtils.Put(eMap, "applyT", (loader, element) => {
    Saxon.Hej.style.StylesheetPackage pack = loader.packStack.Peek();
    Saxon.Hej.expr.Expression select = loader.getFirstChildExpression(element);
    Saxon.Hej.om.StructuredQName modeAtt = loader.getQNameAttribute(element, "mode");
    Saxon.Hej.trans.SimpleMode mode;
    if ((modeAtt != null)) {
        mode = (Saxon.Hej.trans.SimpleMode)(pack.getRuleManager().obtainMode(modeAtt, true));
    } else {
        mode = (Saxon.Hej.trans.SimpleMode)(pack.getRuleManager().obtainMode(null, true));
    }
    string flags = element.getAttributeValue("", "flags");
    if ((flags == null)) {
        flags = "";
    }
    bool useCurrentMode = flags.Contains("c");
    bool useTailRecursion = flags.Contains("t");
    bool implicitSelect = flags.Contains("i");
    bool inStreamableConstruct = flags.Contains("d");
    Saxon.Hej.expr.instruct.ApplyTemplates inst = new Saxon.Hej.expr.instruct.ApplyTemplates(select, useCurrentMode, useTailRecursion, implicitSelect, inStreamableConstruct, mode, loader.packStack.Peek().getRuleManager());
    Saxon.Hej.expr.Expression sep = loader.getExpressionWithRole(element, "separator");
    if ((sep != null)) {
        inst.setSeparatorExpression(sep);
    }
    Saxon.Hej.expr.instruct.WithParam[] actuals = loader.loadWithParams(element, inst, false);
    Saxon.Hej.expr.instruct.WithParam[] tunnels = loader.loadWithParams(element, inst, true);
    inst.setActualParams(actuals);
    inst.setTunnelParams(tunnels);
    int bindingSlot = loader.getIntegerAttribute(element, "bSlot");
    inst.setBindingSlot(bindingSlot);
    return inst;
} );
            Saxon.Impl.Helpers.MapUtils.Put(eMap, "arith", (loader, element) => {
    Saxon.Hej.expr.Expression lhs = loader.getFirstChildExpression(element);
    Saxon.Hej.expr.Expression rhs = loader.getSecondChildExpression(element);
    string code = element.getAttributeValue("", "calc");
    Saxon.Hej.expr.Calculator calc = Saxon.Hej.expr.Calculator.reconstructCalculator(code);
    int @operator = Saxon.Hej.expr.Calculator.operatorFromCode(code[1]);
    int token = Saxon.Hej.expr.Calculator.getTokenFromOperator(@operator);
    Saxon.Hej.expr.ArithmeticExpression exp = new Saxon.Hej.expr.ArithmeticExpression(lhs, token, rhs);
    exp.setCalculator(calc);
    return exp;
} );
            Saxon.Impl.Helpers.MapUtils.Put(eMap, "arith10", (loader, element) => {
    Saxon.Hej.expr.Expression lhs = loader.getFirstChildExpression(element);
    Saxon.Hej.expr.Expression rhs = loader.getSecondChildExpression(element);
    string code = element.getAttributeValue("", "calc");
    Saxon.Hej.expr.Calculator calc = Saxon.Hej.expr.Calculator.reconstructCalculator(code);
    int @operator = Saxon.Hej.expr.Calculator.operatorFromCode(code[1]);
    int token = Saxon.Hej.expr.Calculator.getTokenFromOperator(@operator);
    Saxon.Hej.expr.compat.ArithmeticExpression10 exp = new Saxon.Hej.expr.compat.ArithmeticExpression10(lhs, token, rhs);
    exp.setCalculator(calc);
    return exp;
} );
            Saxon.Impl.Helpers.MapUtils.Put(eMap, "array", (loader, element) => {
    System.Collections.Generic.IList<Saxon.Hej.expr.Expression> children = getChildExpressionList(loader, element);
    System.Collections.Generic.IList<Saxon.Hej.om.GroundedValue> values = new System.Collections.Generic.List<Saxon.Hej.om.GroundedValue>(children.Count);
    foreach (Saxon.Hej.expr.Expression child in children) {
        values.Add(((Saxon.Hej.expr.Literal)(child)).getGroundedValue());
    }
    return Saxon.Hej.expr.Literal.makeLiteral(new Saxon.Hej.ma.arrays.SimpleArrayItem(values));
} );
            Saxon.Impl.Helpers.MapUtils.Put(eMap, "arrayBlock", (loader, element) => {
    System.Collections.Generic.IList<Saxon.Hej.expr.Expression> children = getChildExpressionList(loader, element);
    return new Saxon.Hej.ma.arrays.SquareArrayConstructor(children);
} );
            Saxon.Impl.Helpers.MapUtils.Put(eMap, "atomic", (loader, element) => {
    string valAtt = element.getAttributeValue("", "val");
    Saxon.Hej.type.AtomicType type = (Saxon.Hej.type.AtomicType)(loader.parseAlphaCodeForItemType(element, "type"));
    Saxon.Hej.value.AtomicValue val = type.getStringConverter(loader.config.getConversionRules()).convertString(Saxon.Hej.str.StringView.of(valAtt).tidy()).asAtomic();
    return Saxon.Hej.expr.Literal.makeLiteral(val);
} );
            Saxon.Impl.Helpers.MapUtils.Put(eMap, "atomSing", (loader, element) => {
    Saxon.Hej.expr.Expression body = loader.getFirstChildExpression(element);
    Saxon.Hej.expr.parser.RoleDiagnostic role = Saxon.Hej.expr.parser.RoleDiagnostic.reconstruct(element.getAttributeValue("", "diag"));
    string cardAtt = element.getAttributeValue("", "card");
    bool allowEmpty = "?".Equals(cardAtt);
    return new Saxon.Hej.expr.SingletonAtomizer(body, role, allowEmpty);
} );
            Saxon.Impl.Helpers.MapUtils.Put(eMap, "att", (loader, element) => {
    string displayName = element.getAttributeValue("", "name");
    string[] parts;
    try {
        parts = Saxon.Hej.om.NameChecker.getQNameParts((displayName));
    } catch (Saxon.Hej.om.QNameException err) {
        throw new Saxon.Hej.trans.XPathException(err);
    }
    string uri = element.getAttributeValue("", "nsuri");
    if ((uri == null)) {
        uri = "";
    }
    Saxon.Hej.om.StructuredQName name = new Saxon.Hej.om.StructuredQName(parts[0], uri, parts[1]);
    Saxon.Hej.om.NodeName attName = new Saxon.Hej.om.FingerprintedQName(name, loader.config.getNamePool());
    int validation = Saxon.Hej.lib.Validation.SKIP;
    string valAtt = element.getAttributeValue("", "validation");
    if ((valAtt != null)) {
        validation = Saxon.Hej.lib.Validation.getCode(valAtt);
    }
    Saxon.Hej.type.SchemaType schemaType = loader.getTypeAttribute(element, "type");
    if ((schemaType != null)) {
        validation = Saxon.Hej.lib.Validation.BY_TYPE;
    }
    Saxon.Hej.expr.Expression content = loader.getFirstChildExpression(element);
    Saxon.Hej.expr.instruct.FixedAttribute att = new Saxon.Hej.expr.instruct.FixedAttribute(attName, validation, (Saxon.Hej.type.SimpleType)(schemaType));
    att.setSelect(content);
    return att;
} );
            Saxon.Impl.Helpers.MapUtils.Put(eMap, "attVal", (loader, element) => {
    Saxon.Hej.om.StructuredQName name = loader.getQNameAttribute(element, "name");
    Saxon.Hej.om.FingerprintedQName attName = new Saxon.Hej.om.FingerprintedQName(name, loader.config.getNamePool());
    Saxon.Hej.expr.AttributeGetter getter = new Saxon.Hej.expr.AttributeGetter(attName);
    getter.setRequiredChecks(loader.getIntegerAttribute(element, "chk"));
    return getter;
} );
            Saxon.Impl.Helpers.MapUtils.Put(eMap, "axis", (loader, element) => {
    string axisName = element.getAttributeValue("", "name");
    int axis = Saxon.Hej.om.AxisInfo.getAxisNumber(axisName);
    Saxon.Hej.pattern.NodeTest nt = (Saxon.Hej.pattern.NodeTest)(loader.parseAlphaCodeForItemType(element, "nodeTest"));
    return new Saxon.Hej.expr.AxisExpression(axis, nt);
} );
            Saxon.Impl.Helpers.MapUtils.Put(eMap, "break", (loader, element) => new Saxon.Hej.expr.instruct.BreakInstr());
            Saxon.Impl.Helpers.MapUtils.Put(eMap, "callT", (loader, element) => {
    Saxon.Hej.style.StylesheetPackage pack = loader.packStack.Peek();
    Saxon.Hej.om.StructuredQName name = loader.getQNameAttribute(element, "name");
    Saxon.Hej.trans.SymbolicName symbol = new Saxon.Hej.trans.SymbolicName(Saxon.Hej.om.StandardNames.XSL_TEMPLATE, name);
    Saxon.Hej.expr.Component target = pack.getComponent(symbol);
    Saxon.Hej.expr.instruct.NamedTemplate t;
    if ((target == null)) {
        t = new Saxon.Hej.expr.instruct.NamedTemplate(name);
    } else {
        t = (Saxon.Hej.expr.instruct.NamedTemplate)(target.getActor());
    }
    string flags = element.getAttributeValue("", "flags");
    bool useTailRecursion = ((flags != null) && flags.Contains("t"));
    bool inStreamableConstruct = ((flags != null) && flags.Contains("d"));
    Saxon.Hej.expr.instruct.CallTemplate inst = new Saxon.Hej.expr.instruct.CallTemplate(t, name, useTailRecursion, inStreamableConstruct);
    Saxon.Hej.expr.instruct.WithParam[] actuals = loader.loadWithParams(element, inst, false);
    Saxon.Hej.expr.instruct.WithParam[] tunnels = loader.loadWithParams(element, inst, true);
    inst.setActualParameters(actuals, tunnels);
    int bindingSlot = loader.getIntegerAttribute(element, "bSlot");
    inst.setBindingSlot(bindingSlot);
    loader.addComponentFixup(inst);
    return inst;
} );
            Saxon.Impl.Helpers.MapUtils.Put(eMap, "cast", (loader, element) => {
    Saxon.Hej.expr.Expression body = loader.getFirstChildExpression(element);
    string flags = element.getAttributeValue("", "flags");
    bool allowEmpty = flags.Contains("e");
    if (flags.Contains("a")) {
        Saxon.Hej.value.SequenceType seqType = loader.parseAlphaCode(element, "as");
        return new Saxon.Hej.expr.CastExpression(body, (Saxon.Hej.type.AtomicType)(seqType.getPrimaryType()), allowEmpty);
    } else {
        if (flags.Contains("l")) {
            Saxon.Hej.om.StructuredQName typeName = Saxon.Hej.om.StructuredQName.fromEQName((element.getAttributeValue("", "as")));
            Saxon.Hej.type.SchemaType type = loader.config.getSchemaType(typeName);
            Saxon.Hej.om.NamespaceResolver resolver = element.getAllNamespaces();
            Saxon.Hej.expr.ListConstructorFunction ucf = new Saxon.Hej.expr.ListConstructorFunction((Saxon.Hej.type.ListType)(type), resolver, allowEmpty);
            return new Saxon.Hej.expr.StaticFunctionCall(ucf, new Saxon.Hej.expr.Expression[]{body});
        } else {
            if (flags.Contains("u")) {
                if ((element.getAttributeValue("", "as") != null)) {
                    Saxon.Hej.om.StructuredQName typeName = Saxon.Hej.om.StructuredQName.fromEQName((element.getAttributeValue("", "as")));
                    Saxon.Hej.type.SchemaType type = loader.config.getSchemaType(typeName);
                    Saxon.Hej.om.NamespaceResolver resolver = element.getAllNamespaces();
                    Saxon.Hej.expr.UnionConstructorFunction ucf = new Saxon.Hej.expr.UnionConstructorFunction((Saxon.Hej.type.UnionType)(type), resolver, allowEmpty);
                    return new Saxon.Hej.expr.StaticFunctionCall(ucf, new Saxon.Hej.expr.Expression[]{body});
                } else {
                    Saxon.Hej.type.LocalUnionType type = (Saxon.Hej.type.LocalUnionType)(loader.parseAlphaCode(element, "to").getPrimaryType());
                    Saxon.Hej.om.NamespaceResolver resolver = element.getAllNamespaces();
                    Saxon.Hej.expr.UnionConstructorFunction ucf = new Saxon.Hej.expr.UnionConstructorFunction(type, resolver, allowEmpty);
                    return new Saxon.Hej.expr.StaticFunctionCall(ucf, new Saxon.Hej.expr.Expression[]{body});
                }
            } else {
                throw new Saxon.Ejava.lang.AssertionError(("Unknown simple type variety "+flags));
            }
        }
    }
} );
            Saxon.Impl.Helpers.MapUtils.Put(eMap, "castable", (loader, element) => {
    Saxon.Hej.expr.Expression body = loader.getFirstChildExpression(element);
    string flags = element.getAttributeValue("", "flags");
    bool allowEmpty = flags.Contains("e");
    if (flags.Contains("a")) {
        Saxon.Hej.value.SequenceType seqType = loader.parseAlphaCode(element, "as");
        return new Saxon.Hej.expr.CastableExpression(body, (Saxon.Hej.type.AtomicType)(seqType.getPrimaryType()), allowEmpty);
    } else {
        if (flags.Contains("l")) {
            Saxon.Hej.om.StructuredQName typeName = Saxon.Hej.om.StructuredQName.fromEQName((element.getAttributeValue("", "as")));
            Saxon.Hej.type.SchemaType type = loader.config.getSchemaType(typeName);
            Saxon.Hej.om.NamespaceResolver resolver = element.getAllNamespaces();
            Saxon.Hej.expr.ListCastableFunction ucf = new Saxon.Hej.expr.ListCastableFunction((Saxon.Hej.type.ListType)(type), resolver, allowEmpty);
            return new Saxon.Hej.expr.StaticFunctionCall(ucf, new Saxon.Hej.expr.Expression[]{body});
        } else {
            if (flags.Contains("u")) {
                if ((element.getAttributeValue("", "as") != null)) {
                    Saxon.Hej.om.StructuredQName typeName = Saxon.Hej.om.StructuredQName.fromEQName((element.getAttributeValue("", "as")));
                    Saxon.Hej.type.SchemaType type = loader.config.getSchemaType(typeName);
                    Saxon.Hej.om.NamespaceResolver resolver = element.getAllNamespaces();
                    Saxon.Hej.expr.UnionCastableFunction ucf = new Saxon.Hej.expr.UnionCastableFunction((Saxon.Hej.type.UnionType)(type), resolver, allowEmpty);
                    return new Saxon.Hej.expr.StaticFunctionCall(ucf, new Saxon.Hej.expr.Expression[]{body});
                } else {
                    Saxon.Hej.type.LocalUnionType type = (Saxon.Hej.type.LocalUnionType)(loader.parseAlphaCode(element, "to").getPrimaryType());
                    Saxon.Hej.om.NamespaceResolver resolver = element.getAllNamespaces();
                    Saxon.Hej.expr.UnionCastableFunction ucf = new Saxon.Hej.expr.UnionCastableFunction(type, resolver, allowEmpty);
                    return new Saxon.Hej.expr.StaticFunctionCall(ucf, new Saxon.Hej.expr.Expression[]{body});
                }
            } else {
                throw new Saxon.Ejava.lang.AssertionError(("Unknown simple type variety "+flags));
            }
        }
    }
} );
            Saxon.Impl.Helpers.MapUtils.Put(eMap, "check", (loader, element) => {
    Saxon.Hej.expr.Expression body = loader.getFirstChildExpression(element);
    string cardAtt = element.getAttributeValue("", "card");
    int c;
    switch (cardAtt) {
      case "?":
        c = Saxon.Hej.expr.StaticProperty.ALLOWS_ZERO_OR_ONE;
        break ;
      case "*":
        c = Saxon.Hej.expr.StaticProperty.ALLOWS_ZERO_OR_MORE;
        break ;
      case "+":
        c = Saxon.Hej.expr.StaticProperty.ALLOWS_ONE_OR_MORE;
        break ;
      case "\u00B0":
      case "0":
        c = Saxon.Hej.expr.StaticProperty.ALLOWS_ZERO;
        break ;
      case "1":
        c = Saxon.Hej.expr.StaticProperty.EXACTLY_ONE;
        break ;
      default:
        throw new Saxon.Ejava.lang.IllegalStateException((("Occurrence indicator: '"+cardAtt)+"'"));
    }
    Saxon.Hej.expr.parser.RoleDiagnostic role = Saxon.Hej.expr.parser.RoleDiagnostic.reconstruct(element.getAttributeValue("", "diag"));
    return Saxon.Hej.expr.CardinalityChecker.makeCardinalityChecker(body, c, role);
} );
            Saxon.Impl.Helpers.MapUtils.Put(eMap, "choose", (loader, element) => {
    System.Collections.Generic.IList<Saxon.Hej.expr.Expression> conditions = new System.Collections.Generic.List<Saxon.Hej.expr.Expression>();
    System.Collections.Generic.IList<Saxon.Hej.expr.Expression> actions = new System.Collections.Generic.List<Saxon.Hej.expr.Expression>();
    Saxon.Hej.tree.iter.AxisIterator iter = element.iterateAxis(Saxon.Hej.om.AxisInfo.CHILD, Saxon.Hej.pattern.NodeKindTest.ELEMENT);
    Saxon.Hej.om.NodeInfo child;
    bool odd = true;
    while (((child = ((/*1430*/Saxon.Hej.om.NodeInfo)iter.next())) != null)) {
        if (odd) {
            conditions.Add(loader.loadExpression(child));
        } else {
            actions.Add(loader.loadExpression(child));
        }
        odd = !(odd);
    }
    return new Saxon.Hej.expr.instruct.Choose(conditions.ToArray(), actions.ToArray());
} );
            Saxon.Impl.Helpers.MapUtils.Put(eMap, "coercedFn", (loader, element) => {
    Saxon.Hej.type.ItemType type = loader.parseItemTypeAttribute(element, "type");
    Saxon.Hej.expr.Expression target = loader.getFirstChildExpression(element);
    Saxon.Hej.om.Function targetFn;
    Saxon.Hej.functions.hof.CoercedFunction coercedFn;
    if ((target is Saxon.Hej.functions.hof.UserFunctionReference)) {
        coercedFn = new Saxon.Hej.functions.hof.CoercedFunction((Saxon.Hej.type.SpecificFunctionType)(type));
        Saxon.Hej.functions.hof.CoercedFunction coercedFn2 = coercedFn;
        Saxon.Hej.trans.SymbolicName name = ((Saxon.Hej.functions.hof.UserFunctionReference)(target)).getSymbolicName();
        loader.addCompletionAction(() => coercedFn2.setTargetFunction(loader.getUserFunction((Saxon.Hej.trans.SymbolicName.F)(name))));
    } else {
        if ((target is Saxon.Hej.expr.Literal)) {
            targetFn = (Saxon.Hej.om.Function)(((Saxon.Hej.expr.Literal)(target)).getGroundedValue());
            coercedFn = new Saxon.Hej.functions.hof.CoercedFunction(targetFn, (Saxon.Hej.type.SpecificFunctionType)(type));
        } else {
            throw new Saxon.Ejava.lang.AssertionError();
        }
    }
    return Saxon.Hej.expr.Literal.makeLiteral(coercedFn);
} );
            Saxon.Impl.Helpers.MapUtils.Put(eMap, "comment", (loader, element) => {
    Saxon.Hej.expr.Expression select = loader.getFirstChildExpression(element);
    Saxon.Hej.expr.instruct.Comment inst = new Saxon.Hej.expr.instruct.Comment();
    inst.setSelect(select);
    return inst;
} );
            Saxon.Impl.Helpers.MapUtils.Put(eMap, "compareToInt", (loader, element) => {
    System.Numerics.BigInteger i = Saxon.Impl.Helpers.BigIntegerUtils.ValueOf(element.getAttributeValue("", "val"));
    string opAtt = element.getAttributeValue("", "op");
    Saxon.Hej.expr.Expression lhs = loader.getFirstChildExpression(element);
    return new Saxon.Hej.expr.CompareToIntegerConstant(lhs, parseValueComparisonOperator(opAtt), ((long)i));
} );
            Saxon.Impl.Helpers.MapUtils.Put(eMap, "compareToString", (loader, element) => {
    string s = element.getAttributeValue("", "val");
    string opAtt = element.getAttributeValue("", "op");
    Saxon.Hej.expr.Expression lhs = loader.getFirstChildExpression(element);
    return new Saxon.Hej.expr.CompareToStringConstant(lhs, parseValueComparisonOperator(opAtt), Saxon.Hej.str.StringView.tidy(s));
} );
            Saxon.Impl.Helpers.MapUtils.Put(eMap, "compAtt", (loader, element) => {
    Saxon.Hej.expr.Expression name = loader.getExpressionWithRole(element, "name");
    Saxon.Hej.expr.Expression @namespace = loader.getExpressionWithRole(element, "namespace");
    Saxon.Hej.expr.Expression content = loader.getExpressionWithRole(element, "select");
    int validation = Saxon.Hej.lib.Validation.SKIP;
    string valAtt = element.getAttributeValue("", "validation");
    if ((valAtt != null)) {
        validation = Saxon.Hej.lib.Validation.getCode(valAtt);
    }
    Saxon.Hej.type.SchemaType schemaType = loader.getTypeAttribute(element, "type");
    if ((schemaType != null)) {
        validation = Saxon.Hej.lib.Validation.BY_TYPE;
    }
    Saxon.Hej.expr.instruct.ComputedAttribute att = new Saxon.Hej.expr.instruct.ComputedAttribute(name, @namespace, null, validation, (Saxon.Hej.type.SimpleType)(schemaType), false);
    att.setSelect(content);
    return att;
} );
            Saxon.Impl.Helpers.MapUtils.Put(eMap, "compElem", (loader, element) => {
    Saxon.Hej.expr.Expression name = loader.getExpressionWithRole(element, "name");
    Saxon.Hej.expr.Expression @namespace = loader.getExpressionWithRole(element, "namespace");
    Saxon.Hej.expr.Expression content = loader.getExpressionWithRole(element, "content");
    int validation = Saxon.Hej.lib.Validation.SKIP;
    string valAtt = element.getAttributeValue("", "validation");
    if ((valAtt != null)) {
        validation = Saxon.Hej.lib.Validation.getCode(valAtt);
    }
    Saxon.Hej.type.SchemaType schemaType = loader.getTypeAttribute(element, "type");
    if ((schemaType != null)) {
        validation = Saxon.Hej.lib.Validation.BY_TYPE;
    }
    string flags = element.getAttributeValue("", "flags");
    Saxon.Hej.expr.instruct.ComputedElement inst = new Saxon.Hej.expr.instruct.ComputedElement(name, @namespace, schemaType, validation, true, false);
    if ((flags != null)) {
        inst.setInheritanceFlags(flags);
    }
    inst.setContentExpression(content);
    return inst.simplify();
} );
            Saxon.Impl.Helpers.MapUtils.Put(eMap, "conditionalSort", (loader, element) => {
    Saxon.Hej.expr.Expression lhs = loader.getFirstChildExpression(element);
    Saxon.Hej.expr.Expression rhs = loader.getSecondChildExpression(element);
    return new Saxon.Hej.expr.sort.ConditionalSorter(lhs, (Saxon.Hej.expr.sort.DocumentSorter)(rhs));
} );
            Saxon.Impl.Helpers.MapUtils.Put(eMap, "condCont", (loader, element) => {
    Saxon.Hej.expr.Expression @base = loader.getFirstChildExpression(element);
    return new Saxon.Hej.expr.instruct.WherePopulated(@base);
} );
            Saxon.Impl.Helpers.MapUtils.Put(eMap, "condSeq", (loader, element) => {
    Saxon.Hej.expr.Expression[] args = getChildExpressionArray(loader, element);
    return new Saxon.Hej.expr.instruct.ConditionalBlock(args);
} );
            Saxon.Impl.Helpers.MapUtils.Put(eMap, "consume", (loader, element) => {
    Saxon.Hej.expr.Expression arg = loader.getFirstChildExpression(element);
    return new Saxon.Hej.expr.ConsumingOperand(arg);
} );
            Saxon.Impl.Helpers.MapUtils.Put(eMap, "convert", (loader, element) => {
    Saxon.Hej.expr.Expression body = loader.getFirstChildExpression(element);
    Saxon.Hej.type.ItemType fromType = loader.parseAlphaCodeForItemType(element, "from");
    Saxon.Hej.type.ItemType toType = loader.parseAlphaCodeForItemType(element, "to");
    Saxon.Hej.expr.AtomicSequenceConverter asc = new Saxon.Hej.expr.AtomicSequenceConverter(body, (Saxon.Hej.type.PlainType)(toType));
    if ("p".Equals(element.getAttributeValue("", "flags"))) {
        if (toType.Equals(Saxon.Hej.type.BuiltInAtomicType.DOUBLE)) {
            asc.setConverter(new Saxon.Hej.type.Converter.PromoterToDouble());
        } else {
            asc.setConverter(new Saxon.Hej.type.Converter.PromoterToFloat());
        }
    } else {
        Saxon.Hej.type.Converter c = asc.allocateConverter(loader.config, false, fromType);
        asc.setConverter(c);
    }
    string diag = element.getAttributeValue("", "diag");
    if ((diag != null)) {
        asc.setRoleDiagnostic(Saxon.Hej.expr.parser.RoleDiagnostic.reconstruct(diag));
    }
    return asc;
} );
            Saxon.Impl.Helpers.MapUtils.Put(eMap, "copy", (loader, element) => {
    int validation = Saxon.Hej.lib.Validation.SKIP;
    string valAtt = element.getAttributeValue("", "validation");
    if ((valAtt != null)) {
        validation = Saxon.Hej.lib.Validation.getCode(valAtt);
    }
    Saxon.Hej.type.SchemaType schemaType = loader.getTypeAttribute(element, "type");
    if ((schemaType != null)) {
        validation = Saxon.Hej.lib.Validation.BY_TYPE;
    }
    string sType = element.getAttributeValue("", "sit");
    Saxon.Hej.expr.instruct.Copy inst = new Saxon.Hej.expr.instruct.Copy(false, false, schemaType, validation);
    inst.setContentExpression(loader.getFirstChildExpression(element));
    string flags = element.getAttributeValue("", "flags");
    inst.setCopyNamespaces(flags.Contains("c"));
    inst.setBequeathNamespacesToChildren(flags.Contains("i"));
    inst.setInheritNamespacesFromParent(flags.Contains("n"));
    if ((sType != null)) {
        Saxon.Hej.value.SequenceType st = Saxon.Hej.type.AlphaCode.toSequenceType(sType, loader.getConfiguration());
        inst.setSelectItemType(st.getPrimaryType());
    }
    return inst;
} );
            Saxon.Impl.Helpers.MapUtils.Put(eMap, "copyOf", (loader, element) => {
    Saxon.Hej.expr.Expression select = loader.getFirstChildExpression(element);
    string flags = element.getAttributeValue("", "flags");
    if ((flags == null)) {
        flags = "";
    }
    bool copyNamespaces = flags.Contains("c");
    bool rejectDups = flags.Contains("d");
    int validation = Saxon.Hej.lib.Validation.SKIP;
    string valAtt = element.getAttributeValue("", "validation");
    if ((valAtt != null)) {
        validation = Saxon.Hej.lib.Validation.getCode(valAtt);
    }
    Saxon.Hej.type.SchemaType schemaType = loader.getTypeAttribute(element, "type");
    if ((schemaType != null)) {
        validation = Saxon.Hej.lib.Validation.BY_TYPE;
    }
    Saxon.Hej.expr.instruct.CopyOf inst = new Saxon.Hej.expr.instruct.CopyOf(select, copyNamespaces, validation, schemaType, rejectDups);
    inst.setCopyAccumulators(flags.Contains("m"));
    inst.setCopyLineNumbers(flags.Contains("l"));
    inst.setSchemaAware(flags.Contains("s"));
    inst.setCopyForUpdate(flags.Contains("u"));
    return inst;
} );
            Saxon.Impl.Helpers.MapUtils.Put(eMap, "currentGroup", (loader, element) => new Saxon.Hej.functions.CurrentGroupCall());
            Saxon.Impl.Helpers.MapUtils.Put(eMap, "currentGroupingKey", (loader, element) => new Saxon.Hej.functions.CurrentGroupingKeyCall());
            Saxon.Impl.Helpers.MapUtils.Put(eMap, "curriedFunc", (loader, element) => {
    Saxon.Hej.expr.Expression target = loader.getFirstChildExpression(element);
    Saxon.Hej.om.Function targetFn = (Saxon.Hej.om.Function)(((Saxon.Hej.expr.Literal)(target)).getGroundedValue());
    Saxon.Hej.om.NodeInfo args = loader.getChild(element, 1);
    int count = Saxon.Hej.functions.Count.count(args.iterateAxis(Saxon.Hej.om.AxisInfo.CHILD, Saxon.Hej.pattern.NodeKindTest.ELEMENT));
    Saxon.Hej.om.Sequence[] argValues = new Saxon.Hej.om.Sequence[count];
    count = 0;
    foreach (Saxon.Hej.om.NodeInfo child in args.children(Saxon.Hej.pattern.NodeKindTest.ELEMENT)) {
        if (child.getLocalPart().Equals("x")) {
            argValues[count++] = null;
        } else {
            Saxon.Hej.expr.Expression arg = loader.loadExpression(child);
            argValues[count++] = ((Saxon.Hej.expr.Literal)(arg)).getGroundedValue();
        }
    }
    Saxon.Hej.om.Function f = new Saxon.Hej.functions.hof.CurriedFunction(targetFn, argValues);
    return Saxon.Hej.expr.Literal.makeLiteral(f);
} );
            Saxon.Impl.Helpers.MapUtils.Put(eMap, "cvUntyped", (loader, element) => {
    Saxon.Hej.expr.Expression body = loader.getFirstChildExpression(element);
    Saxon.Hej.type.ItemType toType = loader.parseAlphaCodeForItemType(element, "to");
    if (((Saxon.Hej.type.SimpleType)(toType)).isNamespaceSensitive()) {
        return Saxon.Hej.expr.UntypedSequenceConverter.makeUntypedSequenceRejector(loader.config, body, (Saxon.Hej.type.PlainType)(toType));
    } else {
        Saxon.Hej.expr.UntypedSequenceConverter cv = Saxon.Hej.expr.UntypedSequenceConverter.makeUntypedSequenceConverter(loader.config, body, (Saxon.Hej.type.PlainType)(toType));
        string diag = element.getAttributeValue("", "diag");
        if ((diag != null)) {
            cv.setRoleDiagnostic(Saxon.Hej.expr.parser.RoleDiagnostic.reconstruct(diag));
        }
        return cv;
    }
} );
            Saxon.Impl.Helpers.MapUtils.Put(eMap, "data", (loader, element) => {
    Saxon.Hej.expr.Expression body = loader.getFirstChildExpression(element);
    string diag = element.getAttributeValue("", "diag");
    return new Saxon.Hej.expr.Atomizer(body, ((diag == null) ? null : Saxon.Hej.expr.parser.RoleDiagnostic.reconstruct(diag)));
} );
            Saxon.Impl.Helpers.MapUtils.Put(eMap, "dbl", (loader, element) => {
    string val = element.getAttributeValue("", "val");
    double d = Saxon.Hej.type.StringToDouble.getInstance().stringToNumber(Saxon.Hej.str.StringView.of(val).tidy());
    return Saxon.Hej.expr.Literal.makeLiteral(new Saxon.Hej.value.DoubleValue(d));
} );
            Saxon.Impl.Helpers.MapUtils.Put(eMap, "dec", (loader, element) => {
    string val = element.getAttributeValue("", "val");
    return Saxon.Hej.expr.Literal.makeLiteral(Saxon.Hej.value.BigDecimalValue.makeDecimalValue(val, false).asAtomic());
} );
            Saxon.Impl.Helpers.MapUtils.Put(eMap, "doc", (loader, element) => {
    int validation = Saxon.Hej.lib.Validation.SKIP;
    string valAtt = element.getAttributeValue("", "validation");
    if ((valAtt != null)) {
        validation = Saxon.Hej.lib.Validation.getCode(valAtt);
    }
    Saxon.Hej.type.SchemaType schemaType = loader.getTypeAttribute(element, "type");
    if ((schemaType != null)) {
        validation = Saxon.Hej.lib.Validation.BY_TYPE;
    }
    string flags = element.getAttributeValue("", "flags");
    bool textOnly = ((flags != null) && flags.Contains("t"));
    string @base = element.getAttributeValue("", "base");
    string constantText = element.getAttributeValue("", "text");
    Saxon.Hej.expr.Expression body = loader.getFirstChildExpression(element);
    Saxon.Hej.expr.instruct.DocumentInstr inst = new Saxon.Hej.expr.instruct.DocumentInstr(textOnly, ((constantText == null) ? null : Saxon.Hej.str.StringView.tidy(constantText)));
    inst.setContentExpression(body);
    inst.setValidationAction(validation, schemaType);
    return inst;
} );
            Saxon.Impl.Helpers.MapUtils.Put(eMap, "docOrder", (loader, element) => {
    Saxon.Hej.expr.Expression select = loader.getFirstChildExpression(element);
    bool intra = element.getAttributeValue("", "intra").Equals("1");
    return new Saxon.Hej.expr.sort.DocumentSorter(select, intra);
} );
            Saxon.Impl.Helpers.MapUtils.Put(eMap, "dot", (loader, element) => {
    Saxon.Hej.expr.ContextItemExpression cie = new Saxon.Hej.expr.ContextItemExpression();
    Saxon.Hej.value.SequenceType st = loader.parseAlphaCode(element, "type");
    Saxon.Hej.type.ItemType type = st.getPrimaryType();
    bool maybeAbsent = false;
    if ("a".Equals(element.getAttributeValue("", "flags"))) {
        maybeAbsent = true;
    }
    Saxon.Hej.expr.parser.ContextItemStaticInfo info = loader.getConfiguration().makeContextItemStaticInfo(type, maybeAbsent);
    cie.setStaticInfo(info);
    return cie;
} );
            Saxon.Impl.Helpers.MapUtils.Put(eMap, "elem", (loader, element) => {
    string displayName = element.getAttributeValue("", "name");
    string[] parts;
    try {
        parts = Saxon.Hej.om.NameChecker.getQNameParts((displayName));
    } catch (Saxon.Hej.om.QNameException err) {
        throw new Saxon.Hej.trans.XPathException(err);
    }
    string nsuri = element.getAttributeValue("", "nsuri");
    Saxon.Hej.om.StructuredQName name = new Saxon.Hej.om.StructuredQName(parts[0], nsuri, parts[1]);
    Saxon.Hej.om.NodeName elemName = new Saxon.Hej.om.FingerprintedQName(name, loader.config.getNamePool());
    string ns = element.getAttributeValue("", "namespaces");
    Saxon.Hej.om.NamespaceMap bindings = Saxon.Hej.om.NamespaceMap.emptyMap();
    if (((ns != null) && !(ns.Length==0))) {
        string[] pairs = System.Text.RegularExpressions.Regex.Split(ns, " ");
        foreach (string pair in pairs) {
            int eq = pair.IndexOf('=');
            if ((eq>=0)) {
                string prefix = pair.Substring(0, eq - 0);
                if (prefix.Equals("#")) {
                    prefix = "";
                }
                string uri = pair.Substring((eq+1));
                if (uri.Equals("~")) {
                    uri = Saxon.Hej.lib.NamespaceConstant.getUriForConventionalPrefix(prefix);
                }
                bindings = bindings.put(prefix, uri);
            } else {
                Saxon.Hej.expr.parser.RetainedStaticContext rsc = loader.contextStack.Peek();
                string prefix = pair;
                if (prefix.Equals("#")) {
                    prefix = "";
                }
                string uri = rsc.getURIForPrefix(prefix, true);
                System.Diagnostics.Debug.Assert((uri != null));
                bindings = bindings.put(prefix, uri);
            }
        }
    }
    int validation = Saxon.Hej.lib.Validation.SKIP;
    string valAtt = element.getAttributeValue("", "validation");
    if ((valAtt != null)) {
        validation = Saxon.Hej.lib.Validation.getCode(valAtt);
    }
    Saxon.Hej.type.SchemaType schemaType = loader.getTypeAttribute(element, "type");
    if ((schemaType != null)) {
        validation = Saxon.Hej.lib.Validation.BY_TYPE;
    }
    Saxon.Hej.expr.Expression content = loader.getFirstChildExpression(element);
    Saxon.Hej.expr.instruct.FixedElement elem = new Saxon.Hej.expr.instruct.FixedElement(elemName, bindings, true, true, schemaType, validation);
    string flags = element.getAttributeValue("", "flags");
    if ((flags != null)) {
        elem.setInheritanceFlags(flags);
    }
    elem.setContentExpression(content);
    return elem;
} );
            Saxon.Impl.Helpers.MapUtils.Put(eMap, "empty", (loader, element) => Saxon.Hej.expr.Literal.makeLiteral(Saxon.Hej.value.EmptySequence.getInstance()));
            Saxon.Impl.Helpers.MapUtils.Put(eMap, "emptyTextNodeRemover", (loader, element) => {
    Saxon.Hej.expr.Expression body = loader.getFirstChildExpression(element);
    return new Saxon.Hej.expr.EmptyTextNodeRemover(body);
} );
            Saxon.Impl.Helpers.MapUtils.Put(eMap, "error", (loader, element) => {
    string message = element.getAttributeValue("", "message");
    string code = element.getAttributeValue("", "code");
    bool isTypeErr = "1".Equals(element.getAttributeValue("", "isTypeErr"));
    return new Saxon.Hej.expr.ErrorExpression(message, code, isTypeErr);
} );
            Saxon.Impl.Helpers.MapUtils.Put(eMap, "evaluate", (loader, element) => {
    Saxon.Hej.value.SequenceType required = loader.parseAlphaCode(element, "as");
    Saxon.Hej.expr.Expression xpath = loader.getExpressionWithRole(element, "xpath");
    Saxon.Hej.expr.Expression contextItem = loader.getExpressionWithRole(element, "cxt");
    Saxon.Hej.expr.Expression baseUri = loader.getExpressionWithRole(element, "baseUri");
    Saxon.Hej.expr.Expression namespaceContext = loader.getExpressionWithRole(element, "nsCxt");
    Saxon.Hej.expr.Expression schemaAware = loader.getExpressionWithRole(element, "sa");
    Saxon.Hej.expr.Expression dynamicParams = loader.getExpressionWithRole(element, "wp");
    Saxon.Hej.expr.Expression optionsOp = loader.getExpressionWithRole(element, "options");
    Saxon.Hej.expr.instruct.EvaluateInstr inst = new Saxon.Hej.expr.instruct.EvaluateInstr(xpath, required, contextItem, baseUri, namespaceContext, schemaAware);
    if ((optionsOp != null)) {
        inst.setOptionsExpression(optionsOp);
    }
    string namespaces = element.getAttributeValue("", "schNS");
    if ((namespaces != null)) {
        string[] uris = System.Text.RegularExpressions.Regex.Split(namespaces, " ");
        foreach (string nsUri in uris) {
            inst.importSchemaNamespace((nsUri.Equals("##") ? "" : nsUri));
        }
    }
    System.Collections.Generic.IList<Saxon.Hej.expr.instruct.WithParam> nonTunnelParams = new System.Collections.Generic.List<Saxon.Hej.expr.instruct.WithParam>();
    int slotNumber = 0;
    foreach (Saxon.Hej.om.NodeInfo wp in element.children(Saxon.Hej.pattern.NodeSelector.of(n => n.getLocalPart().Equals("withParam")))) {
        Saxon.Hej.expr.instruct.WithParam withParam = new Saxon.Hej.expr.instruct.WithParam();
        Saxon.Hej.om.StructuredQName paramName = loader.getQNameAttribute(wp, "name");
        withParam.setVariableQName(paramName);
        withParam.setSlotNumber(slotNumber++);
        Saxon.Hej.value.SequenceType reqType = loader.parseAlphaCode(wp, "as");
        withParam.setRequiredType(reqType);
        withParam.setSelectExpression(inst, loader.getFirstChildExpression(wp));
        nonTunnelParams.Add(withParam);
    }
    inst.setActualParameters(nonTunnelParams.ToArray());
    if ((dynamicParams != null)) {
        inst.setDynamicParams(dynamicParams);
    }
    return inst;
} );
            Saxon.Impl.Helpers.MapUtils.Put(eMap, "every", (loader, element) => {
    Saxon.Hej.expr.Expression select = loader.getFirstChildExpression(element);
    int slot = loader.getIntegerAttribute(element, "slot");
    Saxon.Hej.om.StructuredQName name = loader.getQNameAttribute(element, "var");
    Saxon.Hej.value.SequenceType requiredType = loader.parseAlphaCode(element, "as");
    Saxon.Hej.expr.QuantifiedExpression qEx = new Saxon.Hej.expr.QuantifiedExpression();
    qEx.setOperator(Saxon.Hej.expr.parser.Token.EVERY);
    qEx.setSequence(select);
    qEx.setRequiredType(requiredType);
    qEx.setSlotNumber(slot);
    qEx.setVariableQName(name);
    loader.localBindings.Push(qEx);
    Saxon.Hej.expr.Expression action = loader.getSecondChildExpression(element);
    loader.localBindings.Pop();
    qEx.setAction(action);
    return qEx;
} );
            Saxon.Impl.Helpers.MapUtils.Put(eMap, "except", (loader, element) => {
    Saxon.Hej.expr.Expression lhs = loader.getFirstChildExpression(element);
    Saxon.Hej.expr.Expression rhs = loader.getSecondChildExpression(element);
    return new Saxon.Hej.expr.VennExpression(lhs, Saxon.Hej.expr.parser.Token.EXCEPT, rhs);
} );
            Saxon.Impl.Helpers.MapUtils.Put(eMap, "false", (loader, element) => Saxon.Hej.expr.Literal.makeLiteral(Saxon.Hej.value.BooleanValue.FALSE));
            Saxon.Impl.Helpers.MapUtils.Put(eMap, "filter", (loader, element) => {
    Saxon.Hej.expr.Expression lhs = loader.getFirstChildExpression(element);
    Saxon.Hej.expr.Expression rhs = loader.getSecondChildExpression(element);
    string flags = element.getAttributeValue("", "flags");
    Saxon.Hej.expr.FilterExpression fe = new Saxon.Hej.expr.FilterExpression(lhs, rhs);
    fe.setFlags(flags);
    return fe;
} );
            Saxon.Impl.Helpers.MapUtils.Put(eMap, "first", (loader, element) => {
    Saxon.Hej.expr.Expression @base = loader.getFirstChildExpression(element);
    return Saxon.Hej.expr.FirstItemExpression.makeFirstItemExpression(@base);
} );
            Saxon.Impl.Helpers.MapUtils.Put(eMap, "fn", (loader, element) => {
    Saxon.Hej.expr.parser.RetainedStaticContext rsc = loader.makeRetainedStaticContext(element);
    loader.contextStack.Push(rsc);
    Saxon.Hej.expr.Expression[] args = getChildExpressionArray(loader, element);
    string name = element.getAttributeValue("", "name");
    if (name.Equals("_STRING-JOIN_2.0")) {
        name = "string-join";
    }
    Saxon.Hej.expr.Expression e = Saxon.Hej.functions.SystemFunction.makeCall(name, rsc, args);
    if ((e == null)) {
        throw new Saxon.Hej.trans.XPathException(((("Unknown system function "+name)+"#")+args.Length));
    }
    if ((e is Saxon.Hej.expr.SystemFunctionCall)) {
        ((Saxon.Hej.expr.SystemFunctionCall)(e)).allocateArgumentEvaluators(args);
        Saxon.Hej.functions.SystemFunction fn = ((Saxon.Hej.expr.SystemFunctionCall)(e)).getTargetFunction();
        fn.setRetainedStaticContext(rsc);
        Saxon.Hej.om.SequenceIterator iter = element.iterateAxis(Saxon.Hej.om.AxisInfo.ATTRIBUTE);
        Saxon.Hej.om.NodeInfo att;
        Saxon.Ejava.util.Properties props = new Saxon.Ejava.util.Properties();
        while (((att = (Saxon.Hej.om.NodeInfo)(iter.next())) != null)) {
            props.setProperty(att.getLocalPart(), att.getStringValue());
        }
        fn.importAttributes(props);
        loader.addCompletionAction(() => fn.fixArguments(args));
    }
    loader.contextStack.Pop();
    return e;
} );
            Saxon.Impl.Helpers.MapUtils.Put(eMap, "fnCoercer", (loader, element) => {
    Saxon.Hej.type.SpecificFunctionType type = (Saxon.Hej.type.SpecificFunctionType)(loader.parseAlphaCode(element, "to").getPrimaryType());
    Saxon.Hej.expr.parser.RoleDiagnostic role = Saxon.Hej.expr.parser.RoleDiagnostic.reconstruct(element.getAttributeValue("", "diag"));
    Saxon.Hej.expr.Expression arg = loader.getFirstChildExpression(element);
    return new Saxon.Hej.functions.hof.FunctionSequenceCoercer(arg, type, role);
} );
            Saxon.Impl.Helpers.MapUtils.Put(eMap, "fnRef", (loader, element) => {
    loader.needsPELicense("higher order functions");
    string name = element.getAttributeValue("", "name");
    int arity = loader.getIntegerAttribute(element, "arity");
    Saxon.Hej.expr.parser.RetainedStaticContext rsc = loader.makeRetainedStaticContext(element);
    Saxon.Hej.functions.SystemFunction f = null;
    if (name.StartsWith("Q{")) {
        Saxon.Hej.om.StructuredQName qName = Saxon.Hej.om.StructuredQName.fromEQName((name));
        string uri = qName.getURI();
        switch (uri) {
          case Saxon.Hej.lib.NamespaceConstant.MATH:
            f = Saxon.Hej.functions.MathFunctionSet.getInstance().makeFunction(qName.getLocalPart(), arity);
            break ;
          case Saxon.Hej.lib.NamespaceConstant.MAP_FUNCTIONS:
            f = Saxon.Hej.ma.map.MapFunctionSet.getInstance().makeFunction(qName.getLocalPart(), arity);
            break ;
          case Saxon.Hej.lib.NamespaceConstant.ARRAY_FUNCTIONS:
            f = Saxon.Hej.ma.arrays.ArrayFunctionSet.getInstance().makeFunction(qName.getLocalPart(), arity);
            break ;
          case Saxon.Hej.lib.NamespaceConstant.SAXON:
            f = loader.getConfiguration().bindSaxonExtensionFunction(qName.getLocalPart(), arity);
            break ;
        }
    } else {
        f = Saxon.Hej.functions.SystemFunction.makeFunction(name, rsc, arity);
    }
    if ((f == null)) {
        throw new Saxon.Hej.trans.XPathException(((("Unknown system function "+name)+"#")+arity), Saxon.Hej.trans.SaxonErrorCode.SXPK0002);
    }
    return new Saxon.Hej.functions.hof.FunctionLiteral(f);
} );
            Saxon.Impl.Helpers.MapUtils.Put(eMap, "follows", (loader, element) => {
    Saxon.Hej.expr.Expression lhs = loader.getFirstChildExpression(element);
    Saxon.Hej.expr.Expression rhs = loader.getSecondChildExpression(element);
    return new Saxon.Hej.expr.IdentityComparison(lhs, Saxon.Hej.expr.parser.Token.FOLLOWS, rhs);
} );
            Saxon.Impl.Helpers.MapUtils.Put(eMap, "for", (loader, element) => {
    Saxon.Hej.expr.Expression select = loader.getFirstChildExpression(element);
    int slot = loader.getIntegerAttribute(element, "slot");
    Saxon.Hej.om.StructuredQName name = loader.getQNameAttribute(element, "var");
    Saxon.Hej.value.SequenceType requiredType = loader.parseAlphaCode(element, "as");
    Saxon.Hej.expr.ForExpression forEx = new Saxon.Hej.expr.ForExpression();
    forEx.setSequence(select);
    forEx.setRequiredType(requiredType);
    forEx.setSlotNumber(slot);
    forEx.setVariableQName(name);
    loader.localBindings.Push(forEx);
    Saxon.Hej.expr.Expression action = loader.getSecondChildExpression(element);
    loader.localBindings.Pop();
    forEx.setAction(action);
    return forEx;
} );
            Saxon.Impl.Helpers.MapUtils.Put(eMap, "forEach", (loader, element) => {
    Saxon.Hej.expr.Expression lhs = loader.getFirstChildExpression(element);
    Saxon.Hej.expr.Expression rhs = loader.getSecondChildExpression(element);
    Saxon.Hej.expr.Expression threads = loader.getExpressionWithRole(element, "threads");
    if ((threads == null)) {
        Saxon.Hej.expr.instruct.ForEach forEach = new Saxon.Hej.expr.instruct.ForEach(lhs, rhs);
        Saxon.Hej.expr.Expression sep = loader.getExpressionWithRole(element, "separator");
        if ((sep != null)) {
            forEach.setSeparatorExpression(sep);
        }
        return forEach;
    } else {
        Saxon.Hej.expr.instruct.ForEach forEach = new Saxon.Hej.expr.instruct.ForEach(lhs, rhs, false, threads);
        Saxon.Hej.expr.Expression sep = loader.getExpressionWithRole(element, "separator");
        if ((sep != null)) {
            forEach.setSeparatorExpression(sep);
        }
        return loader.getConfiguration().obtainOptimizer().generateMultithreadedInstruction(forEach);
    }
} );
            Saxon.Impl.Helpers.MapUtils.Put(eMap, "forEachGroup", (loader, element) => {
    string algorithmAtt = element.getAttributeValue("", "algorithm");
    byte algo;
    if ("by".Equals(algorithmAtt)) {
        algo = Saxon.Hej.expr.instruct.ForEachGroup.GROUP_BY;
    } else {
        if ("adjacent".Equals(algorithmAtt)) {
            algo = Saxon.Hej.expr.instruct.ForEachGroup.GROUP_ADJACENT;
        } else {
            if ("starting".Equals(algorithmAtt)) {
                algo = Saxon.Hej.expr.instruct.ForEachGroup.GROUP_STARTING;
            } else {
                if ("ending".Equals(algorithmAtt)) {
                    algo = Saxon.Hej.expr.instruct.ForEachGroup.GROUP_ENDING;
                } else {
                    throw new Saxon.Ejava.lang.AssertionError();
                }
            }
        }
    }
    string flags = element.getAttributeValue("", "flags");
    bool composite = ((flags != null) && flags.Contains("c"));
    bool inFork = ((flags != null) && flags.Contains("k"));
    Saxon.Hej.expr.Expression select = loader.getExpressionWithRole(element, "select");
    Saxon.Hej.expr.Expression key;
    if (((algo == Saxon.Hej.expr.instruct.ForEachGroup.GROUP_BY) || (algo == Saxon.Hej.expr.instruct.ForEachGroup.GROUP_ADJACENT))) {
        key = loader.getExpressionWithRole(element, "key");
    } else {
        key = loader.getPatternWithRole(element, "match");
    }
    Saxon.Hej.expr.sort.SortKeyDefinitionList sortKeys = loader.loadSortKeyDefinitions(element);
    if ((sortKeys.size() == 0)) {
        sortKeys = null;
    }
    Saxon.Hej.expr.Expression collationNameExp = loader.getExpressionWithRole(element, "collation");
    Saxon.Hej.expr.Expression content = loader.getExpressionWithRole(element, "content");
    Saxon.Hej.lib.StringCollator collator = null;
    if ((collationNameExp is Saxon.Hej.expr.StringLiteral)) {
        string collationName = ((Saxon.Hej.expr.StringLiteral)(collationNameExp)).getString().ToString();
        collator = loader.config.getCollation(collationName);
    }
    Saxon.Hej.expr.instruct.ForEachGroup feg = new Saxon.Hej.expr.instruct.ForEachGroup(select, content, algo, key, collator, collationNameExp, sortKeys);
    feg.setComposite(composite);
    feg.setIsInFork(inFork);
    return feg;
} );
            Saxon.Impl.Helpers.MapUtils.Put(eMap, "fork", (loader, element) => {
    Saxon.Hej.expr.Expression[] args = getChildExpressionArray(loader, element);
    return new Saxon.Hej.expr.instruct.Fork(args);
} );
            Saxon.Impl.Helpers.MapUtils.Put(eMap, "gc", (loader, element) => {
    string opAtt = element.getAttributeValue("", "op");
    int op = getOperator(opAtt);
    Saxon.Hej.expr.Expression lhs = loader.getFirstChildExpression(element);
    Saxon.Hej.expr.Expression rhs = loader.getSecondChildExpression(element);
    string compAtt = element.getAttributeValue("", "comp");
    Saxon.Hej.expr.sort.AtomicComparer comp = loader.makeAtomicComparer(compAtt, element);
    Saxon.Hej.expr.GeneralComparison gc = new Saxon.Hej.expr.GeneralComparison20(lhs, op, rhs);
    gc.setAtomicComparer(comp);
    return gc;
} );
            Saxon.Impl.Helpers.MapUtils.Put(eMap, "gc10", (loader, element) => {
    string opAtt = element.getAttributeValue("", "op");
    int op = getOperator(opAtt);
    Saxon.Hej.expr.Expression lhs = loader.getFirstChildExpression(element);
    Saxon.Hej.expr.Expression rhs = loader.getSecondChildExpression(element);
    string compAtt = element.getAttributeValue("", "comp");
    Saxon.Hej.expr.compat.GeneralComparison10 gc = new Saxon.Hej.expr.compat.GeneralComparison10(lhs, op, rhs);
    Saxon.Hej.expr.sort.AtomicComparer comp = loader.makeAtomicComparer(compAtt, element);
    gc.setAtomicComparer(comp);
    return gc;
} );
            Saxon.Impl.Helpers.MapUtils.Put(eMap, "gVarRef", (loader, element) => {
    Saxon.Hej.om.StructuredQName name = loader.getQNameAttribute(element, "name");
    Saxon.Hej.expr.GlobalVariableReference @ref = new Saxon.Hej.expr.GlobalVariableReference(name);
    int bindingSlot = loader.getIntegerAttribute(element, "bSlot");
    @ref.setBindingSlot(bindingSlot);
    loader.addComponentFixup(@ref);
    return @ref;
} );
            Saxon.Impl.Helpers.MapUtils.Put(eMap, "homCheck", (loader, element) => {
    Saxon.Hej.expr.Expression body = loader.getFirstChildExpression(element);
    return new Saxon.Hej.expr.HomogeneityChecker(body);
} );
            Saxon.Impl.Helpers.MapUtils.Put(eMap, "ifCall", (loader, element) => {
    Saxon.Hej.expr.Expression[] args = getChildExpressionArray(loader, element);
    Saxon.Hej.om.StructuredQName name = loader.getQNameAttribute(element, "name");
    Saxon.Hej.expr.Expression exp = null;
    if (name.hasURI(Saxon.Hej.lib.NamespaceConstant.MATH)) {
        exp = Saxon.Hej.functions.MathFunctionSet.getInstance().makeFunction(name.getLocalPart(), args.Length).makeFunctionCall(args);
    } else {
        if (name.hasURI(Saxon.Hej.lib.NamespaceConstant.MAP_FUNCTIONS)) {
            exp = Saxon.Hej.ma.map.MapFunctionSet.getInstance().makeFunction(name.getLocalPart(), args.Length).makeFunctionCall(args);
        } else {
            if (name.hasURI(Saxon.Hej.lib.NamespaceConstant.ARRAY_FUNCTIONS)) {
                exp = Saxon.Hej.ma.arrays.ArrayFunctionSet.getInstance().makeFunction(name.getLocalPart(), args.Length).makeFunctionCall(args);
            } else {
                if (name.hasURI(Saxon.Hej.lib.NamespaceConstant.SAXON)) {
                    loader.needsPELicense("Saxon extension functions");
                    exp = null;
                }
            }
        }
    }
    if ((exp == null)) {
        Saxon.Hej.trans.SymbolicName.F sName = new Saxon.Hej.trans.SymbolicName.F(name, args.Length);
        Saxon.Hej.value.SequenceType type = loader.parseAlphaCode(element, "type");
        Saxon.Hej.sxpath.IndependentContext ic = new Saxon.Hej.sxpath.IndependentContext(loader.config);
        Saxon.Hej.expr.parser.RetainedStaticContext rsc = loader.makeRetainedStaticContext(element);
        ic.setBaseURI(rsc.getStaticBaseUriString());
        ic.setPackageData(rsc.getPackageData());
        ic.setXPathLanguageLevel(31);
        ic.setDefaultElementNamespace(rsc.getDefaultElementNamespace());
        ic.setNamespaceResolver(rsc);
        ic.setBackwardsCompatibilityMode(rsc.isBackwardsCompatibility());
        ic.setDefaultCollationName(rsc.getDefaultCollationName());
        ic.setDefaultFunctionNamespace(rsc.getDefaultFunctionNamespace());
        ic.setDecimalFormatManager(rsc.getDecimalFormatManager());
        System.Collections.Generic.IList<string> reasons = new System.Collections.Generic.List<string>();
        exp = loader.config.getIntegratedFunctionLibrary().bind(sName, args, ic, reasons);
        if ((exp == null)) {
            exp = loader.config.getBuiltInExtensionLibraryList().bind(sName, args, ic, reasons);
        }
        if ((exp is Saxon.Hej.expr.SystemFunctionCall)) {
            Saxon.Hej.functions.SystemFunction fn = ((Saxon.Hej.expr.SystemFunctionCall)(exp)).getTargetFunction();
            fn.setRetainedStaticContext(loader.makeRetainedStaticContext(element));
            Saxon.Hej.om.SequenceIterator iter = element.iterateAxis(Saxon.Hej.om.AxisInfo.ATTRIBUTE);
            Saxon.Hej.om.NodeInfo att;
            Saxon.Ejava.util.Properties props = new Saxon.Ejava.util.Properties();
            while (((att = (Saxon.Hej.om.NodeInfo)(iter.next())) != null)) {
                props.setProperty(att.getLocalPart(), att.getStringValue());
            }
            fn.importAttributes(props);
        }
        if ((exp == null)) {
            Saxon.Ejava.lang.StringBuilder msg = new Saxon.Ejava.lang.StringBuilder((("IntegratedFunctionCall to "+sName)+" not found"));
            foreach (string reason in reasons) {
                msg.append(". ").append(reason);
            }
            throw new Saxon.Hej.trans.XPathException(msg.ToString());
        }
        if ((exp is Saxon.Hej.functions.IntegratedFunctionCall)) {
            ((Saxon.Hej.functions.IntegratedFunctionCall)(exp)).getFunction().supplyStaticContext(ic, -1, args);
            ((Saxon.Hej.functions.IntegratedFunctionCall)(exp)).setResultType(type);
        }
    }
    if ((exp is Saxon.Hej.expr.SystemFunctionCall)) {
        ((Saxon.Hej.expr.SystemFunctionCall)(exp)).allocateArgumentEvaluators(args);
    }
    return exp;
} );
            Saxon.Impl.Helpers.MapUtils.Put(eMap, "inlineFn", (loader, element) => {
    Saxon.Hej.om.NodeInfo first = loader.getChild(element, 0);
    Saxon.Hej.expr.instruct.UserFunction uf = loader.readFunction(first);
    return new Saxon.Hej.functions.hof.UserFunctionReference(uf);
} );
            Saxon.Impl.Helpers.MapUtils.Put(eMap, "instance", (loader, element) => {
    Saxon.Hej.expr.Expression body = loader.getFirstChildExpression(element);
    Saxon.Hej.value.SequenceType type = loader.parseAlphaCode(element, "of");
    return new Saxon.Hej.expr.InstanceOfExpression(body, type);
} );
            Saxon.Impl.Helpers.MapUtils.Put(eMap, "int", (loader, element) => {
    System.Numerics.BigInteger i = Saxon.Impl.Helpers.BigIntegerUtils.ValueOf(element.getAttributeValue("", "val"));
    return Saxon.Hej.expr.Literal.makeLiteral(Saxon.Hej.value.IntegerValue.makeIntegerValue(i));
} );
            Saxon.Impl.Helpers.MapUtils.Put(eMap, "intersect", (loader, element) => {
    Saxon.Hej.expr.Expression lhs = loader.getFirstChildExpression(element);
    Saxon.Hej.expr.Expression rhs = loader.getSecondChildExpression(element);
    return new Saxon.Hej.expr.VennExpression(lhs, Saxon.Hej.expr.parser.Token.INTERSECT, rhs);
} );
            Saxon.Impl.Helpers.MapUtils.Put(eMap, "intRangeTest", (loader, element) => {
    Saxon.Hej.expr.Expression val = loader.getFirstChildExpression(element);
    Saxon.Hej.expr.Expression min = loader.getSecondChildExpression(element);
    Saxon.Hej.expr.Expression max = loader.getNthChildExpression(element, 2);
    return new Saxon.Hej.expr.IntegerRangeTest(val, min, max);
} );
            Saxon.Impl.Helpers.MapUtils.Put(eMap, "is", (loader, element) => {
    Saxon.Hej.expr.Expression lhs = loader.getFirstChildExpression(element);
    Saxon.Hej.expr.Expression rhs = loader.getSecondChildExpression(element);
    return new Saxon.Hej.expr.IdentityComparison(lhs, Saxon.Hej.expr.parser.Token.IS, rhs);
} );
            Saxon.Impl.Helpers.MapUtils.Put(eMap, "isLast", (loader, element) => {
    bool cond = element.getAttributeValue("", "test").Equals("1");
    return new Saxon.Hej.expr.IsLastExpression(cond);
} );
            Saxon.Impl.Helpers.MapUtils.Put(eMap, "iterate", (loader, element) => {
    Saxon.Hej.expr.Expression select = loader.getExpressionWithRole(element, "select");
    Saxon.Hej.expr.instruct.LocalParamBlock @params = (Saxon.Hej.expr.instruct.LocalParamBlock)(loader.getExpressionWithRole(element, "params"));
    Saxon.Hej.expr.Expression onCompletion = loader.getExpressionWithRole(element, "on-completion");
    Saxon.Hej.expr.Expression action = loader.getExpressionWithRole(element, "action");
    return new Saxon.Hej.expr.instruct.IterateInstr(select, @params, action, onCompletion);
} );
            Saxon.Impl.Helpers.MapUtils.Put(eMap, "lastOf", (loader, element) => {
    Saxon.Hej.expr.Expression @base = loader.getFirstChildExpression(element);
    return new Saxon.Hej.expr.LastItemExpression(@base);
} );
            Saxon.Impl.Helpers.MapUtils.Put(eMap, "let", (loader, element) => {
    Saxon.Hej.expr.Expression select = loader.getFirstChildExpression(element);
    int slot = loader.getIntegerAttribute(element, "slot");
    int evalMode = loader.getIntegerAttribute(element, "eval");
    Saxon.Hej.om.StructuredQName name = loader.getQNameAttribute(element, "var");
    Saxon.Hej.value.SequenceType requiredType = loader.parseAlphaCode(element, "as");
    Saxon.Hej.expr.LetExpression let = new Saxon.Hej.expr.LetExpression();
    let.setSequence(select);
    let.setRequiredType(requiredType);
    let.setSlotNumber(slot);
    let.setVariableQName(name);
    let.setEvaluator(Saxon.Hej.expr.parser.Evaluators.getEvaluator(evalMode));
    loader.localBindings.Push(let);
    Saxon.Hej.expr.Expression action = loader.getSecondChildExpression(element);
    loader.localBindings.Pop();
    let.setAction(action);
    return let;
} );
            Saxon.Impl.Helpers.MapUtils.Put(eMap, "literal", (loader, element) => {
    System.Collections.Generic.IList<Saxon.Hej.om.Item> children = new System.Collections.Generic.List<Saxon.Hej.om.Item>();
    Saxon.Hej.tree.iter.AxisIterator iter = element.iterateAxis(Saxon.Hej.om.AxisInfo.CHILD, Saxon.Hej.pattern.NodeKindTest.ELEMENT);
    Saxon.Hej.om.NodeInfo child;
    while (((child = ((/*1430*/Saxon.Hej.om.NodeInfo)iter.next())) != null)) {
        Saxon.Hej.expr.Expression e = loader.loadExpression(child);
        children.Add(((Saxon.Hej.expr.Literal)(e)).getGroundedValue().head());
    }
    return Saxon.Hej.expr.Literal.makeLiteral(Saxon.Hej.value.SequenceExtent.makeSequenceExtent(children));
} );
            Saxon.Impl.Helpers.MapUtils.Put(eMap, "lookup", (loader, element) => {
    Saxon.Hej.expr.Expression select = loader.getFirstChildExpression(element);
    Saxon.Hej.expr.Expression key = loader.getSecondChildExpression(element);
    return new Saxon.Hej.expr.LookupExpression(select, key);
} );
            Saxon.Impl.Helpers.MapUtils.Put(eMap, "lookupAll", (loader, element) => {
    Saxon.Hej.expr.Expression select = loader.getFirstChildExpression(element);
    return new Saxon.Hej.expr.LookupAllExpression(select);
} );
            Saxon.Impl.Helpers.MapUtils.Put(eMap, "map", (loader, element) => {
    System.Collections.Generic.IList<Saxon.Hej.expr.Expression> children = getChildExpressionList(loader, element);
    Saxon.Hej.value.AtomicValue key = null;
    Saxon.Impl.DataModel.ImmMap map = new Saxon.Impl.DataModel.ImmMap();
    foreach (Saxon.Hej.expr.Expression child in children) {
        if ((key == null)) {
            key = (Saxon.Hej.value.AtomicValue)(((Saxon.Hej.expr.Literal)(child)).getGroundedValue());
        } else {
            Saxon.Hej.om.GroundedValue value = ((Saxon.Hej.expr.Literal)(child)).getGroundedValue();
            map.initialPut(key, value);
            key = null;
        }
    }
    return Saxon.Hej.expr.Literal.makeLiteral(map);
} );
            Saxon.Impl.Helpers.MapUtils.Put(eMap, "merge", (loader, element) => {
    Saxon.Hej.expr.sort.MergeInstr inst = new Saxon.Hej.expr.sort.MergeInstr();
    Saxon.Hej.tree.iter.AxisIterator kids = element.iterateAxis(Saxon.Hej.om.AxisInfo.CHILD, new Saxon.Hej.pattern.NameTest(Saxon.Hej.type.Type.ELEMENT, Saxon.Hej.lib.NamespaceConstant.SAXON_XSLT_EXPORT, "mergeSrc", loader.config.getNamePool()));
    Saxon.Hej.om.NodeInfo msElem;
    System.Collections.Generic.IList<Saxon.Hej.expr.sort.MergeInstr.MergeSource> list = new System.Collections.Generic.List<Saxon.Hej.expr.sort.MergeInstr.MergeSource>();
    while (((msElem = ((/*1430*/Saxon.Hej.om.NodeInfo)kids.next())) != null)) {
        Saxon.Hej.expr.sort.MergeInstr.MergeSource ms = new Saxon.Hej.expr.sort.MergeInstr.MergeSource(inst);
        string mergeSourceName = msElem.getAttributeValue("", "name");
        if ((mergeSourceName != null)) {
            ms.sourceName = mergeSourceName;
        }
        string valAtt = msElem.getAttributeValue("", "validation");
        if ((valAtt != null)) {
            ms.validation = Saxon.Hej.lib.Validation.getCode(valAtt);
        }
        Saxon.Hej.type.SchemaType schemaType = loader.getTypeAttribute(msElem, "type");
        if ((schemaType != null)) {
            ms.schemaType = schemaType;
            ms.validation = Saxon.Hej.lib.Validation.BY_TYPE;
        }
        string flagsAtt = msElem.getAttributeValue("", "flags");
        ms.streamable = "s".Equals(flagsAtt);
        if (ms.streamable) {
            loader.addCompletionAction(() => ms.prepareForStreaming());
        }
        Saxon.Hej.expr.parser.RetainedStaticContext rsc = loader.makeRetainedStaticContext(element);
        ms.baseURI = rsc.getStaticBaseUriString();
        string accumulatorNames = msElem.getAttributeValue("", "accum");
        if ((accumulatorNames == null)) {
            accumulatorNames = "";
        }
        System.Collections.Generic.IList<Saxon.Hej.om.StructuredQName> accNameList = new System.Collections.Generic.List<Saxon.Hej.om.StructuredQName>();
        Saxon.Ejava.util.StringTokenizer tokenizer = new Saxon.Ejava.util.StringTokenizer(accumulatorNames);
        while (tokenizer.hasMoreTokens()) {
            string token = tokenizer.nextToken();
            Saxon.Hej.om.StructuredQName name = Saxon.Hej.om.StructuredQName.fromEQName((token));
            accNameList.Add(name);
        }
        Saxon.Hej.style.StylesheetPackage pack = loader.getPackStack().Peek();
        loader.addCompletionAction(() => {
            System.Collections.Generic.ISet<Saxon.Hej.expr.accum.Accumulator> accList = new System.Collections.Generic.HashSet<Saxon.Hej.expr.accum.Accumulator>();
            foreach (Saxon.Hej.om.StructuredQName sn in accNameList) {
                foreach (Saxon.Hej.expr.accum.Accumulator test in pack.getAccumulatorRegistry().getAllAccumulators()) {
                    if (test.getAccumulatorName().Equals(sn)) {
                        accList.Add(test);
                    }
                }
            }
            ms.accumulators = accList;
        } );
        Saxon.Hej.expr.Expression forEachItem = loader.getExpressionWithRole(msElem, "forEachItem");
        if ((forEachItem != null)) {
            ms.initForEachItem(inst, forEachItem);
        }
        Saxon.Hej.expr.Expression forEachStream = loader.getExpressionWithRole(msElem, "forEachStream");
        if ((forEachStream != null)) {
            ms.initForEachStream(inst, forEachStream);
        }
        Saxon.Hej.expr.Expression selectRows = loader.getExpressionWithRole(msElem, "selectRows");
        if ((selectRows != null)) {
            ms.initRowSelect(inst, selectRows);
        }
        Saxon.Hej.expr.sort.SortKeyDefinitionList keys = loader.loadSortKeyDefinitions(msElem);
        ms.setMergeKeyDefinitionSet(keys);
        list.Add(ms);
    }
    Saxon.Hej.expr.Expression mergeAction = loader.getExpressionWithRole(element, "action");
    Saxon.Hej.expr.sort.MergeInstr.MergeSource[] mergeSources = list.ToArray();
    inst.init(mergeSources, mergeAction);
    loader.addCompletionAction(() => inst.fixupGroupReferences());
    return inst;
} );
            Saxon.Impl.Helpers.MapUtils.Put(eMap, "mergeAdj", (loader, element) => {
    Saxon.Hej.expr.Expression body = loader.getFirstChildExpression(element);
    return new Saxon.Hej.expr.AdjacentTextNodeMerger(body);
} );
            Saxon.Impl.Helpers.MapUtils.Put(eMap, "message", (loader, element) => {
    Saxon.Hej.expr.Expression select = loader.getExpressionWithRole(element, "select");
    Saxon.Hej.expr.Expression terminate = loader.getExpressionWithRole(element, "terminate");
    Saxon.Hej.expr.Expression error = loader.getExpressionWithRole(element, "error");
    return new Saxon.Hej.expr.instruct.MessageInstr(select, terminate, error);
} );
            Saxon.Impl.Helpers.MapUtils.Put(eMap, "minus", (loader, element) => {
    Saxon.Hej.expr.Expression body = loader.getFirstChildExpression(element);
    return new Saxon.Hej.expr.NegateExpression(body);
} );
            Saxon.Impl.Helpers.MapUtils.Put(eMap, "namespace", (loader, element) => {
    Saxon.Hej.expr.Expression name = loader.getFirstChildExpression(element);
    Saxon.Hej.expr.Expression select = loader.getSecondChildExpression(element);
    Saxon.Hej.expr.instruct.NamespaceConstructor inst = new Saxon.Hej.expr.instruct.NamespaceConstructor(name);
    inst.setSelect(select);
    return inst;
} );
            Saxon.Impl.Helpers.MapUtils.Put(eMap, "nextIteration", (loader, element) => {
    Saxon.Hej.expr.instruct.NextIteration inst = new Saxon.Hej.expr.instruct.NextIteration();
    Saxon.Hej.tree.iter.AxisIterator kids = element.iterateAxis(Saxon.Hej.om.AxisInfo.CHILD, new Saxon.Hej.pattern.NameTest(Saxon.Hej.type.Type.ELEMENT, Saxon.Hej.lib.NamespaceConstant.SAXON_XSLT_EXPORT, "withParam", loader.config.getNamePool()));
    Saxon.Hej.om.NodeInfo wp;
    System.Collections.Generic.IList<Saxon.Hej.expr.instruct.WithParam> @params = new System.Collections.Generic.List<Saxon.Hej.expr.instruct.WithParam>();
    while (((wp = ((/*1430*/Saxon.Hej.om.NodeInfo)kids.next())) != null)) {
        Saxon.Hej.expr.instruct.WithParam withParam = new Saxon.Hej.expr.instruct.WithParam();
        string flags = wp.getAttributeValue("", "flags");
        Saxon.Hej.om.StructuredQName paramName = loader.getQNameAttribute(wp, "name");
        withParam.setVariableQName(paramName);
        int slot = loader.getIntegerAttribute(wp, "slot");
        withParam.setSlotNumber(slot);
        withParam.setRequiredType(Saxon.Hej.value.SequenceType.ANY_SEQUENCE);
        withParam.setSelectExpression(inst, loader.getFirstChildExpression(wp));
        withParam.setRequiredType(loader.parseAlphaCode(wp, "as"));
        withParam.setTypeChecked(((flags != null) && flags.Contains("c")));
        @params.Add(withParam);
    }
    inst.setParameters(@params.ToArray());
    return inst;
} );
            Saxon.Impl.Helpers.MapUtils.Put(eMap, "nextMatch", (loader, element) => {
    string flags = element.getAttributeValue("", "flags");
    bool useTailRecursion = false;
    if (((flags != null) && flags.Contains("t"))) {
        useTailRecursion = true;
    }
    Saxon.Hej.expr.instruct.NextMatch inst = new Saxon.Hej.expr.instruct.NextMatch(useTailRecursion);
    Saxon.Hej.expr.instruct.WithParam[] actuals = loader.loadWithParams(element, inst, false);
    Saxon.Hej.expr.instruct.WithParam[] tunnels = loader.loadWithParams(element, inst, true);
    inst.setActualParams(actuals);
    inst.setTunnelParams(tunnels);
    return inst;
} );
            Saxon.Impl.Helpers.MapUtils.Put(eMap, "node", (loader, element) => {
    int kind = loader.getIntegerAttribute(element, "kind");
    string content = element.getAttributeValue("", "content");
    string baseURI = element.getAttributeValue("", "baseURI");
    Saxon.Hej.om.NodeInfo node;
    switch (kind) {
      case Saxon.Hej.type.Type.DOCUMENT:
      case Saxon.Hej.type.Type.ELEMENT:
        {
            Saxon.Ejavax.xml.transform.stream.StreamSource source = new Saxon.Ejavax.xml.transform.stream.StreamSource(new System.IO.StringReader(content), baseURI);
            node = loader.config.buildDocumentTree(source).getRootNode();
            if ((kind == Saxon.Hej.type.Type.ELEMENT)) {
                node = Saxon.Hej.tree.wrapper.VirtualCopy.makeVirtualCopy(((/*1430*/Saxon.Hej.om.NodeInfo)node.iterateAxis(Saxon.Hej.om.AxisInfo.CHILD, Saxon.Hej.pattern.NodeKindTest.ELEMENT).next()));
            }
            break ;
        }
      case Saxon.Hej.type.Type.TEXT:
      case Saxon.Hej.type.Type.COMMENT:
        {
            Saxon.Hej.tree.util.Orphan o = new Saxon.Hej.tree.util.Orphan(loader.getConfiguration());
            o.setNodeKind((short)(kind));
            o.setStringValue(Saxon.Hej.str.StringView.tidy(content));
            node = o;
            break ;
        }
      default:
        {
            Saxon.Hej.tree.util.Orphan o = new Saxon.Hej.tree.util.Orphan(loader.getConfiguration());
            o.setNodeKind((short)(kind));
            o.setStringValue(Saxon.Hej.str.StringView.tidy(content));
            string prefix = element.getAttributeValue("", "prefix");
            string ns = element.getAttributeValue("", "ns");
            string local = element.getAttributeValue("", "localName");
            if ((local != null)) {
                Saxon.Hej.om.FingerprintedQName name = new Saxon.Hej.om.FingerprintedQName(((prefix == null) ? "" : prefix), ((ns == null) ? "" : ns), local);
                o.setNodeName(name);
            }
            node = o;
            break ;
        }
    }
    return Saxon.Hej.expr.Literal.makeLiteral(node);
} );
            Saxon.Impl.Helpers.MapUtils.Put(eMap, "nodeNum", (loader, element) => {
    string levelAtt = element.getAttributeValue("", "level");
    int level = getLevelCode(levelAtt);
    Saxon.Hej.expr.Expression select = loader.getExpressionWithRole(element, "select");
    Saxon.Hej.pattern.Pattern count = loader.getPatternWithRole(element, "count");
    Saxon.Hej.pattern.Pattern from = loader.getPatternWithRole(element, "from");
    return new Saxon.Hej.expr.instruct.NumberInstruction(select, level, count, from);
} );
            Saxon.Impl.Helpers.MapUtils.Put(eMap, "numSeqFmt", (loader, element) => {
    Saxon.Hej.expr.Expression value = loader.getExpressionWithRole(element, "value");
    Saxon.Hej.expr.Expression format = loader.getExpressionWithRole(element, "format");
    if ((format == null)) {
        format = new Saxon.Hej.expr.StringLiteral("1");
    }
    Saxon.Hej.expr.Expression groupSize = loader.getExpressionWithRole(element, "gpSize");
    Saxon.Hej.expr.Expression groupSeparator = loader.getExpressionWithRole(element, "gpSep");
    Saxon.Hej.expr.Expression letterValue = loader.getExpressionWithRole(element, "letterValue");
    Saxon.Hej.expr.Expression ordinal = loader.getExpressionWithRole(element, "ordinal");
    Saxon.Hej.expr.Expression startAt = loader.getExpressionWithRole(element, "startAt");
    Saxon.Hej.expr.Expression lang = loader.getExpressionWithRole(element, "lang");
    string flags = element.getAttributeValue("", "flags");
    bool backwardsCompatible = ((flags != null) && flags.Contains("1"));
    Saxon.Hej.expr.number.NumberFormatter formatter = null;
    Saxon.Hej.expr.NumberSequenceFormatter ni = new Saxon.Hej.expr.NumberSequenceFormatter(value, format, groupSize, groupSeparator, letterValue, ordinal, startAt, lang, formatter, backwardsCompatible);
    ni.preallocateNumberer(loader.config);
    return ni;
} );
            Saxon.Impl.Helpers.MapUtils.Put(eMap, "onEmpty", (loader, element) => {
    Saxon.Hej.expr.Expression @base = loader.getFirstChildExpression(element);
    return new Saxon.Hej.expr.instruct.OnEmptyExpr(@base);
} );
            Saxon.Impl.Helpers.MapUtils.Put(eMap, "onNonEmpty", (loader, element) => {
    Saxon.Hej.expr.Expression @base = loader.getFirstChildExpression(element);
    return new Saxon.Hej.expr.instruct.OnNonEmptyExpr(@base);
} );
            Saxon.Impl.Helpers.MapUtils.Put(eMap, "or", (loader, element) => {
    Saxon.Hej.expr.Expression lhs = loader.getFirstChildExpression(element);
    Saxon.Hej.expr.Expression rhs = loader.getSecondChildExpression(element);
    return new Saxon.Hej.expr.OrExpression(lhs, rhs);
} );
            Saxon.Impl.Helpers.MapUtils.Put(eMap, "origF", (loader, element) => {
    Saxon.Hej.om.StructuredQName name = loader.getQNameAttribute(element, "name");
    string packKey = element.getAttributeValue("", "pack");
    Saxon.Hej.style.StylesheetPackage declPack = loader.getPackage(packKey);
    if ((declPack == null)) {
        throw new Saxon.Hej.trans.XPathException(("Unknown package key "+packKey));
    }
    int arity = loader.getIntegerAttribute(element, "arity");
    Saxon.Hej.trans.SymbolicName sn = new Saxon.Hej.trans.SymbolicName.F(name, arity);
    Saxon.Hej.expr.Component target = declPack.getComponent(sn);
    Saxon.Hej.expr.instruct.OriginalFunction orig = new Saxon.Hej.expr.instruct.OriginalFunction(target);
    return new Saxon.Hej.functions.hof.FunctionLiteral(orig);
} );
            Saxon.Impl.Helpers.MapUtils.Put(eMap, "origFC", (loader, element) => {
    Saxon.Hej.om.StructuredQName name = loader.getQNameAttribute(element, "name");
    string packKey = element.getAttributeValue("", "pack");
    Saxon.Hej.style.StylesheetPackage declPack = loader.getPackage(packKey);
    if ((declPack == null)) {
        throw new Saxon.Hej.trans.XPathException(("Unknown package key "+packKey));
    }
    Saxon.Hej.expr.Expression[] args = getChildExpressionArray(loader, element);
    int arity = args.Length;
    Saxon.Hej.trans.SymbolicName sn = new Saxon.Hej.trans.SymbolicName.F(name, arity);
    Saxon.Hej.expr.Component target = declPack.getComponent(sn);
    Saxon.Hej.expr.instruct.OriginalFunction orig = new Saxon.Hej.expr.instruct.OriginalFunction(target);
    return new Saxon.Hej.expr.StaticFunctionCall(orig, args);
} );
            Saxon.Impl.Helpers.MapUtils.Put(eMap, "param", (loader, element) => {
    Saxon.Hej.om.StructuredQName name = loader.getQNameAttribute(element, "name");
    int slot = loader.getIntegerAttribute(element, "slot");
    Saxon.Hej.expr.instruct.LocalParam param = new Saxon.Hej.expr.instruct.LocalParam();
    param.setVariableQName(name);
    param.setSlotNumber(slot);
    Saxon.Hej.expr.Expression select = loader.getExpressionWithRole(element, "select");
    if ((select != null)) {
        param.setSelectExpression(select);
        param.computeEvaluationMode();
    }
    Saxon.Hej.expr.Expression convert = loader.getExpressionWithRole(element, "conversion");
    if ((convert != null)) {
        param.setConversion(convert);
    }
    param.setRequiredType(loader.parseAlphaCode(element, "as"));
    string flags = element.getAttributeValue("", "flags");
    if ((flags != null)) {
        param.setTunnel(flags.Contains("t"));
        param.setRequiredParam(flags.Contains("r"));
        param.setImplicitlyRequiredParam(flags.Contains("i"));
    }
    loader.localBindings.Push(param);
    return param;
} );
            Saxon.Impl.Helpers.MapUtils.Put(eMap, "params", (loader, element) => {
    System.Collections.Generic.IList<Saxon.Hej.expr.instruct.LocalParam> children = new System.Collections.Generic.List<Saxon.Hej.expr.instruct.LocalParam>();
    Saxon.Hej.tree.iter.AxisIterator iter = element.iterateAxis(Saxon.Hej.om.AxisInfo.CHILD, Saxon.Hej.pattern.NodeKindTest.ELEMENT);
    Saxon.Hej.om.NodeInfo child;
    while (((child = ((/*1430*/Saxon.Hej.om.NodeInfo)iter.next())) != null)) {
        children.Add((Saxon.Hej.expr.instruct.LocalParam)(loader.loadExpression(child)));
    }
    return new Saxon.Hej.expr.instruct.LocalParamBlock(children.ToArray());
} );
            Saxon.Impl.Helpers.MapUtils.Put(eMap, "partialApply", (loader, element) => {
    int count = Saxon.Hej.functions.Count.count(element.iterateAxis(Saxon.Hej.om.AxisInfo.CHILD, Saxon.Hej.pattern.NodeKindTest.ELEMENT));
    Saxon.Hej.expr.Expression @base = null;
    Saxon.Hej.expr.Expression[] args = new Saxon.Hej.expr.Expression[(count-1)];
    count = 0;
    foreach (Saxon.Hej.om.NodeInfo child in element.children(Saxon.Hej.pattern.NodeKindTest.ELEMENT)) {
        if ((count == 0)) {
            @base = loader.loadExpression(child);
        } else {
            if (child.getLocalPart().Equals("null")) {
                args[(count-1)] = null;
            } else {
                args[(count-1)] = loader.loadExpression(child);
            }
        }
        count++;
    }
    return new Saxon.Hej.functions.hof.PartialApply(@base, args);
} );
            Saxon.Impl.Helpers.MapUtils.Put(eMap, "precedes", (loader, element) => {
    Saxon.Hej.expr.Expression lhs = loader.getFirstChildExpression(element);
    Saxon.Hej.expr.Expression rhs = loader.getSecondChildExpression(element);
    return new Saxon.Hej.expr.IdentityComparison(lhs, Saxon.Hej.expr.parser.Token.PRECEDES, rhs);
} );
            Saxon.Impl.Helpers.MapUtils.Put(eMap, "procInst", (loader, element) => {
    Saxon.Hej.expr.Expression name = loader.getFirstChildExpression(element);
    Saxon.Hej.expr.Expression select = loader.getSecondChildExpression(element);
    Saxon.Hej.expr.instruct.ProcessingInstruction inst = new Saxon.Hej.expr.instruct.ProcessingInstruction(name);
    inst.setSelect(select);
    return inst;
} );
            Saxon.Impl.Helpers.MapUtils.Put(eMap, "qName", (loader, element) => {
    string preAtt = element.getAttributeValue("", "pre");
    string uriAtt = element.getAttributeValue("", "uri");
    string locAtt = element.getAttributeValue("", "loc");
    Saxon.Hej.type.AtomicType type = Saxon.Hej.type.BuiltInAtomicType.QNAME;
    if ((element.getAttributeValue("", "type") != null)) {
        type = (Saxon.Hej.type.AtomicType)(loader.parseItemTypeAttribute(element, "type"));
    }
    Saxon.Hej.value.QualifiedNameValue val;
    if ((type.getPrimitiveType() == Saxon.Hej.om.StandardNames.XS_QNAME)) {
        val = new Saxon.Hej.value.QNameValue(preAtt, uriAtt, locAtt, type, false);
    } else {
        val = new Saxon.Hej.value.NotationValue(preAtt, uriAtt, locAtt, null);
        val.setTypeLabel(type);
    }
    return Saxon.Hej.expr.Literal.makeLiteral(val);
} );
            Saxon.Impl.Helpers.MapUtils.Put(eMap, "range", (loader, element) => {
    int from = loader.getIntegerAttribute(element, "from");
    int to = loader.getIntegerAttribute(element, "to");
    return Saxon.Hej.expr.Literal.makeLiteral(new Saxon.Hej.value.IntegerRange(from, 1, to));
} );
            Saxon.Impl.Helpers.MapUtils.Put(eMap, "resultDoc", (loader, element) => {
    loader.packStack.Peek().setCreatesSecondaryResultDocuments(true);
    Saxon.Hej.expr.Expression href = null;
    Saxon.Hej.expr.Expression format = null;
    Saxon.Hej.expr.Expression content = null;
    string globalProps = element.getAttributeValue("", "global");
    string localProps = element.getAttributeValue("", "local");
    Saxon.Ejava.util.Properties globals = ((globalProps == null) ? new Saxon.Ejava.util.Properties() : loader.importProperties(globalProps));
    Saxon.Ejava.util.Properties locals = ((localProps == null) ? new Saxon.Ejava.util.Properties() : loader.importProperties(localProps));
    System.Collections.Generic.IDictionary<Saxon.Hej.om.StructuredQName,Saxon.Hej.expr.Expression> dynamicProperties = new System.Collections.Generic.Dictionary<Saxon.Hej.om.StructuredQName, Saxon.Hej.expr.Expression>();
    Saxon.Hej.om.NodeInfo child;
    Saxon.Hej.tree.iter.AxisIterator iter = element.iterateAxis(Saxon.Hej.om.AxisInfo.CHILD, Saxon.Hej.pattern.NodeKindTest.ELEMENT);
    while (((child = ((/*1430*/Saxon.Hej.om.NodeInfo)iter.next())) != null)) {
        Saxon.Hej.expr.Expression exp = loader.loadExpression(child);
        string role = child.getAttributeValue("", "role");
        if ("href".Equals(role)) {
            href = exp;
        } else {
            if ("format".Equals(role)) {
                format = exp;
            } else {
                if ("content".Equals(role)) {
                    content = exp;
                } else {
                    Saxon.Hej.om.StructuredQName name = Saxon.Hej.om.StructuredQName.fromEQName((role));
                    Saxon.Impl.Helpers.MapUtils.Put(dynamicProperties, name, exp);
                }
            }
        }
    }
    int validation = Saxon.Hej.lib.Validation.SKIP;
    string valAtt = element.getAttributeValue("", "validation");
    if ((valAtt != null)) {
        validation = Saxon.Hej.lib.Validation.getCode(valAtt);
    }
    Saxon.Hej.type.SchemaType schemaType = null;
    Saxon.Hej.om.StructuredQName typeAtt = loader.getQNameAttribute(element, "type");
    if ((typeAtt != null)) {
        schemaType = loader.config.getSchemaType(typeAtt);
        validation = Saxon.Hej.lib.Validation.BY_TYPE;
    }
    Saxon.Hej.expr.instruct.ResultDocument instr = new Saxon.Hej.expr.instruct.ResultDocument(globals, locals, href, format, validation, schemaType, dynamicProperties, loader.packStack.Peek().getCharacterMapIndex());
    instr.setContentExpression(content);
    if ("a".Equals(element.getAttributeValue("", "flags"))) {
        instr.setAsynchronous(true);
    }
    return instr;
} );
            Saxon.Impl.Helpers.MapUtils.Put(eMap, "root", (loader, element) => new Saxon.Hej.expr.RootExpression());
            Saxon.Impl.Helpers.MapUtils.Put(eMap, "saxonDoctype", (loader, element) => {
    Saxon.Hej.expr.Expression arg = loader.getFirstChildExpression(element);
    return new Saxon.Hej.expr.instruct.Doctype(arg);
} );
            Saxon.Impl.Helpers.MapUtils.Put(eMap, "sequence", (loader, element) => {
    Saxon.Hej.expr.Expression[] args = getChildExpressionArray(loader, element);
    return new Saxon.Hej.expr.instruct.Block(args);
} );
            Saxon.Impl.Helpers.MapUtils.Put(eMap, "slash", (loader, element) => {
    Saxon.Hej.expr.Expression lhs = loader.getFirstChildExpression(element);
    Saxon.Hej.expr.Expression rhs = loader.getSecondChildExpression(element);
    string simpleAtt = element.getAttributeValue("", "simple");
    if ("1".Equals(simpleAtt)) {
        return new Saxon.Hej.expr.SimpleStepExpression(lhs, rhs);
    } else {
        Saxon.Hej.expr.SlashExpression se = new Saxon.Hej.expr.SlashExpression(lhs, rhs);
        if ("2".Equals(simpleAtt)) {
            se.setContextFree(true);
        }
        return se;
    }
} );
            Saxon.Impl.Helpers.MapUtils.Put(eMap, "some", (loader, element) => {
    Saxon.Hej.expr.Expression select = loader.getFirstChildExpression(element);
    int slot = loader.getIntegerAttribute(element, "slot");
    Saxon.Hej.om.StructuredQName name = loader.getQNameAttribute(element, "var");
    Saxon.Hej.value.SequenceType requiredType = loader.parseAlphaCode(element, "as");
    Saxon.Hej.expr.QuantifiedExpression qEx = new Saxon.Hej.expr.QuantifiedExpression();
    qEx.setOperator(Saxon.Hej.expr.parser.Token.SOME);
    qEx.setSequence(select);
    qEx.setRequiredType(requiredType);
    qEx.setSlotNumber(slot);
    qEx.setVariableQName(name);
    loader.localBindings.Push(qEx);
    Saxon.Hej.expr.Expression action = loader.getSecondChildExpression(element);
    loader.localBindings.Pop();
    qEx.setAction(action);
    return qEx;
} );
            Saxon.Impl.Helpers.MapUtils.Put(eMap, "sort", (loader, element) => {
    Saxon.Hej.expr.Expression body = loader.getFirstChildExpression(element);
    Saxon.Hej.expr.sort.SortKeyDefinitionList sortKeys = loader.loadSortKeyDefinitions(element);
    return new Saxon.Hej.expr.sort.SortExpression(body, sortKeys);
} );
            Saxon.Impl.Helpers.MapUtils.Put(eMap, "sourceDoc", (loader, element) => {
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
    if ((flags != null)) {
        if (flags.Contains("s")) {
            loader.addCompletionAction(() => options.setSpaceStrippingRule(loader.getTopLevelPackage().getSpaceStrippingRule()));
        }
        if (flags.Contains("S")) {
            options.setSpaceStrippingRule(Saxon.Hej.om.AllElementsSpaceStrippingRule.getInstance());
        }
        if (flags.Contains("l")) {
            options.setLineNumbering(true);
        }
        options.setExpandAttributeDefaults(flags.Contains("a"));
        if (flags.Contains("d")) {
            options.setDTDValidationMode(Saxon.Hej.lib.Validation.STRICT);
        }
        if (flags.Contains("i")) {
            options.setXIncludeAware(true);
        }
    }
    Saxon.Hej.expr.Expression body = loader.getExpressionWithRole(element, "body");
    Saxon.Hej.expr.Expression href = loader.getExpressionWithRole(element, "href");
    Saxon.Hej.expr.instruct.SourceDocument inst = new Saxon.Hej.expr.instruct.SourceDocument(href, body, options);
    string accumulatorNames = element.getAttributeValue("", "accum");
    processAccumulatorList(loader, inst, accumulatorNames);
    return inst;
} );
            Saxon.Impl.Helpers.MapUtils.Put(eMap, "str", (loader, element) => Saxon.Hej.expr.StringLiteral.makeLiteral(new Saxon.Hej.value.StringValue(element.getAttributeValue("", "val"))));
            Saxon.Impl.Helpers.MapUtils.Put(eMap, "subscript", (loader, element) => {
    Saxon.Hej.expr.Expression lhs = loader.getFirstChildExpression(element);
    Saxon.Hej.expr.Expression rhs = loader.getSecondChildExpression(element);
    return new Saxon.Hej.expr.SubscriptExpression(lhs, rhs);
} );
            Saxon.Impl.Helpers.MapUtils.Put(eMap, "supplied", (loader, element) => {
    int slot = loader.getIntegerAttribute(element, "slot");
    return new Saxon.Hej.expr.SuppliedParameterReference(slot);
} );
            Saxon.Impl.Helpers.MapUtils.Put(eMap, "tail", (loader, element) => {
    Saxon.Hej.expr.Expression select = loader.getFirstChildExpression(element);
    int start = loader.getIntegerAttribute(element, "start");
    return new Saxon.Hej.expr.TailExpression(select, start);
} );
            Saxon.Impl.Helpers.MapUtils.Put(eMap, "tailCallLoop", (loader, element) => {
    Saxon.Hej.expr.Expression body = loader.getFirstChildExpression(element);
    return new Saxon.Hej.expr.TailCallLoop(loader.currentFunction, body);
} );
            Saxon.Impl.Helpers.MapUtils.Put(eMap, "to", (loader, element) => {
    Saxon.Hej.expr.Expression lhs = loader.getFirstChildExpression(element);
    Saxon.Hej.expr.Expression rhs = loader.getSecondChildExpression(element);
    return new Saxon.Hej.expr.RangeExpression(lhs, rhs);
} );
            Saxon.Impl.Helpers.MapUtils.Put(eMap, "treat", (loader, element) => {
    Saxon.Hej.expr.Expression body = loader.getFirstChildExpression(element);
    Saxon.Hej.type.ItemType type = loader.parseAlphaCodeForItemType(element, "as");
    Saxon.Hej.expr.parser.RoleDiagnostic role = Saxon.Hej.expr.parser.RoleDiagnostic.reconstruct(element.getAttributeValue("", "diag"));
    return new Saxon.Hej.expr.ItemChecker(body, type, role);
} );
            Saxon.Impl.Helpers.MapUtils.Put(eMap, "true", (loader, element) => Saxon.Hej.expr.Literal.makeLiteral(Saxon.Hej.value.BooleanValue.TRUE));
            Saxon.Impl.Helpers.MapUtils.Put(eMap, "try", (loader, element) => {
    Saxon.Hej.expr.Expression tryExp = loader.getFirstChildExpression(element);
    Saxon.Hej.expr.TryCatch tryCatch = new Saxon.Hej.expr.TryCatch(tryExp);
    if ("r".Equals(element.getAttributeValue("", "flags"))) {
        tryCatch.setRollbackOutput(true);
    }
    Saxon.Hej.tree.iter.AxisIterator iter = element.iterateAxis(Saxon.Hej.om.AxisInfo.CHILD, new Saxon.Hej.pattern.NameTest(Saxon.Hej.type.Type.ELEMENT, Saxon.Hej.lib.NamespaceConstant.SAXON_XSLT_EXPORT, "catch", loader.config.getNamePool()));
    Saxon.Hej.om.NodeInfo catchElement;
    Saxon.Hej.om.NamePool pool = loader.getConfiguration().getNamePool();
    while (((catchElement = ((/*1430*/Saxon.Hej.om.NodeInfo)iter.next())) != null)) {
        string errAtt = catchElement.getAttributeValue("", "errors");
        string[] tests = System.Text.RegularExpressions.Regex.Split(errAtt, " ");
        System.Collections.Generic.IList<Saxon.Hej.pattern.QNameTest> list = new System.Collections.Generic.List<Saxon.Hej.pattern.QNameTest>();
        foreach (string t in tests) {
            if (t.Equals("*")) {
                list.Add(Saxon.Hej.pattern.AnyNodeTest.getInstance());
            } else {
                if (t.StartsWith("*:")) {
                    list.Add(new Saxon.Hej.pattern.LocalNameTest(pool, Saxon.Hej.type.Type.ELEMENT, t.Substring(2)));
                } else {
                    if (t.EndsWith("}*")) {
                        list.Add(new Saxon.Hej.pattern.NamespaceTest(pool, Saxon.Hej.type.Type.ELEMENT, t.Substring(2, (t.Length-2) - 2)));
                    } else {
                        Saxon.Hej.om.StructuredQName qName = Saxon.Hej.om.StructuredQName.fromEQName((t));
                        list.Add(new Saxon.Hej.pattern.NameTest(Saxon.Hej.type.Type.ELEMENT, new Saxon.Hej.om.FingerprintedQName(qName, pool), pool));
                    }
                }
            }
        }
        Saxon.Hej.pattern.QNameTest test;
        if ((list.Count == 1)) {
            test = list[0];
        } else {
            test = new Saxon.Hej.pattern.UnionQNameTest(list);
        }
        Saxon.Hej.expr.Expression catchExpr = loader.getFirstChildExpression(catchElement);
        tryCatch.addCatchExpression(test, catchExpr);
    }
    return tryCatch;
} );
            Saxon.Impl.Helpers.MapUtils.Put(eMap, "ufCall", (loader, element) => {
    Saxon.Hej.expr.Expression[] args = getChildExpressionArray(loader, element);
    Saxon.Hej.om.StructuredQName name = loader.getQNameAttribute(element, "name");
    Saxon.Hej.expr.UserFunctionCall call = new Saxon.Hej.expr.UserFunctionCall();
    call.setFunctionName(name);
    call.setArguments(args);
    int bindingSlot = loader.getIntegerAttribute(element, "bSlot");
    call.setBindingSlot(bindingSlot);
    string eval = element.getAttributeValue("", "eval");
    if ((eval != null)) {
        string[] evals = System.Text.RegularExpressions.Regex.Split(eval, " ");
        Saxon.Hej.expr.parser.Evaluator[] evalModes = new Saxon.Hej.expr.parser.Evaluator[evals.Length];
        for (int i = 0; (i<evals.Length); i++) {
            evalModes[i] = Saxon.Hej.expr.parser.Evaluators.getEvaluator(Saxon.Impl.Helpers.Utils.ParseInt(evals[i]));
        }
        call.setArgumentEvaluators(evalModes);
    }
    loader.addComponentFixup(call);
    return call;
} );
            Saxon.Impl.Helpers.MapUtils.Put(eMap, "ufRef", (loader, element) => {
    Saxon.Hej.om.StructuredQName name = loader.getQNameAttribute(element, "name");
    int arity = loader.getIntegerAttribute(element, "arity");
    Saxon.Hej.trans.SymbolicName symbolicName = new Saxon.Hej.trans.SymbolicName.F(name, arity);
    Saxon.Hej.functions.hof.UserFunctionReference call = new Saxon.Hej.functions.hof.UserFunctionReference(symbolicName);
    int bindingSlot = loader.getIntegerAttribute(element, "bSlot");
    call.setBindingSlot(bindingSlot);
    loader.addComponentFixup(call);
    return call;
} );
            Saxon.Impl.Helpers.MapUtils.Put(eMap, "union", (loader, element) => {
    Saxon.Hej.expr.Expression lhs = loader.getFirstChildExpression(element);
    Saxon.Hej.expr.Expression rhs = loader.getSecondChildExpression(element);
    return new Saxon.Hej.expr.VennExpression(lhs, Saxon.Hej.expr.parser.Token.UNION, rhs);
} );
            Saxon.Impl.Helpers.MapUtils.Put(eMap, "useAS", (loader, element) => {
    Saxon.Hej.om.StructuredQName name = loader.getQNameAttribute(element, "name");
    bool streamable = "s".Equals(element.getAttributeValue("", "flags"));
    Saxon.Hej.expr.instruct.UseAttributeSet use = new Saxon.Hej.expr.instruct.UseAttributeSet(name, streamable);
    int bindingSlot = loader.getIntegerAttribute(element, "bSlot");
    use.setBindingSlot(bindingSlot);
    loader.addComponentFixup(use);
    return use;
} );
            Saxon.Impl.Helpers.MapUtils.Put(eMap, "valueOf", (loader, element) => {
    Saxon.Hej.expr.Expression select = loader.getFirstChildExpression(element);
    string flags = element.getAttributeValue("", "flags");
    bool doe = ((flags != null) && flags.Contains("d"));
    bool notIfEmpty = ((flags != null) && flags.Contains("e"));
    return new Saxon.Hej.expr.instruct.ValueOf(select, doe, notIfEmpty);
} );
            Saxon.Impl.Helpers.MapUtils.Put(eMap, "varRef", (loader, element) => {
    Saxon.Hej.om.StructuredQName name = loader.getQNameAttribute(element, "name");
    Saxon.Hej.expr.LocalBinding binding = findLocalBinding(loader.localBindings, name);
    if ((binding == null)) {
        throw new Saxon.Hej.trans.XPathException(("No binding found for local variable "+name));
    }
    int slot = loader.getIntegerAttribute(element, "slot");
    Saxon.Hej.expr.LocalVariableReference @ref = new Saxon.Hej.expr.LocalVariableReference(binding);
    @ref.setSlotNumber(slot);
    return @ref;
} );
            Saxon.Impl.Helpers.MapUtils.Put(eMap, "vc", (loader, element) => {
    string opAtt = element.getAttributeValue("", "op");
    int op;
    op = parseValueComparisonOperator(opAtt);
    Saxon.Hej.expr.Expression lhs = loader.getFirstChildExpression(element);
    Saxon.Hej.expr.Expression rhs = loader.getSecondChildExpression(element);
    Saxon.Hej.expr.ValueComparison vc = new Saxon.Hej.expr.ValueComparison(lhs, op, rhs);
    string compAtt = element.getAttributeValue("", "comp");
    Saxon.Hej.expr.sort.AtomicComparer comp = loader.makeAtomicComparer(compAtt, element);
    vc.setAtomicComparer(comp);
    string onEmptyAtt = element.getAttributeValue("", "onEmpty");
    if ((onEmptyAtt != null)) {
        vc.setResultWhenEmpty(Saxon.Hej.value.BooleanValue.get("1".Equals(onEmptyAtt)));
    }
    return vc;
} );
            Saxon.Impl.Helpers.MapUtils.Put(pMap, "p.anchor", (loader, element) => Saxon.Hej.pattern.AnchorPattern.getInstance());
            Saxon.Impl.Helpers.MapUtils.Put(pMap, "p.any", (loader, element) => new Saxon.Hej.pattern.UniversalPattern());
            Saxon.Impl.Helpers.MapUtils.Put(pMap, "p.booleanExp", (loader, element) => {
    Saxon.Hej.expr.Expression exp = loader.getFirstChildExpression(element);
    return new Saxon.Hej.pattern.BooleanExpressionPattern(exp);
} );
            Saxon.Impl.Helpers.MapUtils.Put(pMap, "p.genNode", (loader, element) => {
    Saxon.Hej.pattern.NodeTest type = (Saxon.Hej.pattern.NodeTest)(loader.parseAlphaCodeForItemType(element, "test"));
    Saxon.Hej.expr.Expression exp = loader.getFirstChildExpression(element);
    return new Saxon.Hej.pattern.GeneralNodePattern(exp, type);
} );
            Saxon.Impl.Helpers.MapUtils.Put(pMap, "p.genPos", (loader, element) => {
    Saxon.Hej.pattern.NodeTest type = (Saxon.Hej.pattern.NodeTest)(loader.parseAlphaCodeForItemType(element, "test"));
    Saxon.Hej.expr.Expression exp = loader.getFirstChildExpression(element);
    string flags = element.getAttributeValue("", "flags");
    Saxon.Hej.pattern.GeneralPositionalPattern gpp = new Saxon.Hej.pattern.GeneralPositionalPattern(type, exp);
    gpp.setUsesPosition(!("P".Equals(flags)));
    return gpp;
} );
            Saxon.Impl.Helpers.MapUtils.Put(pMap, "p.nodeSet", (loader, element) => {
    Saxon.Hej.type.ItemType type = loader.parseAlphaCodeForItemType(element, "test");
    Saxon.Hej.expr.Expression select = loader.getFirstChildExpression(element);
    Saxon.Hej.pattern.NodeSetPattern pat = new Saxon.Hej.pattern.NodeSetPattern(select);
    pat.setItemType(type);
    return pat;
} );
            Saxon.Impl.Helpers.MapUtils.Put(pMap, "p.nodeTest", (loader, element) => {
    Saxon.Hej.type.ItemType test = loader.parseAlphaCodeForItemType(element, "test");
    if ((test is Saxon.Hej.pattern.NodeTest)) {
        return new Saxon.Hej.pattern.NodeTestPattern((Saxon.Hej.pattern.NodeTest)(test));
    } else {
        return new Saxon.Hej.pattern.ItemTypePattern(test);
    }
} );
            Saxon.Impl.Helpers.MapUtils.Put(pMap, "p.venn", (loader, element) => {
    Saxon.Hej.pattern.Pattern p0 = loader.getFirstChildPattern(element);
    Saxon.Hej.pattern.Pattern p1 = loader.getSecondChildPattern(element);
    string @operator = element.getAttributeValue("", "op");
    switch (@operator) {
      case "union":
        return new Saxon.Hej.pattern.UnionPattern(p0, p1);
      case "intersect":
        return new Saxon.Hej.pattern.IntersectPattern(p0, p1);
      case "except":
        return new Saxon.Hej.pattern.ExceptPattern(p0, p1);
    }
    return null;
} );
            Saxon.Impl.Helpers.MapUtils.Put(pMap, "p.simPos", (loader, element) => {
    Saxon.Hej.pattern.NodeTest test = (Saxon.Hej.pattern.NodeTest)(loader.parseAlphaCodeForItemType(element, "test"));
    int pos = loader.getIntegerAttribute(element, "pos");
    return new Saxon.Hej.pattern.SimplePositionalPattern(test, pos);
} );
            Saxon.Impl.Helpers.MapUtils.Put(pMap, "p.withCurrent", (loader, element) => {
    Saxon.Hej.expr.flwor.LocalVariableBinding let = new Saxon.Hej.expr.flwor.LocalVariableBinding(Saxon.Hej.functions.Current.FN_CURRENT, Saxon.Hej.value.SequenceType.SINGLE_ITEM);
    let.setSlotNumber(0);
    loader.localBindings.Push(let);
    Saxon.Hej.pattern.Pattern p0 = loader.getFirstChildPattern(element);
    loader.localBindings.Pop();
    return new Saxon.Hej.pattern.PatternThatSetsCurrent(p0, let);
} );
            Saxon.Impl.Helpers.MapUtils.Put(pMap, "p.withUpper", (loader, element) => {
    string axisName = element.getAttributeValue("", "axis");
    int axis = Saxon.Hej.om.AxisInfo.getAxisNumber(axisName);
    Saxon.Hej.pattern.Pattern basePattern = loader.getFirstChildPattern(element);
    Saxon.Hej.pattern.Pattern upperPattern = loader.getSecondChildPattern(element);
    return new Saxon.Hej.pattern.AncestorQualifiedPattern(basePattern, upperPattern, axis);
} );
            Saxon.Impl.Helpers.MapUtils.Put(pMap, "p.withPredicate", (loader, element) => {
    Saxon.Hej.pattern.Pattern basePattern = loader.getFirstChildPattern(element);
    Saxon.Hej.expr.Expression predicate = loader.getSecondChildExpression(element);
    return new Saxon.Hej.pattern.BasePatternWithPredicate(basePattern, predicate);
} );
        }
        private readonly static Saxon.Hej.value.NestedIntegerValue SAXON9911 = new Saxon.Hej.value.NestedIntegerValue(new int[]{9, 9, 1, 1});
        private readonly Saxon.Hej.Configuration config;
        protected internal readonly System.Collections.Generic.Stack<Saxon.Hej.style.StylesheetPackage> packStack = new System.Collections.Generic.Stack<Saxon.Hej.style.StylesheetPackage>();
        private readonly Saxon.Hej.expr.parser.XPathParser parser;
        public readonly System.Collections.Generic.Stack<System.Collections.Generic.IList<Saxon.Hej.expr.ComponentInvocation>> fixups = new System.Collections.Generic.Stack<System.Collections.Generic.IList<Saxon.Hej.expr.ComponentInvocation>>();
        public readonly System.Collections.Generic.IList<Saxon.Hej.om.Action> completionActions = new System.Collections.Generic.List<Saxon.Hej.om.Action>();
        public Saxon.Hej.style.StylesheetPackage topLevelPackage;
        public readonly System.Collections.Generic.IDictionary<string,Saxon.Hej.style.StylesheetPackage> allPackages = new System.Collections.Generic.Dictionary<string, Saxon.Hej.style.StylesheetPackage>();
        public System.Collections.Generic.Stack<Saxon.Hej.expr.LocalBinding> localBindings;
        private readonly Saxon.Hej.functions.ExecutableFunctionLibrary overriding;
        private readonly Saxon.Hej.functions.ExecutableFunctionLibrary underriding;
        private readonly System.Collections.Generic.Stack<Saxon.Hej.expr.parser.RetainedStaticContext> contextStack = new System.Collections.Generic.Stack<Saxon.Hej.expr.parser.RetainedStaticContext>();
        public readonly System.Collections.Generic.IDictionary<Saxon.Hej.trans.SymbolicName,Saxon.Hej.expr.instruct.UserFunction> userFunctions = new System.Collections.Generic.Dictionary<Saxon.Hej.trans.SymbolicName, Saxon.Hej.expr.instruct.UserFunction>();
        private readonly System.Collections.Generic.IDictionary<string,Saxon.Hej.z.IntHashMap<Saxon.Hej.s9api.Location>> locationMap = new System.Collections.Generic.Dictionary<string, Saxon.Hej.z.IntHashMap<Saxon.Hej.s9api.Location>>();
        private readonly System.Collections.Generic.IDictionary<int,Saxon.Hej.expr.Component> componentIdMap = new System.Collections.Generic.Dictionary<int, Saxon.Hej.expr.Component>();
        private readonly System.Collections.Generic.IDictionary<Saxon.Hej.expr.Component,string> externalReferences = new System.Collections.Generic.Dictionary<Saxon.Hej.expr.Component, string>();
        private string relocatableBase = null;
        private Saxon.Hej.value.NestedIntegerValue originalVersion = null;
        public PackageLoaderHE(Saxon.Hej.Configuration config)  {
            this.config = config;
            overriding = new Saxon.Hej.functions.ExecutableFunctionLibrary(config);
            underriding = new Saxon.Hej.functions.ExecutableFunctionLibrary(config);
            try {
                parser = config.newExpressionParser("XP", false, 31);
                Saxon.Hej.om.QNameParser qNameParser = new Saxon.Hej.om.QNameParser(null).withAcceptEQName(true);
                parser.setQNameParser(qNameParser);
            } catch (Saxon.Hej.trans.XPathException e) {
                throw new Saxon.Ejava.lang.AssertionError(e);
            }
        }
        public static void processAccumulatorList(Saxon.Hej.trans.PackageLoaderHE loader, Saxon.Hej.expr.instruct.SourceDocument inst, string accumulatorNames) {
            if ((accumulatorNames != null)) {
                System.Collections.Generic.IList<Saxon.Hej.om.StructuredQName> accNameList = new System.Collections.Generic.List<Saxon.Hej.om.StructuredQName>();
                Saxon.Ejava.util.StringTokenizer tokenizer = new Saxon.Ejava.util.StringTokenizer(accumulatorNames);
                while (tokenizer.hasMoreTokens()) {
                    string token = tokenizer.nextToken();
                    Saxon.Hej.om.StructuredQName name = Saxon.Hej.om.StructuredQName.fromEQName((token));
                    accNameList.Add(name);
                }
                Saxon.Hej.style.StylesheetPackage pack = loader.getPackStack().Peek();
                loader.addCompletionAction(() => {
                    System.Collections.Generic.ISet<Saxon.Hej.expr.accum.Accumulator> list = new System.Collections.Generic.HashSet<Saxon.Hej.expr.accum.Accumulator>();
                    foreach (Saxon.Hej.om.StructuredQName sn in accNameList) {
                        foreach (Saxon.Hej.expr.accum.Accumulator test in pack.getAccumulatorRegistry().getAllAccumulators()) {
                            if (test.getAccumulatorName().Equals(sn)) {
                                list.Add(test);
                            }
                        }
                    }
                    inst.setUsedAccumulators(list);
                } );
            }
        }
        public Saxon.Hej.Configuration getConfiguration() {
            return config;
        }
        public Saxon.Hej.style.StylesheetPackage getTopLevelPackage() {
            return topLevelPackage;
        }
        public Saxon.Hej.style.StylesheetPackage getPackage(string key) {
            return Saxon.Impl.Helpers.MapUtils.Get(allPackages, key);
        }
        public System.Collections.Generic.Stack<Saxon.Hej.style.StylesheetPackage> getPackStack() {
            return packStack;
        }
        public void addCompletionAction(Saxon.Hej.om.Action action) {
            completionActions.Add(action);
        }
        private void addComponentFixup(Saxon.Hej.expr.ComponentInvocation invocation) {
            System.Collections.Generic.IList<Saxon.Hej.expr.ComponentInvocation> currentList = fixups.Peek();
            currentList.Add(invocation);
        }
        public Saxon.Hej.style.StylesheetPackage loadPackage(Saxon.Ejavax.xml.transform.Source source) {
            Saxon.Hej.lib.ParseOptions options = new Saxon.Hej.lib.ParseOptions();
            options.setSpaceStrippingRule(Saxon.Hej.om.AllElementsSpaceStrippingRule.getInstance());
            options.setSchemaValidationMode(Saxon.Hej.lib.Validation.SKIP);
            options.setDTDValidationMode(Saxon.Hej.lib.Validation.SKIP);
            System.Collections.Generic.IList<Saxon.Hej.@event.ProxyReceiver> filters = new System.Collections.Generic.List<Saxon.Hej.@event.ProxyReceiver>(1);
            Saxon.Hej.@event.FilterFactory checksumFactory = next => {
                Saxon.Hej.@event.CheckSumFilter filter = new Saxon.Hej.@event.CheckSumFilter(next);
                filter.setCheckExistingChecksum(true);
                filters.Add(filter);
                return filter;
            } ;
            options.addFilter(checksumFactory);
            Saxon.Hej.om.NodeInfo doc = config.buildDocumentTree(source, options).getRootNode();
            Saxon.Hej.@event.CheckSumFilter csf = (Saxon.Hej.@event.CheckSumFilter)(filters[0]);
            if (!(csf.isChecksumCorrect())) {
                throw new Saxon.Hej.trans.XPathException("Package cannot be loaded: incorrect checksum", Saxon.Hej.trans.SaxonErrorCode.SXPK0002);
            }
            return loadPackageDoc(doc);
        }
        public Saxon.Hej.style.StylesheetPackage loadPackageDoc(Saxon.Hej.om.NodeInfo doc) {
            Saxon.Hej.style.StylesheetPackage pack = config.makeStylesheetPackage();
            pack.setRuleManager(new Saxon.Hej.trans.rules.RuleManager(pack));
            pack.setCharacterMapIndex(new Saxon.Hej.serialize.CharacterMapIndex());
            pack.setJustInTimeCompilation(false);
            if ((packStack.Count==0)) {
                topLevelPackage = pack;
            }
            packStack.Push(pack);
            Saxon.Hej.om.NodeInfo packageElement = ((/*1430*/Saxon.Hej.om.NodeInfo)doc.iterateAxis(Saxon.Hej.om.AxisInfo.CHILD, Saxon.Hej.pattern.NodeKindTest.ELEMENT).next());
            if (!(packageElement.getURI().Equals(Saxon.Hej.lib.NamespaceConstant.SAXON_XSLT_EXPORT))) {
                throw new Saxon.Hej.trans.XPathException("Incorrect namespace for XSLT export file", Saxon.Hej.trans.SaxonErrorCode.SXPK0002);
            }
            if (!(packageElement.getLocalPart().Equals("package"))) {
                throw new Saxon.Hej.trans.XPathException("Outermost element of XSLT export file must be 'package'", Saxon.Hej.trans.SaxonErrorCode.SXPK0002);
            }
            string saxonVersionAtt = packageElement.getAttributeValue("", "saxonVersion");
            if ((saxonVersionAtt == null)) {
                saxonVersionAtt = "9.8.0.1";
            }
            originalVersion = Saxon.Hej.value.NestedIntegerValue.parse(saxonVersionAtt);
            string dmk = packageElement.getAttributeValue("", "dmk");
            if ((dmk != null)) {
                int licenseId = config.registerLocalLicense(dmk);
                pack.setLocalLicenseId(licenseId);
            }
            loadPackageElement(packageElement, pack);
            foreach (System.Collections.Generic.KeyValuePair<Saxon.Hej.expr.Component,string> entry in externalReferences) {
                Saxon.Hej.expr.Component comp = entry.Key;
                Saxon.Ejava.util.StringTokenizer tokenizer = new Saxon.Ejava.util.StringTokenizer(entry.Value);
                while (tokenizer.hasMoreTokens()) {
                    string token = tokenizer.nextToken();
                    int target = Saxon.Impl.Helpers.Utils.ParseInt(token);
                    Saxon.Hej.expr.Component targetComponent = Saxon.Impl.Helpers.MapUtils.Get(componentIdMap, target);
                    if ((targetComponent == null)) {
                        throw new Saxon.Hej.trans.XPathException(("Unresolved external reference to component "+target));
                    }
                    comp.getComponentBindings().Add(new Saxon.Hej.expr.ComponentBinding(targetComponent.getActor().getSymbolicName(), targetComponent));
                }
            }
            return pack;
        }
        public void needsPELicense(string name) {
            int localLicenseId = getTopLevelPackage().getLocalLicenseId();
            config.checkLicensedFeature(Saxon.Hej.Configuration.LicenseFeature.PROFESSIONAL_EDITION, name, localLicenseId);
        }
        public void needsEELicense(string name) {
            int localLicenseId = getTopLevelPackage().getLocalLicenseId();
            config.checkLicensedFeature(Saxon.Hej.Configuration.LicenseFeature.ENTERPRISE_XSLT, name, localLicenseId);
        }
        public void loadPackageElement(Saxon.Hej.om.NodeInfo packageElement, Saxon.Hej.style.StylesheetPackage pack) {
            fixups.Push(new System.Collections.Generic.List<Saxon.Hej.expr.ComponentInvocation>());
            string packageName = packageElement.getAttributeValue("", "name");
            string packageId = packageElement.getAttributeValue("", "id");
            string packageKey = ((packageId == null) ? packageName : packageId);
            bool relocatable = "true".Equals(packageElement.getAttributeValue("", "relocatable"));
            if ((packageName != null)) {
                pack.setPackageName(packageName);
                Saxon.Impl.Helpers.MapUtils.Put(allPackages, packageKey, pack);
            }
            pack.setPackageVersion(new Saxon.Hej.style.PackageVersion(packageElement.getAttributeValue("", "packageVersion")));
            pack.setLanguageVersion(getIntegerAttribute(packageElement, "version"));
            pack.setSchemaAware("1".Equals(packageElement.getAttributeValue("", "schemaAware")));
            if (pack.isSchemaAware()) {
                needsEELicense("schema-awareness");
            }
            string implicitAtt = packageElement.getAttributeValue("", "implicit");
            if ((implicitAtt != null)) {
                pack.setImplicitPackage(implicitAtt.Equals("true"));
            } else {
                pack.setImplicitPackage((originalVersion.compareTo(SAXON9911)<=0));
            }
            pack.setStripsTypeAnnotations("1".Equals(packageElement.getAttributeValue("", "stripType")));
            pack.setKeyManager(new Saxon.Hej.trans.KeyManager(pack.getConfiguration(), pack));
            pack.setDeclaredModes("1".Equals(packageElement.getAttributeValue("", "declaredModes")));
            foreach (Saxon.Hej.om.NodeInfo usePack in packageElement.children(Saxon.Hej.pattern.NodeSelector.of(n => n.getLocalPart().Equals("package")))) {
                Saxon.Hej.style.StylesheetPackage subPack = config.makeStylesheetPackage();
                subPack.setRuleManager(new Saxon.Hej.trans.rules.RuleManager(pack));
                subPack.setCharacterMapIndex(new Saxon.Hej.serialize.CharacterMapIndex());
                subPack.setJustInTimeCompilation(false);
                packStack.Push(subPack);
                loadPackageElement(usePack, subPack);
                packStack.Pop();
                pack.addUsedPackage(subPack);
            }
            Saxon.Hej.functions.FunctionLibraryList functionLibrary = new Saxon.Hej.functions.FunctionLibraryList();
            functionLibrary.addFunctionLibrary(config.getXSLT30FunctionSet());
            functionLibrary.addFunctionLibrary(Saxon.Hej.ma.map.MapFunctionSet.getInstance());
            functionLibrary.addFunctionLibrary(Saxon.Hej.ma.arrays.ArrayFunctionSet.getInstance());
            functionLibrary.addFunctionLibrary(Saxon.Hej.functions.MathFunctionSet.getInstance());
            functionLibrary.addFunctionLibrary(new Saxon.Hej.style.StylesheetFunctionLibrary(pack, true));
            functionLibrary.addFunctionLibrary(new Saxon.Hej.functions.registry.ConstructorFunctionLibrary(config));
            Saxon.Hej.query.XQueryFunctionLibrary queryFunctions = new Saxon.Hej.query.XQueryFunctionLibrary(config);
            functionLibrary.addFunctionLibrary(queryFunctions);
            functionLibrary.addFunctionLibrary(config.getIntegratedFunctionLibrary());
            config.addExtensionBinders(functionLibrary);
            functionLibrary.addFunctionLibrary(new Saxon.Hej.style.StylesheetFunctionLibrary(pack, false));
            pack.setFunctionLibraryDetails(functionLibrary, overriding, underriding);
            Saxon.Hej.expr.parser.RetainedStaticContext rsc = new Saxon.Hej.expr.parser.RetainedStaticContext(config);
            if (relocatable) {
                relocatableBase = packageElement.getBaseURI();
                rsc.setStaticBaseUriString(relocatableBase);
            }
            rsc.setPackageData(pack);
            contextStack.Push(rsc);
            localBindings = new System.Collections.Generic.Stack<Saxon.Hej.expr.LocalBinding>();
            readGlobalContext(packageElement);
            readSchemaNamespaces(packageElement);
            readKeys(packageElement);
            readComponents(packageElement, false);
            Saxon.Hej.om.NodeInfo overridden = ((/*1430*/Saxon.Hej.om.NodeInfo)packageElement.iterateAxis(Saxon.Hej.om.AxisInfo.CHILD, new Saxon.Hej.pattern.NameTest(Saxon.Hej.type.Type.ELEMENT, Saxon.Hej.lib.NamespaceConstant.SAXON_XSLT_EXPORT, "overridden", config.getNamePool())).next());
            if ((overridden != null)) {
                readComponents(overridden, true);
            }
            readAccumulators(packageElement);
            readOutputProperties(packageElement);
            readCharacterMaps(packageElement);
            readSpaceStrippingRules(packageElement);
            readDecimalFormats(packageElement);
            resolveFixups();
            fixups.Pop();
            foreach (Saxon.Hej.om.Action a in completionActions) {
                /*Delegate.doAction()*/a();
            }
            Saxon.Hej.om.StructuredQName defaultModeName = getQNameAttribute(packageElement, "defaultMode");
            if ((defaultModeName == null)) {
                pack.setDefaultMode(Saxon.Hej.trans.Mode.UNNAMED_MODE_NAME);
            } else {
                pack.setDefaultMode(defaultModeName);
            }
        }
        private void readGlobalContext(Saxon.Hej.om.NodeInfo packageElement) {
            Saxon.Hej.expr.instruct.GlobalContextRequirement req = null;
            foreach (Saxon.Hej.om.NodeInfo varElement in packageElement.children(Saxon.Hej.pattern.NodeSelector.of(n => n.getLocalPart().Equals("glob")))) {
                if ((req == null)) {
                    req = new Saxon.Hej.expr.instruct.GlobalContextRequirement();
                    packStack.Peek().setContextItemRequirements(req);
                }
                string use = varElement.getAttributeValue("", "use");
                if ("opt".Equals(use)) {
                    req.setMayBeOmitted(true);
                    req.setAbsentFocus(false);
                } else {
                    if ("pro".Equals(use)) {
                        req.setMayBeOmitted(true);
                        req.setAbsentFocus(true);
                    } else {
                        if ("req".Equals(use)) {
                            req.setMayBeOmitted(false);
                            req.setAbsentFocus(false);
                        }
                    }
                }
                Saxon.Hej.type.ItemType requiredType = parseItemTypeAttribute(varElement, "type");
                if ((requiredType != null)) {
                    req.addRequiredItemType(requiredType);
                }
            }
        }
        protected internal virtual void readSchemaNamespaces(Saxon.Hej.om.NodeInfo packageElement) {
        }
        private void readKeys(Saxon.Hej.om.NodeInfo packageElement) {
            Saxon.Hej.style.StylesheetPackage pack = packStack.Peek();
            Saxon.Hej.om.NodeInfo keyElement;
            Saxon.Hej.tree.iter.AxisIterator iterator = packageElement.iterateAxis(Saxon.Hej.om.AxisInfo.CHILD, new Saxon.Hej.pattern.NameTest(Saxon.Hej.type.Type.ELEMENT, Saxon.Hej.lib.NamespaceConstant.SAXON_XSLT_EXPORT, "key", config.getNamePool()));
            while (((keyElement = ((/*1430*/Saxon.Hej.om.NodeInfo)iterator.next())) != null)) {
                Saxon.Hej.om.StructuredQName keyName = getQNameAttribute(keyElement, "name");
                Saxon.Hej.trans.SymbolicName symbol = new Saxon.Hej.trans.SymbolicName(Saxon.Hej.om.StandardNames.XSL_KEY, keyName);
                string flags = keyElement.getAttributeValue("", "flags");
                bool backwards = ((flags != null) && flags.Contains("b"));
                bool range = ((flags != null) && flags.Contains("r"));
                bool reusable = ((flags != null) && flags.Contains("u"));
                bool composite = ((flags != null) && flags.Contains("c"));
                bool convertUntypedToOther = ((flags != null) && flags.Contains("v"));
                bool strictComparison = ((flags != null) && flags.Contains("s"));
                Saxon.Hej.pattern.Pattern match = getFirstChildPattern(keyElement);
                Saxon.Hej.expr.Expression use = getSecondChildExpression(keyElement);
                string collationName = keyElement.getAttributeValue("", "collation");
                if ((collationName == null)) {
                    collationName = Saxon.Hej.lib.NamespaceConstant.CODEPOINT_COLLATION_URI;
                }
                Saxon.Hej.lib.StringCollator collation = config.getCollation(collationName);
                Saxon.Hej.trans.KeyDefinition keyDefinition = new Saxon.Hej.trans.KeyDefinition(symbol, match, use, collationName, collation);
                int slots = getIntegerAttribute(keyElement, "slots");
                if ((slots != System.Int32.MinValue)) {
                    keyDefinition.setStackFrameMap(new Saxon.Hej.expr.instruct.SlotManager(slots));
                }
                string binds = keyElement.getAttributeValue("", "binds");
                Saxon.Hej.expr.Component keyComponent = keyDefinition.makeDeclaringComponent(Saxon.Hej.trans.Visibility.PRIVATE, pack);
                Saxon.Impl.Helpers.MapUtils.Put(externalReferences, keyComponent, binds);
                if (backwards) {
                    keyDefinition.setBackwardsCompatible(true);
                }
                if (range) {
                    keyDefinition.setRangeKey(true);
                }
                if (composite) {
                    keyDefinition.setComposite(true);
                }
                keyDefinition.setStrictComparison(strictComparison);
                keyDefinition.setConvertUntypedToOther(convertUntypedToOther);
                pack.getKeyManager().addKeyDefinition(keyName, keyDefinition, reusable, pack.getConfiguration());
            }
        }
        private void readComponents(Saxon.Hej.om.NodeInfo packageElement, bool overridden) {
            Saxon.Hej.style.StylesheetPackage pack = packStack.Peek();
            Saxon.Hej.om.NodeInfo child;
            Saxon.Hej.tree.iter.AxisIterator iterator = packageElement.iterateAxis(Saxon.Hej.om.AxisInfo.CHILD, new Saxon.Hej.pattern.NameTest(Saxon.Hej.type.Type.ELEMENT, Saxon.Hej.lib.NamespaceConstant.SAXON_XSLT_EXPORT, "co", config.getNamePool()));
            while (((child = ((/*1430*/Saxon.Hej.om.NodeInfo)iterator.next())) != null)) {
                int id = getIntegerAttribute(child, "id");
                string visAtt = child.getAttributeValue("", "vis");
                Saxon.Hej.trans.Visibility vis = ((visAtt == null) ? Saxon.Hej.trans.Visibility.PRIVATE : (Saxon.Hej.trans.Visibility)Enum.Parse(typeof(Saxon.Hej.trans.Visibility), visAtt.ToUpper()));
                Saxon.Hej.trans.VisibilityProvenance provenance = ((visAtt == null) ? Saxon.Hej.trans.VisibilityProvenance.DEFAULTED : Saxon.Hej.trans.VisibilityProvenance.EXPLICIT);
                string binds = child.getAttributeValue("", "binds");
                string dPackKey = child.getAttributeValue("", "dpack");
                Saxon.Hej.style.StylesheetPackage declaringPackage;
                if ((dPackKey == null)) {
                    declaringPackage = pack;
                } else {
                    if (allPackages.ContainsKey(dPackKey)) {
                        declaringPackage = Saxon.Impl.Helpers.MapUtils.Get(allPackages, dPackKey);
                    } else {
                        declaringPackage = config.makeStylesheetPackage();
                        declaringPackage.setPackageName(dPackKey);
                        declaringPackage.setTargetEdition(config.getEditionCode());
                        declaringPackage.setJustInTimeCompilation(false);
                    }
                }
                Saxon.Hej.expr.Component component;
                int @base = getIntegerAttribute(child, "base");
                if ((@base != System.Int32.MinValue)) {
                    Saxon.Hej.expr.Component baseComponent = Saxon.Impl.Helpers.MapUtils.Get(componentIdMap, @base);
                    if ((baseComponent == null)) {
                        throw new Saxon.Ejava.lang.AssertionError((@base+""));
                    }
                    component = Saxon.Hej.expr.Component.makeComponent(baseComponent.getActor(), vis, provenance, pack, declaringPackage);
                    component.setBaseComponent(baseComponent);
                    if ((component is Saxon.Hej.expr.Component.M)) {
                        pack.getRuleManager().obtainMode(baseComponent.getActor().getComponentName(), true);
                    }
                } else {
                    Saxon.Hej.om.NodeInfo grandchild = ((/*1430*/Saxon.Hej.om.NodeInfo)child.iterateAxis(Saxon.Hej.om.AxisInfo.CHILD, Saxon.Hej.pattern.NodeKindTest.ELEMENT).next());
                    Saxon.Hej.expr.instruct.Actor cc;
                    string kind = grandchild.getLocalPart();
                    bool codeGen = false;
                    switch (kind) {
                      case "template":
                        cc = readNamedTemplate(grandchild);
                        codeGen = true;
                        break ;
                      case "globalVariable":
                        cc = readGlobalVariable(grandchild);
                        codeGen = true;
                        break ;
                      case "globalParam":
                        cc = readGlobalParam(grandchild);
                        break ;
                      case "function":
                        cc = readGlobalFunction(grandchild);
                        codeGen = (((Saxon.Hej.expr.instruct.UserFunction)(cc)).getDeclaredStreamability() == Saxon.Hej.trans.FunctionStreamability.UNCLASSIFIED);
                        break ;
                      case "mode":
                        cc = readMode(grandchild);
                        break ;
                      case "attributeSet":
                        cc = readAttributeSet(grandchild);
                        break ;
                      default:
                        throw new Saxon.Hej.trans.XPathException(("unknown component kind "+kind));
                    }
                    component = Saxon.Hej.expr.Component.makeComponent(cc, vis, provenance, pack, declaringPackage);
                    cc.setDeclaringComponent(component);
                    cc.setDeclaredVisibility(vis);
                }
                Saxon.Impl.Helpers.MapUtils.Put(externalReferences, component, binds);
                Saxon.Impl.Helpers.MapUtils.Put(componentIdMap, id, component);
                if (overridden) {
                    pack.addOverriddenComponent(component);
                } else {
                    if ((component.getVisibility() == Saxon.Hej.trans.Visibility.HIDDEN)) {
                        pack.addHiddenComponent(component);
                    } else {
                        pack.addComponent(component);
                    }
                }
            }
        }
        private Saxon.Hej.expr.instruct.GlobalVariable readGlobalVariable(Saxon.Hej.om.NodeInfo varElement) {
            Saxon.Hej.style.StylesheetPackage pack = packStack.Peek();
            Saxon.Hej.om.StructuredQName variableName = getQNameAttribute(varElement, "name");
            Saxon.Hej.expr.instruct.GlobalVariable var = new Saxon.Hej.expr.instruct.GlobalVariable();
            var.setVariableQName(variableName);
            var.setPackageData(pack);
            var.setRequiredType(parseAlphaCode(varElement, "as"));
            string flags = varElement.getAttributeValue("", "flags");
            if ((flags != null)) {
                if (flags.Contains("a")) {
                    var.setAssignable(true);
                }
                if (flags.Contains("x")) {
                    var.setIndexedVariable();
                }
                if (flags.Contains("r")) {
                    var.setRequiredParam(true);
                }
            }
            int slots = getIntegerAttribute(varElement, "slots");
            if ((slots>0)) {
                var.setContainsLocals(new Saxon.Hej.expr.instruct.SlotManager(slots));
            }
            Saxon.Hej.om.NodeInfo bodyElement = ((/*1430*/Saxon.Hej.om.NodeInfo)varElement.iterateAxis(Saxon.Hej.om.AxisInfo.CHILD, Saxon.Hej.pattern.NodeKindTest.ELEMENT).next());
            if ((bodyElement == null)) {
                var.setBody(Saxon.Hej.expr.Literal.makeEmptySequence());
            } else {
                Saxon.Hej.expr.Expression body = loadExpression(bodyElement);
                var.setBody(body);
                Saxon.Hej.expr.parser.RetainedStaticContext rsc = body.getRetainedStaticContext();
                body.setRetainedStaticContext(rsc);
            }
            pack.addGlobalVariable(var);
            return var;
        }
        private Saxon.Hej.expr.instruct.GlobalParam readGlobalParam(Saxon.Hej.om.NodeInfo varElement) {
            Saxon.Hej.style.StylesheetPackage pack = packStack.Peek();
            Saxon.Hej.om.StructuredQName variableName = getQNameAttribute(varElement, "name");
            localBindings = new System.Collections.Generic.Stack<Saxon.Hej.expr.LocalBinding>();
            Saxon.Hej.expr.instruct.GlobalParam var = new Saxon.Hej.expr.instruct.GlobalParam();
            var.setVariableQName(variableName);
            var.setPackageData(pack);
            var.setRequiredType(parseAlphaCode(varElement, "as"));
            string flags = varElement.getAttributeValue("", "flags");
            if ((flags != null)) {
                if (flags.Contains("a")) {
                    var.setAssignable(true);
                }
                if (flags.Contains("x")) {
                    var.setIndexedVariable();
                }
                if (flags.Contains("r")) {
                    var.setRequiredParam(true);
                }
                if (flags.Contains("i")) {
                    var.setImplicitlyRequiredParam(true);
                }
            }
            int slots = getIntegerAttribute(varElement, "slots");
            if ((slots>0)) {
                var.setContainsLocals(new Saxon.Hej.expr.instruct.SlotManager(slots));
            }
            Saxon.Hej.om.NodeInfo bodyElement = ((/*1430*/Saxon.Hej.om.NodeInfo)varElement.iterateAxis(Saxon.Hej.om.AxisInfo.CHILD, Saxon.Hej.pattern.NodeKindTest.ELEMENT).next());
            if ((bodyElement == null)) {
                var.setBody(Saxon.Hej.expr.Literal.makeEmptySequence());
            } else {
                Saxon.Hej.expr.Expression body = loadExpression(bodyElement);
                var.setBody(body);
                Saxon.Hej.expr.parser.RetainedStaticContext rsc = body.getRetainedStaticContext();
                body.setRetainedStaticContext(rsc);
            }
            return var;
        }
        private Saxon.Hej.expr.instruct.NamedTemplate readNamedTemplate(Saxon.Hej.om.NodeInfo templateElement) {
            Saxon.Hej.style.StylesheetPackage pack = packStack.Peek();
            localBindings = new System.Collections.Generic.Stack<Saxon.Hej.expr.LocalBinding>();
            Saxon.Hej.om.StructuredQName templateName = getQNameAttribute(templateElement, "name");
            string flags = templateElement.getAttributeValue("", "flags");
            int slots = getIntegerAttribute(templateElement, "slots");
            Saxon.Hej.value.SequenceType contextType = parseAlphaCode(templateElement, "cxt");
            Saxon.Hej.type.ItemType contextItemType = ((contextType == null) ? Saxon.Hej.type.AnyItemType.getInstance() : contextType.getPrimaryType());
            Saxon.Hej.expr.instruct.NamedTemplate template = new Saxon.Hej.expr.instruct.NamedTemplate(templateName);
            template.setStackFrameMap(new Saxon.Hej.expr.instruct.SlotManager(slots));
            template.setPackageData(pack);
            template.setRequiredType(parseAlphaCode(templateElement, "as"));
            template.setContextItemRequirements(contextItemType, flags.Contains("o"), !(flags.Contains("s")));
            Saxon.Hej.om.NodeInfo bodyElement = getChildWithRole(templateElement, "body");
            if ((bodyElement == null)) {
                template.setBody(Saxon.Hej.expr.Literal.makeEmptySequence());
            } else {
                Saxon.Hej.expr.Expression body = loadExpression(bodyElement);
                template.setBody(body);
                Saxon.Hej.expr.parser.RetainedStaticContext rsc = body.getRetainedStaticContext();
                body.setRetainedStaticContext(rsc);
            }
            return template;
        }
        private Saxon.Hej.expr.instruct.UserFunction readGlobalFunction(Saxon.Hej.om.NodeInfo functionElement) {
            localBindings = new System.Collections.Generic.Stack<Saxon.Hej.expr.LocalBinding>();
            Saxon.Hej.expr.instruct.UserFunction function = readFunction(functionElement);
            Saxon.Impl.Helpers.MapUtils.Put(userFunctions, function.getSymbolicName(), function);
            underriding.addFunction(function);
            return function;
        }
        private Saxon.Hej.expr.instruct.UserFunction getUserFunction(Saxon.Hej.trans.SymbolicName.F name) {
            return Saxon.Impl.Helpers.MapUtils.Get(userFunctions, name);
        }
        private Saxon.Hej.expr.instruct.UserFunction currentFunction;
        public Saxon.Hej.expr.instruct.UserFunction readFunction(Saxon.Hej.om.NodeInfo functionElement) {
            Saxon.Hej.style.StylesheetPackage pack = packStack.Peek();
            Saxon.Hej.om.StructuredQName functionName = getQNameAttribute(functionElement, "name");
            int slots = getIntegerAttribute(functionElement, "slots");
            string flags = functionElement.getAttributeValue("", "flags");
            if ((flags == null)) {
                flags = "";
            }
            Saxon.Hej.expr.instruct.UserFunction function = makeFunction(flags);
            function.setFunctionName(functionName);
            function.setStackFrameMap(new Saxon.Hej.expr.instruct.SlotManager(slots));
            function.setPackageData(pack);
            function.setRetainedStaticContext(makeRetainedStaticContext(functionElement));
            function.setResultType(parseAlphaCode(functionElement, "as"));
            function.setDeclaredStreamability(Saxon.Hej.trans.FunctionStreamability.UNCLASSIFIED);
            function.incrementReferenceCount();
            int evalMode = getIntegerAttribute(functionElement, "eval");
            if (flags.Contains("p")) {
                function.setDeterminism(Saxon.Hej.expr.instruct.UserFunction.Determinism.PROACTIVE);
            } else {
                if (flags.Contains("e")) {
                    function.setDeterminism(Saxon.Hej.expr.instruct.UserFunction.Determinism.ELIDABLE);
                } else {
                    if (flags.Contains("d")) {
                        function.setDeterminism(Saxon.Hej.expr.instruct.UserFunction.Determinism.DETERMINISTIC);
                    }
                }
            }
            bool streaming = false;
            if (flags.Contains("U")) {
                function.setDeclaredStreamability(Saxon.Hej.trans.FunctionStreamability.UNCLASSIFIED);
            } else {
                if (flags.Contains("A")) {
                    function.setDeclaredStreamability(Saxon.Hej.trans.FunctionStreamability.ABSORBING);
                    streaming = true;
                } else {
                    if (flags.Contains("I")) {
                        function.setDeclaredStreamability(Saxon.Hej.trans.FunctionStreamability.INSPECTION);
                        streaming = true;
                    } else {
                        if (flags.Contains("F")) {
                            function.setDeclaredStreamability(Saxon.Hej.trans.FunctionStreamability.FILTER);
                            streaming = true;
                        } else {
                            if (flags.Contains("S")) {
                                function.setDeclaredStreamability(Saxon.Hej.trans.FunctionStreamability.SHALLOW_DESCENT);
                                streaming = true;
                            } else {
                                if (flags.Contains("D")) {
                                    function.setDeclaredStreamability(Saxon.Hej.trans.FunctionStreamability.DEEP_DESCENT);
                                    streaming = true;
                                } else {
                                    if (flags.Contains("C")) {
                                        function.setDeclaredStreamability(Saxon.Hej.trans.FunctionStreamability.ASCENT);
                                        streaming = true;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            function.setEvaluator(Saxon.Hej.expr.parser.Evaluators.getEvaluator(evalMode));
            currentFunction = function;
            System.Collections.Generic.IList<Saxon.Hej.expr.instruct.UserFunctionParameter> @params = new System.Collections.Generic.List<Saxon.Hej.expr.instruct.UserFunctionParameter>();
            Saxon.Hej.tree.iter.AxisIterator argIterator = functionElement.iterateAxis(Saxon.Hej.om.AxisInfo.CHILD, new Saxon.Hej.pattern.NameTest(Saxon.Hej.type.Type.ELEMENT, Saxon.Hej.lib.NamespaceConstant.SAXON_XSLT_EXPORT, "arg", config.getNamePool()));
            Saxon.Hej.om.NodeInfo argElement;
            int slot = 0;
            while (((argElement = ((/*1430*/Saxon.Hej.om.NodeInfo)argIterator.next())) != null)) {
                Saxon.Hej.expr.instruct.UserFunctionParameter arg = new Saxon.Hej.expr.instruct.UserFunctionParameter();
                arg.setVariableQName(getQNameAttribute(argElement, "name"));
                arg.setRequiredType(parseAlphaCode(argElement, "as"));
                arg.setSlotNumber(slot++);
                @params.Add(arg);
                localBindings.Push(arg);
            }
            function.setParameterDefinitions(@params.ToArray());
            if (streaming) {
                @params[0].setFunctionStreamability(function.getDeclaredStreamability());
            }
            Saxon.Hej.om.NodeInfo bodyElement = getChildWithRole(functionElement, "body");
            if ((bodyElement == null)) {
                function.setBody(Saxon.Hej.expr.Literal.makeEmptySequence());
            } else {
                Saxon.Hej.expr.Expression body = loadExpression(bodyElement);
                function.setBody(body);
                Saxon.Hej.expr.parser.RetainedStaticContext rsc = body.getRetainedStaticContext();
                body.setRetainedStaticContext(rsc);
            }
            for (int i = 0; (i<function.getArity()); i++) {
                localBindings.Pop();
            }
            if ((function.getDeclaredStreamability() != Saxon.Hej.trans.FunctionStreamability.UNCLASSIFIED)) {
                addCompletionAction(() => function.prepareForStreaming());
            }
            return function;
        }
        protected internal virtual Saxon.Hej.expr.instruct.UserFunction makeFunction(string flags) {
            return new Saxon.Hej.expr.instruct.UserFunction();
        }
        private Saxon.Hej.expr.instruct.AttributeSet readAttributeSet(Saxon.Hej.om.NodeInfo aSetElement) {
            Saxon.Hej.style.StylesheetPackage pack = packStack.Peek();
            localBindings = new System.Collections.Generic.Stack<Saxon.Hej.expr.LocalBinding>();
            Saxon.Hej.om.StructuredQName aSetName = getQNameAttribute(aSetElement, "name");
            int slots = getIntegerAttribute(aSetElement, "slots");
            Saxon.Hej.expr.instruct.AttributeSet aSet = new Saxon.Hej.expr.instruct.AttributeSet();
            aSet.setName(aSetName);
            aSet.setStackFrameMap(new Saxon.Hej.expr.instruct.SlotManager(slots));
            aSet.setPackageData(pack);
            aSet.setBody(getFirstChildExpression(aSetElement));
            aSet.setDeclaredStreamable("s".Equals(aSetElement.getAttributeValue("", "flags")));
            return aSet;
        }
        private Saxon.Hej.trans.Mode readMode(Saxon.Hej.om.NodeInfo modeElement) {
            Saxon.Hej.style.StylesheetPackage pack = packStack.Peek();
            Saxon.Hej.om.StructuredQName modeName = getQNameAttribute(modeElement, "name");
            if ((modeName == null)) {
                modeName = Saxon.Hej.trans.Mode.UNNAMED_MODE_NAME;
            }
            Saxon.Hej.trans.SimpleMode mode = (Saxon.Hej.trans.SimpleMode)(pack.getRuleManager().obtainMode(modeName, true));
            int patternSlots = getIntegerAttribute(modeElement, "patternSlots");
            mode.allocatePatternSlots(patternSlots);
            string onNoMatch = modeElement.getAttributeValue("", "onNo");
            Saxon.Hej.trans.rules.BuiltInRuleSet @base;
            if ((onNoMatch != null)) {
                @base = mode.getBuiltInRuleSetForCode(onNoMatch);
                mode.setBuiltInRuleSet(@base);
            }
            string flags = modeElement.getAttributeValue("", "flags");
            if ((flags != null)) {
                mode.setStreamable(flags.Contains("s"));
                if (flags.Contains("t")) {
                    mode.setExplicitProperty("typed", "yes", 1);
                }
                if (flags.Contains("u")) {
                    mode.setExplicitProperty("typed", "no", 1);
                }
                if (flags.Contains("F")) {
                    mode.setRecoveryPolicy(Saxon.Hej.trans.RecoveryPolicy.DO_NOT_RECOVER);
                }
                if (flags.Contains("W")) {
                    mode.setRecoveryPolicy(Saxon.Hej.trans.RecoveryPolicy.RECOVER_WITH_WARNINGS);
                }
                if (flags.Contains("e")) {
                    mode.setHasRules(false);
                }
            }
            System.Collections.Generic.IList<Saxon.Hej.om.StructuredQName> accNames = getListOfQNameAttribute(modeElement, "useAcc");
            addCompletionAction(() => {
                Saxon.Hej.expr.accum.AccumulatorRegistry registry = pack.getAccumulatorRegistry();
                System.Collections.Generic.ISet<Saxon.Hej.expr.accum.Accumulator> accumulators = new System.Collections.Generic.HashSet<Saxon.Hej.expr.accum.Accumulator>();
                foreach (Saxon.Hej.om.StructuredQName qn in accNames) {
                    Saxon.Hej.expr.accum.Accumulator acc = registry.getAccumulator(qn);
                    accumulators.Add(acc);
                }
                mode.setAccumulators(accumulators);
            } );
            Saxon.Hej.tree.iter.AxisIterator iterator2 = modeElement.iterateAxis(Saxon.Hej.om.AxisInfo.DESCENDANT, new Saxon.Hej.pattern.NameTest(Saxon.Hej.type.Type.ELEMENT, Saxon.Hej.lib.NamespaceConstant.SAXON_XSLT_EXPORT, "templateRule", config.getNamePool()));
            Saxon.Hej.om.NodeInfo templateRuleElement0;
            System.Collections.Generic.LinkedList<Saxon.Hej.om.NodeInfo> ruleStack = new System.Collections.Generic.LinkedList<Saxon.Hej.om.NodeInfo>();
            while (((templateRuleElement0 = ((/*1430*/Saxon.Hej.om.NodeInfo)iterator2.next())) != null)) {
                ruleStack.AddFirst(templateRuleElement0);
            }
            foreach (Saxon.Hej.om.NodeInfo templateRuleElement in ruleStack) {
                int precedence = getIntegerAttribute(templateRuleElement, "prec");
                int rank = getIntegerAttribute(templateRuleElement, "rank");
                string priorityAtt = templateRuleElement.getAttributeValue("", "prio");
                double priority = System.Double.Parse(priorityAtt);
                int sequence = getIntegerAttribute(templateRuleElement, "seq");
                int part = getIntegerAttribute(templateRuleElement, "part");
                if ((part == System.Int32.MinValue)) {
                    part = 0;
                }
                int minImportPrecedence = getIntegerAttribute(templateRuleElement, "minImp");
                int slots = getIntegerAttribute(templateRuleElement, "slots");
                bool streamable = "1".Equals(templateRuleElement.getAttributeValue("", "streamable"));
                string tflags = templateRuleElement.getAttributeValue("", "flags");
                Saxon.Hej.value.SequenceType contextType = parseAlphaCode(templateRuleElement, "cxt");
                Saxon.Hej.type.ItemType contextItemType = ((contextType == null) ? Saxon.Hej.type.AnyItemType.getInstance() : contextType.getPrimaryType());
                Saxon.Hej.om.NodeInfo matchElement = getChildWithRole(templateRuleElement, "match");
                Saxon.Hej.pattern.Pattern match = loadPattern(matchElement);
                localBindings = new System.Collections.Generic.Stack<Saxon.Hej.expr.LocalBinding>();
                Saxon.Hej.expr.instruct.TemplateRule template = config.makeTemplateRule();
                template.setMatchPattern(match);
                template.setStackFrameMap(new Saxon.Hej.expr.instruct.SlotManager(slots));
                template.setPackageData(pack);
                template.setRequiredType(parseAlphaCode(templateRuleElement, "as"));
                template.setDeclaredStreamable(streamable);
                template.setContextItemRequirements(contextItemType, !(tflags.Contains("s")));
                Saxon.Hej.om.NodeInfo bodyElement = getChildWithRole(templateRuleElement, "action");
                if ((bodyElement == null)) {
                    template.setBody(Saxon.Hej.expr.Literal.makeEmptySequence());
                } else {
                    Saxon.Hej.expr.Expression body = loadExpression(bodyElement);
                    template.setBody(body);
                    Saxon.Hej.expr.parser.RetainedStaticContext rsc = body.getRetainedStaticContext();
                    body.setRetainedStaticContext(rsc);
                }
                Saxon.Hej.trans.rules.Rule rule = mode.makeRule(match, template, precedence, minImportPrecedence, priority, sequence, part);
                rule.setRank(rank);
                mode.addRule(match, rule);
                mode.setHasRules(true);
            }
            addCompletionAction(() => mode.prepareStreamability());
            return mode;
        }
        private void readAccumulators(Saxon.Hej.om.NodeInfo packageElement) {
            Saxon.Hej.style.StylesheetPackage pack = packStack.Peek();
            Saxon.Hej.om.NodeInfo accElement;
            Saxon.Hej.tree.iter.AxisIterator iterator = packageElement.iterateAxis(Saxon.Hej.om.AxisInfo.CHILD, new Saxon.Hej.pattern.NameTest(Saxon.Hej.type.Type.ELEMENT, Saxon.Hej.lib.NamespaceConstant.SAXON_XSLT_EXPORT, "accumulator", config.getNamePool()));
            while (((accElement = ((/*1430*/Saxon.Hej.om.NodeInfo)iterator.next())) != null)) {
                Saxon.Hej.om.StructuredQName accName = getQNameAttribute(accElement, "name");
                Saxon.Hej.expr.accum.Accumulator acc = new Saxon.Hej.expr.accum.Accumulator();
                Saxon.Hej.expr.Component component = Saxon.Hej.expr.Component.makeComponent(acc, Saxon.Hej.trans.Visibility.PRIVATE, Saxon.Hej.trans.VisibilityProvenance.DEFAULTED, pack, pack);
                acc.setDeclaringComponent(component);
                int iniSlots = getIntegerAttribute(accElement, "slots");
                acc.setSlotManagerForInitialValueExpression(new Saxon.Hej.expr.instruct.SlotManager(iniSlots));
                acc.setAccumulatorName(accName);
                string binds = accElement.getAttributeValue("", "binds");
                Saxon.Impl.Helpers.MapUtils.Put(externalReferences, component, binds);
                bool streamable = "1".Equals(accElement.getAttributeValue("", "streamable"));
                string flags = accElement.getAttributeValue("", "flags");
                bool universal = ((flags != null) && flags.Contains("u"));
                acc.setDeclaredStreamable(streamable);
                acc.setUniversallyApplicable(universal);
                Saxon.Hej.expr.Expression init = getExpressionWithRole(accElement, "init");
                acc.setInitialValueExpression(init);
                Saxon.Hej.om.NodeInfo pre = getChild(accElement, 1);
                readAccumulatorRules(acc, pre);
                Saxon.Hej.om.NodeInfo post = getChild(accElement, 2);
                readAccumulatorRules(acc, post);
                pack.getAccumulatorRegistry().addAccumulator(acc);
            }
        }
        private void readAccumulatorRules(Saxon.Hej.expr.accum.Accumulator acc, Saxon.Hej.om.NodeInfo owner) {
            Saxon.Hej.tree.iter.AxisIterator iterator = owner.iterateAxis(Saxon.Hej.om.AxisInfo.CHILD, new Saxon.Hej.pattern.NameTest(Saxon.Hej.type.Type.ELEMENT, Saxon.Hej.lib.NamespaceConstant.SAXON_XSLT_EXPORT, "accRule", config.getNamePool()));
            Saxon.Hej.om.NodeInfo accRuleElement;
            bool preDescent = owner.getLocalPart().Equals("pre");
            Saxon.Hej.trans.SimpleMode mode = (preDescent ? acc.getPreDescentRules() : acc.getPostDescentRules());
            int patternSlots = getIntegerAttribute(owner, "slots");
            mode.setStackFrameSlotsNeeded(patternSlots);
            while (((accRuleElement = ((/*1430*/Saxon.Hej.om.NodeInfo)iterator.next())) != null)) {
                int slots = getIntegerAttribute(accRuleElement, "slots");
                int rank = getIntegerAttribute(accRuleElement, "rank");
                string flags = accRuleElement.getAttributeValue("", "flags");
                Saxon.Hej.expr.instruct.SlotManager sm = new Saxon.Hej.expr.instruct.SlotManager(slots);
                Saxon.Hej.pattern.Pattern pattern = getFirstChildPattern(accRuleElement);
                Saxon.Hej.expr.Expression select = getSecondChildExpression(accRuleElement);
                Saxon.Hej.expr.accum.AccumulatorRule rule = new Saxon.Hej.expr.accum.AccumulatorRule(select, sm, !(preDescent));
                if (((flags != null) && flags.Contains("c"))) {
                    rule.setCapturing(true);
                }
                mode.addRule(pattern, mode.makeRule(pattern, rule, rank, 0, rank, 0, 0));
            }
            mode.computeRankings(1);
        }
        private void readOutputProperties(Saxon.Hej.om.NodeInfo packageElement) {
            Saxon.Hej.style.StylesheetPackage pack = packStack.Peek();
            Saxon.Hej.om.NodeInfo outputElement;
            Saxon.Hej.tree.iter.AxisIterator iterator = packageElement.iterateAxis(Saxon.Hej.om.AxisInfo.CHILD, new Saxon.Hej.pattern.NameTest(Saxon.Hej.type.Type.ELEMENT, Saxon.Hej.lib.NamespaceConstant.SAXON_XSLT_EXPORT, "output", config.getNamePool()));
            while (((outputElement = ((/*1430*/Saxon.Hej.om.NodeInfo)iterator.next())) != null)) {
                Saxon.Hej.om.StructuredQName outputName = getQNameAttribute(outputElement, "name");
                Saxon.Ejava.util.Properties props = new Saxon.Ejava.util.Properties();
                Saxon.Hej.om.NodeInfo propertyElement;
                Saxon.Hej.tree.iter.AxisIterator iterator1 = outputElement.iterateAxis(Saxon.Hej.om.AxisInfo.CHILD, new Saxon.Hej.pattern.NameTest(Saxon.Hej.type.Type.ELEMENT, Saxon.Hej.lib.NamespaceConstant.SAXON_XSLT_EXPORT, "property", config.getNamePool()));
                while (((propertyElement = ((/*1430*/Saxon.Hej.om.NodeInfo)iterator1.next())) != null)) {
                    string name = propertyElement.getAttributeValue("", "name");
                    if (name.StartsWith("Q{")) {
                        name = name.Substring(1);
                    }
                    string value = propertyElement.getAttributeValue("", "value");
                    if ((name.StartsWith("{http://saxon.sf.net/}") && !(name.Equals(Saxon.Hej.lib.SaxonOutputKeys.STYLESHEET_VERSION)))) {
                        needsPELicense("Saxon output properties");
                    }
                    props.setProperty(name, value);
                }
                if ((outputName == null)) {
                    pack.setDefaultOutputProperties(props);
                } else {
                    pack.setNamedOutputProperties(outputName, props);
                }
            }
        }
        private void readCharacterMaps(Saxon.Hej.om.NodeInfo packageElement) {
            Saxon.Hej.style.StylesheetPackage pack = packStack.Peek();
            Saxon.Hej.om.NodeInfo charMapElement;
            Saxon.Hej.tree.iter.AxisIterator iterator = packageElement.iterateAxis(Saxon.Hej.om.AxisInfo.CHILD, new Saxon.Hej.pattern.NameTest(Saxon.Hej.type.Type.ELEMENT, Saxon.Hej.lib.NamespaceConstant.SAXON_XSLT_EXPORT, "charMap", config.getNamePool()));
            while (((charMapElement = ((/*1430*/Saxon.Hej.om.NodeInfo)iterator.next())) != null)) {
                Saxon.Hej.om.StructuredQName mapName = getQNameAttribute(charMapElement, "name");
                Saxon.Hej.om.NodeInfo mappingElement;
                Saxon.Hej.tree.iter.AxisIterator iterator1 = charMapElement.iterateAxis(Saxon.Hej.om.AxisInfo.CHILD, new Saxon.Hej.pattern.NameTest(Saxon.Hej.type.Type.ELEMENT, Saxon.Hej.lib.NamespaceConstant.SAXON_XSLT_EXPORT, "m", config.getNamePool()));
                Saxon.Hej.z.IntHashMap<string> map = new Saxon.Hej.z.IntHashMap<string>();
                while (((mappingElement = ((/*1430*/Saxon.Hej.om.NodeInfo)iterator1.next())) != null)) {
                    int c = getIntegerAttribute(mappingElement, "c");
                    string s = mappingElement.getAttributeValue("", "s");
                    map.put(c, s);
                }
                Saxon.Hej.serialize.CharacterMap characterMap = new Saxon.Hej.serialize.CharacterMap(mapName, map);
                pack.getCharacterMapIndex().putCharacterMap(mapName, characterMap);
            }
        }
        private void readSpaceStrippingRules(Saxon.Hej.om.NodeInfo packageElement) {
            Saxon.Hej.style.StylesheetPackage pack = packStack.Peek();
            Saxon.Hej.om.NodeInfo element;
            Saxon.Hej.tree.iter.AxisIterator iterator = packageElement.iterateAxis(Saxon.Hej.om.AxisInfo.CHILD, Saxon.Hej.pattern.NodeKindTest.ELEMENT);
            while (((element = ((/*1430*/Saxon.Hej.om.NodeInfo)iterator.next())) != null)) {
                string s = element.getLocalPart();
                switch (s) {
                  case "strip.all":
                    pack.setStripperRules(new Saxon.Hej.om.AllElementsSpaceStrippingRule());
                    pack.setStripsWhitespace(true);
                    break ;
                  case "strip.none":
                    pack.setStripperRules(new Saxon.Hej.om.NoElementsSpaceStrippingRule());
                    break ;
                  case "strip":
                    Saxon.Hej.tree.iter.AxisIterator iterator2 = element.iterateAxis(Saxon.Hej.om.AxisInfo.CHILD, Saxon.Hej.pattern.NodeKindTest.ELEMENT);
                    Saxon.Hej.om.NodeInfo element2;
                    Saxon.Hej.om.SelectedElementsSpaceStrippingRule rules = new Saxon.Hej.om.SelectedElementsSpaceStrippingRule(false);
                    while (((element2 = ((/*1430*/Saxon.Hej.om.NodeInfo)iterator2.next())) != null)) {
                        Saxon.Hej.@event.Stripper.StripRuleTarget which = (element2.getLocalPart().Equals("s") ? Saxon.Hej.@event.Stripper.STRIP : Saxon.Hej.@event.Stripper.PRESERVE);
                        string value = element2.getAttributeValue("", "test");
                        Saxon.Hej.pattern.NodeTest t;
                        if (value.Equals("*")) {
                            t = Saxon.Hej.pattern.NodeKindTest.ELEMENT;
                        } else {
                            t = (Saxon.Hej.pattern.NodeTest)(parseAlphaCodeForItemType(element2, "test"));
                        }
                        int prec = getIntegerAttribute(element2, "prec");
                        Saxon.Hej.pattern.NodeTestPattern pat = new Saxon.Hej.pattern.NodeTestPattern(t);
                        rules.addRule(pat, which, prec, prec);
                    }
                    pack.setStripperRules(rules);
                    pack.setStripsWhitespace(true);
                    break ;
                }
            }
        }
        private void readDecimalFormats(Saxon.Hej.om.NodeInfo packageElement) {
            Saxon.Hej.om.NodeInfo formatElement;
            Saxon.Hej.trans.DecimalFormatManager decimalFormatManager = packStack.Peek().getDecimalFormatManager();
            Saxon.Hej.tree.iter.AxisIterator iterator = packageElement.iterateAxis(Saxon.Hej.om.AxisInfo.CHILD, new Saxon.Hej.pattern.NameTest(Saxon.Hej.type.Type.ELEMENT, Saxon.Hej.lib.NamespaceConstant.SAXON_XSLT_EXPORT, "decimalFormat", config.getNamePool()));
            string[] propertyNames = Saxon.Hej.trans.DecimalSymbols.propertyNames;
            while (((formatElement = ((/*1430*/Saxon.Hej.om.NodeInfo)iterator.next())) != null)) {
                Saxon.Hej.om.StructuredQName name = getQNameAttribute(formatElement, "name");
                Saxon.Hej.trans.DecimalSymbols symbols;
                if ((name == null)) {
                    symbols = decimalFormatManager.getDefaultDecimalFormat();
                } else {
                    symbols = decimalFormatManager.obtainNamedDecimalFormat(name);
                }
                symbols.setHostLanguage(Saxon.Api.HostLanguage.XSLT, 31);
                foreach (string p in propertyNames) {
                    if ((formatElement.getAttributeValue("", p) != null)) {
                        switch (p) {
                          case "NaN":
                            symbols.setNaN(formatElement.getAttributeValue("", "NaN"));
                            break ;
                          case "infinity":
                            symbols.setInfinity(formatElement.getAttributeValue("", "infinity"));
                            break ;
                          case "name":
                            break ;
                          default:
                            symbols.setIntProperty(p, getIntegerAttribute(formatElement, p));
                            break ;
                        }
                    }
                }
            }
        }
        public Saxon.Hej.om.NodeInfo getChild(Saxon.Hej.om.NodeInfo parent, int n) {
            Saxon.Hej.tree.iter.AxisIterator iter = parent.iterateAxis(Saxon.Hej.om.AxisInfo.CHILD, Saxon.Hej.pattern.NodeKindTest.ELEMENT);
            Saxon.Hej.om.NodeInfo node = ((/*1430*/Saxon.Hej.om.NodeInfo)iter.next());
            for (int i = 0; (i<n); i++) {
                node = ((/*1430*/Saxon.Hej.om.NodeInfo)iter.next());
            }
            return node;
        }
        public Saxon.Hej.om.NodeInfo getChildWithRole(Saxon.Hej.om.NodeInfo parent, string role) {
            Saxon.Hej.tree.iter.AxisIterator iter = parent.iterateAxis(Saxon.Hej.om.AxisInfo.CHILD, Saxon.Hej.pattern.NodeKindTest.ELEMENT);
            Saxon.Hej.om.NodeInfo node;
            while (((node = ((/*1430*/Saxon.Hej.om.NodeInfo)iter.next())) != null)) {
                string roleAtt = node.getAttributeValue("", "role");
                if (role.Equals(roleAtt)) {
                    return node;
                }
            }
            return null;
        }
        public Saxon.Hej.expr.Expression getFirstChildExpression(Saxon.Hej.om.NodeInfo parent) {
            Saxon.Hej.om.NodeInfo node = ((/*1430*/Saxon.Hej.om.NodeInfo)parent.iterateAxis(Saxon.Hej.om.AxisInfo.CHILD, Saxon.Hej.pattern.NodeKindTest.ELEMENT).next());
            return loadExpression(node);
        }
        public Saxon.Hej.expr.Expression getSecondChildExpression(Saxon.Hej.om.NodeInfo parent) {
            Saxon.Hej.om.NodeInfo node = getChild(parent, 1);
            return loadExpression(node);
        }
        public Saxon.Hej.expr.Expression getNthChildExpression(Saxon.Hej.om.NodeInfo parent, int n) {
            Saxon.Hej.om.NodeInfo node = getChild(parent, n);
            return loadExpression(node);
        }
        public Saxon.Hej.expr.Expression getExpressionWithRole(Saxon.Hej.om.NodeInfo parent, string role) {
            Saxon.Hej.om.NodeInfo node = getChildWithRole(parent, role);
            return ((node == null) ? null : loadExpression(node));
        }
        public Saxon.Hej.expr.Expression loadExpression(Saxon.Hej.om.NodeInfo element) {
            if ((element == null)) {
                return null;
            }
            string tag = element.getLocalPart();
            Saxon.Hej.trans.PackageLoaderHE.ExpressionLoader loader = Saxon.Impl.Helpers.MapUtils.Get(eMap, tag);
            if ((loader == null)) {
                string message = ("Cannot load expression with tag "+tag);
                string req = Saxon.Impl.Helpers.MapUtils.Get(licensableConstructs, tag);
                if ((req != null)) {
                    message += ((". The stylesheet uses Saxon-"+req)+" features");
                }
                throw new Saxon.Hej.trans.XPathException(message, Saxon.Hej.trans.SaxonErrorCode.SXPK0002);
            } else {
                Saxon.Hej.expr.parser.RetainedStaticContext rsc = makeRetainedStaticContext(element);
                contextStack.Push(rsc);
                Saxon.Hej.expr.Expression exp = /*Delegate.loadFrom()*/loader(this, element);
                exp.setRetainedStaticContextLocally(rsc);
                contextStack.Pop();
                exp.setLocation(makeLocation(element));
                return exp;
            }
        }
        private Saxon.Hej.s9api.Location makeLocation(Saxon.Hej.om.NodeInfo element) {
            string lineAtt = getInheritedAttribute(element, "line");
            string moduleAtt = getInheritedAttribute(element, "module");
            if (((lineAtt != null) && (moduleAtt != null))) {
                int line = Saxon.Impl.Helpers.Utils.ParseInt(lineAtt);
                return allocateLocation(moduleAtt, line);
            } else {
                return Saxon.Hej.expr.parser.Loc.NONE;
            }
        }
        public Saxon.Hej.expr.parser.RetainedStaticContext makeRetainedStaticContext(Saxon.Hej.om.NodeInfo element) {
            Saxon.Hej.style.StylesheetPackage pack = packStack.Peek();
            string baseURIAtt = element.getAttributeValue("", "baseUri");
            string defaultCollAtt = element.getAttributeValue("", "defaultCollation");
            string defaultElementNS = element.getAttributeValue("", "defaultElementNS");
            string nsAtt = element.getAttributeValue("", "ns");
            string versionAtt = element.getAttributeValue("", "vn");
            if (((((((baseURIAtt != null) || (defaultCollAtt != null)) || (nsAtt != null)) || (versionAtt != null)) || (defaultElementNS != null)) || (contextStack.Peek().getDecimalFormatManager() == null))) {
                Saxon.Hej.expr.parser.RetainedStaticContext rsc = new Saxon.Hej.expr.parser.RetainedStaticContext(config);
                rsc.setPackageData(pack);
                if ((defaultCollAtt != null)) {
                    rsc.setDefaultCollationName(defaultCollAtt);
                } else {
                    rsc.setDefaultCollationName(Saxon.Hej.lib.NamespaceConstant.CODEPOINT_COLLATION_URI);
                }
                if ((baseURIAtt != null)) {
                    rsc.setStaticBaseUriString(baseURIAtt);
                } else {
                    if ((relocatableBase != null)) {
                        rsc.setStaticBaseUriString(relocatableBase);
                    } else {
                        string @base = Saxon.Hej.tree.util.Navigator.getInheritedAttributeValue(element, "", "baseUri");
                        if ((@base != null)) {
                            rsc.setStaticBaseUriString(@base);
                        }
                    }
                }
                if ((nsAtt == null)) {
                    nsAtt = Saxon.Hej.tree.util.Navigator.getInheritedAttributeValue(element, "", "ns");
                }
                if (((nsAtt != null) && !(nsAtt.Length==0))) {
                    string[] namespaces = System.Text.RegularExpressions.Regex.Split(nsAtt, " ");
                    foreach (string ns in namespaces) {
                        int eq = ns.IndexOf('=');
                        if ((eq<0)) {
                            throw new Saxon.Ejava.lang.IllegalStateException(("ns="+nsAtt));
                        }
                        string prefix = ns.Substring(0, eq - 0);
                        string uri = ns.Substring((eq+1));
                        if (uri.Equals("~")) {
                            uri = Saxon.Hej.lib.NamespaceConstant.getUriForConventionalPrefix(prefix);
                        }
                        rsc.declareNamespace(prefix, uri);
                    }
                }
                if ((defaultElementNS == null)) {
                    defaultElementNS = Saxon.Hej.tree.util.Navigator.getInheritedAttributeValue(element, "", "defaultElementNS");
                }
                if ((defaultElementNS != null)) {
                    rsc.setDefaultElementNamespace(defaultElementNS);
                }
                rsc.setDecimalFormatManager(packStack.Peek().getDecimalFormatManager());
                return rsc;
            } else {
                return contextStack.Peek();
            }
        }
        private Saxon.Hej.pattern.Pattern getFirstChildPattern(Saxon.Hej.om.NodeInfo parent) {
            Saxon.Hej.om.NodeInfo node = ((/*1430*/Saxon.Hej.om.NodeInfo)parent.iterateAxis(Saxon.Hej.om.AxisInfo.CHILD, Saxon.Hej.pattern.NodeKindTest.ELEMENT).next());
            return loadPattern(node);
        }
        private Saxon.Hej.pattern.Pattern getSecondChildPattern(Saxon.Hej.om.NodeInfo parent) {
            Saxon.Hej.om.NodeInfo node = getChild(parent, 1);
            return loadPattern(node);
        }
        public Saxon.Hej.pattern.Pattern getPatternWithRole(Saxon.Hej.om.NodeInfo parent, string role) {
            Saxon.Hej.om.NodeInfo node = getChildWithRole(parent, role);
            return ((node == null) ? null : loadPattern(node));
        }
        private Saxon.Hej.pattern.Pattern loadPattern(Saxon.Hej.om.NodeInfo element) {
            string tag = element.getLocalPart();
            Saxon.Hej.trans.PackageLoaderHE.PatternLoader loader = Saxon.Impl.Helpers.MapUtils.Get(pMap, tag);
            if ((loader == null)) {
                throw new Saxon.Hej.trans.XPathException(("Cannot load pattern with tag "+tag), Saxon.Hej.trans.SaxonErrorCode.SXPK0002);
            } else {
                Saxon.Hej.pattern.Pattern pat = /*Delegate.loadFrom()*/loader(this, element);
                pat.setLocation(makeLocation(element));
                pat.setRetainedStaticContext(makeRetainedStaticContext(element));
                return pat;
            }
        }
        public Saxon.Hej.type.SchemaType getTypeAttribute(Saxon.Hej.om.NodeInfo element, string attName) {
            string val = element.getAttributeValue("", attName);
            if ((val == null)) {
                return null;
            }
            if (val.StartsWith("xs:")) {
                return config.getSchemaType(new Saxon.Hej.om.StructuredQName("xs", Saxon.Hej.lib.NamespaceConstant.SCHEMA, val.Substring(3)));
            } else {
                Saxon.Hej.om.StructuredQName name = getQNameAttribute(element, attName);
                return config.getSchemaType(name);
            }
        }
        public Saxon.Hej.om.StructuredQName getQNameAttribute(Saxon.Hej.om.NodeInfo element, string localName) {
            string val = element.getAttributeValue("", localName);
            if ((val == null)) {
                return null;
            }
            return Saxon.Hej.om.StructuredQName.fromEQName((val));
        }
        public System.Collections.Generic.IList<Saxon.Hej.om.StructuredQName> getListOfQNameAttribute(Saxon.Hej.om.NodeInfo element, string localName) {
            string val = element.getAttributeValue("", localName);
            if ((val == null)) {
                return new System.Collections.Generic.List<Saxon.Hej.om.StructuredQName>();
            }
            System.Collections.Generic.IList<Saxon.Hej.om.StructuredQName> result = new System.Collections.Generic.List<Saxon.Hej.om.StructuredQName>();
            foreach (string s in System.Text.RegularExpressions.Regex.Split(val, " ")) {
                Saxon.Hej.om.StructuredQName sq = resolveQName(s, element);
                result.Add(sq);
            }
            return result;
        }
        private Saxon.Hej.om.StructuredQName resolveQName(string val, Saxon.Hej.om.NodeInfo element) {
            if (val.StartsWith("Q{")) {
                return Saxon.Hej.om.StructuredQName.fromEQName((val));
            } else {
                if (val.Contains(":")) {
                    return Saxon.Hej.om.StructuredQName.fromLexicalQName((val), true, true, element.getAllNamespaces());
                } else {
                    return new Saxon.Hej.om.StructuredQName("", "", val);
                }
            }
        }
        public int getIntegerAttribute(Saxon.Hej.om.NodeInfo element, string localName) {
            string val = element.getAttributeValue("", localName);
            if ((val == null)) {
                return System.Int32.MinValue;
            }
            try {
                return Saxon.Impl.Helpers.Utils.ParseInt(val);
            } catch (Saxon.Ejava.lang.NumberFormatException e) {
                throw new Saxon.Hej.trans.XPathException((((((("Expected integer value for "+element.getDisplayName())+"/")+localName)+", found '")+val)+"'"), Saxon.Hej.trans.SaxonErrorCode.SXPK0002);
            }
        }
        public string getInheritedAttribute(Saxon.Hej.om.NodeInfo element, string localName) {
            while ((element != null)) {
                string val = element.getAttributeValue("", localName);
                if ((val != null)) {
                    return val;
                }
                element = element.getParent();
            }
            return null;
        }
        public Saxon.Hej.value.SequenceType parseSequenceType(Saxon.Hej.om.NodeInfo element, string name) {
            Saxon.Hej.sxpath.IndependentContext env = makeStaticContext(element);
            string attValue = element.getAttributeValue("", name);
            if ((attValue == null)) {
                return Saxon.Hej.value.SequenceType.ANY_SEQUENCE;
            } else {
                return parser.parseExtendedSequenceType(attValue, env);
            }
        }
        public Saxon.Hej.value.SequenceType parseAlphaCode(Saxon.Hej.om.NodeInfo element, string name) {
            string attValue = element.getAttributeValue("", name);
            if ((attValue == null)) {
                return Saxon.Hej.value.SequenceType.ANY_SEQUENCE;
            } else {
                try {
                    return Saxon.Hej.type.AlphaCode.toSequenceType(attValue, config);
                } catch (Exception e) when (e is Saxon.Ejava.lang.IllegalArgumentException || e is Saxon.Ejava.lang.IllegalStateException) {
                    throw new Saxon.Hej.trans.XPathException(((((((("Invalid alpha code "+element.getDisplayName())+"/@")+name)+"='")+attValue)+"': ")+e.Message));
                }
            }
        }
        public Saxon.Hej.type.ItemType parseAlphaCodeForItemType(Saxon.Hej.om.NodeInfo element, string name) {
            string attValue = element.getAttributeValue("", name);
            if ((attValue == null)) {
                return Saxon.Hej.type.AnyItemType.getInstance();
            } else {
                try {
                    return Saxon.Hej.type.AlphaCode.toItemType(attValue, config);
                } catch (Exception e) when (e is Saxon.Ejava.lang.IllegalArgumentException || e is Saxon.Ejava.lang.IllegalStateException) {
                    throw new Saxon.Hej.trans.XPathException(((((((("Invalid alpha code "+element.getDisplayName())+"/@")+name)+"='")+attValue)+"': ")+e.Message));
                }
            }
        }
        private Saxon.Hej.sxpath.IndependentContext makeStaticContext(Saxon.Hej.om.NodeInfo element) {
            Saxon.Hej.style.StylesheetPackage pack = packStack.Peek();
            Saxon.Hej.sxpath.IndependentContext env = new Saxon.Hej.sxpath.IndependentContext(config);
            Saxon.Hej.om.NamespaceResolver resolver = element.getAllNamespaces();
            env.setNamespaceResolver(resolver);
            env.setImportedSchemaNamespaces(pack.getSchemaNamespaces());
            env.getImportedSchemaNamespaces().Add(Saxon.Hej.lib.NamespaceConstant.ANONYMOUS);
            parser.setQNameParser(parser.getQNameParser().withNamespaceResolver(resolver));
            return env;
        }
        public Saxon.Hej.type.ItemType parseItemTypeAttribute(Saxon.Hej.om.NodeInfo element, string attName) {
            string attValue = element.getAttributeValue("", attName);
            if ((attValue == null)) {
                return Saxon.Hej.type.AnyItemType.getInstance();
            }
            return parseItemType(element, attValue);
        }
        private Saxon.Hej.type.ItemType parseItemType(Saxon.Hej.om.NodeInfo element, string attValue) {
            Saxon.Hej.sxpath.IndependentContext env = makeStaticContext(element);
            return parser.parseExtendedItemType(attValue, env);
        }
        public Saxon.Hej.expr.sort.AtomicComparer makeAtomicComparer(string name, Saxon.Hej.om.NodeInfo element) {
            if (name.Equals("CCC")) {
                return Saxon.Hej.expr.sort.CodepointCollatingComparer.getInstance();
            } else {
                if (name.Equals("CAVC")) {
                    return Saxon.Hej.expr.sort.ContextFreeAtomicComparer.getInstance();
                } else {
                    if (name.StartsWith("GAC|")) {
                        Saxon.Hej.lib.StringCollator collator = config.getCollation(name.Substring(4));
                        return new Saxon.Hej.expr.sort.GenericAtomicComparer(collator, null);
                    } else {
                        if (name.Equals("CalVC")) {
                            return new Saxon.Hej.expr.sort.CalendarValueComparer(null);
                        } else {
                            if (name.Equals("EQC")) {
                                return Saxon.Hej.expr.sort.EqualityComparer.getInstance();
                            } else {
                                if (name.Equals("NC")) {
                                    return Saxon.Hej.expr.sort.NumericComparer.getInstance();
                                } else {
                                    if (name.Equals("NC11")) {
                                        return Saxon.Hej.expr.sort.NumericComparer11.getInstance();
                                    } else {
                                        if (name.Equals("QUNC")) {
                                            return new Saxon.Hej.expr.sort.UntypedNumericComparer();
                                        } else {
                                            if (name.Equals("DblSC")) {
                                                return Saxon.Hej.expr.sort.DoubleSortComparer.getInstance();
                                            } else {
                                                if (name.Equals("DecSC")) {
                                                    return Saxon.Hej.expr.sort.DecimalSortComparer.getDecimalSortComparerInstance();
                                                } else {
                                                    if (name.StartsWith("CAC|")) {
                                                        Saxon.Hej.lib.StringCollator collator = config.getCollation(name.Substring(4));
                                                        return new Saxon.Hej.expr.sort.CollatingAtomicComparer(collator);
                                                    } else {
                                                        if (name.StartsWith("AtSC|")) {
                                                            int nextBar = name.IndexOf('|', 5);
                                                            string fps = name.Substring(5, nextBar - 5);
                                                            int fp = Saxon.Impl.Helpers.Utils.ParseInt(fps);
                                                            string collName = name.Substring((nextBar+1));
                                                            return Saxon.Hej.expr.sort.AtomicSortComparer.makeSortComparer(config.getCollation(collName), fp, new Saxon.Hej.expr.EarlyEvaluationContext(config));
                                                        } else {
                                                            if (name.StartsWith("DESC|")) {
                                                                Saxon.Hej.expr.sort.AtomicComparer @base = makeAtomicComparer(name.Substring(5), element);
                                                                return new Saxon.Hej.expr.sort.DescendingComparer(@base);
                                                            } else {
                                                                if (name.StartsWith("TEXT|")) {
                                                                    Saxon.Hej.expr.sort.AtomicComparer @base = makeAtomicComparer(name.Substring(5), element);
                                                                    return new Saxon.Hej.expr.sort.TextComparer(@base);
                                                                } else {
                                                                    throw new Saxon.Hej.trans.XPathException(("Unknown comparer "+name), Saxon.Hej.trans.SaxonErrorCode.SXPK0002);
                                                                }
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
        private Saxon.Hej.expr.sort.SortKeyDefinitionList loadSortKeyDefinitions(Saxon.Hej.om.NodeInfo element) {
            System.Collections.Generic.IList<Saxon.Hej.expr.sort.SortKeyDefinition> skdl = new System.Collections.Generic.List<Saxon.Hej.expr.sort.SortKeyDefinition>(4);
            Saxon.Hej.om.NodeInfo sortKeyElement;
            Saxon.Hej.tree.iter.AxisIterator iterator = element.iterateAxis(Saxon.Hej.om.AxisInfo.CHILD, new Saxon.Hej.pattern.NameTest(Saxon.Hej.type.Type.ELEMENT, Saxon.Hej.lib.NamespaceConstant.SAXON_XSLT_EXPORT, "sortKey", config.getNamePool()));
            while (((sortKeyElement = ((/*1430*/Saxon.Hej.om.NodeInfo)iterator.next())) != null)) {
                Saxon.Hej.expr.sort.SortKeyDefinition skd = new Saxon.Hej.expr.sort.SortKeyDefinition();
                string compAtt = sortKeyElement.getAttributeValue("", "comp");
                if ((compAtt != null)) {
                    Saxon.Hej.expr.sort.AtomicComparer ac = makeAtomicComparer(compAtt, sortKeyElement);
                    skd.setFinalComparator(ac);
                }
                skd.setSortKey(getExpressionWithRole(sortKeyElement, "select"), true);
                skd.setOrder(getExpressionWithRole(sortKeyElement, "order"));
                skd.setLanguage(getExpressionWithRole(sortKeyElement, "lang"));
                skd.setCollationNameExpression(getExpressionWithRole(sortKeyElement, "collation"));
                skd.setCaseOrder(getExpressionWithRole(sortKeyElement, "caseOrder"));
                skd.setStable(getExpressionWithRole(sortKeyElement, "stable"));
                skd.setDataTypeExpression(getExpressionWithRole(sortKeyElement, "dataType"));
                skdl.Add(skd);
            }
            return new Saxon.Hej.expr.sort.SortKeyDefinitionList(skdl.ToArray());
        }
        private Saxon.Hej.expr.instruct.WithParam[] loadWithParams(Saxon.Hej.om.NodeInfo element, Saxon.Hej.expr.Expression parent, bool needTunnel) {
            System.Collections.Generic.IList<Saxon.Hej.expr.instruct.WithParam> wps = new System.Collections.Generic.List<Saxon.Hej.expr.instruct.WithParam>(4);
            Saxon.Hej.om.NodeInfo wpElement;
            Saxon.Hej.tree.iter.AxisIterator iterator = element.iterateAxis(Saxon.Hej.om.AxisInfo.CHILD, new Saxon.Hej.pattern.NameTest(Saxon.Hej.type.Type.ELEMENT, Saxon.Hej.lib.NamespaceConstant.SAXON_XSLT_EXPORT, "withParam", config.getNamePool()));
            while (((wpElement = ((/*1430*/Saxon.Hej.om.NodeInfo)iterator.next())) != null)) {
                string flags = wpElement.getAttributeValue("", "flags");
                bool isTunnel = ((flags != null) && flags.Contains("t"));
                if ((needTunnel == isTunnel)) {
                    Saxon.Hej.expr.instruct.WithParam wp = new Saxon.Hej.expr.instruct.WithParam();
                    wp.setVariableQName(getQNameAttribute(wpElement, "name"));
                    wp.setSelectExpression(parent, getFirstChildExpression(wpElement));
                    wp.setRequiredType(parseAlphaCode(wpElement, "as"));
                    wp.setTypeChecked(((flags != null) && flags.Contains("c")));
                    wps.Add(wp);
                }
            }
            return wps.ToArray();
        }
        private Saxon.Ejava.util.Properties importProperties(string value) {
            try {
                System.IO.StringReader reader = new System.IO.StringReader(value);
                Saxon.Ejava.util.Properties props = new Saxon.Ejava.util.Properties();
                Saxon.Impl.Helpers.LineNumberReader lnr = new Saxon.Impl.Helpers.LineNumberReader(reader);
                string line;
                while (((line = lnr.ReadLine()) != null)) {
                    int eq = line.IndexOf('=');
                    string key = line.Substring(0, eq - 0);
                    string val = ((eq == (line.Length-1)) ? "" : line.Substring((eq+1)));
                    if ((key.Equals("item-separator") || key.Equals(("Q"+Saxon.Hej.lib.SaxonOutputKeys.NEWLINE)))) {
                        try {
                            val = Saxon.Hej.ma.json.JsonParser.unescape(val, 0, "", -1);
                        } catch (Saxon.Hej.trans.XPathException ignored) {
                        }
                    }
                    if (key.StartsWith("Q{")) {
                        key = key.Substring(1);
                    }
                    props.setProperty(key, val);
                }
                return props;
            } catch (System.IO.IOException e) {
                throw new Saxon.Ejava.lang.AssertionError(e);
            }
        }
        public delegate Saxon.Hej.expr.Expression ExpressionLoader (Saxon.Hej.trans.PackageLoaderHE loader, Saxon.Hej.om.NodeInfo element);
        public delegate Saxon.Hej.pattern.Pattern PatternLoader (Saxon.Hej.trans.PackageLoaderHE loader, Saxon.Hej.om.NodeInfo element);
        protected internal static readonly System.Collections.Generic.IDictionary<string,Saxon.Hej.trans.PackageLoaderHE.ExpressionLoader> eMap = new System.Collections.Generic.Dictionary<string, Saxon.Hej.trans.PackageLoaderHE.ExpressionLoader>(200);
        protected internal static readonly System.Collections.Generic.IDictionary<string,string> licensableConstructs = new System.Collections.Generic.Dictionary<string, string>(30);
        private static int getLevelCode(string levelAtt) {
            if ((levelAtt == null)) {
                return Saxon.Hej.expr.instruct.NumberInstruction.SINGLE;
            } else {
                switch (levelAtt) {
                  case "single":
                    return Saxon.Hej.expr.instruct.NumberInstruction.SINGLE;
                  case "multi":
                    return Saxon.Hej.expr.instruct.NumberInstruction.MULTI;
                  case "any":
                    return Saxon.Hej.expr.instruct.NumberInstruction.ANY;
                  case "simple":
                    return Saxon.Hej.expr.instruct.NumberInstruction.SIMPLE;
                  default:
                    throw new Saxon.Ejava.lang.AssertionError();
                }
            }
        }
        private static Saxon.Hej.expr.LocalBinding findLocalBinding(System.Collections.Generic.Stack<Saxon.Hej.expr.LocalBinding> locals, Saxon.Hej.om.StructuredQName name) {
            foreach (Saxon.Hej.expr.LocalBinding b in locals) {
                if (b.getVariableQName().Equals(name)) {
                    return b;
                }
            }
            return null;
        }
        protected internal static System.Collections.Generic.IList<Saxon.Hej.expr.Expression> getChildExpressionList(Saxon.Hej.trans.PackageLoaderHE loader, Saxon.Hej.om.NodeInfo element) {
            System.Collections.Generic.IList<Saxon.Hej.expr.Expression> children = new System.Collections.Generic.List<Saxon.Hej.expr.Expression>();
            Saxon.Hej.tree.iter.AxisIterator iter = element.iterateAxis(Saxon.Hej.om.AxisInfo.CHILD, Saxon.Hej.pattern.NodeKindTest.ELEMENT);
            Saxon.Hej.om.NodeInfo child;
            while (((child = ((/*1430*/Saxon.Hej.om.NodeInfo)iter.next())) != null)) {
                children.Add(loader.loadExpression(child));
            }
            return children;
        }
        protected internal static Saxon.Hej.expr.Expression[] getChildExpressionArray(Saxon.Hej.trans.PackageLoaderHE loader, Saxon.Hej.om.NodeInfo element) {
            System.Collections.Generic.IList<Saxon.Hej.expr.Expression> children = getChildExpressionList(loader, element);
            return children.ToArray();
        }
        protected internal static int getOperator(string opAtt) {
            int op;
            switch (opAtt) {
              case "=":
                op = Saxon.Hej.expr.parser.Token.EQUALS;
                break ;
              case "!=":
                op = Saxon.Hej.expr.parser.Token.NE;
                break ;
              case "<=":
                op = Saxon.Hej.expr.parser.Token.LE;
                break ;
              case ">=":
                op = Saxon.Hej.expr.parser.Token.GE;
                break ;
              case "<":
                op = Saxon.Hej.expr.parser.Token.LT;
                break ;
              case ">":
                op = Saxon.Hej.expr.parser.Token.GT;
                break ;
              default:
                throw new Saxon.Ejava.lang.IllegalStateException();
            }
            return op;
        }
        private static int parseValueComparisonOperator(string opAtt) {
            int op;
            switch (opAtt) {
              case "eq":
                op = Saxon.Hej.expr.parser.Token.FEQ;
                break ;
              case "ne":
                op = Saxon.Hej.expr.parser.Token.FNE;
                break ;
              case "le":
                op = Saxon.Hej.expr.parser.Token.FLE;
                break ;
              case "ge":
                op = Saxon.Hej.expr.parser.Token.FGE;
                break ;
              case "lt":
                op = Saxon.Hej.expr.parser.Token.FLT;
                break ;
              case "gt":
                op = Saxon.Hej.expr.parser.Token.FGT;
                break ;
              default:
                throw new Saxon.Ejava.lang.IllegalStateException();
            }
            return op;
        }
        static readonly System.Collections.Generic.IDictionary<string,Saxon.Hej.trans.PackageLoaderHE.PatternLoader> pMap = new System.Collections.Generic.Dictionary<string, Saxon.Hej.trans.PackageLoaderHE.PatternLoader>(200);
        private void resolveFixups() {
            Saxon.Hej.style.StylesheetPackage pack = packStack.Peek();
            foreach (Saxon.Hej.expr.ComponentInvocation call in fixups.Peek()) {
                if (processComponentReference(pack, call)) {
                    break ;
                }
            }
            pack.allocateBinderySlots();
        }
        protected internal virtual bool processComponentReference(Saxon.Hej.style.StylesheetPackage pack, Saxon.Hej.expr.ComponentInvocation call) {
            Saxon.Hej.trans.SymbolicName sn = call.getSymbolicName();
            Saxon.Hej.expr.Component c = pack.getComponent(sn);
            if ((c == null)) {
                if ((sn.getComponentName().hasURI(Saxon.Hej.lib.NamespaceConstant.XSLT) && sn.getComponentName().getLocalPart().Equals("original"))) {
                    return true;
                } else {
                    throw new Saxon.Hej.trans.XPathException(("Loading compiled package: unresolved component reference to "+sn));
                }
            }
            if ((call is Saxon.Hej.expr.GlobalVariableReference)) {
                ((Saxon.Hej.expr.GlobalVariableReference)(call)).setTarget(c);
            } else {
                if ((call is Saxon.Hej.expr.UserFunctionCall)) {
                    ((Saxon.Hej.expr.UserFunctionCall)(call)).setFunction((Saxon.Hej.expr.instruct.UserFunction)(c.getActor()));
                    ((Saxon.Hej.expr.UserFunctionCall)(call)).setStaticType(((Saxon.Hej.expr.instruct.UserFunction)(c.getActor())).getResultType());
                } else {
                    if ((call is Saxon.Hej.functions.hof.UserFunctionReference)) {
                        ((Saxon.Hej.functions.hof.UserFunctionReference)(call)).setFunction((Saxon.Hej.expr.instruct.UserFunction)(c.getActor()));
                    } else {
                        if ((call is Saxon.Hej.expr.instruct.CallTemplate)) {
                            ((Saxon.Hej.expr.instruct.CallTemplate)(call)).setTargetTemplate((Saxon.Hej.expr.instruct.NamedTemplate)(c.getActor()));
                        } else {
                            if ((call is Saxon.Hej.expr.instruct.UseAttributeSet)) {
                                ((Saxon.Hej.expr.instruct.UseAttributeSet)(call)).setTarget((Saxon.Hej.expr.instruct.AttributeSet)(c.getActor()));
                            } else {
                                if ((call is Saxon.Hej.expr.instruct.ApplyTemplates)) {
                                    ((Saxon.Hej.expr.instruct.ApplyTemplates)(call)).setMode((Saxon.Hej.trans.SimpleMode)(c.getActor()));
                                } else {
                                    throw new Saxon.Hej.trans.XPathException(("Unknown component reference "+call.GetType()));
                                }
                            }
                        }
                    }
                }
            }
            return false;
        }
        private Saxon.Hej.s9api.Location allocateLocation(string module, int lineNumber) {
            Saxon.Hej.z.IntHashMap<Saxon.Hej.s9api.Location> lineMap = Saxon.Impl.Helpers.MapUtils.Get(locationMap, module);
            if ((lineMap == null)) {
                lineMap = new Saxon.Hej.z.IntHashMap<Saxon.Hej.s9api.Location>();
                Saxon.Impl.Helpers.MapUtils.Put(locationMap, module, lineMap);
            }
            Saxon.Hej.s9api.Location loc = lineMap.get(lineNumber);
            if ((loc == null)) {
                loc = new Saxon.Hej.expr.parser.Loc(module, lineNumber, -1);
                lineMap.put(lineNumber, loc);
            }
            return loc;
        }
    
    }
}