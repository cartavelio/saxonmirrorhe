These files are the "gold master" manifests for the Java jars used as input to Graal VM

They're created by running

```
  jar -tf saxon-{EDITION}-VERSION.jar | grep -v "\\$" | sort
```

on the `{edition}/build/libs/saxon-{edition}c-{version}.jar` created
during the release process

* Removing lines with $-signs says we don't care about the internal
  class structure
* Sorting means we don't care what order the files are added to the jar

The idea is that any deviation from these lists gets flagged in the
build. New files are cause for a warning; missing files will fail the
build.

This is to prevent us from building and releasing bad Graal libraries.
