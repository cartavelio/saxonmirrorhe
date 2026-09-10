package simmons;

import net.sf.saxon.Configuration;
import net.sf.saxon.lib.SchemaURIResolver;
import net.sf.saxon.s9api.Processor;
import net.sf.saxon.s9api.SchemaManager;
import net.sf.saxon.trans.XPathException;

import javax.xml.transform.ErrorListener;
import javax.xml.transform.Source;
import javax.xml.transform.TransformerException;
import javax.xml.transform.stream.StreamSource;
import java.io.File;
import java.io.FileInputStream;
import java.io.FileNotFoundException;
import java.net.URI;

/**
 * Creates a processor.
 */
public class ProcessorFactory {

  static Processor createProcessor(String DIR) {
    Processor processor = new Processor(true);
    processor.setConfigurationProperty("http://saxon.sf.net/feature/schema-validation-mode", "lax");
    SchemaManager schemaManager = processor.getSchemaManager();
    schemaManager.setErrorListener(new ErrorListener() {
      @Override
      public void warning(final TransformerException exception) throws TransformerException {
        System.out.println(exception.getMessage());
      }
      @Override
      public void error(final TransformerException exception) throws TransformerException {
        throw new AssertionError(exception);
      }

      @Override
      public void fatalError(final TransformerException exception) throws TransformerException {
        throw new AssertionError(exception);
      }
    });
    schemaManager.setSchemaURIResolver(new SchemaURIResolver() {
      @Override
      public void setConfiguration(final Configuration arg0) {
      }
      @Override
      public Source[] resolve(final String targetNamespace, final String baseURI, final String[] locations) throws XPathException {
        String location = locations[0];
        URI uri = URI.create(location);
        if (baseURI != null && !uri.isAbsolute()) {
          uri = URI.create(baseURI).resolve(uri);
          location = uri.toString();
        }
        if ("file".equals(uri.getScheme())) {
          return null;
        }
        String prefix = "http://";
        if (location.startsWith(prefix)) {
          File localFile = new File(DIR + "schemas", location.substring(prefix.length()));
          if (localFile.canRead()) {
            try {
              return new StreamSource[] {new StreamSource(new FileInputStream(localFile), location)};
            }
            catch (FileNotFoundException e) {
              throw new XPathException(e);
            }
          }
        }
        throw new AssertionError(baseURI + " → " + location);
      }
    });
    return processor;
  }

}
