/*
Name: EgoDrop HTTP C2 listener.
Author: iss4cf0ng/ISSAC
*/

#pragma once

#include <iostream>
#include <memory>
#include <arpa/inet.h>
#include <mutex>
#include <atomic>

#include "clsLtn.hpp"
#include "clsVictim.hpp"
#include "clsEZData.hpp"
#include "clsTools.hpp"

class clsLtnHttp : public clsLtn
{
private:
    std::shared_ptr<clsVictim> m_victim;
    int m_nPort;

    std::atomic<bool> m_bIsRunning { false };

public:
    clsLtnHttp() = default;

    clsLtnHttp(std::shared_ptr<clsVictim> victim, int nPort)
    {
        m_victim = victim;
        m_nPort = nPort;
    }

    ~clsLtnHttp() = default;

    void fnStart() override
    {

    }

    void fnStop() override
    {
        
    }
};