<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
                version="4.0"
                xmlns:xs="http://www.w3.org/2001/XMLSchema"
                exclude-result-prefixes="#all"
                expand-text="yes">

    <xsl:output method="xml" indent="yes"/>

    <xsl:template match="/" name="xsl:initial-template">
        <xsl:sequence select="invisible-xml(&quot;S=A. A='a'.&quot;)('a')"/>
        <xsl:comment>Run with {system-property('xsl:product-name')} {system-property('xsl:product-version')} at {current-dateTime()}</xsl:comment>
    </xsl:template>

</xsl:stylesheet>