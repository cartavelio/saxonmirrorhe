package nativetests;

import com.saxonica.config.JavaLocalizerFactory;
import com.saxonica.config.ProfessionalConfiguration;
import junit.framework.TestCase;
import net.sf.saxon.Configuration;
import net.sf.saxon.expr.number.Numberer_en;
import net.sf.saxon.lib.LocalizerFactory;
import net.sf.saxon.lib.Numberer;
import net.sf.saxon.s9api.Processor;
import net.sf.saxon.s9api.SaxonApiException;
import net.sf.saxon.s9api.XPathCompiler;

/**
 *  Test various aspects of setting the Saxon Configuration from Java code
 */
public class ConfigurationTest extends TestCase {

    /**
     * Demonstrate use of a user-written LocalizerFactory to customize numbering
     */

    public void testCustomizedNumbering() {
        try {
            Configuration config = new ProfessionalConfiguration();
            LocalizerFactory lf = new JavaLocalizerFactory(config) {
                public Numberer getNumberer(String language, String country) {
                    Numberer n = super.getNumberer(language, country);
                    if (n instanceof Numberer_en) {
                        // request "twenty-six" rather than "twenty six"
                        ((Numberer_en)n).setTensUnitsSeparatorCardinal("-");
                    }
                    return n;
                }
            };
            config.setLocalizerFactory(lf);
            Processor p = new Processor(config);
            XPathCompiler xp = p.newXPathCompiler();
            xp.setLanguageVersion("3.0");
            String result = xp.compile("format-integer(26, 'w')").load().evaluateSingle().toString();
            assertEquals("result", "twenty six", result.replace('-', ' '));
        } catch (SaxonApiException e) {
            fail(e.getMessage());
        }
    }
}

//
// The contents of this file are subject to the Mozilla Public License Version
// 1.0 (the "License");
// you may not use this file except in compliance with the License. You may
// obtain a copy of the
// License at http://www.mozilla.org/MPL/
//
// Software distributed under the License is distributed on an "AS IS" basis,
// WITHOUT WARRANTY OF ANY KIND, either express or implied.
// See the License for the specific language governing rights and limitations
// under the License.
//
// The Original Code is: all this file.
//
// The Initial Developer of the Original Code is Michael Kay,
//
// Portions created by (your name) are Copyright (C) (your legal entity). All
// Rights Reserved.
//
// Contributor(s): none.
//



