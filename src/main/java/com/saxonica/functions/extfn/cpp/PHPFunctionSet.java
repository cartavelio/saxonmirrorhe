package com.saxonica.functions.extfn.cpp;


import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.functions.SystemFunction;
import net.sf.saxon.functions.registry.BuiltInFunctionSet;
import net.sf.saxon.ma.arrays.ArrayItemType;
import net.sf.saxon.ma.arrays.SimpleArrayItem;
import net.sf.saxon.om.NamespaceUri;
import net.sf.saxon.om.NodeInfo;
import net.sf.saxon.om.Sequence;
import net.sf.saxon.om.SequenceTool;
import net.sf.saxon.option.cpp.SaxonCAPI;
import net.sf.saxon.option.cpp.XdmValueForCpp;
import net.sf.saxon.s9api.XdmItem;
import net.sf.saxon.s9api.XdmValue;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.type.AnyItemType;
import net.sf.saxon.type.BuiltInAtomicType;
import net.sf.saxon.type.TypeHierarchy;
import net.sf.saxon.value.*;

import java.util.ArrayList;
import java.util.List;

/**
 * The PHPFunctionSet represents additions to the set of Saxon vendor-supplied
 * extension functions that are available on Saxon-PE/C and Saxon-EE/C
 */
public class PHPFunctionSet extends BuiltInFunctionSet {

    private static final PHPFunctionSet THE_INSTANCE = new PHPFunctionSet();

    private PHPFunctionSet() {
        init();
    }

    public static PHPFunctionSet getInstance() {
        return THE_INSTANCE;
    }

    private void init() {
        register("function", 2, e -> e.populate(PHPFunction::new, AnyItemType.getInstance(), OPT, SIDE | LATE)
                .arg(0, BuiltInAtomicType.STRING, ONE, null)
                .arg(1, ArrayItemType.getInstance(), STAR, null));

    }

    @Override
    public NamespaceUri getNamespace() {
        return NamespaceUri.PHP;
    }


    public static class PHPFunction extends SystemFunction {

        // Declare a native method _native()
        public native Object _phpCall(String name, Object[] args, String[] types);

        @Override
        public Sequence call(XPathContext context, Sequence[] args) throws XPathException {
            String phpFuncName = args[0].head().getStringValue();
            int len = args.length;
            Object[] funcArgs = null;
            String[] funcArgTypes = null;
            if (SaxonCAPI.debug) {
                System.err.println("PHPCall.call: function name: " + phpFuncName + " Argument length: " + len);
            }
            SimpleArrayItem seqArray = (SimpleArrayItem) args[1].materialize();
            int seqLength = seqArray.arrayLength();


            funcArgs = new Object[seqLength];
            funcArgTypes = new String[seqLength];
            Sequence seq = null;

            for (int i = 0; i < seqLength; i++) {
                seq = seqArray.get(i);
                if (SequenceTool.getLength(seq) == 1) {
                    if (seq instanceof NodeInfo) {
                        funcArgTypes[i] = "node";
                        funcArgs[i] = XdmValue.wrap(seq);
                    } else if (seq instanceof AtomicValue) {
                        AtomicValue value = (AtomicValue) seq;
                        BuiltInAtomicType operandType = value.getPrimitiveType();
                        TypeHierarchy th = context.getConfiguration().getTypeHierarchy();
                        boolean isInteger = th.isSubType(operandType, BuiltInAtomicType.INTEGER);
                        if (th.isSubType(operandType, BuiltInAtomicType.STRING)) {
                            funcArgTypes[i] = "string";
                            funcArgs[i] = value.getUnicodeStringValue();
                        } else if (isInteger) {
                            funcArgTypes[i] = "integer";
                            funcArgs[i] = (int) ((IntegerValue) value).longValue();
                        } else if (th.isSubType(operandType, BuiltInAtomicType.DOUBLE)) {
                            funcArgTypes[i] = "double";
                            funcArgs[i] = ((DoubleValue) value).getDoubleValue();
                        } else if (th.isSubType(operandType, BuiltInAtomicType.LONG)) {
                            funcArgTypes[i] = "long";
                            funcArgs[i] = ((Int64Value) value).longValue();
                        } else if (th.isSubType(operandType, BuiltInAtomicType.FLOAT)) {
                            funcArgTypes[i] = "float";
                            funcArgs[i] = ((FloatValue) value).getFloatValue();
                        } else if (th.isSubType(operandType, BuiltInAtomicType.DECIMAL)) {
                            funcArgTypes[i - 1] = "double";
                            funcArgs[i - 1] = ((DecimalValue) value).getDoubleValue();
                        } else if (th.isSubType(operandType, BuiltInAtomicType.BOOLEAN)) {
                            funcArgTypes[i - 1] = "boolean";
                            funcArgs[i - 1] = ((BooleanValue) value).getBooleanValue();
                        }
                    } else {

                        funcArgs[i] = XdmValueForCpp.makeArrayFromXdmValue(XdmValue.wrap(seq));
                        funcArgTypes[i] = "[xdmvalue";

                    }
                }

            }

            if (SaxonCAPI.debug) {
                System.err.println("Phpcall.call: Before call to _phpCall ");
            }
            Object obj = _phpCall(phpFuncName, funcArgs, funcArgTypes);
            if (SaxonCAPI.debug) {
                System.err.println("Phpcall.call: After call to _phpCall ");
            }
            if (obj instanceof Object[]) {
                Object[] objects = (Object[]) obj;
                List<XdmItem> nodeList = new ArrayList<XdmItem>(objects.length);

                for (int i = 0; i < objects.length; i++) {
                    nodeList.add((XdmItem) XdmItem.wrap(NativeCall.convertValueObject(obj, phpFuncName)));
                }
                XdmValue values = new XdmValue(nodeList);
                return values.getUnderlyingValue();

            } else {
                return NativeCall.convertValueObject(obj, phpFuncName);
            }
        }
    }


}

// Copyright (c) 2020-2023 Saxonica Limited
