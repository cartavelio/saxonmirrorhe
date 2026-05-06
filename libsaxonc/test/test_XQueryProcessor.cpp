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
#include <saxonc/XdmItem.h>

#include "saxonc/SaxonProcessor.h"

#include "common.h"

#include <saxonc/XdmValue.h>

using namespace std;
using Catch::Matchers::ContainsSubstring;
using Catch::Matchers::Equals;

TEST_CASE( "XQueryProcessor CWD is correctly inherited from the SaxonProcessor" ) {
    SaxonProcessor *processor = new SaxonProcessor(false);
    processor->setcwd("/path/to/nowhere");

    XQueryProcessor *xquery = processor->newXQueryProcessor();

    REQUIRE_THAT( xquery->getcwd(), Equals("/path/to/nowhere")  );

    delete xquery;
    delete processor;
    processor->release();
    delete SaxonProcessor::sxn_environ;
}

TEST_CASE( "setting XQueryProcessor CWD locally correctly overrides the inherited value" ) {
    SaxonProcessor *processor = new SaxonProcessor(false);
    processor->setcwd("/path/to/nowhere");

    XQueryProcessor *xquery = processor->newXQueryProcessor();
    xquery->setcwd("/path/to/somewhere");

    REQUIRE_THAT( xquery->getcwd(), Equals("/path/to/somewhere")  );

    delete xquery;
    delete processor;
    processor->release();
    delete SaxonProcessor::sxn_environ;
}

TEST_CASE( "Resolving a relative URI in an XQuery query parsed from a string where CWD has" ) {
    string srcfile = string(__FILE__);
    auto last_path_sep = srcfile.find_last_of(path_sep);
    auto base_wd = srcfile.substr(0, last_path_sep) + path_sep + "test_data";
    auto trailing_wd = base_wd + path_sep;


    string input = R"EOQ(xquery version "3.1";
for $item in doc("input.xml")//input
return <p>{data($item)}</p>
)EOQ";
    auto *processor = new SaxonProcessor(false);
    XQueryProcessor *xquery = processor->newXQueryProcessor();
    xquery->setQueryContent(input.c_str());

    SECTION("no trailing path separator") {
        xquery->setcwd(base_wd.c_str());

        auto res = xquery->runQueryToValue();
        auto expected = res->getHead()->getStringValue();
        REQUIRE_THAT( expected , Equals("An input") );
    }

    SECTION("a trailing path separator") {
        xquery->setcwd(trailing_wd.c_str());

        auto res = xquery->runQueryToValue();
        auto expected = res->getHead()->getStringValue();
        REQUIRE_THAT( expected , Equals("An input") );
    }

    delete xquery;
    delete processor;
    processor->release();
    delete SaxonProcessor::sxn_environ;
}
