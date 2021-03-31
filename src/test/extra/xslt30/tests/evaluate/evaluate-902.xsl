<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="2.0"
xmlns:saxon="http://saxon.sf.net/"
xmlns:f="f.uri"
xmlns:fn="http://www.w3.org/2005/xpath-functions"
xmlns:xs="http://www.w3.org/2001/XMLSchema"
xmlns:math="java:java.lang.Math"
xmlns:my="http://my.com/"
exclude-result-prefixes="#all">

<!--  test calling format-number() within saxon:evaluate. -->

<xsl:param name="e1">format-number($p1, '#.00')</xsl:param>
<xsl:param name="e2">format-number($p1, '#,00', 'my:format')</xsl:param>

<xsl:decimal-format name="my:format" decimal-separator="," grouping-separator="'"/>

    
  <xsl:template match="/" name="main">
    <out>
      <z><xsl:copy-of select="saxon:evaluate($e1, 123.456)"/></z>
      <z><xsl:copy-of select="saxon:evaluate($e2, 456.789)"/></z>
    </out>
      
  </xsl:template>
  
 
</xsl:stylesheet>
