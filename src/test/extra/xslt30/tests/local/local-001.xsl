<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="2.1"
  xmlns:xs="http://www.w3.org/2001/XMLSchema"
  exclude-result-prefixes="xs">

<!-- test of numbering in words in Dutch -->

<xsl:template name="main">
<out>
  <xsl:for-each select="1 to 50, 100, 500, 1000, 1001, 2000, 2005, 2012, 2020, 10000, 100000">
     <n n="{.}"><xsl:number value="." format="w" lang="nl"/></n>
  </xsl:for-each>
</out>     
</xsl:template>

</xsl:stylesheet>
  