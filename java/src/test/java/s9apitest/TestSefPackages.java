package s9apitest;

import net.sf.saxon.Version;
import net.sf.saxon.s9api.Processor;
import net.sf.saxon.s9api.QName;
import net.sf.saxon.s9api.SaxonApiException;
import net.sf.saxon.s9api.XsltExecutable;
import org.junit.jupiter.api.Test;
import org.junit.jupiter.api.io.TempDir;

import javax.xml.transform.stream.StreamSource;
import java.io.ByteArrayInputStream;
import java.io.ByteArrayOutputStream;
import java.io.StringReader;
import java.nio.charset.StandardCharsets;
import java.nio.file.Files;
import java.nio.file.Path;

import static org.junit.jupiter.api.Assertions.assertEquals;
import static org.junit.jupiter.api.Assertions.assertNotEquals;
import static org.junit.jupiter.api.Assertions.assertThrows;
import static org.junit.jupiter.api.Assertions.assertTrue;

class TestSefPackages {
    @TempDir
    Path directory;

    @Test
    void saveLoadAndRelinkUsedPackagesAndOverrides() throws Exception {
        String library = """
                <xsl:package name="urn:library" package-version="2.1" version="3.0"
                    xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns:f="urn:components">
                  <xsl:variable name="f:word" select="'library'"/>
                  <xsl:function name="f:private" visibility="private">
                    <xsl:sequence select="$f:word"/>
                  </xsl:function>
                  <xsl:function name="f:label" visibility="public">
                    <xsl:sequence select="f:private()"/>
                  </xsl:function>
                  <xsl:template name="f:emit" visibility="public">
                    <xsl:sequence select="f:label()"/>
                  </xsl:template>
                </xsl:package>
                """;
        String application = """
                <xsl:package name="urn:application" package-version="3.2" version="3.0"
                    xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns:f="urn:components">
                  <xsl:use-package name="urn:library" package-version="2.1">
                    <xsl:override>
                      <xsl:function name="f:label">
                        <xsl:sequence select="concat('override:', xsl:original())"/>
                      </xsl:function>
                    </xsl:override>
                  </xsl:use-package>
                  <xsl:template name="main" visibility="public">
                    <xsl:call-template name="f:emit"/>
                  </xsl:template>
                </xsl:package>
                """;
        var sourceCompiler = new Processor(false).newXsltCompiler();
        var sourceLibrary = sourceCompiler.compilePackage(source(library));
        sourceCompiler.importPackage(sourceLibrary);
        var sourceApplication = sourceCompiler.compilePackage(source(application));
        assertEquals("override:library", callMain(sourceApplication.link()));

        Path libraryFile = directory.resolve("library.sef");
        sourceLibrary.save(libraryFile.toFile());
        var compiler = new Processor(false).newXsltCompiler();
        var loadedLibrary = compiler.loadLibraryPackage(libraryFile.toUri());
        assertEquals("urn:library", loadedLibrary.getName());
        assertEquals("2.1", loadedLibrary.getVersion());
        compiler.importPackage(loadedLibrary);
        assertEquals("override:library", callMain(compiler.compilePackage(source(application)).link()));

        Path applicationFile = directory.resolve("application.sef");
        sourceApplication.save(applicationFile.toFile());
        var loadedApplication = new Processor(false).newXsltCompiler().loadLibraryPackage(applicationFile.toUri());
        assertEquals("urn:application", loadedApplication.getName());
        assertEquals("3.2", loadedApplication.getVersion());
        assertEquals("override:library", callMain(loadedApplication.link()));
        SaxonApiException privateTemplate = assertThrows(SaxonApiException.class,
                () -> loadedApplication.link().load30().callTemplate(new QName("urn:components", "emit")));
        assertEquals("XTDE0040", privateTemplate.getErrorCode().getLocalName());

        Path reexport = directory.resolve("application-reexport.sef");
        loadedApplication.save(reexport.toFile());
        assertEquals("override:library", callMain(
                new Processor(false).newXsltCompiler().loadExecutablePackage(reexport.toUri())));
    }

