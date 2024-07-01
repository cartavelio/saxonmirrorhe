package net.sf.saxon.sxpath;

import net.sf.saxon.Configuration;
import net.sf.saxon.expr.instruct.Executable;
import net.sf.saxon.om.NamespaceUri;
import net.sf.saxon.s9api.HostLanguage;
import net.sf.saxon.expr.PackageData;
import net.sf.saxon.s9api.UnprefixedElementMatchingPolicy;
import net.sf.saxon.type.AnyFunctionType;
import org.junit.jupiter.api.BeforeEach;
import org.junit.jupiter.api.DisplayName;
import org.junit.jupiter.api.Nested;
import org.junit.jupiter.api.Test;

import java.util.Collections;
import java.util.HashMap;
import java.util.Map;

import net.sf.saxon.lib.NamespaceConstant;
import static net.sf.saxon.test.assertions.SaxonMatcherAssert.assertThat;
import static org.hamcrest.CoreMatchers.*;
import static org.hamcrest.Matchers.emptyIterable;

class IndependentContextTest {
    Configuration config = new Configuration();
    IndependentContext subject;

    static final Map<String, NamespaceUri> STANDARD_NAMESPACES;
    static {
        HashMap<String, NamespaceUri> ns = new HashMap<>();
        ns.put("xml", NamespaceUri.XML);
        ns.put("xsl", NamespaceUri.XSLT);
        ns.put("saxon", NamespaceUri.SAXON);
        ns.put("xs", NamespaceUri.SCHEMA);
        ns.put("", NamespaceUri.NULL);
        STANDARD_NAMESPACES = Collections.unmodifiableMap(ns);
    }

    @Nested
    class DefaultConstructor {
        @BeforeEach
        void setup() {
            subject = new IndependentContext(config);
        }

        @Test
        @DisplayName("has the default XPath language level support")
        void xpathVersion() {
            assertThat(subject.getXPathVersion(), equalTo(31));
        }

        @Test
        @DisplayName("Returns its configuration")
        void hasConfig() {
            assertThat(subject.getConfiguration(), is(sameInstance(config)));
        }

        @Test
        @DisplayName("Has the standard namespaces defined (but no others)")
        void standardNamespaces() {
            assertThat(subject.namespaces, equalTo(STANDARD_NAMESPACES));
        }

        @Test
        @DisplayName("The default namespace is the null namespace")
        void defaultNamespace() {
            assertThat(subject.getDefaultElementNamespace(), is(sameInstance(NamespaceUri.NULL)));
        }

        @Test
        @DisplayName("Has no defined Variables")
        void noVars() {
            assertThat(subject.getExternalVariables(), is(emptyIterable()));
        }

        @Test
        @DisplayName("Is using the default function library")
        void defaultFunctionLib() {
            assertThat(subject.usingDefaultFunctionLibrary, is(equalTo(true)));
        }

        @Test
        @DisplayName("The default function library is XPath 3.1's")
        void xpathFunctionLibVersion() {
            // function lib 31
            // This is gnarlier - there's a bunch of repeated logic for constructing 'default' function libraries (FunctionLibraryLists containing FunctionLibraries)
            // Suspect it would be a good candidate for refactoring into an extracted method
            // assertThat(subject.getFunctionLibrary(), is(equalTo(...)));
        }

        @Test
        @DisplayName("Uses the collation set in the configuration")
        void correctCollation() {
            assertThat(subject.getDefaultCollationName(), is(equalTo(config.getDefaultCollationName())));
        }

        @Test
        @DisplayName("undeclared variables not allowed by default")
        void noUndeclaredVars() {
            assertThat(subject.isAllowUndeclaredVariables(), equalTo(false));
        }

        @Test
        @DisplayName("the unprefixed element matching policy is to match to the default namespace")
        void unprefixedMatching() {
            assertThat(subject.getUnprefixedElementMatchingPolicy(), equalTo(UnprefixedElementMatchingPolicy.DEFAULT_NAMESPACE));
        }

        @Nested
        class ContextPackageData {
            PackageData subjectPackageData;
            @BeforeEach
            void setup() {
                subjectPackageData = subject.getPackageData();
            }

            @Test
            @DisplayName("claims to be an XPath host language package")
            void hostLanguage() {
                assertThat(subjectPackageData.getHostLanguage(), equalTo(HostLanguage.XPATH));
            }

