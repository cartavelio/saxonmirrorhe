<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
                xmlns:arch="http://expath.org/ns/archive"
                xmlns:file="http://expath.org/ns/file"
                xmlns:xs="http://www.w3.org/2001/XMLSchema"
                version="2.0" exclude-result-prefixes="xs file">


   <xsl:output indent="yes"/>

   <xsl:param name="zip" select="file:read-binary(resolve-uri('sample.zip'))" />

   <xsl:template name="main" match="/">
       <arch:entries>
          <arch:list>
             <xsl:copy-of select="arch:entries($zip)"/>
          </arch:list>
       </arch:entries>
   </xsl:template>

</xsl:stylesheet>