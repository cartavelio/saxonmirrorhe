import org.gradle.api.GradleException
import org.gradle.api.Plugin
import org.gradle.api.Project

class ManageTestSuitesPluginExtension {
  String w3cQT3TestSuiteDir = System.getenv("W3C_QT3_TEST_SUITE_DIR")
  String w3cQT4TestSuiteDir = System.getenv("W3C_QT4_TEST_SUITE_DIR")
  String w3cXslt30TestSuiteDir = System.getenv("W3C_XSLT30_TEST_SUITE_DIR")
  String w3cXslt40TestSuiteDir = System.getenv("W3C_XSLT40_TEST_SUITE_DIR")
  String w3cXsdTestSuiteDir = System.getenv("W3C_XSD_TEST_SUITE_DIR")
  // I'm not hugely enamored of the casing in this variable, but it's consistent
  // with what the EXPath group uses.
  String w3cEXPathTestSuiteDir = System.getenv("W3C_EXPATH_TEST_SUITE_DIR")
  String saxonExtraTestSuiteDir = System.getenv("SAXON_EXTRA_TEST_SUITE_DIR")
}

public class ManageTestSuitesPlugin implements Plugin<Project> {
  void apply(Project project) {
    def extension = project.extensions.create("testsuites", ManageTestSuitesPluginExtension)

    // For each directory, if there's a project property, use that, otherwise
    // get the default from the environment.

    extension.w3cQT3TestSuiteDir = project.findProperty("w3cQT3TestSuiteDir") ?:
      System.getenv("W3C_QT3_TEST_SUITE_DIR")

    extension.w3cQT4TestSuiteDir = project.findProperty("w3cQT4TestSuiteDir") ?:
      System.getenv("W3C_QT4_TEST_SUITE_DIR")

    extension.w3cXslt30TestSuiteDir = project.findProperty("w3cXslt30TestSuiteDir") ?:
      System.getenv("W3C_XSLT30_TEST_SUITE_DIR")

    extension.w3cXslt40TestSuiteDir = project.findProperty("w3cXslt40TestSuiteDir") ?:
      System.getenv("W3C_XSLT40_TEST_SUITE_DIR")

    extension.w3cXsdTestSuiteDir = project.findProperty("w3cXsdTestSuiteDir") ?:
      System.getenv("W3C_XSD_TEST_SUITE_DIR")

    extension.w3cEXPathTestSuiteDir = project.findProperty("w3cEXPathTestSuiteDir") ?:
      System.getenv("W3C_EXPATH_TEST_SUITE_DIR")

    extension.saxonExtraTestSuiteDir = project.findProperty("saxonExtraTestSuiteDir") ?:
      System.getenv("SAXON_EXTRA_TEST_SUITE_DIR")

    // ============================================================
    // In an ideal world, I'd make the build process more standalone
    // by automatically cloning the repositories if they weren't
    // available. But the QT3 test suite in particular is huge and
    // takes a long time to clone. So for now, just force users to
    // tell us where they've cloned them.

    project.task("cloneQT3TestSuite") {
      doLast {
        if (extension.w3cQT3TestSuiteDir == null) {
          throw new GradleException("The location of the QT3 test suite is unknown.")
        }
      }
    }

    project.task("cloneXslt30TestSuite") {
      doLast {
        if (extension.w3cXslt30TestSuiteDir == null) {
          throw new GradleException("The location of the XSLT 3.0 test suite is unknown.")
        }
      }
    }

    project.task("cloneXsdTestSuite") {
      doLast {
        if (extension.w3cXsdTestSuiteDir == null) {
          throw new GradleException("The location of the XSD test suite is unknown.")
        }
      }
    }

    project.task("cloneQT4TestSuite") {
      doLast {
        if (extension.w3cQT4TestSuiteDir == null) {
          throw new GradleException("The location of the QT4 test suite is unknown.")
        }
      }
    }

    project.task("cloneXslt40TestSuite") {
      doLast {
        if (extension.w3cXslt40TestSuiteDir == null) {
          throw new GradleException("The location of the XSLT 4.0 test suite is unknown.")
        }
      }
    }

    project.task("cloneEXPathTestSuite") {
      doLast {
        if (extension.w3cEXPathTestSuiteDir == null) {
          throw new GradleException("The location of the EXPath test suite is unknown.")
        }
      }
    }

    project.task("cloneSaxonExtraTestSuites") {
      doLast {
        if (extension.saxonExtraTestSuiteDir == null) {
          throw new GradleException("The location of the Saxon extra test suites is unknown.")
        }
      }
    }
  }
}
