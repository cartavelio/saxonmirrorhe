<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
    xmlns:xs="http://www.w3.org/2001/XMLSchema" xmlns:saxon="http://saxon.sf.net/"
    exclude-result-prefixes="xs" version="3.0">
    
    <xsl:variable name="mailSetup" as="map(xs:string, xs:string)">
        <xsl:map>
            <xsl:map-entry key="'to'" select="'me@gmail.com'"/>
            <xsl:map-entry key="'from'" select="'me@gmail.com'"/>
            <xsl:map-entry key="'smtp-server'" select="'smtp.gmail.com'"/>
            <xsl:map-entry key="'username'" select="'me@gmail.com'"/>
            <xsl:map-entry key="'password'" select="'mepassword'"/>
        </xsl:map>
    </xsl:variable>
    
    <xsl:variable name="html" as="xs:string">
        <![CDATA[ <html>
            <body>
                <h1>Test saxon:send-mail</h1>
                <p>Test Ok</p>
            </body>
        </html>]]> 
    </xsl:variable>
    
    <xsl:template name="xsl:initial-template">
        <xsl:value-of select="saxon:send-mail($mailSetup, 'Test', $html, ())"/>
    </xsl:template>
    
</xsl:stylesheet>