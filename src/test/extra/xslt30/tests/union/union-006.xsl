<?xml version="1.0" encoding="utf-8"?> 

<xsl:stylesheet
   xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="3.0"
   xmlns:xs="http://www.w3.org/2001/XMLSchema"
   xmlns:saxon="http://saxon.sf.net/"
   xmlns:cx="http://saxon.sf.net/complex"
   exclude-result-prefixes="xs saxon cx"
   expand-text="yes"
>

  <!-- Duplicates are allowed -->
  
 
  
  <xsl:template name="xsl:initial-template">
    <out>
      <xsl:apply-templates select="current-date() instance of union(xs:string, xs:date, xs:double, xs:date)"/>
    </out>
  </xsl:template>  

</xsl:stylesheet>
