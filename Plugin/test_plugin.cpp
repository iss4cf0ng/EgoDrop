#include <cstdio>

#include "clsPluginAPI.h"

static stAgentAPI* g_api;
static stPluginMeta g_meta =
{
    .szName = "demo",
    .uPluginVersion = 1,
    .uAbiVersion = AGENT_ABI_VERSION,
    .szDescription = "test plugin"
};

const stPluginMeta* fnPluginGetMeta()
{
    return &g_meta;
}

extern "C"
bool fnbPluginInit(stAgentAPI* api)
{
    g_api = api;

    const char* result[] = {
        "[test] init"
    };

    g_api->send_result(g_api->ctx, 1, result);

    return true;
}

extern "C"
int fnPluginRun(int argc, const char** argv)
{
    char buf[256];
    snprintf(buf, sizeof(buf), "[demo] run args=%s", argv ? *argv : "(null)");

    const char* result[] = {
        "buf",
    };

    g_api->send_result(g_api->ctx, 1, result);

    return 0;
}

extern "C"
void fnPluginCleanup()
{
    const char* result[] = {
        "buf",
    };

    g_api->send_result(g_api->ctx, 1, result);
}