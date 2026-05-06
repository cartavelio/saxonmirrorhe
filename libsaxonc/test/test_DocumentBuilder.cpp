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

TEST_CASE( "parsing a document from a string with a DocumentBuilder resolves a relative URI where CWD has" ) {
    string srcfile = string(__FILE__);
    auto last_path_sep = srcfile.find_last_of(path_sep);
    auto base_wd = srcfile.substr(0, last_path_sep) + path_sep + "test_data";
    auto trailing_wd = base_wd + path_sep;


    std::ifstream is(trailing_wd + "has_ent.xml");
    std::stringstream buf;
    buf << is.rdbuf();
    string input = buf.str();
    auto *processor = new SaxonProcessor(false);

    SECTION("no trailing path separator") {
        processor->setcwd(base_wd.c_str());
        DocumentBuilder *builder = processor->newDocumentBuilder();

        auto document = builder->parseXmlFromString(input.c_str(), "UTF-8");
        auto expected = document->getStringValue();
        REQUIRE_THAT( expected , ContainsSubstring("Hello Entity") );
        delete builder;
    }

    SECTION("a trailing path separator") {
        processor->setcwd(trailing_wd.c_str());
        DocumentBuilder *builder = processor->newDocumentBuilder();

        auto document = builder->parseXmlFromString(input.c_str(), "UTF-8");
        auto expected = document->getStringValue();
        REQUIRE_THAT( expected , ContainsSubstring("Hello Entity") );
        delete builder;
    }

    delete processor;
    processor->release();
    delete SaxonProcessor::sxn_environ;
}
