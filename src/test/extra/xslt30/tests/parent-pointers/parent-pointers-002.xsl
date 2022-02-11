<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet 
   xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
   xmlns:saxon="http://saxon.sf.net/"
   xmlns:map="http://www.w3.org/2005/xpath-functions/map"
   xmlns:xs="http://www.w3.org/2001/XMLSchema"
   exclude-result-prefixes="#all" 
   version="3.0">
   
   <xsl:variable name="json" as="xs:string">
      { "one": {"a":1, "b":2, "c":3},
        "two": {"a":11, "b":22, "c":33},
        "three": [{"a":1, "b":2, "c":3}, {"a":11, "b":22, "cxy":{"x":33, "y":44}}]
        }
   </xsl:variable>
   
   <xsl:variable name="parsed-json" as="map(*)" select="parse-json($json, map{xs:QName('saxon:parent-pointers'): true()})"/>
   
   <xsl:template name="xsl:initial-template">
      <out>
         <xsl:variable name="cxy" select="map:find($parsed-json, 'cxy')?1" as="map(*)"/>
         <x><xsl:value-of select="$cxy?x"/></x>
         <xsl:variable name="three2" select="map:get($cxy, xs:QName('saxon:get-parent'))()" as="map(*)"/>
         <z><xsl:value-of select="$three2?a"/></z>
      </out>
   </xsl:template>
   
</xsl:stylesheet>
