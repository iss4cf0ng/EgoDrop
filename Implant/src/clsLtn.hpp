#pragma once

#include <iostream>
#include <string>
#include <vector>

class clsLtn
{
public:
    virtual void fnStart()
    {
        return;
    }

    virtual void fnStop()
    {
        return;
    }

    virtual void fnSendToSub(std::string& szSubID, std::vector<std::string>& vuMsg)
    {
        return;
    }
};