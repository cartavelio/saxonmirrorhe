<xsl:transform xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
    xmlns:xs="http://www.w3.org/2001/XMLSchema"
    xmlns:fc="http://www.saxonica.com/ns/doc/functions"
    version="3.0"
    xmlns:ixsl="http://saxonica.com/ns/interactiveXSLT"
    exclude-result-prefixes="xs fc"
    extension-element-prefixes="ixsl"
    expand-text="yes">

    <xsl:template match="section|article" mode="primary">
        <xsl:variable name="title" select="(h1|h2|h3)[1]"/>
        <xsl:if test="exists($title)">
            <ixsl:set-property name="document.title" select="concat('Saxon ', $title)"/>
        </xsl:if>
        <xsl:apply-templates select="." mode="post-primary"/>
    </xsl:template>

    <xsl:template match="node()|@*" mode="secondary">
        <xsl:copy>
            <xsl:apply-templates select="node()|@*" mode="secondary"/>
        </xsl:copy>
    </xsl:template>
    
    <xsl:template match="section[@class]|article[@class]" mode="post-primary">
        <div class="{@class}">
            <xsl:apply-templates mode="secondary"/>
        </div>
    </xsl:template>

    <xsl:template match="section|article" mode="post-primary">
        <xsl:apply-templates mode="secondary"/>
    </xsl:template>
    
    <xsl:template match="section|article" mode="secondary"/>
    
    
    <xsl:template match="article[@id='functions']|article[@id='functions']/section" mode="primary" priority="10">
        <xsl:apply-templates select="." mode="function-list"/>
        <!-- See functions-data.xsl -->
    </xsl:template>

    <xsl:template match="dfn" mode="secondary">
        <xsl:apply-templates mode="secondary"/>
    </xsl:template>

    <xsl:template match="nav[ul]" mode="secondary">
        <xsl:variable name="except" select="tokenize(@except, ' ')"/>
        <div>
            <xsl:if test="@columns">
                <xsl:attribute name="class" select="'columns'"/>
            </xsl:if>
            <ul>
                <xsl:for-each select="../(article|section)[not($except = @id)]">
                    <li>
                        <p>
                            <span class="link" data-href="{@id}">
                                <xsl:value-of select="@title"/>
                            </span>
                            <xsl:if test="@summary">
                                <xsl:value-of select="':', @summary" separator=" "/>
                            </xsl:if>
                        </p>
                    </li>
                </xsl:for-each>
            </ul>
        </div>
    </xsl:template>

    <xsl:template match="img[@src]" mode="secondary">
        <xsl:variable name="src" select="if (starts-with(@src, '/')) then substring(@src, 2) else ."/>
        <img src="{$src}">
            <xsl:apply-templates select="(@* except @src)" mode="secondary"/>
            <xsl:apply-templates/>
        </img>
    </xsl:template>

    <xsl:template match="a[@class = 'apilink']" mode="secondary">
        <span class="link {@class}" data-href="{@href}">
            <xsl:apply-templates select="node()" mode="secondary"/>
        </span>
    </xsl:template>

    <xsl:template match="a[@class =('javalink','cslink')]" mode="secondary">
        <span data-href="{@href}">
            <xsl:copy-of select="@* except @href"/>
            <xsl:apply-templates/>
        </span>
    </xsl:template>
    
    <xsl:template match="code[@java]" mode="secondary">
        <span data-href="{@java}" class="javalink">
            <xsl:copy-of select="@* except (@java,@class)"/>
            <xsl:apply-templates/>
        </span>
    </xsl:template>
    
    <xsl:template match="code[@cs]" mode="secondary">
        <span data-href="{@cs}" class="cslink">
            <xsl:copy-of select="@* except (@cs,@class)"/>
            <xsl:apply-templates/>
        </span>
    </xsl:template>

    <xsl:template match="a" mode="secondary">
        <xsl:choose>
            <xsl:when
                test="substring(@href, 1, 5) = ('file:','http:') or starts-with(@href, 'https:')">
                <xsl:copy-of select="."/>
            </xsl:when>
            <xsl:when test="substring-after(@href, '.') = ('zip','pdf','txt','xsl','xsd','xml')">
                <xsl:variable name="href"
                    select="if (starts-with(@href, '/')) then substring(@href, 2) else @href"/>
                <a href="{$href}">
                    <xsl:apply-templates select="@* except @href" mode="secondary"/>
                    <xsl:apply-templates/>
                </a>
            </xsl:when>
            <xsl:when test="ends-with(@href ,'..')">
                <xsl:variable name="levels" select="count(tokenize(@href,'/'))"/>
                <span class="link {@class}" data-href="{string-join(for $l in 1 to ($levels+1) return '../','')}{ancestor::section[$levels+1]/@id}">
                    <xsl:apply-templates select="node()" mode="secondary"/>
                </span>
            </xsl:when>
            <xsl:otherwise>
                <span class="link {@class}" data-href="{@href}">
                    <xsl:apply-templates select="node()" mode="secondary"/>
                </span>
            </xsl:otherwise>
        </xsl:choose>
    </xsl:template>

</xsl:transform>
