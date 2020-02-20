# pumb-fishing-site-breaker

Small utility that floods fishing web site that looks like PUMB online by tons of fake phone numbers and passwords to make a life of its authors more complicated.

## How to run:
MacOS:
Open Terminal in the repo's root folder and run `./.build/mac/PumbFishingSiteBreaker --threads-count 100`.

Win:
Open Powershell in the repo's root folder and run  `.build/win/PumbFishingSiteBreaker.exe --threads-count 100`.


## Dev run
Run following script under `./src/PumbFishingSiteBreaker` folder: `dotnet run`.

## Build
1. Version for MacOS:
```sh
dotnet publish --self-contained -r osx-x64 -o ../../.build/mac -c Release
```

1. Version of Win:
```sh
dotnet publish --self-contained -r win-x64 -o ../../.build/win -c Release
```