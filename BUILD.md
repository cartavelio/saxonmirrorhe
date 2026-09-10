# Building Saxon HE, PE, and EE

The Saxon family of products is built with Gradle.

## Prerequisites

* A Java environment suitable for running Gradle. We currently build with JDK 11.

## Environment variables

Some prerequisites for building and/or testing are outside the scope
of the `saxondev` repository. Environment variables are used to
configure access to these items.

* `SAXON_LICENSE_DIR`, the directory that contains your Saxon license.
* `SAXON_EXTRA_TEST_SUITE_DIR`, the Saxon extra test suites from the
  `git@saxonica.plan.io:saxonica/extra-test-suites.git` repository.
* `SAXON_XMARK_DATA`, the XMark test data, usually
  `$SAXON_EXTRA_TEST_SUITE_DIR/data/xmark`.
* `W3C_EXPATH_TEST_SUITE_DIR`, the `tests/qt3` directory of the XPath test suite
  from `git@github.com:expath/expath-cg.git`.
* `W3C_QT3_TEST_SUITE_DIR`, the path to a local clone of the QT3 test suite repo from
  `git@github.com:w3c/qt3tests.git`.
* `W3C_QT4_TEST_SUITE_DIR`, the path to a local clone of the QT4 test suite repo from
  `git@github.com:qt4cg/qt4tests.git`.
* `W3C_XSD_TEST_SUITE_DIR` the path to a local clone of the XSD test suite repo
  from `git@github.com:w3c/xsdtests.git`.
* `W3C_XSLT30_TEST_SUITE_DIR`, the path to a local clone of the XSLT 3.0 test suite repo from
  `git@github.com:w3c/xslt30-test.git`.
* `W3C_XSLT40_TEST_SUITE_DIR`, the path to a local clone of the XSLT 4.0 test suite repo from
  `git@github.com:qt4cg/xslt40-test.git`.
* `W3C_QT4_SPECS_DIR`, the path to a local clone of the QT4 specifications repo from
  `git@github.com:qt4cg/qtspecs.git`.

To build and run the SaxonCS unit tests, the path to the repo is needed, and
currently the easiest way to manage that is to provide it via an environment
variable:

* `SAXONDEV_DIR`, the absolute path to the `saxondev` repository.


## Build targets

An (incomplete) list of interesting build targets.

* `hecRelease`, `pecRelease`, `eecRelease` builds release artifacts for Java versions
* `javaZips` builds all the Java release zip files
* `all_saxonc_releases` builds all the SaxonC releases
* `saxonc_stage_wheels` builds the staging artifacts for SaxonC Python
  wheels. (Note that the Linux flavor must be built in a container
  with a specific Linux environment.)
* `dotnetPublishZip` builds the SaxonCS release artifact
* `javaArchiveHE` makes the Java source artifact for Saxon-HE.
* `dotnetMakeApp` on the Mac, this builds and release application.
  We use DMGCanvas to build and sign the DMG file.
* `releaseResources` builds the release resources.
* `documentation` builds all the documentation. This is a bit finicky
  and may not run easily on all systems.

## Gradle properties

* `sqlServer`, the SQL server to use in testing (either `mysql` or
  `postgresql`), defaults to `postgresql`
* `csconfiguration`, the SaxonCS build configuration, defaults to `Release`
* `csoptimize`, the SaxonCS build optimizer configuration, defaults to `false`
* `saxonLicenseDir`, alternative to the environment variable for locating the Saxon license file
* `saxonTestJava`, path to the version of Java to be used for running unit tests,
  defaults to the current version of Java being used by Gradle.
* `releaseBuild`, is this a release build? If true, extra checks are performed (the build
  must be able to sign release artifacts, etc.)
* `mavenLocalPublish` if set, must be a local filesystem path. All of the maven build
  artifacts for the `publish` target will be placed there.
* `sonatypeUsername` and `sonatypePassword`, credentials for Maven central publication.

### Testing properties

The following properties apply to test suites.

* `td_…` Any property that begins `td_` is passed to the test suite driver. For example,
  `td_t=xxx` will pass `-t:xxx` to the test suite driver.
* `saxonicaTestDriverReportArchive` location of archival test reports.
* `saxonicaTestDriverOutputRedirect`, if `true`, test driver output is
  redirected instead of printed on the console. Redirected output
  appears in the `build/test-results` directory for the test suite.
* `saxonicaTestDriverDebugExec` for debugging the test suite runner; if `true` will
  print the test suite runner command line.
* `saxonicaIgnoreFailures`, if `true` a failing test suite will not abort the build as a failure.

## Running the build

This build uses the Gradle wrapper to ensure portability across different systems.

If you run the build scripts with `~/.gradlew`, you will be using the
version and configuration of Gradle that is known to work. (Other
versions may also work, but then again, they may not.)

These build scripts are shared across the open source HE repository,
the commercial EE/PE repository.

In the open source HE repository, only the HE product can be built.
There are build targets for PE and EE artifacts, but they will not,
and are not expected to, succeed.

## Build outputs

All of the task outputs are written to files and directories under the
`build` directory. Although it shouldn’t be necessary, it is always
safe to completely delete the build directory before executing a task.
This ensures that _everything_ necessary to execute the task will be
rebuilt from the current sources.

### Building wheels

Building wheels is complicated. There are three different Saxon
products each with five different flavors of wheel and each for
several different versions of Python:

* Mac for x86 and aarch64
* Linux for x86 and arm64
* Windows for x86 (and someday, maybe also arm64)

We rely on `cibuildwheel` to create relocatable shared libraries. That
requires building the Linux wheel versions of SaxonC with particular
(and slightly older) versions of the C++ toolchain.

