#include <iostream>
#include <cstring>
#include <unistd.h>
#include <arpa/inet.h>

#define g_szIP "127.0.0.1"
#define g_nPort 4444

using namespace std;

void fnLogError(const string& szMsg);

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

    

    return 0;
}

void fnLogError(const string& szMsg)
{
    cerr << "[-] " << szMsg << endl;
}