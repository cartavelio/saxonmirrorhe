<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
    xmlns:xs="http://www.w3.org/2001/XMLSchema"
    xmlns:ch="http://www.saxonica.com/ns/doc/changes"
    exclude-result-prefixes="xs ch"
    version="3.0">
    
    <xsl:output indent="yes"/>
    
    <!-- converts the changes.xml document to a regular article, so the standard article rendering code can be used -->
    
    <xsl:template match="/">
        <xsl:apply-templates select="*" mode="makepage"/>
    </xsl:template>
    
    <xsl:template match="ch:changes" mode="makepage">
        <xsl:message>Processing changes</xsl:message>
        <article id="changes" title="Change history">
            <h1>Change history</h1>
            <p>In this section you will find a complete list of changes made to the Saxon product since release 7.0, organized in reverse
                order of releases.</p>
            <nav>
                <ul/>
            </nav>
            <xsl:apply-templates mode="ch"/>
        </article>
    </xsl:template>
    
    <xsl:template match="ch:categories" mode="ch"/>
    
    <xsl:template match="ch:release" mode="ch">
        <section id="v{@number}" title="Changes in release {@number}">
            <h1>Changes in release <xsl:value-of select="@number"/> (<xsl:value-of select="format-date(@date, '[D] [MNn] [Y]')"/>)</h1>
            <xsl:apply-templates select="ch:head" mode="ch"/>
            <nav>
                <ul/>
            </nav>
            <!-- Order categories as in ch:categories/ch:cat -->
            <xsl:variable name="release" select="." as="element(ch:release)"/>
            <xsl:variable name="cats" select="/ch:changes/ch:categories/ch:cat" as="node()*"/>
            <xsl:for-each select="$cats">
                <xsl:apply-templates select="$release/ch:category[@name=current()/@name]" mode="ch"/>
            </xsl:for-each>
            <!-- Produce warning for any ch:category with name which does not match that of a
                ch:categories/ch:cat -->
            <xsl:for-each select="$release/ch:category">
                <xsl:if test="not(@name = $cats/@name)">
                    <xsl:message expand-text="yes">Warning: release {$release/@number} category unrecognised: {@name}</xsl:message>
                </xsl:if>
            </xsl:for-each>
        </section>
    </xsl:template>
    
    <xsl:template match="ch:category" mode="ch">
        <section id="{@name}" title="{/ch:changes/ch:categories/ch:cat[@name=current()/@name]/@title}">
            <h1><xsl:value-of select="/ch:changes/ch:categories/ch:cat[@name=current()/@name]/@title"/></h1>
            <xsl:apply-templates mode="ch"/>
        </section>
    </xsl:template>
    
    <xsl:template match="ch:change[@title]" mode="ch">
        <h2><xsl:value-of select="@title"/></h2>
        <xsl:apply-templates mode="ch"/>
    </xsl:template>
    
    <xsl:template match="ch:change" mode="ch">
        <xsl:apply-templates mode="ch"/>
    </xsl:template>
    
    <xsl:template match="*:index" mode="ch">
        <xsl:apply-templates mode="ch"/>
    </xsl:template>
    
    <xsl:template match="*:xref" mode="ch">
        <xsl:choose>
            <xsl:when test="@section eq 'changes'">
                <a href="{concat('/', @section, '/v', substring-after(@subpage, '-'), '/', @page)}"
                    class="bodylink">
                    <xsl:apply-templates mode="ch"/>
                </a>
            </xsl:when>
            <xsl:otherwise>
                <a href="{string-join((concat('/', @section), @page, @subpage), '/')}" class="bodylink">
                    <xsl:apply-templates mode="ch"/>
                </a>
            </xsl:otherwise>
        </xsl:choose>
    </xsl:template>
    
    <xsl:template match="*" mode="ch">
        <xsl:element name="{local-name()}">
            <xsl:copy-of select="@*"/>
            <xsl:apply-templates mode="ch"/>
        </xsl:element>
    </xsl:template>    
    
    <xsl:template match="ch:code[@java]" mode="ch">
        <a href="{@java}" class="javalink">
            <xsl:copy-of select="@* except @java|@class"/>
            <xsl:apply-templates/>
        </a>
    </xsl:template>
    
    
    
</xsl:stylesheet>