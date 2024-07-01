////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) 2018-2023 Saxonica Limited
// This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
// If a copy of the MPL was not distributed with this file, You can obtain one at http://mozilla.org/MPL/2.0/.
// This Source Code Form is "Incompatible With Secondary Licenses", as defined by the Mozilla Public License, v. 2.0.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

package net.sf.saxon.option.cpp;
import net.sf.saxon.lib.ResourceRequest;
import net.sf.saxon.s9api.*;
import org.graalvm.nativeimage.IsolateThread;
import org.graalvm.nativeimage.ObjectHandle;
import org.graalvm.nativeimage.c.function.CEntryPoint;
import org.graalvm.nativeimage.c.type.CCharPointer;
import org.graalvm.nativeimage.c.type.CLongPointer;
import org.graalvm.word.WordFactory;

import javax.xml.transform.Source;
import javax.xml.transform.SourceLocator;
import javax.xml.transform.stream.StreamSource;
import java.io.*;
import java.util.ArrayList;
import java.util.List;



/**
 * This is the SaxonC Message listener implementation of the Saxon MessageListener2 interface
 * Here we capture the xsl:message instructions to a file name, XdmNode array object or totally ignore them
 */
public class SaxonCMessageListener implements MessageListener2 {
    int mode = 0;
    String messageFilename = null;
    File sourceFile = null;
    List<XdmNode> xslMessages = new ArrayList<>();
    Processor processor = null;

    /**
     *  Constructor class to setup the message listener for the Xslt30 Processor in SaxonC
     * @param proc - Saxon Processor object
     * @param cwd - Current working directory
     * @param mode -  option for switching on/off/filename xsl:message
     * @throws SaxonApiException if cannot resolve file to source
     */
    public SaxonCMessageListener(Processor proc, String cwd, String mode) throws SaxonApiException {
        processor = proc;
        if (mode.equals("-:on")) {
            if(SaxonCAPI.debug) {
                System.err.println("SaxonCMessageListener mode = 1");
            }
            this.mode = 1;
        } else if (mode.equals("-:off")) {
            if(SaxonCAPI.debug) {
                System.err.println("SaxonCMessageListener mode = 0");
            }
            this.mode = 0;
        }  else {
            this.mode = 2;
            messageFilename = mode;
            sourceFile = SaxonCAPI.resolveFile(processor, cwd, messageFilename, ResourceRequest.XML_NATURE);
            if(SaxonCAPI.debug) {
                System.err.println("SaxonCMessageListener mode = 2, filename = " + messageFilename + " resolved filename = " + sourceFile.getAbsolutePath());
            }
        }
    }

    /**
     *  create SaxonCMessageListener with reference to object held in the Graalvm ObjectHandles pool.
     *  This methods calls the constructor class
     * @param thread - Graalvm runtime data structure for the thread
     * @param procRef - reference to the Processor object in the ObjectHandles pool
     * @param ccwd  - Current working directory
     * @param c_mode - option for switching on/off/filename xsl:message
     * @return  - long value to the created SaxonCMessageListener object in the ObjectHandles pool
     * @throws SaxonApiException failure in the Message listener
     */
    @CEntryPoint(name = "createSaxonCMessageListener", exceptionHandler = SaxonCExceptionLongHandler.class)
    public static long createSaxonCMessageListener(IsolateThread thread, ObjectHandle procRef, CCharPointer ccwd, CCharPointer c_mode) throws SaxonApiException {
        Processor processor = SaxonCAPI.objectHandles.get(procRef);

        String cwd = null;
        if(ccwd != WordFactory.nullPointer()) {
            cwd =   SaxonCAPI.toJavaString(ccwd, WordFactory.nullPointer());
        }
        String mode = SaxonCAPI.toJavaString(c_mode, WordFactory.nullPointer());

        SaxonCMessageListener listener = new SaxonCMessageListener(processor, cwd, mode);

        ObjectHandle handle = SaxonCAPI.createHandle(listener);

        return handle.rawValue();
    }

