#! /bin/sh

PROJECT_PATH=$(pwd)/$UNITY_PROJECT_PATH
UNITY_BUILD_DIR=$(pwd)/Build
LOG_FILE=$UNITY_BUILD_DIR/unity-test.log


ERROR_CODE=0
echo "Running unity editor tests..."
mkdir $UNITY_BUILD_DIR
/Applications/Unity/Unity.app/Contents/MacOS/Unity \
  -batchmode \
  -nographics \
  -force-free \
  -silent-crashes \
  -logFile \
  -projectPath "$PROJECT_PATH" \
  -runEditorTests \
  | tee "$LOG_FILE"
  
if [ $? = 0 ] ; then
  echo "Tests completed successfully."
  ERROR_CODE=0
else
  echo "Tests failed. Exited with $?."
  ERROR_CODE=1
fi

echo "Finishing with code $ERROR_CODE"
exit $ERROR_CODE
