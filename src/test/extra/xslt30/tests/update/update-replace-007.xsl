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
        <xsl:variable name="last" as="attribute(*)">
          <xsl:attribute name="last" select="count(*)"/>
        </xsl:variable>
        <xsl:for-each select="*">
          <xsl:choose>
            <xsl:when test="@id='1'">
              <saxon:replace select="@id">
                <xsl:attribute name="id2">1</xsl:attribute>
                <xsl:sequence select="$last"/>
              </saxon:replace>
            </xsl:when>
            <xsl:when test="@id='2'">
              <saxon:replace select="@id">
                <xsl:attribute name="id2">2</xsl:attribute>
                <xsl:sequence select="$last"/>
              </saxon:replace>
            </xsl:when>
            <xsl:when test="@id='3'">
              <saxon:replace select="@id">
                <xsl:attribute name="id2">3</xsl:attribute>
                <xsl:sequence select="$last"/>
                <saxon:delete/>
              </saxon:replace>
            </xsl:when>
          </xsl:choose>
        </xsl:for-each>
      </saxon:update>
    </out>
  </xsl:template>  

</xsl:stylesheet>
