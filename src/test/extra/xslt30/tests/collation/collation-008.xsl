<?xml version="1.0" encoding="UTF-8"?>

<!-- test xsl:key with collations -->
<?spec xslt#xsl-key?>
<?dependency saxon?>

<xsl:stylesheet version="2.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" 
xmlns:xs="http://www.w3.org/2001/XMLSchema"
xmlns:saxon="http://saxon.sf.net/"
exclude-result-prefixes="xs saxon">


<xsl:template name="main">
    <out>
      <xsl:variable name="c">http://saxon.sf.net/collation?alphanumeric=yes;ignore-case=yes</xsl:variable>
      <a><xsl:value-of select="collation-key('chap1000', $c) > collation-key('CHAP2', $c)"/></a>
      <b><xsl:value-of select="collation-key('100', $c) lt collation-key('$100', $c)"/></b>
      <c><xsl:value-of select="collation-key('100', $c) lt collation-key('£100', $c)"/></c>
      <d><xsl:value-of select="collation-key('$100', $c) lt collation-key('£100', $c)"/></d>
      <e><xsl:value-of select="collation-key('2D', $c) lt collation-key('1000D', $c)"/></e>
   </out>
</xsl:template>
</xsl:stylesheet>
