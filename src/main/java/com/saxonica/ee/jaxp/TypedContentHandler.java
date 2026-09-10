package com.saxonica.ee.jaxp;

import net.sf.saxon.dom.TypeInfoImpl;
import net.sf.saxon.event.ContentHandlerProxy;
import net.sf.saxon.event.ReceiverOption;
import net.sf.saxon.om.AttributeMap;
import net.sf.saxon.om.NamespaceMap;
import net.sf.saxon.om.NodeName;
import net.sf.saxon.s9api.Location;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.type.AnyType;
import net.sf.saxon.type.SchemaType;
import net.sf.saxon.type.SimpleType;
import org.w3c.dom.TypeInfo;

import javax.xml.validation.TypeInfoProvider;

/**
 * This class is an extension of ContentHandlerProxy that provides access to type
 * information, using the DOM Level 3 TypeInfo interfaces.
 * The ContentHandlerProxy also acts as a TypeInfoProvider, providing information
 * about the type of the current element or attribute.
 */

public class TypedContentHandler extends ContentHandlerProxy {
    private SchemaType pendingElementTypeCode = null;
    private AttributeMap pendingAttributes = null;

    /**
     * Get a TypeInfoProvider to provide type information for the current element or attribute
     * event.
     * @return a new TypeInfoProvider
     */

    public TypeInfoProvider getTypeInfoProvider() {
        return new TypeInfoProviderImpl();
    }

    /**
     * Ask whether this Receiver (or the downstream pipeline) makes any use of the type annotations
     * supplied on element and attribute events
     *
     * @return true if the Receiver makes any use of this information. If false, the caller
     *         may supply untyped nodes instead of supplying the type annotation
     */

    @Override
    public boolean usesTypeAnnotations() {
        return true;
    }

    /**
     * Notify the start of an element
     */

    @Override
    public void startElement(NodeName elemName, SchemaType type,
                             AttributeMap attributes, NamespaceMap namespaces,
                             Location location, int properties) throws XPathException {
        pendingElementTypeCode = type;
        pendingAttributes = attributes;
        super.startElement(elemName, type, attributes, namespaces, location, properties);
    }


    ///////////////////////////////////////////////////////////////////////////////////////////
    // TypeInfoProvider
    ///////////////////////////////////////////////////////////////////////////////////////////

    public class TypeInfoProviderImpl extends TypeInfoProvider {

        /**
         * Returns the immutable {@link org.w3c.dom.TypeInfo} object for the current element.
         *
         * @return An immutable {@link org.w3c.dom.TypeInfo} object that represents the
         *         type of the current element.
         *         Note that the caller can keep references to the obtained
         *         {@link org.w3c.dom.TypeInfo} longer than the callback scope.
         *         <p>Otherwise, this method returns null if the validator is unable to
         *         determine the type of the current element for some reason</p>
         */

        @Override
        public TypeInfo getElementTypeInfo() {
            if (pendingElementTypeCode == null) {
                return new TypeInfoImpl(getConfiguration(), AnyType.getInstance());
            } else {
                return new TypeInfoImpl(getConfiguration(), pendingElementTypeCode);
            }
        }

        /**
         * Returns the immutable {@link org.w3c.dom.TypeInfo} object for the specified
         * attribute of the current element.
         * <p>The method may only be called by the startElement event of
         * the {@link org.xml.sax.ContentHandler} that the application sets to the
         * {@link javax.xml.validation.ValidatorHandler}.</p>
         *
         * @param index The index of the attribute. The same index for
         *              the {@link org.xml.sax.Attributes} object passed to the
         *              <tt>startElement</tt> callback.
         * @return An immutable {@link org.w3c.dom.TypeInfo} object that represents the
         *         type of the specified attribute.
         *         Note that the caller can keep references to the obtained
         *         {@link org.w3c.dom.TypeInfo} longer than the callback scope.
         *         <p>Otherwise, this method returns null if the validator is unable to
         *         determine the type.</p>
         * @throws IndexOutOfBoundsException If the index is invalid.
         * @throws IllegalStateException     If this method is called from other {@link org.xml.sax.ContentHandler}
         *                                   methods.
         */
        @Override
        public TypeInfo getAttributeTypeInfo(int index) {
            if (index < 0 || index > pendingAttributes.size()) {
                throw new IndexOutOfBoundsException("" + index);
            }
            SimpleType type = pendingAttributes.itemAt(index).getType();
            return new TypeInfoImpl(getConfiguration(), type);
        }

        /**
         * Returns <tt>true</tt> if the specified attribute is determined
         * to be an ID.
         *
         * @param index The index of the attribute. The same index for
         *              the {@link org.xml.sax.Attributes} object passed to the
         *              <tt>startElement</tt> callback.
         * @return true
         *         if the type of the specified attribute is ID.
         */
        @Override
        public boolean isIdAttribute(int index) {
            return pendingAttributes.itemAt(index).isId();
        }

        /**
         * Returns <tt>false</tt> if the attribute was added by the validator.
         *
         * @param index The index of the attribute. The same index for
         *              the {@link org.xml.sax.Attributes} object passed to the
         *              <tt>startElement</tt> callback.
         * @return <tt>true</tt> if the attribute was present before the validator
         *         processes input. <tt>false</tt> if the attribute was added
         *         by the validator.
         */

        @Override
        public boolean isSpecified(int index) {
            return !ReceiverOption.contains(pendingAttributes.itemAt(index).getProperties(), ReceiverOption.DEFAULTED_VALUE);
        }

    }
}

// Copyright (c) 2004-2022 Saxonica Limited