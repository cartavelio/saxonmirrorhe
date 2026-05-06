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

TEST_CASE( "Xslt30Processor CWD" ) {
    SaxonProcessor *processor = new SaxonProcessor(false);

    GIVEN( "An Xslt30Processor created from a SaxonProcessor whose CWD is set" ) {
        processor->setcwd("/path/to/nowhere");
        Xslt30Processor *xslt = processor->newXslt30Processor();

        THEN( "The CWD is correctly inherited" ) {
            REQUIRE_THAT( xslt->getcwd(), Equals("/path/to/nowhere")  );
        }

        WHEN( "The CWD is directly set" ) {
            xslt->setcwd("/path/to/somewhere");

            THEN( "the correct value is returned" ) {
                REQUIRE_THAT( xslt->getcwd(), Equals("/path/to/somewhere")  );
            }
        }
        delete xslt;
    }

    delete processor;
    processor->release();
    delete SaxonProcessor::sxn_environ;
}

TEST_CASE( "Xslt30Processor.compilingFromString() relative URI in an XSLT parsed from a string where CWD has" ) {
    string srcfile = string(__FILE__);
    auto last_path_sep = srcfile.find_last_of(path_sep);
    auto base_wd = srcfile.substr(0, last_path_sep) + path_sep + "test_data";
    auto trailing_wd = base_wd + path_sep;
    const auto instance_path = trailing_wd + "input.xml";

    std::ifstream is(trailing_wd + "root.xsl");
    std::stringstream buf;
    buf << is.rdbuf();
    string input = buf.str();
    auto *processor = new SaxonProcessor(false);
    Xslt30Processor *xslt = processor->newXslt30Processor();

    GIVEN( "An XSLT using xsl:import with a relative URI" ) {
        WHEN( "The CWD has no trailing path separator" ) {
            xslt->setcwd(base_wd.c_str());

            THEN( "the xsl:import correctly imports" ) {
                auto executable = xslt->compileFromString(input.c_str());
                auto expected = executable->transformFileToString(instance_path.c_str());
                REQUIRE_THAT( expected , ContainsSubstring("A result") );
            }
        }

        WHEN( "The CWD has a trailing path separator" ) {
            xslt->setcwd(trailing_wd.c_str());

            THEN( "the xsl:import correctly imports" ) {
                auto executable = xslt->compileFromString(input.c_str());
                auto expected = executable->transformFileToString(instance_path.c_str());
                REQUIRE_THAT( expected , ContainsSubstring("A result") );
            }
        }
    }

    delete xslt;
    delete processor;
    processor->release();
    delete SaxonProcessor::sxn_environ;
}
