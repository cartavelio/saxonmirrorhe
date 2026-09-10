////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) 2020 Saxonica Limited
// This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
// If a copy of the MPL was not distributed with this file, You can obtain one at http://mozilla.org/MPL/2.0/.
// This Source Code Form is "Incompatible With Secondary Licenses", as defined by the Mozilla Public License, v. 2.0.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

package net.sf.saxon.lib;

import net.sf.saxon.trans.XPathException;

import javax.xml.XMLConstants;
import javax.xml.transform.Source;
import java.net.URI;
import java.net.URISyntaxException;
import java.util.ArrayList;
import java.util.List;
import java.util.Objects;
import java.util.function.Predicate;

/**
 * This class implements the rules in a property such as {@link XMLConstants#ACCESS_EXTERNAL_SCHEMA},
 * which constrain the set of URIs that can be used by supplying a list of permitted protocols.
 */

public class ProtocolRestricter implements ResourceResolver {

    /**
     * Create a predicate from a list of allowed protocols.
     *
     * <p>The value is a comma-separated list of permitted protocols. A protocol is the
     *  scheme portion of a URI, or in the case of the JAR protocol, "jar"
     *  plus the scheme portion separated by colon. The value "all" gives access
     *  to all protocols (which is the default). The value "" (empty string) disallows
     *  all external resource access. (The format is thus the same as for
     *  <code>XMLConstants.ACCESS_EXTERNAL_SCHEMA</code> and similar attributes.)
     *  @param value the list of allowed protocols
     *  @return a predicate which matches a URI if the protocol is allowed
     */

    public static Predicate<URI> make(String value) {
        Objects.requireNonNull(value);
        value = value.trim();
        if (value.equals("all")) {
            // Allow all URIs
            return uri -> true;
        } else {
            final List<Predicate<URI>> permitted = new ArrayList<>();
            String[] tokens = value.split(",\\s*");
            for (String token : tokens) {
                if (token.startsWith("jar:") && token.length() > 4) {
                    String subScheme = token.substring(4).toLowerCase();
                    permitted.add(uri -> scheme(uri).equals("jar") &&
                            schemeSpecificPart(uri).toLowerCase().startsWith(subScheme));
                } else {
                    permitted.add(uri -> scheme(uri).equals(token));
                }
            }
            return uri -> {
                for (Predicate<URI> pred : permitted) {
                    if (pred.test(uri)) {
                        return true;
                    }
                }
                return false;
            };
        }
    }

    // The following methods are extracted to enable the C# transpiler to recognise what it needs to do...

    private static String scheme(URI uri) {
        return uri.getScheme();
    }

    private static String schemeSpecificPart(URI uri) {
        return uri.getSchemeSpecificPart();
    }

    public static void main(String[] args) throws Exception {
        URI uri = new URI(args[0]);
        System.err.println(uri.getScheme() + " + " + uri.getSchemeSpecificPart());
    }

    private final Predicate<URI> predicate;

    public ProtocolRestricter(Predicate<URI> predicate) {
        this.predicate = predicate;

    }

    /**
     * Process a resource request. This is not a true resolver because it never delivers a Source;
     * rather it validates the resource request to ensure that access to the requested URI is permitted.
     *
     * @param request the resource request
     * @return the returned Source; or null to delegate resolution to another resolver
     * @throws XPathException if the request is invalid in some way, or if the identified resource is unsuitable,
     *                        or if resolution is to fail rather than being delegated to another resolver.
     */
    @Override
    public Source resolve(ResourceRequest request) throws XPathException {
        if (request.uri != null) {
            try {
                URI uriU = new URI(request.uri);
                if (!predicate.test(uriU)) {
                    throw new XPathException("Resource access using URI " + request.uri + " has been disallowed");
                }
            } catch (URISyntaxException e) {
                return null;
            }
        }
        return null;
    }

}

