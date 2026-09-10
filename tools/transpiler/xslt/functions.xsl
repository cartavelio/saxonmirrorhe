<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
   xmlns:map="http://www.w3.org/2005/xpath-functions/map"
   xmlns:xs="http://www.w3.org/2001/XMLSchema"
   xmlns:f="http://ns.saxonica.com/local-functions"
   exclude-result-prefixes="#all"
   version="3.0"
   expand-text="yes"
   default-mode="xml-to-cs">
   
   <!-- 
      This stylesheet module contains supporting functions used by the
      Java-to-C# transpiler stylesheet, and global variables and key
      definitions used by these functions.
   -->
   
   <xsl:key name="digestClassKey" match="class | interface | enum" 
      use="string-join((ancestor::module/@package, ancestor-or-self::*[self::class or self::interface or self::enum]/@name), '.')"/>
   
   <!-- The key for finding a method by name and signature is of the form 'net.sf.saxon.om.NodeInfo#iterateAxis!INT,NodeTest' -->
   <xsl:key name="digestMethodKey" match="method"
      use="concat(ancestor::module/@package, '.', 
      string-join(ancestor-or-self::*[self::class or self::interface]/@name, '.'),
      '#', ./@name, '!', string(@sig))"/>
   
   <!-- The key for finding a method by name and arity is of the form 'net.sf.saxon.om.NodeInfo#iterateAxis!arity' -->
   <xsl:key name="digestMethodArityKey" match="method"
      use="concat(ancestor::module/@package, '.', 
      string-join(ancestor-or-self::*[self::class or self::interface]/@name, '.'),
      '#', ./@name, '!', count(tokenize(string(@sig), ',')))"/>
   
 
   <xsl:variable name="reservedWords" as="xs:string*" select="tokenize('
      abstract as base $$bool break byte case catch char checked class const continue
      decimal default delegate do $$double else enum event explicit extern false
      finally fixed $$float for foreach goto if implicit in $$int interface internal
      is lock long namespace new null object operator out override params private
      protected public readonly ref return sbyte sealed short sizeof stackalloc
      static $$string struct switch this throw true try typeof uint ulong
      unchecked unsafe ushort using virtual $$void volatile while')"/>
   
   <!-- Excluded from the above list are names that we generate in type names, which we don't want escaped:
      string bool int float double long void
      -->
   
   <xsl:variable name="primitiveTypeNames" as="xs:string*" 
      select="tokenize('string bool int float double long void')"/>
   
   <xsl:variable name="renamings" as="map(xs:string, xs:string)" select="map{
      'equals':'Equals',
      'toString':'ToString',    
      'java.lang.System#out' : 'Out',
      'java.lang.System#in' : 'In'}"/>
   
   <xsl:variable name="renamedClasses" as="map(xs:string, xs:string)" select="map{
      'ResourceResolver': 'JJ_ResourceResolver',
      'ResourceRequest': 'JJ_ResourceRequest'
      }"/>
   
   <!-- The following table defines mappings from Java class names to .NET class names. -->
   <!-- These class names are translated wherever they appear. The fields, methods,
      and constructors applied to the class are translated according to the rules
      appearing in the fields.xsl, methods.xsl, and constructors.xsl stylesheet modules. -->
   
   <xsl:variable name="specialTypes" as="map(xs:string, xs:string)" select="map{
      'boolean': 'bool',
      'com.ibm.icu.lang.UScript': 'ICU4N.Globalization.UScript',
      'com.ibm.icu.text.BreakIterator': 'ICU4N.Text.BreakIterator',
      'com.ibm.icu.text.CollationElementIterator': 'ICU4N.Text.CollationElementIterator',
      'com.ibm.icu.text.CollationKey': 'ICU4N.Text.CollationKey',
      'com.ibm.icu.text.Collator': 'ICU4N.Text.Collator',
      'com.ibm.icu.text.Collator.ReorderCodes': 'ICU4N.Text.ReorderCodes',
      'com.ibm.icu.text.RuleBasedCollator': 'ICU4N.Text.RuleBasedCollator',
      'com.ibm.icu.util.VersionInfo': 'ICU4N.Util.VersionInfo',
      'com.saxonica.config.ICULibrary': 'Saxon.Impl.Overrides.ICULibrary',
      'com.saxonica.ee.parallel.MultithreadedContextMappingIterator': 'Saxon.Impl.Parallel.MultithreadedContextMappingIterator',
      'com.saxonica.ee.parallel.MultithreadedItemMappingIterator': 'Saxon.Impl.Parallel.MultithreadedItemMappingIterator',
      (:'com.saxonica.ee.parallel.MultithreadedFocusTrackingIterator': 'Saxon.Impl.Parallel.MultithreadedFocusTrackingIterator',:)
      'com.saxonica.ee.parallel.PushToPull': 'Saxon.Impl.Parallel.PushToPull',
      'com.saxonica.ee.parallel.PushToPull.Pusher': 'Saxon.Impl.Parallel.PushToPull.Pusher',
      'com.saxonica.functions.qt4.ParseHtml': 'Saxon.Impl.Helpers.ParseHtml',
      'java.io.BufferedInputStream': 'System.IO.BufferedStream',
      'java.io.BufferedOutputStream': 'System.IO.BufferedStream',
      'java.io.BufferedReader': 'Saxon.Impl.Helpers.BufferedReader',
      'java.io.ByteArrayInputStream': 'System.IO.MemoryStream',
      'java.io.ByteArrayOutputStream': 'System.IO.MemoryStream',
      'java.io.FileInputStream': 'System.IO.FileStream',
      'java.io.FileOutputStream': 'System.IO.FileStream',
      'java.io.InputStream': 'System.IO.Stream',
      'java.io.InputStreamReader': 'System.IO.StreamReader',
      'java.io.IOException': 'System.IO.IOException',
      'java.io.LineNumberReader': 'Saxon.Impl.Helpers.LineNumberReader',
      'java.io.OutputStream': 'System.IO.Stream',
      'java.io.OutputStreamWriter': 'System.IO.StreamWriter',
      'java.io.Reader': 'System.IO.TextReader',
      'java.io.StringReader': 'System.IO.StringReader',
      'java.io.StringWriter': 'System.IO.StringWriter',
      'java.io.Writer': 'System.IO.TextWriter',
      'java.lang.ArithmeticException': 'System.ArithmeticException',
      'java.lang.ArrayIndexOutOfBoundsException': 'System.IndexOutOfRangeException',
      'java.lang.Boolean': 'bool' (:'System.Boolean':),
      'java.lang.Byte': 'System.Byte',
      'java.lang.Character': 'System.Char',
      'java.lang.CharSequence': 'string',
      'java.lang.Class': 'System.Type',
      'java.lang.ClassCastException': 'System.InvalidCastException',
      'java.lang.Double': 'System.Double',
      'java.lang.Exception': 'System.Exception',
      'java.lang.Float': 'System.Single',
      'java.lang.IndexOutOfBoundsException': 'System.IndexOutOfRangeException',
      'java.lang.Integer': 'int' (:'System.Int32':),
      'java.lang.Long': 'System.Int64',
      'java.lang.Object': 'System.Object',
      'java.lang.RuntimeException': 'System.Exception',
      'java.lang.Short': 'System.Int16',
      'java.lang.StackOverflowError': 'System.StackOverflowException',
      'java.lang.String': 'string',
      'java.lang.StringIndexOutOfBoundsException': 'System.IndexOutOfRangeException',
      'java.lang.InterruptedException': 'System.Threading.ThreadInterruptedException',
      'java.lang.Iterable': 'System.Collections.Generic.IEnumerable',
      'java.lang.ThreadLocal': 'System.Threading.ThreadLocal',
      'java.lang.Throwable': 'System.Exception',
      'java.math.BigDecimal': 'Singulink.Numerics.BigDecimal',
      'java.math.BigInteger': 'System.Numerics.BigInteger',
      'java.net.URI': 'System.Uri',
      'java.net.URISyntaxException': 'System.UriFormatException',
      'java.nio.charset.Charset': 'System.Text.Encoding',
      'java.time.format.DateTimeParseException': 'System.FormatException',
      'java.util.ArrayList': 'System.Collections.Generic.List',
      'java.util.Collection': 'System.Collections.Generic.ICollection',
      'java.util.Comparator': 'System.Collections.Generic.Comparer',
      'java.util.concurrent.ConcurrentHashMap': 'System.Collections.Concurrent.ConcurrentDictionary',
      'java.util.concurrent.ArrayBlockingQueue': 'System.Collections.Concurrent.BlockingCollection',
      'java.util.concurrent.BlockingQueue': 'System.Collections.Concurrent.BlockingCollection',
      'java.util.concurrent.LinkedBlockingQueue': 'System.Collections.Concurrent.BlockingCollection',
      'java.util.function.BiFunction': 'System.Func',
      'java.util.function.Function': 'System.Func',
      'java.util.function.Predicate': 'System.Predicate',
      'java.util.HashMap': 'System.Collections.Generic.Dictionary',
      'java.util.HashSet': 'System.Collections.Generic.HashSet',
      'java.util.Iterator': 'System.Collections.Generic.IEnumerator',
      'java.util.LinkedHashMap': 'Saxon.Impl.Helpers.OrderedDictionary',
      'java.util.LinkedList': 'System.Collections.Generic.LinkedList',
      'java.util.List': 'System.Collections.Generic.IList',
      'java.util.Locale': 'System.Globalization.CultureInfo',
      'java.util.Map': 'System.Collections.Generic.IDictionary',
      'java.util.Map.Entry': 'System.Collections.Generic.KeyValuePair',
      (: 'java.util.Optional': 'System.Nullable', :)
      'java.util.Set': 'System.Collections.Generic.ISet',
      'java.util.TreeMap': 'System.Collections.Generic.SortedDictionary',
      'java.util.TreeSet': 'System.Collections.Generic.SortedSet',
      'java.util.Stack': 'System.Collections.Generic.Stack',
      'net.sf.saxon.expr.sort.LFUCache.EntryWithCounter': 'net.sf.saxon.expr.sort.LFUCache&lt;K,V>.EntryWithCounter',
      (: 'net.sf.saxon.lib.ExtensionFunctionCall': 'Saxon.Api.ExtensionFunctionCall', :)
      (: 'net.sf.saxon.lib.ExtensionFunctionDefinition': 'Saxon.Api.ExtensionFunctionDefinition', :)
      (:'net.sf.saxon.functions.ParseXmlFragment': 'Saxon.Impl.Helpers.ParseXmlFragment',  :)
      'net.sf.saxon.lib.CatalogResourceResolver': 'Saxon.Callbacks.CatalogResourceResolver',
      'net.sf.saxon.lib.ConfigurableResourceResolver': 'Saxon.Callbacks.ConfigurableResourceResolver',
      (:'net.sf.saxon.lib.CommonResourceResolver': 'Saxon.Callbacks.CommonResourceResolver',:)
      'net.sf.saxon.lib.DirectResourceResolver': 'Saxon.Callbacks.DirectResourceResolver',
      'net.sf.saxon.lib.ResourceRequest': 'Saxon.Hej.lib.JJ_ResourceRequest',
      'net.sf.saxon.lib.ResourceResolver': 'Saxon.Hej.lib.JJ_ResourceResolver',
      'net.sf.saxon.lib.StandardLogger': 'Saxon.Callbacks.StandardLogger',
      (:'net.sf.saxon.lib.StandardModuleURIResolver': 'Saxon.Callbacks.StandardModuleURIResolver',:)
      (:'net.sf.saxon.lib.StandardUnparsedTextResolver': 'Saxon.Callbacks.StandardUnparsedTextResolver',:)
      'net.sf.saxon.lib.StandardURIResolver': 'Saxon.Callbacks.StandardURIResolver',
      'net.sf.saxon.ma.arrays.ImmutableArrayItem': 'Saxon.Impl.DataModel.ImmArray',
      'net.sf.saxon.ma.map.HashTrieMap': 'Saxon.Impl.DataModel.ImmMap',
      'net.sf.saxon.ma.trie.ImmutableHashTrieMap': 'System.Collections.Immutable.ImmutableDictionary',
      'net.sf.saxon.ma.trie.ImmutableMap': 'System.Collections.Immutable.ImmutableDictionary',
      'net.sf.saxon.ma.trie.ImmutableList': 'System.Collections.Immutable.ImmutableList',
      'net.sf.saxon.ma.trie.TrieKVP': 'System.Collections.Generic.KeyValuePair',
      'net.sf.saxon.s9api.ExtensionFunction': 'Saxon.Api.ExtensionFunction',
      'net.sf.saxon.s9api.HostLanguage': 'Saxon.Api.HostLanguage',
      'net.sf.saxon.s9api.ItemType': 'Saxon.Api.XdmItemType',
      (: 'net.sf.saxon.s9api.Location': 'Saxon.Api.IXmlLocation', :)
      (:'net.sf.saxon.s9api.MessageListener2': 'Saxon.Api.IMessageListener2', :)
      'net.sf.saxon.s9api.Message': 'Saxon.Api.Message',
      'net.sf.saxon.s9api.QName': 'Saxon.Api.QName',
      (:'net.sf.saxon.s9api.SchemaManager': 'Saxon.Api.SchemaManager',:)
      'net.sf.saxon.s9api.SaxonApiException': 'Saxon.Api.SaxonApiException',
      'net.sf.saxon.s9api.SaxonApiUncheckedException': 'Saxon.Api.SaxonApiException',
      'net.sf.saxon.s9api.SequenceType': 'Saxon.Api.XdmSequenceType',
      'net.sf.saxon.s9api.UnprefixedElementMatchingPolicy': 'Saxon.Api.UnprefixedElementMatchingPolicy',
      'net.sf.saxon.s9api.WhitespaceStrippingPolicy': 'Saxon.Api.WhitespacePolicy',
      'net.sf.saxon.s9api.XdmArray': 'Saxon.Api.XdmArray',
      'net.sf.saxon.s9api.XdmAtomicValue': 'Saxon.Api.XdmAtomicValue',
      'net.sf.saxon.s9api.XdmEmptySequence': 'Saxon.Api.XdmEmptySequence',
      'net.sf.saxon.s9api.XdmFunctionItem': 'Saxon.Api.XdmFunctionItem',     
      'net.sf.saxon.s9api.XdmItem': 'Saxon.Api.XdmItem',
      'net.sf.saxon.s9api.XdmMap': 'Saxon.Api.XdmMap',
      'net.sf.saxon.s9api.XdmNode': 'Saxon.Api.XdmNode',
      'net.sf.saxon.s9api.XdmValue': 'Saxon.Api.XdmValue',
      'net.sf.saxon.resource.ResourceLoader': 'Saxon.Impl.Overrides.ResourceLoader',
      (:'net.sf.saxon.s9api.XmlProcessingError': 'Saxon.Api.XmlProcessingError':)
      'net.sf.saxon.tree.util.ProcInstParser': 'Saxon.Impl.Helpers.ProcInstParser',
      'net.sf.saxon.tree.jiter.MappingJavaIterator': 'Saxon.Impl.Overrides.MappingEnumerator',
      'org.xmlresolver.ResolverFeature': 'Org.XmlResolver.Features.ResolverFeature',
      'net.sf.saxon.lib.DigestMaker': 'Saxon.Impl.Helpers.DigestMaker'
      }"/>
   
   <xsl:variable name="jdkExceptionTypes" as="xs:string*" select="
      'java.io.IOException',
      'java.lang.ArrayIndexOutOfBoundsException',
      'java.lang.AssertionError',
      'java.lang.Exception',
      'java.lang.IndexOutOfBoundsException',
      'java.lang.RuntimeException',
      'java.lang.StackOverflowError',
      'java.lang.StringIndexOutOfBoundsException',
      'java.lang.InterruptedException',
      'java.lang.Throwable',
      'javax.xml.transform.TransformerFactoryConfigurationError'
      "/>
   
   <!-- A list of generic types for which type parameters should be dropped on conversion.
        Currrently only contains one type name
   -->
   <xsl:variable name="dropTypeParameters" as="xs:string*" select="tokenize('System.Type')"/>
 
   <xsl:function name="f:qualifiedName" as="xs:string">
      <xsl:param name="in" as="element(*)"/>
      <xsl:variable name="term" as="xs:string"
         select="$in!(if (@identifier = $reservedWords) then ('@' || @identifier) else ($renamings(@identifier), @identifier)[1])"/>
      <xsl:sequence select="(if ($in/qualifier) then f:qualifiedName($in/qualifier) || '.' else '') || $term"/>
   </xsl:function>
   
   <xsl:function name="f:full-method-name" as="xs:string">
      <!-- Given a MethodDeclaration node, return the full method name in the form package.class[.class]*#method!params -->
      <xsl:param name="decl" as="element()"/>
      <!--<xsl:assert test="$decl/@nodeType = 'MethodDeclaration'"/>
      <xsl:assert test="exists($decl/ancestor::root/packageDeclaration/name)">Method: {path($decl)} in class: {$decl/ancestor::type[1]/name/@identifier}</xsl:assert>
      -->
      <xsl:for-each select="$decl">
         <xsl:choose>
            <xsl:when test="parent::default">
               <!-- this is a default method declaration in the digest -->
               <xsl:text>{ancestor::module[1]/@package}.{ancestor::interface[1]/@name}#{ancestor::method[1]/(@name || '!' || @sig)}</xsl:text>
            </xsl:when>
            <xsl:otherwise>
               <xsl:variable name="sig" select="string-join(parameters/parameter/
                  ((type/@RESOLVED_TYPE) || (if (@isVarArgs='true') then '[]' else '')), ',')"/>
               <xsl:variable name="unescaped" as="xs:string">
                  <xsl:value-of>
                     <xsl:text>{f:qualifiedName(ancestor::root/packageDeclaration/name)}.</xsl:text>
                     <xsl:value-of select="ancestor::*[@nodeType='ClassOrInterfaceDeclaration']/name/@identifier" separator="."/>
                     <xsl:text>#{name/@identifier}!{$sig}</xsl:text>
                  </xsl:value-of>
               </xsl:variable>
               <xsl:sequence select="translate($unescaped, '@', '')"/>
            </xsl:otherwise>
         </xsl:choose>
      </xsl:for-each>
   </xsl:function>
   
   <xsl:function name="f:expressionType" as="xs:string">
      <xsl:param name="expr" as="element(*)"/>
      <xsl:choose>
         <xsl:when test="$expr/@nodeType='ObjectCreationExpr'">
            <xsl:sequence select="$expr/type/@RESOLVED_TYPE"/>
         </xsl:when>
         <xsl:when test="$expr[@RESOLVED_TYPE]/@nodeType='NameExpr'">
            <xsl:sequence select="$expr/@RESOLVED_TYPE"/>
         </xsl:when>
         <xsl:when test="$expr[@UNRESOLVED][self::initializer][parent::variable]/@nodeType='NameExpr'">
            <xsl:sequence select="$expr/../type/@RESOLVED_TYPE"/>
         </xsl:when>
         <xsl:when test="$expr/@nodeType='FieldAccessExpr'">
            <xsl:sequence select="$expr/@RETURN"/>
         </xsl:when>
         <xsl:when test="$expr/@nodeType='MethodCallExpr'">
            <xsl:sequence select="$expr/@RETURN"/>
         </xsl:when>
         <xsl:when test="$expr/@nodeType='StringLiteralExpr'">
            <xsl:sequence select="'java.lang.String'"/>
         </xsl:when>
         <xsl:when test="$expr/@nodeType='IntegerLiteralExpr'">
            <xsl:sequence select="'java.lang.Integer'"/>
         </xsl:when>
         <xsl:when test="$expr/@nodeType='ConditionalExpr'">
            <xsl:sequence select="f:expressionType($expr/thenExpr)"/>
         </xsl:when>
         <xsl:when test="$expr/@nodeType='BinaryExpr'">
            <xsl:sequence select="f:expressionType($expr/left)"/> <!-- guesswork here -->
         </xsl:when>
         <xsl:when test="$expr/@nodeType='ArrayCreationExpr'">
            <xsl:sequence select="f:expressionType($expr/elementType) || '[]'"/>
         </xsl:when>
         <xsl:when test="$expr/@nodeType='CastExpr'">
            <xsl:sequence select="$expr/type/@RESOLVED_TYPE"/>
         </xsl:when>
         <xsl:otherwise>
            <xsl:text>java.lang.Object /* unknown type for {$expr/@nodeType} */</xsl:text>
         </xsl:otherwise>        
      </xsl:choose>
   </xsl:function>
   
   <xsl:function name="f:extract-type-arguments" as="xs:string?">
      <xsl:param name="type-name" as="xs:string"/>
      <!-- If we start with "List<A, B<C, D>>", we want to extract "A, B<C, D>"-->
      <xsl:variable name="type-param" select="$type-name => replace('^[^&lt;]+&lt;', '') => replace('>$', '')"/>   
      <xsl:value-of>
         <xsl:analyze-string select="$type-param" regex="[a-zA-Z0-9\._]+">
            <xsl:matching-substring>
               <xsl:value-of select="f:renameType(.)"/>
            </xsl:matching-substring>
            <xsl:non-matching-substring>
               <xsl:value-of select="."/>
            </xsl:non-matching-substring>
         </xsl:analyze-string>
      </xsl:value-of>
   </xsl:function>
   
   <xsl:function name="f:fullClassName" as="xs:string">
      <xsl:param name="classElement" as="element()*"/>
      <xsl:assert test="$classElement/@nodeType = 'ClassOrInterfaceDeclaration'"/>
      <xsl:sequence
         select="$classElement!string-join((f:qualifiedName(ancestor::root/packageDeclaration/name), 
         ancestor-or-self::*[@nodeType='ClassOrInterfaceDeclaration']/name/@identifier), '.')"/>
   </xsl:function>
   
   <xsl:function name="f:isExceptionType" as="xs:boolean">
      <xsl:param name="type" as="xs:string?"/>
      <xsl:sequence select="$jdkExceptionTypes = $type or ends-with($type, 'Exception')"/>
   </xsl:function>
   
   <xsl:function name="f:isIteratorType" as="xs:boolean">
      <xsl:param name="type" as="xs:string"/>
      <xsl:sequence select="f:degenerify($type) = ('java.util.Iterator', 'net.sf.saxon.s9api.XdmSequenceIterator')"/>
   </xsl:function>
   
   <xsl:function name="f:isComparatorType" as="xs:boolean">
      <xsl:param name="type" as="xs:string"/>
      <xsl:sequence select="f:degenerify($type) = 'java.util.Comparator'"/>
   </xsl:function>
   
   <!-- Given a class in the digest, determine whether this class implements java.util.Iterator
        directly or indirectly; and if it does; return the class over which it iterates -->
   <xsl:function name="f:typeThatThisIteratesOver" as="xs:string?">
      <xsl:param name="type" as="element()"/>
      <xsl:assert test="$type[self::class|self::interface]">Wrong element type in f:iteratesOver: {name($type)}</xsl:assert>
      <xsl:variable name="superTypes" as="element(*)*" select="$type/implements | $type/extends"/>
      <xsl:variable name="iteratorSuperType" select="$superTypes[@name = 'java.util.Iterator']"/>
      <xsl:choose>
         <xsl:when test="$iteratorSuperType">
            <xsl:sequence select="$iteratorSuperType/@params => replace('(^&lt;|&gt;$)', '')"/>
         </xsl:when>
         <xsl:otherwise>
            <xsl:for-each select="$superTypes">
               <xsl:variable name="declaration" select="key('digestClassKey', .)"/>
               <xsl:if test="$declaration">
                  <xsl:sequence select="f:typeThatThisIteratesOver($declaration)"/>
               </xsl:if>
            </xsl:for-each>
         </xsl:otherwise>
      </xsl:choose>
   </xsl:function>
   
   <xsl:function name="f:renameMethod" as="xs:string">
      <xsl:param name="identifier" as="xs:string"/>
      <xsl:param name="context" as="element(*)"/>
      <xsl:variable name="escaped" select="f:escapeSimpleName($identifier)"/>
      <xsl:variable name="expanded" select="f:degenerify($context/@RESOLVED_TYPE) || '#' || $escaped"/>     
      <xsl:choose>
         <xsl:when test="$renamings($identifier)">{$renamings($identifier)}</xsl:when>
         <xsl:when test="$renamings($expanded)">{$renamings($expanded)}</xsl:when>
         <xsl:otherwise>{$escaped}</xsl:otherwise>
      </xsl:choose>
   </xsl:function>
   
   <xsl:function name="f:renameType" as="xs:string">
      <xsl:param name="identifier" as="xs:string"/>
      <xsl:variable name="degen" select="f:degenerify($identifier)" as="xs:string"/>
      <xsl:choose>
         <xsl:when test="exists($specialTypes($degen))">
            <xsl:sequence select="$specialTypes($degen)"/>
         </xsl:when>
         <xsl:otherwise>
            <xsl:sequence select="f:adjustTypeNamespace($degen)"/>
         </xsl:otherwise>
      </xsl:choose>
   </xsl:function>
   
   <xsl:function name="f:adjustTypeNamespace" as="xs:string">
      <xsl:param name="typeName" as="xs:string"/>
      <xsl:choose>
         <xsl:when test="starts-with($typeName, 'java.')">
            <xsl:sequence select="'Saxon.Ejava.' || substring($typeName, 6)"/>
         </xsl:when>
         <xsl:when test="starts-with($typeName, 'javax.')">
            <xsl:sequence select="'Saxon.Ejavax.' || substring($typeName, 7)"/>
         </xsl:when>
         <xsl:when test="starts-with($typeName, 'com.saxonica.')">
            <xsl:sequence select="'Saxon.Eej.' || substring-after($typeName, 'com.saxonica.')"/>
         </xsl:when>
         <xsl:when test="starts-with($typeName, 'net.sf.saxon.')">
            <xsl:sequence select="'Saxon.Hej.' || substring-after($typeName, 'net.sf.saxon.')"/>
         </xsl:when>
         <xsl:otherwise>
            <xsl:sequence select="$typeName"/>
         </xsl:otherwise>
      </xsl:choose>
   </xsl:function>
   
   <xsl:function name="f:renameGenericType" as="xs:string">
      <xsl:param name="identifier" as="xs:string"/>
      <!--<xsl:message>renameGenericType {$identifier}</xsl:message>-->
      <xsl:value-of>
         <xsl:text>{f:renameType($identifier)}</xsl:text>
         <xsl:if test="contains($identifier, '&lt;')">
            <xsl:text>&lt;</xsl:text>
            <xsl:value-of select="($identifier => substring-after('&lt;') => substring-before('>') => tokenize(',')) ! f:renameType(.)"
               separator=", "/>
            <xsl:text>&gt;</xsl:text>
         </xsl:if>
      </xsl:value-of>
   </xsl:function>
   
   <!-- Get the target type of a method call expression or field access expression -->
   <xsl:function name="f:targetType" as="xs:string?">
      <xsl:param name="expr" as="element(*)"/>
      <xsl:variable name="givenType" select="if ($expr!@RESOLVED_TYPE) then $expr!@RESOLVED_TYPE else $expr!scope!@RETURN"/>
      <xsl:sequence select="f:degenerifyWithoutEscaping($givenType)"/>
   </xsl:function>
   
   <xsl:function name="f:degenerify" as="xs:string?">
      <xsl:param name="nameOfType" as="xs:string?"/>
      <xsl:sequence select="replace($nameOfType, '^\? (extends|super) ', '') => replace('&lt;.*$', '') => f:escapeDottedName()"/>
   </xsl:function>
   
   <xsl:function name="f:degenerifyWithoutEscaping" as="xs:string?">
      <xsl:param name="nameOfType" as="xs:string?"/>
      <xsl:sequence select="replace($nameOfType, '^\? (extends|super) ', '') => replace('&lt;.*$', '')"/>
   </xsl:function>
   
   <xsl:function name="f:escapeSimpleName" as="xs:string">
      <xsl:param name="simpleName" as="xs:string"/>
      <xsl:sequence select="if ($simpleName = $reservedWords) then ('@' || $simpleName) else $simpleName"/>
   </xsl:function>
   
   <xsl:function name="f:escapeMethodName" as="xs:string">
      <xsl:param name="simpleName" as="xs:string"/>
      <xsl:sequence select="if ($simpleName = ($reservedWords, $primitiveTypeNames)) then ('@' || $simpleName) else $simpleName"/>
   </xsl:function>
   
   <xsl:function name="f:escapeDottedName" as="xs:string">
      <xsl:param name="nameOfType" as="xs:string?"/>
      <xsl:sequence select="tokenize($nameOfType, '\.') ! f:escapeSimpleName(.) => string-join('.')"/>
   </xsl:function>
   
   <!-- Convert first character of a string, and any character immediately following an underscore, to upper-case -->
   <xsl:function name="f:capitalize" as="xs:string">
      <xsl:param name="in" as="xs:string"/>
      <xsl:sequence select="tokenize($in, '_') ! (upper-case(substring(., 1, 1)) || substring(., 2)) => string-join('')"/>
   </xsl:function>
   
 
   
   <!-- Ask whether the first class implements or extends the second class.
        Both classes are supplied by fully-qualified name, without type arguments.
        The first class must be present in the digest, i.e. must be a Saxon Java class -->
   <xsl:function name="f:extends" as="xs:boolean">
      <xsl:param name="subclass" as="xs:string"/>
      <xsl:param name="superclass" as="xs:string"/>
      <xsl:variable name="digestClass" as="element(*)?"
         select="key('digestClassKey', $subclass, $digest)"/>
      <xsl:choose>
         <xsl:when test="$subclass eq $superclass">
            <xsl:sequence select="true()"/>
         </xsl:when>
         <xsl:when test="empty($digestClass)">
            <xsl:sequence select="false()"/>
         </xsl:when>
         <xsl:when test="$digestClass/(extends|implements)/@name = $superclass">
            <xsl:sequence select="true()"/>
         </xsl:when>
         <xsl:when test="some $name in $digestClass/(extends|implements)/@name satisfies f:extends($name, $superclass)">
            <xsl:sequence select="true()"/>
         </xsl:when>
         <xsl:otherwise>
            <xsl:sequence select="false()"/>
         </xsl:otherwise>
      </xsl:choose>
   </xsl:function>
 
   <!-- Ask whether an interface should be converted to a C# delegate -->
   <xsl:function name="f:isDelegate" as="xs:boolean">
      <xsl:param name="interfaceDecl" as="element()"/>
      <xsl:choose>
         <xsl:when test="$interfaceDecl/annotations/annotation/name/@identifier='CSharpDelegate'">
            <xsl:sequence select="xs:boolean($interfaceDecl/annotations/annotation[name/@identifier='CSharpDelegate']/memberValue/@value)"/>
         </xsl:when>
         <xsl:otherwise>
            <xsl:sequence select="exists($interfaceDecl[@nodeType='ClassOrInterfaceDeclaration'][@isInterface='true']
               [annotations/annotation/name/@identifier='FunctionalInterface'][count(members/member)=1])"/>
         </xsl:otherwise>
      </xsl:choose>      
   </xsl:function>
   
 
   
   <xsl:variable name="operators" as="map(xs:string, xs:string)"
      select="map{'AND': ' &amp;&amp; ',
                  'OR': ' || ',
                  'BINARY_AND': '&amp;',
                  'BINARY_OR': '|',
                  'BITWISE_COMPLEMENT': '~',
                  'XOR': '^',
                  'EQUALS': ' == ',
                  'NOT_EQUALS': ' != ',
                  'LESS': '&lt;',
                  'GREATER': '>',
                  'LESS_EQUALS': '&lt;=',
                  'GREATER_EQUALS': '>=',
                  'LEFT_SHIFT': '&lt;&lt;',
                  'SIGNED_RIGHT_SHIFT': '&gt;&gt;',
                  'UNSIGNED_RIGHT_SHIFT': '&gt;&gt;&gt;',
                  'PLUS': '+',
                  'MINUS': '-',
                  'MULTIPLY': '*',
                  'DIVIDE': '/',
                  'REMAINDER': '%',
                  'LOGICAL_COMPLEMENT': '!'
                  }"/>
 
  
   
   <xsl:function name="f:isInterfaceMember" as="xs:boolean">
      <xsl:param name="expr" as="element(*)"/>
      <xsl:sequence select="$expr/ancestor::type[@nodeType='ClassOrInterfaceDeclaration'][1]/@isInterface='true'"/>
   </xsl:function>
   
   <!-- determine whether an expression is constant expression. The C# language rules are complex,
      but we allow any unary or binary expression whose operands are constant expressions, literals,
      or the names of variables initialised to constant expressions.
      Declaring the item with a "const" keyword is needed to allow its use in switch/case -->
   <xsl:function name="f:isConstantExpression" as="xs:boolean">
      <xsl:param name="expr" as="element()?"/>
      <xsl:sequence select="contains($expr/@nodeType, 'Literal') 
         or ($expr/@nodeType='BinaryExpr' and f:isConstantExpression($expr/left) and f:isConstantExpression($expr/right)) 
         or ($expr/@nodeType='UnaryExpr' and f:isConstantExpression($expr/expression)
         or ($expr/@nodeType='NameExpr' and $expr/ancestor::type[1]/members/member[@nodeType='FieldDeclaration']
              [variables/variable[name/@identifier=$expr/name/@identifier and f:isConstantExpression(initializer)]]))"/>
   </xsl:function>
         
   <xsl:function name="f:allocateInnerClassName" as="xs:string">
      <xsl:param name="creationExpr" as="element(*)"/>
      <xsl:variable name="resolvedName" select="f:resolveTypeName($creationExpr/type)"/>
      <xsl:variable name="allocatedId" as="xs:string">
         <xsl:number select="$creationExpr" level="any"/>
      </xsl:variable>
      <xsl:text>Inner_{tokenize(f:degenerify($resolvedName), '\.')[last()]}_{$allocatedId}</xsl:text>
   </xsl:function>
   
   <xsl:function name="f:resolveTypeName">
      <xsl:param name="typeElem" as="element(type)"/>
      <xsl:choose>
         <xsl:when test="$typeElem/@RESOLVED_TYPE">
            <xsl:sequence select="$typeElem/@RESOLVED_TYPE"/>
         </xsl:when>
         <xsl:when test="$typeElem/@UNRESOLVED and exists($typeElem//import[name/@identifier=$typeElem/@UNRESOLVED])">
            <xsl:sequence select="f:qualifiedName($typeElem//import[name/@identifier=$typeElem/@UNRESOLVED]/name)"/>
         </xsl:when>
         <xsl:otherwise>
            <xsl:sequence select="$typeElem/name/@identifier"/>
         </xsl:otherwise>
      </xsl:choose>
   </xsl:function>
  
   <xsl:function name="f:getImplementedInterfaces" as="element(interface)*">
      <xsl:param name="base" as="element(*) (: class or interface :)"/>
      <xsl:variable name="all-interfaces" as="element(*)*">
         <xsl:variable name="immediate" select="($base/implements | $base/extends) ! key('digestClassKey', @name, $digest)"/>
         <xsl:sequence select="$immediate, $immediate ! f:getImplementedInterfaces(.)"/>
      </xsl:variable>
      <!-- deduplicate and exclude non-interfaces-->
      <xsl:sequence select="$all-interfaces[self::interface] | ()"/>
   </xsl:function>
   
   <xsl:function name="f:gatherDefaultMethods" as="element(*)*">
      <xsl:param name="thisClass" as="element()?"/>
      <xsl:for-each select="f:getImplementedInterfaces($thisClass)">
         <!--<xsl:message>Looking at interface {@name}</xsl:message>-->
         <xsl:sequence select="method[./default]"/>
      </xsl:for-each>
   </xsl:function>
   
   <xsl:function name="f:hasMethod" as="xs:boolean">
      <xsl:param name="thisClass" as="element(class)"/>
      <xsl:param name="method" as="xs:string"/>
      <xsl:param name="sig" as="xs:string"/>
      <!--<xsl:message>Has method {$thisClass/@name}#{$method}!{$sig} ? {exists($thisClass/method[@name=$method and string(@sig)=string($sig)])}</xsl:message>-->
      <xsl:sequence select="exists($thisClass/method[@name=$method and string(@sig)=string($sig)])"/>
   </xsl:function>
  
   
   <xsl:function name="f:arity" as="xs:integer">
      <xsl:param name="methodDigest" as="element(method)"/>
      <xsl:sequence select="if ($methodDigest/@params) then count(tokenize($methodDigest/@params, '\|')) else 0"/>
   </xsl:function>
   
   <xsl:function name="f:hasInheritedMethod" as="xs:boolean">
      <xsl:param name="thisClass" as="element(*)?"/> <!-- 'class' or 'interface' -->
      <xsl:param name="methodName" as="xs:string"/>
      <xsl:param name="arity" as="xs:integer"/>
      <!--<xsl:message>Seeking {$thisClass/@name}::{$methodName}#{$arity}</xsl:message>-->
      <xsl:sequence select="exists($thisClass/method[@name=$methodName and f:arity(.)=$arity])
         or (some $class in $thisClass/(implements|extends)/@name satisfies f:hasInheritedMethod(key('digestClassKey', $class, $digest), $methodName, $arity))"/>
   </xsl:function>
   
   <xsl:function name="f:hasInheritedConcreteMethod" as="xs:boolean">
      <xsl:param name="thisClass" as="element(*)?"/> <!-- 'class' or 'interface' -->
      <xsl:param name="methodName" as="xs:string"/>
      <xsl:param name="sig" as="xs:string"/>
      <!--<xsl:message>Seeking {$thisClass/@name}::{$methodName}#{$arity}</xsl:message>-->
      <xsl:sequence select="exists($thisClass/method[@name=$methodName and string(@sig)=$sig][not(@abstract='1') and not(parent::interface)])
         or (some $class in $thisClass/(implements|extends)/@name satisfies f:hasInheritedConcreteMethod(key('digestClassKey', $class, $digest), $methodName, $sig))"/>
   </xsl:function>
   
   <xsl:function name="f:unescapeJava" as="xs:string">
      <xsl:param name="in" as="xs:string"/>
      <xsl:text>{$in => replace('\\n', '&#xa;') => replace('\\"', '"')}</xsl:text> 
   </xsl:function>
   
   
   <!--<xsl:function name="f:has-concrete-method" as="xs:boolean">
      <!-\- despite the name, an abstract method will do -\->
      <xsl:param name="thisClass" as="element(class)"/>
      <xsl:param name="method" as="xs:string"/>
      <xsl:param name="sig" as="xs:string"/>
      <!-\-<xsl:message>Has concrete method {$thisClass/@name}#{$method}!{$sig} ? {exists($thisClass/method[@name=$method and string(@sig)=string($sig) and not(@abstract='1')])}</xsl:message>-\->
      <xsl:sequence select="exists($thisClass/method[@name=$method and string(@sig)=string($sig) (:and not(@abstract='1') :)])"/>
   </xsl:function>-->
   
</xsl:stylesheet>
