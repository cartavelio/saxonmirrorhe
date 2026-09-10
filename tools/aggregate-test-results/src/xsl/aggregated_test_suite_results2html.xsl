<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet 	
	xmlns:functx="http://www.functx.com"
	xmlns:local="http://saxonica.sf.net/xslt/utils/function"
	xmlns:m="http://xylarium.org/ns/xml/documentation"
	xmlns:math="http://www.w3.org/2005/xpath-functions/math"
	xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
	xmlns:xs="http://www.w3.org/2001/XMLSchema"
	xpath-default-namespace="http://www.w3.org/2012/08/qt-fots-results"
	expand-text="true"
	exclude-result-prefixes="#all"
	version="3.0">
	
	<m:document>
		<m:title>Aggregate Test Results</m:title>
		<m:desc>
			<m:ul>
				<m:ingress>Generates an HTML report that groups and sorts test results by:</m:ingress>
				<m:items>
					<m:li>result category (pass, fail, wrongError, notRun, etc.)</m:li>
					<m:li>reason</m:li>
					<m:li>total cases</m:li>
					<m:li>test set</m:li>
				</m:items>
			</m:ul>
			<m:p>For each test case itemised within a group, the information includes which languages (XPath, XQuery) and editions (EE, PE, HE) are affected.</m:p>
		</m:desc>
		<m:note>
			<m:p>The grouping and sorting varies per result category.</m:p>
		</m:note>
		<m:note>
			<m:p>Result categories other than <m:code>pass</m:code> and <m:code>fail</m:code> are defined in the exceptions file used by the gradle task.</m:p>
		</m:note>
		<m:contributors>
			<m:original-author>Sheila Thomson</m:original-author>
		</m:contributors>
		<m:history created="2026-05-13" />		
	</m:document>
	
	
	<xsl:template match="/">
		<xsl:variable name="doc-title" as="xs:string">
			<xsl:variable name="components" as="xs:string*">Test Results:<xsl:apply-templates select="*" mode="doc-title" /></xsl:variable>
			<xsl:value-of select="string-join($components, ' ')" />
		</xsl:variable>
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
					h2 {{
						margin-top: 2em;
						padding-top: 1em;
						border-top: .5em solid yellow;
					}}
					.summary h2,
					.guidance h2 {{
						margin-top: 1em;
						padding-top: 0;
						border-top: 0;
					}}
					h3 {{
						padding: .25em 1em;
						background-color: rgba(255, 250, 205, 0.5);
					}}
					h3.even {{
						background-color: rgba(255, 250, 205, 0.25);
					}}
					
					sup {{
						font-size: .5em;
					}}
					
					.metadata {{
						margin: 0 0 1.5em 0;
						font-size: .9em;
					}}
					.metadata p {{
						margin: 0;
					}}					
					
					td {{
						padding: 0 1em;
					}}					
					.not-itemised {{
						font-style: italic;
					}}
					span.not-itemised {{
						font-size: smaller;
					}}
					h3 .not-itemised {{
						color: silver;
					}}
					
					.code,
					.language-list span {{
						font-family: monospace;
						background-color: #dcdcdc60;
						padding-inline: .5em;
					}}
					
					.language-list span {{
						border-radius: .5em;
					}}
					.language-list .xp31 {{
						background-color: #4477aa;
						color: #ffffff;
					}}
					.language-list .xp40 {{
						background-color: #56b4e9;
					}}
					.language-list .xq31 {{						
						background-color: #aa3377;
						color: #ffffff;
					}}
					.language-list .xq40 {{
						background-color: #f6a482;
					}}
				</style>
			</head>
			<body>
				<h1>{$doc-title}</h1>
				<div class="metadata">
					<xsl:variable name="test-suite-version" select="distinct-values(*/test-suite-result/submission/test-run/@test-suite-version)" as="xs:string" />
					<p>Test suite version: {$test-suite-version}</p>
					<p>Languages and editions:</p>
					<ul class="language-list">
						<xsl:for-each-group select="*/test-suite-result/product" group-by="@language">
							<xsl:sort select="translate(lower-case(current-grouping-key()), 'abcdefghijklmnopqrstuvwxyz', '')" data-type="number" order="ascending" />
							<xsl:sort select="translate(lower-case(current-grouping-key()), '0123456789', '')" data-type="text" order="ascending" />
							<li>
								<span class="{lower-case(current-grouping-key())}">{current-grouping-key()}</span>
								<xsl:text>: </xsl:text>
								<xsl:call-template name="list-editions"><xsl:with-param name="current-group" select="current-group()" as="node()*" /></xsl:call-template>
							</li>
						</xsl:for-each-group>
					</ul>
				</div>
				<xsl:call-template name="table-of-contents" />
				<section class="guidance">
					<h2>Notes</h2>
					<p>The number in round brackets at the end of a heading is the total number of cases grouped under that heading.</p>
					<p>If more than 20 test cases in the same test file share the same comment, they are not itemised.</p>
					<p>The high-level result categories other than 'Pass' and 'Fail' are defined in the exceptions file that was specified when these tests were run (<span class="code">exception/@report</span>).  The same is true for the reasons cited in all categories except 'Fail' and 'WrongError' (<span class="code">exception/reason</span>).</p>
				</section>
				<xsl:for-each-group select="//test-case" group-by="@result">
					<xsl:sort select="count(current-group())" data-type="number" order="ascending" />
					<xsl:variable name="result-category" select="current-grouping-key()" as="xs:string?" />
					<section>
						<h2 id="{local:to-id(current-grouping-key())}">{functx:capitalize-first(current-grouping-key())} <sup><a href="#toc">^top</a></sup></h2>
						<div class="metadata">
							<p>Total cases: {count(current-group())}</p>
						</div>
						
						<xsl:choose>
							<xsl:when test="current-grouping-key() = 'pass'">
								<p class="not-itemised">(Not itemised)</p>
							</xsl:when>
							<xsl:when test="not(current-grouping-key() = ('bug', 'fail'))">
								<xsl:for-each-group select="current-group()" group-by="@comment">
									<xsl:sort select="count(current-group())" data-type="number" order="descending" />
									<xsl:sort select="current-grouping-key()" data-type="text" order="ascending" />
									<h3>{current-grouping-key()} ({count(current-group())})<xsl:if test="$result-category = ('n/a', 'notRun')"><span class="not-itemised"> - Not itemised</span></xsl:if></h3>
									<xsl:if test="not($result-category = ('n/a', 'notRun'))">
										<ul>
											<xsl:for-each-group select="current-group()" group-by="parent::test-set/@name">
												<xsl:sort select="count(current-group())" data-type="number" order="descending" />
												<xsl:sort select="current-grouping-key()" data-type="text" order="ascending" />
												<li>
													<p>{current-grouping-key()} ({count(current-group())}) <xsl:if test="count(current-group()) > 20"><span class="not-itemised">- Not itemised</span></xsl:if></p>
													<xsl:if test="not(count(current-group()) > 20)">
														<ul>
															<xsl:for-each-group select="current-group()" group-by="@name">
																<li class="name">
																	<xsl:value-of select="current-grouping-key()" />
																	<xsl:call-template name="list-languages">
																		<xsl:with-param name="current-group" select="current-group()" />
																	</xsl:call-template>
																</li>
															</xsl:for-each-group>
														</ul>		
													</xsl:if>
												</li>
											</xsl:for-each-group>
										</ul>
									</xsl:if>
								</xsl:for-each-group>
							</xsl:when>
							<xsl:otherwise>
								<xsl:for-each-group select="current-group()" group-by="parent::test-set/@name">
									<h3>{current-grouping-key()} ({count(current-group())})</h3>
									<table>
										<thead>
											<tr>
												<th class="name">Name</th>
												<th class="comment">Comment</th>
											</tr>
										</thead>
										<tbody>
											<xsl:for-each-group select="current-group()" group-by="@name">
												<tr>
													<td class="name">{current-grouping-key()}</td>
													<td class="comment">
														<xsl:for-each-group select="current-group()" group-by="@comment">
															<xsl:text>{current-grouping-key()}</xsl:text>															
															<xsl:call-template name="list-languages">
																<xsl:with-param name="current-group" select="current-group()" />
															</xsl:call-template>
														</xsl:for-each-group>
													</td>
												</tr>
											</xsl:for-each-group>
										</tbody>								
									</table> 
								</xsl:for-each-group>
							</xsl:otherwise>
						</xsl:choose>						
					</section>
				</xsl:for-each-group>
			</body>
		</html>
		</xsl:result-document>
	</xsl:template>
	
	<xsl:template name="list-languages">
		<xsl:param name="current-group" as="node()*" />
		
		<xsl:text> </xsl:text>
		<span class="language-list">
			<xsl:text>(</xsl:text>
			<xsl:for-each-group select="$current-group" group-by="ancestor::test-suite-result/product/@language">
				<xsl:sort select="translate(lower-case(current-grouping-key()), 'abcdefghijklmnopqrstuvwxyz', '')" data-type="number" order="ascending" />
				<xsl:sort select="translate(lower-case(current-grouping-key()), '0123456789', '')" data-type="text" order="ascending" />
				<span class="{lower-case(current-grouping-key())}">{current-grouping-key()}</span><xsl:text>: </xsl:text>
				<xsl:call-template name="list-editions">
					<xsl:with-param name="current-group" select="current-group()" as="node()*" />
				</xsl:call-template>
				<xsl:text>{if (position() != last()) then '; ' else ()}</xsl:text>
			</xsl:for-each-group>
			<xsl:text>)</xsl:text>
		</span>
	</xsl:template>
	
	
	<xsl:template name="list-editions">
		<xsl:param name="current-group" as="node()*" />	
		
		<xsl:for-each-group select="current-group()" group-by="ancestor::test-suite-result/product/@name/substring-after(., 'Saxon-')">
			<xsl:sort select="current-grouping-key()" data-type="text" order="ascending" />
			<span class="{lower-case(current-grouping-key())}">{current-grouping-key()}</span>
			<xsl:text>{if (position() != last()) then ', ' else ()}</xsl:text>
		</xsl:for-each-group>
	</xsl:template>
	
	

	<xsl:template match="/*" mode="doc-title">
		<xsl:variable name="product-name" select="distinct-values(test-suite-result/product/@name)" as="xs:string*" />
		<xsl:variable name="product-version" select="distinct-values(test-suite-result/product/@version)" as="xs:string" />
		<xsl:variable name="date" select="distinct-values(test-suite-result/submission/test-run/@date-run)" as="xs:string" />
		<xsl:text>{string-join($product-name, ' and ')} {$product-version} ({$date})</xsl:text>
	</xsl:template>
	
	
	<xsl:template name="table-of-contents">
		<xsl:if test="//test-case">
			<section class="summary">
				<h2>Summary</h2>
				<p>High-level result categories:</p>
				<ol id="toc">
					<xsl:for-each-group select="//test-case" group-by="@result">
						<xsl:sort select="count(current-group())" data-type="number" order="ascending" />
						<li><a href="#{local:to-id(current-grouping-key())}">{functx:capitalize-first(current-grouping-key())}</a> ({count(current-group())})</li>
					</xsl:for-each-group>
				</ol>
			</section>
		</xsl:if>
	</xsl:template>
	
	
	<xsl:function name="local:to-id" as="xs:string">
		<xsl:param name="string-in" as="xs:string" />
		
		<xsl:sequence select="translate(lower-case($string-in), ' /:', '-')" />
	</xsl:function>
	
	<xsl:function name="functx:capitalize-first" as="xs:string?">
		<xsl:param name="arg" as="xs:string?"/>
		
		<xsl:sequence select="
				concat(upper-case(substring($arg,1,1)),
				substring($arg,2))
			"/>
		
	</xsl:function>

</xsl:stylesheet>