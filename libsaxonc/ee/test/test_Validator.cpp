////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) 2026 Saxonica Limited.
// This Source Code Form is subject to the terms of the Mozilla Public License,
// v. 2.0. If a copy of the MPL was not distributed with this file, You can
// obtain one at http://mozilla.org/MPL/2.0/. This Source Code Form is
// "Incompatible With Secondary Licenses", as defined by the Mozilla Public
// License, v. 2.0.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
#include <catch2/catch_test_macros.hpp>
#include <catch2/matchers/catch_matchers_string.hpp>
#include "saxonc/SaxonProcessor.h"
#include "saxonc/XdmNode.h"
#include <string>
#include <sstream>
#include <fstream>

#ifdef _WIN32
const std::string path_sep = "\\";
#else
const std::string path_sep = "/";
#endif


using namespace std;
using Catch::Matchers::ContainsSubstring;

TEST_CASE( "Resolving a relative URI in a Schema registered from a string where CWD has" ) {
    string srcfile = string(__FILE__);
    auto last_path_sep = srcfile.find_last_of(path_sep);
    auto base_wd = srcfile.substr(0, last_path_sep) + path_sep + "test_data";
    auto trailing_wd = base_wd + path_sep;


    std::ifstream is(trailing_wd + "root.xsd");
    std::stringstream buf;
    buf << is.rdbuf();
    string input = buf.str();
    auto *processor = new SaxonProcessor(true);
    SchemaValidator *validator = processor->newSchemaValidator();

    SECTION("no trailing path separator") {
        validator->setcwd(base_wd.c_str());

        validator->registerSchemaFromString(input.c_str());
        auto expected = validator->validateToNode((trailing_wd + "instance.xml").c_str())->getStringValue();
        REQUIRE_THAT( expected , ContainsSubstring("A result") );
    }

    SECTION("a trailing path separator") {
        validator->setcwd(trailing_wd.c_str());

        validator->registerSchemaFromString(input.c_str());
        auto expected = validator->validateToNode((trailing_wd + "instance.xml").c_str())->getStringValue();
        REQUIRE_THAT( expected , ContainsSubstring("A result") );
    }

    delete validator;
    delete processor;
    processor->release();
    delete SaxonProcessor::sxn_environ;
}
