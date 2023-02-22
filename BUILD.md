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
./gradlew tasks --all | grep TestSuite_
```

If you want to run just a single test suite, for example, the XSLT 3.0
test suite against EE with bytecode and jit enabled, you can run just
that task (e.g., `ee_runXslt30TestSuite_XT30_bcon_jit`).
(The names get a little clunky.)

If you’re running an individual test suite, there are Gradle
properties that you can use to control additional test suite options:

* `tdOutput` specifies the output filename (`-o:`)
* `tdTree` specifies the tree model (`-tree:`)
* `tdTestSet` specifies the test set (`-s:`)
* `tdTestName` specifies the test name (`-t:`)

More options can easily be added if they would be useful.

(In fact, you can use these properties when you run more than one test
suite, but I think they’re going to be most useful against single test
suites.)