    /**
     * Notify a message written using the <code>xsl:message</code> instruction
     * @param content   a document node representing the message content. Note that the output of
     *                  <code>xsl:message</code> is always an XML document node. It can be flattened to obtain the
     *                  string value if required by calling <code>getStringValue()</code>.
     * @param errorCode a QName containing the error code supplied to the call on xsl:message, or
     *                  its default of err:XTMM9000.
     * @param terminate Set to true if <code>terminate="yes"</code> was specified or to false otherwise.
     *                  The message listener does not need to take any special action based on this parameter, but the information
     *                  is available if required. If <code>terminate="yes"</code> was specified, then the transformation will abort
     *                  with an exception immediately on return from this callback.
     * @param locator   an object that contains the location of the <code>xsl:message</code> instruction in the
     *                  stylesheet that caused this message to be output. This provides access to the URI of the stylesheet module
     */
    @Override
    public void message(XdmNode content, QName errorCode, boolean terminate, SourceLocator locator) {
        if(mode == 0 ) {
            return;
        } else if (mode == 1) {
            xslMessages.add(content);
            System.err.println(content.getStringValue());
        } else if (mode == 2) {
            xslMessages.add(content);
            try (FileWriter fw = new FileWriter(sourceFile, true); //TODO change this to use Source
                 BufferedWriter bw = new BufferedWriter(fw);
                 PrintWriter out = new PrintWriter(bw)) {
                out.println(content.getStringValue());
            } catch (IOException e) {
                System.err.println("Error could not to file:" + sourceFile.getAbsolutePath() + " the xsl:message=" + content.getStringValue());
            }
        }
    }

    /**
     * Get the xsl:messages  as an array of XdmNode objects in the C memory space
     * @param thread - Graalvm runtime data structure for the thread
     * @param alloc  - Allocator function to create C memory space passed from the C++ application
     * @param messageListenerRef - reference to the SaxonCMessageListener object in the ObjectHandles pool
     * @return pointer to the array of XdmNode objects in the C memory space
     */
    @CEntryPoint(name = "j_getXslMessages")
    public static CLongPointer getXslMessages(IsolateThread thread, SaxonCAPI.AllocatorFn alloc, ObjectHandle messageListenerRef) {
        SaxonCMessageListener messageListener = SaxonCAPI.objectHandles.get(messageListenerRef);
        List<XdmNode> xslMessages = messageListener.xslMessages;
        if(SaxonCAPI.debug) {
            System.err.println("SaxonCMessageListener getXslMessage size = " + messageListener.xslMessages.size());
        }
        if( messageListener.xslMessages.size() == 0 ) {
            return WordFactory.nullPointer();
        }
            long[] refs= xslMessages.stream().mapToLong( i ->{
                        ObjectHandle objectHandle = SaxonCAPI.createHandle(i);
                        return objectHandle.rawValue();
                    }).toArray();
            CLongPointer addr = SaxonCAPI.longArrayToNativeWithLength(alloc,refs);
            return addr;
        
    }


    /**
     * Clear the xsl:messages  held in the listener
     * @param thread - Graalvm runtime data structure for the thread
     * @param messageListenerRef - reference to the SaxonCMessageListener object in the ObjectHandles pool
     */
    @CEntryPoint(name = "j_clearXslMessages")
    public static void  clearXslMessages(IsolateThread thread, ObjectHandle messageListenerRef) {
        SaxonCMessageListener messageListener = SaxonCAPI.objectHandles.get(messageListenerRef);
        if(SaxonCAPI.debug) {
                    System.err.println("Before clear - SaxonCMessageListener getXslMessage size = " + messageListener.xslMessages.size());
                }
        messageListener.xslMessages.clear();
        if(SaxonCAPI.debug) {
            System.err.println("After clear - SaxonCMessageListener getXslMessage size = " + messageListener.xslMessages.size());
        }

    }
}