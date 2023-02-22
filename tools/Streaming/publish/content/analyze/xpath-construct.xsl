<!--<?xml version="1.0" encoding="UTF-8"?>-->
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
  xmlns:xs="http://www.w3.org/2001/XMLSchema"
  xmlns:math="http://www.w3.org/2005/xpath-functions/math"
  xmlns:map="http://www.w3.org/2005/xpath-functions/map" xmlns:xp="http://saxonica.com/xpathParse"
  xmlns:fos="http://www.w3.org/xpath-functions/spec/namespace"
  xmlns:e="http://www.w3.org/1999/XSL/Spec/ElementSyntax" xmlns:f="myfunctions"
  xmlns:doc="http://www.saxonica.com/ns/documentation" xmlns:dc="http://purl.org/dc/elements/1.1/"
  xmlns:X="IndirectXSLT" xmlns:s="StreamAnalysis" xmlns:l="LINENUMBER"
  exclude-result-prefixes="xs math" version="3.0">

  <xsl:variable name="rex" select="true()" static="yes"/>

  <xsl:include href="../rex/xpath3.0-parse.xslt"/>

  <!--<xsl:include href="xpath-construct-saxon.xsl" use-when="not($rex)"/>
-->

  <xsl:strip-space elements="*"/>

  <xsl:function name="xp:is.AVT" as="xs:boolean">
    <xsl:param name="test" as="attribute()"/>
    <xsl:sequence select="contains($test,'{')"/>
  </xsl:function>

  <xsl:template name="line-number" doc:doc="Record the line number counted by real elements">
    <xsl:attribute name="l:no"
      select="count((preceding::*|ancestor-or-self::*)[not(self::s:sequence-constructor |
      ancestor-or-self::doc:*)])"
    />
  </xsl:template>

  <xsl:mode name="xp:P" on-no-match="shallow-copy"/>
  <xsl:mode name="xp:implict-select" on-no-match="deep-skip"/>



  <xsl:template match="xsl:apply-templates[empty(@select)]" mode="xp:implict-select">
    <xsl:attribute name="select">implicit</xsl:attribute>
    <xsl:variable name="select" as="attribute()">
      <xsl:attribute name="select">child::node()</xsl:attribute>
    </xsl:variable>
    <xsl:apply-templates select="$select" mode="operand"/>
  </xsl:template>
  <xsl:template match="xsl:copy[empty(@select)]|xsl:next-match|xsl:apply-imports"
    mode="xp:implict-select">
    <xsl:attribute name="select">implicit</xsl:attribute>
    <xsl:variable name="select" as="attribute()">
      <xsl:attribute name="select">.</xsl:attribute>
    </xsl:variable>
    <xsl:apply-templates select="$select" mode="operand"/>
  </xsl:template>

  <xsl:template match="doc:*" mode="xp:P"/>

  <xsl:template match="xsl:import" mode="xp:P">
    <xsl:copy>
      <xsl:sequence select="@*"/>
      <xsl:apply-templates select="doc(resolve-uri(@href,base-uri(.)))/*/*" mode="#current"/>
    </xsl:copy>
  </xsl:template>

  <xsl:template match="xsl:*" mode="xp:P">
    <xsl:copy>
      <xsl:apply-templates select="@*" mode="#current"/>
      <xsl:call-template name="line-number"/>
      <xsl:apply-templates select="." mode="xp:implict-select"/>
      <xsl:apply-templates select="@select,@test,@match,@group-adjacent" mode="operand"/>
      <xsl:apply-templates select="@*[xp:is.AVT(.)]" mode="operand"/>

      <xsl:for-each-group select="*|text()[matches(.,'\S+')]" group-adjacent="xp:is.sequence-constructor(.)">
        <xsl:choose>
          <xsl:when test="current-grouping-key()">
            <s:sequence-constructor>
              <xsl:apply-templates select="current-group()" mode="#current"/>
            </s:sequence-constructor>
          </xsl:when>
          <xsl:otherwise>
            <xsl:apply-templates select="current-group()" mode="#current"/>
          </xsl:otherwise>
        </xsl:choose>
      </xsl:for-each-group>

    </xsl:copy>
  </xsl:template>
  <xsl:template match="xsl:choose" mode="xp:P">
    <xsl:copy>
      <xsl:apply-templates select="@*" mode="#current"/>
      <xsl:call-template name="line-number"/>
      <xsl:apply-templates select="." mode="xp:implict-select"/>
      <xsl:apply-templates select="@select" mode="operand"/>
      <xsl:apply-templates select="@*[xp:is.AVT(.)]" mode="operand"/>
      <xsl:apply-templates select="*" mode="#current"/>
    </xsl:copy>
  </xsl:template>

  <xsl:template match="*[@*[xp:is.AVT(.)]]" mode="xp:P">
    <xsl:copy>
      <xsl:apply-templates select="@*" mode="#current"/>
      <xsl:call-template name="line-number"/>
      <xsl:apply-templates select="@select" mode="operand"/>
      <xsl:apply-templates select="@*[xp:is.AVT(.)]" mode="operand"/>
      <xsl:apply-templates select="*|text()" mode="xsl.body"/>
    </xsl:copy>
  </xsl:template>
  <xsl:template match="xsl:stylesheet | xsl:transform | xsl:next-iteration" mode="xp:P">
    <xsl:copy>
      <xsl:call-template name="line-number"/>
      <xsl:apply-templates select="@*|xsl:*" mode="#current"/>
    </xsl:copy>
  </xsl:template>
  <xsl:template match="*" mode="xp:P">
    <xsl:copy>
      <xsl:apply-templates select="@*" mode="#current"/>
      <xsl:call-template name="line-number"/>
      <xsl:apply-templates select="*|text()" mode="#current"/>
    </xsl:copy>
  </xsl:template>

  <xsl:function name="xp:is.sequence-constructor" as="xs:boolean">
    <xsl:param name="item" as="item()"/>
    <xsl:sequence
      select="empty($item/(self::xsl:param|self::xsl:with-param|self::xsl:on-completion|self::xsl:sort))"
    />
  </xsl:function>

  <xsl:mode name="xsl.body" on-no-match="shallow-copy"/>
  <xsl:template match="xsl:with-param|xsl:param|xsl:on-completion|xsl:sort" mode="xsl.body">
    <xsl:apply-templates select="." mode="xp:P"/>
  </xsl:template>
  <xsl:template match="xsl:text" mode="xsl.body">
    <xsl:copy-of select="."/>
  </xsl:template>
  <xsl:template match="*|text()[matches(.,'\S')]" mode="xsl.body">
    <s:sequence-constructor>
      <xsl:apply-templates select="." mode="xp:P"/>
    </s:sequence-constructor>
  </xsl:template>

  <xsl:mode name="path-to-pattern" on-no-match="shallow-copy"/>
  <xsl:template match="xp:ForwardStep | xp:UnionExpr" mode="path-to-pattern">
    <xsl:element name="xp:{local-name()}P">
      <xsl:apply-templates select="@*|*|text()" mode="#current"/>
    </xsl:element>
  </xsl:template>
  <xsl:template match="xp:FunctionCall[not(@name='root')][empty(parent::*)]" mode="path-to-pattern">
    <xp:RootedPath>
      <xsl:next-match/>
    </xp:RootedPath>
  </xsl:template>



  <xsl:function name="xp:toClark" as="xs:string">
    <xsl:param name="in" as="xs:string"/>
    <xsl:param name="prefixes" as="map(xs:string,xs:anyURI*)"/>
    <xsl:variable name="parts" as="xs:string*">
      <xsl:analyze-string select="$in" regex="(\i\c*):+">
        <xsl:matching-substring>
          <xsl:value-of
            select="if(ends-with(.,'::')) then . else 'Q{'||$prefixes(regex-group(1))||'}'"/>
        </xsl:matching-substring>
        <xsl:non-matching-substring>
          <xsl:value-of select="."/>
        </xsl:non-matching-substring>
      </xsl:analyze-string>
    </xsl:variable>
    <xsl:value-of select="string-join($parts,'')"/>
  </xsl:function>
  <xsl:function name="xp:namespaces" as="map(xs:string,xs:anyURI*)">
    <xsl:param name="context" as="element()?"/>
    <xsl:sequence
      select="map:new(
      $context/(for $p in in-scope-prefixes($context)[not(. = ('xsl','xml'))]  
      return map:entry($p,if($context) then namespace-uri-for-prefix($p,$context) else 'UNKNOWN')))
      "
    />
  </xsl:function>

  <xsl:mode name="operand" on-no-match="shallow-copy"/>

  <xsl:template match="@select|@test|@match|@group-adjacent" mode="operand">
    <xsl:param name="show.raw" as="xs:boolean" select="false()" tunnel="yes"/>
    <xsl:variable name="role" select="name(.)"/>
    <!--    <xsl:variable name="prefixes" select="(if(exists(..)) then in-scope-prefixes(..) else ())[not(. = ('xsl','xml'))]"/>
