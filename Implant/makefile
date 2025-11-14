# Compiler and flags
CXX := g++
CXXFLAGS := -Wall -Wextra -O2 -std=c++17

# Directories
SRC_DIR := src
OBJ_DIR := obj
BIN_DIR := bin

# Target binary
TARGET := $(BIN_DIR)/rat_startup

# Find all .cpp files and corresponding .o files
SRCS := $(wildcard $(SRC_DIR)/*.cpp)
OBJS := $(SRCS:$(SRC_DIR)/%.cpp=$(OBJ_DIR)/%.o)

# External libraries (pkg-config)
OPENSSL_CFLAGS := $(shell pkg-config --cflags openssl)
OPENSSL_LIBS   := $(shell pkg-config --libs openssl)

X11_CFLAGS := $(shell pkg-config --cflags x11)
X11_LIBS   := $(shell pkg-config --libs x11)

# Combine all external flags
CPPFLAGS := $(OPENSSL_CFLAGS) $(X11_CFLAGS)
LDFLAGS  := $(OPENSSL_LIBS) $(X11_LIBS)

# Default target
all: $(TARGET)

# Link target
$(TARGET): $(OBJS) | $(BIN_DIR)
	$(CXX) $(CXXFLAGS) -o $@ $^ $(LDFLAGS)

# Compile source files into object files
$(OBJ_DIR)/%.o: $(SRC_DIR)/%.cpp | $(OBJ_DIR)
	$(CXX) $(CXXFLAGS) $(CPPFLAGS) -c $< -o $@

# Ensure directories exist
$(BIN_DIR) $(OBJ_DIR):
	mkdir -p $@

# Clean build artifacts
clean:
	rm -rf $(OBJ_DIR) $(TARGET)

.PHONY: all clean
