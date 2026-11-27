package internaltests;

import net.sf.saxon.s9api.Processor;
import net.sf.saxon.s9api.XQueryCompiler;
import org.junit.jupiter.api.Test;
import org.junit.jupiter.api.io.TempDir;

import java.net.URI;
import java.nio.charset.StandardCharsets;
import java.nio.file.Files;
import java.nio.file.Path;
import java.util.Comparator;
import java.util.zip.ZipEntry;
import java.util.zip.ZipOutputStream;

/** Real doc() retrieval: raw IRIs and their UTF-8 URI controls identify the same XML. */
public class DocumentIriTest {
    private static final String XML = "<probe>α β</probe>";
    @TempDir
    public Path directory;

    private Path input(String folder) throws Exception {
        return input(folder, "α β");
    }

    private Path input(String folder, String text) throws Exception {
        Path file = directory.resolve(folder).resolve("input.xml");
        Files.createDirectories(file.getParent());
        Files.write(file, ("<probe>" + text + "</probe>").getBytes(StandardCharsets.UTF_8));
        return file;
    }

    private void read(String href, URI base) throws Exception {
        read(href, base, "α β");
    }

    private void read(String href, URI base, String expected) throws Exception {
        XQueryCompiler compiler = new Processor(false).newXQueryCompiler();
        compiler.setBaseURI(base);
        String literal = href.replace("&", "&amp;").replace("'", "''");
        String result = compiler.compile("doc('" + literal + "')/probe/string()")
                .load().evaluateSingle().getStringValue();
        if (!expected.equals(result)) {
            throw new AssertionError("Expected fixture text " + expected + ", got " + result);
        }
    }

    @Test
    public void rawGreekAbsoluteIri() throws Exception {
        String iri = input("uri α").toUri().toASCIIString().replace("%CE%B1", "α");
        if (!iri.contains("uri%20α/")) {
            throw new AssertionError("The regression must pass raw Greek with an encoded space: " + iri);
        }
        read(iri, directory.toUri());
    }

    @Test
    public void utf8EncodedGreekControl() throws Exception {
        read(input("uri α").toUri().toASCIIString(), directory.toUri());
    }

    @Test
    public void rawSpacesAndGreek() throws Exception {
        read(input("uri α").toUri().toASCIIString().replace("%CE%B1", "α").replace("%20", " "),
                directory.toUri());
    }

    @Test
    public void relativeGreekIri() throws Exception {
        input("uri α");
        read("uri%20α/input.xml", directory.toUri());
    }

    @Test
    public void supplementaryCodepoint() throws Exception {
        Path file = input("uri \uD83D\uDE00");
        read(file.toUri().toASCIIString().replace("%F0%9F%98%80", "\uD83D\uDE00"), directory.toUri());
        read(file.toUri().toASCIIString(), directory.toUri());
    }

    @Test
    public void unicodeBaseWithAsciiReference() throws Exception {
        Path file = input("uri α");
        read("input.xml", URI.create(file.getParent().toUri().toASCIIString().replace("%CE%B1", "α")));
    }

    @Test
    public void reservedCharactersAndExistingEscapes() throws Exception {
        // %23, %3F and %25 must retain their meaning; + ; & = @ are path characters.
        read(input("uri #?%;+&=@").toUri().toASCIIString(), directory.toUri());
    }

    @Test
    public void relativeDocumentInJar() throws Exception {
        Path jar = directory.resolve("documents.jar");
        try (ZipOutputStream zip = new ZipOutputStream(Files.newOutputStream(jar))) {
            zip.putNextEntry(new ZipEntry("nested/child/input.xml"));
            zip.write(XML.getBytes(StandardCharsets.UTF_8));
            zip.closeEntry();
        }
        // Keep the URIUtils jar: resolution fix: java.net.URI.resolve alone loses this base.
        read("child/input.xml", URI.create("jar:" + jar.toUri().toASCIIString() + "!/nested/query.xq"));
    }

    private void readNormalizationForms(String prefix, URI base) throws Exception {
        read(prefix + "\u03B1\u0301/input.xml", base, "decomposed");
        read(prefix + "%CE%B1%CC%81/input.xml", base, "decomposed");
        read(prefix + "\u03AC/input.xml", base, "composed");
        read(prefix + "%CE%AC/input.xml", base, "composed");
    }

    @Test
    public void distinctNormalizationFormsAbsolute() throws Exception {
        input("\u03B1\u0301", "decomposed");
        input("\u03AC", "composed");
        readNormalizationForms(directory.toUri().toASCIIString(), directory.toUri());
    }

    @Test
    public void distinctNormalizationFormsRelative() throws Exception {
        input("\u03B1\u0301", "decomposed");
        input("\u03AC", "composed");
        readNormalizationForms("", directory.toUri());
    }

    @Test
    public void distinctNormalizationFormsInJar() throws Exception {
        Path jar = directory.resolve("normalization.jar");
        try (ZipOutputStream zip = new ZipOutputStream(Files.newOutputStream(jar))) {
            zip.putNextEntry(new ZipEntry("nested/\u03B1\u0301/input.xml"));
            zip.write("<probe>decomposed</probe>".getBytes(StandardCharsets.UTF_8));
            zip.closeEntry();
            zip.putNextEntry(new ZipEntry("nested/\u03AC/input.xml"));
            zip.write("<probe>composed</probe>".getBytes(StandardCharsets.UTF_8));
            zip.closeEntry();
        }
        readNormalizationForms("", URI.create("jar:" + jar.toUri().toASCIIString() + "!/nested/query.xq"));
    }

    private interface Check { void run() throws Exception; }

    private static int check(String name, Check test) {
        try {
            test.run();
            System.out.println("PASS " + name);
            return 0;
        } catch (Exception | AssertionError err) {
            System.err.println("FAIL " + name);
            err.printStackTrace();
            return 1;
        }
    }

    /** Run the identical cases against the packaged JAR on Java 17, without JUnit at runtime. */
    public static void main(String[] args) throws Exception {
        if (!"17".equals(System.getProperty("java.specification.version"))) {
            throw new AssertionError("This runtime probe requires Java 17");
        }
        DocumentIriTest test = new DocumentIriTest();
        test.directory = Files.createTempDirectory("saxon-document-iri-");
        int failed;
        try {
            failed = check("rawGreekAbsoluteIri", test::rawGreekAbsoluteIri)
                    + check("utf8EncodedGreekControl", test::utf8EncodedGreekControl)
                    + check("rawSpacesAndGreek", test::rawSpacesAndGreek)
                    + check("relativeGreekIri", test::relativeGreekIri)
                    + check("supplementaryCodepoint", test::supplementaryCodepoint)
                    + check("unicodeBaseWithAsciiReference", test::unicodeBaseWithAsciiReference)
                    + check("reservedCharactersAndExistingEscapes", test::reservedCharactersAndExistingEscapes)
                    + check("relativeDocumentInJar", test::relativeDocumentInJar)
                    + check("distinctNormalizationFormsAbsolute", test::distinctNormalizationFormsAbsolute)
                    + check("distinctNormalizationFormsRelative", test::distinctNormalizationFormsRelative)
                    + check("distinctNormalizationFormsInJar", test::distinctNormalizationFormsInJar);
        } finally {
            try (java.util.stream.Stream<Path> paths = Files.walk(test.directory)) {
                for (Path path : (Iterable<Path>) paths.sorted(Comparator.reverseOrder())::iterator) {
                    Files.delete(path);
                }
            }
        }
        if (failed != 0) {
            throw new AssertionError(failed + " document IRI cases failed");
        }
    }
}
