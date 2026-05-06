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

TEST_CASE( "XPathProcessor CWD is correctly inherited from the SaxonProcessor" ) {
    SaxonProcessor *processor = new SaxonProcessor(false);
    processor->setcwd("/path/to/nowhere");

    XPathProcessor *xpath = processor->newXPathProcessor();

    REQUIRE_THAT( xpath->getcwd(), Equals("/path/to/nowhere")  );

    delete xpath;
    delete processor;
    processor->release();
    delete SaxonProcessor::sxn_environ;
}

TEST_CASE( "setting XPathProcessor CWD locally correctly overrides the inherited value" ) {
    SaxonProcessor *processor = new SaxonProcessor(false);
    processor->setcwd("/path/to/nowhere");

    XPathProcessor *xpath = processor->newXPathProcessor();
    xpath->setcwd("/path/to/somewhere");

    REQUIRE_THAT( xpath->getcwd(), Equals("/path/to/somewhere")  );

    delete xpath;
    delete processor;
    processor->release();
    delete SaxonProcessor::sxn_environ;
}

TEST_CASE( "Resolving a relative URI in an XPath expression parsed from a string where CWD has" ) {
    string srcfile = string(__FILE__);
    auto last_path_sep = srcfile.find_last_of(path_sep);
    auto base_wd = srcfile.substr(0, last_path_sep) + path_sep + "test_data";
    auto trailing_wd = base_wd + path_sep;


    string input = "string(doc('input.xml')/input)";
    auto *processor = new SaxonProcessor(false);
    XPathProcessor *xpath = processor->newXPathProcessor();

    SECTION("no trailing path separator") {
        xpath->setcwd(base_wd.c_str());

        auto expected = xpath->evaluateSingle(input.c_str(), "utf-8")->getStringValue();
        REQUIRE_THAT( expected , ContainsSubstring("An input") );
    }

    SECTION("a trailing path separator") {
        xpath->setcwd(trailing_wd.c_str());

        auto expected = xpath->evaluateSingle(input.c_str(), "utf-8")->getStringValue();
        REQUIRE_THAT( expected , ContainsSubstring("An input") );
    }

    delete xpath;
    delete processor;
    processor->release();
    delete SaxonProcessor::sxn_environ;
}
