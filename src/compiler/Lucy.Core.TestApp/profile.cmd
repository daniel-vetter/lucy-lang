dotnet build
pushd bin\Debug\net6.0
dotnet trace collect --output trace.nettrace -- Lucy.Core.TestApp.exe
dotnet trace convert trace.nettrace -o trace.json --format Chromium
speedscope trace.speedscope.json
popd
pause