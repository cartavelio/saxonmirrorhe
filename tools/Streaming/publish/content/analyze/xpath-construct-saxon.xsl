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
  
  <xsl:mode name="remove.init" on-no-match="shallow-copy"/>
  <xsl:template match="xp:homogeneityChecker" mode="remove.init">
    <xsl:apply-templates select="*" mode="#current"/>
  </xsl:template>
  
  
  <xsl:template match="xp:root" mode="construct">
    <xsl:param name="is.match" as="xs:boolean" select="false()"/>
    <xsl:choose>
      <xsl:when test="$is.match">
        <xp:Root>
          <xsl:attribute name="s:role" select="'match'[$is.match]" on-empty="()"/>
        </xp:Root>
      </xsl:when>
      <xsl:otherwise>
        <xp:FunctionCall name="root">
          <xp:ForwardStep axis="self" nodeTest="node()"/>
        </xp:FunctionCall>
      </xsl:otherwise>
    </xsl:choose>

  </xsl:template>
  <xsl:template match="xp:operator[@op=('or')]" mode="construct">
    <xp:OrExpr>
      <xsl:apply-templates select="@*|*|text()" mode="#current"/>
    </xp:OrExpr>
  </xsl:template>
  <xsl:template match="xp:operator[@op=('and')]" mode="construct">
    <xp:AndExpr>
      <xsl:apply-templates select="@*|*|text()" mode="#current"/>
    </xp:AndExpr>
  </xsl:template>
  <xsl:template match="xp:operator[@op=('+','-')]" mode="construct">
    <xp:AdditiveExpr>
      <xsl:apply-templates select="@*|*|text()" mode="#current"/>
    </xp:AdditiveExpr>
  </xsl:template>
  <xsl:template match="xp:operator[@op=('*','div','idiv','mod')]" mode="construct">
    <xp:MultiplicativeExpr>
      <xsl:apply-templates select="@*|*|text()" mode="#current"/>
    </xp:MultiplicativeExpr>
  </xsl:template>
  <xsl:template
    match="xp:operator[@op=('eq','ne','lt','le','gt','ge',
        '=','!=','&lt;','&lt;=','&gt;','&gt;=',
        'is','&lt;&lt;','&gt;&gt;')]"
    mode="construct">
    <xp:ComparisonExpr>
      <xsl:apply-templates select="@*|*|text()" mode="#current"/>
    </xp:ComparisonExpr>
  </xsl:template>
  <xsl:template match="xp:operator[@op=('to')]" mode="construct">
    <xp:RangeExpr>
      <xsl:apply-templates select="*|text()" mode="#current"/>
    </xp:RangeExpr>
  </xsl:template>
  <xsl:template match="xp:operator[@op=('|')]" mode="construct">
    <xp:UnionExpr>
      <xsl:apply-templates select="*|text()" mode="#current"/>
    </xp:UnionExpr>
  </xsl:template>
  <xsl:template match="xp:operator[@op=('|')][xp:operator[@op=('|')][1]]" mode="construct">
    <xsl:variable name="temp" as="element()*">
      <xsl:next-match/>
    </xsl:variable>
    <xp:UnionExpr>
      <xsl:sequence select="head($temp/*)/*"/>
      <xsl:sequence select="tail($temp/*)"/>
    </xp:UnionExpr>
  </xsl:template>

  <xsl:template match="xp:sequence" mode="construct">
    <xp:Expr>
      <xsl:apply-templates select="@*|*|text()" mode="#current"/>
    </xp:Expr>
  </xsl:template>


  <xsl:template match="xp:functionCall" mode="construct">
    <xp:FunctionCall>
      <xsl:apply-templates select="@*|*|text()" mode="#current"/>
    </xp:FunctionCall>
  </xsl:template>

  <xsl:template
    match="xp:functionCall[@name=($default-dot-functions)][empty(*)] |
    xp:FunctionCall[@name=($default-dot-functions)][empty(*)]"
    mode="construct">
    <xp:FunctionCall>
      <xsl:apply-templates select="@*|*|text()" mode="#current"/>
      <xp:ContextItemExpr/>
    </xp:FunctionCall>
  </xsl:template>
  <xsl:template
    match="xp:functionCall[@name='reverse'][xp:axis[@name=$axes.reverse] | xp:filterExpression/xp:axis[@name=$axes.reverse]]"
    mode="construct" doc:doc="Remove reverse() on reverse axes">
    <xsl:apply-templates select="*" mode="#current"/>
  </xsl:template>

  <xsl:template match="xp:cast" mode="construct">
    <xp:CastExpr>
      <xsl:attribute name="as" select="replace(@as,'Q\{http://www\.w3\.org/2001/XMLSchema\}','xs:')"/>
      <xsl:apply-templates select="(@* except @as)|*|text()" mode="#current"/>
    </xp:CastExpr>
  </xsl:template>

  <xsl:template match="xp:variableReference" mode="construct">
    <xp:VarRef>
      <xsl:apply-templates select="(@* except @slot)|*|text()" mode="#current"/>
    </xp:VarRef>
  </xsl:template>
  <xsl:template match="xp:forEach" mode="construct">
    <xp:SimpleMapExpr>
      <xsl:apply-templates select="@*|*|text()" mode="#current"/>
    </xp:SimpleMapExpr>
  </xsl:template>
  <xsl:template match="xp:return" mode="construct">
    <xsl:apply-templates select="@*|*|text()" mode="#current"/>
  </xsl:template>

  <xsl:template match="xp:some" mode="construct">
    <xp:QuantifiedExpr type="{local-name()}">
      <xp:QE.var name="{@variable}">
        <xsl:apply-templates select="xp:in/*" mode="#current"/>
      </xp:QE.var>
      <xp:QE.satisfies>
        <xsl:apply-templates select="xp:satisfies/*" mode="#current"/>
      </xp:QE.satisfies>
    </xp:QuantifiedExpr>
  </xsl:template>



  <xsl:template match="xp:literal" mode="construct">
    <xsl:param name="label" as="xs:boolean" select="false()"/>
    <xp:Literal>
      <xsl:attribute name="s:role" select="('op'||string(position()))[$label]" on-empty="()"/>
      <xsl:apply-templates select="@*|*|text()" mode="#current"/>
    </xp:Literal>
  </xsl:template>
  <xsl:template match="xp:dot" mode="construct">
    <xp:ContextItemExpr/>
  </xsl:template>
  <xsl:template match="xp:axis" mode="construct">
    <xp:StepExpr>
      <xsl:sequence select="@*"/>
    </xp:StepExpr>
  </xsl:template>
  <xsl:template match="xp:axis[@name=$axes.forward]" mode="construct">
    <xp:ForwardStep axis="{@name}">
      <xsl:sequence select="@* except @name"/>
    </xp:ForwardStep>
  </xsl:template>
  <xsl:template match="xp:axis[@name=$axes.reverse]" mode="construct">
    <xp:ReverseStep axis="{@name}">
      <xsl:sequence select="@* except @name"/>
    </xp:ReverseStep>
  </xsl:template>
  <xsl:template match="xp:parentStep" mode="construct">
    <xp:ReverseStep axis="parent">
      <xsl:sequence select="@*"/>
    </xp:ReverseStep>
  </xsl:template>

  <xsl:template match="xp:slash" mode="construct">
    <xp:PathExpr>
      <xsl:apply-templates select="*" mode="#current">
        <xsl:with-param name="top-level" select="false()" tunnel="yes"/>
      </xsl:apply-templates>
    </xp:PathExpr>
  </xsl:template>

  <!--<xsl:template match="xp:slash" mode="construct" use-when="false()">
    <xsl:param name="top-level" tunnel="yes"/>
    <xsl:variable name="children" as="element()*">
      <xsl:apply-templates select="*" mode="#current">
        <xsl:with-param name="top-level" select="false()" tunnel="yes"/>
      </xsl:apply-templates>
    </xsl:variable>
    <xsl:choose>
      <xsl:when test="$top-level">
        <xp:PathExpr>
          <xsl:copy-of select="$children"/>
        </xp:PathExpr>
      </xsl:when>
      <xsl:otherwise>
        <xp:StepExpr op="/">
          <xsl:copy-of select="$children"/>
        </xp:StepExpr>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <xsl:template match="xp:slash[xp:slash]" mode="construct" use-when="false()">
    <xsl:variable name="temp" as="element()*">
      <xsl:next-match/>
    </xsl:variable>
    <xp:PathExpr>
      <xsl:copy-of select="head($temp/*)/*"/>
      <xsl:copy-of select="tail($temp/*)"/>
    </xp:PathExpr>
  </xsl:template>-->

  <xsl:template match="xp:filterExpression" mode="construct">
    <xsl:variable name="temp" as="element()*">
      <xsl:apply-templates select="*" mode="#current"/>
    </xsl:variable>
    <xp:StepExpr>
      <xsl:sequence select="$temp[1]"/>
      <xp:PredicateList>
        <xp:Predicate>
          <xsl:sequence select="$temp[2]"/>
        </xp:Predicate>
      </xp:PredicateList>
    </xp:StepExpr>
  </xsl:template>
  <xsl:template match="xp:filterExpression[xp:filterExpression[1]]" mode="construct">
    <xsl:variable name="temp" as="element()*">
      <xsl:next-match/>
    </xsl:variable>
    <xp:StepExpr>
      <xsl:sequence select="head(head($temp/*)/*)"/>
      <xp:PredicateList>
        <xsl:sequence select="tail(head($temp/*)/*)/*,tail($temp/*)/*"/>
      </xp:PredicateList>
    </xp:StepExpr>
  </xsl:template>

  <xsl:template match="xp:choose[count(xp:when) = 2][(xp:when)[2]/xp:literal/@value='true']"
    mode="construct">
    <xp:IfExpr>
      <xp:Expr s:role="if">
        <xsl:apply-templates select="xp:when[1]/*" mode="#current"/>
      </xp:Expr>
      <xp:ExprSingle s:role="then">
        <xsl:apply-templates select="xp:then[1]/*" mode="#current"/>
      </xp:ExprSingle>
      <xp:ExprSingle s:role="else">
        <xsl:apply-templates select="xp:then[2]/*" mode="#current"/>
      </xp:ExprSingle>
    </xp:IfExpr>
  </xsl:template>
  <xsl:template match="xp:choose[count(xp:when) = 1]" mode="construct">
    <xp:IfExpr>
      <xp:Expr s:role="if">
        <xsl:apply-templates select="xp:when[1]/*" mode="#current"/>
      </xp:Expr>
      <xp:ExprSingle s:role="then">
        <xsl:apply-templates select="xp:then[1]/*" mode="#current"/>
      </xp:ExprSingle>
      <!-- <xp:ExprSingle s:role="else">
        <xsl:apply-templates select="xp:then[2]/*" mode="#current"/>
      </xp:ExprSingle>-->
    </xp:IfExpr>
  </xsl:template>
  <xsl:template match="xp:for" mode="construct">
    <xp:ForExpr>
      <xp:SimpleForClause>
        <xp:VarName name="{@variable}" s:role="for">
          <xsl:sequence select="@as"/>
        </xp:VarName>
        <xp:ExprSingle s:role="in">
          <xsl:apply-templates select="xp:in/*" mode="#current"/>
        </xp:ExprSingle>
      </xp:SimpleForClause>
      <xp:ExprSingle s:role="return">
        <xsl:apply-templates select="xp:return/*" mode="#current"/>
      </xp:ExprSingle>
    </xp:ForExpr>
  </xsl:template>
  <xsl:template match="xp:let" mode="construct">
    <xp:LetExpr>
      <xp:SimpleLetClause>
        <!--<xp:SimpleLetBinding>-->
        <xp:VarName name="{@variable}" s:role="let">
          <xsl:sequence select="@as"/>
        </xp:VarName>
        <xp:ExprSingle s:role="be">
          <xsl:apply-templates select="xp:be/*" mode="#current"/>
        </xp:ExprSingle>
        <!--</xp:SimpleLetBinding>-->
      </xp:SimpleLetClause>
      <xp:ExprSingle s:role="return">
        <xsl:apply-templates select="* except xp:be" mode="#current"/>
      </xp:ExprSingle>
    </xp:LetExpr>
  </xsl:template>
</xsl:stylesheet>