JParser
=======

The files in this directory are work in progress to do the following:

* Convert the Java source code (of Saxon) to an XML representation of the Java syntax using the open source JavaParser

* Convert the resulting XML to Java or C#

The conversion to Java is purely for testing, to check that it round trips; but it could potentially be used
 to modify the code in various ways, e.g. adding instrumentation or tracing, or for various kinds of preprocessing
 or annotation processing.
 
The conversion to C# is intended to explore the possibility of creating a .NET product by source code conversion.
At present the conversion is mechanistic, but the idea is to add rules that convert calls on Java classes to their
C# equivalents.

The simple Java program toXml.java converts one Java module to XML. The stylesheets xml-to-java.xsl and 
xml-to-csharp.xsl also appear to process a single module. At some stage there must have been some script to convert
the entire code base, but I can't find it.

The file javaparservisited.pdf is a paid-for manual for the javaparser product (the code is open source, the manual is
available for purchase.)

MHK 2021-01-20 