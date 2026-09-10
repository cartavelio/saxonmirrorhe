import com.saxonica.build.*
import com.saxonica.build.java.*
import org.gradle.api.tasks.compile.JavaCompile
import org.gradle.external.javadoc.StandardJavadocDocletOptions
import org.gradle.kotlin.dsl.*

plugins {
    id("java-library")
    `jvm-test-suite`
}

repositories {
    mavenLocal()
    mavenCentral()
    maven { url = uri("https://maven.saxonica.com/maven") }
}

val saxonBuild = the<SaxonBuildExtension>()
val testSuites = the<TestSuitesExtension>()

val extension = project.extensions.create<SaxonJExtension>("saxonj")

configure<TestSuitesExtension> {
    platform = "java"
    edition = extension.edition
}

java {
    sourceCompatibility = JavaVersion.VERSION_17
    targetCompatibility = JavaVersion.VERSION_17
}

val defaultJarManifestClasspath = listOf(
    "lib/xmlresolver-${findProperty("xmlresolverVersion")}.jar", "lib/xmlresolver.jar",
    "lib/xmlresolver-${findProperty("xmlresolverVersion")}-data.jar", "lib/xmlresolverdata-data.jar",
    "lib/jline-${findProperty("jlineVersion")}.jar", "lib/jline.jar",
)
extension.jarManifestClasspath.convention(defaultJarManifestClasspath)
extension.jarManifestClasspathDefaults.convention(defaultJarManifestClasspath)
extension.version = findProperty("saxonVersion") as String

// Create the custom source sets before any dependencies need to be set
val editionSourceSet = sourceSets.create("edition")

// Prevent transitive dependencies from pulling in the wrong (or multiple) versions of things
configurations.all {
  resolutionStrategy.eachDependency {
    if (requested.group == "org.xmlresolver") {
      useVersion(project.findProperty("xmlresolverVersion").toString())
    }
  }
}

// TODO: Exclude xml-apis as a transitive dependency

val javaSourcesBucket by configurations.dependencyScope("javaSourcesBucket")
val javaFeatureKeysBucket by configurations.dependencyScope("javaFeatureKeysBucket")
val featureKeysBucket by configurations.dependencyScope("featureKeysBucket")
val configXsdBucket by configurations.dependencyScope("configXsdBucket")
val sourceArtifacts by configurations.consumable("sourceArtifacts")
val resources by configurations.consumable("resources")
val filteredSourcesDir by configurations.consumable("filteredSourcesDir")
val xmldocletArtifacts by configurations.consumable("xmldocletArtifacts")
val javadocArtifacts by configurations.consumable("javadocArtifacts")
val saxonJarArtifact by configurations.consumable("saxonJarArtifact")

val releaseZips by configurations.consumable("releaseZips")
val releaseMaven by configurations.consumable("releaseMaven")

val preprocessor by configurations.creating {
    extendsFrom(configurations["runtimeOnly"])
}
val testDrivers by configurations.creating {
    extendsFrom(configurations["testImplementation"])
}
val testRuntime by configurations.creating {
    extendsFrom(configurations["runtimeOnly"])
}

val compileOnlyDependencies = configurations.consumable("compileOnlyDependencies") {
    extendsFrom(configurations["compileOnly"])
}

val implementationDependencies = configurations.consumable("implementationDependencies") {
    extendsFrom(configurations["implementation"])
}

val xmldoclet by configurations.creating {
  extendsFrom(configurations["implementation"])
  extendsFrom(configurations["compileOnly"])
}

val downloadReleaseBundled by configurations.creating {}
val editionCompileAndTestOnly by configurations.creating {
    extendsFrom(downloadReleaseBundled)
}
configurations["implementation"].extendsFrom(downloadReleaseBundled)

val editionCompileOnly = configurations["editionCompileOnly"]
editionCompileOnly.extendsFrom(configurations.compileOnly.get())
val editionImplementation by configurations.getting {
    extendsFrom(configurations["implementation"])
}
editionCompileOnly.extendsFrom(configurations.compileOnly.get())

configurations["compileOnly"].extendsFrom(editionCompileAndTestOnly)

val testImplementation by configurations.getting {
  extendsFrom(editionCompileAndTestOnly)
}
val testImplementationResolvable by configurations.resolvable("testImplementationResolvable") {
  extendsFrom(testImplementation)
}

