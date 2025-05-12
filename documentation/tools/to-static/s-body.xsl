<xsl:transform xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
     xmlns:xs="http://www.w3.org/2001/XMLSchema" xmlns:f="urn:internal.function"
     xmlns:def="http://www.saxonica.com/ns/doc/functions" exclude-result-prefixes="f def xs"
     version="2.0">

     <xsl:output method="html"/>
     
     <xsl:variable name="javadoc-available" select="doc-available('../../../build/userdoc/viewer_appJS/javadoc-xml/javadoc-types.xml')"/>
     <xsl:variable name="javadoc-types" select="doc('../../../build/userdoc/viewer_appJS/javadoc-xml/javadoc-types.xml')"/>
     
     <xsl:variable name="dotnetdoc-available" select="doc-available('../../../build/userdoc/viewer_appJS/dotnetdoc-xml/types.xml')"/>
     <xsl:variable name="dotnetdoc-types" select="doc('../../../build/userdoc/viewer_appJS/dotnetdoc-xml/types.xml')"/>

     <xsl:key name="jd-types" match="j" use="string-join(ancestor-or-self::j/@id, '.')"/>

     <xsl:template match="section|article" mode="primary">
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

     <xsl:template match="dfn" mode="secondary">
          <xsl:apply-templates/>
     </xsl:template>

     <xsl:template match="nav[ul]" mode="secondary">
          <div>
               <xsl:if test="@columns">
                    <xsl:attribute name="class" select="'columns'"/>
               </xsl:if>
               <ul>
                    <xsl:for-each select="../(article|section)">
                         <xsl:variable name="href"
                              select="f:index-fix(
                              if (article|section) then
                              @id else concat(@id, '.html'))"/>
                         <li>
                              <p>
                                   <a href="{$href}">
                                        <xsl:value-of select="@title"/>
                                   </a>
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
          <xsl:param name="hash-path" tunnel="yes" required="no" as="xs:string" select="''"/>
          <xsl:variable name="src"
               select="if (starts-with(@src, '/')) then substring(@src, 2) else ."/>
          <img src="{concat('../',$hash-path, $src)}">
               <xsl:apply-templates select="(@* except @src)" mode="secondary"/>
               <xsl:apply-templates/>
          </img>
     </xsl:template>
     
     <xsl:template match="pre[code]" mode="secondary">
          <samp>
               <xsl:apply-templates select="code/node()" mode="secondary"/>
          </samp>
     </xsl:template>

     <xsl:template match="a[@class = 'apilink']" mode="secondary">
          <xsl:param name="hash-path" tunnel="yes" required="no" as="xs:string" select="''"/>
          <xsl:copy copy-namespaces="no">
               <xsl:attribute name="href" select="f:anchor-navigation(., $hash-path)"/>
               <!--<xsl:attribute name="href" select="concat($hash-path, @href)"/>-->
               <xsl:apply-templates select="(@* except @href)" mode="secondary"/>
               <xsl:apply-templates select="node()" mode="secondary"/>
          </xsl:copy>
     </xsl:template>
     
     <xsl:template match="a[@class ='javalink']|code[@java]" mode="secondary">
          <xsl:param name="hash-path" tunnel="yes" required="no" as="xs:string" select="''"/>
          <xsl:variable name="javaclass" select="if (local-name() = 'a') then @href else @java" as="xs:string"/>
          <!--<xsl:message select="'javaclass is ', $javaclass"/>-->
          <xsl:variable name="item-id" select="translate($javaclass, '#','.')"/>
          <xsl:variable name="found" select="if ($javadoc-available) then (exists(key('jd-types', $item-id, $javadoc-types)))
               else true()"
               as="xs:boolean"/>
          <xsl:if test="not($found)">
               <xsl:message>*** Warning: javadoc ref <xsl:value-of select="$item-id"/> not found</xsl:message>
          </xsl:if>
          <xsl:variable name="link" select="translate($javaclass, '.', '/')"/>
          <xsl:variable name="file"
               select="if (contains($link, '#')) 
               then concat(substring-before($link, '#'), '.html#', substring-after($link, '#'))
               else concat($link, '.html')"/>
          <a class="javalink" href="{concat($hash-path, 'javadoc/', $file)}">
               <xsl:apply-templates/>
          </a>
     </xsl:template>
     
     <xsl:template match="a[@class ='cslink']|code[@cs]" mode="secondary">
          <xsl:param name="hash-path" tunnel="yes" required="no" as="xs:string" select="''"/>
          <xsl:variable name="csclass" select="if (local-name() = 'a') then @href else @cs" as="xs:string"/>
          <!--<xsl:message select="'csclass is ', $csclass"/>-->
          <xsl:variable name="item-id" select="translate($csclass, '#','.')"/>
          <xsl:variable name="founddotnet" select="if ($dotnetdoc-available) then (exists(key('jd-types',
               $item-id, $dotnetdoc-types))) else true()"
               as="xs:boolean"/>
          <!--<xsl:variable name="founddotnet" select="false()" as="xs:boolean"/>--><!-- Temporary pre 10 .NET release-->
          <xsl:if test="not($founddotnet)">
               <xsl:message>*** Warning: dotnetdoc ref <xsl:value-of select="$item-id"/> not found</xsl:message>
          </xsl:if>
          <xsl:variable name="link" select="translate($csclass, '.', '/')"/>
          <xsl:variable name="file"
               select="if (contains($link, '#')) 
               then concat(substring-before($link, '#'), '.html#', substring-after($link, '#'))
               else concat($link, '.html')"/>
          <a class="cslink" href="{concat($hash-path, 'dotnetdoc/', $file)}">
               <xsl:apply-templates/>
          </a>
     </xsl:template>

     <xsl:template match="a" mode="secondary">
          <xsl:param name="hash-path" tunnel="yes" required="no" as="xs:string" select="''"/>
          <xsl:choose>
               <xsl:when
                    test="substring(@href, 1, 5) = ('file:','http:')
