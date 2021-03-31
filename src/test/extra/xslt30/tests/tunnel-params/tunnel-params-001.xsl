<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
    xmlns:xs="http://www.w3.org/2001/XMLSchema"
    xmlns:v="http://www.v/"
    exclude-result-prefixes="xs v"
    version="3.0">
    
    <xsl:template name="main">
        <out>
            <xsl:call-template name="two">
                <xsl:with-param name="p1" select="1" tunnel="yes"/>
                <xsl:with-param name="v:par1" select="2" tunnel="yes"/>
            </xsl:call-template>
        </out>
    </xsl:template>
    
    <xsl:template name="two">
        <xsl:call-template name="three">
            <xsl:with-param name="p1" select="5" tunnel="yes"/>
            <xsl:with-param name="v:par2" select="2" tunnel="yes"/>
            <xsl:with-param name="p7" select="7" tunnel="yes"/>
        </xsl:call-template>
    </xsl:template>
    
    <xsl:template name="three" xmlns:map="http://www.w3.org/2005/xpath-functions/map" exclude-result-prefixes="map">
        <xsl:variable name="params" select="Q{http://saxon.sf.net/}tunnel-params()" as="map(xs:QName, item()*)"/>
        <count c="{map:size($params)}"/>
        <p1 value="{$params(QName('', 'p1'))}"/>
        <p7 value="{$params(QName('', 'p7'))}"/>
        <par1 value="{$params(QName('http://www.v/', 'par1'))}"/>
        <par2 value="{$params(QName('http://www.v/', 'par2'))}"/>
    </xsl:template>
    
</xsl:stylesheet>