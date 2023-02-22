import java.io.InputStream;
import javax.xml.transform.stream.StreamSource;
import com.corefiling.xbrl.xiif.core.xpath.saxonImpl.SaxonLicence;
import net.sf.saxon.om.TreeModel;
import net.sf.saxon.s9api.DocumentBuilder;
import net.sf.saxon.s9api.Processor;
import net.sf.saxon.s9api.XPathCompiler;
import net.sf.saxon.s9api.XPathExecutable;
import net.sf.saxon.s9api.XPathSelector;
import net.sf.saxon.s9api.XdmNode;

public class InstanceModelLeak {

  public static void main(final String[] args) throws Exception {
    final Processor processor = new Processor(true);
    processor.setConfigurationProperty("http://saxon.sf.net/feature/schema-validation-mode", "lax");
		processor.getSchemaManager().load(new StreamSource(InstanceModelLeak.class.getResourceAsStream("schema.xsd")));

    final DocumentBuilder builder = processor.newDocumentBuilder();
    builder.setTreeModel(TreeModel.TINY_TREE);

    final InputStream instance = InstanceModelLeak.class.getResourceAsStream("instance.xml");
		XdmNode node = builder.build(new StreamSource(instance));
		WeakReference<TinyTree> tinyTree = new WeakReference<TinyTree>(((TinyDocumentImpl) node.getUnderlyingNode()).getTree());

    final XPathCompiler compiler = processor.newXPathCompiler();
    compiler.importSchemaNamespace("http://example.org");
    compiler.evaluate("namespace-uri-from-QName(//*/foo)", node);
    node = null;
    // At this point the TinyTree cannot be gc'ed because there is a reference to element foo from its UserAtomicType
    
		// ...further instance processing...
		// ...
    // ...
  }
}
