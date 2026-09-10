cmake_minimum_required(VERSION 3.27)

set(CMAKE_CXX_STANDARD 14)
set(CMAKE_CXX_VISIBILITY_PRESET hidden)
set(CMAKE_VISIBILITY_INLINES_HIDDEN YES)

string(TOUPPER ${SaxonC_EDITION} SAXON_EDITION)

# set(SaxonCCore_PackagePrefix CACHE PATH) Path to Graal native-image output; must be set on command line with -D
# set(SaxonCCore_IncludePath CACHE PATH) Path to Graal native-image output; must be set on command line with -D
# set(SAXON_VERSION CACHE STRING) Main Saxon J version; must be set on command line with -D

# Always use `lib` instead of sometimes `lib64`
set(SAXON_CMAKE_INSTALL_LIBDIR "lib")

Include(FetchContent)
FetchContent_Declare(
        Catch2
        GIT_REPOSITORY https://github.com/catchorg/Catch2.git
        GIT_TAG        v3.8.1 # or a later release
)
FetchContent_MakeAvailable(Catch2)
list(APPEND CMAKE_MODULE_PATH ${catch2_SOURCE_DIR}/extras)
include(CTest)
include(Catch)

set(saxonc "saxonc-${SaxonC_EDITION}")

set(THREADS_PREFER_PTHREAD_FLAG ON)
find_package(Threads REQUIRED)

configure_file(../src/lib/saxonc_version.c.in saxonc_version.c @ONLY)
add_library(${saxonc} SHARED
    ${CMAKE_CURRENT_BINARY_DIR}/saxonc_version.c
    ../src/lib/CPPUserFunctionHandler.cpp
    ../src/lib/DocumentBuilder.cpp
    ../src/lib/SaxonApiException.cpp
    ../src/lib/SaxonCGlue.c
    ../src/lib/SaxonCProcessor.c
    ../src/lib/SaxonCXPath.c
    ../src/lib/SaxonProcessor.cpp
    ../src/lib/SchemaValidator.cpp
    ../src/lib/XPathProcessor.cpp
    ../src/lib/XQueryProcessor.cpp
    ../src/lib/XdmArray.cpp
    ../src/lib/XdmAtomicValue.cpp
    ../src/lib/XdmFunctionItem.cpp
    ../src/lib/XdmItem.cpp
    ../src/lib/XdmMap.cpp
    ../src/lib/XdmNode.cpp
    ../src/lib/XdmValue.cpp
    ../src/lib/Xslt30Processor.cpp
    ../src/lib/XsltExecutable.cpp
)
set_target_properties(${saxonc} PROPERTIES
    SOVERSION ${SaxonC_VERSION_MAJOR}
    VERSION ${SaxonC_VERSION}
)
target_link_libraries(${saxonc} PRIVATE Threads::Threads)

include(GenerateExportHeader)
generate_export_header(${saxonc} BASE_NAME saxonc)
target_sources(${saxonc}
    PUBLIC
        FILE_SET common_headers
        TYPE HEADERS
        BASE_DIRS
            ../src/include
        FILES
            ../src/include/saxonc/saxonc_version.h
            ../src/include/saxonc/DocumentBuilder.h
            ../src/include/saxonc/SaxonApiException.h
            ../src/include/saxonc/SaxonCGlue.h
            ../src/include/saxonc/SaxonCProcessor.h
            ../src/include/saxonc/SaxonCXPath.h
            ../src/include/saxonc/SaxonProcessor.h
            ../src/include/saxonc/SchemaValidator.h
            ../src/include/saxonc/UserFunctionHandler.h
            ../src/include/saxonc/XPathProcessor.h
            ../src/include/saxonc/XQueryProcessor.h
            ../src/include/saxonc/XdmArray.h
            ../src/include/saxonc/XdmAtomicValue.h
            ../src/include/saxonc/XdmFunctionItem.h
            ../src/include/saxonc/XdmItem.h
            ../src/include/saxonc/XdmMap.h
            ../src/include/saxonc/XdmNode.h
            ../src/include/saxonc/XdmValue.h
            ../src/include/saxonc/Xslt30Processor.h
            ../src/include/saxonc/XsltExecutable.h
)
target_sources(${saxonc}
    PUBLIC
        FILE_SET edition_headers
        TYPE HEADERS
        BASE_DIRS
            src/include
        FILES
            src/include/saxonc/saxonc-core.h
)
target_sources(${saxonc}
    PUBLIC
        FILE_SET export_header
        TYPE HEADERS
        BASE_DIRS
            ${CMAKE_CURRENT_BINARY_DIR}
        FILES
            ${CMAKE_CURRENT_BINARY_DIR}/saxonc_export.h
)
set(SaxonCCore${SAXON_EDITION}_DIR "${SaxonCCore_PackageDir}/${SAXON_CMAKE_INSTALL_LIBDIR}/cmake/SaxonCCore${SAXON_EDITION}")

find_package(SaxonCCore${SAXON_EDITION} ${SaxonC_VERSION} EXACT REQUIRED)

target_link_libraries(${saxonc} PUBLIC SaxonC${SAXON_EDITION}::saxonc-core-${SaxonC_EDITION})

add_executable(Transform ../src/bin/Transform.c)
target_link_libraries(Transform PRIVATE ${saxonc})

add_executable(Query ../src/bin/Query.c)
target_link_libraries(Query PRIVATE ${saxonc})

# Tests
add_executable(CTestXPath ../samples/cTests/testXPath.c)
target_link_libraries(CTestXPath PRIVATE ${saxonc})

add_executable(CTestXQuery ../samples/cTests/testXQuery.c)
target_link_libraries(CTestXQuery PRIVATE ${saxonc})

