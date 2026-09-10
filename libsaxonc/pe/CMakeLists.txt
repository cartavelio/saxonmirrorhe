cmake_minimum_required(VERSION 3.27)
project(SaxonC VERSION ${SAXON_VERSION})
enable_testing()

set(SaxonC_EDITION "pe" CACHE STRING "Saxon edition")

include("../cmake/libsaxonc.cmake")

add_executable(tests test/test_processor.cpp)
target_link_libraries(tests PRIVATE Catch2::Catch2WithMain)
target_link_libraries(tests PRIVATE  ${saxonc})
catch_discover_tests(tests DL_PATHS $<TARGET_RUNTIME_DLL_DIRS:tests>)

if(PROJECT_IS_TOP_LEVEL)
    # include(CPack) will happen inside the following call
    add_subdirectory(packaging)
endif()