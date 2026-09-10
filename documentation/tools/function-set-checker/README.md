# Utility Tool: Function Set Checker   

__Status:__ works for Sheila (2026-04-23)

## Table of contents

1. [Summary](#summary)
1. [Usage Guide](#usage)
   1. [Running from the command-line](#how-to-run)
   1. [Default settings](#defaults)
1. [Inputs](#inputs)
1. [Outputs](#outputs)
1. [Etymology](#etymology)


## Summary <a id="summary"></a>

An XProc pipeline that searches for Function Set classes in the Java source and compares the list of functions actually registered there against a catalog of expected functions.


## Usage Guide <a id="usage"></a>

The workflow is generally:

1. Pull the latest code from whichever branch of `Saxondev` you intend to check, eg. `saxon12` or `main`
1. Make a note of the time...
1. Run `RUN_ME.xpl` (_see [Running from the command-line](#how-to-run), below_), which will:
	1. Look for files named `*FunctionSet*.java` in the `$source-dir` 
    1. For each function set file found:
        1. Create an XML file, listing all the functions registered in that function set
        1. Store the function set XML in the `$output-dir`
        1. Compare the list of functions in the XML file against a catalog of the functions that are expected to be registered
        1. Create a markdown file listing:
            1. the functions registered that are in the catalog
            1. the functions registered that are __not__ in the catalog
        1. Store the markdown file in `$outputdir/md/`        
    1. Output a directory listing of the function sets found
1. Update the wiki with the markdown output; there's a page per function set.  		


### Runing from the command-line <a id="how-to-run"></a>

#### Using XML Calabash
```powershell
java -jar /path/to/calabash.jar src/xproc/RUN_ME.xpl source-dir=path/to/saxondev/java/src/main/java/ pulled=2026-04-22T13:24:00+01:00
```

To download the latest version of XML Calabash, see [https://codeberg.org/xmlcalabash/xmlcalabash3/releases](https://codeberg.org/xmlcalabash/xmlcalabash3/releases)

For documentation on setting it up, see [https://docs.xmlcalabash.com/userguide/current/part-install.html](https://docs.xmlcalabash.com/userguide/current/part-install.html)


#### Using Morgana

_Not yet tested with Morgana_

### Default settings <a id="defaults"></a>

By default, this pipeline will:

* use `java/src/main/java/` in this repository as the path for the `source-dir` option.  
* use `documentation/tools/function-set-checker/data/tmp/` as the path for the `output-dir` option.

## Inputs <a id="inputs"></a>

### Pulled

_Via the `pulled` option._

An `xs:dateTimeStamp` for when the latest was last pulled from the remote `saxondev` repository, eg. `2026-04-23T11:00:00+01:00`

### Source directory

_Via the `source-dir` option._

Expected to be a path to the `java/src/main/java` directory in the  `saxondev` repository.

_See [Default settings](#defaults), above_


### Output directory

_Via the `output-dir` option._

A path to the directory in which this pipeline will store all its outputs.

_See [Default settings](#defaults), above_

### Data dependencies

#### Function set metadata

**Location**: `data/function_set_metadata.xml`

Provides important information about each function set:
* which catalog it is associated with
* the prefix to use for functions in that function set
* a display name (for the wiki)
* an ID
* its expected location

#### Simplified function catalogs

**Location**: `data/catalogs/`

Each of simplified function catalogs is supplosed to list all the functions in the latest version of it's associated specification.


##### Updating a function catalog

A function catalog should only need to be updated if its associated specification changes.
 
If the source _isn't_ maintained by the QT* CG, you will need to update the simplfied catalog manually.

If the source _is_ maintained by the QT* CG:

1. Pull the latest from the `qtspecs` repository.
1. Transform the QT4 `function-catalog.xml` using `src/xslt/simplify_function_catalog.xsl`
1. Compare the result against the current simplified version of the same catalog.  If it looks good, replace the version in `data/catalogs/` with the newly generated file

_The naming convention used in `data/catalogs` is significant so if you change it, you will also need to change `check_implemented.xsl`._

## Outputs <a id="outputs"></a>

### Directory Listing

The directory listing that lists all the `*FunctionSet*.java` files found in `$source-dir`, but the `c:file` for each Java function wraps the XML version of the same function set. 

### Function set XML

A list of the functions registered in a Java function set file.

### Wiki-ready markdown

Two lists:
* functions registered that are in the associated catalog (expected)
* functions registered that aren't in the associated catalog (unexpected)

## Etymology <a id="etymology"></a>

This tool was originally developed to assist with checking whether any QT4 functions were registered but no longer in the specification.  This involved comparing function sets against QT4 function catalogs.   