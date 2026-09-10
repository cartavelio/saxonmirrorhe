package failing;

import net.sf.saxon.Configuration;
import net.sf.saxon.junit.Testdata;
import net.sf.saxon.lib.Feature;
import net.sf.saxon.lib.SchemaURIResolver;
import net.sf.saxon.s9api.*;
import org.junit.jupiter.api.Disabled;
import org.junit.jupiter.api.Test;

import javax.xml.transform.Source;
import javax.xml.transform.stream.StreamSource;
import java.io.File;
import java.io.StringReader;

import static org.junit.jupiter.api.Assertions.assertEquals;
import static org.junit.jupiter.api.Assertions.fail;

/**
 * This module is a place for parking tests that we are currently failing for a known reason. Failures in these
 * tests are known problems and should not inhibit progress.
 */
public class FailingTests {
    
}

