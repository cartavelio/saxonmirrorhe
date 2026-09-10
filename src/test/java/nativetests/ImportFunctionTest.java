////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) 2018-2023 Saxonica Limited
// This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
// If a copy of the MPL was not distributed with this file, You can obtain one at http://mozilla.org/MPL/2.0/.
// This Source Code Form is “Incompatible With Secondary Licenses”, as defined by the Mozilla Public License, v. 2.0.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

package nativetests;

import com.saxonica.config.ProfessionalConfiguration;
import net.sf.saxon.query.QueryModule;
import net.sf.saxon.query.StaticQueryContext;
import net.sf.saxon.query.XQueryExpression;
import net.sf.saxon.query.XQueryFunctionLibrary;
import net.sf.saxon.s9api.*;
import org.junit.jupiter.api.Test;

import java.io.IOException;
import java.io.StringReader;
import java.net.URISyntaxException;
import java.util.Iterator;
import java.util.Vector;

import static org.junit.jupiter.api.Assertions.fail;

public class ImportFunctionTest {
    private static final Processor processor = new Processor(true);

    public static void main(String[] args) throws SaxonApiException, IOException, URISyntaxException {
        ImportFunctionTest main = new ImportFunctionTest();
        main.testImportFunctions();
    }

    @Test
    public void testImportFunctions() throws SaxonApiException, IOException, URISyntaxException {
        try {
            importFunctions();
            Vector<XdmItem> x = evaluateXPath("f:count((1,2,3))");
        } catch (SaxonApiException e) {
            e.printStackTrace();
            fail();
        }
    }

    protected Vector<XdmItem> evaluateXPath(String xpath) throws SaxonApiException {
        XdmNode doc = null;
        Vector<XdmItem> results = new Vector<XdmItem> ();

        XPathCompiler xcomp = processor.newXPathCompiler();
        xcomp.declareNamespace("f", "http://xmlcalabash.com/ns/functions");

        XPathExecutable xexec = xcomp.compile(xpath);
        XPathSelector selector = xexec.load();

        if (doc != null) {
            selector.setContextItem(doc);
        }

        try {
            Iterator<XdmItem> values = selector.iterator();
            while (values.hasNext()) {
                results.add(values.next());
            }
        } catch (SaxonApiUncheckedException saue) {
            throw saue;
        }

        return results;
    }

    private void importFunctions() {
        String ns = "http://xmlcalabash.com/ns/functions";

        try {
            XQueryCompiler xqcomp = processor.newXQueryCompiler();

            StaticQueryContext sqc = xqcomp.getUnderlyingStaticContext();
            sqc.compileLibrary(new StringReader(query));
            XQueryExpression xqe = sqc.compileQuery("import module namespace f='" + ns + "'; .");
            QueryModule qm = xqe.getMainModule();
            XQueryFunctionLibrary xfl = qm.getGlobalFunctionLibrary();
            ((ProfessionalConfiguration)processor.getUnderlyingConfiguration()).setExtensionBinder("calabash", xfl);
        } catch (Exception e) {
            throw new RuntimeException(e);
        }
    }

    public final static String query =
            "xquery version \"1.0\";\n" +
                    "\n" +
                    "module namespace f=\"http://xmlcalabash.com/ns/functions\";\n" +
                    "\n" +
                    "declare default function namespace \"http://www.w3.org/2005/xpath-functions\";\n" +
                    "\n" +
                    "declare function f:count(\n" +
                    " $seq as item()*\n" +
                    ") as xs:integer\n" +
                    "{\n" +
                    " count($seq)\n" +
                    "};";
}
