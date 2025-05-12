Unicode
=======

This directory contains code designed to extract stuff from the Unicode database published by the
Unicode consortium.

alphanumeric-ranges.xsl extracts a list of consecutive blocks of characters classed as letters 
or digits.

blocks-to-json.xsl converts the files unicodeBlocks.xml and categories.xml from XML to JSON format.
These files are included in the Saxon/Java distribution, and are themselves generated from the
Unicode database.

UnicodeDataGenerator.java was included in the Saxon product until Saxon 10, but not actually used
in production. It's a utility (originally based on Unicode Consortium code) to extract data tables
used to drive Unicode normalization. The resulting data tables were also included in the product.
This mechanism is dropped for Saxon 11, which instead invokes the Unicode Normalization code provided
in the JSK from Java 7 onwards.

UnicodeDataParserFromXML.java was also part of the product until Saxon 10; it was used to load the XML
files created by the UnicodeDataGenerator into Java hash tables for use at run-time by the (now dropped)
Unicode normalization routines.


MHK 2021-01-20 