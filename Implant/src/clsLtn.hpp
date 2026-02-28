/*
Name: C2 Listener
Author: iss4cf0ng/ISSAC

Description:
    This is a parent class for class inheritence.
*/

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