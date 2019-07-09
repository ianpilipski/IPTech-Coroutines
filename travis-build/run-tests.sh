#! /bin/sh

PROJECT_PATH=$(pwd)/$UNITY_PROJECT_PATH
UNITY_BUILD_DIR=$(pwd)/Build
LOG_FILE=$UNITY_BUILD_DIR/unity-test.log
TEST_RESULT_FILE=$UNITY_BUILD_DIR/unity-test-results.xml

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
  -editorTestsResultFile "$TEST_RESULT_FILE" \
  -runEditorTests \
  -username "$UNAME" \
  -password "$UPASS" \
  | tee "$LOG_FILE"
  
if [ $? = 0 ] ; then
  echo "Tests completed successfully."
  ERROR_CODE=0
else
  echo "Tests failed. Exited with $?."
  ERROR_CODE=1
fi

/Applications/Unity/Unity.app/Contents/MacOS/Unity -quit -batchmode -returnlicense

echo "Finishing with code $ERROR_CODE"
exit $ERROR_CODE
