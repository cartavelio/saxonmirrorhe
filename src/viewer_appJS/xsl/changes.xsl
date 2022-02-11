<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
    xmlns:ixsl="http://saxonica.com/ns/interactiveXSLT"
    xmlns:js="http://saxonica.com/ns/globalJS" 
    xmlns:xs="http://www.w3.org/2001/XMLSchema"
    version="3.0" 
    exclude-result-prefixes="xs ch" 
    extension-element-prefixes="ixsl"
    xmlns:ch="http://www.saxonica.com/ns/doc/changes"
    xmlns:f="urn:viewerapp.function"
    expand-text="yes">
    
    <xsl:variable name="saxonVersions" select="doc(concat($location, '/changes.xml'))/ch:changes/ch:release/@number"
        as="xs:string*"/>
    <xsl:variable name="saxonVersions0" select="($saxonVersions, '6.5.5')"
        as="xs:string*"/>
    
    <!-- 2017-09-11 Added 'ALL' selection option for categories -->
    <!-- 2018-05-03 Reorganise and tidy up -->
    <!-- 2020-03-23 Version number ordering handling fixes -->
    
    <!-- Always enter changes mode by this template, matching the document-node of doc(changes.xml) -->

    <xsl:template match="/" mode="changes" xpath-default-namespace="">
        <xsl:param name="selectedCategory" select="()" as="xs:string?"/>
        <xsl:param name="selectedVersionRange" select="()" as="xs:string?"/>
        
        <xsl:variable name="heading" select="concat('Saxon ', ch:changes/@title)"/>
        <ixsl:set-property name="document.title" select="$heading"/>
        <h1><xsl:value-of select="$heading"/></h1>
        
        <xsl:variable name="fromVersion" as="xs:string" 
            select="if (exists($selectedVersionRange)) then substring-before($selectedVersionRange, '-') else $saxonVersions0[2]"/>
        <xsl:variable name="toVersion" as="xs:string" 
            select="if (exists($selectedVersionRange)) then substring-after($selectedVersionRange, '-') else $saxonVersions0[1]"/>
        
        <!--<xsl:message>fromVersion {$fromVersion} toVersion {$toVersion}</xsl:message>-->
        
        <xsl:variable name="selectedCat" select="($selectedCategory, ch:changes/ch:categories/ch:cat[1]/@name)[1]" as="xs:string"/>

        <p>Details of changes in all Saxon releases since 7.0 are available here.</p>

        <p>Show changes between version <select id="fromList">
                <xsl:for-each select="subsequence($saxonVersions0, 2)">
                    <option>
                        <xsl:if test=". eq $fromVersion">
                            <xsl:attribute name="selected">selected</xsl:attribute>
                        </xsl:if>
                        <xsl:value-of select="."/>
                    </option>
                </xsl:for-each>
            </select> and <select id="toList">
                <xsl:for-each select="subsequence($saxonVersions0, 1, index-of($saxonVersions0,
                    $fromVersion) - 1)">
                    <option>
                        <xsl:if test=". eq $toVersion">
                            <xsl:attribute name="selected">selected</xsl:attribute>
                        </xsl:if>
                        <xsl:value-of select="."/>
                    </option>
                </xsl:for-each>
            </select></p>
        <p>Selected category:
            <select id="catList">
                <xsl:for-each select="ch:changes/ch:categories/ch:cat">
                    <option name="{@name}">
                        <xsl:if test="@name = $selectedCat">
                            <xsl:attribute name="selected">selected</xsl:attribute>
                        </xsl:if>
                        <xsl:value-of select="@title"/>
                    </option>
                </xsl:for-each>
            </select>
        </p>

        <div id="changes-content" style="max-width:1300px">
            <xsl:call-template name="showTable">
                <xsl:with-param name="from" select="$fromVersion"/>
                <xsl:with-param name="to" select="$toVersion"/>
                <xsl:with-param name="catName" select="$selectedCat"/>
            </xsl:call-template>
        </div>


    </xsl:template>

    <xsl:template name="showTable">
        <xsl:param name="from" as="xs:string"/>
        <xsl:param name="to" as="xs:string"/>
        <xsl:param name="catName" as="xs:string"/>
        <!-- context item should be doc(changes.xml) document-node -->
        
        <xsl:variable name="releases" select="ch:changes/ch:release" as="node()*"/>
        <xsl:variable name="revOrderedReleases" as="node()*">
            <xsl:for-each select="$releases">
                <xsl:variable name="versionNo" select="f:version-number(@number)"/>
                <xsl:if test="$versionNo gt f:version-number($from) and $versionNo le f:version-number($to)">
                    <xsl:sequence select="."/>
                </xsl:if>
            </xsl:for-each>
        </xsl:variable>
        <xsl:variable name="orderedReleases" select="reverse($revOrderedReleases)" as="node()*"/>
        
        <xsl:variable name="cats" select="ch:changes/ch:categories/ch:cat" as="node()*"/>
        
        <!--<xsl:message>Show table from <xsl:value-of select="f:version-number($from)"/> to <xsl:value-of
            select="f:version-number($to)"/></xsl:message>-->
        <!--<xsl:message>Show table from <xsl:value-of select="$from"/> to <xsl:value-of select="$to"/>
            Category <xsl:value-of select="$catName"/></xsl:message>-->
        
        <xsl:choose>
            <xsl:when test="$orderedReleases/ch:category[(@name, 'all') = $catName]">             
                <xsl:for-each select="$orderedReleases">
                    <h3>Changes <xsl:if test="following-sibling::*">from <xsl:value-of
                        select="following-sibling::*[1]/@number"/></xsl:if> to <xsl:value-of
                            select="@number"/></h3>
                    <xsl:apply-templates select="ch:head"/>
                    <xsl:choose>
                        <xsl:when test="'all' = $catName">
                            <!-- Work from ch:changes/ch:categories/ch:cat to ensure categories
                                appear in the same order (rather than document order which may be
                                different) -->
                            <xsl:variable name="release" select="."/>
                            <xsl:for-each select="$cats">
                                <xsl:variable name="catNm" select="@name"/>
                                <xsl:if test="$release/ch:category[@name = $catNm]/*">
                                    <h3 class="subtitle"><xsl:value-of select="@title"/></h3>
                                    <xsl:apply-templates select="$release/ch:category[@name = $catNm]/*"/>
                                </xsl:if>
                            </xsl:for-each>
                        </xsl:when>
                        <xsl:when test="./ch:category[@name = $catName]/*">
                            <xsl:apply-templates select="./ch:category[@name = $catName]/*"/>
                        </xsl:when>
                        <xsl:otherwise>
                            <p>(None.)</p>
                        </xsl:otherwise>
                    </xsl:choose>
                </xsl:for-each> 
            </xsl:when>
            <xsl:otherwise>
                <strong>No changes in <i><xsl:value-of select="$cats[@name = $catName]/@title"/></i> 
                    for this range of versions.</strong>
            </xsl:otherwise>
        </xsl:choose>

    </xsl:template>
    
    <xsl:template name="changeChange">
        <xsl:param name="from" as="xs:string"/>
        <xsl:param name="to" as="xs:string"/>
        <xsl:param name="catName" as="xs:string"/>
        <xsl:sequence select="f:set-hash(concat('changes/', $catName, '/', $from, '-', $to))"/>
    </xsl:template>


    <xsl:template match="*:select[@id eq 'fromList']" mode="ixsl:onchange" xpath-default-namespace="">
        <xsl:variable name="fromList" select="ixsl:page()/id('fromList')"/>
        <xsl:variable name="toList" select="ixsl:page()/id('toList')"/>
        <xsl:variable name="catList" select="ixsl:page()/id('catList')"/>
        
        <xsl:variable name="from" select="$fromList/*:option[ixsl:get($fromList, 'selectedIndex') + 1]"
            as="xs:string"/>
        <xsl:variable name="to" select="$toList/*:option[ixsl:get($toList, 'selectedIndex') + 1]"
            as="xs:string"/>
        <xsl:variable name="keepTo" select="f:version-number($from) lt f:version-number($to)"/>
        <xsl:variable name="newTo"
            select="if ($keepTo) then $to else $saxonVersions0[index-of($saxonVersions0, $from) - 1]"
            as="xs:string"/>
        <xsl:variable name="catName" as="xs:string"
            select="$catList/*:option[ixsl:get($catList, 'selectedIndex') + 1]/@name"/>

        <xsl:call-template name="changeChange">
            <xsl:with-param name="from" select="$from"/>
            <xsl:with-param name="to" select="$newTo"/>
            <xsl:with-param name="catName" select="$catName"/>
        </xsl:call-template>
    </xsl:template>

    <xsl:template match="*:select[@id eq 'toList']" mode="ixsl:onchange" xpath-default-namespace="">
        <xsl:variable name="fromList" select="ixsl:page()/id('fromList')"/>
        <xsl:variable name="toList" select="ixsl:page()/id('toList')"/>
        <xsl:variable name="catList" select="ixsl:page()/id('catList')"/>
        
        <xsl:variable name="from" select="$fromList/*:option[ixsl:get($fromList, 'selectedIndex') + 1]"
            as="xs:string"/>
        <xsl:variable name="to" select="$toList/*:option[ixsl:get($toList, 'selectedIndex') + 1]"
            as="xs:string"/>
        <xsl:variable name="catName" as="xs:string"
            select="$catList/*:option[ixsl:get($catList, 'selectedIndex') + 1]/@name"/>

        <xsl:call-template name="changeChange">
            <xsl:with-param name="from" select="$from"/>
            <xsl:with-param name="to" select="$to"/>
            <xsl:with-param name="catName" select="$catName"/>
        </xsl:call-template>
    </xsl:template>
    
    <xsl:template match="*:select[@id eq 'catList']" mode="ixsl:onchange" xpath-default-namespace="">
        <xsl:variable name="fromList" select="ixsl:page()/id('fromList')"/>
        <xsl:variable name="toList" select="ixsl:page()/id('toList')"/>
        <xsl:variable name="catList" select="ixsl:page()/id('catList')"/>
        
        <xsl:variable name="from" select="$fromList/*:option[ixsl:get($fromList, 'selectedIndex') + 1]"
            as="xs:string"/>
        <xsl:variable name="to" select="$toList/*:option[ixsl:get($toList, 'selectedIndex') + 1]"
            as="xs:string"/>
        <xsl:variable name="catName" as="xs:string"
            select="$catList/*:option[ixsl:get($catList, 'selectedIndex') + 1]/@name"/>
        
        <xsl:call-template name="changeChange">
            <xsl:with-param name="from" select="$from"/>
            <xsl:with-param name="to" select="$to"/>
            <xsl:with-param name="catName" select="$catName"/>
        </xsl:call-template>
    </xsl:template>

    <xsl:template match="ch:category">
        <xsl:apply-templates/>
    </xsl:template>

    <xsl:template match="ch:category//*" priority="3">
        <xsl:copy>
            <xsl:copy-of select="@*"/>
            <xsl:apply-templates/>
        </xsl:copy>
    </xsl:template>
    
    <xsl:template match="ch:h2" priority="5">
        <h4><xsl:apply-templates/></h4>
    </xsl:template>
    
    <xsl:template match="ch:h3" priority="5">
        <h5><xsl:apply-templates/></h5>
    </xsl:template>
    
    <xsl:template match="ch:change" priority="5">
        <h4><xsl:value-of select="(@title, 'Change')[1]"/></h4>
        <xsl:apply-templates/>
    </xsl:template>

    <xsl:template match="ch:category//ch:*|ch:head//ch:*" priority="4">
        <xsl:element name="{local-name()}">
            <xsl:copy-of select="@*"/>
            <xsl:apply-templates/>
        </xsl:element>
    </xsl:template>
    
    <xsl:template match="ch:category//*:index" priority="4">
        <xsl:apply-templates/>
    </xsl:template>
    
    <xsl:template match="ch:code[@java]" priority="5">
        <span data-href="{@java}" class="javalink">
            <xsl:copy-of select="@* except @java|@class"/>
            <xsl:apply-templates/>
        </span>
    </xsl:template>
    
    <xsl:template match="ch:code[@cs]" priority="5">
        <span data-href="{@cs}" class="cslink">
            <xsl:copy-of select="@* except @cs|@class"/>
            <xsl:apply-templates/>
        </span>
    </xsl:template>
    
    <xsl:template match="*:xref" priority="5">
        <span data-href="{string-join((concat('/',@section), @page, @subpage), '/')}" class="link">
            <xsl:apply-templates/>
        </span>
    </xsl:template>
    
    <xsl:template match="*:a" priority="5">
        <xsl:choose>
            <xsl:when test="substring(@href, 1, 5) = ('file:','http:')">
                <a class="link" href="{@href}">
                    <xsl:value-of select="."/>
                </a>
            </xsl:when>
            <xsl:otherwise>
                <span class="link {@class}" data-href="{@href}">
                    <xsl:apply-templates select="node()" mode="secondary"/>
                </span>
            </xsl:otherwise>
        </xsl:choose>
    </xsl:template>
    
    <xsl:template match="*:a[@class =('javalink','cslink')]" priority="5">
        <span data-href="{@href}">
            <xsl:copy-of select="@* except @href"/>
            <xsl:apply-templates/>
        </span>
    </xsl:template>
    
    <!-- Function to convert version number string to number, to be used for ordering comparisons. -->
    <xsl:function name="f:version-number" as="xs:double">
        <xsl:param name="versionNo" as="xs:string"/>
        <xsl:variable name="versionNoNS" select="normalize-space($versionNo)"/>
        <xsl:variable name="length" select="string-length($versionNoNS)"/>
        <xsl:choose>
            <xsl:when test="$length ge 5">
                <xsl:variable name="versionNoFix" 
                    select="concat(substring($versionNo, 0, $length - 1), substring($versionNo,
                    $length, 1))"/>
                <xsl:sequence select="number($versionNoFix)"/>
            </xsl:when>
            <xsl:otherwise>
                <xsl:sequence select="number($versionNo)"/>
            </xsl:otherwise>
        </xsl:choose>
    </xsl:function>


</xsl:stylesheet>
