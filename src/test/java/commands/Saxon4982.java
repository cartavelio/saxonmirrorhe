package commands;

import com.saxonica.config.EnterpriseConfiguration;
import net.sf.saxon.lib.Feature;
import org.w3c.dom.ls.LSInput;
import org.w3c.dom.ls.LSResourceResolver;
import org.xml.sax.SAXException;

import javax.xml.XMLConstants;
import javax.xml.transform.Source;
import javax.xml.transform.stream.StreamSource;
import javax.xml.validation.SchemaFactory;
import java.io.*;
import java.nio.file.Files;
import java.nio.file.Path;
import java.nio.file.Paths;
import java.util.ArrayList;
import java.util.List;
import java.util.zip.ZipEntry;
import java.util.zip.ZipInputStream;

public class Saxon4982 {

    private static byte[] schemasInZip;

    static {
        File schemaZip = new File(
                "/Users/mike/bugs/2021/4982-Vanhala/SchemaValidatorImplTest_zipped_schemas.zip");
        try {
            schemasInZip = Files.readAllBytes(schemaZip.toPath());
        } catch (IOException e) {
            throw new RuntimeException(e);
        }
    }

    public static void main(String args[]) {
        Saxon4982 application = new Saxon4982();
        for (int i = 0; i < 100; i++) {
            long startTime = System.currentTimeMillis();
            //System.out.println("Loading schemas from zip with Apache's implementation...");
            application.run(getApacheSchemaFactory());
            System.out.println(String.format("Apache took %s milliseconds.", System.currentTimeMillis() - startTime));


            startTime = System.currentTimeMillis();
            //System.out.println("Loading schemas from zip with Saxonica's implementation...");
            application.run(getSaxonSchemaFactory());
            System.out.println(String.format("Saxon took %s milliseconds.", System.currentTimeMillis() - startTime));
        }
    }

    private void run(SchemaFactory schemaFactory) {
        resolverCalls = 0;

        List<Source> sources = new ArrayList<>();
        try {
            addZippedSchemas(sources);
        } catch (IOException e) {
            throw new RuntimeException(e);
        }

        schemaFactory.setResourceResolver(resourceResolver);


        try {
            schemaFactory.newSchema(sources.toArray(new Source[0]));
        } catch (SAXException e) {
            throw new RuntimeException(e);
        }

        System.err.println("Resolver calls: " + resolverCalls);
    }

    private static int resolverCalls = 0;

    /**
     * Resolve an external resource. Sources have system identifiers of the xsd files indicating
     * where files are found as follows: "zip:/path/to/file.xsd".
     * <p>
     * Schema factory calls <code>resolveResource</code> method with systemId of the source file in
     * baseURI parameter, and the imported xsd that needs to be resolved in systemId parameter.
     */
    private static LSResourceResolver resourceResolver = new LSResourceResolver() {
        @Override
        public LSInput resolveResource(String type, String namespaceURI, String publicId, String systemId,
                                       String baseURI) {
            resolverCalls++;
            System.out.println(String.format(
                    "Resolver called with parameters type %s, "
                            + "namespaceURI %s, publicId %s, systemId %s, baseURI %s",
                    type, namespaceURI, publicId, systemId, baseURI));

            String searchedSchemaFile = null;

            if (baseURI != null && baseURI.startsWith("zip:")) {
                String[] splittedBaseURI = baseURI.split(":");
                Path baseFileFolder = Paths.get(splittedBaseURI[1]).getParent();
                searchedSchemaFile = baseFileFolder.resolve(systemId).normalize().toString();
            } else {
                String errorLine = String.format(
                        "The baseURI is lost! Unable to determine where to find xsd file %s as baseURI is now %s",
                        systemId, baseURI);
                throw new RuntimeException(errorLine);
            }

            byte[] uncompressedSchema = getZippedSchema(searchedSchemaFile);
            if (uncompressedSchema == null) {
                String errorLine = String.format(
                        "Schema file %s is supposed to be inside %s, but it was not found!",
                        searchedSchemaFile);
                throw new RuntimeException(errorLine);
            }

            systemId = "zip:" + searchedSchemaFile;
            baseURI = null;

            InputStream stream = new ByteArrayInputStream(uncompressedSchema);
            LSInputImpl input = new LSInputImpl();
            input.setPublicId(publicId);
            input.setSystemId(systemId);
            input.setBaseURI(baseURI);
            input.setCharacterStream(new InputStreamReader(stream));

            System.out.println(String.format("Resolved xsd file content: publicId %s systemId %s baseURI %s", publicId, systemId, baseURI));

            return input;
        }
    };

