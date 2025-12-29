#pragma once

#include <iostream>
#include <unistd.h>
#include <unordered_map>
#include <memory>
#include <string>
#include <vector>

#include <sys/mman.h>

class clsPluginMgr
{
public:
    struct stAgentAPI
    {
        bool bAllow_fs;
        bool bAllow_net;
    };

    struct stPluginInstance
    {
        std::string szName;
        void* handle;
        int fd;
        bool (*init)(stAgentAPI*);
        int (*run)(const char *);
        void (*cleanup)();
    };

private:
    std::unordered_map<std::string, std::unique_ptr<stPluginInstance>> m_plugins;
    stAgentAPI m_api;

public:
    bool fnLoadPlugin(const std::string& szName, const std::vector<uint8_t>& abBytes)
    {
        if (m_plugins.count(szName))
            return false;

        int fd = memfd_create(szName.c_str(), MFD_CLOEXEC);
        write(fd, abBytes.data(), abBytes.size());
        lseek(fd, 0, SEEK_SET);

        char path[64];
        
    }

    bool fnRunPlugin(const std::string& szName, const std::string& args)
    {

    }

    bool fnUnloadPlugin(const std::string& szName)
    {

    }

    void fnListPlugins() const
    {

    }
};