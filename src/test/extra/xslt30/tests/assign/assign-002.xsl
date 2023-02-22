<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet version="3.0"
 xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
 xmlns:xs="http://www.w3.org/2001/XMLSchema"
 xmlns:saxon="http://saxon.sf.net/"
 extension-element-prefixes="saxon"
 exclude-result-prefixes="xs saxon"
>
<xsl:output indent="yes"/>

<!-- Global variable -->
<xsl:variable name="cIndentMin" as="xs:decimal" select="3.0"/>
<xsl:variable name="debug" as="xs:boolean" select="true()"/>
<xsl:variable name="exit"  as="xs:boolean" select="false()" saxon:assignable="yes"/>


<xsl:template match="doc">
    <xsl:copy>
        <xsl:copy-of select="@*"/>
        <xsl:apply-templates select="child::*[1]"/>
    </xsl:copy>
</xsl:template>

<!--
	function:  Normal para template. 
	param   :  See below
               prmIdTo: Id value that limits applying templates to the next element.
               prmListType: 'listordered', 'listitemized'
               prmIndent: Left indent of the prceeding element.
	return  :  Generate "listordered", "listitemized" or "listitem" element.
	note    :
-->
<xsl:template match="para[@type='']">
    <xsl:param name="prmIdTo"     as="xs:string"  select="''"/>
    <xsl:param name="prmListType" as="xs:string"  select="''"/>
    <xsl:param name="prmIndent"   as="xs:decimal" select="0"/>

    <xsl:variable name="currentId" select="generate-id()"/>
    <xsl:variable name="elementName" select="name()"/>
    
    <!-- Process this 'para' and child nodes . -->
    <xsl:element name="{$elementName}">
        <xsl:if test="$debug">
            <xsl:attribute name="id">
                <xsl:value-of select="$currentId"/>
            </xsl:attribute>
        </xsl:if>
        <xsl:apply-templates/>
    </xsl:element>
    <xsl:if test="($prmIdTo ne $currentId)">
        <!-- Process flollowing-sibling node -->
        <xsl:apply-templates select="following-sibling::*[1]">
            <xsl:with-param name="prmIdTo" select="$prmIdTo"/>
            <xsl:with-param name="prmListType" select="$prmListType"/>
            <xsl:with-param name="prmIndent" select="$prmIndent"/>
        </xsl:apply-templates>
    </xsl:if>
</xsl:template>

<!--
	function:  Construct "listordered", "listitemized" element. 
	param   :  See below
               prmIdTo: Id value that limits applying templates to the next element.
               prmListType: 'listordered', 'listitemized'
               prmIndent: Left indent of the prceeding element.
	return  :  Generate "listordered", "listitemized" or "listitem" element.
	note    :
