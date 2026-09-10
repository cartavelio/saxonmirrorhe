# Building SaxonC wheels for HE, PE, and EE

This file is about the SaxonC builds, especially local wheel builds.

## Prerequisites

* Most of the instructions in BUILD.md
* GraalVM + native-image
* pyenv, https://github.com/pyenv/pyenv (macOS, Linux)
* pyenv-win, https://github.com/pyenv-win/pyenv-win (Windows)

## Setup

Gradle tasks make direct use of `pyenv` and `pyenv-win`, so you shouldn't 
need to do anything. The build scripts will error if pyenv cannot be found.

Primary builds use the `pyenv` task defined in `build-graal.gradle`. That 
task defines the Python version to use.

### Managing virtual environments

The gradle tasks create a virtual env named after the current branch, and keep it in `build/pyvenv`.

To activate the virtual env so your shell Python is the right one, using the right virtual env, run:

```shell
$ source build/pyvenv/{branch}/bin/activate
```

Or, in Powershell on Windows:

```pwsh
C:\> build\pyvenv\{branch}\Scripts\Activate.ps1
```

Once activated, the virtual env adds a `deactivate` binary to your `$PATH` so you can simply run `deactivate` to get out of the virtual env and return your Python to system normal.

## Gradle targets

* `EDITIONc_python_build_wheel` Builds a local wheel
* `EDITIONc_python_repair_wheel` Builds the local wheel and ensures the `libsaxon` binary is correctly bundled within it.
* `EDITIONc_python_install_wheel` Builds and repairs the local wheel, and then installs it into the virtualenv for the current branch, uninstalling any existing version.

For example: 
```shell
$ ./gradlew hec_python_install_wheel
```
