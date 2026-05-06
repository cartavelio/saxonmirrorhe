////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) 2025 Saxonica Limited.
// This Source Code Form is subject to the terms of the Mozilla Public License,
// v. 2.0. If a copy of the MPL was not distributed with this file, You can
// obtain one at http://mozilla.org/MPL/2.0/. This Source Code Form is
// "Incompatible With Secondary Licenses", as defined by the Mozilla Public
// License, v. 2.0.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
#include <catch2/catch_test_macros.hpp>
#include <catch2/matchers/catch_matchers_string.hpp>
#include "saxonc/SaxonProcessor.h"
#include <string>

using namespace std;
using Catch::Matchers::Equals;

TEST_CASE( "SaxonProcessor.getSaxonEdition()", "[processor][pe]" ) {
    SaxonProcessor *processor;

    GIVEN( "An unlicensed processor" ) {
        processor = new SaxonProcessor(false);

        THEN( "the edition is reported as HE" ) {
            REQUIRE_THAT( processor->getSaxonEdition(), Equals("HE") );
        }
    }

    GIVEN( "A licensed processor" ) {
        processor = new SaxonProcessor(true);

        THEN( "the edition is reported as PE" ) {
            REQUIRE_THAT( processor->getSaxonEdition(), Equals("PE") );
        }
    }

    delete processor;
    processor->release();
    delete SaxonProcessor::sxn_environ;
}