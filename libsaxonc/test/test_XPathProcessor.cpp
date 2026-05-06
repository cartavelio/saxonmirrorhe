////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) 2026 Saxonica Limited.
// This Source Code Form is subject to the terms of the Mozilla Public License,
// v. 2.0. If a copy of the MPL was not distributed with this file, You can
// obtain one at http://mozilla.org/MPL/2.0/. This Source Code Form is
// "Incompatible With Secondary Licenses", as defined by the Mozilla Public
// License, v. 2.0.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
#include <fstream>
#include <catch2/catch_test_macros.hpp>
#include <catch2/matchers/catch_matchers_string.hpp>
#include "saxonc/SaxonProcessor.h"

#include "common.h"

#include <saxonc/XdmNode.h>

using namespace std;
using Catch::Matchers::ContainsSubstring;
using Catch::Matchers::Equals;

TEST_CASE( "XPathProcessor CWD" ) {
    GIVEN( "an XPathProcessor created by a SaxonProcessor with an explicitly set cwd" ) {
        SaxonProcessor *processor = new SaxonProcessor(false);
        processor->setcwd("/path/to/nowhere");

        XPathProcessor *xpath = processor->newXPathProcessor();

        WHEN( "the cwd is not set directly" ) {
            THEN( "getcwd() returns the Processor's cwd" ) {
                REQUIRE_THAT( xpath->getcwd(), Equals("/path/to/nowhere")  );
            }
        }

        WHEN( "the cwd is set directly" ) {
            xpath->setcwd("/path/to/somewhere");

            THEN( "getcwd() returns the directly set cwd" ) {
                REQUIRE_THAT( xpath->getcwd(), Equals("/path/to/somewhere")  );
            }
        }

        delete xpath;
        delete processor;
        processor->release();
        delete SaxonProcessor::sxn_environ;
    }
}
TEST_CASE( "XPathProcessor URI resolution in expression evaluation" ) {
    string srcfile = string(__FILE__);
    auto last_path_sep = srcfile.find_last_of(path_sep);
    auto base_wd = srcfile.substr(0, last_path_sep) + path_sep + "test_data";

    auto *processor = new SaxonProcessor(false);
    XPathProcessor *xpath = processor->newXPathProcessor();

    GIVEN( "an expression calling doc() with a relative URI" ) {
        string input = "string(doc('input.xml')/input)";

        WHEN( "CWD has no trailing path separator" ) {
            xpath->setcwd(base_wd.c_str());

            THEN( "The doc() function returns the correct document" ) {
                auto expected = xpath->evaluateSingle(input.c_str(), "utf-8")->getStringValue();
                REQUIRE_THAT( expected , ContainsSubstring("An input") );
            }
        }

        WHEN( "CWD has a trailing path separator" ) {
            auto trailing_wd = base_wd + path_sep;
            xpath->setcwd(trailing_wd.c_str());

            THEN( "The doc() function returns the correct document" ) {
                auto expected = xpath->evaluateSingle(input.c_str(), "utf-8")->getStringValue();
                REQUIRE_THAT( expected , ContainsSubstring("An input") );
            }
        }
    }

    delete xpath;
    delete processor;
    processor->release();
    delete SaxonProcessor::sxn_environ;
}
// TEST_CASE( "Resolving a relative URI in an XPath expression parsed from a string where CWD has" ) {
//     string srcfile = string(__FILE__);
//     auto last_path_sep = srcfile.find_last_of(path_sep);
//     auto base_wd = srcfile.substr(0, last_path_sep) + path_sep + "test_data";
//     auto trailing_wd = base_wd + path_sep;
//
//
//     string input = "string(doc('input.xml')/input)";
//     auto *processor = new SaxonProcessor(false);
//     XPathProcessor *xpath = processor->newXPathProcessor();
//
//     SECTION("no trailing path separator") {
//         xpath->setcwd(base_wd.c_str());
//
//         auto expected = xpath->evaluateSingle(input.c_str(), "utf-8")->getStringValue();
//         REQUIRE_THAT( expected , ContainsSubstring("An input") );
//     }
//
//     SECTION("a trailing path separator") {
//         xpath->setcwd(trailing_wd.c_str());
//
//         auto expected = xpath->evaluateSingle(input.c_str(), "utf-8")->getStringValue();
//         REQUIRE_THAT( expected , ContainsSubstring("An input") );
//     }
//
//     delete xpath;
//     delete processor;
//     processor->release();
//     delete SaxonProcessor::sxn_environ;
// }
