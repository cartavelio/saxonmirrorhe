cmake_minimum_required(VERSION 3.27)
project(SaxonC VERSION ${SAXON_VERSION})
enable_testing()

set(SaxonC_EDITION "he" CACHE STRING "Saxon edition")
include("../cmake/common-vars.cmake")

configure_file(${SAXONC_SRC_DIR}/lib/saxonc_version.c.in saxonc_version.c @ONLY)
include("../cmake/libsaxonc.cmake")
if(PROJECT_IS_TOP_LEVEL)
    add_subdirectory("../test" "test")
    add_subdirectory("../src/bin" "bin")
    add_subdirectory("../packaging" "packaging")
endif()

include(CMakePrintHelpers)
cmake_print_variables(CMAKE_SYSTEM_PROCESSOR)

#add_executable(tests test/test_processor.cpp)
#target_link_libraries(tests PRIVATE Catch2::Catch2WithMain)
#target_link_libraries(tests PRIVATE ${saxonc})
#catch_discover_tests(tests DL_PATHS "$<TARGET_RUNTIME_DLL_DIRS:${saxonc}>")

#if(PROJECT_IS_TOP_LEVEL)
#    # include(CPack) will happen inside the following call
#    add_subdirectory(packaging)
#endif()