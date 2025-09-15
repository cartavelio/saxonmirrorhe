<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
    xmlns:xs="http://www.w3.org/2001/XMLSchema"
    exclude-result-prefixes="xs"
    version="3.0">
    
    <xsl:mode on-no-match="shallow-copy"/>
    <xsl:output indent="yes" suppress-indentation="p type typedesc"/>
    
    <xsl:template match="feature">
        <xsl:copy>
          <xsl:if test="not(featureCode)">
            <featureCode><xsl:number/></featureCode>
          </xsl:if>
          <xsl:apply-templates/>
        </xsl:copy>
    </xsl:template>
    
</xsl:stylesheet>
