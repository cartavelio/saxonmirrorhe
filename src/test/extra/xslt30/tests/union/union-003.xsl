<?xml version="1.0" encoding="utf-8"?> 

<xsl:stylesheet
   xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="4.0"
   xmlns:xs="http://www.w3.org/2001/XMLSchema"
   xmlns:saxon="http://saxon.sf.net/"
   xmlns:cx="http://saxon.sf.net/complex"
   exclude-result-prefixes="xs saxon cx"
   expand-text="yes"
>

  <!-- instance of union -->

  
  <xsl:template name="xsl:initial-template">
    <out>
      <xsl:variable name="x" select="'ABCD' cast as union(xs:hexBinary, xs:base64Binary)"/>
      <a>{(xs:hexBinary('AAAA'), xs:base64Binary(xs:hexBinary('AAAA'))) instance of union(xs:hexBinary, xs:base64Binary)+}</a>
    </out>
  </xsl:template>  

</xsl:stylesheet>
