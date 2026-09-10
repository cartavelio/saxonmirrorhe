<?xml version="1.0"?> 
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="3.0"
xmlns:saxon="http://saxon.sf.net/" 
xmlns:xs="http://www.w3.org/2001/XMLSchema"
xmlns:my="http://my.net/"
exclude-result-prefixes="my saxon xs">

  <!-- PURPOSE test saxon:highest() on strings with a collation -->

  <xsl:variable name="data" as="element()*">
    <a id="1">cat</a>
    <a id="2">cart</a>
    <a id="3">Catherine</a>
    <a id="4">CAT</a>
    <a id="5">katy</a>
    <a id="6">cathartic</a>
    <a id="7">Katy</a>
    <a id="8">KATY</a>
  </xsl:variable>
  
  <xsl:template name="main">
    <out>
      <xsl:copy-of select="saxon:highest($data, my:val#1)"
        default-collation="http://www.w3.org/2005/xpath-functions/collation/html-ascii-case-insensitive" />
      <xsl:copy-of select="max($data)"
        default-collation="http://www.w3.org/2005/xpath-functions/collation/html-ascii-case-insensitive" />
    </out>
  </xsl:template>
  
  <xsl:function name="my:val" as="xs:string">
    <xsl:param name="item" as="element()"/>
    <xsl:sequence select="xs:string($item)"/>
  </xsl:function>
  
</xsl:stylesheet>
