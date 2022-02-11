<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
   xmlns:xs="http://www.w3.org/2001/XMLSchema"
   exclude-result-prefixes="xs"
   version="2.0">
   
   <xsl:output method="text"/>
   
   <xsl:variable name="genDate"
      select="'Resources download build date:',format-dateTime(current-dateTime(),'[D01] [MNn] [Y]')"/>
   
   <xsl:variable name="readText">Saxon Resources Download
------------------------

<xsl:value-of select="$genDate"/>

This saxon-resources download contains Saxon documentation, and sample applications and drivers. 

To access the documentation, open doc/index.html in a browser and follow the instructions and links

See below for details of the download contents.


Directories
-----------

1. doc
------
Contains the Saxon API documentation. Open index.html in a browser as a starting point.

2. drivers
----------
Drivers for the W3C test suites.

3. notices
----------
Notices for components redistributed with Saxon. 

4. samples
----------
Sample applications relevant for all Saxon editions (Saxon-HE, Saxon-PE and Saxon-EE), and both
platforms (Java and .NET). See the "Sample Saxon Applications" section of the Saxon documentation
for details.

5. use-cases
------------
W3C XQuery Use Cases - queries (some of which have been corrected from those published by W3C),
together with batch scripts for running them.

6. source-userdoc
-----------------
Provided as a zip. Unzip to access the main Saxon documentation in directory viewer_appJS.

The Saxon documentation supplied in this resources download runs as an interactive application
driven by Saxon-JS in the browser (a copy of Saxon-JS is included in the download). However, most
browsers do not allow Saxon-JS to run from a local file system, so the documentation application may
not work directly. It is recommended that the HTTP protocol is used to access the resource files, so
it may be necessary to host this directory on a localhost web-server.


</xsl:variable>
   
   <xsl:variable name="readTextCS">Saxon Resources Download
------------------------

<xsl:value-of select="$genDate"/>

This saxon-resources download contains SaxonCS documentation, and sample applications. 

To access the documentation, open doc/index.html in a browser and follow the instructions and links

See below for details of the download contents.


Directories
-----------

1. doc
------
Contains the Saxon API documentation. Open index.html in a browser as a starting point.
      
2. notices
----------
Notices for components redistributed with Saxon.

3. samples
----------
Sample applications relevant for SaxonCS on .NET. See the "Sample Saxon Applications" section of
the Saxon documentation for details.

4. source-userdoc
-----------------
Provided as a zip. Unzip to access the main Saxon documentation in directory viewer_appJS.
      
The Saxon documentation supplied in this resources download runs as an interactive application
driven by Saxon-JS in the browser (a copy of Saxon-JS is included in the download). However, most
browsers do not allow Saxon-JS to run from a local file system, so the documentation application may
not work directly. It is recommended that the HTTP protocol is used to access the resource files, so
it may be necessary to host this directory on a localhost web-server.


</xsl:variable>
   
   <xsl:template name="main">
      <xsl:sequence select="$readText"/>
   </xsl:template>
   
</xsl:stylesheet>