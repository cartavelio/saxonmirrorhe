import com.saxonica.build.csharp.*
import com.saxonica.build.LifecycleTasks
import com.saxonica.build.java.JarFinder
import com.saxonica.build.java.PreprocessSourcesTask
import org.gradle.kotlin.dsl.create
import org.gradle.kotlin.dsl.register
import java.util.UUID
import kotlin.text.endsWith
import kotlin.text.replace
import kotlin.text.substring

plugins {
  id("java-library")
  id("saxon-build")
}

repositories {
  mavenLocal()
  mavenCentral()
  maven { url = uri("https://maven.saxonica.com/maven") }
}

val saxonBuild = the<SaxonBuildExtension>()

val extension = project.extensions.create<SaxonCSExtension>("saxoncs")


extension.classpath = configurations.compileClasspath
extension.release.convention("${project.findProperty("csharpBuild") ?: "Debug"}")
extension.targetHost = saxonBuild.targetHost
extension.dotnet = saxonBuild.dotnet

val defaultArch = extension.targetHost.map { if (it.isArm64) "arm64" else "x64" }
val defaultPlatform = extension.targetHost.map { if (it.isMacOS) "osx" else if (it.isWindows) "win" else "linux" } as Provider<String>

val javaSourcesBucket by configurations.dependencyScope("javaSourcesBucket")
val csharpFeatureKeysBucket by configurations.dependencyScope("csharpFeatureKeysBucket")
val javaFeatureKeysBucket by configurations.dependencyScope("javaFeatureKeysBucket")
val transpilerRuntime by configurations.dependencyScope("transpilerRuntime")
val editionJavaRuntime by configurations.dependencyScope("editionJavaRuntime")

val releaseArtifacts by configurations.consumable("releaseArtifacts")
val apiSourceArtifacts by configurations.consumable("apiSourceArtifacts")
val htmlDocArtifacts by configurations.consumable("htmlDocArtifacts")
val xmlDocArtifacts by configurations.consumable("xmlDocArtifacts")

val preprocessor by configurations.dependencyScope("preprocessor")
val preprocessorClasspath by configurations.resolvable("preprocessorClasspath") {
  extendsFrom(preprocessor)
}

val sampleSourcesBucket by configurations.dependencyScope("sampleSourcesBucket")
val sampleSources = configurations.resolvable("sampleSources") {
  extendsFrom(sampleSourcesBucket)
}

dependencies {
  javaSourcesBucket(files(layout.projectDirectory.files("../../java/src/main/java")))
  csharpFeatureKeysBucket(project(mapOf("path" to ":featurekeys",
    "configuration" to "csharpFeatureKeys")))
  javaFeatureKeysBucket(project(mapOf("path" to ":featurekeys",
    "configuration" to "javaFeatureKeys")))
  sampleSourcesBucket(project(mapOf("path" to ":samples",
    "configuration" to "sampleSources")))

  transpilerRuntime(project(":csharp:transpiler", "runtimeElements"))
  transpilerRuntime("com.ibm.icu:icu4j:${findProperty("icu4jVersion")}")
  transpilerRuntime("org.xmlresolver:xmlresolver:${findProperty("xmlresolverVersion")}")
  transpilerRuntime("org.xmlresolver:xmlresolver:${findProperty("xmlresolverVersion")}:data")
  transpilerRuntime(files(saxonBuild.saxonLicenseDir.get()))

  editionJavaRuntime(project(":java", "runtimeElements"))

  compileOnly(project(":csharp:transpiler", "runtimeElements"))

  preprocessor("com.igormaznitsa:jcp:7.0.4")
  preprocessor("com.tonicsystems:jarjar:0.6")
  preprocessor(files(saxonBuild.saxonLicenseDir.get()))
}

val javaSources = configurations.resolvable("javaSources") {
  extendsFrom(javaSourcesBucket)
}

val csharpFeatureKeysSources = configurations.resolvable("csharpFeatureKeysSources") {
  extendsFrom(csharpFeatureKeysBucket)
}

val javaFeatureKeysSources = configurations.resolvable("javaFeatureKeysSources") {
  extendsFrom(javaFeatureKeysBucket)
}

