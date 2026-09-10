<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
    xmlns:xs="http://www.w3.org/2001/XMLSchema" xmlns:local="http://local-functions/"
    exclude-result-prefixes="xs local" version="2.0">
    
    <!-- Use this stylesheet to generate the output prod-desc.xml
        (the source document for the product descriptions), 
        from the input tools/docs/feature-matrix/feature-matrix.xml-->

    <xsl:output method="xml" indent="yes"/>
    
    <xsl:variable name="all-variants" as="xs:string*">
        <xsl:for-each select="featureMatrix/packages/package">
            <xsl:variable name="package" select="."/>
            <xsl:for-each select="tokenize(@platforms, '\s')[. ne 'JS']">
                <xsl:value-of select="concat(., '-', $package/@edition)"/>
            </xsl:for-each>
        </xsl:for-each>
        <xsl:value-of select="'JS'"/>
    </xsl:variable>
    
    <xsl:variable name="version" select="featureMatrix/@version" as="xs:string"/>
    <xsl:variable name="c-version" select="if (featureMatrix/@Cversion) then featureMatrix/@Cversion else featureMatrix/@version" as="xs:string"/>
    <xsl:variable name="js-version" select="featureMatrix/@JSversion" as="xs:string"/>

    <xsl:template match="featureMatrix">
        <xsl:variable name="j-release-date" select="if (@Jrelease) then @Jrelease else @release" as="xs:string"/>
        <xsl:variable name="cs-release-date" select="if (@CSrelease) then @CSrelease else @release" as="xs:string"/>
        <xsl:variable name="c-release-date" select="if (@Crelease) then @Crelease else @release" as="xs:string"/>
        <xsl:variable name="js-release-date" select="@JSrelease" as="xs:string"/>
        <doc j-version="Version {$version} released {$j-release-date}" 
            cs-version="Version {$version} released {$cs-release-date}" 
            c-version="Version {$c-version} released {$c-release-date}"
            js-version="Version {$js-version} released {$js-release-date}" >
            <xsl:text>&#xa;</xsl:text>
            <xsl:comment>Generated - <xsl:value-of select="current-dateTime()"/> from
                tools/docs/feature-matrix/feature-matrix.xml,
             using stylesheet tools/docs/feature-matrix/feature-matrix-to-prod-desc.xsl</xsl:comment>
            <xsl:text>&#xa;</xsl:text>
            <xsl:call-template name="header-section"/>
            <xsl:call-template name="doc-desc"/>
            <xsl:call-template name="footer-section"/>

            <h1 class="proddesc">Language Support</h1>
            <table class="feature-set" title="XSLT (Transformation Processing)">
                <tbody>
                    <xsl:call-template name="feature-sections">
                        <xsl:with-param name="topic" select="'XSLT'"/>
                    </xsl:call-template>
                </tbody>
            </table>
            <xsl:call-template name="language-section">
                <xsl:with-param name="topic" select="'XSLT'"/>
            </xsl:call-template>
            <table class="feature-set" title="XPath">
                <tbody>
                    <xsl:call-template name="feature-sections">
                        <xsl:with-param name="topic" select="'XPath'"/>
                    </xsl:call-template>
                </tbody>
            </table>
            <xsl:call-template name="language-section">
                <xsl:with-param name="topic" select="'XPath'"/>
            </xsl:call-template>
            <table class="feature-set" title="XQuery">
                <tbody>
                    <xsl:call-template name="feature-sections">
                        <xsl:with-param name="topic" select="'XQuery'"/>
                    </xsl:call-template>
                </tbody>
            </table>
            <xsl:call-template name="language-section">
                <xsl:with-param name="topic" select="'XQuery'"/>
            </xsl:call-template>
            <table class="feature-set"
                title="XSD (XML Schema Validation)">
                <tbody>
                    <xsl:call-template name="feature-sections">
                        <xsl:with-param name="topic" select="'XSD'"/>
                    </xsl:call-template>
                </tbody>
            </table>

            <h1 class="proddesc">
                <xsl:value-of select="'Performance Features'"/>
            </h1>
            <xsl:call-template name="feature-sections">
                <xsl:with-param name="topic" select="'Performance'"/>
            </xsl:call-template>
            <h1 class="proddesc">
                <xsl:value-of select="'Extensibility'"/>
            </h1>
            <xsl:call-template name="feature-sections">
                <xsl:with-param name="topic" select="'Extensibility'"/>
            </xsl:call-template>
            <h1 class="proddesc">
                <xsl:value-of select="'Localization'"/>
            </h1>
            <xsl:call-template name="feature-sections">
                <xsl:with-param name="topic" select="'Localization'"/>
            </xsl:call-template>
            <h1 class="proddesc">
                <xsl:value-of select="'Interfaces and APIs'"/>
            </h1>
            <xsl:call-template name="feature-sections">
                <xsl:with-param name="topic" select="'Interfaces'"/>
            </xsl:call-template>
        </doc>
    </xsl:template>

    <xsl:template name="feature-sections">
        <xsl:param name="topic"/>
        <xsl:for-each select="feature[@class eq $topic]/products/product">
            <xsl:variable name="topic-shortName" select="ancestor::feature/shortName"/>
            <xsl:variable name="details" select="ancestor::feature/details"/>
            <xsl:variable name="spec" select="ancestor::feature/spec"/>
            <xsl:variable name="lang" select="ancestor::feature/@lang"/>
            <xsl:variable name="product-siblings" select="parent::products/product"/>
            <xsl:variable name="variants" as="xs:string*">
                <xsl:choose>
                    <xsl:when test="exists(subsumed-by)">
                        <!-- This feature may be "subsumed by" others.
                            Upper bound for this feature's variants range comes from intersection of product
                            lists for 'subsumed-by' features. -->
                        <xsl:variable name="this-product" select="."/>
                        <xsl:variable name="ids" select="tokenize(subsumed-by/@feature, '\s')"/>
                        <xsl:variable name="subsuming-features" select="ancestor::featureMatrix/feature[string(@id) =
                            $ids]"/>
                        <!--<xsl:message><xsl:value-of select="$topic-shortName"/></xsl:message>
                        <xsl:message>subsumed by: <xsl:value-of select="$ids"/></xsl:message>
                        <xsl:message>features count: <xsl:value-of select="count($subsuming-features)"/></xsl:message>-->
                        <xsl:iterate select="$subsuming-features">
                            <xsl:param name="variants" select="()" as="xs:string*"/>
                            <xsl:variable name="variants-range" as="xs:string*"
                                select="local:get-variants-range($this-product, ./products/product)"/>
                            <!--<xsl:message>variants-range: <xsl:value-of select="$variants-range"/></xsl:message>-->
                            <xsl:variable name="intersection" as="xs:string*"
                                select="if (empty($variants)) then $variants-range else distinct-values($variants[. = $variants-range])"/>
                            <xsl:sequence select="$intersection"/>
                            <xsl:next-iteration>
                                <xsl:with-param name="variants" select="$intersection"/>
                            </xsl:next-iteration>
                        </xsl:iterate>
                    </xsl:when>
                    <xsl:when test="count($product-siblings) gt 1">
                        <xsl:choose>
                            <xsl:when test="@level eq 'Basic'">
                                <xsl:sequence select="local:get-variants-range($product-siblings[@level eq 'Basic'],
                                    $product-siblings[contains(@level, 'Adv')])"/>
                            </xsl:when>
                            <xsl:when test="@level eq 'Open'">
                                <xsl:sequence select="local:get-variants-range($product-siblings[@level eq 'Open'],
                                    $product-siblings[@level eq 'Inc'])"/>
                            </xsl:when>
                            <xsl:otherwise>
                                <xsl:sequence select="local:get-variants(.)"/>
                            </xsl:otherwise>
                        </xsl:choose>
                    </xsl:when>
                    <xsl:otherwise>
                        <xsl:sequence select="local:get-variants(.)"/>
                    </xsl:otherwise>
                </xsl:choose>
            </xsl:variable>
            <section
                title="{$topic-shortName}{if (exists(@level) and @level ne 'Inc') then concat(' (',local:prod-desc-level(@level),')') else ''}"
                variant="{string-join($variants, ' ')}">
                <!-- Feature descriptions may differ by product.
                    The primary mechanism for this is to use multiple products/product elements:
                    these result in multiple rows in the Feature Matrix table, as well as
                    alternative descriptions in the Product Descriptions.
                    The second mechanism is to use description and alt-description
                    elements. These will result in alternative descriptions in the PDs, but not in
                    the Feature Matrix. (e.g. used to add info for 'Import stylesheet packages' when
                    product does not also have 'Export' feature) -->
                <xsl:choose>
                    <xsl:when test="exists(description[not(@generic)])">
                        <xsl:copy-of select="description/*"/>
                    </xsl:when>
                    <xsl:when test="alt-description">
                        <xsl:variable name="feature" select="ancestor::feature"/>
                        <xsl:for-each select="description[not(@generic)]">
                            <xsl:variable name="variants" select="local:get-variants(.)" as="xs:string*"/>
                            <div variant="{string-join($variants, ' ')}">
                                <xsl:copy-of select="./*"/>
                            </div>
                        </xsl:for-each>
                        <xsl:for-each select="alt-description">
                            <xsl:variable name="variants" select="local:get-variants-range(., ancestor::product/description)" as="xs:string*"/>
                            <div variant="{string-join($variants, ' ')}">
                                <xsl:copy-of select="./*"/>
                            </div>
                        </xsl:for-each>
                    </xsl:when>
                    <xsl:when test="ancestor::feature/alt-description">
                        <xsl:variable name="feature" select="ancestor::feature"/>
                        <xsl:for-each select="$feature/description">
                            <xsl:variable name="variants" select="local:get-variants(.)" as="xs:string*"/>
                            <div variant="{string-join($variants, ' ')}">
                                <xsl:copy-of select="./*"/>
                            </div>
                        </xsl:for-each>
                        <xsl:for-each select="$feature/alt-description">
                            <xsl:variable name="variants" select="local:get-variants-range(., $feature/description)" as="xs:string*"/>
                            <div variant="{string-join($variants, ' ')}">
                                <xsl:copy-of select="./*"/>
                            </div>
                        </xsl:for-each>
                    </xsl:when>
                    <xsl:otherwise>
                        <xsl:copy-of select="ancestor::feature/description/*"/>
                    </xsl:otherwise>
                </xsl:choose>
                <!-- The details and spec links are the same for all (optional) features of the same
                    language (i.e. XSLT 3.0, XPath 3.1 and XQuery 3.1), and so do not need to be
                    repeated. These are included instead at the Language level using the
                    'language-section' template. -->
                <xsl:if test="empty($lang) and exists($details)">
                    <p variant="{string-join($variants[. ne 'JS'], ' ')}">For more details see: <xsl:for-each select="$details[@link]">
                        <xsl:variable name="ext" select="if (./@subpages='has-subpages') then
                            '/index.html' else '.html'"/>
                            <a href="https://www.saxonica.com/html/documentation{$version}/{./@link}{$ext}"
                                class="bodylink"><xsl:value-of select="./@name"/></a>
                            <xsl:if test="not(position() = last())">
                                <xsl:text>, </xsl:text>
                            </xsl:if>
                        </xsl:for-each>. </p>
                    <xsl:if test="$variants = 'JS' and exists($details[@JSlink])">
                        <p variant="JS">For more details see: <xsl:for-each select="$details[@JSlink]">
                            <a href="https://www.saxonica.com/saxon-js/documentation/index.html#!{./@JSlink}"
                                class="bodylink"><xsl:value-of select="./@JSname"/></a>
                            <xsl:if test="not(position() = last())">
                                <xsl:text>, </xsl:text>
                            </xsl:if>
                        </xsl:for-each>. </p>
                    </xsl:if>
                </xsl:if>
                <xsl:if test="empty($lang) and exists($spec)">
                    <p>
                        <xsl:text>Relevant W3C Specification: </xsl:text>
                        <a href="{$spec/@link}" class="bodylink">
                            <xsl:value-of
                                select="concat(($spec/@name, $topic-shortName)[1], ' ', $spec/@level, ' (', $spec/@date, ')')"
                            />
                        </a>.                        
                    </p>
                </xsl:if>
            </section>
        </xsl:for-each>
    </xsl:template>
    
    <xsl:template name="language-section">
        <xsl:param name="topic"/>
        <!-- Add a paragraph (conditional on edition variant) listing excluded features -->
        <xsl:variable name="features" select="feature[@class eq $topic]"/>
        <xsl:variable name="JS-relevant" select="$topic ne 'XQuery'" as="xs:boolean"/>
        <xsl:for-each select="$all-variants">
            <xsl:variable name="platform" select="tokenize(., '-')[1]"/>
            <xsl:variable name="edition" select="tokenize(., '-')[2]"/>
            <xsl:variable name="exclusions" as="element(feature)*">
                <xsl:choose>
                    <xsl:when test="$platform eq 'J'">
                        <xsl:sequence
                            select="$features[not(tokenize(local:editions-from-minimal(products/product/@J),
                            '\s') = $edition)]"/>
                    </xsl:when>
                    <xsl:when test="$platform eq 'CS'">
                        <xsl:sequence
                            select="$features[not(tokenize(local:editions-from-minimal(products/product/@CS),
                            '\s') = $edition)]"/>
                    </xsl:when>
                    <xsl:when test="$platform eq 'C'">
                        <xsl:sequence
                            select="$features[not(tokenize(local:editions-from-minimal(products/product/@C),
                            '\s') = $edition)]"/>
                    </xsl:when>
                    <xsl:when test="$platform eq 'JS' and $JS-relevant">
                        <xsl:sequence
                            select="$features[not(tokenize(local:editions-from-minimal(products/product/@JS),
                            '\s') = 'HE')]"/>
                    </xsl:when>
                </xsl:choose>
            </xsl:variable>
            <xsl:if test="exists($exclusions)">
                <p variant="{.}">Optional features not provided:
                    <xsl:for-each select="$exclusions">
                        <xsl:value-of select="shortName"/>
                        <xsl:if test="not(position() = last())">
                            <xsl:text>, </xsl:text>
                        </xsl:if>
                    </xsl:for-each>.
                </p>
            </xsl:if>
        </xsl:for-each>
        <!-- Include details and spec links for the language (i.e. XSLT 3.0, XPath 3.1 and XQuery
            3.1) at this level. -->
        <xsl:variable name="details" select="$features[@lang][1]/details"/>
        <xsl:variable name="spec" select="$features[@lang][1]/spec"/>
        <xsl:if test="exists($details)">
            <p variant="{string-join($all-variants[. ne 'JS'], ' ')}">For more details see: <xsl:for-each select="$details[@link]">
                <xsl:variable name="ext" select="if (./@subpages='has-subpages') then
                    '/index.html' else '.html'"/>
                <a href="https://www.saxonica.com/html/documentation{$version}/{./@link}{$ext}"
                    class="bodylink"><xsl:value-of select="./@name"/></a>
                <xsl:if test="not(position() = last())">
                    <xsl:text>, </xsl:text>
                </xsl:if>
            </xsl:for-each>. </p>
            <xsl:if test="$JS-relevant">
                <p variant="JS">For more details see: <xsl:for-each select="$details[@JSlink]">
                    <a href="https://www.saxonica.com/saxon-js/documentation/index.html#!{./@JSlink}"
                        class="bodylink"><xsl:value-of select="./@JSname"/></a>
                    <xsl:if test="not(position() = last())">
                        <xsl:text>, </xsl:text>
                    </xsl:if>
                </xsl:for-each>. </p>
            </xsl:if>
        </xsl:if>
        <xsl:if test="exists($spec)">
            <p>
                <xsl:if test="not($JS-relevant)">
                    <xsl:attribute name="variant" select="string-join($all-variants[. ne 'JS'], ' ')"/>
                </xsl:if>
                <xsl:text>Relevant W3C Specification: </xsl:text>
                <a href="{$spec/@link}" class="bodylink">
                    <xsl:value-of
                        select="concat($spec/@name, ' ', $spec/@level, ' (', $spec/@date, ')')"
                    />
                </a>.                        
            </p>
        </xsl:if>
    </xsl:template>

    <xsl:template name="header-section">
        <section class="header">
            <table>
                <tbody>
                    <tr>
                        <td class="important">
                            <xsl:for-each select="$all-variants">
                                <title variant="{.}">
                                    <xsl:value-of select="concat('Product Description for ', local:variant-name(.))"/>
                                </title>
                            </xsl:for-each>
                        </td>
                        <td>
                            <p class="right">
                                <img src="../../saxonica_logo.gif"/>
                                <br/>
                                <!--<span value="$timestamp"/>
              <br/>-->
                                <span value="$version"/>
                                <!-- This is an example of doing a page number - 
        the use-when="PDF" means it will only appear in the PDF version
        Normal styling can be achieved through CSS, 
        but the <page-number/> substitute doesn't have any properties - use <span> around to style.
        To change the page number format itself look in config.xml
      -->
                                <span class="page-number" use-when="PDF">
                                    <br/>Page <page-number/>/<total-pages/></span>
                            </p>
                        </td>
                    </tr>
                </tbody>
            </table>
        </section>
    </xsl:template>

    <xsl:template name="doc-desc">
        <xsl:for-each select="$all-variants">
            <p variant="{.}">
                <xsl:value-of select="concat('This document lists the features provided by ',
                    local:variant-desc(.), '.')"/>
            </p>
        </xsl:for-each>
        <p>This document does not form part of any contract unless expressly incorporated.</p>
    </xsl:template>

    <xsl:template name="footer-section">
        <section class="footer">
            <hr/>
            <table>
                <tbody>
                    <tr>
                        <xsl:for-each select="$all-variants">
                            <td class="footerName" variant="{.}">
                                <xsl:value-of select="concat('Product Description for ', local:variant-name(.))"/>
                            </td>
                        </xsl:for-each>
                        <td class="copyright right">
                            <xsl:value-of select="concat('© Saxonica Ltd. ', format-dateTime(current-dateTime(),'[Y]'))"/>
                        </td>
                    </tr>
                </tbody>
            </table>
        </section>
    </xsl:template>
    
    <xsl:function name="local:variant-name" as="xs:string">
        <xsl:param name="variant" as="xs:string"/>
        <xsl:variable name="platform" select="tokenize($variant, '-')[1]"/>
        <xsl:variable name="edition" select="tokenize($variant, '-')[2]"/>
        <xsl:variable name="package-name">
            <xsl:choose>
                <xsl:when test="$edition = 'HE'">(Home Edition)</xsl:when>
                <xsl:when test="$edition = 'PE'">(Professional Edition)</xsl:when>
                <xsl:when test="$edition = 'EE'">(Enterprise Edition)</xsl:when>
                <xsl:when test="$edition = 'EET'">(Enterprise Edition, Transformation Package)</xsl:when>
                <xsl:when test="$edition = 'EEQ'">(Enterprise Edition, Query Package)</xsl:when>
                <xsl:when test="$edition = 'EEV'">(Enterprise Edition, Validation Package)</xsl:when>
                <xsl:otherwise>
                    <xsl:value-of select="'&#xa0;'"/>
                </xsl:otherwise>
            </xsl:choose>
        </xsl:variable>
        <xsl:choose>
            <xsl:when test="$platform eq 'JS'">
                <xsl:value-of select="'SaxonJS'"/>
            </xsl:when>
            <xsl:otherwise>
                <xsl:value-of select="concat('Saxon', $variant, ' ', $package-name)"/>
            </xsl:otherwise>
        </xsl:choose>
    </xsl:function>
    
    <xsl:function name="local:variant-desc" as="xs:string">
        <xsl:param name="variant" as="xs:string"/>
        <xsl:variable name="platform" select="tokenize($variant, '-')[1]"/>
        <xsl:variable name="edition" select="tokenize($variant, '-')[2]"/>
        <xsl:variable name="package-desc">
            <xsl:choose>
                <xsl:when test="$platform eq 'JS'"></xsl:when>
                <xsl:when test="$edition = 'HE'"> open source Home Edition (Saxon-HE)</xsl:when>
                <xsl:when test="$edition = 'PE'"> Professional Edition (Saxon-PE)</xsl:when>
                <xsl:when test="$edition = 'EE'"> Enterprise Edition (Saxon-EE)</xsl:when>
                <xsl:when test="$edition = 'EET'"> Enterprise Edition, Transformation Package 
                    (Saxon-EET) - a license that enables all the facilities of Saxon Professional
                    Edition, plus XSD schema validation, plus enterprise features (including Schema
                    Awareness) for XSLT (but not XQuery)</xsl:when>
                <xsl:when test="$edition = 'EEQ'"> Enterprise Edition, Query Package 
                    (Saxon-EEQ) - a license that enables all the facilities of Saxon Professional
                    Edition, plus XSD schema validation, plus enterprise features (including Schema
                    Awareness) for XQuery (but not XSLT)</xsl:when>
                <xsl:when test="$edition = 'EEV'"> Enterprise Edition, Validation Package 
                    (Saxon-EEV) - a license that enables all the facilities of Saxon Professional
                    Edition, plus XSD schema validation; it does not include Schema Aware XSLT or
                    XQuery</xsl:when>
                <xsl:otherwise>
                    <xsl:value-of select="'&#xa0;'"/>
                </xsl:otherwise>
            </xsl:choose>
        </xsl:variable>
        <xsl:variable name="platform-desc">
            <xsl:choose>
                <xsl:when test="$platform = 'J'">, for Java</xsl:when>
                <xsl:when test="$platform = 'CS'">, supported on .NET 5</xsl:when>
                <xsl:when test="$platform = 'C'">, for C/C++, PHP and Python</xsl:when>
                <xsl:when test="$platform = 'JS'">, for JavaScript in the browser and on Node.js</xsl:when>
                <xsl:otherwise>
                    <xsl:value-of select="'&#xa0;'"/>
                </xsl:otherwise>
            </xsl:choose>
        </xsl:variable>
        <xsl:variable name="version-number">
            <xsl:choose>
                <xsl:when test="$platform = 'C'"><xsl:value-of select="$c-version"/></xsl:when>
                <xsl:when test="$platform = 'JS'"><xsl:value-of select="$js-version"/></xsl:when>
                <xsl:otherwise>
                    <xsl:value-of select="$version"/>
                </xsl:otherwise>
            </xsl:choose>
        </xsl:variable>
        <xsl:value-of select="concat('Saxon', $platform, ' ', $version-number, $package-desc, $platform-desc)"/>
    </xsl:function>

    <xsl:function name="local:prod-desc-level" as="xs:string">
        <xsl:param name="level" as="xs:string?"/>
        <xsl:choose>
            <xsl:when test="contains($level, 'Adv')">Advanced</xsl:when>
            <xsl:when test="contains($level, 'SA') and contains($level, 'Strm')">Schema Aware &amp; Streaming</xsl:when>
            <xsl:when test="contains($level, 'SA')">Schema Aware</xsl:when>
            <xsl:when test="contains($level, 'HOF')">Higher-Order Functions</xsl:when>
            <xsl:when test="contains($level, 'Basic')">Basic</xsl:when>
            <xsl:when test="contains($level, 'Open')">not included, but open source</xsl:when>
            <xsl:when test="contains($level, 'Inc')">Included</xsl:when>
            <xsl:otherwise>
                <xsl:value-of select="'&#xa0;'"/>
            </xsl:otherwise>
        </xsl:choose>
    </xsl:function>
    
    <xsl:function name="local:editions-from-minimal" as="xs:string">
        <xsl:param name="minEdition" as="xs:string?"/>
        <xsl:variable name="ordered-editions" select="'HE PE EEV EET EEQ EE'"/>
        <xsl:choose>
            <xsl:when test="empty($minEdition)"><xsl:value-of select="''"/></xsl:when>
            <xsl:when test="contains($minEdition, 'EET') and contains($minEdition, 'EEQ')">EET EEQ EE</xsl:when>
            <xsl:when test="contains($minEdition, 'EET') and not(contains($minEdition, 'EEQ'))">EET EE</xsl:when>
            <xsl:when test="contains($ordered-editions, $minEdition)"><xsl:value-of
                select="concat($minEdition, substring-after($ordered-editions, $minEdition))"/></xsl:when>
            <xsl:otherwise>
                <xsl:value-of select="''"/>
            </xsl:otherwise>
        </xsl:choose>
    </xsl:function>
    
    <xsl:function name="local:get-variants" as="xs:string*">
        <xsl:param name="block" as="element()"/>
        <xsl:sequence>
            <xsl:sequence select="tokenize(local:editions-from-minimal($block/@J))!concat('J-',.)"/>
            <xsl:sequence select="tokenize(local:editions-from-minimal($block/@CS))!concat('CS-',.)"/>
            <xsl:sequence select="tokenize(local:editions-from-minimal($block/@C))!concat('C-',.)"/>
            <xsl:if test="$block/@JS"><xsl:value-of select="'JS'"/></xsl:if>
        </xsl:sequence>
    </xsl:function>
    
    <xsl:function name="local:get-variants-range" as="xs:string*">
        <xsl:param name="block1" as="element()"/>
        <xsl:param name="block2" as="element()"/>
        <xsl:variable name="lower" as="xs:string*">
            <xsl:sequence select="tokenize(local:editions-from-minimal($block1/@J))!concat('J-',.)"/>
            <xsl:sequence select="tokenize(local:editions-from-minimal($block1/@CS))!concat('CS-',.)"/>
            <xsl:sequence select="tokenize(local:editions-from-minimal($block1/@C))!concat('C-',.)"/>
            <xsl:if test="$block1/@JS"><xsl:value-of select="'JS'"/></xsl:if>
        </xsl:variable>
        <xsl:variable name="upper" as="xs:string*">
            <xsl:sequence select="tokenize(local:editions-from-minimal($block2/@J))!concat('J-',.)"/>
            <xsl:sequence select="tokenize(local:editions-from-minimal($block2/@CS))!concat('CS-',.)"/>
            <xsl:sequence select="tokenize(local:editions-from-minimal($block2/@C))!concat('C-',.)"/>
            <xsl:if test="$block2/@JS"><xsl:value-of select="'JS'"/></xsl:if>
        </xsl:variable>
        <xsl:sequence select="$lower[not(. = $upper)]"/>
    </xsl:function>

</xsl:stylesheet>
