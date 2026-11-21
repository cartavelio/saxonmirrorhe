# Saxon-HE

[![SaxonJ-HE build and tests on main](https://github.com/cartavelio/saxonmirrorhe/actions/workflows/saxonj-he.yml/badge.svg?branch=main&event=push)](https://github.com/cartavelio/saxonmirrorhe/actions/workflows/saxonj-he.yml?query=branch%3Amain+event%3Apush)

Saxon Home Edition (Saxon-HE) is the open-source XSLT, XQuery, and XPath processor developed by Saxonica and its contributors. Cartavelio maintains this fork of [Saxonica’s upstream Saxon-HE source repository](https://saxonica.plan.io/projects/saxonmirrorhe/repository).

Development happens on `main`, based on Saxon 13. The fork restores the Java Sapling builder overloads and fixes an XSLT issue where an inline function returned to Java could lose access to the stylesheet’s global parameters. [Sapling fix](https://github.com/cartavelio/saxonmirrorhe/pull/2) · [XSLT fix](https://github.com/cartavelio/saxonmirrorhe/pull/3)

## Build and test SaxonJ-HE

Use JDK 21. The shared Gradle configuration also looks for the `dotnet` executable on your `PATH`. After cloning, run these commands from the repository root. The first command installs the required Git hooks.

```sh
cp hooks/* .git/hooks/
./gradlew --configure-on-demand :java:he:stageHeRuntime :java:he:editionTest
java/he/ci/smoke.sh java/he/build/he-runtime
```

The runnable JAR and its dependencies are in `java/he/build/he-runtime/`. The smoke test checks that a source stylesheet and its saved compiled form (SEF) produce the expected output for the same inputs.

GitHub Actions runs these checks on pull requests and pushes to `main`. To use a CI build, open a successful workflow run and download the `saxonj-he-runtime` artifact. The HE test report is available as `he-test-report`. [Workflow runs](https://github.com/cartavelio/saxonmirrorhe/actions/workflows/saxonj-he.yml)

For the broader Saxon build, see the [upstream setup notes](READMEs/README.FIRST.md) and [build instructions](BUILD.md).
