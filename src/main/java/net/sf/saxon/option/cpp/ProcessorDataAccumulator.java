package net.sf.saxon.option.cpp;

import net.sf.saxon.s9api.Processor;
import net.sf.saxon.s9api.XdmValue;
import org.graalvm.nativeimage.IsolateThread;
import org.graalvm.nativeimage.ObjectHandle;
import org.graalvm.nativeimage.c.function.CEntryPoint;
import org.graalvm.nativeimage.c.type.CCharPointer;
import org.graalvm.nativeimage.c.type.CTypeConversion;

import java.util.ArrayList;
import java.util.Arrays;

public class ProcessorDataAccumulator {
    private ArrayList<String> paramNames;
    private ArrayList values;

    public ProcessorDataAccumulator(){
        paramNames = new ArrayList<String>();
        values = new ArrayList();
    }

    public ProcessorDataAccumulator(int cap){
        paramNames = new ArrayList<String>(cap);
        values = new ArrayList(cap);
    }


    public ProcessorDataAccumulator(XdmValue[] ivalues){
        values =  new ArrayList<XdmValue>(Arrays.asList(ivalues));

    }

    @CEntryPoint(name = "createProcessorDataWithArray")
    public static long createProcessorDataWithArray(IsolateThread thread, ObjectHandle arrayRef){

        
        ProcessorDataAccumulator processorData = new ProcessorDataAccumulator();
        ObjectHandle handle = SaxonCAPI.createHandle(processorData);
        return handle.rawValue();
    }


    @CEntryPoint(name = "createProcessorData")
    public static long createProcessorData(IsolateThread thread){
        ProcessorDataAccumulator processorData = new ProcessorDataAccumulator();
        ObjectHandle handle = SaxonCAPI.createHandle(processorData);
        return handle.rawValue();
    }


    @CEntryPoint(name = "createProcessorDataWithCapacity")
    public static long createProcessorDataWithCapacity(IsolateThread thread, int cap){
        ProcessorDataAccumulator processorData = new ProcessorDataAccumulator(cap);
        ObjectHandle handle = SaxonCAPI.createHandle(processorData);
        return handle.rawValue();
    }


    @CEntryPoint(name = "addProcessorDataPair")
    public static void addProcessorDataPair(IsolateThread thread, ObjectHandle processorDataRef, CCharPointer cparamName, ObjectHandle objectRef){

        ProcessorDataAccumulator processorData = SaxonCAPI.objectHandles.get(processorDataRef);
        String paramNamei = CTypeConversion.toJavaString(cparamName);
        Object object = SaxonCAPI.objectHandles.get(objectRef);

        processorData.paramNames.add(paramNamei);
        processorData.values.add(object);

    }



    @CEntryPoint(name = "addProcessorPropertyPair")
    public static void addProcessorPropertyPair(IsolateThread thread, ObjectHandle processorDataRef, CCharPointer cpropName, CCharPointer cpropvalue){

        ProcessorDataAccumulator processorData = SaxonCAPI.objectHandles.get(processorDataRef);
        String paramNamei = CTypeConversion.toJavaString(cpropName);
        String object = CTypeConversion.toJavaString(cpropvalue);

        processorData.paramNames.add(paramNamei);
        processorData.values.add(object);

    }


    @CEntryPoint(name = "addProcessorValue")
    public static void addProcessorValue(IsolateThread thread, ObjectHandle processorDataRef, ObjectHandle objectRef){

        ProcessorDataAccumulator processorData = SaxonCAPI.objectHandles.get(processorDataRef);
        Object object = SaxonCAPI.objectHandles.get(objectRef);

        processorData.values.add(object);

    }


    @CEntryPoint(name = "addProcessorProperty")
    public static void addProcessorProperty(IsolateThread thread, ObjectHandle processorDataRef, CCharPointer cparamName){

        ProcessorDataAccumulator processorData = SaxonCAPI.objectHandles.get(processorDataRef);
        String paramNamei = CTypeConversion.toJavaString(cparamName);

        processorData.paramNames.add(paramNamei);

    }


    @CEntryPoint(name = "clearProcessorData")
    public static void clearProcessorDataAccumulator(IsolateThread thread, ObjectHandle processorDataRef){
        ProcessorDataAccumulator processorData = SaxonCAPI.objectHandles.get(processorDataRef);
        processorData.paramNames.clear();
        processorData.values.clear();

    }


    public String [] getParameterNames() {
        return paramNames.toArray(new String[paramNames.size()]);

    }

    public Object [] getValues() {
        return values.toArray();

    }

    public int size(){
        return values.size();
    }







}
