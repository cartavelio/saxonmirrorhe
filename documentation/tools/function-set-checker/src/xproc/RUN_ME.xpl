<?xml version="1.0" encoding="UTF-8"?>
<p:declare-step     
    xmlns:c="http://www.w3.org/ns/xproc-step"
    xmlns:cx="http://xmlcalabash.com/ns/extensions"
    xmlns:fn="http://www.w3.org/2005/xpath-functions"
    xmlns:m="http://xylarium.org/ns/xml/documentation"
    xmlns:p="http://www.w3.org/ns/xproc"
    xmlns:u="http://saxonica.sf.net/xproc/utils"
    xmlns:xs="http://www.w3.org/2001/XMLSchema"
    version="3.1"
    expand-text="true"
    type="u:function-set-java2xml"
    name="function-set-java2xml">
    
    <p:documentation>
        <m:note>
        	<m:p>Commands for running with XML Calabash 3:</m:p>
        	<m:code-block>
        		<m:code-line>java -jar /path/to/xmlcalabash-app-3.0.44.jar src/xproc/RUN_ME.xpl source-dir=path/to/saxondev/java/src/main/java/ pulled=2026-04-22T13:24:00+01:00</m:code-line>
        	</m:code-block>
        </m:note>
    </p:documentation>
	
    
    <p:documentation>
    	<m:desc>A list of all the function set classes in the Java source code, where they are located and a list of all the function registered within that function set.</m:desc>
    	<m:note>
    		<m:p>This process creates additional files in the <m:code>output-dir</m:code>, including wiki-ready markdown lists of the functions included (or not) in the latest version of the specification associated with each function set.</m:p>
    	</m:note>
    </p:documentation>
    <p:output port="result" sequence="true" />


	<p:documentation>
		<m:desc>When the source data was last pulled from the remote repository.</m:desc>
		<m:note>
			<m:p>To find out when you last pulled that repo, in a terminal, in the root of that repo, execute the command <m:code>ls -lct</m:code> and look at the date and time on the <m:code>java</m:code> directory.  This method isn't 100% reliable but should be good enough in most cases.</m:p>
		</m:note>
	</p:documentation>
    <p:option name="pulled" as="xs:dateTimeStamp?" />
	
	<p:documentation>
		<m:desc>A path to the Java source code, for finding and parsing the FunctionSet class files..</m:desc>
	</p:documentation>
	<p:option name="source-dir" select="resolve-uri('../../../../../java/src/main/java/', static-base-uri())" as="xs:anyURI?" />
	
	<p:documentation>
		<m:desc>A path to where the stored outputs from this process should be created.</m:desc>
	</p:documentation>
    <p:option name="output-dir" select="resolve-uri('../../data/tmp/', static-base-uri())" as="xs:anyURI?" />    

	<p:documentation>
		<m:desc>A path to the root directory for this utility (function-status).</m:desc>
	</p:documentation>
	<p:variable name="project-root" select="resolve-uri('../../', static-base-uri())" as="xs:anyURI" />
	
	<p:variable name="standardised-source-dir" select="resolve-uri(concat($source-dir, if (not(ends-with($source-dir, '/'))) then '/' else ()), $project-root)" as="xs:anyURI" />
	<p:variable name="standardised-output-dir" select="resolve-uri(concat($output-dir, if (not(ends-with($output-dir, '/'))) then '/' else ()), $project-root)" as="xs:anyURI" />
	
	<p:documentation>
		<m:desc>Search for Java function set files in the source code.</m:desc>
	</p:documentation>
    <p:directory-list path="{$standardised-source-dir}" max-depth="unbounded" include-filter=".*FunctionSet.*.java" message="'Searching {$standardised-source-dir}'" />
	
	<p:documentation>
		<m:desc>On every <m:code>c:file</m:code>, set <m:code>@xml:base</m:code> to the absolute path of the directory that file is in.</m:desc>
		<m:note>
			<m:p>This is so that when looping over the <m:code>c:file</m:code> elements, it's possible to find out where that file is located.  Out-of-the-box, <m:code>@xml:base</m:code> unhelpfully only contains the name of the file itself (which is already available in <m:code>@name</m:code>). When looping in XProc, it's often not possible to access the ancestor nodes which are required to build the absolute path.</m:p>
		</m:note>
	</p:documentation>
	<p:xslt>
		<p:with-input port="stylesheet" href="../xslt/fix_xml_base.xsl" />
	</p:xslt>
	
	<p:documentation>
		<m:desc>For each *FunctionSet.java file...</m:desc>
	</p:documentation>
	<p:viewport match="//c:file" name="process-file">
		
		<p:variable name="href" select="concat(/c:file/@xml:base, /c:file/@name)"  as="xs:anyURI" />
		<p:variable name="filename" select="/c:file/@name"  as="xs:string" />
		
		<p:documentation>
			<m:desc>Load the *FunctionSet.java file as a text document.</m:desc>
		</p:documentation>
		<p:load href="{$href}" content-type="text/plain" />
		
		
		<p:documentation>
			<m:desc>Use <m:code>fn:analyze-string</m:code> to mark the name of each function registered.</m:desc>
		</p:documentation>
		<p:identity>
			<p:with-input>
				<p:inline>{fn:analyze-string(string(.), concat('(register|reg1)\(', codepoints-to-string(34), '([^', codepoints-to-string(34), ']*)', codepoints-to-string(34)))}</p:inline>
			</p:with-input>
		</p:identity>
		
		<p:choose>		
			
			<p:documentation>
				<m:desc>If at least one instance of a function being registered was found...</m:desc>
			</p:documentation>
			<p:when test="/fn:analyze-string-result/fn:match/fn:group[@nr = '2']">
						
				<p:documentation>
					<m:desc>Create an XML document listing all the functions registered.</m:desc>
				</p:documentation>
				<p:xslt name="find-registrations">
					<p:with-input port="stylesheet" href="../xslt/function_set_java2xml.xsl" />
					<p:with-option name="parameters" select="map{ 
						'filename' : $filename,
						'location' : substring-after($href, $standardised-source-dir),
						'pulled' : $pulled
						}" />
				</p:xslt>
				
				<p:documentation>
					<m:desc>Store a copy of the XML function set document.</m:desc>
				</p:documentation>
				<p:store href="{$standardised-output-dir}/{substring-before($filename, '.java')}.xml" />
				
				
				<p:documentation>
					<m:desc>Cross-reference the functions against the function catalog associated with this function set.</m:desc>
					<m:note>
						<m:ul>
							<m:ingress><m:code>data/function_set_metadata.xml</m:code> contains the following information per function set:</m:ingress>
							<m:items>
								<m:li>which catalog to use</m:li>
								<m:li>which prefix to use for all functions in that function set</m:li>
								<m:li>the display name for the function set</m:li>
								<m:li>an ID (the Java package and class name)</m:li>
							</m:items>
						</m:ul>
					</m:note>
					<m:note>
						<m:p>The stylesheet includes a hard-coded reference to <m:code>data/function_set_metadata.xml</m:code>.</m:p>
					</m:note>
				</p:documentation>
				<p:xslt name="check-implemented">
					<p:with-input port="stylesheet" href="../xslt/check_implemented.xsl" />
				</p:xslt>
						
				<p:documentation>
					<m:desc>Store a copy of the list of functions registered that are in (or not) the associated function catalog/specification.</m:desc>
				</p:documentation>		
				<p:store href="{$standardised-output-dir}/md/{substring-before($filename, '.java')}.md" />
								
								
				<p:documentation>
					<m:desc>Return a copy of the <m:code>c:file</m:code> element, wrapping a copy of the XML version of that function registry.</m:desc>
				</p:documentation>										
				<p:insert match="/c:file" position="first-child">
					<p:with-input port="insertion" pipe="result@find-registrations" />
					<p:with-input port="source">
						<p:pipe step="process-file" port="current" />
					</p:with-input>
				</p:insert>
				
			</p:when>
			
			<p:documentation>
				<m:desc>If no functions appear to have been registered, return a copy of the <m:code>c:file</m:code> element, unchanged.</m:desc>
			</p:documentation>
			<p:otherwise>
				<p:identity>
					<p:with-input>
						<p:pipe step="process-file" port="current" />
					</p:with-input>
				</p:identity>
			</p:otherwise>
		</p:choose>
		
	</p:viewport>
    
</p:declare-step>