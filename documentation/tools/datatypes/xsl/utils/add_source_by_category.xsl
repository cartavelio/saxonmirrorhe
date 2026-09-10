<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet 
	xmlns:int="http://saxon.sf.net/xslt/functions/internal"
	xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
	xmlns:xs="http://www.w3.org/2001/XMLSchema"
	xmlns:math="http://www.w3.org/2005/xpath-functions/math"
	exclude-result-prefixes="xs math int"
	expand-text="true"
	version="3.0">
	
	<xsl:output indent="true" 
		suppress-indentation="p name description source" 
		omit-xml-declaration="false" />
	
	<xsl:variable name="type-name" select="'Item'" as="xs:string" />
	<xsl:variable name="source-id" select="'xpath-40-matching-item'" as="xs:string" />
	
	<xsl:template match="datatype[category/type/@name = $type-name][not(sources/source/@ref = $source-id)][sources]/sources">
		<xsl:copy>
			<xsl:apply-templates select="@*, node()" />
			<source ref="{$source-id}" />
		</xsl:copy>
	</xsl:template>


	<xsl:template match="datatype[category/type/@name = $type-name][not(sources/source/@ref = $source-id)][not(sources)]">
		<xsl:copy>
			<xsl:apply-templates select="@*, node()[not(local-name() = 'sub-types')]" />
			<sources>
				<source ref="{$source-id}" />
			</sources>
			<xsl:apply-templates select="sub-types" />
		</xsl:copy>
	</xsl:template>
	
	
	<xsl:template match="node() | attribute()">
		<xsl:copy>
			<xsl:apply-templates select="@*, node()" />
		</xsl:copy>
	</xsl:template>

		
	
</xsl:stylesheet>