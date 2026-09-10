<?xml version="1.0" encoding="UTF-8"?>
<sch:schema 
	xmlns:sch="http://purl.oclc.org/dsdl/schematron" queryBinding="xslt3"
	xmlns:sqf="http://www.schematron-quickfix.com/validator/process"
	xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
	
	<sch:let name="xsd-special-datatypes" value="'xs:anySimpleType', 'xs:anyAtomicType'" />
	<sch:let name="xsd-list-types" value="'xs:ENTITIES', 'xs:IDREFS', 'xs:NMTOKENS'" />
	<sch:let name="xsd-primitive-datatypes" value="'xs:anyURI', 'xs:base64Binary', 'xs:boolean', 'xs:date', 'xs:dateTime', 'xs:decimal', 'xs:double', 'xs:duration', 'xs:float', 'xs:gDay', 'xs:gMonth', 'xs:gMonthDay', 'xs:gYear', 'xs:gYearMonth', 'xs:hexBinary', 'xs:NOTATION', 'xs:QName', 'xs:string', 'xs:time'" />
	<sch:let name="xsd-non-primitives-derived-from-anyAtomicType" value="'xs:dateTimeStamp', 'xs:dayTimeDuration', 'xs:yearMonthDuration', 'xs:integer', 'xs:long', 'xs:int', 'xs:short', 'xs:byte', 'xs:nonNegativeInteger', 'xs:positiveInteger', 'xs:unsignedLong', 'xs:unsignedInt', 'xs:unsignedShort', 'xs:unsignedByte', 'xs:nonPositiveInteger', 'xs:negativeInteger', 'xs:normalizedString', 'xs:token', 'xs:language', 'xs:Name', 'xs:NCName', 'xs:ENTITY', 'xs:ID', 'xs:IDREF', 'xs:NMTOKEN'" />
	<sch:let name="xsd-atomic-variety" value="'xs:anyAtomicType', 'xs:untypedAtomic', $xsd-primitive-datatypes, $xsd-non-primitives-derived-from-anyAtomicType" />
	<sch:let name="xsd-pre-defined" value="'xs:anyType', $xsd-special-datatypes, $xsd-atomic-variety, $xsd-primitive-datatypes, $xsd-list-types, $xsd-non-primitives-derived-from-anyAtomicType" />
	<sch:let name="xsd-ordinary" value="for $datatype in $xsd-pre-defined return if ($datatype = ($xsd-primitive-datatypes, $xsd-special-datatypes)) then () else $datatype" />
	<sch:let name="xsd-abstract" value="'list', 'union'" />
	
	<sch:let name="xdm-xsd-extensions" value="'xs:anyAtomicType', 'xs:dayTimeDuration', 'xs:yearMonthDuration', 'xs:error', 'xs:dateTimeStamp', 'xs:untypedAtomic', 'xs:numeric'" />
	<sch:let name="xdm-schema" value="$xsd-primitive-datatypes, $xsd-list-types, $xdm-xsd-extensions" />
	<sch:let name="xdm-pre-defined" value="$xdm-xsd-extensions, $xdm-schema" />
	
	<sch:let name="xpath-pure-union" value="'xs:numeric', 'xs:error'" />
	<sch:let name="xpath-generalized-atomic-types" value="$xsd-atomic-variety, $xpath-pure-union, 'enum()'" />	
	<sch:let name="xpath-namespace-sensitive" value="'xs:QName', 'xs:NOTATION'" />
	
	<!-- TODO: Check against 4.0 -->
	<sch:let name="xpath-pre-defined" value="$xpath-pure-union, $xpath-generalized-atomic-types, $xpath-namespace-sensitive" />
	
	<!-- https://qt4cg.org/specifications/xquery-40/xpath-40.html#prod-xpath40-XNodeType -->
	<sch:let name="xpath-xnode-types" value="'node()', 'document-node()', 'element()', 'attribute()', 'processing-instruction()', 'comment()', 'text()', 'namespace-node()', 'schema-element()', 'schema-attribute()'" />
	
	<!-- https://qt4cg.org/specifications/xquery-40/xpath-40.html#id-matching-item -->
	<sch:let name="xpath-item-types" value="'item()', $xpath-xnode-types, 'jnode()', 'gnode()', 'function()', 'map()', 'array()', 'record()', 'enum()'" />
	
	<!-- https://qt4cg.org/specifications/xquery-40/xpath-40.html#id-sequencetype-syntax -->
	<sch:let name="xpath-sequence" value="'empty-sequence()', $xpath-item-types" />
	
	<!-- Needs checking -->
	<sch:let name="xpath-abstract" value="'xs:NOTATION', 'xs:anySimpleType', 'xs:anyAtomicType', 'sequence', 'xs:anyType', 'gnode()'" />
	
	<sch:let name="all-datatypes" value="distinct-values(($xsd-pre-defined, $xdm-pre-defined, $xpath-pre-defined, $xpath-sequence))" />
	<!-- TODO: What about xs:untyped? -->

	

	<sch:pattern>
		<sch:title>Cross-references</sch:title>
		
		<sch:rule context="*[@id]">
			<sch:assert role="error" test="self::*/@id = lower-case(current()/@id)">ID is not all lower-case: <sch:value-of select="current()/@id"/></sch:assert>			
		</sch:rule>
		
	</sch:pattern>


	<sch:pattern id="list-membership" abstract="true">
			
		<sch:rule context="datatype[category/type/@name = $list-name]">
			<sch:assert role="error" test="name = $list-members">Unexpected inclusion of <sch:value-of select="name"/> in <sch:value-of select="$list-name"/> datatype</sch:assert>
			<sch:assert role="warning" test="sources[source/@ref = $source-id]"><sch:value-of select="ancestor-or-self::datatype[1]/name"/> is missing a source: <sch:value-of select="$source-id" /> (for <sch:value-of select="$list-name" /> type)</sch:assert>
		</sch:rule>			
			
		<sch:rule context="datatype[not(category/type/@name = $list-name)]/name">
			<sch:report role="error" test=". = $list-members"><sch:value-of select="parent::*/name"/> is missing <sch:value-of select="$list-name"/> type</sch:report>			
		</sch:rule>
							
	</sch:pattern>


	<sch:pattern>
		<sch:title>Comparison family</sch:title>
		
		<sch:rule context="datatype[category/type/@name = 'Item'][not(category/type/@name[starts-with(., 'Abstract')])]/comparison-family">
			<sch:assert role="warning" test=". != ''">Missing comparison family for <sch:value-of select="parent::*/name"/></sch:assert>			
		</sch:rule>
		
		<sch:rule context="datatype[not(category/type/@name = 'Item') or (category/type/@name = 'Item' and category/type/@name[starts-with(., 'Abstract')])]/comparison-family[. != '']">
			<sch:assert role="error" test=". != ''">Unexpected comparison family for <sch:value-of select="parent::*/name"/></sch:assert>			
		</sch:rule>
	</sch:pattern>



	<sch:pattern is-a="list-membership">
		<sch:title>Special datatypes (XSD)</sch:title>
		
		<sch:param name="list-name" value="'Special (XSD)'" />
		<sch:param name="list-members" value="($xsd-special-datatypes)" />
		<sch:param name="source-id" value="'xml-schema-11-2-special-datatypes'" />
				
	</sch:pattern>
	
	
	<sch:pattern is-a="list-membership">
		<sch:title>Special datatypes (XPath)</sch:title>
		
		<sch:param name="list-name" value="'Special (XPath)'" />
		<sch:param name="list-members" value="('empty-sequence()')" />
		<sch:param name="source-id" value="'xpath-40-sequence-types'" />
		
	</sch:pattern>
	
	
	
	<sch:pattern is-a="list-membership">
		<sch:title>Atomic datatypes</sch:title>
		
		<sch:param name="list-name" value="'Atomic'" />
		<sch:param name="list-members" value="('xs:anyAtomicType', $xsd-atomic-variety)" />
		<sch:param name="source-id" value="'xml-schema-11-2-atomic'" />
		
	</sch:pattern>
	
	<sch:pattern is-a="list-membership">
		<sch:title>Generalized Atomic datatypes</sch:title>
		
		<sch:param name="list-name" value="'Generalized Atomic (XPath)'" />
		<sch:param name="list-members" value="($xpath-generalized-atomic-types)" />
		<sch:param name="source-id" value="'xpath-40-generalized-atomic-type'" />
		
	</sch:pattern>
	
	<sch:pattern is-a="list-membership">
		<sch:title>List datatypes</sch:title>
		
		
		<sch:param name="list-name" value="'List'" />
		<sch:param name="list-members" value="($xsd-list-types)" />
		<sch:param name="source-id" value="'xml-schema-11.2-list'" />
		
	</sch:pattern>

	<sch:pattern is-a="list-membership">
		<sch:title>Union datatypes</sch:title>
		
		
		<sch:param name="list-name" value="'Union'" />
		<sch:param name="list-members" value="($xpath-pure-union)" />
		<sch:param name="source-id" value="'xml-schema-11-2-union'" />
		
	</sch:pattern>
	
	
	<sch:pattern is-a="list-membership">
		<sch:title>XSD pre-defined types</sch:title>
		
		<sch:param name="list-name" value="'Schema (XSD)'" />
		<sch:param name="list-members" value="($xsd-pre-defined)" />
		<sch:param name="source-id" value="'xml-schema-11-2'" />
		
	</sch:pattern>
	
	<sch:pattern is-a="list-membership">
		<sch:title>XSD primitive datatypes</sch:title>
		
		<sch:param name="list-name" value="'Primitive'" />
		<sch:param name="list-members" value="($xsd-primitive-datatypes)" />
		<sch:param name="source-id" value="'xml-schema-11-2-primitive'" />
	</sch:pattern>
	
	
	<sch:pattern is-a="list-membership">
		<sch:title>XDM Schema datatypes</sch:title>
		<!-- https://qt4cg.org/specifications/xpath-datamodel-40/Overview.html#xs-types -->
		
		<sch:param name="list-name" value="'Schema (XDM)'" />
		<sch:param name="list-members" value="($xdm-schema)" />
		<sch:param name="source-id" value="'xdm-40-types'" />
		
	</sch:pattern>
	
	<sch:pattern is-a="list-membership">
		<sch:title>Namespace-sensitive datatypes</sch:title>
		<!-- https://qt4cg.org/specifications/xquery-40/xpath-40.html#id-namespace-sensitive -->
		
		<sch:param name="list-name" value="'Namespace-sensitive (XPath)'" />
		<sch:param name="list-members" value="($xpath-namespace-sensitive)" />
		<sch:param name="source-id" value="'xpath-40-namespace-sensitive-datatypes'" />
		
	</sch:pattern>
	
	
	<sch:pattern is-a="list-membership">
		<sch:title>Ordinary</sch:title>
		<!-- https://www.w3.org/TR/xmlschema11-2/#dt-ordinary -->
		
		<sch:param name="list-name" value="'Ordinary'" />
		<sch:param name="list-members" value="($xsd-ordinary)" />
		<sch:param name="source-id" value="'xml-schema-11-2-ordinary'" />
		
	</sch:pattern>

	
	<!-- TODO: date/time -->
	<!-- TODO: Numeric -->
	<!-- TODO: XPath sequence types -->
		
	<sch:pattern is-a="list-membership">
		<sch:title>Item</sch:title>
		
		<sch:param name="list-name" value="'Item'" />
		<sch:param name="list-members" value="($xpath-item-types)" />
		<sch:param name="source-id" value="'xpath-40-matching-item'" />
		
	</sch:pattern>		
		
	
	<sch:pattern is-a="list-membership">
		<sch:title>XSD abstract datatypes</sch:title>
		
		<sch:param name="list-name" value="'Abstract (XSD)'" />
		<sch:param name="list-members" value="($xsd-abstract)" />
		<sch:param name="source-id" value="'xml-schema-11-2-atomic-list-union'" />
		
	</sch:pattern>
	
	
	<sch:pattern is-a="list-membership">
		<sch:title>XPath abstract datatypes</sch:title>
		
		<sch:param name="list-name" value="'Abstract (XPath)'" />
		<sch:param name="list-members" value="($xpath-abstract)" />
		<sch:param name="source-id" value="'xpath-31-cast'" />
		
	</sch:pattern>
	
	
	<sch:pattern>
		<sch:title>Missing datatypes</sch:title>
		<sch:let name="missing-datatypes" value="for $datatype in $all-datatypes return if (//datatype/name = $datatype) then () else $datatype" />
		

		<sch:rule context="datatypes">
			<sch:report role="error" test="count($missing-datatypes) > 0">Missing datatype(s): <sch:value-of select="string-join($missing-datatypes, ', ')" /></sch:report>								
		</sch:rule>
		
	</sch:pattern>
	
	
	<sch:pattern>
		<sch:title>Duplicate datatype</sch:title>
				
		<sch:rule context="datatype">
			<sch:let name="name" value="name" />
			<sch:let name="id" value="@id" />
			<sch:let name="earlier-matches" value="self::datatype/(ancestor::datatype|preceding::datatype)[@id != $id][name = $name]/@id" />
			
			<sch:report role="error" test="count($earlier-matches) > 0">Duplicate name. A datatype named '<sch:value-of select="$name" />' has already been defined (ID: <sch:value-of select="string-join($earlier-matches, ', ')" />)</sch:report>								
		</sch:rule>
		
	</sch:pattern>


</sch:schema>