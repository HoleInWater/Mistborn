@echo off
echo.
echo MISTBORN ERA ONE - PROJECT CHECK
echo ======================================
echo.
echo TODO COMMENTS IN CODE:
echo ----------------------
findstr /s /n "TODO" Assets\_Project\Scripts\*.cs
echo.
echo GIT STATUS:
git status --short
echo.
echo RECENT COMMITS:
git log --oneline -5
echo.
echo Check complete.
