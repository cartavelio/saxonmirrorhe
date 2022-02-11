<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet version="2.0" 
				xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
				xmlns:saxon="http://saxon.sf.net/"
				xmlns:xs="http://www.w3.org/2001/XMLSchema"
				exclude-result-prefixes="saxon xs">
				
				<!-- test type annotation on untyped nodes -->
	
<xsl:output indent="yes"/>				
				
<xsl:template name="main">
 <xsl:variable name="data">
   <e att="123">abc<xsl:comment>comment</xsl:comment>
    <xsl:processing-instruction name="pi">r squared</xsl:processing-instruction>
   </e>
 </xsl:variable>
<out>
<nodes>
 <doc v="{saxon:type-annotation($data)}"/>
 <el v="{saxon:type-annotation($data/e)}"/>
 <at v="{saxon:type-annotation($data/e/@att)}"/>
 <co v="{saxon:type-annotation($data/e/comment())}"/>
 <pi v="{saxon:type-annotation($data/e/processing-instruction())}"/> 
 <te v="{saxon:type-annotation($data/e/text())}"/>
 <ns v="{saxon:type-annotation($data/e/namespace::xml)}"/>
 <es v="{saxon:type-annotation($data/f)}"/>
</nodes>
<atomics>
 <doc v="{saxon:type-annotation(data($data))}"/>
 <el v="{saxon:type-annotation(data($data/e))}"/>
 <at v="{saxon:type-annotation(data($data/e/@att))}"/>
 <co v="{saxon:type-annotation(data($data/e/comment()))}"/>
 <pi v="{saxon:type-annotation(data($data/e/processing-instruction()))}"/>  
 <te v="{saxon:type-annotation(data($data/e/text()))}"/>
 <ns v="{saxon:type-annotation(data($data/e/namespace::xml))}"/>  
 <es v="{saxon:type-annotation(data($data/f))}"/> 
</atomics>
</out>
</xsl:template>
	
</xsl:stylesheet>
