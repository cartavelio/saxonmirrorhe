import com.saxonica.build.LifecycleTasks
import com.saxonica.build.TargetHost
import com.saxonica.build.python.*
import org.apache.tools.ant.filters.ReplaceTokens
import org.gradle.kotlin.dsl.create

plugins {
    base
    id("saxon-build")
}

val extension = project.extensions.create<SaxonCPythonExtension>("saxoncPython")
val manageBranches = the<ManageBranchesExtension>()
val saxonBuild = the<SaxonBuildExtension>()

val targetHost = TargetHost.current()
val unixish = targetHost.isMacOS || targetHost.isLinux
val venvBaseDir = project.layout.buildDirectory.dir("pyvenv")
val pythonVersion: String by project
val pythonVersionsJson = layout.projectDirectory.file("../python-versions.json")
val pythonVersions = PythonVersions(pythonVersionsJson.asFile)
val manylinuxTarget: String by project

extension.pythonBinDirName.convention(if (unixish) "bin" else "scripts")
extension.venvBinarySuffix.convention(if (unixish) "" else ".exe")
extension.venvBaseDir.convention(venvBaseDir)
extension.venvBasePath.convention(venvBaseDir.map { it.asFile.toString() })
extension.version.convention(pythonVersion)
extension.manylinuxTarget.convention(manylinuxTarget)
extension.supportedPythonVersions = pythonVersions.supported

val libsaxoncReleaseArtifactsBucket by configurations.dependencyScope("libsaxoncReleaseArtifactsBucket")
val libsaxoncReleaseArtifactsDir by configurations.resolvable("libsaxoncReleaseArtifactsDir") {
    extendsFrom(libsaxoncReleaseArtifactsBucket)
}
val libsaxoncDebugArtifactsBucket by configurations.dependencyScope("libsaxoncDebugArtifactsBucket")
val libsaxoncDebugArtifactsDir by configurations.resolvable("libsaxoncDebugArtifactsDir") {
    extendsFrom(libsaxoncDebugArtifactsBucket)
}
val saxonCSampleDataBucket by configurations.dependencyScope("saxonCSampleDataBucket")
val saxonCSampleData by configurations.resolvable("saxonCSampleData") {
    extendsFrom(saxonCSampleDataBucket)
}
val devDebugPythonBucket by configurations.dependencyScope("devDebugPythonBucket")
val devDebugPythonExe by configurations.resolvable("debugPython") {
    extendsFrom(devDebugPythonBucket)
}

val releaseWhl by configurations.consumable("releaseWhl")

dependencies {
    libsaxoncReleaseArtifactsBucket(extension.edition.map { edition ->
        project(path = ":libsaxonc:${edition}", configuration = "releaseArtifactsDir")
    })
    libsaxoncDebugArtifactsBucket(extension.edition.map { edition ->
        project(path = ":libsaxonc:${edition}", configuration = "debugArtifactsDir")
    })
    saxonCSampleDataBucket(extension.edition.map { edition ->
        project(path = ":libsaxonc:${edition}", configuration = "sampleData")
    })
    devDebugPythonBucket(project(path=":python", configuration="debugPython"))
}

val pythonSrcDir = layout.projectDirectory.dir(provider { "../saxonc" })

val releaseWheels = tasks.register("release_wheels") {
    group = LifecycleTasks.RELEASE_GROUP
    description = "Build release wheels for all supported Python versions"
    outputs.dir(layout.buildDirectory.file("repaired-wheels"))

    doLast {
      mkdir(layout.buildDirectory.file("repaired-wheels"))
    }

    doLast {
      tasks.withType<RepairLocalWheel>().filter { it.name.contains("release_wheel") }.forEach { rtask ->
        copy {
          into(layout.buildDirectory.file("repaired-wheels"))
          from(rtask.outputs)
        }
      }
    }
}

releaseWhl.outgoing.artifact(releaseWheels) { builtBy(releaseWheels) }

