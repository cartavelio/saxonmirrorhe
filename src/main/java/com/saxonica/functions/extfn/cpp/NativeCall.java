package com.saxonica.functions.extfn.cpp;

import net.sf.saxon.Version;
import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.functions.SystemFunction;
import net.sf.saxon.java.JavaPlatform;
import net.sf.saxon.ma.arrays.SimpleArrayItem;
import net.sf.saxon.om.*;
import net.sf.saxon.option.cpp.NativePlatform;
import net.sf.saxon.option.cpp.SaxonCAPI;
import net.sf.saxon.option.cpp.XdmValueForCpp;
import net.sf.saxon.s9api.XdmItem;
import net.sf.saxon.s9api.XdmNode;
import net.sf.saxon.s9api.XdmValue;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.type.BuiltInAtomicType;
import net.sf.saxon.type.TypeHierarchy;
import net.sf.saxon.value.*;

import java.util.ArrayList;
import java.util.List;

/**
 * Class to call Native functions under C/C++
 */
public class NativeCall extends SystemFunction {

    /**
     * detect the operating system from the os.name System property and cache
     * the result
     *
     * @return the operating system detected
     */
    public static String getLibraryExtension() {
        String extension = "";
        String OS = System.getProperty("os.name", "generic").toLowerCase();
        if ((OS.indexOf("mac") >= 0) || (OS.indexOf("darwin") >= 0)) {
            extension = "dylib";
        } else if (OS.indexOf("win") >= 0) {
            extension = "dll";
        } else if (OS.indexOf("nux") >= 0) {
            extension = "so";
        } else {
            extension = "so";
        }
        return extension;
    }

    public static void loadLibrary(String cwd, String libraryName) {
        String home = (cwd != null ? cwd : Version.platform.getInstallationDirectory(Version.softwareEdition, null));

        String libName = (home == null  || home.isEmpty() ? "" : home + "/") + libraryName + "." + getLibraryExtension();
        try {
            //System.out.println("Library Name=. " + libName);
            //System.err.println("Err.println Library Name=. " + libName);
            System.load(libName); // Load native library at runtime
        } catch (UnsatisfiedLinkError ex) {
            System.err.println("Native code library failed to load. " + libName + "\n" + ex);
        }
        //System.loadLibrary("saxoneec");


    }


    @Override
    public Sequence call(XPathContext context, Sequence[] args) throws XPathException {
        String phpFuncName = args[0].head().getStringValue();
        int len = args.length;
        Object[] funcArgs = null;
        String[] funcArgTypes = null;
        if (SaxonCAPI.debug) {
            System.err.println("NativeCall.call: function name: " + phpFuncName + " Argument length: " + len);
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
                        funcArgs[i] = new Integer((int) ((IntegerValue) value).longValue());
                    } else if (th.isSubType(operandType, BuiltInAtomicType.DOUBLE)) {
                        funcArgTypes[i] = "double";
                        funcArgs[i] = new Double(((DoubleValue) value).getDoubleValue());
                    } else if (th.isSubType(operandType, BuiltInAtomicType.LONG)) {
                        funcArgTypes[i] = "long";
                        funcArgs[i] = new Long(((Int64Value) value).longValue());
                    } else if (th.isSubType(operandType, BuiltInAtomicType.FLOAT)) {
                        funcArgTypes[i] = "float";
                        funcArgs[i] = new Float(((FloatValue) value).getFloatValue());

                    } else if ((!isInteger) && th.isSubType(operandType, BuiltInAtomicType.DECIMAL)) {
                        funcArgTypes[i - 1] = "double";
                        funcArgs[i - 1] = new Double(((DecimalValue) value).getDoubleValue());
                    } else if (th.isSubType(operandType, BuiltInAtomicType.BOOLEAN)) {
                        funcArgTypes[i - 1] = "boolean";
                        funcArgs[i - 1] = new Boolean(((BooleanValue) value).getBooleanValue());
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
        Object obj = _nativeCall(phpFuncName, funcArgs, funcArgTypes);
        if (SaxonCAPI.debug) {
            System.err.println("Phpcall.call: After call to _phpCall ");
        }
        if (obj instanceof Object[]) {
            Object[] objects = (Object[]) obj;
            List<XdmItem> nodeList = new ArrayList<XdmItem>(objects.length);

            for (int i = 0; i < objects.length; i++) {
                nodeList.add((XdmItem) XdmItem.wrap(convertValueObject(obj, phpFuncName)));
            }
            XdmValue values = new XdmValue(nodeList);
            return values.getUnderlyingValue();

        } else {
            return convertValueObject(obj, phpFuncName);
        }

    }

    public static Sequence convertValueObject(Object obj, String phpFuncName) throws XPathException {
        if (obj instanceof String) {
            return new StringValue((String) obj);
        } else if (obj instanceof Integer) {
            return new Int64Value(((Integer) obj).intValue());
        } else if (obj instanceof Double) {
            return new DoubleValue((Double) obj);
        } else if (obj instanceof Boolean) {
            return BooleanValue.get((Boolean) obj);
        } else if (obj instanceof Long) {
            return new Int64Value(((Long) obj).intValue());

        } else if (obj instanceof XdmNode) {
            return ((XdmNode) obj).getUnderlyingValue();

        } else if (obj == null) {
            throw new XPathException("NULL returned from php:function:" + phpFuncName + ". Maybe function does not exist");
        }
        throw new XPathException("Unknown return type from php:function: " + phpFuncName + ".");

    }


    // Declare a native method _native()
    public native Object _nativeCall(String name, Object[] args, String[] types);


}

// Copyright (c) 2020-2022 Saxonica Limited


