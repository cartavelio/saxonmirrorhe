<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet version="2.0" 
        xmlns:xsl="http://www.w3.org/1999/XSL/Transform" 
        xmlns:xs="http://www.w3.org/2001/XMLSchema" 
        xmlns:xdt="http://www.w3.org/2005/xpath-datatypes" 
        xmlns:xfn="http://www.w3.org/2005/xpath-functions"
        xmlns:saxon="http://saxon.sf.net/" extension-element-prefixes="saxon" 
        xmlns:h="http://www.xyz.com/h"
        exclude-result-prefixes="h xsl xs xdt xfn saxon"
>
<xsl:output method="xml" version="1.0" encoding="UTF-8" indent="yes"/> 

<!-- A more complex example of saxon:evaluate() with multiple parameters --> 

<xsl:variable name="resource">
    <res>
        <resgrp>
                <thing id="a" color="red" size="big" weight="heavy"/>
                <thing id="b" color="green" size="any" weight="any"/>
                <thing id="c" color="blue" size="small" weight="light"/>
                <thing id="d" color="red" size="small" weight="heavy"/>
                <thing id="e" color="green" size="small" weight="heavy"/>
                <thing id="f" color="blue" size="big" weight="heavy"/>
                <thing id="g" color="red" size="big" weight="light"/>
                <thing id="h" color="green" size="small" weight="light"/>
                <thing id="i" color="blue" size="small" weight="heavy"/>
        </resgrp>
    </res>
</xsl:variable>

<xsl:template name="main">
    <xsl:variable name="queryseq">
        <aa>red big heavy</aa>
        <aa>blue small light</aa>
        <aa>red small light</aa>
        <aa>green big light</aa>
    </xsl:variable>
    <out>
    <xsl:for-each select="$queryseq/*">
        <xsl:variable name="parms" select="."/>
        <xsl:copy-of select="h:getresource('//thing[@color = $p1 and @size = $p2 and @weight = $p3]', '@id', $parms)//@id"/>
    </xsl:for-each>
    </out>
</xsl:template>

<xsl:function name="h:getresource">
    <xsl:param name="component"/>
    <xsl:param name="item"/>
    <xsl:param name="params"/>
    <xsl:variable name="para" select="xfn:tokenize($params, ' ')"/>
    <xsl:variable name="xpath-str"><xsl:value-of select="concat('.', $component)"/></xsl:variable>
    <res>
        <xsl:for-each select="$resource/res">
            <xsl:for-each select="saxon:evaluate($xpath-str, $para[1], $para[2], $para[3], $para[4], $para[5], $para[6], $para[7], $para[8], $para[9])">
                 
                <xsl:choose>
                    <xsl:when test="string-length($item)">
                        <item><xsl:copy-of select="saxon:evaluate($item)"/></item>
                    </xsl:when>
                    <xsl:otherwise><xsl:copy-of select="."/></xsl:otherwise>
                </xsl:choose>
            </xsl:for-each>
        </xsl:for-each>
    </res>
</xsl:function>

</xsl:stylesheet>
