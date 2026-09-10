package com.saxonica.trans;

import com.saxonica.config.JavaExtensionLibrary;
import com.saxonica.config.ProfessionalConfiguration;
import com.saxonica.expr.JavaExtensionFunctionCall;
import com.saxonica.expr.MemoFunction;
import com.saxonica.functions.hof.*;
import com.saxonica.xsltextn.instruct.Assign;
import com.saxonica.xsltextn.instruct.DoInstr;
import com.saxonica.xsltextn.instruct.While;
import net.sf.saxon.Configuration;
import net.sf.saxon.expr.*;
import net.sf.saxon.expr.instruct.GlobalVariable;
import net.sf.saxon.expr.instruct.OriginalFunction;
import net.sf.saxon.expr.instruct.UserFunction;
import net.sf.saxon.expr.instruct.WithParam;
import net.sf.saxon.expr.parser.RetainedStaticContext;
import net.sf.saxon.expr.parser.RoleDiagnostic;
import net.sf.saxon.functions.FunctionLibrary;
import net.sf.saxon.functions.MathFunctionSet;
import net.sf.saxon.functions.SystemFunction;
import net.sf.saxon.lib.NamespaceConstant;
import net.sf.saxon.ma.arrays.ArrayFunctionSet;
import net.sf.saxon.ma.map.MapFunctionSet;
import net.sf.saxon.om.*;
import net.sf.saxon.pattern.NameTest;
import net.sf.saxon.pattern.NodeKindTest;
import net.sf.saxon.style.StylesheetPackage;
import net.sf.saxon.sxpath.IndependentContext;
import net.sf.saxon.trans.PackageLoaderHE;
import net.sf.saxon.trans.SaxonErrorCode;
import net.sf.saxon.trans.SymbolicName;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.tree.iter.AxisIterator;
import net.sf.saxon.type.*;
import net.sf.saxon.value.SequenceType;

import java.util.ArrayList;
import java.util.List;

/**
 * This class reads the XML exported form of a package and reconstructs the package object in memory.
 */

public class PackageLoaderPE extends PackageLoaderHE {

    public PackageLoaderPE(ProfessionalConfiguration config) {
        super(config);
    }


    protected void readSchemaNamespaces(NodeInfo packageElement) throws XPathException {
        NodeInfo varElement;
        AxisIterator iterator = packageElement.iterateAxis(AxisInfo.CHILD,
                                                           new NameTest(Type.ELEMENT, NamespaceConstant.SAXON_XSLT_EXPORT, "schema", getConfiguration().getNamePool()));
        Configuration config = getConfiguration();
        while ((varElement = iterator.next()) != null) {
            String ns = varElement.getAttributeValue("", "ns");
            if (!config.isSchemaAvailable(ns) && !ns.equals(NamespaceConstant.ANONYMOUS)) {
                throw new XPathException(
                        "The XSLT package requires access to " +
                                (ns.isEmpty() ? "a no-namespace schema" : "a schema for namespace " + ns) +
                                ", but this is not available in this Configuration", SaxonErrorCode.SXPK0003);
            }
            packStack.peek().getSchemaNamespaces().add(ns);
        }
    }

    protected UserFunction makeFunction(String flags) {
        return flags.contains("m") ? new MemoFunction() : new UserFunction();
    }


