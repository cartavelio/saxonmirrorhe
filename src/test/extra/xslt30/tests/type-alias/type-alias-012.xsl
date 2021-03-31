<?xml version="1.0" encoding="utf-8"?> 

<xsl:stylesheet
   xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="3.0"
   xmlns:xs="http://www.w3.org/2001/XMLSchema"
   xmlns:saxon="http://saxon.sf.net/"
   xmlns:cx="http://saxon.sf.net/complex"
   exclude-result-prefixes="xs saxon cx"
   expand-text="yes"
>

  <xsl:variable name="m" select="map{'a':1, 'b':'two', 'c':current-date()}"/>
  <xsl:template name="xsl:initial-template">
    <out>
      <a>{ $m instance of tuple(a, b, c) }</a>
      <b>{ $m instance of tuple(a, b, c, d) }</b>
      <c>{ $m instance of tuple(a as xs:integer, b, c as xs:date?) }</c>
      <d>{ $m instance of tuple(a, b, c, d as xs:date?) }</d>
      <e>{ $m instance of tuple(a, b, c, d as xs:date) }</e>
      <f>{ $m instance of tuple(c, *) }</f>
    </out>
  </xsl:template>  

</xsl:stylesheet>
