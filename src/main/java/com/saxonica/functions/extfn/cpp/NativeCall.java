package com.saxonica.functions.extfn.cpp;

import net.sf.saxon.om.Sequence;
import net.sf.saxon.option.cpp.ProcessorDataAccumulator;
import net.sf.saxon.option.cpp.SaxonCAPI;
import net.sf.saxon.option.cpp.XdmValueForCpp;
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
    private CFunctionPointer pythonProc;
    SaxonCAPI.AllocatorFn alloc;
    private boolean useNativeTypes;
    private XdmValue returnValue;



    public NativeCall(QName qn) {
        this.name = qn;
    }

    public NativeCall(QName funcQname, SequenceType[] sequenceTypes, SequenceType returnSequenceType, CFunctionPointer pythonProc, CFunctionPointer func, SaxonCAPI.AllocatorFn alloc, XdmValue returnVal, boolean useNativeTypes) {
        this.name = funcQname;
        this.sequenceType = sequenceTypes;
        this.returnType = returnSequenceType;
        this.func = func;
        this.alloc = alloc;
        this.pythonProc = pythonProc;
        this.useNativeTypes = useNativeTypes;
        this.returnValue = returnVal;

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
    @CFunction
    public static native ObjectHandle c_nativeCall(CCharPointer c_funcName, CFunctionPointer proc,  CFunctionPointer func, ObjectHandle returnValueRef, int occurrenceValue, int useNativeTypes, int length, CLongPointer addr);


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


        String nameStr = name.getLocalName();
        if(SaxonCAPI.debug) {
            System.err.println("Java SaxonC Debug: in the call method");
            System.err.println("Java SaxonC function name="+nameStr);
        }

        try(CTypeConversion.CCharPointerHolder holder = CTypeConversion.toCString(nameStr)) {

            int occurrenceValue = returnType.getOccurrenceIndicator().ordinal();

            long[] argumentRefs = null;
            CLongPointer addr = WordFactory.nullPointer();
            if(arguments.length > 0) {
                argumentRefs = Arrays.stream(arguments).mapToLong((i -> {
                    ObjectHandle objectHandle = SaxonCAPI.createHandle(i);
                    return objectHandle.rawValue();
                })).toArray();

                addr = SaxonCAPI.longArrayToNative(alloc, argumentRefs);
            }

            if(SaxonCAPI.debug) {
                System.err.println("Java SaxonC Debug NativeCall.call= " + arguments.length);

                System.err.println("Java SaxonC return OccurrenceIndicator="+returnType.getOccurrenceIndicator().toString());
                System.err.println("Java SaxonC return OccurrenceIndicator cardinality="+occurrenceValue);
                QName returnTypeQName = returnType.getItemType().getTypeName();
                if(returnTypeQName != null) {
                    System.err.println("Java SaxonC return TypeName=" + returnTypeQName);
                } else {
                    System.err.println("Java SaxonC return type is null");
                    returnType.getItemType().getUnderlyingItemType();

                }

                if (useNativeTypes) {
                    System.err.println("Java SaxonC Debug NativeCall.call useNativeType is True ");

                } else {
                    System.err.println("Java SaxonC Debug NativeCall.call useNativeType is False ");
                }
            }

            ObjectHandle returnValueRef = SaxonCAPI.createHandle(returnValue);




            ObjectHandle valueRef = c_nativeCall(holder.get(), pythonProc, func, returnValueRef, occurrenceValue, (useNativeTypes? 1 : 0), arguments.length, addr); // pass arguments as an array

             // get info on the return type
            if (valueRef != WordFactory.nullPointer()) {
                Object obj = SaxonCAPI.objectHandles.get(valueRef);

                XdmValue xdmValue = SaxonCAPI.convertObjectToXdmValue(obj);

                if (xdmValue != null) {
                    return xdmValue;

                }

            }
        }

        return XdmEmptySequence.getInstance();
    }
}

// Copyright (c) 2020-2024 Saxonica Limited


