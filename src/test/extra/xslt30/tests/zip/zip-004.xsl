<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
                xmlns:arch="http://expath.org/ns/archive"
                xmlns:xs="http://www.w3.org/2001/XMLSchema"
                version="2.0" exclude-result-prefixes="xs">


   <xsl:output indent="yes"/>

   <xsl:param name="zip" />

   <xsl:template name="main" match="/">
       <entries>
          <list>
             <xsl:copy-of select="arch:entries($zip)"/>
          </list>
       </entries>
   </xsl:template>

</xsl:stylesheet>