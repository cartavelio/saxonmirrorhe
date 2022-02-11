<?xml version="1.0" encoding="utf-8"?> 

<?dependency saxon?>
<?spec xslt#variables?>
<!-- A stylesheet that combines saxon:assign with recursive templates -->
<!-- was bug83 -->                                                                                                                                                

<!DOCTYPE xsl:stylesheet [ 
  <!ENTITY nbsp "&#160;"> 
  <!ENTITY mdash "&#8212;">
  <!ENTITY laquo "&#171;">
  <!ENTITY raquo "&#187;">
]>

<xsl:stylesheet
   xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="1.0"
   xmlns:saxon="http://saxon.sf.net/"
   xmlns:fo="http://www.w3.org/1999/XSL/Format"
   xmlns:dk="http://www.kirsanov.com"
   xmlns:rx="http://www.renderex.com/XSL/Extensions"
   extension-element-prefixes="saxon dk"
>

<xsl:variable name="string" saxon:assignable="yes"/>
<xsl:variable name="oldstring" saxon:assignable="yes"/>
<xsl:variable name="period" select="0" saxon:assignable="yes"/>
<xsl:variable name="comma" select="0" saxon:assignable="yes"/>
<xsl:variable name="countquote" select="0" saxon:assignable="yes"/>
<xsl:variable name="apostrophe">'</xsl:variable> 
<xsl:variable name="quote">"</xsl:variable>
<xsl:variable name="daggers">&#8224;&#8224;&#8224;&#8224;&#8224;&#8224;&#8224;&#8224;&#8224;&#8224;</xsl:variable>  

                                                      
<xsl:output method="xml" omit-xml-declaration="no" encoding="utf-8" indent="no"/>
  
  <xsl:template match="/">
    <out><xsl:apply-templates/></out>
  </xsl:template>

<!-- special symbols -->

<xsl:template match="p/text()|dk:dropcapright/text()|dk:dropcapbottom/text()|gt/text()|caption/text()|h1/text()|h2/text()|h3/text()|h4/text()|h5/text()|fn/text()|dk:beforepagebr/text()|dk:afterpagebr/text()|text[not(@type='code')]/text()|comment/text()|subtext[not(@type='code')]/text()">
<xsl:choose>
  <xsl:when test="($period = 1) and starts-with(., '.')">
    <saxon:assign name="string" select="substring(., 2)"/>
    <saxon:assign name="period" select="0"/>
  </xsl:when>
  <xsl:when test="($comma = 1) and starts-with(., ',')">
    <saxon:assign name="string" select="substring(., 2)"/>
    <saxon:assign name="comma" select="0"/>
  </xsl:when>
  <xsl:otherwise>
    <saxon:assign name="string" select="."/>
  </xsl:otherwise>
</xsl:choose>
<xsl:call-template name="rec-change"/>
<xsl:value-of select="$string"/>
</xsl:template>

<xsl:template name="rec-change">
<saxon:assign name="oldstring" select="$string"/>

<xsl:if test="contains($string, 'fi')">
  <saxon:assign name="string"><xsl:value-of select="substring-before($string, 'fi')"/>&#64257;<xsl:value-of select="substring-after($string, 'fi')"/></saxon:assign>
</xsl:if>

<xsl:if test="contains($string, 'fl')">
  <saxon:assign name="string"><xsl:value-of select="substring-before($string, 'fl')"/>&#64258;<xsl:value-of select="substring-after($string, 'fl')"/></saxon:assign>
</xsl:if>

<xsl:if test="contains($string, ' -') and not(contains($string, ' --'))">
  <saxon:assign name="string"><xsl:value-of select="substring-before($string, ' -')"/>&#160;&#8212;<xsl:value-of select="substring-after($string, ' -')"/></saxon:assign>
</xsl:if>

<xsl:if test="contains($string, $apostrophe)">    
  <saxon:assign name="string"><xsl:value-of select="substring-before($string, $apostrophe)"/>&#8217;<xsl:value-of select="substring-after($string, $apostrophe)"/></saxon:assign>
</xsl:if>

<xsl:if test="contains($string, $quote)">
    <xsl:choose>
      <xsl:when test="$countquote=0">
        <saxon:assign name="string"><xsl:value-of select="substring-before($string, $quote)"/>&#8220;<xsl:value-of select="substring-after($string, $quote)"/></saxon:assign>
        <saxon:assign name="countquote" select="1"/>
      </xsl:when>
      <xsl:when test="$countquote=1">
        <saxon:assign name="string"><xsl:value-of select="substring-before($string, $quote)"/>&#8221;<xsl:value-of select="substring-after($string, $quote)"/></saxon:assign>
        <saxon:assign name="countquote" select="0"/>
      </xsl:when>
    </xsl:choose>
    <xsl:call-template name="rec-change"/>
</xsl:if>

<xsl:if test="not($oldstring = $string)">
  <xsl:call-template name="rec-change"/>
</xsl:if>
</xsl:template>


</xsl:stylesheet>
