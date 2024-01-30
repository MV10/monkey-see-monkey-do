@fsutil dirty query %systemdrive% 1>nul 2>&1
@if not %errorlevel% equ 0 goto :fail
sc stop "Monkey Hi Hat TCP Relay (msmd)"
@goto:eof
:fail
@echo:
@echo Failed. Run as Administrator.
@echo:
:eof