* Mac for x86 can be built on an Intel Mac
* Mac for aarch64 be built on an Apple Silicon Mac
* Linux for x86 is built in a [Docker container](https://saxonica.plan.io/projects/ci-tools/repository/33/revisions/main/show/docker/saxondev)
* Linux for Arm64 is built in a VMWare container on an Apple Silicon
  Mac. The `cibuildwheel` process runs in that container because there
  is no Arm64 host available on GitHub.
* Windows for x86 is built on a Windows machine

The `cibuildwheel` for all of the platforms except Linux/arm64 runs in
the [BuildWheels](https://github.com/Saxonica/BuildWheels/) repository
on GitHub. Because we don’t have the sources for SaxonC available on
GitHub, a yet even more complicated process is used.

1. Build SaxonC for Python wheels using the `saxonc_stage_wheels`
   target in the appropriate environment.
2. Copy the staged wheels from the `build/distributions/` directory to
   the staging area on `dev.saxonica.com`. The staging area is
   `/var/www/html/dev.saxonica.com/github-wheel-sources/USER/`, where
   USER is your login id.
3. Update the `config.json` file in the `BuildWheels` project.
4. Push the change to `config.json` to trigger a build.

The `config.json` file has several properties:

```
{
  "user": "norm",
  "macx86_64": true,
  "macaarch64": true,
  "macpath": "/Volumes/Saxonica/src/saxonica/saxondev",
  "linux": false,
  "windows": false,
  "date": "2023-05-02T19-00-00"
}
```

* `user` identifies the user and determines which staged libraries will be retrieved.
* `macx86_64` and `macaarch64` determine whether the Mac wheels should be built.
* `macpath` is the absolute path to the `saxondev` repository on the
  machine where you built the Mac release. This is required by the Mac version of the
  tool that builds relocatable shared libraries.
* `linux` determines whether the (x86) Linux wheels should be built.
* `windows` determines whether the Windows wheels should be built.
* `date` is actually irrelevant, but gives you something to change if
  you need to trigger new build for the same configuration.

The `saxonc_build_wheels` target will automate all of this for you on
a Mac. It doesn’t work for Linux because you have to build in a
container anyway. It doesn’t work for Windows because it relies on
`scp` and other tools that aren’t typically installed.

## Signing jars

If your environment is configured to sign jars, running the release
and zip tasks will create signed jars.

## Running JUnit tests

The main entry point for running the JUnit tests is the `eejTests`
task. By default, this task runs all of the JUnit tests. You can
select packages, classes, methods, and tags with Gradle properties:

* `testPackage` specifies a package to test. 
* `testClass` specifies a class to test.
* `testMethod` specifies a method to test.
* `testTag` specifies tags to test.

Each of these properties can be a comma-separated list of values. If
the value begins with a hyphen, it acts as an exclude rather than an
include. In other words, `-PtestTag=a,b,c` runs all tests tagged “a”,
“b”, or “c” whereas `-PtestTag=-b` runs all of the tests *except*
those tagged with “b”.

If a list of classes is given, any unqualified class name will be
assumed to be in the same package as the previous class. So
`-PtestClass=package.Test1,Test2` is exactly the same as
`-PtestClass=package.Test1,package.Test2`.

Similarly, if a list of methods is given, any method name that doesn’t
include a “#” will be assumed to be in the same class as the previous
method. So `-PtestMethod=package.class#one,two` is the same as
`-PtestMethod=package.class#one,package.class#two`.

## Running the samples

There are tasks to each of the samples programs under HE, PE, and EE
(where appropriate). The non-debug versions write output to
`build/samples/results` rather than to stdout.

The umbrella tasks `he_samples` and `ee_samples` will run all of them.
When run this way, the output is spot-checked with a Python program,
`tools/sample-check.py`. The checking is not exhaustive, it simply
matches a few regular expressions against the lines at the beginning
of the file.

To run that program, you need Python version 3 and the `click` module.

The sample checker will cause the build to fail if any of the samples
appear to have errors. If you want to run all the samples and ignore
errors, set the Gradle property `ignoreSampleErrors`.

## Running the test suites

There are build tasks for running the W3C and Saxonica test suites
against the Java builds. The umbrella task is `testSuitesJava`. That
runs them all. If you’re interested in only one release, you can run
`hejTestSuitesJava`, `pejTestSuitesJava` or `eejTestSuitesJava`.

The release report can be built with the `testSuiteReportsJava` task.
That task runs the report against any results that you have lying
around. If you want to run all the tests and generate the report,
that’s what the `runTestSuiteReportsJava` task does.

The way this all works is that a different Gradle task is constructed
for each combination to be tested: product release, test suite, and
test suite options.

If you want to see all of the test suites, try

```
./gradlew tasks --all | grep TestSuite
```

If you want to run just a single test suite, for example, the XSLT 3.0
test suite against EE with bytecode and jit enabled, you can run just
that task (e.g., `eeXslt30TestSuite_XT30_bcon_jit`).
(The names get a little clunky.)

The Python test suites are qt3 and xslt30. They can be run with any of
the Python builds. The build target `all_python_test_suites` will run
them all. The individual tests are `{h,e,p}ec_{xslt30,qt3}_test_suite`

### The test suites

Instructions for configuring the test suites can be found at the top of 
`test-suites.gradle`.

## Viewing task dependencies

The [Talaiot plugin](https://github.com/cdsap/Talaiot) can be used to view task
dependencies. It’s not installed by default because it often generates
warnings when Gradle is upgraded.

Try:

```
plugins {
  id "com.cdsap.talaiot" version "1.5.1"
}
```

and this configuration:

```
talaiot {
  metrics {
    performanceMetrics = false
  }
  publishers {
    taskDependencyGraphPublisher {
      gexf = false
      dot = true
      html = false
    }
  }
}
```
