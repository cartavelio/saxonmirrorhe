<xsl:stylesheet version="3.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns:xs="http://www.w3.org/2001/XMLSchema"
  xmlns:doc="http://www.saxonica.com/ns/documentation" xmlns:jwl="http://jwlresearch.net/" expand-text="yes">
  <!--the main stylesheet that runs against the pd content in prod-desc_9.n.xml-->
  <doc:doc xmlns="http://www.w3.org/1999/xhtml" title="Saxon Datasheets">
    <h1>Generate Product Variant Datasheets</h1>
    <p>This stylesheet generates HTML and FO versions of the product variant datasheets.</p>
    <h2>Directions for use</h2>
    <p>Define the product variant components in the main XML file (currently a file named 'prod-desc-<em>version</em>.xml').</p>
    <p>Edit the main styling required in the CSS stylesheet (default <code>saxondocs_new.css</code>). This will be mostly sizing, colours, fonting, space
      before/after, indenting etc.</p>
    <p>Change the page size and header/footer sizes in the file <code>config.xml</code>. (In future we could also add additional sections that could be added
      for example to the header.]</p>
    <p>Run this stylesheet (<code>prod-desc-to-fo.xsl</code>) in Saxon (either independently or under Oxygen) under XSLT 3.0 mode, with the product details file
      as input.</p>
    <p>The results will appear as a set of HTML and FO files. The HTML files are all in a directory html/, the FO in an fo/ directory.</p>
    <p>Use FOP (e.g. under Oxygen) to convert FO to PDF files. [Note - make sure the PDF file isn't open in Acrobat - it's a common problem.]</p>
    <p>
      <strong>If you run under Oxygen, then you may be able do a batch conversion of the FO files to PDF by selecting each of them, right click, and
        'Transform/Apply Transformation Scenario' - the default for FO files is to generate the PDF.</strong> I may also be able to include the last two as a
      single step if this is required. </p>

    <h2>Technical details</h2>
    <p>This stylesheet is designed as follows:</p>
    <ul>
      <li>
        <p>The variants required are a sequence of string names as the parameter $variants, e.g. <code>'HE','PE'</code> etc. The default value is the current
          set of product variants.</p>
      </li>
      <li>
        <p>The input is a product details XML structure, where each section that is variant-specific is decorated with a <code>@variant</code> attribute
          containing a whitespace-separated list of the variants it relevant to, e.g. <code>variant="PE EE"</code>.</p>
        <p>Top level sections of this structure can be decorated <code>@class="header|footer"</code>, which will collect them into a header and a footer section
          respectively.</p>
      </li>
      <li>
        <p>A set of datasheets is collected for each variant, using mode <code>variant</code>. These are stored in a map, as are all results.</p>
      </li>
      <li>
        <p>Each definition is converted into an HTML document using mode <code>doc</code>. The overall template for these documents is defined in an XSLT
          template (stored separately in <code>prod-desc-template.xsl</code>) . These HTML documents are styled though a CSS stylesheet, whose name is a
          parameter <code>$CSS</code>, which, if relative, is resolved relative to the input data.</p>
      </li>
      <li>
        <p>The bodies of these documents are converted into XSL-FO sources, first by styling (using a function <code>jwl:applyCSS($stylesheet.URI,$doc)</code>
          that applies CSS properties to an XML tree leaving the results as attributes), then by using the mode <code>FO</code> to produce XSL-FO instructions,
          which is defined in the stylesheet <code>prod-fo.xsl</code></p>
      </li>
      <li>
        <p>The results appear in '<em>variant</em>-prod-desc.xml' files for the HTML and '<em>variant</em>-prod-desc.fo' files for the FO source</p>
      </li>
      <li>
        <p>Use FOP to convert the FO files.</p>
      </li>
    </ul>
    <h3>Required resources</h3>
    <ul>
      <li>
        <p><code><eval xpath="tokenize(base-uri(.),'/')[last()]"/></code> - the main stylesheet to be evaluated over the product description.</p>
      </li>
      <li>
        <p><code>prod-desc-template.xsl</code> - the overall HTML template used to define the outer shell of the HTMLresults.</p>
      </li>
      <li>
        <p><code>prod-fo.xsl</code> - converts (styled) HTML to FO. </p>
      </li>
      <li>
        <p><code>css2xslt.xsl</code> - applies CSS to an XML tree, leaving results as attributes. </p>
      </li>
      <li>
        <p><code>self-doc.xsl</code> - self-documents a document.</p>
      </li>
      <li>
        <p><code>config.xml</code> - configuration file for things like page size and headers and footers.</p>
      </li>
      <!--  <xsl:for-each select="$this.document/xsl:include">
        <li>{@href}</li>
      </xsl:for-each>-->
    </ul>
    <h2>Restrictions &amp; shortcomings</h2>
    <ul>
      <li>
        <p>Relative column widths can be used by styling <code>width:75%</code> through CSS onto table cells. Currently this will work for styling just
            <i>one</i> column, but is erroneous beyond that.</p>
      </li>
    </ul>

  </doc:doc>

  <xsl:include href="prod-fo.xsl"/>
  <xsl:include href="css2xslt.xsl"/>
  <xsl:include href="self-doc.xsl"/>
  <xsl:strip-space elements="*"/>
  <xsl:output method="xml" indent="yes"/>
  <xsl:output name="xml" method="xml" indent="yes"/>
  <xsl:output name="html" method="html"/>
  <xsl:output name="xhtml" method="xhtml"/>
  
  <xsl:param name="possible-base-uri" as="xs:string?"/>

  <xsl:param name="variants" select="
    'J-HE',
    'J-PE',
    'J-EE',
    'J-EET',
    'J-EEQ',
    'J-EEV',
    'CS-EE',
    'CS-EET',
    'CS-EEQ',
    'CS-EEV',
    'C-HE',
    'C-PE',
    'C-EE',
    'C-EET',
    'C-EEQ',
    'C-EEV',
    'JS'"/>
  <xsl:param name="CSS" as="xs:string">saxondocs_new.css</xsl:param>
  <xsl:variable name="CSS.URI" as="xs:anyURI" select="resolve-uri($CSS)"/>

  <xsl:variable name="this.document" as="document-node()" select="doc('')"/>

  <xsl:variable name="baseURI" select="($possible-base-uri,base-uri(/))[1]"/>

  <xsl:template match="/">
    <xsl:message select="concat('URI for CSS:  ', $CSS.URI)"/>
    <xsl:message select="concat('Absolute base URI for output:  ', $baseURI)"/>
    <xsl:result-document href="TTT.xml" format="xml">
      <xsl:call-template name="CSS2XSLT">
        <xsl:with-param name="URI" select="$CSS.URI"/>
      </xsl:call-template>
    </xsl:result-document>
    <xsl:variable name="prod.doc" select="doc"/>
    <xsl:variable name="variant-defs" as="map(xs:string,item()*)">
      <xsl:map>
        <xsl:for-each select="$variants">
          <xsl:variable name="platform" select="tokenize(., '-')[1]"/>
          <xsl:variable name="version" select="if ($platform eq 'C') then $prod.doc/@c-version else (if
            ($platform eq 'JS') then $prod.doc/@js-version else $prod.doc/@version)"/>
          <xsl:map-entry key=".">
            <xsl:apply-templates select="$prod.doc" mode="variant">
              <xsl:with-param name="variant" select="." tunnel="yes"/>
              <xsl:with-param name="version" select="$version" tunnel="yes"/>
            </xsl:apply-templates>
          </xsl:map-entry>
        </xsl:for-each>
      </xsl:map>
    </xsl:variable>
    <xsl:variable name="variant-docs" as="map(xs:string,item()*)">
      <xsl:map>
        <xsl:for-each select="$variants">
          <xsl:map-entry key=".">
            <xsl:apply-templates select="$variant-defs(.)" mode="doc"/>
          </xsl:map-entry>
        </xsl:for-each>
      </xsl:map>
    </xsl:variable>
    <xsl:variable name="variant-bodies" as="map(xs:string,item()*)">
      <xsl:map>
        <xsl:for-each select="$variants">
          <xsl:map-entry key="." select="jwl:applyCSS($CSS.URI,$variant-docs(.)/body)"/>
        </xsl:for-each>
      </xsl:map>
    </xsl:variable>
    <xsl:variable name="variant-fo" as="map(xs:string,item()*)">
      <xsl:map>
        <xsl:for-each select="$variants">
          <xsl:map-entry key=".">
            <xsl:apply-templates select="$variant-bodies(.)" mode="FO"/>
          </xsl:map-entry>
        </xsl:for-each>
      </xsl:map>
    </xsl:variable>

    <xsl:for-each select="$variants">
      <xsl:result-document href="html/{.}-prod-desc.html" format="html">
        <xsl:apply-templates select="$variant-docs(.)" mode="remove-FO"/>
      </xsl:result-document>
      <xsl:result-document href="html/{.}-prod-desc.xhtml" format="xhtml">
        <xsl:apply-templates select="$variant-docs(.)" mode="remove-FO"/>
      </xsl:result-document>
      <!--   <xsl:message select="
          'Generating',
          ."/>-->
      <xsl:result-document href="fo/{.}.fo">
        <xsl:sequence select="$variant-fo(.)"/>
      </xsl:result-document>
      <xsl:result-document href="{.}.body.xml">
        <xsl:sequence select="$variant-bodies(.)"/>
      </xsl:result-document>
    </xsl:for-each>
    <xsl:result-document href="../README.html" format="html">
      <xsl:sequence select="doc:self-doc($this.document, $CSS)"/>
    </xsl:result-document>
    <RESULT>
      <xsl:sequence select="$variant-defs('EET')"/>
    </RESULT>

  </xsl:template>


  <xsl:mode name="variant" on-no-match="shallow-copy" doc:doc="Collect the appropriate variant, which is defined though the tunneled parameter $variant."/>


  <xsl:template match="*" mode="variant">
    <xsl:param name="variant" tunnel="yes"/>
    <xsl:choose>
      <xsl:when test="exists(@variant) and tokenize(@variant, '\s') = $variant">
        <xsl:copy>
          <xsl:copy-of select="@* except @variant"/>
          <xsl:apply-templates mode="#current"/>
        </xsl:copy>
      </xsl:when>
      <xsl:when test="not(@variant)">
        <xsl:copy>
          <xsl:copy-of select="@*"/>
          <xsl:apply-templates mode="#current"/>
        </xsl:copy>
      </xsl:when>
    </xsl:choose>
  </xsl:template>

  <xsl:template match="span[@value = '$timestamp']" mode="variant">
    <xsl:copy>
      <xsl:sequence select="@* except @value"/>{format-dateTime(current-dateTime(),'[D1o] [MNn] [Y] @ [H01]:[m01]')}</xsl:copy>
  </xsl:template>
  <xsl:template match="span[@value = '$version']" mode="variant">
    <xsl:param name="version" tunnel="yes"/>
    <xsl:copy>
      <xsl:sequence select="@* except @value"/>{$version}</xsl:copy>
  </xsl:template>



  <xsl:mode name="doc" on-no-match="shallow-copy"
    doc:doc="Create an HTML version of a given datasheet. This also collects headers and footers at the ends of the document"/>

  <xsl:include href="prod-desc-template.xsl"/>


  <xsl:template match="title" mode="doc">
    <h1 class="proddesc">
      <xsl:sequence select="node()"/>
    </h1>
  </xsl:template>
  <xsl:template match="section" mode="doc">
    <!-- <h2 class="license"><xsl:number/>.&#xa0;&#xa0;<xsl:value-of select="@title"/></h2>-->
    <h2 class="license">{@title}</h2>
    <xsl:apply-templates mode="#current"/>
  </xsl:template>

  <!--<xsl:template match="section[@format='table']" mode="doc">
    <h1 class="license">{@title}</h1>
    <table>
      <xsl:if test="@widths">
        <colgroup>
          <xsl:for-each select="tokenize(@widths,'\s+')">
            <col style="width:{.}%"/>
          </xsl:for-each>
        </colgroup>
      </xsl:if>
      <tbody>
        <xsl:apply-templates mode="#current"/>
      </tbody>
    </table>
  </xsl:template>-->


  <!-- We avoid empty tables as FOP moans.... -->
  <xsl:template match="table[@title]" mode="doc">
    <xsl:variable name="table" as="element()?">
      <xsl:next-match/>
    </xsl:variable>
    <xsl:if test="exists($table)">
      <h2 class="license">{@title}</h2>
      <xsl:sequence select="$table"/>
    </xsl:if>
  </xsl:template>
  <xsl:template match="table" mode="doc">
    <xsl:variable name="body" as="element()">
      <xsl:apply-templates select="*" mode="#current"/>
    </xsl:variable>
    <xsl:if test="exists($body/tr)">
      <xsl:copy>
        <xsl:apply-templates select="@*" mode="#current"/>
        <colgroup>
          <xsl:for-each select="1 to max($body/tr/count(td))">
            <col/>
          </xsl:for-each>
        </colgroup>
        <xsl:sequence select="$body"/>
      </xsl:copy>
    </xsl:if>
  </xsl:template>


  <xsl:template match="section[@class = 'feature-set']/section | tbody/section" mode="doc">
    <tr>
      <td>
        <h3 class="license">{@title}</h3>
      </td>
      <td class="feature-body">
        <xsl:apply-templates mode="#current"/>
      </td>
    </tr>
  </xsl:template>

  <xsl:template match="list[@number]" mode="doc">
    <ol type="{@number}">
      <xsl:apply-templates mode="#current"/>
    </ol>
  </xsl:template>

  <xsl:template match="list" mode="doc">
    <ul>
      <xsl:apply-templates mode="#current"/>
    </ul>
  </xsl:template>

  <xsl:template match="item" mode="doc">
    <li>
      <p>
        <xsl:apply-templates mode="#current"/>
      </p>
    </li>
  </xsl:template>

  <xsl:template match="phrase" mode="doc">
    <xsl:apply-templates mode="#current"/>
  </xsl:template>

  <xsl:template match="i" mode="doc">
    <i>
      <xsl:apply-templates mode="#current"/>
    </i>
  </xsl:template>

  <xsl:template match="term" mode="doc">
    <xsl:text>"</xsl:text>
    <xsl:value-of select="."/>
    <xsl:text>"</xsl:text>
  </xsl:template>

  <xsl:template match="
      section[@class = ('header',
      'footer')]" mode="doc">
    <div>
      <xsl:sequence select="@* except @class"/>
      <xsl:apply-templates mode="#current"/>
    </div>
  </xsl:template>



  <xsl:mode name="remove-FO" on-no-match="shallow-copy" doc:doc="Remove FO/PDF conditional parts"/>
  <xsl:template match="
      *[@use-when = ('PDF',
      'FO')]" mode="remove-FO"/>

</xsl:stylesheet>
