cmake_minimum_required(VERSION 3.27)
project(SaxonC VERSION 12.6.0)
enable_testing()

set(SaxonC_EDITION "he" CACHE STRING "Saxon edition")

include("../cmake/libsaxonc.cmake")

if(PROJECT_IS_TOP_LEVEL)
    # include(CPack) will happen inside the following call
    add_subdirectory(packaging)
endif()