    @Test
    void preserveOverriddenModeAndNextMatch() throws Exception {
        var compiler = new Processor(false).newXsltCompiler();
        var library = compiler.compilePackage(source("""
                <xsl:package name="urn:modes" version="3.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
                  <xsl:mode name="render" visibility="public" on-no-match="shallow-skip"/>
                  <xsl:template match="item" mode="render">
                    <xsl:sequence select="'base'"/>
                  </xsl:template>
                </xsl:package>
                """));
        compiler.importPackage(library);
        var application = compiler.compilePackage(source("""
                <xsl:package name="urn:mode-application" version="3.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
                  <xsl:use-package name="urn:modes">
                    <xsl:override>
                      <xsl:template match="item" mode="render">
                        <xsl:variable name="original" as="item()*"><xsl:next-match/></xsl:variable>
                        <xsl:sequence select="concat('override:', $original)"/>
                      </xsl:template>
                    </xsl:override>
                  </xsl:use-package>
                  <xsl:template name="main" visibility="public">
                    <xsl:variable name="input" as="element()"><item/></xsl:variable>
                    <xsl:apply-templates select="$input" mode="render"/>
                  </xsl:template>
                </xsl:package>
                """));
        var original = application.link();
        assertEquals("override:base", callMain(original));

        assertEquals("override:base", callMain(reload(export(original))));
    }

    @Test
    void reuseSharedDependencyAcrossTwoPackagePaths() throws Exception {
        var compiler = new Processor(false).newXsltCompiler();
        compiler.importPackage(compiler.compilePackage(source("""
                <xsl:package name="urn:shared" version="3.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
                             xmlns:f="urn:components">
                  <xsl:function name="f:shared" visibility="public"><xsl:sequence select="'shared'"/></xsl:function>
                </xsl:package>
                """)));
        for (String name : new String[]{"left", "right"}) {
            compiler.importPackage(compiler.compilePackage(source("""
                    <xsl:package name="urn:%1$s" version="3.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
                                 xmlns:f="urn:components">
                      <xsl:use-package name="urn:shared"/>
                      <xsl:function name="f:%1$s" visibility="public">
                        <xsl:sequence select="concat('%1$s:', f:shared())"/>
                      </xsl:function>
                    </xsl:package>
                    """.formatted(name))));
        }
        var application = compiler.compilePackage(source("""
                <xsl:package name="urn:both" version="3.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
                             xmlns:f="urn:components">
                  <xsl:use-package name="urn:left"/>
                  <xsl:use-package name="urn:right"/>
                  <xsl:template name="main" visibility="public">
                    <xsl:sequence select="concat(f:left(), '|', f:right())"/>
                  </xsl:template>
                </xsl:package>
                """)).link();
        assertEquals("left:shared|right:shared", callMain(application));

        byte[] sef = export(application);
        assertTrue(new String(sef, StandardCharsets.UTF_8).contains("<packageRef"));
        assertEquals("left:shared|right:shared", callMain(reload(sef)));
    }

    @Test
    void resolveRuntimeResourcesAfterMovingIncludedStylesheets() throws Exception {
        Path project = Files.createDirectories(directory.resolve("project"));
        Path module = Files.createDirectories(project.resolve("module"));
        Path deployment = Files.createDirectories(directory.resolve("deployment"));
        Path main = Files.writeString(project.resolve("main.xsl"), """
                <xsl:stylesheet version="3.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
                  <xsl:include href="module/included.xsl"/>
                </xsl:stylesheet>
                """);
        Path included = Files.writeString(module.resolve("included.xsl"), """
                <xsl:stylesheet version="3.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
                  <xsl:template name="main">
                    <xsl:sequence select="string(doc('data.xml')/value)"/>
                  </xsl:template>
                </xsl:stylesheet>
                """);
        Files.writeString(module.resolve("data.xml"), "<value>compile-time location</value>");
        Files.writeString(deployment.resolve("data.xml"), "<value>deployment location</value>");

        var compiler = new Processor(false).newXsltCompiler();
        Path ordinary = deployment.resolve("ordinary.sef");
        var compiled = compiler.compilePackage(new StreamSource(main.toFile()));
        assertEquals("compile-time location", callMain(compiled.link()));
        compiled.save(ordinary.toFile());

        compiler.setRelocatable(true);
        Path relocatable = deployment.resolve("relocatable.sef");
        compiler.compilePackage(new StreamSource(main.toFile())).save(relocatable.toFile());
        Files.move(main, directory.resolve("main.disabled"));
        Files.move(included, directory.resolve("included.disabled"));

        var targetCompiler = new Processor(false).newXsltCompiler();
        assertEquals("compile-time location", callMain(targetCompiler.loadExecutablePackage(ordinary.toUri())));
        var loaded = targetCompiler.loadLibraryPackage(relocatable.toUri());
        assertEquals("deployment location", callMain(loaded.link()));

        Path nextDeployment = Files.createDirectories(directory.resolve("next-deployment"));
        Files.writeString(nextDeployment.resolve("data.xml"), "<value>second deployment</value>");
        Path reexport = nextDeployment.resolve("reexport.sef");
        loaded.save(reexport.toFile());
        assertEquals("second deployment", callMain(
                new Processor(false).newXsltCompiler().loadExecutablePackage(reexport.toUri())));
    }

