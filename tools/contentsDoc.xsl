<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
    xmlns:xs="http://www.w3.org/2001/XMLSchema" xmlns:fn="http://www.saxonica.com/functions"
    exclude-result-prefixes="xs" version="2.0">
    
    <!-- Stylesheet used to build the documentation full contents pages.
        Uses the reference document 'doc-ref.xml' to get correct path names, extensions, etc
        for different product versions, and for the static HTML and dynamic XML versions of
        the website. -->
    
    <xsl:import href="contents-table.xsl"/>

    <xsl:output method="xml" encoding="UTF-8" indent="yes"/>

    <xsl:param name="base-uri"/>
    <xsl:param name="version-number"/>
    <xsl:param name="major-version"/>
    <xsl:variable name="version-ref" select="document('doc-ref.xml')/*/version[@number = $version-number]"/>    

    <xsl:template match="/">
        <xsl:call-template name="contents">
            <xsl:with-param name="method" select="'dynamic'"/>
            <xsl:with-param name="doc-homepage" as="xs:string"
                select="concat('../',$version-ref/@dynamic,'/index.html#!')"/>
            <xsl:with-param name="href" select="'contents-page.xml'"/>
        </xsl:call-template>
        <xsl:call-template name="contents">
            <xsl:with-param name="method" select="'static'"/>
            <xsl:with-param name="doc-homepage" as="xs:string" select="$version-ref/static-page/@doc-entry"/>
            <xsl:with-param name="href" select="'../../contents-page-static.html'"/>
        </xsl:call-template>
    </xsl:template>

    <xsl:template name="contents">
        <xsl:param name="method"/>
        <xsl:param name="doc-homepage"/>
        <xsl:param name="href"/>
        <xsl:result-document href="{concat($base-uri,'/',$href)}">
            <xsl:if test="$method='dynamic'">
                <xsl:processing-instruction name="xml-stylesheet">
                 href="../make-menu.xsl" type="text/xsl"
            </xsl:processing-instruction>
            </xsl:if>
            <html>
                <head>
                    <xsl:comment> Generated at <xsl:value-of select="format-dateTime(current-dateTime(), '[D] [MNn] [Y0001] at [H01]:[m01]')"/>
          </xsl:comment>
                    <this-is section="documentation" page="documentation" folder="{$version-ref/@dir}"
                        subpage="contents-page"/>
                    <title>Saxonica: Saxon <xsl:value-of select="$version-number"/> Documentation Contents</title>
                    <meta name="title" content="Saxonica: Saxon {$version-number} Documentation Contents"/>
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
                    <h1>Saxon <xsl:value-of select="$version-number"/> Documentation - Full Contents</h1>
                    <xsl:for-each select="contents/section">
                        <xsl:variable name="section" select="."/>
                        <xsl:if test="$method = 'dynamic'">
                            <p class="subhead">
                                <a class="toc2" href="{concat($doc-homepage, @name)}">
                                    <xsl:value-of select="@title"/>
                                </a>
                            </p>
                            <xsl:call-template name="contents-list-dynamic">
                                <xsl:with-param name="section" select="$section"/>
                                <xsl:with-param name="doc-homepage" select="$doc-homepage"/>
                            </xsl:call-template>                            
                        </xsl:if>
                        <xsl:if test="$method = 'static'">
                            <p class="subhead">
                                <a class="toc2" href="{concat($doc-homepage, @name, '/index.html')}">
                                    <xsl:value-of select="@title"/>
                                </a>
                            </p>
                            <xsl:call-template name="contents-list-static">
                                <xsl:with-param name="section" select="$section"/>
                                <xsl:with-param name="doc-homepage" select="$doc-homepage"/>
                            </xsl:call-template>
                        </xsl:if>
                    </xsl:for-each>
                    <div id="fixed-aside">
                        <xsl:call-template name="doc-contents-table">
                            <xsl:with-param name="method" select="$method"/>
                            <xsl:with-param name="version-number" select="$version-number"/>
                        </xsl:call-template>
                    </div>
                </body>
            </html>
        </xsl:result-document>
    </xsl:template>

    <xsl:template name="contents-list-dynamic">
        <xsl:param name="section"/>
        <xsl:param name="doc-homepage"/>
        <ul>
            <xsl:for-each select="page">
                <xsl:variable name="page" select="."/>
                <xsl:variable name="page-name">
                    <xsl:choose>
                        <xsl:when test="$section/@name = 'changes'">
                            <xsl:choose>
                                <xsl:when test="$page/@name = '7.0'">
                                    <xsl:value-of select="concat('6.5.5-',$page/@name)"/>
                                </xsl:when>
                                <xsl:otherwise>
                                    <xsl:value-of
                                        select="concat(following-sibling::page[1]/@name, '-',$page/@name)"
                                    />
                                </xsl:otherwise>
                            </xsl:choose>
                        </xsl:when>
                        <xsl:otherwise>
                            <xsl:value-of select="$page/@name"/>
                        </xsl:otherwise>
                    </xsl:choose>
                </xsl:variable>
                <li>
                    <a class="toc2" href="{concat($doc-homepage, $section/@name, '/', $page-name)}">
                        <xsl:value-of select="$page/@title"/>
                    </a>
                    <xsl:if test="subpage">
                        <ul>
                            <xsl:for-each select="subpage">
                                <xsl:variable name="subpage" select="."/>
                                <xsl:choose>
                                    <xsl:when test="$section/@name = 'changes'">
                                        <li>
                                            <a
                                                href="{concat($doc-homepage, $section/@name, '/', ./@name, '/', $page-name)}">
                                                <xsl:value-of select="@title"/>
                                            </a>
                                        </li>
                                    </xsl:when>
                                    <xsl:otherwise>
                                        <li>
                                            <a
                                                href="{concat($doc-homepage, $section/@name, '/', $page-name, '/', ./@name)}">
                                                <xsl:value-of select="@title"/>
                                            </a>
                                            <xsl:if test="subpage">
                                                <xsl:variable name="sub1-page"
                                                  select="concat($doc-homepage, $section/@name, '/', $page-name, '/', ./@name)"/>
                                                <ul>
                                                  <xsl:for-each select="subpage">
                                                  <xsl:variable name="sub2page" select="."/>
                                                  <li>
                                                  <a href="{concat($sub1-page, '/', ./@name)}">
                                                  <xsl:value-of select="@title"/>
                                                  </a>
                                                  <xsl:if test="subpage">
                                                  <ul>
                                                  <xsl:for-each select="subpage">
                                                  <li>
                                                  <a
                                                  href="{concat($sub1-page, '/', $sub2page/@name, '/', ./@name)}">
                                                  <xsl:value-of select="@title"/>
                                                  </a>

                                                  </li>
                                                  </xsl:for-each>
                                                  </ul>
                                                  </xsl:if>
                                                  </li>
                                                  </xsl:for-each>
                                                </ul>
                                            </xsl:if>
                                        </li>
                                    </xsl:otherwise>
                                </xsl:choose>
                            </xsl:for-each>
                        </ul>
                    </xsl:if>
                </li>
            </xsl:for-each>
        </ul>
    </xsl:template>

    <xsl:template name="contents-list-static">
        <xsl:param name="section"/>
        <xsl:param name="doc-homepage"/>
        <ul>
            <xsl:for-each select="page">
                <xsl:variable name="page" select="."/>
                <xsl:variable name="page-name">
                    <xsl:choose>
                        <!-- Note that the changes section is slightly different for 9.4 -->
                        <xsl:when test="$section/@name = 'changes' and $version-number != '9.4'">
                            <xsl:value-of select="concat('v',$page/@name)"/>
                        </xsl:when>
                        <xsl:otherwise>
                            <xsl:value-of select="$page/@name"/>
                        </xsl:otherwise>
                    </xsl:choose>
                </xsl:variable>
                <li>
                    <a class="toc2"
                        href="{concat($doc-homepage, $section/@name, '/', $page-name, fn:extension(.))}">
                        <xsl:value-of select="$page/@title"/>
                    </a>
                    <xsl:if test="subpage">
                        <ul>
                            <xsl:for-each select="subpage">
                                <xsl:variable name="subpage" select="."/>
                                <li>
                                    <a
                                        href="{concat($doc-homepage, $section/@name, '/', $page-name, '/', ./@name, fn:extension(.))}">
                                        <xsl:value-of select="@title"/>
                                    </a>
                                    <xsl:if test="subpage">
                                        <xsl:variable name="sub1-page"
                                            select="concat($doc-homepage, $section/@name, '/', $page-name, '/', ./@name)"/>
                                        <ul>
                                            <xsl:for-each select="subpage">
                                                <xsl:variable name="sub2page" select="."/>
                                                <li>
                                                  <a
                                                      href="{concat($sub1-page, '/', ./@name, fn:extension(.))}">
                                                  <xsl:value-of select="@title"/>
                                                  </a>
                                                  <xsl:if test="subpage">
                                                  <ul>
                                                  <xsl:for-each select="subpage">
                                                  <li>
                                                  <a
                                                  href="{concat($sub1-page, '/', $sub2page/@name, '/', ./@name, fn:extension(.))}">
                                                  <xsl:value-of select="@title"/>
                                                  </a>
                                                  </li>
                                                  </xsl:for-each>
                                                  </ul>
                                                  </xsl:if>
                                                </li>
                                            </xsl:for-each>
                                        </ul>
                                    </xsl:if>
                                </li>
                            </xsl:for-each>
                        </ul>
                    </xsl:if>
                </li>
            </xsl:for-each>
        </ul>
    </xsl:template>

    <xsl:function name="fn:extension" as="xs:string">
        <xsl:param name="node" as="node()"/>
        <xsl:choose>
            <xsl:when test="$node/*">
                <xsl:value-of select="'/index.html'"/>
            </xsl:when>
            <xsl:otherwise>
                <xsl:value-of select="'.html'"/>
            </xsl:otherwise>
        </xsl:choose>
    </xsl:function>

</xsl:stylesheet>
