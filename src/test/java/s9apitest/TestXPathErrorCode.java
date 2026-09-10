/*
 * � 2011 by Ignacio Hernandez-Ros.
 * This work may be reproduced and redistributed, in whole or in part,
 * without alteration and without prior written permission,
 * solely by educational institutions for nonprofit administrative
 * or educational purposes provided all copies contain the
 * following statement:
 * "� 2011 Ignacio Hernandez-Ros. This work is reproduced and distributed with the permission of Ignacio Hernandez-Ros. No other use is permitted without the express prior written permission of Ignacio Hernandez-Ros. For permission, contact ignacio@hernandez-ros.com"
 *
 * File creation date 03/06/2011
 */
/**
 *
 */
package s9apitest;

import com.saxonica.config.EnterpriseConfiguration;
import net.sf.saxon.expr.StaticContext;
import net.sf.saxon.expr.instruct.SlotManager;
import net.sf.saxon.lib.ErrorReporter;
import net.sf.saxon.om.NamespaceResolver;
import net.sf.saxon.om.NamespaceUri;
import net.sf.saxon.om.StructuredQName;
import net.sf.saxon.s9api.*;
import net.sf.saxon.sxpath.IndependentContext;
import net.sf.saxon.trans.XPathException;
import org.junit.jupiter.api.Test;

import javax.xml.namespace.QName;
import javax.xml.transform.stream.StreamSource;
import java.io.StringReader;
import java.util.HashSet;
import java.util.Iterator;
import java.util.List;
import java.util.Set;

import static org.junit.jupiter.api.Assertions.assertFalse;
import static org.junit.jupiter.api.Assertions.assertTrue;
import static org.junit.jupiter.api.Assertions.fail;

/**
 * @author Ignacio
 *
 */
public class TestXPathErrorCode {

	static NamespaceResolver voidNamespaceResolver = new NamespaceResolver() {

		@Override
        public NamespaceUri getURIForPrefix(String prefix, boolean useDefault) {
			return null;
		}

		@Override
        public Iterator<String> iteratePrefixes() {
			return null;
		}}
	;


    @Test
    public void testXPathFoo() {
        try {
            String expression = "$foo:c1";

            HashSet<QName> vRequiredVariables = new HashSet<QName>();
            HashSet<net.sf.saxon.s9api.QName> preDefinedVariables = new HashSet<net.sf.saxon.s9api.QName>();

            // copy all variable names in the set. do not use the bindable set because of exceptions will be thrown later
            HashSet<net.sf.saxon.s9api.QName> allVars = new HashSet<net.sf.saxon.s9api.QName>();
            allVars.addAll(preDefinedVariables);

            HashSet<net.sf.saxon.s9api.QName> defineVars = new HashSet<net.sf.saxon.s9api.QName>();

            boolean again;
            XPathExecutable executable = null;
            do {
                again = false;
                XPathCompiler xpathCompiler = getFPXPathCompiler(voidNamespaceResolver/*,fragment*/,true);
                for (net.sf.saxon.s9api.QName qname : defineVars)
                    xpathCompiler.declareVariable(qname);
                try {
                    executable = xpathCompiler.compile(expression);
                    IndependentContext ic = (IndependentContext)xpathCompiler.getUnderlyingStaticContext();
                    SlotManager sl = ic.getStackFrameMap();
                    List<StructuredQName> lst = sl.getVariableMap();
                    Iterator<StructuredQName> iter = lst.iterator();
                    while (iter.hasNext()) {
                        vRequiredVariables.add(javaxQName(iter.next()));
                    }
                    fail("No error detected");
                } catch (SaxonApiException e) {
                    Throwable cause = e.getCause();
                    assertTrue(cause instanceof XPathException);
                    assertTrue(e.getErrorCode().getLocalName().equals("XPST0081"));
                }
            } while (again);
        } catch (Exception e) {
            fail(e.getMessage());
        }
    }

