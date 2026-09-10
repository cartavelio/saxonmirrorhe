package nativetests;

import net.sf.saxon.Configuration;
import net.sf.saxon.TransformerFactoryImpl;
import net.sf.saxon.value.SequenceType;
import org.junit.jupiter.api.Test;

import javax.xml.transform.Templates;
import javax.xml.transform.Transformer;
import javax.xml.transform.TransformerException;
import javax.xml.transform.TransformerFactory;
import javax.xml.transform.stream.StreamResult;
import javax.xml.transform.stream.StreamSource;
import java.io.StringReader;

import static org.junit.jupiter.api.Assertions.assertNotNull;

// Bug report against 9.0.0.4 from Mike Rutter, 8 May 2007
// Causes NPE in 9.0.0.4 due to incorrect sequence of static initialization
// Fixed in 9.1

public class TestStaticInitBug {
   private static final SequenceType showTheBug = SequenceType.ATOMIC_SEQUENCE;

   private static final String xsl =
      "<?xml version='1.0' encoding='UTF-8'?>" +
      "<xsl:stylesheet version='2.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform' " +
      "xmlns:ssmarc='http://www.loc.gov/MARC21/slim/' xmlns:saxon='http://saxon.sf.net/' extension-element-prefixes='saxon'> " +
      "<xsl:function name='ssmarc:datafield'>" +
      "  <xsl:param name='ind1' />" +
      "  <xsl:variable name=\"ind1-norm\" select=\"if (string-length($ind1) = 0) then ' ' else $ind1\" />" +
      "  <xsl:sequence select='$ind1-norm'/>" +
      "</xsl:function>" +
      "</xsl:stylesheet>";

   private static final String xml =
      "<?xml version='1.0' encoding='UTF-8'?><record><datafield ind1=' '></datafield></record>";

   @Test
   public void testStaticInitBug() throws TransformerException {
      TransformerFactory tfactory = new TransformerFactoryImpl(new Configuration());
      StreamSource templateSource = new StreamSource(new StringReader(xsl));
      StreamSource source = new StreamSource(new StringReader(xml));
      StreamResult result = new StreamResult(System.out);
      Templates templates = tfactory.newTemplates(templateSource);
      Transformer transformer = templates.newTransformer();
      transformer.transform(source, result);
      assertNotNull(showTheBug.getPrimaryType());
   }
}

