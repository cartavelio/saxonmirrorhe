<xsl:stylesheet version="2.0" 
  xmlns:xsl="http://www.w3.org/1999/XSL/Transform" 
  xmlns:xs="http://www.w3.org/2001/XMLSchema" 
  exclude-result-prefixes="xs">
  
  <!-- see bug 2220 -->
  
  <xsl:output indent="yes"/>

  <xsl:template match="/" name="main">
    <out>
     <xsl:for-each select="1 to 1000" saxon:threads="8" xmlns:saxon="http://saxon.sf.net/" exclude-result-prefixes="saxon">
       <xsl:variable name="temp" as="element()">
         <doc>
           <xsl:for-each select="1 to .+50">
              <ele><xsl:value-of select="."/></ele>
           </xsl:for-each>
         </doc>
       </xsl:variable>
       <in top="{name($temp)}"><xsl:value-of select="count($temp/ele[last()]/preceding-sibling::*)"/></in>
     </xsl:for-each>
    </out> 
  </xsl:template>

</xsl:stylesheet>