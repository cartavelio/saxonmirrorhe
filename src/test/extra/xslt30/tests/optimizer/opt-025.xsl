<xsl:stylesheet version="3.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" 
    xmlns:xs="http://www.w3.org/2001/XMLSchema">

    <xsl:template match="/">
        <out>
            <xsl:call-template name="call">
                <xsl:with-param name="index" select="0"/>
                <xsl:with-param name="max" select="10000"/>
            </xsl:call-template>
        </out>
    </xsl:template>
    
    <xsl:template name="call">
        <xsl:param name="index" as="xs:integer"/>
        <xsl:param name="max" as="xs:integer"/>
        
        <value>
            <xsl:value-of select="$index"/>
        </value>
        
        <xsl:if test="$index lt $max">
            <xsl:call-template name="call">
                <xsl:with-param name="index" select="$index + 1"/>
                <xsl:with-param name="max" select="$max"/>
            </xsl:call-template>
        </xsl:if>
    </xsl:template>
    
</xsl:stylesheet>