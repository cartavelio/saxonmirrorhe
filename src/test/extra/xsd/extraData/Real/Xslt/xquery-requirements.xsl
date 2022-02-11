<?xml version="1.0"?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
                version="1.0">

<xsl:import href="xmlspec.xsl"/>

<xsl:template match="glist[@role='req']/gitem/label">
  <dt class="label">
    <xsl:call-template name="anchor">
      <xsl:with-param name="node" select=".."/>
    </xsl:call-template>
    <xsl:call-template name="anchor"/>

    <xsl:number level="multiple" count="gitem|div1|div2|div3|div4|div5"/>
    <xsl:text> </xsl:text>
    <xsl:apply-templates/>
  </dt>
</xsl:template>

</xsl:stylesheet>
