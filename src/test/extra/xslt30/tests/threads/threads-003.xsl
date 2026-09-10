<xsl:stylesheet version="2.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">

<!-- Basic test of saxon:threads on xsl:for-each with local variables -->

<xsl:output indent="yes"/>
<xsl:strip-space elements="*"/>

<xsl:template match="/">
<out>
  <xsl:for-each select="//ITEM" saxon:threads="4" xmlns:saxon="http://saxon.sf.net/" exclude-result-prefixes="saxon">
    <xsl:variable name="price" select="PRICE"/>
    <book price="{$price}">      
      <xsl:apply-templates>
        <xsl:with-param name="p" select="position()"/>
        <xsl:with-param name="cost" select="$price * 1.175"/>
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
  <xsl:param name="cost"/>
  <author><xsl:value-of select="."/></author>
  <position><xsl:value-of select="$p"/></position>
  <gross-price><xsl:value-of select="$cost"/></gross-price>
</xsl:template>

<xsl:template match="ITEM/*" priority="1"/>

</xsl:stylesheet>