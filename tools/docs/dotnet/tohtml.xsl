<xsl:stylesheet version="3.0" 
   xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
   xmlns:xs="http://www.w3.org/2001/XMLSchema"
   xmlns:f="http://local-functions/"
   xmlns:map="http://www.w3.org/2005/xpath-functions/map"
   exclude-result-prefixes="xs f map">
   
  <xsl:output method="html" indent="yes"/>
  
  <xsl:variable name="root" select="/"/>
  
  <!-- Used for classes which implement an interface, and subclasses of a class -->
  <xsl:key name="class-by-base" match="class | interface | enum | delegate" 
    use="tokenize(@base, ', ', 'q')"/>
  
  <xsl:key name="class-by-name" match="class | interface | enum | delegate" 
    use="if (contains(@name,'.')) then tokenize(@name, '\.')[last()] else @name"/>
  
  <xsl:template match="/">
    <xsl:call-template name="buildIndexPage"/>
    <xsl:apply-templates select="package/*"/>
  </xsl:template>
  
  <xsl:template match="class | interface | enum | delegate">
    <xsl:variable name="shortname" select="@name"/>
    <xsl:variable name="fullname" select="if (self::delegate) then @fullname else @fulltype"/>
    <xsl:variable name="is-nested" select="contains(@name, '.')"/>
    <xsl:variable name="depth" select="if ($is-nested) then 2 else 1"/>
    <xsl:variable name="file-ext" select="translate(@name, '.','/')"/>
    <xsl:variable name="css-location" select="if (not($is-nested)) then
      '../../saxon-dotnetdoc.css' else '../../../saxon-dotnetdoc.css'"/>
    <xsl:result-document href="Saxon/Api/{$file-ext}.html">
      <html>
        <head>
          <title><xsl:value-of select="$shortname"/></title>
          <meta name="keywords" content="{$fullname} {name()}"/>
          <link rel="stylesheet" type="text/css" href="{$css-location}" title="Style"/>
        </head>
        <body>
          <div class="contents">
            <xsl:call-template name="contents">
              <xsl:with-param name="depth" select="$depth"/>
            </xsl:call-template>
          </div>
          <div class="margin">
            &#xa0;
          </div>
          <div class="main">
            <h3>Saxon.Api</h3>
            <h1>
              <span class="classLabel"><xsl:value-of select="f:titleCase(name())"/>&#xa0;</span><xsl:value-of select="concat($shortname, f:flatTypeParams(.))"/>
            </h1>
      
            <xsl:if test="self::interface and exists(key('class-by-base', ./@name, $root))">
              <dl>
                <dt>All Known Implementing Classes</dt>
                <dd>
                  <xsl:call-template name="class-by-base">
                    <xsl:with-param name="class-name" select="@name"/>
                    <xsl:with-param name="depth" select="$depth"/>
                  </xsl:call-template>
                </dd>
              </dl>
            </xsl:if>
      
            <xsl:if test="$is-nested">
              <dl>
                <dt>Enclosing class</dt>
                <dd>
                  <xsl:sequence select="f:classLink(concat('Saxon.Api.', substring-before($shortname, '.')), $depth)"/>
                </dd>
              </dl>
            </xsl:if>
            
            <xsl:if test="self::class and exists(key('class-by-base', ./@name, $root))">
              <dl>
                <dt>Direct Known Subclasses</dt>
                <dd>
                  <xsl:call-template name="class-by-base">
                    <xsl:with-param name="class-name" select="@name"/>
                    <xsl:with-param name="depth" select="$depth"/>
                  </xsl:call-template>
                </dd>
              </dl>
            </xsl:if>
            
            <hr/>
            
            <dl>
              <dt>
                <div class="code">
                  <xsl:if test="@public = 'true'">public </xsl:if>
                  <xsl:if test="@public = 'false'">protected </xsl:if>
                  <xsl:if test="@abstract = 'true'">abstract </xsl:if>
                  <xsl:value-of select="name()"/>
                  <xsl:text>&#xa0;</xsl:text>
                  <xsl:choose>
                    <xsl:when test="self::delegate">
                      <xsl:sequence select="f:showGenericType(., $depth)"/>
                      <xsl:text>&#xa0;</xsl:text>
                      <span class="signature">
                        <xsl:value-of select="@name"/>
                        <xsl:text>(</xsl:text>
                        <xsl:for-each select="param">
                          <xsl:variable name="position" select="position()"/>
                          <xsl:sequence select="f:showGenericType(., $depth)"/>
                          <xsl:text>&#xa0;</xsl:text>
                          <xsl:variable name="paramName" select="../comments/param[$position]/@name"/>
                          <xsl:if test="not($paramName)">
                            <xsl:message>Missing parameter description in <xsl:value-of select="../../@name, ../@name"/></xsl:message>
                          </xsl:if>
                          <xsl:value-of select="($paramName, @name)[1]"/>
                          <xsl:if test="position() ne last()"><xsl:text>, </xsl:text></xsl:if>
                        </xsl:for-each>
                        <xsl:text>)</xsl:text>
                      </span>
                    </xsl:when>
                    <xsl:otherwise><xsl:value-of select="concat(@name, f:flatTypeParams(.))"/></xsl:otherwise>
                  </xsl:choose>
                </div>
              </dt>
              <xsl:apply-templates select="extends">
                <xsl:with-param name="depth" select="$depth"/>
              </xsl:apply-templates>
              <xsl:apply-templates select="implements">
                <xsl:with-param name="depth" select="$depth"/>
              </xsl:apply-templates>
            </dl>
            
            <xsl:apply-templates select="comments">
              <xsl:with-param name="is-nested" select="$is-nested" tunnel="yes"/>
            </xsl:apply-templates>
      
            <xsl:if test="class">
              <a name="subclasses"/>
              
              <table>
                <tr class="TableHeading">
                  <th colspan="2"><h3>Nested Classes</h3></th>
                </tr>
                <xsl:for-each select="class">
                  <xsl:sort select="lower-case(@name)"/>
                  <tr class="TableRow">
                    <td class="col1">class</td>
                    <td>
                      <a href="{concat(translate(@name, '.','/'),'.html')}" class="apilink"><xsl:value-of select="@name"/></a>
                      <xsl:apply-templates select="(comments/obsolete, comments/summary)[1]" mode="comments">
                        <xsl:with-param name="is-nested" select="$is-nested"/>
                      </xsl:apply-templates>
                    </td>
                  </tr>
                </xsl:for-each>
              </table>
              <p><xsl:text>&#xa0;</xsl:text></p>
            </xsl:if>
        
            <xsl:if test="field[@public='true']">
              <a name="field_summary"/>
              
              <table>
                <tr class="TableHeading">
                  <th colspan="2"><h3>Field Summary</h3></th>
                </tr>
                <xsl:for-each select="field[@public='true']">
                  <xsl:sort select="lower-case(@name)"/>
                  <tr class="TableRow">
                    <td class="col1">
                      <xsl:if test="@static='true'">static</xsl:if>
                      <xsl:text>&#xa0;</xsl:text>
                      <xsl:sequence select="f:showGenericType(., $depth)"/>
                    </td>
                    <td>
                      <span class="signature"><a href="#{@name}" class="apilink"><xsl:value-of select="@name"/></a></span>
                      <div class="fnormal"><xsl:apply-templates select="(comments/obsolete, comments/summary)[1]" mode="comments">
                        <xsl:with-param name="is-nested" select="$is-nested" tunnel="yes"/>
                      </xsl:apply-templates></div>
                    </td>
                  </tr>
                </xsl:for-each>
              </table>
              <p><xsl:text>&#xa0;</xsl:text></p>
            </xsl:if>
    
            <xsl:if test="constructor[@public='true']">
              <a name="constructor_summary"/>
              
              <table>
                <tr class="TableHeading">
                  <th colspan="2"><h3>Constructor Summary</h3></th>
                </tr>
                <xsl:for-each select="constructor[@public='true']">
                  <xsl:sort select="lower-case(@name)"/>
                  <tr class="TableRow">
                    <td>
                      <span class="signature">
                        <a href="#{f:makeMethodAnchor(.)}" class="apilink"><xsl:value-of select="@name"/></a>
                        <xsl:text> (</xsl:text>
                        <xsl:for-each select="param">
                          <xsl:variable name="position" select="position()"/>
                          <xsl:sequence select="f:showGenericType(., $depth)"/>
                          <xsl:text>&#xa0;</xsl:text>
                          <xsl:variable name="paramName" select="../comments/param[$position]/@name"/>
                          <xsl:if test="not($paramName)">
                            <xsl:message>Missing parameter description in <xsl:value-of select="../../@name, ../@name"/></xsl:message>
                          </xsl:if>
                          <xsl:value-of select="$paramName"/>
                          <xsl:if test="position() ne last()"><xsl:text>, </xsl:text></xsl:if>
                        </xsl:for-each>
                        <xsl:text>)</xsl:text>
                      </span>
                      <div class="fnormal"><xsl:apply-templates select="(comments/obsolete, comments/summary)[1]" mode="comments">
                        <xsl:with-param name="is-nested" select="$is-nested" tunnel="yes"/>
                      </xsl:apply-templates></div>
                    </td>
                  </tr>
                </xsl:for-each>
              </table>
              <p><xsl:text>&#xa0;</xsl:text></p>
            </xsl:if>  
    
            <xsl:if test="property[@public='true']">
              <a name="property_summary"/>
              
              <table>
                <tr class="TableHeading">
                  <th colspan="2"><h3>Property Summary</h3></th>
                </tr>
                <xsl:for-each select="property[@public='true']">
                  <xsl:sort select="lower-case(@name)"/>
                  <tr class="TableRow">
                    <td class="col1">
                      <xsl:if test="@static='true'">static</xsl:if>
                      <xsl:text>&#xa0;</xsl:text>
                      <xsl:sequence select="f:showGenericType(., $depth)"/>
                    </td>
                    <td>
                      <span class="signature">
                        <a href="#{@name}" class="apilink"><xsl:value-of select="@name"/></a>
                      </span>
                      <div class="fnormal"><xsl:apply-templates select="(comments/obsolete, comments/summary)[1]" mode="comments">
                        <xsl:with-param name="is-nested" select="$is-nested" tunnel="yes"/>
                      </xsl:apply-templates></div>
                    </td>
                  </tr>
                </xsl:for-each>
              </table>
              <p><xsl:text>&#xa0;</xsl:text></p>
            </xsl:if>
            
            <xsl:if test="indexer[@public='true']">
              <a name="indexer_summary"/>
              
              <table>
                <tr class="TableHeading">
                  <th colspan="2"><h3>Indexer Summary</h3></th>
                </tr>
                <xsl:for-each select="indexer[@public='true']">
                  <xsl:sort select="lower-case(@name)"/>
                  <tr class="TableRow">
                    <td class="col1">
                      <xsl:if test="@static='true'">static</xsl:if>
                      <xsl:text>&#xa0;</xsl:text>
                      <xsl:sequence select="f:showGenericType(., $depth)"/>
                    </td>
                    <td>
                      <span class="signature">
                        <a href="#{@name}" class="apilink"><xsl:value-of select="@name"/></a>
                        <xsl:text> [</xsl:text>
                        <xsl:for-each select="param">
                          <xsl:variable name="position" select="position()"/>
                          <xsl:sequence select="f:showGenericType(., $depth)"/>
                          <xsl:text>&#xa0;</xsl:text>
                          <xsl:variable name="paramName" select="../comments/param[$position]/@name"/>
                          <xsl:if test="not($paramName)">
                            <xsl:message>Missing parameter description in <xsl:value-of select="../../@name, ../@name"/></xsl:message>
                          </xsl:if>
                          <xsl:value-of select="($paramName, @name)[1]"/>
                          <xsl:if test="position() ne last()"><xsl:text>, </xsl:text></xsl:if>
                        </xsl:for-each>
                        <xsl:text>]</xsl:text>
                      </span>
                      <div class="fnormal"><xsl:apply-templates select="(comments/obsolete, comments/summary)[1]" mode="comments">
                        <xsl:with-param name="is-nested" select="$is-nested" tunnel="yes"/>
                      </xsl:apply-templates></div>
                    </td>
                  </tr>
                </xsl:for-each>
              </table>
              <p><xsl:text>&#xa0;</xsl:text></p>
            </xsl:if>
    
            <xsl:if test="method[@public='true']">
              <a name="method_summary"/>
              
              <table>
                <tr class="TableHeading">
                  <th colspan="2"><h3>Method Summary</h3></th>
                </tr>
                <xsl:for-each select="method[@public='true']">
                  <xsl:sort select="lower-case(@name)"/>
                  <tr class="TableRow">
                    <td class="col1">
                      <xsl:if test="@static='true'">static</xsl:if>
                      <xsl:text>&#xa0;</xsl:text>
                      <xsl:if test="paramtypes">
                        <xsl:sequence select="f:showParamTypes(., $depth)"/>
                        <xsl:text>&#xa0;</xsl:text>
                      </xsl:if>
                      <xsl:sequence select="f:showGenericType(., $depth)"/>
                    </td>
                    <td>
                      <span class="signature">
                        <a href="#{f:makeMethodAnchor(.)}" class="apilink"><xsl:value-of select="@name"/></a>
                        <xsl:text> (</xsl:text>
                        <xsl:for-each select="param">
                          <xsl:variable name="position" select="position()"/>
                          <xsl:sequence select="f:showGenericType(., $depth)"/>
                          <xsl:text>&#xa0;</xsl:text>
                          <xsl:variable name="paramName" select="../comments/param[$position]/@name"/>
                          <xsl:if test="not($paramName)">
                            <xsl:message>Missing parameter description in <xsl:value-of select="../../@name, ../@name"/></xsl:message>
                          </xsl:if>
                          <xsl:value-of select="($paramName, @name)[1]"/>
                          <xsl:if test="position() ne last()"><xsl:text>, </xsl:text></xsl:if>
                        </xsl:for-each>
                        <xsl:text>)</xsl:text>
                      </span>
                      <div class="fnormal"><xsl:apply-templates select="(comments/obsolete, comments/summary)[1]" mode="comments">
                        <xsl:with-param name="is-nested" select="$is-nested" tunnel="yes"/>
                      </xsl:apply-templates></div>
                    </td>
                  </tr>
                </xsl:for-each>
              </table>
              <p><xsl:text>&#xa0;</xsl:text></p>
            </xsl:if>
    
            <xsl:if test="field[@public='true']">
              <a name="field_detail"/>
              
              <div class="section">
                <h2>Field Detail</h2>
                
                <xsl:for-each select="field[@public='true']">
                  <xsl:sort select="lower-case(@name)"/>
                  <a name="{@name}"/>
                  <div class="member">
                    <h3><xsl:value-of select="@name"/></h3>
                    <div class="code">
                      <xsl:if test="@public='true'">public&#xa0;</xsl:if>
                      <xsl:if test="@static='true'">static&#xa0;</xsl:if>
                      <xsl:if test="@readonly='true'">readonly&#xa0;</xsl:if>
                      <xsl:sequence select="f:showGenericType(., $depth)"/>
                      <xsl:text>&#xa0;</xsl:text>
                      <xsl:value-of select="@name"/>
                    </div>
                    <xsl:apply-templates select="comments">
                      <xsl:with-param name="is-nested" select="$is-nested" tunnel="yes"/>
                    </xsl:apply-templates>
                  </div>
                </xsl:for-each>
              </div>
              
            </xsl:if>
    
            <xsl:if test="constructor[@public='true']">
              <a name="constructor_detail"/>
              
              <div class="section">
                <h2>Constructor Detail</h2>
                
                <xsl:for-each select="constructor[@public='true']">
                  <xsl:sort select="lower-case(@name)"/>
                  <a name="{f:makeMethodAnchor(.)}"/>
                  <div class="member">
                    <h3><xsl:value-of select="@name"/></h3>
                    <div class="code">
                      <xsl:if test="@public='true'">public&#xa0;</xsl:if>
                      <xsl:value-of select="@name"/>  
                      <xsl:text>(</xsl:text>
                      <xsl:for-each select="param">
                        <xsl:variable name="position" select="position()"/>
                        <xsl:sequence select="f:showGenericType(., $depth)"/>
                        <xsl:text>&#xa0;</xsl:text>
                        <xsl:value-of select="../comments/param[$position]/@name"/>
                        <xsl:if test="position() ne last()">
                          <xsl:text>,</xsl:text>
                          <br/>
                          <xsl:for-each select="1 to (8 + string-length(../@name))">&#xa0;</xsl:for-each>
                        </xsl:if>
                      </xsl:for-each>
                      <xsl:text>)</xsl:text>  
                    </div>
                    <xsl:apply-templates select="comments">
                      <xsl:with-param name="is-nested" select="$is-nested" tunnel="yes"/>
                    </xsl:apply-templates>
                  </div>
                </xsl:for-each>
              </div>
              
            </xsl:if>
    
            <xsl:if test="property[@public='true']">
              <a name="property_detail"/>
              
              <div class="section">
                <h2>Property Detail</h2>
                
                <xsl:for-each select="property[@public='true']">
                  <xsl:sort select="lower-case(@name)"/>
                  <a name="{@name}"/>
                  <div class="member">
                    <h3><xsl:value-of select="@name"/></h3>
                    <div class="code">
                      <xsl:if test="@public='true'">public&#xa0;</xsl:if>
                      <xsl:if test="@static='true'">static&#xa0;</xsl:if>
                      <xsl:if test="@abstract='true'">abstract&#xa0;</xsl:if>
                      <xsl:if test="@readonly='true'">readonly&#xa0;</xsl:if>
                      <xsl:if test="@virtual='true'">virtual&#xa0;</xsl:if>
                      <xsl:if test="@override='true'">override&#xa0;</xsl:if>
                      <xsl:sequence select="f:showGenericType(., $depth)"/>
                      <xsl:text>&#xa0;</xsl:text>
                      <xsl:value-of select="@name"/>
                      <xsl:text>&#xa0;{</xsl:text>
                      <xsl:if test="@get='true'">get; </xsl:if>
                      <xsl:if test="@set='true'">set; </xsl:if>
                      <xsl:if test="@init='true'">init; </xsl:if>
                      <xsl:text>}</xsl:text>
                    </div>
                    <xsl:apply-templates select="comments">
                      <xsl:with-param name="is-nested" select="$is-nested" tunnel="yes"/>
                    </xsl:apply-templates>
                  </div>
                </xsl:for-each>
              </div>
              
            </xsl:if>
            
            <xsl:if test="indexer[@public='true']">
              <a name="indexer_detail"/>
              
              <div class="section">
                <h2>Indexer Detail</h2>
                
                <xsl:for-each select="indexer[@public='true']">
                  <xsl:sort select="lower-case(@name)"/>
                  <a name="{@name}"/>
                  <div class="member">
                    <h3><xsl:value-of select="@name"/></h3>
                    <div class="code">
                      <xsl:if test="@public='true'">public&#xa0;</xsl:if>
                      <xsl:if test="@static='true'">static&#xa0;</xsl:if>
                      <xsl:if test="@abstract='true'">abstract&#xa0;</xsl:if>
                      <xsl:if test="@readonly='true'">readonly&#xa0;</xsl:if>
                      <xsl:if test="@virtual='true'">virtual&#xa0;</xsl:if>
                      <xsl:if test="@override='true'">override&#xa0;</xsl:if>
                      <xsl:sequence select="f:showGenericType(., $depth)"/>
                      <xsl:text>&#xa0;</xsl:text>
                      <xsl:value-of select="'this'"/>
                      <xsl:text>[</xsl:text>
                      <xsl:for-each select="param">
                        <xsl:variable name="position" select="position()"/>
                        <xsl:sequence select="f:showGenericType(., $depth)"/>
                        <xsl:text>&#xa0;</xsl:text>
                        <xsl:value-of select="../comments/param[$position]/@name"/>
                        <xsl:if test="position() ne last()">
                          <xsl:text>,</xsl:text>
                          <br/>
                          <xsl:for-each select="1 to (8 + string-length(../@name))">&#xa0;</xsl:for-each>
                        </xsl:if>
                      </xsl:for-each>
                      <xsl:text>]</xsl:text>
                    </div>
                    <xsl:apply-templates select="comments">
                      <xsl:with-param name="is-nested" select="$is-nested" tunnel="yes"/>
                    </xsl:apply-templates>
                  </div>
                </xsl:for-each>
              </div>
              
            </xsl:if>
    
            <xsl:if test="method[@public='true']">
              <a name="method_detail"/>
              
              <div class="section">
                <h2>Method Detail</h2>
                
                <xsl:for-each select="method[@public='true']">
                  <xsl:sort select="lower-case(@name)"/>
                  <a name="{f:makeMethodAnchor(.)}"/>
                  <div class="member">
                    <h3><xsl:value-of select="@name"/></h3>
                    <div class="fullsignature">
                      <span class="fblock">
                        <xsl:if test="@public='true'">public&#xa0;</xsl:if>
                        <xsl:if test="@abstract='true'">abstract&#xa0;</xsl:if>
                        <xsl:if test="@static='true'">static&#xa0;</xsl:if>
                        <xsl:if test="@virtual='true'">virtual&#xa0;</xsl:if>
                        <xsl:if test="@override='true'">override&#xa0;</xsl:if>
                        <xsl:sequence select="f:showGenericType(., $depth)"/>
                        <xsl:text>&#xa0;</xsl:text>
                        <xsl:value-of select="@name"/>
                        <xsl:if test="paramtypes">
                          <xsl:sequence select="f:showParamTypes(., $depth)"/>
                        </xsl:if>
                      </span>
                      <span class="fblock">
                        <xsl:text>(</xsl:text>
                        <xsl:for-each select="param">
                          <xsl:variable name="position" select="position()"/>
                          <xsl:sequence select="f:showGenericType(., $depth)"/>
                          <xsl:text>&#xa0;</xsl:text>
                          <xsl:variable name="paramName" select="../comments/param[$position]/@name"/>
                          <xsl:value-of select="($paramName, @name)[1]"/>
                          <xsl:if test="position() ne last()"> 
                            <xsl:text>, </xsl:text>
                            <br/>
                          </xsl:if>
                        </xsl:for-each>
                        <xsl:text>)</xsl:text>    
                        <xsl:if test="comments/exception">
                          <br/>
                          <xsl:text>throws</xsl:text>
                          <br/>
                          <xsl:for-each select="comments/exception">
                            <xsl:sequence select="f:classLink(@name, $depth)"/>
                            <xsl:if test="position() ne last()"> 
                              <xsl:text>, </xsl:text>
                              <br/>
                            </xsl:if>
                          </xsl:for-each>
                        </xsl:if>
                      </span>
                    </div>
                    <div class="fcomment">
                      <xsl:apply-templates select="comments">
                        <xsl:with-param name="is-nested" select="$is-nested" tunnel="yes"/>
                      </xsl:apply-templates>
                    </div>
                  </div>
                </xsl:for-each>
              </div>
              
            </xsl:if>
            
            <p><xsl:text>&#xa0;</xsl:text></p>
          </div>
          <div class="margin">
            &#xa0;
          </div>
        </body>
      </html>
      
    </xsl:result-document>
    <xsl:apply-templates select="class"/>
  </xsl:template>
  
  <xsl:template match="extends">
    <xsl:param name="depth" as="xs:integer"/>
    <dt> extends <xsl:sequence select="f:showGenericType(interface, $depth)"/></dt>
  </xsl:template>
  
  <xsl:template match="implements">
    <xsl:param name="depth" as="xs:integer"/>
    <dt>
      <xsl:text> implements </xsl:text>
      <xsl:for-each select="interface">
        <xsl:sequence select="f:showGenericType(., $depth)"/>
        <xsl:if test="position() ne last()">, </xsl:if>
      </xsl:for-each>
    </dt>
  </xsl:template>
  
  <xsl:template name="class-by-base">
    <xsl:param name="class-name" as="xs:string"/>
    <xsl:param name="depth" as="xs:integer"/>
    <xsl:for-each select="key('class-by-base', $class-name, $root)">
      <xsl:sort select="lower-case(@name)"/>
      <xsl:sequence select="f:classLink(if (self::delegate) then @fullname else @fulltype, $depth)"/>
      <xsl:if test="position() ne last()"><xsl:text>, </xsl:text></xsl:if>
    </xsl:for-each>
  </xsl:template>
  
  <xsl:function name="f:titleCase" as="xs:string">
    <xsl:param name="in" as="xs:string"/>
    <xsl:sequence select="concat(upper-case(substring($in,1,1)), substring($in,2))"/>
  </xsl:function>
  
  <xsl:function name="f:classLink" as="node()">
    <xsl:param name="in" as="xs:string?"/>
    <xsl:param name="depth" as="xs:integer"/><!-- Current depth of page: 0 = package level, 1 = member, 2 = nested class -->
    <xsl:variable name="prefix-to-package" as="xs:string" 
      select="if ($depth = 2) then '../../' else if ($depth = 1) then '../' else ''"/>
    <xsl:variable name="prefix-to-member" as="xs:string" 
      select="if ($depth = 2) then '../' else if ($depth = 1) then '' else 'Saxon/Api/'"/>
    <xsl:variable name="isArray" as="xs:boolean" select="ends-with($in, '[]')"/>
    <xsl:variable name="in2" select="if ($isArray) then translate($in, '[]','') else $in"/>
    <xsl:variable name="end" select="if ($isArray) then '[]' else ''"/>
    <xsl:choose>
      <xsl:when test="starts-with($in, 'net.sf.saxon.') or starts-with($in, 'com.saxonica.')">
        <xsl:choose>
          <xsl:when test="contains($in, '#')">
            <a href="{$prefix-to-package}../../javadoc/{translate(substring-before($in2, '#'), '.',
              '/')}.html#{substring-after($in2, '#')}" class="apilink java"><xsl:value-of select="$in"/></a>
          </xsl:when>
          <xsl:otherwise>
            <a href="{$prefix-to-package}../../javadoc/{translate($in2, '.', '/')}.html" class="apilink java"><xsl:value-of select="$in"/></a>
          </xsl:otherwise>
        </xsl:choose>
      </xsl:when>
      <xsl:when test="starts-with($in, 'Saxon.Api.')">
        <xsl:variable name="short" select="substring-after($in2, 'Saxon.Api.')"/>
        <xsl:variable name="short" select="if (contains($short, '.')) then substring-after($short, '.') else $short"/>
        <xsl:variable name="className" select="if (contains($short, '#')) then substring-before($short, '#') else $short"/>
        <xsl:variable name="class" select="key('class-by-name', $className, $root)"/>
        <xsl:choose>
          <!-- Note: translate on $class/@name required for nested classes -->
          <xsl:when test="exists($class) and not(contains($short, '#'))">
            <a href="{concat($prefix-to-member, translate($class/@name, '.', '/'))}.html" 
              class="apilink"><xsl:value-of select="concat($class/@name,$end)"/></a>
          </xsl:when>
          <xsl:when test="exists($class)">
            <a href="{concat($prefix-to-member, translate($class/@name, '.', '/'))}.html#{substring-after($short, '#')}" 
              class="apilink">
              <xsl:value-of select="concat($class/@name, '.', substring-after($short, '#'), $end)"/>
            </a>
          </xsl:when>
          <xsl:otherwise>
            <xsl:message>Saxon.Api type not found <xsl:value-of select="$in"/></xsl:message>
            <xsl:value-of select="$in"/>
          </xsl:otherwise>
        </xsl:choose>
      </xsl:when>
      <xsl:when test="starts-with($in, 'System.')">
        <xsl:sequence select="f:makeSystemLink($in)"/>
      </xsl:when>
      <xsl:otherwise>
        <xsl:variable name="class" select="key('class-by-name', $in2, $root)"/>
        <xsl:choose>
          <xsl:when test="$class">
            <a href="{$prefix-to-member}{translate($class/@name, '.', '/')}.html" 
              class="apilink"><xsl:value-of select="concat($class/@name, $end)"/></a>
          </xsl:when>
          <xsl:otherwise>
            <xsl:if test="exists($in) and not($in = ('void', '', 'T', 'TInput', 'TResult', 'T1')) and not(starts-with($in, 'java.') or starts-with($in, 'javax.'))">
              <xsl:message>type not recognised <xsl:value-of select="$in"/></xsl:message>
            </xsl:if>
            <xsl:value-of select="$in"/>
          </xsl:otherwise>
        </xsl:choose>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:function>
  
  <xsl:function name="f:makeMethodAnchor" as="xs:string">
    <xsl:param name="in" as="element()"/>
    <xsl:value-of>
      <xsl:value-of select="$in/@name"/>
      <xsl:if test="$in/self::method or $in/self::constructor">    
        <xsl:text>(</xsl:text>
        <xsl:for-each select="$in/param">
          <xsl:value-of select="f:shortName(@type)"/><!-- Only use local class name for param -->
          <xsl:if test="position() ne last()"><xsl:text>,</xsl:text></xsl:if>
        </xsl:for-each>
        <xsl:text>)</xsl:text> 
      </xsl:if>
    </xsl:value-of>
  </xsl:function>
  
  <!-- Function f:showGenericType is similar to f:showGenericType from jdc-body.xsl
    to include type parameters (rather than just using f:classLink which omits these) -->
  
  <xsl:function name="f:showGenericType" as="node()*">
    <xsl:param name="member" as="node()"/>
    <!-- $member may be field, param, type, property, method, extends/interface or implements/interface -->
    <xsl:param name="depth" as="xs:integer"/>
    <xsl:for-each select="$member">
      <xsl:if test="@extendby">
        <xsl:value-of select="'out '"/>
      </xsl:if>
      <xsl:if test="@super">
        <xsl:value-of select="'in '"/>
      </xsl:if>
      <xsl:if test="@fulltype|@type">
        <xsl:sequence select="f:classLink((@fulltype,@type)[1], $depth)"/>
      </xsl:if>
      <xsl:if test="exists(type)">
        <span class="operator">
          <xsl:text>&lt;</xsl:text>
          <xsl:for-each select="type">
            <xsl:sequence select="f:showGenericType(., $depth)"/>
            <xsl:if test="position() ne last()"><xsl:text>,&#xa0;</xsl:text></xsl:if>
          </xsl:for-each>
          <xsl:text>&gt;</xsl:text>
        </span>
      </xsl:if>
    </xsl:for-each>
  </xsl:function>
  
  <xsl:function name="f:showParamTypes" as="element()*">
    <!-- Now only called for generic methods, i.e. after checking for paramtypes element -->
    <xsl:param name="method"/>
    <xsl:param name="depth" as="xs:integer"/>
    <xsl:for-each select="$method/paramtypes">
      <span class="operator">
        <xsl:text>&lt;</xsl:text>
        <xsl:for-each select="type"><!-- in merged.xml, paramtypes/type element only has @name, not @fulltype -->
          <xsl:sequence select="f:classLink(@name, $depth)"/>
          <!-- Note type/bounds not actually used yet -->
          <!--<xsl:if test="bounds">
            <xsl:text> extends </xsl:text>
            <xsl:for-each select="bounds/limit">
              <xsl:sequence select="f:showGenericType(., $depth)"/>
            </xsl:for-each>
          </xsl:if>-->
          <xsl:if test="position() ne last()"><xsl:text>,&#xa0;</xsl:text></xsl:if>
        </xsl:for-each>
        <xsl:text>&gt;</xsl:text>
      </span>
    </xsl:for-each>
  </xsl:function>
  
  <xsl:function name="f:flatTypeParams" as="xs:string?">
    <xsl:param name="member" as="node()"/>
    <xsl:variable name="flatTypeParams" as="xs:string*">
      <xsl:for-each select="$member">
        <xsl:if test="@extendby">
          <xsl:value-of select="'out '"/>
        </xsl:if>
        <xsl:if test="@super">
          <xsl:value-of select="'in '"/>
        </xsl:if>
        <xsl:if test="@type and (local-name() ne 'delegate')">
          <xsl:value-of select="f:shortName(@type)"/>
        </xsl:if>
        <xsl:if test="exists(type) and (local-name() ne 'delegate')">
          <xsl:value-of select="'&lt;'"/>
          <xsl:for-each select="type">
            <xsl:value-of select="f:flatTypeParams(.)"/>
            <xsl:if test="position() ne last()">, </xsl:if>
          </xsl:for-each>
          <xsl:value-of select="'&gt;'"/>
        </xsl:if>
      </xsl:for-each>
    </xsl:variable>
    <xsl:value-of select="string-join($flatTypeParams)"/>
  </xsl:function>
  
  <xsl:template match="comments">
    <xsl:apply-templates select="obsolete, summary, remarks" mode="comments"/>
    <xsl:if test="param">
      <p class="methodLabel">Parameters:</p>
      <xsl:apply-templates select="param" mode="comments"/>
    </xsl:if>
    <xsl:apply-templates select="returns, value" mode="comments"/>
    <xsl:if test="exception">
      <p class="methodLabel">Throws:</p>
      <xsl:apply-templates select="exception" mode="comments"/>
    </xsl:if>
    <xsl:for-each select="example">
      <p class="methodLabel">Example:</p>
      <samp>
        <xsl:apply-templates select="code/text()"/>
      </samp>
    </xsl:for-each>
  </xsl:template>
  
  <xsl:template match="*" mode="comments">
    <xsl:copy>
      <xsl:copy-of select="@*"/>
      <xsl:apply-templates mode="comments">
      </xsl:apply-templates>
    </xsl:copy>
  </xsl:template>
  
  <xsl:template match="summary[para] | remarks[para]" mode="comments table-summary">
    <xsl:apply-templates mode="#current"/>
  </xsl:template>
  
  <xsl:template match="obsolete" mode="comments table-summary">
    <p>
      Deprecated <xsl:apply-templates mode="#current"/>
    </p>
  </xsl:template>
  
  <xsl:template match="summary | remarks | para" mode="comments table-summary">
    <p>
      <xsl:apply-templates mode="#current"/>
    </p>
  </xsl:template>
  
  <xsl:template match="c" mode="comments table-summary">
    <code>
      <xsl:apply-templates mode="#current"/>
    </code>
  </xsl:template>
  
  <xsl:template match="list" mode="comments table-summary">
    <ul>
      <xsl:apply-templates mode="#current"/>
    </ul>
  </xsl:template>
  
  <xsl:template match="list[@type='number']" mode="comments table-summary">
    <ol>
      <xsl:apply-templates mode="#current"/>
    </ol>
  </xsl:template>
  
  <xsl:template match="item" mode="comments table-summary">
    <li>
      <xsl:apply-templates mode="#current"/>
    </li>
  </xsl:template>
  
  <xsl:template match="em" mode="comments table-summary">
    <em>
      <xsl:apply-templates mode="#current"/>
    </em>
  </xsl:template>
  
  <xsl:template match="tag[@kind='see']" mode="comments">
    <xsl:param name="is-nested" as="xs:boolean" tunnel="yes"/>
    <xsl:sequence select="f:classLink(., (if ($is-nested) then 2 else 1))"/>
  </xsl:template>
  
  <xsl:template match="tag[@kind='see']" mode="table-summary">
    <xsl:sequence select="f:classLink(., 0)"/>
  </xsl:template>
  
  <xsl:template match="param" mode="comments">
    <div class="params">
      <code><xsl:value-of select="@name"/></code>
      <xsl:if test="node()">
        <xsl:text>&#xa0;-&#xa0;</xsl:text><xsl:apply-templates mode="comments"/>
      </xsl:if>
    </div>
  </xsl:template>
  
  <xsl:template match="exception" mode="comments">
    <xsl:param name="is-nested" as="xs:boolean" tunnel="yes"/>
    <div class="params">
      <xsl:sequence select="f:classLink(@name, (if ($is-nested) then 2 else 1))"/>
      <xsl:if test="node()">
        <xsl:text>&#xa0;-&#xa0;</xsl:text><xsl:apply-templates mode="comments"/>
      </xsl:if>
    </div>
  </xsl:template>
  
  <xsl:template match="returns|value" mode="comments">
    <p class="methodLabel"><xsl:value-of select="if (self::returns) then 'Returns:' else 'Value:'"/></p>
    <div class="params"><xsl:apply-templates mode="comments"/></div>
  </xsl:template>


  <xsl:template name="contents">
    <xsl:param name="depth" as="xs:integer"/>
    <xsl:variable name="dir" select="if ($depth eq 0) then 'Saxon/Api/' else if ($depth eq 2) then '../' else ''"/>
    <h3><a href="{$dir}../../index.html" class="home">Saxon.Api</a></h3>
    
    <ul class="list">
      <xsl:for-each select="/package/*">
        <xsl:sort select="lower-case(@name)"/>
        <li>
          <a href="{$dir}{@name}.html"><xsl:value-of select="concat(@name, f:flatTypeParams(.))"/></a>
          <xsl:if test="class">
            <ul>
              <xsl:for-each select="class">
                <li><xsl:text>&#xa0;&#xa0;-&#xa0;</xsl:text><a href="{$dir}{translate(@name, '.', '/')}.html"><xsl:value-of 
                  select="substring-after(@name, '.')"/></a></li>
              </xsl:for-each>
            </ul>
          </xsl:if>
        </li>
      </xsl:for-each>
    </ul>
    <p><xsl:text>&#xa0;</xsl:text></p>
    
  </xsl:template>
  
  <xsl:template name="buildIndexPage">
    <html>
      <head>
        <title>Saxon.Api - the Saxon Application Programming Interface for .NET</title>
        <meta name="keywords" content="Saxon.Api Saxon .NET"/>
        <link rel="stylesheet" type="text/css" href="saxon-dotnetdoc.css" title="Style"/>
      </head>
      <body>
        <div class="contents">
          <xsl:call-template name="contents">
            <xsl:with-param name="depth" select="0"/>
          </xsl:call-template>
        </div>
        <div class="margin">
          &#xa0;
        </div>
        <div class="main">
          <h1>Saxon.Api - the SaxonCS Application Programming Interface for .NET</h1>
          <p>This namespace contains Saxon's application programming interfaces for using XSLT, XQuery,
            XPath and XSD on .NET.</p>
          <p>The top-level object is the <a href="Saxon/Api/Processor.html" class="apilink">Processor</a>;
            this allows creation of factory classes for using each of the different processing
            languages.</p>
          <p>The classes are all found in the assembly: <code>saxoncs.dll</code>.</p>
          <p>Further background information about this API is available at 
            <a href="https://www.saxonica.com/html/documentation11/saxon-cs/dotnetapi.html"
              >https://www.saxonica.com/html/documentation11/saxon-cs/dotnetapi.html</a></p>
          <!-- Include summary tables for interfaces, delegates, classes, enums, and exceptions -->
          <p><xsl:text>&#xa0;</xsl:text></p>
          <xsl:call-template name="summary-table">
            <xsl:with-param name="member" select="'Interface'"/>
          </xsl:call-template>
          <xsl:call-template name="summary-table">
            <xsl:with-param name="member" select="'Delegate'"/>
          </xsl:call-template>
          <xsl:call-template name="summary-table">
            <xsl:with-param name="member" select="'Class'"/>
            <xsl:with-param name="exception" select="false()"/>
          </xsl:call-template>
          <xsl:call-template name="summary-table">
            <xsl:with-param name="member" select="'Enum'"/>
          </xsl:call-template>
          <xsl:call-template name="summary-table">
            <xsl:with-param name="member" select="'Class'"/>
            <xsl:with-param name="exception" select="true()"/>
          </xsl:call-template>
        </div>
        <div class="margin">
          &#xa0;
        </div>
      </body>
    </html>
  </xsl:template>
  
  <xsl:template name="summary-table">
    <xsl:param name="member" as="xs:string"/>
    <xsl:param name="exception" select="false()"/>
    <table>
      <thead>
        <tr class="TableHeading">
          <th colspan="2"><h3><xsl:value-of select="if ($exception) then 'Exception' else $member"/> Summary</h3></th>
        </tr>
        <tr class="TableHeading">
          <td><b>Name</b></td>
          <td><b>Description</b></td>
        </tr>
      </thead>
      <tbody>
        <xsl:for-each select="$root//*[local-name() eq lower-case($member)][@public='true'][if
          ($exception) then (@base eq 'System.Exception') else true()]">
          <xsl:sort select="lower-case(@name)"/>
          <tr class="TableRow">
            <td>
              <xsl:sequence select="f:classLink(if (self::delegate) then @fullname else @fulltype, 0)"/>
            </td>
            <td>
              <div class="fnormal"><xsl:apply-templates select="comments/summary" mode="table-summary"/></div>
            </td>
          </tr>
        </xsl:for-each>
      </tbody>
    </table>
    <p><xsl:text>&#xa0;</xsl:text></p>
  </xsl:template>
  
  <xsl:function name="f:makeSystemLink" as="node()">
    <xsl:param name="in" as="xs:string"/>
    <xsl:variable name="isArray" as="xs:boolean" select="ends-with($in, '[]')"/>
    <xsl:variable name="in2" select="if ($isArray) then translate($in, '[]','') else $in"/>
    <xsl:variable name="end" select="if ($isArray) then '[]' else ''"/>
    <a href="{concat('http://msdn.microsoft.com/en-us/library/', lower-case($in2), 
      '(VS.80).aspx')}" class="apilink system" target="_blank"><xsl:value-of select="concat(f:shortName($in2), $end)"/></a>
  </xsl:function>
  
  <!-- Get short name for type: use abbreviated name for relevant System types, 
        and nested name for nested classes. -->
  <xsl:function name="f:shortName" as="xs:string">
    <xsl:param name="type" as="xs:string"/>
    <xsl:choose>
      <xsl:when test="not(contains($type, '.'))">
        <xsl:value-of select="$type"/>
      </xsl:when>
      <xsl:when test="starts-with($type, 'System.') and map:contains($typeAbbreviationsMap, $type)">
        <xsl:value-of select="map:get($typeAbbreviationsMap, $type)"/>
      </xsl:when>
      <xsl:when test="starts-with($type, 'System.')">
        <xsl:value-of select="tokenize($type, '\.')[last()]"/>
      </xsl:when>
      <xsl:when test="starts-with($type, 'Saxon.Api.')">
        <xsl:value-of select="substring-after($type, 'Saxon.Api.')"/>
      </xsl:when>
      <xsl:otherwise>
        <!-- For Java types, want the parts of $type beginning with a capital letter -->
        <xsl:value-of select="tokenize($type, '\.')[matches(., '^[A-Z]')] => string-join('.')"/>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:function>
  
  <!-- Get abbreviations for relevant System types in tohtml.xsl and convert-to-jel-package.xsl -->
  <xsl:variable name="typeAbbreviationsMap" as="map(xs:string, xs:string)"
    select="map{
    'System.Boolean':'bool',
    'System.Char':'char',
    'System.Decimal':'decimal',
    'System.Double':'double',
    'System.Single':'float',
    'System.Int32':'int',
    'System.Int64':'long',
    'System.Object':'object',
    'System.String':'string'
    }"/>
      

</xsl:stylesheet>
   
 
