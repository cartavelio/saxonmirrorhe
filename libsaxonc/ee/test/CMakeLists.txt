add_executable(tests test_processor.cpp)
target_link_libraries(tests PRIVATE Catch2::Catch2WithMain)
target_link_libraries(tests PRIVATE ${saxonc})
catch_discover_tests(tests DL_PATHS "$<TARGET_RUNTIME_DLL_DIRS:${saxonc}>")

include(CMakePrintHelpers)
cmake_print_variables(CMAKE_CURRENT_BINARY_DIR)

add_executable(CppTestValidator ../samples/cppTests/testValidator.cpp)
target_link_libraries(CppTestValidator PRIVATE ${saxonc})
target_include_directories(CppTestValidator PRIVATE
    PATHS "${SAXON_LIBRARY_INCLUDE_DIR}" "../../samples/cppTests"
)
add_test(NAME cppValidator COMMAND CppTestValidator "${CMAKE_CURRENT_BINARY_DIR}/../samples/data" WORKING_DIRECTORY "${CMAKE_CURRENT_BINARY_DIR}/../ctest-working-dir")
set_tests_properties(
    cppValidator
    PROPERTIES
    ENVIRONMENT_MODIFICATION
        "PATH=path_list_append:$<TARGET_RUNTIME_DLL_DIRS:${saxonc}>"
)
