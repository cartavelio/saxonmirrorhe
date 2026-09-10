<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="2.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
  xmlns:cat="http://www.saxonica.com/ns/doc/catalog"
  xmlns:fn="http://www.saxonica.com/ns/doc/functions"
  xmlns:ch="http://www.saxonica.com/ns/doc/changes" xmlns:xs="http://www.w3.org/2001/XMLSchema"
  xmlns:saxon="http://saxon.sf.net/" exclude-result-prefixes="cat fn ch xs saxon">

  <xsl:output method="xml" encoding="UTF-8" indent="no"/>
  
  <xsl:param name="version-number"/>

  <xsl:variable name="sections" select="/cat:catalog/cat:section/@ref"/>
  
  <!-- Note that for the 9.4 documentation, the Changes and Functions sections have a slightly different
     format to those in 9.5 onwards; so these are treated as a special case. -->

  <xsl:template match="/">
    <xsl:result-document method="xml" indent="no">
      <xsl:comment> Generated at <xsl:value-of select="format-dateTime(current-dateTime(), '[D] [MNn] [Y0001] at [H01]:[m01]')"/>
      </xsl:comment>
      <contents>
        <xsl:for-each select="$sections[. ne 'javadoc' and . ne 'dotnetdoc']">
          <xsl:variable name="sname" select="string(.)"/>
          <xsl:variable name="sect" select="document(concat(., '.xml'),/)"/>

          <xsl:choose>
            <xsl:when test="$sname = 'functions' and $version-number = '9.4'">
              <section name="{$sname}" title="{$sect//fn:functions/@title}">
                <xsl:for-each select="$sect//fn:function">
                  <page name="{fn:name}" title="{fn:name}"/>                  
                </xsl:for-each>
              </section>
            </xsl:when>
            <xsl:when test="$sname = 'functions'">
              <section name="{$sname}" title="{$sect/article/@title}">
                <xsl:for-each select="$sect/article/section">
                  <xsl:variable name="fns" select="fn:function"/>
                  <xsl:variable name="sfns" as="element()*" select="fn:fn-list($fns)"/>
                  <page name="{@id}" title="{@title}">
                    <xsl:for-each select="$sfns">
                      <subpage name="{fn:name}" title="{fn:name}"/>
                    </xsl:for-each>
                  </page>
                </xsl:for-each>
              </section>
            </xsl:when>
            <xsl:when test="$sname = 'changes' and $version-number != '9.4'">
              <section name="{$sname}" title="{$sect/ch:changes/@title}">
                <xsl:for-each select="$sect/ch:changes/ch:release">
                  <xsl:variable name="release" select="."/>
                  <page name="{@number}" title="Version {@number} ({@date})">
                    <xsl:for-each select="$sect/ch:changes/ch:categories/ch:cat">
                      <xsl:if test="./@name = $release/ch:category/@name">
                        <subpage name="{@name}" title="{@title}"/>
                      </xsl:if>
                    </xsl:for-each>
                  </page>
                </xsl:for-each>
              </section>
            </xsl:when>
            <xsl:otherwise>
              <section name="{$sname}" title="{$sect/article/@title}">
                <xsl:for-each select="$sect/article/section">
                  <xsl:variable name="h" select="."/>
                  <page name="{$h/@id}" title="{$h/@title}">
                    <xsl:for-each select="section">
                      <subpage name="{@id}" title="{@title}">
                        <xsl:for-each select="section">
                          <subpage name="{@id}" title="{@title}">
                            <xsl:for-each select="section">
                              <subpage name="{@id}" title="{@title}"/>                              
                            </xsl:for-each>
                          </subpage>
                        </xsl:for-each>
                      </subpage>
                    </xsl:for-each>
                  </page>
                </xsl:for-each>
              </section>
            </xsl:otherwise>
          </xsl:choose>
        </xsl:for-each>
      </contents>
    </xsl:result-document>
  </xsl:template>

  <xsl:function name="fn:fn-list" as="element()*">
    <xsl:param name="fns" as="element()*"/>
    <xsl:perform-sort select="$fns">
      <xsl:sort select="lower-case(name)" lang="en"/>
    </xsl:perform-sort>
  </xsl:function>

</xsl:stylesheet>
