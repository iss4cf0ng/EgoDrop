#include <iostream>
#include <arpa/inet.h>
#include <netinet/in.h>
#include <sys/socket.h>
#include <unistd.h>
#include <thread>
#include <vector>
#include <string>
#include <atomic>
#include <mutex>
#include <queue>

#include "clsPluginAPI.h"

static stAgentAPI* g_api;
static std::atomic<bool> g_stop = false;

extern "C"
{
    //Plugin meta data.
    static stPluginMeta g_meta = {
        .szName = "NetSpyder", //Name.
        .uPluginVersion = 1, //Version.
        .uAbiVersion = AGENT_ABI_VERSION, //ABI version.
        .szDescription = "Ping/TCP/SMB/ARP Scanner.", //Description.
    };

    const stPluginMeta* fnPluginGetMeta()
    {
        return &g_meta;
    }

    bool fnbPluginInit(stAgentAPI* api)
    {
        g_api = api;
        
        const char* result[] = {
            "[test] init"
        };

        g_api->send_result(g_api->ctx, 1, result);

        return true;
    }

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

    void fnPluginCleanup()
    {
        const char* result[] = {
            "buf",
        };

        g_api->send_result(g_api->ctx, 1, result);
    }
}