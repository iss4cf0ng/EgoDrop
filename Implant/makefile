# Variables
CXX = g++
CXXFLAGS = -Wall -O2 -std=c++17
SRC_DIR = src
BIN_DIR = bin
TARGET = $(BIN_DIR)/rat_startup

# Collect all .cpp files in src/
SRCS = $(wildcard $(SRC_DIR)/*.cpp)

# OpenSSL flags using pkg-config
OPENSSL_CFLAGS := $(shell pkg-config --cflags openssl)
OPENSSL_LIBS   := $(shell pkg-config --libs openssl)

# X11 flags
X11_CFLAGS := $(shell pkg-config --cflags x11)
X11_LIBS   := $(shell pkg-config --libs x11)

# Default target
all: $(BIN_DIR) $(TARGET)

# Compile all cpp directly into single binary
$(TARGET): $(SRCS)
	$(CXX) $(CXXFLAGS) $(OPENSSL_CFLAGS) $(X11_CFLAGS) -o $@ $^ $(OPENSSL_LIBS) $(X11_LIBS)

# Ensure bin/ exists
$(BIN_DIR):
	mkdir -p $(BIN_DIR)

# Clean only the binary
clean:
	rm -f $(TARGET)