    private static SchemaFactory getSaxonSchemaFactory() {
        EnterpriseConfiguration conf = new EnterpriseConfiguration();
        //conf.setConfigurationProperty(FeatureKeys.LICENSE_FILE_LOCATION, "../saxon-license.lic");
        conf.setConfigurationProperty(Feature.MULTIPLE_SCHEMA_IMPORTS, Boolean.FALSE);
        return new com.saxonica.ee.jaxp.SchemaFactoryImpl(conf);
    }

    private static SchemaFactory getApacheSchemaFactory() {
        SchemaFactory schemaFactory = SchemaFactory.newInstance(XMLConstants.W3C_XML_SCHEMA_NS_URI,
                                                                "org.apache.xerces.jaxp.validation.XMLSchemaFactory", Saxon4982.class.getClassLoader());
        return schemaFactory;
    }

    /**
     * Search a xsd file from the zip. Return an uncompressed byte array of the file content.
     */
    private static byte[] getZippedSchema(String searchedSchemaFile) {
        try (ZipInputStream zi = new ZipInputStream(new ByteArrayInputStream(schemasInZip))) {
            ZipEntry zipEntry = null;
            while ((zipEntry = zi.getNextEntry()) != null) {
                if (!zipEntry.isDirectory()) {
                    String normalizedName = Paths.get("/", zipEntry.getName()).normalize().toString();
                    if (normalizedName.equals(searchedSchemaFile)) {
                        ByteArrayOutputStream os = new ByteArrayOutputStream();
                        int b = zi.read();
                        while (b >= 0) {
                            os.write(b);
                            b = zi.read();
                        }
                        os.flush();

                        return os.toByteArray();
                    }
                }
            }
        } catch (IOException e) {
            throw new RuntimeException(e);
        }
        return null;
    }

    /**
     * Read through the zip file and make <code>Source</code> entries from every xsd-file. Generate
     * a systemId of the xsd file indicating where the file is found as follows:
     * "zip:/path/to/file.xsd"
     */
    private static void addZippedSchemas(List<Source> sources) throws IOException {
        try (ZipInputStream zi = new ZipInputStream(new ByteArrayInputStream(schemasInZip))) {
            ZipEntry zipEntry = null;
            while ((zipEntry = zi.getNextEntry()) != null) {
                if (!zipEntry.isDirectory() && zipEntry.getName().toLowerCase().endsWith(".xsd")) {
                    String normalizedName = Paths.get("/", zipEntry.getName()).normalize().toString();

                    try (ByteArrayOutputStream os = new ByteArrayOutputStream()) {
                        int b = zi.read();
                        while (b >= 0) {
                            os.write(b);
                            b = zi.read();
                        }
                        os.flush();

                        byte[] schemaDoc = os.toByteArray();
                        String systemId = String.format("zip:%s", normalizedName);
                        sources.add(new StreamSource(new MonitoringStream(schemaDoc, systemId), systemId));
                        System.out.println(String.format("Found %s from the zip. Added source with systemId %s", normalizedName, systemId));
                    }
                }
            }
        }
    }

    static class MonitoringStream extends ByteArrayInputStream {
        private String systemId;
        public MonitoringStream(byte[] bytes, String systemId) {
            super(bytes);
            this.systemId= systemId;
        }
        public void close() {
            System.out.println("Closing " + systemId);
        }

    }

    static class LSInputImpl implements LSInput {
        private Reader characterStream;
        private InputStream byteStream;
        private String stringData;
        private String systemId;
        private String publicId;
        private String baseURI;
        private String encoding;
        private boolean certifiedText;

        @Override
        public Reader getCharacterStream() {
            return characterStream;
        }

        @Override
        public void setCharacterStream(Reader characterStream) {
            this.characterStream = characterStream;
        }

        @Override
        public InputStream getByteStream() {
            return byteStream;
        }

        @Override
        public void setByteStream(InputStream byteStream) {
            this.byteStream = byteStream;
        }

        @Override
        public String getStringData() {
            return stringData;
        }

        @Override
        public void setStringData(String stringData) {
            this.stringData = stringData;
        }

        @Override
        public String getSystemId() {
            return systemId;
        }

        @Override
        public void setSystemId(String systemId) {
            this.systemId = systemId;
        }

        @Override
        public String getPublicId() {
            return publicId;
        }

        @Override
        public void setPublicId(String publicId) {
            this.publicId = publicId;
        }

        @Override
        public String getBaseURI() {
            return baseURI;
        }

        @Override
        public void setBaseURI(String baseURI) {
            this.baseURI = baseURI;
        }

        @Override
        public String getEncoding() {
            return encoding;
        }

        @Override
        public void setEncoding(String encoding) {
            this.encoding = encoding;
        }

        public boolean getCertifiedText() {
            return certifiedText;
        }

        @Override
        public void setCertifiedText(boolean certifiedText) {
            this.certifiedText = certifiedText;
        }
    }


}

