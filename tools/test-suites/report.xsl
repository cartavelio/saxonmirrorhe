<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
                xmlns:t="http://www.w3.org/2012/08/qt-fots-results"
                xmlns:xs="http://www.w3.org/2001/XMLSchema"
                exclude-result-prefixes="xs"
                version="3.0">

  <xsl:param name="rundate" select="()"/>

  <xsl:variable name="input-docs-uri" as="xs:anyURI*">
    <xsl:for-each select="uri-collection('../../build/test-results?recurse=yes')">
      <!-- This stylesheet operates on the raw output, not the formatted results. -->
      <xsl:if test="not(contains(., '/results/'))
                    and not(contains(., '/xsd/'))
                    and not(ends-with(., '/exceptions.xml'))
                    and ends-with(., '.xml')">
        <xsl:sequence select="."/>
      </xsl:if>
    </xsl:for-each>
  </xsl:variable>

  <xsl:variable name="run-dates"
                select="distinct-values($input-docs-uri
                                        ! substring-after(., 'build/test-results/')
                                        ! tokenize(., '/')[3])"/>

  <!-- This stylesheet only expects to format the results of a single day. -->
  <xsl:variable name="reportdate"
                select="if (empty($rundate))
                        then sort($run-dates)[last()]
                        else $run-dates[. = $rundate]"/>

  <xsl:output indent="yes"/>

  <xsl:template name="xmain">
    <xsl:for-each select="$input-docs-uri">
      <xsl:message select="."/>
    </xsl:for-each>
    <xsl:sequence select="$run-dates"/>
    <xsl:sequence select="'R:', $reportdate"/>
    <doc/>
  </xsl:template>

  <xsl:template name="main">
    <xsl:if test="empty($reportdate)">
      <xsl:message terminate="yes">
        <xsl:text>No reports exist for </xsl:text>
        <xsl:sequence select="$rundate"/>
      </xsl:message>
    </xsl:if>

    <xsl:variable name="xmlReport" as="element()*">
      <xsl:call-template name="ToXml" />
    </xsl:variable>

    <xsl:result-document href="report.xml">
      <out>
        <xsl:comment>
          Generated - <xsl:value-of select="current-dateTime()"/>
        </xsl:comment>
        <xsl:copy-of select="$xmlReport" />
      </out>
    </xsl:result-document>

    <xsl:call-template name="ToHtml">
      <xsl:with-param name="xmlReport" select="$xmlReport" />
    </xsl:call-template>
  </xsl:template>

  <xsl:template name="ToXml" as="element()*">
      <xsl:for-each select="$input-docs-uri[contains(., '/' || $reportdate || '/')]">
        <xsl:variable name="input-docs" select="doc(.)"/>
        <xsl:variable name="test-cases"
                      select="$input-docs/test-suite-result/test-set/test-case"/>
        <xsl:variable name="failures" select="$test-cases[@result='fail']"/>
        <xsl:variable name="product" as="element(product)">
          <xsl:element name="product">
            <xsl:choose>
              <xsl:when test="exists($input-docs//product)">

                <xsl:choose>
                  <xsl:when test="contains(., 'extra')">
                    <xsl:attribute name="test-suite" select="'saxon-extra'" />
                  </xsl:when>
                  <xsl:when test="not(contains(., 'extra'))">
                    <xsl:attribute name="test-suite" select="'w3c'" />
                  </xsl:when>
                </xsl:choose>

                <xsl:attribute name="name" select="$input-docs//product/@name" />
                <xsl:attribute name="version" select="$input-docs//product/@version" />
                <xsl:attribute name="bytecode" select="$input-docs//product/@bytecode" />
                <xsl:attribute name="language" select="$input-docs//product/@language" />
                <xsl:attribute name="date-run" select="$input-docs//test-run/@date-run"/>

              </xsl:when>
              <xsl:when test="exists($input-docs//implementation)">

                <xsl:choose>
                  <xsl:when test="contains(., 'extra')">
                    <xsl:attribute name="test-suite" select="'saxon-extra'" />
                  </xsl:when>
                  <xsl:when test="not(contains(., 'extra'))">
                    <xsl:attribute name="test-suite" select="'w3c'" />
                  </xsl:when>
                </xsl:choose>

                <xsl:attribute name="name" select="$input-docs//implementation/@name" />
                <xsl:attribute name="version" select="$input-docs//implementation/@version" />
                <xsl:attribute name="bytecode" select="$input-docs//implementation/configuration/@bytecode" />
                <xsl:attribute name="language" select="$input-docs//implementation/configuration/@lang" />
                <xsl:attribute name="jit" select="$input-docs//implementation/configuration/@jit" />
                <xsl:attribute name="date-run" select="$input-docs//test-run/@dateRun"/>

              </xsl:when>
              <xsl:otherwise>
                <xsl:message>UNKNOWN Found with uri=<xsl:value-of select="."/></xsl:message>
                <xsl:attribute name="name" select="'UNKNOWN'" />
                <xsl:attribute name="bytecode" select="'NA'" />
                <xsl:attribute name="langauge" select="'NA'" />

              </xsl:otherwise>
            </xsl:choose>
          </xsl:element>
        </xsl:variable>

        <product edition="{$product/@name}" bytecode="{$product/@bytecode}"
                 lang="{$product/@language}" version="{$product/@version}">
          <xsl:if test="exists($product/@jit)">
            <xsl:attribute name="jit" select="$product/@jit"/>
          </xsl:if>
          <xsl:if test="contains(., 'unfolded')">
            <xsl:attribute name="unfolded" select="'yes'"/>
          </xsl:if>
          <test-run date="{$product/@date-run}" test-suite="{$product/@test-suite}"/>
          <tests successes="{count($test-cases[@result='pass'])}"
                 failures="{count($failures)}"
                 incorrect-errorcode="{count($test-cases[@result='wrongError'])}"
                 not-run="{count($test-cases[@result='n/a'])}"/>
          <xsl:if test="count($failures) gt 30">
            <failure-distribution>
              <xsl:for-each-group select="$failures" group-by="../@name">
                <test-set-failures name="{current-grouping-key()}"
                                   no="{count(current-group())}"/>
              </xsl:for-each-group>
            </failure-distribution>
          </xsl:if>
          <xsl:for-each select="$failures">
            <test-case-fail name="{@name}" comment="{@comment}"/>
          </xsl:for-each>
        </product>
      </xsl:for-each>
  </xsl:template>

  <xsl:template name="ToHtml">
    <xsl:param name="xmlReport" as="element()*" />
    <xsl:result-document href="report.html">
      <html>
        <head><title>Saxon Release reports</title>

        <style type="text/css">
          .tg  {border-collapse:collapse;border-spacing:0;}
          .tg td{border-color:black;border-style:solid;border-width:1px;font-family:Arial, sans-serif;font-size:14px;
          overflow:hidden;padding:10px 5px;word-break:normal;}
          .tg th{border-color:black;border-style:solid;border-width:1px;font-family:Arial, sans-serif;font-size:14px;
          font-weight:normal;overflow:hidden;padding:10px 5px;word-break:normal;}
          .tg .tg-1wig{font-weight:bold;text-align:left;vertical-align:top}
          .tg .tg-mnhx{background-color:#fe0000;text-align:left;vertical-align:top}
          .tg .tg-fd62{background-color:#32cb00;text-align:left;vertical-align:top}
          .tg .tg-s7ni{background-color:#f8ff00;text-align:left;vertical-align:top}
          .tg .tg-0lax{text-align:left;vertical-align:top}
        </style>
        </head>
        <body>
          <h1>Saxon Release Reports</h1>
          <p>
            <b>Saxon Version: <xsl:value-of select="string(($xmlReport[1])/@version)"/></b>
          </p>
          <p>Date: <xsl:value-of select="$reportdate"/></p>

          <table class="tg">
            <thead>
              <th class="tg-1wig">Product</th>
              <th class="tg-fd62"><b>Pass</b></th>
              <th class="tg-mnhx"><b>Fail</b></th>
              <th class="tg-s7ni"><b>Incorrect Error code</b></th>
              <th class="tg-0lax"><b>Skip</b></th>
              <th class="tg-0lax"><b>Test Failures</b></th>
            </thead>
            <!--Count=<xsl:value-of select="count($input-docs)"/> -->
            <xsl:for-each select="$xmlReport">
              <xsl:sort select="@edition" />
              <xsl:sort select="@lang" />
              <!-- Dir=<xsl:value-of select="string(document-uri(.))"/> -->

              <xsl:element name="tr">
                <td  class="tg-1wig" width="50">
                  <xsl:value-of select="@edition"/>
                  <br/>
                  Bytecode=<xsl:value-of select="@bytecode"/>
                  <br/>
                  <xsl:if test="exists(@jit)">
                    JIT=<xsl:value-of select="@jit"/>
                    <br/>
                  </xsl:if>

                  Lang=<xsl:value-of select="@lang"/>
                  <br/>
                  Test-Suite= <xsl:value-of select="./test-run/@test-suite"/>
                  <br/>
                  <xsl:if test="exists(@export)">
                    Export= <xsl:value-of select="@export"/>
                    <br/>
                  </xsl:if>

                  <!--Dir=<xsl:value-of select="../@dir"/> -->

                </td>
                <td  class="tg-fd62">
                  <xsl:value-of select="tests/@successes"/>
                </td>
                <td class="tg-mnhx">
                  <xsl:value-of select="tests/@failures"/>
                </td>
                <td class="tg-s7ni">
                  <xsl:value-of select="tests/@incorrect-errorcode"/>
                </td>
                <td class="tg-0lax">
                  <xsl:value-of select="tests/@not-run"/>
                </td>
                <td class="tg-0lax">
                  <xsl:for-each select="test-case-fail">
                    <xsl:value-of select="@name"/>, &#x20;

                  </xsl:for-each>
                </td>



              </xsl:element>




            </xsl:for-each>
          </table>
        </body>

      </html>
    </xsl:result-document>

  </xsl:template>


</xsl:stylesheet>