val wheelTasksBuilder = WheelTasksBuilder.create(project, extension, saxonBuild)
val pyprojectToml = pythonSrcDir.map { it.file("pyproject.toml") }

val supportedPythonExes = mapOf<String, Provider<RegularFile>>(*extension.supportedPythonVersions.get().map { pythonVersion ->
    val v = pythonVersion.version
    val pythonBucket by configurations.dependencyScope("python${v}Bucket")
    val pythonExe by configurations.resolvable("python${v}") {
        extendsFrom(pythonBucket)
    }
    dependencies {
        pythonBucket(project(path = ":python", configuration = "python${v}"))
    }
    v to pythonExe.elements.map { layout.buildDirectory.file(it.single().asFile.readText()).get() }
}.toTypedArray())

extension.supportedPythonVersions.get().forEach { pythonVersion ->
    val v = pythonVersion.version
    val buildDirectory = layout.buildDirectory.dir(v)

    val venv = tasks.register<PythonVenv>("release_venv_${v}") {
        pythonEnv = extension
        branch = manageBranches.currentBranch
        suffix = "${v}-release"
        pyenvPythonExe = supportedPythonExes.get(v)!!
        buildSystemBootstrap = pyprojectToml
    }

    val releaseWheelPlatformTagTask = tasks.register<WheelCompatibilityTags>("wheel_platform_tag_${v}") {
        this.pythonExe = venv.map { it.pythonExe.get() }
        manylinuxTarget = extension.manylinuxTarget
        macosxDeploymentTarget = pythonVersion.macOsMinVersion(targetHost)
    }

    val repairDevWheel = wheelTasksBuilder {
        prefix = "${v}_release"
        sourceDir = pythonSrcDir
        venvTask = venv
        buildDir = buildDirectory.map { it.dir("release") }
        platformTagTask = releaseWheelPlatformTagTask
        libsaxoncDir = libsaxoncReleaseArtifactsDir.elements
    }

    val installDevWheel = tasks.register<InstallLocalWheel>("install_${v}_release_wheel") {
        group = LifecycleBasePlugin.BUILD_GROUP
        description = "Install the release wheel into the branch's VENV"
        pipExe = venv.map { it.pipExe.get() }
        wheel = repairDevWheel.map { it.repairedWheel.get() }
    }

    val pytestCopy = tasks.register<Copy>("${v}_pytest_copy") {
        from(pythonSrcDir.map { it.dir("test") })
        into(buildDirectory.map { it.dir("pytest") })
    }
    val pytestCopyDir = pytestCopy.map { it.destinationDir }

    val pipInstall = project.tasks.register<PipInstall>("${v}_pip_install") {
        pipExe = venv.map { it.pipExe.get() }
        requirements = pythonSrcDir.map { it.file("requirements.txt") }
    }

    val pyTest = tasks.register<PytestRunner>("${v}_pytest") {
        group = LifecycleBasePlugin.VERIFICATION_GROUP
        description = "Run unit tests with pytest"
        inputs.files(pyTestSamples)
        inputs.dir(pytestCopyDir)
        dependsOn(installDevWheel)
        dependsOn(pipInstall)

        pytest = venv.map { it.venvBinHostedExe("pytest").get() }

        workingDir = pythonSrcDir
        environment("SAXONC_HOME" to saxonBuild.saxonLicenseDir, "SAXONC_EDITION" to extension.edition)

        baseArgs("--import-mode=importlib", "--data-dir", pyTestData)
        tests("test")//, "tests")
    }

    releaseWheels {
        if (pythonVersion.hasReleaseArtifact) dependsOn(repairDevWheel)
    }
}

val devPyRelease = extension.version.map { pythonVersions.findSupported(it) }
val devPythonExe = supportedPythonExes.get(extension.version.get())!! // configurations.named<ResolvableConfiguration>(pythonVersions[extension.version.get()]!!)

