<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
    xmlns:xs="http://www.w3.org/2001/XMLSchema"
    xmlns:saxon="http://saxon.sf.net/"
    exclude-result-prefixes="xs"
    extension-element-prefixes="saxon"
    version="3.0">
    
    <xsl:output method="json" indent="yes" saxon:property-order="N * C" saxon:line-length="120" saxon:indent-spaces="1"/>
    
    <xsl:template match="*">
        <xsl:map>
            <xsl:map-entry key="'N'" select="local-name()"/>
            <xsl:apply-templates select="@*"/>
            <xsl:if test="has-children(.)">
                <xsl:map-entry key="'C'">
                    <saxon:array>
                        <xsl:for-each select="*">
                            <saxon:array-member>
                                <xsl:apply-templates select="."/>
                            </saxon:array-member>
                        </xsl:for-each>
                    </saxon:array>
                </xsl:map-entry>
            </xsl:if>
        </xsl:map>
    </xsl:template>
    
    <xsl:template match="@*">
        <xsl:map-entry key="local-name()" select="string(.)"/>
    </xsl:template>
    
</xsl:stylesheet>