val transpiler = configurations.resolvable("transpiler") {
  extendsFrom(transpilerRuntime)
  extendsFrom(editionJavaRuntime)
}

val editionJava = configurations.resolvable("editionJava") {
  extendsFrom(editionJavaRuntime)
}

// from() is set in the edition build.gradle.kts
val filteredSources = tasks.register<Sync>("filteredSources") {
  into(layout.buildDirectory.dir("tmp/filtered-sources"))
  duplicatesStrategy = DuplicatesStrategy.WARN
  filter { line ->
    if (line.contains("net.sf.saxon.xpath.JAXPXPathStaticContext")
      || line.contains("import com.saxonica.ee.jaxp.SchemaFactoryImpl")
      || line.contains("import net.sf.saxon.stax")
      || line.contains("import net.sf.saxon.jaxp")) {
      "// " + line
    } else {
      line
    }
  }
}

val bundledFilteredSources = tasks.register<Sync>("bundleFilteredSources") {
  into(layout.buildDirectory.dir("tmp/java"))
  from(filteredSources)
  from(javaFeatureKeysSources)
  duplicatesStrategy = DuplicatesStrategy.WARN
}

val preprocessJava = tasks.register<PreprocessSourcesTask>("preprocessJava") {
  classpath = preprocessorClasspath
  sources = bundledFilteredSources.map { it.destinationDir }

  opts {
    off("PE", "STREAM", "SAXONC", "BYTECODE")
    on("CSHARP", "HOF")
  }

  output = layout.buildDirectory.dir("src/main/java")

  doLast {
    saxonBuild.patchReleaseInfo(
      layout.buildDirectory.file("src/main/java/net/sf/saxon/Version.java").get().asFile,
      "${findProperty("saxonVersion")!!}",
      "${findProperty("saxonMajorReleaseDate")!!}")
    val verifier = layout.buildDirectory.file("src/main/java/com/saxonica/config/JavaVerifier.java").get().asFile
    val dsaPublicKey = findProperty("dsaPublicKey")?.toString()
    val ed25519PublicKey = findProperty("ed25519PublicKey")?.toString()
    if (verifier.exists()) {
      saxonBuild.patchKey(verifier, "dsa", dsaPublicKey)
      saxonBuild.patchKey(verifier, "ed25519", ed25519PublicKey)
    }
  }
}

val jarFinder = JarFinder(configurations.named("transpiler").get())

val transpileToXml = tasks.register<JavaExec>("transpileToXml") {
  group = LifecycleBasePlugin.BUILD_GROUP
  inputs.files(preprocessJava)
  inputs.dir(layout.projectDirectory.dir("../transpiler/tools/xslt"))
  outputs.dir(layout.buildDirectory.dir("tmp/xml"))
  doFirst {
    mkdir("${layout.buildDirectory.dir("tmp/xml").get()}")
  }

  classpath = configurations.named("transpiler").get()
  mainClass = "ToXml"
  args("-j:${layout.buildDirectory.dir("src").get()}",
    "-repo:${layout.projectDirectory.file("../transpiler/")}",
    "-xml:${layout.buildDirectory.dir("tmp/xml").get()}",
    "-h:${layout.buildDirectory.dir("src/main/java").get()}",
    "-src:${layout.buildDirectory.dir("src/main/java").get()}",
    "-${extension.edition.get()}:${editionJava.get().map { it }.get(0)}",
    "-icu4j:${jarFinder.find("icu4j-${findProperty("icu4jVersion")}.jar")}")
}

val transpileToDigest = tasks.register<JavaExec>("transpileToDigest") {
  group = LifecycleBasePlugin.BUILD_GROUP
  inputs.dir(layout.projectDirectory.dir("../transpiler/tools/xslt"))
  inputs.files(transpileToXml)
  outputs.file(layout.buildDirectory.file("tmp/digest.xml"))

  // Fix Windows filenames so that Java doesn't get confused
  val xdigest = saxonBuild.puri("${layout.projectDirectory.file("../transpiler/tools/xslt/xml-to-digest.xsl")}")
  val xmldir = saxonBuild.puri("${layout.buildDirectory.dir("tmp/xml").get()}")
  val tmpdigest = saxonBuild.puri("${layout.buildDirectory.file("tmp/digest.xml").get()}")

  classpath = preprocessorClasspath
  mainClass = "net.sf.saxon.Transform"
  args("-it",
    "-xsl:${xdigest}",
    "-o:${tmpdigest}",
    "xmlDir=${xmldir}")
}

