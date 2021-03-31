<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
                xmlns:arch="http://expath.org/ns/archive"
                xmlns:file="http://expath.org/ns/file"
                xmlns:xs="http://www.w3.org/2001/XMLSchema"
                version="2.0" exclude-result-prefixes="file xs">


   <xsl:output indent="yes"/>

   <xsl:param name="zip" select="file:read-binary(resolve-uri('sample.zip'))" />

   <xsl:template name="main" match="/">
       <arch:entries>
          <arch:text>
             <xsl:copy-of select="arch:extract-text($zip, 'file111.txt')" />
          </arch:text>
       </arch:entries>
   </xsl:template>

</xsl:stylesheet>