configurations.configureEach {
  exclude("net.sf.saxon", "Saxon-HE")
  exclude("com.saxonica", "Saxon-EE")
}

extension.javaTestPathSuffix = "test/java"
val javaMainSourceRoot = extension.sourceRoot.map { it.dir("main/java") }
val javaMainResourcesRoot = extension.sourceRoot.map { it.dir("main/resources") }
val javaTestSourceRoot = extension.javaTestSourceRoot
val javaTestResourceRoot = extension.sourceRoot.map { it.dir("test/resources") }
val libTidyPath = extension.sourceRoot.map { it.file("../lib/Tidy.jar") }
dependencies {
    javaSourcesBucket(files(javaMainSourceRoot))
    javaFeatureKeysBucket(project(mapOf("path" to ":featurekeys",
                                        "configuration" to "javaFeatureKeys")))
    featureKeysBucket(project(mapOf("path" to ":featurekeys",
                                    "configuration" to "featureKeysSources")))
    configXsdBucket(project(mapOf("path" to ":samples",
                                  "configuration" to "configXsd")))

    xmldoclet("com.saxonica:xmldoclet:${findProperty("xmldocletVersion")}")
    xmldoclet(editionSourceSet.output)

    downloadReleaseBundled("org.xmlresolver:xmlresolver:${findProperty("xmlresolverVersion")}")
    downloadReleaseBundled("org.xmlresolver:xmlresolver:${findProperty("xmlresolverVersion")}:data")
    downloadReleaseBundled("org.nineml:coffeegrinder:${findProperty("ninemlVersion")}") {
        exclude("net.sf.saxon", "Saxon-HE")
        exclude("com.saxonica", "Saxon-EE")
        // I don't think these should be in the dependencies for CoffeeFilter, but
        // https://discuss.gradle.org/t/how-do-i-track-down-a-dependency/52035
        exclude("org.jetbrains.kotlin", "kotlin-stdlib")
        exclude("org.jetbrains", "annotations")
    }
    downloadReleaseBundled("org.nineml:coffeefilter:${findProperty("ninemlVersion")}") {
        exclude("net.sf.saxon", "Saxon-HE")
        exclude("com.saxonica", "Saxon-EE")
        exclude("org.relaxng", "jing")
        // I don't think these should be in the dependencies for CoffeeFilter, but
        // https://discuss.gradle.org/t/how-do-i-track-down-a-dependency/52035
        exclude("org.jetbrains.kotlin", "kotlin-stdlib")
        exclude("org.jetbrains", "annotations")
    }

    editionCompileAndTestOnly("xom:xom:1.3.5")
    editionCompileAndTestOnly("org.jdom:jdom:1.1.3")
    editionCompileAndTestOnly("org.jdom:jdom2:2.0.6.1")
    editionCompileAndTestOnly("dom4j:dom4j:1.6.1")
    editionCompileAndTestOnly("org.apache.ws.commons.axiom:axiom:1.2.15")
    editionCompileAndTestOnly("org.apache.ws.commons.axiom:axiom-dom:1.2.15")
    editionCompileAndTestOnly("org.apache.ws.commons.axiom:axiom-impl:1.2.15")
    implementation("nu.validator:htmlparser:${findProperty("htmlparserVersion")}")
    downloadReleaseBundled("jline:jline:${findProperty("jlineVersion")}")
    compileOnly("javax.mail:javax.mail-api:${findProperty("mailVersion")}")
    compileOnly("com.sun.mail:javax.mail:${findProperty("mailVersion")}")
    compileOnly("org.graalvm.sdk:graal-sdk:${findProperty("graalSdkVersion")}")
    compileOnly("org.graalvm.nativeimage:svm:${findProperty("graalSvmVersion")}")
    compileOnly(libTidyPath.map { files(it.asFile) })

    testImplementation("org.junit.jupiter:junit-jupiter:5.9.2")
    testImplementation("org.junit.jupiter:junit-jupiter-api:5.9.2")
    testImplementation("org.junit.jupiter:junit-jupiter-engine:5.9.2")
    testImplementation("org.apiguardian:apiguardian-api:1.0.0")
    testImplementation("org.hamcrest:hamcrest:2.2")
    testImplementation("org.apache.ant:ant:1.10.9")
    testImplementation("com.squareup.okhttp3:mockwebserver:4.12.0")
    testImplementation("com.sparkjava:spark-core:2.9.4")
    testImplementation("${findProperty("sqlDriverGroup")}:${findProperty("sqlDriverName")}:${findProperty("sqlDriverVersion")}")
    testImplementation(files(saxonBuild.saxonLicenseDir.get()))

    preprocessor("com.igormaznitsa:jcp:7.0.4")
    preprocessor("com.tonicsystems:jarjar:0.6")
    preprocessor("net.sf.saxon:Saxon-HE:12.4")
    preprocessor(files(saxonBuild.saxonLicenseDir.get()))
}