val transpile = tasks.register<JavaExec>("transpile") {
  group = LifecycleBasePlugin.BUILD_GROUP
  inputs.files(transpileToDigest)
  outputs.file(layout.buildDirectory.file("digest.xml"))

  // Fix Windows filenames so that Java doesn't get confused
  val rdigest = saxonBuild.puri("${layout.projectDirectory.file("../transpiler/tools/xslt/refine-digest.xsl")}")
  val xmldir = saxonBuild.puri("${layout.buildDirectory.dir("tmp/xml").get()}")
  val tmpdigest = saxonBuild.puri("${layout.buildDirectory.file("tmp/digest.xml").get()}")
  val digest = saxonBuild.puri("${layout.buildDirectory.file("digest.xml").get()}")

  classpath = preprocessorClasspath
  mainClass = "net.sf.saxon.Transform"
  args("-xsl:${rdigest}",
    "-s:${tmpdigest}",
    "-o:${digest}",
    "xmlDir=${xmldir}")
}

val generateCode = tasks.register<JavaExec>("generateCode") {
  group = LifecycleBasePlugin.BUILD_GROUP
  inputs.files(transpile)
  inputs.files(layout.buildDirectory.dir("tmp/xml"))
  outputs.dir(layout.buildDirectory.dir("generated/Saxon"))

  classpath = configurations.named("transpiler").get()
  classpath += editionJava.get()

  mainClass = "ToXml"
  args("-cs:${layout.buildDirectory.dir("generated").get()}",
    "-xml:${layout.buildDirectory.dir("tmp/xml/main/java").get()}",
    "-repo:${layout.projectDirectory}/../transpiler/",
    "-digest:${layout.buildDirectory.file("digest.xml").get()}",
    "-${extension.edition.get()}:${editionJava.get().map { it }.get(0)}",
    "-icu4j:${jarFinder.find("icu4j-${findProperty("icu4jVersion")}.jar")}",
    "-debug")

}

val preprocessCS = tasks.register<Copy>("preprocessCS") {
  from(layout.projectDirectory.dir("../src/main/cs"))
  into(layout.buildDirectory.dir("cs"))
}

// I'm trying to make this build practical to manage and yet flexible enough
// to handle the complexity of CS builds. For CS, we build for x64 or arm64,
// depending on the platform and architecture. In addition, for MacOS in a
// Release build, we build *both* of those and make a universal binary.
val architectures = if (findProperty("buildArch") != null) {
  listOf(findProperty("buildArch").toString())
} else if (saxonBuild.targetHost.get().isMacOS
  && extension.release.get() == "Release") {
  listOf("arm64", "x64")
} else {
  listOf(extension.arch.get())
}

val copyResources = tasks.register("copyResources") {
  doLast {
    copy {
      // These are copied into build/{arch} so that ../ will be correct
      // relative to either the build directory build/{arch}/SaxonCS or
      // the csharp/{ee,pe,he} directories because (at least some?) relative
      // paths in a csproj file are relative to the build location, not
      // the csproj file. ¯\_(ツ)_/¯
      from(layout.projectDirectory.dir(".."))
      into(layout.buildDirectory.dir(architectures.first()))
      include("LICENSE.txt")
      include("README.md")
    }
    copy {
      from(layout.projectDirectory.dir("../../java/src/main/resources"))
      into(layout.buildDirectory.dir("resources"))
      exclude("**/.DS_Store")
    }
  }
}

val configure_task = tasks.register("dotnetConfigure") {
  dependsOn("generateCode")
  dependsOn("copyResources")
}

