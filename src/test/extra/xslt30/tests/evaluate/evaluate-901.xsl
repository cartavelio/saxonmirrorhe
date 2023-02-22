<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="2.0"
xmlns:saxon="http://saxon.sf.net/"
xmlns:f="f.uri"
xmlns:fn="http://www.w3.org/2005/xpath-functions"
xmlns:xs="http://www.w3.org/2001/XMLSchema"
xmlns:math="java:java.lang.Math"
exclude-result-prefixes="f fn xs math">

<!--  test type error on a function called within saxon:evaluate -->
<!-- formerly saxon9083-err -->

<xsl:param name="e1">true()</xsl:param>
<xsl:param name="e2">concat('a', 'b')</xsl:param>
<xsl:param name="e3">fn:position()</xsl:param>
<xsl:param name="e4">xs:integer('3')</xsl:param>
<xsl:param name="e5">f:square('5')</xsl:param>
<xsl:param name="e6">math:max(5,2)</xsl:param>
    
  <xsl:template name="main">
  <xsl:for-each select="1 to 1">
    <out>
      <a><xsl:value-of select="saxon:evaluate($e1)"/></a>
      <a><xsl:value-of select="saxon:evaluate($e2)"/></a>
      <a><xsl:value-of select="saxon:evaluate($e3)"/></a>
      <a><xsl:value-of select="saxon:evaluate($e4)"/></a>
      <a><xsl:value-of select="saxon:evaluate($e5)"/></a>
      <a><xsl:value-of select="saxon:evaluate($e6)"/></a>
    </out>
  </xsl:for-each>    
  </xsl:template>
  
  <xsl:function name="f:square" as="xs:integer" visibility="public">
    <xsl:param name="x" as="xs:integer"/>
    <xsl:sequence select="$x*$x"/>
  </xsl:function>


  
</xsl:stylesheet>
