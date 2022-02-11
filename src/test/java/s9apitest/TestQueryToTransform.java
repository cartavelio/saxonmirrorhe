package s9apitest;

import jaxptest.ConfigTest;
import junit.framework.TestCase;
import net.sf.saxon.s9api.*;

import javax.xml.transform.stream.StreamSource;
import java.io.File;
import java.net.URI;

/**
 *
 */
public class TestQueryToTransform extends TestCase {

    public void testQueryToTransform() throws Exception {
        Processor proc = new Processor(true);
        XQueryCompiler comp = proc.newXQueryCompiler();
        comp.setBaseURI(new URI("file:///c:/aplEjXML/"));
        String qry_base = "<a>1234</a>";

        XQueryExecutable exp = comp.compile(qry_base);
        XQueryEvaluator qe = exp.load();
        XdmValue result = qe.evaluate();

        XsltCompiler comp_xslt = proc.newXsltCompiler();
        comp_xslt.setSchemaAware(false);
        XsltExecutable template = comp_xslt.compile(new StreamSource(new File(ConfigTest.DATA_DIR + "identity.xsl")));
        XsltTransformer trans1 = template.load();

        for (XdmItem item : result) {
            //XdmNode source = proc.newDocumentBuilder().build(((XdmNode)item).asSource());
            trans1.setInitialContextNode((XdmNode)item);
            XdmDestination resultTree = new XdmDestination();
            trans1.setDestination(resultTree);
            trans1.transform();
            assertEquals("<a>1234</a>", resultTree.getXdmNode().toString().trim());
        }

    }

    public void testQueryToTransformPiped() throws Exception {
        Processor proc = new Processor(true);
        XQueryCompiler comp = proc.newXQueryCompiler();
        comp.setBaseURI(new URI("file:///c:/aplEjXML/"));
        String qry_base = "<a>1234</a>";

        XQueryExecutable exp = comp.compile(qry_base);
        XQueryEvaluator qe = exp.load();

        XsltCompiler comp_xslt = proc.newXsltCompiler();
        comp_xslt.setSchemaAware(false);
        XsltExecutable template = comp_xslt.compile(new StreamSource(new File(ConfigTest.DATA_DIR + "identity.xsl")));
        XsltTransformer trans1 = template.load();

        XdmDestination resultTree = new XdmDestination();
        trans1.setDestination(resultTree);


        qe.setDestination(trans1);
        qe.run();

        assertEquals("<a>1234</a>", resultTree.getXdmNode().toString().trim());
    }
}

//
// The contents of this file are subject to the Mozilla Public License Version
// 1.0 (the "License");
// you may not use this file except in compliance with the License. You may
// obtain a copy of the
// License at http://www.mozilla.org/MPL/
//
// Software distributed under the License is distributed on an "AS IS" basis,
// WITHOUT WARRANTY OF ANY KIND, either express or implied.
// See the License for the specific language governing rights and limitations
// under the License.
//
// The Original Code is: all this file.
//
// The Initial Developer of the Original Code is Michael Kay,
//
// Portions created by (your name) are Copyright (C) (your legal entity). All
// Rights Reserved.
//
// Contributor(s): none.
//



