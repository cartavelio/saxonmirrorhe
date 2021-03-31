<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet 
   xmlns:xsl="http://www.w3.org/1999/XSL/Transform" 
   xmlns:saxon="http://saxon.sf.net/"
   xmlns:map="http://www.w3.org/2005/xpath-functions/map"
   xmlns:array="http://www.w3.org/2005/xpath-functions/array"
   xmlns:xs="http://www.w3.org/2001/XMLSchema"
   extension-element-prefixes="saxon"
   exclude-result-prefixes="#all" 
   expand-text="yes"
   version="3.0">

   <xsl:template name="xsl:initial-template">
      <xsl:variable name="json" expand-text="no">
         { "a": {
              "x":1,
              "y":2,
              "z": [1,2,3, {"quantity":17}]
              },
           "b": {
              "x":11,
              "y":22,
              "z": [11,22,33, {"quantity":39}]
              }
         }
      </xsl:variable>
      <xsl:variable name="updated" as="map(*)">
         <saxon:deep-update
            root = "parse-json($json)"
            select = "map:find(., 'z')?*?4"
            action = "map:put(.,'quantity', ?quantity+1)"/>
      </xsl:variable>
      <out>
         <xsl:value-of select="sort($updated => map:find('quantity'))"/>
      </out>
   </xsl:template>

 

</xsl:stylesheet>

