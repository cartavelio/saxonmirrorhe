<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet 
	xmlns:c="http://www.w3.org/ns/xproc-step"
	xmlns:m="http://xylarium.org/ns/xml/documentation"
	xmlns:xs="http://www.w3.org/2001/XMLSchema"
	xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
	expand-text="true"
	exclude-result-prefixes="#all"
	version="3.0">
	
	<xsl:strip-space elements="*"/>
	<xsl:output method="xml" indent="true" />
	
	<m:documentation>
		<m:note>
			<m:ol>
				<m:ingress>Expected inputs:</m:ingress>
				<m:items>
					<m:li>the aggregated output from a single test suite version, for multiple Saxon products.</m:li>
				</m:items>
			</m:ol>
		</m:note>
	</m:documentation>


	<xsl:template match="*:test-suite-result/*:product">
		<xsl:copy>
			<xsl:apply-templates select="ancestor::*:test-suite-result[1]/@gradle-task" mode="infer-mode" />
			<xsl:apply-templates select="attribute(), node()" />
		</xsl:copy>
	</xsl:template>
	
	
	<xsl:template match="@gradle-task" mode="infer-mode">
		<xsl:variable name="product" select="tokenize(., '_')[1]" as="xs:string" />
		<xsl:variable name="test-suite-name" select="ancestor::*:aggregated-test-results[1]/@test-suite-name" as="xs:string" />
		<xsl:variable name="language" select="substring-after(., concat('_', $test-suite-name, '_')) ! tokenize(., '_')[1]" as="xs:string" />
		<xsl:variable name="mode" select="substring-after(., concat(string-join(($product, $test-suite-name, $language), '_'), '_'))" as="xs:string?" />
		
		<xsl:if test="$mode != ''">
			<xsl:attribute name="mode" select="$mode" />
		</xsl:if>
	</xsl:template>
	
	
	<xsl:template match="node() | attribute()">
		<xsl:copy>
			<xsl:apply-templates select="attribute(), node()" />
		</xsl:copy>
	</xsl:template>
	
</xsl:stylesheet>