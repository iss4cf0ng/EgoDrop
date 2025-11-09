

#include <sstream>
#include <vector>
#include <string>
#include <array>
#include <memory>
#include <map>

#include "clsTools.hpp"

class clsServMgr
{
public:
    struct stServiceInfo
    {
        std::string szName;
        std::string szDescription;
        std::string szLoadState;
        std::string szActiveState;
        std::string szSubState;
        std::string szMainPid;
        std::string szExecStart;
    };

public:
    clsServMgr()
    {

    }

    ~clsServMgr() = default;

    std::vector<stServiceInfo> fnGetServices()
    {
        std::vector<stServiceInfo> services;
        std::string szList = clsTools::fnExec("system list-units --type=service --all --no-legend --no-pager");
        
        std::istringstream iss(szList);
        std::string szLine;

        while (std::getline(iss, szLine))
        {
            std::istringstream ls(szLine);
            stServiceInfo info;
            ls >> info.szName >> info.szLoadState >> info.szActiveState >> info.szSubState;

            std::getline(ls, info.szDescription);
            if (info.szName.empty())
                continue;

            std::string szDetail = clsTools::fnExec("systemctl show " + info.szName + " --no-pager");
            std::istringstream ds(szDetail);
            std::string szKV;

            while (std::getline(ds, szKV))
            {
                auto pos = szKV.find('=');
                if (pos == std::string::npos)
                    continue;

                std::string szKey = szKV.substr(0, pos);
                std::string szVal = szKV.substr(pos + 1);

                if (szKey == "MainPID")
                    info.szMainPid = szVal;
                else if (szKey == "ExecStart")
                    info.szExecStart = szVal;
            }

            services.push_back(info);
        }

        return services;
    }
};