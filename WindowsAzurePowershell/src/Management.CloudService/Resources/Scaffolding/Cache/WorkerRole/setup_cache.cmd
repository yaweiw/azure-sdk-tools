@echo on
cd /d "%~dp0"

bin\WindowsAzure.Caching.MemcacheShim\ClientPerfCountersInstaller.exe install
bin\WindowsAzure.Caching.MemcacheShim\MemcacheShimInstaller.exe