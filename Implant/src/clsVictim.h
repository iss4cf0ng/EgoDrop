#include <arpa/inet.h>
#include <unistd.h>

class clsVictim
{

    public:
        int m_nSktSrv;

        clsVictim(int sktSrv);

        int fnnSend();
        int fnnSend(const std::string& szData);
};