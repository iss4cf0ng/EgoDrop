// xgrabkey.c
#include <X11/Xlib.h>
#include <X11/keysym.h>
#include <stdio.h>
#include <stdlib.h>

int main() {
    Display *d = XOpenDisplay(NULL);
    if (!d) { fprintf(stderr, "Cannot open display\n"); return 1; }

    Window root = DefaultRootWindow(d);

    // Example: Ctrl+Alt+H
    int keycode = XKeysymToKeycode(d, XStringToKeysym("H"));
    unsigned int modifiers = ControlMask | Mod1Mask; // Mod1 is usually Alt

    // Grab on the root window (global)
    XGrabKey(d, keycode, modifiers, root, True, GrabModeAsync, GrabModeAsync);
    XSelectInput(d, root, KeyPressMask);

    printf("Listening for Ctrl+Alt+H... press it in any window\n");

    XEvent ev;
    while (1) {
        XNextEvent(d, &ev);
        if (ev.type == KeyPress) {
            XKeyEvent *ke = (XKeyEvent*)&ev;
            if (ke->keycode == keycode && (ke->state & modifiers) == modifiers) {
                printf("Hotkey pressed!\n");
                // handle event...
            }
        }
    }

    XCloseDisplay(d);
    return 0;
}
