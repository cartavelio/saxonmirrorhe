<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
  xmlns:xs="http://www.w3.org/2001/XMLSchema" xmlns:pkg="http://expath.org/ns/pkg"
  xmlns:web="http://expath.org/ns/webapp" xmlns:app="http://saxonica.com/tools/analyze-stream"
  xmlns:saxon="http://saxon.sf.net/" xmlns:doc="http://www.saxonica.com/ns/documentation"
  xmlns:H="http://www.w3.org/1999/xhtml" xmlns:TC="http://www.w3.org/2012/10/xslt-test-catalog"
  xmlns:file="http://expath.org/ns/file" xmlns:map="http://www.w3.org/2005/xpath-functions/map"
  xmlns:s="StreamAnalysis" exclude-result-prefixes="#all" version="3.0">

  <pkg:import-uri>http://saxonica.com/tools/analyze-stream/gen-analyze.xsl</pkg:import-uri>

  <xsl:include href="analyze/stream-controls.xsl"/>
  <xsl:include href="collect-example.xsl"/>

  <xsl:param name="spec.container">_spec</xsl:param>
  <xsl:param name="spec.iframe" as="xs:boolean" select="false()" static="yes"/>

  <xsl:output name="xml" method="xml" indent="yes"/>
  <!--<xsl:output method="html" omit-xml-declaration="yes"/>-->



  <xsl:variable name="examples" select="doc(resolve-uri('stream-examples.xml'))"/>
  <xsl:variable name="baseURI" select="base-uri($examples)"/>

  <xsl:variable name="spec.name"
    doc:doc="The XLST3.0 spec URL relative to the static URI (this stylesheet)"
    >http://www.w3.org/TR/xslt-30/</xsl:variable>
  <xsl:variable name="spec" select="doc($spec.name)"/>
  <xsl:variable name="spec.ref" select="$spec//H:h2[H:a/@id='w3c-doctype']"/>
  <xsl:variable name="spec.out"
    doc:doc="The name to use for the XSLT3.0 spec, relative to the output URI"
    >XSLT.3.0.Spec.xhtml</xsl:variable>
  <xsl:variable name="GSR" select="$spec//H:div[H:h4/H:a/@id='general-streamability-rules']"/>

  <xsl:variable name="stream.tests" as="map(xs:anyURI,item()*)">
    <xsl:map>
      <xsl:apply-templates select="$examples/samples/*" mode="collect-example">
        <xsl:with-param name="read-doc" select="false()" tunnel="yes"/>
      </xsl:apply-templates>
    </xsl:map>
  </xsl:variable>

  <xsl:template match="/">

    <web:response status="200" message="Ok">
      <!-- Depends on the Accept header actually, see
              http://www.w3.org/TR/xhtml-media-types/.  Good candidate
              for a tool function provided by Servlex itself. -->
      <web:body content-type="text/html" method="html"/>
    </web:response>

    <html xsl:expand-text="yes" xmlns="http://www.w3.org/1999/xhtml">
      <head>
        <title>Saxonica Community: W3C XSLT 3.0 Streaming analysis tool</title>
        <meta name="description" content=""/>
        <meta name="keywords" content=""/>
        <meta name="coverage" content="Worldwide"/>
        <meta name="copyright" content="Copyright Saxonica Limited"/>
        <link rel="stylesheet" href="style/saxondocs.css" type="text/css"/>
        <link rel="stylesheet" href="style/stream-analyze.css" type="text/css"/>
        <script language="javascript" xsl:expand-text="no">
          function resize_background() {
          var docHeight = Math.max(body.scrollHeight, body.clientHeight, screen
          .zero(self.innerHeight));
          document.getElementById('lhLightArea').style.height = docHeight;
          
          document.getElementById('lhDkBlueArea').style.height = docHeight;
          
          document.getElementById('rhDkBlueArea').style.height = docHeight;
          }
        </script>
      </head>
      <body onload="resize_background()" class="main">
        <!--<script type="text/javascript" language="javascript" src="Saxonce/Saxonce.nocache.js"/>-->
        <!--        <script type="application/xslt+xml" language="xslt2.0" src="style/show-stream.xsl" data-initial-template="go"/>
