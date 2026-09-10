////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) 2018-2026 Saxonica Limited
// This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
// If a copy of the MPL was not distributed with this file, You can obtain one at http://mozilla.org/MPL/2.0/.
// This Source Code Form is "Incompatible With Secondary Licenses", as defined by the Mozilla Public License, v. 2.0.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////


package net.sf.saxon.functions;

import net.sf.saxon.Configuration;
import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.expr.parser.Loc;
import net.sf.saxon.expr.parser.RoleDiagnostic;
import net.sf.saxon.lib.ConversionRules;
import net.sf.saxon.ma.map.KeyValuePair;
import net.sf.saxon.ma.map.MapItem;
import net.sf.saxon.om.GroundedValue;
import net.sf.saxon.om.Sequence;
import net.sf.saxon.om.StructuredQName;
import net.sf.saxon.trans.Err;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.type.AtomicType;
import net.sf.saxon.type.BuiltInAtomicType;
import net.sf.saxon.type.Converter;
import net.sf.saxon.type.coercion.CoercionRules;
import net.sf.saxon.value.AtomicValue;
import net.sf.saxon.value.QNameValue;
import net.sf.saxon.value.SequenceType;
import net.sf.saxon.value.StringValue;

import java.util.*;
import java.util.function.Supplier;

/**
 * This class implements the rules for options parameters, as used in functions such as parse-json, serialize,
 * json-to-XML, map:merge. It provides a convenient way of ensuring that the options parameter conventions
 * are enforced.
 *
 * <p>An instance of this class contains information about all the options defined for one system-defined function.</p>
 */
public class OptionsParameter {

    private final Map<String, SequenceType> allowedOptions = new HashMap<>(8);
    private final Map<String, GroundedValue> defaultValues = new HashMap<>(8);
    private final Set<String> requiredOptions = new HashSet<>(4);
    private final Map<String, Set<String>> allowedValues = new HashMap<>(8);
    private String errorCodeForDisallowedValue;
    private String errorCodeForAbsentValue = "SXJE9999";
    private boolean allowCastFromString = false;
    private final int languageVersion;

    public OptionsParameter(int version) {
        this.languageVersion = version;
    }

    /**
     * Register a permitted option keyword, and the associated type of value, without defining
     * a default value
     * @param name the option keyword
     * @param type the required type
     */
    public void addAllowedOption(String name, SequenceType type) {
        allowedOptions.put(name, type);
    }

    /**
     * Register a required option keyword, and the associated type of value, where omitting
     * the value is an error
     *
     * @param name the option keyword
     * @param type the required type
     */
    public void addRequiredOption(String name, SequenceType type) {
        allowedOptions.put(name, type);
        requiredOptions.add(name);
    }

    /**
     * Register a permitted option keyword, and the associated type of value, with a default value
     *
     * @param name the option keyword
     * @param type the required type
     * @param defaultValue the default value if the option is not specified; or null
     *                                 if no default is defined
     */
    public void addAllowedOption(String name, SequenceType type, GroundedValue defaultValue) {
        allowedOptions.put(name, type);
        if (defaultValue != null) {
            defaultValues.put(name, defaultValue);
        }
    }

    /**
     * Enumerate the permitted values for an option keyword
     * @param name the option keyword
     * @param errorCode the error to be reported if the supplied value is not one of those permitted
     * @param values the permitted values
     */

    public void setAllowedValues(String name, String errorCode, String... values) {
        HashSet<String> valueSet = new HashSet<>(Arrays.asList(values));
        allowedValues.put(name, valueSet);
        errorCodeForDisallowedValue = errorCode;
    }

    /**
     * Get the XPath language version
     * @return the language version (for example 31 or 40)
     */

    public int getLanguageVersion() {
        return languageVersion;
    }

    /**
     * Process the options supplied on a function call, if any.
     *
     * @param arguments  the arguments to the function call
     * @param optionsArg the zero-based index of the options argument
     * @param context    the dynamic evaluation context
     * @return the options, after validation and expansion of defaults
     * @throws XPathException if any supplied options are invalid
     */

    public ProcessedOptions processSuppliedOptions(
            Sequence[] arguments, int optionsArg, XPathContext context) throws XPathException {
        MapItem options = null;
        if (arguments.length > optionsArg) {
            options = (MapItem) arguments[optionsArg].head();
        }
        if (options == null) {
            return getDefaultOptions();
        }
        return processSuppliedOptions(options, context);
    }


    /**
     * Process an XPath map containing the supplied values. Options that are recognized are copied
     * into the result map, after validation and expansion of defaults. Unrecognized options are
     * ignored, in accordance with the option parameter conventions
     *
     * @param supplied the supplied options as an XPath map object
     * @param context  the dynamic evaluation context
     * @return the validated options as a Java map
     * @throws XPathException if any supplied options are invalid
     */

