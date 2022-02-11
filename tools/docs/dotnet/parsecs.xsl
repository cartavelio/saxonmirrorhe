<xsl:stylesheet version="3.0" 
   xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
   xmlns:xs="http://www.w3.org/2001/XMLSchema"
   xmlns:f="http://local-functions/"
   exclude-result-prefixes="f xs">
  
  <xsl:output method="xml" indent="yes"/>
  
  <!-- This stylesheet reads the C# source code of the Saxon.Api component and extracts information from
     it that is not provided by the standard api-doc compiler output. This information is then merged
     with the api-doc.xml file to provide the input to the documentation generator -->
  
  <!-- Can use <c> or <code> tags within documentation comments to get code font in documentation. -->

  <xsl:param name="inputdir" required="yes"/>
  <xsl:variable name="files" select="uri-collection($inputdir || '?select=*.cs')"/>
  <!--  'Configuration.cs',
    'Destination.cs',
    'Errors.cs',
    'Extensibility.cs',
    'Feature.cs',
    'FeatureKeys.cs',
    'Model.cs',
    'NamespaceConstant.cs',
    'Schema.cs',
    'Types.cs',
    'XPath.cs',
    'XQuery.cs',
    'Xslt.cs'"/>-->
  
  <xsl:template name="main">
    <xsl:sequence select="f:group(f:parse())"/>
  </xsl:template>
  
  <xsl:function name="f:parse" as="element(package)">  
    <package name="Saxon.Api">
      <xsl:for-each select="$files">
        <xsl:variable name="filename" select="."/>
        <xsl:message select="'Processing ' || $filename"/>
        <xsl:variable name="filecontent" select="unparsed-text($filename)"/>
        <xsl:variable name="filecontent" select="replace($filecontent, '/\*\*public\*\*/\s+', 'public ')"/>
        <xsl:variable name="lines" select="tokenize($filecontent, '\r?\n')"/>
        <xsl:for-each select="$lines">
          <xsl:variable name="line" select="normalize-space(.)"/>
          <xsl:choose>
            <xsl:when test="starts-with($line, 'using')">
              <!-- capture J-names and full types -->
              <xsl:variable name="tokens" select="tokenize(substring-after($line, 'using '), '\s')"/>
              <xsl:variable name="is-J-name" select="contains($line, '=')"/>
              <xsl:if test="$is-J-name">
                <xsl:variable name="J-name" select="$tokens[1]"/>
                <xsl:variable name="fulltype" select="translate($tokens[last()], ';@', '')"/>
                <using class="{$J-name}" fulltype="{$fulltype}"/>
              </xsl:if>
            </xsl:when>
            <xsl:when test="starts-with($line, 'namespace')">
              <!--<namespace name="{normalize-space(substring-after($line, 'namespace'))}"/>-->
            </xsl:when>
            <xsl:when test="$line = 'get' or starts-with($line, 'get {') or starts-with($line, 'get =>')">
              <get/>
            </xsl:when>
            <xsl:when test="$line = 'set' or starts-with($line, 'set {') or starts-with($line, 'set =>')">
              <set/>
            </xsl:when> 
            <xsl:when test="starts-with($line, '[Obsolete(')">
              <!-- If line contains + then go get next line too...  -->
              <xsl:variable name="position-of-first-line" select="position()"/>
              <!--<xsl:message expand-text="true">{$line}</xsl:message>-->
              <xsl:variable name="obsolete-strings" as="xs:string*">
                <xsl:iterate select="subsequence($lines, $position-of-first-line)">
                  <xsl:param name="position" select="$position-of-first-line"/>
                  <xsl:value-of select="substring-after(., '&quot;') => substring-before('&quot;')"/>
                  <xsl:choose>
                    <xsl:when test="ends-with($lines[$position], '+')">
                      <xsl:next-iteration>
                        <xsl:with-param name="position" select="$position +1"/>
                      </xsl:next-iteration>
                    </xsl:when>
                    <xsl:otherwise>
                      <xsl:break/>
                    </xsl:otherwise>
                  </xsl:choose>
                </xsl:iterate>
              </xsl:variable>
              <obsolete><xsl:value-of select="string-join($obsolete-strings, ' ')"/></obsolete>
            </xsl:when>
            <!-- Include internal classes (to later be removed in merge.xsl), so that any public
                methods inside them are not mistakenly attached to the wrong class -->
            <xsl:when test="starts-with($line, 'public') or starts-with($line, 'protected') or
              starts-with($line, 'internal')">
              <xsl:variable name="is-property-with-getter" select="contains($line, '=>')"/>
              <xsl:variable name="line-pre-arrow" 
                select="if ($is-property-with-getter) then normalize-space(substring-before($line, '=>')) else $line"/>
              <xsl:variable name="tokens" select="tokenize($line-pre-arrow, '\s')"/>
              <xsl:variable name="is-class" select="$tokens = 'class'"/>
              <xsl:variable name="is-interface" select="$tokens = 'interface'"/>
              <xsl:variable name="is-delegate" select="$tokens = 'delegate'"/> 
              <xsl:variable name="is-enum" select="$tokens = 'enum'"/>
              <xsl:variable name="is-internal" select="$tokens = 'internal'"/>
              <xsl:variable name="is-public" select="$tokens = 'public'"/>
              <xsl:variable name="is-abstract" select="$tokens = 'abstract'"/>
              <xsl:variable name="is-virtual" select="$tokens = 'virtual'"/>
              <xsl:variable name="is-override" select="$tokens = 'override'"/>
              <xsl:variable name="is-static" select="$tokens = 'static'"/>
              <xsl:variable name="is-readonly" select="$tokens = 'readonly'"/>
              <xsl:variable name="is-initializer" select="contains($line-pre-arrow, '=')"/>
              <xsl:variable name="is-method" select="contains($line-pre-arrow, '(') and not($is-initializer)"/>
              <xsl:variable name="is-indexer" select="$tokens!starts-with(., 'this[') = true()"/>
              
              <xsl:variable name="has-explicit-base" select="($is-enum or $is-class or $is-interface) and contains($line, ':')"/>
              <xsl:variable name="explicitBase" select="if ($has-explicit-base) 
                then normalize-space(translate(substring-after($line, ':'), '{', '')) else ()"/>
              <xsl:variable name="base" select="if ($has-explicit-base) then $explicitBase else 'object'"/>
              <xsl:variable name="line-pre-base" select="if ($has-explicit-base) then substring-before($line, ':') else $line"/>
              
              <xsl:variable name="is-constructor-from-base" select="($is-method) and contains($line-pre-arrow, ':')"/>
              <!-- NOTE: Update the $line-pre-arrow variable for constructors defined from base constructors -->
              <xsl:variable name="line-pre-arrow" select="if ($is-constructor-from-base) then
                normalize-space(substring-before($line-pre-arrow, ':')) else $line-pre-arrow"/>
              
              <xsl:variable name="is-generic-type" select="(not($is-method or $is-initializer) and contains($line-pre-base, '&lt;')) or 
                ($is-method and (substring-before($line-pre-arrow, '(') => tokenize('\s')) => reverse() =>
                tail() => reverse() => string-join(' ') => contains('&gt;')) or 
                ($is-initializer and substring-before($line-pre-arrow, '=') => contains('&lt;'))"/>
              
              <xsl:variable name="is-generic-method" select="$is-method and
                ends-with(normalize-space(substring-before($line-pre-arrow, '(')), '&gt;')"/>
              
              <xsl:variable name="is-method-on-generic-interface-impl" select="$is-method and
                substring-before($line-pre-arrow, '(') => tokenize('\s') => reverse() => head() =>
                contains('&gt;.')"/>
              
              <xsl:variable name="method-input" select="if ($is-method) then
                (normalize-space(substring-before(substring-after($line-pre-arrow, '('),')'))) else ''"/>
              <xsl:variable name="has-args" select="$is-method and ($method-input != '')"/>
              <!-- Can't always just tokenize $method-input on ','. 
                This is wrong when some argument has generic type with more than one type parameter. -->
              <xsl:variable name="method-args" as="xs:string*">
                <xsl:choose>
                  <xsl:when test="$has-args and matches($method-input,
                    '(&lt;[^&gt;]*,)|(&lt;[^&gt;]*&lt;[^&gt;&lt;]*&gt;,)')">
                    <!-- Method has some arg with generic type with more than one type parameter.
                      e.g. public void SetInitialTemplateParameters(Dictionary<QName, XdmValue> parameters, bool tunnel) 
                      public static Step<XdmNode, XdmNode> PathFromList(List<Step<XdmNode, XdmNode>> steps)
                      Predicates.Every  (Step<TInput, TResult> step, IPredicate<TResult> condition)
                      First collapse spaces between &lt; and &gt; before tokenizing on ', '. -->
                    <!-- This also allows for handling nested generic types
                      e.g. public Step(Func<TInput, IEnumerable<TResult>> f)
                      handling if the nested generic is the first type param or not. -->
                    <!-- And should not catch the case when there is more than one arg with generic type arg
                    e.g. public static IEnumerable<TInput> Concat<TInput>(IEnumerable<TInput> first, IEnumerable<TInput> second) -->
                    <!--<xsl:message expand-text="yes">generic+ arg $method-input {$method-input}</xsl:message>-->
                    <xsl:variable name="collapse-space-seq" as="xs:string*">
                      <xsl:analyze-string select="$method-input"
                        regex="{'&lt;([^&gt;]*|([^&gt;]*&lt;[^&gt;]*&gt;[^&gt;&lt;]*)),([^&lt;]*|([^&gt;]*&lt;[^&gt;]*&gt;[^&gt;&lt;]*))&gt;'}">
                        <xsl:matching-substring>
                          <!--<xsl:message expand-text="yes">matching-substring {.}</xsl:message>-->
                          <xsl:value-of select="translate(., ' ', '')"/>
                        </xsl:matching-substring>
                        <xsl:non-matching-substring>
                          <xsl:value-of select="."/>
                        </xsl:non-matching-substring>
                      </xsl:analyze-string>
                    </xsl:variable>
                    <xsl:variable name="tokens" select="tokenize(string-join($collapse-space-seq), ', ')"/>
                    <!--<xsl:message expand-text="yes">$method-args {$tokens}</xsl:message>-->
                    <xsl:sequence select="$tokens"/>
                  </xsl:when>
                  <xsl:when test="$has-args">
                    <xsl:sequence select="tokenize($method-input, ',')"/>
                  </xsl:when>
                  <xsl:otherwise>
                    <xsl:value-of select="''"/>
                  </xsl:otherwise>
                </xsl:choose>
              </xsl:variable>
              
              <xsl:variable name="name">
                <xsl:choose>
                  <xsl:when test="$has-explicit-base and not($is-generic-type)">
                    <xsl:choose>
                      <xsl:when test="$tokens = ':'">
                        <xsl:sequence select="$tokens[index-of($tokens, ':')-1]"/>
                      </xsl:when>
                      <xsl:otherwise>
                        <xsl:variable name="nameToken" select="$tokens[contains(.,':')]"/>
                        <xsl:sequence select="substring-before($nameToken,':')"/>
                      </xsl:otherwise>
                    </xsl:choose>
                  </xsl:when>
                  <xsl:when test="$is-method-on-generic-interface-impl">
                    <!-- e.g. IPredicate<T1> IPredicate<T>.And<T1>(IPredicate<T1> other)
                              IPredicate<T> IPredicate<T>.Negate() -->
                    <xsl:sequence select="substring-before($line-pre-arrow, '(') => tokenize('\s') =>
                      reverse() => head() =>
                      replace('^.*\.([A-Za-z0-9_]+)(&lt;([A-Za-z0-9_,\s]+)&gt;)?\s*$', '$1')"/>
                  </xsl:when>
                  <xsl:when test="$is-generic-method">
                    <xsl:sequence select="replace($line-pre-arrow, '^.*\s([A-Za-z0-9_]+)&lt;([A-Za-z0-9_,\s]+)&gt;\s*\(.*', '$1')"/>
                  </xsl:when>
                  <xsl:when test="$is-method">
                    <xsl:sequence select="replace($line-pre-arrow, '^.*\s([A-Za-z0-9_]+)\s*\(.*', '$1')"/>
                  </xsl:when>
                  <xsl:when test="$is-indexer">
                    <xsl:sequence select="'this'"/>
                  </xsl:when>
                  <xsl:when test="$is-initializer">
                    <xsl:analyze-string select="$line-pre-arrow" regex="([A-Za-z0-9_]+)\s*=">
                      <xsl:matching-substring><xsl:sequence select="regex-group(1)"/></xsl:matching-substring>
                      <xsl:non-matching-substring></xsl:non-matching-substring>
                    </xsl:analyze-string>
                  </xsl:when>
                  <xsl:when test="$has-explicit-base and $is-generic-type and ($is-class or $is-interface)">
                    <!-- e.g. interface IXdmEnumerable<out XdmItem> : IEnumerable<XdmItem> -->
                    <xsl:sequence select="substring-before(tokenize($line-pre-base, '\s')[contains(., '&lt;')], '&lt;')"/>
                  </xsl:when>
                  <xsl:when test="$is-generic-type and ($is-class or $is-interface)">
                    <!-- e.g. class (e.g Feature<T>) or interface (e.g. Predicate<T>) -->
                    <xsl:sequence select="substring-before($tokens[not(. = ('{',
                      '{get;}','get;','set;','{set;}','init;','}'))][contains(., '&lt;')][last()], '&lt;')"/>
                  </xsl:when>
                  <xsl:otherwise>
                    <xsl:sequence select="$tokens[not(. = ('{', '{get;}','get;','set;','{set;}','init;','}', 'private'))][last()]"/>
                  </xsl:otherwise>
                </xsl:choose>
              </xsl:variable>
              
              <xsl:variable name="name" select="translate($name, ';', '')"/>
              
              <!-- Normally the type (of the property or method) is the token that appears before its name. 
                But this fails for a property where the name is the same as the type, e.g. "public Uri Uri". 
                So we handle this case specially.  -->
              <!-- Note that for constructors, $line is just something like "public A()"; then $type will be ''. -->
              <!-- Also need a special case if the type is a generic type with more than one type parameter
                e.g. Dictionary<QName, ParameterDetails> 
                  or when processing a method of a class implementing a generic interface 
                  (so type is not directly before method name)
                  e.g. IPredicate<T1> IPredicate<T>.And<T1>(IPredicate<T1> other) -->
              <xsl:variable name="type-is-name" select="count($tokens[translate(., ';', '') eq $name]) gt 1"/>
              <xsl:variable name="type">
                <xsl:choose>
                  <xsl:when test="$type-is-name">
                    <xsl:value-of select="$name"/>
                  </xsl:when>
                  <xsl:when test="$is-generic-type and ($is-class or $is-interface)">
                    <!-- e.g. class (e.g Feature<T>) or interface (e.g. Predicate<T>) 
                      here $type will include the type parameter part, e.g. Feature<T> -->
                    <xsl:variable name="string" select="if (ends-with($line-pre-base, '{')) then
                      substring-before($line-pre-base, '{') else $line-pre-base" />
                    <xsl:value-of select="concat($name, substring-after($string, $name))" />
                  </xsl:when>
                  <xsl:when test="$is-method-on-generic-interface-impl">
                    <!-- e.g. IPredicate<T1> IPredicate<T>.And<T1>(IPredicate<T1> other)
                              IPredicate<T> IPredicate<T>.Negate() -->
                    <xsl:sequence select="substring-before($line, $name) => tokenize('\s') =>
                      reverse() => tail() => head()"/>
                  </xsl:when>
                  <xsl:otherwise>
                    <!-- Note could still be wrong if we had something like "public Uri<T> Uri" i.e. name is part of type -->
                    <xsl:variable name="string" select="substring-before($line-pre-arrow, concat(' ', $name))"/>
                    <xsl:choose>
                      <xsl:when test="$is-generic-type">
                        <xsl:variable name="Tclass" select="(substring-before($string,'&lt;')
                          => tokenize('\s'))[last()]"/>
                        <xsl:variable name="typeParams" select="concat('&lt;', substring-after($string,'&lt;')) 
                          => translate(' ', '')"/>
                        <xsl:value-of select="concat($Tclass, $typeParams)"/>
                      </xsl:when>
                      <xsl:otherwise>
                        <xsl:value-of select="tokenize($string, '\s')[last()][. != 'public']"/>
                        <!-- [. != 'public'] filter is for constructors; then $type is '' in this case -->
                      </xsl:otherwise>
                    </xsl:choose>
                  </xsl:otherwise>
                </xsl:choose>
              </xsl:variable>
              <!--<xsl:if test="$is-generic-type">
                <xsl:message expand-text="true">generic type: name {$name}</xsl:message>
                <xsl:message expand-text="true">generic type: type {$type}</xsl:message>
              </xsl:if>-->
              
              <xsl:choose>
                <xsl:when test="$is-class">
                  <class name="{$name}" public="{$is-public}" abstract="{$is-abstract}" static="{$is-static}" base="{$base}">
                    <xsl:if test="$is-internal">
                      <xsl:attribute name="internal" select="$is-internal"/>
                    </xsl:if>
                    <xsl:if test="$is-generic-type">
                      <!--<xsl:message>generic type class <xsl:value-of select="$name"/></xsl:message>-->
                      <xsl:sequence select="f:getTypeParamTypes($type)"/>
                    </xsl:if>
                  </class>
                </xsl:when>
                <xsl:when test="$is-interface">
                  <interface name="{$name}" public="{$is-public}" base="{$base}">
                    <xsl:if test="$is-internal">
                      <xsl:attribute name="internal" select="$is-internal"/>
                    </xsl:if>
                    <xsl:if test="$is-generic-type">
                      <!--<xsl:message>generic type interface <xsl:value-of select="$name"/></xsl:message>-->
                      <xsl:sequence select="f:getTypeParamTypes($type)"/>
                    </xsl:if>
                  </interface>
                </xsl:when>
                <xsl:when test="$is-delegate">
                  <delegate name="{$name}" public="{$is-public}"
                      type="{if ($is-generic-type) then substring-before($type, '&lt;') else $type}">
                    <xsl:if test="$is-internal">
                      <xsl:attribute name="internal" select="$is-internal"/>
                    </xsl:if>
                    <xsl:if test="$is-generic-type">
                      <!--<xsl:message>generic type interface <xsl:value-of select="$name"/></xsl:message>-->
                      <xsl:sequence select="f:getTypeParamTypes($type)"/>
                    </xsl:if>
                    <xsl:if test="$has-args">
                      <!--<xsl:if test="$has-args and matches($method-input, '^.*&lt;.*,.*&gt;')">
                        <xsl:message expand-text="true">method args for: {$name}</xsl:message>
                      </xsl:if>-->
                      <xsl:for-each select="$method-args">
                        <xsl:variable name="arg-type" select="substring-before(normalize-space(.),' ')"/>
                        <!--<xsl:if test="$has-args and matches($method-input, '^.*&lt;.*,.*&gt;')">
                          <xsl:message expand-text="true">method arg: {$arg-type}</xsl:message>
                        </xsl:if>-->
                        <xsl:choose>
                          <xsl:when test="contains($arg-type, '&lt;')">
                            <param type="{substring-before($arg-type, '&lt;')}" name="{substring-after(normalize-space(.),' ')}">
                              <xsl:sequence select="f:getTypeParamTypes($arg-type)"/>
                            </param>
                          </xsl:when>
                          <xsl:otherwise>
                            <param type="{$arg-type}" name="{substring-after(normalize-space(.),' ')}"/>
                          </xsl:otherwise>
                        </xsl:choose>
                      </xsl:for-each>
                    </xsl:if>
                  </delegate>
                </xsl:when>
                <xsl:when test="$is-enum">
                  <enum name="{$name}" public="{$is-public}" base="{$base}">
                    <xsl:if test="$is-internal">
                      <xsl:attribute name="internal" select="$is-internal"/>
                    </xsl:if>
                  </enum>
                </xsl:when>
                <xsl:when test="$is-method">
                  <method-or-constructor name="{$name}" public="{$is-public}" static="{$is-static}" abstract="{$is-abstract}" 
                    type="{if ($is-generic-type) then substring-before($type, '&lt;') else $type}"
                    override="{$is-override}" virtual="{$is-virtual}">
                    <xsl:if test="$is-generic-type">
                      <!--<xsl:message>generic type method <xsl:value-of select="$name"/></xsl:message>-->
                      <xsl:sequence select="f:getTypeParamTypes($type)"/>
                    </xsl:if>
                    <xsl:if test="$is-generic-method">
                      <!--<xsl:message>generic method <xsl:value-of select="$name"/></xsl:message>-->
                      <xsl:variable name="typeParams" as="xs:string*">
                        <xsl:sequence select="normalize-space(substring-before($line, '(')) => 
                          substring-after($name) => translate(' ', '') => f:getTypeParams()"/>
                      </xsl:variable>
                      <paramtypes>
                        <xsl:for-each select="$typeParams">
                          <type name="{.}"/>
                        </xsl:for-each>
                      </paramtypes>
                    </xsl:if>
                    <xsl:if test="$has-args">
                      <!--<xsl:if test="$has-args and matches($method-input, '^.*&lt;.*,.*&gt;')">
                        <xsl:message expand-text="true">method args for: {$name}</xsl:message>
                      </xsl:if>-->
                      <xsl:for-each select="$method-args">
                        <xsl:variable name="arg-tokens" select="tokenize(normalize-space(.), ' ')"/>
                        <xsl:variable name="arg-type" select="if ($arg-tokens[1] eq 'params') then
                          $arg-tokens[2] else $arg-tokens[1]"/>
                        <xsl:variable name="arg-name" select="if ($arg-tokens[1] eq 'params') then
                          $arg-tokens[3] else $arg-tokens[2]"/>
                        <!--<xsl:if test="$has-args and matches($method-input, '^.*&lt;.*,.*&gt;')">
                          <xsl:message expand-text="true">method arg: {$arg-type}</xsl:message>
                        </xsl:if>-->
                        <xsl:choose>
                          <xsl:when test="contains($arg-type, '&lt;')">
                            <xsl:variable name="isArray" select="ends-with($arg-type, '[]')"/>
                            <param type="{concat(substring-before($arg-type, '&lt;'), if ($isArray)
                              then '[]' else '')}" name="{$arg-name}">
                              <xsl:sequence select="f:getTypeParamTypes($arg-type)"/>
                            </param>
                          </xsl:when>
                          <xsl:otherwise>
                            <param type="{$arg-type}" name="{$arg-name}"/>
                          </xsl:otherwise>
                        </xsl:choose>
                      </xsl:for-each>
                    </xsl:if>
                  </method-or-constructor>
                </xsl:when>
                <xsl:when test="$is-initializer">
                  <field name="{$name}" public="{$is-public}" static="{$is-static}" abstract="{$is-abstract}" 
                    override="{$is-override}" virtual="{$is-virtual}" readonly="{$is-readonly}"
                    type="{if ($is-generic-type) then substring-before($type, '&lt;') else $type}" >
                    <xsl:if test="$is-generic-type">
                      <!--<xsl:message>generic type field <xsl:value-of select="$name"/></xsl:message>-->
                      <xsl:sequence select="f:getTypeParamTypes($type)"/>
                    </xsl:if>
                  </field>
                </xsl:when>
                <xsl:when test="$is-indexer">
                  <indexer name="Item" public="{$is-public}" static="{$is-static}" abstract="{$is-abstract}" 
                    override="{$is-override}" virtual="{$is-virtual}" readonly="{$is-readonly}"
                    type="{if ($is-generic-type) then substring-before($type, '&lt;') else $type}">
                    <xsl:variable name="index-tokens"
                      select="tokenize(normalize-space(substring-before(substring-after($line-pre-arrow,
                      '['),']')), ' ')"/>
                    <index type="{$index-tokens[1]}" name="{$index-tokens[2]}"/>
                  </indexer>
                </xsl:when>
                <xsl:otherwise>
                  <property initializer="{$is-initializer}" name="{$name}" public="{$is-public}" static="{$is-static}" readonly="{$is-readonly}" 
                    override="{$is-override}" virtual="{$is-virtual}" abstract="{$is-abstract}" 
                    type="{if ($is-generic-type) then substring-before($type, '&lt;') else $type}">
                    <xsl:if test="$is-generic-type">
                      <!--<xsl:message>generic type property <xsl:value-of select="$name"/></xsl:message>-->
                      <xsl:sequence select="f:getTypeParamTypes($type)"/>
                    </xsl:if>
                  </property>
                  <xsl:if test="$is-property-with-getter or ($tokens = 'get;' and
                    not($tokens[index-of($tokens, 'get;')-1] eq 'private'))">
                    <get />
                  </xsl:if>
                  <xsl:if test="$tokens = 'set;' and not($tokens[index-of($tokens, 'set;')-1] eq 'private')">
                    <set />
                  </xsl:if>
                  <xsl:if test="$tokens = 'init;'">
                    <init />
                  </xsl:if>
                </xsl:otherwise>
              </xsl:choose>
            </xsl:when>
          </xsl:choose>
        </xsl:for-each>
      </xsl:for-each>
    </package>  
  </xsl:function>

  <xsl:function name="f:group" as="element(package)">
    <xsl:param name="in" as="element(package)"/>
    <package>
      <xsl:copy-of select="$in/@*"/>
      <xsl:for-each-group select="$in//using" group-by="@class">
        <xsl:sort select="@class"/>
        <xsl:copy-of select="current-group()[1]"/>
      </xsl:for-each-group>
      <!-- ouput JType shortnames here -->
      <xsl:for-each-group select="$in/*" group-starting-with="class|interface|enum|delegate">
        <xsl:copy>
          <xsl:copy-of select="@*"/>
          <xsl:copy-of select="type"/>
          <xsl:copy-of select="param"/>
          <xsl:apply-templates select="remove(current-group(), 1)" mode="refine">
            <xsl:with-param name="type" select="current-group()[1]/@name"/>
          </xsl:apply-templates>
        </xsl:copy>
      </xsl:for-each-group>
    </package>
  </xsl:function>
  
  <xsl:function name="f:getTypeParams" as="xs:string*">
    <xsl:param name="genericType" as="xs:string"/>
    <!-- This may be a full generic type string, or just the part of the string with the type parameters -->
    <!-- Now only used for type parameters for generic methods -->
    <xsl:sequence select="substring-after($genericType, '&lt;') => substring-before('&gt;') => tokenize(',', 'q')"/>
  </xsl:function>
  
  <xsl:function name="f:getTypeParamTypes" as="element(type)*">
    <xsl:param name="genericType" as="xs:string"/><!-- A single generic type -->
    <!--<xsl:message expand-text="yes">f:getTypeParamTypes {$genericType}</xsl:message>-->
    <!-- Should only return the types of the type parameters, not the main type -->
    <!-- Also handles covariant|contravariant types, i.e. keywords out|in, e.g. IXdmEnumerator<out XdmItem> -->
    <!-- Also handles nested generic type parameters
      e.g. Func<TInput,IEnumerable<TResult>> 
        for public Step(Func<TInput, IEnumerable<TResult>> f)
      e.g. List<Step<XdmNode,XdmNode>> 
        for public static Step<XdmNode, XdmNode> PathFromList(List<Step<XdmNode, XdmNode>> steps) -->
    <xsl:variable name="typeParamString" as="xs:string?">
      <xsl:analyze-string select="$genericType" regex="[^&lt;]*&lt;([A-Za-z0-9_,\.&lt;&gt;\s]+)&gt;">
        <xsl:matching-substring><xsl:sequence select="regex-group(1)"/></xsl:matching-substring>
        <xsl:non-matching-substring></xsl:non-matching-substring>
      </xsl:analyze-string>
    </xsl:variable>
    <xsl:choose>
      <xsl:when test="contains($typeParamString, '&lt;')">
        <!-- Contains nested generic type params -->
        <xsl:choose>
          <xsl:when test="contains($typeParamString, ',')">
            <!-- Contains multiple type params at this level, or nested -->
            <!--<xsl:message expand-text="yes">*** multiple type params {$typeParamString}</xsl:message>-->
            <!-- We want to split the list of types (e.g. TInput,IEnumerable<TResult>), by tokenizing on ','
                 but need to be careful not to split nested generic types with more than one type parameter
                 (e.g. Step<XdmNode,XdmNode> for XdmNode.Select) -->
            <!-- The approach we take here is to:
                  1. tokenize on ','; and then 
                  2. check through the sequence, and if we have '<' without '>' then join the tokens
                  back together. -->
            <xsl:variable name="param-split" as="xs:string*">
              <xsl:iterate select="tokenize($typeParamString, ',')">
                <xsl:param name="tokens" select="()" as="xs:string*"/>
                <xsl:param name="generic" select="''" as="xs:string"/>
                <xsl:on-completion>
                  <xsl:sequence select="$tokens"/>
                </xsl:on-completion>
                <xsl:variable name="token" select="." as="xs:string"/>
                <!--<xsl:message expand-text="yes">$token {$token}</xsl:message>-->
                <xsl:variable name="newTokens" as="xs:string*">
                  <xsl:choose>
                    <xsl:when test="$generic ne ''">
                      <xsl:sequence select="if (contains($token, '&gt;') and not(contains($token,
                        '&lt;'))) then ($tokens, $generic||','||$token) else $tokens"/>
                    </xsl:when>
                    <xsl:when test="contains($token, '&lt;') and not(contains($token, '&gt;'))">
                      <xsl:sequence select="$tokens"/>
                    </xsl:when>
                    <xsl:when test="contains($token, '&gt;') and not(contains($token, '&lt;'))">
                      <xsl:message expand-text="yes">*** WARNING problem in param-split at token: {$token}</xsl:message>
                      <xsl:message expand-text="yes">    tokens: {$tokens}</xsl:message>
                      <xsl:message expand-text="yes">    generic: {$generic}</xsl:message>
                      <xsl:sequence select="$tokens"/>
                    </xsl:when>
                    <xsl:otherwise>
                      <xsl:sequence select="$tokens, $token"/>
                    </xsl:otherwise>
                  </xsl:choose>
                </xsl:variable>
                <xsl:variable name="newGeneric" as="xs:string">
                  <xsl:choose>
                    <xsl:when test="$generic ne ''">
                      <xsl:sequence select="if (contains($token, '&gt;') and not(contains($token,
                        '&lt;'))) then '' else $generic||','||$token"/>
                    </xsl:when>
                    <xsl:when test="contains($token, '&lt;') and not(contains($token, '&gt;'))">
                      <xsl:sequence select="$token"/>
                    </xsl:when>
                    <xsl:when test="contains($token, '&gt;') and not(contains($token, '&lt;'))">
                      <!--<xsl:message expand-text="yes">*** WARNING problem in param-split at token: {$token}</xsl:message>-->
                      <xsl:sequence select="$generic"/>
                    </xsl:when>
                    <xsl:otherwise>
                      <xsl:sequence select="$generic"/>
                    </xsl:otherwise>
                  </xsl:choose>
                </xsl:variable>
                <!--<xsl:message expand-text="yes">$newTokens {$newTokens}</xsl:message>
                <xsl:message expand-text="yes">$newGeneric {$newGeneric}</xsl:message>-->
                <xsl:next-iteration>
                  <xsl:with-param name="tokens" select="$newTokens"/>
                  <xsl:with-param name="generic" select="$newGeneric"/>
                </xsl:next-iteration>
              </xsl:iterate>
            </xsl:variable>
            <!--<xsl:message expand-text="yes">** $param-split {$param-split}</xsl:message>-->
            
            <xsl:for-each select="$param-split">
              <xsl:choose>
                <xsl:when test="contains(., '&lt;')">
                  <type type="{substring-before(., '&lt;')}">
                    <xsl:sequence select="f:getTypeParamTypes(.)"/>
                  </type>
                </xsl:when>
                <xsl:otherwise>
                  <xsl:sequence select="f:getSingleTypeParamType(.)"/>
                </xsl:otherwise>
              </xsl:choose>
            </xsl:for-each>
          </xsl:when>
          
          <xsl:otherwise>
            <!-- Only one type param -->
            <type type="{substring-before($typeParamString, '&lt;')}">
              <xsl:sequence select="f:getTypeParamTypes($typeParamString)"/>
            </type>
          </xsl:otherwise>
        </xsl:choose>
      </xsl:when>
      
      <xsl:otherwise>
        <!-- No nested generic type params, so can just tokenize on ',' directly -->
        <xsl:variable name="typeParams" as="xs:string*" select="tokenize($typeParamString, ',', 'q')"/>
        <xsl:for-each select="$typeParams">
          <xsl:sequence select="f:getSingleTypeParamType(.)"/>
        </xsl:for-each>
      </xsl:otherwise>
    </xsl:choose>
    
  </xsl:function>
  
  <xsl:function name="f:getSingleTypeParamType" as="element(type)">
    <xsl:param name="typeParam" as="xs:string"/><!-- The non-generic type of a single type parameter -->
    <xsl:choose>
      <xsl:when test="starts-with($typeParam, 'out ')">
        <type type="{substring-after($typeParam, 'out ')}" extendby="out"/>
      </xsl:when>
      <xsl:when test="starts-with($typeParam, 'in ')">
        <type type="{substring-after($typeParam, 'in ')}" super="in"/>
      </xsl:when>
      <xsl:otherwise>
        <type type="{$typeParam}"/>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:function>

  <xsl:template match="*" mode="refine">
    <xsl:copy-of select="."/>
  </xsl:template>
  
  <xsl:template match="using" mode="refine"/>

  <xsl:template match="method-or-constructor" mode="refine">
    <xsl:param name="type"/>
    <xsl:element name="{if ($type=@name) then 'constructor' else 'method'}">
      <xsl:copy-of select="@*|*"/>
    </xsl:element>
  </xsl:template>
        
</xsl:stylesheet> 
   
 