<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
    xmlns:xs="http://www.w3.org/2001/XMLSchema" exclude-result-prefixes="xs" version="2.0">
    
    <!-- Stylesheet used by contentsDoc.xsl and doc-page.xsl to build the contents tables 
        (with direct links to the main sections of the relevant version of the documentation), 
        common to these pages -->

    <xsl:variable name="doc-ref" select="document('doc-ref.xml')"/>

    <xsl:template name="doc-contents-table">
        <xsl:param name="method"/>
        <xsl:param name="version-number"/>
        <xsl:variable name="extension">
            <xsl:if test="$method = 'dynamic'">
                <xsl:value-of select="'xml'"/>
            </xsl:if>
            <xsl:if test="$method = 'static'">
                <xsl:value-of select="'html'"/>
            </xsl:if>
        </xsl:variable>
        <xsl:variable name="version-ref" select="$doc-ref/*/version[@number = $version-number]"/>
        <xsl:variable name="doc-version" select="$version-ref/@dir"/>
        <xsl:variable name="doc-entry">
            <xsl:if test="$method = 'dynamic'">
                <xsl:value-of select="$version-ref/dynamic-page/@doc-entry"/>
            </xsl:if>
            <xsl:if test="$method = 'static'">
                <xsl:value-of select="$version-ref/static-page/@doc-entry"/>
            </xsl:if>
        </xsl:variable>
        <xsl:variable name="doc-extension">
            <xsl:choose>
                <xsl:when test="$method = 'dynamic'">
                    <xsl:value-of select="''"/>
                </xsl:when>
                <xsl:when test="$method = 'static'">
                    <xsl:value-of select="'/index.html'"/>
                </xsl:when>
            </xsl:choose>
        </xsl:variable>
        <xsl:variable name="contents-doc" select="concat($doc-version,'/contents.xml')"/>
        <h2>Documentation</h2>
        <table class="doc-contents">
            <!--<tr id="top">
                <td>
                    <p>
                        <a href="documentation.{$extension}">Documentation</a>
                    </p>
                </td>
            </tr>-->
            <tr>
                <td>
                    <p>
                        <a href="contents-page.{$extension}">Full Contents</a>
                    </p>
                </td>
            </tr>
            <xsl:for-each select="document(concat('../build/website/',$contents-doc))/*/section">
                <tr>
                    <td>
                        <p>
                            <a href="{$doc-entry}{@name}{$doc-extension}">
                                <xsl:value-of select="@title"/>
                            </a>
                        </p>
                    </td>
                </tr>
            </xsl:for-each>
        </table>
    </xsl:template>

</xsl:stylesheet>