val venv = tasks.register<PythonVenv>("venv") {
    pyenvPythonExe = devPythonExe
    pythonEnv = extension
    branch = manageBranches.currentBranch
    buildSystemBootstrap = pyprojectToml
}

val debugVenv = tasks.register<PythonVenv>("debug_venv") {
    pyenvPythonExe = devDebugPythonExe.elements.map { project.file(it.single().asFile.readText()) }
    pythonEnv = extension
    branch = manageBranches.currentBranch
    buildSystemBootstrap = pyprojectToml
    suffix = "debug"
}

/* We need the local dev/debug wheels to be built and repaired, but auditwheel currently refuses to just fix a wheel up
 * and not monkey with the platform tag. Use the most recent manylinux target for this machine.
 */
val localDevManylinuxTargetTask = tasks.register<LocalManylinuxTargetTask>("local_dev_manylinux_target") {
    cachefile = layout.buildDirectory.file("local-manylinux-target-cache/target")
}

val devWheelPlatformTagTask = tasks.register<WheelCompatibilityTags>("dev_wheel_platform_tag") {
    pythonExe = venv.map { it.pythonExe.get() }
    manylinuxTarget = localDevManylinuxTargetTask.map { it.manylinuxTarget }
    macosxDeploymentTarget = devPyRelease.map { it.macOsMinVersion(targetHost) }
}

val devWheelDebugPlatformTagTask = tasks.register<WheelCompatibilityTags>("dev_wheel_debug_platform_tag") {
    pythonExe = debugVenv.map { it.pythonExe.get() }
    manylinuxTarget = localDevManylinuxTargetTask.map { it.manylinuxTarget }
    macosxDeploymentTarget = devPyRelease.map { it.macOsMinVersion(targetHost) }
}

val pipInstall = project.tasks.register<PipInstall>("pip_install") {
    pipExe = venv.map { it.pipExe.get() }
    requirements = layout.projectDirectory.dir("../saxonc").file("requirements.txt")
}

val repairDevWheel = wheelTasksBuilder {
    sourceDir = pythonSrcDir
    venvTask = venv
    buildDir = layout.buildDirectory
    platformTagTask = devWheelPlatformTagTask
    libsaxoncDir = libsaxoncReleaseArtifactsDir.elements
}

val repairDevDebugWheel = wheelTasksBuilder {
    prefix = "debug"
    debug = true
    sourceDir = pythonSrcDir
    venvTask = debugVenv
    buildDir = layout.buildDirectory
    platformTagTask = devWheelDebugPlatformTagTask
    libsaxoncDir = libsaxoncDebugArtifactsDir.elements
}

val installDevWheel = tasks.register<InstallLocalWheel>("install_wheel") {
    group = LifecycleBasePlugin.BUILD_GROUP
    description = "Install the release wheel into the branch's VENV"
    pipExe = venv.map { it.pipExe.get() }
    wheel = repairDevWheel.map { it.repairedWheel.get() }
}

val installDevDebugWheel = tasks.register<InstallLocalWheel>("install_debug_wheel") {
    group = LifecycleBasePlugin.BUILD_GROUP
    description = "Install the debug wheel into the branch's VENV"
    pipExe = debugVenv.map { it.pipExe.get() }
    wheel = repairDevDebugWheel.map { it.repairedWheel.get() }
}

val pyTestSamples = tasks.register<Copy>("pytest_data") {
    from(saxonCSampleData)
    into(layout.buildDirectory.dir("samples/data"))
}
val pyTestData = pyTestSamples.map { it.destinationDir }

val pytestCopy = tasks.register<Copy>("pytest_copy") {
    from(layout.projectDirectory.dir("../saxonc/test"))
    into(layout.buildDirectory.dir("pytest"))
}
val pytestCopyDir = pytestCopy.map { it.destinationDir }

