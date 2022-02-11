<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet 
   xmlns:xsl="http://www.w3.org/1999/XSL/Transform" 
   xmlns:saxon="http://saxon.sf.net/"
   xmlns:map="http://www.w3.org/2005/xpath-functions/map"
   xmlns:array="http://www.w3.org/2005/xpath-functions/array"
   xmlns:xs="http://www.w3.org/2001/XMLSchema"
   extension-element-prefixes="saxon"
   exclude-result-prefixes="#all" 
   expand-text="no"
   version="3.0">
   
   <xsl:variable name="json" as="xs:string">
      [{ "country": "UK",
      "cities": [
      { "name": "London",
      "population" : 7200000
      },
      { "name": "Birmingham",
      "population": 992000
      },
      { "name": "Leeds",
      "population": 720000
      }
      ]},
      { "country": "France",
      "cities": [
      { "name": "Paris",
      "population" : 12341418
      },
      { "name": "Lyon",
      "population": 2214068
      },
      { "name": "Marseille",
      "population": 1727070
      },
      93.7
      ]}
      ]
   </xsl:variable>

   <xsl:template name="xsl:initial-template">
      <xsl:variable name="table" as="item()*">
         <saxon:tabulate-maps
            root = "parse-json($json)"
            select = "?*?cities?*"/>
      </xsl:variable>
      <out>
         <xsl:value-of select="serialize(array{$table}, map{'method':'json'})"/>
      </out>
   </xsl:template>

 

</xsl:stylesheet>

