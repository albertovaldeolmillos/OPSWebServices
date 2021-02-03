
$folderPath = (Get-Item -Path ".\").FullName
$xUnitConsole = "$folderPath\packages\xunit.runner.console.2.4.0\tools\net46\xunit.console.exe"
$openCoverConsole = "$folderPath\packages\OpenCover.4.6.519\tools\OpenCover.Console.exe"

# UNIT TEST PROJECTS
$PDMHelpers = "$folderPath\PDMHelpers.UnitTests\bin\Debug\PDMHelpers.UnitTests.dll"
$PDMCompute = "$folderPath\PDMCompute.UnitTests\bin\Debug\PDMCompute.UnitTests.dll"
$PDMMessages = "$folderPath\PDMMessages.UnitTests\bin\Debug\PDMMessages.UnitTests.dll"

& "$openCoverConsole"  -output:.\reports\coverage.xml -register:user -target:"$xUnitConsole"  -targetargs:"$PDMHelpers" -targetdir:"$folderPath\PDMHelpers.UnitTests\bin\Debug" -filter:"+[PDMHelpers]*" -oldstyle
& "$openCoverConsole"  -output:.\reports\coverage.xml -mergeoutput  -register:user -target:"$xUnitConsole"  -targetargs:"$PDMCompute" -targetdir:"$folderPath\PDMCompute.UnitTests\bin\Debug" -filter:"+[PDMCompute]*" -oldstyle
& "$openCoverConsole"  -output:.\reports\coverage.xml -mergeoutput  -register:user -target:"$xUnitConsole"  -targetargs:"$PDMMessages" -targetdir:"$folderPath\PDMMessages.UnitTests\bin\Debug" -filter:"+[PDMMessages]*" -oldstyle
