<xsl:stylesheet exclude-result-prefixes="#all" version="2.0" xmlns:saxon="http://saxon.sf.net/"
    xmlns:xs="http://www.w3.org/2001/XMLSchema" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
    <xsl:output indent="yes"/>
    <xsl:param name="hexBin" select="'FF'"/>
    <xsl:template name="main">
        <out>
            <xsl:variable name="o" select="saxon:hexBinary-to-octets(xs:hexBinary($hexBin))"/>
            <o value="{$o}"
               instance="{$o instance of xs:unsignedByte}"
               castable="{$o castable as xs:unsignedByte}"
               cast="{$o cast as xs:unsignedByte}"/>
        </out>
    </xsl:template>
</xsl:stylesheet>
