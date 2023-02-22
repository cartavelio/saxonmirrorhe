<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet version="3.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
    xmlns:map="http://www.w3.org/2005/xpath-functions/map"
    xmlns:xs="http://www.w3.org/2001/XMLSchema" xmlns:saxon="http://saxon.sf.net/"
    xmlns:k="http://saxon.sf.net/keymap"
    exclude-result-prefixes="map xs saxon k">

    <!-- Basic test of saxon:key-map -->

    <xsl:output method="xml" indent="yes" saxon:indent-spaces="4"/>

    <xsl:key name="k:trans-date" match="transaction" use="string(@date)" saxon:range-key="yes"/>
    
    <xsl:param name="kLexical">k:trans-date</xsl:param>
    <xsl:param name="kEQName">Q{http://saxon.sf.net/keymap}trans-date</xsl:param>

    <xsl:template match="/" name="main">
        <out>
            <xsl:call-template name="a"/>
            <xsl:call-template name="b"/>
            <xsl:call-template name="c"/>
            <xsl:call-template name="d"/>
            <xsl:call-template name="e"/>
            <xsl:call-template name="f"/>
            <xsl:call-template name="g"/>
            <xsl:call-template name="h"/>
        </out>
    </xsl:template>

    <xsl:template name="a">
        <a>
            <xsl:variable name="map"
                select="saxon:key-map('k:trans-date', doc('../../docs/transactions.xml'), 
                '2006-02-20', '2006-02-23')"/>
            <xsl:value-of select="map:size($map)"/>
        </a>
    </xsl:template>

    <xsl:template name="b">
        <b>
            <xsl:variable name="map"
                select="saxon:key-map($kLexical, doc('../../docs/transactions.xml'), 
                (), '2006-02-23')"/>
            <xsl:value-of select="map:keys($map)" separator=","/>
        </b>
    </xsl:template>

    <xsl:template name="c">
        <c>
            <xsl:variable name="map"
                select="saxon:key-map($kEQName, doc('../../docs/transactions.xml'), 
                '2006-02-20', ())"/>
            <xsl:value-of select="map:keys($map)" separator=","/>
        </c>
    </xsl:template>

    <xsl:template name="d">
        <d>
            <xsl:variable name="map"
                select="saxon:key-map('Q{http://saxon.sf.net/keymap}trans-date', doc('../../docs/transactions.xml'), 
            '2006-02-20', '2006-02-23')"/>
            <xsl:copy-of select="map:keys($map)!$map(.)"/>
        </d>
    </xsl:template>

    <xsl:template name="e">
        <e>
            <xsl:variable name="map"
                select="saxon:key-map('k:trans-date', doc('../../docs/transactions.xml'), 
                '2006-02-20', '2006-02-23')"/>
            <xsl:variable name="map2" select="map:merge(($map, map{'1900-01-01' : 'happy new century'}))"/>
            <xsl:copy-of select="map:get($map2, '1900-01-01')"/>
        </e>
    </xsl:template>
    
    <xsl:template name="f">
        <f>
            <xsl:variable name="map"
                select="saxon:key-map('k:trans-date', doc('../../docs/transactions.xml'), 
                '2006-02-20', '2006-02-23')"/>
            <xsl:for-each select="map:keys($map)">
                <group date="{.}">
                    <xsl:copy-of select="$map(.)"/>
                </group>
            </xsl:for-each>
        </f>
    </xsl:template>
    
    <xsl:template name="g">
        <g>
            <!-- TODO: order of keys() after a map:remove() is not well defined -->
            <xsl:variable name="map"
                select="saxon:key-map('k:trans-date', doc('../../docs/transactions.xml'), 
                '2006-02-20', '2006-02-23')"/>
            <xsl:variable name="map2" select="map:remove($map, '2006-02-22')"/>
            <xsl:copy-of select="map:keys($map2)!$map2(.)"/>
        </g>
    </xsl:template>
    
    <xsl:template name="h">
        <h>
            <xsl:variable name="map"
                select="saxon:key-map('k:trans-date', doc('../../docs/transactions.xml'), 
                '2006-02-20', '2006-02-23')"/>
            <xsl:sequence select="map:for-each($map, function($k, $v) { $k || '=(' || string-join($v/@value, '+') || ')'})"/>
        </h>
    </xsl:template>

</xsl:stylesheet>
