<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet exclude-result-prefixes="#all" version="3.0" xmlns:saxon="http://saxon.sf.net/" 
  xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
  
  <xsl:use-package name="http://saxon.sf.net/test/threads-025" package-version="1.0">    
    <xsl:override>
      <xsl:attribute-set name="b-set" use-attribute-sets="a-set">
        <xsl:attribute name="b" select="position()"/>
      </xsl:attribute-set>
    </xsl:override>
  </xsl:use-package>

  <xsl:attribute-set name="b-set" use-attribute-sets="a-set">
    <xsl:attribute name="b" select="position()"/>
  </xsl:attribute-set>
  
  <xsl:template name="main">
    <out>
      <xsl:for-each saxon:threads="4" select="1 to 200">
        <e xsl:use-attribute-sets="a-set" />
      </xsl:for-each>
    </out>
  </xsl:template>
</xsl:stylesheet>