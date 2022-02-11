<xsl:stylesheet exclude-result-prefixes="#all" version="3.0" 
  xmlns:jt="http://saxon.sf.net/java-type" xmlns:xs="http://www.w3.org/2001/XMLSchema" 
  xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
  <xsl:output indent="yes" omit-xml-declaration="yes" />
  <xsl:template name="main">
    <xsl:variable as="jt:java.math.BigInteger" name="big-int" 
      select="Q{java:java.math.BigInteger}ZERO()" />
    <test>
      <xsl:value-of select="Q{java:java.math.BigInteger}add($big-int, 1)" />
    </test>
  </xsl:template>
</xsl:stylesheet>