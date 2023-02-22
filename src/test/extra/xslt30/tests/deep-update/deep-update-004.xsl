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
      <xsl:variable name="updated" as="array(*)">
         <saxon:deep-update
            root = "[[1,[2,2]],[3,[4,4]],[5,[6,6]],[7,[8,8]]]"
            select = "?*?2"
            action = "array:append(.,11)"/>
      </xsl:variable>
      <out><xsl:value-of select="serialize($updated, map{'method':'json'})"/></out>
   </xsl:template>

 

</xsl:stylesheet>

