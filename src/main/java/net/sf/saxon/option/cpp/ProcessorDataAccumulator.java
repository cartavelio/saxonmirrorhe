package net.sf.saxon.option.cpp;

import org.graalvm.nativeimage.IsolateThread;
import org.graalvm.nativeimage.ObjectHandle;
import org.graalvm.nativeimage.c.function.CEntryPoint;
import org.graalvm.nativeimage.c.type.CCharPointer;
import org.graalvm.nativeimage.c.type.CTypeConversion;

import java.util.ArrayList;

public class ProcessorDataAccumulator {
    private ArrayList<String> paramNames;
    private ArrayList values;

    public ProcessorDataAccumulator(){
        paramNames = new ArrayList<String>();
        values = new ArrayList();
    }


    @CEntryPoint(name = "createProcessorData")
    public static ProcessorDataAccumulator createProcessorData(IsolateThread thread){
        ProcessorDataAccumulator processorData = new ProcessorDataAccumulator();

        return processorData;
    }


    @CEntryPoint(name = "addProcessorDataPair")
    public static void addProcessorData(IsolateThread thread, ObjectHandle processorDataRef, CCharPointer cparamName, ObjectHandle objectRef){

        ProcessorDataAccumulator processorData = SaxonCAPI.objectHandles.get(processorDataRef);
        String paramNamei = CTypeConversion.toJavaString(cparamName);
        Object object = SaxonCAPI.objectHandles.get(objectRef);

        processorData.paramNames.add(paramNamei);
        processorData.values.add(object);

    }


    @CEntryPoint(name = "addProcessorValue")
    public static void addProcessorValue(IsolateThread thread, ObjectHandle processorDataRef, ObjectHandle objectRef){

        ProcessorDataAccumulator processorData = SaxonCAPI.objectHandles.get(processorDataRef);
        Object object = SaxonCAPI.objectHandles.get(objectRef);

        processorData.values.add(object);

    }


    @CEntryPoint(name = "clearProcessorData")
    public static void clearProcessorDataAccumulator(IsolateThread thread, ObjectHandle processorDataRef){
        ProcessorDataAccumulator processorData = SaxonCAPI.objectHandles.get(processorDataRef);
        processorData.paramNames.clear();
        processorData.values.clear();

    }


    public String [] getParameterNames() {
        return (String[]) paramNames.toArray();

    }

    public Object [] getValues() {
        return values.toArray();

    }







}
