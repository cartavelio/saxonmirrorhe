<?xml version="1.0" encoding="utf-8"?> 

<xsl:stylesheet
   xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="3.0"
   xmlns:xs="http://www.w3.org/2001/XMLSchema"
   xmlns:saxon="http://saxon.sf.net/"
   extension-element-prefixes="saxon"
   exclude-result-prefixes="xs"
   expand-text="yes"
>

  <xsl:variable name="in">
    <doc>
      <a id="1"/>
      <b id="2"/>
      <c id="3"/>
    </doc>
  </xsl:variable>
  
  <xsl:template name="xsl:initial-template">
    <out>
      <saxon:update select="$in/doc">
        <xsl:if test="count(*) eq 3">
          <saxon:replace select="a/@id">
            <xsl:attribute name="id1" select="'a1'"/>
            <xsl:attribute name="id2" select="'a2'"/>
          </saxon:replace>
        </xsl:if>
      </saxon:update>
    </out>
  </xsl:template>  

</xsl:stylesheet>
