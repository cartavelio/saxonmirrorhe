<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
                xmlns:arch="http://expath.org/ns/archive"
                xmlns:file="http://expath.org/ns/file"
                xmlns:xs="http://www.w3.org/2001/XMLSchema"
                version="3.0" exclude-result-prefixes="file arch xs">


   <xsl:output indent="yes"/>

   <xsl:param name="zip" select="file:read-binary(resolve-uri('sample.zip'))" />

   <xsl:template name="main" match="/">
       <entries>
         <some-xml>
             <xsl:copy-of select="parse-xml(arch:extract-text($zip, 'hello.xml'))"/>
          </some-xml>
       </entries>
   </xsl:template>

</xsl:stylesheet>