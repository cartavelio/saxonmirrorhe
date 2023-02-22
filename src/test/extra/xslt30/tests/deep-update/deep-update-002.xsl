<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet 
   xmlns:xsl="http://www.w3.org/1999/XSL/Transform" 
   xmlns:saxon="http://saxon.sf.net/"
   xmlns:map="http://www.w3.org/2005/xpath-functions/map"
   xmlns:xs="http://www.w3.org/2001/XMLSchema"
   extension-element-prefixes="saxon"
   exclude-result-prefixes="#all" 
   expand-text="yes"
   version="3.0">

   <xsl:template name="xsl:initial-template">
      <xsl:variable name="in" select="json-doc('customers.json')"/>
      <xsl:variable name="updated" as="array(*)">
         <saxon:deep-update
            root = "$in"
            select = "?*[?customer = 'Jones the Baker'] ? orders?*[?order-no = 123] ? order-lines?*[contains(?product, 'flour')]"
            action = "map:put(., 'quantity', ?quantity * 2)"/>
      </xsl:variable>
      <out><xsl:value-of select="serialize($updated?1?orders?1?order-lines, map{'method':'json', QName('http://saxon.sf.net/','property-order'): ('quantity', 'product')})"/></out>
   </xsl:template>

 

</xsl:stylesheet>

