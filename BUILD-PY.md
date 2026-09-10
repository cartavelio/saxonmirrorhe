# Building SaxonC wheels for HE, PE, and EE

This file is about the SaxonC builds, especially the wheel builds.

## Prerequisites

* Most of the instructions in BUILD.md
* GraalVM
* pyenv, https://github.com/pyenv/pyenv
* pyenv-virtualenv, the virtual environment plugin for pyenv

## Setup

Virtual environments work by modifying your shell. To make them work
in the Gradle build, you have to configure the shell parts before you start.

The build scripts assume that virtual environments with particular names will exist.
The names are “saxon-{branch}”, for example `saxon-main` and `saxon-saxon12`.

### Managing virtual environments

To create a virtual environment, run a command like this:

```
$ pyenv virtualenv 3.9.16 saxon-saxon12
```

That command creates an environment named `saxon-saxon12` based of
Python version 3.9.16.

The command `pyenv versions` will tell you what versions you have installed.

The command `pyenv uninstall name` will uninstall the virtual
environment named `name`.

Suppose, for example, that you want to try Saxon 12 with Python 3.10.

```
$ pyenv versions
  system
* 3.9.16 (set by /Users/ndw/.pyenv/version)
```

I don’t currently have Python 3.10 installed, so:

```
$ pyenv install 3.10
```

Then remove the current 3.9.16 version:

```
$ pyenv uninstall saxon-saxon12
```

Then install the new one and activate it:

```
$ pyenv virtualenv 3.10 saxon-saxon12
  …
$ pyenv activate saxon-saxon12
```

You should now be running 3.10 with the correct virtual environment:

```
$ python3 —version
Python 3.10.12
$ pyenv version
saxon-saxon12 (set by PYENV_VERSION environment variable)
```

You can use `pyenv deactivate` and `pyenv activate` to switch between
existing environments.

## Gradle targets

* `pyenvEnvironment` tests that you (a) have a virtual environment and
  (b) that it’s the correct environment for the branch you have checked out.
* `pyenv_install_saxon_EDITIONc` installs Saxon C EDITION. For example,
  `penv_install_saxon_eec` installs Saxon EE in the current virtual environment.
