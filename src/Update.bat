@echo off
git clone https://github.com/MaaXYZ/MaaFramework --progress --branch gh-pages -v --

cd MaaFramework
git checkout gh-pages --
git pull --no-rebase --tags --no-prune --

cd ../Doxygen
call Update.bat

pause
