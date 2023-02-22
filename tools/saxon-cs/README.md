# SaxonCS

## What is SaxonCS?

SaxonCS is a library for XSLT, XQuery, XPath, and XSD processing.

It offers a high level of conformance with the latest W3C standards:
XSLT 3.0, XQuery 3.1, XPath 3.1, XSD 1.1.

It provides a C# API to allow transformations and queries to be controlled from a C# application,
and also offers a command line interface.

SaxonCS is a proprietary product created and distributed by Saxonica Ltd. You will need to purchase a license key to activate the product. To acquire a license key, 
use the [Saxonica online shop](https://www.saxonica.com/shop/index.html), or email
saxon-orders@saxonica.com.

## How does it relate to other Saxon products?

SaxonCS is built from the same source code as the long-established Java product. 
It was created by converting the source code of SaxonJ to C# source 
code, using a custom transpiler written by Saxonica
(primarily in XSLT 3.0, since you asked).

It is single-sourced with the Java product; the code has not been forked, and all maintenance 
and future development will be done on the Java base code.

The product includes some C# code for interfacing to the .NET platform,
and to provide a C# API following C# coding conventions.

SaxonCS, in its initial form, is based on the Enterprise Edition of Saxon, which means that it offers 
high-performance features exclusive to the EE version, such as advanced
optimization, schema-awareness, streaming, and multi-threading. 
Some platform-specific features such as bytecode generation are excluded.

License files are compatible between SaxonCS and SaxonJ.

Stylesheet export files (SEF files) are compatible between SaxonCS, SaxonC, and SaxonJ.

SaxonCS can also generate SEF files for SaxonJS.

## How does it relate to other XML libraries on .NET?

SaxonCS uses the `System.Xml.XmlParser` classes for parsing XML.

SaxonCS can work with a DOM tree delivered as a `System.Xml.XmlDocument`. 
You can either convert a DOM to Saxon's optimized internal tree structure, or
you can use Saxon's XPath or XQuery engine to query a DOM tree directly.

SaxonCS offers an alternative to the Microsoft XSLT 1.0 and XSD 1.0 processors bundled
with .NET Core, which have never been updated to the latest W3C specifications.
The newer standards offer a wide range of features that greatly extend the power
of the tools. 

It should be possible to run stylesheets developed for the 
Microsoft XSLT 1.0 processor under SaxonCS provided that they do
not take advantage of Microsoft-specific XSLT extensions. SaxonCS
offers "1.0 compatibility mode" to assist with the transition.

Since the functionality of the later standards is so much richer, Saxon does not
attempt to offer API compatibility with the Microsoft processors.

## What about the old Saxon/.NET product?

Up to and including Saxon 10, Saxonica delivered a product on .NET Framework built
from the Java product by converting the JAR files at bytecode level using
the IKVMC cross-compiler. This product relied on a converted version of the 
Java run-time library to function. The IKVMC technology was never ported
to .NET Core, so a new approach was needed, and Saxonica settled on source
code conversion as the most promising approach. A particular benefit is that
it now uses the .NET run-time library in place of a converted Java run-time 
library, which significantly reduces the footprint.

The API for SaxonCS is closely based on the API for the older product,
but it is not 100% compatible; some changes will be needed to existing 
applications. The changes have been designed to improve the conformance
to C# house-style and to take advantage of more modern features in the C# 
language, for example by using delegates rather than interfaces for callbacks.

The older Saxon/.NET product will remain supported for the time being,
but will not be further developed.

## What's new in this version?

SaxonCS is based on version 11 of the source code, and was released
ahead of the Java version. As usual there's a comprehensive list of changes
in the documentation. Apart from supporting .NET 5, you'll find there are
lots of new features to make processing JSON easier; it's getting close to
the point where JSON is supported almost as well as XML. Certainly, there's
no better library available for applications that need to mix and match XML
and JSON, or do custom conversions from one to the other.

For a record of bugs fixed in each maintenance release, please consult the
[issues list](https://saxonica.plan.io/projects/saxon/issues) — you can filter
the list on the property "Fixed in maintenance release". This is also the place
to report any problems you find.

## Where do I find documentation?

It's all [online](https://www.saxonica.com/documentation12/). 

(Delivered, in case you're interested, by SaxonJS running in your browser.)

## What if I hit problems?

If you have coding problems with XSLT, XPath, XQuery, or XSD, you will usually be
able to get help by asking on StackOverflow.

If you suspect a Saxon bug, please report it on the
[issues list](https://saxonica.plan.io/projects/saxon/issues).

## Will SaxonCS be integrated in Biztalk?

Microsoft currently offer XSLT 3.0 capability in Biztalk based on an integration
of Saxon-HE. We don't know if they have any plans for updating to SaxonCS; we'll
let you know when we hear.



