package simmons;

import net.sf.saxon.lib.Feature;
import net.sf.saxon.s9api.Processor;
import net.sf.saxon.s9api.SaxonApiException;
import org.junit.jupiter.api.Disabled;
import org.junit.jupiter.api.Tag;
import org.junit.jupiter.api.Test;

import javax.xml.transform.stream.StreamSource;

import static org.junit.jupiter.api.Assertions.assertEquals;

/**
 * Reproduces unexplained slowness in Saxon grammar loading.
 */
public class TestSchemaLoading {

    private final Processor _processor;

    public TestSchemaLoading() {
        _processor = ProcessorFactory.createProcessor();
    }

    private void loadSchema(final String location) throws SaxonApiException {
        _processor.setConfigurationProperty(Feature.TIMING, true);
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
        loadSchema("filing-copy/taxonomy/ext.xsd");
        assertSchemaCount();
        System.err.println("Time in ms: " + (System.currentTimeMillis() - start));
    }


    /**
     * In this case we load full_ifrs-cor before ext.xsd, its fast too.
     */
    @Test
    @Tag("mikeonly")
    public void testFastCaseB() throws Exception {
        loadSchema("http://xbrl.ifrs.org/taxonomy/2019-03-27/full_ifrs/full_ifrs-cor_2019-03-27.xsd");
        loadSchema("filing/taxonomy/ext.xsd");
        assertSchemaCount();
    }

    /**
     * In this case we load full_ifrs-cor after ext.xsd, its really slow!
     */
    @Test
    @Tag("mikeonly")
    public void testSlowCase() throws Exception {
        long start = System.currentTimeMillis();
        loadSchema("filing/taxonomy/ext.xsd");
        loadSchema("http://xbrl.ifrs.org/taxonomy/2019-03-27/full_ifrs/full_ifrs-cor_2019-03-27.xsd");
        assertSchemaCount();
        System.err.println("Time in ms: " + (System.currentTimeMillis() - start));
    }

}
