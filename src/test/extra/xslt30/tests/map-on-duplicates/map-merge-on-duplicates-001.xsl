<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet version="3.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
    xmlns:map="http://www.w3.org/2005/xpath-functions/map"
    xmlns:xs="http://www.w3.org/2001/XMLSchema" xmlns:saxon="http://saxon.sf.net/"
    exclude-result-prefixes="#all">

 <xsl:template match="/">
     <xsl:variable name="map" as="map(*)">
         <xsl:sequence select="map:merge(//transaction ! map:entry(xs:date(@date), xs:decimal(@value)), 
             map{QName('http://saxon.sf.net/', 'on-duplicates'): function($x, $y){$x + $y}})"/>
     </xsl:variable>
     <out>
         <xsl:for-each select="sort(map:keys($map))">
             <period date="{.}" total="{$map(.)}"/>
         </xsl:for-each>
     </out>
 </xsl:template>
 
</xsl:stylesheet>
