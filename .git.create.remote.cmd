@echo off

set git.remote.project=CodeFexWebMedia
set git.remote.repository=CodeFex.NetCore.git
set git.remote.base=https://github.com

set git.remote.url=%git.remote.base%/%git.remote.project%/%git.remote.repository%

git remote add origin %git.remote.url%
git push --mirror --force
