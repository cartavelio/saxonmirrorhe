package com.saxonica.functions.extfn.cpp;

import net.sf.saxon.om.Sequence;
import net.sf.saxon.option.cpp.SaxonCAPI;
import net.sf.saxon.s9api.*;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.value.BooleanValue;
import net.sf.saxon.value.DoubleValue;
import net.sf.saxon.value.Int64Value;
import net.sf.saxon.value.StringValue;
import org.graalvm.nativeimage.IsolateThread;
import org.graalvm.nativeimage.ObjectHandle;
import org.graalvm.nativeimage.c.function.CEntryPoint;
import org.graalvm.nativeimage.c.function.CFunction;
import org.graalvm.nativeimage.c.function.CFunctionPointer;
import org.graalvm.nativeimage.c.type.CCharPointer;
import org.graalvm.nativeimage.c.type.CLongPointer;
import org.graalvm.nativeimage.c.type.CTypeConversion;
import org.graalvm.word.WordFactory;

import java.util.Arrays;


/**
 * Class to call Native functions under C/C++
 */
//@CContext(MyDirectives.class)
public class NativeCall implements ExtensionFunction {

    private SequenceType[] sequenceType = new SequenceType[0];
    private SequenceType returnType = ExtensionFunction.super.getResultType();
    private QName name;
    private CFunctionPointer func;
    SaxonCAPI.AllocatorFn alloc;



    public NativeCall(QName qn) {
        this.name = qn;
    }

    public NativeCall(QName funcQname, SequenceType[] sequenceTypes, SequenceType returnSequenceType, CFunctionPointer func, SaxonCAPI.AllocatorFn alloc) {
        this.name = funcQname;
        this.sequenceType = sequenceTypes;
        this.returnType = returnSequenceType;
        this.func = func;
        this.alloc = alloc;

    }


//    @Override
//    public Sequence call(XPathContext context, Sequence[] args) throws XPathException {
//        String phpFuncName = args[0].head().getStringValue();
//        int len = args.length;
//        Object[] funcArgs = null;
//        String[] funcArgTypes = null;
//        if (SaxonCAPI.debug) {
//            System.err.println("NativeCall.call: function name: " + phpFuncName + " Argument length: " + len);
//        }
//        SimpleArrayItem seqArray = (SimpleArrayItem) args[1].materialize();
//        int seqLength = seqArray.arrayLength();
//        funcArgs = new Object[seqLength];
//        funcArgTypes = new String[seqLength];
//        Sequence seq = null;
//
//        for (int i = 0; i < seqLength; i++) {
//            seq = seqArray.get(i);
//            if (SequenceTool.getLength(seq) == 1) {
//                if (seq instanceof NodeInfo) {
//                    funcArgTypes[i] = "node";
//                    funcArgs[i] = XdmValue.wrap(seq);
//                } else if (seq instanceof AtomicValue) {
//                    AtomicValue value = (AtomicValue) seq;
//                    BuiltInAtomicType operandType = value.getPrimitiveType();
//                    TypeHierarchy th = context.getConfiguration().getTypeHierarchy();
//                    boolean isInteger = th.isSubType(operandType, BuiltInAtomicType.INTEGER);
//                    if (th.isSubType(operandType, BuiltInAtomicType.STRING)) {
//                        funcArgTypes[i] = "string";
//                        funcArgs[i] = value.getUnicodeStringValue();
//                    } else if (isInteger) {
//                        funcArgTypes[i] = "integer";
//                        funcArgs[i] = new Integer((int) ((IntegerValue) value).longValue());
//                    } else if (th.isSubType(operandType, BuiltInAtomicType.DOUBLE)) {
//                        funcArgTypes[i] = "double";
//                        funcArgs[i] = new Double(((DoubleValue) value).getDoubleValue());
//                    } else if (th.isSubType(operandType, BuiltInAtomicType.LONG)) {
//                        funcArgTypes[i] = "long";
//                        funcArgs[i] = new Long(((Int64Value) value).longValue());
//                    } else if (th.isSubType(operandType, BuiltInAtomicType.FLOAT)) {
//                        funcArgTypes[i] = "float";
//                        funcArgs[i] = new Float(((FloatValue) value).getFloatValue());
//
//                    } else if ((!isInteger) && th.isSubType(operandType, BuiltInAtomicType.DECIMAL)) {
//                        funcArgTypes[i - 1] = "double";
//                        funcArgs[i - 1] = new Double(((DecimalValue) value).getDoubleValue());
//                    } else if (th.isSubType(operandType, BuiltInAtomicType.BOOLEAN)) {
//                        funcArgTypes[i - 1] = "boolean";
//                        funcArgs[i - 1] = new Boolean(((BooleanValue) value).getBooleanValue());
//                    }
//                } else {
//
//                    funcArgs[i] = XdmValueForCpp.makeArrayFromXdmValue(XdmValue.wrap(seq));
//                    funcArgTypes[i] = "[xdmvalue";
//
//                }
//            }
//
//        }
//        if (SaxonCAPI.debug) {
//            System.err.println("Phpcall.call: Before call to _phpCall ");
//        }
//        Object obj = null;//_nativeCall(/*phpFuncName, funcArgs, funcArgTypes*/);
//        if (SaxonCAPI.debug) {
//            System.err.println("Phpcall.call: After call to _phpCall ");
//        }
//        if (obj instanceof Object[]) {
//            Object[] objects = (Object[]) obj;
//            List<XdmItem> nodeList = new ArrayList<XdmItem>(objects.length);
//
//            for (int i = 0; i < objects.length; i++) {
//                nodeList.add((XdmItem) XdmItem.wrap(convertValueObject(obj, phpFuncName)));
//            }
//            XdmValue values = new XdmValue(nodeList);
//            return values.getUnderlyingValue();
//
//        } else {
//            return convertValueObject(obj, phpFuncName);
//        }
//
//    }

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

