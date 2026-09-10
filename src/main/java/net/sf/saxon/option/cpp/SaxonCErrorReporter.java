package net.sf.saxon.option.cpp;

import net.sf.saxon.lib.StandardErrorReporter;
import net.sf.saxon.lib.StandardLogger;
import net.sf.saxon.s9api.XmlProcessingError;

import java.io.PrintStream;
import java.io.StringWriter;

public class SaxonCErrorReporter  extends StandardErrorReporter {

    static StringWriter errors = new StringWriter();

    public SaxonCErrorReporter(){
        setLogger(new StandardLogger(errors));
    }

    @Override
    public void report(XmlProcessingError error) {
        super.report(error);
        if(SaxonCAPI.debug) {
            System.err.println("SaxonCErrorReporter.report current error=" + error.getMessage());
            System.err.println("SaxonCErrorReporter.report current error=" + errors.toString());

            System.err.println("All error SaxonCErrorReporter.report=" + errors.toString());
            System.err.println("logger: " + errors.getClass().hashCode());
        }
    }

    public void setStandardErrorOutput(PrintStream out) {
        setLogger(new StandardLogger(out));
    }

    public StringWriter getErrors() {
        return errors;
    }

    public void reset(){
        errors = new StringWriter();
    }

}
