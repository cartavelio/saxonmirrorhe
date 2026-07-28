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
					<m:li>the aggregated output from QT3-4 XPath and XQuery test suites for multiple Saxon products.</m:li>
				</m:items>
			</m:ol>
		</m:note>
	</m:documentation>

	<xsl:template match="/">
		<xsl:for-each-group select="aggregated-test-results/c:directory/c:directory" group-by="@name">
			<xsl:variable name="test-suite-name" select="current-grouping-key()" as="xs:string" />
			<xsl:for-each-group select="current-group()/descendant::*:test-suite-result" group-by="*:submission/*:test-run/@test-suite-version">
				<xsl:document>
					<aggregated-test-results test-suite-name="{$test-suite-name}" test-suite-version="{current-grouping-key()}">
						<xsl:apply-templates select="current-group()" />
					</aggregated-test-results>
				</xsl:document>
			</xsl:for-each-group>
		</xsl:for-each-group>
	</xsl:template>
	
	<xsl:template match="*:test-suite-result">
		<xsl:copy>
			<xsl:attribute name="gradle-task" select="parent::c:file/@name/substring-before(., '.xml')" />
			<xsl:apply-templates select="attribute(), node()" />
		</xsl:copy>
	</xsl:template>
	
	<xsl:template match="node() | attribute()">
		<xsl:copy>
			<xsl:apply-templates select="attribute(), node()" />
		</xsl:copy>
	</xsl:template>
	
</xsl:stylesheet>