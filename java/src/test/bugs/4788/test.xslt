<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet version="3.0"
                xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
                xmlns:test="http://test.com/extension"
                xmlns:saxon="http://saxon.sf.net/">

    <xsl:template match="/">
        <root>
            <xsl:for-each select="*/*" saxon:threads="32">
                <row test="{test:test(position())}" />
            </xsl:for-each>
        </root>
    </xsl:template>

</xsl:stylesheet>