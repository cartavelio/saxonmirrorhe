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


  <xsl:include href="postureSweep-expression.xsl"/>
  <xsl:include href="postureSweep-xslt.xsl"/>

  <xsl:variable name="postureAbbrev" as="map(xs:string,xs:string)"
    select="map{'G':='grounded','Cr':='crawling','Cl':='climbing','S':='striding', 'R' := 'roaming'}"/>
  <xsl:variable name="sweepAbbrev" as="map(xs:string,xs:string)"
    select="map{'M':='motionless','Co':='consuming','FR':='free-ranging','Call':='caller-determined'}"/>

  <xsl:variable name="sweep-values" select="'motionless','consuming','free-ranging'"/>
  <xsl:variable name="wider" as="map(*)" use-when="false()"
    select="
    map:new($sweep-values !
            (let $pos:= position(), $name := .
            return map:entry($name,
                             map:new($sweep-values!map:entry(.,if(position() gt $pos) then . else $name))
                             ))
           )"/>
  
  <xsl:variable name="wider" as="map(*)" use-when="true()">
    <xsl:map>
      <xsl:for-each select="$sweep-values">
        <xsl:variable name="pos" select="position()"/>
        <xsl:variable name="name" select="."/>
        <xsl:map-entry key=".">
          <xsl:map>
            <xsl:for-each select="$sweep-values">
              <xsl:map-entry key=".">
                <xsl:value-of select="if(position() gt $pos) then . else $name"/>
              </xsl:map-entry>
            </xsl:for-each>
          </xsl:map>
        </xsl:map-entry>
      </xsl:for-each>
    </xsl:map>
  </xsl:variable>

  <xsl:function name="xp:wider" as="xs:string?">
    <xsl:param name="sweeps" as="xs:string*"/>
    <xsl:sequence select="fold-left($sweeps,'motionless',function($a,$b){$wider($a)($b)})"/>
  </xsl:function>

  <xsl:function name="xp:combined-posture" as="xs:string">
    <xsl:param name="postures" as="xs:string*"/>
    <xsl:sequence
      select="if($postures = 'roaming') then 'roaming' else
            if(every $p in $postures satisfies $p = 'grounded') then 'grounded' else
            if($postures = 'climbing' and (every $p in $postures[not(. = 'climbing')] satisfies $p = 'grounded')) then 'climbing' else
            if($postures = 'striding' and (every $p in $postures[not(. = 'striding')] satisfies $p = 'grounded')) then 'striding' else
            if($postures = 'crawling' and (every $p in $postures[not(. = 'crawling')] satisfies $p = ('striding','grounded'))) then 'climbing' else
            'roaming'"
    />
  </xsl:function>

  <xsl:variable name="childless-nodes"
    select="'text()','attribute()','comment()','processing-instruction()','namespace-node()'"/>


  <xsl:function name="xp:not-motionless" as="xs:boolean">
    <xsl:param name="item" as="element()?"/>
    <xsl:sequence select="$item/@s:sweep=('consuming','free-ranging')"/>
  </xsl:function>

  <!-- Using these variables rather than strings descreases chances of spelling errors -->
  <xsl:variable name="p.grounded" as="attribute()" doc:doc="Grounded posture">
    <xsl:attribute name="s:posture">grounded</xsl:attribute>
  </xsl:variable>
  <xsl:variable name="p.crawling" as="attribute()" doc:doc="Crawling posture">
    <xsl:attribute name="s:posture">crawling</xsl:attribute>
  </xsl:variable>
  <xsl:variable name="p.climbing" as="attribute()" doc:doc="Climbing posture">
    <xsl:attribute name="s:posture">climbing</xsl:attribute>
  </xsl:variable>
  <xsl:variable name="p.striding" as="attribute()" doc:doc="Striding posture">
    <xsl:attribute name="s:posture">striding</xsl:attribute>
  </xsl:variable>
  <xsl:variable name="p.roaming" as="attribute()" doc:doc="Roaming posture">
    <xsl:attribute name="s:posture">roaming</xsl:attribute>
  </xsl:variable>

  <xsl:variable name="s.motionless" as="attribute()" doc:doc="Motionless sweep">
    <xsl:attribute name="s:sweep">motionless</xsl:attribute>
  </xsl:variable>
  <xsl:variable name="s.consuming" as="attribute()" doc:doc="Consuming sweep">
    <xsl:attribute name="s:sweep">consuming</xsl:attribute>
  </xsl:variable>
  <xsl:variable name="s.free-ranging" as="attribute()" doc:doc="Free-ranging sweep">
    <xsl:attribute name="s:sweep">free-ranging</xsl:attribute>
  </xsl:variable>

  <xsl:variable name="RFR" as="attribute()*" select="$p.roaming,$s.free-ranging"
    doc:doc="Roaming and free-ranging, i.e. lost"/>


  <xsl:mode name="addPosture" on-no-match="shallow-copy"/>
  <xsl:template match="*" mode="addPosture">
    <xsl:param name="contextPosture" as="xs:string?" tunnel="yes"/>
    <xsl:copy>
      <xsl:apply-templates select="@*" mode="#current"/>
      <xsl:variable name="posture" select="$postures(local-name())"/>
      <xsl:call-template name="record-contextPosture"/>
      <xsl:call-template name="stream-rule"/>
      <xsl:attribute name="s:posture" select="$posture" on-empty="()"/>
      <xsl:apply-templates select="*|text()" mode="#current">
        <xsl:with-param name="contextPosture"
          select="($context-postures(local-name()),$contextPosture)[1]" tunnel="yes"/>
      </xsl:apply-templates>
    </xsl:copy>
  </xsl:template>

  <xsl:template name="record-contextPosture" as="attribute()?">
    <xsl:param name="contextPosture" as="xs:string?" tunnel="yes"/>
    <xsl:attribute name="s:contextPosture" select="$contextPosture" on-empty="()"/>
  </xsl:template>


  <!-- Hyperlinks to the spec rules -->
  <xsl:template name="stream-rule-xsl" as="attribute()?">
    <xsl:variable name="spec" select="$spec-ref.xslt(local-name(.))"/>
    <xsl:attribute name="s:href" select="$spec" on-empty="()"/>
  </xsl:template>
  <xsl:template name="stream-rule-expr" as="attribute()?">
    <xsl:variable name="spec"
      select="if(self::xp:FunctionCall and ($fn-spec-ref(@name) or $fn-builtin-ref(@name))) then ($fn-spec-ref(@name),$fn-builtin-ref(@name))[1] else $spec-ref(local-name())"/>
    <xsl:attribute name="s:href" select="$spec" on-empty="()"/>
  </xsl:template>
  <xsl:template name="stream-rule" as="attribute()?">
    <xsl:choose>
      <xsl:when test="self::xsl:*">
        <xsl:call-template name="stream-rule-xsl"/>
      </xsl:when>
      <xsl:when test="self::xp:*">
        <xsl:call-template name="stream-rule-expr"/>
      </xsl:when>
    </xsl:choose>
  </xsl:template>



  <xsl:template match="*[not(self::xsl:*|self::xp:*)]" mode="addPosture">
    <xsl:call-template name="general-streamability"/>
  </xsl:template>







  <xsl:template name="general-streamability">
    <xsl:param name="spec-rule" as="attribute()?" select="()"/>
    <xsl:copy>
      <xsl:apply-templates select="@*" mode="#current"/>
      <xsl:call-template name="record-contextPosture"/>
      <xsl:sequence select="$spec-rule"/>
      <xsl:call-template name="general-streamability.operands">
        <xsl:with-param name="children" as="item()*">
          <xsl:apply-templates select="*|text()" mode="addPosture"/>
        </xsl:with-param>
      </xsl:call-template>
    </xsl:copy>
  </xsl:template>



  <xsl:variable name="table.1.b.iii.B" as="map(*)"
    select="map{
        'grounded' := map{'absorption':='S',            'inspection':='S', 'transmission':='S', 'navigation':='S'},
        'climbing' := map{'absorption':='free-ranging', 'inspection':='S', 'transmission':='S', 'navigation':='free-ranging'},
        'striding' := map{'absorption':='consuming',    'inspection':='S', 'transmission':='S', 'navigation':='free-ranging'},
        'crawling' := map{'absorption':='free-ranging', 'inspection':='S', 'transmission':='S', 'navigation':='free-ranging'}
        }"/>
  <xsl:variable name="table.axes" as="map(*)"
    select="map{ 'grounded':=0,'climbing':=1, 'striding':=2, 'crawling' := 3,
        'absorption' := 1, 'inspection' := 2, 'transmission' := 3, 'navigation' := 4} "/>

  <xsl:template name="general-streamability.operands">
    <xsl:param name="children" as="item()*"/>
    <xsl:param name="debug" as="xs:string?" tunnel="yes"/>
    <xsl:variable name="swept" as="item()*">
      <xsl:for-each select="$children">
        <xsl:choose>
          <xsl:when test=". instance of text()">
            <xsl:sequence select="."/>
          </xsl:when>
          <xsl:otherwise>
            <xsl:variable name="T" select="@s:staticType"/>
            <xsl:variable name="P" select="@s:posture"/>
            <xsl:variable name="S" select="@s:sweep"/>
            <xsl:variable name="U" select="@s:usage"/>
            <xsl:copy>
              <xsl:sequence select="@*"/>
              <xsl:attribute name="s:sweepOriginal" select="@s:sweep" on-empty="()"/>
              <xsl:attribute name="s:usageOriginal" select="@s:usage" on-empty="()"/>
              <xsl:variable name="results" as="map(*)*">
                <xsl:choose>
                  <xsl:when test="$S = 'free-ranging' or $P = 'roaming'">
                    <xsl:sequence
                      select="map{'rule':='1.b.i',
                        'sweep':='free-ranging'}"
                    />
                  </xsl:when>
                  <xsl:when test="$P = 'grounded'">
                    <xsl:sequence
                      select="map{'rule':='1.b.ii',
                        'sweep':= @s:sweep}"/>
                  </xsl:when>
                  <xsl:otherwise>
                    <xsl:variable name="childless.absorption"
                      select="$U = 'absorption' and xp:is-childless-type($T)"/>
                    <xsl:variable name="Up"
                      select="if($childless.absorption) then 'inspection' else $U"/>
                    <xsl:sequence
                      select="map{'rule' := '1.b.iii.A.' || (if($childless.absorption) then 'I' else 'II'),
                      'usage':= if(empty($Up)) then 'NO USAGE DEFINED' else $Up}"/>
                    <xsl:if test="exists($Up) and exists($P)">
                      <xsl:try>
                        <xsl:sequence
                          select="map{'rule':=string-join(('1.b.iii.B',
                                            ($P,$Up)!$table.axes(.)!string(.)),'.'),
                                                'sweep':= (let $s := $table.1.b.iii.B($P)($Up) return (if($s eq 'S') then @s:sweep else $s))}"/>
                        <xsl:catch>
                          <xsl:message select="$debug,'Error in GS table -','$P='||$P,'$Up='||$Up"/>
                          <xsl:sequence select="map:entry('error','GStable')"/>
                        </xsl:catch>
                      </xsl:try>
                    </xsl:if>
                  </xsl:otherwise>
                </xsl:choose>
              </xsl:variable>
              <xsl:attribute name="ERROR" select="$results!.('error')" on-empty="()"/>
              <xsl:attribute name="s:sweep" select="$results!.('sweep')"/>
              <xsl:attribute name="s:usage" select="$results!.('usage')" on-empty="()"/>
              <xsl:attribute name="s:generalOperand" select="$results!.('rule')" separator=","/>
              <xsl:choose>
                <xsl:when test="$results!.('sweep')='consuming'">
                  <xsl:attribute name="s:generalOperand" select="$results!.('rule'),'1.c.i'"
                    separator=","/>
                  <xsl:attribute name="s:potentialConsumer"/>
                </xsl:when>
                <xsl:when test="@s:usage='transmission' and not(@s:posture = 'grounded')">
                  <xsl:attribute name="s:generalOperand" select="$results!.('rule'),'1.c.ii'"
                    separator=","/>
                  <xsl:attribute name="s:potentialConsumer"/>
                </xsl:when>
              </xsl:choose>
              <xsl:sequence select="*|text()"/>
            </xsl:copy>
          </xsl:otherwise>
        </xsl:choose>
      </xsl:for-each>
    </xsl:variable>
    <!--<xsl:variable name="potential.consumers"
            select="$swept[@s:sweep='consuming' 
        or (@s:usage='transmission' and not(@s:posture = 'grounded'))]"/>-->
    <xsl:variable name="potential.consumers" select="$swept[@s:potentialConsumer]"/>
    <xsl:variable name="results" as="map(*)">
      <xsl:choose>
        <xsl:when test="empty($swept)">
          <xsl:sequence
            select="map{'rule':='2.a',
              'posture':='grounded',
              'sweep':='motionless'}"
          />
        </xsl:when>
        <xsl:when test="$swept/@s:sweep='free-ranging'">
          <xsl:sequence
            select="map{'rule':='2.b',
              'posture':='roaming',
              'sweep':='free-ranging'}"
          />
        </xsl:when>
        <xsl:when test="count($potential.consumers) gt 1">
          <xsl:choose>
            <!-- Need to add 2.c.i choice-operand-group refinement -->
            <xsl:when test="every $o in $potential.consumers satisfies exists($o/@s:choice-group)">
              <xsl:sequence
                select="map{'rule':='2.b',
                  'posture':=xp:combined-posture($potential.consumers/@s:posture),
                  'sweep':=xp:wider($potential.consumers/@s:sweep)}"
              />
            </xsl:when>
            <xsl:otherwise>
              <xsl:sequence
                select="map{'rule':='2.c.ii',
                  'posture':='roaming',
                  'sweep':='free-ranging'}"
              />
            </xsl:otherwise>
          </xsl:choose>
        </xsl:when>
        <xsl:when test="count($potential.consumers) eq 1">
          <xsl:for-each select="$potential.consumers">
            <xsl:choose>
              <!-- Add higher-order operand 2.d.i -->
              <xsl:when test="@s:higher-order-op">
                <xsl:sequence
                  select="map{'rule':='2.d.i',
                    'posture':='roaming',
                    'sweep':='free-ranging'}"
                />
              </xsl:when>
              <xsl:when test="@s:usage=('absorption','inspection')">
                <xsl:sequence
                  select="map{'rule':='2.d.ii',
                    'posture':='grounded',
                    'sweep':='consuming'}"
                />
              </xsl:when>
              <xsl:when test="@s:posture='crawling' and self::xp:FunctionCall[@name='head']">
                <xsl:sequence
                  select="map{'rule':='2.d.iii',
                  'posture':='striding',
                  'sweep':= @s:sweep}"
                />
              </xsl:when>
              <xsl:otherwise>
                <xsl:sequence
                  select="map{'rule':='2.d.iv',
                    'posture':= @s:posture,
                    'sweep':= @s:sweep}"
                />
              </xsl:otherwise>
            </xsl:choose>
          </xsl:for-each>
        </xsl:when>
        <xsl:otherwise>
          <xsl:sequence
            select="map{'rule':='2.e',
              'posture':='grounded',
              'sweep':='motionless'}"
          />
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>
    <xsl:attribute name="s:posture" select="$results('posture')"/>
    <xsl:attribute name="s:sweep" select="$results('sweep')"/>
    <xsl:attribute name="s:general" select="$results('rule')"/>
    <xsl:copy-of select="$swept"/>
  </xsl:template>


  <xsl:mode name="GS-find-operands" on-no-match="deep-skip"/>
  <xsl:mode name="GS-higher-order-operands" on-no-match="deep-skip"/>

  <xsl:template name="general-streamability.operands.ids">
    <xsl:param name="contextPosture" as="xs:string?" tunnel="yes"/>
    <xsl:param name="operands" as="item()*">
      <xsl:apply-templates select="." mode="GS-find-operands"/>
    </xsl:param>
    <xsl:param name="context-postures" as="map(xs:string,item()*)" select="map:new(())"/>
    <xsl:param name="children" as="item()*">
      <xsl:for-each select="*|text()">
        <xsl:apply-templates select="." mode="addPosture">
          <xsl:with-param name="contextPosture" as="xs:string?"
            select="($context-postures(@id),$contextPosture)[1]" tunnel="yes"/>
        </xsl:apply-templates>
      </xsl:for-each>
    </xsl:param>
    <xsl:variable name="operands.id" select="$operands/@id"/>

    <!-- <xsl:message select="$operands"/>
    <xsl:message select="'KEYS:',for $k in map:keys($context-postures) return $k||':='||$context-postures($k)"/>-->
    <xsl:variable name="swept" as="item()*">
      <xsl:for-each select="$children/descendant-or-self::*[@id=$operands.id]">
        <xsl:variable name="T" select="@s:staticType"/>
        <xsl:variable name="P" select="@s:posture"/>
        <xsl:variable name="S" select="@s:sweep"/>
        <xsl:variable name="U" select="@s:usage"/>
        <xsl:copy>
          <xsl:sequence select="@*"/>
          <xsl:attribute name="s:sweepOriginal" select="@s:sweep" on-empty="()"/>
          <xsl:attribute name="s:usageOriginal" select="@s:usage" on-empty="()"/>
          <xsl:variable name="results" as="map(*)*">
            <xsl:sequence select="map{'posture':=$P, 'USAGE':=$U}"/>
            <xsl:choose>
              <xsl:when test="$S = 'free-ranging' or $P = 'roaming'">
                <xsl:sequence
                  select="map{'rule':='1.b.i',
                      'sweep':='free-ranging'}"/>
              </xsl:when>
              <xsl:when test="$P = 'grounded'">
                <xsl:sequence
                  select="map{'rule':='1.b.ii',
                      'sweep':= @s:sweep}"/>
              </xsl:when>
              <xsl:otherwise>
                <xsl:variable name="childless.absorption"
                  select="$U = 'absorption' and xp:is-childless-type($T)"/>
                <xsl:variable name="Up" select="if($childless.absorption) then 'inspection' else $U"/>
                <xsl:sequence
                  select="map{'rule' := '1.b.iii.A.' || (if($childless.absorption) then 'I' else 'II'),
                      'usage':= if(empty($Up)) then 'NO USAGE DEFINED' else $Up}"/>
                <xsl:if test="exists($Up) and exists($P)">
                  <xsl:sequence
                    select="map{'rule':=string-join(('1.b.iii.B',
                        ($P,$Up)!$table.axes(.)!string(.)),'.')}"/>
                  <xsl:if test="map:keys($table.1.b.iii.B) = $P">
                    <xsl:variable name="row" select="$table.1.b.iii.B($P)"/>
                    <xsl:if test="map:keys($row)=$Up">
                      <xsl:sequence
                        select="map{'sweep':= (let $s := $table.1.b.iii.B($P)($Up) return (if($s eq 'S') then @s:sweep else $s))}"
                      />
                    </xsl:if>
                  </xsl:if>
                </xsl:if>
              </xsl:otherwise>
            </xsl:choose>
          </xsl:variable>
          <!-- <xsl:attribute name="POSTURE" select="$results!.('posture')"/>
          <xsl:attribute name="USAGE" select="$results!.('USAGE')"/>-->
          <xsl:attribute name="s:sweep" select="$results!.('sweep')"/>
          <xsl:attribute name="s:usage" select="$results!.('usage')" on-empty="()"/>
          <xsl:attribute name="s:generalOperand" select="$results!.('rule')" separator=","/>
          <xsl:choose>
            <xsl:when test="$results!.('sweep')='consuming'">
              <xsl:attribute name="s:generalOperand" select="$results!.('rule'),'1.c.i'"
                separator=","/>
              <xsl:attribute name="s:potentialConsumer"/>
            </xsl:when>
            <xsl:when test="@s:usage='transmission' and not(@s:posture = 'grounded')">
              <xsl:attribute name="s:generalOperand" select="$results!.('rule'),'1.c.ii'"
                separator=","/>
              <xsl:attribute name="s:potentialConsumer"/>
            </xsl:when>
          </xsl:choose>
          <xsl:sequence select="*|text()"/>
        </xsl:copy>
      </xsl:for-each>
    </xsl:variable>
    <!--<xsl:variable name="potential.consumers"
            select="$swept[@s:sweep='consuming' 
        or (@s:usage='transmission' and not(@s:posture = 'grounded'))]"/>-->
    <xsl:variable name="potential.consumers" select="$swept[@s:potentialConsumer]"/>
    <xsl:variable name="results" as="map(*)">
      <xsl:choose>
        <xsl:when test="empty($swept)">
          <xsl:sequence
            select="map{'rule':='2.a',
            'posture':='grounded',
            'sweep':='motionless'}"
          />
        </xsl:when>
        <xsl:when test="$swept/@s:sweep='free-ranging'">
          <xsl:sequence
            select="map{'rule':='2.b',
            'posture':='roaming',
            'sweep':='free-ranging'}"
          />
        </xsl:when>
        <xsl:when test="count($potential.consumers) gt 1">
          <xsl:choose>
            <!-- Need to add 2.c.i choice-operand-group refinement -->
            <xsl:when test="every $o in $potential.consumers satisfies exists($o/@s:choiceGroup)">
              <xsl:sequence
                select="map{'rule':='2.b',
                'posture':=xp:combined-posture($potential.consumers/@s:posture),
                'sweep':=xp:wider($potential.consumers/@s:sweep)}"
              />
            </xsl:when>
            <xsl:otherwise>
              <xsl:sequence
                select="map{'rule':='2.c.ii',
                'posture':='roaming',
                'sweep':='free-ranging'}"
              />
            </xsl:otherwise>
          </xsl:choose>
        </xsl:when>
        <xsl:when test="count($potential.consumers) eq 1">
          <xsl:for-each select="$potential.consumers">
            <xsl:choose>
              <!-- Add higher-order operand 2.d.i -->
              <xsl:when test="@s:higherOrder">
                <xsl:sequence
                  select="map{'rule':='2.d.i',
                  'posture':='roaming',
                  'sweep':='free-ranging'}"
                />
              </xsl:when>
              <xsl:when test="@s:usage=('absorption','inspection')">
                <xsl:sequence
                  select="map{'rule':='2.d.ii',
                  'posture':='grounded',
                  'sweep':='consuming'}"
                />
              </xsl:when>
              <xsl:otherwise>
                <xsl:sequence
                  select="map{'rule':='2.d.iii',
                  'posture':= @s:posture,
                  'sweep':= @s:sweep}"
                />
              </xsl:otherwise>
            </xsl:choose>
          </xsl:for-each>
        </xsl:when>
        <xsl:otherwise>
          <xsl:sequence
            select="map{'rule':='2.e',
            'posture':='grounded',
            'sweep':='motionless'}"
          />
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>
    <xsl:attribute name="s:posture" select="$results('posture')"/>
    <xsl:attribute name="s:sweep" select="$results('sweep')"/>
    <xsl:attribute name="s:general" select="$results('rule')"/>
    <xsl:apply-templates select="$children" mode="modify-status">
      <xsl:with-param name="new-operands" select="$swept" tunnel="yes"/>
    </xsl:apply-templates>
    <!--<xsl:copy-of select="$swept"/>-->
  </xsl:template>

  <xsl:mode name="modify-status" on-no-match="shallow-copy"/>
  <xsl:template match="*" mode="modify-status">
    <xsl:param name="new-operands" as="element()*" tunnel="yes"/>
    <xsl:choose>
      <xsl:when test="@id=$new-operands/@id">
        <xsl:copy-of select="$new-operands[@id=current()/@id]"/>
      </xsl:when>
      <xsl:otherwise>
        <xsl:next-match/>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>


</xsl:stylesheet>