    public static QName javaxQName(StructuredQName qname) {
		return new QName(
				qname.getURI(),
				qname.getLocalPart(),
				qname.getPrefix()
			);
	}

	/**
	 * Always return a new XPathCompiler object with the IndependentContext
	 * namespace Resolver set to the namespaceResolver supplied as a parameter.
	 * This function also adds the XFI function library to the XPath compiler.
	 *
	 * @param namespaceResolver
	 * @param schemaAware if true, the new XPathCompiler will be Schema Aware. All schemas currently loaded will be in the scope schemas for XPath compilation. It is safe to always put 'true' here.
	 * @return XPathCompiler
	 */
	public static XPathCompiler getFPXPathCompiler(NamespaceResolver namespaceResolver/*, XMLFragment fragment*/, boolean schemaAware) {
		Processor proc = new Processor(true);
		//ActivateSaxon.activate(proc);

		XPathCompiler fpXPath = proc.newXPathCompiler();
//		getContainer().getFastConfiguration().getUnderlyingConfiguration().setSchemaValidationMode(Validation.LAX);
		StaticContext env = fpXPath.getUnderlyingStaticContext();
		if (env instanceof IndependentContext) {
			IndependentContext ic = (IndependentContext)env;
			//Set imported = ic.getImportedSchemaNamespaces();
			ic.setNamespaceResolver(namespaceResolver);
			EnterpriseConfiguration conf = (EnterpriseConfiguration)proc.getUnderlyingConfiguration();
			@SuppressWarnings("unchecked")
			Set<String> schemas = conf.getSuperSchema().getTargetNamespaces();
			ic.setImportedSchemaNamespaces(schemas);
		}

		return fpXPath;
	}

    @Test
    public void test4300() {
        try {
            Processor processor = new Processor(true);
            DocumentBuilder builder = processor.newDocumentBuilder();
            XdmNode node1 = builder.build(new StreamSource(new StringReader("<a>22</a>")));
            XdmNode node2 = builder.build(new StreamSource(new StringReader("<a>23</a>")));
            XPathCompiler xpath = processor.newXPathCompiler();
            ItemTypeFactory itf = new ItemTypeFactory(processor);
            ItemType decimalType = itf.getAtomicType(new net.sf.saxon.s9api.QName("http://www.w3.org/2001/XMLSchema", "decimal"));
            xpath.declareVariable(new net.sf.saxon.s9api.QName("urn:us:xpath:var", "node1"));
            xpath.declareVariable(new net.sf.saxon.s9api.QName("urn:us:xpath:var", "node2"));
            xpath.declareNamespace("vmgv", "urn:us:xpath:var");
            xpath.declareNamespace("saxon", "http://saxon.sf.net/");
            XPathExecutable xx = xpath.compile("saxon:deep-equal($vmgv:node1, $vmgv:node2, () ,'w?')");

            XPathSelector selector = xx.load();
            LocalErrorReporter errorListener = new LocalErrorReporter();
            selector.setErrorReporter(errorListener);
            selector.setVariable(new net.sf.saxon.s9api.QName("urn:us:xpath:var", "node1"), node1);
            selector.setVariable(new net.sf.saxon.s9api.QName("urn:us:xpath:var", "node2"), node2);

            XdmAtomicValue deepEqualResult = (XdmAtomicValue) selector.evaluateSingle();
            //System.err.println(errorListener.errors);
            assertTrue(errorListener.errors.toString().contains("nodes at position 1 differ"));
            assertFalse(deepEqualResult.getBooleanValue());
        } catch (SaxonApiException err) {
            err.printStackTrace();
            fail();
        }


    }

    private static class LocalErrorReporter implements ErrorReporter {
        private final StringBuilder errors;

        LocalErrorReporter() {
            errors = new StringBuilder();
        }

        @Override
        public void report(XmlProcessingError error) {
            if (error.isWarning()) {
                errors.append(error.getMessage());
            } else {
                fail(error.getMessage());
            }
        }

    }


}

