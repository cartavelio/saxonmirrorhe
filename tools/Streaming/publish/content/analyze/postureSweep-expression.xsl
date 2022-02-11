<?xml version="1.0" encoding="UTF-8"?>

<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
  xmlns:xs="http://www.w3.org/2001/XMLSchema"
  xmlns:math="http://www.w3.org/2005/xpath-functions/math"
  xmlns:map="http://www.w3.org/2005/xpath-functions/map" xmlns:xp="http://saxonica.com/xpathParse"
  xmlns:fos="http://www.w3.org/xpath-functions/spec/namespace"
  xmlns:e="http://www.w3.org/1999/XSL/Spec/ElementSyntax" xmlns:f="myfunctions"
  xmlns:doc="http://www.saxonica.com/ns/documentation" xmlns:dc="http://purl.org/dc/elements/1.1/"
  xmlns:X="IndirectXSLT" xmlns:s="StreamAnalysis" exclude-result-prefixes="xs math fos"
  version="3.0">
  <xsl:variable name="rex1" select="true()" static="yes"/>
  <xsl:variable name="saxonAxis" select="true()" static="yes"
    doc:doc="Additional functionality in Saxon"/>

  <xsl:template name="expr-init" doc:doc="Common initialisation for an expression">
    <xsl:sequence select="@*"/>
    <xsl:call-template name="stream-rule-expr"/>
    <xsl:call-template name="record-contextPosture"/>
  </xsl:template>


  <xsl:template match="xp:*" mode="addPosture">
    <xsl:call-template name="general-streamability">
      <xsl:with-param name="spec-rule" as="attribute()?">
        <xsl:call-template name="stream-rule-expr"/>
      </xsl:with-param>
    </xsl:call-template>
  </xsl:template>

  <xsl:include href="postureSweep-pattern.xsl"/>

  <xsl:template match="xp:FunctionCall[@name='root']" mode="addPosture">
    <xsl:variable name="X" as="element()">
      <xsl:apply-templates select="*" mode="#current"/>
    </xsl:variable>
    <xsl:choose>
      <!-- Something on the posture -->
      <xsl:when test="$X[@s:staticType='document-node()' and @s:posture=$p.striding]">
        <xsl:copy>
          <xsl:call-template name="expr-init"/>
          <xsl:sequence select="$X/(@s:posture,@s:sweep),$X"/>
        </xsl:copy>
      </xsl:when>
      <xsl:otherwise>
        <!-- Rewrite head((X)/ancestor-or-self::node()) -->
        <xsl:variable name="Xp" as="element()*">
          <xp:FunctionCall name="head" s:staticType="node()?">
            <xsl:sequence select="@s:usage"/>
            <xp:RelativePathExpr s:usage="transmission">
              <xsl:sequence select="*"/>
              <xp:AxisStep axis="ancestor-or-self" nodeTest="node()"/>
            </xp:RelativePathExpr>
          </xp:FunctionCall>
        </xsl:variable>
        <xsl:apply-templates select="$Xp" mode="#current"/>
      </xsl:otherwise>
    </xsl:choose>

  </xsl:template>
  <xsl:template match="xp:FunctionCall[@name='last']" mode="addPosture">
    <xsl:param name="contextPosture" as="xs:string?" tunnel="yes"/>
    <xsl:copy>
      <xsl:call-template name="expr-init"/>
      <xsl:sequence
        select="if($contextPosture = ('striding','crawling','roaming'))
         then $RFR else ($p.grounded,$s.motionless)"
      />
    </xsl:copy>
  </xsl:template>
  <!-- The current rules for
        
for $v in S return R

are

	• If R is not grounded and motionless, then roaming and free-ranging.
	• Otherwise, the posture and sweep of S.
	
Firstly, rule 1 is ambiguous.The intended reading is (not(grounded and motionless)) rather than (not(grounded) and motionless).

But the requirement to be motionless seems unnecessary. It disallows, for example

for $x in product/copy(.) return $x/price - $x/discount

Secondly, the rule as written allows

