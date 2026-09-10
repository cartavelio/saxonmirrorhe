/// /////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) 2026 Saxonica Limited
// This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
// If a copy of the MPL was not distributed with this file, You can obtain one at http://mozilla.org/MPL/2.0/.
// This Source Code Form is "Incompatible With Secondary Licenses", as defined by the Mozilla Public License, v. 2.0.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

package net.sf.saxon.regex;

import net.sf.saxon.str.UnicodeString;

/**
 * Information about a matching regex group
 * @param index the index number of the group (by counting opening parens)
 * @param groupName the group name if any, otherwise null
 * @param startPosition the zero-based start position of the match
 * @param content the matching substring
 */

public record RegexGroupInfo(int index, UnicodeString groupName, int startPosition, UnicodeString content) {

}

