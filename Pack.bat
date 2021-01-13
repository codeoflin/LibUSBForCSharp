dotnet pack -c release
copy .\bin\release\*.nupkg z:\MyNuget\
del .\bin\release\*.nupkg