set -e
rm -rf ./build
./gradlew clean saxonc_stage_wheels all_saxonc_releases
./gradlew -PbuildMacArch=x64 dotnetMakeApp
./gradlew -PbuildMacArch=arm64 dotnetMakeApp
lipo -create -output ./SaxonCS \
     build/cs/bin/Release/net6.0/osx-arm64/publish/SaxonCS.app/Contents/MacOS/SaxonCS \
     build/cs/bin/Release/net6.0/osx-x64/publish/SaxonCS.app/Contents/MacOS/SaxonCS
mkdir -p build/cs/upublish/SaxonCS.app
rsync -var build/cs/bin/Release/net6.0/osx-x64/publish/SaxonCS.app/ build/cs/upublish/SaxonCS.app/
mv ./SaxonCS build/cs/upublish/SaxonCS.app/Contents/MacOS/
codesign --force --options runtime --entitlements tools/saxon-cs/entitlements.plist --deep --sign "Developer ID Application: Saxonica Limited" --timestamp build/cs/upublish/SaxonCS.app
