package com.saxonica.ee.schema.sdoc;

import com.saxonica.ee.schema.SchemaCompiler;
import net.sf.saxon.Configuration;
import net.sf.saxon.PreparedStylesheet;
import net.sf.saxon.Version;
import net.sf.saxon.event.PipelineConfiguration;
import net.sf.saxon.event.TreeReceiver;
import net.sf.saxon.expr.instruct.GlobalParameterSet;
import net.sf.saxon.om.NodeInfo;
import net.sf.saxon.om.TreeInfo;
import net.sf.saxon.style.Compilation;
import net.sf.saxon.trans.CompilerInfo;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.trans.XsltController;
import net.sf.saxon.tree.linked.LinkedTreeBuilder;
import net.sf.saxon.type.SchemaException;

import javax.xml.transform.TransformerException;
import javax.xml.transform.stream.StreamSource;
import java.io.InputStream;
import java.util.ArrayList;

/**
 * Class that applies a standard transformation to a schema document: specifically, the chameleon transformation
 * or the override transformation
 */

public class SchemaDocumentTransformer {

    private PreparedStylesheet pss;

    public SchemaDocumentTransformer(Configuration config, String filename) throws SchemaException {
        compileStylesheet(config, filename);
    }

    private void compileStylesheet(Configuration config, String filename) throws SchemaException {
        try {
            InputStream in = Version.platform.locateResource(filename, new ArrayList<>());
            StreamSource ss = new StreamSource(in, filename);
            CompilerInfo info = config.getDefaultXsltCompilerInfo();
            //config.setConfigurationProperty(FeatureKeys.DEBUG_BYTE_CODE, true);
            pss = Compilation.compileSingletonPackage(config, info, ss);
        } catch (XPathException e) {
            throw new SchemaException(e);
        }
    }

    public TreeInfo transform(SchemaCompiler compiler, PipelineConfiguration pipe, SchemaElement referrer,
                              NodeInfo input, GlobalParameterSet params)
            throws SchemaException {
        try {
            XsltController transformer = pss.newController();
            transformer.initializeController(params);
            LinkedTreeBuilder builder = new LinkedTreeBuilder(pipe);
            builder.setNodeFactory(new SchemaNodeFactory(compiler, pipe, referrer));
            transformer.setBaseOutputURI(input.getSystemId());
            transformer.setGlobalContextItem(input);
            transformer.applyTemplates(input, new TreeReceiver(builder));
            return builder.getCurrentRoot().getTreeInfo();
        } catch (TransformerException e) {
            System.err.println(e.getMessageAndLocation());
            throw new SchemaException(e);
        }

    }
}

// Copyright (c) 2011-2022 Saxonica Limited
