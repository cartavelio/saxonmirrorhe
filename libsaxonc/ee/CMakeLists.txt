cmake_minimum_required(VERSION 3.27)

project(SaxonCEE VERSION ${SAXON_VERSION})
enable_testing()

set(SaxonC_EDITION "ee" CACHE STRING "Saxon edition")
set(SaxonC_VERSION_MAJOR "${SaxonCEE_VERSION_MAJOR}")
set(SaxonC_VERSION "${SaxonCEE_VERSION}")

include("../cmake/libsaxonc.cmake")

add_executable(Validate src/bin/Validate.c)
target_link_libraries(Validate PRIVATE ${saxonc})
target_link_libraries(Validate PRIVATE "${SAXON_LIBRARY}")
target_include_directories(Validate PRIVATE
    PATHS "${SAXON_LIBRARY_INCLUDE_DIR}"
)

add_executable(CppTestValidator samples/cppTests/testValidator.cpp)
target_link_libraries(CppTestValidator PRIVATE ${saxonc})
target_include_directories(CppTestValidator PRIVATE
    PATHS "${SAXON_LIBRARY_INCLUDE_DIR}" "../samples/cppTests"
)
add_test(NAME cppValidator COMMAND CppTestValidator)

if(PROJECT_IS_TOP_LEVEL)
    # include(CPack) will happen inside the following call
    add_subdirectory(packaging)
endif()