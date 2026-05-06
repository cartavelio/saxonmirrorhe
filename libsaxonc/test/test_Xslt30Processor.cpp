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
#include <sstream>

#include "common.h"

#include <saxonc/XdmNode.h>

using namespace std;
using Catch::Matchers::ContainsSubstring;
using Catch::Matchers::Equals;

TEST_CASE( "CWD is correctly inherited from the Processor" ) {
    SaxonProcessor *processor = new SaxonProcessor(false);
    processor->setcwd("/path/to/nowhere");

    Xslt30Processor *xslt = processor->newXslt30Processor();

    REQUIRE_THAT( xslt->getcwd(), Equals("/path/to/nowhere")  );

    delete xslt;
    delete processor;
    processor->release();
    delete SaxonProcessor::sxn_environ;
}

TEST_CASE( "setting CWD locally correctly overrides the inherited value" ) {
    SaxonProcessor *processor = new SaxonProcessor(false);
    processor->setcwd("/path/to/nowhere");

    Xslt30Processor *xslt = processor->newXslt30Processor();
    xslt->setcwd("/path/to/somewhere");

    REQUIRE_THAT( xslt->getcwd(), Equals("/path/to/somewhere")  );

    delete xslt;
    delete processor;
    processor->release();
    delete SaxonProcessor::sxn_environ;
}

TEST_CASE( "Resolving a relative URI in an XSLT parsed from a string where CWD has" ) {
    string srcfile = string(__FILE__);
    auto last_path_sep = srcfile.find_last_of(path_sep);
    auto base_wd = srcfile.substr(0, last_path_sep) + path_sep + "test_data";
    auto trailing_wd = base_wd + path_sep;


    std::ifstream is(trailing_wd + "root.xsl");
    std::stringstream buf;
    buf << is.rdbuf();
    string input = buf.str();
    auto *processor = new SaxonProcessor(false);
    Xslt30Processor *xslt = processor->newXslt30Processor();

    SECTION("no trailing path separator") {
        xslt->setcwd(base_wd.c_str());

        auto executable = xslt->compileFromString(input.c_str());
        auto expected = executable->transformFileToString((trailing_wd + "input.xml").c_str());
        REQUIRE_THAT( expected , ContainsSubstring("A result") );
    }

    SECTION("a trailing path separator") {
        xslt->setcwd(trailing_wd.c_str());

        auto executable = xslt->compileFromString(input.c_str());
        auto expected = executable->transformFileToString((trailing_wd + "input.xml").c_str());
        REQUIRE_THAT( expected , ContainsSubstring("A result") );
    }

    delete xslt;
    delete processor;
    processor->release();
    delete SaxonProcessor::sxn_environ;
}
