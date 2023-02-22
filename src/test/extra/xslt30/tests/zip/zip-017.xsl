<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
                xmlns:arch="http://expath.org/ns/archive"
                xmlns:file="http://expath.org/ns/file"
                xmlns:xs="http://www.w3.org/2001/XMLSchema"
                version="3.0" exclude-result-prefixes="arch file xs">


   <xsl:output indent="yes"/>

   <xsl:param name="zip" select="file:read-binary(resolve-uri('sample.zip'))" />

   <xsl:template name="main" match="/">
       <entries>
          <text>
             <xsl:copy-of select="arch:extract-text((), 'file.txt')"/>
          </text>
       </entries>
   </xsl:template>

</xsl:stylesheet>