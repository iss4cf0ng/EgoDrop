#pragma once

#include <filesystem>
#include <fstream>
#include <string>
#include <iostream>
#include <vector>
#include <csignal>
#include <unistd.h>
#include <sys/types.h>

#include "clsTools.hpp"

class clsProcMgr
{
public:
    struct stProcInfo
    {
        uint nPID;
        uint nPpid;

        std::string szUser;
        std::string szName;
        std::string szCmdLine;
        std::string szExePath;
        
        long rss_kb;
        long vms_kb;
    };

public:
    clsProcMgr()
    {

    }

    ~clsProcMgr() = default;

    std::vector<stProcInfo> fnGetProcesses()
    {
        std::vector<stProcInfo> processes;
        for (const auto& entry : std::filesystem::directory_iterator("/proc"))
        {
            if (!entry.is_directory())
                continue;

            
        }

        return processes;
    }

    RETMSG fnKillProcess(pid_t nPid)
    {
        if (kill(nPid, SIGKILL) == 0)
        {
            return { 1, "" };
        }
        else
        {
            return { 0, "" };
        }
    }

    RETMSG fnStopProcess(pid_t nPid)
    {

    }

    RETMSG fnContinueProcess(pid_t nPid)
    {
        
    }
};