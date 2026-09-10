# CMake and CLion

To make use of the CLion debugger and cmake/ctest integration, you need to ensure
that you do three things:

1. Open the `ee`, `he`, or `pe` directory in CLion, rather than this folder
   (`libsaxonc`), or the main `saxondev` folder.
2. Set the build directory to `build` in the correct place in the settings:
   Settings -> Build, Execution, Deployment -> CMake -> build directory.
3. Run the `libsaxonc:{edition}:cmake_generate` gradle task for the edition you
   want to work with, i.e. `./gradlew libsaxonc:ee:cmake_generate`.

You should only need to run the `cmake_generate` gradle task once, and it doesn't
really matter if you run it before or after the other steps above: CMake will
complain until that process has created a CMakeCache.txt with the values it
needs to find  `libsaxonc-core-{edition}`.

## Optionally

You can also set the CMake generator in Settings -> Build, Execution,
Deployment -> CMake to either _‘Let CMake decide’_ or _‘Ninja Multi-Config’_.

## Tests run through `ctest`

`ctest` tests are configured to expect a `build/ctest-working-dir` folder, but it
won't (currently) create that. run `./gradlew libsaxon:{edition}:ctest_working_dir`
to create and (if needed) clean it out, i.e. `./gradlew libsaxonc:he:ctest_working_dir.`