val javaSources = configurations.resolvable("javaSources") {
    extendsFrom(javaSourcesBucket)
}
val unprocessedJavaSources = configurations.consumable("unprocessedJavaSources") {
    extendsFrom(javaSourcesBucket)
}
val javaFeatureKeysSources = configurations.resolvable("javaFeatureKeysSources") {
    extendsFrom(javaFeatureKeysBucket)
}
val featureKeysSources = configurations.resolvable("featureKeysSources") {
    extendsFrom(featureKeysBucket)
}
val configXsd = configurations.resolvable("configXsd") {
  extendsFrom(configXsdBucket)
}

// from() is set in the edition build.gradle.kts
val filteredSources = tasks.register<Sync>("filteredSources") {
    into(layout.buildDirectory.dir("tmp/filtered-sources"))
    duplicatesStrategy = DuplicatesStrategy.WARN
}

val bundledFilteredSources = tasks.register<Sync>("bundleFilteredSources") {
    into(layout.buildDirectory.dir("tmp/bundled-filtered-sources"))
    from(filteredSources)
    from(javaFeatureKeysSources)
    duplicatesStrategy = DuplicatesStrategy.WARN
}

filteredSourcesDir.outgoing.artifact(bundledFilteredSources.map { it.destinationDir }) { builtBy(bundledFilteredSources) }