for $x in 1 to 3 return child::product

which should not be streamable.

Assuming we don't pursue the idea of binding variables to streamed nodes (see bug 23391), I think the rules should be:

1. If R is not grounded, then roaming and free-ranging
2. Otherwise, the general streamability rules apply, where the two operands are:
   * S (usage navigation)
   * R (usage transmission, higher-order)
     -->

  <xsl:template use-when="true()"
    match="xp:PathExpr[xp:PathExpr/xp:ForwardStep[@axis='descendant-or-self'][@nodeTest='node()']]
        [xp:ForwardStep[@axis='child']]"
    mode="addPosture">
    <xsl:variable name="main" select="xp:ForwardStep"/>
    <xsl:variable name="temp" as="element()">
      <xsl:copy>
        <xsl:sequence select="@*"/>
        <xsl:for-each select="xp:PathExpr">
          <xp:StepExpr>
            <xsl:sequence select="@*,*[1]"/>
            <xp:ForwardStep axis="descendant-or-self" nodeTest="{$main/@nodeTest}">
              <xsl:sequence select="$main/@s:*"/>
            </xp:ForwardStep>
          </xp:StepExpr>
        </xsl:for-each>
        <xp:PredicateList>
          <xp:Predicate>
            <xp:ReverseStep axis="parent" nodeTest="node()"/>
          </xp:Predicate>
        </xp:PredicateList>
      </xsl:copy>
    </xsl:variable>
    <xsl:apply-templates select="$temp" mode="#current"/>
  </xsl:template>

  <xsl:template use-when="true()"
    match="RelativePathExpr[RelativePathExpr/AxisStep[@axis='descendant-or-self']]
    [AxisStep[@axis='child']]"
    xpath-default-namespace="http://saxonica.com/xpathParse" xmlns="http://saxonica.com/xpathParse"
    mode="addPosture">
    <xsl:variable name="main" select="AxisStep"/>
    <xsl:variable name="parent" select="RelativePathExpr/AxisStep/@nodeTest"/>
    <xsl:variable name="temp" as="element()">
      <xsl:copy>
        <xsl:sequence select="@*"/>
        <xsl:attribute name="SSSSSSSSS"/>
        <xsl:for-each select="RelativePathExpr">
          <xsl:sequence select="@*,*[1]"/>
          <AxisStep axis="descendant-or-self" nodeTest="{$main/@nodeTest}">
            <xsl:sequence select="$main/@s:*"/>
            <PredicateList>
              <Predicate>
                <AxisStep axis="parent" nodeTest="{$parent}"/>
              </Predicate>
            </PredicateList>
          </AxisStep>
        </xsl:for-each>
      </xsl:copy>
    </xsl:variable>
    <xsl:apply-templates select="$temp" mode="#current"/>
  </xsl:template>

  <xsl:template match="xp:ContextItemExpr" mode="addPosture" priority="1">
    <xsl:param name="contextPosture" as="xs:string?" tunnel="yes"/>
    <xsl:variable name="axis" select="@axis"/>
    <xsl:copy>
      <xsl:call-template name="expr-init"/>
      <xsl:attribute name="s:posture" select="$contextPosture"/>
      <xsl:attribute name="s:sweep">motionless</xsl:attribute>
    </xsl:copy>
  </xsl:template>

  <xsl:template match="xp:PathExpr|xp:RelativePathExpr" mode="addPosture">
    <xsl:param name="debug" as="xs:string?" tunnel="yes"/>
    <xsl:copy>
      <xsl:call-template name="expr-init"/>
      <xsl:variable name="op.left" as="element()">
        <xsl:apply-templates select="*[1]" mode="#current"/>
      </xsl:variable>
      <xsl:variable name="op.right" as="element()*">
        <xsl:apply-templates select="*[2]" mode="#current">
          <xsl:with-param name="contextPosture" select="$op.left/@s:posture" tunnel="yes"/>
        </xsl:apply-templates>
      </xsl:variable>
      <xsl:choose>
        <xsl:when test="count(*) eq 1">
          <xsl:sequence select="$op.left/(@s:sweep,@s:posture)"/>
        </xsl:when>
        <xsl:otherwise>
          <xsl:attribute name="s:posture" select="$op.right/@s:posture"/>
          <xsl:if test="exists($op.left/@s:sweep) and exists($op.right/@s:sweep)">
            <xsl:try>
              <xsl:attribute name="s:sweep" select="$wider($op.left/@s:sweep)($op.right/@s:sweep)"/>
              <xsl:catch>
                <xsl:message
                  select="$debug,'Error in expression sweep widening -','left='||$op.left/@s:sweep,
              'right='||$op.right/@s:sweep"
                />
              </xsl:catch>
            </xsl:try>
          </xsl:if>
        </xsl:otherwise>
      </xsl:choose>
      <xsl:copy-of select="$op.left,$op.right"/>
    </xsl:copy>
  </xsl:template>

  <xsl:template match="xp:PredicateList" mode="addPosture">
    <xsl:copy>
      <xsl:sequence select="@*"/>
      <xsl:apply-templates select="*" mode="#current"/>
    </xsl:copy>
  </xsl:template>
  <xsl:template match="xp:Predicate" mode="addPosture">
    <xsl:copy>
      <xsl:sequence select="@*"/>
      <xsl:variable name="children" as="element()*">
        <xsl:apply-templates select="*" mode="#current"/>
      </xsl:variable>
      <xsl:sequence select="$children/(@s:posture|@s:sweep),$children"/>
    </xsl:copy>
  </xsl:template>

  <xsl:template match="xp:StepExpr[xp:PredicateList]" mode="addPosture">
    <xsl:copy>
      <xsl:call-template name="expr-init"/>
      <xsl:variable name="op.controlling" as="element()">
        <xsl:apply-templates select="*[@s:focus='controlling']" mode="#current"/>
      </xsl:variable>
      <xsl:variable name="op.controlled" as="element()*">
        <xsl:apply-templates select="*[@s:focus='controlled']" mode="#current">
          <xsl:with-param name="contextPosture" select="$op.controlling/@s:posture" tunnel="yes"/>
        </xsl:apply-templates>
      </xsl:variable>
      <xsl:choose>
        <xsl:when test="$op.controlled/@s:sweep = 'motionless'">
          <xsl:sequence select="$op.controlling/(@s:posture|@s:sweep)"/>
        </xsl:when>
        <xsl:otherwise>
          <xsl:sequence select="$RFR"/>
        </xsl:otherwise>
      </xsl:choose>
      <xsl:copy-of select="$op.controlling,$op.controlled"/>
    </xsl:copy>
  </xsl:template>

  <xsl:template match="xp:PostfixExpr[xp:Predicate]" mode="addPosture">
    <xsl:copy>
      <xsl:call-template name="expr-init"/>
      <xsl:variable name="op.left" as="element()">
        <xsl:apply-templates select="*[1]" mode="#current"/>
      </xsl:variable>
      <xsl:variable name="op.right" as="element()*">
        <xsl:apply-templates select="*[2]" mode="#current">
          <xsl:with-param name="contextPosture" select="$op.left/@s:posture" tunnel="yes"/>
        </xsl:apply-templates>
      </xsl:variable>
      <xsl:variable name="children" as="element()*">
        <xsl:apply-templates select="*" mode="#current"/>
      </xsl:variable>
      <xsl:choose>
        <xsl:when
          test="$op.left
          [@s:posture='crawling' and @s:sweep='consuming']
          [self::xp:Literal[@type=('xs:decimal','xs:integer','xs:double')]]">
          <xsl:attribute name="s:posture">crawling</xsl:attribute>
          <xsl:attribute name="s:sweep">consuming</xsl:attribute>
        </xsl:when>
        <xsl:when test="$op.right[@s:sweep='motionless']">
          <xsl:sequence select="$op.left/(@s:posture|@s:sweep)"/>
        </xsl:when>
        <xsl:otherwise>
          <xsl:sequence select="$RFR"/>
        </xsl:otherwise>
      </xsl:choose>
      <xsl:sequence select="$op.left,$op.right"/>
    </xsl:copy>
  </xsl:template>


  <xsl:template match="xp:SimpleMapExpr" mode="addPosture">
    <xsl:copy>
      <xsl:call-template name="expr-init"/>
      <xsl:variable name="op.left" as="element()">
        <xsl:apply-templates select="*[1]" mode="#current"/>
      </xsl:variable>
      <xsl:variable name="op.right" as="element()*">
        <xsl:apply-templates select="*[2]" mode="#current">
          <xsl:with-param name="contextPosture" select="$op.left/@s:posture" tunnel="yes"/>
        </xsl:apply-templates>
      </xsl:variable>
      <xsl:sequence select="$op.right/(@s:posture,@s:sweep)"/>
      <!--<xsl:attribute name="s:posture" select="$op.right/@s:posture"/>
      <xsl:attribute name="s:sweep" select="$op.right/@s:sweep"/>-->
      <xsl:copy-of select="$op.left,$op.right"/>
    </xsl:copy>
  </xsl:template>

  <xsl:template match="ForExpr" mode="addPosture"
    xpath-default-namespace="http://saxonica.com/xpathParse">
    <xsl:copy>
      <xsl:call-template name="expr-init"/>
      <xsl:variable name="children" as="element()*">
        <xsl:apply-templates select="*" mode="#current"/>
      </xsl:variable>
      <xsl:choose>
        <xsl:when test="$children[2]/@s:posture = 'grounded'">
          <xsl:call-template name="general-streamability.operands">
            <xsl:with-param name="children" as="item()*" select="$children"/>
          </xsl:call-template>
        </xsl:when>
        <xsl:otherwise>
          <xsl:sequence select="$RFR,$children"/>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:copy>
  </xsl:template>

 <!-- <xsl:template match="xp:ForwardStep| xp:ReverseStep | xp:ForwardStepP" mode="addPosture"
    priority="1" use-when="not($rex1)">
    <xsl:param name="contextPosture" as="xs:string?" tunnel="yes"/>
    <xsl:variable name="axis" select="@axis"/>
    <xsl:copy>
      <xsl:call-template name="expr-init"/>
      <xsl:choose>
        <xsl:when test="$contextPosture = 'grounded'">
          <xsl:attribute name="s:posture">grounded</xsl:attribute>
          <xsl:attribute name="s:sweep">motionless</xsl:attribute>
        </xsl:when>
        <xsl:when test="$contextPosture = 'roaming'">
          <xsl:attribute name="s:posture">roaming</xsl:attribute>
          <xsl:attribute name="s:sweep">free-ranging</xsl:attribute>
        </xsl:when>
        <xsl:when test="@s:contextItemType = 'text()' and $axis = 'child'">
          <!-\- More constructs here - generalized -\->
          <xsl:attribute name="s:posture">grounded</xsl:attribute>
          <xsl:attribute name="s:sweep">motionless</xsl:attribute>
        </xsl:when>
        <xsl:when test="xp:PredicateList">
          <!-\- Check Predicate not motionless -\->
          <xsl:attribute name="s:posture">roaming</xsl:attribute>
          <xsl:attribute name="s:sweep">free-ranging</xsl:attribute>
        </xsl:when>
        <xsl:when
          test="$contextPosture = 'climbing' and
                    $axis = ('self','parent','ancestor-or-self','ancestor','attribute','namespace')">
          <xsl:attribute name="s:posture">climbing</xsl:attribute>
          <xsl:attribute name="s:sweep">motionless</xsl:attribute>
        </xsl:when>
        <xsl:when
          test="$contextPosture = 'striding' and
                    $axis = ('parent','ancestor-or-self','ancestor','attribute','namespace')">
          <xsl:attribute name="s:posture">climbing</xsl:attribute>
          <xsl:attribute name="s:sweep">motionless</xsl:attribute>
        </xsl:when>
        <xsl:when test="$contextPosture = 'striding' and
                    $axis = ('self')">
          <xsl:attribute name="s:posture">striding</xsl:attribute>
          <xsl:attribute name="s:sweep">motionless</xsl:attribute>
        </xsl:when>
        <xsl:when test="$contextPosture = 'striding' and
                    $axis = ('child')">
          <xsl:attribute name="s:posture">striding</xsl:attribute>
          <xsl:attribute name="s:sweep">consuming</xsl:attribute>
        </xsl:when>
        <xsl:when
          test="$contextPosture = 'striding' and
                    $axis = ('descendant','descendant-or-self')">
          <xsl:attribute name="s:posture">crawling</xsl:attribute>
          <xsl:attribute name="s:sweep">consuming</xsl:attribute>
        </xsl:when>
        <xsl:when
          test="$contextPosture = 'crawling' and
                    $axis = ('parent','ancestor-or-self','ancestor','attribute','namespace')">
          <xsl:attribute name="s:posture">climbing</xsl:attribute>
          <xsl:attribute name="s:sweep">motionless</xsl:attribute>
        </xsl:when>
        <xsl:when test="$contextPosture = 'crawling' and
                    $axis = ('self')">
          <xsl:attribute name="s:posture">crawling</xsl:attribute>
          <xsl:attribute name="s:sweep">motionless</xsl:attribute>
        </xsl:when>
        <xsl:when
          test="$contextPosture = 'crawling' and
          $axis = ('child','descendant','descendant-or-self')"
          use-when="$saxonAxis">
          <xsl:attribute name="s:posture">crawling</xsl:attribute>
          <xsl:attribute name="s:sweep">consuming</xsl:attribute>
        </xsl:when>
        <xsl:otherwise>
          <xsl:attribute name="TOOKDEFAULT"/>
          <xsl:attribute name="s:posture">roaming</xsl:attribute>
          <xsl:attribute name="s:sweep">free-ranging</xsl:attribute>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:copy>
  </xsl:template>-->

  <xsl:template match="xp:ForwardStep| xp:ReverseStep | xp:ForwardStepP" mode="addPosture"
    priority="1">
    <xsl:param name="contextPosture" as="xs:string?" tunnel="yes"/>
    <xsl:variable name="axis" select="@axis"/>
    <xsl:copy>
      <xsl:call-template name="expr-init"/>
    </xsl:copy>
  </xsl:template>

  <xsl:template match="xp:AxisStep" mode="addPosture" priority="1">
    <xsl:param name="contextPosture" as="xs:string?" tunnel="yes"/>
    <xsl:variable name="axis"
      select="(@axis,(xp:ForwardStep|xp:ReverseStep|xp:ForwardStepP)/@axis)[1]"/>
    <xsl:copy>
      <xsl:call-template name="expr-init"/>
      <xsl:variable name="children" as="element()*">
        <xsl:apply-templates select="*" mode="#current"/>
      </xsl:variable>
      <xsl:choose>
        <xsl:when test="$contextPosture = 'grounded'">
          <xsl:sequence select="$p.grounded,$s.motionless"/>
        </xsl:when>
        <xsl:when test="$contextPosture = 'roaming'">
          <xsl:sequence select="$RFR"/>
        </xsl:when>
        <xsl:when test="@s:contextItemType = 'text()' and $axis = 'child'">
          <!-- More constructs here - generalized -->
          <xsl:sequence select="$p.grounded,$s.motionless"/>
        </xsl:when>
        <!-- Point 4 -->
        <xsl:when test="$children/xp:Predicate[xp:not-motionless(.)]">
          <xsl:sequence select="$RFR"/>
        </xsl:when>
        <xsl:when
          test="$contextPosture = 'climbing' and
          $axis = ('self','parent','ancestor-or-self','ancestor','attribute','namespace')">
          <xsl:sequence select="$p.climbing,$s.motionless"/>
        </xsl:when>
        <xsl:when
          test="$contextPosture = 'striding' and
          $axis = ('parent','ancestor-or-self','ancestor','attribute','namespace')">
          <xsl:sequence select="$p.climbing,$s.motionless"/>
        </xsl:when>
        <xsl:when test="$contextPosture = 'striding' and
          $axis = ('self')">
          <xsl:sequence select="$p.striding,$s.motionless"/>
        </xsl:when>
        <xsl:when test="$contextPosture = 'striding' and
          $axis = ('child')">
          <xsl:sequence select="$p.striding,$s.consuming"/>
        </xsl:when>
        <xsl:when
          test="$contextPosture = 'striding' and
          $axis = ('descendant','descendant-or-self')">
          <xsl:sequence select="$p.crawling,$s.consuming"/>
        </xsl:when>
        <xsl:when
          test="$contextPosture = 'crawling' and
          $axis = ('parent','ancestor-or-self','ancestor','attribute','namespace')">
          <xsl:sequence select="$p.climbing,$s.motionless"/>
        </xsl:when>
        <xsl:when test="$contextPosture = 'crawling' and
          $axis = ('self')">
          <xsl:sequence select="$p.crawling,$s.motionless"/>
        </xsl:when>
        <xsl:when
          test="$saxonAxis and $contextPosture = 'crawling' and
          $axis = ('child','descendant','descendant-or-self')">
          <xsl:sequence select="$p.crawling,$s.consuming"/>
        </xsl:when>
        <xsl:otherwise>
          <xsl:attribute name="TOOKDEFAULT"/>
          <xsl:sequence select="$RFR"/>
        </xsl:otherwise>
      </xsl:choose>
      <xsl:sequence select="$children"/>
    </xsl:copy>
  </xsl:template>


  <xsl:template match="xp:UnionExpr" mode="addPosture">
    <xsl:copy>
      <xsl:call-template name="expr-init"/>
      <xsl:variable name="ops" as="element()+">
        <xsl:apply-templates select="*" mode="#current"/>
      </xsl:variable>
      <xsl:choose>
        <xsl:when test="$ops[@s:sweep='free-ranging']">
          <xsl:sequence select="$RFR"/>
          <!--<xsl:attribute name="s:posture">roaming</xsl:attribute>
          <xsl:attribute name="s:sweep">free-ranging</xsl:attribute>-->
        </xsl:when>
        <xsl:when test="$ops[@s:posture='grounded' and @s:sweep='motionless']">
          <xsl:variable name="gm"
            select="($ops[@s:posture='grounded' and @s:sweep='motionless'])[1]"/>
          <xsl:sequence select="($ops except $gm)/(@s:posture|@s:sweep)"/>
        </xsl:when>
        <xsl:when
          test="every $o in $ops satisfies $o[@s:posture='climbing' and @s:sweep='motionless']">
          <xsl:attribute name="s:posture">climbing</xsl:attribute>
          <xsl:attribute name="s:sweep">motionless</xsl:attribute>
        </xsl:when>
        <xsl:when test="every $o in $ops satisfies $o[@s:posture=('striding','crawling')]">
          <xsl:attribute name="s:posture">crawling</xsl:attribute>
          <xsl:attribute name="s:sweep" select="$wider($ops[1]/@s:sweep)($ops[2]/@s:sweep)"/>
        </xsl:when>
        <xsl:otherwise>
          <xsl:sequence select="$RFR"/>
          <!--<xsl:attribute name="s:posture">roaming</xsl:attribute>
          <xsl:attribute name="s:sweep">free-ranging</xsl:attribute>-->
        </xsl:otherwise>
      </xsl:choose>
      <xsl:sequence select="$ops"/>
    </xsl:copy>
  </xsl:template>

  <!-- HERE -->
  <xsl:template match="xp:PathExpr/xp:VarRef" mode="addPosture">
    <xsl:copy>
      <xsl:call-template name="expr-init"/>
      <xsl:choose>
        <xsl:when test="@group-bound"/>
        <xsl:otherwise>
          <xsl:attribute name="s:posture">roaming</xsl:attribute>
          <xsl:attribute name="s:sweep">free-ranging</xsl:attribute>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:copy>
  </xsl:template>
</xsl:stylesheet>
