package com.saxonica.trans;

import com.saxonica.config.JavaExtensionLibrary;
import com.saxonica.config.ProfessionalConfiguration;
import com.saxonica.expr.JavaExtensionFunctionCall;
import com.saxonica.xsltextn.instruct.Assign;
import com.saxonica.xsltextn.instruct.DeepUpdate;
import com.saxonica.xsltextn.instruct.DoInstr;
import com.saxonica.xsltextn.instruct.While;
import net.sf.saxon.Configuration;
import net.sf.saxon.expr.*;
import net.sf.saxon.expr.instruct.GlobalVariable;
import net.sf.saxon.expr.instruct.MemoFunction;
import net.sf.saxon.expr.instruct.UserFunction;
import net.sf.saxon.functions.FunctionLibrary;
import net.sf.saxon.functions.FunctionLibraryList;
import net.sf.saxon.functions.hof.AtomicConstructorFunction;
import net.sf.saxon.functions.hof.FunctionLiteral;
import net.sf.saxon.functions.hof.UserFunctionReference;
import net.sf.saxon.lib.NamespaceConstant;
import net.sf.saxon.om.NamespaceUri;
import net.sf.saxon.om.NodeInfo;
import net.sf.saxon.om.StandardNames;
import net.sf.saxon.om.StructuredQName;
import net.sf.saxon.pattern.NodeSelector;
import net.sf.saxon.style.StylesheetPackage;
import net.sf.saxon.sxpath.IndependentContext;
import net.sf.saxon.trans.PackageLoaderHE;
import net.sf.saxon.trans.SaxonErrorCode;
import net.sf.saxon.trans.SymbolicName;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.type.AtomicType;
import net.sf.saxon.type.JavaExternalObjectType;
import net.sf.saxon.type.SchemaType;
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


    @Override
    protected void readSchemaNamespaces(NodeInfo packageElement) throws XPathException {
        Configuration config = getConfiguration();
        for (NodeInfo varElement : packageElement.children(NodeSelector.of(n -> n.getLocalPart().equals("schema")))) {
            String ns = varElement.getAttributeValue(NamespaceUri.NULL, "ns");
            if (!config.isSchemaAvailable(NamespaceUri.of(ns)) && !ns.equals(NamespaceConstant.ANONYMOUS)) {
                throw new XPathException(
                        "The XSLT package requires access to " +
                                (ns.isEmpty() ? "a no-namespace schema" : "a schema for namespace " + ns) +
                                ", but this is not available in this Configuration", SaxonErrorCode.SXPK0003);
            }
            packStack.peek().getImportedSchemaNamespaces().add(NamespaceUri.of(ns));
        }
    }

    @Override
    protected UserFunction makeFunction(String flags) {
        return flags.contains("m") ? new MemoFunction() : new UserFunction();
    }


    static {

        eMap.put("acFnRef", (loader, element) -> {
            StructuredQName name = loader.getQNameAttribute(element, "name");
            SchemaType type = loader.getSchema().getSchemaType(name);
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
                GlobalVariable var = (GlobalVariable) loader.getTopLevelPackage().getComponent(sn).getActor();
                inst.fixup(var);
            });
            return inst;
        });



        eMap.put("deepUpdate", (loader, element) -> {
            loader.needsPELicense("saxon:deep-update");
            Expression root = loader.getExpressionWithRole(element, "root");
            Expression select = loader.getExpressionWithRole(element, "select");
            Expression action = loader.getExpressionWithRole(element, "action");
            return new DeepUpdate(root, select, action);
        });

        eMap.put("do", (loader, element) -> {
            Expression arg = loader.getFirstChildExpression(element);
            return new DoInstr(arg);
        });
        
        //#if CSHARP==false
        eMap.put("javaCall", (loader, element) -> {
            StructuredQName name = loader.getQNameAttribute(element, "name");
            Expression[] args = getChildExpressionArray(loader, element);
            loader.needsPELicense("Java extension functions");
            FunctionLibrary lib = ((ProfessionalConfiguration)loader.getConfiguration()).getExtensionBinder("java");
            SymbolicName.F sName = new SymbolicName.F(name, args.length);
            StaticContext env = new IndependentContext(loader.getConfiguration());
            List<String> reasons = new ArrayList<>();
            Expression exp = lib.bind(sName, args, null, env, reasons);
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
                    if (element.getAttributeValue(NamespaceUri.NULL, "arg" + i + "type") == null) {
                        throw new XPathException("SEF file needs to be re-exported using Saxon 9.7.0.12 or later: see bug 3019");
                    }
                    argTypes[i] = loader.parseAlphaCode(element, "arg" + i + "type");
                }
                exp = ((JavaExtensionLibrary.UnresolvedExtensionFunctionCall) exp).resolve(argTypes);
            }
            if (exp instanceof JavaExtensionFunctionCall) {
                ((JavaExtensionFunctionCall) exp).prepareForDynamicCall(loader.getConfiguration());
                String declaredType = element.getAttributeValue(NamespaceUri.NULL, "declType");
                if (declaredType != null) {
                    try {
                        StructuredQName classQName = StructuredQName.fromEQName(declaredType);
                        if (classQName.hasURI(NamespaceUri.JAVA_TYPE)) {
                            String className = JavaExternalObjectType.localNameToClassName(classQName.getLocalPart());
                            Configuration config = loader.getConfiguration();
                            Class<?> resultClass = config.getClass(className, false);
                            JavaExternalObjectType jeot = JavaExternalObjectType.of(resultClass);
                            ((JavaExtensionFunctionCall) exp).adjustRequiredType(jeot);
                        }
                    } catch (Exception err) {
                        throw new XPathException("Unable to load type " + declaredType + " needed by an extension function");
                    }
                }
            }
            return exp;
        });
        //#endif

        eMap.put("while", (loader, element) -> {
            loader.needsPELicense("saxon:while");
            Expression lhs = loader.getFirstChildExpression(element);
            Expression rhs = loader.getSecondChildExpression(element);
            return new While(lhs, rhs);
        });

    }


    @Override
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

    @Override
    protected void addVendorFunctionLibrary(FunctionLibraryList targetList, Configuration config) {
        targetList.addFunctionLibrary(((ProfessionalConfiguration)config).getVendorFunctionSet());
    }


}

// Copyright (c) 2018-2023 Saxonica Limited

