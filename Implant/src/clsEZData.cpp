#include <stdio.h>
#include <stdlib.h>

#include "clsEZData.h"

char *clsEZData::fnszB64_encode(const unsigned char *data, size_t input_length, size_t *output_length)
{
    *output_length = 4 * ((input_length + 2) / 3);
    
    
}

char *clsEZData::fnszB64_decode(const char *data, size_t input_length, size_t *output_length)
{

}