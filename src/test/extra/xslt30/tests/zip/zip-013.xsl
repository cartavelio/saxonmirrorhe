<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
                xmlns:arch="http://expath.org/ns/archive"
                xmlns:file="http://expath.org/ns/file"
                xmlns:saxon="http://saxon.sf.net/"
                xmlns:xs="http://www.w3.org/2001/XMLSchema"
                version="3.0" exclude-result-prefixes="arch file saxon xs">


   <xsl:output indent="yes"/>

   <xsl:param name="zip" select="file:read-binary(resolve-uri('sample.zip'))" />
   <xsl:param name="entry" select="'a', 'b', current-date()"/>

   <xsl:template name="main" match="/">
       <entries>
          <html>
             <xsl:copy-of select="saxon:parse-html(arch:extract-text($zip, $entry))"/>
          </html>
       </entries>
   </xsl:template>

</xsl:stylesheet>