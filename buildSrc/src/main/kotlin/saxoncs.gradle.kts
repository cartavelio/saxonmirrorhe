import com.saxonica.build.Architectures
import com.saxonica.build.csharp.*
import com.saxonica.build.LifecycleTasks
import com.saxonica.build.java.JarFinder
import com.saxonica.build.java.PreprocessSourcesTask
import com.saxonica.build.AwsSigningResources
import com.saxonica.build.UnsignedArtifactUploadTask
import com.saxonica.build.SignedArtifactDownloadTask
import org.gradle.api.attributes.Usage
import org.gradle.kotlin.dsl.create
import org.gradle.kotlin.dsl.register
import java.util.UUID
import kotlin.text.endsWith
import kotlin.text.replace
import kotlin.text.substring

plugins {
  base
  id("jvm-ecosystem")
  id("saxon-build")
}

repositories {
  mavenLocal()
  mavenCentral()
  maven { url = uri("https://maven.saxonica.com/maven") }
}

val saxonBuild = the<SaxonBuildExtension>()

val extension = project.extensions.create<SaxonCSExtension>("saxoncs")

extension.targetHost = saxonBuild.targetHost
extension.dotnet = saxonBuild.dotnet

val javaSourcesBucket by configurations.dependencyScope("javaSourcesBucket")
val csharpFeatureKeysBucket by configurations.dependencyScope("csharpFeatureKeysBucket")
val javaFeatureKeysBucket by configurations.dependencyScope("javaFeatureKeysBucket")

val saxonRuntime by configurations.dependencyScope("saxonRuntime")
val saxonClasspath by configurations.resolvable("saxonClasspath") {
  extendsFrom(saxonRuntime)
}
val transpilerRuntime by configurations.dependencyScope("transpilerRuntime")
val transpilerInputSaxonJarBucket by configurations.dependencyScope("transpilerInputSaxonJarBucket")

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

  saxonRuntime("net.sf.saxon:Saxon-HE:12.9")

  transpilerRuntime(project(":csharp:transpiler"))
  transpilerRuntime("com.ibm.icu:icu4j:${findProperty("icu4jVersion")}")
  transpilerRuntime("org.xmlresolver:xmlresolver:${findProperty("xmlresolverVersion")}")
  transpilerRuntime("org.xmlresolver:xmlresolver:${findProperty("xmlresolverVersion")}:data")
  transpilerRuntime(files(saxonBuild.saxonLicenseDir.get()))

  transpilerInputSaxonJarBucket(project(":java", "saxonJarArtifact"))

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

val transpilerInputSaxonJar = configurations.resolvable("transpilerInputSaxonJar") {
  extendsFrom(transpilerInputSaxonJarBucket)
}

