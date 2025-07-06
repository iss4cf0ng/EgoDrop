#include "clsTools.h"

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