-->
        <div id="rhDkBlueArea"
          style="position: fixed; width: 123px; height: 2000px; z-index: 1; right: 0px; top: 0px; border: 1px none #000000; background-color: #C1CEDE; layer-background-color: #C1CEDE; visibility: visible;"/>
        <div id="lhLightArea"
          style="position: fixed; width: 34px; height: 2000px; z-index: 1; left: 66px; top: 0px; border: 1px none #000000; background-color: #f6fffb; layer-background-color: #f6fffb; visibility: visible;"/>

        <div id="lhDkBlueArea"
          style="position: fixed; width: 66px; height: 2000px; z-index: 1; left: 0px; top: 0px; border: 1px none #000000; background-color: #C1CEDE; layer-background-color: #C1CEDE; visibility: visible;"/>
        <div id="LogoArea"
          style="position: absolute; width: 340px; height: 72px; z-index: 3; right: 0px; top: 0px; border: 1px none #000000; visibility: visible;">
          <a href="https://www.saxonica.com/">
            <img src="images/saxonica_logo.gif" width="340" height="72" border="0"
              alt="saxonica.com"/>
          </a>
        </div>

        <div id="MainTextArea"
          style="position: absolute; height: 100%; z-index: 5; left: 130px; right: 260px; top: 110px; border: 1px none #000000; background-color: #E4EEF0; visibility: visible;">
          <h2>Saxonica Community: W3C XSLT 3.0 Streaming analysis tool</h2>
          <address>Generated @ {current-dateTime()}</address>
          <!--<p id="loading">Loading Saxon-CE - please wait</p>-->
          <a href="index.html">Home</a>
          <!--<p>{$baseURI}</p>-->
          <table>
            <tbody>
              <tr>
                <td>
                  <fieldset>
                    <legend>Upload your own stylesheet</legend>
                    <form action="analyzeStream" method="post" enctype="multipart/form-data"
                      target="analysis">
                      <span>File:</span>
                      <input type="file" name="file"/>
                      <br/>
                      <input type="submit" value="Analyze"/>
                    </form>
                  </fieldset>
                </td>
                <td>
                  <fieldset>
                    <legend>Examine a test or example</legend>
                    <xsl:variable name="samples" as="element()*">
                      <xsl:for-each-group select="map:keys($stream.tests)"
                        group-by="if(matches(.,'/strm/'))
            then replace(.,'(strm)/[^/]*/.*$','$1') else replace(.,'/[^/]+$','')">
                        <xsl:sort select="."/>
                        <div>
                          <p>{replace(current-grouping-key(),replace(string($baseURI),'[^/]+$',''),'')} ({count(current-group())})</p>
                          <form action="analyzeStreamExample" method="post"
                            enctype="multipart/form-data" target="analysis">
                            <select class="genSelector" name="which">
                              <xsl:for-each select="current-group()">
                                <xsl:sort select="."/>
                                <option value="{.}">{s:name-for(.)}</option>
                              </xsl:for-each>
                            </select><input type="submit"
                              value="Analyze"/>
                          </form>
                        </div>
                      </xsl:for-each-group>
                    </xsl:variable>
                    <table>
                      <tbody>
                        <xsl:for-each-group select="$samples" group-by="(position() - 1) idiv 2">
                          <tr>
                            <xsl:for-each select="current-group()">
                              <td>
                                <xsl:sequence select="."/>
                              </td>
                            </xsl:for-each>
                          </tr>
                        </xsl:for-each-group>
                      </tbody>
                    </table>
                  </fieldset>
                </td>
              </tr>
            </tbody>
          </table>
          <iframe id="analysis" name="analysis" xsl:use-when="not($spec.iframe)"/>
          <table id="mainTable" xsl:use-when="$spec.iframe">
            <tbody>
              <tr>
                <td class="VALIGN">
                  <iframe id="analysis" name="analysis"/>
                </td>
                <td class="VALIGN">
                  <div id="spec">
                    <iframe name="{$spec.container}" src="XSLT3.0-Spec"/>
                  </div>
                </td>
              </tr>
            </tbody>
          </table>
        </div>
      </body>
    </html>
  </xsl:template>

  <xsl:function name="s:name-for" as="xs:string">
    <xsl:param name="in" as="xs:string"/>
    <xsl:value-of select="replace(tokenize($in,'/')[last()],'\.[a-z]+$','')"/>
  </xsl:function>

</xsl:stylesheet>
