<xsl:stylesheet version="2.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
    xmlns:xs="http://www.w3.org/2001/XMLSchema"
    xmlns:f="http://saxonica.com/ns/unicode"
    xpath-default-namespace="http://www.unicode.org/ns/2003/ucd/1.0"
    exclude-result-prefixes="xs f"
    >
    
    <!-- Output a list of the start and end points of contiguous ranges of characters
     classified as letters or digits.

     Note this doesn't handle the CJK Extended Ideograph ranges A and B, 3400-4DB5 and 20000-2A6D6,
     which have to be edited in by hand. Also 4E00-9FBB and AC00-D7A3
     
     This version modified 2018-05-10 by MHK to handle the format in ucd.all.flat.xml found in Unicode 10.0.0 data files
-->
    
    <xsl:output method="text"/>
    <xsl:param name="data" select="doc('ucd.all.flat.xml')"/>
    
    <xsl:function name="f:isAlphaNum" as="xs:boolean">
        <xsl:param name="char" as="element(char)"/>
        <xsl:sequence select="$char/@gc=('Nd', 'Nl', 'No', 'Lu', 'Ll', 'Lt', 'Lm', 'Lo')"/>
    </xsl:function>
    
    <xsl:function name="f:hexToInt" as="xs:integer?">
        <xsl:param name="hex" as="xs:string?"/>
        <xsl:sequence select="if (empty($hex)) then () else Integer:parseInt($hex, 16)"
            xmlns:Integer="java:java.lang.Integer"/>
    </xsl:function>
    
    <xsl:param name="p"/>
    <xsl:template name="test">
        <xsl:value-of select="f:hexToInt($p)"/>
    </xsl:template>
    
    <xsl:template name="main">
        
        <xsl:text>int[] startPoints = new int[]{</xsl:text>
        <xsl:for-each-group select="$data/ucd/repertoire/char" group-adjacent="concat(f:isAlphaNum(.), f:hexToInt(@cp) - position())">
            <xsl:if test="f:isAlphaNum(.)">
                <xsl:text>0x</xsl:text>
                <xsl:value-of select="current-group()[1]/@cp"/>
                <xsl:text>, </xsl:text>
            </xsl:if>
        </xsl:for-each-group>
        <xsl:text>};&#xa;&#xa;</xsl:text>
        <xsl:text>int[] endPoints = new int[]{</xsl:text>
        <xsl:for-each-group select="$data/ucd/repertoire/char" group-adjacent="concat(f:isAlphaNum(.), f:hexToInt(@cp) - position())">
            <xsl:if test="f:isAlphaNum(.)">
                <xsl:text>0x</xsl:text>
                <xsl:value-of select="current-group()[last()]/@cp"/>
                <xsl:text>, </xsl:text>
            </xsl:if>
        </xsl:for-each-group>
        <xsl:text>};&#xa;&#xa;</xsl:text>
        
    </xsl:template>
    
    
</xsl:stylesheet>