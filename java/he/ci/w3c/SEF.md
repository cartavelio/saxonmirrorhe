# SaxonJ-HE SEF qualification

See the [public API and command-line examples](../../../../README.md#save-and-load-a-compiled-stylesheet).

## Writer and loader inventory

- **Package envelope and identities.** `StylesheetPackage.export` emits package metadata, the used-package graph, component identities, overridden components, output properties, and the package's default mode. `PackageLoaderHE.loadPackageElement` reconstructs the package graph before resolving bindings. `Component.export` and `readComponents` share the component representation for modes, templates, attribute sets, global values, and user functions.
- **Stylesheet declarations.** The key, accumulator, space-stripping, character-map, decimal-symbol, and global-context writers share their existing representations with the corresponding `PackageLoaderHE.read*` methods. The loader also restores the association between a mode and its accumulators. Tests check whitespace counts, key lookup, named output, primary and secondary serialization, context requirements, and lazy global evaluation.
- **Expressions and patterns.** Expression classes emit their operands and attributes through `ExpressionPresenter`. `PackageLoaderHE` dispatches expression tags through `eMap` and pattern tags through `pMap`. The XSLT catalog exercises template dispatch, patterns, loops, constructors, comparisons, casting, grouping, sorting, and error handling.
- **Function values.** The loader handles system and vendor references, user function bindings, inline bodies, transformation-bound inline references, and partial application. Saxon 12 represents inline functions as user-function components. The loader reuses that representation for closures and functions nested in maps and arrays. Tests check nested captures, calls from Java, values stored in maps and arrays, and named references with retained static context.
- **Context and deployment.** `ExpressionPresenter` and `PackageLoaderHE.makeRetainedStaticContext` preserve the static context that expressions need. The export and load paths distinguish original module URIs from relocatable SEF URIs. The CLI smoke test moves the source files away before running both forms in fresh Java processes.
- **File and stream behavior.** Checksums and format versions are checked on load. Missing component IDs, invalid binding slots, and malformed binding references fail before execution. Concurrent tests export the same executable to independent streams. A failed stream write reports an exception, and package save stages a completed file before replacing its destination.

## Conformance checks

The W3C job retains the existing source XSLT and QT3 comparisons. It runs the same XSLT catalog again and exports compiled executables through s9api. Each executable reloads in a fresh HE processor. The driver shares the catalog's resource configuration and node identity infrastructure so that it can supply the same test inputs to the loaded executable. Runtime library packages supplied by the catalog are also exported and reloaded.

The comparison considers cases that passed from source. Successful round trips satisfy the same W3C result or error assertions after reload. The focused HE tests additionally compare exact serialized results, including `[ADA]`, and error codes for selected cases. Source failures, unavailable catalog features, and cases that pass without producing an executable, such as static error tests, are counted as exclusions.

Of 14,601 catalog cases, all 5,984 eligible cases passed after export and reload. The source run did not pass 6,414 cases, and 2,203 source-passing cases produced no executable.

The `transform-004` case covers a user function returned by static `fn:transform()`. The SEF embeds the separately compiled stylesheet and identifies the function by its component ID in that package. The focused API regression also calls the function after reload and re-export with the source modules removed. It verifies that the function retains its supplied stylesheet parameters and global context item.

`XT30-SEF.tsv` records any expected divergences and currently has no entries. CI requires every eligible case to match its expected status and reports additions, status changes, and newly passing cases for review.

## Compatibility

The HE writer targets XSLT 3.0. Schema-aware exports and XSLT 4.0 compilation require support outside this HE build. Fork-generated Saxon 12 SEFs use format version `12.1`. Use the same Saxon major version for export and reload. Older Saxon 12 SEFs without a fork format marker may load when their checksums and constructs are supported.

The built-in HE registries supply the supported Saxon and EXSLT Common references. EXPath File and Binary references have operation-specific load errors in this build. Calling `fn:load-xquery-module` reports `FOQM0006` when the XQuery module compiler is unavailable.

## Saxon 13 reference timings

For the original Saxon 13 port, on JDK 21 and an Intel Core i5-1235U, 75 in-process samples per stylesheet followed 30 warmup iterations. Each sample used fresh HE processors, timed compilation from the stylesheet file, exported to memory, and loaded the resulting bytes into the second processor. Processor setup, copying the input bytes, execution, and JVM startup were outside the timings. Source and reloaded execution produced the same serialization for both fixtures.

| Stylesheet | SEF size | Median compile | Median export | Median reload and link |
| --- | ---: | ---: | ---: | ---: |
| `labels.xsl` | 3,178 bytes | 3.069 ms | 0.973 ms | 1.936 ms |
| `books-to-json.xsl` | 4,885 bytes | 1.764 ms | 0.549 ms | 1.109 ms |
