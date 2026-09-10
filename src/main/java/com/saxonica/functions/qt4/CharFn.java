package com.saxonica.functions.qt4;

import net.sf.saxon.Version;
import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.functions.SystemFunction;
import net.sf.saxon.om.Sequence;
import net.sf.saxon.str.*;
import net.sf.saxon.trans.Err;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.value.StringValue;

import java.io.*;
import java.nio.charset.StandardCharsets;
import java.util.ArrayList;
import java.util.HashMap;

/**
 * Implements the fn:char() function.
 * This expands HTML-defined entity references or numeric character references
 */

public class CharFn extends SystemFunction {


    private static HashMap<UnicodeString, UnicodeString> ENTITIES = null;

    /**
     * Evaluate this function call
     *
     * @param context   The XPath dynamic evaluation context
     * @param arguments The values of the arguments to the function call. Each argument value (which is in general
     *                  a sequence) is supplied in the form of a sequence.
     * @return the results of the function.
     * @throws XPathException if a dynamic error occurs during evaluation of the function. The Saxon run-time
     *                        code will add information about the error location.
     */

    @Override
    public Sequence call(XPathContext context, Sequence[] arguments) throws XPathException {
        StringValue name = (StringValue) arguments[0].head();
        UnicodeString uName = name.getUnicodeStringValue();
        if (!uName.isEmpty()) {
            int lead = uName.codePointAt(0);
            if (lead == '\\' && uName.length() == 2) {
                switch (uName.codePointAt(1)) {
                    case 'n':
                        return new StringValue(new UnicodeChar(10));
                    case 't':
                        return new StringValue(new UnicodeChar(9));
                    case 'r':
                        return new StringValue(new UnicodeChar(13));
                    default:
                        return invalid(uName);
                }
            } else if (lead == '#' && uName.length() >= 2) {
                try {
                    int cp;
                    if (uName.codePointAt(1) == 'x') {
                        cp = Integer.parseInt(uName.substring(2).toString(), 16);
                    } else {
                        cp = Integer.parseInt(uName.substring(1).toString(), 10);
                    }
                    if (!context.getConfiguration().getValidCharacterChecker().test(cp)) {
                        throw new XPathException("Character supplied to fn:char is not a valid XML character: " + uName, "FOCH0005");
                    }
                    return new StringValue(new UnicodeChar(cp));
                } catch (NumberFormatException e) {
                    invalid(uName);
                }
            } else {
                return new StringValue(readEntity(uName));
            }
        }
        return invalid(uName);
    }

    public static UnicodeString NewLineName = BMPString.of("NewLine");

    public Sequence invalid(UnicodeString name) throws XPathException {
        throw new XPathException("Invalid argument to fn:char(): " + Err.wrap(name.toString()), "FOCH0005");
    }

    public UnicodeString readEntity(UnicodeString name) throws XPathException {
        synchronized (CharFn.class) {
            if (ENTITIES == null) {
                build();
            }
        }
        UnicodeString value = ENTITIES.get(name);
        if (value == null) {
            invalid(name);
        }
        return value;
    }

    static void build() {
        // Build hash table of entity name-to-value mappings, from the entities.txt file
        // which is generated from https://html.spec.whatwg.org/entities.json using the query
        // below, after manually deleting the entry for "NewLine" which is handled specially.
        /*
              ( let $d :=json-doc('/Users/mike/GitHub/qt4cg/qt4tests/fn/char/entities.json')
                return for $k in Q{http://www.w3.org/2005/xpath-functions/map}keys($d)
                       return (translate(substring($k,2), ';', '') || '=' || ($d($k))?characters))
              => string-join(codepoints-to-string(10))
         */
        ENTITIES = new HashMap<>(2500);
        ENTITIES.put(NewLineName, new UnicodeChar(10));
        InputStream in = Version.platform.locateResource("entities.txt", new ArrayList<>());
        if (in == null) {
            throw new RuntimeException("Unable to read entities.txt file");
        }
        LineNumberReader reader = new LineNumberReader(new InputStreamReader(in, StandardCharsets.UTF_8), 2048);
        String line;
        try {
            while ((line = reader.readLine()) != null) {
                int eq = line.indexOf('=');
                String key = line.substring(0, eq);
                String value = line.substring(eq + 1);
                UnicodeString uValue = StringTool.fromCharSequence(value);
                if (uValue.length() == 1) {
                    // common case
                    uValue = new UnicodeChar(uValue.codePointAt(0));
                }
                ENTITIES.put(new Twine8(key), uValue);
            }
            reader.close();
        } catch (IOException e) {
            throw new RuntimeException("Failed to read entities.txt file", e);
        }

    }
}

// Copyright (c) 2020-2023 Saxonica Limited