val build_task = tasks.register("dotnetBuild")
val publish_task = tasks.register("dotnetPublish")
val makeapp_task = tasks.register("dotnetMakeApp")
for (buildArch in architectures) {
  // These tasks run every time because the .nupkg file changes every time.
  // The DotNet tasks use the outputDir as an output directory. This should
  // probably be reworked so that only the /obj/ directory is considered,
  // or something.
  val configure = tasks.register<DotNetConfiguration>("configure_${buildArch}") {
    group = LifecycleBasePlugin.BUILD_GROUP
    classpath = configurations.named("transpiler").get()
    dotnetversion = saxonBuild.dotnetversion
    version = saxonBuild.version
    edition = extension.edition
    release = extension.release
    arch = buildArch
    platform = defaultPlatform
  }
  configure_task { dependsOn(configure) }

  val csrid = defaultPlatform.map { "${it}-${buildArch}" }
  val release = extension.release.get()

  val build = tasks.register<DotNetBuild>("dotnetBuild_${buildArch}") {
    group = LifecycleBasePlugin.BUILD_GROUP
    dependsOn(configure)
    dotnet = extension.dotnet
    version = saxonBuild.version
    edition = extension.edition
    target = "SaxonCS"
    arch = buildArch
    platform = defaultPlatform

    inputs.files(configure)
    inputs.files(preprocessCS)
    inputs.files(generateCode)
    inputs.files(copyResources)
    inputs.files(csharpFeatureKeysSources)
    outputs.dir(layout.buildDirectory.dir("${buildArch}/SaxonCS/bin/${release}/net${saxonBuild.dotnetversion.get()}/${csrid.get()}"))
    outputs.file(layout.buildDirectory.file("${buildArch}/SaxonCS/bin/${release}/SaxonCS.${saxonBuild.version.get()}.nupkg"))
  }
  build_task { dependsOn(build) }

  val publish = tasks.register<DotNetPublish>("dotnetPublish_${buildArch}") {
    LifecycleTasks.RELEASE_GROUP
    dotnet = extension.dotnet
    version = saxonBuild.version
    edition = extension.edition
    arch = buildArch
    platform = defaultPlatform
    target = "SaxonCS"
    inputs.files(build)
    outputs.dir(layout.buildDirectory.dir("${buildArch}/SaxonCS/bin/${release}/net${saxonBuild.dotnetversion.get()}/${csrid.get()}/publish"))
  }

  publish_task { dependsOn(publish) }

  val makeapp = tasks.register<DotNetMakeApp>("dotnetMakeApp_${buildArch}") {
    LifecycleTasks.RELEASE_GROUP
    dotnet = extension.dotnet
    version = saxonBuild.version
    edition = extension.edition
    arch = buildArch
    target = "SaxonCS"
    inputs.files(build)
  }
  makeapp_task { dependsOn(makeapp) }
}

