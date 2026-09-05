include_guard(GLOBAL)

find_program(SPELLJAMMER_DOTNET_EXECUTABLE dotnet REQUIRED)
set(SPELLJAMMER_ROOT "${CMAKE_CURRENT_LIST_DIR}/..")

if(CMAKE_BUILD_TYPE)
    set(SPELLJAMMER_DOTNET_CONFIGURATION ${CMAKE_BUILD_TYPE})
else()
    set(SPELLJAMMER_DOTNET_CONFIGURATION Debug)
endif()

add_custom_target(SpelljammerLocalizationRuntime
    COMMAND ${CMAKE_COMMAND} -E env DOTNET_CLI_TELEMETRY_OPTOUT=1
        ${SPELLJAMMER_DOTNET_EXECUTABLE} build
        ${SPELLJAMMER_ROOT}/Source/Spelljammer.Localization/Spelljammer.Localization.csproj
        --configuration ${SPELLJAMMER_DOTNET_CONFIGURATION}
        -p:Platform=AnyCPU
        -p:RestoreIgnoreFailedSources=true
    WORKING_DIRECTORY ${SPELLJAMMER_ROOT}
    COMMENT "Building the Spelljammer localization runtime"
    VERBATIM
)

add_custom_target(SpelljammerLocalizationCompiler
    COMMAND ${CMAKE_COMMAND} -E env DOTNET_CLI_TELEMETRY_OPTOUT=1
        ${SPELLJAMMER_DOTNET_EXECUTABLE} build
        ${SPELLJAMMER_ROOT}/Tools/Spelljammer.Localization.Compiler/Spelljammer.Localization.Compiler.csproj
        --configuration ${SPELLJAMMER_DOTNET_CONFIGURATION}
        -p:Platform=AnyCPU
        -p:RestoreIgnoreFailedSources=true
    WORKING_DIRECTORY ${SPELLJAMMER_ROOT}
    COMMENT "Building the Spelljammer localization compiler"
    VERBATIM
)
add_dependencies(SpelljammerLocalizationCompiler SpelljammerLocalizationRuntime)

add_custom_target(SpelljammerLocalizationCatalogs
    COMMAND ${CMAKE_COMMAND} -E make_directory
        ${CMAKE_CURRENT_BINARY_DIR}/Localization/en-US
    COMMAND ${CMAKE_COMMAND} -E env DOTNET_CLI_TELEMETRY_OPTOUT=1
        ${SPELLJAMMER_DOTNET_EXECUTABLE} run
        --project ${SPELLJAMMER_ROOT}/Tools/Spelljammer.Localization.Compiler/Spelljammer.Localization.Compiler.csproj
        --configuration ${SPELLJAMMER_DOTNET_CONFIGURATION}
        -p:Platform=AnyCPU
        --no-build
        -- compile
        ${SPELLJAMMER_ROOT}/Content/Localization/en-US/core.sfloc.json
        ${CMAKE_CURRENT_BINARY_DIR}/Localization/en-US/core.sfloc
    BYPRODUCTS ${CMAKE_CURRENT_BINARY_DIR}/Localization/en-US/core.sfloc
    WORKING_DIRECTORY ${SPELLJAMMER_ROOT}
    COMMENT "Compiling deterministic Spelljammer localization catalogs"
    VERBATIM
)
add_dependencies(SpelljammerLocalizationCatalogs SpelljammerLocalizationCompiler)

if(BUILD_TESTING)
    add_custom_target(SpelljammerLocalizationUnitTests
        COMMAND ${CMAKE_COMMAND} -E env DOTNET_CLI_TELEMETRY_OPTOUT=1
            ${SPELLJAMMER_DOTNET_EXECUTABLE} build
            ${SPELLJAMMER_ROOT}/Tests/Spelljammer.Localization.Tests/Spelljammer.Localization.Tests.csproj
            --configuration ${SPELLJAMMER_DOTNET_CONFIGURATION}
            -p:Platform=AnyCPU
            -p:RestoreIgnoreFailedSources=true
        WORKING_DIRECTORY ${SPELLJAMMER_ROOT}
        COMMENT "Compiling the Spelljammer localization unit-test target (execution is CI-owned)"
        VERBATIM
    )
    add_dependencies(SpelljammerLocalizationUnitTests SpelljammerLocalizationCatalogs)

    add_test(NAME SpelljammerLocalizationTests
        COMMAND ${SPELLJAMMER_DOTNET_EXECUTABLE} run
            --project ${SPELLJAMMER_ROOT}/Tests/Spelljammer.Localization.Tests/Spelljammer.Localization.Tests.csproj
            --configuration ${SPELLJAMMER_DOTNET_CONFIGURATION}
            -p:Platform=AnyCPU
            --no-build
            --no-restore
    )
    set_tests_properties(SpelljammerLocalizationTests PROPERTIES
        LABELS SpelljammerLocalizationUnit
    )
endif()
