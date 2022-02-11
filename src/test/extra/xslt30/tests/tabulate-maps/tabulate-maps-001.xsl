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

   <xsl:template name="xsl:initial-template" as="map(*)*">
      <xsl:variable name="table" as="map(*)*">
         <saxon:tabulate-maps
            root = "json-doc('customers.json')"
            select = "map:find(., 'order-lines')?*?*"/>
      </xsl:variable>
      <xsl:sequence select="$table"/>
   </xsl:template>

 

</xsl:stylesheet>

