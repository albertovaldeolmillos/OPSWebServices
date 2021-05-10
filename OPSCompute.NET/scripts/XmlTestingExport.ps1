$folderPath = (Get-Item -Path ".\").FullName
$xUnitConsole = "$folderPath\packages\xunit.runner.console.2.4.0\tools\net46\xunit.console.exe"

# UNIT TEST PROJECTS
$PDMHelpers = "$folderPath\PDMHelpers.UnitTests\bin\Debug\PDMHelpers.UnitTests.dll"
$PDMCompute = "$folderPath\PDMCompute.UnitTests\bin\Debug\PDMCompute.UnitTests.dll"
$PDMMessages = "$folderPath\PDMMessages.UnitTests\bin\Debug\PDMMessages.UnitTests.dll"

# XUNIT TEST REPORTS
& "$xUnitConsole" "$PDMHelpers" "$PDMCompute" "$PDMMessages" -xml .\reports\UnitTest.xml