val transpilerClasspath by configurations.resolvable("transpilerClasspath") {
  extendsFrom(saxonRuntime)
  extendsFrom(transpilerRuntime)
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

val jarFinder = JarFinder(transpilerClasspath)

val transpileToXml = tasks.register<JavaExec>("transpileToXml") {
  group = LifecycleBasePlugin.BUILD_GROUP
  inputs.files(preprocessJava)
  inputs.files(transpilerInputSaxonJar)
  inputs.dir(layout.projectDirectory.dir("../transpiler/tools/xslt"))
  outputs.dir(layout.buildDirectory.dir("tmp/xml"))
  doFirst {
    mkdir("${layout.buildDirectory.dir("tmp/xml").get()}")
  }

  classpath = transpilerClasspath
  mainClass = "ToXml"
  args("-j:${layout.buildDirectory.dir("src").get()}",
    "-repo:${layout.projectDirectory.file("../transpiler/")}",
    "-xml:${layout.buildDirectory.dir("tmp/xml").get()}",
    "-h:${layout.buildDirectory.dir("src/main/java").get()}",
    "-src:${layout.buildDirectory.dir("src/main/java").get()}",
    "-${extension.edition.get()}:${transpilerInputSaxonJar.map { it.singleFile }.get() }",
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

  classpath = transpilerClasspath
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

  classpath = transpilerClasspath
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

  classpath = transpilerClasspath

  mainClass = "ToXml"
  args("-cs:${layout.buildDirectory.dir("generated").get()}",
    "-xml:${layout.buildDirectory.dir("tmp/xml/main/java").get()}",
    "-repo:${layout.projectDirectory}/../transpiler/",
    "-digest:${layout.buildDirectory.file("digest.xml").get()}",
    "-${extension.edition.get()}:${transpilerInputSaxonJar.get().map { it }.get(0)}",
    "-icu4j:${jarFinder.find("icu4j-${findProperty("icu4jVersion")}.jar")}",
    "-debug")

}

val preprocessCS = tasks.register<Sync>("preprocessCS") {
  from(layout.projectDirectory.dir("../src/main/cs"))
  into(layout.buildDirectory.dir("cs"))
}

// I'm trying to make this build practical to manage and yet flexible enough
// to handle the complexity of CS builds. For CS, we build for x64 or arm64,
// depending on the platform and architecture. In addition, for MacOS in a
// Release build, we build *both* of those and make a universal binary.
val additionalArchitectures = listOf(Architectures.ARM64, Architectures.X86_64) - extension.architecture.get()

val copyJavaResources = tasks.register<Copy>("copy-java-resources") {
  into(layout.buildDirectory.dir("resources"))
  from(layout.projectDirectory.dir("../../java/src/main/resources")) {
    exclude("**/.DS_Store")
  }
}

data class DotNetTasks(
  val configure: TaskProvider<DotNetConfiguration>,
  val build: TaskProvider<DotNetBuild>,
  val publish: TaskProvider<DotNetPublish>,
  val nupkg: TaskProvider<DotNetCreateNupkg>,
  val makeApp: TaskProvider<DotNetMakeApp>
)

private fun generateDotnetTasks(buildArch: Architectures, buildConfiguration: CSReleaseConfiguration, suffix: String): DotNetTasks {
  val configure = tasks.register<DotNetConfiguration>("dotnet-configure-${buildArch}-${suffix}") {
    csProjXsl = layout.projectDirectory.file("../tools/generateCSProjFiles.xsl")
    resources.from(layout.projectDirectory.dir("..").files("LICENSE.txt"))
    resources.from(layout.projectDirectory.dir(".").files("README.md"))
    group = LifecycleBasePlugin.BUILD_GROUP
    classpath = saxonClasspath
    dotnetversion = saxonBuild.dotnetversion
    version = saxonBuild.version
    edition = extension.edition
    release = buildConfiguration
    arch = buildArch
    platform = extension.platform
  }

  fun setTaskInputSources(inputSources: ConfigurableFileCollection) {
    inputSources.from(preprocessCS)
    inputSources.from(generateCode)
    inputSources.from(copyJavaResources)
    inputSources.from(csharpFeatureKeysSources)
  }

  val build = tasks.register<DotNetBuild>("dotnet-build-${buildArch}-${suffix}") {
    group = LifecycleBasePlugin.BUILD_GROUP
    dotnet = extension.dotnet
    version = saxonBuild.version
    dotnetversion = saxonBuild.dotnetversion
    edition = extension.edition
    configuration = buildConfiguration
    slnFile = configure.map { it.appSlnFile.get() }
    target = "SaxonCS"
    arch = buildArch
    platform = extension.platform

    setTaskInputSources(inputSources)
  }

  val publish = tasks.register<DotNetPublish>("dotnet-publish-${buildArch}-${suffix}") {
    LifecycleTasks.RELEASE_GROUP
    dotnet = extension.dotnet
    dotnetversion = saxonBuild.dotnetversion
    version = saxonBuild.version
    edition = extension.edition
    configuration = buildConfiguration
    arch = buildArch
    platform = extension.platform
    target = "SaxonCS"
    slnFile = configure.map { it.appSlnFile.get() }

    setTaskInputSources(inputSources)
  }

  val nupkg = tasks.register<DotNetCreateNupkg>("dotnet-pack-${buildArch}-${suffix}") {
    LifecycleTasks.RELEASE_GROUP
    dotnet = extension.dotnet
    dotnetversion = saxonBuild.dotnetversion
    version = saxonBuild.version
    edition = extension.edition
    configuration = buildConfiguration
    arch = buildArch
    platform = extension.platform
    target = "SaxonCS"
    slnFile = configure.map { it.appSlnFile.get() }

    setTaskInputSources(inputSources)
  }

  val makeapp = tasks.register<DotNetMakeApp>("dotnet-makeapp-${buildArch}-${suffix}") {
    LifecycleTasks.RELEASE_GROUP
    dotnet = extension.dotnet
    version = saxonBuild.version
    edition = extension.edition
    dotnetversion = saxonBuild.dotnetversion
    configuration = buildConfiguration
    platform = extension.platform
    arch = buildArch
    target = "SaxonCS"
    inputs.files(build)
  }

  return DotNetTasks(configure, build, publish, nupkg, makeapp)
}

private fun dotnetPublishZipArtifactTaskName(lowercaseConfiguration: String): String {
  return "dotnet-publish-${lowercaseConfiguration}-zip"
}

fun csZipArtifactTasks(configuration: CSReleaseConfiguration, publishTask: TaskProvider<DotNetPublish>): TaskProvider<Zip> {
  val ziparch = if (extension.targetHost.get().isArm64) "aarch64" else "x86_64"
  val zipver = saxonBuild.version.get().replace(".", "-")
  val zipname = extension.targetHost.get().platform.replaceFirstChar { it.uppercase() }
  val lowercaseConfiguration = configuration.toString().lowercase()

  val stageZip = tasks.register("stage-${lowercaseConfiguration}-zip") {
    inputs.files(publishTask)
    inputs.files(layout.projectDirectory.dir("../../src/main/zipdoc"))
    outputs.dir(layout.buildDirectory.dir("SaxonCS-${extension.edition.get().uppercase()}.app"))

    doFirst {
      delete(layout.buildDirectory.dir("SaxonCS-${extension.edition.get().uppercase()}.app"))
    }

    doLast {
      copy {
        from(publishTask.map { it.outputDir.get() })
        into(layout.buildDirectory.dir("SaxonCS-${extension.edition.get().uppercase()}.app/SaxonCS-${extension.edition.get().uppercase()}"))
      }

      copy {
        from(layout.projectDirectory.dir("../../src/main/zipdoc"))
        into(layout.buildDirectory.dir("SaxonCS-${extension.edition.get().uppercase()}.app/doc"))
      }
    }
  }

  return tasks.register<Zip>(dotnetPublishZipArtifactTaskName(lowercaseConfiguration)) {
    destinationDirectory = layout.buildDirectory.dir("${configuration}Zip")

    var dirver = saxonBuild.version.get()
    if (dirver.endsWith(".0")) {
      dirver = dirver.substring(0, dirver.length - 2)
    }

    archiveFileName = "SaxonCS-${extension.edition.get().uppercase()}-${zipver}-${zipname}-${ziparch}.zip"
    from(stageZip)
    into("/SaxonCS-${extension.edition.get().uppercase()}-${dirver}/")
  }
}

// Generate Release tasks
val releaseBuildArch = extension.architecture.get()
val (configureTask, buildTask, publishTask, nupkgBuildTask, makeAppTask) = generateDotnetTasks(releaseBuildArch, CSReleaseConfiguration.Release, "release")
tasks.named(LifecycleBasePlugin.ASSEMBLE_TASK_NAME) {
  dependsOn(buildTask)
}
tasks.named(LifecycleTasks.RELEASE_GROUP) {
  dependsOn(publishTask)
}

val releaseZipArtifactTasks = mutableListOf<TaskProvider<Zip>>()

if (!saxonBuild.targetHost.get().isMacOS) {
  releaseZipArtifactTasks.add(csZipArtifactTasks(CSReleaseConfiguration.Release, publishTask))
} else {
  // Additional tasks to build a macOS universal binary
  val dotnetTasks = additionalArchitectures.map { buildArch ->
    buildArch to generateDotnetTasks(buildArch, CSReleaseConfiguration.Release, "release")
  }.toMap()

  val makeAppTasks = (dotnetTasks.values.map { it.makeApp } + makeAppTask).toTypedArray()

  tasks.named(LifecycleBasePlugin.ASSEMBLE_TASK_NAME) {
    dotnetTasks.values.forEach { dependsOn(it.build) }
  }

  val buildUnsignedMacOSAppTask = tasks.register<Exec>("unsigned-macos-release-app") {
    group = LifecycleTasks.RELEASE_GROUP

    inputs.files(*makeAppTasks)
    outputs.file(layout.buildDirectory.file("SaxonCS-${extension.edition.get().uppercase()}.app/Contents/MacOS/SaxonCS-${extension.edition.get().uppercase()}"))

    doFirst {
      // If a previous build put the docs in there, delete them; they aren't
      // allowed in a signed build
      delete(layout.buildDirectory.dir("SaxonCS-${extension.edition.get().uppercase()}.app/doc"))
      delete(layout.buildDirectory.dir("SaxonCS-${extension.edition.get().uppercase()}.app/SaxonCS-${extension.edition.get().uppercase()}"))
    }

    doFirst {
      copy {
        from(makeAppTask)
        into(layout.buildDirectory.dir("SaxonCS-${extension.edition.get().uppercase()}.app"))
        exclude("Contents/MacOS/SaxonCS-${extension.edition.get().uppercase()}")
      }
    }

    workingDir = layout.buildDirectory.get().asFile
    commandLine("lipo", "-create", "-output",
      "${layout.buildDirectory.file("SaxonCS-${extension.edition.get().uppercase()}.app/Contents/MacOS/SaxonCS-${extension.edition.get().uppercase()}").get()}",
      *makeAppTasks.map { it.map { it.outputDir.get().dir("Contents/MacOS/SaxonCS-${extension.edition.get().uppercase()}") }.get().asFile }.toTypedArray())
  }

  val signedMacOSAppTask = tasks.register<Exec>("macos-release-app") {
    inputs.files(buildUnsignedMacOSAppTask)
    outputs.dir(layout.buildDirectory.dir("SaxonCS-${extension.edition.get().uppercase()}.app"))

    // Putting documentation in the app causes the signature to fail
    // "unsealed content is present in the bundle root"
    commandLine("codesign", "--force", "--options", "runtime",
      "--entitlements",
      "${layout.projectDirectory.file("../tools/entitlements.plist")}",
      "--deep", "--sign", "Developer ID Application: Saxonica Limited",
      "--timestamp",
      "${layout.buildDirectory.file("SaxonCS-${extension.edition.get().uppercase()}.app").get()}")
  }

  val zipArtifactTask = tasks.register<Zip>(dotnetPublishZipArtifactTaskName("release")) {
    destinationDirectory = layout.buildDirectory.dir("ReleaseZip")
    archiveFileName = "SaxonCS-${saxonBuild.version.get().replace(".", "-")}-MacOS.zip"

    from(signedMacOSAppTask)
    into("/SaxonCS-${extension.edition.get().uppercase()}.app/")
  }

  releaseZipArtifactTasks.add(zipArtifactTask)
}

// Generate Debug tasks
val debugBuildArch = extension.targetHost.get().architectureConst
val debugTasks = generateDotnetTasks(debugBuildArch, CSReleaseConfiguration.Debug, "debug")
csZipArtifactTasks(CSReleaseConfiguration.Debug, debugTasks.publish)

// Copy Artifacts to a release dir
val artifacts = tasks.register<Copy>("downloadRelease") {
  group = LifecycleBasePlugin.BUILD_GROUP

  into(layout.buildDirectory.dir("releases"))

  from(releaseZipArtifactTasks)
  from(nupkgBuildTask.map { it.nupkgFile.get() })
}

releaseArtifacts.outgoing.artifacts(artifacts.map { it.outputs.files }) { builtBy(artifacts) }

// ============================================================

val signingResources = tasks.register<AwsSigningResources>("signingResources") {
  region = "eu-west-2"
  stackName = "SaxonicaCiCdStack"
}

val unsignedNugetPkgs = tasks.register<Sync>("unsignedNugetPkgs") {
  inputs.files(nupkgBuildTask)
  into(layout.buildDirectory.dir("unsignedNugetPkgs"))
  from(nupkgBuildTask.map { it.nupkgFile.get() })
}

val uploadUnsignedNugetPkg = tasks.register<UnsignedArtifactUploadTask>("uploadUnsignedNugetPkg") {
  inputDir = layout.projectDirectory.dir(unsignedNugetPkgs.map { it.destinationDir.toString() })
  bucket = signingResources.map { it.codeSigningInputsBucket.get() }
  queueUrl = signingResources.map { it.windowsCodeSigningInputsQueueUrl.get() }
  region = signingResources.map { it.region.get() }
  baseKeyPrefix = "inputs"
  jobFileDir = layout.buildDirectory.dir("uploadNugetPkg")
}

val downloadSignedNugetPkg = tasks.register<SignedArtifactDownloadTask>("downloadSignedNugetPkg") {
  outputDir = layout.buildDirectory.dir("signed-nuget")
  bucket = signingResources.map { it.codeSigningOutputsBucket.get() }
  queueUrl = signingResources.map { it.windowsCodeSigningOutputsQueueUrl.get() }
  region = signingResources.map { it.region.get() }
  jobFile = uploadUnsignedNugetPkg.map { it.jobFile.get() }
}

// ============================================================
// Documentation tasks

val docParse = tasks.register<JavaExec>("docParse") {
  group = "Internal"
  description = "Parse dotnet cs files for docs"
//  dependsOn("dotnetBuild")

  inputs.files(layout.projectDirectory.dir("../src/main/cs/Saxon/Api"))
  inputs.files(csharpFeatureKeysSources)
  inputs.file(layout.projectDirectory.file("../tools/parsecs.xsl"))
  outputs.file(layout.buildDirectory.file("dotnetdoc/names.xml"))

  classpath = saxonClasspath
  mainClass = "net.sf.saxon.Transform"
  args("-it:main",
    "-xsl:${layout.projectDirectory.file("../tools/parsecs.xsl")}",
    "-o:${layout.buildDirectory.file("dotnetdoc/names.xml").get().asFile}",
    "inputdir=file://${layout.projectDirectory.file("../src/main/cs/Saxon/Api")}",
    "featurekeysdir=file://${layout.projectDirectory.file("../../featurekeys/build/cs/Saxon/Api")}")
}

val apiDocFile = buildTask.map { it.apiDocumentationFile.get() }

val docMergeTask = tasks.register<JavaExec>("docMerge") {
  group = "Internal"
  description = "Merge API docs for dotnet"

  inputs.files(apiDocFile)
  inputs.files(docParse)
  inputs.file(layout.projectDirectory.file("../tools/merge.xsl"))
  outputs.file(layout.buildDirectory.file("dotnetdoc/merged.xml"))

  classpath = saxonClasspath
  mainClass = "net.sf.saxon.Transform"
  args("-it:main",
    "-xsl:${layout.projectDirectory.file("../tools/merge.xsl")}",
    "-o:${layout.buildDirectory.file("dotnetdoc/merged.xml").get().asFile}",
    "names-file=${docParse.get().outputs.getFiles().getSingleFile()}",
    "api-doc-file=${apiDocFile.get().asFile}")
}

val dotnetDocHtml = tasks.register<DotNetHtmlDoc>("docHtml") {
  group = LifecycleTasks.DOCUMENTATION_GROUP
  description = "Create HTML output for dotnet docs"

  classpath = saxonClasspath
  merged = docMergeTask.map { it.outputs.files.singleFile }
  xslt = layout.projectDirectory.file("../tools/doctohtml.xsl")
  cssFiles.from(layout.projectDirectory.file("../tools/saxon-dotnetdoc.css"))
  outputDir = layout.buildDirectory.dir("dotnetdoc-html")
}

val dotnetDocXml = tasks.register<JavaExec>("docXml") {
  group = LifecycleTasks.DOCUMENTATION_GROUP
  description = "Create XML version of dotnet docs"

  inputs.files(docMergeTask)
  inputs.file(layout.projectDirectory.file("../tools/convert-to-jel-package.xsl"))
  outputs.dir(layout.buildDirectory.file("dotnetdoc-xml"))

  classpath = saxonClasspath
  mainClass = "net.sf.saxon.Transform"
  args("-s:${docMergeTask.get().outputs.files.singleFile}",
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

val test = tasks.register<DotNetRunner>("cs-test") {
  group = LifecycleBasePlugin.VERIFICATION_GROUP
  description = "Run SaxonCS nUnit tests"

  inputs.files(csprojTemplateTask)
  inputs.dir(layout.projectDirectory.dir("../src/test/nunit"))
  val reportDir = layout.buildDirectory.dir("reports/tests/csTest")
  outputs.dir(reportDir)

  dotnet = extension.dotnet
  command = "test"
  args("SaxonCS.sln", "--nologo", "--", "NUnit.TestOutputXml=${reportDir.get()}")
}

tasks.named(LifecycleBasePlugin.CHECK_TASK_NAME) {
  dependsOn(test)
}

// ============================================================

val copySampleResources = tasks.register<Copy>("copy_sample_sources") {
  into(layout.buildDirectory.dir("samples/resources"))
  from(sampleSources)
}

tasks.register<Exec>("samples") {
  dependsOn(buildTask)
  dependsOn(copySampleResources)

  val arch = extension.architecture.get().msbuild
  val plat = extension.platform.get().msbuild
  val edition = extension.edition.get()

  // FIXME: should the .net 8.0 ref use saxonBuild.dotnetversion?
  // FIXME: Should probably use a pyenv python / venv from the python subproject,
  //        Not to mention needing a requirements.txt as this fails for me with a
  //        missing pypi module
  commandLine = listOf("python", "../../tools/run-samples.py",
    "--edition", edition,
    "--platform", "csharp",
    "--csexec", "build/${arch}/SaxonCS/bin/${CSReleaseConfiguration.Release}/net8.0/${plat}-${arch}/SaxonCS")
}

// ============================================================

val csprojTemplateTask = tasks.register<JavaExec>("csproj-template") {
  inputs.files(layout.projectDirectory.file("../tools/generateCSProjFiles.xsl"))
  outputs.file(layout.projectDirectory.file("SaxonCS.csproj"))
  outputs.file(layout.projectDirectory.file("SaxonCS.sln"))
  outputs.file(layout.projectDirectory.file("NUnitTests.csproj"))
  outputs.file(layout.projectDirectory.file("Directory.Build.props"))
  classpath = saxonClasspath
  mainClass = "net.sf.saxon.Transform"
  args("-it",
    "-xsl:${layout.projectDirectory.file("../tools/generateCSProjFiles.xsl")}",
    "-o:${layout.projectDirectory.file("SaxonCS.sln")}",
    "release=${CSReleaseConfiguration.Debug}",
    "platform=${extension.platform.get().msbuild}",
    "arch=${extension.architecture.get().msbuild}",
    "edition=${extension.edition.get()}",
    "framework=net${saxonBuild.dotnetversion.get()}",
    "saxonVersion=${saxonBuild.version.get()}",
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
  dependsOn(buildTask)
  dotnet = extension.dotnet
  version = saxonBuild.version
  dotnetversion = saxonBuild.dotnetversion
  edition = extension.edition
  configuration = CSReleaseConfiguration.Debug
  slnFile = configureTask.map { it.testSlnFile.get() }

  group = LifecycleTasks.ACCEPTANCE_TEST_GROUP
  target = "TestDrivers"
  arch = extension.architecture
  platform = extension.platform
  inputs.files(copyTestDrivers)
}
