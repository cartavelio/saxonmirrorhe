<xsl:stylesheet version="3.0" 
   xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
   xmlns:xs="http://www.w3.org/2001/XMLSchema"
   xmlns:f="http://local-functions/"
   xmlns:map="http://www.w3.org/2005/xpath-functions/map"
   xmlns:functx="http://www.functx.com"
   exclude-result-prefixes="xs f map functx">
  
  <xsl:output method="xml" indent="yes"/>
  
  <xsl:param name="names-file" select="'names.xml'"/>
  <xsl:param name="api-doc-file" select="'apidoc.xml'"/>
  
  <xsl:variable name="names-doc" select="doc($names-file)"/> 
  <xsl:variable name="api-doc" select="doc($api-doc-file)"/>
  
  <xsl:key name="class-by-name" match="class | interface | enum | delegate" 
    use="if (contains(@name,'.')) then tokenize(@name, '\.')[last()] else @name"/>
  
  <xsl:template name="main">
    <xsl:apply-templates select="$names-doc/package"/>
  </xsl:template>
  
  <xsl:template match="package">
    <xsl:copy>
      <xsl:copy-of select="@*"/>
      <xsl:apply-templates select="*"/>
    </xsl:copy>
  </xsl:template>
  
  <!-- Do not include internal classes, interfaces, or enums -->
  <xsl:template match="(class|interface|enum|delegate)[@internal='true']"/>
  
  <!-- class, interface, enum all begin 'T:' (enum followed by 'F:' classes) -->

  <xsl:template match="class | interface | enum | delegate">
    <xsl:param name="process-nested-class" select="false()"/>
    
    <!-- Generic class e.g. Feature<T> corresponding apiclass name is 
      T:Saxon.Api.Feature`1 -->
    <!-- Generic method e.g. SetProperty<T> corresponding apiclass name is 
      M:Saxon.Api.Processor.SetProperty``1(Saxon.Api.Feature{``0},``0) -->
    <!-- Generic method on generic class (interface) e.g. Or<T> method of IPredicate<T>
      corresponding apiclass name is 
      M:Saxon.Api.IPredicate`1.Or``1(Saxon.Api.IPredicate{``0}) -->
    
    <xsl:variable name="shortname" select="@name"/>
    <xsl:variable name="apiclass" select="$api-doc//member[starts-with(@name, 'T:') and
      ((tokenize(@name, '\.')[last()] eq $shortname)
      or (substring-before(tokenize(@name, '\.')[last()], '`') eq $shortname))]"/>
    <!--<xsl:message>processing class <xsl:value-of select="$apiclass/@name, $shortname"/></xsl:message>-->
    <xsl:if test="empty($apiclass)">
      <xsl:message>--- Missing documentation for <xsl:value-of select="$shortname"/></xsl:message>
    </xsl:if>
    <xsl:if test="self::interface[empty(*)]">
      <xsl:message>---- <xsl:value-of select="@name"/> interface has no methods or properties;
        the cs code lines should begin '/**public**/' so that they are included</xsl:message>
    </xsl:if>
    
    
    <xsl:variable name="fullname" select="substring-after($apiclass/@name, ':')"/>
    <xsl:variable name="fullname" select="if (contains($fullname,'`')) then
      substring-before($fullname, '`') else $fullname"/>
    <xsl:variable name="nestedname" select="substring-after($fullname, 'Saxon.Api.')"/>
    <xsl:variable name="is-nested" select="not($shortname eq $nestedname)"/>
    <!--<xsl:if test="$is-nested">
      <xsl:message>nested class <xsl:value-of select="$nestedname, $shortname"/> 
        process nested class <xsl:value-of select="$process-nested-class"/></xsl:message>
    </xsl:if>-->
    
    <!-- Check that $api-doc contains comments for all fields, constructors, properties, and methods -->
    <xsl:variable name="field-count" select="count($api-doc//member[starts-with(@name, concat('F:Saxon.Api.',
      $nestedname, '.')) or starts-with(@name, concat('F:Saxon.Api.', $nestedname, '`'))])"/>
    <xsl:if test="count(field[@public ne 'false']) - $field-count gt 0">
      <xsl:message>Missing <xsl:value-of 
        select="count(field[@public ne 'false']) - $field-count"/> field comments in <xsl:value-of 
          select="@name"/></xsl:message>
    </xsl:if>
    
    <xsl:variable name="constructor-count" select="count($api-doc//member[(starts-with(@name, concat('M:Saxon.Api.',
      $nestedname, '.')) or starts-with(@name, concat('M:Saxon.Api.', $nestedname, '`'))) 
      and contains(@name, '.#ctor')])"/>
    <xsl:if test="count(constructor[@public ne 'false']) - $constructor-count gt 0">
      <xsl:message>Missing <xsl:value-of 
        select="count(constructor[@public ne 'false']) - $constructor-count"/> constructor comments in <xsl:value-of 
          select="@name"/></xsl:message>
    </xsl:if>
    
    <xsl:variable name="property-count" select="count($api-doc//member[starts-with(@name, concat('P:Saxon.Api.', 
      $nestedname, '.')) or starts-with(@name, concat('P:Saxon.Api.', $nestedname, '`'))])"/>
    <xsl:if test="count(property[@public ne 'false']) + count(indexer[@public ne 'false']) - $property-count gt 0">
      <xsl:message>Missing <xsl:value-of 
        select="count(property[@public ne 'false']) + count(indexer[@public ne 'false']) 
        - $property-count"/> property comments in <xsl:value-of select="@name"/></xsl:message>
    </xsl:if>
    
    <xsl:variable name="method-count" select="count($api-doc//member[(starts-with(@name, concat('M:Saxon.Api.',
      $nestedname, '.')) or starts-with(@name, concat('M:Saxon.Api.', $nestedname, '`'))) 
      and not(contains(@name, '.#ctor'))])"/>
    <xsl:if test="count(method[@public ne 'false']) - $method-count gt 0">
      <xsl:message>Missing <xsl:value-of 
        select="count(method[@public ne 'false']) - $method-count"/> method comments in <xsl:value-of 
          select="@name"/></xsl:message>
    </xsl:if>
    
    <xsl:variable name="apisubclasses" select="$api-doc//member[starts-with(@name, concat('T:',
      $fullname, '.'))]"/>
    <xsl:variable name="has-subclasses" select="exists($apisubclasses)"/>
    <!--<xsl:if test="$has-subclasses">
      <xsl:message>subclasses of <xsl:value-of select="$fullname"/> : <xsl:value-of
        select="$apisubclasses/@name"/></xsl:message>
    </xsl:if>-->
    
    <xsl:if test="not($is-nested) or $process-nested-class">
      <xsl:copy>
        <xsl:copy-of select="@* except (@base|@name|@type)"/>
        <xsl:attribute name="name" select="if ($process-nested-class) then $nestedname else @name"/>
        <xsl:attribute name="{if (self::delegate) then 'fullname' else 'fulltype'}" select="$fullname"/>
        <xsl:if test="@base">
          <xsl:variable name="superclasses" select="tokenize(@base, ', ', 'q')"/>
          <!-- For generic superclasses, can just remove type parameters in @base -->
          <xsl:variable name="superclassesMain" select="$superclasses!(if (contains(., '&lt;')) then (substring-before(.,
            '&lt;')) else .)"/>
          <xsl:attribute name="base" select="string-join($superclassesMain!f:getKnownFullType(.), ', ')"/>
          <!-- If $base is a class, then this is a subclass which extends the base class; 
                if $base is an interface, then this is a class which implements the base interface. -->
          <xsl:call-template name="process-base">
            <xsl:with-param name="derived-class" select="." as="element()" tunnel="yes"/>
            <xsl:with-param name="base-attr-value" select="string(@base)"/>
          </xsl:call-template>
        </xsl:if>
        <xsl:if test="@type">
          <!-- for delegates -->
          <xsl:attribute name="type" select="@type"/>
          <xsl:attribute name="fulltype" select="f:getKnownFullType(@type)"/>
        </xsl:if>
        <xsl:apply-templates select="type"/><!-- for classes and interfaces which are generic types -->
        <xsl:if test="self::delegate">
          <xsl:sequence select="f:params-from-names-doc(param)"/>
        </xsl:if>
        <comments>
          <xsl:variable name="obsolete" select="reverse(preceding-sibling::*)[1]/self::obsolete"/>
          <xsl:apply-templates select="$obsolete" mode="comment"/>
          <xsl:apply-templates select="$apiclass/*" mode="comment"/>
        </comments>
        <xsl:apply-templates select="*[not(@public='false')] except (type)"/><!-- Go process methods etc. -->
        <xsl:if test="self::enum">
          <xsl:variable name="fields" select="$api-doc//member[starts-with(@name, concat('F:', $fullname, '.'))]"/>
          <xsl:if test="empty($fields)">
            <xsl:message>---- <xsl:value-of select="@name"/> enum has no fields, check for missing comments</xsl:message>
          </xsl:if>
          <xsl:for-each select="$fields">
            <field name="{tokenize(@name, '\.')[last()]}"
              public="true">
              <comments>
                <xsl:apply-templates select="*" mode="comment"/>
              </comments>
            </field>
          </xsl:for-each>
        </xsl:if>
        
        <!-- If class has nested subclasses, process them now. -->
        <xsl:if test="$has-subclasses">
          <xsl:variable name="subclass-shortnames" select="$apisubclasses/@name/tokenize(., '\.')[last()]"/>
          <xsl:apply-templates select="//class[@name = $subclass-shortnames]">
            <xsl:with-param name="process-nested-class" select="true()"/>
          </xsl:apply-templates>
        </xsl:if>
      </xsl:copy>
    </xsl:if>
    
  </xsl:template>
  
  <xsl:template name="process-base">
    <xsl:param name="derived-class" as="element()" tunnel="yes"/>
    <xsl:param name="base-attr-value" as="xs:string"/>
    <xsl:variable name="superclasses" select="tokenize($base-attr-value, ', ', 'q')"/>
    
    <!--<xsl:message>process-base of <xsl:value-of select="$derived-class/@name"/>: <xsl:value-of
      select="$base-attr-value"/></xsl:message>-->
    
    <xsl:call-template name="base-class">
      <xsl:with-param name="superclasses" select="$superclasses"/>
    </xsl:call-template>
    <xsl:call-template name="base-interfaces">
      <xsl:with-param name="superclasses" select="$superclasses"/>
    </xsl:call-template>
    
  </xsl:template>
  
  <xsl:template name="base-class">
    <xsl:param name="derived-class" as="element()" tunnel="yes"/>
    <xsl:param name="superclasses" as="xs:string*"/>
    <xsl:variable name="bClass" as="element(interface)*">
      <xsl:for-each select="$superclasses">
        <xsl:variable name="superclass" select="if (contains(., '&lt;')) then substring-before(., '&lt;') else ."/>
        <xsl:variable name="basei" select="key('class-by-name', $superclass, $names-doc)"/>
        <xsl:choose>
          <xsl:when test="$superclass = $knownInterfaces"/>
          <xsl:when test="$superclass = 'object'"/><!-- Don't include extends child if the base is 'System.Object' -->
          <xsl:when test="exists($basei) and (local-name($basei) ne 'interface')">
            <xsl:sequence select="f:getBase(., f:saxonFullName($basei), $basei)"/>
          </xsl:when>
          <xsl:when test="empty($basei)">
            <!-- If non-local super is not known to be an interface, then assume it is a class.
                  ($knownInterfaces must be maintained.) -->
            <xsl:if test="contains($superclass, '.') and not(contains($superclass, 'Saxon.Hej'))">
              <xsl:message>-- Unhandled nested superclass: <xsl:value-of select="$superclass"/></xsl:message>
            </xsl:if>
            <xsl:variable name="knownType" select="f:getKnownFullType($superclass)"/>
            <xsl:sequence select="f:getBase(., $knownType, ())"/>
          </xsl:when>
        </xsl:choose>
      </xsl:for-each>
    </xsl:variable>
    <xsl:if test="count($bClass) gt 1">
      <!-- Only really possible to extend one class. Warning means $knownInterfaces may need updating. -->
      <xsl:message>Warning: update $knownInterfaces. Class with multiple base classes - <xsl:value-of select="$derived-class/@name"/></xsl:message>
    </xsl:if>
    <xsl:if test="exists($bClass)">
      <extends>
        <xsl:sequence select="$bClass"/>
      </extends>
    </xsl:if>
  </xsl:template>
  
  <xsl:template name="base-interfaces">
    <xsl:param name="superclasses" as="xs:string*"/>
    <xsl:variable name="bInterfaces" as="element(interface)*">
      <xsl:for-each select="$superclasses">
        <xsl:variable name="superclass" select="if (contains(., '&lt;')) then substring-before(., '&lt;') else ."/>
        <xsl:variable name="basei" select="key('class-by-name', $superclass, $names-doc)"/>
        <xsl:choose>
          <xsl:when test="exists($basei) and (local-name($basei) eq 'interface')">
            <xsl:sequence select="f:getBase(., f:saxonFullName($basei), $basei)"/>
          </xsl:when>
          <xsl:when test="$superclass = $knownInterfaces">
            <xsl:variable name="base-is-nested" select="contains($superclass, '.')"/>
            <xsl:variable name="base-top-class" select="if ($base-is-nested) then
              substring-before($superclass, '.') else $superclass"/>
            <xsl:variable name="nested-name" select="if ($base-is-nested) then substring-after($superclass,
              $base-top-class) else ''"/>
            <xsl:variable name="knownType" select="f:getKnownFullType($base-top-class)"/>
            <xsl:sequence select="f:getBase(., concat($knownType, $nested-name), ())"/>
          </xsl:when>
          <!-- Otherwise the superclass is assumed to be a class not an interface; and processed by base-class. -->
        </xsl:choose>
      </xsl:for-each>
    </xsl:variable>
    <xsl:if test="exists($bInterfaces)">
      <implements>
        <xsl:sequence select="$bInterfaces"/>
      </implements>
    </xsl:if>
  </xsl:template>
  
  <xsl:function name="f:getBase" as="element(interface)">
    <xsl:param name="superclass" as="xs:string"/>
    <xsl:param name="fulltype" as="xs:string"/>
    <xsl:param name="base" as="element()?"/>
    <interface type="{$fulltype}">
      <!-- Iterate to construct full type hierarchy -->
      <xsl:if test="exists($base) and $base/@base">
        <xsl:call-template name="process-base">
          <xsl:with-param name="derived-class" select="$base" as="element()" tunnel="yes"/>
          <xsl:with-param name="base-attr-value" select="string($base/@base)"/>
        </xsl:call-template>
      </xsl:if>
      <xsl:if test="contains($superclass, '&lt;')">
        <!-- Cannot just copy $basei/type directly, as type parameters may be different 
            (e.g. when base interface has covariant type parameter) -->
        <xsl:variable name="typeParams"  as="xs:string*"
          select="substring-after($superclass, '&lt;') => substring-before('&gt;') => tokenize(',', 'q')"/>
        <xsl:for-each select="$typeParams">
          <type type="{.}"/>
        </xsl:for-each>
      </xsl:if>
    </interface>
  </xsl:function>
  
  <xsl:function name="f:getTypeParamTypes" as="element(type)*">
    <xsl:param name="typeParamString" as="xs:string"/>
    <xsl:variable name="typeParams"  as="xs:string*"
      select="substring-after($typeParamString, '&lt;') => substring-before('&gt;') => tokenize(',', 'q')"/>
    <xsl:for-each select="$typeParams">
      <type type="{.}"/>
    </xsl:for-each>
  </xsl:function>

  <xsl:template match="constructor">
    <!--<xsl:message>processing <xsl:value-of select="@name"/> : with param <xsl:value-of
    select="param/@name"/></xsl:message>-->
    <xsl:copy>
      <xsl:copy-of select="@* except @type"/>
      <xsl:variable name="position" select="count(preceding-sibling::constructor[@public='true'])+1"/>
      <xsl:variable name="apimember" select="$api-doc//member[starts-with(@name,'M:Saxon.Api.') and 
        (
        ends-with(functx:substring-before-if-contains(@name, '('), concat(current()/@name,'.#ctor')) or 
        (contains(functx:substring-before-if-contains(@name, '('), concat(current()/@name, '`')) and
        ends-with(functx:substring-before-if-contains(@name, '('), '.#ctor'))
        )][$position]"/>
      
      <!--<xsl:message>position <xsl:value-of select="$position"/></xsl:message>
      <xsl:message>apimember <xsl:value-of select="$apimember/@name"/> : with param <xsl:value-of
        select="param/@name"/></xsl:message>-->
      <xsl:if test="empty($apimember)">
        <xsl:message>*** apimember not found for <xsl:value-of select="@name"/> constructor</xsl:message>
      </xsl:if>
      <xsl:sequence select="f:extract-params($apimember/@name)"/>
      <xsl:if test="count(f:extract-params($apimember/@name)) = 0">
        <xsl:sequence select="f:params-from-names-doc(param)"/>
      </xsl:if>
      <comments>
        <xsl:variable name="obsolete" select="reverse(preceding-sibling::*)[1]/self::obsolete"/>
        <xsl:apply-templates select="$obsolete" mode="comment"/>
        <xsl:apply-templates select="$apimember/*" mode="comment"/>
      </comments>
    </xsl:copy>
  </xsl:template>
  
  <xsl:template match="node()|@*" mode="comment">
    <xsl:copy>
      <xsl:apply-templates mode="comment" select="node()|@*"/>
    </xsl:copy>
  </xsl:template>
  
  <xsl:template match="exception" mode="comment">
    <xsl:copy>
      <xsl:attribute name="name" select="if (@name) then @name else (substring-after(@cref,':'))"/>
      <xsl:apply-templates mode="comment"/>
    </xsl:copy>
  </xsl:template>
  
  <xsl:template match="see" mode="comment">
    <xsl:variable name="cref" select="substring-after(@cref,':')"/>
    
    <xsl:variable name="preArgs" select="if (contains($cref,'(')) then substring-before($cref,'(') else $cref"/>
    <xsl:variable name="args" select="substring-before(substring-after($cref,'('), ')')"/>
    
    <!-- Handle generics: if class contains '{ ... }' then chop out this part -->
    <xsl:variable name="preArgs" select="if (contains($preArgs,'{')) then 
      concat(substring-before($preArgs,'{'), substring-after($preArgs,'}')) else $preArgs"/>
    
    <!-- First convert separator '.' to '#' for methods, properties, etc on Class -->
    <xsl:variable name="class" as="xs:string">
      <xsl:choose>
        <xsl:when test="starts-with($preArgs, 'Saxon.Api.')">
          <xsl:value-of select="concat('Saxon.Api.', translate(substring-after($preArgs, 'Saxon.Api.'), '.', '#'))"/>
        </xsl:when>
        <xsl:otherwise>
          <xsl:value-of select="replace(replace($preArgs, 'Saxon.Hej.', 'net.sf.saxon.'), 'Saxon.Eej.', 'com.saxonica.')"/>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>
    
    <xsl:variable name="argStr" as="xs:string">
      <!-- For methods with arguments, remove all package names (not just 'Saxon.Api.') for arg types.
          Otherwise fully qualified names appear in documentation app unnecessarily. 
          Also, for arguments of generic type, drop type parameters i.e. {...} -->
      <!-- e.g. for Saxon.Api.Xslt30Transformer.SetInitialTemplateParameters
            (System.Collections.Generic.Dictionary{Saxon.Api.QName,Saxon.Api.XdmValue},System.Boolean) 
            $argStr becomes (Dictionary,Boolean) -->
      <xsl:variable name="argsMain" as="xs:string">
        <xsl:choose>
          <xsl:when test="contains($args, '{')">
            <xsl:variable name="main-args" as="xs:string*">
              <xsl:analyze-string select="$args" regex="{'\{.*\}'}">
                <xsl:non-matching-substring>
                  <xsl:value-of select="."/>
                </xsl:non-matching-substring>
              </xsl:analyze-string>
            </xsl:variable>
            <xsl:value-of select="string-join($main-args)"/>
          </xsl:when>
          <xsl:otherwise>
            <xsl:value-of select="$args"/>
          </xsl:otherwise>
        </xsl:choose>
      </xsl:variable>
      
      <xsl:choose>
        <xsl:when test="$argsMain ne ''">
          <xsl:variable name="argLocal" select="tokenize($argsMain, ',', 'q')!(tokenize(., '.', 'q')[last()])" as="xs:string*"/>
          <xsl:value-of select="concat('(', string-join($argLocal, ','), ')')"/>
        </xsl:when>
        <xsl:otherwise>
          <xsl:value-of select="''"/>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>
    
    <!-- Second convert separator '+' to '.' for nested classes.
          Also remove generic type marker e.g. `1 -->
    <tag kind="see">
      <xsl:value-of select="concat(replace(translate($class, '+', '.'), '``*[0-9]+', ''), $argStr)"/>
    </tag>
    
  </xsl:template>
  
  <xsl:function name="functx:substring-before-if-contains" as="xs:string?"
    xmlns:functx="http://www.functx.com">
    <xsl:param name="arg" as="xs:string?"/>
    <xsl:param name="delim" as="xs:string"/>
    
    <xsl:sequence select="
      if (contains($arg,$delim))
      then substring-before($arg,$delim)
      else $arg
      "/>
    
  </xsl:function>

  <xsl:template match="method">
    <xsl:copy>
      <xsl:copy-of select="@* except @type"/>
      <xsl:if test="@type">
        <xsl:attribute name="type" select="f:getKnownFullType(@type)"/>
      </xsl:if>
      <xsl:apply-templates select="type"/><!-- for methods which return generic types -->
      <xsl:apply-templates select="paramtypes"/><!-- for generic methods -->
      <xsl:variable name="position" select="count(preceding-sibling::method[@public='true' and @name=current()/@name])+1"/>
      <xsl:variable name="apimember" select="$api-doc//member[starts-with(@name,'M:Saxon.Api.') and 
        (
        ends-with(functx:substring-before-if-contains(@name, '('), concat(current()/../@name, '.', current()/@name)) or 
        (contains(functx:substring-before-if-contains(@name, '('), concat(current()/../@name, '`')) and
        ends-with(functx:substring-before-if-contains(@name, '('), concat('.', current()/@name))) or
        contains(functx:substring-before-if-contains(@name, '('), concat(current()/../@name,  '.',
        current()/@name, '`')) or 
        (contains(functx:substring-before-if-contains(@name, '('), concat(current()/../@name, '`')) and
        contains(functx:substring-before-if-contains(@name, '('), concat('.', current()/@name, '`')))
        )][$position]"/>
      
      <xsl:if test="empty($apimember)">
        <xsl:message>*** apimember not found for <xsl:value-of select="../@name"/>.<xsl:value-of select="@name"/></xsl:message>
      </xsl:if>
      
      <!-- Usually get params from apidoc.xml - in this case all types have been fully resolved.
          When there are parameters with generic types, or this is a generic method, or generic class,
          it is easier to get the parameters from names.xml, than try to extract them from apidoc.xml.
          Types of parameters will be resolved in the next step of the pipeline as required.
          For generic methods, the information in apidoc.xml may not be good enough for
          parameters with types which use the method's type parameters
          e.g. "M:Saxon.Api.Processor.SetProperty``1(Saxon.Api.Feature{``0},``0)"
          or for methods of classes of generic type, with parameters with types which use the
          class's type parameters e.g. "M:Saxon.Api.Predicate`1.matches(`0)" -->
      <xsl:variable name="is-generic-method" select="exists(paramtypes)"/>
      <xsl:variable name="apidoc-params" select="f:extract-params($apimember/@name)"/>
      <xsl:sequence select="if ($is-generic-method or count($apidoc-params) = 0) 
        then f:params-from-names-doc(param) else $apidoc-params"/>
      <comments>
        <xsl:variable name="obsolete" select="reverse(preceding-sibling::*)[1]/self::obsolete"/>
        <xsl:apply-templates select="$obsolete" mode="comment"/>
        <xsl:apply-templates select="$apimember/*" mode="comment"/>
      </comments>
    </xsl:copy>
  </xsl:template>   

  <xsl:template match="property">
    <xsl:copy>
      <xsl:copy-of select="@* except @type"/>
      <xsl:if test="@type">
        <xsl:attribute name="type" select="f:getKnownFullType(@type)"/>
      </xsl:if>
      <xsl:variable name="position" select="count(preceding-sibling::property[@name=current()/@name])+1"/>
      <xsl:variable name="apimember" select="$api-doc//member[starts-with(@name,'P:Saxon.Api.') and 
        (ends-with(@name, concat(current()/../@name, '.', current()/@name)) or 
        (contains(@name, concat(current()/../@name, '`')) and ends-with(@name, concat('.', current()/@name))))][$position]"/>
      <xsl:if test="empty($apimember)">
        <xsl:message>*** apimember not found for <xsl:value-of select="../@name"/>.<xsl:value-of select="@name"/></xsl:message>
      </xsl:if>
      <xsl:if test="subsequence(following-sibling::*, 1, 2)/self::get">
        <xsl:attribute name="get">true</xsl:attribute>
      </xsl:if>
      <xsl:if test="subsequence(following-sibling::*, 1, 2)/self::set">
        <xsl:attribute name="set">true</xsl:attribute>
      </xsl:if>
      <xsl:if test="subsequence(following-sibling::*, 1, 2)/self::init">
        <xsl:attribute name="init">true</xsl:attribute>
      </xsl:if>
      <xsl:apply-templates select="type"/><!-- for properties which are generic types -->
      <comments>
        <xsl:variable name="obsolete" select="reverse(preceding-sibling::*)[1]/self::obsolete"/>
        <xsl:apply-templates select="$obsolete" mode="comment"/>
        <xsl:apply-templates select="$apimember/*" mode="comment"/>
      </comments>
    </xsl:copy>
  </xsl:template>
  
  <xsl:template match="field">
    <xsl:copy>
      <xsl:copy-of select="@* except @type"/>
      <xsl:if test="@type">
        <xsl:attribute name="type" select="f:getKnownFullType(@type)"/>
      </xsl:if>
      <xsl:apply-templates select="type"/><!-- for fields which are generic types -->
      <xsl:variable name="apimember" select="$api-doc//member[starts-with(@name,'F:Saxon.Api.') and 
        (ends-with(@name, concat(current()/../@name, '.', current()/@name)) or 
        (contains(@name, concat(current()/../@name, '`')) and ends-with(@name, concat('.', current()/@name))))]"/>
      <xsl:if test="empty($apimember)">
        <xsl:message>*** apimember not found for <xsl:value-of select="../@name"/>.<xsl:value-of select="@name"/></xsl:message>
      </xsl:if>
      <comments>
        <xsl:variable name="obsolete" select="reverse(preceding-sibling::*)[1]/self::obsolete"/>
        <xsl:apply-templates select="$obsolete" mode="comment"/>
        <xsl:apply-templates select="$apimember/*" mode="comment"/>
      </comments>
    </xsl:copy>
  </xsl:template>
  
  <xsl:template match="indexer">
    <xsl:copy>
      <xsl:copy-of select="@* except @type"/>
      <xsl:if test="@type">
        <xsl:attribute name="type" select="f:getKnownFullType(@type)"/>
      </xsl:if>
      <xsl:apply-templates select="type"/><!-- for indexers which are generic types -->
      <xsl:variable name="apimember" select="$api-doc//member[starts-with(@name,'P:Saxon.Api.') and 
        (ends-with(functx:substring-before-if-contains(@name, '('), concat(current()/../@name, '.', current()/@name)) or 
        (contains(functx:substring-before-if-contains(@name, '('), concat(current()/../@name, '`')) and 
        ends-with(functx:substring-before-if-contains(@name, '('), concat('.', current()/@name))))]"/>
      <xsl:if test="empty($apimember)">
        <xsl:message>*** apimember not found for <xsl:value-of select="../@name"/>.<xsl:value-of select="@name"/></xsl:message>
      </xsl:if>
      <xsl:variable name="is-generic-method" select="exists(paramtypes)"/>
      <xsl:variable name="apidoc-params" select="f:extract-params($apimember/@name)"/>
      <xsl:sequence select="if ($is-generic-method or count($apidoc-params) = 0) 
        then f:params-from-names-doc(param) else $apidoc-params"/>
      <comments>
        <xsl:variable name="obsolete" select="reverse(preceding-sibling::*)[1]/self::obsolete"/>
        <xsl:apply-templates select="$obsolete" mode="comment"/>
        <xsl:apply-templates select="$apimember/*" mode="comment"/>
      </comments>
    </xsl:copy>
  </xsl:template>
  
  <xsl:template match="using"/> 
  
  <xsl:template match="get|set|init"/> 
  
  <xsl:template match="obsolete"/>
  
  <xsl:template match="param"/>
  
  <xsl:template match="paramtypes">
    <xsl:copy-of select="."/>
  </xsl:template>
  
  <xsl:template match="type">
    <xsl:copy>
      <xsl:copy-of select="@* except @type"/>
      <xsl:if test="@type">
        <xsl:attribute name="type" select="f:getKnownFullType(@type)"/>
      </xsl:if>
      <xsl:apply-templates select="type"/><!-- for parameters which are generic types -->
    </xsl:copy>
  </xsl:template>
  
  <xsl:template match="*">
    <xsl:message>*** No template found for <xsl:value-of select="name()"/></xsl:message>
  </xsl:template>
  
  <!-- If apidoc.xml does not have (good enough) param information, then get it from names.xml -->
  <xsl:function name="f:params-from-names-doc" as="element()*">
    <xsl:param name="params" as="element()*"/>
    <xsl:for-each select="$params">
      <xsl:copy>
        <xsl:copy-of select="@* except @type"/>
        <xsl:if test="@type">
          <xsl:attribute name="type" select="f:getKnownFullType(@type)"/>
        </xsl:if>
        <xsl:apply-templates select="type"/><!-- for parameters which are generic types -->
      </xsl:copy>
    </xsl:for-each>
  </xsl:function>

  <xsl:function name="f:extract-params" as="element()*">
    <!-- Returns empty sequence if this is a generic method, or any params have generic type (i.e.
      $in contains '{'), or any param is the generic param of the class (e.g. M:Saxon.Api.Predicate`1.matches(`0)) -->
    <xsl:param name="in" as="xs:string?"/>
    <xsl:variable name="paramTypes" select="substring-after($in, '(') => substring-before(')') => tokenize(',')"/>
    <!--<xsl:if test="exists($paramTypes[not(matches(., '[A-Za-z]'))])">
      <xsl:message>$paramTypes[not(matches(., '[A-Za-z]'))] <xsl:value-of select="$paramTypes[not(matches(., '[A-Za-z]'))]"/></xsl:message>
      <xsl:message>$in <xsl:value-of select="$in"/></xsl:message>
    </xsl:if>-->
    <!--<xsl:if test="contains($in, '(') and contains($in, '{')">
      <xsl:message>f:extract-params $in contains { <xsl:value-of select="$in"/></xsl:message>
    </xsl:if>-->
    <xsl:if test="contains($in, '(') and not(contains($in, '{')) and empty($paramTypes[not(matches(., '[A-Za-z]'))])">
      <xsl:for-each select="$paramTypes">
        <param type="{f:getKnownFullType(.)}"/>
      </xsl:for-each>
    </xsl:if>
  </xsl:function>
  
  <!-- Get the full type name for classes, interfaces, and enums in the Saxon.Api namespace.
      Note that this also works for nested classes (i.e. don't just prepend 'Saxon.Api'). -->
  <xsl:function name="f:saxonFullName" as="xs:string">
    <xsl:param name="class" as="element()"/><!-- class, interface, or enum element from names.xml -->
    <xsl:variable name="shortname" select="$class/@name"/>
    <xsl:variable name="apiclass" select="$api-doc//member[starts-with(@name, 'T:') and
      ((tokenize(@name, '\.')[last()] eq $shortname)
      or (substring-before(tokenize(@name, '\.')[last()], '`') eq $shortname))]"/>
    <!--<xsl:message>processing class <xsl:value-of select="$apiclass/@name, $shortname"/></xsl:message>-->
    <xsl:variable name="fullname" select="substring-after($apiclass/@name, ':')"/>
    <xsl:variable name="fullname" select="if (contains($fullname,'`')) then
      substring-before($fullname, '`') else $fullname"/>
    <xsl:sequence select="$fullname"/>
  </xsl:function>
  
  <xsl:key name="known-classes" match="using" use="@class"/>
  
  <!-- Resolve types for known Java types (as obtained from 'using' elements in names.xml), 
    and known .NET System types (maintained in $knownSystemClassesMap).
    Note that the types for Saxon.Api members are not resolved (use f:saxonFullName instead).
    Check the names.xml document 'using' elements for full type names from aliases (e.g. 'J' names); 
    be careful for lists (i.e. when the type ends with '[]') -->
  <xsl:function name="f:getKnownFullType" as="xs:string">
    <xsl:param name="in" as="xs:string"/><!-- $in might already be full type -->
    <xsl:variable name="in2" select="normalize-space($in)"/>
    <xsl:variable name="is-list" select="contains($in2, '[]')"/>
    <xsl:variable name="in3" select="if ($is-list) then translate($in2, '[]', '') else $in2"/>
    <xsl:variable name="known-class" select="key('known-classes', $in3, $names-doc)"/>
    <xsl:variable name="out" select="if ($known-class) then
      string($known-class/@fulltype) else if (map:contains($knownSystemClassesMap, $in3)) then
      map:get($knownSystemClassesMap, $in3) else $in3"/>
    <xsl:variable name="out" select="replace($out, 'Saxon.Hej.', 'net.sf.saxon.')"/>
    <xsl:variable name="out" select="replace($out, 'Saxon.Eej.', 'com.saxonica.')"/>
    <xsl:sequence select="if ($is-list) then concat($out,'[]') else $out"/>
  </xsl:function>
  
  <!-- Types which are known to be interfaces, rather than classes. TODO check use/status e.g. IResource -->
  <xsl:variable name="knownInterfaces" select="('IEnumerable', 'IInvalidityHandler', 'IDestination')"/>
  
  <!-- Resolve all known System types. Map from short name to resolved type. 
      This map requires maintenance. -->
  <xsl:variable name="knownSystemClassesMap" as="map(xs:string,xs:string)"
    select="map{
    'bool':'System.Boolean',
    'byte':'System.Byte',
    'char':'System.Char',
    'decimal':'System.Decimal',
    'double':'System.Double',
    'float':'System.Single',
    'int':'System.Int32',
    'long':'System.Int64',
    'object':'System.Object',
    'string':'System.String',
    'Action':'System.Action',
    'ArgumentException':'System.ArgumentException',
    'Boolean':'System.Boolean',
    'Char':'System.Char',
    'DateTime':'System.DateTime',
    'DateTimeOffset':'System.DateTimeOffset',
    'Decimal':'System.Decimal',
    'Double':'System.Double',
    'Exception':'System.Exception',
    'Func':'System.Func',
    'Object':'System.Object',
    'Predicate':'System.Predicate',
    'String':'System.String',
    'Uri':'System.Uri',
    'DictionaryEntry':'System.Collections.DictionaryEntry',
    'ICollection':'System.Collections.ICollection',
    'IEnumerator':'System.Collections.IEnumerator',
    'IEnumerable':'System.Collections.IEnumerable',
    'IList':'System.Collections.IList',
    'Dictionary':'System.Collections.Generic.Dictionary',
    'HashSet':'System.Collections.Generic.HashSet',
    'IDictionary':'System.Collections.Generic.IDictionary',
    'List':'System.Collections.Generic.List',
    'Stream':'System.IO.Stream',
    'TextReader':'System.IO.TextReader',
    'TextWriter':'System.IO.TextWriter',
    'Encoding':'System.Text.Encoding',
    'XmlDocument':'System.Xml.XmlDocument',
    'XmlNode':'System.Xml.XmlNode',
    'XmlNodeType':'System.Xml.XmlNodeType',
    'XmlQualifiedName':'System.Xml.XmlQualifiedName',
    'XmlResolver':'System.Xml.XmlResolver'
    }"/>
  
  
    
</xsl:stylesheet>  
 