if ((saxonBuild.targetHost.get().isMacOS
      && extension.release.get() == "Release"
      && architectures.size == 2)) {
  val buildOsxApp = tasks.register<Exec>("buildOsxApp") {
    group = LifecycleTasks.RELEASE_GROUP
    inputs.files(makeapp_task)
    outputs.file(layout.buildDirectory.file("SaxonCS.app/Contents/MacOS/SaxonCS"))

    val arm64 = layout.buildDirectory.dir("arm64/SaxonCS/bin/Release/net${saxonBuild.dotnetversion.get()}").get()
    val x64 = layout.buildDirectory.dir("x64/SaxonCS/bin/Release/net${saxonBuild.dotnetversion.get()}").get()

    doFirst {
      // If a previous build put the docs in there, delete them; they aren't
      // allowed in a signed build
      delete(layout.buildDirectory.dir("SaxonCS.app/doc"))
      delete(layout.buildDirectory.dir("SaxonCS.app/SaxonCS"))
    }

    doFirst {
      copy {
        from("${arm64}/osx-arm64/publish/SaxonCS.app")
        into(layout.buildDirectory.dir("SaxonCS.app"))
        exclude("Contents/MacOS/SaxonCS")
      }
    }

    workingDir = layout.buildDirectory.get().asFile
    commandLine("lipo", "-create", "-output",
      "${layout.buildDirectory.file("SaxonCS.app/Contents/MacOS/SaxonCS").get()}",
      "${arm64}/osx-arm64/publish/SaxonCS.app/Contents/MacOS/SaxonCS",
      "${x64}/osx-x64/publish/SaxonCS.app/Contents/MacOS/SaxonCS")
  }

  val osxApp = tasks.register<Exec>("osxApp") {
    inputs.files(buildOsxApp)
    outputs.dir(layout.buildDirectory.dir("SaxonCS.app"))

    // Putting documentation in the app causes the signature to fail
    // "unsealed content is present in the bundle root"
    commandLine("codesign", "--force", "--options", "runtime",
      "--entitlements",
      "${layout.projectDirectory.file("../tools/entitlements.plist")}",
      "--deep", "--sign", "Developer ID Application: Saxonica Limited",
      "--timestamp",
      "${layout.buildDirectory.file("SaxonCS.app").get()}")
  }
  makeapp_task { finalizedBy(osxApp) }

  // We can't copy the documentation into the distribution because we've
  // signed it
  tasks.register<Zip>("dotnetPublishZip") {
    archiveFileName = "SaxonCS-${saxonBuild.version.get().replace(".", "-")}-MacOS.zip"
    from(osxApp)
    into("/SaxonCS.app/")
  }
} else {
  val ziparch = if (architectures[0] == "arm64") "aarch64" else "x86_64"
  val zipver = saxonBuild.version.get().replace(".", "-")
  val zipname = if (saxonBuild.systemOs.get().isWindows()) {
    "Windows"
  } else if (saxonBuild.targetHost.get().isMacOS) {
    "MacOS"
  } else {
    "Linux"
  }

  val stageZip = tasks.register("stageZip") {
    inputs.files(publish_task)
    inputs.files(layout.projectDirectory.dir("../../src/main/zipdoc"))
    outputs.dir(layout.buildDirectory.dir("SaxonCS.app"))

    doFirst {
      delete(layout.buildDirectory.dir("SaxonCS.app"))
    }

    doLast {
      copy {
        from(tasks.named("dotnetPublish_${architectures[0]}").get().outputs)
        into(layout.buildDirectory.dir("SaxonCS.app/SaxonCS"))
      }

      copy {
        from(layout.projectDirectory.dir("../../src/main/zipdoc"))
        into(layout.buildDirectory.dir("SaxonCS.app/doc"))
      }
    }
  }

  tasks.register<Zip>("dotnetPublishZip") {
    var dirver = saxonBuild.version.get()
    if (dirver.endsWith(".0")) {
      dirver = dirver.substring(0, dirver.length - 2)
    }

    archiveFileName = "SaxonCS-${zipver}-${zipname}-${ziparch}.zip"
    from(stageZip)
    into("/SaxonCS-${dirver}/")
  }
}

val nupkgBuildTask = tasks.named<DotNetBuild>("dotnetBuild_${architectures[0]}")

val artifacts = tasks.register("downloadRelease") {
  inputs.files(tasks.named("dotnetPublishZip"))
  inputs.files(nupkgBuildTask.map { it.outputs.files.filter { it.extension == "nupkg" } })
  outputs.dir(layout.buildDirectory.dir("releases"))
  group = "build"

  doLast {
    copy {
      into(layout.buildDirectory.dir("releases"))
      from(tasks.named("dotnetPublishZip"))
    }

    copy {
      into(layout.buildDirectory.dir("releases"))
      from(nupkgBuildTask.map { it.outputs.files.filter { it.extension == "nupkg" } })
    }
  }
}

releaseArtifacts.outgoing.artifact(artifacts)

// ============================================================
// Documentation tasks

