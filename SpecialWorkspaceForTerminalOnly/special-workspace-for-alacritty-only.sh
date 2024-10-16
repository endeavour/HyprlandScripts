#!/bin/sh
SOCKET_ADDR=$XDG_RUNTIME_DIR/hypr/$HYPRLAND_INSTANCE_SIGNATURE/.socket.sock
SOCKET2_ADDR=$XDG_RUNTIME_DIR/hypr/$HYPRLAND_INSTANCE_SIGNATURE/.socket2.sock
socat -U - UNIX-CONNECT:$SOCKET2_ADDR | dotnet fsi watch.fsx | while IFS= read -r line; do echo Moving window to previous workspace; echo -n $line | socat -u - UNIX-CONNECT:$SOCKET_ADDR; done
