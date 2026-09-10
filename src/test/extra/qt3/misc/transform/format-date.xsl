<xsl:stylesheet version="2.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
    xmlns:xs="http://www.w3.org/2001/XMLSchema">    
    <xsl:template name='main'>
        <out>
            <xsl:value-of select='format-date(xs:date("2015-03-12Z"), "[Dwo] [MNn]")' />
        </out>
    </xsl:template> 
</xsl:stylesheet>