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
import java.net.URISyntaxException;
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
                @DisabledOnOs(OS.WINDOWS)
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

        @Test
        @DisplayName("returns a URI not a Source")
        void passesThroughToResolveToSourceAndReturnsSystemId() throws SaxonApiException {
            expected = URI.create(subject.resolveToSource(config, cwd, "source.xml", nature).getSystemId());

            actual = subject.resolveToURI(config, cwd, "source.xml");

            assertThat(actual, equalTo(expected));
        }
    }

    @Nested
    class ResolvingCwdToBaseURI {
        URI expected;
        URI actual;

        @Nested
        @EnabledOnOs(OS.WINDOWS)
        @DisplayName("On Windows")
        class WindowsOnly {
            @Test
            @DisplayName("handles a standard drive:folder path")
            void handlesWindowsPath() throws SaxonApiException {
                expected = URI.create("file:///C/path/to/dir/");

                actual = subject.cwdToBaseURI("C:\\path\\to\\dir");

                assertThat(actual, equalTo(expected));
            }

            @Test
            @DisplayName("Handles a UNC path")
            void handlesUncPath() throws SaxonApiException {
                expected = URI.create("file://thing/share/path/to/dir/");

                actual = subject.cwdToBaseURI("\\\\thing\\share\\path\\to\\dir");

                assertThat(actual, equalTo(expected));
            }
        }

//        @Nested
//        @DisabledOnOs(OS.WINDOWS)
//        @DisplayName("On Linux/macOS")
//
//        class UnicesOnly {
//
//        }
        @Test
        @DisplayName("converts an absolute path with a trailing slash to an absolute URI with a trailing slash")
        void absPathWithSlashBecomesAbsURIWithTrailingSlash() throws SaxonApiException {
            expected = URI.create("file:///path/to/dir/");

            actual = subject.cwdToBaseURI("/path/to/dir");

            assertThat(actual, equalTo(expected));
        }

        @Test
        @DisplayName("converts an absolute path without a trailing slash to an absolute URI with a trailing slash")
        void absPathBecomesAbsURIWithTrailingSlash() throws SaxonApiException {
            expected = URI.create("file:///path/to/dir/");

            actual = subject.cwdToBaseURI("/path/to/dir/");

            assertThat(actual, equalTo(expected));
        }
    }
}