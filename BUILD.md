# Building Saxon

The Saxon family of products is built with Gradle. This document describes the
“new” build system.

## General structure

The build is organized around a repository and a set of git submodules for the
test suites. The build is managed by Gradle build scripts written in Kotlin.

### Motivation

The refactoring that went into this repository (compared to the previous) is
motivated by a goal of having faster and more predictable builds. The previous
build had a common, shared set of source directories. When different products
were built, those sources were copied and sometimes mechanically edited for the
product being built. This meant re-compiling things for every build. In this
repository, sources are separated and builds share build artifacts. This
leverages an underlying design pattern of Gradle rather than fighting against
it.

## Prerequisites

* A Java environment suitable for running Gradle. We currently build with
  JDK 21.

* Clone/Check out the repository

It’s probably best to make a new clone:

```
git clone git@saxonica.plan.io:saxonica/saxondev.git
```

Then checkout the branch you want. (The default will be on some horribly
out-of-date version of a defunct branch called `master`)

```
git checkout saxon12
```

* Initialize and update the submodules

```
git submodule init
git submodule update
```

* Copy the `hooks/pre-commit` file to `.git/hooks` (!!!)

You **must** **must** **must** copy the hooks file:

```
cp hooks/pre-commit .git/hooks/
```

* If you decide to make new clones for the refactored build, it’s best to remove
  the origin remote in your *old* versions
  
```
cd /where/you/have/the/previous/version
git remote remove origin
```

## TL;DR

See “Building with IntelliJ” or “Building with Rider” below.

## Organization

A summary of where things are, and why. Some very common files and directories
(`.git`, `.gitignore`, `build.gradle.kts`, etc. are elided for simplicity).

### Top level:

At the top level, there are directories for the subprojects and assorted
“global” tools, documentation, and miscellany.

* `BUILD.md`, this README
* `buildSrc`, Gradle plugins that simplify builds across the repository
* `csharp`, the C# products
* `documentation`, documentation
* `hooks`, git hooks (mostly to prevent putting open source and commercial code in a single commit)
* `java`, the Java products
* `libsaxonc`, the C++ family of products
* `libsaxonc-core`, the build scripts for running GraalVM to produce the core libraries
* `misc`, miscellanous tools and files not better categorized elsewhere
* `php`, the PHP products
* `python`, the Python products
* `READMEs`, README files, notices, and assorted documentation
* `src`, sources common to all products
* `submodules`, the test suites (and perhaps other dependencies)
* `tools`, tools (also a bit miscellaneous)
* `unicode`, support for building the tables needed for Unicode strings

### Java builds

In the Java builds, there are directories for each product and common tools.

* `he` and `pe`, the product builds; the top-level `java` directory builds
  roughly `ee`. It’s what IntelliJ builds and it has all of the sources.
  There are separate build tasks for “real” releases.
* `keys`, feature keys common to all Java products
* `lib`, libraries not available over Maven
* `src`, the sources
* `tools`, Java-specific tools used in the build or release process

### C++ builds

The `libsaxonc-core` projects is mostly self-contained; it manages the GraalVM
build from Java.

* `he`, `pe`, `ee`, the product builds
* `documentation`, C++ documentation builds
* `lib`, extra Jars, (`xercesImpl.jar`)
* `src`, the sources
* `tools`, C++-specific tools used in the build or release process

The `libsaxonc` project builds the C++ shared library that ships with the
SaxonC, Python, and PHP products.

* `he`, `pe`, `ee`, the product builds
* `cmake`, The CMake file
* `doc`, documentation builds (including Doxygen)
* `samples`, the samples
* `src`, the sources

The `php` project builds the PHP extensions.

* `he`, `pe`, `ee`, the product builds
* `saxon-config-base.m4`, the M4 macro configuration
* `src`, sources specific to PHP

The `python` project builds the Python extensions and the Python test driver.

* `he`, `pe`, `ee`, the product builds
* `documentation`, Python documentation builds
* `pyenv`, the pyenv tools for switching Python environments
* `saxonc`, the wheel build files
* `testsuite_driver`, the test suite driver

### C# builds

The C# projects are managing two distinct uses: development and debugging in a
tool like Rider, and release builds. The two processes share a lot of
commonality, but they are distinct. In particular, the solution file(s) (e.g.,
`SaxonCS-arm64.sln`) are for development. It’s not used for releases. The
solution files for a release are generated under the `{product}/build`
directory.

* `ee`, (and eventually `pe` and `he`), the product builds
* `keys`, feature keys common to all C# products
* `SaxonCS-arm64.sln`, the MSBuild solution file for use in Rider
* `src`, the sources, including the test driver
* `tools`, C#-specific tools used in the build or release process
* `transpiler`, the transpiler

