<xsl:stylesheet version="2.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">

<!-- Basic test of saxon:threads on xsl:for-each -->

<xsl:output indent="yes"/>
<xsl:strip-space elements="*"/>

<xsl:template match="/">
<out>
  <xsl:for-each select="//ITEM" saxon:threads="4" xmlns:saxon="http://saxon.sf.net/" exclude-result-prefixes="saxon">
    <book>
      <xsl:apply-templates>
        <xsl:with-param name="p" select="position()"/>
      </xsl:apply-templates>
    </book>
  </xsl:for-each>
</out>
</xsl:template>

<xsl:template match="TITLE" priority="2">
  <xsl:copy-of select="."/>
</xsl:template>

<xsl:template match="AUTHOR" priority="2">
  <xsl:param name="p"/>
  <author><xsl:value-of select="."/></author>
  <position><xsl:value-of select="$p"/></position>
</xsl:template>

<xsl:template match="ITEM/*" priority="1"/>

</xsl:stylesheet>