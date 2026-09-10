add_executable(tests test_processor.cpp)
target_link_libraries(tests PRIVATE Catch2::Catch2WithMain)
target_link_libraries(tests PRIVATE ${saxonc})
catch_discover_tests(tests DL_PATHS "$<TARGET_RUNTIME_DLL_DIRS:tests>")

add_executable(CppTestValidator ../samples/cppTests/testValidator.cpp)
target_link_libraries(CppTestValidator PRIVATE ${saxonc})
target_include_directories(CppTestValidator PRIVATE
    PATHS "${SAXON_LIBRARY_INCLUDE_DIR}" "../../samples/cppTests"
)
add_test(NAME cppValidator COMMAND CppTestValidator "${CMAKE_CURRENT_BINARY_DIR}/../samples/data" WORKING_DIRECTORY "${CMAKE_CURRENT_BINARY_DIR}/../ctest-working-dir")

set(dll_dirs "$<TARGET_RUNTIME_DLL_DIRS:CppTestValidator>")
set(dll_dirs_is_empty "$<STREQUAL:${dll_dirs},>")
set(env_op "PATH=path_list_append:")
set_tests_properties(
    cppValidator
    PROPERTIES
    ENVIRONMENT_MODIFICATION
        "$<$<NOT:${dll_dirs_is_empty}>:${env_op}$<JOIN:${dll_dirs},$<SEMICOLON>${env_op}>>"
)
