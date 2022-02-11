<?xml version="1.0"?>
<xsl:stylesheet version="2.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
 xmlns:f="http://local-functions/"
 xmlns:xs="http://www.w3.org/2001/XMLSchema" 
 exclude-result-prefixes="f xs">

<!-- test higher-order functions saxon:lowest(), saxon:highest() -->
<!-- formerly saxon039 -->

<xsl:output indent="yes"/>

    <xsl:template match="/" xmlns:saxon="http://saxon.sf.net/" exclude-result-prefixes="saxon">
    <out>
        <xsl:variable name="exp" select="f:value#1"/>
        <min><xsl:value-of select="saxon:lowest(//sale, $exp)/@id"/></min>
        <max><xsl:value-of select="saxon:highest(//sale, $exp)/@id"/></max>
        <minprice><xsl:value-of select="saxon:lowest(//sale/@price)/../@id"/></minprice>
        <maxprice><xsl:value-of select="saxon:highest(//sale/@price)/../@id"/></maxprice>
    </out>
    </xsl:template>
    
    <xsl:function name="f:value" as="xs:double">
      <xsl:param name="node" as="element()"/>
      <xsl:sequence select="$node/@qty * $node/@price"/>
    </xsl:function>
</xsl:stylesheet>

