cmake_minimum_required(VERSION 3.27)

set(CMAKE_CXX_STANDARD 14)
set(CMAKE_CXX_VISIBILITY_PRESET hidden)
set(CMAKE_VISIBILITY_INLINES_HIDDEN YES)

set(saxonc_core "saxonc-core-${SaxonCCore_EDITION}")
string(TOUPPER ${SaxonCCore_EDITION} SAXON_EDITION)

add_library(${saxonc_core} SHARED
    ../cmake/ersatz/lib/graal_isolate.cpp
)
set_target_properties(${saxonc_core} PROPERTIES
    SOVERSION ${SaxonCCore_VERSION_MAJOR}
    VERSION ${SaxonCCore_VERSION}
)

target_sources(${saxonc_core}
    PUBLIC
        FILE_SET HEADERS
        BASE_DIRS
            ../cmake/ersatz/include
        FILES
            "../cmake/ersatz/include/saxonc-core-${SaxonCCore_EDITION}.h"
            "../cmake/ersatz/include/graal_isolate.h"
)
include(GNUInstallDirs)
include(CMakePackageConfigHelpers)

configure_package_config_file(../cmake/SaxonCCoreConfig.cmake.in
        ${CMAKE_CURRENT_BINARY_DIR}/SaxonCCore${SAXON_EDITION}Config.cmake
        INSTALL_DESTINATION lib/cmake/SaxonCCore${SAXON_EDITION}
)

write_basic_package_version_file(
        ${CMAKE_CURRENT_BINARY_DIR}/SaxonCCore${SAXON_EDITION}ConfigVersion.cmake
        VERSION ${SaxonCCore_VERSION}
        COMPATIBILITY SameMajorVersion
)

install(FILES ${CMAKE_CURRENT_BINARY_DIR}/SaxonCCore${SAXON_EDITION}Config.cmake
        ${CMAKE_CURRENT_BINARY_DIR}/SaxonCCore${SAXON_EDITION}ConfigVersion.cmake
        DESTINATION ${CMAKE_INSTALL_LIBDIR}/cmake/SaxonCCore${SAXON_EDITION}
)

install(TARGETS ${saxonc_core}
        EXPORT "SaxonCCore${SAXON_EDITION}"
        FILE_SET HEADERS
)
install(EXPORT "SaxonCCore${SAXON_EDITION}"
        DESTINATION ${CMAKE_INSTALL_LIBDIR}/cmake/SaxonCCore${SAXON_EDITION}
        NAMESPACE "SaxonC${SAXON_EDITION}::"
)
