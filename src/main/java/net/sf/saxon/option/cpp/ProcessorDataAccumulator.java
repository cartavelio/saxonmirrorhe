////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) 2023 Saxonica Limited
// This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
// If a copy of the MPL was not distributed with this file, You can obtain one at http://mozilla.org/MPL/2.0/.
// This Source Code Form is "Incompatible With Secondary Licenses", as defined by the Mozilla Public License, v. 2.0.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

package net.sf.saxon.option.cpp;

import net.sf.saxon.s9api.SaxonApiException;
import net.sf.saxon.s9api.XdmValue;
import org.graalvm.nativeimage.IsolateThread;
import org.graalvm.nativeimage.ObjectHandle;
import org.graalvm.nativeimage.c.function.CEntryPoint;
import org.graalvm.nativeimage.c.type.CCharPointer;
import org.graalvm.word.WordFactory;

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

    public ProcessorDataAccumulator(String [] props){
        paramNames = new ArrayList<String>(Arrays.asList(props));
    }

    public ProcessorDataAccumulator(int cap, boolean propsOnly){

        paramNames = new ArrayList<String>(cap);
    }


    public ProcessorDataAccumulator(XdmValue[] ivalues){
        values =  new ArrayList<XdmValue>(Arrays.asList(ivalues));

    }

    public void addProperty(String name){
         paramNames.add(name);
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
        String paramNamei = null;
        try {
            paramNamei = SaxonCAPI.toJavaString(cparamName, WordFactory.nullPointer());
        } catch(SaxonApiException ex) {
            if(SaxonCAPI.debug) {
                System.err.println(ex.getMessage());
            }
            return;
        }
        Object object = SaxonCAPI.objectHandles.get(objectRef);

        if(SaxonCAPI.debug) {
            System.err.println("ProcessorDataAccumulator - data-pair - addProcessor paramName= " + paramNamei);
            System.err.println("ProcessorDataAccumulator - data-pair - addProcessorValue= " +object.getClass().getName());
        }

        processorData.paramNames.add(paramNamei);
        processorData.values.add(object);

    }



    @CEntryPoint(name = "addProcessorPropertyPair")
    public static void addProcessorPropertyPair(IsolateThread thread, ObjectHandle processorDataRef, CCharPointer cpropName, CCharPointer cpropvalue){

        ProcessorDataAccumulator processorData = SaxonCAPI.objectHandles.get(processorDataRef);
        String paramNamei = null;
        String object = null;

        try {
            paramNamei = SaxonCAPI.toJavaString(cpropName, WordFactory.nullPointer());
            object = SaxonCAPI.toJavaString(cpropvalue, WordFactory.nullPointer());
        } catch(SaxonApiException ex) {
            if(SaxonCAPI.debug) {
                System.err.println(ex.getMessage());
            }
            return;
        }

        if(SaxonCAPI.debug) {
            System.err.println("ProcessorDataAccumulator - pair - addProcessor paramName= " + paramNamei);
            System.err.println("ProcessorDataAccumulator - pair - addProcessorValue= " +object);
        }

        processorData.paramNames.add(paramNamei);
        processorData.values.add(object);

    }


    @CEntryPoint(name = "addProcessorValue")
    public static void addProcessorValue(IsolateThread thread, ObjectHandle processorDataRef, ObjectHandle objectRef){

        ProcessorDataAccumulator processorData = SaxonCAPI.objectHandles.get(processorDataRef);
        Object object = SaxonCAPI.objectHandles.get(objectRef);

        if(SaxonCAPI.debug) {
            System.err.println("ProcessorDataAccumulator - addProcessorValue type= " + object.getClass().getName());
        }

        processorData.values.add(object);

    }

    @CEntryPoint(name = "j_getProcessorDataProperty", exceptionHandler = SaxonCExceptionCCharHandler.class)
    public static CCharPointer j_getProcessorDataProperty(IsolateThread thread, SaxonCAPI.AllocatorCCharFn alloc, ObjectHandle processorDataRef, int index){

        ProcessorDataAccumulator processorData = SaxonCAPI.objectHandles.get(processorDataRef);
        String data = processorData.paramNames.get(index);
        CCharPointer a = SaxonCAPI.stringToNative2(alloc, data);
        return a;

    }


    @CEntryPoint(name = "addProcessorProperty")
    public static void addProcessorProperty(IsolateThread thread, ObjectHandle processorDataRef, CCharPointer cparamName){

        ProcessorDataAccumulator processorData = SaxonCAPI.objectHandles.get(processorDataRef);
        String paramNamei = null;
        try {
            paramNamei = SaxonCAPI.toJavaString(cparamName, WordFactory.nullPointer());
        } catch(SaxonApiException ex) {
            if(SaxonCAPI.debug) {
                System.err.println(ex.getMessage());
            }
            return;
        }

        if(SaxonCAPI.debug) {
            System.err.println("ProcessorDataAccumulator - addProcessorValue paramNamei= " + paramNamei);
        }

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
