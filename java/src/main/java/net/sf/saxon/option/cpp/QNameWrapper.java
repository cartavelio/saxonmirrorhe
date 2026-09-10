package net.sf.saxon.option.cpp;

import net.sf.saxon.om.NamespaceUri;
import net.sf.saxon.s9api.QName;
import net.sf.saxon.s9api.SaxonApiException;
import net.sf.saxon.s9api.XdmNode;
import org.graalvm.nativeimage.IsolateThread;
import org.graalvm.nativeimage.ObjectHandle;
import org.graalvm.nativeimage.c.function.CEntryPoint;
import org.graalvm.nativeimage.c.type.CCharPointer;
import org.graalvm.nativeimage.c.type.CTypeConversion;
import org.graalvm.word.WordFactory;

public class QNameWrapper extends SaxonCAPI{

    /**
     * Construct a QName using a namespace prefix, a namespace URI, and a local name (in that order).
     * <p>This constructor does not check that the components of the QName are lexically valid.</p>
     *
     * @param thread - Graalvm runtime data structure for the thread
     * @param c_prefix - Reference in the ObjectHandles pool to the prefix of the name. Use either the string "" or null for names that have
     *                  no prefix (that is, they are in the default namespace)
     * @param c_uri - Reference in the ObjectHandles pool to the namespace URI. Use either the string "" or null for names that are not in any namespace.
     * @param c_local - Reference in the ObjectHandles pool to the local part of the name
     * @return long value for the Object Handle of the QName object
     */

    //const char * prefix, const char * uri, const char * localName
    @CEntryPoint(name = "j_getQName")
    public static long getQName(IsolateThread thread, CCharPointer c_prefix, CCharPointer c_uri, CCharPointer c_local, SaxonError saxonError) {
        if(c_local == WordFactory.nullPointer()) {
            SaxonCExceptionHandler.createStandardErrorResult(saxonError, "Invalid QName");
            return SaxonCAPI.STATUS_EXCEPTION;
        }

        String uri = null;
        try {
            if (c_uri != WordFactory.nullPointer()) {
                uri = SaxonCAPI.toJavaString(c_uri, WordFactory.nullPointer());
            }
            String prefix = null;
            if (c_prefix != WordFactory.nullPointer()) {
                prefix = SaxonCAPI.toJavaString(c_prefix, WordFactory.nullPointer());
            }
            String local = SaxonCAPI.toJavaString(c_local, WordFactory.nullPointer());

            QName qname = new QName(uri, prefix, local);

            return SaxonCAPI.createHandle(qname).rawValue();
        } catch (Exception e) {
            SaxonCExceptionHandler.setErrorDetails(e, saxonError, null);
            return SaxonCAPI.STATUS_EXCEPTION;
        }
    }


    /**
     * Construct a QName using a local name.
     * <p>This constructor does not check that the components of the QName are lexically valid.</p>
     *
     * @param thread - Graalvm runtime data structure for the thread
     * @param c_local - Reference in the ObjectHandles pool to the local part of the name
     * @return long value for the Object Handle of the QName object
     */

    //const char * prefix, const char * uri, const char * localName
    @CEntryPoint(name = "j_getQNameFromLocal")
    public static long getQNameFromLocal(IsolateThread thread, CCharPointer c_local, SaxonError saxonError) {
        if(c_local == WordFactory.nullPointer()) {
            SaxonCExceptionHandler.createStandardErrorResult(saxonError, "Invalid QName");
            return SaxonCAPI.STATUS_EXCEPTION;
        }
        try {

            String local = SaxonCAPI.toJavaString(c_local, WordFactory.nullPointer());

            QName qname = new QName(local);

            return SaxonCAPI.createHandle(qname).rawValue();
        } catch (Exception e) {
            SaxonCExceptionHandler.setErrorDetails(e, saxonError, null);
            return SaxonCAPI.STATUS_EXCEPTION;
        }
    }


    /**
     * Construct a QName using a local name.
     * <p>This constructor does not check that the components of the QName are lexically valid.</p>
     *
     * @param thread - Graalvm runtime data structure for the thread
     * @param c_lexical - Reference in the ObjectHandles pool to the local part of the name
     * @return long value for the Object Handle of the QName object
     */