// Work out where *this* build of *this* release put the API documentation
// and move it to a consistent location we can refer to from elsewhere.
// If more than one architecture is being built, we only need the first one.
val apiDocumentation = tasks.register<Copy>("apiDocumentation") {
  val arch = architectures.first()
  val release = extension.release.get()
  val archdir = "${extension.platform.get()}-${arch}"
  val dotnetver = saxonBuild.dotnetversion.get()
  val buildtask = tasks.named("dotnetBuild_${arch}")
  dependsOn(buildtask)

  into(layout.buildDirectory.dir("apidoc"))
  from("${buildtask.get().outputs.getFiles().first()}/bin/${release}/net${dotnetver}/${archdir}") {
    include("SaxonCS.xml")
  }
}

val docParse = tasks.register<JavaExec>("docParse") {
  group = "Internal"
  description = "Parse dotnet cs files for docs"
  dependsOn("dotnetBuild")

  inputs.files(layout.projectDirectory.dir("../src/main/cs/Saxon/Api"))
  inputs.files(csharpFeatureKeysSources)
  inputs.file(layout.projectDirectory.file("../tools/parsecs.xsl"))
  outputs.file(layout.buildDirectory.file("dotnetdoc/names.xml"))

  classpath = preprocessorClasspath
  mainClass = "com.saxonica.Transform"
  args("-it:main",
    "-xsl:${layout.projectDirectory.file("../tools/parsecs.xsl")}",
    "-o:${layout.buildDirectory.file("dotnetdoc/names.xml").get().asFile}",
    "inputdir=file://${layout.projectDirectory.file("../src/main/cs/Saxon/Api")}",
    "featurekeysdir=file://${layout.projectDirectory.file("../../featurekeys/build/cs/Saxon/Api")}")
}

tasks.register<JavaExec>("docMerge") {
  group = "Internal"
  description = "Merge API docs for dotnet"

  val arch = architectures.first()
  val buildtask = tasks.named("dotnetBuild_${arch}")

  dependsOn(buildtask)
  inputs.files(apiDocumentation)
  inputs.files(docParse)
  inputs.file(layout.projectDirectory.file("../tools/merge.xsl"))
  outputs.file(layout.buildDirectory.file("dotnetdoc/merged.xml"))

  classpath = preprocessorClasspath
  mainClass = "com.saxonica.Transform"
  args("-it:main",
    "-xsl:${layout.projectDirectory.file("../tools/merge.xsl")}",
    "-o:${layout.buildDirectory.file("dotnetdoc/merged.xml").get().asFile}",
    "names-file=${docParse.get().outputs.getFiles().getSingleFile()}",
    "api-doc-file=${layout.buildDirectory.file("apidoc/SaxonCS.xml").get().asFile}")
}

tasks.register<Copy>("docCss") {
  group = "Internal"
  description = "Copies the CSS for dotnet docs"

  into(layout.buildDirectory.dir("dotnetdoc-html"))
  from(layout.projectDirectory.dir("../tools")) {
    include("saxon-dotnetdoc.css")
  }
}

val dotnetDocHtml = tasks.register<JavaExec>("docHtml") {
  group = LifecycleTasks.DOCUMENTATION_GROUP
  description = "Create HTML output for dotnet docs"
  dependsOn("docMerge")
  dependsOn("docCss")

  inputs.file(layout.buildDirectory.file("dotnetdoc/merged.xml"))
  inputs.file(layout.projectDirectory.file("../tools/doctohtml.xsl"))
  outputs.dir(layout.buildDirectory.file("dotnetdoc-html"))

  classpath = preprocessorClasspath
  mainClass = "com.saxonica.Transform"
  args("-s:${layout.buildDirectory.file("dotnetdoc/merged.xml").get().asFile}",
    "-xsl:${layout.projectDirectory.file("../tools/doctohtml.xsl")}",
    "-o:${layout.buildDirectory.file("dotnetdoc-html/index.html").get().asFile}")
}

