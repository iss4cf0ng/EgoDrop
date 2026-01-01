#pragma once

#ifdef __cplusplus
extern "C"
{
    typedef struct plugin_meta_t
    {
        const char* name;
        const char* version;
        const char* author;
        const char* description;
        const char* args;
    } plugin_meta_t;

    extern const plugin_meta_t PLUGIN_META;
}
#endif