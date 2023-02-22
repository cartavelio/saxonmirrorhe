<xsl:stylesheet  version="2.0"
    xmlns:xsl="http://www.w3.org/1999/XSL/Transform">

    <xsl:import href="B.xsl"/>

    <xsl:template match="/" name="main">
        <xsl:result-document method="xml">
            <foo>bar</foo>
        </xsl:result-document>
    </xsl:template>

</xsl:stylesheet>