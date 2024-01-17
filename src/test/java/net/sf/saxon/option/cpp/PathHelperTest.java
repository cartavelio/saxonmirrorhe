package net.sf.saxon.option.cpp;

import net.sf.saxon.Configuration;
import net.sf.saxon.lib.MockResolver;
import net.sf.saxon.lib.ResourceRequest;
import net.sf.saxon.lib.NullResolver;
import net.sf.saxon.s9api.SaxonApiException;
import org.junit.jupiter.api.*;
import org.junit.jupiter.api.condition.DisabledOnOs;
import org.junit.jupiter.api.condition.EnabledOnOs;
import org.junit.jupiter.api.condition.OS;

import javax.xml.transform.Source;

import java.net.URI;
import java.net.URISyntaxException;

import static org.junit.jupiter.api.Assertions.*;

class PathHelperTest {
    Configuration config = new Configuration();


    @Nested
    class ResolvingToSource {
        String expected;
        String actual;
        String nature = ResourceRequest.XML_NATURE;

        @Nested
        @EnabledOnOs(OS.WINDOWS)
        class WindowsOnly {
        }

        @Nested
        @DisabledOnOs(OS.WINDOWS)
        class UnixishOnly {
            String cwd = "/path/to/cwd";
            @Test
            void relativePathProducesSource() throws SaxonApiException {
                expected = "file:///path/to/cwd/source.xml";

                Source result = PathHelper.resolveToSource(config, cwd, "source.xml", nature);
                actual = result.getSystemId();

                assertEquals(expected, actual);
            }

            @Nested
            @TestInstance(TestInstance.Lifecycle.PER_CLASS)
            class WithCatalogResolverFailure {
                @BeforeAll
                void setResolver() {
                    config.setResourceResolver(new NullResolver());
                }

                @Test
                void relativePathProducesSource() throws SaxonApiException {
                    expected = "file:///path/to/cwd/source.xml";

                    Source result = PathHelper.resolveToSource(config, cwd, "source.xml", nature);
                    actual = result.getSystemId();

                    assertEquals(expected, actual);
                }
            }

            @Nested
            @TestInstance(TestInstance.Lifecycle.PER_CLASS)
            class WithCatalogResolverSuccess {
                public final URI resolverSourceURI = URI.create("http://example.org/source");
                @BeforeAll
                void setResolver() {
                    config.setResourceResolver(new MockResolver(resolverSourceURI));
                }

                @Test
                void relativePathProducesSource() throws SaxonApiException {
                    expected = resolverSourceURI.toASCIIString();

                    Source result = PathHelper.resolveToSource(config, cwd, "source.xml", nature);
                    actual = result.getSystemId();

                    assertEquals(expected, actual);
                }
            }
        }
    }

    @Nested
    class PathNormalisation {
        @Nested
        class PathsThatAreURIs {
            @Nested
            class PathsThatArePaths {
                URI expected;
                URI actual;

                @Nested
                @EnabledOnOs(OS.WINDOWS)
                class WindowsOnly {
                    @Test
                    void absolutePathWithSpaces() throws URISyntaxException {
                        expected = URI.create("file:///C:/Spaced%20Name/test.xsl");

                        actual = PathHelper.normalisePathToURI("C:\\Spaced Name\\test.xsl");

                        assertEquals(expected, actual, "paths should be normalised correctly");
                    }
                }

                @Nested
                @DisabledOnOs(OS.WINDOWS)
                class UnixishOnly {
                    @Test
                    void absolutePathWithSpaces() throws URISyntaxException {
                        expected = URI.create("file:/spaced%20name/test.xsl");

                        actual = PathHelper.normalisePathToURI("/spaced name/test.xsl");

                        assertEquals(expected, actual, "absolute paths should be normalised into file URIs");
                    }

                    @Test
                    void relativePathWithSpaces() throws URISyntaxException {
                        actual = PathHelper.normalisePathToURI("spaced name/test.xsl");

                        assertNull(actual, "relative paths should not be touched");
                    }

                    @Test
                    void looksLikeAWindowsPath() throws URISyntaxException {
                        actual = PathHelper.normalisePathToURI("C:/windows/like/path");

                        assertNull(actual, "relative paths should not be touched");
                    }
                }

                @Test
                void pathsWithAQuestionMark() throws URISyntaxException {
                    expected = URI.create("file:/path/to%3Fnot-a-query");

                    actual = PathHelper.normalisePathToURI("/path/to?not-a-query");

                    assertEquals(expected, actual, "question marks should be escaped");
                }

                @Test
                void pathsWithAHashMark() throws URISyntaxException {
                    expected = URI.create("file:/path/to%23not-a-fragment");

                    actual = PathHelper.normalisePathToURI("/path/to#not-a-fragment");

                    assertEquals(expected, actual, "hash marks should be escaped");
                }
            }

            URI expected;
            URI actual;

            @Test
            void tripleSlashFileUrisAreReturnedUnchanged() throws URISyntaxException {
                String uri = "file:///spaced%20name/test.xsl";
                expected = URI.create(uri);

                actual = PathHelper.normalisePathToURI(uri);

                assertEquals(expected, actual, "file URI should be returned unchanged");

            }

            @Test
            void singleSlashFileUrisAreReturnedUnchanged() throws URISyntaxException {
                String uri = "file:/spaced%20name/test.xsl";

                expected = URI.create(uri);

                actual = PathHelper.normalisePathToURI(uri);

                assertEquals(expected, actual, "file URI should be returned unchanged");
            }
        }
    }
}