-->
    <!--<xsl:variable name="parent" select="parent::*"/>
    <xsl:variable name="prefixes"
      select="map:new(
      $parent/(for $p in in-scope-prefixes($parent)[not(. = ('xsl','xml'))]  
      return map:entry($p,if($parent) then namespace-uri-for-prefix($p,$parent) else 'UNKNOWN')))
      "/>-->
    <xsl:variable name="prefixes" select="xp:namespaces(parent::*)"/>
    <!-- <xsl:for-each select="map:keys($prefixes)">
      <xsl:message select=".,':=',$prefixes(.)"/>
    </xsl:for-each>-->
    <!-- <xsl:message select="'Clark:',xp:toClark(.,$prefixes)"/>-->
    <!--  <xsl:variable name="parse" select="xp:xpathParse(xp:toClark(.,$prefixes))/*/*"
      use-when="not($rex)"/>-->
    <xsl:variable name="parse" select="xp:parse.xpath(.)"/>
    <xsl:variable name="path" as="element()">
      <!--<xsl:variable name="construct" as="element()*" use-when="not($rex)">
        <xsl:variable name="init" as="element()*">
          <xsl:apply-templates select="$parse" mode="remove.init"/>
        </xsl:variable>
        <xsl:apply-templates select="$init" mode="construct">
          <xsl:with-param name="top-level" select="true()" tunnel="yes"/>
          <xsl:with-param name="is-match" select="name(.)='match'"/>
        </xsl:apply-templates>
      </xsl:variable>-->
      <xsl:variable name="construct" as="element()*">
        <xsl:apply-templates select="$parse" mode="construct">
          <xsl:with-param name="top-level" select="true()" tunnel="yes"/>
          <xsl:with-param name="is-match" select="name(.)='match'"/>
        </xsl:apply-templates>
      </xsl:variable>
      <xsl:variable name="temp" as="element()*">
        <xsl:choose>
          <xsl:when test="$role='match'">
            <xsl:apply-templates select="$construct" mode="path-to-pattern"/>
          </xsl:when>
          <xsl:otherwise>
            <xsl:sequence select="$construct"/>
          </xsl:otherwise>
        </xsl:choose>
      </xsl:variable>
      <xsl:apply-templates select="$temp" mode="collapse-construct"/>
    </xsl:variable>

    <xsl:choose>
      <xsl:when test="$show.raw">
        <xsl:sequence select="$parse"/>
      </xsl:when>
      <xsl:otherwise>
        <xsl:for-each select="$path">
          <xsl:copy>
            <xsl:sequence select="@*"/>
            <xsl:attribute name="s:role" select="$role"/>
            <xsl:sequence select="*|text()"/>
          </xsl:copy>
        </xsl:for-each>
      </xsl:otherwise>
    </xsl:choose>

  </xsl:template>

  <xsl:template match="@*[xp:is.AVT(.)]" mode="operand">
    <xsl:param name="show.raw" as="xs:boolean" select="false()" tunnel="yes"/>
    <xsl:variable name="role" select="name(.)"/>
    <xsl:variable name="prefixes" select="xp:namespaces(parent::*)"/>
    <!-- Use an analyse string -->
    <xsl:variable name="avt" select="replace(.,'^.*\{|\}.*$','')"/>
    <!--<xsl:message select="'Clark:',xp:toClark($avt,$prefixes)"/>-->

    <!-- <xsl:variable name="parse" select="xp:xpathParse(xp:toClark($avt,$prefixes))/*/*"/>-->
    <!--<xsl:variable name="parse" select="xp:xpathParse(xp:toClark($avt,$prefixes))/*/*"
      use-when="not($rex)"/>-->
    <xsl:variable name="parse" select="xp:parse.xpath($avt)"/>

    <xsl:variable name="path" as="element()">
      <!--      <xsl:variable name="construct" as="element()*" use-when="not($rex)">
        <xsl:variable name="init" as="element()*">
          <xsl:apply-templates select="$parse" mode="remove.init"/>
        </xsl:variable>
        <xsl:apply-templates select="$init" mode="construct">
          <xsl:with-param name="top-level" select="true()" tunnel="yes"/>
          <xsl:with-param name="is-match" select="name(.)='match'"/>
        </xsl:apply-templates>
      </xsl:variable>-->
      <xsl:variable name="construct" as="element()*">
        <xsl:apply-templates select="$parse" mode="construct">
          <xsl:with-param name="top-level" select="true()" tunnel="yes"/>
          <xsl:with-param name="is-match" select="name(.)='match'"/>
        </xsl:apply-templates>
      </xsl:variable>
      <xsl:variable name="temp" as="element()*">
        <xsl:choose>
          <xsl:when test="$role='match'">
            <xsl:apply-templates select="$construct" mode="path-to-pattern"/>
          </xsl:when>
          <xsl:otherwise>
            <xsl:sequence select="$construct"/>
          </xsl:otherwise>
        </xsl:choose>
      </xsl:variable>
      <xsl:apply-templates select="$temp" mode="collapse-construct"/>
    </xsl:variable>

    <xsl:choose>
      <xsl:when test="$show.raw">
        <xsl:sequence select="$parse"/>
      </xsl:when>
      <xsl:otherwise>
        <xsl:for-each select="$path">
          <xsl:copy>
            <xsl:sequence select="@*"/>
            <xsl:attribute name="s:role" select="'AVT',$role,position()" separator="."/>
            <xsl:sequence select="*|text()"/>
          </xsl:copy>
        </xsl:for-each>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>






  <xsl:mode name="construct" on-no-match="shallow-copy"/>

  <!-- These rewrites need to be early, but not sure here... -->

  <xsl:template match="RelativePathExpr[count(*) gt 2]" mode="construct"
    xpath-default-namespace="http://saxonica.com/xpathParse">
    <xsl:copy>
      <xsl:variable name="head" as="element()*">
        <xsl:copy>
          <xsl:sequence select="*[position() ne last()]"/>
        </xsl:copy>
      </xsl:variable>
      <xsl:apply-templates select="$head" mode="#current"/>
      <xsl:apply-templates select="*[last()]" mode="#current"/>
    </xsl:copy>
  </xsl:template>

  <xsl:template
    match="RelativePathExpr[AxisStep[@axis='descendant-or-self']/
    following-sibling::*[1]/self::AxisStep[@axis='child']]"
    mode="construct" xpath-default-namespace="http://saxonica.com/xpathParse"
    xmlns="http://saxonica.com/xpathParse" priority="1">
    <xsl:variable name="temp" as="element()">
      <xsl:variable name="children" as="element()*">
        <xsl:apply-templates select="*" mode="#current"/>
      </xsl:variable>
      <xsl:variable name="parts" as="element()*">
        <xsl:for-each-group select="$children"
          group-starting-with="AxisStep[@axis=('descendant-or-self','attribute')]" bind-group="g">
          <xsl:choose>
            <xsl:when test="$g[1]/self::AxisStep[@axis='descendant-or-self']">
              <AxisStep axis="descendant-or-self">
                <xsl:variable name="node-tests" select="reverse($g/@nodeTest)"/>
                <xsl:sequence select="head($node-tests)"/>
                <PredicateList>
                  <Predicate>
                    <xsl:variable name="tests" as="element()*">
                      <xsl:for-each select="tail(reverse($g))">
                        <xsl:copy>
                          <xsl:attribute name="axis">parent</xsl:attribute>
                          <xsl:sequence select="@nodeTest"/>
                        </xsl:copy>
                      </xsl:for-each>
                    </xsl:variable>
                    <xsl:choose>
                      <xsl:when test="count($tests) gt 1">
                        <RelativePathExpr>
                          <xsl:sequence select="$tests"/>
                        </RelativePathExpr>
                      </xsl:when>
                      <xsl:otherwise>
                        <xsl:sequence select="$tests"/>
                      </xsl:otherwise>
                    </xsl:choose>
                  </Predicate>
                </PredicateList>
              </AxisStep>
            </xsl:when>
            <xsl:otherwise>
              <xsl:sequence select="$g"/>
            </xsl:otherwise>
          </xsl:choose>
        </xsl:for-each-group>
      </xsl:variable>
      <xsl:choose>
        <xsl:when test="count($parts) = 1">
          <xsl:sequence select="$parts"/>
        </xsl:when>
        <xsl:otherwise>
          <xsl:copy>
            <xsl:sequence select="$parts"/>
          </xsl:copy>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>
    <xsl:apply-templates select="$temp" mode="#current"/>
  </xsl:template>
  
  <xsl:template match="xp:FunctionCall[@name=($default-dot-functions)][empty(*)]" mode="construct">
    <xp:FunctionCall>
      <xsl:apply-templates select="@*|*|text()" mode="#current"/>
      <xp:ContextItemExpr defaulted="."/>
    </xp:FunctionCall>
  </xsl:template>



  <xsl:variable name="axes.forward" as="xs:string*"
    select="('child','descendant','attribute',
    'descendant-or-self','following-sibling','following','namespace')"/>
  <xsl:variable name="axes.reverse" as="xs:string*"
    select="('parent','ancestor',
    'ancestor-or-self','preceding-sibling','preceding')"/>




  <xsl:mode name="collapse-construct" on-no-match="shallow-copy"/>
  <xsl:template match="xp:ExprSingle[count(*) eq 1]" mode="collapse-construct">
    <xsl:variable name="attributes" select="@*"/>
    <xsl:for-each select="*">
      <xsl:copy>
        <xsl:sequence select="$attributes,@*"/>
        <xsl:apply-templates select="*" mode="#current"/>
      </xsl:copy>
    </xsl:for-each>
  </xsl:template>
</xsl:stylesheet>
