<xsl:package name="http://package-104/" package-version="2.1.0.5" version="3.0"
    xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
    <xsl:use-package name="http://package-105/" package-version="*"/>
    <xsl:mode/>
    <xsl:template name="main" visibility="public">
        <out>
            <xsl:value-of select="$v"/>
        </out>
    </xsl:template>
</xsl:package>
