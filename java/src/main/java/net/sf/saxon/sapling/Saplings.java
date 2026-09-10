////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) 2018-2026 Saxonica Limited
// This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
// If a copy of the MPL was not distributed with this file, You can obtain one at http://mozilla.org/MPL/2.0/.
// This Source Code Form is "Incompatible With Secondary Licenses", as defined by the Mozilla Public License, v. 2.0.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

package net.sf.saxon.sapling;

import net.sf.saxon.s9api.QName;
//#if CSHARP == false
import net.sf.saxon.sapling.dsl.DocumentBuilder;
import net.sf.saxon.sapling.dsl.ElementBuilder;
import net.sf.saxon.transpile.CSharpModifiers;
//#endif

/**
 * This is a non-instantiable class holding a number of convenience methods for creating
 * sapling nodes of different kinds.
 *
 * @see SaplingNode
 */

public class Saplings {

    private Saplings(){}

    /**
     * Create a sapling document node, with no children
     * @return the new node
     */

    @CSharpModifiers(code = {"internal static"})
    public static SaplingDocument doc() {
        return new SaplingDocument();
    }

//#if CSHARP == false
    /**
     * Create a sapling document node, which is passed to the DSL lambda
     * @param definition the DSL definition to build with
     * @return the new node
     */

    public static SaplingDocument doc(net.sf.saxon.sapling.dsl.DocumentDsl definition) {
        SaplingDocument document = new SaplingDocument();
        DocumentBuilder builder = new DocumentBuilder(document);
        definition.buildWith(builder);
        return builder.getResult();
    }
//#endif

    /**
     * Create a sapling document node with a specified base URI, with no children
     * @param baseUri the base URI of the document
     * @return the new node
     */

    @CSharpModifiers(code = {"internal static"})
    public static SaplingDocument doc(String baseUri) {
        return new SaplingDocument(baseUri);
    }

//#if CSHARP == false
    /**
     * Create a sapling document nodewith a specified base URI, which is passed to the DSL lambda
     * @param baseUri the base URI of the document
     * @param definition the DSL definition to build with
     * @return the new node
     */

    public static SaplingDocument doc(String baseUri, net.sf.saxon.sapling.dsl.DocumentDsl definition) {
        SaplingDocument document = new SaplingDocument(baseUri);
        DocumentBuilder builder = new DocumentBuilder(document);
        definition.buildWith(builder);
        return builder.getResult();
    }
//#endif

    /**
     * Create a sapling element node, in no namespace, with no attributes and no children
     * @param name the local name of the element node. This must be a valid NCName, but the
     *             current implementation does not check this.
     * @return the new node
     */

    @CSharpModifiers(code = {"internal static"})
    public static SaplingElement elem(String name) {
        return new SaplingElement(name);
    }

    /**
     * Create a sapling element node, which may or may not be in a namespace, with no attributes and no children
     *
     * @param qName the qualified name of the element. The current implementation does not check that the
     *              parts of the qualified name are syntactically valid.
     * @return the new node
     */

    @CSharpModifiers(code = {"internal static"})
    public static SaplingElement elem(QName qName) {
        return new SaplingElement(qName);
    }

//#ifdef CSHARP == false
    /**
     * Create a sapling element node, in no namespace, which is passed to the DSL lambda
     * @param name the local name of the element node. This must be a valid NCName, but the
     *             current implementation does not check this.
     * @param definition a lambda which will be invoked to allow the creation of child nodes and text content
     *                   for this element
     * @return the new node
     */

    public static SaplingElement elem(String name, net.sf.saxon.sapling.dsl.ElementDsl definition) {
        SaplingElement element =  new SaplingElement(name);
        ElementBuilder builder = new ElementBuilder(element);
        definition.buildWith(builder);
        return builder.getResult();
    }

    /**
     * Create a sapling element node, which may or may not be in a namespace, which is passed to the DSL lambda
     * @param qName the qualified name of the element. The current implementation does not check that the
     *              parts of the qualified name are syntactically valid.
     * @param definition a lambda which will be invoked to allow the creation of child nodes and text content
     *                   for this element
     * @return the new node     */

    public static SaplingElement elem(QName qName, net.sf.saxon.sapling.dsl.ElementDsl definition) {
        SaplingElement element =  new SaplingElement(qName);
        ElementBuilder builder = new ElementBuilder(element);
        definition.buildWith(builder);
        return builder.getResult();
    }
//#endif

    /**
     * Create a sapling text node, with a given string value
     * @param value the string value of the text node
     * @return the new node
     */

    @CSharpModifiers(code = {"internal static"})
    public static SaplingText text(String value) {
        return new SaplingText(value);
    }

    /**
     * Create a sapling comment node, with a given string value
     *
     * @param value the string value of the comment node. This must not contain "--" as a substring,
     *              but the current implementation does not check this.
     * @return the new node
     */

    @CSharpModifiers(code = {"internal static"})
    public static SaplingComment comment(String value) {
        return new SaplingComment(value);
    }

    /**
     * Create a sapling processing instruction node, with a given name and string value
     * @param target the target (name) of the processing instruction. This must be a valid NCName,
     *               but the current implementation does not check this.
     * @param data the string value of the processing instruction node. This must not contain {@code "?>"} as a substring,
     *               but the current implementation does not check this.
     * @return the new node
     */

    @CSharpModifiers(code = {"internal static"})
    public static SaplingProcessingInstruction pi(String target, String data) {
        return new SaplingProcessingInstruction(target, data);
    }
}

