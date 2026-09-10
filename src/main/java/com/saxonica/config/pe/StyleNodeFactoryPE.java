package com.saxonica.config.pe;

import com.saxonica.config.ProfessionalConfiguration;
import com.saxonica.xsltextn.ExtensionElementCreator;
import net.sf.saxon.Configuration;
import net.sf.saxon.event.PipelineConfiguration;
import net.sf.saxon.expr.accum.AccumulatorRegistry;
import net.sf.saxon.lib.Feature;
import net.sf.saxon.om.*;
import net.sf.saxon.s9api.Location;
import net.sf.saxon.style.*;
import net.sf.saxon.trans.SaxonErrorCode;
import net.sf.saxon.trans.XmlProcessingIncident;
import net.sf.saxon.tree.linked.ElementImpl;
import net.sf.saxon.tree.linked.NodeFactory;
import net.sf.saxon.type.SchemaType;


/**
 * Class StyleNodeFactoryPE: Subclass of StyleNodeFactory used for Saxon-PE. <br>
 * A Factory for nodes in the stylesheet tree. <br>
 * This version differs from the parent class in allowing extension elements to be constructed
 * if the URI corresponds to an extension element factory registered in the Configuration
 *
 */

public class StyleNodeFactoryPE extends StyleNodeFactory implements NodeFactory {

    protected boolean allowExtensions;

    /**
     * Create the node factory for representing an XSLT stylesheet as a tree structure
     *
     * @param config the Saxon configuration
     * @param compilation the compilation episode (compiling one package)
     */

    public StyleNodeFactoryPE(Configuration config, Compilation compilation) {
        super(config, compilation);
        allowExtensions = config.getBooleanProperty(Feature.ALLOW_EXTERNAL_FUNCTIONS);
    }

    /**
     * Create an Element node. Note, if there is an error detected while constructing
     * the Element, we add the element anyway, and return success, but flag the element
     * with a validation error. This allows us to report more than
     * one error from a single compilation.
     */

    @Override
    public ElementImpl makeElementNode(
            NodeInfo parent,
            NodeName elementName,
            SchemaType elementType,
            boolean isNilled,
            AttributeMap attlist,
            NamespaceMap namespaces,
            PipelineConfiguration pipe,
            Location location,
            int sequence) {
        boolean toplevel = parent instanceof XSLModuleRoot;
        if (elementName.hasURI(NamespaceUri.XSLT) ||
                !allowExtensions ||
                parent instanceof DataElement) {
            return super.makeElementNode(parent, elementName, elementType, isNilled, attlist, namespaces,
                    pipe, location, sequence);
        }

        StyleElement extensionElement = null;
        NamespaceUri uri = elementName.getNamespaceUri();
        ExtensionElementCreator eef = ((ProfessionalConfiguration) config).getExtensionElementCreator(uri);
        if (eef != null) {
            String localName = elementName.getLocalPart();
            extensionElement = eef.createExtensionElement(localName);
        }

        if (extensionElement == null) {
            ElementImpl instr = super.makeElementNode(parent, elementName, elementType, isNilled, attlist, namespaces,
                    pipe, location, sequence);
            if (instr instanceof StyleElement) {
                ((StyleElement)instr).setCompilation(getCompilation());
            }
            return instr;
        }

        // We have a name that is recognized as an extension element name.
        // It's actually an extension element if (a) it's a top-level element
        // and the namespace is Saxon, or (b) it's an instruction and the namespace is
        // declared as an extension element namespace

        String baseURI = null;
        int lineNumber = -1;
        int columnNumber = -1;
        baseURI = location.getSystemId();
        lineNumber = location.getLineNumber();
        columnNumber = location.getColumnNumber();


        String localname = elementName.getLocalPart();
        StyleElement temp;

        if (allowExtensions) {
            // We can't work out the final class of the node until we've examined its attributes
            // such as version and extension-element-prefixes; but we can have a good guess, and
            // change it later if need be.
            temp = extensionElement;
            temp.setCompilation(getCompilation());
        } else {
            temp = new AbsentExtensionElement();
            XmlProcessingIncident reason = new XmlProcessingIncident("Extension instructions are disabled");
            temp.setValidationError(reason, StyleElement.OnFailure.REPORT_IF_INSTANTIATED);
        }

        temp.setNamespaceMap(namespaces);
        temp.initialise(elementName, elementType, attlist, parent, sequence);
        temp.setLocation(baseURI, lineNumber, columnNumber);
        temp.processStandardAttributes(NamespaceUri.XSLT);

        // Now we work out what class of element we really wanted, and change it if necessary

        if (uri.equals(NamespaceUri.SAXON)) {
            if (!(toplevel || temp.isExtensionNamespace(NamespaceUri.SAXON))) {
                // We've got an element such as saxon:call-template but the
                // user didn't declare the Saxon namespace as an extension element
                // namespace. Chances are they just forgot.
                XmlProcessingIncident te = new XmlProcessingIncident("saxon:" + localname +
                        " is not being treated as an extension instruction because the namespace " +
                        " http://saxon.sf.net/ has not been declared in extension-element-prefixes",
                                                                     SaxonErrorCode.SXWN9008, temp).asWarning();
                pipe.getErrorReporter().report(te);
                temp = new LiteralResultElement();
            }
        }

        return temp;
    }

    /**
     * Method to support the element-available() function
     *
     *
     * @param uri       the namespace URI
     * @param localName the local Name
     * @param instructionsOnly true if only instructions qualify
     * @return true if an extension element of this name is recognized
     */

    @Override
    public boolean isElementAvailable(NamespaceUri uri, String localName, boolean instructionsOnly) {
        boolean b = super.isElementAvailable(uri, localName, instructionsOnly);
        if (b) {
            return true;
        }
        if (!allowExtensions) {
            return false;
        }
        ExtensionElementCreator factory = ((ProfessionalConfiguration) config).getExtensionElementCreator(uri);
        if (factory == null) {
            return false;
        }
        try {
            StyleElement s = factory.createExtensionElement(localName);
            return !instructionsOnly || s.isInstruction();
        } catch (Exception e) {
            return false;
        }
    }

    @Override
    public AccumulatorRegistry makeAccumulatorManager() {
        return new AccumulatorRegistry();
    }




}

// Copyright (c) 2009-2022 Saxonica Limited