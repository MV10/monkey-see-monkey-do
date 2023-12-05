@openfiles.exe 1>nul 2>&1
@if not %errorlevel% equ 0 goto :fail
sc create "Monkey Hi Hat TCP Relay (msmd)" binpath= "C:\Program Files\mhh\msmdwindows.exe" start= delayed-auto displayname= "McGuireV10.com Monkey Hi Hat Remote Command Relay"
@goto:eof
:fail
@echo:
@echo Failed. Run as Administrator.
@echo:
:eof