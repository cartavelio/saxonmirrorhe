import java.io.File;
import java.net.URL;

import javax.xml.transform.Source;
import javax.xml.transform.stream.StreamSource;

import com.saxonica.config.EnterpriseConfiguration;

import net.sf.saxon.lib.FeatureKeys;
import net.sf.saxon.om.NodeInfo;

public class Run {

	public static void main(String[] args) throws Exception {
		
		EnterpriseConfiguration config = new EnterpriseConfiguration();
		
		config.setBooleanProperty(FeatureKeys.MULTIPLE_SCHEMA_IMPORTS, true);
		
		loadSchemaOK(config,new File("src/schema.xsd").toURI().toString());
		loadSchemaOK(config,new File("src/schema1.xsd").toURI().toString());
		loadSchemaOK(config,new File("src/schema2.xsd").toURI().toString());

		EnterpriseConfiguration config1 = new EnterpriseConfiguration();
		
		config1.setBooleanProperty(FeatureKeys.MULTIPLE_SCHEMA_IMPORTS, true);
		
		loadSchemaBad(config1,new File("src/schema.xsd").toURI().toString());
		loadSchemaBad(config1,new File("src/schema1.xsd").toURI().toString());
		loadSchemaBad(config1,new File("src/schema2.xsd").toURI().toString());

    }
	
	private static void loadSchemaOK(EnterpriseConfiguration config, String urlStr) throws Exception {
		URL url = new URL(urlStr);
		Source s = new StreamSource( url.openConnection().getInputStream(),urlStr);
		config.addSchemaSource( s );
		
	}
	
	private static void loadSchemaBad(EnterpriseConfiguration config, String urlStr) throws Exception {
		URL url = new URL(urlStr);
		Source s = new StreamSource( url.openConnection().getInputStream(),urlStr );
		NodeInfo ni = config.buildDocumentTree(s).getRootNode();
		//System.out.println( QueryResult.serialize(ni) );
		//System.out.println( ni.getSystemId() );
		config.addSchemaSource( ni);
		
	}


}
