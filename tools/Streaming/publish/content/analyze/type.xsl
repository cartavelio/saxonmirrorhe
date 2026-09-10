<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
  xmlns:xs="http://www.w3.org/2001/XMLSchema"
  xmlns:math="http://www.w3.org/2005/xpath-functions/math"
  xmlns:map="http://www.w3.org/2005/xpath-functions/map" xmlns:xp="http://saxonica.com/xpathParse"
  xmlns:fos="http://www.w3.org/xpath-functions/spec/namespace"
  xmlns:e="http://www.w3.org/1999/XSL/Spec/ElementSyntax" xmlns:f="myfunctions"
  xmlns:doc="http://www.saxonica.com/ns/documentation" xmlns:dc="http://purl.org/dc/elements/1.1/"
  xmlns:s="StreamAnalysis" exclude-result-prefixes="xs math fos" version="3.0">





  <xsl:mode name="s:Type" on-no-match="shallow-copy"/>
  <xsl:template match="xp:raw|xp:*[@s:op.role='match']" mode="s:Type">
    <xsl:copy-of select="."/>
  </xsl:template>
  <xsl:template match=" xsl:stylesheet|xsl:transform" mode="s:Type">
    <xsl:copy>
      <xsl:variable name="variables" as="element()*">
        <!-- Should be an interation -->
        <xsl:apply-templates select="xsl:variable|xsl:param" mode="#current"/>
      </xsl:variable>
      <xsl:apply-templates select="@*|*|text()" mode="#current">
        <xsl:with-param name="variables" as="map(xs:string,element())"
          select="map:new($variables/map:entry(@name,.))" tunnel="yes"/>
      </xsl:apply-templates>
    </xsl:copy>
  </xsl:template>
  <xsl:template match=" xsl:mode|xsl:output|xsl:strip-space" mode="s:Type">
    <xsl:copy-of select="."/>
  </xsl:template>

  <xsl:template match="*" mode="s:Type">
    <xsl:param name="variables" as="map(xs:string,element())" select="map:new()" tunnel="yes"/>
    <xsl:param name="focus-label" as="attribute()*" select="()"/>
    <xsl:copy>
      <xsl:sequence select="@*,$focus-label"/>
      <xsl:variable name="children" as="item()*">
        <xsl:iterate select="*|text()">
          <xsl:param name="variables" select="$variables"/>
          <xsl:variable name="temp" as="item()*">
            <xsl:apply-templates select="." mode="#current">
              <xsl:with-param name="variables" select="$variables" tunnel="yes"/>
            </xsl:apply-templates>
          </xsl:variable>
          <xsl:sequence select="$temp"/>
          <xsl:if test="self::xsl:variable|self::xsl:param">
            <xsl:next-iteration>
              <xsl:with-param name="variables" as="map(xs:string,element())"
                select="map:new(($variables,map:entry(@name,$temp)))"/>
            </xsl:next-iteration>
          </xsl:if>
        </xsl:iterate>
      </xsl:variable>
      <xsl:variable name="staticType" as="xs:string?">
        <xsl:apply-templates select="." mode="s:staticTypeVal">
          <xsl:with-param name="children" select="$children[self::*]"/>
        </xsl:apply-templates>
      </xsl:variable>
      <xsl:attribute name="s:staticType" select="$staticType" on-empty="()"/>
      <xsl:copy-of select="$children"/>
    </xsl:copy>
  </xsl:template>

  <xsl:template match="xp:*|xsl:*|s:sequence-constructor" mode="s:Type">
    <xsl:param name="contextItemType" as="xs:string?" select="()" tunnel="yes"/>
    <xsl:param name="focus-changer" select="()" as="xs:integer?" tunnel="yes"/>
    <xsl:param name="focus-label" as="attribute()*" select="()"/>
    <xsl:param name="show.focus-ids" as="xs:boolean" select="false()" tunnel="yes"/>
    <xsl:param name="variables" as="map(xs:string,element())" select="map:new()" tunnel="yes"/>

    <xsl:variable name="id" select="count(ancestor::*|preceding::*)"/>
    <xsl:variable name="focus-change" select="$focus-change(local-name(.))"/>
    <xsl:variable name="changes-focus" select="exists($focus-change)"/>

    <xsl:copy>
      <xsl:sequence select="@*,$focus-label"/>
      <xsl:attribute name="s:focus-changing" select="'true'[exists($focus-change)]" on-empty="()"/>
      <xsl:attribute name="s:contextItemType" select="$contextItemType" on-empty="()"/>
      <xsl:attribute name="s:focus-setter" select="$focus-changer[$show.focus-ids]" on-empty="()"/>
      <xsl:choose>
        <xsl:when test="exists($focus-change)">
          <xsl:attribute name="s:focus-id" select="$id[$show.focus-ids]" on-empty="()"/>
          <xsl:variable name="except" select="xp:raw|xsl:param|xsl:with-param|text()"/>
          <xsl:variable name="is-stream" select="self::xsl:stream"/>
          <!-- <xsl:attribute name="s:focus-changing" select="'true'"/>-->
          <xsl:variable name="controlling.index" select="index-of($focus-change,'controlling')[1]"/>
          <xsl:variable name="controller"
            select="if(exists($controlling.index)) then (* except $except)[$controlling.index] else ()"/>
          <xsl:variable name="typed-controller" as="element()?">
            <xsl:apply-templates select="$controller" mode="#current">
              <xsl:with-param name="focus-label" as="attribute()*">
                <xsl:attribute name="s:focus">controlling</xsl:attribute>
              </xsl:with-param>
            </xsl:apply-templates>
          </xsl:variable>
          <xsl:variable name="type" select="($typed-controller/@s:staticType,$contextItemType)[1]"/>
          <xsl:variable name="children" as="item()*">
            <xsl:iterate select="*|text()">
              <xsl:param name="variables" select="$variables"/>
              <xsl:choose>
                <xsl:when test=". is $controller">
                  <xsl:copy-of select="$typed-controller"/>
                </xsl:when>
                <xsl:otherwise>
                  <xsl:variable name="real" select="exists(. except $except)"/>
                  <xsl:variable name="pos"
                    select="count((preceding-sibling::*|preceding-sibling::text()) except $except) + 1"/>
                  <xsl:variable name="temp" as="item()*">
                    <xsl:apply-templates select="." mode="#current">
                      <xsl:with-param name="variables" select="$variables" tunnel="yes"/>
                      <xsl:with-param name="contextItemType"
                        select="if($is-stream) then 'document-node()' else
                        if($real and $focus-change[$pos] eq 'controlled') then
                        $type else $contextItemType"
                        tunnel="yes"/>
                      <xsl:with-param name="focus-changer"
                        select="if($real) then $id else $focus-changer" tunnel="yes"/>
                      <xsl:with-param name="focus-label" as="attribute()*">
                        <xsl:attribute name="s:focus" select="$focus-change[$pos][$real]"
                          on-empty="()"/>
                      </xsl:with-param>
                    </xsl:apply-templates>
                  </xsl:variable>
                  <xsl:sequence select="$temp"/>
                  <xsl:if test="self::xsl:variable|self::xsl:param">
                    <xsl:next-iteration>
                      <xsl:with-param name="variables" as="map(xs:string,element())"
                        select="map:new(($variables,map:entry(@name,$temp)))"/>
                    </xsl:next-iteration>
                  </xsl:if>
                </xsl:otherwise>
              </xsl:choose>
            </xsl:iterate>
          </xsl:variable>
          <xsl:variable name="staticType" as="xs:string?">
            <xsl:apply-templates select="." mode="s:staticTypeVal">
              <xsl:with-param name="children" select="$children[self::*]"/>
            </xsl:apply-templates>
          </xsl:variable>
          <xsl:attribute name="s:staticType" select="$staticType" on-empty="()"/>
          <xsl:copy-of select="$children"/>
        </xsl:when>
        <xsl:otherwise>
          <xsl:variable name="children" as="item()*">
            <xsl:iterate select="*|text()">
              <xsl:param name="variables" select="$variables"/>
              <xsl:variable name="temp" as="item()*">
                <xsl:apply-templates select="." mode="#current">
                  <xsl:with-param name="variables" select="$variables" tunnel="yes"/>
                </xsl:apply-templates>
              </xsl:variable>
              <xsl:sequence select="$temp"/>
              <xsl:if test="self::xsl:variable|self::xsl:param|self::xp:QE.var">
                <xsl:next-iteration>
                  <xsl:with-param name="variables" as="map(xs:string,element())"
                    select="map:new(($variables,map:entry(@name,$temp)))"/>
                </xsl:next-iteration>
              </xsl:if>
            </xsl:iterate>
          </xsl:variable>
          <xsl:variable name="staticType" as="xs:string?">
            <xsl:apply-templates select="." mode="s:staticTypeVal">
              <xsl:with-param name="children" select="$children[self::*]"/>
            </xsl:apply-templates>
          </xsl:variable>
          <xsl:attribute name="s:staticType" select="($staticTypes(local-name()),$staticType)[1]"
            on-empty="()"/>
          <xsl:copy-of select="$children"/>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:copy>
  </xsl:template>

  <xsl:template match="xp:AxisStep" mode="s:Type">
    <xsl:param name="focus-label" as="attribute()*" select="()"/>
    <xsl:copy>
      <xsl:sequence select="@*,$focus-label"/>
      <xsl:variable name="staticType" as="xs:string?">
        <xsl:apply-templates select="." mode="s:staticTypeVal"/>
      </xsl:variable>
      <xsl:attribute name="s:staticType" select="$staticType" on-empty="()"/>
      <xsl:apply-templates select="*" mode="#current">
        <xsl:with-param name="contextItemType" select="$staticType" tunnel="yes"/>
        <xsl:with-param name="focus-label" as="attribute()*">
          <xsl:attribute name="s:focus" select="'controlled'" on-empty="()"/>
        </xsl:with-param>
      </xsl:apply-templates>
    </xsl:copy>
  </xsl:template>

  <xsl:template match="xp:VarRef" mode="s:Type">
    <xsl:param name="focus-label" as="attribute()*" select="()"/>
    <xsl:param name="variables" as="map(xs:string,element())" select="map:new()" tunnel="yes"/>
    <xsl:copy>
      <xsl:sequence select="@*,$focus-label"/>
      <xsl:attribute name="vars" select="map:keys($variables)"/>
      <xsl:variable name="staticType" as="xs:string?">
        <xsl:apply-templates select="." mode="s:staticTypeVal"/>
      </xsl:variable>
      <xsl:attribute name="s:staticType" select="$staticType" on-empty="()"/>
    </xsl:copy>
  </xsl:template>

  <xsl:template match="xp:ContextItemExpr" mode="s:Type">
    <xsl:param name="contextItemType" as="xs:string?" select="()" tunnel="yes"/>
    <xsl:param name="focus-changer" select="()" as="xs:integer?" tunnel="yes"/>
    <xsl:param name="focus-label" as="attribute()*" select="()"/>
    <xsl:param name="show.focus-ids" as="xs:boolean" select="false()" tunnel="yes"/>
    <xsl:copy>
      <xsl:sequence select="@*,$focus-label"/>
      <xsl:attribute name="s:contextItemType" select="$contextItemType" on-empty="()"/>
      <xsl:attribute name="s:staticType" select="$contextItemType" on-empty="()"/>
      <xsl:attribute name="s:focus-setter" select="$focus-changer[$show.focus-ids]" on-empty="()"/>
    </xsl:copy>
  </xsl:template>

  <!--  <xsl:mode name="s:staticType" on-no-match="shallow-copy"/>

    <xsl:template match="*" mode="s:staticType">
        <xsl:copy>
            <xsl:sequence select="@*"/>
            <xsl:variable name="children" as="item()*">
                <xsl:apply-templates select="*|text()" mode="#current"/>
            </xsl:variable>
            <xsl:variable name="staticType" as="xs:string?">
                <xsl:apply-templates select="." mode="s:staticTypeVal">
                    <xsl:with-param name="children" select="$children[self::*]"/>
                </xsl:apply-templates>
            </xsl:variable>
            <xsl:attribute name="s:staticType" select="$staticType" on-empty="()"/>
            <xsl:copy-of select="$children"/>
        </xsl:copy>
    </xsl:template>-->

  <!--<xsl:template match="*" mode="s:staticType">
        <xsl:copy>
            <xsl:apply-templates select="@*" mode="#current"/>
            <xsl:attribute name="s:staticType"
                select="concat('element(Q{',namespace-uri(.),'}',local-name(.),')')"/>
            <xsl:apply-templates select="*|text()" mode="#current"/>
        </xsl:copy>
    </xsl:template>-->


  <xsl:mode name="s:staticTypeVal" on-no-match="deep-skip"/>

  <xsl:template match="*" mode="s:staticTypeVal" as="xs:string?">
    <xsl:variable name="ns" select="namespace-uri(.)"/>
    <xsl:value-of select="string-join(('element(',('Q{',$ns,'}')[$ns],local-name(.),')'),'')"/>
  </xsl:template>

  <xsl:function name="xp:sequence-constructor" as="xs:boolean">
    <xsl:param name="item" as="node()"/>
    <xsl:sequence
      select="$item/empty(self::xp:*|
            self::xsl:param|self::xsl:with-param|
            self::xsl:sort|
            self::xsl:next-iteration|self::xsl:on-completion)"
    />
  </xsl:function>

  <xsl:template match="xsl:*" mode="s:staticTypeVal" as="xs:string?" priority="1">
    <xsl:param name="use-detailed-types" as="xs:boolean" select="false()" tunnel="yes"/>
    <xsl:param name="children" select="()" as="element()*"/>
    <xsl:choose>
      <xsl:when test="$use-detailed-types">
        <xsl:next-match>
          <xsl:with-param name="children" as="element()*" select="$children"/>
        </xsl:next-match>
      </xsl:when>
      <xsl:otherwise>item()</xsl:otherwise>
    </xsl:choose>
  </xsl:template>
  <!--   <xsl:template match="xsl:*" mode="s:staticTypeVal" as="xs:string?" use-when="false()">
        <xsl:text>item()</xsl:text>
    </xsl:template>-->

  <xsl:template match="s:sequence-constructor" mode="s:staticTypeVal" as="xs:string?" priority="1.5">
    <xsl:param name="children" select="()" as="element()*"/>
    <xsl:value-of
      select="xp:composite-type-multiple($children[not(self::xsl:variable)]/@s:staticType)"/>
  </xsl:template>
  <xsl:template match="xsl:variable|xsl:param" mode="s:staticTypeVal" as="xs:string?" priority="1.5">
    <xsl:param name="children" select="()" as="element()*"/>
    <xsl:value-of select="xp:composite-type-multiple($children/@s:staticType)"/>
  </xsl:template>
  <xsl:template match="xsl:iterate | xsl:for-each | xsl:stream | xsl:template"
    mode="s:staticTypeVal" as="xs:string?">
    <xsl:param name="children" select="()" as="element()*"/>
    <xsl:value-of
      select="xp:composite-type-multiple($children[self::s:sequence-constructor]/@s:staticType)"/>
  </xsl:template>
  <xsl:template match="xsl:stylesheet|xsl:transform" mode="s:staticTypeVal" as="xs:string?"/>
  <xsl:template match="xsl:apply-templates | xsl:apply-imports | xsl:next-iteration"
    mode="s:staticTypeVal" as="xs:string?">item()*</xsl:template>
  <!--   <xsl:template match="xsl:text | xsl:value-of" mode="s:staticTypeVal" as="xs:string?" use-when="false()">
        <xsl:text>text()</xsl:text>
    </xsl:template>-->
  <xsl:template match="xsl:copy-of|xsl:param|xsl:with-param" mode="s:staticTypeVal" as="xs:string?">
    <xsl:param name="children" select="()" as="element()*"/>
    <xsl:value-of select="xp:composite-type($children/@s:staticType)"/>
  </xsl:template>
  <xsl:template match="xsl:if|xsl:break" mode="s:staticTypeVal" as="xs:string?">
    <xsl:param name="children" select="()" as="element()*"/>
    <xsl:value-of
      select="xp:composite-type-possible($children[self::s:sequence-constructor]/@s:staticType)"/>
  </xsl:template>
  <xsl:template match="xsl:choose" mode="s:staticTypeVal" as="xs:string?">
    <xsl:param name="children" select="()" as="element()*"/>
    <xsl:value-of
      select="xp:composite-type-possible($children/s:sequence-constructor/@s:staticType)"/>
  </xsl:template>
  <xsl:template match="xsl:when|xsl:otherwise" mode="s:staticTypeVal" as="xs:string?"/>




  <xsl:template match="xp:*" mode="s:staticTypeVal" as="xs:string?">
    <xsl:param name="children" select="()" as="element()*"/>
    <xsl:value-of select="$staticTypes(local-name(.))"/>
  </xsl:template>
  <!--    <xsl:template match="xp:Root" mode="s:staticTypeVal" as="xs:string?">
        <xsl:param name="children" select="()" as="element()*"/>
        <xsl:text>document-node()</xsl:text>
    </xsl:template>-->
  <xsl:template match="xp:Literal" mode="s:staticTypeVal" as="xs:string?">
    <xsl:value-of select="@type"/>
  </xsl:template>
  <xsl:variable name="type-preserving-functions"
    select="'remove','head','tail','subsequence','unordered',
    'outermost','innermost','exactly-one','zero-or-more','one-or-more',
    'trace'"/>
  <xsl:template match="xp:FunctionCall" mode="s:staticTypeVal" as="xs:string?">
    <xsl:param name="children" select="()" as="element()*"/>
    <xsl:try>
      <xsl:choose>
        <xsl:when test="@name=$type-preserving-functions">
          <xsl:value-of select="($children/@s:staticType)[1]"/>
        </xsl:when>
        <xsl:otherwise>
          <xsl:value-of select="xp:composite-type($functions(@name)('returnType'))"/>
        </xsl:otherwise>
      </xsl:choose>
      <xsl:catch>
        <xsl:attribute name="noTypeFor" select="@name||'returnType'"/>
      </xsl:catch>
    </xsl:try>
  </xsl:template>

  <xsl:template match="xp:CastExpr" mode="s:staticTypeVal" as="xs:string?">
    <xsl:value-of select="@as"/>
  </xsl:template>

  <xsl:template match="xp:UnionExpr" mode="s:staticTypeVal" as="xs:string?">
    <xsl:param name="children" select="()" as="element()*"/>
    <xsl:value-of select="xp:composite-type($children/@s:staticType)"/>
  </xsl:template>

  <xsl:template match="xp:Expr" mode="s:staticTypeVal" as="xs:string?">
    <xsl:param name="children" select="()" as="element()*"/>
    <xsl:value-of select="xp:composite-type-cardinal($children/@s:staticType)"/>
  </xsl:template>
  <xsl:template match="xp:IfExpr" mode="s:staticTypeVal" as="xs:string?">
    <xsl:param name="children" select="()" as="element()*"/>
    <xsl:value-of select="xp:composite-type($children[@s:role=('then','else')]/@s:staticType)"/>
  </xsl:template>

  <xsl:template match="xp:ZZZAxisStep" mode="s:staticTypeVal" as="xs:string?">
    <xsl:param name="children" select="()" as="element()*"/>
    <xsl:value-of select="$children[1]/@s:staticType"/>
  </xsl:template>

  <xsl:template
    match="xp:ForwardStep[@axis='self'][@nodeTest='node()']|
    xp:AxisStep[@axis='self'][@nodeTest='node()']"
    mode="s:staticTypeVal" as="xs:string?">
    <xsl:param name="contextItemType" as="xs:string?" select="()" tunnel="yes"/>
    <xsl:value-of select="$contextItemType"/>
  </xsl:template>
  <xsl:template
    match="xp:RelativePathExpr | xp:PathExpr | xp:AxisStep |
        xp:ForwardStep | xp:ReverseStep |
        xp:StepExpr |
        xp:ForwardStepP"
    mode="s:staticTypeVal" as="xs:string?">
    <xsl:param name="children" select="()" as="element()*"/>
    <xsl:value-of
      select="replace(
            (@nodeTest,($children[not(self::xp:PredicateList)])[last()]/@s:staticType)[1],
            'Q\{\}','')"
    />
  </xsl:template>
  <xsl:template match="xp:LetExpr" mode="s:staticTypeVal" as="xs:string?">
    <xsl:param name="children" select="()" as="element()*"/>
    <xsl:value-of select="$children[last()]/@s:staticType"/>
  </xsl:template>
  <xsl:template match="xp:ForExpr|xp:SimpleMapExpr" mode="s:staticTypeVal" as="xs:string?">
    <xsl:param name="children" select="()" as="element()*"/>
    <xsl:value-of
      select="let $t := $children[last()]/@s:staticType return if(matches($t,'\*$|\?$')) then $t else concat($t,'*')"
    />
  </xsl:template>
  <xsl:template match="xp:IntersectExceptExpr" mode="s:staticTypeVal" as="xs:string?">
    <xsl:param name="children" select="()" as="element()*"/>
    <xsl:sequence select="$children[1]/@s:staticType"/>
  </xsl:template>

  <xsl:template match="xp:QE.var" mode="s:staticTypeVal" as="xs:string?" priority="1.5">
    <xsl:param name="children" select="()" as="element()*"/>
    <xsl:value-of select="xp:composite-type-multiple($children/@s:staticType)"/>
  </xsl:template>

  <xsl:template match="xp:VarRef" mode="s:staticTypeVal" as="xs:string">
    <xsl:param name="variables" as="map(xs:string,element())" select="map:new()" tunnel="yes"/>
    <xsl:value-of select="($variables(@name)/@s:staticType,'UNKNOWN VAR/TYPE')[1]"/>
  </xsl:template>



</xsl:stylesheet>
