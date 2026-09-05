find_program(SPRITEFORGE_DOTNET_EXECUTABLE dotnet REQUIRED)

if(CMAKE_BUILD_TYPE)
    set(SPRITEFORGE_DOTNET_CONFIGURATION ${CMAKE_BUILD_TYPE})
else()
    set(SPRITEFORGE_DOTNET_CONFIGURATION Debug)
endif()

add_custom_target(GameLocalizationRuntime
    COMMAND ${CMAKE_COMMAND} -E env DOTNET_CLI_TELEMETRY_OPTOUT=1
        ${SPRITEFORGE_DOTNET_EXECUTABLE} build
        ${CMAKE_CURRENT_SOURCE_DIR}/Localization/SpriteForge.Game.Localization.csproj
        --configuration ${SPRITEFORGE_DOTNET_CONFIGURATION}
        -p:Platform=AnyCPU
        -p:RestoreIgnoreFailedSources=true
    WORKING_DIRECTORY ${CMAKE_CURRENT_SOURCE_DIR}
    COMMENT "Building the Game localization runtime"
    VERBATIM
)

add_custom_target(GameLocalizationCompiler
    COMMAND ${CMAKE_COMMAND} -E env DOTNET_CLI_TELEMETRY_OPTOUT=1
        ${SPRITEFORGE_DOTNET_EXECUTABLE} build
        ${CMAKE_CURRENT_SOURCE_DIR}/Tools/Localization/SpriteForge.Localization.Compiler.csproj
        --configuration ${SPRITEFORGE_DOTNET_CONFIGURATION}
        -p:Platform=AnyCPU
        -p:RestoreIgnoreFailedSources=true
    WORKING_DIRECTORY ${CMAKE_CURRENT_SOURCE_DIR}
    COMMENT "Building the Game localization compiler"
    VERBATIM
)
add_dependencies(GameLocalizationCompiler GameLocalizationRuntime)

add_custom_target(GameLocalizationCatalogs
    COMMAND ${CMAKE_COMMAND} -E make_directory
        ${CMAKE_CURRENT_BINARY_DIR}/Localization/en-US
    COMMAND ${CMAKE_COMMAND} -E env DOTNET_CLI_TELEMETRY_OPTOUT=1
        ${SPRITEFORGE_DOTNET_EXECUTABLE} run
        --project ${CMAKE_CURRENT_SOURCE_DIR}/Tools/Localization/SpriteForge.Localization.Compiler.csproj
        --configuration ${SPRITEFORGE_DOTNET_CONFIGURATION}
        -p:Platform=AnyCPU
        --no-build
        -- compile
        ${CMAKE_CURRENT_SOURCE_DIR}/Content/Localization/en-US/core.sfloc.json
        ${CMAKE_CURRENT_BINARY_DIR}/Localization/en-US/core.sfloc
    BYPRODUCTS ${CMAKE_CURRENT_BINARY_DIR}/Localization/en-US/core.sfloc
    WORKING_DIRECTORY ${CMAKE_CURRENT_SOURCE_DIR}
    COMMENT "Compiling deterministic Game localization catalogs"
    VERBATIM
)
add_dependencies(GameLocalizationCatalogs GameLocalizationCompiler)

if(BUILD_TESTING)
    add_custom_target(GameLocalizationUnitTests
        COMMAND ${CMAKE_COMMAND} -E env DOTNET_CLI_TELEMETRY_OPTOUT=1
            ${SPRITEFORGE_DOTNET_EXECUTABLE} build
            ${CMAKE_CURRENT_SOURCE_DIR}/Tests/Localization/SpriteForge.Game.Localization.Tests.csproj
            --configuration ${SPRITEFORGE_DOTNET_CONFIGURATION}
            -p:Platform=AnyCPU
            -p:RestoreIgnoreFailedSources=true
        WORKING_DIRECTORY ${CMAKE_CURRENT_SOURCE_DIR}
        COMMENT "Compiling the Game localization unit-test target (execution is CI-owned)"
        VERBATIM
    )
    add_dependencies(GameLocalizationUnitTests GameLocalizationCatalogs)

    add_test(NAME GameLocalizationTests
        COMMAND ${SPRITEFORGE_DOTNET_EXECUTABLE} run
            --project ${CMAKE_CURRENT_SOURCE_DIR}/Tests/Localization/SpriteForge.Game.Localization.Tests.csproj
            --configuration ${SPRITEFORGE_DOTNET_CONFIGURATION}
            -p:Platform=AnyCPU
            --no-build
            --no-restore
    )
    set_tests_properties(GameLocalizationTests PROPERTIES
        LABELS GameLocalizationUnit
    )
endif()
