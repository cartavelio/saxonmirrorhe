<?xml version="1.0"?> 
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="3.0"
xmlns:saxon="http://saxon.sf.net/" 
xmlns:xs="http://www.w3.org/2001/XMLSchema"
xmlns:my="http://my.net/"
exclude-result-prefixes="my saxon xs">

  <!-- PURPOSE test saxon:highest() on empty sequence - Saxon 9.2 version using higher-order functions -->

  <xsl:variable name="data" as="element()*">

  </xsl:variable>
  
  <xsl:template name="main">
    <out>
      <xsl:copy-of select="saxon:highest($data, my:val#1)"/>
    </out>
  </xsl:template>
  
  <xsl:function name="my:val" as="xs:decimal">
    <xsl:param name="item" as="element()"/>
    <xsl:sequence select="xs:decimal($item)"/>
  </xsl:function>
  
</xsl:stylesheet>
