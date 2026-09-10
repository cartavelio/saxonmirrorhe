# SaxonC-@EDITION@ @version@ source download:

SaxonC-@EDITION@ is a compiled variant of Saxon from the Java
platform to the C/C++ platform using GraalVM native-image.

SaxonC-@EDITION@ provides processing in XSLT 3.0, XQuery 3.1 and
XPath 3.1; and Schema validation 1.0/1.1 with SaxonC-EE.
SaxonC-EE and SaxonC-PE also include support for many features in
the XPath, XQuery, and XSLT 4.0 specifications currently in
development in the QT4 Community Group.

This download contains the source files used to build SaxonC-@EDITION@, along
with the source for the Python package.

PHP sources are included as part of the main SaxonC download, available from the
[SaxonC download page][]

[SaxonC download page]: https://www.saxonica.com/download/c.xml

## What’s included

The source for the Python package, which is written with Cython 3, is included,
along with python-specific C++ code, under the `python/` directory.

The additional Java files which are used with the rest of SaxonJ and GraalVM to
build the `libsaxonc-core` are included under `src/main/java.`.
The C/C++ source files used to build `libsaxonc` itself are included under
`src/main/c/`.

The MPL 2.0 license this code is licensed under is included under `notices/`

## Other resources

SaxonC-specific samples can be found at the [saxonc-resources][] GitHub
repository.

[saxonc-resources]: https://github.com/Saxonica/saxonc-resources

Full documentation for SaxonC 13 is available. The [SaxonC 13 documentation][]
contains information specific to SaxonC - including instructions for installing
and configuring the product, details of the SaxonC C/C++ API, PHP API and
Python3 API, and sample code. Meanwhile, the main [Saxon 13 documentation][]
contains further information relevant to SaxonC.

[SaxonC 13 documentation]: https://www.saxonica.com/saxon-c/documentation13/index.html

[Saxon 13 documentation]: https://www.saxonica.com/documentation13/index.html

For further information, see the [SaxonC web page][]. For bug reporting,
lists of known bugs, and help forums, please visit
[https://saxonica.plan.io/](https://saxonica.plan.io/).

[SaxonC web page]: https://www.saxonica.com/saxon-c/index.xml