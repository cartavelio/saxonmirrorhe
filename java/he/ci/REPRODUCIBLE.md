# Reproducible HE JARs

Use the normal HE Maven JAR task with an explicit maintenance release date:

```sh
./gradlew --configure-on-demand --no-daemon --no-build-cache \
  -PsaxonReleaseDate=2026-07-07 :java:he:editionMavenJar
```

Select the actual release date for the artifact you maintain; the date above is
an example input, not an assertion that this fork was published on that date.
`saxonReleaseDate` must be a real calendar date in exactly `YYYY-MM-DD` format.
Empty, malformed, or impossible dates fail preprocessing with a diagnostic.
When omitted, the default remains the build JVM's current local date. The date
is a declared preprocessing input, so changing it also regenerates `Version.java`
in an incremental build. The major release date and engine version still come
from `gradle.properties`; the generated build number still identifies the source
Git commit. No engine version or source date is relabeled.

HE archives use Gradle's stable archive timestamps and sorted entry order.
The existing filtering, preprocessing, manifest, resources, and notices still
run normally. There is no post-build JAR normalization. Reproduction requires
the same Git revision (including build logic), release inputs, dependency bytes,
JDK 21 distribution, Gradle wrapper, and build platform. The shared configuration
also requires a genuine .NET 8 SDK on `PATH`; it does not run .NET compilation
for this Java task. The HE runtime bytecode target remains Java 8 and can run
on Java 17.

The wrapper checks the Gradle 9.4.1 binary distribution against its published
SHA-256. Dependency versions are selected by the existing build scripts and
catalog. They are **not** a complete dependency lock or verification policy:
the Java conventions also consult Maven Local, Maven Central, and Saxonica;
build logic consults Maven Central, the plugin portal, and JitPack. A later
immutable acquisition must preserve and verify the entire resolved closure,
including plugin/buildSrc artifacts and metadata, and isolate Maven Local.
The probe below records coordinate/cache paths and SHA-256 values for the
resolved closure in each `dependencies.json`. These inventories record the
bytes fetched; they do not independently authenticate their publishers or
prevent repositories from replacing them. Gradle's existing
`--write-verification-metadata sha256` mechanism can bootstrap an audited
verification policy, but generated checksums alone are not such an audit.

Run the focused build tests from the repository root with JDK 21 and .NET 8:

```sh
python3 java/he/ci/release-date.py
python3 java/he/ci/reproducible.py /absolute/new/scratch/directory --targeted-tests
```

The reproducibility probe uses two untouched `git archive` exports of `HEAD`,
separate OS/JVM homes and Gradle caches, no build cache, different source mtimes,
and time zones 26 hours apart. Git identity is read from the original repository
through Git's environment variables; the exports are not additional checkouts.
It compares the raw normal JAR bytes, records entry-content hashes, and checks
the generated release-date contract. Commit changes before running it and keep
`HEAD` unchanged until it finishes. The optional targeted tests keep
the five owner test classes and their existing 15 test cases unchanged.
`release-date.py` executes preprocessing to check a leap date, incremental date
changes, the current-date default, and invalid values. Neither test rewrites
generated source or assembled JAR entries. CI's normal HE verification remains
`./gradlew --console=plain --configure-on-demand :java:he:editionTest` and
`java/he/ci/smoke.sh java/he/build/he-runtime` after `:java:he:stageHeRuntime`.