Today, there’s only one solution file, for arm64 releases on MacOS. We’ll need
to add more if/when we want to setup development environments on other
architectures.

### Documentation

The documentation sources are (mostly) in the documentation project along with
the tools used by the build process.

* `src`, the sources
* `tools`, documentat-specific tools used in the build or release process

Documentation is a little bit messy because sources are also drawn from other
projects (JavaDoc from the Java project, Doxygen from the C++ project, etc.)

### Common sources

The top-level `src` directory contains sources, samples, data files and other
artifacts that are relevant to more than one subproject.

### Submodules

The `submodules` contain the test suites. In the future, if we have additional
projects that are separate but part of the build or release, they will be added
here.

### Gradle buildSrc

The refactored build makes quite significant use of the ability to create Gradle
plugins. Many of our builds are very similar. As much as is practical, the
similar parts of the build are managed by plugins. This keeps the actual build
scripts short and simple.

## Understanding tasks

In a Gradle project with subprojects, tasks are run by specifying the subproject
and the task. For example,

```
./gradlew java:he:jar
```

That runs the “jar” task for the “he” build of “java”. 

If you specify a task without a subproject, then that task runs in all the
subprojects where it exists. For example:

```
./gradlew jar
```

runs *all* of the `jar` tasks across the whole set of projects.

### Explore the tasks with the “tasks” task

You can review the available tasks with the “tasks” task. For example:

```
./gradlew java:ee:tasks
Running ./gradlew… (instead of /opt/homebrew/bin/gradle)
> Task :buildSrc:checkKotlinGradlePluginConfigurationErrors
> Task :buildSrc:generateExternalPluginSpecBuilders UP-TO-DATE
> Task :buildSrc:extractPrecompiledScriptPluginPlugins UP-TO-DATE
> Task :buildSrc:compilePluginsBlocks UP-TO-DATE
> Task :buildSrc:generatePrecompiledScriptPluginAccessors UP-TO-DATE
> Task :buildSrc:generateScriptPluginAdapters UP-TO-DATE
> Task :buildSrc:compileKotlin UP-TO-DATE
> Task :buildSrc:compileJava NO-SOURCE
> Task :buildSrc:compileGroovy NO-SOURCE
> Task :buildSrc:pluginDescriptors UP-TO-DATE
> Task :buildSrc:processResources UP-TO-DATE
> Task :buildSrc:classes UP-TO-DATE
> Task :buildSrc:jar UP-TO-DATE

> Task :java:ee:tasks

------------------------------------------------------------
Tasks runnable from project ':java:ee'
------------------------------------------------------------

Build tasks
-----------
assemble - Assembles the outputs of this project.
build - Assembles and tests this project.
buildDependents - Assembles and tests this project and all projects that depend on it.
buildNeeded - Assembles and tests this project and all projects it depends on.
classes - Assembles main classes.
clean - Deletes the build directory.
jar - Assembles a jar archive containing the classes of the 'main' feature.
release
testClasses - Assembles test classes.

Documentation tasks
-------------------
javadoc - Generates Javadoc API documentation for the 'main' feature.

Help tasks
----------
buildEnvironment - Displays all buildscript dependencies declared in project ':java:ee'.
dependencies - Displays all dependencies declared in project ':java:ee'.
dependencyInsight - Displays the insight into a specific dependency in project ':java:ee'.
help - Displays a help message.
javaToolchains - Displays the detected java toolchains.
kotlinDslAccessorsReport - Prints the Kotlin code for accessing the currently available project extensions and conventions.
outgoingVariants - Displays the outgoing variants of project ':java:ee'.
projects - Displays the sub-projects of project ':java:ee'.
properties - Displays the properties of project ':java:ee'.
resolvableConfigurations - Displays the configurations that can be resolved in project ':java:ee'.
tasks - Displays the tasks runnable from project ':java:ee'.

Testsuite tasks
---------------
test_suite_expath_XP31
test_suite_expath_XP31_unfolded
test_suite_expath_XQ31
test_suite_expath_XQ40
test_suite_qt3_XP31
test_suite_qt3_XP31_unfolded
test_suite_qt3_XQ31
test_suite_qt3_XQ31_unfolded
test_suite_qt3extra_XP31
test_suite_qt3extra_XQ31
test_suite_qt3extra_XQ40
test_suite_qt4_XP40
test_suite_qt4_XQ31
test_suite_qt4_XQ31_unfolded
test_suite_qt4_XQ40
test_suite_xslt30_XT30
test_suite_xslt30_XT30_export
test_suite_xslt30_XT30_strict
test_suite_xslt30extra_XT30
test_suite_xslt30extra_XT30_export
test_suite_xslt30extra_XT30_strict
test_suite_xslt30extra_XT40
test_suite_xslt30extra_XT40_export
test_suite_xslt30extra_XT40_strict
test_suite_xslt40_XT40
test_suite_xslt40_XT40_strict

Verification tasks
------------------
check - Runs all checks.
test - Runs the test suite.
```

