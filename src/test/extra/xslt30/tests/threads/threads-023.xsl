<xsl:stylesheet version="2.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">

<!-- Basic test of saxon:threads on xsl:apply-templates -->

<xsl:output indent="yes"/>

<xsl:template match="/">
<out>
  <xsl:apply-templates select="//ITEM" saxon:threads="4" xmlns:saxon="http://saxon.sf.net/" exclude-result-prefixes="saxon"/>
</out>
</xsl:template>

<xsl:template match="ITEM">
  <title pos="{position()}"><xsl:value-of select="TITLE"/></title>
</xsl:template>  

</xsl:stylesheet>