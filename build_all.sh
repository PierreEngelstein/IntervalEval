#!/bin/bash

targets=("win-x64" "win-arm64" "linux-x64" "linux-arm64")
version=$1
if [ -z $version ]; then
    version="dev"
fi

echo "[INFO] Building version $version"

rm -rf bin/
rm -rf publish/
mkdir -p bin/
mkdir -p publish/
rm -rf *.zip

dotnet clean
dotnet restore

for target in ${targets[@]}; do
    echo "======================="
    echo "BUILDING TARGET $target"
    echo "======================="
    cd IntervalEval.Front
    echo "[INFO] Building WEB executable"
    dotnet publish -c release -r $target -o ../publish/${target}_${version}_WEB /p:TrimUnusedDependencies=true --self-contained true -p:PublishSingleFile=true
    cd ../IntervalEval.Optimizer
    echo "[INFO] Building CONSOLE executable"
    dotnet publish -c release -r $target -o ../publish/${target}_${version}_CONSOLE /p:TrimUnusedDependencies=true --self-contained true -p:PublishSingleFile=true
    cd ..
    rm -rf publish/${target}_${version}_WEB/*.pdb
    rm -rf publish/${target}_${version}_CONSOLE/*.pdb
    cd publish/${target}_${version}_WEB
    echo "[INFO] Creating archives"
    zip -r ../../bin/${target}_${version}_WEB.zip ./*
    cd ../${target}_${version}_CONSOLE
    zip -r ../../bin/${target}_${version}_CONSOLE.zip ./*
    cd ../../
    echo "[INFO] Target $target done"
done