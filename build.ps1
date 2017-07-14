$root = $PSScriptRoot;

& "C:\Program Files (x86)\Microsoft Visual Studio\Preview\Enterprise\MSBuild\15.0\bin\msbuild.exe" $root\RepoPilotUpdater\RepoPilotUpdater.sln
$exe = "$root\RepoPilotUpdater\RepoPilotUpdater\bin\Debug\RepoPilotUpdater.exe"
echo "using pilot updater $exe"

pushd $root\buildtools;
#& .\build.cmd -- /p:BuildNumberMajor=09999 /p:BuildNumberMinor=99;
$package = gci bin\packages | % { $_.Name } > "$root\RepoPilotUpdater\RepoPilotUpdater\bin\Debug\packages.list"
$version = $matches[1];
popd
echo "using BuildTools version $version"

echo "client root: $root\clients"

gci $root\clients | % {
	echo "running $exe $root\RepoPilotUpdater\RepoPilotUpdater\bin\Debug\packages.list $($_.FullName) $root\buildtools\bin\packages"
	& $exe "$root\RepoPilotUpdater\RepoPilotUpdater\bin\Debug\packages.list" $_.FullName "$root\buildtools\bin\packages"

	
	
}

