<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet 
	xmlns:c="http://www.w3.org/ns/xproc-step"
	xmlns:fn="http://www.w3.org/2005/xpath-functions"
	xmlns:local="http://saxonica.sf.net/xslt/utils/function"
	xmlns:m="http://xylarium.org/ns/xml/documentation"
	xmlns:xs="http://www.w3.org/2001/XMLSchema"
	xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
	expand-text="true"
	exclude-result-prefixes="#all"
	version="3.0">
	
	<xsl:param name="filename" as="xs:string" />
	<xsl:param name="location" as="xs:string" />
	<xsl:param name="pulled" as="xs:dateTimeStamp?" />	
	
	<xsl:key name="function-set-metadata" match="function-set" use="@location" />
	
	<xsl:strip-space elements="*"/>
	<xsl:output method="xml" indent="true" />
	
	<xsl:variable name="metadata-lookup" select="document('../../data/function_set_metadata.xml')" as="document-node()" />
	
	<m:documentation>
		<m:note>
			<m:ol>
				<m:ingress>Expected inputs:</m:ingress>
				<m:items>
					<m:li>a *FunctionSet.java file, as text/plain.</m:li>
				</m:items>
			</m:ol>
		</m:note>
	</m:documentation>

	
	<xsl:template match="/*:analyze-string-result">
		<xsl:variable name="package" select="fn:analyze-string(fn:non-match[1]/text(), 'package ([^;]*);')/*:match[1]/*:group/string(.)" as="xs:string" />		
		<xsl:variable name="metadata" select="$metadata-lookup/key('function-set-metadata', $location)" as="element()?" />
		
		<registry id="{$package}.{substring-before($filename, '.java')}" pulled="{$pulled}" name="{$metadata/@name}" catalog="{$metadata/@catalog}">
			<xsl:for-each-group select="descendant::*:match/*:group[@nr = '2']" group-by="text()">
				<function name="{if ($metadata/@prefix/normalize-space(.) = '') then current-grouping-key() else string-join(($metadata/@prefix, current-grouping-key()), ':')}" />
			</xsl:for-each-group>							
		</registry>
	</xsl:template>
	
	
</xsl:stylesheet>