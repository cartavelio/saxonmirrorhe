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
find_package(Threads REQUIRED)

# Tests
add_executable(CTestXPath ../samples/cTests/testXPath.c)
target_link_libraries(CTestXPath PRIVATE ${saxonc})

add_executable(CTestXQuery ../samples/cTests/testXQuery.c)
target_link_libraries(CTestXQuery PRIVATE ${saxonc})

add_executable(CTestXslt ../samples/cTests/testXSLT.c)
target_link_libraries(CTestXslt PRIVATE ${saxonc})

add_test(NAME cXPath COMMAND CTestXPath "${CMAKE_CURRENT_SOURCE_DIR}/../samples/data" WORKING_DIRECTORY "${CMAKE_CURRENT_BINARY_DIR}")
add_test(NAME cXQuery COMMAND CTestXQuery "${CMAKE_CURRENT_SOURCE_DIR}/../samples/data" WORKING_DIRECTORY "${CMAKE_CURRENT_BINARY_DIR}")
add_test(NAME cXslt COMMAND CTestXslt "${CMAKE_CURRENT_SOURCE_DIR}/../samples/data" WORKING_DIRECTORY "${CMAKE_CURRENT_BINARY_DIR}")

add_executable(CppTestXPath ../samples/cppTests/testXPath.cpp)
target_link_libraries(CppTestXPath PRIVATE ${saxonc})

add_executable(CppTestXQuery ../samples/cppTests/testXQuery.cpp)
target_link_libraries(CppTestXQuery PRIVATE ${saxonc})

add_executable(CppTestXslt ../samples/cppTests/testXSLT30.cpp)
target_link_libraries(CppTestXslt PRIVATE ${saxonc})
target_link_libraries(CppTestXslt PRIVATE Threads::Threads)

add_test(NAME cppXPath COMMAND CppTestXPath "${CMAKE_CURRENT_SOURCE_DIR}/../samples/data" WORKING_DIRECTORY "${CMAKE_CURRENT_BINARY_DIR}")
add_test(NAME cppXQuery COMMAND CppTestXQuery "${CMAKE_CURRENT_SOURCE_DIR}/../samples/data" WORKING_DIRECTORY "${CMAKE_CURRENT_BINARY_DIR}")
add_test(NAME cppXslt COMMAND CppTestXslt "${CMAKE_CURRENT_SOURCE_DIR}/../samples/data" WORKING_DIRECTORY "${CMAKE_CURRENT_BINARY_DIR}")

set_tests_properties(
    cXPath cXQuery cXslt cppXPath cppXQuery cppXslt
    PROPERTIES
    ENVIRONMENT_MODIFICATION
        "PATH=path_list_append:$<TARGET_RUNTIME_DLL_DIRS:${saxonc}>"
)
