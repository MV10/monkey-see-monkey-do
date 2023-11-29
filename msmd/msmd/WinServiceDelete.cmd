@openfiles.exe 1>nul 2>&1
@if not %errorlevel% equ 0 goto :fail
sc delete "Monkey Hi Hat TCP Relay (msmd)"
@goto:eof
:fail
@echo:
@echo Failed. Run as Administrator.
@echo:
:eof