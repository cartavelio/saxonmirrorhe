<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
  xmlns:xs="http://www.w3.org/2001/XMLSchema"
  xmlns:math="http://www.w3.org/2005/xpath-functions/math"
  xmlns:uca="java:com.saxonica.expr.sort.UcaCollatorUsingIcu"
  xmlns:icuN="java:com.saxonica.config.pe.ICUNumbererPE" xmlns:f="myfunctions"
  xmlns:map="http://www.w3.org/2005/xpath-functions/map" xmlns:saxon="http://saxon.sf.net/"
  xmlns:X="IndirectXSLT" xmlns:doc="http://www.saxonica.com/ns/doc"
  exclude-result-prefixes="xs math map f uca icuN saxon xsl doc" version="3.0"> 

  <doc:doc>
    <p>This stylesheet generates sections of the Saxon documentation for the UCA/ICU collation and
      numbering support.</p>
  </doc:doc>

  <xsl:variable name="add.diagnostics" select="false()" as="xs:boolean" static="yes"
    doc:doc="Adds some diagnostic info to the resulting documentation"/>

  <xsl:param name="output-root" as="xs:string">locales-ICU</xsl:param>
  <xsl:param name="output" as="xs:string?"
    doc:doc="Possible output URI for use in Ant, which will be resolved relative to base-URI"
    >extensibility/catalog.xml</xsl:param>

    <xsl:variable name="base-uri" select="base-uri(/)"/>
  
  <xsl:variable name="use-last" as="map(*)" select="map{'duplicates':'use-last'}"/>

  <xsl:include href="map-print.xsl"/>
  <xsl:namespace-alias stylesheet-prefix="X" result-prefix="xsl"/>
  <xsl:output method="xml" indent="no"/>
  <xsl:output name="text" method="text" indent="no"/>

  <xsl:variable name="audit"
    select="'Generated from:',resolve-uri(document('')/base-uri()),format-dateTime(current-dateTime(),'[Y] [MNn] [D01] @ [H01]:[m]')"/>

  <xsl:variable name="abbrev"
    select="function($s) {string-join(tokenize($s,'-')!
    (lower-case(substring(.,1,1))||(if(position() gt 5) then lower-case(substring(.,2,1)) else '')),
    '')}"/>
  <xsl:function name="f:val" as="element()">
    <xsl:param name="s" as="xs:string"/>
    <span class="value">
      <xsl:value-of select="$s"/>
    </span>
  </xsl:function>
  <xsl:function name="f:val-abbrev" as="element()" expand-text="yes">
    <xsl:param name="s" as="xs:string"/>
    <span>{$s} (<span class="value">{$abbreviations($s)}</span>)</span>
  </xsl:function>
  <xsl:variable name="with-abbrev" select="function($s) {f:val-abbrev($s)}"/>

  <xsl:function name="f:br">
    <xsl:param name="in" as="item()*"/>
    <xsl:for-each select="$in">
      <xsl:sort/>
      <xsl:sequence select="."/>
      <xsl:if test="position() ne last()">
        <br/>
      </xsl:if>
    </xsl:for-each>
  </xsl:function>

  <xsl:variable name="langs.uca.all" as="map(xs:string,xs:string)"
    select="map:merge(uca:getLocales()! (let $t := tokenize(.,';') return map:entry($t[1],$t[2])), $use-last)"/>
  <xsl:variable name="langs.uca" as="map(xs:string,item()*)">
    <xsl:map>
      <xsl:for-each-group select="map:keys($langs.uca.all)" group-by="replace(.,'-.*','')">
        <xsl:map-entry key="current-grouping-key()" select="current-group()"/>
      </xsl:for-each-group>
    </xsl:map>
  </xsl:variable>

  <xsl:variable name="langs.icuN.all" as="map(xs:string,xs:string)"
    select="map:merge(icuN:getLocales()! (let $t := tokenize(.,';') return map:entry($t[1],$t[2])), $use-last)"/>
  <xsl:variable name="langs.icuN" as="map(xs:string,item()*)">
    <xsl:map>
      <xsl:for-each-group select="map:keys($langs.icuN.all)" group-by="replace(.,'-.*','')">
        <xsl:map-entry key="current-grouping-key()" select="current-group()"/>
      </xsl:for-each-group>
    </xsl:map>
  </xsl:variable>

  <xsl:variable name="numberings" as="map(xs:string,item()*)">
    <xsl:map>
      <xsl:for-each select="map:keys($langs.icuN)">
        <xsl:variable name="base" select="tokenize(.,';')"/>
        <xsl:variable name="numberings"
          select="icuN:availableSpelloutNumberings($base[1])!replace(.,'^%','')"/>
        <xsl:variable name="ordinal.numberings"
          select="icuN:availableOrdinalNumberings($base[1])!replace(.,'^%','')"/>
        <xsl:map-entry key="." select="$numberings,$ordinal.numberings"/>
      </xsl:for-each>
    </xsl:map>
  </xsl:variable>


  <xsl:variable name="abbreviations.normal"
    select="map:merge(distinct-values(map:keys($numberings)!$numberings(.))!
    map:entry(.,$abbrev(.)), $use-last)"/>
  <xsl:variable name="abbreviations.special"
    select="map {'spellout-cardinal-financial' : 'scfi',
    'spellout-cardinal-neuter' : 'scne',
    'spellout-ordinal-neuter' : 'sone' }"/>
  <xsl:variable name="abbreviations"
    select="map:merge(($abbreviations.normal,$abbreviations.special), $use-last)"/>

  <xsl:variable name="clashes"
    select="for $k in map:keys($abbreviations)
    return if($abbreviations($k) = map:keys($abbreviations)[not(. = $k)]!$abbreviations(.)) then $k else ()"/>
  <xsl:variable name="variables" as="map(xs:string,item()*)"
    select="map { 
    'ICU-languages' : $langs.icuN.all,
    'ICU-language-groups' : $langs.icuN,
    'ICU-abbreviations' : $abbreviations,
    'ICU-numberings' : $numberings }
    "/>
  <xsl:variable name="map.descriptions" as="element()*">
    <part name="ICU-languages" type="map(xs:string,xs:string)">
      <desc>Descriptions of all the languages supported</desc>
      <key>Language code (e.g. <code>'en-GB'</code>)</key>
      <value>Language description string, e.g. <code><xsl:value-of
            select="string-join($langs.icuN.all('en-GB')!$quoter(.),',')"/></code></value>
    </part>
    <part name="ICU-language-groups" type="map(xs:string,xs:string*)">
      <desc>All the language <em>groupings</em> supported for a given base language.</desc>
      <key>Language code (e.g. <code>az</code>)</key>
      <value>Sequence of language (locale) codes, e.g. <code>(<xsl:value-of
            select="string-join($langs.icuN('az')!$quoter(.),',')"/>)</code></value>
    </part>
    <part name="ICU-abbreviations" type="map(xs:string,xs:string)">
      <desc>Abbreviation codes for the numbering schemes</desc>
      <key>Scheme name (e.g. <code>'spellout-ordinal'</code>)</key>
      <value>Private tag code for the scheme, e.g. <code>'so'</code></value>
    </part>
    <part name="ICU-numberings" type="map(xs:string,xs:string*)">
      <desc>The numbering schemes supported for each base language</desc>
      <key>Language code (e.g. <code>af</code>)</key>
      <value>Sequence of numbering scheme names, e.g. <code>(<xsl:value-of
            select="string-join($numberings('af')!$quoter(.),',')"/>)</code></value>
    </part>
  </xsl:variable>
  <xsl:variable name="preference-order" select="icuN:getPreferences()"/>

  <xsl:template match="/">
    <xsl:call-template name="go"/>
  </xsl:template>

  <xsl:template name="go" expand-text="yes">
    <!-- <result>
      <timestamp>{current-dateTime()}</timestamp>
      <ICU-version>{icuN:getICUVersion()}</ICU-version>
    </result>-->
    <xsl:variable name="output-root"
      select="if(exists($output))
      then resolve-uri($output-root,xs:string(resolve-uri($output,xs:string($base-uri)))) 
      else $output-root"/>

    <xsl:result-document href="{$output-root}.xsl" expand-text="yes" xmlns="IndirectXSLT">
      <stylesheet version="3.0">
        <xsl:comment>AUTO-GENERATED: do not edit</xsl:comment>
        <xsl:comment>{$audit}</xsl:comment>
        <xsl:for-each select="map:keys($variables)">
          <variable name="{.}">
            <xsl:attribute name="select">
              <xsl:call-template name="map-print-XPath">
                <xsl:with-param name="items" select="$variables(.)"/>
              </xsl:call-template>
            </xsl:attribute>
          </variable>
        </xsl:for-each>
      </stylesheet>
    </xsl:result-document>

    <xsl:result-document href="{$output-root}.xml" expand-text="yes">
      <xsl:variable name="width" select="3"/>
      <section id="UCA-locales" title="UCA-supported locales">
        <xsl:comment>AUTO-GENERATED: do not edit</xsl:comment>
        <xsl:comment>{$audit}</xsl:comment>
        <h1>UCA-supported locales</h1>
        <p xsl:use-when="$add.diagnostics">{$audit}</p>
        <aside>Since Saxon 9.8, the ICU library is updated from version 57.1 to 59.1; which brings
          Unicode 9.0 support.</aside>
        <p>These are the locales ({count(map:keys($langs.uca))} base languages,
          {count(map:keys($langs.uca.all)) - count(map:keys($langs.uca))} variants) supported for
          collation in Saxon-PE/EE, from the library of <a href="http://site.icu-project.org/">ICU -
            International Components for Unicode</a>, version {icuN:getICUVersion()}, implementing
          UCA version {uca:getUCAVersion()}:</p>
        <table>
          <xsl:comment>{$audit}</xsl:comment>
          <thead>
            <tr>
              <td colspan="2">Group</td>
              <td colspan="{2 * $width}">Variants</td>
            </tr>
            <tr>
              <td>code</td>
              <td>language</td>
              <xsl:for-each select="1 to $width">
                <td>code</td>
                <td>variant</td>
              </xsl:for-each>
            </tr>
          </thead>
          <tbody>
            <xsl:for-each select="map:keys($langs.uca)">
              <xsl:sort/>
              <xsl:variable name="base" select="."/>
              <xsl:variable name="variants" select="$langs.uca(.)"/>
              <xsl:variable name="mod" select="(count($variants) + $width - 1) idiv $width"/>
              <xsl:variable name="rows" as="element()*">
                <xsl:for-each-group select="$variants" group-by="(position() - 1) mod $mod">
                  <tr>
                    <xsl:for-each select="current-group()">
                      <xsl:variable name="parts" select="tokenize(.,';')"/>
                      <td class="value">{.}</td>
                      <td>{$langs.uca.all(.)}</td>
                    </xsl:for-each>
                  </tr>
                </xsl:for-each-group>
              </xsl:variable>
              <xsl:for-each select="$rows">
                <xsl:copy>
                  <xsl:choose>
                    <xsl:when test="position() eq 1">
                      <xsl:attribute name="class">lang-start</xsl:attribute>
                      <td class="value">{$base}</td>
                      <td>{$langs.uca.all($base)}</td>
                    </xsl:when>
                    <xsl:otherwise>
                      <td/>
                      <td/>
                    </xsl:otherwise>
                  </xsl:choose>
                  <xsl:sequence select="*"/>
                </xsl:copy>
              </xsl:for-each>
            </xsl:for-each>
          </tbody>
        </table>
      </section>
    </xsl:result-document>

    <xsl:result-document href="{$output-root}-numbering.java" format="text" expand-text="no"> static
      { <xsl:for-each select="map:keys($abbreviations)">
        <xsl:sort/> schemeCodes.put("<xsl:value-of select="$abbreviations(.)"/>","%<xsl:value-of
          select="."/>"); </xsl:for-each> } </xsl:result-document>

    <xsl:result-document href="{$output-root}-numbering.xml" expand-text="yes">
      <section id="ICU-numbers-and-dates" title="Numbers and dates from ICU">
        <xsl:comment>AUTO-GENERATED: do not edit</xsl:comment>
        <xsl:comment>{$audit}</xsl:comment>
        <h1>Numbers and dates from ICU</h1>
        <p xsl:use-when="$add.diagnostics">The time now, in Welsh, is
          {format-time(current-time(),'[HWw] [mw]','cy',(),())}</p>
        <p><a href="http://site.icu-project.org/">ICU - International Components for Unicode</a>
          (ICU) provides extensive facilities for localized numbering and date formatting, which are
          supported in Saxon-PE and -EE from version 9.6.</p>
        <p>The ICU features require a sizeable (~7MByte) library which may be supplied either in the
          main JAR file, or as a separate JAR, which can itself either be a 'minimised' version in
          the Saxonica distribution, or a complete ICU4J JAR downloaded from the ICU site.</p>
        <p>In the case that the ICU features have <strong>not</strong> been loaded within
          Saxon-PE/EE, support for numbering and dates for Danish, Dutch, Flemish, French (and
          Belgian French), German, Italian and Swedish is provided as detailed in the table of <a
            href="/localization/localizing-numbers-and-dates/other-numberings">Numberings for selected
            languages</a>.</p>
        <h2 class="subtitle">Numbering</h2>
        <p>ICU supports a large set of language-specific rulesets for supporting different forms of
          spelled-out numbering and digit-ordinal treatment. For example:</p>
        <table>
          <thead class="params">
            <tr>
              <td>Language</td>
              <td>Number</td>
              <td>Ruleset</td>
              <td>XPath</td>
              <td>Result</td>
            </tr>
          </thead>
          <tbody>
            <xsl:variable name="examples" as="element()*">
              <example lang="en;English" value="1123" scheme="spellout-cardinal"/>
              <example lang="en-US;English(US)" value="242" scheme="spellout-ordinal-verbose"/>
              <example lang="cy;Welsh" value="132" scheme="spellout-cardinal-feminine"/>
              <example lang="es-PT;Portugese" value="242" scheme="spellout-ordinal-feminine"/>
            </xsl:variable>
            <xsl:for-each select="$examples">
              <tr>
                <td><code>{tokenize(@lang,';')[1]}</code><br/>{tokenize(@lang,';')[2]}</td>
                <td>{@value}</td>
                <td>%{@scheme}</td>
                <td>
                  <code>format-integer({@value},'Ww','{tokenize(@lang,';')[1]}-x-{$abbreviations(@scheme)}')</code>
                </td>
                <td>{format-integer(@value,'Ww',tokenize(@lang,';')[1]||'-x-'||$abbreviations(@scheme))}</td>
              </tr>
            </xsl:for-each>
            <xsl:variable name="examples.ordinal" as="element()*">
              <example lang="en;English" value="1123" scheme="spellout-cardinal"/>
              <example lang="en-US;English(US)" value="242" scheme="spellout-ordinal-verbose"/>
              <example lang="cy;Welsh" value="132" scheme="spellout-cardinal-feminine"/>
              <example lang="es-PT;Portugese" value="242" scheme="spellout-ordinal-feminine"/>
            </xsl:variable>
            <xsl:for-each select="$examples.ordinal">
              <tr>
                <td><code>{tokenize(@lang,';')[1]}</code><br/>{tokenize(@lang,';')[2]}</td>
                <td>{@value}</td>
                <td/>
                <td>
                  <code>format-integer({@value},'1;o','{tokenize(@lang,';')[1]}')</code>
                </td>
                <td>{format-integer(@value,'1;o',tokenize(@lang,';')[1])}</td>
              </tr>
            </xsl:for-each>
          </tbody>
        </table>
        <p>In all cases it appears to be that the numbering scheme names and their behaviour is
          taken from the base language, with no regional variation - i.e. <code>es-PT</code> and
            <code>es-419</code> would produce the same results.</p>
        <h3>Explicitly named numbering schemes</h3>
        <p>The simplest way to invoke a specific ICU numbering scheme (or spellout ruleset) is to name it explictly.
          In <a class="bodylink code" href="/functions/fn/format-integer">format-integer()</a> this can be
          done within the parentheses in the format 
          picture, for example:</p> 
        <samp>format-integer(@value,'1;o(%spellout-ordinal-verbose)',$lang)</samp>
        <p>When this approach is used, it does not matter whether "o" (for ordinal) or "c" (for
        cardinal) is specified; the chosen spellout rules take precedence.</p>
        <p>Similarly, with <a class="bodylink code" href="/xsl-elements/number">xsl:number</a>, you can
          specify <code>ordinal="%spellout-ordinal-verbose"</code>.
        Again, it does not matter whether the numbering scheme is cardinal, ordinal, or something else: 
        as noted in the XSLT 3.0 specification, the attribute name <code>ordinal</code> is a misnomer.</p>
        <p>If a spellout name is chosen that does not exist for the chosen language, Saxon attempts to fall back
          to a default scheme, it does not report an error.</p>
        <p>See <a class="bodylink" href="ICU-numbering">Supported ICU numbering schemes</a> for a full list
          of the names of numbering schemes available in different languages.</p>
        <aside><p>The XSLT 3.0 specification suggests the use of ICU spellout names as described here,
          but does not mandate it.</p></aside>
        <h3>Alternative ways to specify numbering schemes</h3>
        <p>To avoid such a direct dependency on ICU spellout names, and for compatibility with
        earlier Saxon and XSLT versions, alternative mechanisms are available, as described below.</p>
        <p>To invoke one of these schemes, an IETF BCP47 standard <em>private tag</em> can be appended
          to the language tag, with format <code>-x-<em>code</em></code>. With a very small number
          of exceptions (to avoid clashes) these codes are encoded as the sequence of first letters
          of each word, the result being all lower case: for example <code>%spellout-ordinal-verbose</code> 
          with American English may be invoked using language code <code>en-US-x-sov</code>.
          Further examples of use are shown in the 
          table above. These private tag codes are recognised for 'word' numbering purposes on both
          <a class="bodylink code" href="/functions/fn/format-integer">format-integer()</a> and 
          <a class="bodylink code" href="/xsl-elements/number">xsl:number</a> language arguments.</p>
        <p>A full list of the scheme codes and their support in a given language is given in <a
            class="bodylink" href="ICU-numbering">Supported ICU numbering schemes</a>.</p>
        <p>In the absence of such a private tag, the following strategies are adopted:</p>
        <dl>
          <dt>Cardinal spellout</dt>
          <dd>When directed to generate a cardinal number using the 'w' patterns, the first of the
            following schemes is used, if found: <xsl:for-each select="$preference-order"
                ><code>spellout-cardinal{.}</code><xsl:if test="position() ne last()">,
              </xsl:if></xsl:for-each>. It appears that within ICU all languages contain at least
            one of these schemes, but if not, any scheme whose name matches the regular expression
              <code>^%spellout-cardinal</code> is used (choosing the first provided for the
            locale).</dd>
          <dt>Ordinal spellout</dt>
          <dd>When directed to generate an ordinal number using the 'w' patterns, the first of the
            following schemes is used, if found: <xsl:for-each select="$preference-order"
                ><code>spellout-ordinal{.}</code><xsl:if test="position() ne last()">,
              </xsl:if></xsl:for-each>. In the absence of any of these schemes, any scheme whose
            name matches the regular expression <code>^%spellout-ordinal</code> is used (choosing
            the first provided for the locale). In the case of there being <em>no</em> ordinal
            scheme available for the locale (or a language that does not have ordinals) the default
              <strong>cardinal</strong> scheme is used.</dd>
          <dt>Ordinal digits</dt>
          <dd>When producing an ordinal digit suffix (e.g. {format-integer(13,'1;o','es')} in
            Spanish), the 'digit-ordinal' ruleset is used by default - for those cases where there
            are specialist forms (e.g. Catalan and Spanish), the private tag must be set to get the
            specialist behaviour. </dd>
        </dl>
        <h3>English numbering</h3>
        <p>ICU renders <code>22</code> in words as "twenty-two", whereas Saxon has traditionally output
          "twenty two", with a space rather than hyphen separator. By default, for compatibility, the ICU
          result for all English schemes (cardinal and ordinal) is modified to use space as the
          separator. This can be modified by adding the extension <code>hyphen</code> or <code>nohyphen</code>
          to the language code: for example <code>en-x-hyphen</code> produces "twenty-two" while
          <code>en-x-nohyphen</code> produces "twenty two". This may be combined with other modifiers, for example
          <code>en-x-sov-hyphen</code> gives hyphenated verbose output.</p>
        <p>The default use of a '-verbose' scheme means that spellout of <code>118</code> yields
          "{format-integer(118,'w','en-x-scv')}", following the British usage rather than
          "{format-integer(118,'w','en-x-sc')}", which is the (US) shortened form. </p>
        <h3>Using format-picture options within spelled-out numbering</h3>
        <p><code>format-number()</code> and <code>format-integer()</code> can support further
          implementation-dependent control of numbering though parameters attached to cardinal
            (<code>c</code>) and ordinal (<code>o</code>) modifiers, e.g.
            <code>format-integer(1,'Ww;o(-er)','de')</code> which is intended to produce "Erster" in
          the recommended approach.</p>
        <p>In Saxon-PE/EE, there are two forms of such parameterisation supported for spelled-out
          (i.e. <code>W|w</code>) formats:</p>
        <ul>
          <li><strong><code>-<em>suffix</em></code></strong>. This requires a numbering of the given
            sort (cardinal or ordinal) which ends in the suffix given, if one is available for the
            locale in the implementation. </li>
          <li><strong><code>2=<em>example</em></code></strong>. This requires a numbering of the
            given sort (cardinal or ordinal) using the numbering scheme for which <code>2</code>
            would be formatted as <em>example</em> (ignoring case), if one is available for the
            locale in the implementation. For example <code>w;o(2=Secundo)</code> in Italian should
            yield "{format-integer(123,'w;o(2=Secondo)','it')}" for 123, whereas with
              <code>w;o(2=Seconda)</code> the result is
            "{format-integer(123,'w;o(2=Seconda)','it')}".</li>
        </ul>
        <p>For German (<code>lang="de"</code>), ICU at one time did not provide case/gender-variable ordinal
          word numbering (i.e. only "Erste" and not "Erster"). The Saxon implementation therefore supports the
                <strong><code>-<em>suffix</em></code></strong> ordinal option described above, which
          replaces the trailing 'e' on the generated ordinal. Thus
            <code>format-integer(1,'w;o(-en)','de')</code> produces
          "{format-integer(1,'w;o(-en)','de')}". </p>
        <h2 class="subtitle">Dates</h2>
        <p>ICU also provides facilities for localized date formatting, principally for names of days
          of the week and months, though a variety of calendars and epoch naming facilities are also
          available. In Saxon-PE/EE naming of months and days (through picture fields on <a 
            class="bodylink code" href="/functions/fn/format-date">format-date()</a>) is localised through
          the local language in scope. These appear to be all based on the base language, with no
          regional variations.</p>
        <p>As is required from the specification, when the language locale requested is not
          implemented, the result of <code>format-date()</code> or <code>format-dateTime()</code> is
          prefixed with "[Language: <em>default language code</em>]". </p>
        <h2 class="subtitle">Title case</h2>
        <p>Protocols for title case of sequences of words can differ markedly between languages,
          with many keeping strictly to lower-case throughout, and a very few (such as Dutch with
          'iJ') having very specialist rules. In Saxon, when title case is requested (e.g.
            <code>Ww</code> in <code>format-integer()</code> or <code>[MNn]</code> in
            <code>format-date()</code>) the Saxon implementation follows these rules:</p>
        <ul>
          <li>If the case requested is <code>lower</code> or <code>upper</code>, all words in the
            return from ICU are forced to the appropriate case.</li>
          <li>If the case requested is <code>title</code> then the returned string from ICU is
            examined:
            <ul>
          <li>If the first two characters are uppercase followed by lowercase, then the return is
            inferred already to be in title case and the result is returned unmodified.</li>
          <li>If not, then any letter preceded by a non-letter (or at start of string) is forced to
            uppercase, other letters are forced to lowercase.</li>
          <li>Some obvious 'joiner' words (e.g. 'and' in English, 'ën' in Dutch) that would
            otherwise be title-cased, are forced to lower-case.</li></ul></li>
        </ul>
        <p>Note that this may have problems, i.e. a title case could be forced on a language that
          otherwise might only ever use a uniform case. If you discover issues in a language you are
          using, please let us know.</p>

        <div xsl:use-when="false()">
          <h2>Maps of features</h2>
          <p>Saxon can provide a series of maps that describe the supported locales, numbering
            schemes and appropriate codes, which can be consulted programmatically to determine
            whether various forms of language and numbering are supported in the implementation.
            These are:</p>
          <table>
            <thead class="params">
              <tr>
                <td>Name</td>
                <td>Type</td>
                <td>Key</td>
                <td>Value</td>
                <td>Description</td>
              </tr>
            </thead>
            <tbody>
              <xsl:for-each select="$map.descriptions">
                <tr>
                  <td>{@name}</td>
                  <td>{@type}</td>
                  <td>
                    <xsl:sequence select="key/(*|text())"/>
                  </td>
                  <td>
                    <xsl:sequence select="value/(*|text())"/>
                  </td>
                  <td>
                    <xsl:sequence select="desc/(*|text())"/>
                  </td>
                </tr>
              </xsl:for-each>
            </tbody>
          </table>
        </div>
        <section id="ICU-numbering" title="Supported ICU numbering schemes">
          <h1>Supported ICU numbering schemes</h1>
          <aside>Since Saxon 9.8, the ICU library is updated from version 57.1 to 59.1; which brings
            Unicode 9.0 support.</aside>
          <p>The names of the numbering schemes and the codes to be used for them (ICU
            version:{icuN:getICUVersion()}) are shown in the following table, with those whose
            derivation differs from the standard rule (first-letter of each word) marked '*':</p>
          <xsl:if test="exists($clashes)">
            <p>Warning - use of the following codes is currently not supported, due to clashes:</p>
            <ul>
              <xsl:for-each select="$clashes">
                <xsl:sort/>
                <li>{$with-abbrev(.)}</li>
              </xsl:for-each>
            </ul>
          </xsl:if>
          <table>
            <thead class="params">
              <tr>
                <td>Code</td>
                <td>Ruleset name</td>
              </tr>
            </thead>
            <tbody>
              <xsl:variable name="special" select="map:keys($abbreviations.special)"/>
              <xsl:for-each select="map:keys($abbreviations)">
                <xsl:sort/>
                <tr>
                  <td class="value">{$abbreviations(.)}</td>
                  <td>{.}<xsl:if use-when="true()" test=". = $special"> * </xsl:if></td>
                  <!--<td>{.}<xsl:if use-when="true()" test=". = map:keys($abbreviations.special)"> * </xsl:if></td>-->
                </tr>
              </xsl:for-each>
            </tbody>
          </table>
          <p>These codes can be used irrespective of other control mechanisms: for example using a
            cardinal scheme will override an ordinal request on an <code>xsl:number</code>
            instruction. They are ONLY used for spelled-out numbering of words. Use of an unknown
            ruleset (e.g. spellout-ordinal for Welsh) will raise a warning.</p>
          <p>The available numbering schemes for the {count(map:keys($numberings))} languages
            supported for numbering in ICU version {icuN:getICUVersion()} are given in the following
            table:</p>
          <xsl:variable name="with-abbrev"
            select="function($s) {string-join(($s,' (',$abbreviations($s),')'),'')}"/>
          <table>
            <thead class="params">
              <tr>
                <td colspan="2">Group</td>
                <td colspan="1">Spellout<!-- Numbering--></td>
                <td>Ordinal</td>
              </tr>
              <tr>
                <td>code</td>
                <td>language</td>
                <td>Numbering</td>
                <td>Numbering</td>
              </tr>
            </thead>
            <tbody>
              <xsl:for-each select="map:keys($numberings)">
                <xsl:sort/>
                <xsl:variable name="this" select="$numberings(.)"/>
                <xsl:variable name="base" select="tokenize(.,';')"/>
                <xsl:variable name="spellout" select="$this[starts-with(.,'spellout-')]"/>
                <xsl:variable name="cardinals"
                  select="$spellout[starts-with(.,'spellout-cardinal')]"/>
                <xsl:variable name="ordinals" select="$spellout[starts-with(.,'spellout-ordinal')]"/>
                <xsl:variable name="numbers" select="$spellout[starts-with(.,'spellout-number')]"/>
                <xsl:variable name="ordinal.numberings" select="$this[starts-with(.,'digits')]"/>
                <xsl:variable name="other-rules"
                  select="$this[not(. = ($spellout,$ordinal.numberings))]"/>
                <tr>
                  <td class="value">{.}</td>
                  <td>{$langs.icuN.all(.)}{if(exists($other-rules)) then ' WARNING: other
                    rules:'||string-join($other-rules,',') else ''}</td>
                  <td>
                    <xsl:sequence select="f:br($spellout!f:val-abbrev(.))"/>
                  </td>
                  <td>
                    <xsl:sequence select="f:br($ordinal.numberings!f:val-abbrev(.))"/>
                  </td>
                </tr>
              </xsl:for-each>
            </tbody>
          </table>
        </section>
      </section>
    </xsl:result-document>


  </xsl:template>
</xsl:stylesheet>