    @CEntryPoint(name = "j_testNativeC")
    public static void testNativeC(IsolateThread thread){
        System.err.println("In the testNativeC method");
        //NativeCall call = new NativeCall();
        //int value = c_nativeCall();
        //System.err.println("cp1 testNativeC method value=" + value);
        System.err.println("exit testNativeC method");
    }


    // Declare a native method _native()
    @CFunction
    public static native ObjectHandle c_nativeCall(CCharPointer c_funcName, CFunctionPointer func, int length, CLongPointer addr);


    @Override
    public QName getName() {
        return name;
    }

    @Override
    public SequenceType getResultType() {
        return returnType;
    }

    @Override
    public SequenceType[] getArgumentTypes() {
        return sequenceType;
    }

    @Override
    public XdmValue call(XdmValue[] arguments) throws SaxonApiException {
        if(SaxonCAPI.debug) {
            System.err.println("Java SaxonC Debug: in the call method");
        }

        String nameStr = name.getEQName();
        try(CTypeConversion.CCharPointerHolder holder = CTypeConversion.toCString(nameStr)) {



            long[] argumentRefs = null;
            CLongPointer addr = WordFactory.nullPointer();
            if(arguments.length > 0) {
                argumentRefs = Arrays.stream(arguments).mapToLong((i -> {
                    ObjectHandle objectHandle = SaxonCAPI.createHandle(i);
                    return objectHandle.rawValue();
                })).toArray();

                addr = SaxonCAPI.longArrayToNative(alloc, argumentRefs);
            }

            System.err.println("Java SaxonC Debug NativeCall.call= " + arguments.length);


            ObjectHandle valueRef = c_nativeCall(holder.get(), func, arguments.length, addr); // pass arguments as an array

             // get info on the return type
            if (valueRef != WordFactory.nullPointer()) {
                XdmAtomicValue xdmValue = SaxonCAPI.objectHandles.get(valueRef);

                if (xdmValue != null) {
                    return xdmValue;

                } else {
                    System.err.println("xdmValue in userFunction is null!!");
                }

            }
        } 
        System.err.println("after c_NativeCall method");

        return XdmEmptySequence.getInstance();
    }
}

// Copyright (c) 2020-2024 Saxonica Limited