-->
<xsl:template match="para[(@type='listordered') or (@type='listitemized')]">
    <xsl:param name="prmIdTo"     as="xs:string"  select="''"/>
    <xsl:param name="prmListType" as="xs:string"  select="''"/>
    <xsl:param name="prmIndent"   as="xs:decimal" select="0"/>
    
    <xsl:variable name="currentListType" as="xs:string" select="string(@type)"/>
    <xsl:variable name="currentId" as="xs:string" select="generate-id()"/>
    <xsl:variable name="currentIndent" as="xs:decimal" select="xs:decimal(@leftIndent)"/>
    
    <!-- Is new list begins from this para? -->
    <xsl:variable name="isNewList" as="xs:boolean">
        <xsl:call-template name="isNewList">
        	<xsl:with-param name="prmPreviousListType" select="$prmListType"/>
        	<xsl:with-param name="prmPreviousIndent"   select="$prmIndent"/>
        	<xsl:with-param name="prmCurrentListType"  select="$currentListType"/>
        	<xsl:with-param name="prmCurrentIndent"    select="$currentIndent"/>
        </xsl:call-template>
    </xsl:variable>
    
    <!-- The range of the child of listitem (to next list item) -->
    <xsl:variable name="listitemRange" as="xs:string+">
        <xsl:choose>
            <xsl:when test="following-sibling::*[1]">
                <xsl:for-each select="following-sibling::*[1]">
                    <xsl:iterate select="following-sibling::*">
                        <xsl:param name="prmPreviousId" as="xs:string" select="$currentId"/>
                        <xsl:param name="prmPreviousIndent" as="xs:decimal" select="$currentIndent"/>
                        <xsl:param name="prmPreviousListType" as="xs:string" select="$currentListType"/>
                        
                        <xsl:variable name="currentId" select="generate-id()"/>
                        <xsl:variable name="currentListType" as="xs:string" select="string(@type)"/>
                        <xsl:variable name="currentIndent" as="xs:decimal" select="xs:decimal(@leftIndent)"/>
                        
                        <xsl:variable name="previousIndentMin" as="xs:decimal" select="$prmPreviousIndent - $cIndentMin"/>
                        <xsl:variable name="previousIndentMax" as="xs:decimal" select="$prmPreviousIndent + $cIndentMin"/>
                        <saxon:assign name="exit" select="false()"/>
                        <xsl:try>
                            <xsl:choose>
                                <xsl:when test="(string($currentListType)) and ($currentListType ne $prmPreviousListType)">
                                    <!-- Same level different type list -->
                                    <xsl:choose>
                                        <xsl:when test="($previousIndentMin lt $currentIndent) and ($currentIndent lt $previousIndentMax)">
                                            <!-- this list will be ended here! (We don't need next-id) -->
                                            <xsl:sequence select="$prmPreviousId"/>
                                            <xsl:value-of select="error(xs:QName('dummy'))"/>
                                        </xsl:when>
                                        <!-- Low level (more indented) different type list -->
                                        <xsl:when test="$previousIndentMax le $currentIndent">
                                            <xsl:choose>
                                                <xsl:when test="following-sibling::*"/>
                                                <xsl:otherwise>
                                                    <xsl:sequence select="$currentId"/>
                                                    <xsl:value-of select="error(xs:QName('dummy'))"/>
                                                </xsl:otherwise>
                                            </xsl:choose>
                                        </xsl:when>
                                        <!-- Higher level different type list -->
                                        <xsl:otherwise>
                                            <xsl:sequence select="$prmPreviousId"/>
                                            <xsl:value-of select="error(xs:QName('dummy'))"/>
                                        </xsl:otherwise>
                                    </xsl:choose>
                                </xsl:when>
                    			<!-- Meet the same type list -->
                                <xsl:when test="(string($currentListType)) and ($currentListType eq  $prmPreviousListType)">
                                    <xsl:choose>
                                        <!-- Same level list -->
                                        <xsl:when test="($previousIndentMin lt $currentIndent) and ($currentIndent lt $previousIndentMax)">
                                            <xsl:sequence select="$prmPreviousId"/>
                                            <xsl:sequence select="$currentId"/>
                                            <xsl:value-of select="error(xs:QName('dummy'))"/>
                                        </xsl:when>
                                        <!-- Lower level list (More indented) -->
                                        <xsl:when test="$previousIndentMax le $currentIndent">
                                            <xsl:choose>
                                                <xsl:when test="following-sibling::*">
                                                </xsl:when>
                                                <xsl:otherwise>
                                                    <xsl:sequence select="$currentId"/>
                                                    <xsl:value-of select="error(xs:QName('dummy'))"/>
                                                </xsl:otherwise>
                                            </xsl:choose>
                                        </xsl:when>
                                        <!-- Higher level list -->
                                        <xsl:otherwise>
                                            <xsl:sequence select="$prmPreviousId"/>
                                            <xsl:value-of select="error(xs:QName('dummy'))"/>
                                        </xsl:otherwise>
                                    </xsl:choose>
                                </xsl:when>
                                <!-- Non list para -->
                                <xsl:otherwise>
                                    <xsl:choose>
                                        <!-- Same indent level element -->
                                        <xsl:when test="($previousIndentMin lt $currentIndent) and ($currentIndent lt $previousIndentMax)">
                                            <xsl:sequence select="$prmPreviousId"/>
                                            <xsl:value-of select="error(xs:QName('dummy'))"/>
                                        </xsl:when>
                                        <!-- More indented (nested) element -->
                                        <xsl:when test="$previousIndentMax le $currentIndent">
                                            <xsl:choose>
                                                <xsl:when test="following-sibling::*">
                                                </xsl:when>
                                                <xsl:otherwise>
                                                    <xsl:sequence select="$currentId"/>
                                                    <xsl:value-of select="error(xs:QName('dummy'))"/>
                                                </xsl:otherwise>
                                            </xsl:choose>
                                        </xsl:when>
                                        <!-- High level (more outdented) element -->
                                        <xsl:otherwise>
                                            <xsl:sequence select="$prmPreviousId"/>
                                            <xsl:value-of select="error(xs:QName('dummy'))"/>
                                        </xsl:otherwise>
                                    </xsl:choose>
                                </xsl:otherwise>
                            </xsl:choose>
                            <xsl:catch errors="dummy">
                                <saxon:assign name="exit" select="true()" />
                            </xsl:catch>
                        </xsl:try>
                        <xsl:choose>
                            <xsl:when test="not($exit)">
                                <xsl:next-iteration>
                                    <xsl:with-param name="prmPreviousId" select="$currentId"/>
                                    <xsl:with-param name="prmPreviousIndent" select="$prmPreviousIndent"/>
                                    <xsl:with-param name="prmPreviousListType" select="$prmPreviousListType" />
                                </xsl:next-iteration>
                            </xsl:when>
                            <xsl:otherwise>
                                <xsl:break/>
                            </xsl:otherwise>
                        </xsl:choose>
                    </xsl:iterate>
                </xsl:for-each>    
            </xsl:when>
            <xsl:otherwise>
                <xsl:sequence select="$currentId"/>
            </xsl:otherwise>
        </xsl:choose>
    </xsl:variable>
    
    <!-- Result of range -->
    <xsl:variable name="idToListitem" as="xs:string" select="$listitemRange[1]"/>
    <xsl:variable name="idNextListitem" as="xs:string" select="if (empty($listitemRange[2])) then '' else $listitemRange[2]"/>

    <!-- xsl:message>idToListitem=<xsl:value-of select="$idToListitem"/></xsl:message>
    <xsl:message>idNextListitem=<xsl:value-of select="$idNextListitem"/></xsl:message -->

    <xsl:choose>
        <!-- If $isNewList is false(), there is already a 'listordered', 'listitemized'. -->
        <xsl:when test="not($isNewList)">
            <xsl:element name="listitem">
                <xsl:if test="$debug">
                    <xsl:attribute name="id">
                        <xsl:value-of select="$currentId"/>
                    </xsl:attribute>
                    <xsl:attribute name="id-to">
                        <xsl:value-of select="$idToListitem"/>
                    </xsl:attribute>
                    <xsl:attribute name="id-next">
                        <xsl:value-of select="$idNextListitem"/>
                    </xsl:attribute>
                </xsl:if>
                <xsl:apply-templates/>

                <xsl:if test="string($idToListitem) and ($idToListitem ne $currentId)">
                    <!-- Process lower level elements of 'listitem' -->
                    <xsl:apply-templates select="following-sibling::*[1]">
                        <xsl:with-param name="prmIdTo" select="$idToListitem"/>
                        <xsl:with-param name="prmListType" select="$currentListType"/>
                        <xsl:with-param name="prmIndent" select="$currentIndent"/>
                    </xsl:apply-templates>
                </xsl:if>
            </xsl:element>
            <!-- Next element will be same level 'listitem' -->
            <xsl:if test="string($idNextListitem)">
                <xsl:apply-templates select="following-sibling::*[generate-id() = $idNextListitem]">
                    <xsl:with-param name="prmIdTo" select="$prmIdTo"/>
                    <xsl:with-param name="prmListType" select="$prmListType"/>
                    <xsl:with-param name="prmIndent" select="$prmIndent"/>
                </xsl:apply-templates>
            </xsl:if>
        </xsl:when>
        
        <xsl:otherwise>
            <!-- If $isNewList is true(), the new 'listordered' or 'listitemized' starts from here. -->
            <!-- Range of "listordered"  -->
            <xsl:variable name="listRange" as="xs:string+">
                <xsl:choose>
                    <xsl:when test="following-sibling::*[1]">
                        <xsl:for-each select="following-sibling::*[1]">
                            <xsl:iterate select="following-sibling::*">
                                <xsl:param name="prmPreviousId" as="xs:string" select="$currentId"/>
                                <xsl:param name="prmPreviousIndent" as="xs:decimal" select="$currentIndent"/>
                                <xsl:param name="prmPreviousListType" as="xs:string" select="$currentListType"/>
                                
                                <xsl:variable name="currentId" select="generate-id()"/>
                                <xsl:variable name="currentListType" as="xs:string" select="string(@type)"/>
                                <xsl:variable name="currentIndent" as="xs:decimal" select="xs:decimal(@leftIndent)"/>
                                
                                <xsl:variable name="previousIndentMin" as="xs:decimal" select="$prmPreviousIndent - $cIndentMin"/>
                                <xsl:variable name="previousIndentMax" as="xs:decimal" select="$prmPreviousIndent + $cIndentMin"/>
                                <saxon:assign name="exit" select="false()"/>
                                <xsl:try>
                                    <xsl:choose>
                                        <!-- Meet the different type list -->
                                        <xsl:when test="(string($currentListType)) and ($currentListType ne $prmPreviousListType)">
                                            <xsl:choose>
                                                <!-- Same level different type list -->
                                                <xsl:when test="($previousIndentMin lt $currentIndent) and ($currentIndent lt $previousIndentMax)">
                                                    <xsl:sequence select="$prmPreviousId"/>
                                                    <xsl:sequence select="$currentId"/>
                                                    <xsl:value-of select="error(xs:QName('dummy'))"/>
                                                </xsl:when>
                                                <!-- Low level (more indented) diffrent type list -->
                                                <xsl:when test="$previousIndentMax le $currentIndent">
                                                    <xsl:choose>
                                                        <xsl:when test="following-sibling::*">
                                                        </xsl:when>
                                                        <xsl:otherwise>
                                                            <xsl:sequence select="$currentId"/>
                                                            <xsl:value-of select="error(xs:QName('dummy'))"/>
                                                        </xsl:otherwise>
                                                    </xsl:choose>
                                                </xsl:when>
                                                <!-- Higher level different type list -->
                                                <xsl:otherwise>
                                                    <xsl:sequence select="$prmPreviousId"/>
                                                    <xsl:value-of select="error(xs:QName('dummy'))"/>
                                                </xsl:otherwise>
                                            </xsl:choose>
                                        </xsl:when>
                            			<!-- meet the same type list -->
                                        <xsl:when test="(string($currentListType)) and ($currentListType eq  $prmPreviousListType)">
                                            <xsl:choose>
                                                <!-- Same level list -->
                                                <xsl:when test="($previousIndentMin lt $currentIndent) and ($currentIndent lt $previousIndentMax)">
                                                </xsl:when>
                                                <!-- Lower level list (More indented) -->
                                                <xsl:when test="$previousIndentMax &lt;= $currentIndent">
                                                    <xsl:choose>
                                                        <xsl:when test="following-sibling::*">
                                                        </xsl:when>
                                                        <xsl:otherwise>
                                                            <xsl:sequence select="$currentId"/>
                                                            <xsl:value-of select="error(xs:QName('dummy'))"/>
                                                        </xsl:otherwise>
                                                    </xsl:choose>
                                                </xsl:when>
                                                <!-- Higher level list -->
                                                <xsl:otherwise>
                                                    <xsl:sequence select="$prmPreviousId"/>
                                                    <xsl:value-of select="error(xs:QName('dummy'))"/>
                                                </xsl:otherwise>
                                            </xsl:choose>
                                        </xsl:when>
                                        <!-- Non list para -->
                                        <xsl:otherwise>
                                            <xsl:choose>
                                                <!-- Same level indented element -->
                                                <xsl:when test="($previousIndentMin lt $currentIndent) and ($currentIndent lt  $previousIndentMax)">
                                                    <xsl:sequence select="$prmPreviousId"/>
                                                    <xsl:sequence select="$currentId"/>
                                                    <xsl:value-of select="error(xs:QName('dummy'))"/>
                                                </xsl:when>
                                                <!-- More indented element -->
                                                <xsl:when test="($previousIndentMax le $currentIndent)">
                                                    <xsl:choose>
                                                        <xsl:when test="following-sibling::*">
                                                        </xsl:when>
                                                        <xsl:otherwise>
                                                            <xsl:sequence select="$currentId"/>
                                                            <xsl:value-of select="error(xs:QName('dummy'))"/>
                                                        </xsl:otherwise>
                                                    </xsl:choose>
                                                </xsl:when>
                                                <!-- High level element. (More outdented element) -->
                                                <xsl:otherwise>
                                                    <xsl:sequence select="$prmPreviousId"/>
                                                    <xsl:value-of select="error(xs:QName('dummy'))"/>
                                                </xsl:otherwise>
                                            </xsl:choose>
                                        </xsl:otherwise>
                                    </xsl:choose>
                                    <xsl:catch errors="dummy">
                                        <saxon:assign name="exit" select="true()" />
                                    </xsl:catch>
                                </xsl:try>
                                <xsl:choose>
                                    <xsl:when test="not($exit)">
                                        <xsl:next-iteration>
                                            <xsl:with-param name="prmPreviousId" select="$currentId"/>
                                            <xsl:with-param name="prmPreviousIndent" select="$currentIndent"/>
                                            <xsl:with-param name="prmPreviousListType" select="$currentListType"/>
                                        </xsl:next-iteration>
                                    </xsl:when>
                                    <xsl:otherwise>
                                        <xsl:break/>
                                    </xsl:otherwise>
                                </xsl:choose>
                            </xsl:iterate>
                        </xsl:for-each>
                    </xsl:when>
                    <xsl:otherwise>
                        <xsl:sequence select="$currentId"/>
                    </xsl:otherwise>
                </xsl:choose>
            </xsl:variable>
            
            <!-- Result of range -->
            <xsl:variable name="idToList" select="$listRange[1]"/>
            <xsl:variable name="idNextList" select="if (empty($listRange[2])) then '' else $listRange[2]"/>
            
            <xsl:element name="{$currentListType}">
                <xsl:if test="$debug">
                    <xsl:attribute name="id-to">
                        <xsl:value-of select="$idToList"/>
                    </xsl:attribute>
                    <xsl:attribute name="id-next">
                        <xsl:value-of select="$idNextList"/>
                    </xsl:attribute>
                </xsl:if>
                <xsl:element name="listitem">
                    <xsl:if test="$debug">
                        <xsl:attribute name="id">
                            <xsl:value-of select="$currentId"/>
                        </xsl:attribute>
                        <xsl:attribute name="id-to">
                            <xsl:value-of select="$idToListitem"/>
                        </xsl:attribute>
                        <xsl:attribute name="id-next">
                            <xsl:value-of select="$idNextListitem"/>
                        </xsl:attribute>
                    </xsl:if>
                    
                    <xsl:apply-templates/>
                    
                    <xsl:if test="string($idToListitem) and ($idToListitem ne $currentId)">
                        <!-- Process lower level elements of 'listitem' -->
                        <xsl:apply-templates select="following-sibling::*[1]">
                            <xsl:with-param name="prmIdTo" select="$idToListitem"/>
                            <xsl:with-param name="prmListType" select="$currentListType"/>
                            <xsl:with-param name="prmIndent" select="$currentIndent"/>
                        </xsl:apply-templates>
                    </xsl:if>
                </xsl:element>
                
                <xsl:if test="string($idNextListitem)">
                    <xsl:apply-templates select="following-sibling::*[generate-id() = $idNextListitem]">
                        <xsl:with-param name="prmIdTo" select="$prmIdTo"/>
                        <xsl:with-param name="prmListType" select="$currentListType"/>
                        <xsl:with-param name="prmIndent" select="$currentIndent"/>
                    </xsl:apply-templates>
                </xsl:if>
            </xsl:element>
            
            <!-- xsl:message>$idNextList=<xsl:value-of select="$idNextList"/></xsl:message>
            <xsl:message>$idToList=<xsl:value-of select="$idToList"/></xsl:message>
            <xsl:message>$prmIdTo=<xsl:value-of select="$prmIdTo"/></xsl:message>
            <xsl:message>========================================================</xsl:message -->
            
            <!-- Process the next element of 'listordered', 'listitemized' -->
            <xsl:choose>
                <!-- Normal pattern -->
                <xsl:when test="string($idNextList)">
                    <xsl:apply-templates select="following-sibling::*[generate-id()=$idNextList]">
                        <xsl:with-param name="prmIdTo" select="$prmIdTo"/>
                        <xsl:with-param name="prmListType" select="$prmListType"/>
                        <xsl:with-param name="prmIndent" select="$prmIndent"/>
                    </xsl:apply-templates>
                </xsl:when>
                <!-- Abnormal indentation pattern -->
                <xsl:when test="(string($prmIdTo)) and (string($idToList) ne string($prmIdTo))">
                    <xsl:variable name="nextId">
                        <xsl:call-template name="getNextElement">
                            <xsl:with-param name="prmIdTo" select="$idToList"/>
                        </xsl:call-template>
                    </xsl:variable>
                    <!-- xsl:message>$nextId=<xsl:value-of select="$nextId"/></xsl:message>
                    <xsl:message>========================================================</xsl:message -->
                    <xsl:if test="string($nextId)">
                        <xsl:apply-templates select="following-sibling::*[generate-id()=$nextId]">
                            <xsl:with-param name="prmIdTo" select="$prmIdTo"/>
                            <xsl:with-param name="prmListType" select="$prmListType"/>
                            <xsl:with-param name="prmIndent" select="$prmIndent"/>
                        </xsl:apply-templates>
                    </xsl:if>
                </xsl:when>
            </xsl:choose>
        </xsl:otherwise>
    </xsl:choose>
</xsl:template>

<!--
	function :  Decide new list or not
	param    :  New and old list type, indent value
	return   :  xs:boolean
	note     :
-->
<xsl:template name="isNewList" as="xs:boolean">
  	<xsl:param name="prmPreviousListType" as="xs:string"/>
  	<xsl:param name="prmPreviousIndent"   as="xs:decimal"/>
  	<xsl:param name="prmCurrentListType"  as="xs:string"/>
  	<xsl:param name="prmCurrentIndent"    as="xs:decimal"/>
  	<xsl:choose>
        <!-- Different list type -->
        <xsl:when test="$prmPreviousListType != $prmCurrentListType">
            <xsl:sequence select="true()"/>
        </xsl:when>
        <!-- Same list type -->
        <xsl:otherwise>
            <xsl:variable name="previousIndentMin" as="xs:decimal" select="$prmPreviousIndent - $cIndentMin"/>
            <xsl:variable name="previousIndentMax" as="xs:decimal" select="$prmPreviousIndent + $cIndentMin"/>
            <xsl:choose>
                <xsl:when test="($previousIndentMin lt  $prmCurrentIndent) and ($prmCurrentIndent lt $previousIndentMax)">
                    <xsl:sequence select="false()"/>
                </xsl:when>
                <xsl:otherwise>
                    <xsl:sequence select="true()"/>
                </xsl:otherwise>
            </xsl:choose>
        </xsl:otherwise>
    </xsl:choose>
</xsl:template>

<!--
	function :  Get next element of redundantly nested list. 
	param    :  Last element id.
	return   :  Next element id.
	note     :  
-->
<xsl:template name="getNextElement" as="xs:string?">
    <xsl:param name="prmIdTo"/>
    <xsl:variable name="idToNode" select="//*[generate-id()=$prmIdTo]"/>
    <xsl:sequence select="generate-id($idToNode/following-sibling::*[1])"/>
</xsl:template>

</xsl:stylesheet>
