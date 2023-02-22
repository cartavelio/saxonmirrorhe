<xsl:stylesheet xmlns:xsl = "http://www.w3.org/1999/XSL/Transform" version="3.0" 
    xmlns:xs="http://www.w3.org/2001/XMLSchema" 
    xmlns:pB="http://www.ctk.cz/B/">
    
    <xsl:use-package name='http://www.ctk.cz/B/'/>
    <xsl:template name="xsl:initial-template" as="xs:string">
        <xsl:sequence select="pB:fB()"/>
    </xsl:template>
</xsl:stylesheet>