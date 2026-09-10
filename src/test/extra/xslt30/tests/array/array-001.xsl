<?xml version="1.0" encoding="utf-8"?> 

<xsl:stylesheet
   xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="3.0"
   xmlns:saxon="http://saxon.sf.net/"
   xmlns:array="http://www.w3.org/2005/xpath-functions/array"
   extension-element-prefixes="saxon"
   exclude-result-prefixes="#all"
>

<xsl:template name="xsl:initial-template">
  <xsl:variable name="a" as="array(*)">
    <saxon:array composite="no">
      <xsl:sequence select="1 to 10"/>
    </saxon:array>
  </xsl:variable>
  <out count="{array:size($a)}" values="{$a?*}" last="{$a?10}"/>
</xsl:template>


</xsl:stylesheet>
