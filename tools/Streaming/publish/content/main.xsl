<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
  xmlns:xs="http://www.w3.org/2001/XMLSchema" xmlns:pkg="http://expath.org/ns/pkg"
  xmlns:web="http://expath.org/ns/webapp" xmlns:app="http://saxonica.com/tools/analyze-stream"
  xmlns:saxon="http://saxon.sf.net/" xmlns:doc="http://www.saxonica.com/ns/documentation"
  xmlns:H="http://www.w3.org/1999/xhtml" exclude-result-prefixes="#all" version="3.0">

  <xsl:include href="analyze/stream-serialize.xsl"/>
  <xsl:include href="analyze/streamability.xsl"/>

  <pkg:import-uri>http://saxonica.com/tools/analyze-stream/main.xsl</pkg:import-uri>

  <xsl:output name="xml" method="xml" indent="yes"/>
  <!--<xsl:output method="html" omit-xml-declaration="yes"/>-->

  <xsl:variable name="spec.name"
    doc:doc="The XLST3.0 spec URL relative to the static URI (this stylesheet)"
    >http://www.w3.org/TR/xslt-30/</xsl:variable>
  <xsl:variable name="spec" select="doc($spec.name)"/>
  <xsl:variable name="spec.ref" select="$spec//H:h2[H:a/@id='w3c-doctype']"/>
  <xsl:variable name="spec.out"
    doc:doc="The name to use for the XSLT3.0 spec, relative to the output URI"
    >XSLT3.0-Spec</xsl:variable>
  <xsl:variable name="GSR" select="$spec//H:div[H:h4/H:a/@id='general-streamability-rules']"/>


  <xsl:param name="web:input" as="item()+"/>


  <xsl:template match="/">
    <xsl:variable name="request" select="$web:input[1]" as="element(web:request)"/>
    <xsl:variable name="bodies" select="$web:input[2]" as="item()*"/>
    <xsl:variable name="cd"
      select="$request/web:multipart/web:header[@name eq 'content-disposition']/@value"/>
    <xsl:variable name="data" xmlns="">
      <xsl:variable name="r">(\i\c+)="([^"]*)"</xsl:variable>
      <xsl:analyze-string select="$cd" regex="{$r}">
        <xsl:matching-substring>
          <xsl:element name="{regex-group(1)}">
            <xsl:value-of select="regex-group(2)"/>
          </xsl:element>
        </xsl:matching-substring>
      </xsl:analyze-string>
    </xsl:variable>
    <xsl:variable name="filename" select="replace(($data/filename,$bodies)[1],
      replace(base-uri(doc('')),'[^/]+$',''),'')"/>
    <xsl:variable name="content-type" select="$request/web:multipart/web:body/@content-type"/>
    <web:response status="200" message="Ok">
      <!-- Depends on the Accept header actually, see
              http://www.w3.org/TR/xhtml-media-types/.  Good candidate
              for a tool function provided by Servlex itself. -->
      <web:body content-type="text/html" method="html"/>
    </web:response>
    <html xsl:expand-text="yes" xmlns="http://www.w3.org/1999/xhtml">
      <head>
        <title>Streaming analysis of file: {$filename}</title>
        <meta name="description" content=""/>
        <meta name="keywords" content=""/>
        <meta name="coverage" content="Worldwide"/>
        <meta name="copyright" content="Copyright Saxonica Limited"/>
        <link rel="stylesheet" href="style/saxondocs.css" type="text/css"/>
        <link rel="stylesheet" href="style/stream-analyze.css" type="text/css"/>
      </head>
      <body>
        <script type="text/javascript" language="javascript" src="Saxonce/Saxonce.nocache.js"/>
        <script type="application/xslt+xml" language="xslt2.0" src="style/show-stream.xsl" data-initial-template="go"/>
        <!--        <script type="text/javascript" language="javascript" xsl:expand-text="no">
          function parent() {
           return window.parent;
           }
        </script>-->
<!--        <pre><xsl:sequence select="$web:input"/></pre>
-->        <p class="config">Date:<span class="value">{current-dateTime()}</span> XSLT:<span
            class="value">{system-property('xsl:version')}</span> Saxon:<span class="value"
            >{system-property('xsl:product-version')}</span>
          <!--<span>{base-uri(doc(''))}</span>-->
        </p>
        <!--<p id="loading">Loading Saxon-CE - please wait</p>-->
        <table>
          <tbody>
            <tr>
              <td class="VALIGN" id="controls">
                <xsl:apply-templates select="$stream-controls" mode="make-controls"/>
              </td>
              <td class="VALIGN">
                <h3>Streaming analysis of file: {$filename}</h3>
                <xsl:choose>
                  <xsl:when test="$content-type = 'application/octet-stream'">
                    <p class="foo">
                      <xsl:value-of
                        select="saxon:base64Binary-to-string(xs:base64Binary($bodies),'UTF8')"/>
                    </p>
                  </xsl:when>
                  <xsl:when test="$content-type = ('text/xml','text/xsl','text/plain')">
                    <pre>
                      <xsl:variable name="parsed">
                        <xsl:for-each select="if($content-type='text/xsl') then saxon:parse($bodies)
                                         else if($content-type='text/plain') then doc(normalize-space($bodies))
                                         else $bodies">
                          <xsl:call-template name="analyze-streamability"/>
                        </xsl:for-each>
                      </xsl:variable>
                      <xsl:apply-templates select="$parsed" mode="serialize">
                        <xsl:with-param name="truncate.attributes" select="100" tunnel="yes"/>
                        <xsl:with-param name="truncate" select="100" tunnel="yes"/>
                        <xsl:with-param name="spec" select="$spec.out" tunnel="yes"/>
                        <xsl:with-param name="normalize.attributes" select="true()" tunnel="yes"/>
                      </xsl:apply-templates>
                   </pre>
                  </xsl:when>
                  <xsl:otherwise>
                    <p> Unknown content type: {$content-type}</p>
                  </xsl:otherwise>
                </xsl:choose>
              </td>
            </tr>
          </tbody>
        </table>
      </body>
    </html>
  </xsl:template>

</xsl:stylesheet>
