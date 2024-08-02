<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
  xmlns:xs="http://www.w3.org/2001/XMLSchema"
  xmlns:math="http://www.w3.org/2005/xpath-functions/math"
  xmlns:map="http://www.w3.org/2005/xpath-functions/map" xmlns:xp="http://saxonica.com/xpathParse"
  xmlns:fos="http://www.w3.org/xpath-functions/spec/namespace"
  xmlns:e="http://www.w3.org/1999/XSL/Spec/ElementSyntax" xmlns:f="myfunctions"
  xmlns:doc="http://www.saxonica.com/ns/documentation" xmlns:dc="http://purl.org/dc/elements/1.1/"
  xmlns:X="IndirectXSLT" xmlns:s="StreamAnalysis" xmlns:H="http://www.w3.org/1999/xhtml"
  xmlns:ST="http://www.w3.org/2005/05/xslt20-test-catalog"
  xmlns:TC="http://www.w3.org/2012/10/xslt-test-catalog" xmlns:file="http://expath.org/ns/file"
  exclude-result-prefixes="xs math fos" version="3.0">

  <xsl:param name="show.raw" select="false()"/>
  <xsl:variable name="baseURI" select="base-uri(/)"/>
  <xsl:param name="outURI" select="resolve-uri('out/',$baseURI)"/>


  <xsl:namespace-alias stylesheet-prefix="X" result-prefix="xsl"/>
  <xsl:include href="analyze/stream-serialize.xsl"/>
  <xsl:include href="analyze/streamability.xsl"/>

  <xsl:variable name="spec.name"
    doc:doc="The XLST3.0 spec URL relative to the static URI (this stylesheet)"
    >specs/WD-xslt-30-20131212.html</xsl:variable>
  <xsl:variable name="spec" select="doc($spec.name)"/>
  <xsl:variable name="spec.ref" select="$spec//H:h2[H:a/@id='w3c-doctype']"/>
  <xsl:variable name="spec.out"
    doc:doc="The name to use for the XSLT3.0 spec, relative to the output URI"
    >XSLT.3.0.Spec.xhtml</xsl:variable>
  <xsl:variable name="GSR" select="$spec//H:div[H:h4/H:a/@id='general-streamability-rules']"/>
  <xsl:output method="xml" indent="yes"/>
  <xsl:output method="xml" indent="yes" name="xml"/>
  <xsl:output method="html" indent="no" name="xhtml"/>
  <xsl:output method="xhtml" indent="no" name="s:xhtml"/>


  <xsl:variable name="folders"
    select="for $h in samples/tests-from-folders/@href 
    return file:list(resolve-uri($h),true(),'*test-set.xml')!
    resolve-uri(replace(.,'\\','/'),resolve-uri($h))"/>
  <xsl:variable name="tests.functions"
    select="distinct-values(for $f in $folders 
    return doc($f)//TC:stylesheet[not(@role='secondary')]/@file/resolve-uri(.,$f))"/>


  <xsl:variable name="stream.tests" as="map(xs:anyURI,item()+)">
    <xsl:map>
      <xsl:apply-templates select="samples/*" mode="collect-example"/>
    </xsl:map>
  </xsl:variable>
  <xsl:variable name="stream.entry-named-template" as="map(xs:anyURI,xs:string?)">
    <xsl:map>
      <xsl:for-each select="samples/tests-from/@href">
        <xsl:variable name="uri" select="resolve-uri(.)"/>
        <xsl:variable name="catalog" select="doc($uri)/ST:testcases"/>
        <xsl:variable name="tests-uri" select="resolve-uri($catalog/@InputOffsetPath,$uri)"/>
        <xsl:for-each select="$catalog/testcase[category='streaming']"
          xpath-default-namespace="http://www.w3.org/2005/05/xslt20-test-catalog">
          <xsl:map-entry key="resolve-uri(input/stylesheet/@file,$tests-uri)">
            <xsl:sequence select="input/entry-named-template/@qname/string(.)"/>
          </xsl:map-entry>
        </xsl:for-each>
      </xsl:for-each>
    </xsl:map>
  </xsl:variable>
  <xsl:variable name="error-tests" as="xs:anyURI*">
    <xsl:sequence select="map:keys($stream.tests)[contains(.,'-err')]"/>
    <xsl:sequence select="map:keys($stream.tests)[$stream.tests(.)/self::TC:error]"/>
  </xsl:variable>


  <xsl:include href="publish/content/collect-example.xsl"/>



  <xsl:function name="s:name-for" as="xs:string">
    <xsl:param name="in" as="xs:string"/>
    <xsl:value-of select="replace(tokenize($in,'/')[last()],'\.[a-z]+$','')"/>
  </xsl:function>
  <xsl:template match="/">

    <!--<xsl:message select="$folders"/>-->
    <xsl:for-each select="$tests.functions" use-when="false()">
      <xsl:message select="."/>
    </xsl:for-each>

    <xsl:result-document href="{$spec.out}" method="xhtml">
      <xsl:sequence select="$spec"/>
    </xsl:result-document>
    <!-- <xsl:result-document href="GSR.xml" method="xhtml">
      <xsl:sequence select="$GSR"/>
    </xsl:result-document>-->
    <results>
      <config xsl:expand-text="yes">
        <DateTime>{current-dateTime()}</DateTime>
        <xsl:for-each select="'version','vendor','product-name','product-version','is-schema-aware'">
          <xsl:element name="{.}">{system-property('xsl:'||.)}</xsl:element>
        </xsl:for-each>
      </config>
      <defaultDot>
        <xsl:value-of select="$default-dot-functions"/>
      </defaultDot>
      <spec-ref.xslt>
        <xsl:call-template name="map:print">
          <xsl:with-param name="items" select="$spec-ref.xslt"/>
        </xsl:call-template>
      </spec-ref.xslt>
      <spec-ref.expr>
        <xsl:call-template name="map:print">
          <xsl:with-param name="items" select="$spec-ref"/>
        </xsl:call-template>
      </spec-ref.expr>
      <wider>
        <xsl:call-template name="map:print">
          <xsl:with-param name="items" select="$wider"/>
        </xsl:call-template>
      </wider>
      <functions xsl:expand-text="yes">
        <xsl:for-each select="map:keys($functions)">
          <xsl:sort select="."/>
          <function name="{.}" returnType="{$functions(.)('returnType')}">
            <xsl:variable name="args" select="$functions(.)('args')"/>
            <xsl:for-each select="map:keys($args)">
              <xsl:sort select="."/>
              <xsl:copy-of select="$args(.)"/>
            </xsl:for-each>
          </function>
        </xsl:for-each>
      </functions>
      <instructions xsl:expand-text="yes">
        <xsl:for-each select="map:keys($instructions)">
          <xsl:sort select="."/>
          <instruction name="{.}">
            <xsl:sequence select="$instructions(.)"/>
          </instruction>
        </xsl:for-each>
      </instructions>
      <usagePatterns>
        <xsl:call-template name="map:print">
          <xsl:with-param name="items" select="$usagePatterns"/>
        </xsl:call-template>
      </usagePatterns>
      <least-common-supertype>
        <xsl:call-template name="map:print">
          <xsl:with-param name="items" select="$least-common-supertypes"/>
        </xsl:call-template>
      </least-common-supertype>
      <focus-changing>
        <xsl:call-template name="map:print">
          <xsl:with-param name="items" select="$focus-changing"/>
        </xsl:call-template>
      </focus-changing>
      <postures>
        <xsl:call-template name="map:print">
          <xsl:with-param name="items" select="$postures"/>
        </xsl:call-template>
      </postures>

      <!--      <xsl:for-each select="map:keys($stream.tests)">
        <xsl:sort select="."/>
        <!-\-<xsl:message select=".,':',$stream.tests(.)"/>-\->
        <xsl:message select="."/>
      </xsl:for-each>-->
      <!-- <xsl:message select="$error-tests!tokenize(.,'/')[last()]"/>-->
      <xsl:variable name="results" as="map(xs:anyURI,map(*))">
        <xsl:map>
          <xsl:for-each select="map:keys($stream.tests)">
            <xsl:sort select="."/>
            <xsl:variable name="file" select="tokenize(.,'/')[last()]"/>
            <xsl:map-entry key=".">
              <xsl:map>
                <xsl:map-entry key="'title'">
                  <xsl:value-of select="tokenize(.,'/')[last()]"/>
                </xsl:map-entry>
                <xsl:map-entry key="'parse'">
                  <xsl:for-each select="$stream.tests(.[1])">
                    <xsl:call-template name="analyze-streamability">
                      <xsl:with-param name="show.raw" select="$show.raw"/>
                      <xsl:with-param name="debug" select="'file:'||$file" tunnel="yes"/>
                    </xsl:call-template>
                  </xsl:for-each>
                </xsl:map-entry>
              </xsl:map>
            </xsl:map-entry>
          </xsl:for-each>
        </xsl:map>
      </xsl:variable>
      <xsl:for-each select="map:keys($results)">
        <!--<example uri="{.}">
          <xsl:sequence select="$results(.)('parse')"/>
        </example>-->
        <xsl:variable name="name" select="s:name-for(.)"/>
        <xsl:result-document href="annotated/{$name}.xsl" method="xml">
          <xsl:sequence select="$results(.)('parse')"/>
        </xsl:result-document>
        <xsl:result-document href="display/{$name}.html" format="s:xhtml">
          <html xsl:expand-text="yes">
            <head xsl:expand-text="no">
              <title>
                <xsl:value-of select="$name"/>
              </title>
              <meta name="description" content=""/>
              <meta name="keywords" content=""/>
              <meta name="coverage" content="Worldwide"/>
              <meta name="copyright" content="Copyright Saxonica Limited"/>
              <link rel="stylesheet" href="../style/saxondocs.css" type="text/css"/>
              <link rel="stylesheet" href="../style/stream-analyze.css" type="text/css"/>
            </head>
            <body>
              <pre><xsl:apply-templates select="$results(.)('parse')" mode="serialize">
                                <xsl:with-param name="truncate.attributes" select="100" tunnel="yes"/>
                                                      <xsl:with-param name="truncate" select="100" tunnel="yes"/>
                                                      <xsl:with-param name="spec" select="$spec.out" tunnel="yes"/>
                                                      <xsl:with-param name="normalize.attributes" select="true()" tunnel="yes"/>
                            </xsl:apply-templates></pre>
            </body>
          </html>
        </xsl:result-document>
      </xsl:for-each>
      <xsl:result-document href="{replace(tokenize(base-uri(.),'/')[last()],'\.xml$','.html')}"
        format="xhtml">
        <xsl:variable name="cases" as="xs:anyURI*">
          <xsl:perform-sort select="map:keys($results)">
            <xsl:sort select="."/>
          </xsl:perform-sort>
        </xsl:variable>
        <html xsl:expand-text="yes">
          <head xsl:expand-text="no">
            <title>Saxonica Community: W3C XSLT 3.0 Streaming analysis tool</title>
            <meta name="description" content=""/>
            <meta name="keywords" content=""/>
            <meta name="coverage" content="Worldwide"/>
            <meta name="copyright" content="Copyright Saxonica Limited"/>
            <meta name="title" content="Saxonica: XSLT and XQuery Processing"/>
            <link rel="stylesheet" href="style/saxondocs.css" type="text/css"/>
            <link rel="stylesheet" href="style/stream-analyze.css" type="text/css"/>
            <script language="javascript">
                            function resize_background(){
                            var docHeight = Math.max(body.scrollHeight,
                            body.clientHeight, screen.zero(self.innerHeight));
                            document.getElementById('lhLightArea').style.height=docHeight;                            
                            document.getElementById('lhDkBlueArea').style.height=docHeight;                            
                            document.getElementById('rhDkBlueArea').style.height=docHeight;
                            }
                        </script>
          </head>
          <!--<body onload="resize_background()" class="main">-->
          <body class="main">
            <script type="text/javascript" language="javascript" src="Saxonce/Saxonce.nocache.js"/>
            <script type="application/xslt+xml" language="xslt2.0" src="style/show-stream.xsl" data-initial-template="go"/>
            <div id="rhDkBlueArea"
              style="position:fixed; width:123px;         height:2000px; z-index:1; right: 0px; top: 0px;  border: 1px none         #000000; background-color: #C1CEDE; layer-background-color: #C1CEDE;         visibility: visible;"/>
            <div id="lhLightArea"
              style="position:fixed; width:34px;         height:2000px; z-index:1; left: 66px; top: 0px; border: 1px none         #000000; background-color: #f6fffb; layer-background-color: #f6fffb;         visibility: visible;"/>
            <div id="lhDkBlueArea"
              style="position:fixed; width:66px;         height:2000px; z-index:1; left: 0px; top: 0px; border: 1px none #000000;         background-color: #C1CEDE; layer-background-color: #C1CEDE; visibility:         visible;"/>
            <div id="LogoArea"
              style="position:absolute; width:340px; height:72px; z-index:3; right: 0px; top: 0px; border: 1px none #000000; visibility: visible;">
              <a href="https://www.saxonica.com/">
                <img src="style/img/saxonica_logo.gif" width="340" height="72" border="0"
                  alt="saxonica.com"/>
              </a>
            </div>
            <div id="MainTextArea"
              style="position:absolute; height:100%; z-index:5; left: 130px; right: 260px; top: 110px; border: 1px none #000000; background-color: #E4EEF0; visibility: visible;">
              <p id="loading">Please wait - loading CE</p>
              <h2>Streamability Analysis</h2>
              <p class="config">Date:<span class="value">{current-dateTime()}</span> Saxon:<span
                  class="value">{system-property('xsl:product-version')}</span> Reference:<span
                  class="value">{$spec.ref}</span></p>
              <table id="mainTable">
                <tbody>
                  <tr>
                    <td class="VALIGN">
                      <xsl:apply-templates select="$stream-controls" mode="make-controls"/>
                      <!-- <xsl:if test="$show.raw">
                        <div><input id="show.raw" type="checkbox" name="raw" value="raw"/>Raw
                          XPath</div>
                      </xsl:if>-->
                    </td>
                    <td class="VALIGN">
                      <xsl:for-each-group select="$cases"
                        group-by="if(matches(.,'/tests/'))
                        then replace(.,'(tests/[^/]*/).*$','$1') else replace(.,'/[^/]+$','')">
                        <div>
                          <p>Location: {current-grouping-key()}</p>
                          <div>
                            <select class="genSelector">
                              <xsl:for-each select="current-group()">
                                <!--<option value="{position()}">{$results(.)('title')}</option>-->
                                <option
                                  value="{resolve-uri('display/'||s:name-for(.)||'.html',$outURI)}"
                                  >{$results(.)('title')}</option>
                              </xsl:for-each>
                            </select>({count(current-group())} tests) <xsl:variable
                              name="problems.negative"
                              select="current-group()[not(.=$error-tests)][$results(.)('parse')//@s:streamable='false']!s:name-for(.)"/>
                            <div>{count($problems.negative)} false negatives:
                              {$problems.negative}</div>
                            <xsl:variable name="problems.positive"
                              select="current-group()[.=$error-tests][empty($results(.)('parse')//@s:streamable[.='false'])]!s:name-for(.)"/>
                            <div>{count($problems.positive)} false positives:
                              {$problems.positive}</div>
                            <xsl:for-each select="current-group()" use-when="false()">
                              <div class="Case">
                                <p>{substring-after(.,current-grouping-key())}</p>
                                <pre><xsl:apply-templates select="$results(.)('parse')" mode="serialize">
                                <xsl:with-param name="truncate.attributes" select="100" tunnel="yes"/>
                                                      <xsl:with-param name="truncate" select="100" tunnel="yes"/>
                                                      <xsl:with-param name="spec" select="$spec.out" tunnel="yes"/>
                                                      <xsl:with-param name="normalize.attributes" select="true()" tunnel="yes"/>
                            </xsl:apply-templates></pre>
                              </div>
                            </xsl:for-each>
                          </div>
                        </div>
                      </xsl:for-each-group>
                      <div id="contain" class="Contain">
                        <pre/>
                      </div>
                    </td>
                    <td>
                      <div class="tabset">
                        <input type="radio" name="spec" value="XSLT" checked="true">XSLT
                          Spec.</input>
                        <input type="radio" name="spec" value="GSR">General Streamability</input>
                      </div>
                      <table>
                        <tbody>
                          <tr>
                            <td class="VALIGN" name="XSLT">
                              <div id="spec">
                                <iframe name="spec" src="{$spec.out}"/>
                              </div>
                            </td>
                            <td class="VALIGN" name="GSR">
                              <div id="gsr">
                                <xsl:apply-templates select="$GSR" mode="id-GSR"/>
                              </div>
                            </td>
                          </tr>
                        </tbody>
                      </table>
                    </td>
                     <xsl:if test="$show.raw">
                      <td class="VALIGN">
                        <pre id="raw"/>
                      </td>
                    </xsl:if>
                  </tr>
                </tbody>
              </table>
            </div>
          </body>
        </html>
      </xsl:result-document>
    </results>
  </xsl:template>



  <xsl:mode name="id-GSR" on-no-match="shallow-copy"/>
  <xsl:template match="H:li" mode="id-GSR">
    <xsl:copy>
      <xsl:sequence select="@*"/>
      <xsl:attribute name="index">
        <xsl:number level="multiple" format="1.a.i.A.I"/>
      </xsl:attribute>
      <xsl:apply-templates select="*|text()" mode="#current"/>
    </xsl:copy>
  </xsl:template>
  <xsl:template match="H:li//H:td" mode="id-GSR">
    <xsl:copy>
      <xsl:sequence select="@*"/>
      <xsl:attribute name="index">
        <xsl:number count="H:li|H:tr|H:td" level="multiple" format="1.a.i.A.1.1"/>
      </xsl:attribute>
      <xsl:apply-templates select="*|text()" mode="#current"/>
    </xsl:copy>
  </xsl:template>

  <xsl:include href="map-print.xsl"/>
</xsl:stylesheet>
