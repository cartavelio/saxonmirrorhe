# Datatypes Table


## What purpose does it serve?

Type categories are defined (and modified or clarified) all over the place in the (multiple) spec documents.  It's a challenge to keep it straight in your head and it's handy to be able to look it up in one place.


## Why not just author it as a table?

That was iteration one.  It was surpisingly difficult to maintain, in particular checking membership of a type category, derived-from and sub-types.
It also took up a lot of space in the document it's now XIncluded into, making it awkward to work around.

Authoring it as hierarchical XML means that:
* the following can be automatically generated (less hassle, greater consistency, less room for error):
	* derived-from
	* sub-types
	* anchors for links (`td/@id`)
	* links to cross-referenced datatypes
	* style-related markup, including (but not limited to) the `qt4` and `aka` class names
	* boilerplate text:
		* "Introduced in \{spec name and version\}." - _depends on `datatype/@first-introduced`_
		* "An alternative name for \{link to other datatype\}" - _depends on `datatype/@ref`_
* an RNC schema can be used to check for structural errors, such as a missing but required markup (automatically done if you open datatypes.xml in oxygen, thanks to xml-model).
* Schematron can be used to check for:
	* missing or unexpected datatypes 
	* membership of type categories (expected and unexpected)
	* missing required data, such as:
		* a comparison family for non-abstract datatypes  
	* unexpected data, such as:
		* a comparison family for abstract datatypes
* the membership for each type category can be maintained as a list in a variable in Schematron

## Validation

`datatypes.xml` is expected to be valid to all of the following schemas:
* `datatypes.rnc`
* `datatypes.sch`

If you open `datatypes.xml` in Oxygen, it should automatically be validated against both schemas, thanks to the `<?xml-model … ?>` processing instruction, which has been used to associate each of them with the XML file.

## Conversion to a table

`xml/datatypes.xml` is converted to a table by `xsl/xml2html.xsl`

## How is the table included in the generated userdocs?

## Gradle tasks

Running `./gradlew :documentation:gen_datatypes_table` will:
* apply `datatypes.xsl` to `datatypes.xml`; and
* save the result to `/saxondev/documentation/build/src/userdoc/datatypes/datatypes_table.xml`

Generation of the table is included in:
* `./gradlew gen_doc` - _an internal task to build the documentation XML_
