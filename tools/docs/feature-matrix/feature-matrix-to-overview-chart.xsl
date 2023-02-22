<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
    xmlns:xs="http://www.w3.org/2001/XMLSchema" xmlns:f="http://local-functions/"
    exclude-result-prefixes="xs f" version="2.0">

    <xsl:output method="xhtml" indent="yes"/>
    
    <xsl:variable name="version" select="featureMatrix/@version"/>

    <xsl:template match="featureMatrix">
        <xsl:processing-instruction name="xml-stylesheet">href="../make-menu.xsl" type="text/xsl"</xsl:processing-instruction>
        <xsl:variable name="documentation-version" select="concat('documentation',$version)"/>
        <xsl:variable name="page" select="concat('feature-matrix-',replace($version,'\.','-'))"/>
        <html>
            <head>
                <this-is section="products" page="@@feature-matrix-X@@" subpage="{$page}"/>
                <meta http-equiv="Content-Type" content="text/html; charset=UTF-8" />
                <meta name="description" content="the full range of Saxon products and features implementing four W3C defined languages on four technology platforms such as Java, .NET, Native, JavaScript" />
                <meta name="keywords" content="XSLT, XQuery, XSD, XPath, Java, .NET, Native, JavaScript" />
                <meta name="coverage" content="Worldwide" />
                <meta name="copyright" content="Copyright Saxonica Ltd" />
                <meta name="robots" content="follow" />
                <title>Saxonica: Saxon Product/Feature Matrix</title>
            </head>
            <body class="main">
                <h1>Saxon <xsl:value-of select="$version"/> product comparison (Feature Matrix)</h1>
                <p>This chart describes the features present in each edition of Saxon <xsl:value-of
                    select="$version"/> available for Java and .NET, and in each edition for the
                    latest releases of SaxonC and SaxonJS.</p>
                <p><b>At the time of publication, not all editions of Saxon 11 are available. The information
                for editions that are not yet available is provisional and subject to change.</b></p>
                <p>
                    <i>For features in older versions of the products, see the relevant Feature Matrix:<br />
                        <xsl:for-each select="(10, 9.9, 9.8, 9.7, 9.6)[number($version) ge .]">
                            <a class="bodylink" href="feature-matrix-{replace(string(.), '\.', '-')}.xml">Saxon <xsl:value-of select="string(.)"/></a>
                            <xsl:if test="not(position()=last())">
                                <xsl:text>, </xsl:text>
                            </xsl:if>
                        </xsl:for-each>
                        <xsl:text>.</xsl:text>
                    </i>
                </p>
                <p>The table below lists product features and indicates which features are available for
                    products on the different platforms. If the feature is restricted to certain packages on
                    a platform, then the table shows the <b>minimal</b> product package(s) required. The
                    product packages are ordered as: PE &#x2282; EE-V &#x2282; (EE-T | EE-Q) &#x2282; EE. For
                    instance, this means all PE and EE-V features are available with all EE packages.</p>
                <p>'&#x25cf;' means the feature is available for all editions on that platform.</p>
                <table class="feature-matrix feature-matrix-11" id="overview">
                        <tbody>
                            <xsl:for-each select="feature">
                                <xsl:variable name="feature-desc" select="description"/>
                                <xsl:choose>
                                    <xsl:when test="./@class = 'heading'">
                                        <tr class="feature-heading">
                                            <td style="text-align:left;">
                                                <xsl:value-of select="shortName"/> 
                                            </td>
                                            <td>Java SaxonJ</td>
                                            <td>.NET SaxonCS</td>
                                            <td>Native SaxonC</td>
                                            <td>JavaScript SaxonJS</td>
                                        </tr>
                                    </xsl:when>
                                    <xsl:otherwise>
                                        <xsl:variable name="topic-shortName" select="shortName"/>
                                        <xsl:variable name="details" select="details"/>
                                        <xsl:variable name="spec" select="spec"/>
                                        <xsl:for-each select="products/product[@editions and (not(exists(@level)) or @level ne 'Open')]">
                                            <xsl:variable name="editions" as="xs:string*">
                                                <xsl:choose>
                                                    <xsl:when test="exists(@level)">
                                                        <xsl:variable name="level" as="xs:string" select="tokenize(@level, '\s')[position() = last()]"/>                                                        
                                                        <xsl:for-each select="ancestor::products/product[$level = tokenize(@level, '\s')]">
                                                            <xsl:sequence select="tokenize(@editions, '\s')"/>
                                                        </xsl:for-each>
                                                    </xsl:when>
                                                    <xsl:otherwise>
                                                        <xsl:sequence select="tokenize(@editions, '\s')"/>
                                                    </xsl:otherwise>
                                                </xsl:choose>
                                            </xsl:variable>
                                        <tr>
                                            <td class="topic">
                                                <xsl:variable name="feature-with-level" as="xs:string" 
                                                    select="concat($topic-shortName, if (exists(@level) and @level ne 'Inc') then concat(' (',f:prod-desc-level(@level),')') else '')"/>
                                                <div class="popup2"> 
                                                    <xsl:value-of select="$feature-with-level"/>  
                                                    <span>
                                                        <h2><xsl:value-of select="$feature-with-level"/></h2>
                                                        <xsl:choose>
                                                            <xsl:when test="exists(description)">
                                                                <xsl:copy-of select="description/*"/>
                                                            </xsl:when>
                                                            <xsl:otherwise>
                                                                <xsl:copy-of select="$feature-desc/*"/>
                                                            </xsl:otherwise>
                                                        </xsl:choose>
                                                        <xsl:if test="exists($details)">
                                                            <p>
                                                                <xsl:text>Further info: </xsl:text>
                                                                <xsl:for-each
                                                                    select="$details">
                                                                    <a
                                                                        href="{concat('../', $documentation-version, '/#!', @link)}"
                                                                        class="bodylink {@subpages}">
                                                                        <xsl:value-of select="@name"/>
                                                                    </a>
                                                                    <xsl:if test="not(position()=last())">
                                                                        <xsl:text>, </xsl:text>
                                                                    </xsl:if>
                                                                </xsl:for-each>
                                                            </p>
                                                        </xsl:if> 
                                                        <xsl:if test="exists($spec)">
                                                            <p><a href="{$spec/@link}" target="_blank">
                                                                <i><xsl:value-of
                                                                    select="concat(($spec/@name, $topic-shortName)[1],
                                                                    ' ', $spec/@level, ' (', $spec/@date, ')')"/></i>
                                                            </a>
                                                            </p>                                                               
                                                        </xsl:if>
                                                    </span>                                                    
                                                </div>                                                
                                            </td>
                                            <td>
                                                <p>                                                    
                                                  <xsl:sequence select="f:license(java)"/>
                                                </p>
                                            </td>
                                            <td>
                                                <p>
                                                    <xsl:sequence select="f:licenseCS(cs)"/>
                                                </p>
                                            </td>
                                            <td>
                                                <p>
                                                    <xsl:sequence select="f:license(c)"/>
                                                </p>
                                            </td>
                                            <td>
                                                <p>
                                                    <xsl:sequence select="f:licenseJS(js)"/>
                                                </p>
                                            </td>
                                        </tr>
                                        </xsl:for-each>
                                    </xsl:otherwise>
                                </xsl:choose>
                            </xsl:for-each>
                        </tbody>
                    </table>
                
                <xsl:call-template name="aside"/>
                
                    
                    
            </body>
        </html>
    </xsl:template>
    
    <xsl:template name="aside">
        <xsl:choose>
            <xsl:when test="$version = ('10')">
                <div id="aside">
                    <h2>View the full product descriptions</h2>
                    <ul>
                        <li><a class="title" target="_blank"
                            href="http://www.saxonica.com/products/PD{$version}/HE.pdf">
                            <b>HE: </b>Saxon Home Edition (open source)</a></li>
                        <li><a class="title" target="_blank" href="http://www.saxonica.com/products/PD{$version}/PE.pdf">
                            <b>PE: </b>Saxon Professional Edition</a></li>
                        <li><a class="title" target="_blank" href="http://www.saxonica.com/products/PD{$version}/EE.pdf">
                            <b>EE: </b>Saxon Enterprise Edition</a></li>
                        <li><a class="title" target="_blank" href="http://www.saxonica.com/products/PD{$version}/EE-T.pdf">
                            <b>EE-T: </b>Saxon Enterprise Edition, Transformation package 
                            (restricted license allowing XSLT
                            and Schema processing only)</a></li>
                        <li><a class="title" target="_blank" href="http://www.saxonica.com/products/PD{$version}/EE-Q.pdf">
                            <b>EE-Q: </b>Saxon Enterprise Edition, Query package
                            (restricted license allowing
                            XQuery and Schema processing only)</a></li>
                        <li><a class="title" target="_blank" href="http://www.saxonica.com/products/PD{$version}/EE-V.pdf">
                            <b>EE-V: </b>Saxon Enterprise Edition, Validation package
                            (restricted license allowing
                            Schema processing only)</a></li>
                        <li><a class="title" target="_blank" href="http://www.saxonica.com/products/PD{$version}/CS.pdf">
                            <b>CS: </b>Saxon Enterprise Edition for .NET 5</a></li>
                        <li><a class="title" href="../saxon-js/index.xml"><b>JS2: </b>SaxonJS 2, JavaScript processor</a></li>
                    </ul>
                </div>
            </xsl:when>
            <xsl:otherwise>
                <div id="aside">
                    <h2>Packages by platform</h2>
                    <p><b>Java: Note that SaxonJ 11 is not yet released.</b></p>
                    <p><b>.NET: SaxonCS for .NET 5 is only available as the Enterprise Edition.</b></p>
                    <p><b>Native:</b> The latest version is SaxonC 1.2 built from Saxon 9.9.1.</p>
                    <p><b>JavaScript:</b> SaxonJS 2 does not require a commercial license.</p>
                </div>
                
                <div id="aside">
                    <h2>Product packages</h2>
                    <p>Editions available for SaxonJ and SaxonC:</p>
                    <ul>
                        <li>
                            <b>HE: </b>Saxon Home Edition (open source)</li>
                        <li>
                            <b>PE: </b>Saxon Professional Edition</li>
                    </ul>
                    <p>Enterprise Edition packages for SaxonJ, SaxonCS, and SaxonC:</p>
                    <ul>
                        <li>
                            <b>EE: </b>Saxon Enterprise Edition</li>
                        <li>
                            <b>EE-T: </b>Saxon Enterprise Edition, Transformation package 
                            (restricted license allowing XSLT
                            and Schema processing only)</li>
                        <li>
                            <b>EE-Q: </b>Saxon Enterprise Edition, Query package
                            (restricted license allowing
                            XQuery and Schema processing only)</li>
                        <li>
                            <b>EE-V: </b>Saxon Enterprise Edition, Validation package
                            (restricted license allowing
                            Schema processing only)</li>
                    </ul>
                </div>
            </xsl:otherwise>
        </xsl:choose>
        
    </xsl:template>
    
    <xsl:function name="f:license" as="xs:string">
        <xsl:param name="platform" as="element()?"/>
        <xsl:choose>
            <xsl:when test="empty($platform)">
                <xsl:value-of select="'&#xa0;'"/>
            </xsl:when>
            <xsl:when test="$platform/@editions = 'HE'">
                <xsl:value-of select="'&#x25cf;'"/>
            </xsl:when>
            <xsl:otherwise>
                <xsl:value-of select="$platform/@editions"/>
            </xsl:otherwise>
        </xsl:choose>
    </xsl:function>
    
    <xsl:function name="f:licenseCS" as="xs:string">
        <xsl:param name="platform" as="element()?"/>
        <xsl:choose>
            <xsl:when test="empty($platform)">
                <xsl:value-of select="'&#xa0;'"/>
            </xsl:when>
            <xsl:when test="$platform/@editions = ('HE','PE','EE-V')">
                <xsl:value-of select="'&#x25cf;'"/>
            </xsl:when>
            <xsl:otherwise>
                <xsl:value-of select="$platform/@editions"/>
            </xsl:otherwise>
        </xsl:choose>
    </xsl:function>
    
    <xsl:function name="f:licenseJS">
        <xsl:param name="platform" as="element()?"/>
        <xsl:choose>
            <xsl:when test="empty($platform)">
                <xsl:value-of select="'&#xa0;'"/>
            </xsl:when>
            <xsl:otherwise>
                <xsl:value-of select="'&#x25cf;'"/>
            </xsl:otherwise>
        </xsl:choose>
    </xsl:function>
    
    <xsl:function name="f:prod-desc-level" as="xs:string">
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
