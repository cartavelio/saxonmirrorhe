<?xml version="1.0"?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
                version="1.0">

<xsl:import href="xsl-query.xsl"/>

<xsl:param name="additional.css">
  <xsl:text>
span.found       { background-color: #FFFFCC; 
                       
                             font-weight: bold; 
                           }
</xsl:text>
</xsl:param>

<xsl:template match="phrase">
  <span>
    <xsl:if test="@role">
      <xsl:attribute name="class">
        <xsl:value-of select="@role"/>
      </xsl:attribute>
    </xsl:if>
    <xsl:apply-templates/>
  </span>
</xsl:template>

</xsl:stylesheet>
