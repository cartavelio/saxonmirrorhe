<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet exclude-result-prefixes="#all" version="2.0" xmlns:saxon="http://saxon.sf.net/" 
  xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
  
  <xsl:attribute-set name="a-set">
    <xsl:attribute name="a" select="position()"/>
  </xsl:attribute-set>
  
  <xsl:template name="main">
    <out>
      <xsl:for-each saxon:threads="4" select="1 to 200">
        <e xsl:use-attribute-sets="a-set" />
      </xsl:for-each>
    </out>
  </xsl:template>
</xsl:stylesheet>