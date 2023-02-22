<?xml version="1.0"?> 

<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="1.0"
xmlns:saxon="http://saxon.sf.net/">

<!-- Test saxon:path function -->

<xsl:template match="/">
<out>
    <xsl:for-each select="//node()">
        <p><xsl:value-of select="saxon:path()"/></p>
    </xsl:for-each>
    <xsl:for-each select="//@*">
        <p><xsl:value-of select="saxon:path()"/></p>
    </xsl:for-each>   
    <xsl:for-each select="//namespace::*">
        <xsl:sort select="name()"/>
        <p><xsl:value-of select="saxon:path()"/></p>
    </xsl:for-each>
</out>
</xsl:template>
 
</xsl:stylesheet>