    //const char * prefix, const char * uri, const char * localName
    @CEntryPoint(name = "j_getQNameFromNode")
    public static long getQNameFromNode(IsolateThread thread, CCharPointer c_lexical, ObjectHandle c_node, SaxonError saxonError) {
        if(c_lexical == WordFactory.nullPointer() || c_node == WordFactory.nullPointer()) {
            SaxonCExceptionHandler.createStandardErrorResult(saxonError, "Invalid QName");
            return SaxonCAPI.STATUS_EXCEPTION;
        }

        try {

            String lexical = SaxonCAPI.toJavaString(c_lexical, WordFactory.nullPointer());

            XdmNode node = SaxonCAPI.objectHandles.get(c_node);

            QName qname = new QName(lexical, node);

            return SaxonCAPI.createHandle(qname).rawValue();
        } catch (Exception e) {
            SaxonCExceptionHandler.setErrorDetails(e, saxonError,null);
            return SaxonCAPI.STATUS_EXCEPTION;
        }
    }



    /**
     * Construct a QName using a namespace prefix, a namespace URI, and a local name (in that order).
     * <p>This constructor does not check that the components of the QName are lexically valid.</p>
     *
     * @param thread - Graalvm runtime data structure for the thread
     * @param c_uri - Reference in the ObjectHandles pool to the namespace URI. Use either the string "" or null for names that are not in any namespace.
     * @param c_lexical - Reference in the ObjectHandles pool to either the local part of the name, or the prefix and local part in the format prefix:local
     * @return long value for the Object Handle of the QName object
     */

    //const char * prefix, const char * uri, const char * localName
    @CEntryPoint(name = "j_getQNameFromLexical")
    public static long getQNameFromLexical(IsolateThread thread, CCharPointer c_uri,   CCharPointer c_lexical, SaxonError saxonError) {
        if(c_uri == WordFactory.nullPointer() || c_uri == WordFactory.nullPointer() || c_lexical == WordFactory.nullPointer()) {
            SaxonCExceptionHandler.createStandardErrorResult(saxonError, "Invalid QName");
            return SaxonCAPI.STATUS_EXCEPTION;
        }

        try {

            String uri = SaxonCAPI.toJavaString(c_uri, WordFactory.nullPointer());
            String lexical = SaxonCAPI.toJavaString(c_lexical, WordFactory.nullPointer());

            QName qname = new QName(uri, lexical);

            return SaxonCAPI.createHandle(qname).rawValue();
        } catch (Exception e) {
            SaxonCExceptionHandler.setErrorDetails(e, saxonError, null);
            return SaxonCAPI.STATUS_EXCEPTION;
        }
    }



    @CEntryPoint(name = "j_isQNameEqual")
    public static byte j_isQNameEqual(IsolateThread thread,  ObjectHandle c_qname, ObjectHandle c_otherQName) {
        if(c_qname == WordFactory.nullPointer() || c_otherQName == WordFactory.nullPointer()) {
            return CTypeConversion.toCBoolean(false);
        }

        QName qname = SaxonCAPI.objectHandles.get(c_qname);
        QName otherQName = SaxonCAPI.objectHandles.get(c_otherQName);

        return CTypeConversion.toCBoolean(qname.equals(otherQName));
    }


    /**
     * Factory method to construct a QName from a string containing the expanded
     * QName in Clark notation, that is, <code>{uri}local</code>
     * <p>The prefix part of the <code>QName</code> will be set to an empty string.</p>
     *
     * @param c_clarkname C Char pointer for the string of the URI in Clark notation:
     *                   <code>{uri}local</code> if the
     *                     name is in a namespace, or simply <code>local</code> if not.
     * @return ObjectHandle for the QName corresponding to the supplied name in Clark notation. This will always
     *         have an empty prefix.
     * @throws IllegalArgumentException if the format of the supplied name is incorrect
     */
    @CEntryPoint(name = "j_fromClarkName")
    public static long j_fromClarkName(IsolateThread thread, CCharPointer c_clarkname, SaxonError saxonError) {

        try {
            String clarkname = SaxonCAPI.toJavaString(c_clarkname, WordFactory.nullPointer());
            QName qname = QName.fromClarkName(clarkname);

            return SaxonCAPI.createHandle(qname).rawValue();
        } catch (Exception e) {
            SaxonCExceptionHandler.setErrorDetails(e, saxonError, null);
            return SaxonCAPI.STATUS_EXCEPTION;
        }
    }

    @CEntryPoint(name = "j_fromEQName")
    public static long j_fromEQName(IsolateThread thread,  CCharPointer c_eqname, SaxonError saxonError) {
        if(c_eqname == WordFactory.nullPointer()) {
            SaxonCExceptionHandler.createStandardErrorResult(saxonError, "ClarkName string is null");
            return SaxonCAPI.STATUS_EXCEPTION;
        }

        try {
            String eqname = SaxonCAPI.toJavaString(c_eqname, WordFactory.nullPointer());
            QName qname = QName.fromEQName(eqname);

            return SaxonCAPI.createHandle(qname).rawValue();
        }  catch (Exception e) {
            SaxonCExceptionHandler.setErrorDetails(e, saxonError, null);
            return SaxonCAPI.STATUS_EXCEPTION;
        }
    }

