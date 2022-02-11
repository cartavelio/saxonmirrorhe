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
        "three": [{"a":1, "b":2, "c":3}, {"a":11, "b":22, "c":33}]
        }
   </xsl:variable>
   
   <xsl:variable name="parsed-json" as="map(*)" select="parse-json($json, map{xs:QName('saxon:parent-pointers'): true()})"/>
   
   <xsl:template name="xsl:initial-template">
      <out>
         <keys>
            <one>
               <xsl:value-of select="(($parsed-json => map:find('one'))?1 => map:get(xs:QName('saxon:get-key'))) ()"/>
            </one>
           <three>
               <xsl:value-of select="(($parsed-json => map:find('three'))?*)?* ! map:get(., xs:QName('saxon:get-key'))()"/>
            </three>
         </keys>
      </out>
   </xsl:template>
   
</xsl:stylesheet>
