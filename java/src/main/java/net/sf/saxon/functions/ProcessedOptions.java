////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) 2018-2026 Saxonica Limited
// This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
// If a copy of the MPL was not distributed with this file, You can obtain one at http://mozilla.org/MPL/2.0/.
// This Source Code Form is "Incompatible With Secondary Licenses", as defined by the Mozilla Public License, v. 2.0.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////


package net.sf.saxon.functions;

import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.ma.map.MapItem;
import net.sf.saxon.om.GroundedValue;
import net.sf.saxon.om.Item;
import net.sf.saxon.str.UnicodeString;
import net.sf.saxon.value.BooleanValue;

import java.util.HashMap;
import java.util.Map;
import java.util.Optional;

/**
 * This class represents the results of processing an options parameter, delivered as the result
 * of {@link OptionsParameter#processSuppliedOptions(MapItem, XPathContext)}
 */
public class ProcessedOptions {

    private final Map<String, GroundedValue> optionValues;

    public final static ProcessedOptions EMPTY_OPTIONS = new ProcessedOptions(new HashMap<>());

    public ProcessedOptions(Map<String, GroundedValue> optionValues) {
        this.optionValues = optionValues;
    }

    /**
     * Get the underlying map of options values
     */

    public Map<String, GroundedValue> getMap() {
        return optionValues;
    }

    /**
     * Ask if a particular option has been supplied
     *
     * @param option the name of the option
     * @return true if the option was supplied or has a default value
     */

    public boolean containsKey(String option) {
        return optionValues.containsKey(option);
    }

    public Iterable<String> keySet() {
        return optionValues.keySet();
    }

    public boolean isEmpty() {
        return optionValues.isEmpty();
    }

    /**
     * Get the raw value of a supplied option, or its default if it was absent
     * @param option the name of the option
     * @return the value of the option, or its default if one was defined
     */

    public GroundedValue get(String option) {
        return optionValues.get(option);
    }

    /**
     * Get the first and only item in the raw value of a supplied option, or its default if it was absent
     *
     * @param option the name of the option
     * @return the value of the option, or its default if one was defined, or null if the option
     * was not supplied and there is no default. The method should only be called when the option
     * declaration defines the cardinality of the option as zero or one, or exactly one.
     */

    public Item asItem(String option) {
        GroundedValue value = optionValues.get(option);
        if (value == null || value.head() == null) {
            return null;
        }
        return value.head();
    }

    /**
     * Get the value of an option as a string, returning null if the option
     * was absent and has no default
     */

    public String asString(String option) {
        GroundedValue value = optionValues.get(option);
        if (value == null || value.head() == null) {
            return null;
        }
        return value.head().getStringValue();
    }

    /**
     * Get the value of an option as a string, returning null if the option
     * was absent and has no default
     */

    public Optional<String> asOptionalString(String option) {
        GroundedValue value = optionValues.get(option);
        if (value == null || value.head() == null) {
            return Optional.empty();
        }
        return Optional.of(value.head().getStringValue());
    }

    /**
     * Get the value of an option as an integer, returning null if the option
     * was absent and has no default
     */

    public Optional<Long> asOptionalLong(String option) {
        GroundedValue value = optionValues.get(option);
        if (value == null || value.head() == null) {
            return Optional.empty();
        }
        return Optional.of(Long.parseLong(value.head().getStringValue()));
    }

    /**
     * Get the value of an option as a boolean
     */

    public boolean asBoolean(String option, boolean defaultValue) {
        GroundedValue value = optionValues.get(option);
        if (value == null || value.head() == null) {
            return defaultValue;
        }
        return ((BooleanValue)value.head()).getBooleanValue();
    }

    /**
     * Get the value of an option as a string, returning null if the option
     * was absent and has no default
     */

    public Optional<Boolean> asOptionalBoolean(String option) {
        GroundedValue value = optionValues.get(option);
        if (value == null || value.head() == null) {
            return Optional.empty();
        }
        return Optional.of(((BooleanValue) value.head()).getBooleanValue());
    }


    /**
     * Get the value of an option as a Unicode string, returning null if the option
     * was absent and has no default
     */

    public UnicodeString asUnicodeString(String option) {
        GroundedValue value = optionValues.get(option);
        if (value == null || value.head() == null) {
            return null;
        }
        return value.head().getUnicodeStringValue();
    }

    /**
     * Get the value of an option as an optional Unicode string
     */

    public Optional<UnicodeString> asOptionalUnicodeString(String option) {
        GroundedValue value = optionValues.get(option);
        if (value == null || value.head() == null) {
            return Optional.empty();
        }
        return Optional.of(value.head().getUnicodeStringValue());
    }
}