val preprocessJava = tasks.register<PreprocessSourcesTask>("preprocessJava") {
    classpath.from(configurations.named("preprocessor"))
    sources = bundledFilteredSources.map { it.destinationDir }

    opts {
        off("EE", "PE", "STREAM", "SCHEMA", "OPT", "SAXONC", "BYTECODE", "CSHARP")
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

sourceArtifacts.outgoing.artifact(preprocessJava)

val preprocessResources = tasks.register<ProcessResources>("editionProcessResources") {
    into(layout.buildDirectory.dir("src/main/resources"))
    from(javaMainResourcesRoot) {
        include("net/**")
    }
}

sourceArtifacts.outgoing.artifact(preprocessResources)
resources.outgoing.artifact(preprocessResources) { builtBy(preprocessResources) }

sourceSets.named("edition") {
    java {
        setSrcDirs(listOf(preprocessJava))
    }
    resources {
        setSrcDirs(listOf(preprocessResources))
    }
}

sourceSets {
    main {
        java {
            srcDir(javaFeatureKeysSources)
        }
    }
}

val compile = tasks.named("compileEditionJava")
tasks.register("allJars") {
    finalizedBy("editionDownloadJar")
    finalizedBy("testDriversDownloadJar")
}

testing {
  suites {
    val test by getting(JvmTestSuite::class) {
      useJUnitJupiter()
    }

    register<JvmTestSuite>("editionTest") {
      // Keep a record of the suite around so that we can add test sources at plugin configuration time
      extension.editionTestSuite = this
      sources {
        java {
          srcDir(javaTestSourceRoot)
        }
        resources {
          setSrcDirs(listOf(javaTestResourceRoot))
        }
      }
      dependencies {
        implementation(editionSourceSet.output)
        implementation(testImplementationResolvable)
      }

      targets {
        val processEditionTestResources = tasks.named<ProcessResources>("processEditionTestResources")
        val resourceJars = processEditionTestResources.map {
          files(it.destinationDir).asFileTree.filter { f ->
            f.extension == "jar"
          }
        }

        all {
          testTask.configure {
            useJUnitPlatform {
              excludeTags("performance", "mikeonly")
            }

            environment("SAXON_TEST_DATA", extension.testDataDir.map { it.asFile }.get().toString())
            classpath += resourceJars.get()
          }
          tasks.named(LifecycleBasePlugin.CHECK_TASK_NAME) {
            dependsOn(testTask)
          }
        }
      }
    }
  }
}

tasks.named<Test>("test") {
    environment("SAXON_TEST_DATA", extension.testDataDir.map { it.asFile }.get().toString())
}

// This is not a <Javadoc> task because the Gradle task is so lame
val editionJavadoc = tasks.register<Exec>("editionJavadoc") {
  group =  LifecycleTasks.DOCUMENTATION_GROUP
  inputs.files(editionSourceSet.output)
  inputs.files(preprocessJava)
  dependsOn(preprocessJava)

  outputs.dir(layout.buildDirectory.dir("javadoc"))

  val sourceDir = layout.buildDirectory.dir("src/main/java").get()
  val sourcePath = sourceDir.asFile.toPath()
  val relativizedPath: (File) -> java.nio.file.Path = { f ->
    sourcePath.relativize(f.toPath().parent)
  }
  val toPackageName: (java.nio.file.Path) -> String = { p ->
    p.iterator().asSequence().joinToString(".")
  }
  if (sourceDir.asFileTree.map(relativizedPath).toSet().map(toPackageName).isEmpty()) {
    executable = "/bin/echo"
    args("***********************************************************************\n"
         + "Run the Javadoc task again. It depends on the output of preprocessJava.\n"
         + "***********************************************************************")
  } else {
    executable = "javadoc"
    args(
        "-sourcepath", "${sourcePath}",
        "-Xdoclint:none", // -Xdoclint:all, accessibility, html, missing, reference, syntax, none
        "-quiet",
        "-d", "${layout.buildDirectory.dir("javadoc").get()}",
        "-protected",
        "-bottom", "Copyright (c) 2004-2025 Saxonica Limited. All rights reserved.",
        "-doctitle", "Saxon ${extension.version.get()} Java API Documentation",
        "-group", "s9api Interfaces", "net.sf.saxon.s9api:net.sf.saxon.s9api.push:net.sf.saxon.s9api.streams",
        "-group", "Other Interfaces", "com.saxonica.config:net.sf.saxon:net.sf.saxon.lib:net.sf.saxon.om:net.sf.saxon.query:net.sf.saxon.sxpath:net.sf.saxon.value:net.sf.saxon.type:net.sf.saxon.xpath:net.sf.saxon.xqj",
        "-group", "External Interfaces", "javax.xml.xquery",
        "-source", "17"
    )

    args("-classpath", editionSourceSet.compileClasspath.joinToString(":"))
    args(sourceDir.asFileTree
             .map(relativizedPath).toSet()
             .map(toPackageName)
    )
  }
}

tasks.named(LifecycleTasks.DOCUMENTATION_TASK_NAME) {
    dependsOn(editionJavadoc)
}

javadocArtifacts.outgoing.artifact(editionJavadoc)

val xmljavadoc = tasks.register<Javadoc>("xmljavadoc") {
    group = LifecycleTasks.DOCUMENTATION_GROUP
  source = editionSourceSet.java
  classpath = xmldoclet
  title = "Saxon ${extension.edition.get().uppercase()} ${findProperty("saxonVersion")} Java API Documentation"
  setDestinationDir(layout.buildDirectory.dir("xmldoclet").get().asFile)
  val opts = options as StandardJavadocDocletOptions
  opts.addStringOption("sourcepath", editionSourceSet.java.srcDirs.first().toString())
  opts.docletpath = configurations.named("xmldoclet").get().files.toList()
  opts.doclet = "com.saxonica.xmldoclet.XmlDoclet"
}

xmldocletArtifacts.outgoing.artifact(xmljavadoc)

val jarMetaInf = tasks.register<JarManifestExtrasTask>("meta-inf-services") {
    outputDir = layout.buildDirectory.dir("jar-meta-inf-services")
    serviceEntries = extension.metaInfServices
}

val (editionDownloadJarTask, editionMavenJarTask) = extension.editionJarTasks(project) {
    common {
        inputs.property("inclusions", extension.editionJarInclusions)
        inputs.property("exclusions", extension.editionJarExclusions)
        from(jarMetaInf)
        from(compile) {
          exclude("previous-compilation-data.bin")
        }
        from(project.rootProject.layout.projectDirectory.dir("java/src/main/resources")) {
          include("net/sf/saxon/data/**")
        }

        exclude(extension.editionJarExclusions.get())
        include(extension.editionJarInclusions.get())

        manifest {
            attributes(mapOf("Main-Class" to extension.manifestMainClass))
        }
    }

    download {
        manifest {
            attributes(mapOf("Class-Path" to extension.jarManifestClasspath.map { entries -> (entries + "lib").joinToString(separator = " ") }))
        }
    }
}

tasks.named(LifecycleBasePlugin.ASSEMBLE_TASK_NAME) {
    dependsOn(editionDownloadJarTask, editionMavenJarTask)
}

val compileTestDrivers = tasks.register<JavaCompile>("compileTestDrivers") {
    destinationDirectory = layout.buildDirectory.dir("testdriver/classes")
    classpath = editionSourceSet.compileClasspath + files(compile)

    source = files(javaSources).asFileTree
    include("net/sf/saxon/testdriver/**.java")
}

val jarTaskBuilder = extension.jarTaskBuilder(project)

val (editionDownloadSourceJarTask, editionMavenSourceJarTask) = jarTaskBuilder("editionSource") {
    jarClassifier = "sources"
    common {
        from(preprocessJava)
        from(preprocessResources)
    }
}

val (testdriverJarTask, _) = jarTaskBuilder("testDrivers") {
    jarClassifier = "test"
    common {
        from(compileTestDrivers)
    }
}

val (_, testdriverMavenSourcesJarTask) = jarTaskBuilder("testDriversSource") {
    jarClassifier = "test-sources"
    common {
        from(preprocessJava) {
            include("net/sf/saxon/testdriver/**.java")
        }
    }
}

val (_, javadocMavenJarTask) = jarTaskBuilder("javadoc") {
    common {
        from(editionJavadoc)
    }
}

val signedReleaseArtifacts = findProperty("signedReleaseArtifacts") != "false"

val signingDownloadJars = tasks.register<Sync>("signingDownloadJars") {
    duplicatesStrategy = DuplicatesStrategy.WARN
    into(layout.buildDirectory.dir("signing/download"))
    from(editionDownloadJarTask)
    from(testdriverJarTask)
}

val signingMavenJars = tasks.register<Sync>("signingMavenJars") {
    duplicatesStrategy = DuplicatesStrategy.WARN
    into(layout.buildDirectory.dir("signing/mavendownload"))
    from(tasks.named<Jar>("editionMavenJar"))
    from(tasks.named<Jar>("javadocMavenJar"))
    if (extension.edition.get().uppercase() == "HE") {
      from(tasks.named<Jar>("editionSourceMavenJar"))
    }
    from(tasks.named<Jar>("testDriversMavenJar"))
    from(tasks.named<Jar>("testDriversSourceMavenJar"))
}

val signingResources = tasks.register<AwsSigningResources>("signingResources") {
    region = "eu-west-2"
    stackName = "SaxonicaCiCdStack"
}

val uploadUnsignedDownloadJars = tasks.register<UnsignedArtifactUploadTask>("uploadUnsignedDownloadJars") {
    inputDir = signingDownloadJars.map { it.destinationDir }
    bucket = signingResources.map { it.codeSigningInputsBucket.get() }
    queueUrl = signingResources.map { it.codeSigningInputsQueueUrl.get() }
    region = signingResources.map { it.region.get() }
    baseKeyPrefix = "inputs"
    jobFileDir = layout.buildDirectory.dir("uploadUnsignedDownloadJars")
}

val uploadUnsignedMavenJars = tasks.register<UnsignedArtifactUploadTask>("uploadUnsignedMavenJars") {
    inputDir = signingMavenJars.map { it.destinationDir }
    bucket = signingResources.map { it.codeSigningInputsBucket.get() }
    queueUrl = signingResources.map { it.codeSigningInputsQueueUrl.get() }
    region = signingResources.map { it.region.get() }
    baseKeyPrefix = "inputs"
    jobFileDir = layout.buildDirectory.dir("uploadMavenDownloadJars")
}

val downloadSignedDownloadJars = tasks.register<SignedArtifactDownloadTask>("downloadSignedDownloadJars") {
    outputDir = layout.buildDirectory.dir("signed-download")
    bucket = signingResources.map { it.codeSigningOutputsBucket.get() }
    queueUrl = signingResources.map { it.codeSigningOutputsQueueUrl.get() }
    region = signingResources.map { it.region.get() }
    jobFile = uploadUnsignedDownloadJars.map { it.jobFile.get() }
}

val downloadSignedMavenJars = if (signedReleaseArtifacts) {
  tasks.register<SignedArtifactDownloadTask>("downloadSignedMavenJars") {
    outputDir = layout.buildDirectory.dir("signed-maven")
    bucket = signingResources.map { it.codeSigningOutputsBucket.get() }
    queueUrl = signingResources.map { it.codeSigningOutputsQueueUrl.get() }
    region = signingResources.map { it.region.get() }
    jobFile = uploadUnsignedMavenJars.map { it.jobFile.get() }
  }
} else {
  tasks.register<Copy>("downloadSignedMavenJars") {
    dependsOn(signingMavenJars)
    into(layout.buildDirectory.dir("signed-maven"))
    from(layout.buildDirectory.dir("signing/mavendownload"))
  }
}

val downloadRelease = tasks.register<Zip>("downloadRelease") {
    group = LifecycleTasks.RELEASE_GROUP
    val releaseDocsRoot = extension.sourceRoot.dir("../../src/main")
    archiveBaseName = extension.edition.map { edition ->
        "Saxon${edition.uppercase()}${saxonBuild.publishVersion.get().replace(".", "-")}J"
    }

    destinationDirectory = layout.buildDirectory.dir("releases")

    if (signedReleaseArtifacts) {
      val name = "signed-download/saxon-${extension.edition.get()}"
      val vers = saxonBuild.publishVersion.get()
      from(downloadSignedDownloadJars.map { layout.buildDirectory.file("${name}-${vers}.jar") })
      from(downloadSignedDownloadJars.map { layout.buildDirectory.file("${name}-test-${vers}.jar") })
    } else {
      from(editionDownloadJarTask)
      from(testdriverJarTask)
    }

    into("lib") {
      from(downloadReleaseBundled.getIncoming().files)
    }
    into("doc") {
      from(releaseDocsRoot.map { it.dir("zipdoc") }) {
        include("**/*")
      }
    }
    into("notices") {
      from(releaseDocsRoot.map { it.dir("notices") })
      include("JAMESCLARK.txt", "JLINE2.txt", "LICENSE.txt")
    }
}

val release = tasks.named(LifecycleTasks.RELEASE_TASK_NAME) {
    dependsOn(downloadRelease)
}

val stageMaven = tasks.register<Zip>("stageMaven") {
    dependsOn("publish")
    archiveBaseName = "Saxon${extension.edition.get().uppercase()}-maven"
    destinationDirectory = layout.buildDirectory.dir("mavenrelease")
    from(layout.buildDirectory.dir("maven-o-matic"))
}

val copy_feature_keys = tasks.register<Copy>("copy_feature_keys") {
  group = "Internal"
  into(layout.buildDirectory.dir("featureKeys"))
  from(featureKeysSources)
}

val gizmo = tasks.register<JavaExec>("gizmo") {
  classpath = editionSourceSet.runtimeClasspath + files(saxonBuild.saxonLicenseDir.get())

  mainClass = "net.sf.saxon.Gizmo"

  standardInput = System.`in`
}

val transform = tasks.register<JavaExec>("transform") {
  classpath = editionSourceSet.runtimeClasspath + files(saxonBuild.saxonLicenseDir.get())

  mainClass = "net.sf.saxon.Transform"
}

val query = tasks.register<JavaExec>("query") {
  classpath = editionSourceSet.runtimeClasspath + files(saxonBuild.saxonLicenseDir.get())

  mainClass = "net.sf.saxon.Query"
}

saxonJarArtifact.outgoing.artifact(editionMavenJarTask) { builtBy(editionMavenJarTask) }
releaseZips.outgoing.artifact(downloadRelease) { builtBy(downloadRelease) }
releaseMaven.outgoing.artifact(stageMaven) { builtBy(stageMaven)}