    static {

        eMap.put("acFnRef", (loader, element) -> {
            StructuredQName name = loader.getQNameAttribute(element, "name");
            SchemaType type = loader.getConfiguration().getSchemaType(name);
            if (!(type instanceof AtomicType)) {
                throw new XPathException("No atomic type " + name.getEQName() + " found");
            }
            AtomicConstructorFunction fn = new AtomicConstructorFunction((AtomicType) type, null);
            return new FunctionLiteral(fn);
        });

        eMap.put("assign", (loader, element) -> {
            loader.needsPELicense("saxon:assign");
            final StructuredQName varName = loader.getQNameAttribute(element, "name");
            Expression select = loader.getFirstChildExpression(element);
            final Assign inst = new Assign(select);
            loader.completionActions.add(() -> {
                SymbolicName sn = new SymbolicName(StandardNames.XSL_VARIABLE, varName);
                GlobalVariable var = (GlobalVariable) loader.getPackage().getComponent(sn).getActor();
                inst.fixup(var);
            });
            return inst;
        });


        eMap.put("coercedFn", (loader, element) -> {
            ItemType type = loader.parseItemTypeAttribute(element, "type");
            Expression target = loader.getFirstChildExpression(element);
            Function targetFn;
            CoercedFunction coercedFn;
            if (target instanceof UserFunctionReference) {
                coercedFn = new CoercedFunction((SpecificFunctionType) type);
                final CoercedFunction coercedFn2 = coercedFn;
                final SymbolicName name = ((UserFunctionReference) target).getSymbolicName();
                loader.completionActions.add(() -> coercedFn2.setTargetFunction(loader.userFunctions.get(name)));
            } else if (target instanceof FunctionLiteral) {
                targetFn = (Function) ((Literal) target).getValue();
                coercedFn = new CoercedFunction(targetFn, (SpecificFunctionType) type);
            } else {
                throw new AssertionError();
            }
            return Literal.makeLiteral(coercedFn);
        });


        eMap.put("curriedFunc", (loader, element) -> {
            Expression target = loader.getFirstChildExpression(element);
            Function targetFn = (Function) ((Literal) target).getValue();
            NodeInfo args = loader.getChild(element, 1);
            int count = 0;
            AxisIterator iter = args.iterateAxis(AxisInfo.CHILD, NodeKindTest.ELEMENT);
            NodeInfo child;
            while ((child = iter.next()) != null) {
                count++;
            }
            Sequence[] argValues = new Sequence[count];
            iter = args.iterateAxis(AxisInfo.CHILD, NodeKindTest.ELEMENT);
            count = 0;
            while ((child = iter.next()) != null) {
                if (child.getLocalPart().equals("x")) {
                    argValues[count++] = null;
                } else {
                    Expression arg = loader.loadExpression(child);
                    argValues[count++] = ((Literal) arg).getValue();
                }
            }
            Function f = new CurriedFunction(targetFn, argValues);
            return Literal.makeLiteral(f);
        });

        eMap.put("do", (loader, element) -> {
            Expression arg = loader.getFirstChildExpression(element);
            return new DoInstr(arg);
        });

        eMap.put("evaluate", (loader, element) -> {
            SequenceType required = loader.parseSequenceType(element, "as");
            Expression xpath = loader.getExpressionWithRole(element, "xpath");
            Expression contextItem = loader.getExpressionWithRole(element, "cxt");
            Expression baseUri = loader.getExpressionWithRole(element, "baseUri");
            Expression namespaceContext = loader.getExpressionWithRole(element, "nsCxt");
            Expression schemaAware = loader.getExpressionWithRole(element, "sa");
            Expression dynamicParams = loader.getExpressionWithRole(element, "wp");
            Expression optionsOp = loader.getExpressionWithRole(element, "options");

            EvaluateInstr inst =
                    new EvaluateInstr(xpath, required, contextItem, baseUri, namespaceContext, schemaAware);
            if(optionsOp != null) {
                inst.setOptionsExpression(optionsOp);
            }
            String namespaces = element.getAttributeValue("", "schNS");
            if (namespaces != null) {
                String[] uris = namespaces.split(" ");
                for (String uri : uris) {
                    if (uri.equals("##")) {
                        uri = "";
                    }
                    inst.importSchemaNamespace(uri);
                }
            }

            NameTest test = new NameTest(Type.ELEMENT, NamespaceConstant.SAXON_XSLT_EXPORT,
                                         "withParam", loader.getConfiguration().getNamePool());
            AxisIterator kids = element.iterateAxis(AxisInfo.CHILD, test);
            NodeInfo wp;
            List<WithParam> nonTunnelParams = new ArrayList<>();
            int slotNumber = 0;
            while ((wp = kids.next()) != null) {
                WithParam withParam = new WithParam();
                StructuredQName paramName = loader.getQNameAttribute(wp, "name");
                withParam.setVariableQName(paramName);
                withParam.setSlotNumber(slotNumber++);
                SequenceType reqType = loader.parseSequenceType(wp, "as");
                withParam.setRequiredType(reqType);
                withParam.setSelectExpression(inst, loader.getFirstChildExpression(wp));
                nonTunnelParams.add(withParam);
            }
            inst.setActualParameters(
                    nonTunnelParams.toArray(new WithParam[0]));
            if (dynamicParams != null) {
                inst.setDynamicParams(dynamicParams);
            }
            return inst;

        });


        eMap.put("fnCoercer", (loader, element) -> {
            SpecificFunctionType type = (SpecificFunctionType) loader.parseSequenceType(element, "to").getPrimaryType();
            RoleDiagnostic role = RoleDiagnostic.reconstruct(element.getAttributeValue("", "diag"));
            Expression arg = loader.getFirstChildExpression(element);
            return new FunctionSequenceCoercer(arg, type, role);
        });

        eMap.put("fnRef", (loader, element) -> {
            loader.needsPELicense("higher order functions");
            String name = element.getAttributeValue("", "name");
            int arity = loader.getIntegerAttribute(element, "arity");
            RetainedStaticContext rsc = loader.makeRetainedStaticContext(element);
            SystemFunction f = null;
            if (name.startsWith("Q{")) {
                StructuredQName qName = StructuredQName.fromEQName(name);
                String uri = qName.getURI();
                switch (uri) {
                    case NamespaceConstant.MATH:
                        f = MathFunctionSet.getInstance().makeFunction(qName.getLocalPart(), arity);
                        break;
                    case NamespaceConstant.MAP_FUNCTIONS:
                        f = MapFunctionSet.getInstance().makeFunction(qName.getLocalPart(), arity);
                        break;
                    case NamespaceConstant.ARRAY_FUNCTIONS:
                        f = ArrayFunctionSet.getInstance().makeFunction(qName.getLocalPart(), arity);
                        break;
                    case NamespaceConstant.SAXON:
                        f = ((ProfessionalConfiguration)loader.getConfiguration()).getVendorFunctionSet().makeFunction(qName.getLocalPart(), arity);
                        break;
                }
            } else {
                f = SystemFunction.makeFunction(name, rsc, arity);
            }
            if (f == null) {
                throw new XPathException("Unknown system function " + name + "#" + arity, SaxonErrorCode.SXPK0002);
            }
            return new FunctionLiteral(f);
        });

        eMap.put("inlineFn", (loader, element) -> {
            NodeInfo first = loader.getChild(element, 0);
            UserFunction uf = loader.readFunction(first);
            return new UserFunctionReference(uf);
        });

        eMap.put("javaCall", (loader, element) -> {
            StructuredQName name = loader.getQNameAttribute(element, "name");
            Expression[] args = getChildExpressionArray(loader, element);
            loader.needsPELicense("Java extension functions");
            FunctionLibrary lib = ((ProfessionalConfiguration)loader.getConfiguration()).getExtensionBinder("java");
            SymbolicName.F sName = new SymbolicName.F(name, args.length);
            StaticContext env = new IndependentContext(loader.getConfiguration());
            List<String> reasons = new ArrayList<>();
            Expression exp = lib.bind(sName, args, env, reasons);
            if (exp == null) {
                StringBuilder msg = new StringBuilder("Failed to load Java extension function " + name.getEQName());
                for (String reason : reasons) {
                    msg.append(". ").append(reason);
                }
                throw new XPathException(msg.toString());
            }
            if (exp instanceof JavaExtensionLibrary.UnresolvedExtensionFunctionCall) {
                SequenceType[] argTypes = new SequenceType[args.length];
                for (int i = 0; i < args.length; i++) {
                    // Bug in releases up to and including 9.7.0.11
                    if (element.getAttributeValue("", "arg" + i + "type") == null) {
                        throw new XPathException("SEF file needs to be re-exported using Saxon 9.7.0.12 or later: see bug 3019");
                    }
                    argTypes[i] = loader.parseSequenceType(element, "arg" + i + "type");
                }
                exp = ((JavaExtensionLibrary.UnresolvedExtensionFunctionCall) exp).resolve(argTypes);
            }
            if (exp instanceof JavaExtensionFunctionCall) {
                ((JavaExtensionFunctionCall) exp).prepareForDynamicCall(loader.getConfiguration());
                String declaredType = element.getAttributeValue("", "declType");
                if (declaredType != null) {
                    SequenceType returnType = loader.parseSequenceType(element, "declType");
                    if (returnType.getPrimaryType() instanceof JavaExternalObjectType) {
                        ((JavaExtensionFunctionCall) exp).adjustRequiredType(
                                (JavaExternalObjectType) returnType.getPrimaryType());
                    }
                }
            }
            return exp;
        });

        eMap.put("origF", (loader, element) -> {
            StructuredQName name = loader.getQNameAttribute(element, "name");
            String packKey = element.getAttributeValue("", "pack");
            StylesheetPackage declPack = loader.allPackages.get(packKey);
            if (declPack == null) {
                throw new XPathException("Unknown package key " + packKey);
            }

            int arity = loader.getIntegerAttribute(element, "arity");
            SymbolicName sn = new SymbolicName.F(name, arity);
            Component target = declPack.getComponent(sn);
            OriginalFunction orig = new OriginalFunction(target);
            return new FunctionLiteral(orig);
        });

        eMap.put("partialApply", (loader, element) -> {
            AxisIterator iter = element.iterateAxis(AxisInfo.CHILD, NodeKindTest.ELEMENT);
            int count = 0;
            NodeInfo child;
            while ((child = iter.next()) != null) {
                count++;
            }
            Expression base = null;
            Expression[] args = new Expression[count - 1];
            count = 0;
            iter = element.iterateAxis(AxisInfo.CHILD, NodeKindTest.ELEMENT);
            while ((child = iter.next()) != null) {
                if (count == 0) {
                    base = loader.loadExpression(child);
                } else if (child.getLocalPart().equals("null")) {
                    args[count - 1] = null;
                } else {
                    args[count - 1] = loader.loadExpression(child);
                }
                count++;
            }
            return new PartialApply(base, args);
        });

        eMap.put("ufRef", (loader, element) -> {
            StructuredQName name = loader.getQNameAttribute(element, "name");
            int arity = loader.getIntegerAttribute(element, "arity");
            SymbolicName symbolicName = new SymbolicName.F(name, arity);
            UserFunctionReference call = new UserFunctionReference(symbolicName);
            int bindingSlot = loader.getIntegerAttribute(element, "bSlot");
            call.setBindingSlot(bindingSlot);
            loader.fixups.peek().add(call);
            return call;
        });

        eMap.put("while", (loader, element) -> {
            loader.needsPELicense("saxon:while");
            Expression lhs = loader.getFirstChildExpression(element);
            Expression rhs = loader.getSecondChildExpression(element);
            return new While(lhs, rhs);
        });

    }


    protected boolean processComponentReference(StylesheetPackage pack, ComponentInvocation call) throws XPathException {
        SymbolicName sn = call.getSymbolicName();
        if (call instanceof UserFunctionReference) {
            Component c = pack.getComponent(sn);
            if (c == null) {
                return super.processComponentReference(pack, call);
            }
            ((UserFunctionReference) call).setFunction((UserFunction) c.getActor());
        } else {
            return super.processComponentReference(pack, call);
        }
        return false;
    }


}

// Copyright (c) 2018 Saxonica Limited.

