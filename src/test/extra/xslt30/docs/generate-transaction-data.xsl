<xsl:stylesheet version="1.0"
                xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
                xmlns:random="http://exslt.org/random"
                xmlns:saxon="http://saxon.sf.net/"
                xmlns:xs="http://www.w3.org/2001/XMLSchema"
                extension-element-prefixes="random saxon xs">

<xsl:variable name="begin" select="xs:dateTime('2007-01-01T00:00:00')"/>
<xsl:variable name="last" select="$begin" saxon:assignable="yes"/>

<xsl:template name="main">
<account nr="76543210">
  <account-number>01234567</account-number>
    <xsl:for-each select="random:random-sequence(100000)">
      <saxon:assign name="last" select="$last + xs:dayTimeDuration('P1D')*."/>
      <transaction value="{format-number(30* (. - 0.25), '0.00')}"
                           date="{xs:date($last)}"/>
   </xsl:for-each>
</account>
</xsl:template>

<xsl:output indent="yes"/>
              
</xsl:stylesheet>