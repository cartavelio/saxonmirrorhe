# Building Saxon HE, PE, and EE

The Saxon family of products  is built with Gradle.

## Prerequisites

* A Java environment suitable for running Gradle.

## Environment variables

Some prerequisites for building and/or testing are outside the scope
of the build. Environment variables are used to configure access to
these items.

* `SAXON_LICENSE_DIR`: The directory that contains your Saxon EE license.
* `W3C_QT3_TEST_SUITE_DIR`: The directory that contains your local
  clone of the [W3C QT3 test suite repository](https://github.com/w3c/qt3tests).
* `W3C_XSLT30_TEST_SUITE_DIR`: The directory that contains your local
  clone of the W3C [XSLT 3.0 test suite repository](https://github.com/w3c/xslt30-test).
* `W3C_XSD_TEST_SUITE_DIR`: The directory that contains your local
  clone of the W3C [XSD test suite repository](https://github.com/w3c/xsdtests).

## Gradle properties

T.B.D.

## Running the build

This build uses the Gradle wrapper to ensure portability across different systems.

If you run the build scripts with `~/.gradlew`, you will be using the
version and configuration of Gradle that is known to work. (Other
versions may also work, but then again, they may not.)

These build scripts are shared across the open source HE repository,
the commercial EE/PE repository, and an internal repository that is
used for both current and future development.

In the open source HE repository, only the HE product can be built.
There are build targets for PE and EE artifacts, but they will not,
and are not expected to, succeed.

## T.B.D.

## Build outputs

All of the task outputs are written to files and directories under the
`build` directory. Although it shouldn’t be necessary, it is always
safe to completely delete the build directory before executing a task.
This ensures that _everything_ necessary to execute the task will be
rebuilt from the current sources.

## Signing jars

If your environment is configured to sign jars, running the release
and zip tasks with the Gradle property `signJars` set to `true` will
create signed jars. For example,

```
./gradlew -PsignJars=true javaZips
```

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

Every test suite task has two versions, one with the plain name and
one with `debug_` prepended to the name. (For example,
`debug_eeXslt30TestSuite_XT30_bcon_jit`)

The distinction is this: the task with `debug_` prepended runs against
the source code compiled without any preprocessing. This is the
version that you will want to run in the debugger, for example. The
version without `debug_` in the front runs against the production
version of the release.

### The testSuite closures

Tests are now configured with a set of methods on the `testSuite` extension.
For example:

```
testSuite.qt3 {
  debug true
  taskName "adHocTest"
  release "ee"
  language "XP31"
  directory "${projectDir}/src/test/extra/qt3"
  bytecode true
  option "-s:saxon-doc"
  option "-t:s-doc-001"
}
```

That creates two tasks that run the W3C QT3 test suite on the
specified directory using the EE release, the XP31 language, with
bytecode enabled. The additional options `-s:saxon-doc` and
`-t:s-doc-001` will be passed to the test suite.

The tasks are named `adHocTest` and `debug_adHocTest`. If you don’t
specify at task name, you’ll probably get a conflict with existing
tests because the default names are computed from the options.

Enabling `debug` will cause Gradle to print out the properties that
will be used to run the test suite. Most can be changed with methods
in the closure.

It would be tedious if you had to edit the Gradle build file and
create a new test for every set of options you might want to run, so
you can pass arbitrary options by encoding them in Gradle parameters.
The equivalent task using this method would be this one:

`./gradlew -Ptd_s=saxon-doc -Ptd_t=s-doc-001 debug_eeQT3ExtraTestSuite_XP31_bcoff`

Any Gradle parameter of the form `td_x=y` will be passed to the test suite driver as `-x:y`.

There’s one other option recognized by the driver configuration,
`tdReport`. This changes the output reporting directory.

## Viewing task dependencies

You can get nice diagrams showing task dependencies by adding
this plugin to `build.gradle`:

```
plugins {
  id "com.cdsap.talaiot" version "1.3.5"
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

It’s not in the build by default because it generates a deprecation warning for Gradle 7.0 on every run.

