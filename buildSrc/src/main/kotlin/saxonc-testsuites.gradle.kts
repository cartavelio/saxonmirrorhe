import com.saxonica.build.LifecycleTasks
import com.saxonica.build.python.*
import org.gradle.api.tasks.*
import org.gradle.internal.os.OperatingSystem
import org.gradle.kotlin.dsl.register
import org.gradle.kotlin.dsl.the

val saxonBuild = the<SaxonBuildExtension>()
val saxonC = the<SaxonCPythonExtension>()

val systemOs = OperatingSystem.current()
val unixish = systemOs.isMacOsX() || systemOs.isLinux()

val venv = tasks.named<PythonVenv>("venv")

val testSuites = tasks.named(LifecycleTasks.ACCEPTANCE_TEST_TASK_NAME)

val testsuitesPipInstall = tasks.register<PipInstall>("testsuites_pip_install") {
    pipExe = venv.map { it.pipExe.get() }
    requirements = layout.projectDirectory.dir("../testsuite_driver").file("requirements.txt")
}

val testsuiteInstallWheel = tasks.named<InstallLocalWheel>("install_wheel")

fun qtNTestSuite(qtVersion: String, lang: String): TaskProvider<TestSuiteTask> {
    return tasks.register<TestSuiteTask>("test_suite_${qtVersion}_${lang}") {
        group = LifecycleTasks.ACCEPTANCE_TEST_GROUP
        python = venv.map { it.pythonExe.get() }
        environment("SAXONC_HOME" to saxonBuild.saxonLicenseDir,
            "SAXON_LICENSE_DIR" to saxonBuild.saxonLicenseDir,
            "SAXONC_EDITION" to saxonC.edition)

        driver = layout.projectDirectory.file("../testsuite_driver/qt3_test_suite_driver.py")
        suiteDir = layout.projectDirectory.dir("../../submodules/${qtVersion}")
        outputDir = layout.buildDirectory.dir("suite/${qtVersion}_${lang}")
        language = lang
        wheelInstaller(testsuiteInstallWheel)
        driverRequirements(testsuitesPipInstall)
    }
}

listOf("qt3", "qt4").forEach { qtVersion ->
    val qtNTestSuite = tasks.register("test_suite_${qtVersion}_all")

    listOf("XP31", "XQ31", "XP40", "XQ40").forEach { lang ->
        var suite = qtNTestSuite(qtVersion, lang)

        qtNTestSuite.configure {
            dependsOn(suite)
        }
    }

    testSuites.configure {
        dependsOn(qtNTestSuite)
    }
}

listOf("xslt30", "xslt40").forEach { xslt ->
    val suite = tasks.register<TestSuiteTask>("test_suite_${xslt}") {
        group = LifecycleTasks.ACCEPTANCE_TEST_GROUP
        python = venv.map { it.pythonExe.get() }
        environment("SAXONC_HOME" to saxonBuild.saxonLicenseDir,
            "SAXON_LICENSE_DIR" to saxonBuild.saxonLicenseDir,
            "SAXONC_EDITION" to saxonC.edition)

        driver = layout.projectDirectory.file("../testsuite_driver/xslt30_test_suite_driver.py")
        suiteDir = layout.projectDirectory.dir("../../submodules/${xslt}")
        outputDir = layout.buildDirectory.dir("suite/${xslt}")
        wheelInstaller(testsuiteInstallWheel)
        driverRequirements(testsuitesPipInstall)
    }

    testSuites.configure {
        dependsOn(suite)
    }
}
