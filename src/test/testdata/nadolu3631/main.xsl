<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
    xmlns:xs="http://www.w3.org/2001/XMLSchema"
    exclude-result-prefixes="xs"
    version="2.0"
    xmlns:saxon="http://saxon.sf.net/">
    
    <xsl:output saxon:next-in-chain="second.xsl" method="xml"/>
    
    <xsl:template match="/">
        <mainElement>
            <test/>
        </mainElement>    
    </xsl:template>
    
</xsl:stylesheet>