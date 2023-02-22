package simmons;

import net.sf.saxon.s9api.Processor;
import net.sf.saxon.s9api.SaxonApiException;
import org.junit.jupiter.api.Assertions;
import org.junit.jupiter.api.Tag;
import org.junit.jupiter.api.Test;

import javax.xml.transform.stream.StreamSource;

import static org.junit.jupiter.api.Assertions.assertEquals;

/**
 * Reproduces unexplained slowness in Saxon grammar loading.
 */
public class TestSchemaLoading {

    private final Processor _processor;
    private final String DIR = "/Users/mike/bugs/2020/4554-Simmons/saxon-schema-bug/";

    public TestSchemaLoading() {
        _processor = ProcessorFactory.createProcessor(DIR);
    }

    private void loadSchema(final String location) throws SaxonApiException {
        //_processor.setConfigurationProperty(Feature.TIMING, true);
        _processor.getSchemaManager().load(new StreamSource(location));
    }

    private void assertSchemaCount() {
        assertEquals(15, _processor.getUnderlyingConfiguration().getImportedNamespaces().size());
    }

    /**
     * In this case we've modified ext.xsd to import full_ifrs-cor, its fast.
     */
    @Test
    @Tag("mikeonly")
    public void testFastCase() throws Exception {
        long start = System.currentTimeMillis();
        loadSchema(DIR + "filing-copy/taxonomy/ext.xsd");
        assertSchemaCount();
        final long elapsed = System.currentTimeMillis() - start;
        Assertions.assertTrue(elapsed < 5000, "testFastCase");
        System.err.println("Time in ms (testFastCase): " + elapsed);
    }


    /**
     * In this case we load full_ifrs-cor before ext.xsd, its fast too.
     */
    @Test
    @Tag("mikeonly")
    public void testFastCaseB() throws Exception {
        long start = System.currentTimeMillis();
        loadSchema("http://xbrl.ifrs.org/taxonomy/2019-03-27/full_ifrs/full_ifrs-cor_2019-03-27.xsd");
        loadSchema(DIR + "filing/taxonomy/ext.xsd");
        assertSchemaCount();
        final long elapsed = System.currentTimeMillis() - start;
        Assertions.assertTrue(elapsed < 5000, "testFastCaseB");
        System.err.println("Time in ms (testFastCaseB): " + elapsed);
    }

    /**
     * In this case we load full_ifrs-cor after ext.xsd, its really slow!
     */
    @Test
    @Tag("mikeonly")
    public void testSlowCase() throws Exception {
        long start = System.currentTimeMillis();
        loadSchema(DIR + "filing/taxonomy/ext.xsd");
        loadSchema("http://xbrl.ifrs.org/taxonomy/2019-03-27/full_ifrs/full_ifrs-cor_2019-03-27.xsd");
        assertSchemaCount();
        final long elapsed = System.currentTimeMillis() - start;
        Assertions.assertTrue(elapsed < 5000, "testFastCase");
        System.err.println("Time in ms (testSlowCase): " + elapsed);
    }

}
