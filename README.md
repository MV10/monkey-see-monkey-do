# monkey-see-monkey-do

| Version 1.0.0 is available on the Releases page at the right.

A simple service that listens on a TCP port for [Monkey Hi Hat](https://github.com/MV10/monkey-hi-hat) (MHH) commands and relays them to the locally-running copy. If the application isn't running when a command is received, the program will launch it, then relay the command, which is the main value of this service. Any response to the command will be relayed back to the sender, as if MHH itself was contacted directly.

Although MHH itself has the ability to listen on TCP (and this relay service requires it), if the program is to be used exclusively remotely (such as via [Monkey Droid](https://github.com/MV10/monkey-droid) from an Android phone or tablet, or a remote Windows laptop or computer), the most practical usage (without this service) is to leave MHH running indefinitely -- probably in standby mode, which still leaves a console window on-screen in the current iteration. This service eliminates that need.

The program must be executed from the same directory as `mhh.exe`.

## TCP Port Configuration
The application is deployed alongside MHH and reads the MHH configuration file (subject to all the same environment variables and other config file rules, which are documented in the MHH repository). The MHH `UnsecuredPort` setting must be used (which tells MHH to listen over TCP, and port 50001 is the default), and this service also requires an `UnsecuredRelayPort` setting (which is the port used by this service, and port 50002 is the default). If either is not found, the service will log an error and exit. You can also specify `RelayIPType` with a value of 4 or 6 to restrict localhost IP resolution to IPv4 or IPv6, which can result in much lower connection latency.

A remotely-issued command flows through a sequence which looks something like this (assuming the default TCP ports are used), and the response string returns to Monkey Droid the same way, in reverse order:

```
Monkey Droid command
  --> command sent to LAN TCP 50002 
    --> Monkey-See-Monkey-Do receives
      --> if localhost TCP 50001 isn't listening, launch MHH 
        --> command sent to localhost TCP 50001 
          --> Monkey Hi Hat receives
```

Therefore, MHH should also probably be configured to start in fullscreen, not start in standby, and not quit to standby.

## Windows Service
Currently only the Windows service has been tested. It runs under the name `Monkey Hi Hat TCP Relay (msmd)`. Use the provided batch files (running as Administrator) to manage the service. The service is created for delayed auto-start whenever Windows is booted.

* WinServiceCreate
* WinServiceRun
* WinServiceStop
* WinServiceDelete

Run the program interactively (from a console window) at least once, so that you'll be prompted to allow the program to communicate through the Windows firewall:

![firewall prompt](https://mcguirev10.com/assets/misc/msmd-firewall.jpg)

## Linux Daemon
At the moment one of the underlying libraries used by MHH has a bug that prevents Linux builds (or at least makes them very impractical to use/test/maintain). A fix should be available relatively soon, at which point I'll be able to do some testing and get it working and document the setup. Much of the code is already there to run as a systemd service daemon, but nothing has been tested. At that time more instructions will be provided about how to use the provided unit file (`msmd.service`).

## Logging
Activity and problems are output to the console when running interactively. When running as a service, messages are logged to `msmd.log` in the application directory. This is a simple append-over-time log which is overwritten every time the service starts.

## Testing
Although the program is intended to be called from Monkey Droid, you can issue commands to it (or to Monkey Hi Hat directly) via the `tcpargs` demo program in the [CommandLineSwitchPipe](https://github.com/MV10/CommandLineSwitchPipe) repository (which is how Monkey Hi Hat listens for commands). For example, this will launch MHH then return some statistics like frame rate:

`tcpargs localhost 50002 --info`

For debugging the project interactively in Visual Studio, the Debug Properties working directory must point to a location with the Monkey Hi Hat executable, since the deployed application will be side-by-side with MHH itself.

## Beware the BOM
If you're editing plain-text files like the `cmd` files, be aware of a long-standing Visual Studio bug that inserts useless, invisible BOM (Byte Order Marker) characters into the file. (UTF-8 is by definition single-byte, so "byte order" is meaningless.) The only way to fix the file is to modify it in a better editor like Notepad++. More than 6 years and they still haven't fixed this. Pretty sad.

https://developercommunity.visualstudio.com/t/utf-8-save-as-without-signature-default-request-to/787476

## TODO
* Test Linux process-launch
* Finish Linux systemd support https://devblogs.microsoft.com/dotnet/net-core-and-systemd/
