<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="2.0"
    xmlns:xs="http://www.w3.org/2001/XMLSchema"
    xmlns:fc="http://www.saxonica.com/ns/doc/functions"
    xmlns:xi="http://www.w3.org/2001/XInclude"
    exclude-result-prefixes="xs fc xi">

    <xsl:import href="../../../src/viewer_appJS/xsl/functions-body.xsl"/>
    
    <xsl:variable name="implement-path" select="'../doc/implement.xml'"/>
    
    <!-- Further info for function namespace page -->
    <xsl:function name="fc:namespace-info">
        <xsl:param name="ns" as="xs:string"/>
        <xsl:choose>
            <xsl:when test="$ns = 'http://www.w3.org/2005/xpath-functions'">It includes both the
                core functions defined in XPath, and the additional functions defined in the XSLT
                specification.</xsl:when>
            <xsl:when test="$ns = 'http://saxon.sf.net/'">For general information about using these
                functions see <a class="bodylink"
                    href="/extensions/functions/saxon-extension-functions">Saxon extension
                    functions</a>.</xsl:when>
            <xsl:when test="$ns = 'http://www.w3.org/2005/xpath-functions/map'">It includes the core
                functions for handling maps defined in XPath 3.1.</xsl:when>
            <xsl:when test="$ns = 'http://www.w3.org/2005/xpath-functions/array'">It includes the
                core functions for handling arrays defined in XPath 3.1.</xsl:when>
            <xsl:when test="$ns = 'http://www.w3.org/2005/xpath-functions/math'">It includes the
                core mathematical and trigonometric functions defined in XPath 3.0.</xsl:when>
            <xsl:when test="starts-with($ns, 'http://expath.org/')">For general information about
                using these functions see <a class="bodylink" href="/extensions/functions/expath"
                    >EXPath extensions</a>.</xsl:when>
            <xsl:when test="starts-with($ns, 'http://exslt.org/')">For general information about
                using these functions see <a class="bodylink" href="/extensions/functions/exslt"
                    >EXSLT extensions</a>.</xsl:when>
            <xsl:when test="$ns = 'http://saxon.sf.net/sql'">For general information about using
                these functions see <a class="bodylink" href="/sql-extension">Saxon SQL
                    extension</a>.</xsl:when>
            <xsl:otherwise>
                <xsl:value-of select="''"/>
            </xsl:otherwise>
        </xsl:choose>
    </xsl:function>

    <xsl:template match="@* | node()">
        <xsl:copy>
            <xsl:apply-templates select="@*, node()"/>
        </xsl:copy>
    </xsl:template>
    
    <xsl:template match="/">
        <xsl:message>Processing functions</xsl:message>
        <xsl:apply-templates select="article"/>
    </xsl:template>
    
    <xsl:template match="article">
        <article id="{@id}" title="{@title}">
            <xsl:apply-templates/>
        </article>
    </xsl:template>

    <!-- Function Library main page. -->
    <xsl:template match="article/p">
        <p>This section of the documentation provides a complete list of functions implemented in
            the Saxon function library, organized by namespace. Click on a namespace to see the list
            of functions in that namespace.</p>
        <nav>
            <ul/>
        </nav>

        <p>For further general information about using functions not in the standard function
            namespace, see <a class="bodylink" href="/extensions/functions">Extension functions</a>. </p>

        <xsl:apply-templates select="section"/>
    </xsl:template>

    <xsl:template match="section">
        <section id="{@id}" title="{@title}">
            <h1>
                <xsl:value-of select="@title"/>
            </h1>
            <xsl:variable name="namespace" select="fc:function[1]/fc:name/@namespace"/>
            <xsl:variable name="sfns" as="element()*" select="fc:function"/>
            <!-- Implementation note on the namespace page. -->
            <p class="small-note">The information in this section indicates which functions are
                implemented in this Saxon release, and any restrictions in the current
                implementation. <xsl:copy-of select="fc:namespace-info($namespace)"/>
            </p>
            <p>Number of functions: <xsl:value-of select="count($sfns)"/></p>
            <!--<p>The functions in this namespace are listed below:</p>-->

            <xsl:variable name="colsize" select="xs:integer(ceiling(count($sfns) div 3))"/>
            <div style="overflow:auto; margin-top:5px;margin-bottom:5px">
                <table class="fn-list-by-ns">
                    <tr>
                        <td valign="top">
                            <p>
                                <xsl:apply-templates
                                    select="$sfns[(position() - 1) idiv $colsize eq 0]" mode="index"
                                />
                            </p>
                        </td>
                        <td valign="top">
                            <p>
                                <xsl:apply-templates
                                    select="$sfns[(position() - 1) idiv $colsize eq 1]" mode="index"
                                />
                            </p>
                        </td>
                        <td valign="top">
                            <p>
                                <xsl:apply-templates
                                    select="$sfns[(position() - 1) idiv $colsize eq 2]" mode="index"
                                />
                            </p>
                        </td>
                    </tr>
                </table>
            </div>
            <xsl:apply-templates select="$sfns" mode="f"/>
        </section>
    </xsl:template>

    <xsl:template match="fc:function" mode="index">
        <p>
            <a href="{fc:name}">
                <xsl:value-of select="fc:name"/>
            </a>
        </p>
    </xsl:template>

    <!--  Function pages for each function. -->
    <xsl:template match="fc:function" mode="f">
        <section id="{fc:name}" title="{fc:name}">

            <xsl:variable name="MyFunction" select="."/>
            <xsl:variable name="namespace" select="ancestor::section/@id"/>
            <xsl:variable name="default.saxon"
                select="$implementation-doc/implement/default[@id = $namespace]/@saxon"/>
            <xsl:variable name="default.platform"
                select="$implementation-doc/implement/default[@id = $namespace]/@platform"/>


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
                    <xsl:value-of
                        select="
                            for $i in $p-list/fc:in-spec
                            return
                                $i"
                    />
                </xsl:variable>

                <xsl:for-each-group select="$p-list" group-by="fc:in-spec">
                    <xsl:sort select="current-grouping-key()"/>
                    <xsl:variable name="p" select="current-grouping-key()"/>
                    <xsl:if
                        test="not($p = ('xpath20', 'xpath30', 'xslt20')) and 
                        not($p = 'xslt20' and contains($all-spec, 'xpath20')) and not($p = 'xslt30' and contains($all-spec, 'xpath30'))">
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
                    'default')[1])"/>
                <xsl:value-of
                    select="
                    fc:platform((fc:saxon-edition/@platform,
                    $default.platform,
                    'default')[1])"/>
            </p>

            <xsl:apply-templates select="fc:status" mode="f"/>
            <xsl:apply-templates select="fc:notes" mode="f"/>
            <xsl:apply-templates select="fc:details" mode="f"/>

            <xsl:if test="fc:see-also">
                <h3 class="subtitle">See also:</h3>
                <xsl:apply-templates select="fc:see-also" mode="f"/>
            </xsl:if>
        </section>
    </xsl:template>

    <!-- See also section with links to other related functions. -->
    <xsl:template match="fc:see-also" mode="f">
        <xsl:variable name="prefix" select="fc:usual-prefix(@namespace)"/>
        <xsl:variable name="ns-ref" select="fc:namespace-ref(@namespace)"/>
        <p>
            <a class="bodylink code" href="/functions/{$ns-ref}/{@name}"><xsl:value-of
                    select="concat($prefix, ':', @name)"/>()</a>
        </p>
    </xsl:template>

    <!-- Templates to override those in the imported stylesheet functions-body.xsl -->

    <xsl:template match="a | fc:a" mode="f">
        <xsl:choose>
            <xsl:when
                test="
                    substring(@href, 1, 5) = ('file:',
                    'http:')">
                <a class="link" href="{@href}">
                    <!-- these were span elements -->
                    <xsl:value-of select="."/>
                </a>
            </xsl:when>
            <xsl:otherwise>
                <a class="{@class}" href="{@href}">
                    <xsl:apply-templates select="node()" mode="secondary"/>
                </a>
            </xsl:otherwise>
        </xsl:choose>
    </xsl:template>

    <xsl:template match="a[@class = 'javalink'] | fc:a[@class = 'javalink']" mode="f">
        <a class="{@class}" href="{@href}">
            <xsl:copy-of select="@* except @href"/>
            <xsl:apply-templates/>
        </a>
    </xsl:template>

</xsl:stylesheet>
