#!/usr/bin/env python3
"""Build two untouched Git source exports and compare the normal HE artifacts."""
import argparse
import hashlib
import json
import os
from pathlib import Path
import subprocess
import tarfile
import zipfile


TARGETED = [
    "UnionPatternSlotAllocationTest", "GeneralNodePatternSlotAllocationTest",
    "SplitUnionRegistrationTest", "ApplyImportsPatternBindingTest", "NamespaceMapTest",
]


def digest(data):
    return hashlib.sha256(data).hexdigest()


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("output", type=Path, help="new directory outside the repository")
    parser.add_argument("--release-date", default="2026-07-07")
    parser.add_argument("--targeted-tests", action="store_true")
    args = parser.parse_args()
    repo = Path(__file__).resolve().parents[3]
    output = args.output.resolve()
    if output.is_relative_to(repo):
        parser.error("output must be outside the repository")
    output.mkdir(parents=True, exist_ok=False)
    revision = subprocess.check_output(["git", "-C", str(repo), "rev-parse", "HEAD"], text=True).strip()
    git_dir = subprocess.check_output(["git", "-C", str(repo), "rev-parse", "--absolute-git-dir"], text=True).strip()
    archive = output / "source.tar"
    subprocess.run(["git", "-C", str(repo), "archive", revision, "-o", str(archive)], check=True)
    records = []
    entry_contents = []
    entry_timestamps = []
    for index, (mtime, timezone) in enumerate([(946684800, "Pacific/Kiritimati"), (1735689600, "Etc/GMT+12")], 1):
        root = output / str(index)
        source = root / "source"
        source.mkdir(parents=True)
        with tarfile.open(archive) as tar:
            tar.extractall(source, filter="data")
        for path in source.rglob("*"):
            if not path.is_symlink():
                os.utime(path, (mtime, mtime))
        home = root / "home"
        home.mkdir()
        env = dict(os.environ, HOME=str(home), GRADLE_USER_HOME=str(root / "gradle"),
                   DOTNET_CLI_HOME=str(root / "dotnet-home"), TZ=timezone,
                   GIT_DIR=git_dir, GIT_WORK_TREE=str(source),
                   JAVA_TOOL_OPTIONS=f"-Duser.timezone={timezone} -Duser.home={home}")
        local_date = subprocess.check_output(["date", "+%F"], env=env, text=True).strip()
        command = [str(source / "gradlew"), "--configure-on-demand", "--no-daemon",
                   "--no-build-cache", "--max-workers=2", "--console=plain",
                   f"-PsaxonReleaseDate={args.release_date}", ":java:he:editionMavenJar"]
        if args.targeted_tests and index == 1:
            command += [":java:he:editionTest"]
            for test in TARGETED:
                command += ["--tests", f"internaltests.{test}"]
        with (root / "build.log").open("w") as log:
            subprocess.run(command, cwd=source, env=env, stdout=log, stderr=subprocess.STDOUT, check=True, timeout=600)
        assert subprocess.check_output(["git", "-C", str(repo), "rev-parse", "HEAD"], text=True).strip() == revision, "HEAD moved during the probe"
        jars = list((source / "java/he/build").rglob("Saxon-HE-12.10.jar"))
        if len(jars) != 1:
            raise AssertionError(f"Expected one Maven HE JAR, found {jars}")
        jar = jars[0]
        with zipfile.ZipFile(jar) as z:
            contents = {entry.filename: digest(z.read(entry)) for entry in z.infolist()}
            timestamps = {entry.filename: entry.date_time for entry in z.infolist()}
        entry_contents.append(contents)
        entry_timestamps.append(timestamps)
        generated = source / "java/he/build/src/main/java/net/sf/saxon/Version.java"
        dependencies = {
            str(path.relative_to(root / "gradle/caches/modules-2/files-2.1")): digest(path.read_bytes())
            for path in sorted((root / "gradle/caches/modules-2/files-2.1").rglob("*")) if path.is_file()
        }
        (root / "dependencies.json").write_text(json.dumps(dependencies, indent=2) + "\n")
        # This file is the generated public version contract, not build implementation text.
        expected_date = f'private static final String MINOR_RELEASE_DATE = "{args.release_date}";'
        records.append(dict(jar=str(jar), sha256=digest(jar.read_bytes()), entries=len(contents),
                            content_sha256=digest(json.dumps(contents, sort_keys=True).encode()),
                            dependency_files=len(dependencies),
                            generated_version_sha256=digest(generated.read_bytes()),
                            declared_date_generated=expected_date in generated.read_text(),
                            timezone=timezone, build_local_date=local_date, source_mtime=mtime))
        (root / "entries.json").write_text(json.dumps(dict(contents=contents, timestamps=timestamps), indent=2) + "\n")
    names = set(entry_contents[0]) | set(entry_contents[1])
    receipt = dict(revision=revision, source_tar_sha256=digest(archive.read_bytes()),
                   release_date=args.release_date, builds=records,
                   entry_content_differences=sum(entry_contents[0].get(n) != entry_contents[1].get(n) for n in names),
                   entry_timestamp_differences=sum(entry_timestamps[0].get(n) != entry_timestamps[1].get(n) for n in names),
                   byte_equal=records[0]["sha256"] == records[1]["sha256"])
    (output / "receipt.json").write_text(json.dumps(receipt, indent=2) + "\n")
    print(json.dumps(receipt, indent=2))
    assert receipt["byte_equal"], "Normal HE JAR bytes differ between clean builds"
    assert all(r["declared_date_generated"] for r in records), "Release date did not govern generated Version.java"


if __name__ == "__main__":
    main()
