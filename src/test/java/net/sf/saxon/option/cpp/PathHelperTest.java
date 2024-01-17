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

import static net.sf.saxon.test.assertions.SaxonMatcherAssert.assertThat;
import static org.hamcrest.Matchers.*;

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
            String cwd = "Z:\\path\\to\\cwd";
            String forwardSlashCwd = "Z:/path/to/cwd";

            @Nested
            @TestInstance(TestInstance.Lifecycle.PER_CLASS)
            class WithCatalogResolverFailure {
                @BeforeAll
                void setResolver() {
                    config.setResourceResolver(new NullResolver());
                }

                @Test
                @DisplayName("a simple filename-only relative path produces a Source")
                void relativePathProducesSource() throws SaxonApiException {
                    expected = "file:///Z:/path/to/cwd/source.xml";

                    Source result = PathHelper.resolveToSource(config, cwd, "source.xml", nature);
                    actual = result.getSystemId();

                    assertThat(actual, equalTo(expected));
                }

                @Test
                @DisplayName("a relative path with directories in it using the windows path separator produces a Source")
                void relativePathWithDepthProducesSource() throws SaxonApiException {
                    expected = "file:///Z:/path/to/cwd/folder/source.xml";

                    Source result = PathHelper.resolveToSource(config, cwd, "folder\\source.xml", nature);
                    actual = result.getSystemId();

                    assertThat(actual, equalTo(expected));
                }

                @Test
                @DisplayName("absolute paths using forward slash produce a source")
                void absolutePathProducesSource() throws SaxonApiException {
                    expected = "file:/Z:/new/path/to/source.xml";

                    Source result = PathHelper.resolveToSource(config, cwd, "Z:/new/path/to/source.xml", nature);
                    actual = result.getSystemId();

                    assertThat(actual, equalTo(expected));
                }

                @Test
                @DisplayName("absolute paths using backslash produce a source")
                void absolutePathWithBackslashProducesSource() throws SaxonApiException {
                    expected = "file:/Z:/new/path/to/source.xml";

                    Source result = PathHelper.resolveToSource(config, cwd, "Z:\\new\\path\\to\\source.xml", nature);
                    actual = result.getSystemId();

                    assertThat(actual, equalTo(expected));
                }

                @Test
                @DisplayName("Fully-qualified URIs produce a source")
                void uriProducesSource() throws SaxonApiException {
                    expected = "http://example.org/path/to/source.xml";

                    Source result = PathHelper.resolveToSource(config, cwd, "http://example.org/path/to/source.xml", nature);
                    actual = result.getSystemId();

                    assertThat(actual, equalTo(expected));
                }
            }

            @Nested
            @TestInstance(TestInstance.Lifecycle.PER_CLASS)
            class WithCatalogResolverSuccess {
                public String input;
                public final URI resolverSourceURI = URI.create("http://example.org/source");
                public final MockResolver mockResolver = new MockResolver(resolverSourceURI);

                @BeforeAll
                void setResolver() {
                    config.setResourceResolver(mockResolver);
                }
                
                @Test
                @DisplayName("relative paths produce a source resolved against the absolute path created by resolving the input against the CWD")
                void relativePathProducesSource() throws SaxonApiException {
                    input = "source.xml";
                    expected = mockResolver.resolvedMockUri("file:///" + forwardSlashCwd + "/" + input);

                    Source result = PathHelper.resolveToSource(config, cwd, input, nature);
                    actual = result.getSystemId();

                    assertThat(actual, equalTo(expected));
                }

                @Test
                @DisplayName("absolute paths (with forward slash as separator) produce a source")
                void absolutePathProducesSource() throws SaxonApiException {
                    input = "Z:/new/path/to/source.xml";
                    expected = mockResolver.resolvedMockUri("file:/" + input);

                    Source result = PathHelper.resolveToSource(config, cwd, input, nature);
                    actual = result.getSystemId();

                    assertThat(actual, equalTo(expected));
                }

                @Test
                @DisplayName("absolute paths (with backslash as separator) produce a source")
                void absolutePathWithBackslashProducesSource() throws SaxonApiException {
                    input = "Z:\\new\\path\\to\\source.xml";
                    expected = mockResolver.resolvedMockUri("file:/Z:/new/path/to/source.xml");

                    Source result = PathHelper.resolveToSource(config, cwd, input, nature);
                    actual = result.getSystemId();

                    assertThat(actual, equalTo(expected));
                }

                @Test
                @DisplayName("Fully-qualified URIs produce a source")
                void uriProducesSource() throws SaxonApiException {
                    input = "http://new/path/to/source.xml";
                    expected = mockResolver.resolvedMockUri(input);

                    Source result = PathHelper.resolveToSource(config, cwd, input, nature);
                    actual = result.getSystemId();

                    assertThat(actual, equalTo(expected));
                }
            }
        }

        @Nested
        @DisabledOnOs(OS.WINDOWS)
        class UnixishOnly {
            String cwd = "/path/to/cwd";

            @Nested
            @TestInstance(TestInstance.Lifecycle.PER_CLASS)
            class WithCatalogResolverFailure {
                @BeforeAll
                void setResolver() {
                    config.setResourceResolver(new NullResolver());
                }

                @Test
                @DisplayName("relative paths produce a source resolved against the CWD")
                void relativePathProducesSource() throws SaxonApiException {
                    expected = "file:///path/to/cwd/source.xml";

                    Source result = PathHelper.resolveToSource(config, cwd, "source.xml", nature);
                    actual = result.getSystemId();

                    assertThat(actual, equalTo(expected));
                }

                @Test
                @DisplayName("absolute paths produce a source")
                void absolutePathProducesSource() throws SaxonApiException {
                    expected = "file:/new/path/to/source.xml";

                    Source result = PathHelper.resolveToSource(config, cwd, "/new/path/to/source.xml", nature);
                    actual = result.getSystemId();

                    assertThat(actual, equalTo(expected));
                }

                @Test
                @DisplayName("Fully-qualified URIs produce a source")
                void uriProducesSource() throws SaxonApiException {
                    expected = "http://new/path/to/source.xml";

                    Source result = PathHelper.resolveToSource(config, cwd, "http://new/path/to/source.xml", nature);
                    actual = result.getSystemId();

                    assertThat(actual, equalTo(expected));
                }
            }

            @Nested
            @TestInstance(TestInstance.Lifecycle.PER_CLASS)
            class WithCatalogResolverSuccess {
                public String input;
                public final URI resolverSourceURI = URI.create("http://example.org/source");
                public final MockResolver mockResolver = new MockResolver(resolverSourceURI);
                @BeforeAll
                void setResolver() {
                    config.setResourceResolver(mockResolver);
                }

                @Test
                @DisplayName("relative paths produce a source resolved against the absolute path created by resolving the input against the CWD")
                void relativePathProducesSource() throws SaxonApiException {
                    input = "source.xml";
                    expected = mockResolver.resolvedMockUri("file://" + cwd + "/" + input);

                    Source result = PathHelper.resolveToSource(config, cwd, input, nature);
                    actual = result.getSystemId();

                    assertThat(actual, equalTo(expected));
                }

                @Test
                @DisplayName("absolute paths produce a source")
                void absolutePathProducesSource() throws SaxonApiException {
                    input = "/new/path/to/source.xml";
                    expected = mockResolver.resolvedMockUri("file:" + input);

                    Source result = PathHelper.resolveToSource(config, cwd, input, nature);
                    actual = result.getSystemId();

                    assertThat(actual, equalTo(expected));
                }

                @Test
                @DisplayName("Fully-qualified URIs produce a source")
                void uriProducesSource() throws SaxonApiException {
                    input = "http://new/path/to/source.xml";
                    expected = mockResolver.resolvedMockUri(input);

                    Source result = PathHelper.resolveToSource(config, cwd, input, nature);
                    actual = result.getSystemId();

                    assertThat(actual, equalTo(expected));
                }
            }
        }
    }

    @Nested
    class ResolvingToURI {
        URI expected;
        URI actual;
        String nature = ResourceRequest.XML_NATURE;

        @Nested
        @EnabledOnOs(OS.WINDOWS)
        class WindowsOnly {
            String cwd = "Z:\\path\\to\\cwd";

            @Test
            void relativePathProducesURI() throws SaxonApiException {
                expected = URI.create("file:///Z:/path/to/cwd/source.xml");

                URI actual = PathHelper.resolveToURI(config, cwd, "source.xml");

                assertThat(actual, equalTo(expected));
            }

            @Test
            void relativePathWithSpacesProducesURI() throws SaxonApiException {
                expected = URI.create("file:///Z:/path/to/cwd/spaced%20name/test.xsl");
                actual = PathHelper.resolveToURI(config, cwd,"spaced name\\test.xsl");

                assertThat("paths should be correctly percent-encoded", actual, equalTo(expected));
            }

            @Test
            void absolutePathProducesURI() throws SaxonApiException {
                expected = URI.create("file:///X:/other/path/to/source.xml");

                URI actual = PathHelper.resolveToURI(config, cwd, "X:\\other\\path\\to\\source.xml");

                assertThat(actual, equalTo(expected));
            }

            @Test
            void fileUriIsReturnedUnchanged() throws SaxonApiException {
                expected = URI.create("file:///X:/other/path/to/source.xml");

                URI actual = PathHelper.resolveToURI(config, cwd, expected.toASCIIString());

                assertThat(actual, equalTo(expected));
            }
        }

        @Nested
        @DisabledOnOs(OS.WINDOWS)
        class UnixishOnly {
            String cwd = "/path/to/cwd";
            @Test
            void relativePathProducesURI() throws SaxonApiException {
                expected = URI.create("file:///path/to/cwd/source.xml");

                URI actual = PathHelper.resolveToURI(config, cwd, "source.xml");

                assertThat(actual, equalTo(expected));
            }

            @Test
            void relativePathWithSpacesProducesURI() throws SaxonApiException {
                expected = URI.create("file:///path/to/cwd/spaced%20name/test.xsl");
                actual = PathHelper.resolveToURI(config, cwd,"spaced name/test.xsl");

                assertThat("paths should be correctly percent-encoded", actual, equalTo(expected));
            }

            @Test
            void absolutePathProducesURI() throws SaxonApiException {
                expected = URI.create("file:///other/path/to/source.xml");

                URI actual = PathHelper.resolveToURI(config, "", "/other/path/to/source.xml");

                assertThat(actual, equalTo(expected));
            }

            @Test
            void fileUriIsReturnedUnchanged() throws SaxonApiException {
                expected = URI.create("file:///other/path/to/source.xml");

                URI actual = PathHelper.resolveToURI(config, cwd, expected.toASCIIString());

                assertThat(actual, equalTo(expected));
            }
        }

        @Test
        void httpUriIsReturnedUnchanged() throws SaxonApiException {
            expected = URI.create("http://example.org/source.xml");

            URI actual = PathHelper.resolveToURI(config, null, expected.toASCIIString());

            assertThat(actual, equalTo(expected));
        }
    }

    @Nested
    class PathNormalisation {
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

                    assertThat("paths should be normalised correctly", actual, equalTo(expected));
                }

                @Test
                void relativePathWithSpaces() throws URISyntaxException {
                    actual = PathHelper.normalisePathToURI("spaced name\\test.xsl");

                    assertThat("relative paths should not be touched", actual, is(nullValue()));
                }

                @Test
                void looksLikeAUnixPath() throws URISyntaxException {
                    actual = PathHelper.normalisePathToURI("/unix/like/path");

                    assertThat("relative paths should not be touched", actual, is(nullValue()));
                }

                @Test
                void pathsWithAQuestionMark() throws URISyntaxException {
                    expected = URI.create("file:///Z:/path/to%3Fnot-a-query");

                    actual = PathHelper.normalisePathToURI("Z:\\path\\to?not-a-query");

                    assertThat("question marks should be escaped", actual, equalTo(expected));
                }

                @Test
                void pathsWithAHashMark() throws URISyntaxException {
                    expected = URI.create("file:///Z:/path/to%23not-a-fragment");

                    actual = PathHelper.normalisePathToURI("Z:\\path\\to#not-a-fragment");

                    assertThat("hash marks should be escaped", actual, equalTo(expected));
                }
            }

            @Nested
            @DisabledOnOs(OS.WINDOWS)
            class UnixishOnly {
                @Test
                void absolutePathWithSpaces() throws URISyntaxException {
                    expected = URI.create("file:/spaced%20name/test.xsl");

                    actual = PathHelper.normalisePathToURI("/spaced name/test.xsl");

                    assertThat("absolute paths should be normalised into file URIs", actual, equalTo(expected));
                }

                @Test
                void relativePathWithSpaces() throws URISyntaxException {
                    actual = PathHelper.normalisePathToURI("spaced name/test.xsl");

                    assertThat("relative paths should not be touched", actual, is(nullValue()));
                }

                @Test
                void looksLikeAWindowsPath() throws URISyntaxException {
                    actual = PathHelper.normalisePathToURI("C:/windows/like/path");

                    assertThat("relative paths should not be touched", actual, is(nullValue()));
                }

                @Test
                void pathsWithAQuestionMark() throws URISyntaxException {
                    expected = URI.create("file:/path/to%3Fnot-a-query");

                    actual = PathHelper.normalisePathToURI("/path/to?not-a-query");

                    assertThat("question marks should be escaped", actual, equalTo(expected));
                }

                @Test
                void pathsWithAHashMark() throws URISyntaxException {
                    expected = URI.create("file:/path/to%23not-a-fragment");

                    actual = PathHelper.normalisePathToURI("/path/to#not-a-fragment");

                    assertThat("hash marks should be escaped", actual, equalTo(expected));
                }
            }
        }

        @Nested
        class PathsThatAreURIs {
            URI expected;
            URI actual;

            @Test
            void tripleSlashFileUrisAreReturnedUnchanged() throws URISyntaxException {
                String uri = "file:///spaced%20name/test.xsl";
                expected = URI.create(uri);

                actual = PathHelper.normalisePathToURI(uri);

                assertThat("file URI should be returned unchanged", actual, equalTo(expected));
            }

            @Test
            void singleSlashFileUrisAreReturnedUnchanged() throws URISyntaxException {
                String uri = "file:/spaced%20name/test.xsl";

                expected = URI.create(uri);

                actual = PathHelper.normalisePathToURI(uri);

                assertThat("file URI should be returned unchanged", actual, equalTo(expected));
            }
        }
    }
}