            @Test
            @DisplayName("claims XPath 3.1")
            void lanugageVersion() {
                assertThat(subjectPackageData.getHostLanguageVersion(), equalTo(31));
            }

            @Test
            @DisplayName("The package is not Schema-aware")
            void schemaAware() {
                assertThat(subjectPackageData.isSchemaAware(), equalTo(false));
            }
        }
    }


    @Nested
    class CopyConstructor {
        IndependentContext src;
        Executable executable;

        @BeforeEach
        void setup() {
            src = new IndependentContext(config);
            executable = new Executable(config);

            // declare some namespaces
            src.declareNamespace("", NamespaceUri.of("http://example.org/#null"));
            src.declareNamespace("eg", NamespaceUri.of("http://example.org/"));

            // declare a variable
            src.declareVariable(NamespaceUri.of("http://example.org/"), "eg-var");

            // alter some fields
            src.setAllowUndeclaredVariables(true);
            src.setUnprefixedElementMatchingPolicy(UnprefixedElementMatchingPolicy.ANY_NAMESPACE);
            src.setDefaultCollationName(NamespaceConstant.HTML5_CASE_BLIND_COLLATION_URI);
            src.setSchemaAware(true);
            src.setXPathLanguageLevel(30);
            src.setRequiredContextItemType(AnyFunctionType.ANY_FUNCTION);
            src.setExecutable(executable);

            subject = new IndependentContext(src);
        }

        @Test
        @DisplayName("Returns its configuration")
        void hasConfig() {
            assertThat(subject.getConfiguration(), is(sameInstance(config)));
        }

        @Test
        @DisplayName("Has the namespaces from its source")
        void inheritsNamespaces() {
            assertThat(subject.namespaces, is(equalTo(src.namespaces)));
        }

        @Test
        @DisplayName("The default namespace is inherited from the source")
        void defaultNamespace() {
            assertThat(subject.getDefaultElementNamespace(), is(equalTo(src.getDefaultElementNamespace())));
        }

        @Test
        @DisplayName("Has no defined Variables")
        void noVars() {
            assertThat(subject.getExternalVariables(), is(emptyIterable()));
        }

        @Test
        @DisplayName("Is using the default function library")
        void defaultFunctionLib() {
            assertThat(subject.usingDefaultFunctionLibrary, is(equalTo(true)));
        }

        @Test
        @DisplayName("Uses the correct collation")
        void correctCollation() {
            assertThat(subject.getDefaultCollationName(), is(equalTo(NamespaceConstant.HTML5_CASE_BLIND_COLLATION_URI)));
        }

        @Test
        @DisplayName("undeclared variables are allowed")
        void noUndeclaredVars() {
            assertThat(subject.isAllowUndeclaredVariables(), equalTo(true));
        }

        @Test
        @DisplayName("the unprefixed element matching policy is set correctly")
        void unprefixedMatching() {
            assertThat(subject.getUnprefixedElementMatchingPolicy(), equalTo(UnprefixedElementMatchingPolicy.ANY_NAMESPACE));
        }

        @Test
        @DisplayName("inherits the XPath version")
        void xpathVersion() {
            assertThat(subject.getXPathVersion(), equalTo(30));
        }

        @Test
        @DisplayName("inherits the required context item type")
        void requiredContextItem() {
            assertThat(subject.getRequiredContextItemType(), is(sameInstance(AnyFunctionType.ANY_FUNCTION)));
        }

        @Test
        @DisplayName("inherits the executable")
        void hasExecutable() {
            assertThat(subject.getExecutable(), is(sameInstance(executable)));
        }

        @Nested
        class ContextPackageData {
            PackageData subjectPackageData;
            @BeforeEach
            void setup() {
                subjectPackageData = subject.getPackageData();
            }

            @Test
            @DisplayName("claims to be an XPath host language package")
            void hostLanguage() {
                assertThat(subjectPackageData.getHostLanguage(), equalTo(HostLanguage.XPATH));
            }

            @Test
            @DisplayName("claims XPath 3.0")
            void lanugageVersion() {
                assertThat(subjectPackageData.getHostLanguageVersion(), equalTo(30));
            }

            @Test
            @DisplayName("The package is Schema-aware")
            void schemaAware() {
                assertThat(subjectPackageData.isSchemaAware(), equalTo(true));
            }
        }
    }
}