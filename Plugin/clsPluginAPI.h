#pragma once

#include <cstdint>
#include <string>
#include <vector>

#define AGENT_ABI_VERSION 1 //Appication Binary Interface.

extern "C" 
{
    struct stPluginMeta
    {
        const char* szName;
        uint32_t uPluginVersion;
        uint32_t uAbiVersion;
        const char* szDescription;
    };

    struct stAgentAPI
    {
        void* ctx;
        void (*send_result)(void* ctx, int argc, const char **argv);
    };

    const stPluginMeta* fnPluginGetMeta();

    bool fnbPluginInit(stAgentAPI* api);
    int fnPluginRun(int argc, const char** argv);
    void fnPluginCleanup();
}