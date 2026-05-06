# Building SaxonC

## Dependencies

* GraalVM Community Edition (https://github.com/graalvm/graalvm-ce-builds/releases/)
  * After installation, create an environment variable named GRAALVM_HOME and set its value to be a path to the root directory of your GraalVM installation
* cmake >= v.3.1 (https://cmake.org/)
* Ninja (https://ninja-build.org/)
* libz (https://docs.oracle.com/cd/E88353_01/html/E37842/libz-3.html) - possibly only needed on Linux
* pyenv (https://github.com/pyenv/pyenv)
    * On Linux, will also need to install optional modules: https://devguide.python.org/getting-started/setup-building/#install-dependencies
    
## Running the build task

### HE
```
./gradlew libsaxonc:he:release
```

