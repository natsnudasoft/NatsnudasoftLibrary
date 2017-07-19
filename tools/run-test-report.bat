@echo off
SET openCoverVersion=4.6.519
SET xunitRunnerVersion=2.2.0
SET reportGeneratorVersion=2.5.8
SET coverallsVersion=1.3.4
IF "%1"=="buildserver" (
    "%~dp0..\packages\OpenCover.%openCoverVersion%\tools\OpenCover.Console.exe" -register:user "-filter:+[*]* -[*Tests]*" -target:"%~dp0..\packages\xunit.runner.console.%xunitRunnerVersion%\tools\xunit.console.exe" -targetargs:"\"%~dp0..\test\unit\NatsnudasoftTests\bin\Release\Natsnudasoft.NatsnudasoftTests.dll\" -noshadow -appveyor" -excludebyattribute:*.ExcludeFromCodeCoverage* -excludebyfile:*Designer.cs -output:coverage.xml
    "%~dp0..\packages\coveralls.io.%coverallsVersion%\tools\coveralls.net.exe" --opencover coverage.xml
    IF %errorlevel%==0 echo Report generated and sent to coveralls...
) ELSE (
    "%~dp0..\packages\OpenCover.%openCoverVersion%\tools\OpenCover.Console.exe" -register:user "-filter:+[*]* -[*Tests]*" -target:"%~dp0..\packages\xunit.runner.console.%xunitRunnerVersion%\tools\xunit.console.exe" -targetargs:"\"%~dp0..\test\unit\NatsnudasoftTests\bin\Debug\Natsnudasoft.NatsnudasoftTests.dll\" -noshadow" -excludebyattribute:*.ExcludeFromCodeCoverage* -excludebyfile:*Designer.cs -output:coverage.xml
    "%~dp0..\packages\ReportGenerator.%reportGeneratorVersion%\tools\ReportGenerator.exe" -reports:coverage.xml -targetdir:coverage
    echo Press any key to display report...
    pause >nul
    start coverage\index.htm
)