    public ProcessedOptions processSuppliedOptions(MapItem supplied, XPathContext context) throws XPathException {
        Map<String, GroundedValue> result = new HashMap<>();
        Configuration config = context.getConfiguration();
        CoercionRules coercionRules = CoercionRules.forVersion(config, languageVersion);

        for (String req : requiredOptions) {
            if (supplied.get(new StringValue(req)) == null) {
                throw new XPathException("No value supplied for required option: " + req, errorCodeForAbsentValue);
            }
        }

        // A 4.0 rule: reject mis-spelt option names
        if (getLanguageVersion() >= 40) {
            for (KeyValuePair kvp : supplied.keyValuePairs()) {
                if (!(kvp.key() instanceof StringValue && allowedOptions.containsKey(kvp.key().getStringValue())) &&
                        !(kvp.key() instanceof QNameValue && !((QNameValue) kvp.key()).getNamespaceURI().isEmpty())) {
                    throw new XPathException("Unrecognized option: " + Err.depict(kvp.key()), "XPTY0004").asTypeError();
                }
            }
        }

        for (Map.Entry<String, SequenceType> allowedoptions : allowedOptions.entrySet()) {
            String nominalKey = allowedoptions.getKey();
            AtomicValue actualKey;
            if (nominalKey.startsWith("Q{")) {
                actualKey = new QNameValue(StructuredQName.fromEQName40((nominalKey)), BuiltInAtomicType.QNAME);
            } else {
                actualKey = new StringValue(nominalKey);
            }
            SequenceType required = allowedoptions.getValue();
            GroundedValue actual = supplied.get(actualKey);
            if (actual != null) {
                if (!required.matches(actual)) {
                    boolean ok = false;
                    if (actual instanceof StringValue && allowCastFromString
                            && required.getPrimaryType() instanceof AtomicType) {
                        try {ConversionRules rules = context.getConfiguration().getConversionRules();
                            actual = Converter.convert((StringValue)actual, (AtomicType)required.getPrimaryType(), rules);
                            ok = true;
                        } catch (XPathException err) {
                            ok = false;
                        }
                    }
                    if (!ok) {
                        Supplier<RoleDiagnostic> role =
                                () -> new RoleDiagnostic(RoleDiagnostic.OPTION, nominalKey, 0, "XPTY0004");
                        actual = coercionRules.coerce(
                                actual, required, SequenceType.ANY_SEQUENCE, config, role, Loc.NONE).materialize();
                    }
                }
                actual = actual.materialize();
                Set<String> permitted = allowedValues.get(nominalKey);
                if (permitted != null) {
                    if (!(actual instanceof AtomicValue) || !permitted.contains(((AtomicValue)actual).getStringValue())) {
                        StringBuilder message = new StringBuilder("Invalid option " + nominalKey + "=" + Err.depictSequence(actual) + ". Valid values are:");
                        int i = 0;
                        for (String v : permitted) {
                            message.append(i++ == 0 ? " " : ", ").append(v);
                        }
                        throw new XPathException(message.toString(), errorCodeForDisallowedValue);
                    }
                }
                result.put(nominalKey, actual);
            } else {
                GroundedValue def = defaultValues.get(nominalKey);
                if (def != null) {
                    result.put(nominalKey, def);
                }
            }
        }

        return new ProcessedOptions(result);

    }

    /**
     * Get a Java map containing the default values of registered options
     * @return a map containing the default values
     */

    public ProcessedOptions getDefaultOptions()  {
        return new ProcessedOptions(new HashMap<>(defaultValues));
    }

    /**
     * Get the error code to be used when a required option is not supplied
     * @return the local part of the error code to be used
     */

    public String getErrorCodeForAbsentValue() {
        return errorCodeForAbsentValue;
    }

    /**
     * Set the error code to be used when a required option is not supplied
     * @param errorCodeForAbsentValue the local part of the error code to be used
     */

    public void setErrorCodeForAbsentValue(String errorCodeForAbsentValue) {
        this.errorCodeForAbsentValue = errorCodeForAbsentValue;
    }

    /**
     * Ask whether it is permissible to supply the value as a string, which is cast to the
     * required type, rather than supplying the required type directly. Normally false.
     * @return true if it is permitted to supply the value as a string
     */

    public boolean isAllowCastFromString() {
        return allowCastFromString;
    }

    /**
     * Say whether it is permissible to supply the value as a string, which is cast to the
     * required type, rather than supplying the required type directly. Normally false.
     * @param allowCastFromString true if it is permitted to supply the value as a string
     */

    public void setAllowCastFromString(boolean allowCastFromString) {
        this.allowCastFromString = allowCastFromString;
    }


}