    @Test
    void rejectCorruptedChecksum() throws Exception {
        XsltExecutable executable = new Processor(false).newXsltCompiler().compile(source("""
                <xsl:stylesheet version="3.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
                  <xsl:template name="main"><xsl:sequence select="'intact'"/></xsl:template>
                </xsl:stylesheet>
                """));
        String original = new String(export(executable), StandardCharsets.UTF_8);
        String corrupted = original.replace("intact", "modified");
        assertNotEquals(original, corrupted);
        SaxonApiException error = assertThrows(SaxonApiException.class,
                () -> reload(corrupted.getBytes(StandardCharsets.UTF_8)));
        assertEquals("SXPK0002", error.getErrorCode().getLocalName());
        assertTrue(error.getMessage().contains("incorrect checksum"));
    }

    @Test
    void rejectUnknownVersionsAndInvalidBindings() throws Exception {
        assertLoadError(bindingFixture("99", "30", "1", "0"), "SXPK0002",
                "Unsupported HE stylesheet export version: 99");
        assertLoadError(bindingFixture("1", "99", "1", "0"), "SXPK0002",
                "Unsupported XSLT version in stylesheet export: 99");
        assertLoadError(bindingFixture("1", "30", "unparseable", "0"), "SXPK0005",
                "Invalid component binding 'unparseable'");
        assertLoadError(bindingFixture("1", "30", "42", "0"), "SXPK0005",
                "Unresolved reference to component 42");
        assertLoadError(bindingFixture("1", "30", "1", "4"), "SXPK0005",
                "Invalid binding slot 4");
    }

    private static void assertLoadError(byte[] sef, String code, String message) {
        SaxonApiException error = assertThrows(SaxonApiException.class, () -> reload(sef));
        assertEquals(code, error.getErrorCode().getLocalName());
        assertTrue(error.getMessage().contains(message), error::getMessage);
    }

    private static byte[] bindingFixture(String sefVersion, String xsltVersion, String binds, String slot)
            throws Exception {
        var config = new Processor(false).getUnderlyingConfiguration();
        var buffer = new ByteArrayOutputStream();
        var out = config.newExpressionExporter("HE", buffer, config.makeStylesheetPackage());
        out.startElement("package");
        out.emitAttribute("saxonVersion", Version.getProductVersion());
        out.emitAttribute("sefVersion", sefVersion);
        out.emitAttribute("version", xsltVersion);
        out.emitAttribute("packageVersion", "1");
        out.emitAttribute("implicit", "true");
        out.startElement("co");
        out.emitAttribute("id", "0");
        out.emitAttribute("binds", binds);
        out.startElement("template");
        out.emitAttribute("name", "main");
        out.emitAttribute("flags", "os");
        out.emitAttribute("slots", "0");
        out.setChildRole("body");
        out.startElement("callT");
        out.emitAttribute("name", "callee");
        out.emitAttribute("bSlot", slot);
        out.endElement();
        out.endElement();
        out.endElement();
        out.startElement("co");
        out.emitAttribute("id", "1");
        out.emitAttribute("binds", "");
        out.startElement("template");
        out.emitAttribute("name", "callee");
        out.emitAttribute("flags", "os");
        out.emitAttribute("slots", "0");
        out.endElement();
        out.endElement();
        out.endElement();
        out.close();
        return buffer.toByteArray();
    }

    private static String callMain(XsltExecutable executable) throws SaxonApiException {
        return executable.load30().callTemplate(new QName("main")).itemAt(0).getStringValue();
    }

    private static StreamSource source(String stylesheet) {
        return new StreamSource(new StringReader(stylesheet), "urn:test:sef-packages");
    }

    private static byte[] export(XsltExecutable executable) throws SaxonApiException {
        var buffer = new ByteArrayOutputStream();
        executable.export(buffer);
        return buffer.toByteArray();
    }

    private static XsltExecutable reload(byte[] sef) throws SaxonApiException {
        return new Processor(false).newXsltCompiler().loadExecutablePackage(
                new StreamSource(new ByteArrayInputStream(sef)));
    }
}
