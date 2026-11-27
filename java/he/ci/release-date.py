#!/usr/bin/env python3
"""Exercise the release-date task input, incremental updates and invalid inputs."""
import datetime
import os
from pathlib import Path
import subprocess
import tempfile


repo = Path(__file__).resolve().parents[3]
generated = repo / "java/he/build/src/main/java/net/sf/saxon/Version.java"
command = [str(repo / "gradlew"), "--configure-on-demand", "--no-daemon",
           "--max-workers=2", "--console=plain", ":java:he:preprocessJava"]
env = dict(os.environ, TZ="UTC", JAVA_TOOL_OPTIONS=os.environ.get("JAVA_TOOL_OPTIONS", "") + " -Duser.timezone=UTC")
for value in ["2000-02-29", "2001-03-01", None, "2025-02-29", "2026-7-07", "", "nonsense"]:
    valid = value in ["2000-02-29", "2001-03-01", None]
    before = datetime.datetime.now(datetime.timezone.utc).date().isoformat()
    with tempfile.TemporaryFile(mode="w+") as log:
        result = subprocess.run(command + ([] if value is None else [f"-PsaxonReleaseDate={value}"]),
                                cwd=repo, env=env, stdout=log, stderr=subprocess.STDOUT, timeout=300)
        log.seek(0)
        output = log.read()
    if valid:
        assert result.returncode == 0, output
        after = datetime.datetime.now(datetime.timezone.utc).date().isoformat()
        expected = [before, after] if value is None else [value]
        assert any(f'private static final String MINOR_RELEASE_DATE = "{date}";' in generated.read_text()
                   for date in expected), f"Wrong generated release date for {value!r}"
    else:
        assert result.returncode != 0, f"Invalid date accepted: {value!r}"
        assert "saxonReleaseDate must be a valid date in YYYY-MM-DD format" in output, output
    print(f"PASS release date {value!r}", flush=True)
