<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
    xmlns:xs="http://www.w3.org/2001/XMLSchema"
    exclude-result-prefixes="xs"
    version="2.0">
       
    <xsl:import href="make-menu.xsl"/>
    
    <xsl:variable name="homepage" select="'../welcome/welcome.html'"/>    
    
    <xsl:template match="/">
        <xsl:apply-imports />
    </xsl:template>
    
    <xsl:template match="a">
        <xsl:copy>            
            <xsl:apply-templates select="@*,node()"/>            
        </xsl:copy>
    </xsl:template>
    
    <xsl:template match="@*">
        <xsl:copy> 
        
        </xsl:copy>
    </xsl:template>
    
    <xsl:template match="@href">
        <xsl:choose>
            <xsl:when test="contains(.,'user-doc/1.1/index.html')">
                <xsl:attribute name="href" select="parent::a/@id" />
            </xsl:when>
            <xsl:when test="contains(.,'saxon-js/documentation/index.html')">
                <xsl:attribute name="href" select="." />
            </xsl:when>
           <xsl:when test="contains(.,'saxon-c/documentation/index.html')">
              <xsl:attribute name="href" select="." />
           </xsl:when>
            <xsl:when test="contains(.,'xqts100/results100-88.xml') or contains(.,'xslts1.0.4/published-results8.8.xml')
                or contains(.,'http://www.xml') or contains(.,'.xml.') or contains(.,'shop.html#!') or contains(.,'.xmlprague')">
                <xsl:attribute name="href" select="." />
            </xsl:when>
            <xsl:otherwise>
                <xsl:variable name="extension">
                    <xsl:if test="count(tokenize(tokenize(.,'#!')[2],'/')) gt 1 and not(contains(parent::a/@class,'has-subpages'))">
                        <xsl:value-of select="'.html'"/>
                    </xsl:if>
                    <xsl:if test="count(tokenize(tokenize(.,'#!')[2],'/')) eq 1 or contains(parent::a/@class,'has-subpages')">
                        <xsl:value-of select="'/index.html'"/>
                    </xsl:if>
                </xsl:variable>
                <xsl:choose>
                    <xsl:when test="contains(.,'index.html#!')">
                        <xsl:attribute name="href" select="concat(replace(., 'index.html#!', ''), $extension)" />
                    </xsl:when>
                    <xsl:when test="contains(.,'documentation9.5/#!')">
                        <xsl:attribute name="href" select="concat(replace(., 'documentation9.5/#!', '../documentation9.5/'), $extension)" />
                    </xsl:when>
                    <xsl:when test="contains(.,'/#!')">
                        <xsl:attribute name="href" select="concat(replace(., '/#!', '/'), $extension)" />
                    </xsl:when>
                    <xsl:when test="contains(.,'#!')">
                        <xsl:attribute name="href" select="concat(replace(., '#!', '/'), $extension)" />
                    </xsl:when>
                    <xsl:otherwise>
                        <xsl:attribute name="href" select="replace(., '\.xml', '.html')" />
                    </xsl:otherwise>
                </xsl:choose>
            </xsl:otherwise>
        </xsl:choose>        
    </xsl:template>    
    
    <xsl:template mode="menu-area" match="*">
        <xsl:next-match>
            <xsl:with-param name="extension" select="'html'"></xsl:with-param>
        </xsl:next-match>
    </xsl:template>
    
    <xsl:template match="div[@id='sitemap']">
        <xsl:next-match>
            <xsl:with-param name="extension" select="'html'"></xsl:with-param>
        </xsl:next-match>
    </xsl:template>
            
</xsl:stylesheet>
