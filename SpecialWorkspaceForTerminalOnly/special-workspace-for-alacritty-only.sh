#!/bin/sh
ACTIVE_WORKSPACE=1
SOCKET_ADDR=$XDG_RUNTIME_DIR/hypr/$HYPRLAND_INSTANCE_SIGNATURE/.socket.sock
SOCKET2_ADDR=$XDG_RUNTIME_DIR/hypr/$HYPRLAND_INSTANCE_SIGNATURE/.socket2.sock
socat -U - UNIX-CONNECT:$SOCKET2_ADDR | dotnet fsi special-workspace-for-alacritty-only.fsx $ACTIVE_WORKSPACE | while IFS= read -r line; do echo Moving window to previous workspace; echo -n $line | socat -u - UNIX-CONNECT:$SOCKET_ADDR; done
