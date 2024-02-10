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
import org.junit.jupiter.api.io.TempDir;

import javax.xml.transform.Source;

import java.io.IOException;
import java.net.URI;
import java.nio.file.Files;
import java.nio.file.Path;
import java.nio.file.Paths;
import java.util.function.Function;

import static net.sf.saxon.test.assertions.SaxonMatcherAssert.assertThat;
import static org.hamcrest.Matchers.*;
import static org.junit.jupiter.api.Assertions.assertThrows;

class PathHelperTest {
    Configuration config = new Configuration();
    String cwd = System.getProperty("java.io.tmpdir");
    Function<Path, Boolean> pathVerifier = (path) -> true;
    String nature = ResourceRequest.XML_NATURE;
    PathHelper subject = new PathHelper(pathVerifier);

    @Nested
    @DisplayName("verifying paths can be resolved on a local filesystem")
    class FsPathVerification {
        @Test
        @DisplayName("Absolute paths that cannot be resolved throw a SaxonApiException")
        void badPathsExplode(@TempDir Path tmpDir) {
            subject = new PathHelper();
            String cwd = tmpDir.toString();

            assertThrows(SaxonApiException.class, () -> { subject.resolveToSource(config, cwd, "404.xml", nature); });
        }

        @Test
        @DisplayName("Absolute paths that can be resolved don't throw")
        void goodPathsDoNotExplode(@TempDir Path tmpDir) throws SaxonApiException, IOException {
            subject = new PathHelper();
            String cwd = tmpDir.toString();
            Path expected = tmpDir.resolve("200.xml");
            Files.createFile(expected);

            Source actual = subject.resolveToSource(config, cwd, "200.xml", nature);

            assertThat(actual.getSystemId(), equalTo(expected.toUri().toString()));
        }

    }

    @Nested
    @DisplayName("Resolving Paths and URIs to Sources with resolveToSource()")
    class ResolvingToSource {
        URI expected;
        URI actual;
        Path cwdPath = Paths.get(cwd);

        @Nested
        @DisplayName("Non-URIs are resolved as filesystem paths")
        class NonURIs {
            @Test
            @DisplayName("relative paths that cannot be resolved throw a SaxonApiException")
            void relativePathsMustBeResolvable() {
                subject = new PathHelper((path) -> false);
                assertThrows(SaxonApiException.class, () -> { subject.resolveToSource(config, cwd, "404.xml", nature); });
            }

            @Test
            @DisplayName("a simple filename-only relative path produces a Source")
            void relativePathProducesSource() throws SaxonApiException {
                expected = cwdPath.toUri().resolve("source.xml");

                Source result = subject.resolveToSource(config, cwd, "source.xml", nature);
                actual = URI.create(result.getSystemId());
                assertThat(actual, equalTo(expected));
            }

            @Nested
            @EnabledOnOs(OS.WINDOWS)
            @DisplayName("On Windows")
            class WindowsOnly {
                @Test
                @DisplayName("a relative path with directories in it using the windows path separator produces a Source")
                void relativePathWithDepthProducesSource() throws SaxonApiException {
                    expected = cwdPath.toUri().resolve("folder/source.xml");

                    Source result = subject.resolveToSource(config, cwd, "folder\\source.xml", nature);
                    actual = URI.create(result.getSystemId());

                    assertThat(actual, equalTo(expected));
                }

                @Test
                @DisplayName("absolute paths using forward slash produce a source")
                void absolutePathProducesSource() throws SaxonApiException {
                    expected = cwdPath.toUri().resolve("folder/source.xml");

                    Source result = subject.resolveToSource(config, cwd, cwdPath.resolve("folder/source.xml").toString(), nature);
                    actual = URI.create(result.getSystemId());

                    assertThat(actual, equalTo(expected));
                }

                @Test
                @DisplayName("absolute paths using backslash produce a source")
                void absolutePathWithBackslashProducesSource() throws SaxonApiException {
                    expected = cwdPath.toUri().resolve("source.xml");

                    Source result = subject.resolveToSource(config, cwd, cwdPath.resolve("source.xml").toString(), nature);
                    actual = URI.create(result.getSystemId());

                    assertThat(actual, equalTo(expected));
                }