or contains(@href,'>') or starts-with(@href, 'https:')">
                    <xsl:copy-of select="." copy-namespaces="no"/>
               </xsl:when>
               <xsl:when test="substring-after(@href, '.') = ('zip','pdf','txt','xsl','xsd','xml')">
                    <xsl:variable name="href"
                         select="if (starts-with(@href, '/')) then substring(@href, 2) else @href"/>
                    <a href="{concat('../',$hash-path, $href)}">
                         <xsl:apply-templates select="@* except @href" mode="secondary"/>
                         <xsl:apply-templates/>
                    </a>
               </xsl:when>
               <xsl:otherwise>
                    <xsl:copy>
                         <xsl:attribute name="href" select="f:anchor-navigation(., $hash-path)"/>
                         <xsl:apply-templates select="(@* except @href)" mode="secondary"/>
                         <xsl:apply-templates select="node()" mode="secondary"/>
                    </xsl:copy>
               </xsl:otherwise>
          </xsl:choose>
          <!--
<xsl:if test="@href eq 'instructions'">
<xsl:message terminate="yes">
terminated on 'instructions'
</xsl:message>
</xsl:if>
-->
     </xsl:template>

     <xsl:function name="f:anchor-navigation">
          <xsl:param name="a" as="node()"/>
          <xsl:param name="hash-path" as="xs:string"/>
          <xsl:variable name="href" select="$a/@href"/>
          <xsl:variable name="section"
               select="($a/ancestor::section[1]|$a/ancestor::article|
$a/ancestor::def:function|$a/ancestor::def:functions)[last()]"/>

          <xsl:for-each select="$section">
               <!-- path to the section containing the href - use for resolving relative paths -->
               <xsl:variable name="path-to-section" as="xs:string*"
                    select="string-join(
