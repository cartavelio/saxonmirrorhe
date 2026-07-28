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
					<m:li>the aggregated output from running the aggregated-test-results gradle task for multiple editions of SaxonJ.</m:li>
				</m:items>
			</m:ol>
		</m:note>
	</m:documentation>


	<xsl:template match="/">
		<xsl:variable name="doc-title" select="'Test Results: Aggregated Result Files'" as="xs:string" />
		<xsl:result-document>
			<html>
				<head>
					<title>{$doc-title}</title>
					<style type="text/css">
						body {{
						padding: 1em;
						}}
						
						h1, h2 {{
						margin-bottom: .25em;
						}}
						h1 {{
						margin-top: 0;
						}}
					</style>
				</head>
				<body>
					<h1>{$doc-title}</h1>
					<p>A list of all the result files included in the aggregated data.</p>
					<ul>
						<xsl:apply-templates select="aggregated-test-results/descendant::c:file">
							<xsl:sort select="@name" data-type="text" order="ascending" />
						</xsl:apply-templates>
					</ul>
					<h2>Content Models</h2>
					<p>The same list, grouped by namespace-uri:</p>
					<ul>
						<xsl:for-each-group select="aggregated-test-results/descendant::c:file/*" group-by="namespace-uri()">
							<li>
								<h3>{current-grouping-key()} ({count(current-group())})</h3>
								<ul>
									<xsl:apply-templates select="current-group()/parent::c:file">
										<xsl:sort select="@name" data-type="text" order="ascending" />
									</xsl:apply-templates>
								</ul>
							</li>
						</xsl:for-each-group>
					</ul>
				</body>
			</html>
		</xsl:result-document>		
	</xsl:template>
	
	
	<xsl:template match="c:file">
		<li>{@name}</li>
	</xsl:template>
	
	
	
	<xsl:template match="node() | attribute()">
		<xsl:copy>
			<xsl:apply-templates select="attribute(), node()" />
		</xsl:copy>
	</xsl:template>
	
</xsl:stylesheet>