#include <stdio.h>
#include <stdlib.h>

class clsEZData
{
public:
    const char *pB64Table = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+/";

    char *fnszB64_encode(const unsigned char *data, size_t input_length, size_t *output_length);
    char *fnszB64_decode(const char *data, size_t input_length, size_t *output_length);
};