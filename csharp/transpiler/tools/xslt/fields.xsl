<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
   xmlns:xs="http://www.w3.org/2001/XMLSchema"
   xmlns:f="http://ns.saxonica.com/local-functions"
   exclude-result-prefixes="#all"
   expand-text="yes"
   default-mode="fields"
   version="3.0">
   
   <xsl:mode name="fields" saxon:trace="no" xmlns:saxon="http://saxon.sf.net/" on-no-match="fail"/>
   
   <!-- This module is concerned with field access expressions, especially rules for 
      properties that are specific to particular classes. All the rules in this 
      module are in mode "fields", and all pertain to expressions satisfying
      the pattern `*[@nodeType='FieldAccessExpr']`, so this condition is
      not re-stated as part of each pattern.
   -->
   
   <!--<xsl:template match="*" priority="1000">
      <xsl:message>Field access to {name/@identifier}</xsl:message>
      <xsl:text>/*Field access to {name/@identifier}*/</xsl:text>
      <xsl:next-match/>
   </xsl:template>-->
   
   <!-- .length accessor on arrays -->
   <xsl:template match="*[name/@identifier='length'][ends-with(@RESOLVED_TYPE, '[]')]" priority="50">
      <xsl:apply-templates select="scope" mode="xml-to-cs"/>
      <xsl:text>.Length</xsl:text>
   </xsl:template>
   
   <!-- .class accessor on any object -->
   <xsl:template match="*[name/@identifier='class']" priority="50">
      <xsl:text>typeof(</xsl:text>
      <xsl:apply-templates select="scope" mode="xml-to-cs"/>
      <xsl:text>)</xsl:text>
   </xsl:template>
   
   <!-- Identify an unspecific static field accessor  -->
   <xsl:template match="*[scope/name/@nodeType='SimpleName'][ends-with(f:degenerify(@RESOLVED_TYPE), '.'||scope/name/@identifier)][not(scope/scope)]" 
      priority="18">
      <xsl:text>{f:renameGenericType(@RESOLVED_TYPE)}.{name/@identifier}</xsl:text>
   </xsl:template>
   
   <!-- Identify a static field accessor on java.lang.System -->
   <xsl:template match="*[scope/name/@nodeType='SimpleName'][@RESOLVED_TYPE = 'java.lang.System']" priority="19">
      <xsl:choose>
         <xsl:when test="name/@identifier = 'in'">System.Console.In</xsl:when>
         <xsl:when test="name/@identifier = 'out'">System.Console.Out</xsl:when>
         <xsl:when test="name/@identifier = 'err'">System.Console.Error</xsl:when>
         <xsl:otherwise>
            <xsl:next-match/>
         </xsl:otherwise>
      </xsl:choose>
   </xsl:template>
   
   <!-- Identify a static field accessor on java.lang.Boolean -->
   <xsl:template match="*[scope/name/@nodeType='SimpleName'][@RESOLVED_TYPE = 'java.lang.Boolean']" priority="19">
      <xsl:choose>
         <xsl:when test="name/@identifier = 'TRUE'">true</xsl:when>
         <xsl:when test="name/@identifier = 'FALSE'">false</xsl:when>
         <xsl:otherwise>
            <xsl:next-match/>
         </xsl:otherwise>
      </xsl:choose>
   </xsl:template>
   
   <!-- Identify a static field accessor such as Integer.MAX_VALUE -->
   <xsl:template match="*[scope/name/@nodeType='SimpleName'][@RESOLVED_TYPE = 'java.lang.Integer']" priority="19">
      <xsl:choose>
         <xsl:when test="name/@identifier = 'MAX_VALUE'">System.Int32.MaxValue</xsl:when>
         <xsl:when test="name/@identifier = 'MIN_VALUE'">System.Int32.MinValue</xsl:when>
         <xsl:when test="name/@identifier = 'BYTES'">4</xsl:when>
         <xsl:when test="name/@identifier = 'SIZE'">32</xsl:when>
         <xsl:otherwise>System.Int32.{name/@identifier}</xsl:otherwise>
      </xsl:choose>
   </xsl:template>
   
   <!-- Identify a static field accessor such as Long.MAX_VALUE -->
   <xsl:template match="*[scope/name/@nodeType='SimpleName'][@RESOLVED_TYPE = 'java.lang.Long']" priority="19">
      <xsl:choose>
         <xsl:when test="name/@identifier = 'MAX_VALUE'">System.Int64.MaxValue</xsl:when>
         <xsl:when test="name/@identifier = 'MIN_VALUE'">System.Int64.MinValue</xsl:when>
         <xsl:otherwise>System.Int64.{name/@identifier}</xsl:otherwise>
      </xsl:choose>
   </xsl:template>
   
   <!-- Identify a static field accessor such as Short.MAX_VALUE -->
   <xsl:template match="*[scope/name/@nodeType='SimpleName'][@RESOLVED_TYPE = 'java.lang.Short']" priority="19">
      <xsl:choose>
         <xsl:when test="name/@identifier = 'MAX_VALUE'">System.Int16.MaxValue</xsl:when>
         <xsl:when test="name/@identifier = 'MIN_VALUE'">System.Int16.MinValue</xsl:when>
         <xsl:otherwise>System.Int16.{name/@identifier}</xsl:otherwise>
      </xsl:choose>
   </xsl:template>
   
   <!-- Identify a static field accessor such as Byte.MAX_VALUE -->
   <xsl:template match="*[scope/name/@nodeType='SimpleName'][@RESOLVED_TYPE = 'java.lang.Byte']" priority="19">
      <xsl:choose>
         <xsl:when test="name/@identifier = 'MAX_VALUE'">127</xsl:when>
         <xsl:when test="name/@identifier = 'MIN_VALUE'">-128</xsl:when>
         <xsl:when test="name/@identifier = 'SIZE'">8</xsl:when>
         <xsl:otherwise>System.SByte.{name/@identifier}</xsl:otherwise>
      </xsl:choose>
   </xsl:template>
   
   <!-- Identify a static field accessor such as Double.POSITIVE_INFINITY -->
   <xsl:template match="*[scope/name/@nodeType='SimpleName'][@RESOLVED_TYPE = 'java.lang.Double']" priority="19">
      <xsl:choose>
         <xsl:when test="name/@identifier = 'POSITIVE_INFINITY'">System.Double.PositiveInfinity</xsl:when>
         <xsl:when test="name/@identifier = 'NEGATIVE_INFINITY'">System.Double.NegativeInfinity</xsl:when>
         <xsl:when test="name/@identifier = 'MAX_VALUE'">System.Double.MaxValue</xsl:when>
         <xsl:when test="name/@identifier = 'MIN_VALUE'">System.Double.MinValue</xsl:when>
         <xsl:otherwise>System.Double.{name/@identifier}</xsl:otherwise>
      </xsl:choose>
   </xsl:template>
   
   <!-- Identify a static field accessor such as Float.POSITIVE_INFINITY -->
   <xsl:template match="*[scope/name/@nodeType='SimpleName'][@RESOLVED_TYPE = 'java.lang.Float']" priority="19">
      <xsl:choose>
         <xsl:when test="name/@identifier = 'POSITIVE_INFINITY'">System.Single.PositiveInfinity</xsl:when>
         <xsl:when test="name/@identifier = 'NEGATIVE_INFINITY'">System.Single.NegativeInfinity</xsl:when>
         <xsl:when test="name/@identifier = 'MAX_VALUE'">System.Single.MaxValue</xsl:when>
         <xsl:when test="name/@identifier = 'MIN_VALUE'">System.Single.MinValue</xsl:when>
         <xsl:otherwise>System.Single.{name/@identifier}</xsl:otherwise>
      </xsl:choose>
   </xsl:template>
   
   <!-- Identify a static field accessor such as Character.LETTER_NUMBER -->
   <xsl:template match="*[scope/name/@nodeType='SimpleName'][@RESOLVED_TYPE = 'java.lang.Character']" priority="19">
      <xsl:choose>
         <xsl:when test="name/@identifier = 'DECIMAL_DIGIT_NUMBER'">System.Globalization.UnicodeCategory.DecimalDigitNumber</xsl:when>
         <xsl:when test="name/@identifier = 'LETTER_NUMBER'">System.Globalization.UnicodeCategory.LetterNumber</xsl:when>
         <xsl:when test="name/@identifier = 'OTHER_NUMBER'">System.Globalization.UnicodeCategory.OtherNumber</xsl:when>
         <xsl:when test="name/@identifier = 'UPPERCASE_LETTER'">System.Globalization.UnicodeCategory.UppercaseLetter</xsl:when>
         <xsl:when test="name/@identifier = 'LOWERCASE_LETTER'">System.Globalization.UnicodeCategory.LowercaseLetter</xsl:when>
         <xsl:when test="name/@identifier = 'MODIFIER_LETTER'">System.Globalization.UnicodeCategory.ModifierLetter</xsl:when>
         <xsl:when test="name/@identifier = 'OTHER_LETTER'">System.Globalization.UnicodeCategory.OtherLetter</xsl:when> 
         <xsl:otherwise>System.Globalization.UnicodeCategory.{name/@identifier}</xsl:otherwise>
      </xsl:choose>
   </xsl:template>
   
   <!-- Identify static field accessors such as BigInteger.ZERO -->
   
   <xsl:template match="*[scope/name/@nodeType='SimpleName'][@RESOLVED_TYPE = 'java.math.BigInteger']" priority="19">
      <xsl:choose>
         <xsl:when test="name/@identifier='ZERO'">System.Numerics.BigInteger.Zero</xsl:when>
         <xsl:when test="name/@identifier='ONE'">System.Numerics.BigInteger.One</xsl:when>
         <xsl:when test="name/@identifier='TWO'">Saxon.Impl.Helpers.BigIntegerUtils.TWO</xsl:when>
         <xsl:otherwise>
            <xsl:next-match/>
         </xsl:otherwise>
      </xsl:choose>
   </xsl:template>
   
   <!-- Identify static field accessors such as BigDecimal.TEN -->
   <xsl:template match="*[scope/name/@nodeType='SimpleName'][@RESOLVED_TYPE = 'java.math.BigDecimal']" priority="19">
      <xsl:choose>
         <xsl:when test="name/@identifier='ZERO'">Singulink.Numerics.BigDecimal.Zero</xsl:when>
         <xsl:when test="name/@identifier='ONE'">Singulink.Numerics.BigDecimal.One</xsl:when>
         <xsl:when test="name/@identifier='TEN'">(Singulink.Numerics.BigDecimal)10</xsl:when>
         <xsl:otherwise>
            <xsl:next-match/>
         </xsl:otherwise>
      </xsl:choose>
   </xsl:template>
   
   <!-- Identify static field accessors in java.math.RoundingMode -->
   <!-- Assume that we're using these in the context of BigDecimal arithmetic -->
   <xsl:template match="*[scope/name/@nodeType='SimpleName'][@RESOLVED_TYPE = 'java.math.RoundingMode']" priority="20">
      <xsl:choose>
         <xsl:when test="name/@identifier='CEILING'">Singulink.Numerics.RoundingMode.ToPositiveInfinity</xsl:when>
         <xsl:when test="name/@identifier='DOWN'">Singulink.Numerics.RoundingMode.ToZero</xsl:when>
         <xsl:when test="name/@identifier='FLOOR'">Singulink.Numerics.RoundingMode.ToNegativeInfinity</xsl:when>
         <xsl:when test="name/@identifier='HALF_DOWN'">Singulink.Numerics.RoundingMode.MidpointToZero</xsl:when>
         <xsl:when test="name/@identifier='HALF_EVEN'">Singulink.Numerics.RoundingMode.MidpointToEven</xsl:when>
         <xsl:when test="name/@identifier='HALF_UP'">Singulink.Numerics.RoundingMode.MidpointAwayFromZero</xsl:when>
         <xsl:when test="name/@identifier='UNNECESSARY'">Singulink.Numerics.RoundingMode.ToZero</xsl:when>
         <xsl:when test="name/@identifier='UP'">Singulink.Numerics.RoundingMode.AwayFromZero</xsl:when>
         <xsl:otherwise>
            <xsl:next-match/>
         </xsl:otherwise>
      </xsl:choose>
   </xsl:template>
   
   <!-- Identify static field accessors in net.sf.saxon.s9api.WhitespaceStrippingPolicy -->
   <xsl:template match="*[scope/name/@nodeType='SimpleName'][@RESOLVED_TYPE = 'net.sf.saxon.s9api.WhitespaceStrippingPolicy']" priority="20">
      <xsl:choose>
         <xsl:when test="name/@identifier='NONE'">Saxon.Api.WhitespacePolicy.PreserveAll</xsl:when>
         <xsl:when test="name/@identifier='IGNORABLE'">Saxon.Api.WhitespacePolicy.StripIgnorable</xsl:when>
         <xsl:when test="name/@identifier='ALL'">Saxon.Api.WhitespacePolicy.StripAll</xsl:when>
         <xsl:when test="name/@identifier='UNSPECIFIED'">Saxon.Api.WhitespacePolicy.Unspecified</xsl:when>
         <xsl:otherwise>
            <xsl:next-match/>
         </xsl:otherwise>
      </xsl:choose>
   </xsl:template>
   
   <!-- Identify static field accessors in net.sf.saxon.s9api.UnprefixedElementMatchingPolicy -->
   <xsl:template match="*[scope/name/@nodeType='SimpleName'][@RESOLVED_TYPE = 'net.sf.saxon.s9api.UnprefixedElementMatchingPolicy']" priority="20">
      <xsl:choose>
         <xsl:when test="name/@identifier='DEFAULT_NAMESPACE'">Saxon.Api.UnprefixedElementMatchingPolicy.DefaultNamespace</xsl:when>
         <xsl:when test="name/@identifier='ANY_NAMESPACE'">Saxon.Api.UnprefixedElementMatchingPolicy.AnyNamespace</xsl:when>
         <xsl:when test="name/@identifier='DEFAULT_NAMESPACE_OR_NONE'">Saxon.Api.UnprefixedElementMatchingPolicy.DefaultNamespaceOrNone</xsl:when>
         <xsl:otherwise>
            <xsl:next-match/>
         </xsl:otherwise>
      </xsl:choose>
   </xsl:template>
   
   <xsl:variable name="atomicTypes" as="xs:string*" select="tokenize('
      boolean integer double float decimal string date dateTime time duration dayTimeDuration 
      gYear gMonth gDay gYearMonth gMonthDay anyURI hexBinary base64Binary untypedAtomic QName NOTATION')"/>
   
   <xsl:function name="f:toUpperWithUnderscore">
      <xsl:param name="in" as="xs:string"/>
      <xsl:sequence select="$in => replace('(\p{Ll})(\p{Lu})', '$1_$2') => upper-case()"/>
   </xsl:function>
   
   <xsl:variable name="nodeKinds" as="xs:string*" select="tokenize('
      DOCUMENT_NODE ELEMENT_NODE ATTRIBUTE_NODE COMMENT_NODE TEXT_NODE PROCESSING_INSTRUCTION_NODE NAMESPACE_NODE')"/>
   
   <xsl:variable name="upperAtomicTypes" as="xs:string*" select="$atomicTypes ! f:toUpperWithUnderscore(.)"/>
   
   <xsl:template match="*[scope/name/@nodeType='SimpleName'][@RESOLVED_TYPE = 'net.sf.saxon.s9api.ItemType']" priority="20">
      <xsl:choose>
         <xsl:when test="name/@identifier=$upperAtomicTypes">
            <xsl:text>Saxon.Api.XdmAtomicType.{f:capitalize(lower-case(name/@identifier))}</xsl:text>
         </xsl:when>
         <xsl:when test="name/@identifier=$nodeKinds">
            <xsl:text>Saxon.Api.XdmNodeKind.{f:capitalize(lower-case(name/@identifier)) => replace('Node$', '')}</xsl:text>
            <!-- Doesn't work for PROCESSING_INSTRUCTION -->
         </xsl:when>
         <xsl:otherwise>
            <xsl:next-match/>
         </xsl:otherwise>
      </xsl:choose>
   </xsl:template>
   
   <!-- Identify static field accessors such as java.io.File.pathSeparator -->
   <xsl:template match="*[scope/name/@nodeType='SimpleName'][@RESOLVED_TYPE = 'java.io.File']" priority="20">
      <xsl:choose>
         <xsl:when test="name/@identifier='pathSeparator'">System.IO.Path.PathSeparator.ToString()</xsl:when>
         <xsl:otherwise>
            <xsl:next-match/>
         </xsl:otherwise>
      </xsl:choose>
   </xsl:template>
   
   <!-- Identify static field accessors such as java.nio.charset.StandardCharsets.UTF_8 -->
   <xsl:template match="*[scope/name/@nodeType='SimpleName'][@RESOLVED_TYPE = 'java.nio.charset.StandardCharsets']" priority="20">
      <xsl:choose>
         <xsl:when test="name/@identifier='ISO_8859_1'">System.Text.Encoding.Latin1</xsl:when>
         <xsl:when test="name/@identifier='UTF_8'">System.Text.Encoding.UTF8</xsl:when>
         <xsl:when test="name/@identifier='UTF_16'">System.Text.Encoding.Unicode</xsl:when>
         <xsl:when test="name/@identifier='UTF_16BE'">System.Text.Encoding.BigEndianUnicode</xsl:when>
         <xsl:when test="name/@identifier='UTF_16LE'">System.Text.Encoding.Unicode</xsl:when>
         <xsl:when test="name/@identifier='US_ASCII'">System.Text.Encoding.ASCII</xsl:when>
         <xsl:otherwise>
            <xsl:next-match/>
         </xsl:otherwise>
      </xsl:choose>
   </xsl:template>
   
   <!-- Fields on com.ibm.icu.text.BreakIterator -->
   <xsl:template match="*[scope/name/@nodeType='SimpleName'][@RESOLVED_TYPE = 'com.ibm.icu.text.BreakIterator']" priority="20">
      <xsl:choose>
         <xsl:when test="name/@identifier='DONE'">(int)ICU4N.Text.BreakIterator.Done</xsl:when>
         <xsl:otherwise>
            <xsl:next-match/>
         </xsl:otherwise>
      </xsl:choose>
   </xsl:template>
   
   <!-- Fields on com.ibm.icu.text.Collator -->
   <xsl:template match="*[scope/name/@nodeType='SimpleName'][@RESOLVED_TYPE = 'com.ibm.icu.text.Collator']" priority="20">
      <xsl:choose>
         <xsl:when test="name/@identifier='PRIMARY'">(int)ICU4N.Text.CollationStrength.Primary</xsl:when>
         <xsl:when test="name/@identifier='SECONDARY'">(int)ICU4N.Text.CollationStrength.Secondary</xsl:when>
         <xsl:when test="name/@identifier='TERTIARY'">(int)ICU4N.Text.CollationStrength.Tertiary</xsl:when>
         <xsl:when test="name/@identifier='QUATERNARY'">(int)ICU4N.Text.CollationStrength.Quaternary</xsl:when>
         <xsl:when test="name/@identifier='IDENTICAL'">(int)ICU4N.Text.CollationStrength.Identical</xsl:when>
         <xsl:when test="name/@identifier='NO_DECOMPOSITION'">ICU4N.Text.NormalizationMode.NoDecomposition</xsl:when>
         <xsl:when test="name/@identifier='CANONICAL_DECOMPOSITION'">ICU4N.Text.NormalizationMode.CanonicalDecomposition</xsl:when>
         <xsl:otherwise>
            <xsl:next-match/>
         </xsl:otherwise>
      </xsl:choose>
   </xsl:template>
   
   <!-- Fields on com.ibm.icu.text.CollationElementIterator -->
   <xsl:template match="*[scope/name/@nodeType='SimpleName'][@RESOLVED_TYPE = 'com.ibm.icu.text.CollationElementIterator']" priority="20">
      <xsl:choose>
         <xsl:when test="name/@identifier='NULLORDER'">ICU4N.Text.CollationElementIterator.NullOrder</xsl:when>
         <xsl:when test="name/@identifier='IGNORABLE'">ICU4N.Text.CollationElementIterator.Ignorable</xsl:when>
         <!-- note - it was misspelled in ICU4N as Ingorable. Raised as issue #35. Fixed in current version. -->
         <xsl:otherwise>
            <xsl:next-match/>
         </xsl:otherwise>
      </xsl:choose>
   </xsl:template>
   
   <!-- Fields on com.ibm.icu.text.Collator.ReorderCodes -->
   <xsl:template match="*[scope/name/@nodeType='SimpleName'][@RESOLVED_TYPE = 'com.ibm.icu.text.Collator.ReorderCodes']" priority="20">
      <xsl:choose>
         <xsl:when test="name/@identifier='SPACE'">ICU4N.Text.ReorderCodes.Space</xsl:when>
         <xsl:when test="name/@identifier='PUNCTUATION'">ICU4N.Text.ReorderCodes.Punctuation</xsl:when>
         <xsl:when test="name/@identifier='SYMBOL'">ICU4N.Text.ReorderCodes.Symbol</xsl:when>
         <xsl:when test="name/@identifier='CURRENCY'">ICU4N.Text.ReorderCodes.Currency</xsl:when>
         <xsl:when test="name/@identifier='DIGIT'">ICU4N.Text.ReorderCodes.Digit</xsl:when>
         <xsl:otherwise>
            <xsl:next-match/>
         </xsl:otherwise>
      </xsl:choose>
   </xsl:template>
   

   
 <!--  <xsl:template match="*[$refactoredProperties(name/@identifier)]" priority="20">
      <xsl:value-of select="f:refactor-property-reference(., name/@identifier)"/>
   </xsl:template>-->
   
   <!-- static field accessor for a field in a generic type such as Feature -->
   <xsl:template match="*[@FIELD_TYPE]" priority="25">
      <xsl:text>{f:renameGenericType(@FIELD_TYPE)}.{name/@identifier}</xsl:text>
   </xsl:template>
   
   <!-- default rule for field access expressions -->
   <xsl:template match="*">
      <xsl:apply-templates select="scope" mode="xml-to-cs"/>
      <xsl:text>.</xsl:text>
      <xsl:apply-templates select="name" mode="xml-to-cs"/>
   </xsl:template>

</xsl:stylesheet>