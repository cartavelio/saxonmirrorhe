<xsl:stylesheet version="2.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" 
    xmlns:saxon="http://saxon.sf.net/"
    exclude-result-prefixes="saxon">

<!-- Test of saxon:threads on xsl:for-each with variables outside the for-each -->

<xsl:output indent="yes"/>
<xsl:strip-space elements="*"/>

<xsl:variable name="in" select="/"/>

<xsl:template match="/">
<out>
  <xsl:variable name="results" as="element(*)*">
    <xsl:for-each select="1 to 100" saxon:threads="6">
      <xsl:message select="100 - ."/>
      <xsl:variable name="n" select="."/>
      <xsl:variable name="trans" select="$in/*/transaction[position() gt $n]"/>    
      <xsl:for-each select="$in/*/transaction[@value*100 gt current()]" saxon:threads="8">
          <xsl:variable name="price" select="@value"/>
          <xsl:variable name="p" select="position()"/>
          <xsl:if test="$price gt $trans[$p]/@value">
            <book phase="{$n}" price="{$price}" date="{@date}"/> 
          </xsl:if>       
      </xsl:for-each>
    </xsl:for-each>  
  </xsl:variable>
  <xsl:value-of select="count($results)"/>
</out>
</xsl:template>



</xsl:stylesheet>