val dotnetDocXml = tasks.register<JavaExec>("docXml") {
  group = LifecycleTasks.DOCUMENTATION_GROUP
  description = "Create XML version of dotnet docs"
  dependsOn("docMerge")

  inputs.file(layout.buildDirectory.file("dotnetdoc/merged.xml"))
  inputs.file(layout.projectDirectory.file("../tools/convert-to-jel-package.xsl"))
  outputs.dir(layout.buildDirectory.file("dotnetdoc-xml"))
  classpath = preprocessorClasspath
  mainClass = "com.saxonica.Transform"
  args("-s:${layout.buildDirectory.file("dotnetdoc/merged.xml").get().asFile}",
    "-xsl:${layout.projectDirectory.file("../tools/convert-to-jel-package.xsl")}",
    "-o:${layout.buildDirectory.file("dotnetdoc-xml/packages/Saxon.Api.xml").get().asFile}")
}

htmlDocArtifacts.outgoing.artifact(dotnetDocHtml)
xmlDocArtifacts.outgoing.artifact(dotnetDocXml)

tasks.named(LifecycleTasks.DOCUMENTATION_TASK_NAME) {
  dependsOn(dotnetDocHtml)
  dependsOn(dotnetDocXml)
}

// ============================================================

tasks.register<Exec>("csTest") {
  group = "Verification"
  description = "Run SaxonCS nUnit tests"
  dependsOn("dotnetBuild")
  dependsOn("csproj-template")

  val reportDir = layout.buildDirectory.dir("reports/tests/csTest")
  inputs.dir(layout.projectDirectory.dir("../src/test/nunit"))
  outputs.dir(reportDir)

  commandLine("${saxonBuild.dotnet.get()}", "test", "SaxonCS.sln",
    "--nologo", "--", "NUnit.TestOutputXml=${reportDir.get()}")
}

// ============================================================

val copySampleResources = tasks.register<Copy>("copy_sample_sources") {
  into(layout.buildDirectory.dir("samples/resources"))
  from(sampleSources)
}

tasks.register<Exec>("samples") {
  dependsOn("dotnetBuild")
  dependsOn(copySampleResources)

  val arch = extension.arch.get()
  val rel = extension.release.get()
  val plat = extension.platform.get()
  val edition = extension.edition.get()

  // FIXME: should the .net 8.0 ref use saxonBuild.dotnetversion?
  commandLine = listOf("python", "../../tools/run-samples.py",
    "--edition", edition,
    "--platform", "csharp",
    "--csexec", "build/${arch}/SaxonCS/bin/${rel}/net8.0/${plat}-${arch}/SaxonCS")
}

// ============================================================

val templateName = project.findProperty("projectTemplateName") ?: "SaxonCS"

tasks.register<JavaExec>("csproj-template") {
  inputs.files(layout.projectDirectory.file("../tools/generateCSProjFiles.xsl"))
  outputs.file(layout.projectDirectory.file("${templateName}.csproj"))
  outputs.file(layout.projectDirectory.file("${templateName}.sln"))
  classpath = preprocessorClasspath
  mainClass = "net.sf.saxon.Transform"
  args("-it",
    "-xsl:${layout.projectDirectory.file("../tools/generateCSProjFiles.xsl")}",
    "-o:${layout.projectDirectory.file("${templateName}.sln")}",
    "release=${extension.release.get()}",
    "platform=${extension.platform.get()}",
    "arch=${extension.arch.get()}",
    "edition=${extension.edition.get()}",
    "framework=net${saxonBuild.dotnetversion.get()}",
    "saxonVersion=${findProperty("saxonVersion")!!}",
    "uuid1=${UUID.randomUUID()}",
    "uuid2=${UUID.randomUUID()}",
    "uuid3=${UUID.randomUUID()}",
    "uuid4=${UUID.randomUUID()}")
}

val copyTestDrivers = tasks.register<Copy>("copyTestDrivers") {
  group = "Internal"
  description = "Build the C# test drivers"
  from(layout.projectDirectory.dir("../src/test/testrunner"))
  into(layout.buildDirectory.dir("src/testrunner"))
}

tasks.register<DotNetBuild>("dotnetBuildTestDrivers") {
  group = LifecycleTasks.ACCEPTANCE_TEST_GROUP
  dependsOn("dotnetBuild")
  target = "TestDrivers"
  arch = extension.arch
  dotnet = extension.dotnet
  version = saxonBuild.version
  edition = extension.edition
  platform = defaultPlatform
  inputs.files(copyTestDrivers)
}
