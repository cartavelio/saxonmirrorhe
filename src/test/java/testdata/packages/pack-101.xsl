<xsl:package name="http://package-one" package-version="2.1.0.5" version="3.0"
    xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
    <xsl:use-package name="http://package-two/" package-version="*"/>
    <xsl:param name="c" select="2"/>
    <xsl:mode/>
    <xsl:template name="main" visibility="public">
        <out>
            <xsl:value-of select="$v"/>
        </out>
    </xsl:template>
</xsl:package>
