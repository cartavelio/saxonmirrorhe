<?xml version="1.0"?>
<xsl:stylesheet version="2.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">

<!-- Test use of saxon:eval() in a predicate. Was saxon042. -->

    <xsl:template match="/" xmlns:saxon="http://saxon.sf.net/">
    <out>
        <xsl:variable name="gt0" select="saxon:expression('@qty * @price &gt; 0')"/>
        <xsl:variable name="gt15" select="saxon:expression('@qty * @price &gt; 15')"/>
        <xsl:variable name="gt1500" select="saxon:expression('@qty * @price &gt; 1500')"/>        
        <gt0><xsl:value-of select="every $s in //sale satisfies $s[saxon:eval($gt0)]"/></gt0>
        <gt15><xsl:value-of select="every $s in //sale satisfies $s[saxon:eval($gt15)]"/></gt15>
        <gt1500><xsl:value-of select="every $s in //sale satisfies $s[saxon:eval($gt1500)]"/></gt1500>              
    </out>
    </xsl:template>
    <xsl:output indent="yes"/>
</xsl:stylesheet>

