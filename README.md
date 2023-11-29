# monkey-see-monkey-do

| Work in progress, not yet released.

A simple service that listens on a TCP port for [Monkey Hi Hat](https://github.com/MV10/monkey-hi-hat) (MHH) commands and relays them to the locally-running copy. If the application isn't running when a command is received, the program will launch it, then relay the command.

Although MHH itself has the ability to listen on TCP (and this relay service requires it), if the program is to be used exclusively remotely (such as via [Monkey Droid](https://github.com/MV10/monkey-droid) from an Android phone or tablet, or a remote Windows laptop or computer), the most practical usage (without this service) is to leave MHH running indefinitely -- probably in standby mode, which still leaves a console window on-screen in the current iteration. This service eliminates that need.

## Configuration
The application is deployed alongside MHH and reads the MHH configuration file (subject to all the same environment variables and other config file rules, which are documented in the MHH repository). The MHH `UnsecuredPort` setting must be used (which tells MHH to listen over TCP, and port 50001 is the default), and this service also requires an `UnsecuredRelayPort` setting (which is the port used by this service, and port 50002 is the default). If either is not found, the service will log an error (on Windows to Event Viewer) and exit.

A remotely-issued command flows through sequence that looks something like this (assuming the default TCP ports are used):

```
Monkey Droid --> TCP:50002 --> Monkey-See-Monkey-Do --> (launch MHH) --> TCP:50001 --> Monkey Hi Hat
```

## Windows Service
Currently only the Windows service has been tested. It runs under the name `Monkey Hi Hat TCP Relay (msmd)`. Use the provided batch files (running as Administrator) to manage the service:

* WinServiceCreate
* WinServiceRun
* WinServiceStop
* WinServiceDelete

## Linux Daemon
At the moment one of the underlying libraries used by Monkey Hi Hat has a bug that prevents Linux builds (or at least makes them very impractical). Once that is fixed, I'll be able to do some testing and get it working and document the setup. Much of the code is already there to run as a systemd service daemon, but nothing has been tested.
