<?xml version="1.0" encoding="utf-8"?> 

<xsl:stylesheet
   xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="3.0"
   xmlns:saxon="http://saxon.sf.net/"
   xmlns:array="http://www.w3.org/2005/xpath-functions/array"
   extension-element-prefixes="saxon"
   exclude-result-prefixes="#all"
>
  
  <xsl:output method="json" build-tree="no"/>

<xsl:template match="/">
  <saxon:array>
    <xsl:for-each-group select=".//transaction" group-by="@date">
      <saxon:array select="current-group()/@value ! string()"/>
    </xsl:for-each-group>
  </saxon:array>
</xsl:template>


</xsl:stylesheet>
