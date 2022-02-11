# Building Saxon HE, PE, and EE

The Saxon Java, .NET, and C family of products are built with Gradle.

These build scripts are shared across the open source HE repository,
the commercial EE/PE repository, and an internal repository that is
used for both current and future development. The same build scripts
are shared across all of the repositories, even though not all the
build tasks are applicable in all of the repositories.

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