    @CEntryPoint(name = "j_isQNameValid")
    public static byte j_isQNameValid(IsolateThread thread, ObjectHandle c_qname, SaxonError saxonError)  {
        if(c_qname == WordFactory.nullPointer()) {
            SaxonCExceptionHandler.createStandardErrorResult(saxonError, "QName string is null");
            return (byte)SaxonCAPI.STATUS_EXCEPTION;
        }

        QName qname = SaxonCAPI.objectHandles.get(c_qname);
        return CTypeConversion.toCBoolean(qname.isValid(null));
    }

    @CEntryPoint(name = "j_getPrefixFromQName")
    public static CCharPointer j_getPrefixFromQName(IsolateThread thread, SaxonCAPI.AllocatorCCharFn alloc, ObjectHandle c_qname) {


        QName qname = SaxonCAPI.objectHandles.get(c_qname);
        String prefix = qname.getPrefix();

        return SaxonCAPI.stringToNative2(alloc, prefix);
    }

    @CEntryPoint(name = "j_getNamespaceFromQName")
    public static CCharPointer j_getNamespaceFromQName(IsolateThread thread, SaxonCAPI.AllocatorCCharFn alloc, ObjectHandle c_qname) {

        QName qname = SaxonCAPI.objectHandles.get(c_qname);
        NamespaceUri namespacei = qname.getNamespaceUri();
        if(namespacei == null) {
            return WordFactory.nullPointer();
        }

        return SaxonCAPI.stringToNative2(alloc, namespacei.toString());
    }

    @CEntryPoint(name = "j_getNamespaceUriFromQName")
    public static CCharPointer j_getNamespaceUriFromQName(IsolateThread thread, SaxonCAPI.AllocatorCCharFn alloc, ObjectHandle c_qname) {
        if(c_qname == WordFactory.nullPointer()) {
            return WordFactory.nullPointer();
        }

        QName qname = SaxonCAPI.objectHandles.get(c_qname);
        NamespaceUri namespacei = qname.getNamespaceUri();
        if(namespacei == null) {
            return WordFactory.nullPointer();
        }

        return SaxonCAPI.stringToNative2(alloc, namespacei.toString());
    }

    @CEntryPoint(name = "j_getLocalNameFromQName")
    public static CCharPointer j_getLocalNameFromQName(IsolateThread thread, SaxonCAPI.AllocatorCCharFn alloc, ObjectHandle c_qname) {
        if(c_qname == WordFactory.nullPointer()) {
            return WordFactory.nullPointer();
        }

        QName qname = SaxonCAPI.objectHandles.get(c_qname);
        String localName = qname.getLocalName();

        return SaxonCAPI.stringToNative2(alloc, localName);
    }

    @CEntryPoint(name = "j_getClarkNameFromQName")
    public static CCharPointer j_getClarkNameFromQName(IsolateThread thread, SaxonCAPI.AllocatorCCharFn alloc, ObjectHandle c_qname) {
        if(c_qname == WordFactory.nullPointer()) {
            return WordFactory.nullPointer();
        }

        QName qname = SaxonCAPI.objectHandles.get(c_qname);
        String clarkName = qname.getClarkName();

        return SaxonCAPI.stringToNative2(alloc, clarkName);
    }

    @CEntryPoint(name = "j_getEQNameFromQName")
    public static CCharPointer j_getEQNameFromQName(IsolateThread thread, SaxonCAPI.AllocatorCCharFn alloc, ObjectHandle c_qname) {
        if(c_qname == WordFactory.nullPointer()) {
            return WordFactory.nullPointer();
        }

        QName qname = SaxonCAPI.objectHandles.get(c_qname);
        String eqName = qname.getEQName();

        return SaxonCAPI.stringToNative2(alloc, eqName);
    }

    @CEntryPoint(name = "j_toStringQName")
    public static CCharPointer j_tostringFromQName(IsolateThread thread, SaxonCAPI.AllocatorCCharFn alloc, ObjectHandle c_qname) {
        QName qname = SaxonCAPI.objectHandles.get(c_qname);
        String qnameStr = qname.toString();

        return SaxonCAPI.stringToNative2(alloc, qnameStr);
    }


}
