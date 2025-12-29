#pragma once

#include <filesystem>
#include <fstream>
#include <string>
#include <iostream>
#include <vector>
#include <csignal>
#include <unistd.h>
#include <sys/types.h>
#include <sstream>

#include "clsTools.hpp"

class clsProcMgr
{
public:
    struct stProcInfo
    {
        uint nPID = 0;
        uint nPpid = 0;

        std::string szName;
        std::string szUser;
        std::string szCmdLine;
        std::string szExePath;
        
        long rss_kb;
        long vms_kb;

        std::vector<uint> vChildren;
    };

public:
    clsProcMgr() = default;

    ~clsProcMgr() = default;

    std::vector<stProcInfo> fnGetProcesses()
    {
        std::vector<stProcInfo> processes;
        std::unordered_map<uint, size_t> pidIndex;
        std::unordered_map<uint, std::vector<uint>> ppidMap;

        for (const auto& entry : std::filesystem::directory_iterator("/proc"))
        {
            if (!entry.is_directory())
                continue;

            std::string szPid = entry.path().filename().string();
            if (!fnbIsNumber(szPid))
                continue;

            stProcInfo pInfo;
            pInfo.nPID = std::stoul(szPid);

            std::ifstream status(entry.path() / "status");
            std::string line;
            uint_fast8_t uid = 0;

            //Status.
            while (std::getline(status, line))
            {
                if (line.rfind("Name:", 0) == 0)
                    pInfo.szName = line.substr(6);
                else if (line.rfind("Pid:", 0) == 0)
                    pInfo.nPID = std::stoul(line.substr(5));
                else if (line.rfind("PPid:", 0) == 0)
                    pInfo.nPpid = std::stoul(line.substr(6));
                else if (line.rfind("VmRSS:", 0) == 0)
                    pInfo.rss_kb = std::stol(line.substr(7));
                else if (line.rfind("VmSize:", 0) == 0)
                    pInfo.vms_kb = std::stol(line.substr(8));
            }

            //User.
            if (passwd* pw = getpwuid(uid))
                pInfo.szUser = pw->pw_name;

            //Cmdline.
            std::ifstream cmd(entry.path() / "cmdline");
            std::string arg;
            while (std::getline(cmd, arg, '\0'))
                pInfo.szCmdLine += arg + " ";

            //exe.
            std::error_code ec;
            auto szExePath = std::filesystem::read_symlink(entry.path() / "exe", ec);
            if (!ec)
                pInfo.szExePath = szExePath.string();

            pidIndex[pInfo.nPID] = processes.size();
            ppidMap[pInfo.nPpid].push_back(pInfo.nPID);

            processes.push_back(std::move(pInfo));
        }

        //Subprocess.
        for (auto& p : processes)
        {
            if (ppidMap.count(p.nPID))
                p.vChildren = ppidMap[p.nPID];
        }

        return processes;
    }

    RETMSG fnKillProcess(pid_t nPid)
    {
        return kill(nPid, SIGKILL) == 0 ? RETMSG{ 1, "" } : RETMSG { 0, strerror(errno) };
    }

    RETMSG fnStopProcess(pid_t nPid)
    {
        return kill(nPid, SIGSTOP) == 0 ? RETMSG{ 1, "" } : RETMSG { 0, strerror(errno) };
    }

    RETMSG fnContinueProcess(pid_t nPid)
    {
        return kill(nPid, SIGCONT) == 0 ? RETMSG{ 1, "" } : RETMSG { 0, strerror(errno) };
    }

    std::string fnParser(std::vector<stProcInfo>& lsProc)
    {
        std::vector<std::string> lsResult;

        for (auto& proc : lsProc)
        {
            std::vector<std::string> tmp;
            tmp.push_back(std::to_string(proc.nPID));
            tmp.push_back(std::to_string(proc.nPpid));
            tmp.push_back(proc.szName);
            tmp.push_back(proc.szUser);
            tmp.push_back(proc.szCmdLine);
            tmp.push_back(proc.szExePath);
            tmp.push_back(std::to_string(proc.rss_kb));
            tmp.push_back(std::to_string(proc.vms_kb));
            tmp.push_back(clsEZData::fnszSendParser(proc.vChildren));

            lsResult.push_back(clsEZData::fnszSendParser(tmp, ","));
        }

        return clsEZData::fnszSendParser(lsResult);
    }

private:
    bool fnbIsNumber(const std::string& s)
    {
        return !s.empty() && std::all_of(s.begin(), s.end(), ::isdigit);
    }

};