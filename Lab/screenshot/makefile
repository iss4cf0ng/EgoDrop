
CXX      := g++
CXXFLAGS := -Wall -O2 -std=c++17
SRC      := src/main.cpp
OBJ      := bin/main.o
BIN      := bin/screenshot

all: $(BIN)

$(BIN): $(OBJ) | bin
	$(CXX) $(CXXFLAGS) $^ -o $@

$(OBJ): $(SRC) | bin
	$(CXX) $(CXXFLAGS) -c $< -o $@

bin:
	mkdir -p $@

.PHONY: clean
clean:
	rm -f $(OBJ) $(BIN)