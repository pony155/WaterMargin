include_guard(GLOBAL)

find_program(WATERMARGIN_DOTNET_EXECUTABLE dotnet REQUIRED)
set(WATERMARGIN_ROOT "${CMAKE_CURRENT_LIST_DIR}/..")

if(CMAKE_BUILD_TYPE)
    set(WATERMARGIN_DOTNET_CONFIGURATION ${CMAKE_BUILD_TYPE})
else()
    set(WATERMARGIN_DOTNET_CONFIGURATION Debug)
endif()

add_custom_target(WaterMarginLocalizationRuntime
    COMMAND ${CMAKE_COMMAND} -E env DOTNET_CLI_TELEMETRY_OPTOUT=1
        ${WATERMARGIN_DOTNET_EXECUTABLE} build
        ${WATERMARGIN_ROOT}/Source/WaterMargin.Localization/WaterMargin.Localization.csproj
        --configuration ${WATERMARGIN_DOTNET_CONFIGURATION}
        -p:Platform=AnyCPU
        -p:RestoreIgnoreFailedSources=true
    WORKING_DIRECTORY ${WATERMARGIN_ROOT}
    COMMENT "Building the WaterMargin localization runtime"
    VERBATIM
)

add_custom_target(WaterMarginLocalizationCompiler
    COMMAND ${CMAKE_COMMAND} -E env DOTNET_CLI_TELEMETRY_OPTOUT=1
        ${WATERMARGIN_DOTNET_EXECUTABLE} build
        ${WATERMARGIN_ROOT}/Tools/WaterMargin.Localization.Compiler/WaterMargin.Localization.Compiler.csproj
        --configuration ${WATERMARGIN_DOTNET_CONFIGURATION}
        -p:Platform=AnyCPU
        -p:RestoreIgnoreFailedSources=true
    WORKING_DIRECTORY ${WATERMARGIN_ROOT}
    COMMENT "Building the WaterMargin localization compiler"
    VERBATIM
)
add_dependencies(WaterMarginLocalizationCompiler WaterMarginLocalizationRuntime)

add_custom_target(WaterMarginLocalizationCatalogs
    COMMAND ${CMAKE_COMMAND} -E make_directory
        ${CMAKE_CURRENT_BINARY_DIR}/Localization/en-US
    COMMAND ${CMAKE_COMMAND} -E env DOTNET_CLI_TELEMETRY_OPTOUT=1
        ${WATERMARGIN_DOTNET_EXECUTABLE} run
        --project ${WATERMARGIN_ROOT}/Tools/WaterMargin.Localization.Compiler/WaterMargin.Localization.Compiler.csproj
        --configuration ${WATERMARGIN_DOTNET_CONFIGURATION}
        -p:Platform=AnyCPU
        --no-build
        -- compile
        ${WATERMARGIN_ROOT}/Content/Localization/en-US/core.sfloc.json
        ${CMAKE_CURRENT_BINARY_DIR}/Localization/en-US/core.sfloc
    BYPRODUCTS ${CMAKE_CURRENT_BINARY_DIR}/Localization/en-US/core.sfloc
    WORKING_DIRECTORY ${WATERMARGIN_ROOT}
    COMMENT "Compiling deterministic WaterMargin localization catalogs"
    VERBATIM
)
add_dependencies(WaterMarginLocalizationCatalogs WaterMarginLocalizationCompiler)

if(BUILD_TESTING)
    add_custom_target(WaterMarginLocalizationUnitTests
        COMMAND ${CMAKE_COMMAND} -E env DOTNET_CLI_TELEMETRY_OPTOUT=1
            ${WATERMARGIN_DOTNET_EXECUTABLE} build
            ${WATERMARGIN_ROOT}/Tests/WaterMargin.Localization.Tests/WaterMargin.Localization.Tests.csproj
            --configuration ${WATERMARGIN_DOTNET_CONFIGURATION}
            -p:Platform=AnyCPU
            -p:RestoreIgnoreFailedSources=true
        WORKING_DIRECTORY ${WATERMARGIN_ROOT}
        COMMENT "Compiling the WaterMargin localization unit-test target (execution is CI-owned)"
        VERBATIM
    )
    add_dependencies(WaterMarginLocalizationUnitTests WaterMarginLocalizationCatalogs)

    add_test(NAME WaterMarginLocalizationTests
        COMMAND ${WATERMARGIN_DOTNET_EXECUTABLE} run
            --project ${WATERMARGIN_ROOT}/Tests/WaterMargin.Localization.Tests/WaterMargin.Localization.Tests.csproj
            --configuration ${WATERMARGIN_DOTNET_CONFIGURATION}
            -p:Platform=AnyCPU
            --no-build
            --no-restore
    )
    set_tests_properties(WaterMarginLocalizationTests PROPERTIES
        LABELS WaterMarginLocalizationUnit
    )
endif()
