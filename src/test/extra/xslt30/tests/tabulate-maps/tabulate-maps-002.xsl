<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns:saxon="http://saxon.sf.net/"
   xmlns:map="http://www.w3.org/2005/xpath-functions/map"
   xmlns:array="http://www.w3.org/2005/xpath-functions/array"
   xmlns:xs="http://www.w3.org/2001/XMLSchema" extension-element-prefixes="saxon"
   exclude-result-prefixes="#all" expand-text="yes" version="3.0">

   <!-- Hierarchic inversion using tabulate-maps followed by grouping -->

   <xsl:template name="xsl:initial-template" as="array(*)">
      <xsl:variable name="table" as="map(*)*">
         <saxon:tabulate-maps root="json-doc('customers.json')"
            select="map:find(., 'order-lines')?*?*"/>
      </xsl:variable>

      <saxon:array>
         <xsl:for-each-group select="$table" group-by="?product">
               <xsl:map>
                  <xsl:map-entry key="'product'" select="current-grouping-key()"/>
                  <xsl:map-entry key="'customers'">
                     <saxon:array composite="yes">
                        <xsl:for-each select="current-group()">
                           <saxon:array-member>
                              <xsl:map>
                                 <xsl:map-entry key="'name'" select="?customer"/>
                                 <xsl:map-entry key="'quantity'" select="?quantity"/>
                                 <xsl:map-entry key="'date'" select="?date"/>
                              </xsl:map>
                           </saxon:array-member>
                        </xsl:for-each>
                     </saxon:array>
                  </xsl:map-entry>
               </xsl:map>           
         </xsl:for-each-group>
      </saxon:array>
   </xsl:template>



</xsl:stylesheet>