                @Test
                @DisplayName("Absolute paths that cannot be resolved throw a SaxonApiException")
                void absolutePathsMustExist() {
                    subject = new PathHelper((path) -> false);
                    assertThrows(SaxonApiException.class, () -> { subject.resolveToSource(config, cwd, "Z:\\Nonsense\\404.xml", nature); });
                }

                @Test
                @DisplayName("Absolute paths (using forward slashes) that cannot be resolved throw a SaxonApiException")
                void absolutePathsUsingForwardSlashesMustExist() {
                    subject = new PathHelper((path) -> false);
                    assertThrows(SaxonApiException.class, () -> { subject.resolveToSource(config, cwd, "Z:/Nonsense/404.xml", nature); });
                }
            }

            @Nested
            @DisabledOnOs(OS.WINDOWS)
            @DisplayName("On macOS and Linux")
            class UnixishOnly {
                @Test
                @DisplayName("a relative path with directories in it produces a Source")
                void relativePathWithDepthProducesSource() throws SaxonApiException {
                    expected = cwdPath.toUri().resolve("folder/source.xml");

                    Source result = subject.resolveToSource(config, cwd, "folder/source.xml", nature);
                    actual = URI.create(result.getSystemId());

                    assertThat(actual, equalTo(expected));
                }

                @Test
                @DisplayName("a relative path with spaces in it produces a Source")
                void relativePathWithSpacesProducesSource() throws SaxonApiException {
                    expected = cwdPath.toUri().resolve("space%20me/source.xml");

                    Source result = subject.resolveToSource(config, cwd, "space me/source.xml", nature);
                    actual = URI.create(result.getSystemId());

                    assertThat(actual, equalTo(expected));
                }

                @Test
                @DisplayName("absolute paths produce a source")
                void absolutePathProducesSource() throws SaxonApiException {
                    expected = cwdPath.toUri().resolve("source.xml");

                    Source result = subject.resolveToSource(config, cwd, cwdPath.resolve("source.xml").toString(), nature);
                    actual = URI.create(result.getSystemId());

                    assertThat(actual, equalTo(expected));
                }

                @Test
                @DisplayName("Absolute paths that cannot be resolved throw a SaxonApiException")
                void absolutePathsMustExist() {
                    subject = new PathHelper((path) -> false);
                    assertThrows(SaxonApiException.class, () -> { subject.resolveToSource(config, cwd, "/path/to/404.xml", nature); });
                }
            }

            @Nested
            @DisplayName("Dealing with path characters that need percent-encoding in a URI")
            class PercentEncoding {
                Path input;

                @Test
                @DisplayName("Absolute paths with spaces are correctly resolved and encoded")
                void absolutePathWithSpaces() throws SaxonApiException {
                    input = cwdPath.resolve("space me").resolve("source.xml");
                    expected = input.toUri();

                    Source result = subject.resolveToSource(config, cwd, input.toString(), nature);
                    actual = URI.create(result.getSystemId());

                    assertThat(actual, equalTo(expected));
                    assertThat("The space should have been percent-encoded", actual.toString(), containsString("space%20me"));
                }

                @Test
                @DisplayName("a relative path with spaces in it is correctly resolved and encoded")
                void relativePathWithSpacesProducesSource() throws SaxonApiException {
                    input = Paths.get("space me","source.xml");
                    expected = cwdPath.toUri().resolve("space%20me/source.xml");

                    Source result = subject.resolveToSource(config, cwd, input.toString(), nature);
                    actual = URI.create(result.getSystemId());

                    assertThat(actual, equalTo(expected));
                }

                @Test
                @DisplayName("Paths with a question mark in get that percent-encoded, not treated as the start of the query string")
                void pathsWithAQuestionMark() throws SaxonApiException {
                    input = cwdPath.resolve("to?not-a-query.xml");
                    expected = input.toUri();

                    Source result = subject.resolveToSource(config, cwd, input.toString(), nature);
                    actual = URI.create(result.getSystemId());

                    assertThat(actual, equalTo(expected));
                    assertThat("The question mark should have been percent-encoded", actual.toString(), containsString("to%3Fnot-a-query"));
                }

