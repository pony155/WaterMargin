# Build integration

The repository root is a standalone CMake project for Game-owned tooling. The
root `CMakeLists.txt` enables CTest and includes the focused calendar and
localization declarations.

`Calendar.cmake` declares these targets:

| Target | Purpose |
| --- | --- |
| `WaterMarginCalendarRuntime` | Compile the ancient Chinese calendar runtime and vendored astronomy dependency. |
| `WaterMarginCalendarUnitTests` | Compile, but do not execute, the calendar test program. |

`Localization.cmake` declares these targets:

| Target | Purpose |
| --- | --- |
| `WaterMarginLocalizationRuntime` | Compile the localization runtime assembly. |
| `WaterMarginLocalizationCompiler` | Compile the source-catalog compiler. |
| `WaterMarginLocalizationCatalogs` | Compile the authored `en-US` catalog. |
| `WaterMarginLocalizationUnitTests` | Compile, but do not execute, the test program. |

Configure and compile runtime test targets from the repository root:

```powershell
cmake -S . -B cmake-build-debug -G Ninja -DCMAKE_BUILD_TYPE=Debug
cmake --build cmake-build-debug --target WaterMarginCalendarUnitTests
cmake --build cmake-build-debug --target WaterMarginLocalizationUnitTests
```

Unit-test execution remains user/CI-owned under the repository policy.
