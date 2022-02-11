<?xml version="1.0" encoding="utf-8"?> 

<xsl:stylesheet
   xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="4.0"
   xmlns:xs="http://www.w3.org/2001/XMLSchema"
   xmlns:saxon="http://saxon.sf.net/"
   xmlns:cx="http://saxon.sf.net/complex"
   exclude-result-prefixes="xs saxon cx"
   expand-text="yes"
>

  <xsl:function name="cx:binEquals" as="xs:boolean">
    <xsl:param name="a" as="union(xs:hexBinary, xs:base64Binary)"/>
    <xsl:param name="b" as="union(xs:hexBinary, xs:base64Binary)"/>
    <xsl:sequence select="xs:hexBinary($a) eq xs:hexBinary($b)"/>
  </xsl:function>
  
  <xsl:template name="xsl:initial-template">
    <out>
      <a>{cx:binEquals(xs:hexBinary(''), xs:base64Binary(''))}</a>
    </out>
  </xsl:template>  

</xsl:stylesheet>