                @Test
                @DisplayName("Paths with a hash mark in get that percent-encoded, not treated as the start of the fragment identifier")
                void pathsWithAHashMark() throws SaxonApiException {
                    assertThat("hash marks should be escaped", actual, equalTo(expected));
                    input = cwdPath.resolve("to#not-a-fragment.xml");
                    expected = input.toUri();

                    Source result = subject.resolveToSource(config, cwd, input.toString(), nature);
                    actual = URI.create(result.getSystemId());

                    assertThat(actual, equalTo(expected));
                    assertThat("The hash mark should have been percent-encoded", actual.toString(), containsString("to%23not-a-fragment"));
                }
            }

            @Nested
            @DisplayName("URIs are resolved through the catalog resolver")
            class URIs {
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
                    @DisplayName("HTTP URIs produce a source")
                    void httpUriProducesSource() throws SaxonApiException {
                        input = "http://new/path/to/source.xml";
                        expected = mockResolver.resolvedMockUri(input);

                        Source result = subject.resolveToSource(config, cwd, input, nature);
                        actual = URI.create(result.getSystemId());

                        assertThat(actual, equalTo(expected));
                    }

                    @Test
                    @DisplayName("File :/// URIs produce a source")
                    void fileUriProducesSource() throws SaxonApiException {
                        input = "file:///new/path/to/source.xml";
                        expected = mockResolver.resolvedMockUri(input);

                        Source result = subject.resolveToSource(config, cwd, input, nature);
                        actual = URI.create(result.getSystemId());

                        assertThat(actual, equalTo(expected));
                    }

                    @Test
                    @DisplayName("File :/ URIs produce a source")
                    void fileOneSlashUriProducesSource() throws SaxonApiException {
                        input = "file:/new/path/to/source.xml";
                        expected = mockResolver.resolvedMockUri(input);

                        Source result = subject.resolveToSource(config, cwd, input, nature);
                        actual = URI.create(result.getSystemId());

                        assertThat(actual, equalTo(expected));
                    }

                    @Test
                    @DisplayName("Windows File :///C:/ URIs produce a source")
                    void windowsFileUriProducesSource() throws SaxonApiException {
                        input = "file:///Z:/new/path/to/source.xml";
                        expected = mockResolver.resolvedMockUri(input);

                        Source result = subject.resolveToSource(config, cwd, input, nature);
                        actual = URI.create(result.getSystemId());

                        assertThat(actual, equalTo(expected));
                    }

                    @Test
                    @DisplayName("Windows File :/C:/ URIs produce a source")
                    void windowsOneSlashFileUriProducesSource() throws SaxonApiException {
                        input = "file:/Z:/new/path/to/source.xml";
                        expected = mockResolver.resolvedMockUri(input);

                        Source result = subject.resolveToSource(config, cwd, input, nature);
                        actual = URI.create(result.getSystemId());

                        assertThat(actual, equalTo(expected));
                    }
                }

                @Nested
                @TestInstance(TestInstance.Lifecycle.PER_CLASS)
                class WithCatalogResolverFailure {
                    @BeforeAll
                    void setResolver() {
                        config.setResourceResolver(new NullResolver());
                    }

                    @Test
                    @DisplayName("Fully-qualified URIs produce a source")
                    void uriProducesSource() throws SaxonApiException {
                        expected = URI.create("http://example.org/new/path/to/source.xml");

                        Source result = subject.resolveToSource(config, cwd, "http://example.org/new/path/to/source.xml", nature);
                        actual = URI.create(result.getSystemId());

                        assertThat(actual, equalTo(expected));
                    }

                    @Test
                    @DisplayName("File :/ URIs produce a source")
                    void fileOneSlashUriProducesSource() throws SaxonApiException {
                        expected = URI.create("file:/new/path/to/source.xml");

                        Source result = subject.resolveToSource(config, cwd, "file:/new/path/to/source.xml", nature);
                        actual = URI.create(result.getSystemId());

                        assertThat(actual, equalTo(expected));
                    }
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
}