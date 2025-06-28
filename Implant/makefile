# Variables
CXX = g++
CXXFLAGS = -Wall -O2
SRC_DIR = src
BIN_DIR = bin
TARGET = $(BIN_DIR)/rat_startup

# Collect all .cpp files in src/
SRCS = $(wildcard $(SRC_DIR)/*.cpp)

# Default target
all: $(BIN_DIR) $(TARGET)

# Compile all cpp directly into single binary
$(TARGET): $(SRCS)
	$(CXX) $(CXXFLAGS) -o $@ $^

# Ensure bin/ exists
$(BIN_DIR):
	mkdir -p $(BIN_DIR)

# Clean only the binary
clean:
	rm -f $(TARGET)