We’ve attempted to put tasks into groups. If you think a task is in the wrong
group, or a new group is needed, you can update the task or open a build issue.

## Managing the test suites

The test suites are now submodules in the `submodules` directory. These are the
test suites that the build will use.

When you update a test suite (via pull request on the test suites repository,
for example), you can then pull the latest in the appropriate submodule and
commit that change.

In this way, the versions of the test suites are managed independently for the
various Saxon development branches. And they’re consistent across all of systems
where the test suites are run.

## Building with IntelliJ

1. Start IntelliJ and open the project as a Gradle project
2. Under Module Settings, select an SDK
3. In a Run/Debug configuration, select the `Saxon.java.main` classpath

You can also open the Gradle sidebar; from the sidebar you can choose tasks. For
example, Saxon/java/ee/Tasks/build/release will build a Java EE release.

### Running test suites

This part is a work in progress; you can make Run/Debug configurations to run
the test suites. 

1. Open the Gradle sidebar; from the sidebar navigate to the test set you want
   to run. For example, Saxon/java/ee/Tasks/testsuite/test_suite_expath_XP31
2. Clicking on that test suite will run it.
3. To specify a set or test, edit the configuration and add -Ptd_s and/or -Ptd_t
   to the run: `test_suite_xpath_XP31 -Ptd_s=expath-archive -Ptd_t=EXPath-archive-delete-002`

## Building With Rider

1. Start Rider and open the SaxonCS or TestDriver solution. The solutions are
   called SaxonCS-EE-arm64 and TestDriver-EE-arm64. There will have to be other,
   different versions for non-Mac, non-arm64 platforms, but we haven’t tried to
   set those up yet.
2. You must manually run the `csharp:ee:dotnetBuild` task by hand before you start
   (and after every `clean` build)
3. If you don’t run the initial gradle task after a clean build, Rider will be
   confused. Quit Rider, run the task, and restart Rider. :-(
4. Build and configure tests as usual.

# Useful build tasks

There are lots of tasks in each project. This section summarizes the ones you’re
likely to want to run directly.

Note that if you run a task without a prefix, for example `downloadRelease`,
Gradle will run that task in all of the projects that have a task with that name.
So `./gradlew java:ee:downloadRelease`, makes the Java EE release, 
`./gradlew downloadRelease`, makes *all* the releases.

## Java

To support debugging in IntelliJ, the Java project is configured slightly
differently. There are two editions, “he” and “pe”. The “ee” edition is what you
get from `:java:{task}` without specifying a subproject. So `{edition}` is
optional in the list below, leave it out for EE.

- `:java:{edition}:clean`, clean out the build files
- `:java:{edition}:downloadRelease`, make the release zip
- `:java:{edition}:javadocDownloadJar`, to make the JavaDoc jar
- `:java:{edition}:testDriversDownloadJar`, to make the test drivers jar
- `:java:{edition}:javadoc`, to make the (HTML) JavaDoc
- `:java:{edition}:xmljavadoc`, to make the XML JavaDoc

### Testsuite tasks

Not all of these are configured yet.

- `:java:{edition}:test_suite_{suite}_{language}`
- `:java:{edition}:all_test_suites`, run all the test suites

## C#

The only C# edition today is `ee` but `he` and `pe` are expected in the `main`
branch.

- `:csharp:{edition}:docHtml`, to make the HTML documentation
- `:csharp:{edition}:docXml`, to make the XML documentation
- `:csharp:ee:javadoc`, generates Javadoc API documentation (???)

- `:csharp:{edition}:generateCode`, transpile Java into C#
- `:csharp:{edition}:dotnetBuild`, build .NET
- `:csharp:{edition}:dotnetBuildTestDrivers`, build .NET test drivers 
- `:csharp:{edition}:dotnetPublish`, publish .NET
- `:csharp:{edition}:dotnetPublishZip`, publish .NET zip
- `:csharp:keys:featureKeys`, build the feature keys

## Testsuite tasks

Not all of these are configured yet.

- `:csharp:{edition}:test_suite_{suite}_{language}`
- `:csharp:{edition}:all_test_suites`, run all the test suites

## C/C++

TBD. Matt?

## Python

TBD. Matt?

## PHP

TBD. Matt?
