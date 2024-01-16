@echo off
cd ../../
set ROBUST_DISABLE_SANDBOX=1
call dotnet run --project Content.Client --no-build %*
pause
