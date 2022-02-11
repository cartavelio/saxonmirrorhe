<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
  xmlns:xs="http://www.w3.org/2001/XMLSchema"
  xmlns:math="http://www.w3.org/2005/xpath-functions/math"
  xmlns:map="http://www.w3.org/2005/xpath-functions/map" xmlns:xp="http://saxonica.com/xpathParse"
  xmlns:fos="http://www.w3.org/xpath-functions/spec/namespace"
  xmlns:e="http://www.w3.org/1999/XSL/Spec/ElementSyntax" xmlns:f="myfunctions"
  xmlns:doc="http://www.saxonica.com/ns/documentation" xmlns:dc="http://purl.org/dc/elements/1.1/"
  xmlns:X="IndirectXSLT" xmlns:s="StreamAnalysis" exclude-result-prefixes="xs math fos"
  version="3.0">
  <xsl:variable name="stream-controls" as="element()">
    <controls>
      <set label="type">
        <check type="staticType" label="static" abbrev="t"/>
        <check type="contextItemType" label="context" abbrev="ct"/>
      </set>
      <check type="role" label="role" abbrev="r"/>
      <check type="focus" label="focus" abbrev="f"/>
      <set label="usage">
        <check type="usage" label="adjusted" abbrev="U'"/>
        <check type="usageOriginal" label="original" abbrev="u"/>
      </set>
      <set label="posture">
        <check type="posture" label="final" abbrev="p"/>
        <check type="contextPosture" label="context" abbrev="cp"/>
      </set>
      <set label="sweep">
        <check type="sweep" label="adjusted" abbrev="S'"/>
        <check type="sweepOriginal" label="original" abbrev="s"/>
      </set>
      <set label="General Streamability Rules">
        <check type="general" label="Applied" abbrev="rule"/>
        <check type="generalOperand" label="Operand" abbrev="rule.op"/>
        <check type="potentialConsumer" label="Potential Consumer" abbrev="PC"/>
        <check type="choiceGroup" label="Choice Group" abbrev="CG"/>
      </set>
    </controls>
  </xsl:variable>
  <xsl:template match="controls" mode="make-controls"> Show <div>
      <xsl:apply-templates mode="#current"/>
    </div>
  </xsl:template>
  <xsl:template match="check" mode="make-controls" expand-text="yes">
    <div>
      <input class="showType" type="checkbox" name="{@type}" value="{@type}"/>{@label}</div>
  </xsl:template>
  <xsl:template match="set" mode="make-controls" expand-text="yes">
    <div class="set">
      <span class="label">{@label}</span>
      <xsl:apply-templates mode="#current"/>
    </div>
  </xsl:template>
</xsl:stylesheet>