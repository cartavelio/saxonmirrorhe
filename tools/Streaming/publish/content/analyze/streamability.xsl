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

  <!--<xsl:variable name="useRex" as="xs:boolean" select="true()" static="yes"/>-->

  <xsl:import href="catalogs.xsl"/>

  <xsl:include href="xpath-construct.xsl"/>

  <xsl:include href="type-hierarchy.xsl"/>

  <xsl:include href="type.xsl"/>

  <xsl:include href="postureSweep.xsl"/>

  <xsl:function name="xp:invert" as="map(xs:string,xs:string)">
    <xsl:param name="map" as="map(xs:string,xs:string)"/>
    <xsl:sequence select="map:new((map:keys($map) ! (let $k := . return map:entry($map(.),$k))))"/>
  </xsl:function>

  <xsl:template name="analyze-streamability">
    <xsl:param name="show.raw" as="xs:boolean" select="false()"/>
    <xsl:variable name="construct" as="element()*">
      <xsl:apply-templates select="./*" mode="xp:P">
        <xsl:with-param name="show.raw" as="xs:boolean" select="$show.raw" tunnel="yes"/>
      </xsl:apply-templates>
    </xsl:variable>
    <xsl:variable name="construct.numbered" as="element()*">
      <xsl:apply-templates select="$construct" mode="number-nodes"/>
    </xsl:variable>
    <xsl:variable name="contextItemTyped" as="element()*">
      <xsl:apply-templates select="$construct.numbered" mode="s:Type">
        <xsl:with-param name="use-detailed-types" select="true()" tunnel="yes"/>
      </xsl:apply-templates>
    </xsl:variable>
    <xsl:variable name="usage" as="element()*">
      <xsl:apply-templates select="$contextItemTyped" mode="addUsage"/>
    </xsl:variable>
    <xsl:variable name="posture" as="element()*">
      <xsl:apply-templates select="$usage" mode="addPosture"/>
    </xsl:variable>
    <xsl:sequence select="if($show.raw) then $construct else ()" use-when="true()"/>
    <xsl:sequence select="$contextItemTyped" use-when="false()"/>
    <xsl:sequence select="$usage" use-when="false()"/>
    <xsl:sequence select="if($show.raw) then () else $posture" use-when="true()"/>
  </xsl:template>

  <xsl:mode name="number-nodes" on-no-match="shallow-copy"/>
  <xsl:template match="*" mode="number-nodes">
    <xsl:copy>
      <xsl:attribute name="id" select="generate-id(.)"/>
      <xsl:apply-templates select="@*|*|text()" mode="#current"/>
    </xsl:copy>
  </xsl:template>

  <xsl:template match="xp:raw" mode="s:staticType contextItemTyped addUsage addPosture">
    <xsl:copy-of select="."/>
  </xsl:template>


  <xsl:variable name="XSLTConstructs" as="element()">
    <xsltConstructs>
      <X:for-each focus-changing="controlling controlled"
        f-c="select:controlling sequence:controlled"/>
      <X:iterate focus-changing="controlling controlled"/>
      <X:result-document staticType="item()*"/>
      <X:stream focus-changing="controlled" context-posture="striding"/>
      <X:template focus-changing="controlling controlled" href="#streamable-templates"/>
      <X:text staticType="text()"/>
      <X:value-of staticType="text()"/>
      <X:when href="#streamability-xsl-choose"/>
      <X:otherwise href="#streamability-xsl-choose"/>
      <s:sequence-constructor href="#classifying-sequence-constructors"/>
    </xsltConstructs>
  </xsl:variable>

  <xsl:variable name="expressionConstructs" as="element()*">
    <expressionConstructs href="#classifying-expressions">
      <xp:Expr usage="T*"/>
      <xp:ForExpr usage="N T" href="#streamability-of-for-expressions"/>
      <xp:SimpleForClause/>
      <xp:LetExpr usage="N T" usageOLD="let:T be:N return:T"/>
      <xp:QuantifiedExpr staticType="xs:boolean" href="#streamability-of-quantified-expressions"/>
      <xp:IfExpr usage="if:I then:T else:T" href="#streamability-of-if-expressions"
        choice-group="then else"/>
      <xp:OrExpr staticType="xs:boolean" usage="I I"/>
      <xp:AndExpr staticType="xs:boolean" usage="I I"/>
      <xp:ComparisonExpr staticType="xs:boolean" usage="A A"/>
      <xp:StringConcatExpr staticType="xs:string" usage="A A"/>
      <xp:RangeExpr staticType="xs:integer*" usage="A A"/>
      <xp:AdditiveExpr staticType="xs:anyAtomicType" usage="A A"/>
      <xp:MultiplicativeExpr staticType="xs:anyAtomicType" usage="A A"/>
      <xp:UnionExpr href="#streamability-of-union-expressions"/>
      <xp:IntersectExceptExpr href="#streamability-of-union-expressions"/>
      <xp:InstanceOfExpr staticType="xs:boolean"/>
      <xp:TreatExpr usage="T"/>
      <xp:CastableExpr usage="A" staticType="xs:boolean"/>
      <xp:CastExpr usage="A"/>
      <xp:UnaryExpr staticType="numeric"/>
      <xp:SimpleMapExpr focus-changing="controlling controlled"
        href="#streamability-of-simple-mapping-expressions"/>
      <xp:PathExpr focus-changing="controlling controlled" href="#streamability-of-path-expressions"/>
      <xp:RelativePathExpr/>
      <xp:AxisStep focus-changing="controlling controlled" href="#streamability-of-axis-steps"/>
      <xp:StepExpr focus-changing="controlling controlled"/>
      <xp:ForwardStep href="#streamability-of-axis-steps"/>
      <xp:ReverseStep href="#streamability-of-axis-steps"/>
      <xp:PostfixExpr href="#streamability-of-filter-expressions"/>
      <xp:Literal posture="grounded" sweep="motionless"/>
      <xp:VarRef href="#streamability-of-variable-references"/>
      <xp:ParenthesizedExpr/>
      <xp:ContextItemExpr href="#streamability-of-context-item-expression"/>
      <xp:FunctionCall href="#streamability-of-function-calls"/>
      <xp:NamedFunctionRef href="#streamability-of-named-function-ref"/>
      <xp:InlineFunctionExpr href="#streamability-of-inline-functions"/>
      <xp:MapExpr staticType="map(*)"/>
      <xp:Root staticType="document-node()"/>
    </expressionConstructs>
  </xsl:variable>
  <xsl:variable name="patternConstructs" as="element()*">
    <expressionConstructs href="#classifying-expressions">
      <!--<xp:Expr usage="T*"/>
      <xp:ForExpr href="#streamability-of-for-expressions"/>
      <xp:SimpleForClause/>
      <xp:LetExpr usage="let:T be:N return:T"/>
      <xp:QuantifiedExpr staticType="xs:boolean" href="#streamability-of-quantified-expressions"/>
      <xp:IfExpr usage="if:I then:A else:A" href="#streamability-of-if-expressions"/>
      <xp:OrExpr staticType="xs:boolean" usage="I I"/>
      <xp:AndExpr staticType="xs:boolean" usage="I I"/>
      <xp:ComparisonExpr staticType="xs:boolean" usage="A A"/>
      <xp:StringConcatExpr staticType="xs:string" usage="A A"/>
      <xp:RangeExpr staticType="xs:integer*" usage="A A"/>
      <xp:AdditiveExpr staticType="xs:anyAtomicType" usage="A A"/>
      <xp:MultiplicativeExpr staticType="xs:anyAtomicType" usage="A A"/>-->
      <xp:UnionExprP href="#streamability-of-union-expressions"/>
      <!--<xp:IntersectExceptExpr href="#streamability-of-union-expressions"/>
      <xp:InstanceOfExpr staticType="xs:boolean"/>
      <xp:TreatExpr/>
      <xp:CastableExpr staticType="xs:boolean"/>
      <xp:CastExpr/>
      <xp:UnaryExpr staticType="numeric"/>
      <xp:SimpleMapExpr focus-changing="controlling controlled"
        href="#streamability-of-simple-mapping-expressions"/>
      <xp:PathExpr focus-changing="controlling controlled" href="#streamability-of-path-expressions"/>
      <xp:RelativePathExpr/>
      <xp:AxisStep href="#streamability-of-axis-steps"/>
      <xp:StepExpr focus-changing="controlling controlled"/>-->
      <xp:ForwardStepP href="#streamability-of-axis-steps"/>
      <!--<xp:ReverseStep href="#streamability-of-axis-steps"/>
      <xp:PostFixExpr/>
      <xp:Literal posture="grounded" sweep="motionless"/>
      <xp:VarRef href="#streamability-of-variable-references"/>
      <xp:ParenthesizedExpr/>
      <xp:ContextItemExpr href="#streamability-of-context-item-expression"/>
      <xp:FunctionCall href="#streamability-of-function-calls"/>
      <xp:NamedFunctionRef href="#streamability-of-named-function-ref"/>
      <xp:InlineFunctionExpr href="#streamability-of-inline-functions"/>
      <xp:MapExpr staticType="map(*)"/>
      <xp:Root staticType="document-node()"/>-->
    </expressionConstructs>
  </xsl:variable>

  <xsl:variable name="special-built-in-functions" as="element()">
    <special-functions>
      <accumulator-after/>
      <accumulator-before/>
      <current/>
      <current-group/>
      <current-grouping-key/>
      <function-lookup/>
      <last/>
      <outermost/>
      <position/>
      <root/>
      <unparsed-entity-public-id/>
      <unparsed-entity-uri/>
    </special-functions>
  </xsl:variable>

  <xsl:variable name="constructs" select="$expressionConstructs|$patternConstructs|$XSLTConstructs"/>

  <xsl:variable name="staticTypes" as="map(xs:string,xs:string)"
    select="map:new($constructs/*[@staticType]!map:entry(local-name(.),string(@staticType)))"/>
  <xsl:variable name="postures" as="map(xs:string,xs:string)"
    select="map:new($constructs/*[@posture]!map:entry(local-name(.),string(@posture)))"/>
  <xsl:variable name="context-postures" as="map(xs:string,xs:string)"
    select="map:new($constructs/*[@context-posture]!map:entry(local-name(.),string(@context-posture)))"/>
  <xsl:variable name="sweeps" as="map(xs:string,xs:string)"
    select="map:new($constructs/*[@sweep]!map:entry(local-name(.),string(@sweep)))"/>
  <xsl:variable name="general-stream" as="map(xs:string,xs:boolean)"
    select="map{$constructs/* ! local-name() := exists(@usage)}"/>
  <xsl:variable name="spec-ref" as="map(xs:string,xs:string)"
    select="map:new(($expressionConstructs|$patternConstructs)/* ! map:entry(local-name(.),string((@href,../@href)[1])))"/>
  <xsl:variable name="fn-spec-ref" as="map(xs:string,xs:string)"
    select="map:new($special-built-in-functions/* ! map:entry(local-name(.),concat('#streamability-fn-',local-name(.))))"/>
  <xsl:variable name="fn-builtin-ref" as="map(xs:string,xs:string)"
    select="map:new(map:keys($functions) ! map:entry(.,'#classifying-built-in-functions'))"/>
  <xsl:variable name="spec-ref.xslt" as="map(xs:string,xs:string)"
    select="map:new((
        map:keys($instructions) ! (let $ln := replace(.,'xsl:','') return map:entry($ln,'#streamability-xsl-'||$ln)),
        $XSLTConstructs/*[@href] ! map:entry(local-name(.),string(@href))
        ))"/>


  <xsl:variable name="focus-changing" as="map(xs:string,attribute()*)" use-when="true()">
    <xsl:map>
      <xsl:for-each select="$constructs/*[@focus-changing]">
        <xsl:map-entry key="local-name(.)">
          <xsl:for-each select="tokenize(@focus-changing,'\s+')">
            <xsl:attribute name="s:focus" select="."/>
          </xsl:for-each>
        </xsl:map-entry>
      </xsl:for-each>
    </xsl:map>
  </xsl:variable>


  <xsl:variable name="focus-changing" as="map(xs:string,attribute()*)" use-when="false()">
    <xsl:variable name="temp" as="item()*">
      <xsl:for-each select="$constructs/*[@focus-changing]">
        <xsl:variable name="attr" as="attribute()*">
          <xsl:for-each select="tokenize(@focus-changing,'\s+')">
            <xsl:attribute name="s:focus" select="."/>
          </xsl:for-each>
        </xsl:variable>
        <xsl:sequence select="map:entry(local-name(.),$attr)"/>
      </xsl:for-each>
    </xsl:variable>
    <xsl:sequence select="map:new($temp)"/>
  </xsl:variable>
  
  <xsl:variable name="focus-change" as="map(xs:string,xs:string*)"
    select="
    map:new($constructs/*[@focus-changing]/map:entry(local-name(.),tokenize(@focus-changing,'\s+')))"> </xsl:variable>

  <xsl:variable name="focus-change" as="map(xs:string,xs:string*)" use-when="false()">
    <xsl:map>
      <xsl:for-each select="$constructs/*[@focus-changing]">
        <xsl:map-entry key="local-name(.)">
          <xsl:sequence select="tokenize(@focus-changing,'\s+')"/>
        </xsl:map-entry>
      </xsl:for-each>
    </xsl:map>
  </xsl:variable>


  <xsl:variable name="usageAbbrev" as="map(xs:string,xs:string)"
    select="map{'A':='absorption','I':='inspection','N':='navigation','T':='transmission','TD':='type-determined'}"/>
  <xsl:variable name="usagePatterns" as="map(xs:string,map(*))"
    select="map:new($constructs/*[@usage]!
        map:entry(local-name(.), map:new(
        let $parts := tokenize(@usage,'\s+') return
        (if(contains(@usage,':')) then 
        ($parts ! (let $t := tokenize(.,':') return map:entry($t[1],$usageAbbrev($t[2]))))
        else if(contains(@usage,'*')) then
        map:entry('ALL',$usageAbbrev(replace(@usage,'\*','')))
        else
        for $i in 1 to count($parts) return map:entry($i,$usageAbbrev($parts[$i])))            
        )))"/>
  <xsl:variable name="choice-groups" as="map(xs:string,xs:string*)"
    select="map:new($constructs/*[@choice-group]/map:entry(local-name(.),tokenize(@choice-group,'\s+')))"/>


  <xsl:function name="xp:type-determined-usage" as="xs:string">
    <xsl:param name="type" as="xs:string?"/>
    <!-- TODO - add function subtype -->
    <xsl:sequence
      select="if($type) then if(xp:least-common-supertype($type,'xs:anyAtomicType') = 'xs:anyAtomicType')
            then 'absorption' else 'navigation' else 'navigation'"
    />
  </xsl:function>

  <xsl:mode name="addUsage" on-no-match="shallow-copy"/>
  <xsl:template match="*" mode="addUsage">
    <xsl:copy>
      <xsl:apply-templates select="@*" mode="#current"/>
      <xsl:apply-templates select="." mode="Usage"/>
      <xsl:apply-templates select="*|text()" mode="#current"/>
    </xsl:copy>
  </xsl:template>

  <xsl:mode name="Usage" on-no-match="deep-skip"/>

  <xsl:template match="xp:IfExpr/*[@s:role]|xp:ZZZLetExpr/*[@s:role]" mode="Usage" as="attribute()*">
    <xsl:variable name="role" select="@s:role"/>
    <xsl:attribute name="s:usage" select="$usagePatterns(local-name(..))($role)"/>
    <xsl:if test="@s:role = $choice-groups(local-name(..))">
      <xsl:attribute name="s:choice-group">yes</xsl:attribute>
    </xsl:if>
    <!-- Something for choice-groups here -->
  </xsl:template>

  <xsl:template
    match="Expr/* | AdditiveExpr/* | OrExpr/* | AndExpr/* | MultiplicativeExpr/*|
        ComparisonExpr/* | StringConcatExpr/* | RangeExpr/* |
        ForExpr/* | LetExpr/* | TreatExpr/* | CastableExpr/* | CastExpr/*"
    mode="Usage" as="attribute()*" xpath-default-namespace="http://saxonica.com/xpathParse">
    <xsl:variable name="pos" select="count(preceding-sibling::*) + 1"/>
    <xsl:attribute name="s:usage"
      select="for $r in ($pos,'ALL') return $usagePatterns(local-name(..))($r)"/>
  </xsl:template>

  <xsl:template match="xp:*[@s:role='href']|xp:*[contains(@s:role,'AVT.')]" mode="Usage"
    as="attribute()*">
    <xsl:attribute name="s:usage">absorption</xsl:attribute>
  </xsl:template>



  <xsl:template match="xp:FunctionCall/*" mode="Usage">
    <xsl:if test="../@name='remove'">
      <xsl:attribute name="{../@name}.pos" select="count(preceding-sibling::*) + 1"/>
    </xsl:if>
    <xsl:if test="exists($functions(../@name))">

      <xsl:attribute name="s:usage"
        select="let $p := count(preceding-sibling::*) + 1 return ($functions(../@name)('args')($p)/@usage,'absorption')[1]"
      />
    </xsl:if>
  </xsl:template>



  <xsl:template match="xsl:*/*[@s:role=('select','test') or starts-with(@s:role,'AVT.')]"
    mode="Usage">
    <xsl:variable name="role" select="replace(@s:role,'^AVT\.|\.\d+$','')"/>
    <xsl:variable name="usage"
      select="($instructions(name(..))/self::e:attribute[@name=$role]/@usage,
            $declarations(name(..))/self::e:attribute[@name=$role]/@usage)[1]"/>
    <xsl:attribute name="s:usage"
      select="if($usage = 'type-determined') then xp:type-determined-usage(parent::*/@as) else $usage"
      on-empty="()"/>
  </xsl:template>

  <xsl:template match="xsl:*/s:sequence-constructor" mode="Usage">
    <xsl:variable name="name" select="local-name()"/>
    <xsl:variable name="model" select="$instructions(name(..))/(self::e:model|e:model)[@name=$name]"/>
    <xsl:attribute name="s:usage" select="$model/@usage" on-empty="()"/>
    <!--<xsl:attribute name="s:OTHERusage"
      select="$model/@usage[exists(current()/parent::xsl:otherwise)]"
      on-empty="()"/>-->
    <xsl:attribute name="s:choiceGroup" select="$model/@choice-group" on-empty="()"/>
  </xsl:template>
  <xsl:template match="s:sequence-constructor/xsl:*" mode="Usage">
    <xsl:attribute name="s:usage">transmission</xsl:attribute>
  </xsl:template>
  <xsl:template match="xsl:ZZZnext-iteration/xsl:with-param/xp:*[@s:role='select']" mode="Usage"
    priority="1">
    <xsl:attribute name="s:usage" select="xp:type-determined-usage(@s:staticType)"/>
  </xsl:template>




</xsl:stylesheet>