val pyTest = tasks.register<PytestRunner>("pytest") {
    group = LifecycleBasePlugin.VERIFICATION_GROUP
    description = "Run unit tests with pytest"
    inputs.files(pyTestSamples)
    inputs.dir(pytestCopyDir)
    dependsOn(installDevWheel)
    dependsOn(pipInstall)

    pytest = venv.map { it.venvBinHostedExe("pytest").get() }

    workingDir = layout.projectDirectory.dir("../saxonc")
    environment("SAXONC_HOME" to saxonBuild.saxonLicenseDir, "SAXONC_EDITION" to extension.edition)

    baseArgs("--import-mode=importlib", "--data-dir", pyTestData)
    tests("test")//, "tests")
}

tasks.named(LifecycleBasePlugin.CHECK_TASK_NAME) {
    dependsOn(pyTest)
}

val packagedSamplesDir = layout.buildDirectory.dir("packaged-samples")
val packageSamplesTask = tasks.register<Copy>("package_samples") {
    into(packagedSamplesDir.map { it.dir("samples") })
    from(layout.projectDirectory.dir("../saxonc/samples")) {
        include("*.py")
        filter { line ->
            if (line == "import saxonche as saxonc") "import saxonc${extension.edition.get()} as saxonc" else line
        }
    }
    from(layout.projectDirectory.dir("../saxonc/samples")) {
        exclude("*.py")
    }
}
val packagePytestsTask = tasks.register<Copy>("package_pytests") {
    into(packagedSamplesDir.map { it.dir("test") })
    from(layout.projectDirectory.dir("../saxonc/test"))
    from(layout.projectDirectory.dir("test"))
}
val packageTestdriverTask = tasks.register<Copy>("package_testdriver") {
    into(packagedSamplesDir.map { it.dir("testsuite_driver") })
    from(layout.projectDirectory.dir("../testsuite_driver")) {
        exclude(".venv/**", ".idea/**")
    }
}
val packageSamples = tasks.register<Copy>("package_samples_for_release") {
    dependsOn(packageSamplesTask, packagePytestsTask , packageTestdriverTask)
    into(packagedSamplesDir)
    from(layout.projectDirectory.dir("../")) {
        include("README-samples.md")
        rename { name -> "README.md" }
        filter<ReplaceTokens>("tokens" to mapOf("edition" to extension.edition.get()))
    }
}

val inputSrcDir = layout.projectDirectory.dir("../saxonc/src")
val releasableSrcDir = layout.buildDirectory.dir("releasable-src")
val sourceFilesForRelease = tasks.register<Sync>("source_files_for_release") {
    into(releasableSrcDir)
    from(inputSrcDir) {
        exclude("saxonc/readme.txt")
        filter<ReplaceTokens>("tokens" to mapOf("edition" to extension.edition.get(), "VERSION" to saxonBuild.version.get()))
    }
}
val venvTask = venv

val sphinxDocSources = tasks.register<Sync>("sphinx-doc-sources") {
    val autostarRegex = Regex("""^\.\.\s+(?<directive>auto[^:]+)::\s+saxonche\.""")
    into(layout.buildDirectory.dir("doc/source"))
    from(layout.projectDirectory.dir("../saxonc/doc")) {
        filter { line ->
            if (line == ".. automodule:: saxonche") ".. automodule:: saxonc${extension.edition.get()}"
            else if (autostarRegex.containsMatchIn(line)) autostarRegex.replace(line, ".. \${directive}:: saxonc${extension.edition.get()}.")
            else line
        }
    }
}

val sphinxDocTask = tasks.register<SphinxDoc>("sphinx-doc") {
    venv = venvTask
    docSrcDir = sphinxDocSources.map { it.destinationDir }
    docBuildDir = layout.buildDirectory.dir("doc/build")
    wheelInstaller(installDevWheel)
}

val packagedSamples by configurations.consumable("packagedSamples")
packagedSamples.outgoing.artifact(packagedSamplesDir) { builtBy(packageSamples) }
val sourceFiles by configurations.consumable("sourceFiles")
sourceFiles.outgoing.artifact(releasableSrcDir) { builtBy(sourceFilesForRelease) }
