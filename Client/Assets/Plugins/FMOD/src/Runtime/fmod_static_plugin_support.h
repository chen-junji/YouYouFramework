// Support code for static plugin registration code in generated file fmod_register_static_plugins.cpp
// Needs the following macros defined before inclusion to select the correct behaviour:
// * FMOD_LINKAGE_STATIC
// * UNITY_2019_1_OR_NEWER

#include "il2cpp-class-internals.h" // So that il2cpp-codegen.h compiles
#include "codegen/il2cpp-codegen.h" // For DEFAULT_CALL definition

struct FMOD_SYSTEM;
struct FMOD_DSP_DESCRIPTION;

typedef unsigned int (DEFAULT_CALL *RegisterDSPFunction)(FMOD_SYSTEM *, const FMOD_DSP_DESCRIPTION *, void *);

static RegisterDSPFunction sRegisterDSP = nullptr;

#if FMOD_LINKAGE_STATIC

extern "C" unsigned int FMOD5_System_RegisterDSP(FMOD_SYSTEM *system, const FMOD_DSP_DESCRIPTION *description, void *handle);

void InitializeRegisterDSP(const char *)
{
    sRegisterDSP = FMOD5_System_RegisterDSP;
}

#else

#include "utils/StringUtils.h"
#include "os/LibraryLoader.h"
#if !UNITY_2020_1_OR_NEWER
    #include "vm/LibraryLoader.h"
#endif

#if UNITY_2019_1_OR_NEWER
    #include "utils/StringViewUtils.h"
#endif

void InitializeRegisterDSP(const char *coreLibraryName)
{
    using il2cpp::utils::StringUtils;
#if UNITY_2019_1_OR_NEWER
    using il2cpp::utils::StringViewUtils;
#endif

    Il2CppNativeString libraryName(StringUtils::Utf8ToNativeString(coreLibraryName));

#if UNITY_2020_1_OR_NEWER
    // il2cpp::vm::LibraryLoader functionality has been merged into il2cpp::os::LibraryLoader
    void *library = il2cpp::os::LibraryLoader::LoadDynamicLibrary(StringViewUtils::StringToStringView(libraryName));
#elif UNITY_2019_1_OR_NEWER
    // il2cpp::vm::LibraryLoader::LoadLibrary has been renamed to LoadDynamicLibrary
    void *library = il2cpp::vm::LibraryLoader::LoadDynamicLibrary(StringViewUtils::StringToStringView(libraryName));
#else
    // il2cpp::vm::LibraryLoader::LoadLibrary makes the library path absolute if necessary
    void *library = il2cpp::vm::LibraryLoader::LoadLibrary(libraryName);
#endif

    sRegisterDSP = (RegisterDSPFunction)il2cpp::os::LibraryLoader::GetFunctionPointer(library, "FMOD_System_RegisterDSP");
}

#endif

