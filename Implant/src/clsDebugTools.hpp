#pragma once

#include <iostream>

#include "clsTools.hpp"
#include "clsEZData.hpp"

class clsDebugTools
{
public:
    clsDebugTools() = default;

    ~clsDebugTools() = default;

    static void fnPrintStringList(std::vector<std::string>& vsData)
    {
        for (int i = 0; i < vsData.size(); i++)
            std::cout << vsData[i] << ",";
        
        std::cout << std::endl;
    }
    static void fnPrintStringList(const std::vector<std::string>& vsData)
    {
        std::vector<std::string> ls;
        ls.reserve(vsData.size());
        ls.insert(ls.end(), vsData.begin(), vsData.end());

        fnPrintStringList(ls);
    }
};