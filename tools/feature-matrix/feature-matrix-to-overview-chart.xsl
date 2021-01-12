<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
    xmlns:xs="http://www.w3.org/2001/XMLSchema" xmlns:f="http://local-functions/"
    exclude-result-prefixes="xs f" version="2.0">

    <xsl:output method="xhtml" indent="yes"/>
    
    <xsl:param name="major-version"/>
    <xsl:variable name="version" select="featureMatrix/@version"/>

    <xsl:template match="featureMatrix">
        <xsl:processing-instruction name="xml-stylesheet">href="../make-menu.xsl" type="text/xsl"</xsl:processing-instruction>
        <xsl:variable name="documentation-version">
            <xsl:choose>
                <xsl:when test="$version = $major-version">                    
                    <xsl:value-of select="'documentation'"/>
                </xsl:when>
                <xsl:otherwise>
                    <xsl:value-of select="concat('documentation',$version)"/>
                </xsl:otherwise>
            </xsl:choose>
        </xsl:variable>
        <xsl:variable name="page" select="concat('feature-matrix-',replace($version,'\.','-'))"/>
        <html>
            <head>
                <xsl:choose>
                    <xsl:when test="$version = $major-version">
                        <this-is section="products" page="{$page}" subpage=""/>                        
                    </xsl:when>
                    <xsl:otherwise>
                        <this-is section="products" page="{concat('feature-matrix-',replace($major-version,'\.','-'))}" subpage="{$page}"/>
                    </xsl:otherwise>
                </xsl:choose>
                <meta http-equiv="Content-Type" content="text/html; charset=UTF-8"/>
                <title>Saxonica: Saxon Product/Feature Matrix</title>
            </head>
            <body class="main">
                <h1>Saxon <xsl:value-of select="$version"/> product comparison (Feature Matrix)</h1>
                <xsl:if test="number($version) ge 10">
                    <p>
                        <i>For features in version 9.9 products, see the <a class="bodylink"
                            href="feature-matrix-9-9.xml">Saxon 9.9 Feature Matrix</a>.</i>
                    </p>
                </xsl:if>
                <xsl:if test="number($version) ge 9.9">
                    <p>
                        <i>For features in version 9.8 products, see the <a class="bodylink"
                            href="feature-matrix-9-8.xml">Saxon 9.8 Feature Matrix</a>.</i>
                    </p>
                </xsl:if>
                <xsl:if test="number($version) ge 9.8">
                    <p>
                        <i>For features in version 9.7 products, see the <a class="bodylink"
                            href="feature-matrix-9-7.xml">Saxon 9.7 Feature Matrix</a>.</i>
                    </p>
                </xsl:if>
                <xsl:if test="number($version) ge 9.7">
                    <p>
                        <i>For features in version 9.6 products, see the <a class="bodylink"
                            href="feature-matrix-9-6.xml">Saxon 9.6 Feature Matrix</a>.</i>
                    </p>
                </xsl:if>
                    <p>The table below lists product features and indicates which features are
                        present in which product package.</p>
                    <table class="feature-matrix" id="overview">
                        <xsl:if test="number($version) le 9.7">
                        <thead>
                            <tr class="feature-heading">
                                <td style="text-align:left;">
                                    <xsl:value-of select="'Language Support'"/> 
                                </td>
                                <td>HE</td>
                                <td>PE</td>
                                <td>EE</td>
                                <td>EE&#8209;T</td>
                                <td>EE&#8209;Q</td>
                                <td>EE&#8209;V</td>
                                <xsl:if test="number($version) ge 9.8 and number($version) lt 10">
                                    <td>JS</td>
                                </xsl:if>
                                <xsl:if test="number($version) ge 10">
                                    <td>JS2</td>
                                </xsl:if>
                            </tr>
                        </thead>
                        </xsl:if>
                        <tbody>
                            <xsl:for-each select="feature">
                                <xsl:variable name="feature-desc" select="description"/>
                                <xsl:choose>
                                    <xsl:when test="./@class = 'heading'">
                                        <tr class="feature-heading">
                                            <td style="text-align:left;">
                                                <xsl:value-of select="shortName"/> 
                                            </td>
                                            <td>HE</td>
                                            <td>PE</td>
                                            <td>EE</td>
                                            <td>EE&#8209;T</td>
                                            <td>EE&#8209;Q</td>
                                            <td>EE&#8209;V</td>
                                            <xsl:if test="number($version) ge 9.8 and number($version) lt 10">
                                                <td>JS</td>
                                            </xsl:if>
                                            <xsl:if test="number($version) ge 10">
                                                <td>JS2</td>
                                            </xsl:if>
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
                                                <div class="{if (number($version) ge 9.8) then 'popup2' else 'popup'}"> 
                                                    <xsl:value-of select="$feature-with-level"/>  
                                                    <span>
                                                        <h2><xsl:value-of select="$feature-with-level"/></h2>
                                                        <xsl:choose>
                                                            <xsl:when test="exists(./*)">
                                                                <xsl:copy-of select="./*"/>
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
                                                  <xsl:sequence select="f:dot($editions,'HE')"/>
                                                </p>
                                            </td>
                                            <td>
                                                <p>
                                                    <xsl:sequence select="f:dot($editions,'PE')"/>
                                                </p>
                                            </td>
                                            <td>
                                                <p>
                                                    <xsl:sequence select="f:dot($editions,'EE')"/>
                                                </p>
                                            </td>
                                            <td>
                                                <p>
                                                    <xsl:sequence select="f:dot($editions,'EE-T')"/>
                                                </p>
                                            </td>
                                            <td>
                                                <p>
                                                    <xsl:sequence select="f:dot($editions,'EE-Q')"/>
                                                </p>
                                            </td>
                                            <td>
                                                <p>
                                                    <xsl:sequence select="f:dot($editions,'EE-V')"/>
                                                </p>
                                            </td>
                                            <xsl:if test="number($version) ge 9.8 and number($version) lt 10">
                                                <td>
                                                    <p>
                                                        <xsl:sequence select="f:dot($editions,'JS')"/>
                                                    </p>
                                                </td>
                                            </xsl:if>
                                            <xsl:if test="number($version) ge 10">
                                                <td>
                                                    <p>
                                                        <xsl:sequence select="f:dot($editions,'JS2')"/>
                                                    </p>
                                                </td>
                                            </xsl:if>
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
        <div id="aside">
            <xsl:choose>
                <xsl:when test="$version = ('9.6', '9.7', '9.8', '9.9', '10')">
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
                        <xsl:if test="number($version) ge 9.8 and number($version) lt 10">
                            <li><a class="title" href="../saxon-js/archive.xml"><b>JS: </b>Saxon-JS, JavaScript run-time</a></li>
                        </xsl:if>
                        <xsl:if test="number($version) ge 10">
                            <li><a class="title" href="../saxon-js/index.xml"><b>JS2: </b>Saxon-JS 2, JavaScript processor</a></li>
                        </xsl:if>
                    </ul>
                </xsl:when>
                <xsl:otherwise>
                    <h2>Product packages</h2>
                    <ul>
                        <li>
                            <b>HE: </b>Saxon Home Edition (open source)</li>
                        <li>
                            <b>PE: </b>Saxon Professional Edition</li>
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
                        <xsl:if test="number($version) ge 9.8 and number($version) lt 10">
                            <li>
                                <b>JS: </b>Saxon-JS, JavaScript run-time</li>
                        </xsl:if>
                        <xsl:if test="number($version) ge 10">
                            <li>
                                <b>JS2: </b>Saxon-JS 2, JavaScript processor</li>
                        </xsl:if>
                    </ul>
                </xsl:otherwise>
            </xsl:choose>
        </div>
    </xsl:template>
    
    <xsl:function name="f:dot">
        <xsl:param name="editions" as="xs:string*"/>
        <xsl:param name="product" as="xs:string"/>
        <xsl:choose>
            <xsl:when test="$product = $editions">                
                <xsl:value-of select="'&#x25cf;'"/> <!-- &#8729; bullet operator. &#x25cf; black circle-->
                <!--<img border="0" src="../img/tick-dark-blue-white-bkgd.png" width="25px"/>-->                
            </xsl:when>            
            <xsl:otherwise>
                <xsl:value-of select="'&#xa0;'"/>
            </xsl:otherwise>
        </xsl:choose>
    </xsl:function>

    <xsl:function name="f:tick">
        <xsl:param name="node" as="node()?"/>
        <xsl:choose>
            <xsl:when test="exists($node)">
                <xsl:choose>
                    <xsl:when test="$node/@level eq 'Inc'">
                        <img border="0" src="../img/tick-dark-blue-white-bkgd.png" width="25px"/>
                    </xsl:when>
                    <xsl:when test="$node/@level eq 'Open'">
                        <xsl:value-of select="'&#xa0;'"/>
                    </xsl:when>
                    <xsl:when test="exists($node/@level)">
                        <xsl:value-of select="$node/@level"/>
                    </xsl:when>
                    <xsl:otherwise>
                        <img border="0" src="../img/tick-dark-blue-white-bkgd.png" width="25px"/>
                    </xsl:otherwise>
                </xsl:choose>
            </xsl:when>
            <xsl:otherwise>
                <xsl:value-of select="'&#xa0;'"/>
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
