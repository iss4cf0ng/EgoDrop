#pragma once


#include "clsHostScan.hpp"
#include "clsThreadPool.hpp"
#include "clsEZData.hpp"
#include "clsTools.hpp"
#include "clsVictim.hpp"

class clsArpScan : public clsHostScan
{
public:
    clsArpScan()
    {

    }

    void fnStart(std::vector<std::string>& lsIP, std::vector<int>& lnPort) override
    {

    }

    void fnStop() override
    {

    }
};