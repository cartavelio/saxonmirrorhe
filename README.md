# Saxon-HE

[![SaxonJ-HE build and tests on main](https://github.com/cartavelio/saxonmirrorhe/actions/workflows/saxonj-he.yml/badge.svg?branch=main&event=push)](https://github.com/cartavelio/saxonmirrorhe/actions/workflows/saxonj-he.yml?query=branch%3Amain+event%3Apush)

Saxon Home Edition (Saxon-HE) is the open-source XSLT, XQuery, and XPath processor developed by Saxonica and its contributors. Cartavelio maintains this fork of [Saxonica’s upstream Saxon-HE source repository](https://saxonica.plan.io/projects/saxonmirrorhe/repository).

Development happens on `main`, based on Saxon 13.

## Differences from upstream

- **Building XML in Java.** The HE build restores the `Saplings.elem` overloads that accept a Java lambda and the `toXdmNode(Processor)` method. Java applications can use these methods to build an XML element and pass it directly to an XPath query or XSLT transformation. [PR #2](https://github.com/cartavelio/saxonmirrorhe/pull/2)
- **Calling XSLT functions from Java.** When a stylesheet returns an inline function to Java, the function keeps access to the global parameters of the transformation that created it. Two runs of the same stylesheet can return functions that each see their own parameter values when called later. [PR #3](https://github.com/cartavelio/saxonmirrorhe/pull/3)
- **Comparing values that contain functions.** XPath 3.1 requires `deep-equal()` to raise `FOTY0015` when it compares ordinary function items, even two references to the same function. The fork reports that error consistently, including when the comparison reaches a function nested in a map or array. [PR #8](https://github.com/cartavelio/saxonmirrorhe/pull/8)
- **Running compiled stylesheets in HE.** A Professional Edition (PE) check used to reject existing stylesheet export files (SEFs) containing standard XPath function references such as `abs#1` or `map:size#1`, even though HE already supports these functions. The fork loads those references using only the open-source HE implementations. [PR #10](https://github.com/cartavelio/saxonmirrorhe/pull/10) · [Saxonica’s HE feature list](https://www.saxonica.com/html/documentation13/conformance/xslt30.html)

## Build and test SaxonJ-HE

Use JDK 21. The shared Gradle configuration also looks for the `dotnet` executable on your `PATH`. After cloning, run these commands from the repository root. The first command installs the required Git hooks.

```sh
cp hooks/* .git/hooks/
./gradlew --configure-on-demand :java:he:stageHeRuntime :java:he:editionTest
java/he/ci/smoke.sh java/he/build/he-runtime
```

The runnable JAR and its dependencies are in `java/he/build/he-runtime/`. The smoke test checks that a source stylesheet and its saved compiled form (SEF) produce the expected output for the same inputs.

GitHub Actions runs these checks on pull requests and pushes to `main`. To use a CI build, open a successful workflow run and download the `saxonj-he-runtime` artifact. The HE test report is available as `he-test-report`. [Workflow runs](https://github.com/cartavelio/saxonmirrorhe/actions/workflows/saxonj-he.yml)

## Save and load a compiled stylesheet

The fork can save a compiled XSLT 3.0 stylesheet as a checksummed XML SEF for the HE target. Export and execution can run in different Java processes.

```sh
java -jar saxon-he-13.1.jar -xsl:transform.xsl -nogo -export:transform.sef
java -jar saxon-he-13.1.jar -xsl:transform.sef -s:input.xml -o:result.xml
```

In Java, compile a package and save it to a file. Load that file with a fresh processor.

```java
Path file = Path.of("transform.sef");
Processor source = new Processor(false);
XsltPackage compiled = source.newXsltCompiler()
    .compilePackage(new StreamSource(new File("transform.xsl")));
compiled.save(file.toFile());

Processor target = new Processor(false);
XsltExecutable executable = target.newXsltCompiler().loadExecutablePackage(file.toUri());
executable.load30().applyTemplates(
    new StreamSource(new File("input.xml")),
    target.newSerializer(new File("result.xml")));
```

Use `XsltExecutable.export(OutputStream)` when the application supplies a stream. A successful export closes the stream. An exception can leave partial bytes in a caller-supplied stream. `XsltPackage.save(File)` writes a temporary file and replaces the destination after serialization succeeds.

Ordinary exports preserve the stylesheet modules' static base URIs. Set `XsltCompiler.setRelocatable(true)` before compilation or pass `-relocate:on` to resolve relative runtime resources from the deployed SEF location. Supply a system ID when loading a relocatable SEF from a stream. Keep runtime resources available and configure the loading processor with the application's resource resolvers and other settings.

Use matching builds of this fork to produce and consume SEFs. The loader checks the format version for fork-generated files and accepts older SEFs when their format and constructs are supported. The qualification notes include tested features, known gaps, and timings. [HE SEF qualification](java/he/ci/w3c/SEF.md)

For the broader Saxon build, see the [upstream setup notes](READMEs/README.FIRST.md) and [build instructions](BUILD.md).
