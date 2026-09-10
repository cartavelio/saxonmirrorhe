<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
    xmlns:xs="http://www.w3.org/2001/XMLSchema" xmlns:local="http://local-functions/"
    exclude-result-prefixes="xs local" version="2.0">
    
    <!-- Use this stylesheet to generate the output prod-desc.xml
        (the source document for the product descriptions), 
        from the input tools/docs/feature-matrix/feature-matrix.xml-->
<!--    2021-03-18 TODO Use of documentation{$version} for all links is dependent on website redirects -->

    <xsl:output method="xml" indent="yes"/>
    
    <xsl:variable name="edition" as="xs:string*" select="('HE', 'PE', 'EE', 'EE-T', 'EE-Q', 'EE-V', 'CS')"/>
    <xsl:variable name="version" select="featureMatrix/@version"/>

    <xsl:template match="featureMatrix">
        <xsl:variable name="release-date" select="@release"/>
        <doc version="Version {$version} released {$release-date}">
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
        <xsl:for-each select="feature[@class eq $topic]/products/product[@editions]">
            <xsl:variable name="topic-shortName" select="ancestor::feature/shortName"/>
            <xsl:variable name="editions" select="@editions"/>
            <xsl:variable name="details" select="ancestor::feature/details"/>
            <xsl:variable name="spec" select="ancestor::feature/spec"/>
            <xsl:variable name="lang" select="ancestor::feature/@lang"/>
            <section
                title="{$topic-shortName}{if (exists(@level) and @level ne 'Inc') then concat(' (',local:prod-desc-level(@level),')') else ''}"
                variant="{$editions}">
                <!-- Feature descriptions may differ by product.
                    The primary mechanism for this is to use multiple products/product elements:
                    these result in multiple rows in the Feature Matrix table, as well as
                    alternative descriptions in the Product Descriptions.
                    The second mechanism is to use description/@editions and alt-description
                    elements. These will result in alternative descriptions in the PDs, but not in
                    the Feature Matrix. (e.g. used to add info for 'Import stylesheet packages' when
                    product does not also have 'Export' feature) -->
                <xsl:choose>
                    <xsl:when test="exists(description)">
                        <xsl:copy-of select="description/*"/>
                    </xsl:when>
                    <xsl:when test="ancestor::feature/description/@editions">
                        <xsl:variable name="feature" select="ancestor::feature"/>
                        <div variant="{$feature/description/@editions}">
                            <xsl:copy-of select="$feature/description/*"/>
                        </div>
                        <xsl:for-each select="$feature/alt-description">
                            <div variant="{./@editions}">
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
                    <p>For more details see: <xsl:for-each select="$details">
                        <xsl:variable name="ext" select="if (./@subpages='has-subpages') then
                            '/index.html' else '.html'"/>
                            <a href="https://www.saxonica.com/html/documentation{$version}/{./@link}{$ext}"
                                class="bodylink"><xsl:value-of select="./@name"/></a>
                            <xsl:if test="not(position() = last())">
                                <xsl:text>, </xsl:text>
                            </xsl:if>
                        </xsl:for-each>. </p>
                    
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
        <xsl:for-each select="$edition">
            <xsl:variable name="exclusions"
                select="$features[not(tokenize(products/product/@editions, '\s') = current())]"/>
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
            <p>For more details see: <xsl:for-each select="$details">
                <xsl:variable name="ext" select="if (./@subpages='has-subpages') then
                    '/index.html' else '.html'"/>
                <a href="https://www.saxonica.com/html/documentation{$version}/{./@link}{$ext}"
                    class="bodylink"><xsl:value-of select="./@name"/></a>
                <xsl:if test="not(position() = last())">
                    <xsl:text>, </xsl:text>
                </xsl:if>
            </xsl:for-each>. </p>
        </xsl:if>
        <xsl:if test="exists($spec)">
            <p>
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
                            <xsl:for-each select="$edition">
                                <title variant="{.}">
                                    <xsl:value-of select="concat('Product Description for ', local:edition-name(.))"/>
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
        <!-- do we need any caveats here about what product description conforms to?-->     
        
        <!-- This document lists the features that Saxon-X supports, and describes the level of that support.-->
        <!-- This document lists the features supported by Saxon-X, and describes the level of that support.-->
        <!-- This document lists the features provided by Saxon 9.x -X.-->   
        <xsl:for-each select="$edition">
            <p variant="{.}">
                <xsl:value-of select="concat('This document lists the features provided by Saxon ',
                    $version, ' ', local:edition-desc(.), '.')"/>
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
                        <xsl:for-each select="$edition">
                            <td class="footerName" variant="{.}">
                                <xsl:value-of select="concat('Product Description for ', local:edition-name(.))"/>
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
    
    <xsl:function name="local:edition-name" as="xs:string">
        <xsl:param name="edition" as="xs:string"/>
        <xsl:choose>
            <xsl:when test="$edition = 'HE'">Saxon-HE (Home Edition)</xsl:when>
            <xsl:when test="$edition = 'PE'">Saxon-PE (Professional Edition)</xsl:when>
            <xsl:when test="$edition = 'EE'">Saxon-EE (Enterprise Edition)</xsl:when>
            <xsl:when test="$edition = 'EE-T'">Saxon-EE-T (Enterprise Edition, Transformation Package)</xsl:when>
            <xsl:when test="$edition = 'EE-Q'">Saxon-EE-Q (Enterprise Edition, Query Package)</xsl:when>
            <xsl:when test="$edition = 'EE-V'">Saxon-EE-V (Enterprise Edition, Validation Package)</xsl:when>
            <xsl:when test="$edition = 'CS'">SaxonCS</xsl:when>
            <xsl:otherwise>
                <xsl:value-of select="'&#xa0;'"/>
            </xsl:otherwise>
        </xsl:choose>
    </xsl:function>
    
    <xsl:function name="local:edition-desc" as="xs:string">
        <xsl:param name="edition" as="xs:string"/>
        <xsl:choose>
            <xsl:when test="$edition = 'HE'">open source Home Edition (Saxon-HE)</xsl:when>
            <xsl:when test="$edition = 'PE'">Professional Edition (Saxon-PE)</xsl:when>
            <xsl:when test="$edition = 'EE'">Enterprise Edition (Saxon-EE)</xsl:when>
            <xsl:when test="$edition = 'EE-T'">Enterprise Edition, Transformation Package 
                (Saxon-EE-T) - a license that enables all the facilities of Saxon Professional
                Edition, plus XSD schema validation, plus enterprise features (including Schema
                Awareness) for XSLT (but not XQuery)</xsl:when>
            <xsl:when test="$edition = 'EE-Q'">Enterprise Edition, Query Package 
                (Saxon-EE-Q) - a license that enables all the facilities of Saxon Professional
                Edition, plus XSD schema validation, plus enterprise features (including Schema
                Awareness) for XQuery (but not XSLT)</xsl:when>
            <xsl:when test="$edition = 'EE-V'">Enterprise Edition, Validation Package 
                (Saxon-EE-V) - a license that enables all the facilities of Saxon Professional
                Edition, plus XSD schema validation; it does not include Schema Aware XSLT or
                XQuery</xsl:when>
            <xsl:when test="$edition = 'CS'">SaxonCS, supported on .NET 5</xsl:when>
            <xsl:otherwise>
                <xsl:value-of select="'&#xa0;'"/>
            </xsl:otherwise>
        </xsl:choose>
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

</xsl:stylesheet>