add_executable(CTestXslt ../samples/cTests/testXSLT.c)
target_link_libraries(CTestXslt PRIVATE ${saxonc})

add_test(NAME cXPath COMMAND CTestXPath "${CMAKE_CURRENT_SOURCE_DIR}/../samples/data" WORKING_DIRECTORY "${CMAKE_CURRENT_BINARY_DIR}/ctest-working-dir")
add_test(NAME cXQuery COMMAND CTestXQuery "${CMAKE_CURRENT_SOURCE_DIR}/../samples/data" WORKING_DIRECTORY "${CMAKE_CURRENT_BINARY_DIR}/ctest-working-dir")
add_test(NAME cXslt COMMAND CTestXslt "${CMAKE_CURRENT_SOURCE_DIR}/../samples/data" WORKING_DIRECTORY "${CMAKE_CURRENT_BINARY_DIR}/ctest-working-dir")

add_executable(CppTestXPath ../samples/cppTests/testXPath.cpp)
target_link_libraries(CppTestXPath PRIVATE ${saxonc})

add_executable(CppTestXQuery ../samples/cppTests/testXQuery.cpp)
target_link_libraries(CppTestXQuery PRIVATE ${saxonc})

add_executable(CppTestXslt ../samples/cppTests/testXSLT30.cpp)
target_link_libraries(CppTestXslt PRIVATE ${saxonc})
target_link_libraries(CppTestXslt PRIVATE Threads::Threads)

add_test(NAME cppXPath COMMAND CppTestXPath "${CMAKE_CURRENT_SOURCE_DIR}/../samples/data" WORKING_DIRECTORY "${CMAKE_CURRENT_BINARY_DIR}/ctest-working-dir")
add_test(NAME cppXQuery COMMAND CppTestXQuery "${CMAKE_CURRENT_SOURCE_DIR}/../samples/data" WORKING_DIRECTORY "${CMAKE_CURRENT_BINARY_DIR}/ctest-working-dir")
add_test(NAME cppXslt COMMAND CppTestXslt "${CMAKE_CURRENT_SOURCE_DIR}/../samples/data" WORKING_DIRECTORY "${CMAKE_CURRENT_BINARY_DIR}/ctest-working-dir")

set_tests_properties(
    cXPath cXQuery cXslt cppXPath cppXQuery cppXslt
    PROPERTIES
    ENVIRONMENT_MODIFICATION
        "PATH=path_list_append:$<TARGET_RUNTIME_DLL_DIRS:${saxonc}>"
)


include(GNUInstallDirs)
include(CMakePackageConfigHelpers)

configure_package_config_file(../cmake/SaxonCConfig.cmake.in
       ${CMAKE_CURRENT_BINARY_DIR}/SaxonC${SAXON_EDITION}Config.cmake
       INSTALL_DESTINATION ${SAXON_CMAKE_INSTALL_LIBDIR}/cmake/SaxonC${SAXON_EDITION}
)
write_basic_package_version_file(
        ${CMAKE_CURRENT_BINARY_DIR}/SaxonC${SAXON_EDITION}ConfigVersion.cmake
        VERSION ${SaxonC_VERSION}
        COMPATIBILITY SameMajorVersion
)

install(TARGETS ${saxonc}
        EXPORT "SaxonC${SAXON_EDITION}"
        LIBRARY DESTINATION ${SAXON_CMAKE_INSTALL_LIBDIR}
        FILE_SET common_headers
        FILE_SET edition_headers
        FILE_SET export_header
)

install(TARGETS Transform Query)

install(EXPORT "SaxonC${SAXON_EDITION}"
        DESTINATION ${SAXON_CMAKE_INSTALL_LIBDIR}/cmake/SaxonC${SAXON_EDITION}
        NAMESPACE "SaxonC${SAXON_EDITION}::"
)

install(FILES ${CMAKE_CURRENT_BINARY_DIR}/SaxonC${SAXON_EDITION}Config.cmake
        ${CMAKE_CURRENT_BINARY_DIR}/SaxonC${SAXON_EDITION}ConfigVersion.cmake
        DESTINATION ${SAXON_CMAKE_INSTALL_LIBDIR}/cmake/SaxonC${SAXON_EDITION}
)

# Files from saxonc-core
install(FILES
        ${SaxonCCore_PackageDir}/include/graal_isolate.h
        ${SaxonCCore_PackageDir}/include/saxonc-core-${SaxonC_EDITION}.h
        TYPE INCLUDE
)

install(DIRECTORY ${SaxonCCore_PackageDir}/${SAXON_CMAKE_INSTALL_LIBDIR}/
        DESTINATION ${SAXON_CMAKE_INSTALL_LIBDIR}
        USE_SOURCE_PERMISSIONS
        FILES_MATCHING
        PATTERN cmake EXCLUDE
        PATTERN *.dylib
        PATTERN *.lib
        PATTERN *.so
        PATTERN *.so.*
)
install(DIRECTORY ${SaxonCCore_PackageDir}/bin/
        TYPE BIN
        USE_SOURCE_PERMISSIONS
        FILES_MATCHING
        PATTERN cmake EXCLUDE
        PATTERN *.dll
)

install(DIRECTORY ${SaxonCCore_PackageDir}/${SAXON_CMAKE_INSTALL_LIBDIR}/cmake/SaxonCCore${SAXON_EDITION}/
        DESTINATION ${SAXON_CMAKE_INSTALL_LIBDIR}/cmake/SaxonC${SAXON_EDITION}
        PATTERN *Config.cmake EXCLUDE
        PATTERN *ConfigVersion.cmake EXCLUDE
)

#set(CMAKE_CXX_FLAGS "${CMAKE_CXX_FLAGS} -fsanitize=address")
