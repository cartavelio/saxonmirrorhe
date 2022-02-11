<?xml version="1.0"?> 
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="2.0">

  <!-- PURPOSE test saxon:unparsed-entities function -->

  <xsl:variable name="root" select="/"/>
  <xsl:template match="/">
    <out>
      <xsl:for-each select="saxon:unparsed-entities(/)" xmlns:saxon="http://saxon.sf.net/" exclude-result-prefixes="saxon">
       <xsl:variable name="ename" select="."/>
       <ent name="{.}"
           sys="{$root/unparsed-entity-uri($ename)}"
           pub="{$root/unparsed-entity-public-id($ename)}"/>
      </xsl:for-each>     
    </out>
  </xsl:template>
  
  <xsl:output indent="yes"/>
 
</xsl:stylesheet>