(ancestor::article|ancestor::section|ancestor::def:functions)/@id,
'/')"/>
               <!-- if this is a folder (contains other sections) then add the section name -->
               <xsl:variable name="base-path" as="xs:string"
                    select="string-join(($path-to-section, @id, *:name), '/')"/>
               <xsl:variable name="is-absolute" select="starts-with($href, '/')" as="xs:boolean"/>
               <xsl:variable name="resolved-ref">
                    <xsl:choose>
                         <xsl:when test="$is-absolute">
                              <xsl:value-of select="substring($href,2)"/>
                         </xsl:when>
                         <xsl:otherwise>
                              <xsl:variable name="r-href"
                                   select="resolve-uri($href, concat('http://a.com/', $base-path,'/'))"/>
                              <xsl:sequence select="substring($r-href, 14)"/>
                         </xsl:otherwise>
                    </xsl:choose>
               </xsl:variable>
               <xsl:variable name="fragment" select="if (contains($resolved-ref, '@')) then
                    concat('#', substring-after($resolved-ref, '@')) else ''"/>
               <xsl:variable name="resolved-ref" select="if (contains($resolved-ref, '@')) then
                    substring-before($resolved-ref, '@') else $resolved-ref"/>

               <!-- Fix for links to javadoc and dotnetdoc APIs -->
               <xsl:choose>
                    <xsl:when test="$resolved-ref eq 'javadoc' or $resolved-ref eq 'dotnetdoc'">
                         <xsl:sequence select="concat($hash-path,$resolved-ref)"/>
                    </xsl:when>
                    <xsl:otherwise>

                         <xsl:variable name="ids" select="tokenize($resolved-ref, '/')"/>
                         <xsl:variable name="parent" select="$section-docs[*/@id eq $ids[1]]/*"/>

                         <xsl:variable name="ref-section" as="element()?">
                              <xsl:call-template name="get-section-element">
                                   <xsl:with-param name="ids" select="$ids"/>
                                   <xsl:with-param name="parent" select="$parent"/>
                                   <xsl:with-param name="index" select="2"/>
                              </xsl:call-template>
                         </xsl:variable>

<!--                         
<xsl:message>
parent-id <xsl:value-of select="$parent/@id"/>
is-abs <xsl:value-of select="$is-absolute"/>
path-to-section: <xsl:value-of select="$path-to-section"/>
base-path <xsl:value-of select="$base-path"/>
resolved-ref <xsl:value-of select="$resolved-ref"/>
</xsl:message>
-->

                         <!--  prefix was '../' for non absolute links -->
                         <xsl:for-each select="$ref-section">
                              <xsl:variable name="prefix" select="$hash-path"/>

                              <xsl:variable name="fpath" select="$resolved-ref"/>
                              <xsl:variable name="ext"
                                   select="if (article|section|def:function) then '' else '.html'"/>
                              <xsl:variable name="pre-filepath"
                                   select="concat($prefix, $fpath, $ext, $fragment)"/>
                              <xsl:variable name="filepath" select="f:index-fix($pre-filepath)"/>
                              <xsl:sequence select="$filepath"/>

                              <!--
<xsl:message>
section-doc-ids: <xsl:value-of select="$section-docs/*/@id"/>
href <xsl:value-of select="$href"/>
base-path: <xsl:value-of select="$base-path"/>
resolved-ref <xsl:value-of select="$resolved-ref"/>
prefix: <xsl:value-of select="$prefix"/>
file-path: <xsl:value-of select="$filepath"/>
parent-id <xsl:value-of select="$parent/@id"/>
sec-id: <xsl:value-of select="@id"/>
</xsl:message>
-->

                         </xsl:for-each>

                         <!--
<xsl:if test="$href eq '../linenumber'">
<xsl:message terminate="yes">
section-doc-ids: <xsl:value-of select="$section-docs/*/@id"/>
ids[1]: <xsl:value-of select="$ids[1]"/>
terminated for ../xslt20
</xsl:message>
</xsl:if>
-->

                         <xsl:choose>
                           <xsl:when test="empty($ref-section)">
                             <xsl:message>
                               <xsl:text>*** Warning: at base-path: </xsl:text>
                               <xsl:value-of select="$base-path"/>
                               <xsl:text> href: </xsl:text>
                               <xsl:value-of select="$href"/>
                               <xsl:text> not resolved </xsl:text>
                             </xsl:message>
                             <xsl:sequence select="$href"/>
                           </xsl:when>
                         </xsl:choose>

                    </xsl:otherwise>
               </xsl:choose>

          </xsl:for-each>

     </xsl:function>

     <!-- fixes case where index.html name at the folder top-level may clash with
     an item within the folder also named index (e.g. saxon:index)  -->
     <xsl:function name="f:index-fix">
          <xsl:param name="path"/>
          <xsl:sequence
               select="if (ends-with($path, '/index.html') or $path eq 'index.html') then
concat(substring($path,1,string-length($path) - 5),'-1.html') 
else $path "
          />
     </xsl:function>

</xsl:transform>
