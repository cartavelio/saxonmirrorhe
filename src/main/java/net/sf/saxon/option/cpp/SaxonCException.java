////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) 2014 Saxonica Limited.
// This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
// If a copy of the MPL was not distributed with this file, You can obtain one at http://mozilla.org/MPL/2.0/.
// This Source Code Form is "Incompatible With Secondary Licenses", as defined by the Mozilla Public License, v. 2.0.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

package net.sf.saxon.option.cpp;

import net.sf.saxon.s9api.SaxonApiException;
import org.graalvm.nativeimage.IsolateThread;
import org.graalvm.nativeimage.ObjectHandle;
import org.graalvm.nativeimage.c.function.CEntryPoint;

/**
 * * This class is to use with Saxon on C++
 */
public class SaxonCException {
    private SaxonApiException saxonException = null;

    public SaxonCException(Throwable cause) {
        if(cause instanceof SaxonApiException) {
            this.saxonException = (SaxonApiException) cause;
        }  else {
            saxonException = new SaxonApiException(cause);
        }

    }

   /*public static SaxonCException makeSaxonCException(Throwable cause){
        SaxonCException cException = new SaxonCException(cause);
        if(cause instanceof XPathException) {
            StructuredQName qname = ((XPathException) cause).getErrorCodeQName();
              if(qname != null) {
                  cException.set

              }
        }
    }    */



    /**
     * Create a SaxonApiException
     *
     * @param message the message
     */

    public SaxonCException(String message) {
        saxonException = new SaxonApiException(message);

    }





    public String getErrorCodeString(){

            return "";


    }

    public String getErrorMessage(){
        if(saxonException != null) {
            return saxonException.getMessage();
        }
        return "";
    }

    @CEntryPoint(name = "j_getErrorMessage")
    public static String j_getErrorMessage(IsolateThread thread, ObjectHandle exceptionRef){
        SaxonCException exception = SaxonCAPI.objectHandles.get(exceptionRef);
        return exception.getErrorMessage();
    }

    /*public int getLinenumber(){
        return linenumber;
    }

    public boolean isGlobalError(){
        return isGlobalError;
    }

     public boolean isTypeError(){
        return isTypeError;
    }

    public boolean isStaticError(){
        return isStaticError;
    }  */
}
