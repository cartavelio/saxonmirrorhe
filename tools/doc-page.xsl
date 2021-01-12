<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
    xmlns:xs="http://www.w3.org/2001/XMLSchema" version="2.0">
    
    <!-- Stylesheet used to build the main documentation pages for the different product 
        versions, and for the static HTML and dynamic XML versions of the website. -->
    
    <xsl:import href="contents-table.xsl"/>

    <xsl:output method="xhtml" indent="yes"/>
    <xsl:strip-space elements="*"/>

    <xsl:param name="version-number"/>
    <xsl:param name="method"/>
    <xsl:param name="major-version"/>

    <xsl:template match="version[@number = $version-number]">
        <xsl:if test="$method = 'dynamic'">
            <xsl:processing-instruction name="xml-stylesheet">href="../make-menu.xsl" type="text/xsl"</xsl:processing-instruction>
        </xsl:if>
        <html>
            <head>
                <this-is section="documentation" page="documentation" folder="{./@dir}"
                    subpage=""/>
                <meta http-equiv="Content-Type" content="text/html; charset=UTF-8"/>
                <title>Saxonica: Saxon <xsl:value-of select="$version-number"/> Documentation</title>
                <meta name="coverage" content="Worldwide"/>
                <meta name="copyright" content="Copyright Saxonica Ltd"/>
                <xsl:choose>
                    <xsl:when test="$method='static' and $version-number = $major-version">
                        <meta name="robots" content="index, follow"/>
                    </xsl:when>
                    <xsl:otherwise>
                        <meta name="robots" content="index, nofollow"/>
                    </xsl:otherwise>
                </xsl:choose> 
            </head>
            <body class="main">
                <h1>Saxon <xsl:value-of select="$version-number"/> Documentation</h1>
                <xsl:variable name="saxon-release">
                    <xsl:choose>
                        <xsl:when test="$version-number = $major-version">Saxon 
                            <xsl:value-of select="$version-number"/> (the latest
                            release)</xsl:when>
                        <xsl:otherwise>Saxon <xsl:value-of select="$version-number"/>
                        </xsl:otherwise>
                    </xsl:choose>
                </xsl:variable>
                <p>Select which Saxon version you are interested in from the list below, or use the
                    menu on the right to go directly to the documentation for <xsl:value-of
                        select="$saxon-release"/>. </p>
                <xsl:if test="$method = 'dynamic'">
                    <xsl:choose>
                        <xsl:when test="$version-number &gt;= 9.9">
                            <p>
                                <i>Two versions of the documentation are provided: an
                                    interactive version driven by <a class="bodylink" href="../saxon-js/index.xml"
                                        >Saxon-JS</a> in the browser, and an alternative static HTML page
                                    version for non-Javascript browsers.</i>
                            </p>
                        </xsl:when>
                        <xsl:otherwise>
                            <p>
                                <i>Since Saxon 9.4, we provide two versions of the documentation: an
                                    interactive version driven by <a class="bodylink" href="../ce/index.xml"
                                        >Saxon-CE</a> in the browser, and an alternative static HTML page
                                    version for non-Javascript browsers.</i>
                            </p>
                        </xsl:otherwise>
                    </xsl:choose>
                </xsl:if>

                <xsl:if test="$version-number &gt;= 9.7">
                    <h2>Server Processing: Saxon-HE, Saxon-PE and Saxon-EE</h2>
                </xsl:if>

                <xsl:if test="$method = 'dynamic'">
                    <xsl:call-template name="doc-table-dynamic"/>
                </xsl:if>

                <xsl:if test="$method = 'static'">
                    <xsl:call-template name="doc-table-static"/>
                </xsl:if>
                
                <xsl:if test="$version-number &gt;= 9.7">
                    <xsl:call-template name="SaxonC"/>
                </xsl:if>

                <xsl:if test="$version-number &gt;= 9.7">
                    <xsl:call-template name="browser-information"/>
                </xsl:if>

                <h2>Documentation for earlier releases</h2>
                <ul class="index-list">
                    <xsl:for-each select="/documentation-pages/version[number(@number) lt number($version-number)]">
                        <li>
                            <xsl:if test="$method = 'dynamic'">
                                <a href="../{@dir}/{index-link/@dynamic}" class="bodylink">Saxon
                                        <xsl:value-of select="@number"/> documentation</a>
                            </xsl:if>
                            <xsl:if test="$method = 'static'">
                                <xsl:choose>
                                    <xsl:when test="number(@number) lt 9.3">
                                        <a href="../../{@dir}/{index-link/@static}" class="bodylink"
                                            >Saxon <xsl:value-of select="@number"/>
                                            documentation</a>
                                    </xsl:when>
                                    <xsl:otherwise>
                                        <a href="../{@dir}/{index-link/@static}" class="bodylink"
                                            >Saxon <xsl:value-of select="@number"/>
                                            documentation</a>
                                    </xsl:otherwise>
                                </xsl:choose>
                            </xsl:if>
                        </li>
                    </xsl:for-each>
                </ul>

                <div id="fixed-aside">
                    <xsl:call-template name="doc-contents-table">
                        <xsl:with-param name="method" select="$method"/>
                        <xsl:with-param name="version-number" select="$version-number"/>
                    </xsl:call-template>
                </div>
            </body>
        </html>
    </xsl:template>

    <xsl:template name="doc-table-dynamic">
        <table class="doc-table">
            <xsl:for-each select="dynamic-page/documentation">
                <xsl:variable name="documentation-name">
                    <xsl:value-of select="concat('Saxon ',$version-number, ' ')"/>
                    <xsl:choose>
                        <xsl:when test="@type = 'Saxon'">
                            <xsl:text>documentation</xsl:text>
                        </xsl:when>
                        <xsl:otherwise>
                            <xsl:value-of select="@type"/>
                        </xsl:otherwise>
                    </xsl:choose>
                </xsl:variable>
                <xsl:variable name="folder-path-dynamic">
                    <xsl:if test="ancestor::version/@dir ne ancestor::version/@dynamic">
                        <xsl:value-of select="concat('../',ancestor::version/@dynamic,'/')"/>
                    </xsl:if>
                </xsl:variable>
                <tr>
                    <td>
                        <xsl:if test="@dynamic">
                            <a href="{$folder-path-dynamic}{@dynamic}" class="bodylink">
                                <div class="DocButton">
                                    <b>
                                        <xsl:value-of select="$documentation-name"/>
                                        <br/>(interactive XML version)</b>
                                </div>
                            </a>
                        </xsl:if>
                    </td>
                    <td>
                        <a href="{@static}" class="bodylink">
                            <div class="DocButton"><xsl:value-of select="$documentation-name"
                                /><br/>(static HTML version)</div>
                        </a>
                    </td>
                </tr>
            </xsl:for-each>
        </table>
    </xsl:template>

    <xsl:template name="doc-table-static">
        <table class="doc-table">
            <xsl:for-each select="static-page/documentation">
                <xsl:variable name="documentation-name">
                    <xsl:value-of select="concat('Saxon ',$version-number, ' ')"/>
                    <xsl:choose>
                        <xsl:when test="@type = 'Saxon'">
                            <xsl:text>documentation</xsl:text>
                        </xsl:when>
                        <xsl:otherwise>
                            <xsl:value-of select="@type"/>
                        </xsl:otherwise>
                    </xsl:choose>
                </xsl:variable>
                <tr>
                    <td>
                        <a href="{@static}" class="bodylink">
                            <div class="DocButton">
                                <b>
                                    <xsl:value-of select="$documentation-name"/>
                                </b>
                            </div>
                        </a>
                    </td>
                </tr>
            </xsl:for-each>
        </table>
    </xsl:template>
    
    <xsl:template name="SaxonC">
        <h2>Server Processing for C/C++, PHP and Python: Saxon-HE/C, Saxon-PE/C, Saxon-EE/C</h2>
        <xsl:if test="$method = 'dynamic'">
            <table class="doc-table">
                <xsl:for-each select="dynamic-page/saxonc">
                    <tr>
                        <td>
                            <a href="{@dynamic}" class="bodylink">
                                <div class="DocButton">
                                    <b>
                                        <xsl:value-of select="@type"/>
                                        documentation<br/>(interactive XML version)</b>
                                </div>
                            </a>
                        </td>
                    </tr>
                </xsl:for-each>
            </table>
        </xsl:if>
        <xsl:if test="$method = 'static'">
            <table class="doc-table">
                <xsl:for-each select="static-page/saxonc">
                    <tr>
                        <td><!-- Note only dynamic documentation app available for Saxon-C -->
                            <a href="{@dynamic}" class="bodylink">
                                <div class="DocButton">
                                    <b>
                                        <xsl:value-of select="@type"/>
                                        documentation<br/>(interactive XML)</b>
                                </div>
                            </a>
                        </td>
                    </tr>
                </xsl:for-each>
            </table>
        </xsl:if>
    </xsl:template>

    <xsl:template name="browser-information">
        <h2>Browser Processing: Saxon-JS</h2>
        <xsl:if test="$method = 'dynamic'">
            <table class="doc-table">
                <xsl:for-each select="dynamic-page/browser">
                    <tr>
                        <td>
                            <a href="{@dynamic}" class="bodylink">
                                <div class="DocButton">
                                    <b>
                                        <xsl:value-of select="@type"/>
                                        documentation<br/>(interactive XML)</b>
                                </div>
                            </a>
                        </td>
                    </tr>
                </xsl:for-each>
            </table>
        </xsl:if>
        <xsl:if test="$method = 'static'">
            <table class="doc-table">
                <xsl:for-each select="static-page/browser">
                    <tr>
                        <td><!-- Note only dynamic documentation app available for Saxon-JS -->
                            <a href="{@dynamic}" class="bodylink">
                                <div class="DocButton">
                                    <b>
                                        <xsl:value-of select="@type"/> 
                                        documentation<br/>(interactive XML)</b>
                                </div>
                            </a>
                        </td>
                    </tr>
                </xsl:for-each>
            </table>
        </xsl:if>
    </xsl:template>
    
</xsl:stylesheet>
