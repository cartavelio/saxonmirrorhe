<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="3.0"
 xmlns:xs="http://www.w3.org/2001/XMLSchema" exclude-result-prefixes="xs fc ixsl"
 xmlns:ixsl="http://saxonica.com/ns/interactiveXSLT" extension-element-prefixes="ixsl"
 xmlns:fc="http://www.saxonica.com/ns/doc/functions"
 expand-text="yes">

 <xsl:template match="xsl:stylesheet" mode="f"/>

 <!-- Description in the hover box. -->
 <xsl:template match="fc:description" mode="fn-description">
  <div>
   <xsl:apply-templates mode="f"/>
  </div>
 </xsl:template>

 <!-- Description in the hover box, when there are multiple signatures. -->
 <xsl:template match="fc:signatures" mode="fn-description">
  <xsl:if test="fc:proto/fc:description">
   <ol>
    <xsl:for-each select="fc:proto/fc:description">
     <li>
      <xsl:apply-templates mode="f"/>
     </li>
    </xsl:for-each>
   </ol>
  </xsl:if>
 </xsl:template>

 <!-- Function Library main page. -->
 <xsl:template match="article" mode="function-list">
  <xsl:variable name="title" select="(h1 | h2 | h3)[1]"/>
  <xsl:if test="exists($title)">
   <ixsl:set-property name="document.title" select="concat('Saxon ', $title)"/>
  </xsl:if>
  <h1>Function Library</h1>
  <p>This section of the documentation provides a complete list of functions implemented in the
   Saxon function library, organized by namespace. Click on a namespace to see the list of functions
   in that namespace.</p>
  <table class="list">
   <xsl:for-each select="section">
    <xsl:variable name="ns-ref" select="fc:namespace-ref(fc:function[1]/fc:name/@namespace)"/>
    <tr>
     <td>
      <span class="link" data-href="/functions/{$ns-ref}">
       <xsl:value-of select="$ns-ref"/>
      </span>
     </td>
     <td>
      <p>
       <xsl:value-of select="fc:function[1]/fc:name/@namespace"/>
      </p>
     </td>
    </tr>
   </xsl:for-each>
  </table>
  <p>For further general information about using functions not in the standard function namespace,
   see <span class="link" data-href="/extensions/functions">Extension functions</span>. </p>
 </xsl:template>

 <!-- Namespace main page. -->
 <xsl:template match="section" mode="function-list">
  <xsl:variable name="title" select="(h1 | h2 | h3)[1]"/>
  <xsl:variable name="namespace" select="fc:function[1]/fc:name/@namespace"/>
  <xsl:if test="exists($title)">
   <ixsl:set-property name="document.title" select="concat('Saxon ', $title)"/>
  </xsl:if>

   <!-- Default content of hover box. -->
   <div id="fn-desc">
    <h4>Functions in namespace <xsl:value-of select="$namespace"/></h4>
    <p>
     <i>Hover over a listed function to view a brief description. For full details, select the
      function entry.</i>
    </p>
   </div>

   <!-- div for namespace functions list, on main page (scrollable section). -->
   
   <xsl:variable name="height"
    select="ixsl:get(ixsl:page()/html/body/div[@id = 'wrap']/div[@id = 'main'], 'offsetHeight') - 140"/>
   <!--<xsl:message>Height is {$height}</xsl:message>-->

   <div style="overflow:auto; height:{$height}px; margin-top:5px;margin-bottom:5px">

    <!-- Namespace functions list, on main page. -->
    <xsl:variable name="sfns" as="element()*" select="fc:fn-list(fc:function)"/>

    <p>Number of functions: <xsl:value-of select="count($sfns)"/></p>

    <xsl:variable name="colsize" select="xs:integer(ceiling(count($sfns) div 3))"/>

    <table class="fn-list-by-ns">
     <tr>
      <td valign="top">
       <p>
        <xsl:apply-templates select="$sfns[(position() - 1) idiv $colsize eq 0]" mode="index"/>
       </p>
      </td>
      <td valign="top">
       <p>
        <xsl:apply-templates select="$sfns[(position() - 1) idiv $colsize eq 1]" mode="index"/>
       </p>
      </td>
      <td valign="top">
       <p>
        <xsl:apply-templates select="$sfns[(position() - 1) idiv $colsize eq 2]" mode="index"/>
       </p>
      </td>
     </tr>
    </table>

   </div>

   <!-- Implementation note at the bottom of namespace page. -->
   <p class="small-note">The information in this section indicates which functions are implemented
    in this Saxon release, and any restrictions in the current implementation. <xsl:copy-of
     select="fc:namespace-info($namespace)"/>
   </p>

  
 </xsl:template>

 <!-- Contents of namespace function list: links to each function in namespace. -->
 <xsl:template match="fc:function" mode="index">
  <span class="flink" data-href="{fc:name}" data-ns="{fc:namespace-ref(fc:name/@namespace)}">
   <xsl:value-of select="fc:name"/>
  </span>
  <br/>
 </xsl:template>


 <!--  Function pages for each function. -->
 <xsl:template match="fc:function" priority="5" mode="f">

  <xsl:variable name="MyFunction" select="."/>
  <xsl:variable name="namespace" select="ancestor::section/@id"/>
  <xsl:variable name="default.saxon"
   select="doc($implement-path)/implement/default[@id = $namespace]/@saxon"/>
  <xsl:variable name="default.platform"
   select="doc($implement-path)/implement/default[@id = $namespace]/@platform"/>

  <ixsl:set-property name="document.title"
   select="concat('Saxon ', fc:namespace-ref(fc:name/@namespace), ':', fc:name)"/>

  <h1>
   <xsl:value-of select="concat(fc:usual-prefix(fc:name/@namespace), ':', fc:name)"/>
  </h1>

  <xsl:apply-templates select="fc:description" mode="f"/>

  <!-- Determine if all signatures of the function appear in the same set of versions of specifications. 
       If so, just one 'Specs versions & Saxon editions' note, otherwise one per signature. -->
  <xsl:variable name="same-spec-references" as="xs:boolean"
   select="
    every $proto in subsequence(fc:signatures/fc:proto, 2)
     satisfies deep-equal(fc:signatures/fc:proto[1]/fc:in-spec, $proto/fc:in-spec)"/>

  <xsl:apply-templates select="fc:signatures" mode="f">
   <xsl:with-param name="output-spec-references" select="not($same-spec-references)"/>
  </xsl:apply-templates>

  <h3 class="subtitle">Namespace</h3>
  <p>
   <xsl:value-of select="$MyFunction/fc:name/@namespace"/>
  </p>

  <xsl:if test=".//fc:in-spec">
   <h3 class="subtitle">Links to <xsl:if
     test="contains($MyFunction/fc:name/@namespace, 'xpath-functions')">
     <xsl:text>W3C </xsl:text></xsl:if>specifications</h3>
   <xsl:variable name="fname" select="$MyFunction/fc:name"/>
   <xsl:variable name="p-list" select="fc:signatures/fc:proto"/>
   <xsl:variable name="all-spec">
    <xsl:value-of select="
      for $i in $p-list/fc:in-spec
      return
       $i"/>
   </xsl:variable>

   <xsl:for-each-group select="$p-list" group-by="fc:in-spec">
    <xsl:sort select="current-grouping-key()"/>
    <xsl:variable name="p" select="current-grouping-key()"/>
    <xsl:if
     test="not($p = 'xslt20' and contains($all-spec, 'xpath20')) and not($p = 'xslt30' and contains($all-spec, 'xpath30'))">
     <p>
      <a href="{fc:spec-href($p, $fname)}">
       <xsl:value-of select="fc:spec-link-name($p)"/>
      </a>
     </p>
    </xsl:if>
   </xsl:for-each-group>
  </xsl:if>
  
  <h3 class="subtitle">Saxon availability</h3>
  <p>
   <xsl:if test="$same-spec-references">
    <xsl:if test=".//fc:in-spec">
     <xsl:value-of select="fc:specs(fc:signatures/fc:proto[1]/fc:in-spec)"/>
    </xsl:if>
   </xsl:if>
   <xsl:value-of
    select="
    fc:edition((fc:saxon-edition,
    $default.saxon,
    'ALL')[1])"/>
   <xsl:value-of
    select="
    fc:platform((fc:saxon-edition/@platform,
    $default.platform,
    'ALL')[1])"/>
  </p>

  <xsl:apply-templates select="fc:status" mode="f"/>
  <xsl:apply-templates select="fc:notes" mode="f"/>
  <xsl:apply-templates select="fc:details" mode="f"/>

  <xsl:if test="fc:see-also">
   <h3 class="subtitle">See also:</h3>
   <xsl:apply-templates select="fc:see-also" mode="f"/>
  </xsl:if>

 </xsl:template>

 <!-- See also section with links to other related functions. -->
 <xsl:template match="fc:see-also" mode="f">
  <xsl:variable name="prefix" select="fc:usual-prefix(@namespace)"/>
  <xsl:variable name="ns-ref" select="fc:namespace-ref(@namespace)"/>
  <p>
   <span class="link" data-href="/functions/{$ns-ref}/{@name}"><xsl:value-of
     select="concat($prefix, ':', @name)"/>()</span>
  </p>
 </xsl:template>

</xsl:stylesheet>
