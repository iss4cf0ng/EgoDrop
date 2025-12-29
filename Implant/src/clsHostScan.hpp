#pragma once

#include <iostream>
#include <vector>

class clsHostScan
{
public:
    virtual void fnStart(std::vector<std::string>& lsIP, std::vector<int>& lnPort)
    {

    }

    virtual void fnStop()
    {

    }
};