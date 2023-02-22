<?xml version="1.0"?> 

<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="3.0"
  xmlns:saxon="http://saxon.sf.net/" 
  xmlns:f="http://local-functions/"
  xmlns:xs="http://www.w3.org/2001/XMLSchema"  
 exclude-result-prefixes="f xs" >

<!-- Test saxon:leading(). Was saxon045. -->


<xsl:template match="doc">
    <out>
    <xsl:for-each select="title">
        <chapter title="{.}">
        <xsl:for-each select="saxon:leading(following-sibling::*, f:isPara#1)">
            <p><xsl:value-of select="."/></p>
        </xsl:for-each>
        </chapter>
    </xsl:for-each>
    </out>
</xsl:template>

    <xsl:function name="f:isPara" as="xs:boolean">
      <xsl:param name="node" as="element()"/>
      <xsl:sequence select="exists($node/self::para)"/>
    </xsl:function>

 
</xsl:stylesheet>
