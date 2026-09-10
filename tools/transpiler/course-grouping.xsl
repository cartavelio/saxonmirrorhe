<xsl:transform xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="4.0">
   
   <xsl:template name="xsl:initial-template">
      <xsl:array>
         <xsl:for-each-group select="json-doc('courses.json')??entry::email" 
                             group-by="?value">
            <xsl:sort select="current-grouping-key()"/>
            <xsl:map>
               <xsl:map-entry key="'email'" 
                              select="current-grouping-key()"/>
               <xsl:map-entry key="'courses'">
                  <xsl:array select="current-group() ? ancestors() ? course"/>
               </xsl:map-entry>
            </xsl:map>
         </xsl:for-each-group>
      </xsl:array>
   </xsl:template>
   
</xsl:transform>
