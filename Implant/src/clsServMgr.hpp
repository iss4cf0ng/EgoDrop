#pragma once

#include <sstream>
#include <vector>
#include <string>
#include <array>
#include <memory>
#include <map>
#include <cstdlib>

#include "clsTools.hpp"
#include "clsEZData.hpp"

class clsServMgr
{
public:
    struct stServiceInfo
    {
        std::string szName;
        std::string szLoadState;
        std::string szActiveState;
        std::string szSubState;
        std::string szEnabled;
        int nMainPid;
        std::string szExecStart;
        std::string szDescription;
    };

public:
    clsServMgr() = default;

    ~clsServMgr() = default;

    stServiceInfo fnGetServiceInfo(const std::string& szName)
    {
        stServiceInfo info {};
        info.szName = szName;

        std::string cmd = "systemctl show " + szName + " --no-pager";
        FILE* fp = popen(cmd.c_str(), "r");
        if (!fp) return info;

        char line[512];
        while (fgets(line, sizeof(line), fp))
        {
            std::string s(line);

            auto getVal = [&](const std::string& key) -> std::string {
                if (s.rfind(key + "=", 0) == 0)
                    return s.substr(key.size() + 1);
                return "";
            };

            if (!getVal("LoadState").empty())
                info.szLoadState = getVal("LoadState");
            if (!getVal("ActiveState").empty())
                info.szActiveState = getVal("ActiveState");
            if (!getVal("SubState").empty())
                info.szSubState = getVal("SubState");
            if (!getVal("MainPID").empty())
                info.nMainPid = std::stoi(getVal("MainPID"));
            if (!getVal("ExecStart").empty())
                info.szExecStart = getVal("ExecStart");
            if (!getVal("Description").empty())
                info.szDescription = getVal("Description");
            if (!getVal("UnitFileState").empty())
                info.szEnabled = getVal("UnitFileState");
        }

        pclose(fp);
        return info;
    }

    std::vector<stServiceInfo> fnGetAllServices()
    {
        std::vector<stServiceInfo> services;

        FILE* fp = popen("systemctl list-unit-files --type=service --no-pager", "r");
        if (!fp)
            return services;

        char line[256];

        while (fgets(line, sizeof(line), fp))
        {
            std::string s(line);

            if (s.find(".service") == std::string::npos)
                continue;

            std::istringstream iss(s);
            std::string serviceName;
            iss >> serviceName;

            if (serviceName.find(".service") == std::string::npos)
                continue;

            services.push_back(fnGetServiceInfo(serviceName));
        }

        pclose(fp);

        return services;
    }
};