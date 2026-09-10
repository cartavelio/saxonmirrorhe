<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet 
	xmlns:fos="http://www.w3.org/xpath-functions/spec/namespace"
	xmlns:local="http://saxonica.sf.net/xslt/utils/function"
	xmlns:math="http://www.w3.org/2005/xpath-functions/math"
	xmlns:m="http://xylarium.org/ns/xml/documentation"
	xmlns:xs="http://www.w3.org/2001/XMLSchema"
	xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
	expand-text="true"
	exclude-result-prefixes="#all"
	version="3.0">
	
	<xsl:param name="default-prefix" select="'fn'" as="xs:string" />
	
	<xsl:strip-space elements="*"/>
	<xsl:output method="xml" indent="true" />
	
	<xsl:template match="/">
		<xsl:processing-instruction name="xml-model">href="../../schemas/simplified_catalog.rnc" type="application/relax-ng-compact-syntax" </xsl:processing-instruction>
		<catalog>
			<xsl:apply-templates />
		</catalog>
	</xsl:template>
	
	<xsl:template match="fos:functions">
		<xsl:apply-templates select="* except fos:global-variables" />
	</xsl:template>
	
	<xsl:template match="fos:record-type">
		<xsl:variable name="prefix" select="local:get-prefix(self::*)" as="xs:string?" />
		<function name="{if (normalize-space($prefix) = '') then @id else string-join(($prefix, @id), ':')}" />
	</xsl:template>
	
	<xsl:template match="fos:function">
		<xsl:variable name="prefix" select="local:get-prefix(self::*)" as="xs:string?" />
		<function name="{if (normalize-space($prefix) = '') then @name else string-join(($prefix, @name), ':')}" />
	</xsl:template>
	
	<xsl:template match="comment() | processing-instruction()" />
	
	<xsl:template match="element() | attribute()">
		<xsl:copy copy-namespaces="no">
			<xsl:apply-templates select="attribute(), node()" />
		</xsl:copy>
	</xsl:template>
	
	<xsl:function name="local:get-prefix" as="xs:string">
		<xsl:param name="function-in" as="element()" />
		
		<xsl:value-of select="if ($function-in/@prefix/normalize-space(.) != '') then $function-in/@prefix else $default-prefix" />

	</xsl:function>
	
</xsl:stylesheet>