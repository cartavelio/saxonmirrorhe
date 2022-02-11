<?xml version="1.0" encoding="iso-8859-1"?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" 
version="2.0">

<!-- Test rule-based collation URI -->

   <xsl:output method="xml" indent="yes" omit-xml-declaration="no"/>
   
   
   <xsl:variable name="rules"><![CDATA[
     ' ' ,  ':' ,  ';' ,  '<'  , '=' ,  '>'  , '?' ,  '@', '!',
 '[' ,  '\' ,  ']' ,  '^' ,  '_' ,  '`',
 '{' ,  '|' ,  '}' ,  '~'
 '!' ,  '"' ,  '#' ,  '$' ,  '%' ,  '&',  ''' ,  '(' ,  ')' ,  '*'  , '+'  , ','  , '-' , '.' ,  '/'

< A,a  < B,b  < C,c  < D,d  < E,e  < F,f  < G,g
< H,h  < I,i  < J,j  < K,k  < L,l < N,n   < M,m 
< O,o  < P,p  < Q,q  < R,r  < S,s  < T,t
< U,u < ü  < V,v  < W,w  < X,x  < Y,y  < Z,z

< 0  < 1  < 2  < 3  < 4  < 5  < 6  < 7  < 8  < 9
]]></xsl:variable>

   <xsl:template name="main">
   <out>
       <xsl:for-each select="('amy', 'annie', 'Annie', 'charles', 'mary', 'nathan', 'Mary', 'stephen')">
         <xsl:sort select="." collation="http://saxon.sf.net/collation?rules={encode-for-uri($rules)}"/>
         <a><xsl:value-of select="."/></a>
       </xsl:for-each>
   </out>
   </xsl:template>


</xsl:stylesheet>

