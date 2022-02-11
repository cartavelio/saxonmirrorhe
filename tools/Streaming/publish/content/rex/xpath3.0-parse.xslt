<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
  xmlns:xs="http://www.w3.org/2001/XMLSchema"
  xmlns:math="http://www.w3.org/2005/xpath-functions/math" xmlns:p="CR-xpath-30-20130108"
  xmlns:xp="http://saxonica.com/xpathParse" xmlns:s="StreamAnalysis"
  exclude-result-prefixes="xs math" version="3.0">

  <xsl:include href="CR-xpath-30-20130108.xslt"/>

  <xsl:variable name="r.string">'[^']*'</xsl:variable>
  <xsl:variable name="r.dslash">//</xsl:variable>
  <xsl:variable name="r.at">@(\i\c*|\*)</xsl:variable>
  <xsl:variable name="r.parent">\.\./</xsl:variable>

  <xsl:function name="xp:modify.xpath" as="xs:string" expand-text="yes">
    <xsl:param name="xpath" as="xs:string"/>
    <xsl:variable name="parts" as="xs:string*">
      <xsl:analyze-string select="$xpath"
        regex="{string-join(($r.string,$r.dslash,$r.at,$r.parent),'|')}">
        <xsl:matching-substring>
          <xsl:choose>
            <xsl:when test="matches(.,$r.string)">{.}</xsl:when>
            <xsl:when test="matches(.,$r.dslash)">/descendant-or-self::node()/</xsl:when>
            <xsl:when test="matches(.,$r.at)">attribute::{substring(.,2)}</xsl:when>
            <xsl:when test="matches(.,$r.parent)">parent::node()/</xsl:when>
          </xsl:choose>
        </xsl:matching-substring>
        <xsl:non-matching-substring>
          <xsl:value-of select="."/>
        </xsl:non-matching-substring>
      </xsl:analyze-string>
    </xsl:variable>
    <xsl:value-of select="string-join($parts,'')"/>
  </xsl:function>

  <xsl:function name="xp:parse.xpath" as="element()*">
    <xsl:param name="xpath" as="xs:string"/>
    <xsl:variable name="expr" select="xp:modify.xpath($xpath)"/>
    <xsl:variable name="parse" select="p:parse-XPath($expr)"/>
    <xsl:try>
      <xsl:variable name="temp.1" as="element()*">
        <xsl:apply-templates select="$parse" mode="process"/>
      </xsl:variable>
      <xsl:variable name="temp.2" as="element()*">
        <xsl:apply-templates select="$temp.1" mode="process.1"/>
      </xsl:variable>
      <xsl:variable name="temp.3" as="element()*">
        <xsl:apply-templates select="$temp.2" mode="process.2"/>
      </xsl:variable>
      <xsl:apply-templates select="$temp.3" mode="process.3"/>
      <xsl:catch>
        <xsl:message select="'Error parsing:',$xpath,'&#xA;',$expr,'&#xA;',$parse"/>
        <xsl:message>
          <xsl:apply-templates select="$parse" mode="process"/>
        </xsl:message>
      </xsl:catch>
    </xsl:try>
  </xsl:function>





  <xsl:mode name="process" on-no-match="shallow-copy"/>
  <xsl:mode name="process.1" on-no-match="shallow-copy"/>
  <xsl:mode name="process.2" on-no-match="shallow-copy"/>
  <xsl:mode name="process.3" on-no-match="shallow-copy"/>

  <xsl:template match="*" mode="process.3">
    <xsl:element name="xp:{local-name()}">
      <xsl:apply-templates select="@*|*|text()" mode="#current"/>
    </xsl:element>
  </xsl:template>


  <xsl:variable name="root" as="element()*">
    <xsl:variable name="root.rewrite">root(self::node()) treat as document-node()</xsl:variable>
    <xsl:apply-templates select="p:parse-XPath($root.rewrite)" mode="process"/>
  </xsl:variable>

  <xsl:template match="PathExpr[TOKEN[1]='/']" mode="process.1">
    <xsl:choose>
      <xsl:when test="count(*) = 1">
        <AxisStep axis="self" nodeTest="document-node()"/>
        <!--<xsl:apply-templates select="$root" mode="#current"/>-->
      </xsl:when>
      <xsl:otherwise>
        <RelativePathExpr>
          <xsl:apply-templates select="$root,*" mode="#current"/>
        </RelativePathExpr>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <xsl:template match="AxisStep" mode="process">
    <xsl:copy>
      <xsl:apply-templates select="ForwardStep/*|ReverseStep/*,PredicateList" mode="#current"/>
    </xsl:copy>
  </xsl:template>

  <xsl:template match="ForwardAxis" mode="process">
    <xsl:attribute name="axis" select="TOKEN[1]"/>
  </xsl:template>
  <xsl:template match="AbbrevForwardStep" mode="process">
    <xsl:attribute name="axis" select="if(TOKEN='@') then 'attribute' else 'child'"/>
    <xsl:apply-templates select="* except TOKEN" mode="#current"/>
  </xsl:template>
  <xsl:template match="NameTest" mode="process">
    <xsl:variable name="attribute"
      select="../parent::AbbrevForwardStep/TOKEN='@' or ../parent::ForwardStep/ForwardAxis/TOKEN[1]='attribute'"/>
    <xsl:attribute name="nodeTest"
      select="if($attribute) then 'attribute('||.||')' else 'element('||.||')'"/>
  </xsl:template>
  <xsl:template match="KindTest" mode="process">
    <xsl:attribute name="nodeTest" select="."/>
  </xsl:template>


  <xsl:template match="AbbrevReverseStep" mode="process">
    <xsl:attribute name="axis">parent</xsl:attribute>
    <xsl:attribute name="nodeTest">node()</xsl:attribute>
  </xsl:template>
  <xsl:template match="ReverseAxis" mode="process">
    <xsl:attribute name="axis" select="TOKEN[1]"/>
  </xsl:template>



  <xsl:template match="XPath" mode="process">
    <xsl:apply-templates select="* except EOF" mode="#current"/>
  </xsl:template>
  <xsl:template match="PredicateList[empty(*)]" mode="process"/>
  <xsl:template match="Predicate" mode="process">
    <xsl:copy>
      <xsl:apply-templates select="*" mode="#current"/>
    </xsl:copy>
  </xsl:template>
  <xsl:template match="ContextItemExpr" mode="process">
    <xsl:copy/>
  </xsl:template>

  <xsl:template match="text()[matches(.,'^\s+$')]" mode="process"/>
  <xsl:template
    match="TOKEN[.=('[',']','(',')','ZZ/',',','/','!',
    '+','-','*','div','mod','treat','as','and','or','|','instance', 'of',
    'for','some','every','let','in',':=','$','satisfies','return')]"
    mode="process.1"/>

  <xsl:template match="ComparisonExpr[GeneralComp|ValueComp|NodeComp]" mode="process">
    <xsl:copy>
      <xsl:attribute name="op" select="GeneralComp|ValueComp|NodeComp"/>
      <xsl:apply-templates select="* except (GeneralComp|ValueComp|NodeComp)" mode="#current"/>
    </xsl:copy>
  </xsl:template>
  <xsl:template match="AdditiveExpr[TOKEN]|MultiplicativeExpr[TOKEN]|UnaryExpr[TOKEN]"
    mode="process">
    <xsl:copy>
      <xsl:attribute name="op" select="TOKEN"/>
      <xsl:apply-templates select="*" mode="#current"/>
    </xsl:copy>
  </xsl:template>
  <xsl:template match="AdditiveExpr[count(*) gt 3]|MultiplicativeExpr[count(*) gt 3]" mode="process"
    priority="1">
    <xsl:variable name="temp" as="element()">
      <xsl:copy>
        <xsl:sequence select="subsequence(*,1,2)"/>
        <xsl:copy>
          <xsl:sequence select="subsequence(*,3)"/>
        </xsl:copy>
      </xsl:copy>
    </xsl:variable>
    <xsl:apply-templates select="$temp" mode="#current"/>
  </xsl:template>

  <xsl:template match="ForExpr[count(SimpleForClause/(*)) gt 2]|
    LetExpr[count(SimpleLetClause/(*)) gt 2]" mode="process" priority="1">
    <xsl:variable name="temp" as="element()">
      <xsl:variable name="var" select="(* except TOKEN)[1]"/>
      <xsl:copy>
        <xsl:for-each select="$var">
          <xsl:copy>
            <xsl:sequence select="subsequence(*,1,2)"/>
          </xsl:copy>
        </xsl:for-each>
        <xsl:copy>
          <xsl:for-each select="$var">
            <xsl:copy>
              <xsl:sequence select="subsequence(*,3)"/>
            </xsl:copy>
          </xsl:for-each>
          <xsl:sequence select="* except $var"/>
        </xsl:copy>
      </xsl:copy>
    </xsl:variable>
    <xsl:apply-templates select="$temp" mode="#current"/>
  </xsl:template>
  <!--<xsl:template match="LetExpr[count(SimpleLetClause/(*)) gt 2]" mode="process" priority="1">
    <xsl:variable name="temp" as="element()">
      <xsl:copy>
        <SimpleLetClause>
          <xsl:sequence select="subsequence(SimpleLetClause/*,1,2)"/>
        </SimpleLetClause>
        <xsl:copy>
          <SimpleLetClause>
            <xsl:sequence select="subsequence(SimpleLetClause/*,3)"/>
          </SimpleLetClause>
          <xsl:sequence select="* except SimpleLetClause"/>
        </xsl:copy>
      </xsl:copy>
    </xsl:variable>
    <xsl:apply-templates select="$temp" mode="#current"/>
  </xsl:template>-->

  <xsl:template match="FunctionCall" mode="process">
    <xsl:copy>
      <xsl:attribute name="name" select="FunctionEQName"/>
      <xsl:apply-templates select="* except FunctionEQName" mode="#current"/>
    </xsl:copy>
  </xsl:template>
  <xsl:template match="ArgumentList" mode="process">
    <xsl:apply-templates select="*" mode="#current"/>
  </xsl:template>


  <xsl:template match="QuantifiedExpr" mode="process">
    <xsl:copy>
      <xsl:attribute name="type" select="TOKEN[1]"/>
      <xsl:for-each-group select="* except TOKEN[1]"
        group-starting-with="TOKEN[.=('$','satisfies')]" bind-group="parts">
        <xsl:choose>
          <xsl:when test="$parts[1]='$'">
            <QE.in var="{$parts[self::VarName]}">
              <xsl:apply-templates select="$parts[not(self::VarName)]" mode="#current"/>
            </QE.in>
          </xsl:when>
          <xsl:otherwise>
            <QE.satisfies>
              <xsl:apply-templates select="$parts" mode="#current"/>
            </QE.satisfies>
          </xsl:otherwise>
        </xsl:choose>
      </xsl:for-each-group>
    </xsl:copy>
  </xsl:template>


  <xsl:template match="ForExpr/ExprSingle|ForExpr/LetExpr" mode="process">
    <xsl:copy>
      <xsl:attribute name="s:role">return</xsl:attribute>
      <xsl:apply-templates select="*|text()" mode="#current"/>
    </xsl:copy>
  </xsl:template>
  <xsl:template match="SimpleForBinding|SimpleLetBinding" mode="process">
    <xsl:copy>
      <xsl:attribute name="var" select="VarName"/>
      <xsl:apply-templates select="* except VarName|text()" mode="#current"/>
    </xsl:copy>
  </xsl:template>



  <xsl:template match="VarRef" mode="process">
    <xsl:copy>
      <xsl:attribute name="name" select="VarName"/>
    </xsl:copy>
  </xsl:template>
  <xsl:template match="VarName" mode="process">
    <xsl:copy>
      <xsl:attribute name="name" select="."/>
    </xsl:copy>
  </xsl:template>

  <xsl:template match="QName[FunctionName]" mode="process">
    <xsl:copy>
      <xsl:attribute name="local" select="."/>
    </xsl:copy>
  </xsl:template>

  <xsl:template match="Literal" mode="process">
    <xsl:copy>
      <xsl:choose>
        <xsl:when test="StringLiteral">
          <xsl:attribute name="type">xs:string</xsl:attribute>
        </xsl:when>
        <xsl:when test="NumericLiteral/IntegerLiteral">
          <xsl:attribute name="type">xs:integer</xsl:attribute>
        </xsl:when>
        <xsl:when test="NumericLiteral/DecimalLiteral">
          <xsl:attribute name="type">xs:decimal</xsl:attribute>
        </xsl:when>
        <xsl:when test="NumericLiteral/DoubleLiteral">
          <xsl:attribute name="type">xs:double</xsl:attribute>
        </xsl:when>
      </xsl:choose>
      <xsl:attribute name="value" select="."/>
    </xsl:copy>
  </xsl:template>

  <xsl:template
    match="Expr | ExprSingle | OrExpr | AndExpr | ComparisonExpr | StringConcatExpr | RangeExpr | 
    AdditiveExpr | MultiplicativeExpr | UnionExpr | IntersectExceptExpr |
    InstanceofExpr | TreatExpr | CastableExpr | CastExpr | UnaryExpr |
    ValueExpr | SimpleMapExpr | PathExpr | RelativePathExpr | StepExpr |
    PostfixExpr | PrimaryExpr |
    Argument | NodeTest"
    mode="process">
    <xsl:choose>
      <xsl:when test="count(* except PredicateList[empty(*)]) eq 1 and not(TOKEN='/')">
        <xsl:apply-templates mode="#current"/>
      </xsl:when>
      <xsl:otherwise>
        <xsl:next-match/>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>
  <xsl:template match="SimpleForClause | SimpleLetClause" mode="process">
    <xsl:choose>
      <xsl:when test="count(* except TOKEN) eq 1">
        <xsl:apply-templates mode="#current"/>
      </xsl:when>
      <xsl:otherwise>
        <xsl:next-match/>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>
  <xsl:template match="ParenthesizedExpr" mode="process">
    <xsl:choose>
      <xsl:when test="count(* except TOKEN[.=('(',')')]) eq 1">
        <xsl:apply-templates mode="#current"/>
      </xsl:when>
      <xsl:otherwise>
        <xsl:next-match/>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>
</xsl:stylesheet>
