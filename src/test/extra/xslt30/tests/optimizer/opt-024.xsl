<xsl:stylesheet version="3.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" 
    xmlns:xs="http://www.w3.org/2001/XMLSchema">
    
    <xsl:variable name="call" select="'123'"/>
    
    <xsl:template match="/">
        <out>
            <xsl:apply-templates select="$call">
                <xsl:with-param name="index" select="0"/>
                <xsl:with-param name="max" select="10000"/>
            </xsl:apply-templates>
        </out>
    </xsl:template>
    
    <xsl:template match="$call">
        <xsl:param name="index" as="xs:integer"/>
        <xsl:param name="max" as="xs:integer"/>
        
        <value>
            <xsl:value-of select="$index"/>
        </value>
        
        <xsl:if test="$index lt $max">
            <xsl:apply-templates select="$call">
                <xsl:with-param name="index" select="$index + 1"/>
                <xsl:with-param name="max" select="$max"/>
            </xsl:apply-templates>
        </xsl:if>
    </xsl:template>
    
</xsl:stylesheet>