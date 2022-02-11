<xsl:transform xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
    xmlns:ixsl="http://saxonica.com/ns/interactiveXSLT" 
    xmlns:xs="http://www.w3.org/2001/XMLSchema"
    xmlns:js="http://saxonica.com/ns/globalJS"
    xmlns:fnd="http://www.saxonica.com/ns/doc/functions"
    xmlns:svg="http://www.w3.org/2000/svg" 
    xmlns:cat="http://www.saxonica.com/ns/doc/catalog"
    xmlns:ch="http://www.saxonica.com/ns/doc/changes" 
    xmlns:f="urn:viewerapp.function"
    extension-element-prefixes="ixsl"
    version="3.0"
    exclude-result-prefixes="#all"
    expand-text="yes">
    
    <xsl:import href="body.xsl"/>
    <!-- Link checker not yet implemented for viewer_appJS -->
    <!--<xsl:import href="checklinks.xsl"/>-->
    <xsl:import href="findtext.xsl"/>
    <xsl:import href="app-functions.xsl"/>
    <xsl:import href="functions-body.xsl"/>
    <xsl:import href="functions-data.xsl"/>
    <xsl:import href="jdp-body.xsl"/>
    <xsl:import href="jdc-body.xsl"/>
    <xsl:import href="changes.xsl"/>
    
    <xsl:param name="product" select="'Saxon'" as="xs:string"/>
    <xsl:param name="SEFbuildDate" select="'2020-08-24'" as="xs:string" static="yes"/>
    <xsl:param name="showStatusMessage" select="false()" as="xs:boolean"/>
  
    <xsl:variable name="location" select="resolve-uri('doc', ixsl:location())"/>
    <xsl:variable name="implement-path" select="concat($location, '/implement.xml')"/>
    
    <xsl:variable name="jd-path" select="resolve-uri('javadoc-xml/javadoc-tree.xml', $location)"/>
    <xsl:variable name="nd-path" select="resolve-uri('dotnetdoc-xml/tree.xml', $location)"/>
    <xsl:variable name="jd-search" select="resolve-uri('javadoc-xml/javadoc-types.xml', $location)"/>
    <xsl:variable name="nd-search" select="resolve-uri('dotnetdoc-xml/types.xml', $location)"/>
    <xsl:variable name="jpackage-path" select="resolve-uri('javadoc-xml/javadoc-packages.xml', $location)"/>
    <xsl:variable name="npackage-path" select="resolve-uri('dotnetdoc-xml/packages.xml', $location)"/>
    <xsl:variable name="jclass-path" select="string(resolve-uri('javadoc-xml/packages/', $location))"/>
    <xsl:variable name="nclass-path" select="string(resolve-uri('dotnetdoc-xml/packages/', $location))"/>
    
    <xsl:variable name="navlist" as="node()" select="ixsl:page()/html/body/div/div[@id = 'nav']"/>
    
    <!-- $args used by show-tools (link checker) -->
    <xsl:variable name="args" select="f:parse-uri()"/>
    
    <xsl:template name="main">
        <xsl:variable name="catDocName" select="concat($location, '/catalog.xml')"/>
        
        <xsl:if test="$showStatusMessage">
            <xsl:message expand-text="yes">{$SEFbuildDate} {$product} documentation app running with Saxon-JS {ixsl:eval('SaxonJS.getProcessorInfo()["productVersion"]')}</xsl:message>
        </xsl:if>
        <ixsl:schedule-action document="{$catDocName}">
            <xsl:call-template name="list">
                <xsl:with-param name="catDocName" select="$catDocName"/>
            </xsl:call-template>
        </ixsl:schedule-action>
        
        <ixsl:schedule-action document="{$implement-path}">
            <!-- This ensures that implement.xml has been loaded, and so future calls to
                doc($implement-path) are fine -->
            <xsl:call-template name="get-implement-doc"/>
        </ixsl:schedule-action>
    </xsl:template>
    
    <xsl:template name="list"> 
        <!-- Called from within ixsl:schedule-action document={$docName} -->
        <xsl:param name="catDocName"/>
        <xsl:result-document href="#list" method="ixsl:replace-content">
            <xsl:apply-templates
                select="doc($catDocName)/cat:catalog/cat:section"/>
        </xsl:result-document>
        
        <ixsl:schedule-action wait="1">
            <xsl:call-template name="init"/>
        </ixsl:schedule-action>
    </xsl:template>
    
    <xsl:template match="cat:section">
        <li class="closed" data-part="{@ref}">
            <span class="item">
                <xsl:value-of select="."/>
            </span>
        </li>
    </xsl:template>
    
    <xsl:template name="get-implement-doc">
        <xsl:sequence select="doc($implement-path)[current-date() lt xs:date('2000-01-01')]"/>
    </xsl:template>
    
    <xsl:template name="init">
        <!--<xsl:call-template name="show-tools"/>-->
        <xsl:call-template name="process-hashchange"/>
    </xsl:template>


    <xsl:template match="." mode="ixsl:onhashchange">
        <xsl:call-template name="process-hashchange"/>
    </xsl:template>

    <!-- Arrow keys for navigation, and return key for search -->
    <xsl:template match="." mode="ixsl:onkeydown">
        <xsl:variable name="event" select="ixsl:event()"/>
        <xsl:variable name="key" select="ixsl:get($event, 'key')" as="xs:string"/>
        <xsl:variable name="class"
            select="if ($key = ('ArrowLeft', 'Left', 'PageUp')) then 'arrowLeft'
            else if ($key = ('ArrowRight', 'Right', 'PageDown')) then 'arrowRight'
            else if ($key eq 'Enter') then 'enter' else ()"/>
        <xsl:if test="exists($class)">
            <!--<xsl:message>keydown <xsl:value-of select="$key"/></xsl:message>-->
            <xsl:choose>
                <xsl:when test="$class eq 'enter'">
                    <xsl:call-template name="run-search"/>
                </xsl:when>
                <xsl:otherwise>
                    <xsl:sequence select="f:navpage($class)"/>
                </xsl:otherwise>
            </xsl:choose>
        </xsl:if>
    </xsl:template>
    
    <xsl:template match="p[@class eq 'arrowNone']" mode="ixsl:onclick">
        <xsl:for-each select="$navlist/ul/li">
            <ixsl:set-attribute name="class" select="'closed'"/>
        </xsl:for-each>
    </xsl:template>
    
    <!-- Previously svg:g[@data-href] too, but can't find use anymore (2018-05-02) -->
    <xsl:template match="span[@data-href]" mode="ixsl:onclick">
        <xsl:sequence select="f:anchor-navigation(.)"/>
    </xsl:template>
    
    <xsl:template name="process-hashchange">
        <xsl:variable name="hash-parts" select="tokenize(f:get-hash(),'/')"/>
        <xsl:variable name="start" select="$hash-parts[1]"/>
        <xsl:variable name="docName"
            select="if ($start eq 'javadoc') then $jd-path 
            else if ($start eq 'dotnetdoc') then $nd-path 
            else concat($location, '/', $start,'.xml')"/>
        
        <!-- For javadoc and dotnetdoc, fetch multiple documents  -->
        <xsl:variable name="apidoc" select="$start = ('javadoc', 'dotnetdoc')"/>
        <xsl:variable name="class-path" select="if ($apidoc) then (if ($hash-parts[1] eq 'javadoc') then
            $jclass-path else $nclass-path) else ()" as="xs:string?"/>
        <xsl:variable name="docpath" select="if ($apidoc and count($hash-parts) gt 1) then
            concat($class-path, $hash-parts[2],'.xml') else ()" as="xs:string?"/>
        
        <xsl:variable name="docList"
            select="if ($start eq 'javadoc') 
            then string-join(($jd-path, $jpackage-path, $docpath), ' ') 
            else if ($start eq 'dotnetdoc') 
            then string-join(($nd-path, $npackage-path, $docpath), ' ')
            else $docName"/>
        
        <!-- For some pages, the content takes a while to load, so adding a message may be friendly -->
        <xsl:result-document href="#main" method="ixsl:replace-content">
            <p>&#160;</p>
            <p><i>Please wait. Loading content for page: </i></p>
            <p><xsl:value-of select="f:get-hash()"/></p>
            <div class="loader"></div> <!-- Loading image -->
        </xsl:result-document>
        
        <ixsl:schedule-action document="{$docList}">
            <xsl:call-template name="render-page">
                <xsl:with-param name="docName" select="$docName"/>
                <xsl:with-param name="hash-parts" select="$hash-parts"/>
            </xsl:call-template>
        </ixsl:schedule-action>
        
    </xsl:template>

    <xsl:template name="render-page">
        <!-- Called from within ixsl:schedule-action document={$docName} -->
        <xsl:param name="docName"/>
        <xsl:param name="hash-parts"/>
        
        <xsl:variable name="start" select="$hash-parts[1]"/>
        
        <!-- $first-item is the li node of the navlist with the @data-part $start -->
        <xsl:variable name="first-item" select="f:get-first-item($start)" as="node()?"/> 
        
        <xsl:choose>
            <xsl:when test="not(doc-available($docName))">
                <xsl:result-document href="#main" method="ixsl:replace-content">
                    <xsl:sequence select="f:docnotfound($docName)"/>
                </xsl:result-document>
            </xsl:when>
            <xsl:when test="not(exists($first-item))">
                <xsl:result-document href="#main" method="ixsl:replace-content">
                    <h1>Page Not Found</h1>
                    <p>Error in URI hash-path:</p>
                    <p>List item '<xsl:value-of select="$start"/>' not found</p>
                </xsl:result-document>
            </xsl:when>
            <xsl:otherwise>
                <xsl:variable name="doc" select="doc($docName)"/>
                
                <xsl:variable name="count" select="count($hash-parts)"/>
                <xsl:variable name="hpart" select="$hash-parts[$count]"/>
                <xsl:variable name="subpage"
                    select="if ($start = ('javadoc', 'dotnetdoc')) then substring-after($hpart, '@') 
                    else ''"/>
                
                <xsl:variable name="hash-parts-simp" select="if ($subpage eq '') then $hash-parts else
                    (subsequence($hash-parts, 1, $count - 1), substring-before($hpart, '@'))"/>
                
                <xsl:call-template name="show-listitems">
                    <xsl:with-param name="doc" select="$doc"/>
                    <xsl:with-param name="hash-parts" select="$hash-parts-simp"/>
                    <xsl:with-param name="index" select="1"/>
                    <xsl:with-param name="item" select="$first-item"/>
                </xsl:call-template>
                
                <xsl:call-template name="main-content">
                    <xsl:with-param name="doc" select="$doc"/>
                    <xsl:with-param name="hash-parts" select="$hash-parts"/>
                </xsl:call-template>
                
                <xsl:call-template name="show-trail">
                    <xsl:with-param name="doc" select="$doc"/>
                    <xsl:with-param name="hash-parts" select="$hash-parts"/>
                </xsl:call-template>
                
                <!-- Does this require delay? Requires show-listitems to be finished -->
                <xsl:call-template name="highlight-item"> 
                    <xsl:with-param name="parent" select="$navlist"/>
                    <xsl:with-param name="hash-parts" select="$hash-parts-simp"/>
                    <xsl:with-param name="index" select="1"/>
                </xsl:call-template>
                
                <xsl:call-template name="platform-info"/>
                
            </xsl:otherwise>
            
        </xsl:choose>
    </xsl:template>
    
    <xsl:template name="main-content">
        <xsl:param name="hash-parts"/>
        <xsl:param name="doc"/>
        
        <xsl:variable name="start" select="$hash-parts[1]"/>
        <xsl:variable name="count" select="count($hash-parts)"/>
        <xsl:variable name="hpart" select="$hash-parts[$count]"/>
        <xsl:variable name="subpage"
            select="if (contains($hpart, '@')) then substring-after($hpart, '@') else ''"/>
        
        <xsl:variable name="hash-parts-simp" select="if ($subpage eq '') then $hash-parts else
            (subsequence($hash-parts, 1, $count - 1), substring-before($hpart, '@'))"/>
        
        <xsl:choose>
            <xsl:when test="$start eq 'javadoc'">
                <ixsl:set-property name="document.title" select="'Saxon Java API Documentation'"/>
                <xsl:call-template name="apidoc-page">
                    <xsl:with-param name="package-path" select="$jpackage-path"/>
                    <xsl:with-param name="hash-parts" select="$hash-parts"/>
                </xsl:call-template>
            </xsl:when>
            <xsl:when test="$start eq 'dotnetdoc'">
                <ixsl:set-property name="document.title" select="'Saxon .NET API Documentation'"/>
                <xsl:call-template name="apidoc-page">
                    <xsl:with-param name="package-path" select="$npackage-path"/>
                    <xsl:with-param name="hash-parts" select="$hash-parts"/>
                </xsl:call-template>
            </xsl:when>
            <xsl:otherwise>
                <xsl:result-document href="#main" method="ixsl:replace-content">
                    <!--<xsl:message>replace-content #main: {$hash-parts}, count {$count}</xsl:message>-->
                    <xsl:choose>
                        <xsl:when test="$start eq 'changes'">
                            <xsl:choose>
                                <xsl:when test="$count eq 1">
                                    <xsl:apply-templates select="$doc" mode="changes"/>
                                </xsl:when>
                                <xsl:when test="$count eq 2">
                                    <xsl:apply-templates select="$doc" mode="changes">
                                        <xsl:with-param name="selectedCategory" select="$hash-parts[2]"/>
                                    </xsl:apply-templates>
                                </xsl:when>
                                <xsl:when test="$count eq 3">
                                    <xsl:apply-templates select="$doc" mode="changes">
                                        <xsl:with-param name="selectedCategory" select="$hash-parts[2][. != '']"/>
                                        <xsl:with-param name="selectedVersionRange" select="$hash-parts[3][. != '']"/>
                                    </xsl:apply-templates>
                                </xsl:when>
                            </xsl:choose>
                        </xsl:when>
                        <xsl:when test="$count eq 1">
                            <xsl:apply-templates select="$doc" mode="primary"/>
                        </xsl:when>
                        <xsl:when test="$start eq 'functions' and $count eq 3">
                            <xsl:apply-templates mode="f" 
                                select="$doc//fnd:function[fnd:name[. eq $hpart][fnd:namespace-ref(@namespace) = $hash-parts[2]]]"/>
                        </xsl:when>
                        <xsl:otherwise>
                            <xsl:apply-templates select="$doc/*" mode="get-section">
                                <xsl:with-param name="hash-parts" select="$hash-parts-simp"/>
                                <xsl:with-param name="parent" select="$doc/*"/>
                                <xsl:with-param name="index" select="2"/>
                            </xsl:apply-templates>
                        </xsl:otherwise>
                    </xsl:choose>
                    
                    <xsl:sequence select="f:scrollpage($subpage)"/>
                    
                </xsl:result-document>
            </xsl:otherwise>
        </xsl:choose>
    </xsl:template>
    
    <xsl:template name="apidoc-page">
        <xsl:param name="package-path"/>
        <xsl:param name="hash-parts"/>
        
        <!--<xsl:message>package-page: {$hash-parts}, count {count($hash-parts)}</xsl:message>-->
        <xsl:choose>
            <xsl:when test="not(doc-available($package-path))">
                <xsl:result-document href="#main" method="ixsl:replace-content">
                    <xsl:sequence select="f:docnotfound($package-path)"/>
                </xsl:result-document>
            </xsl:when>
            <xsl:when test="count($hash-parts) eq 1">
                <xsl:result-document href="#main" method="ixsl:replace-content">
                    <xsl:apply-templates mode="jdp" select="doc($package-path)/article"/>
                </xsl:result-document>
            </xsl:when>
            <xsl:otherwise>
                <xsl:call-template name="apidoc-subpage">
                    <xsl:with-param name="package-path" select="$package-path"/>
                    <xsl:with-param name="hash-parts" select="$hash-parts"/>
                </xsl:call-template>
            </xsl:otherwise>
        </xsl:choose>
    </xsl:template>
    
    <xsl:template name="apidoc-subpage">
        <xsl:param name="package-path"/>
        <xsl:param name="hash-parts"/><!-- count ge 2 -->
        <xsl:variable name="class-path" select="if ($hash-parts[1] eq 'javadoc') then $jclass-path else $nclass-path"/>
        <xsl:variable name="docpath" select="concat($class-path, $hash-parts[2],'.xml')"/>
        
        <xsl:variable name="count" select="count($hash-parts)"/>
        <xsl:variable name="subpage" select="substring-after($hash-parts[$count], '@')"/>
        
        <xsl:choose>
            <xsl:when test="not(doc-available($docpath))">
                <xsl:result-document href="#main" method="ixsl:replace-content">
                    <xsl:sequence select="f:pkgnotfound($hash-parts)"/>
                </xsl:result-document>
            </xsl:when>
            <xsl:otherwise>
                <xsl:variable name="packageDoc" select="doc($docpath)"/>
                <xsl:variable name="hp" select="$hash-parts[$count]"/>
                <xsl:variable name="hpj"
                    select="if ($subpage ne '') then substring($hp , 1, (string-length($hp) - string-length($subpage)) - 1) else $hp"/>
                
                <!--<xsl:message>class-page replace-content #main: {$hash-parts}, count {$count}</xsl:message>-->
                
                <xsl:choose>
                    <xsl:when test="$count eq 2">
                        <xsl:result-document href="#main" method="ixsl:replace-content">
                            <xsl:if test="empty($packageDoc)">
                                <xsl:sequence select="f:pkgnotfound($hash-parts)"/>
                            </xsl:if>
                            <xsl:for-each select="doc($package-path)/article/section[@id eq $hpj]">
                                <xsl:apply-templates mode="pkg-header" select="."/>
                                <xsl:apply-templates mode="summarise-pkg" select="$packageDoc"/>
                                <xsl:apply-templates mode="jdp" select="."/>
                            </xsl:for-each>
                            <xsl:sequence select="f:scrollpage($subpage)"/>
                        </xsl:result-document>
                    </xsl:when>
                    <xsl:when test="$count gt 2">
                        <xsl:variable name="showclass"
                            select="if ($count eq 3) 
                            then $packageDoc/package/(class|interface|enum|delegate)[@id eq $hpj]
                            else $packageDoc/package/class/class[@id eq concat($hash-parts[$count - 1],'.',$hpj)]"/>
                        <xsl:if test="empty($showclass)">
                            <xsl:result-document href="#main" method="ixsl:replace-content">
                                <xsl:sequence select="f:classnotfound($hash-parts)"/>
                            </xsl:result-document>
                        </xsl:if>
                        <xsl:if test="exists($showclass)">
                            <xsl:apply-templates select="$showclass" mode="show-class">
                                <xsl:with-param name="subpage" select="$subpage" as="xs:string*"/>
                                <!-- $subpage is substring-after($hash-parts[$count], '@') -->
                            </xsl:apply-templates>
                        </xsl:if>
                    </xsl:when>
                </xsl:choose>
                
            </xsl:otherwise>
        </xsl:choose>
    </xsl:template>
    
    <!-- Used by javadoc and dotnetdoc to scroll to particular method, etc. -->
    <!-- Also used for feature keys -->
    <xsl:template name="scrollpage">
        <xsl:param name="id"/>
        <xsl:variable name="subnode"
            select="(ixsl:page()/html/body/(./div[@id = 'wrap']/div[@id = 'main']/div[@class
            = 'section']/div[@class = 'method']/h3[. eq $id], .//*[@id = $id]))[1]"
            as="node()?"/>
        <!-- Note that scroll to overpopulated method just goes to first one  -->
        
        <xsl:choose>
            <xsl:when test="exists($subnode)">
                <xsl:sequence select="ixsl:call($subnode, 'scrollIntoView', [true()])"/>
            </xsl:when>
            <xsl:otherwise>
                <ixsl:set-property
                    object="ixsl:page()/html/body/div[@id = 'wrap']/div[@id = 'main']"
                    name="scrollTop" select="0"/>
            </xsl:otherwise>
        </xsl:choose>
    </xsl:template>
    
    <xsl:function name="f:scrollpage">
        <xsl:param name="id"/>
        <ixsl:schedule-action wait="1">
            <xsl:call-template name="scrollpage">
                <xsl:with-param name="id" select="$id"/>
            </xsl:call-template>
        </ixsl:schedule-action>
    </xsl:function>
    
    
    <xsl:template name="platform-info">
        <xsl:variable name="csswitch" select="ixsl:page()//input[@id = 'cscheckbox']"/>
        <xsl:variable name="javaswitch" select="ixsl:page()//input[@id = 'javacheckbox']"/>
        <xsl:variable name="cswitch" select="ixsl:page()//input[@id = 'ccheckbox']"/>
        
        <xsl:variable name="maindiv" select="ixsl:page()/html/body/div[@id = 'wrap']/div[@id = 'main']"/>
        <xsl:variable name="platformdiv" select="ixsl:page()/html/body/div[@id = 'wrap']/div[@id eq 'platform']"/>
        
        <xsl:variable name="on-changes-page" select="exists($maindiv/div[@id = 'changes-content'])"/>
        <xsl:variable name="changeContentDivs" select="if ($on-changes-page) then (ixsl:page()/html/body/div[@id =
            'wrap']/div[@id = 'main']/div[@id = 'changes-content']//div[@class = 'changeContent']) else
            ()"/>
        
        <xsl:variable name="platformfndiv" select="ixsl:page()/html/body/div[@id = 'wrap']/div[@id =
            'main']/div/div[@id eq 'platform-fn']"/>
        <xsl:variable name="on-fn-list-page" select="exists($platformfndiv)"/>
        
        <xsl:for-each select="$maindiv//*[tokenize(@class)='csonly']">
            <ixsl:set-style name="display" 
                select="if (ixsl:get($csswitch,'checked')) 
                then if (self::tr) then 'table-row' 
                else if (self::li) then 'list-item'
                else 'block' 
                else 'none'"/>
        </xsl:for-each>
        <xsl:for-each select="$maindiv//*[tokenize(@class)='javaonly']">
            <ixsl:set-style name="display" 
                select="if (ixsl:get($javaswitch,'checked')) 
                then if (self::tr) then 'table-row' 
                else if (self::li) then 'list-item'
                else 'block' 
                else 'none'"/>
        </xsl:for-each>
        <xsl:for-each select="$maindiv//*[tokenize(@class)='conly']">
            <ixsl:set-style name="display" 
                select="if (ixsl:get($cswitch,'checked')) 
                then if (self::tr) then 'table-row' 
                else if (self::li) then 'list-item'
                else 'block' 
                else 'none'"/>
        </xsl:for-each>
        
        <xsl:if test="$on-fn-list-page">
            <!-- If we're on a functions index page, then update the display of list items as necessary. -->
            <xsl:for-each select="$maindiv//span[@class='flink']">
                <ixsl:set-style name="display" 
                    select="if ((tokenize(@data-platform)=('J','ALL') and ixsl:get($javaswitch,'checked')) or 
                    (tokenize(@data-platform)=('CS','ALL') and ixsl:get($csswitch,'checked')) or 
                    (tokenize(@data-platform)=('C','ALL') and ixsl:get($cswitch,'checked'))) 
                    then 'inline'
                    else 'none'"/>
            </xsl:for-each>
            
            <xsl:variable name="count" select="count($maindiv//span[@class='flink'][ixsl:style(.)?display eq 'inline'])"/>
            <!--<xsl:message>functions count: <xsl:value-of select="$count"/></xsl:message>-->
            <xsl:result-document href="#platform-fn-count" method="ixsl:replace-content">
                <xsl:value-of select="$count"/>
            </xsl:result-document>
            
            <!-- If there are no available functions for the selected platforms, then display info in the
                platform-fn div with a suggestion to switch platform checkboxes. -->
            <xsl:choose>
                <xsl:when test="$count eq 0">
                    <xsl:call-template name="platform-fn-warning"/>
                    <ixsl:set-style name="display" select="'block'" object="$platformfndiv"/>
                </xsl:when>
                <xsl:otherwise>
                    <ixsl:set-style name="display" select="'none'" object="$platformfndiv"/>
                </xsl:otherwise>
            </xsl:choose>
        </xsl:if>
        
        <xsl:if test="$on-changes-page">
            <xsl:for-each select="$changeContentDivs">
                <xsl:variable name="platform-ch-div" select="preceding-sibling::div[@class eq 'platform-ch']"/>
                <xsl:variable name="count" select="count(./*[ixsl:style(.)?display eq 'block'])"/>
                <!--<xsl:message>count displayed changes content: <xsl:value-of select="$count"/></xsl:message>-->
                <xsl:choose>
                    <xsl:when test="$count eq 0">
                        <xsl:call-template name="platform-ch-warning">
                            <xsl:with-param name="platform-ch-div" select="$platform-ch-div"/>
                        </xsl:call-template>
                        <ixsl:set-style name="display" select="'block'" object="$platform-ch-div"/>
                    </xsl:when>
                    <xsl:otherwise>
                        <ixsl:set-style name="display" select="'none'" object="$platform-ch-div"/>
                    </xsl:otherwise>
                </xsl:choose>
            </xsl:for-each>
        </xsl:if>
        
        <!-- Check whether there is anything displayed in the main div - if not, display info in the
            platform div with suggestion to switch platform checkboxes. -->
        <xsl:choose>
            <xsl:when test="not(ixsl:get($javaswitch,'checked')) and
                empty($maindiv/*[not(@class='javaonly')])">
                <xsl:call-template name="platform-warning">
                    <xsl:with-param name="platform" select="'Java'"/>
                </xsl:call-template>
                <ixsl:set-style name="display" select="'block'" object="$platformdiv"/>
            </xsl:when>
            <xsl:when test="not(ixsl:get($csswitch,'checked')) and
                empty($maindiv/*[not(@class='csonly')])">
                <xsl:call-template name="platform-warning">
                    <xsl:with-param name="platform" select="'C#'"/>
                </xsl:call-template>
                <ixsl:set-style name="display" select="'block'" object="$platformdiv"/>
            </xsl:when>
            <xsl:when test="not(ixsl:get($cswitch,'checked')) and
                empty($maindiv/*[not(@class='conly')])">
                <xsl:call-template name="platform-warning">
                    <xsl:with-param name="platform" select="'C'"/>
                </xsl:call-template>
                <ixsl:set-style name="display" select="'block'" object="$platformdiv"/>
            </xsl:when>
            <xsl:otherwise>
                <ixsl:set-style name="display" select="'none'" object="$platformdiv"/>
            </xsl:otherwise>
        </xsl:choose>
        
    </xsl:template>
    
    <xsl:template name="platform-warning">
        <xsl:param name="platform" as="xs:string"/>
        <xsl:result-document href="#platform" method="ixsl:replace-content">
            <div class="warning">
                <p><b>Page contains <xsl:value-of select="$platform"/> specific information only.</b></p>
                <p>To switch to show the <xsl:value-of select="$platform"/> specific
                    information, use the checkbox below (or the checkbox in the footer).</p>
                <label for="javacheckbox2"> Java: </label>
                <input type="checkbox" id="javacheckbox2" name="platform" value="java">
                    <xsl:if test="ixsl:get(ixsl:page()//input[@id = 'javacheckbox'],'checked')">
                        <xsl:attribute name="checked" select="'checked'"/>
                    </xsl:if>
                </input>
                <label for="cscheckbox2"> C#: </label>
                <input type="checkbox" id="cscheckbox2" name="platform" value="cs">
                    <xsl:if test="ixsl:get(ixsl:page()//input[@id = 'cscheckbox'],'checked')">
                        <xsl:attribute name="checked" select="'checked'"/>
                    </xsl:if>
                </input>
                <label for="ccheckbox2"> C: </label>
                <input type="checkbox" id="ccheckbox2" name="platform" value="c">
                    <xsl:if test="ixsl:get(ixsl:page()//input[@id = 'ccheckbox'],'checked')">
                        <xsl:attribute name="checked" select="'checked'"/>
                    </xsl:if>
                </input>
            </div>
        </xsl:result-document>
    </xsl:template>
    
    <xsl:template name="platform-fn-warning">
        <!--<xsl:message>platform-fn-warning called</xsl:message>-->
        <xsl:result-document href="#platform-fn" method="ixsl:replace-content">
            <div class="warning">
                <p><b>No functions available for the platforms selected.</b></p>
                <p>To switch to show the functions available on other platforms, use the checkbox below (or
                    the checkbox in the footer).</p>
                <label for="javacheckbox2"> Java: </label>
                <input type="checkbox" id="javacheckbox2" name="platform" value="java">
                    <xsl:if test="ixsl:get(ixsl:page()//input[@id = 'javacheckbox'],'checked')">
                        <xsl:attribute name="checked" select="'checked'"/>
                    </xsl:if>
                </input>
                <label for="cscheckbox2"> C#: </label>
                <input type="checkbox" id="cscheckbox2" name="platform" value="cs">
                    <xsl:if test="ixsl:get(ixsl:page()//input[@id = 'cscheckbox'],'checked')">
                        <xsl:attribute name="checked" select="'checked'"/>
                    </xsl:if>
                </input>
                <label for="ccheckbox2"> C: </label>
                <input type="checkbox" id="ccheckbox2" name="platform" value="c">
                    <xsl:if test="ixsl:get(ixsl:page()//input[@id = 'ccheckbox'],'checked')">
                        <xsl:attribute name="checked" select="'checked'"/>
                    </xsl:if>
                </input>
            </div>
        </xsl:result-document>
    </xsl:template>
    
    <xsl:template name="platform-ch-warning">
        <xsl:param name="platform-ch-div" as="element(div)"/>
        <xsl:message>platform-ch-warning called</xsl:message>
        <xsl:for-each select="$platform-ch-div">
            <xsl:result-document href="?." method="ixsl:replace-content">
                <div class="warning">
                    <p><b>No changes in this category documented for the platforms selected.</b></p>
                    <p>To switch to show the documented changes on other platforms, use the checkboxes in the footer.</p>
                </div>
            </xsl:result-document>
        </xsl:for-each>
        
    </xsl:template>
    

    <xsl:template match="section|article" mode="get-section">
        <xsl:param name="hash-parts" as="xs:string*"/>
        <xsl:param name="parent" as="node()?"/>
        <xsl:param name="index" as="xs:integer"/>
        <xsl:variable name="sectionEl" select="./(section|article)[@id eq $hash-parts[$index]]"/>
        <xsl:choose>
            <xsl:when test="empty($sectionEl)">
                <p>Error in URI hash-path:</p>
                <p>Section '<xsl:value-of select="$hash-parts[$index]"/>' not found in path: <xsl:value-of
                        select="$hash-parts" separator="/"/></p>
            </xsl:when>
            <xsl:when test="$index gt count($hash-parts)"/>
            <xsl:when test="$index eq count($hash-parts)">
                <xsl:apply-templates select="$sectionEl" mode="primary"/>
            </xsl:when>
            <xsl:otherwise>
                <xsl:apply-templates select="$sectionEl" mode="get-section">
                    <xsl:with-param name="hash-parts" select="$hash-parts"/>
                    <xsl:with-param name="parent" select="$sectionEl"/>
                    <xsl:with-param name="index" select="$index + 1"/>
                </xsl:apply-templates>
            </xsl:otherwise>
        </xsl:choose>
    </xsl:template>


    <xsl:template match="span[@class eq 'item']|li[@class = ('closed','open','empty')]"
        mode="ixsl:onclick">
        <xsl:apply-templates select="." mode="handle-itemclick"/>
    </xsl:template>

    <xsl:template match="*" mode="handle-itemclick">
        <xsl:variable name="partseq" select="(., ancestor::li)/@data-part" as="xs:string*"/>
        <xsl:variable name="new-hash" select="string-join($partseq, '/')"/>
        <xsl:variable name="isSpan" select="@class eq 'item'" as="xs:boolean"/>
        <!--<xsl:message>new-hash {$new-hash}</xsl:message>-->
        <xsl:for-each select="if ($isSpan) then .. else .">
            <xsl:choose>
                <xsl:when test="@class eq 'open' and not($isSpan)">
                    <ixsl:set-attribute name="class" select="'closed'"/>
                </xsl:when>
                <xsl:otherwise>
                    <xsl:sequence select="js:disableScroll()"/>
                    <xsl:choose>
                        <xsl:when test="f:get-hash() eq $new-hash">
                            <xsl:variable name="new-class" select="f:get-open-class(@class)"/>
                            <ixsl:set-attribute name="class" select="$new-class"/>
                            <xsl:if test="empty(ul)">
                                <xsl:call-template name="process-hashchange"/>
                            </xsl:if>
                        </xsl:when>
                        <xsl:otherwise>
                            <xsl:sequence select="f:set-hash($new-hash)"/>
                        </xsl:otherwise>
                    </xsl:choose>
                </xsl:otherwise>
            </xsl:choose>
        </xsl:for-each>
    </xsl:template>
    
    <xsl:template match="li[@class eq 'trail']" mode="ixsl:onclick">
        <xsl:sequence select="f:crumb-navigation(.)"/>
    </xsl:template>
    
    <xsl:template match="input[@id = 'javacheckbox']" mode="ixsl:onclick">
        <xsl:for-each select="ixsl:page()//input[@id = 'javacheckbox2']">
            <ixsl:set-property name="checked" select="ixsl:get(., 'checked')" object="."/>
        </xsl:for-each>
        <xsl:call-template name="platform-info"/>
    </xsl:template>
    
    <xsl:template match="input[@id = 'cscheckbox']" mode="ixsl:onclick">
        <xsl:for-each select="ixsl:page()//input[@id = 'cscheckbox2']">
            <ixsl:set-property name="checked" select="ixsl:get(., 'checked')" object="."/>
        </xsl:for-each>
        <xsl:call-template name="platform-info"/>
    </xsl:template>
    
    <xsl:template match="input[@id = 'ccheckbox']" mode="ixsl:onclick">
        <xsl:for-each select="ixsl:page()//input[@id = 'ccheckbox2']">
            <ixsl:set-property name="checked" select="ixsl:get(., 'checked')" object="."/>
        </xsl:for-each>
        <xsl:call-template name="platform-info"/>
    </xsl:template>
    
    <xsl:template match="input[@id = 'javacheckbox2']" mode="ixsl:onclick">
        <ixsl:set-property name="checked" select="ixsl:get(., 'checked')" 
            object="ixsl:page()//input[@id = 'javacheckbox']"/>
        <xsl:call-template name="platform-info"/>
    </xsl:template>
    
    <xsl:template match="input[@id = 'cscheckbox2']" mode="ixsl:onclick">
        <ixsl:set-property name="checked" select="ixsl:get(., 'checked')" 
            object="ixsl:page()//input[@id = 'cscheckbox']"/>
        <xsl:call-template name="platform-info"/>
    </xsl:template>
    
    <xsl:template match="input[@id = 'ccheckbox2']" mode="ixsl:onclick">
        <ixsl:set-property name="checked" select="ixsl:get(., 'checked')" 
            object="ixsl:page()//input[@id = 'ccheckbox']"/>
        <xsl:call-template name="platform-info"/>
    </xsl:template>
    
    <xsl:template match="p[@class = ('arrowLeft','arrowRight')]" mode="ixsl:onclick">
        <xsl:sequence select="f:navpage(@class)"/>
    </xsl:template>

    <xsl:function name="f:navpage">
        <xsl:param name="class"/>
        <xsl:call-template name="navpage">
            <xsl:with-param name="class" select="$class"/>
        </xsl:call-template>
    </xsl:function>

    <xsl:template name="navpage">
        <xsl:param name="class" as="xs:string"/>
        <xsl:variable name="hash-parts" select="tokenize(f:get-hash(),'/')"/>
        <xsl:variable name="start" select="$hash-parts[1]"/>
        <xsl:variable name="push" as="xs:string">
            <xsl:choose>
                <xsl:when test="$start eq 'functions' and count($hash-parts) eq 3">
                    <xsl:variable name="fn" select="f:get-function($hash-parts[2], $hash-parts[3])" as="element(fnd:function)?"/>
                    <xsl:variable name="next-fn" select="if ($class eq 'arrowLeft') then $fn/preceding-sibling::*[1] else $fn/following-sibling::*[1]"/>
                    <xsl:sequence select="if (exists($next-fn)) 
                                          then concat('functions/', $hash-parts[2], '/', $next-fn/fnd:name)
                                          else concat('functions/', $hash-parts[2])"/>
                       <!-- TODO: after the last function in a namespace, move on to the next namespace... -->
                    <!-- Currently we move back to namespace main page, then next goes to following
                        namespace; rather than going through functions. -->
                </xsl:when>
                <xsl:when test="$start eq 'changes' and count($hash-parts) ge 2">
                    <xsl:variable name="next-cat" select="doc(concat($location,'/changes.xml'))/ch:changes/ch:categories/ch:cat[@name=$hash-parts[2]]/
                        (if ($class eq 'arrowLeft') then preceding-sibling::*[1] else following-sibling::*[1])/@name"/>
                    <xsl:choose>
                        <xsl:when test="exists($next-cat)">
                            <xsl:sequence select="concat('changes/', $next-cat, if (count($hash-parts) eq 2) then () else concat('/', $hash-parts[3]))"/>
                        </xsl:when>
                        <xsl:when test="$class eq 'arrowLeft'">
                            <xsl:sequence select="'changes'"/>
                        </xsl:when>
                        <xsl:otherwise>
                            <xsl:variable name="c" as="node()"
                                select="f:get-item(subsequence($hash-parts, 1, 2), f:get-first-item($start), 1)"/>
                            <xsl:variable name="new-li" select="$c/following::li[1]"/>
                            <xsl:sequence select="$new-li/@data-part"/>
                        </xsl:otherwise>
                    </xsl:choose>
                </xsl:when>
                <xsl:otherwise>
                    <xsl:variable name="c" as="node()"
                        select="f:get-item($hash-parts, f:get-first-item($start), 1)"/>
                    <xsl:variable name="new-li"
                        select="if ($class eq 'arrowLeft') then
                        ($c/preceding::li[1] union $c/parent::ul/parent::li)[last()]
                        else ($c/ul/li union $c/following::li)[1]"/>

                    <xsl:sequence select="string-join(($new-li/ancestor::li union $new-li)/@data-part,'/')"/>
                </xsl:otherwise>
            </xsl:choose>
        </xsl:variable>

        <xsl:sequence select="f:set-hash($push)"/>
    </xsl:template>
    
    <xsl:template match="span[@data-href]" mode="ixsl:onmouseover">
        <xsl:if test="@class eq 'flink'">
            <xsl:variable name="docName" select="concat($location, '/functions.xml')"/>
            <!-- In fact, flinks only used on the functions namespace pages, in which case we know
                that functions.xml is already loaded.
                So the ixsl:schedule-action is not actually required (but it does no harm). -->
            <ixsl:schedule-action document="{$docName}">
                <xsl:call-template name="show-fn">
                    <xsl:with-param name="span" select="."/>
                    <xsl:with-param name="docName" select="$docName"/>
                </xsl:call-template>
            </ixsl:schedule-action>
        </xsl:if>
    </xsl:template>
    
    <xsl:template name="show-fn">
        <!-- Called from within ixsl:schedule-action document={$docName} -->
        <xsl:param name="span"/>
        <xsl:param name="docName"/>
        <xsl:variable name="href" select="$span/@data-href"/>
        <xsl:variable name="fn"
            select="doc($docName)/article/section[@id=$span/@data-ns]/fnd:function[fnd:name = $href]"/>
        <xsl:result-document href="#fn-desc" method="ixsl:replace-content">
            <h4>
                <xsl:value-of select="$href"/>
            </h4>
            <xsl:apply-templates select="$fn/fnd:description|$fn/fnd:signatures"
                mode="fn-description"/>
        </xsl:result-document>
    </xsl:template>
    
    
    <xsl:template name="show-trail">
        <xsl:param name="hash-parts" as="xs:string*"/>
        <xsl:param name="doc" as="node()?"/>
        <xsl:result-document href="#trail" method="ixsl:replace-content">
            <li id="trail1"><a href="http://saxonica.com">Saxonica ▷</a></li>
            <li id="trail2"><a
                href="http://www.saxonica.com/documentation/documentation.xml">Saxon ▷</a></li>
            <xsl:call-template name="get-trail">
                <xsl:with-param name="parent" select="$doc"/>
                <xsl:with-param name="hash-parts" select="$hash-parts"/>
                <xsl:with-param name="index" select="1"/>
            </xsl:call-template>
        </xsl:result-document>
    </xsl:template>

    <xsl:template name="get-trail">
        <xsl:param name="hash-parts" as="xs:string*"/>
        <xsl:param name="parent" as="node()?"/>
        <xsl:param name="index" as="xs:integer"/>
        <xsl:variable name="section" select="$parent/*[@id eq $hash-parts[$index]]"/> 
        <xsl:variable name="title" select="($section/@title, $section/@id, $hash-parts[$index])[1]"/>
        <xsl:choose>
            <xsl:when test="$index gt count($hash-parts)"/>
            <xsl:when test="$index eq count($hash-parts) and empty($section)">
                <!-- $section will be empty for Functions pages (note that Changes handled
                    separately)
                and javadoc page anchor links - i.e. containing @-->
                <xsl:choose>
                    <xsl:when test="$hash-parts[1] eq 'functions'">
                        <xsl:variable name="last" select="$hash-parts[$index]"/>
                        <xsl:variable name="function" select="$parent/fnd:function[fnd:name = $last]"/>
                        <xsl:variable name="prefix"
                            select="f:usual-prefix($function/fnd:name/@namespace)"/>
                        <xsl:variable name="preName" select="if ($prefix ne '') then concat($prefix,
                            ':', $last) else $last"/>
                        <li idt="{$last}" class="trail">
                            <xsl:value-of select="$preName"/>
                        </li>
                    </xsl:when>
                    <xsl:otherwise>
                        <xsl:variable name="all" select="$hash-parts[$index]"/>
                        <xsl:variable name="pre" select="substring-before($all, '@')"/>
                        
                        <xsl:choose>
                            <xsl:when test="$pre eq ''">
                                <li idt="{$section/@id}" class="trail">
                                    <xsl:value-of select="$all"/>
                                </li>
                            </xsl:when>
                            <xsl:otherwise>
                                <xsl:variable name="post" select="substring-after($all, '@')"/>
                                <li idt="{$pre}" class="trail"><xsl:value-of select="$pre"/> &#x25b7;</li>
                                <li ida="{$post}" class="trail"><xsl:value-of select="$post"/></li>
                            </xsl:otherwise>
                        </xsl:choose>
                    </xsl:otherwise>
                </xsl:choose>
                
            </xsl:when>
            <xsl:when test="$index eq count($hash-parts)">
                <li idt="{$section/@id}" class="trail">
                    <xsl:value-of select="$title"/>
                </li>
            </xsl:when>
            <xsl:when test="$hash-parts[1] eq 'changes'">
                <li idt="{$section/@id}" class="trail">
                    <xsl:value-of select="$title"/> &#x25b7;</li>
                <xsl:call-template name="get-trail-changes">
                    <xsl:with-param name="hash-parts" select="$hash-parts"/>
                    <xsl:with-param name="changes-doc" select="$section"/>
                    <xsl:with-param name="index" select="$index + 1"/>
                </xsl:call-template>
            </xsl:when>
            <xsl:otherwise>
                <li idt="{$section/@id}" class="trail">
                    <xsl:value-of select="$title"/> &#x25b7;</li>
                <xsl:call-template name="get-trail">
                    <xsl:with-param name="hash-parts" select="$hash-parts"/>
                    <xsl:with-param name="parent" select="$section"/>
                    <xsl:with-param name="index" select="$index + 1"/>
                </xsl:call-template>
            </xsl:otherwise>
        </xsl:choose>
    </xsl:template>
    
    <xsl:template name="get-trail-changes">
        <xsl:param name="hash-parts" as="xs:string*"/>
        <xsl:param name="changes-doc" as="node()?"/>
        <xsl:param name="index" as="xs:integer"/>
        <xsl:choose>
            <xsl:when test="$index gt count($hash-parts)"/>
            <xsl:when test="$index eq count($hash-parts)">
                <li idt="{$hash-parts[$index]}" class="trail">
                    <xsl:value-of select="($changes-doc/ch:categories/ch:cat[@name eq $hash-parts[$index]]/@title,
                        $hash-parts[$index])[1]"/>
                </li>
            </xsl:when>
            <xsl:otherwise>
                <li idt="{$hash-parts[$index]}" class="trail">
                    <xsl:value-of select="($changes-doc/ch:categories/ch:cat[@name eq $hash-parts[$index]]/@title,
                        $hash-parts[$index])[1]"/> &#x25b7;</li>
                <xsl:call-template name="get-trail-changes">
                    <xsl:with-param name="hash-parts" select="$hash-parts"/>
                    <xsl:with-param name="changes-doc" select="$changes-doc"/>
                    <xsl:with-param name="index" select="$index + 1"/>
                </xsl:call-template>
            </xsl:otherwise>
        </xsl:choose>
    </xsl:template>

    <xsl:template name="show-listitems">
        <xsl:param name="doc" as="node()"/> <!-- document-node when $index=1, section node subsequently -->
        <xsl:param name="hash-parts"/>
        <xsl:param name="index" as="xs:integer"/>
        <xsl:param name="item" as="node()?"/> <!-- li node of the navlist with the @data-part $hash-parts[$index] -->
        
        <xsl:variable name="hpart" select="$hash-parts[$index]"/>

        <xsl:for-each select="$item">
            <ixsl:set-attribute name="class" select="f:get-open-class(@class)"/>
            <xsl:choose>
                <xsl:when test="$index eq 1 and $hash-parts[1] eq 'changes'">
                    <xsl:result-document href="?." method="ixsl:replace-content">
                        <xsl:call-template name="add-list-changes">
                            <xsl:with-param name="doc" select="$doc"/>
                            <xsl:with-param name="hash-parts" select="$hash-parts"/>
                        </xsl:call-template>
                    </xsl:result-document>
                </xsl:when>
                <xsl:when test="exists(ul)">
                    <xsl:if test="$index lt count($hash-parts)">
                        <xsl:call-template name="show-listitems">
                            <xsl:with-param name="doc" select="$doc/*[@id eq $hpart]"/>
                            <xsl:with-param name="hash-parts" select="$hash-parts"/>
                            <xsl:with-param name="index" select="$index + 1"/>
                            <xsl:with-param name="item" select="ul/li[@data-part eq $hash-parts[$index + 1]]"/>
                        </xsl:call-template>
                    </xsl:if>
                </xsl:when>
                <xsl:otherwise>
                    <xsl:result-document href="?." method="ixsl:append-content">
                        <xsl:call-template name="add-list">
                            <xsl:with-param name="this-section" select="$doc/*[@id eq $hpart]"/>
                            <!-- |$doc/fnd:function[string(fnd:name) eq $hpart] -->
                            <xsl:with-param name="hash-parts" select="$hash-parts"/>
                            <xsl:with-param name="index" select="$index"/>
                        </xsl:call-template>
                    </xsl:result-document>
                </xsl:otherwise>
            </xsl:choose>
        </xsl:for-each>
    </xsl:template>

    <!-- Saxon-JS documentation version changes to make this work for ixsl-extension/functions too: 
            see if test="exists($section/fnd:function)" branch 
        - But we don't include the full list of functions in the main Saxon documentation app
            - see unused "add-list-fn" template below -->
    <xsl:template name="add-list">
        <xsl:param name="this-section" as="node()"/>
        <xsl:param name="hash-parts" as="xs:string*"/>
        <xsl:param name="index" as="xs:integer"/>

        <xsl:if test="exists($this-section/(section|j))">
            <ul>
                <xsl:for-each select="$this-section/(section|j)">
                    <xsl:variable name="onpath" as="xs:boolean*"
                        select="$index lt count($hash-parts) and @id eq $hash-parts[$index + 1]"/>
                    <xsl:variable name="contains" select="exists(section|j)"/>
                    <li data-part="{@id}">
                        <xsl:attribute name="class"
                            select="if ($onpath and $contains) then 'open'
                                    else if ($contains) then 'closed'
                                    else 'empty'"/>
                        <span class="item">
                            <xsl:value-of select="if (@title) then @title else @id"/>
                        </span>
                        <xsl:if test="$onpath">
                            <xsl:call-template name="add-list">
                                <xsl:with-param name="this-section"
                                    select="$this-section/(section|j)[@id = $hash-parts[$index + 1]]"/>
                                <xsl:with-param name="hash-parts" select="$hash-parts"/>
                                <xsl:with-param name="index" select="$index + 1"/>
                            </xsl:call-template>
                        </xsl:if>
                    </li>
                </xsl:for-each>
            </ul>
        </xsl:if>
    </xsl:template>
    
    <xsl:template name="add-list-changes">
        <xsl:param name="doc" as="node()"/>
        <xsl:param name="hash-parts" as="xs:string*"/>
        <span class="item">
            <xsl:value-of select="$doc/ch:changes/@title"/>
        </span>
        <ul>
            <xsl:for-each select="$doc/ch:changes/ch:categories/ch:cat">
                <li data-part="{@name}" class="empty">
                    <span class="item">
                        <xsl:value-of select="@title"/>
                    </span>
                </li>
            </xsl:for-each>
        </ul>
    </xsl:template>

    <!-- Reinstate? the navlist can always be hidden -->
    <!--<xsl:template name="add-list-fn">
        <xsl:param name="top-name" as="xs:string"/>
        <xsl:param name="fn-name" as="xs:string*"/>
        <span class="item">
            <xsl:value-of select="$top-name"/>
        </span>
        <xsl:if test="exists($fn-name)">
            <ul>
                <li data-part="{$fn-name}" class="empty">
                    <span class="item">[<xsl:value-of select="$fn-name"/>]</span>
                </li>
            </ul>
        </xsl:if>
    </xsl:template>-->

    <xsl:template name="highlight-item">
        <xsl:param name="parent" as="node()?"/> <!-- navlist when $index=1 -->
        <xsl:param name="hash-parts" as="xs:string*"/>
        <xsl:param name="index" as="xs:integer"/>
        <xsl:variable name="hitem" select="$parent/ul/li[@data-part eq $hash-parts[$index]]"/>
        <xsl:choose>
            <xsl:when test="$hash-parts[1] = ('functions','changes') and count($hash-parts) eq 3">
                <xsl:call-template name="highlight-item">
                    <xsl:with-param name="parent" select="$parent"/>
                    <xsl:with-param name="hash-parts" select="remove($hash-parts, 3)"/>
                    <xsl:with-param name="index" select="$index"/>
                </xsl:call-template>
            </xsl:when>
            <xsl:when test="$index lt count($hash-parts)">
                <xsl:call-template name="highlight-item">
                    <xsl:with-param name="parent" select="$hitem"/>
                    <xsl:with-param name="hash-parts" select="$hash-parts"/>
                    <xsl:with-param name="index" select="$index + 1"/>
                </xsl:call-template>
            </xsl:when>
            <xsl:otherwise>
                <xsl:for-each select="$hitem/span">
                    <xsl:for-each select="js:swapItem(.)">
                        <ixsl:set-attribute name="class" select="'item'"/>
                    </xsl:for-each>
                    <xsl:sequence select="js:enableScroll()"/>
                    <ixsl:set-attribute name="class" select="'hot'"/>
                </xsl:for-each>
                <xsl:if test="(ixsl:style(ixsl:page()/html/body/div/div[@class eq 'found'])?display
                    ne 'none') and not($hash-parts[1] = ('javadoc', 'dotnetdoc'))">
                    <ixsl:schedule-action wait="10">
                        <xsl:call-template name="highlighting"/>
                    </ixsl:schedule-action>
                </xsl:if>
            </xsl:otherwise>
        </xsl:choose>
    </xsl:template>
    
    <xsl:template name="show-tools">
        <xsl:if test="$args/@*[name() eq 'test'] eq 'ON'">
            <xsl:for-each
                select="ixsl:page()/html/body/div/div[@id eq 'footer']/div[@id eq 'test']">
                <ixsl:set-property object="." name="style.display" select="'block'"/>
            </xsl:for-each>
            <xsl:for-each select="$navlist">
                <ixsl:set-property object="." name="style.bottom" select="'210px'"/>
            </xsl:for-each>
            <xsl:for-each select="$navlist/../div[@id eq 'main']">
                <ixsl:set-property object="." name="style.bottom" select="'210px'"/>
            </xsl:for-each>
        </xsl:if>
    </xsl:template>

    <xsl:function name="f:crumb-navigation">
        <xsl:param name="c" as="node()"/>
        <xsl:variable name="seq" select="$c/preceding-sibling::*/@idt|$c/@idt"
            as="xs:string*"/>
        <xsl:variable name="anchor" select="$c/@ida" as="xs:string?"/>
        <xsl:variable name="new-hash" select="if ($anchor) then concat(string-join($seq,'/'), '@', $anchor) else string-join($seq,'/')"/>
        <xsl:sequence select="f:set-hash($new-hash)"/>
    </xsl:function>

    <xsl:function name="f:anchor-navigation">
        <xsl:param name="c" as="node()"/>
        <xsl:variable name="href">
            <xsl:choose>
                <xsl:when test="tokenize($c/@class) = ('javalink', 'cslink', 'apilink')">
                    <!--<xsl:message>javalink or cslink</xsl:message>-->
                    <xsl:variable name="data-href" select="$c/@data-href"/>
                    <xsl:variable name="ref"
                        select="for $a in substring-before($data-href,'(') return if ($a eq '') then $data-href else $a"/>
                    <xsl:variable name="pageref"
                        select="if (ends-with($ref, ']')) then substring-before($ref, '[') else $ref"/>
                    <xsl:variable name="tokens" select="tokenize($pageref,'\.')" as="xs:string*"/>
                    <xsl:variable name="paths" as="xs:string*"
                        select="for $t in 1 to count($tokens),
                                    $ch in substring($tokens[$t],1,1) return
                                    if (upper-case($ch) eq $ch) then
                                    concat('/',$tokens[$t]) else concat('.',$tokens[$t])
                                    "/>
                    <xsl:choose>
                        <xsl:when test="$pageref eq 'Saxon.Api'">
                            <xsl:value-of select="'dotnetdoc/Saxon.Api'"/>
                        </xsl:when>
                        <xsl:when test="contains($pageref, 'Saxon.Api')">
                            <xsl:value-of select="concat('dotnetdoc/Saxon.Api/',
                                translate(substring-after($pageref, 'Saxon.Api.'),'.','/'))"/>
                        </xsl:when>
                        <xsl:otherwise>
                            <xsl:variable name="section" select="if (contains($pageref, 'Saxon.Api') or
                                starts-with($pageref, 'System.')) then 'dotnetdoc/' else 'javadoc/'"/>
                            <xsl:value-of select="concat($section, substring(string-join($paths,''),2))"/>
                        </xsl:otherwise>
                    </xsl:choose>
                </xsl:when>
                <xsl:otherwise>
                    <xsl:variable name="ahref"
                        select="resolve-uri($c/@data-href, concat('http://a.com/', f:get-hash(),'/'))"/>
                    <xsl:value-of select="substring(string($ahref), 14)"/>
                    <!--<xsl:message>anchor-navigation {substring(string($ahref), 14)}</xsl:message>-->
                </xsl:otherwise>
            </xsl:choose>
        </xsl:variable>
        <xsl:sequence select="f:set-hash(translate($href, '#','@'))"/>
    </xsl:function>

    <xsl:function name="f:set-hash">
        <xsl:param name="hash"/>
        <ixsl:set-property name="location.hash" select="concat('!',$hash)"/>
    </xsl:function>

    <xsl:function name="f:get-open-class" as="xs:string">
        <xsl:param name="class" as="xs:string"/>
        <xsl:sequence select="if ($class eq 'empty') then 'empty' else 'open'"/>
    </xsl:function>

    <xsl:function name="f:get-first-item" as="node()?">
        <xsl:param name="start"/>
        <xsl:sequence select="$navlist/ul/li[@data-part = $start]"/>
    </xsl:function>
    
    <xsl:function name="f:get-item" as="node()"><!-- TODO Next page for Javadoc from class@method -->
        <xsl:param name="hash-parts" as="xs:string*"/>
        <xsl:param name="item" as="node()"/>
        <xsl:param name="index" as="xs:integer"/>
        <xsl:choose>
            <xsl:when test="$index eq count($hash-parts)">
                <xsl:sequence select="$item"/>
            </xsl:when>
            <xsl:otherwise>
                <xsl:variable name="new-item" select="$item/ul/li[@data-part eq $hash-parts[$index+1]]"/>
                <xsl:sequence select="f:get-item($hash-parts, $new-item, $index + 1)"/>
            </xsl:otherwise>
        </xsl:choose>
    </xsl:function>
    
    <xsl:function name="f:get-function">
        <xsl:param name="prefix" as="xs:string"/>
        <xsl:param name="local" as="xs:string"/>
        <xsl:sequence
            select="doc(concat($location, '/functions.xml'))//fnd:function[fnd:name[. = $local
            and fnd:namespace-ref(@namespace) = $prefix]]" />
        
        <!-- Note that this function is only called from navpage template, when stepping through functions pages, in which case
            the document 'functions.xml' has certainly already been loaded -->
        
    </xsl:function>

    <!-- hash is prefixed with ! as the 'hashbang' SEO measure: eg. http:/a.com#!about/gwt -->
    <xsl:function name="f:get-hash">
        <xsl:variable name="url" select="ixsl:location()"/>
        <!--<xsl:message>current url {$url}</xsl:message>-->
        <xsl:variable name="hash"
            select="if(contains($url, '%21')) then substring-after($url, '%21') else substring(ixsl:get(ixsl:window() , 'location.hash'),3)"/>
        <xsl:sequence select="if (string-length($hash) gt 0) then $hash else ($navlist/ul/li)[1]/@data-part"/>
    </xsl:function>

    <xsl:function name="f:parse-uri">
        <args>
            <xsl:analyze-string regex="([^=&amp;]+)=([^&amp;]*)"
                select="substring(ixsl:get(ixsl:window(), 'location.search'),2)">
                <xsl:matching-substring>
                    <xsl:attribute name="{regex-group(1)}" select="regex-group(2)"/>
                </xsl:matching-substring>
            </xsl:analyze-string>
        </args>
    </xsl:function>
    
    <xsl:function name="f:docnotfound">
        <xsl:param name="docpath" as="xs:string*"/>
        <h1>Page Not Found</h1>
        <p>Error in URI hash-path:</p>
        <p>Document '<xsl:value-of select="$docpath"/>' not found</p>
    </xsl:function>

</xsl:transform>
