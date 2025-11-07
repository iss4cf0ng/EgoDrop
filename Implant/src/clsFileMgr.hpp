#pragma once

#include <iostream>
#include <sstream>
#include <fstream>
#include <unistd.h>
#include <string>
#include <vector>

#include <sys/stat.h>
#include <ctime>
#include <iomanip>
#include <dirent.h>

#include "clsTools.hpp"

class clsFileMgr
{
private:
    std::string m_szInitFileDir;
    
public:
    struct stFileInfo
    {
        bool mIsDir;
        std::string szFilePath;
        std::string szPermission;
        size_t nFileSize;
        std::string szCreationDate;
        std::string szLastModifiedDate;
        std::string szLastAccessedDate;
    };

    clsFileMgr()
    {

    }

    ~clsFileMgr()
    {

    }

    stFileInfo fnGetFileInfo(const std::string& szFullPath)
    {
        struct stat st;

        if (stat(szFullPath.c_str(), &st) != 0)
        {
            //todo: print error.
            return {  };
        }

        size_t nSize = st.st_size; //bytes
        
        stFileInfo info = {
            st.st_mode & S_IFDIR,
            szFullPath,
            fnszGetPermission(st),
            nSize,
            fnTimeToString(st.st_ctime),
            fnTimeToString(st.st_mtime),
            fnTimeToString(st.st_atime),
        };

        return info;
    }

    std::vector<stFileInfo> fnScandir(const std::string& szDirPath)
    {
        struct dirent** namelist;
        int n = scandir(szDirPath.data(), &namelist, nullptr, alphasort);
        if (n < 0)
        {
            
        }

        std::vector<stFileInfo> uvFileInfo(n);
        while (n--)
        {
            const char* pFileName = namelist[n]->d_name;
            uvFileInfo.push_back(fnGetFileInfo(namelist[n]->d_name));
            free(namelist[n]);
        }

        free(namelist);

        return uvFileInfo;
    }

private:
    std::string fnszGetPermission(struct stat st)
    {
        std::ostringstream oss;

        oss << ((st.st_mode & S_IRUSR) ? "r" : "-");
        oss << ((st.st_mode & S_IWUSR) ? "w" : "-");
        oss << ((st.st_mode & S_IXUSR) ? "x" : "-");

        oss << ((st.st_mode & S_IRGRP) ? "r" : "-");
        oss << ((st.st_mode & S_IWGRP) ? "w" : "-");
        oss << ((st.st_mode & S_IXGRP) ? "x" : "-");

        oss << ((st.st_mode & S_IROTH) ? "r" : "-");
        oss << ((st.st_mode & S_IWOTH) ? "w" : "-");
        oss << ((st.st_mode & S_IXOTH) ? "x" : "-");

        return oss.str();
    }

    std::string fnTimeToString(std::time_t t)
    {
        std::tm tm{};
        localtime_r(&t, &tm);

        std::ostringstream oss;
        oss << std::put_time(&tm, "%Y-%m-%d %H:%M:%S");

        return oss.str();
    }

};