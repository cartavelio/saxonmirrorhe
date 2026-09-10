// //////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) 2026 Saxonica Limited
// This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
// If a copy of the MPL was not distributed with this file, You can obtain one at http://mozilla.org/MPL/2.0/.
// This Source Code Form is “Incompatible With Secondary Licenses”, as defined by the Mozilla Public License, v. 2.0.
// //////////////////////////////////////////////////////////////////////////////////////////////////////////////////

package net.sf.saxon.option.cpp;

import net.sf.saxon.lib.StandardErrorReporter;
import net.sf.saxon.lib.StandardLogger;
import net.sf.saxon.s9api.XmlProcessingError;

import java.io.PrintStream;
import java.io.StringWriter;

public class SaxonCErrorReporter  extends StandardErrorReporter {

    StringWriter errors;

    public SaxonCErrorReporter(){
        errors = new StringWriter();
        setLogger(StandardLogger.makeLogger(errors));
        /*
        TODO this won't work because its static
        if(SaxonCAPI.standardErrorOutputStream != null){
            setStandardErrorOutput(SaxonCAPI.standardErrorOutputStream);
        } */
    }

    @Override
    public void report(XmlProcessingError error) {
        super.report(error);
        if(SaxonCAPI.debug) {
            System.err.println("SaxonCErrorReporter.report current error=" + error.getMessage());

            System.err.println("All error SaxonCErrorReporter.report=" + errors.toString());
            System.err.println("logger: " + errors.getClass().hashCode());
        }
    }

    public void setStandardErrorOutput(PrintStream out) {
        setLogger(StandardLogger.makeLogger(out));
    }

    public StringWriter getErrors() {
        return errors;
    }

    public void reset(){
        errors = new StringWriter();
        setLogger(StandardLogger.makeLogger(errors));
    }

}
