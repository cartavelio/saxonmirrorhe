<?xml version="1.0" encoding="utf-8"?> 

<xsl:stylesheet
   xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="4.0"
   xmlns:xs="http://www.w3.org/2001/XMLSchema"
   xmlns:saxon="http://saxon.sf.net/"
   xmlns:cx="http://saxon.sf.net/complex"
   exclude-result-prefixes="xs saxon cx"
   expand-text="yes"
>

  <!-- Casting uses the first one that works -->
  
 
  
  <xsl:template name="xsl:initial-template">
    <out>
      <xsl:variable name="x" select="'2020' cast as union(xs:gYear, xs:integer)"/>
      <xsl:sequence select="$x instance of xs:gYear"/>
    </out>
  </xsl:template>  

</xsl:stylesheet>
