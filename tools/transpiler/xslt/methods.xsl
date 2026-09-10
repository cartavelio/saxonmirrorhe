<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
   xmlns:xs="http://www.w3.org/2001/XMLSchema"
   xmlns:f="http://ns.saxonica.com/local-functions"
   xmlns:map="http://www.w3.org/2005/xpath-functions/map"
   exclude-result-prefixes="#all"
   expand-text="yes"
   default-mode="methods"
   version="3.0">
   
   <xsl:mode name="methods" on-no-match="fail" saxon:trace="no" xmlns:saxon="http://saxon.sf.net/"/>
   
   <!-- This module is concerned with method calls, especially rules for method
      calls that are specific to particular classes. All the rules in this 
      module are in mode "methods", and all pertain to expressions satisfying
      the pattern `match="*[@nodeType='MethodCallExpr']`, so this condition is
      not re-stated as part of each pattern.
   -->
   
   <xsl:template match="*" priority="1000">
      <!--<xsl:message>METHOD CALL {name/@identifier}() targetType={f:targetType(.)} 
         digest={key('digestClassKey', f:degenerify(@RESOLVED_TYPE), $digest)/@name}</xsl:message>-->
      <xsl:next-match/>
   </xsl:template>
   
   <xsl:template name="scopeIfPresent">
      <xsl:if test="exists(scope)">
         <xsl:apply-templates select="scope" mode="xml-to-cs"/>
         <xsl:text>.</xsl:text>
      </xsl:if>
   </xsl:template>
   
   <!-- Many of the rules for method calls use a simple templating language which is hopefully intuitive...-->
   <xsl:function name="f:formatCall" as="xs:string">
      <xsl:param name="methodCall" as="element(*)"/>
      <xsl:param name="layout" as="xs:string"/>
      <xsl:value-of>
         
         <xsl:analyze-string select="$layout" regex="%[^%]+%">
            <xsl:matching-substring>
               <xsl:choose>
                  <xsl:when test=".='%scope%'">
                     <xsl:apply-templates select="$methodCall/scope" mode="xml-to-cs">
                        <xsl:with-param name="indent" tunnel="yes" select="0"/>
                     </xsl:apply-templates>
                  </xsl:when>
                  <xsl:when test=".='%name%'">
                     <xsl:sequence select="string($methodCall/name/@identifier)"/>
                  </xsl:when>
                  <xsl:when test=".='%Name%'">
                     <xsl:sequence select="f:capitalize($methodCall/name/@identifier)"/>
                  </xsl:when>
                  <xsl:when test=".='%args%'">
                     <xsl:apply-templates select="$methodCall/arguments" mode="xml-to-cs">
                        <xsl:with-param name="indent" tunnel="yes" select="0"/>
                     </xsl:apply-templates>
                  </xsl:when>
                  <xsl:when test=".='%arg1%'">
                     <xsl:apply-templates select="$methodCall/arguments/argument[1]" mode="xml-to-cs">
                        <xsl:with-param name="indent" tunnel="yes" select="0"/>
                     </xsl:apply-templates>
                  </xsl:when>
                  <xsl:when test=".='%arg2%'">
                     <xsl:apply-templates select="$methodCall/arguments/argument[2]" mode="xml-to-cs">
                        <xsl:with-param name="indent" tunnel="yes" select="0"/>
                     </xsl:apply-templates>
                  </xsl:when>
                  <xsl:when test=".='%arg3%'">
                     <xsl:apply-templates select="$methodCall/arguments/argument[3]" mode="xml-to-cs">
                        <xsl:with-param name="indent" tunnel="yes" select="0"/>
                     </xsl:apply-templates>
                  </xsl:when>
                  <xsl:when test=".='%arg4%'">
                     <xsl:apply-templates select="$methodCall/arguments/argument[4]" mode="xml-to-cs">
                        <xsl:with-param name="indent" tunnel="yes" select="0"/>
                     </xsl:apply-templates>
                  </xsl:when>
                  <xsl:when test=".='%tail%'">
                     <xsl:apply-templates select="tail($methodCall/arguments/argument)" mode="xml-to-cs">
                        <xsl:with-param name="indent" tunnel="yes" select="0"/>
                     </xsl:apply-templates>
                  </xsl:when>
                  <xsl:otherwise>
                     <xsl:text>/* UNKNOWN TAG IN LAYOUT: "{.}"*/</xsl:text>
                     <xsl:message terminate="yes">/* UNKNOWN TAG IN LAYOUT: "{.}"*/</xsl:message>
                  </xsl:otherwise>
               </xsl:choose>
            </xsl:matching-substring>
            <xsl:non-matching-substring>
               <xsl:sequence select="."/>
            </xsl:non-matching-substring>
         </xsl:analyze-string>
         
      </xsl:value-of>
   </xsl:function>
   
   <xsl:function name="f:applyFormat" as="xs:string">
      <xsl:param name="methodCall" as="element(*)"/>
      <xsl:param name="rules" as="map(xs:string, xs:string)"/>
      <xsl:variable name="key" as="xs:string" 
         select="$methodCall/name/@identifier || '#' || count($methodCall/arguments/argument)"/>
      <xsl:variable name="rule" as="xs:string" 
         select="($rules($key), $rules('*'))[1]"/>
      <xsl:sequence select="f:formatCall($methodCall, $rule)"/>
   </xsl:function>
   
   <!--<xsl:template match="iterable[@nodeType='MethodCallExpr']
                                [starts-with(@RESOLVED_TYPE, 'java.util.Map')]
                                [name/@identifier='entrySet']" 
                                priority="30" mode="methods">
      <xsl:apply-templates select="scope" mode="xml-to-cs"/>
      <xsl:text>/* .entrySet() */</xsl:text>
   </xsl:template>-->
   
   <xsl:template match="*[name/@identifier='getClass'][empty(arguments/argument)]" priority="100" mode="methods">
      <xsl:choose>
         <xsl:when test="scope">
            <xsl:apply-templates select="scope" mode="xml-to-cs"/>
         </xsl:when>
         <xsl:otherwise>
            <xsl:text>this</xsl:text>
         </xsl:otherwise>
      </xsl:choose>     
      <xsl:text>.GetType()</xsl:text>
   </xsl:template>
   
   <xsl:template match="*[name/@identifier='hashCode'][empty(arguments/argument)]" priority="100" mode="methods">
      <xsl:call-template name="scopeIfPresent"/>
      <xsl:text>GetHashCode()</xsl:text>
   </xsl:template>
   
   <!-- Special for System.err.println() -->
   
   <xsl:template match="*[@nodeType='MethodCallExpr']
                           [name/@identifier='println']
                           [scope[@RESOLVED_TYPE='java.lang.System' and name/@identifier='err']]" 
                 priority="50">
      <text>System.Console.Error.WriteLine(</text>
      <xsl:apply-templates select="arguments" mode="xml-to-cs"/>
      <xsl:text>)</xsl:text>
   </xsl:template>
 
   
   <!-- Convert calls on delegate methods -->
   <!-- For example 'consumer.accept(item)' becomes 'consumer(item)' if consumer is a functional interface
        that converts to a C# delegate -->
   <xsl:template match="*[key('digestClassKey', f:degenerifyWithoutEscaping(@RESOLVED_TYPE), $digest)/@delegate='1']" 
      priority="30" mode="methods">     
      <xsl:text>/*Delegate.{name/@identifier}()*/</xsl:text>
      <xsl:apply-templates select="scope" mode="xml-to-cs"/>
      <xsl:text>(</xsl:text>
      <xsl:apply-templates select="arguments" mode="xml-to-cs"/>
      <xsl:text>)</xsl:text>
   </xsl:template>
   
   <!-- Calls on a system-supplied delegate type such as java.util.functions.Consumer -->
   <xsl:template match="*[starts-with(@RESOLVED_TYPE, 'java.util.function.')]" 
      priority="28" mode="methods">
      <!--<xsl:message>Function.{name/@identifier}()</xsl:message> -->     
      <xsl:apply-templates select="scope" mode="xml-to-cs"/>
      <xsl:text>(</xsl:text>
      <xsl:apply-templates select="arguments" mode="xml-to-cs"/>
      <xsl:text>)</xsl:text>
   </xsl:template>
   
   <!-- A method call of the form CSharp.emitCode("goto case 3") is recognised as indicating fall-through in a switch -->
   <xsl:template match="*[@RESOLVED_TYPE=('net.sf.saxon.transpile.CSharp')]" 
      priority="30" mode="methods">
      <!--<xsl:message>CSharp.{name/@identifier}()</xsl:message>-->
      <xsl:choose>
         <xsl:when test="name/@identifier = 'emitCode'">
            <xsl:text>{arguments/argument[1]/@value => translate('\', '')}</xsl:text>
         </xsl:when>
         <xsl:when test="name/@identifier = 'methodRef'">
            <xsl:apply-templates select="arguments/argument[1]" mode="xml-to-cs"/>
         </xsl:when>
         <xsl:when test="name/@identifier = 'staticRef'">
            <xsl:apply-templates select="arguments/argument[1]" mode="xml-to-cs"/>
         </xsl:when>
         <xsl:when test="name/@identifier = 'constructorRef'">
            <xsl:apply-templates select="arguments/argument[1]" mode="xml-to-cs"/>
         </xsl:when>
         <xsl:otherwise>
            <xsl:message terminate="yes">*** Unknown method CSharp.{name/@identifier} in {base-uri()}***</xsl:message>
         </xsl:otherwise>
      </xsl:choose>
      
   </xsl:template>
   
   
   <!-- Convert method calls applied to java.lang.String -->
   <xsl:template match="*[@RESOLVED_TYPE=('java.lang.String','java.lang.CharSequence')]" 
      priority="30" mode="methods">
      
      <xsl:sequence select="f:applyFormat(., map{
         (: TODO: equals() and hashCode() should use StringComparison.Ordinal :)
         'compareTo#1':       'System.String.Compare(%scope%, %arg1%, System.StringComparison.Ordinal)', 
         'charAt#1':          '%scope%[%args%]',
         'equalsIgnoreCase#1':'Saxon.Impl.Helpers.StringUtils.stringEqualsIgnoreCase(%scope%, %args%)',
         'getChars#4':        'Saxon.Impl.Helpers.StringUtils.stringToCharArray(%scope%, %args%)',
         'getBytes#1':        'Saxon.Impl.Helpers.StringUtils.EncodingForName(%args%).GetBytes(%scope%)',
         'hashCode#0':        '%scope%.GetHashCode()',
         'intern#0':          'System.String.Intern(%scope%)',
         'isEmpty#0':         '%scope%.Length==0',
         'length#0':          '%scope%.Length',
         'matches#1':         'System.Text.RegularExpressions.Regex.IsMatch(%scope%, %args%)',
         'replaceFirst#2':    'new System.Text.RegularExpressions.Regex(%arg1%).Replace(%scope%, %arg2%, 1)',
         'replaceAll#2':      'System.Text.RegularExpressions.Regex.Replace(%scope%, %args%)',
         'split#1':           'System.Text.RegularExpressions.Regex.Split(%scope%, %args%)',
         'substring#1':       '%scope%.Substring(%args%)',
         'substring#2':       '%scope%.Substring(%arg1%, %arg2% - %arg1%)',
         'subSequence#1':     '%scope%.Substring(%args%)',
         'subSequence#2':     '%scope%.Substring(%arg1%, %arg2% - %arg1%)',
         'toLowerCase#0':     '%scope%.ToLower()',
         'toUpperCase#0':     '%scope%.ToUpper()',
         '*':                 '%scope%.%Name%(%args%)' 
         })"/>
      
   </xsl:template>
   

   
   <xsl:template match="*[@RESOLVED_TYPE='java.lang.Integer']" priority="30" mode="methods">
      <xsl:sequence select="f:applyFormat(., map{
         'parseInt#1':     'Saxon.Impl.Helpers.Utils.ParseInt(%args%)',
         'parseInt#2':     'Saxon.Impl.Helpers.Utils.ParseInt(%args%)',
         'rotateRight#2':  'Saxon.Impl.Helpers.Utils.RotateRight(%args%)',
         'signum#1':       'System.Math.Sign(%args%)',
         'valueOf#1':      'System.Int32.Parse(%args%)',
         'compare#2':      '%arg1%.CompareTo(%arg2%)',
         'toHexString#1':  'Saxon.Impl.Helpers.Utils.intToHexString(%args%)',
         'toString#1':     '%args%.ToString()',
         '*':              '%scope%.%Name%(%args%)' 
         })"/>
   </xsl:template>
   
   <!-- Convert method calls applied to built-in numeric types (java.lang.Long) -->
   <xsl:template match="*[@RESOLVED_TYPE='java.lang.Long']" 
      priority="30" mode="methods">
      <xsl:choose>
         <xsl:when test="name/@identifier = 'parseLong' and count(arguments/argument) = 1">
            <xsl:text>System.Int64.Parse(</xsl:text>
            <xsl:apply-templates select="arguments" mode="xml-to-cs"/>
            <xsl:text>)</xsl:text>
         </xsl:when>
         <xsl:when test="name/@identifier = 'parseLong' 
                           and count(arguments/argument) = 2
                           and arguments/argument[2]/@value='16'">
            <xsl:text>System.Int64.Parse(</xsl:text>
            <xsl:apply-templates select="arguments/argument[1]" mode="xml-to-cs"/>
            <xsl:text>, System.Globalization.NumberStyles.HexNumber)</xsl:text>
         </xsl:when>
         <xsl:when test="name/@identifier = 'signum'">
            <xsl:text>System.Math.Sign(</xsl:text>
            <xsl:apply-templates select="arguments" mode="xml-to-cs"/>
            <xsl:text>)</xsl:text>
         </xsl:when>
         <xsl:when test="name/@identifier = 'compare'">
            <xsl:apply-templates select="arguments/argument[1]" mode="xml-to-cs"/>
            <xsl:text>.CompareTo(</xsl:text>
            <xsl:apply-templates select="arguments/argument[2]" mode="xml-to-cs"/>
            <xsl:text>)</xsl:text>
         </xsl:when>
         <xsl:when test="name/@identifier = 'toHexString'">
            <xsl:text>Saxon.Impl.Helpers.Utils.longToHexString(</xsl:text>
            <xsl:apply-templates select="arguments" mode="xml-to-cs"/>
            <xsl:text>)</xsl:text>
         </xsl:when>
         <xsl:otherwise>
            <xsl:apply-templates select="scope" mode="xml-to-cs"/>
            <xsl:text>.</xsl:text>
            <xsl:apply-templates select="f:capitalize(name/@identifier)" mode="xml-to-cs"/>
            <xsl:text>(</xsl:text>
            <xsl:apply-templates select="arguments" mode="xml-to-cs"/>
            <xsl:text>)</xsl:text>
         </xsl:otherwise>
      </xsl:choose>
   </xsl:template>
   
 
   
   <!-- Convert method calls applied to java.lang.Double -->
   <xsl:template match="*[@RESOLVED_TYPE='java.lang.Double']" 
      priority="30" mode="methods">
      <xsl:sequence select="f:applyFormat(., map{
         'parseDouble#1':         'Saxon.Impl.Helpers.Utils.ParseDouble(%args%)',
         'signum#1':              'System.Math.Sign(%args%)',
         'compare#2':             '%arg1%.CompareTo(%arg2%)',
         'isInfinite#1':          'System.Double.IsInfinity(%args%)',
         'valueOf#1':             '(%args%)',
         'doubleToLongBits#1':    'System.BitConverter.DoubleToInt64Bits(%args%)',
         'doubleToRawLongBits#1': 'System.BitConverter.DoubleToInt64Bits(%args%)',
         'toString#1':            '(%args%).ToString(System.Globalization.CultureInfo.InvariantCulture)',
         '*':                     '%scope%.%Name%(%args%)' 
         })"/>
   </xsl:template>
   
   <!-- Convert method calls applied to java.lang.Float -->
   <xsl:template match="*[@RESOLVED_TYPE='java.lang.Float']" 
      priority="30" mode="methods">
      <xsl:sequence select="f:applyFormat(., map{
         'parseFloat#1':          'System.Single.Parse(%args%, System.Globalization.CultureInfo.InvariantCulture)',
         'signum#0':              'System.Math.Sign(%args%)',
         'compare#2':             '%arg1%.CompareTo(%arg2%)',
         'isInfinite#1':          'System.Single.IsInfinity(%args%)',
         'valueOf#1':             '(%args%)',
         'floatToIntBits#1':      'System.BitConverter.SingleToInt32Bits(%args%)',
         'floatToRawIntBits#1':   'System.BitConverter.SingleToInt32Bits(%args%)',
         'toString#1':            '(%args%).ToString(System.Globalization.CultureInfo.InvariantCulture)',
         '*':                     '%scope%.%Name%(%args%)' 
         })"/>
   </xsl:template>
   
   <!-- Convert method calls applied to java.util.EnumSet -->
   <xsl:template match="*[starts-with(@RESOLVED_TYPE, 'java.util.EnumSet')]" 
      priority="30" mode="methods">
      <xsl:choose>
         <xsl:when test="name/@identifier = 'of'">
            <xsl:variable name="expandedArgs" as="xs:string*">
               <xsl:for-each select="arguments/argument">
                  <xsl:variable name="expandedArg" as="xs:string">
                     <xsl:value-of>
                        <xsl:apply-templates select="." mode="xml-to-cs"/>
                     </xsl:value-of>
                  </xsl:variable>
                  <xsl:sequence select="$expandedArg"/>
               </xsl:for-each>
            </xsl:variable>
            <xsl:text>(</xsl:text>
            <xsl:value-of select="$expandedArgs" separator="|"/>
            <xsl:text>)</xsl:text>
         </xsl:when>
         <xsl:when test="name/@identifier = 'copyOf'">
            <xsl:text>(</xsl:text>
            <xsl:apply-templates select="arguments" mode="xml-to-cs"/>
            <xsl:text>)</xsl:text>
         </xsl:when>
         <xsl:when test="name/@identifier = 'noneOf'">
            <xsl:text>{arguments/argument[1]/type/@RESOLVED_TYPE => f:adjustTypeNamespace()}.NONE</xsl:text>
         </xsl:when>
         <xsl:when test="name/@identifier = 'contains'">
            <xsl:apply-templates select="scope" mode="xml-to-cs"/>
            <xsl:text>.HasFlag(</xsl:text>
            <xsl:apply-templates select="arguments" mode="xml-to-cs"/>
            <xsl:text>)</xsl:text>
         </xsl:when>
         <xsl:when test="name/@identifier = 'add'">
            <!-- assume context is a statement like `propSet.add(property)` -->
            <xsl:apply-templates select="scope" mode="xml-to-cs"/>
            <xsl:text> |= (</xsl:text>
            <xsl:apply-templates select="arguments" mode="xml-to-cs"/>
            <xsl:text>)</xsl:text>
         </xsl:when>
         <xsl:when test="name/@identifier = 'remove'">
            <!-- assume context is a statement like `propSet.remove(property)` -->
            <xsl:apply-templates select="scope" mode="xml-to-cs"/>
            <xsl:text> &amp;= ~(</xsl:text>
            <xsl:apply-templates select="arguments" mode="xml-to-cs"/>
            <xsl:text>)</xsl:text>
         </xsl:when>
         <xsl:when test="name/@identifier = 'retainAll'">
            <!-- assume context is a statement like `propSet.retainAll(property)` -->
            <xsl:apply-templates select="scope" mode="xml-to-cs"/>
            <xsl:text> &amp;= (</xsl:text>
            <xsl:apply-templates select="arguments" mode="xml-to-cs"/>
            <xsl:text>)</xsl:text>
         </xsl:when>
         <xsl:otherwise>
            <xsl:next-match/>
         </xsl:otherwise>
      </xsl:choose>
   </xsl:template>
   
   <!-- Convert method calls applied to net.sf.saxon.om.EnumSetTool -->
   <xsl:template match="*[@RESOLVED_TYPE = 'net.sf.saxon.om.EnumSetTool']" 
      priority="30" mode="methods">
      <xsl:choose>
         <xsl:when test="name/@identifier = 'intersect'">
            <xsl:variable name="expandedArgs" as="xs:string*">
               <xsl:for-each select="arguments/argument">
                  <xsl:variable name="expandedArg" as="xs:string">
                     <xsl:value-of>
                        <xsl:apply-templates select="." mode="xml-to-cs"/>
                     </xsl:value-of>
                  </xsl:variable>
                  <xsl:sequence select="$expandedArg"/>
               </xsl:for-each>
            </xsl:variable>
            <xsl:text>(</xsl:text>
            <xsl:value-of select="$expandedArgs" separator="&amp;"/>
            <xsl:text>)</xsl:text>
         </xsl:when>
         <xsl:when test="name/@identifier = 'union'">
            <xsl:variable name="expandedArgs" as="xs:string*">
               <xsl:for-each select="arguments/argument">
                  <xsl:variable name="expandedArg" as="xs:string">
                     <xsl:value-of>
                        <xsl:apply-templates select="." mode="xml-to-cs"/>
                     </xsl:value-of>
                  </xsl:variable>
                  <xsl:sequence select="$expandedArg"/>
               </xsl:for-each>
            </xsl:variable>
            <xsl:text>(</xsl:text>
            <xsl:value-of select="$expandedArgs" separator="|"/>
            <xsl:text>)</xsl:text>
         </xsl:when>
         <xsl:otherwise>
            <xsl:next-match/>
         </xsl:otherwise>
      </xsl:choose>
   </xsl:template>
   
   <!--<!-\- Convert method calls applied to java.util.Optional (which translates to System.Nullable) -\->
   <xsl:template match="*[starts-with(@RESOLVED_TYPE, 'java.util.Optional')]" 
      priority="30" mode="methods">
      <xsl:choose>
         <xsl:when test="name/@identifier = 'of'">
            <xsl:apply-templates select="arguments" mode="xml-to-cs"/>
         </xsl:when>
         <xsl:when test="name/@identifier = 'empty'">
            <xsl:text>null</xsl:text>
         </xsl:when>
         <xsl:when test="name/@identifier = 'isEmpty'">
            <xsl:text>!</xsl:text>
            <xsl:apply-templates select="scope" mode="xml-to-cs"/>
            <xsl:text>.HasValue</xsl:text>
         </xsl:when>
         <xsl:when test="name/@identifier = 'isPresent'">
            <xsl:variable name="paramType" select="f:extract-type-arguments(@RESOLVED_TYPE)"/>
            <xsl:message>paramType = {$paramType}</xsl:message>
            <xsl:choose>
               <xsl:when test="matches($paramType, 'System\.(Boolean|Int32|Int64)')">
                  <xsl:apply-templates select="scope" mode="xml-to-cs"/>
                  <xsl:text>.HasValue</xsl:text>
               </xsl:when>
               <xsl:otherwise>
                  <xsl:text>(</xsl:text>
                  <xsl:apply-templates select="scope" mode="xml-to-cs"/>
                  <xsl:text> != null)</xsl:text>
               </xsl:otherwise>
            </xsl:choose>           
         </xsl:when>
         <xsl:when test="name/@identifier = 'get'">
            <xsl:variable name="paramType" select="f:extract-type-arguments(@RESOLVED_TYPE)"/>
            <xsl:choose>
               <xsl:when test="matches($paramType, 'System\.(Boolean|Int32|Int64)')">
                  <xsl:apply-templates select="scope" mode="xml-to-cs"/>
                  <xsl:text>.Value</xsl:text>
               </xsl:when>
               <xsl:otherwise>
                  <xsl:apply-templates select="scope" mode="xml-to-cs"/>
               </xsl:otherwise>
            </xsl:choose>  
         </xsl:when>
         <xsl:otherwise>
            <xsl:next-match/>
         </xsl:otherwise>
      </xsl:choose>
   </xsl:template>
   -->
   <!-- Convert method calls applied to java.net.URI (which translates to System.Uri) -->
   <xsl:template match="*[@RESOLVED_TYPE='java.net.URI']" 
      priority="30" mode="methods">
      <xsl:sequence select="f:applyFormat(., map{
         'create#1':                  'new System.Uri(%args%, System.UriKind.RelativeOrAbsolute)',
         'getAuthority#0':            '%scope%.Authority',
         'getFragment#0':             'Saxon.Impl.Helpers.UriUtils.GetFragment(%scope%)',
         'getHost#0':                 '%scope%.Host',
         'getPath#0':                 '%scope%.LocalPath',
         'getPort#0':                 '%scope%.Port',
         'getQuery#0':                'Saxon.Impl.Helpers.UriUtils.GetQuery(%scope%)',
         'getRawAuthority#0':         '%scope%.Authority',
         'getRawFragment#0':          '%scope%.Fragment',
         'getRawPath#0':              '%scope%.LocalPath',
         'getRawQuery#0':             '%scope%.Query',
         'getRawSchemeSpecificPart#0':'%scope%.PathAndQuery',
         'getRawUserInfo#0':          '%scope%.UserInfo',
         'getScheme#0':               '%scope%.Scheme',
         'getSchemeSpecificPart#0':   '%scope%.GetComponents(System.UriComponents.PathAndQuery, System.UriFormat.Unescaped)',
         'getUserInfo#0':             '%scope%.UserInfo',
         'isAbsolute#0':              '%scope%.IsAbsoluteUri',
         'isOpaque#0':                'Saxon.Impl.Helpers.UriUtils.IsOpaque(%scope%)',
         'normalize#0':               'Saxon.Impl.Helpers.UriUtils.Normalize(%scope%)',
         'resolve#1':                 'new System.Uri(%scope%, %args%)',
         'toASCIIString#0':           '%scope%.OriginalString',
         'toURL#0':                   'Saxon.Impl.Helpers.UriUtils.ToURL(%scope%)',
         '*':                         '%scope%.%Name%(%args%)' 
         })"/>
 

   </xsl:template>
   
   <!-- Convert method calls applied to Exception classes -->
   <xsl:template match="*[(exists(@RESOLVED_TYPE) and f:isExceptionType(@RESOLVED_TYPE)) 
             or (exists(@DECLARING_TYPE) and f:isExceptionType(@DECLARING_TYPE))]" priority="25">
      <!--<xsl:message>Exception.{name/@identifier}()</xsl:message>-->
      <xsl:choose>
         <xsl:when test="name/@identifier = 'getMessage'">
            <xsl:call-template name="scopeIfPresent"/>
            <xsl:text>Message</xsl:text>
         </xsl:when>
         <xsl:when test="name/@identifier = 'getCause'">
            <xsl:call-template name="scopeIfPresent"/>
            <xsl:text>InnerException</xsl:text>
         </xsl:when>
         <xsl:when test="name/@identifier = 'printStackTrace'">
            <xsl:text>System.Console.Error.WriteLine(</xsl:text>
            <xsl:apply-templates select="scope" mode="xml-to-cs"/>
            <xsl:text>)</xsl:text>
         </xsl:when>
         <xsl:otherwise>
            <xsl:call-template name="scopeIfPresent"/>
            <xsl:apply-templates select="name/@identifier" mode="xml-to-cs"/>
            <xsl:text>(</xsl:text>
            <xsl:apply-templates select="arguments" mode="xml-to-cs"/>
            <xsl:text>)</xsl:text>
         </xsl:otherwise>
      </xsl:choose>
   </xsl:template>
   
   <!-- Convert .ordinal() and .values() methods on an enumeration value -->
   
   <xsl:template match="*[name/@identifier=('ordinal', 'values')]
         [empty(arguments/argument)]
         [key('digestClassKey', @RESOLVED_TYPE, $digest)[self::enum]]" 
      priority="30" mode="methods">
      <xsl:choose>
         <xsl:when test="name/@identifier='ordinal'">
            <xsl:text>((int)</xsl:text>
            <xsl:apply-templates select="scope" mode="xml-to-cs"/>
            <xsl:text>)</xsl:text>
         </xsl:when>
         <xsl:when test="name/@identifier='values' and @RESOLVED_TYPE = 'net.sf.saxon.type.PrimitiveUType'">
            <!-- TODO: avoid special-casing this! Needed for UType.decompose() method -->
            <xsl:text>Saxon.Hej.type.PrimitiveUType.values()</xsl:text>
         </xsl:when>
         <xsl:when test="name/@identifier='values'">
            <xsl:text>System.Enum.GetValues(typeof({@RESOLVED_TYPE => f:adjustTypeNamespace()}))</xsl:text>
         </xsl:when>
      </xsl:choose>      
   </xsl:template>
   
   <!-- `MyEnum.valueOf(str)` where MyEnum is a simple enumeration class
        becomes `(MyEnum) Enum.Parse(typeof(MyEnum), str)` -->
   
   <xsl:template match="*[name/@identifier=('valueOf')]
      [count(arguments/argument) eq 1]
      [arguments/argument[1][(@RETURN, @RESOLVED_TYPE)='java.lang.String' 
                             or @nodeType='StringLiteralExpr']]
      [key('digestClassKey', @RESOLVED_TYPE, $digest)[self::enum][@simple='1']]" 
      priority="30" mode="methods">
      <xsl:text>({f:adjustTypeNamespace(@RESOLVED_TYPE)})Enum.Parse(typeof({f:adjustTypeNamespace(@RESOLVED_TYPE)}), </xsl:text>
      <xsl:apply-templates select="arguments" mode="xml-to-cs"/>
      <xsl:text>)</xsl:text>
   </xsl:template>
   
   <!-- Convert method calls applied to java.lang.Class (which becomes System.Type) -->
   <xsl:template match="*[@RESOLVED_TYPE = 'java.lang.Class' or starts-with(@RESOLVED_TYPE, 'java.lang.Class&lt;')]" 
      priority="30" mode="methods">
      <!--<xsl:message>Class.{name/@identifier}()</xsl:message>-->
      <xsl:variable name="renamedMethods" as="map(*)" select="map{}"/>
      <xsl:choose>
         <xsl:when test="name/@identifier = 'getName'">
            <xsl:apply-templates select="scope" mode="xml-to-cs"/>
            <xsl:text>.Name</xsl:text>
         </xsl:when>
         <xsl:when test="name/@identifier = 'getSimpleName'">
            <xsl:apply-templates select="scope" mode="xml-to-cs"/>
            <xsl:text>.Name</xsl:text>
         </xsl:when>
         <xsl:when test="name/@identifier = 'getSuperClass'">
            <xsl:apply-templates select="scope" mode="xml-to-cs"/>
            <xsl:text>.BaseType</xsl:text>
         </xsl:when>
         <xsl:when test="name/@identifier = 'isArray'">
            <xsl:apply-templates select="scope" mode="xml-to-cs"/>
            <xsl:text>.IsArray</xsl:text>
         </xsl:when>
         <xsl:when test="name/@identifier = 'isInterface'">
            <xsl:apply-templates select="scope" mode="xml-to-cs"/>
            <xsl:text>.IsInterface</xsl:text>
         </xsl:when>
         <xsl:when test="name/@identifier = 'newInstance'">
            
            <!--<expression nodeType="MethodCallExpr" RETURN="T"
               RESOLVED_TYPE="java.lang.Class&lt;? extends net.sf.saxon.Configuration>">
               <name nodeType="SimpleName" identifier="newInstance"/>
               <scope nodeType="NameExpr"
                  RESOLVED_TYPE="java.lang.Class&lt;? extends net.sf.saxon.Configuration>">
                  <name nodeType="SimpleName" identifier="configurationClass"/>
               </scope>
            </expression>-->
            
            <xsl:variable name="classExpr">
               <xsl:apply-templates select="scope" mode="xml-to-cs"/>
            </xsl:variable>
            
            <xsl:if test="starts-with(@RESOLVED_TYPE, 'java.lang.Class&lt;? extends ')">
               <xsl:text>({replace(@RESOLVED_TYPE, 'java\.lang\.Class&lt;\? extends ', '') => replace('>$', '') => f:adjustTypeNamespace()})</xsl:text>
            </xsl:if>
            <xsl:text>System.Activator.CreateInstance({$classExpr})</xsl:text>
         </xsl:when>
         <xsl:otherwise>
            <xsl:call-template name="scopeIfPresent"/>
            <xsl:apply-templates select="f:capitalize(name/@identifier)" mode="xml-to-cs"/>
            <xsl:text>(</xsl:text>
            <xsl:apply-templates select="arguments" mode="xml-to-cs"/>
            <xsl:text>)</xsl:text>
         </xsl:otherwise>
      </xsl:choose>
   </xsl:template>
   
   <!-- Convert method calls applied to java.lang.Math (which becomes System.Math) -->
   <xsl:template match="*[@RESOLVED_TYPE = 'java.lang.Math']" priority="30" mode="methods">
      <xsl:sequence select="f:applyFormat(., map{
         'random#0':          '(new System.Random()).NextDouble()',
         'round#1':           '(long)System.Math.Round(%args%, System.MidpointRounding.AwayFromZero)',
         'ceil#1':            'System.Math.Ceiling(%args%)',
         'addExact#2':        'Saxon.Ejava.lang.Math.addExact(%args%)',
         'multiplyExact#2':   'Saxon.Ejava.lang.Math.multiplyExact(%args%)',
         '*':                 'System.Math.%Name%(%args%)' 
         })"/>
   </xsl:template>
   
   <!-- Convert method calls applied to java.math.BigInteger (which becomes System.Numerics.BigInteger) -->
   <xsl:template match="*[@RESOLVED_TYPE = 'java.math.BigInteger']" 
      priority="20" mode="methods">
      <xsl:sequence select="f:applyFormat(., map{
         'abs#0':                'System.Numerics.BigInteger.Abs(%scope%)',
         'add#1':                'System.Numerics.BigInteger.Add(%scope%, %args%)',
         'subtract#1':           'System.Numerics.BigInteger.Subtract(%scope%, %args%)',
         'multiply#1':           'System.Numerics.BigInteger.Multiply(%scope%, %args%)',
         'divide#1':             'System.Numerics.BigInteger.Divide(%scope%, %args%)',
         'negate#0':             'System.Numerics.BigInteger.Negate(%scope%)',
         'mod#1':                'Saxon.Impl.Helpers.BigIntegerUtils.Mod(%scope%, %args%)',
         'remainder#1':          'Saxon.Impl.Helpers.BigIntegerUtils.Remainder(%scope%, %args%)',
         'divideAndRemainder#1': 'Saxon.Impl.Helpers.BigIntegerUtils.DivideAndRemainder(%scope%, %args%)',
         'signum#0':             'Saxon.Impl.Helpers.BigIntegerUtils.Signum(%scope%)',
         'valueOf#1':            'Saxon.Impl.Helpers.BigIntegerUtils.ValueOf(%args%)',
         'longValue#0':          '((long)%scope%)',
         'longValueExact#0':     'Saxon.Impl.Helpers.BigIntegerUtils.LongValueExact(%scope%)',
         'intValue#0':           '((int)%scope%)',
         'intValueExact#0':      'Saxon.Impl.Helpers.BigIntegerUtils.IntValueExact(%scope%)',
         'doubleValue#0':        '((double)%scope%)',
         'pow#1':                'System.Numerics.BigInteger.Pow(%scope%, %args%)',
         'shiftLeft#1':          '((%scope%) &lt;&lt; (%args%))',
         'toByteArray#0':        '%scope%.ToByteArray(isUnsigned: false, isBigEndian: true)',
         '*':                    '%scope%.%Name%(%args%)'
         })"/>
   </xsl:template>
   
   <!-- Convert method calls applied to java.math.BigDecimal (which becomes Singulink.Numerics.BigDecimal) -->
   <xsl:template match="*[@RESOLVED_TYPE = 'java.math.BigDecimal']" 
      priority="20" mode="methods">
      <xsl:sequence select="f:applyFormat(., map{
         'abs#0':           'Singulink.Numerics.BigDecimal.Abs(%scope%)',
         'add#1':           '(%scope%+%args%)',
         'subtract#1':      '(%scope%-%args%)',
         'multiply#1':      '(%scope%*%args%)',
         'divide#1':        'Singulink.Numerics.BigDecimal.Divide(%scope%, %args%, 18)',
         'divide#2':        'Singulink.Numerics.BigDecimal.Divide(%scope%, %args%)',
         'divide#3':        'Singulink.Numerics.BigDecimal.Divide(%scope%, %args%)',
         'negate#0':        '-%scope%',
         'mod#1':           'Saxon.Impl.Helpers.BigDecimalUtils.Mod(%scope%, %args%)',
         'signum#0':        '%scope%.Sign',
         'remainder#1':     'Saxon.Impl.Helpers.BigDecimalUtils.Remainder(%scope%, %args%)',
         'divideToIntegralValue#1':     
                            'Saxon.Impl.Helpers.BigDecimalUtils.Idiv(%scope%, %args%)',
         'divideAndRemainder#1':        
                            'Saxon.Impl.Helpers.BigDecimalUtils.DivideAndRemainder(%scope%, %args%)',
         'valueOf#1':        'Saxon.Impl.Helpers.BigDecimalUtils.ValueOf(%args%)',
         'intValue#0':       '((int)%scope%)',
         'longValue#0':      '((long)%scope%)',
         'doubleValue#0':    '((double)%scope%)',
         'floatValue#0':     '((float)%scope%)',
         'intValueExact#0':  'Saxon.Impl.Helpers.BigDecimalUtils.IntValueExact(%scope%)',
         'longValueExact#0': 'Saxon.Impl.Helpers.BigDecimalUtils.LongValueExact(%scope%)',
         'setScale#1':       '%scope%', (:no-op, values are normalized:)
         'setScale#2':       '%scope%', (:no-op, values are normalized:)
         'stripTrailingZeros#0':       '%scope%', (:no-op, values are normalized:)
         'toBigInteger#0':   '((System.Numerics.BigInteger)%scope%)',
         '*':                '%scope%.%Name%(%args%)' 
         })"/>

   </xsl:template>
   
   <!-- Special-case methods on java.lang.Character -->
   <!-- Note, the result fofr getType() is of type UnicodeCategory, which will cause problems if it is bound to a variable -->
   <xsl:template match="*[starts-with(@RESOLVED_TYPE, 'java.lang.Character')]" 
      priority="20" mode="methods">
      <xsl:sequence select="f:applyFormat(., map{
         'getType#1':           'System.Globalization.CharUnicodeInfo.GetUnicodeCategory(%args%)',
         'toLowerCase#1':     'System.Char.ToLowerInvariant(%args%)',
         'toUpperCase#1':     'System.Char.ToUpperInvariant(%args%)',
         '*':                   '%scope%.%Name%(%args%)'
         })"/>
   </xsl:template>
   

   <!--<!-\- Special-case EnumSet.noneOf(Class<E> x) and EnumSet.of(Class.ENUM), etc-\->
   <xsl:template match="*[starts-with(@RESOLVED_TYPE, 'java.util.EnumSet')]" 
      priority="20" mode="methods">
    <!-\-  <xsl:message>EnumSet.{name/@identifier}()</xsl:message>-\->
      <xsl:choose>
         <xsl:when test="name/@identifier = 'noneOf' and arguments/argument[1]/@nodeType='ClassExpr'">
            <xsl:text>java.util.EnumSet&lt;{arguments/argument[1]/type/@RESOLVED_TYPE}>.noneOf()</xsl:text>
         </xsl:when>
         <xsl:when test="name/@identifier = 'of'  and arguments/argument[1]/@nodeType='FieldAccessExpr'">
            <xsl:text>java.util.EnumSet&lt;{arguments/argument[1]/@RESOLVED_TYPE}>.of(</xsl:text>
            <xsl:apply-templates select="arguments" mode="xml-to-cs"/>
            <xsl:text>)</xsl:text>
         </xsl:when>
         <xsl:otherwise>
            <xsl:next-match/>
         </xsl:otherwise>
      </xsl:choose>
   </xsl:template>-->
   
   <!--Handle List, ArrayList -->
   <xsl:template match="*[f:degenerifyWithoutEscaping(@RESOLVED_TYPE) = 
      ('java.util.List', 'java.util.ArrayList', 'java.util.Collection')]" 
      priority="20" mode="methods">
      <xsl:sequence select="f:applyFormat(., map{
         'add#2':         '%scope%.Insert(%args%)',
         'addAll#1':      'Saxon.Impl.Helpers.ListUtils.AddAll(%scope%, %args%)',
         'get#1':         '%scope%[%args%]',
         'getFirst#0':    '%scope%.First.Value',
         'isEmpty#0':     '%scope%.Count==0',
         'iterator#0':    '%scope%.GetEnumerator()',
         'offer#1':       '%scope%.AddLast(%args%)',
         'remove#1':      'Saxon.Impl.Helpers.ListUtils.Remove(%scope%, %args%)',
         'removeFirst#0': '%scope%.RemoveFirst()',
         'set#2':         '%scope%[%arg1%] = %arg2%',
         'size#0':        '%scope%.Count',
         'sort#1':        'Saxon.Impl.Helpers.ListUtils.Sort(%scope%, %args%)',
         'subList#2':     'Saxon.Impl.Helpers.ListUtils.SubList(%scope%, %args%)',
         'toArray#1':     '%scope%.ToArray()', (: Linq extension method :)
         'trimToSize#0':  'Saxon.Impl.Helpers.ListUtils.TrimToSize(%scope%)',
         '*':             '%scope%.%Name%(%args%)' 
         })"/>
   </xsl:template>
   
   <!--Handle LinkedList -->
   <xsl:template match="*[f:degenerifyWithoutEscaping(@RESOLVED_TYPE) = 'java.util.LinkedList']" 
      priority="20" mode="methods">
      <xsl:sequence select="f:applyFormat(., map{
         'add#1':         '%scope%.AddLast(%args%)',
         'add#2':         '%scope%.Insert(%args%)',
         'addAll#1':      'Saxon.Impl.Helpers.LinkedListUtils.AddAll(%scope%, %args%)',
         'addFirst#1':    '%scope%.AddFirst(%args%)',
         'get#1':         '%scope%[%args%]',
         'getFirst#0':    '%scope%.First.Value',
         'isEmpty#0':     '%scope%.Count==0',
         'iterator#0':    '%scope%.GetEnumerator()',
         'offer#1':       '%scope%.AddLast(%args%)',
         'peekFirst#0':   '%scope%.First.Value',
         'pollFirst#0':   'Saxon.Impl.Helpers.LinkedListUtils.PollFirst(%scope%)',
         'remove#1':      '%scope%.Remove(%args%)',
         'removeFirst#0': '%scope%.RemoveFirst()',
         'set#2':         '%scope%[%arg1%] = %arg2%',
         'size#0':        '%scope%.Count',
         'toArray#1':     '%scope%.ToArray()', (: Linq extension method :)
         '*':             '%scope%.%Name%(%args%)' 
         })"/>
   </xsl:template>
   
   <!-- java.util.Stack -->
   <xsl:template match="*[f:targetType(.) = 'java.util.Stack']" 
      priority="20" mode="methods">
      <xsl:sequence select="f:applyFormat(., map{
         'empty#0':       '(%scope%.Count==0)',
         'isEmpty#0':     '(%scope%.Count==0)',
         'size#0':        '%scope%.Count',
         'addAll#1':      'Saxon.Impl.Helpers.StackUtils.AddAll(%scope%, %args%)',
         '*':             '%scope%.%Name%(%args%)' 
         })"/>
   </xsl:template>
   
   <!-- Map, HashMap, etc -->
   <xsl:template match="*[f:targetType(.) = 
         ('java.util.Map', 'java.util.HashMap', 'java.util.concurrent.ConcurrentHashMap',
         'java.util.TreeMap',
         'java.util.LinkedHashMap', 'java.util.IdentityHashMap',
         'net.sf.saxon.type.ValidationParams')]" 
         priority="20" mode="methods">
      <xsl:choose>
         <xsl:when test="name/@identifier='entrySet' and count(arguments/argument)=0 and self::iterable">
            <!-- special case entrySet() when it appears as `for (Map.Entry<K,V> : map.entrySet())` -->
            <xsl:apply-templates select="scope" mode="xml-to-cs"/>
         </xsl:when>
         <xsl:otherwise>
            <xsl:sequence select="f:applyFormat(., map{
               'ceilingKey#1':          'Saxon.Impl.Helpers.TreeMapUtils.CeilingKey(%scope%, %args%)',
               'computeIfAbsent#2':     'Saxon.Impl.Helpers.MapUtils.ComputeIfAbsent(%scope%, %args%)',
               'firstKey#0':            'Saxon.Impl.Helpers.TreeMapUtils.FirstKey(%scope%)',
               'floorKey#1':            'Saxon.Impl.Helpers.TreeMapUtils.FloorKey(%scope%, %args%)',
               'get#1':                 'Saxon.Impl.Helpers.MapUtils.Get(%scope%, %args%)',
               'getOrDefault#2':        'Saxon.Impl.Helpers.MapUtils.GetOrDefault(%scope%, %args%)',
               'higherKey#1':           'Saxon.Impl.Helpers.TreeMapUtils.HigherKey(%scope%, %args%)',
               'isEmpty#0':             '%scope%.Count==0',
               'keySet#0':              'Saxon.Impl.Helpers.MapUtils.KeySet(%scope%)',
               'lastKey#0':             'Saxon.Impl.Helpers.TreeMapUtils.FirstKey(%scope%)',
               'merge#3':               'Saxon.Impl.Helpers.MapUtils.Merge(%scope%, %args%)',
               'put#2':                 'Saxon.Impl.Helpers.MapUtils.Put(%scope%, %args%)',
               (:'put#2':                 '%scope%[%arg1%] = %arg2%', :)
               'putIfAbsent#2':         'Saxon.Impl.Helpers.MapUtils.PutIfAbsent(%scope%, %args%)',
               'putAll#1':              'Saxon.Impl.Helpers.MapUtils.PutAll(%scope%, %args%)',
               'setValidationParams#3': '%scope%.%name%(%args%)', 
               'size#0':                '%scope%.Count',
               'values#0':              '%scope%.Values',
               '*':                     '%scope%.%Name%(%args%)' 
               })"/>     
         </xsl:otherwise>
      </xsl:choose>
   </xsl:template>
   
   <!--Handle BitSet -->
   <xsl:template match="*[f:degenerifyWithoutEscaping(@RESOLVED_TYPE) = 'java.util.BitSet']" 
      priority="20" mode="methods">
      <xsl:sequence select="f:applyFormat(., map{
         'get#1':         '%scope%[%args%]',
         'set#1':         '%scope%.Set(%args%, true)'
         })"/>
   </xsl:template>
   
   <!-- ImmutableHashTrieMap -->
   <xsl:template match="*[f:targetType(.) = ('io.vavr.collection.HashMap', 'io.vavr.collection.LinkedHashMap')]" 
      priority="20" mode="methods">
      <xsl:choose>
         <xsl:when test="name/@identifier='entrySet' and count(arguments/argument)=0 and self::iterable">
            <!-- special case entrySet() when it appears as `for (Map.Entry<K,V> : map.entrySet())` -->
            <xsl:apply-templates select="scope" mode="xml-to-cs"/>
         </xsl:when>
         <xsl:otherwise>
            <xsl:sequence select="f:applyFormat(., map{
               'get#1':             'Saxon.Impl.Helpers.MapUtils.Get(%scope%, %args%)',
               'getOrDefault#2':    'Saxon.Impl.Helpers.MapUtils.GetOrDefault(%scope%, %args%)',
               'empty#0':           'new System.Collections.Immutable.ImmutableDictionary()',
               'isEmpty#0':         '%scope%.Count==0',
               'keySet#0':          'Saxon.Impl.Helpers.MapUtils.KeySet(%scope%)',
               'put#2':             '%scope%.SetItem(%args%)',
               'size#0':            '%scope%.Count',
               'values#0':          '%scope%.Values',
               '*':                 '%scope%.%Name%(%args%)' 
               })"/>     
         </xsl:otherwise>
      </xsl:choose>
   </xsl:template>
   
   <!-- net.sf.saxon.ma.trie.ImmutableList -->
   
   <xsl:template match="*[f:targetType(.) = 'net.sf.saxon.ma.trie.ImmutableList']" 
      priority="20" mode="methods">
            <xsl:sequence select="f:applyFormat(., map{
               'prepend#1':         '%scope%.Insert(0, %args%)',
               'reverse#0':         '%scope%.Reverse()',
               '*':                 '%scope%.%Name%(%args%)' 
               })"/>     
   </xsl:template>
   
   
   <!--net.sf.saxon.ma.trie.TrieKVP-->
   <xsl:template match="*[f:targetType(.) = 'net.sf.saxon.ma.trie.TrieKVP']" 
      priority="20" mode="methods">
      <xsl:sequence select="f:applyFormat(., map{
         'getKey#0':          '%scope%.Key',
         'getValue#0':        '%scope%.Value',
         '*':                 '%scope%.%Name%(%args%)' 
         })"/>     
   </xsl:template>
   
   
   <!-- java.util.Map.Entry (which becomes Collections.Generic.KeyValuePair) -->
   <xsl:template match="*[f:targetType(.) = 'java.util.Map.Entry']" 
      priority="20" mode="methods">
      <xsl:choose>
         <xsl:when test="name/@identifier='entrySet' and count(arguments/argument)=0 and self::iterable">
            <!-- special case entrySet() when it appears as `for (Map.Entry<K,V> : map.entrySet())` -->
            <xsl:apply-templates select="scope" mode="xml-to-cs"/>
         </xsl:when>
         <xsl:otherwise>
            <xsl:sequence select="f:applyFormat(., map{
               'getKey#0':   '%scope%.Key',
               'getValue#0': '%scope%.Value',
               '*':          '%scope%.%Name%(%args%)' 
               })"/>     
         </xsl:otherwise>
      </xsl:choose>
   </xsl:template>
   
   <!--Handle Set, HashSet, TreeSet -->
   <xsl:template match="*[f:targetType(.) = ('java.util.Set', 'java.util.HashSet', 'java.util.TreeSet')]" 
      priority="20" mode="methods">
      <xsl:sequence select="f:applyFormat(., map{
         'add#1':             '%scope%.Add(%args%)',
         'addAll#1':          'Saxon.Impl.Helpers.SetUtils.AddAll(%scope%, %args%)',
         'contains#1':        '%scope%.Contains(%args%)',
         'containsAll#1':     '(%args%).All(%scope%.Contains)',
         'isEmpty#0':         '%scope%.Count==0',
         'iterator#0':        '%scope%.GetEnumerator()',
         'pollFirst#0':       'Saxon.Impl.Helpers.SetUtils.PollFirst(%scope%)',
         'remove#1':          '%scope%.Remove(%args%)',
         'size#0':            '%scope%.Count',
         'values#0':          '%scope%.Values',
         '*':                 '%scope%.%Name%(%args%)' 
         })"/>     
   </xsl:template>
   
   <xsl:template match="*[f:targetType(.) = 'net.sf.saxon.expr.parser.PathMap.PathMapNodeSet']" 
      priority="20" mode="methods">
      <!-- Same as Set and HashSet, except for the default action, which doesn't capitalize -->
      <xsl:sequence select="f:applyFormat(., map{
         'add#1':             '%scope%.Add(%args%)',
         'addAll#1':          'Saxon.Impl.Helpers.SetUtils.AddAll(%scope%, %args%)',
         'contains#1':        '%scope%.Contains(%args%)',
         'containsAll#1':     'Saxon.Impl.Helpers.SetUtils.ContainsAll(%scope%, %args%)',
         'isEmpty#0':         '%scope%.Count==0',
         'remove#1':          '%scope%.Remove(%args%)',
         'size#0':            '%scope%.Count',
         'values#0':          '%scope%.Values',
         '*':                 '%scope%.%name%(%args%)' 
         })"/>     
   </xsl:template>
   
   <!--Handle java.lang.Iterator, which becomes IEnumerator. -->
   <!--This conversion is optimistic, because the methods hasNext() and next() have no direct
      equivalents (hasNext() shouldn't change the iterator position, but MoveNext does). We
      do the conversion anyway and hope for the best -->
   
   <xsl:template match="*[f:targetType(.) = 'java.util.Iterator']" 
      priority="20" mode="methods">
      <xsl:variable name="suppressWarning" select="ancestor::*[@nodeType='MethodDeclaration'][1]/annotations/annotation[name/@identifier='CSharpSuppressWarnings']//@value='UnsafeIteratorConversion'"/>
      <xsl:choose>
         <xsl:when test="name/@identifier='hasNext' and count(arguments/argument)=0">
            <xsl:apply-templates select="scope" mode="xml-to-cs"/>
            <xsl:text>.MoveNext(){' /*UNSAFE CONVERSION*/'[not($suppressWarning)]}</xsl:text>
            <xsl:if test="not($suppressWarning)">
               <xsl:message>*** WARNING: in {ancestor::*[@nodeType='MethodDeclaration'][1]/name/@identifier} of {ancestor::*[@nodeType='ClassOrInterfaceDeclaration'][1]/name/@identifier
                  }: conversion of Iterator needs manual checking. Use @CSharpSuppressWarnings("UnsafeIteratorConversion") to suppress this message.</xsl:message>
            </xsl:if>
         </xsl:when>
         <xsl:when test="name/@identifier='next' and count(arguments/argument)=0">
            <xsl:apply-templates select="scope" mode="xml-to-cs"/>
            <xsl:text>.Current</xsl:text>
         </xsl:when>
   
         <xsl:otherwise>
            <xsl:next-match/>
         </xsl:otherwise>
      </xsl:choose>
   </xsl:template>
   
   <!--Handle java.lang.Iterable, which becomes IEnumerable. -->
   <!--This conversion is optimistic, because the methods hasNext() and next() have no direct
      equivalents (hasNext() shouldn't change the iterator position, but MoveNext does). We
      do the conversion anyway and hope for the best -->
   
   <xsl:template match="*[f:targetType(.) = 'java.lang.Iterable']" 
      priority="20" mode="methods">
      <xsl:choose>
         <xsl:when test="name/@identifier='iterator' and count(arguments/argument)=0">
            <xsl:apply-templates select="scope" mode="xml-to-cs"/>
            <xsl:text>.GetEnumerator()</xsl:text>
         </xsl:when>
         <xsl:otherwise>
            <xsl:next-match/>
         </xsl:otherwise>
      </xsl:choose>
   </xsl:template>
   
   <xsl:template match="*[f:targetType(.) = 'java.util.Comparator' ]" 
      priority="20" mode="methods">
      <xsl:sequence select="f:applyFormat(., map{
         '*':           '%scope%.%Name%(%args%)'
         })"/>
   </xsl:template>
   
   <!-- java.util.concurrent.BlockingQueue (becomes System.Collections.Concurrent.BlockingCollection) -->
   <xsl:template match="*[f:targetType(.) = 'java.util.concurrent.BlockingQueue' ]" 
      priority="20" mode="methods">
      <xsl:sequence select="f:applyFormat(., map{
         'put#1':       '%scope%.Add(%args%)',
         'take#0':      '%scope%.Take()',
         'clear#0':     'foreach (var item in %scope%.GetConsumingEnumerable()) {}',
         'poll#0':      'Saxon.Impl.Helpers.QueueUtils.Poll(%scope%)',
         '*':           '%scope%.%Name%(%args%)'
         })"/>
   </xsl:template>
   
   <!-- java.lang.ThreadLocal -->
   <xsl:template match="*[f:targetType(.) = 'java.lang.ThreadLocal' ]" 
      priority="20" mode="methods">
      <xsl:sequence select="f:applyFormat(., map{
         'get#0':       '%scope%.Value',
         'set#1':       '%scope%.Value = %args%',   (:assumes it appears as a statement:)
         '*':           '%scope%.%Name%(%args%)'
         })"/>
   </xsl:template>
   
   <!--<xsl:template match="*[@RESOLVED_TYPE = ('net.sf.saxon.s9api.XdmArray', 
                                            'net.sf.saxon.s9api.XdmAtomicValue',
                                            'net.sf.saxon.s9api.XdmEmptySequence',
                                            'net.sf.saxon.s9api.XdmFunctionItem',
                                            'net.sf.saxon.s9api.XdmItem',
                                            'net.sf.saxon.s9api.XdmMap',
                                            'net.sf.saxon.s9api.XdmNode',
                                            'net.sf.saxon.s9api.XdmValue')]" 
      priority="20" mode="methods">
      <xsl:sequence select="f:applyFormat(., map{
         'getUnderlyingValue#0':  '%scope%.Unwrap()',
         '*':                     '%scope%.%Name%(%args%)'
         })"/>
   </xsl:template>-->
   
   <!-- com.ibm.icu.lang.UScript: ICU4N.Globalization.UScript -->
   
   <xsl:template match="*[f:targetType(.) = 'com.ibm.icu.lang.UScript']" 
      priority="20" mode="methods">
      <xsl:sequence select="f:applyFormat(., map{
         'getCode#1':         'ICU4N.Globalization.UScript.GetCode(%args%)',
         '*':                 '%scope%.%Name%(%args%)' 
         })"/>     
   </xsl:template>
   
   <!-- com.ibm.icu.text.BreakIterator (=ICU4N.Text.BreakIterator) -->
   
   <xsl:template match="*[f:targetType(.) = 'com.ibm.icu.text.BreakIterator']" 
      priority="20" mode="methods">
      <xsl:sequence select="f:applyFormat(., map{
         'getCharacterInstance#0':     'ICU4N.Text.BreakIterator.GetCharacterInstance()',
         '*':                          '%scope%.%Name%(%args%)' 
         })"/>     
   </xsl:template>
   
   <!-- com.ibm.icu.text.Collator (=ICU4N.Text.Collator) -->
   
   <xsl:template match="*[f:targetType(.) = 'com.ibm.icu.text.Collator']" 
      priority="20" mode="methods">
      <xsl:sequence select="f:applyFormat(., map{
         'getInstance#0':     'ICU4N.Text.Collator.GetInstance()',
         'getInstance#1':     'ICU4N.Text.Collator.GetInstance(%args%)',
         '*':                 '%scope%.%Name%(%args%)' 
         })"/>     
   </xsl:template>
   
   <!-- com.ibm.icu.text.CollationElementIterator (=ICU4N.Text.CollationElementIterator) -->
   
   <xsl:template match="*[f:targetType(.) = 'com.ibm.icu.text.CollationElementIterator']" 
      priority="20" mode="methods">
      <xsl:sequence select="f:applyFormat(., map{
         'primaryOrder#1':    'ICU4N.Text.CollationElementIterator.PrimaryOrder(%args%)',
         'secondaryOrder#1':  'ICU4N.Text.CollationElementIterator.SecondaryOrder(%args%)',
         'tertiaryOrder#1':   'ICU4N.Text.CollationElementIterator.TertiaryOrder(%args%)',
         '*':                 '%scope%.%Name%(%args%)' 
         })"/>     
   </xsl:template>
   
   <!-- com.ibm.icu.text.CollationKey (=ICU4N.Text.CollationKey) -->
   
   <xsl:template match="*[f:targetType(.) = 'com.ibm.icu.text.CollationKey']" 
      priority="20" mode="methods">
      <xsl:sequence select="f:applyFormat(., map{
         '*':                 '%scope%.%Name%(%args%)' 
         })"/>     
   </xsl:template>
   
   <!-- com.ibm.icu.text.RuleBasedCollator (=ICU4N.Text.RuleBasedCollator) -->
   
   <xsl:template match="*[f:targetType(.) = 'com.ibm.icu.text.RuleBasedCollator']" 
      priority="20" mode="methods">
      <xsl:sequence select="f:applyFormat(., map{
         'getAvailableLocales#0':         'ICU4N.Text.RuleBasedCollator.',
         'getStrength#0':                 '(int)%scope%.Strength',
         'getVariableTop#0':              '%scope%.VariableTop',
         'isFrenchCollation#0':           '%scope%.IsFrenchCollation',
         'isLowerCaseFirst#0':            '%scope%.IsLowerCaseFirst',
         'isUpperCaseFirst#0':            '%scope%.IsUpperCaseFirst',
         'setAlternateHandlingShifted#1': '%scope%.IsAlternateHandlingShifted = (%args%)',
         'setCaseLevel#1':                '%scope%.IsCaseLevel = (%args%)',
         'setDecomposition#1':            '%scope%.Decomposition = (%args%)',
         'setFrenchCollation#1':          '%scope%.IsFrenchCollation = (%args%)',
         'setLowerCaseFirst#1':           '%scope%.IsLowerCaseFirst = (%args%)',
         'setMaxVariable#1':              '%scope%.MaxVariable = (%args%)',
         'setNumericCollation#1':         '%scope%.IsNumericCollation = (%args%)',
         'setStrength#1':                 '%scope%.Strength = (ICU4N.Text.CollationStrength)(%args%)',
         'setUpperCaseFirst#1':           '%scope%.IsUpperCaseFirst = (%args%)',
         '*':                             '%scope%.%Name%(%args%)' 
         })"/>     
   </xsl:template>
   
   <!-- com.ibm.icu.util.VersionInfo (=ICU4N.Util.VersionInfo) -->
   
   <xsl:template match="*[f:targetType(.) = 'com.ibm.icu.util.VersionInfo']" 
      priority="20" mode="methods">
      <xsl:sequence select="f:applyFormat(., map{
         'getInstance#0':              'ICU4N.Util.VersionInfo.GetInstance()',
         'getInstance#1':              'ICU4N.Util.VersionInfo.GetInstance(%args%)',
         'getMajor#0':                 '%scope%.Major',
         'getMinor#0':                 '%scope%.Minor',
         'getMilli#0':                 '%scope%.Milli',
         'getMicro#0':                 '%scope%.Micro',
         '*':                          '%scope%.%Name%(%args%)' 
         })"/>     
   </xsl:template>
   
   <xsl:template match="*[f:targetType(.) = 'net.sf.saxon.s9api.QName']" 
      priority="20" mode="methods">
      <xsl:sequence select="f:applyFormat(., map{
         'getClarkName#0':        '%scope%.ClarkName',
         'getEQName#0':           '%scope%.EQName',
         'getPrefix#0':           '%scope%.Prefix',
         'getLocalName#0':        '%scope%.LocalName',
         'getNamespace#0':        '%scope%.NamespaceUri.ToString()',
         'getNamespaceUri#0':     '%scope%.NamespaceUri',
         '*':                     '%scope%.%Name%(%args%)'
         })"/>
   </xsl:template>
 
   
   <xsl:template match="*[f:targetType(.) = 'net.sf.saxon.s9api.XdmArray']" 
      priority="20" mode="methods">
      <xsl:sequence select="f:applyFormat(., map{
         'getUnderlyingValue#0':  '((net.sf.saxon.ma.arrays.ArrayItem)%scope%.Unwrap())',
         '*':                     '%scope%.%Name%(%args%)'
         })"/>
   </xsl:template>
   
   <xsl:template match="*[f:targetType(.) = 'net.sf.saxon.s9api.XdmAtomicValue']" 
      priority="20" mode="methods">
      <xsl:sequence select="f:applyFormat(., map{
         'getUnderlyingValue#0':  '((net.sf.saxon.value.AtomicValue)%scope%.Unwrap())',
         '*':                     '%scope%.%Name%(%args%)'
         })"/>
   </xsl:template>
   
   <xsl:template match="*[f:targetType(.) = 'net.sf.saxon.s9api.XdmEmptySequence']" 
      priority="20" mode="methods">
      <xsl:sequence select="f:applyFormat(., map{
         'getUnderlyingValue#0':  '((net.sf.saxon.value.EmptySequence)%scope%.Unwrap())',
         'getInstance#0':         '%scope%.Instance',
         '*':                     '%scope%.%Name%(%args%)'
         })"/>
   </xsl:template>
   
   <xsl:template match="*[f:targetType(.) = 'net.sf.saxon.s9api.XdmFunctionItem']" 
      priority="20" mode="methods">
      <xsl:sequence select="f:applyFormat(., map{
         'getUnderlyingValue#0':  '((net.sf.saxon.om.Function)%scope%.Unwrap())',
         '*':                     '%scope%.%Name%(%args%)'
         })"/>
   </xsl:template>
   
   <xsl:template match="*[f:targetType(.) = 'net.sf.saxon.s9api.XdmItem']" 
      priority="20" mode="methods">
      <xsl:sequence select="f:applyFormat(., map{
         'getUnderlyingValue#0':  '((Saxon.Hej.om.Item)%scope%.Unwrap())',
         '*':                     '%scope%.%Name%(%args%)'
         })"/>
   </xsl:template>
   
   <xsl:template match="*[f:targetType(.) = 'net.sf.saxon.s9api.ItemType']" 
      priority="20" mode="methods">
      <xsl:sequence select="f:applyFormat(., map{
         'getUnderlyingItemType#0':  '((Saxon.Hej.type.ItemType)%scope%.Unwrap())',
         '*':                        '%scope%.%Name%(%args%)'
         })"/>
   </xsl:template>
   
   <!-- net.sf.saxon.s9api.ExtensionFunctionCall -->
   <xsl:template match="*[f:targetType(.) = 'net.sf.saxon.s9api.ExtensionFunctionCall']" 
      priority="20" mode="methods">
      <xsl:sequence select="f:applyFormat(., map{
         'supplyStaticContext#3':    '%scope%.SupplyStaticContext(%arg1%)',
         '*':                        '%scope%.%Name%(%args%)'
         })"/>
   </xsl:template>
   
   <!-- net.sf.saxon.s9api.WhitespaceStrippingPolicy -->
   <xsl:template match="*[f:targetType(.) = 'net.sf.saxon.s9api.WhitespaceStrippingPolicy']" 
      priority="20" mode="methods">
      <xsl:sequence select="f:applyFormat(., map{
         'getSpaceStrippingRule#0':  '%scope%.Implementation',
         'makeStripper#0':           '(next => (new Saxon.Hej.@event.Stripper(%scope%.Implementation, next)))',
         '*':                        '%scope%.%Name%(%args%)'
         })"/>
   </xsl:template>
   
   <xsl:template match="*[f:targetType(.) = 'net.sf.saxon.s9api.XdmMap']" 
      priority="20" mode="methods">
      <xsl:sequence select="f:applyFormat(., map{
         'getUnderlyingValue#0':  '((Saxon.Hej.ma.map.MapItem)%scope%.Unwrap())',
         '*':                     '%scope%.%Name%(%args%)'
         })"/>
   </xsl:template>
   
   <xsl:template match="*[f:targetType(.) = 'net.sf.saxon.s9api.XdmNode']" 
      priority="20" mode="methods">
      <xsl:sequence select="f:applyFormat(., map{
         'getUnderlyingNode#0':   '((Saxon.Hej.om.NodeInfo)%scope%.Unwrap())',
         'getUnderlyingValue#0':  '((Saxon.Hej.om.NodeInfo)%scope%.Unwrap())',
         'getStringValue#0':      '%scope%.StringValue',
         '*':                     '%scope%.%Name%(%args%)'
         })"/>
   </xsl:template>
   
   <xsl:template match="*[f:targetType(.) = 'net.sf.saxon.s9api.XdmValue']" 
      priority="20" mode="methods">
      <xsl:sequence select="f:applyFormat(., map{
         'getUnderlyingValue#0':  '((Saxon.Hej.om.GroundedValue)%scope%.Unwrap())',
         'isEmpty#0':             '%scope%.Count==0',
         'size#0':                '%scope%.Count==0',
         '*':                     '%scope%.%Name%(%args%)'
         })"/>
   </xsl:template>
   <!--<xsl:template match="*[f:targetType(.) = 'net.sf.saxon.s9api.XmlProcessingError']" 
      priority="20" mode="methods">
      <xsl:sequence select="f:applyFormat(., map{
         'getCause#0':              '%scope%.Cause',
         'getErrorCode#0':          '%scope%.ErrorCode',
         'getHostLanguage#0':       '%scope%.HostLanguage',
         'getLocation#0':           '%scope%.Location',
         'getMessage#0':            '%scope%.Message',
         'getTerminationMessage#0': '%scope%.TerminationMessage',
         'isStaticError#0':         '%scope%.IsStaticError',
         'isTypeError#0':           '%scope%.IsTypeError',
         'isWarning#0':             '%scope%.IsWarning',
         'setFatal#1':              '%scope%.IsFatal = true',
         'setTerminationMessage#1': '%scope%.TerminationMessage = %args%',
         '*':                       '%scope%.%Name%(%args%)'
         })"/>
   </xsl:template>-->
   
   <xsl:template match="*[f:targetType(.) = 'net.sf.saxon.s9api.MessageListener2']" 
      priority="20" mode="methods">
      <xsl:sequence select="f:applyFormat(., map{
         'message#4'  : '%scope%.Message(%arg1%, %arg2%, %arg3%, new Saxon.Api.XmlLocation {BaseUri = new Uri(%arg4%.getSystemId()), LineNumber = %arg4%.getLineNumber()})',
         '*':           '%scope%.%Name%(%args%)'
         })"/>
   </xsl:template>
   
   <!--<xsl:template match="*[@RESOLVED_TYPE = 'net.sf.saxon.lib.ExtensionFunctionDefinition']" 
      priority="20" mode="methods">
      <xsl:sequence select="f:applyFormat(., map{
         'getArgumentTypes#0':  '%scope%.getUnderlyingArgumentTypes(%args)',
         'getResultType#0':     '%scope%.getUnderlyingResultType(%args)',
         'getFunctionQName#0':  '%scope%.FunctionName.GetStructuredQName()',
         'makeCallExpression#0':'%scope%.MakeFunctionCall()',
         'trustResultType#0':   '%scope%.TrustResultType',
         '*':                   '%scope%.%Name%(%args%)'
         })"/>
   </xsl:template>-->
 
   
   <!-- Special-case Collections.emptyIterator() and similar methods -->
   <!-- Reuse the "diamond" logic for determining the required type -->
   <xsl:template match="*[f:targetType(.) = 'java.util.Collections']" 
      priority="20" mode="methods">
     <!-- <xsl:message>Collections.{name/@identifier}()</xsl:message>-->
      <!--<xsl:variable name="targetClass" as="xs:string?">
         <xsl:choose>
            <xsl:when test="../@nodeType='ReturnStmt'">
               <xsl:sequence select="ancestor::member/type/typeArguments/typeArgument[1]/@RESOLVED_TYPE ! string()"/>
            </xsl:when>
         </xsl:choose>
      </xsl:variable>-->
      <xsl:variable name="targetClass" as="xs:string">
         <xsl:apply-templates select="." mode="diamond"/>
      </xsl:variable>
      <xsl:choose>
         <xsl:when test="name/@identifier = 'emptyIterator' and $targetClass">
            <xsl:text>new System.Collections.Generic.List&lt;{f:renameGenericType($targetClass)}>().GetEnumerator();</xsl:text>
         </xsl:when>
         <xsl:when test="name/@identifier = 'emptyList' and $targetClass">
            <xsl:text>new System.Collections.Generic.List&lt;{f:renameGenericType($targetClass)}>()</xsl:text>
         </xsl:when>
         <xsl:when test="name/@identifier = 'emptySet' and $targetClass">
            <xsl:text>new System.Collections.Generic.HashSet&lt;{f:renameGenericType($targetClass)}>()</xsl:text>
         </xsl:when>
         <xsl:when test="name/@identifier = 'emptyMap' and $targetClass">
            <xsl:text>new System.Collections.Generic.Dictionary&lt;{f:renameGenericType($targetClass)}>()</xsl:text>
         </xsl:when>
         <xsl:when test="name/@identifier = 'singleton' and $targetClass">
            <xsl:text>java.util.Collections.singleton&lt;{f:renameGenericType($targetClass)}>(</xsl:text>
            <xsl:apply-templates select="arguments" mode="xml-to-cs"/>
            <xsl:text>)</xsl:text>
         </xsl:when>
         <xsl:otherwise>
            <xsl:next-match/>
         </xsl:otherwise>
      </xsl:choose>
   </xsl:template>
   
   <!-- Special-case Optional.empty() and Optional.of() -->
   <!-- Currently handled only when appearing in a return statement, the type is taken from the containing method declaration -->
   <xsl:template match="*[starts-with(@RESOLVED_TYPE, 'java.util.Optional')]" 
      priority="20" mode="methods">
      <!--<xsl:message>Optional.{name/@identifier}()</xsl:message>-->
      <xsl:variable name="targetClass" as="xs:string">
         <xsl:apply-templates select="." mode="diamond"/>
      </xsl:variable>
      <xsl:choose>
         <xsl:when test="name/@identifier = ('empty', 'of', 'ofNullable') and $targetClass">
            <xsl:text>Saxon.Ejava.util.Optional&lt;{f:renameGenericType($targetClass)}>.{name/@identifier}(</xsl:text>
            <xsl:apply-templates select="arguments" mode="xml-to-cs"/>
            <xsl:text>)</xsl:text>
         </xsl:when>
         <xsl:otherwise>
            <xsl:next-match/>
         </xsl:otherwise>
      </xsl:choose>
   </xsl:template>
   
   <!-- 'java.util.Locale': 'System.Globalization.CultureInfo' -->
   
   <xsl:template match="*[@RESOLVED_TYPE = 'java.util.Locale']" priority="20">
      <xsl:sequence select="f:applyFormat(., map{
         'getCountry#0':     '(%scope%.IsNeutralCulture ? null : new System.Globalization.RegionInfo(%scope%.LCID).TwoLetterISORegionName)',
         'getDefault#0':     'System.Globalization.CultureInfo.CurrentCulture',
         'getDisplayName#0': '%scope%.Name',
         'getLanguage#0':    '%scope%.Name',
         '*':                '%scope%.%Name%(%args%)'
         })"/>
   </xsl:template>
   
   
   <!-- Translate methods used on IO classes -->
   <xsl:template match="*[starts-with(@RESOLVED_TYPE, 'java.io.') and not(@RESOLVED_TYPE = 'java.io.File')]" 
      priority="20" mode="methods">
      <xsl:choose>
         <xsl:when test="@RESOLVED_TYPE = 'java.io.IOException' and name/@identifier = 'getMessage'">
            <xsl:apply-templates select="scope" mode="xml-to-cs"/>
            <xsl:text>.Message</xsl:text>
         </xsl:when>
         <!-- stream.write(byte b) becomes stream.WriteByte(b). Needs better type inferencing! -->
         <xsl:when test="name/@identifier = 'write' 
                         and ends-with(@RESOLVED_TYPE, 'Stream') 
                         and count(arguments/argument) = 1
                         and (f:expressionType(arguments/argument[1])=('java.lang.Byte', 'byte', 'java.lang.int', 'int')
                             or arguments/argument[1][@nodeType='ArrayAccessExpr'][*[2][@RESOLVED_TYPE='byte[]']])">
            <xsl:apply-templates select="scope" mode="xml-to-cs"/>
            <xsl:text>.WriteByte((byte)</xsl:text>
            <xsl:apply-templates select="arguments" mode="xml-to-cs"/>
            <xsl:text>)</xsl:text>           
         </xsl:when>
         <!-- stream.toByteArray() becomes stream.ToArray(). -->
         <xsl:when test="name/@identifier = 'toByteArray' and count(arguments/argument) eq 0">
            <xsl:apply-templates select="scope" mode="xml-to-cs"/>
            <xsl:text>.ToArray()</xsl:text>           
         </xsl:when>
         <!-- stream.skip(n) becomes stream.Seek(n, SeekOrigin.Current). -->
         <xsl:when test="name/@identifier = 'skip' and count(arguments/argument) eq 1">
            <xsl:apply-templates select="scope" mode="xml-to-cs"/>
            <xsl:text>.Seek(</xsl:text>
            <xsl:apply-templates select="arguments" mode="xml-to-cs"/>
            <xsl:text>, System.IO.SeekOrigin.Current)</xsl:text>
         </xsl:when>
         <!-- writer.getEncoding() becomes writer.Encoding.EncodingName -->
         <xsl:when test="name/@identifier = 'getEncoding' and count(arguments/argument) eq 0">
            <xsl:apply-templates select="scope" mode="xml-to-cs"/>
            <xsl:text>.Encoding.EncodingName</xsl:text>           
         </xsl:when>
         <xsl:otherwise>
            <xsl:apply-templates select="scope" mode="xml-to-cs"/>
            <xsl:text>.</xsl:text>
            <xsl:apply-templates select="f:capitalize(name/@identifier)" mode="xml-to-cs"/>
            <xsl:text>(</xsl:text>
            <xsl:apply-templates select="arguments" mode="xml-to-cs"/>
            <xsl:text>)</xsl:text>
         </xsl:otherwise>
      </xsl:choose>
   </xsl:template>
   
   <!-- java.nio.charset.StandardCharsets => System.Text.Encoding -->
   <xsl:template match="*[@RESOLVED_TYPE = 'java.nio.charset.StandardCharsets']" priority="20">
      <xsl:sequence select="f:applyFormat(., map{
         'forName#1':      'Saxon.Impl.Helpers.StringUtils.EncodingForName(%args%)',
         '*':              '%scope%.%Name%(%args%)'
         })"/>
   </xsl:template>
   
   <!-- java.nio.charset.Charset => System.Text.Encoding -->
   <xsl:template match="*[@RESOLVED_TYPE = 'java.nio.charset.Charset']" priority="20">
      <xsl:sequence select="f:applyFormat(., map{
         'forName#1':      'Saxon.Impl.Helpers.StringUtils.EncodingForName(%args%)',
         '*':              '%scope%.%Name%(%args%)'
         })"/>
   </xsl:template>
   
   <!-- int[].clone() -->
   <xsl:template match="*[@RESOLVED_TYPE = 'int[]'](:[name/@identifier = 'clone']:)" priority="20">
      <xsl:sequence select="f:applyFormat(., map{
         'clone#0':      'Saxon.Ejava.util.Arrays.copyOf(%scope%, %scope%.Length)',
         '*':            '%scope%.%Name%(%args%)'
         })"/>
   </xsl:template>
   
   
   <xsl:template match="*" priority="10" mode="methods">
      <xsl:variable name="targetType" select="(@RESOLVED_TYPE,@DECLARING_TYPE)[1]" as="xs:string?"/>
      <xsl:variable name="methodName" select="string(name/@identifier)"/>
      <xsl:variable name="arity" select="count(arguments/argument)"/>
      <xsl:variable name="digestMethod" select="f:candidateMethods($targetType, $methodName, $arity)"/>
      <!--<xsl:message>Method call = {$methodName}#{$arity} digestMethods={count($digestMethod)} 
         returns={$digestMethod/@returns} csReturns={$digestMethod/@csReturns}</xsl:message>-->
      <!-- Be prepared to cast the result to the required type in the case where the C# method returns a supertype of 
           the return type of the corresponding Java method -->
      <xsl:variable name="optionalCast" 
         select="if (count($digestMethod)=1 and $digestMethod[@returns != @csReturns] and not(../@nodeType='ExpressionStmt')) 
                 then f:renameGenericType($digestMethod/@returns) 
                 else ''"/>
      <!--<xsl:message> cast = {$optionalCast}</xsl:message>-->
      <xsl:if test="$optionalCast">(({$optionalCast})</xsl:if>
      
      <xsl:if test="scope">
         <xsl:choose>
           <xsl:when test="scope/name/@nodeType='SimpleName' and not (scope/@UNRESOLVED) and ends-with(f:degenerify(@RESOLVED_TYPE), '.'||scope/name/@identifier) and not(scope/scope)">
               <!-- A simple static method call -->
               <xsl:text>{f:escapeDottedName(@RESOLVED_TYPE) => f:adjustTypeNamespace()}</xsl:text>
            </xsl:when>
            <xsl:otherwise>
               <xsl:apply-templates select="scope" mode="xml-to-cs"/>
            </xsl:otherwise>
         </xsl:choose>         
         <xsl:text>.</xsl:text>         
      </xsl:if>
      <xsl:apply-templates select="typeArguments" mode="xml-to-cs"/>
        
           
      <xsl:apply-templates select="name" mode="xml-to-cs"/>
      <xsl:text>(</xsl:text> 
      <xsl:apply-templates select="arguments" mode="xml-to-cs"/>
      <xsl:text>)</xsl:text>            
         
      <xsl:if test="$optionalCast">)</xsl:if>
   </xsl:template>
   


</xsl:stylesheet>
