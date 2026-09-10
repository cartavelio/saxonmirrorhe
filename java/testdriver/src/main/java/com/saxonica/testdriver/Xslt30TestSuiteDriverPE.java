////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) 2018-2023 Saxonica Limited
// This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
// If a copy of the MPL was not distributed with this file, You can obtain one at http://mozilla.org/MPL/2.0/.
// This Source Code Form is "Incompatible With Secondary Licenses", as defined by the Mozilla Public License, v. 2.0.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

package com.saxonica.testdriver;


import com.saxonica.config.ProfessionalConfiguration;
import com.saxonica.ee.domino.DominoTree;
import com.saxonica.option.sql.SQLElementFactory;
import net.sf.saxon.Configuration;
import net.sf.saxon.lib.Feature;
import net.sf.saxon.om.NodeInfo;
import net.sf.saxon.om.TreeModel;
import net.sf.saxon.option.axiom.AxiomObjectModel;
import net.sf.saxon.option.dom4j.DOM4JObjectModel;
import net.sf.saxon.option.jdom2.JDOM2ObjectModel;
import net.sf.saxon.option.xom.XOMObjectModel;
import net.sf.saxon.s9api.Processor;
import net.sf.saxon.s9api.SaxonApiException;
import net.sf.saxon.s9api.XdmNode;
import net.sf.saxon.s9api.XdmValue;
import net.sf.saxon.testdriver.Xslt30TestSuiteDriverHE;
import net.sf.saxon.trans.XPathException;
import org.w3c.dom.Document;

/**
 * Created by debbie on 12/09/14.
 */

// Switched from "extends Xslt30TestSuiteDriverHE" to test new s9api features - can switch back any time - MHK 2017-07-04
// Switched back from "extends Xslt30TestSuiteDriverHE_Q" to test new s9api features - can switch back any time - MHK 2017-07-04

public class Xslt30TestSuiteDriverPE extends Xslt30TestSuiteDriverHE {



    public Xslt30TestSuiteDriverPE() {
        super();
    }


    public static void main(String[] args) throws Exception {
        if (args.length < 2 || args[0].equals("-?")) {
            usage();
            System.exit(2);
        }

        new Xslt30TestSuiteDriverPE().go(args);
    }


    @Override
    public void go(String[] args) throws Exception {
        driverProc = new Processor(true);
        driverProc.setConfigurationProperty(Feature.XML_VERSION, "1.1");
        Licensor licensor = new Licensor();
        licensor.activate(driverProc);
        super.go(args);
    }

    /**
     * Return the appropriate tree model to use. This adds to the base set available
     *
     * @param s The name of the tree model required
     * @return The tree model - null if model requested is unrecognised
     */
    @Override
    protected TreeModel getTreeModel(String s) {
        if (s.equalsIgnoreCase("jdom2")) {
            return new JDOM2ObjectModel();
        } else if (s.equalsIgnoreCase("dom4j")) {
            return new DOM4JObjectModel();
        } else if (s.equalsIgnoreCase("xom")) {
            return new XOMObjectModel();
        } else if (s.equalsIgnoreCase("axiom")) {
            return new AxiomObjectModel();
        } else {
            return super.getTreeModel(s);
        }
    }

    @Override
    public XdmNode makeDominoTree(Document doc, Configuration config, String baseUri) throws SaxonApiException {
        XdmNode result = null;
        DominoTree dominoTree = null;
        try {
            dominoTree = DominoTree.makeTree(doc, config, baseUri);
        } catch (XPathException e) {
            throw new SaxonApiException(e);
        }
        NodeInfo root = dominoTree.getRootNode();
        result = (XdmNode) XdmValue.wrap(root);
        return result;
    }

    @Override
    public void prepareForSQL(Processor processor) {
        try {
            if (processor.getUnderlyingConfiguration() instanceof ProfessionalConfiguration) {
                //environment.processor.setConfigurationProperty(FeatureKeys.TRACE_EXTERNAL_FUNCTIONS, true);
                ((ProfessionalConfiguration) processor.getUnderlyingConfiguration())
                        .setExtensionElementNamespace("http://saxon.sf.net/sql", new SQLElementFactory());
            }
        } catch (XPathException e) {
            e.printStackTrace();
        }
    }



}
