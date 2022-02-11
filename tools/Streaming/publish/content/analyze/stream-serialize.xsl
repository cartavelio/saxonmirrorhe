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

  <xsl:import href="serialize.xsl"/>

  <xsl:include href="stream-controls.xsl"/>

  <xsl:param name="spec.container">_spec</xsl:param>

  <xsl:variable name="prop-names" as="map(xs:string,xs:string)"
    select="map:new($stream-controls//check ! map:entry(@type,string(@abbrev)))"> </xsl:variable>


  <xsl:variable name="expander" as="element()">
    <span class="folder">
      <span class="collapse">
        <img src="images/collapse-small-blue-Shapes4FREE.png"/>
      </span>
      <span class="expand">
        <img src="images/expand-small-blue-Shapes4FREE.png"/>
      </span>
    </span>
  </xsl:variable>
  <xsl:variable name="blank" as="element()">
    <span>
      <xsl:text>  </xsl:text>
    </span>
  </xsl:variable>

  <xsl:template match="xsl:*/@select | xsl:*/@test | xsl:*/@match | xsl:*/@group-adjacent"
    mode="serialize">
    <xsl:next-match>
      <xsl:with-param name="attribute.class">XPExpr</xsl:with-param>
    </xsl:next-match>
  </xsl:template>

  <xsl:template match="@id" mode="serialize"/>

  <xsl:template match="*[not(self::xp:*)]/@*[contains(.,'{')]" mode="serialize">
    <xsl:next-match>
      <xsl:with-param name="attribute.class">XPExpr</xsl:with-param>
      <xsl:with-param name="attributes" as="attribute()*">
        <xsl:attribute name="type" select="concat('AVT.',name(.))"/>
      </xsl:with-param>
    </xsl:next-match>
  </xsl:template>


  <xsl:template match="s:*[@s:href]|xsl:*[@s:href]|xp:*[@s:href]" mode="serialize" priority="2">
    <xsl:param name="spec" as="xs:string" tunnel="yes"/>
    <xsl:param name="spec.container" as="xs:string" select="$spec.container" tunnel="yes"/>
    <xsl:next-match>
      <xsl:with-param name="href" as="attribute()*" tunnel="yes">
        <xsl:attribute name="href" select="$spec||@s:href"/>
        <xsl:attribute name="target" select="$spec.container"/>
      </xsl:with-param>
    </xsl:next-match>
  </xsl:template>
  <xsl:template match="@s:href" mode="serialize"/>
  <xsl:template match="*[@s:*]" mode="serialize" priority="1">
    <xsl:variable name="temp" as="element()">
      <xsl:copy>
        <xsl:sequence select="(@* except @s:*),*|text()"/>
      </xsl:copy>
    </xsl:variable>
    <xsl:apply-templates select="$temp" mode="#current">
      <xsl:with-param name="after-head" as="item()*" tunnel="yes">
        <span class="stream">
          <xsl:apply-templates select="@s:*" mode="#current"/>
        </span>
      </xsl:with-param>
      <xsl:with-param name="role" as="attribute()*" tunnel="yes">
        <xsl:if test="@s:role">
          <xsl:attribute name="role" select="replace(@s:role,'\.\d+$','')"/>
        </xsl:if>
      </xsl:with-param>
    </xsl:apply-templates>
  </xsl:template>
  <xsl:template match="xsl:stream[@s:*]|xsl:template[@s:*]" mode="serialize" priority="1.5">
    <xsl:variable name="temp" as="element()">
      <xsl:copy>
        <xsl:sequence select="(@* except @s:*),*|text()"/>
      </xsl:copy>
    </xsl:variable>
    <span class="topLevelXSL">
      <xsl:apply-templates select="$temp" mode="#current">
        <xsl:with-param name="streamable" as="xs:boolean" select="(@s:streamable,false())[1]"/>
        <xsl:with-param name="after-head" as="item()*" tunnel="yes">
          <span class="stream">
            <xsl:apply-templates select="@s:* except @s:streamable" mode="#current"/>
          </span>
        </xsl:with-param>
      </xsl:apply-templates>
    </span>
  </xsl:template>



  <xsl:template match="s:sequence-constructor" mode="serialize">
    <xsl:param name="expander" as="element()*" select="$expander" tunnel="yes"/>
    <xsl:apply-imports>
      <xsl:with-param name="element.class">XSLsequence</xsl:with-param>
      <xsl:with-param name="body.class">XSLbody</xsl:with-param>
      <xsl:with-param name="name" select="local-name(.)"/>
      <xsl:with-param name="folder" as="element()?"
        select="if(* except xp:*) then $expander else $blank"/>
    </xsl:apply-imports>
  </xsl:template>

  <xsl:template match="xsl:stream|xsl:template" mode="serialize">
    <xsl:param name="streamable" as="xs:boolean" select="false()"/>
    <xsl:param name="expander" as="element()*" select="$expander" tunnel="yes"/>
    <xsl:apply-imports>
      <xsl:with-param name="element.class"
        select="if($streamable) then 'xslstreamyes' else 'xslstreamno'"/>
      <xsl:with-param name="body.class">XSLbody</xsl:with-param>
      <xsl:with-param name="folder" as="element()?"
        select="if(* except xp:*) then $expander else $blank"/>
    </xsl:apply-imports>
  </xsl:template>
  <xsl:template match="xsl:*" mode="serialize">
    <xsl:param name="expander" as="element()*" select="$expander" tunnel="yes"/>
    <span>
      <xsl:apply-imports>
        <xsl:with-param name="element.class">XSLelem</xsl:with-param>
        <xsl:with-param name="body.class">XSLbody</xsl:with-param>
        <xsl:with-param name="folder" as="element()?"
          select="if(* except xp:*) then $expander else $blank"/>
      </xsl:apply-imports>
    </span>
  </xsl:template>


  <xsl:template match="*" mode="serialize">
    <xsl:param name="expander" as="element()*" select="$expander" tunnel="yes"/>
    <xsl:apply-imports>
      <xsl:with-param name="folder" as="element()?"
        select="if(* except xp:*) then $expander else $blank"/>
    </xsl:apply-imports>
  </xsl:template>
  <xsl:template match="xp:*" mode="serialize">
    <xsl:param name="surround.top" select="true()" tunnel="yes"/>
    <xsl:param name="role" as="attribute()*" tunnel="yes"/>
    <xsl:variable name="temp" as="item()*">
      <xsl:apply-imports>
        <xsl:with-param name="element.class">XPelem</xsl:with-param>
        <xsl:with-param name="name" select="local-name(.)"/>
        <xsl:with-param name="truncate" select="200" tunnel="yes"/>
        <xsl:with-param name="surround.top" as="xs:boolean" select="false()" tunnel="yes"/>
      </xsl:apply-imports>
    </xsl:variable>
    <xsl:choose>
      <xsl:when test="$surround.top">
        <span class="XPtop">
          <xsl:sequence select="$role"/>
          <xsl:copy-of select="$temp"/>
        </span>
      </xsl:when>
      <xsl:otherwise>
        <xsl:copy-of select="$temp"/>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <xsl:template match="xp:raw" mode="serialize">
    <span class="XPathRAW">
      <xsl:next-match>
        <xsl:with-param name="indent" select="''" tunnel="yes"/>
      </xsl:next-match>
    </span>
  </xsl:template>

  <xsl:template
    match="@s:staticType | @s:role | @s:usage | @s:usageOriginal 
        | @s:focus | @s:contextItemType 
        | @s:sweep | @s:sweepOriginal
        | @s:posture | @s:contextPosture
        | @s:potentialConsumer | @s:choiceGroup | @s:higherOrder"
    mode="serialize" expand-text="yes">
    <span class="streamProperty {local-name(.)}" type="{local-name(.)}">
      <xsl:text> </xsl:text>
      <span class="propName">{xp:abbreviate(local-name(.))}:</span>{xp:abbreviate(.)}</span>
  </xsl:template>

  <xsl:template match="@s:general | @s:generalOperand" mode="serialize" expand-text="yes">
    <xsl:param name="spec" as="xs:string" tunnel="yes"/>
    <xsl:param name="spec.container" as="xs:string" select="$spec.container" tunnel="yes"/>    
    <span class="streamProperty {local-name(.)}" type="{local-name(.)}">
      <xsl:text> </xsl:text>
      <a href="{$spec}#gsr-{tokenize(.,',')[1]}" target="{$spec.container}">
        <span class="propName">{xp:abbreviate(local-name(.))}:</span>{xp:abbreviate(.)}</a>
    </span>
  </xsl:template>


  <!-- <xsl:variable name="prop-names" as="map(xs:string,xs:string)"
    select="map{'staticType':='t','role':='r', 'usage':='u', 'usageOriginal':='uO','focus':='f',
        'contextItemType':='ct','sweep':='s','sweepOriginal' := 'sO', 'posture':='p','contextPosture':='cp',
        'general':='rule', 'generalOperand' := 'rule.op','potentialConsumer':='PC'}"/>-->

  <xsl:variable name="abbreviateXX" as="map(xs:string,xs:string)"
    select="map:new(($prop-names,xp:invert($usageAbbrev),xp:invert($postureAbbrev),xp:invert($sweepAbbrev)))"/>
  <xsl:variable name="abbreviate" as="map(xs:string,xs:string)" select="$prop-names"/>
  <xsl:function name="xp:abbreviate" as="xs:string">
    <xsl:param name="val" as="xs:string"/>
    <xsl:value-of select="($abbreviate($val),$val)[1]"/>
  </xsl:function>

  <xsl:template match="@s:focus-changing" mode="serialize" expand-text="yes">
    <span type="focus" class="streamProperty focus">
      <xsl:text> </xsl:text>
      <span class="small">{xp:abbreviate('focus')}:</span>
      <xsl:text>change</xsl:text>
      <!--<xsl:next-match/>-->
    </span>
  </xsl:template>
  <xsl:template match="@s:focus-id" mode="serialize" expand-text="yes">
    <span class="focus">
      <xsl:text> </xsl:text>
      <span class="small">focusID:</span>{.}</span>
  </xsl:template>
  <xsl:template match="@s:focus-setter" mode="serialize" expand-text="yes">
    <span class="focus">
      <xsl:text> </xsl:text>
      <span class="small">focusSET:</span>{.}</span>
  </xsl:template>

  <xsl:template match="~map(*)" mode="serialize">
    <xsl:param name="show-empty-entries" select="false()" as="xs:boolean" tunnel="yes"/>
    <xsl:param name="indent" select="''" tunnel="yes"/>
    <xsl:param name="indent.by" select="'  '" tunnel="yes"/>
    <xsl:variable name="map" select="."/>
    <span class="MAP">{ <xsl:for-each select="map:keys(.)">
        <xsl:sort select="."/>
        <xsl:variable name="val" select="$map(.)"/>
        <xsl:variable name="is-empty"
          select="empty($val) or ($val instance of map(*) and empty(map:keys($val)))"/>
        <xsl:if test="$show-empty-entries or not($is-empty)">
          <xsl:value-of select="$indent"/><span class="key"><xsl:value-of select="."/>:=</span>
          <xsl:apply-templates select="$val" mode="#current">
            <xsl:with-param name="indent" select="concat($indent,$indent.by)" tunnel="yes"/>
          </xsl:apply-templates>
          <xsl:if test="position() ne last()">,</xsl:if><xsl:text>&#xA;</xsl:text>
        </xsl:if>
      </xsl:for-each>} </span>
  </xsl:template>
</xsl:stylesheet>
