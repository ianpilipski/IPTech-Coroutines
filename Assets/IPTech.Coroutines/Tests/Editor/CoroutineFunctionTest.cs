/*
	IPTech.Coroutines is a coroutine and debug visualizer library

    Copyright (C) 2019  Ian Pilipski

    This program is free software: you can redistribute it and/or modify
    it under the terms of the MIT license

    You should have received a copy of the MIT License
    along with this program.  If not, see <https://opensource.org/licenses/MIT>.
*/

using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using IPTech.Coroutines;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.TestTools;
using Range = NUnit.Framework.RangeAttribute;

namespace CoroutineFunctionTests {
	public class YeildingKnownUnityTypes : TestFixture {

		[Test]
		public void WaitForSeconds_WaitsForExpectedSeconds() {
			CallInfo ci = new CallInfo();
			CFunc coroutineUnderTest = new CFunc(RoutineYield(ci, () => new WaitForSeconds(2)));

			CallMoveNextUntilCoroutineCompletes(coroutineUnderTest, 5);

			Assert.GreaterOrEqual(ci.TotalSeconds, 2);
		}

		[Test]
		public void WaitForSecondsRealtime_WaitsForExpectedSeconds() {
			CallInfo ci = new CallInfo();
			CFunc coroutineUnderTest = new CFunc(RoutineYield(ci, () => new WaitForSecondsRealtime(2)));

			CallMoveNextUntilCoroutineCompletes(coroutineUnderTest, 5);

			Assert.GreaterOrEqual(ci.TotalSeconds, 2);
		}

		[Test]
		public void WWW_RunsToCompletion() {
			CallInfo ci = new CallInfo();
			CFunc coroutineUnderTest = new CFunc(RoutineYield(ci, () => new WWW("http://httpbin.org/get?hello")));

			CallMoveNextUntilCoroutineCompletes(coroutineUnderTest, 10);

			Assert.IsTrue(ci.LastLineHit);
		}

		[Test]
		public void AsyncOperation_RunsToCompletion() {
			CallInfo ci = new CallInfo();
			CFunc coroutineUnderTest = new CFunc(RoutineYield(ci, () => {
				DownloadHandlerBuffer buffer = new DownloadHandlerBuffer();
				UnityWebRequest wr = new UnityWebRequest("http://httpbin.org/get?hello");
				wr.downloadHandler = buffer;
				return wr.SendWebRequest();
			}));

			CallMoveNextUntilCoroutineCompletes(coroutineUnderTest, 60);

			Assert.IsTrue(ci.LastLineHit);
		}

		[Test]
		public void WaitForEndOfFrame_TickGroupReturnsEndOfFrame() {
			CallInfo ci = new CallInfo();
			CFunc coroutineUnderTest = new CFunc(RoutineYield(ci, () => new WaitForEndOfFrame()));

			coroutineUnderTest.MoveNext();

			Assert.AreEqual(ETickGroup.EndOfFrame, coroutineUnderTest.TickGroup);
		}

		[Test]
		public void WaitForFixedUpdate_TickGroupReturnsFixedUpdate() {
			CallInfo ci = new CallInfo();
			CFunc coroutineUnderTest = new CFunc(RoutineYield(ci, () => new WaitForFixedUpdate()));

			coroutineUnderTest.MoveNext();

			Assert.AreEqual(ETickGroup.FixedUpdate, coroutineUnderTest.TickGroup);
		}
	}

	public class CurrentProperty : TestFixture {
		CFunc coroutineUnderTest;

		[SetUp]
		public void SetUp() {
			CallInfo ci = new CallInfo();
			coroutineUnderTest = new CFunc(RoutineLoop(ci, 1));
		}

		[Test]
		public void Default_IsNull() {
			Assert.IsNull(coroutineUnderTest.Current);
		}

		[Test]
		public void WhenCompleted_IsNull() {
			CallMoveNextUntilCoroutineCompletesOrHitsMaxIterations(coroutineUnderTest);

			Assert.IsNull(coroutineUnderTest.Current);
		}

		[Test]
		public void WhenYieldingNonYieldFactoryItems_IsYieldedValue() {
			CallInfo ci = new CallInfo();
			object yieldObject = new object();
			coroutineUnderTest = new CFunc(RoutineYield(ci, () => yieldObject));

			coroutineUnderTest.MoveNext();

			Assert.AreSame(yieldObject, coroutineUnderTest.Current);
		}

		[Test]
		public void WhenYieldingYieldFactoryItems_IsNull() {
			CallInfo ci = new CallInfo();
			object yieldObject = new WaitForSecondsRealtime(1);
			coroutineUnderTest = new CFunc(RoutineYield(ci, () => yieldObject));

			coroutineUnderTest.MoveNext();

			Assert.IsNull(coroutineUnderTest.Current);
		}

		[Test]
		public void WhenYieldingCoroutineFunction_IsNull() {
			CallInfo ci = new CallInfo();
			object yieldObject = new CFunc(RoutineYield(ci, () => null));
			coroutineUnderTest = new CFunc(RoutineYield(ci, () => yieldObject));

			coroutineUnderTest.MoveNext();

			Assert.IsNull(coroutineUnderTest.Current);
		}
	}

	public class FunctionNameProperty : TestFixture {
		[Test]
		public void ReturnsExpectedValue() {
			CallInfo ci = new CallInfo();
			IEnumerator coroutine = RoutineYield(ci, () => null);
			string expectedName = coroutine.ToString();
			CFunc coroutineUnderTest = new CFunc(coroutine);

			Assert.AreEqual(expectedName, coroutineUnderTest.FunctionName);
		}
	}

	public class TickGroupProperty : TestFixture {
		ETickGroup EXPECTED_DEFAULT = ETickGroup.Update;

		[Test]
		public void Default_ReturnsUpdate() {
			CallInfo ci = new CallInfo();
			CFunc coroutineUnderTest = new CFunc(RoutineYield(ci, () => null));

			Assert.AreEqual(EXPECTED_DEFAULT, coroutineUnderTest.TickGroup);
		}

		[Test]
		public void WhenYieldWaitForEndOfFrame_ReturnsEndOfFrame() {
			CallInfo ci = new CallInfo();
			CFunc coroutineUnderTest = new CFunc(RoutineYield(ci, () => new WaitForEndOfFrame()));
			Assume.That(coroutineUnderTest.TickGroup == EXPECTED_DEFAULT);

			coroutineUnderTest.MoveNext();

			Assert.AreEqual(ETickGroup.EndOfFrame, coroutineUnderTest.TickGroup);
		}

		[Test]
		public void WhenYieldWaitForFixedUpdate_ReturnsFixedUpdate() {
			CallInfo ci = new CallInfo();
			CFunc coroutineUnderTest = new CFunc(RoutineYield(ci, () => new WaitForFixedUpdate()));
			Assume.That(coroutineUnderTest.TickGroup == EXPECTED_DEFAULT);

			coroutineUnderTest.MoveNext();

			Assert.AreEqual(ETickGroup.FixedUpdate, coroutineUnderTest.TickGroup);
		}
	}

	public class MoveNextMethod : TestFixture {
		[Test]
		public void WithError_ThrowsException() {
			CallInfo ci = new CallInfo();
			CFunc coroutineUnderTest = new CFunc(RoutineYield(ci, () => { throw new TestException(); }));

			Assert.Throws<TestException>(() => {
				coroutineUnderTest.MoveNext();
			});
		}

		[Test]
		public void WhenCatch_WithError_DoesNotThrowException() {
			CallInfo ci = new CallInfo();
			CFunc coroutineUnderTest = new CFunc(RoutineYield(ci, () => { throw new TestException(); })).Catch(e => {
				Debug.Log("Caught exception: " + e);
			});

			Assert.DoesNotThrow(() => {
				coroutineUnderTest.MoveNext();
			});
		}

		[Test]
		public void WhenFinally_WithError_ThrowsException() {
			CallInfo ci = new CallInfo();
			CFunc coroutineUnderTest = new CFunc(RoutineYield(ci, () => { throw new TestException(); })).Finally(() => {
				Debug.Log("Called Finally");
			});

			Assert.Throws<TestException>(() => {
				coroutineUnderTest.MoveNext();
			});
		}

		[Test]
		public void Executes_Expected_Number_Of_Times([Range(1, 10)] int expectedIterations) {
			CallInfo ci = new CallInfo();
			CFunc coroutineUnderTest = new CFunc(RoutineLoop(ci, expectedIterations));

			int actualIterations = CallMoveNextUntilCoroutineCompletesOrHitsMaxIterations(coroutineUnderTest);

			Assert.AreEqual(expectedIterations, actualIterations);
		}

		[Test]
		public void Yielding_IEnumerator_evaluates_in_succession_waiting_for_return() {
			CallInfo ci = new CallInfo();
			CFunc coroutineUnderTest = new CFunc(RoutineWithTwoInnerLoops(ci, 10));
			int iter = CallMoveNextUntilCoroutineCompletesOrHitsMaxIterations(coroutineUnderTest);

			Assert.AreEqual(19, iter);
			Assert.IsTrue(ci.LastLineHit);
		}

		[Test]
		public void Yielding_CoroutineWrapper_will_properly_complete_the_yielded_routine() {
			CallInfo ci = new CallInfo();
			CallInfo innerCi = new CallInfo();
			CFunc innerCoroutineFunction = new CFunc(RoutineLoop(innerCi, 10));
			CFunc coroutineUnderTest = new CFunc(RoutineYield(ci, () => innerCoroutineFunction));

			CallMoveNextUntilCoroutineCompletesOrHitsMaxIterations(coroutineUnderTest);

			Assert.IsTrue(coroutineUnderTest.IsDone);
			Assert.IsTrue(ci.LastLineHit);
			Assert.IsTrue(innerCoroutineFunction.IsDone);
			Assert.IsTrue(innerCi.LastLineHit);
		}

		[Test]
		public void Is_only_updated_by_the_registered_updater() {
			const int loopCount = 10;
			CallInfo ciOne = new CallInfo();
			CallInfo ciTwo = new CallInfo();
			CallInfo ciInner = new CallInfo();

			CFunc crInner = new CFunc(RoutineLoop(ciInner, loopCount));
			CFunc crOne = new CFunc(RoutineYield(ciOne, () => crInner));
			CFunc crTwo = new CFunc(RoutineYield(ciTwo, () => crInner));

			// Calling MoveNext on crOne should yield crInner, and claim it as the Updater of crInner
			// No other coroutine should update crInner at this point and only waits for completion
			crOne.MoveNext();
			Assert.IsTrue(ciOne.FirstLineHit);
			Assert.IsTrue(ciInner.FirstLineHit);
			Assert.IsFalse(ciInner.LastLineHit);

			// This call should not advance the inner coroutine at all.
			int iter = 0;
			while(iter++ < loopCount * 10) {
				crTwo.MoveNext();
			}
			Assert.IsTrue(ciTwo.FirstLineHit);
			Assert.IsFalse(ciTwo.LastLineHit);
			Assert.IsFalse(ciInner.LastLineHit);

			// Advance the owning coroutine to the expected loopCount - 1 so that it should
			// complete the inner Coroutine and itself.
			iter = 0;
			while(iter++ < loopCount) {
				crOne.MoveNext();
			}
			Assert.IsTrue(ciInner.LastLineHit);
			Assert.IsTrue(ciOne.LastLineHit);
			Assert.IsTrue(crOne.IsDone);
			Assert.IsTrue(crInner.IsDone);

			//Advance the crTwo one time should now complete it
			crTwo.MoveNext();
			Assert.IsTrue(ciTwo.LastLineHit);
			Assert.IsTrue(crTwo.IsDone);
		}
	}

	public class IsDoneProperty : TestFixture {
		CallInfo ci;

		[SetUp]
		public void SetUp() {
			ci = new CallInfo();
		}

		[Test]
		public void Default_ReturnsFalse() {
			CFunc coroutineUnderTest = new CFunc(RoutineLoop(ci, 1));

			Assert.IsFalse(coroutineUnderTest.IsDone);
		}

		[Test]
		public void WhenCoroutineCompletes_ReturnsTrue() {
			CFunc coroutineUnderTest = new CFunc(RoutineLoop(ci, 1));
			Assume.That(coroutineUnderTest.IsDone == false);

			CallMoveNextUntilCoroutineCompletesOrHitsMaxIterations(coroutineUnderTest);

			Assert.IsTrue(coroutineUnderTest.IsDone);
		}

		[Test]
		public void WhenCorotuineThrows_ReturnsTrue() {
			TestException.Expect();
			CFunc coroutineUnderTest = new CFunc(RoutineLoop(ci, 1, true));
			Assume.That(coroutineUnderTest.IsDone == false);

			CallMoveNextUntilCoroutineCompletesOrHitsMaxIterations(coroutineUnderTest);

			Assert.IsTrue(coroutineUnderTest.IsDone);
		}
	}

	public class ErrorProperty : TestFixture {
		CallInfo ci;

		[SetUp]
		public void SetUp() {
			ci = new CallInfo();
		}

		[Test]
		public void DefaultValue_IsNull() {
			CFunc coroutineUnderTest = new CFunc(RoutineLoop(ci, 1, true));

			Assert.IsNull(coroutineUnderTest.Error);
		}

		[Test]
		public void WhenCoroutineThrows_IsNotNull() {
			TestException.Expect();
			CFunc coroutineUnderTest = new CFunc(RoutineLoop(ci, 1, true));
			Assume.That(coroutineUnderTest.Error == null);

			CallMoveNextUntilCoroutineCompletesOrHitsMaxIterations(coroutineUnderTest);

			Assert.IsNotNull(coroutineUnderTest.Error);
		}

		[Test]
		public void WhenCoroutineCompletes_IsNull() {
			CFunc coroutineUnderTest = new CFunc(RoutineLoop(ci, 1));

			CallMoveNextUntilCoroutineCompletesOrHitsMaxIterations(coroutineUnderTest);

			Assert.IsNull(coroutineUnderTest.Error);
		}
	}

	namespace CatchMethod {
		public class WhenCoroutineThrows : TestFixture {
			Exception errorHandled;
			CFunc coroutineUnderTest;

			[SetUp]
			public void SetUp() {
				CallInfo ci = new CallInfo();
				errorHandled = null;
				coroutineUnderTest = new CFunc(RoutineLoop(ci, 1, true));
				coroutineUnderTest.Catch(e => {
					errorHandled = e;
				});
			}

			[Test]
			public void Handler_IsCalled() {
				CallMoveNextUntilCoroutineCompletesOrHitsMaxIterations(coroutineUnderTest);

				Assert.IsNotNull(errorHandled);
			}

			[Test]
			public void IsDone_ReturnsTrue() {
				CallMoveNextUntilCoroutineCompletesOrHitsMaxIterations(coroutineUnderTest);

				Assert.IsTrue(coroutineUnderTest.IsDone);
			}

			[Test]
			public void Error_ReturnsNull() {
				CallMoveNextUntilCoroutineCompletesOrHitsMaxIterations(coroutineUnderTest);

				Assert.IsNull(coroutineUnderTest.Error);
			}
		}

		public class WithRethrow : TestFixture {
			Exception errorHandled;
			Exception errorRethrown;
			CFunc coroutineUnderTest;

			[SetUp]
			public void SetUp() {
				CallInfo ci = new CallInfo();
				errorHandled = null;
				errorRethrown = new TestRethrowException();
				coroutineUnderTest = new CFunc(RoutineLoop(ci, 1, true));
				coroutineUnderTest.Catch(e => {
					errorHandled = e;
					throw errorRethrown;
				});
			}

			[Test]
			public void Handler_IsCalled() {
				TestRethrowException.Expect();

				CallMoveNextUntilCoroutineCompletesOrHitsMaxIterations(coroutineUnderTest);

				Assert.IsNotNull(errorHandled);
			}

			[Test]
			public void IsDone_ReturnsTrue() {
				TestRethrowException.Expect();

				CallMoveNextUntilCoroutineCompletesOrHitsMaxIterations(coroutineUnderTest);

				Assert.IsTrue(coroutineUnderTest.IsDone);
			}

			[Test]
			public void Error_ReturnsRethrownValue() {
				TestRethrowException.Expect();

				CallMoveNextUntilCoroutineCompletesOrHitsMaxIterations(coroutineUnderTest);

				Assert.AreSame(errorRethrown, coroutineUnderTest.Error);
			}
		}
	}

	public class FinallyMethod : TestFixture {
		[Test]
		public void WhenCompleted_HandlerIsCalled() {
			bool finallyHandlerCalled = false;
			CallInfo ci = new CallInfo();
			CFunc coroutineUnderTest = new CFunc(RoutineLoop(ci, 1));
			coroutineUnderTest.Finally(() => {
				finallyHandlerCalled = true;
			});

			CallMoveNextUntilCoroutineCompletesOrHitsMaxIterations(coroutineUnderTest);

			Assert.IsTrue(coroutineUnderTest.IsDone);
			Assert.IsNull(coroutineUnderTest.Error);
			Assert.IsTrue(finallyHandlerCalled);
		}

		[Test]
		public void WhenThrows_HandlerIsCalled() {
			TestException.Expect();
			bool goalReached = false;
			CallInfo ci = new CallInfo();
			CFunc coroutineUnderTest = new CFunc(RoutineLoop(ci, 1, true));
			coroutineUnderTest.Finally(() => {
				goalReached = true;
			});
			CallMoveNextUntilCoroutineCompletesOrHitsMaxIterations(coroutineUnderTest);

			Assert.IsTrue(coroutineUnderTest.IsDone);
			Assert.IsNotNull(coroutineUnderTest.Error);
			Assert.IsTrue(goalReached);
		}

		[Test]
		public void WhenThrowsWithCatch_HandlerIsCalled() {
			bool goalReached = false;
			bool errorReached = false;
			CallInfo ci = new CallInfo();
			CFunc coroutineUnderTest = new CFunc(RoutineLoop(ci, 1, true));
			coroutineUnderTest.Catch(e => {
				errorReached = true;
			}).Finally(() => {
				goalReached = true;
			});
			CallMoveNextUntilCoroutineCompletesOrHitsMaxIterations(coroutineUnderTest);

			Assert.IsTrue(coroutineUnderTest.IsDone);
			Assert.IsNull(coroutineUnderTest.Error);
			Assert.IsTrue(errorReached);
			Assert.IsTrue(goalReached);
		}
	}

	public class RegisterUpdaterMethod : TestFixture {
		[Test]
		public void RegistersUpdater_IsUpdatedByReturnsTrueForUpdater() {
			CallInfo ci = new CallInfo();
			CFunc cf = new CFunc(RoutineYield(ci, () => null));
			object expectedOwner = new object();

			cf.RegisterUpdater(expectedOwner);

			Assert.IsTrue(cf.IsUpdatedBy(expectedOwner));
		}

		[Test]
		public void WhenUpdaterAlreadyRegisterd_ThrowsExpectedException() {
			object expecteOwner = new object();
			CallInfo ci = new CallInfo();
			CFunc coroutineUnderTest = new CFunc(RoutineYield(ci, () => null));
			coroutineUnderTest.RegisterUpdater(expecteOwner);

			Assert.Throws<CFunc.RunnerAlreadyDefined>(() => {
				coroutineUnderTest.RegisterUpdater(expecteOwner);
			});
		}
	}

	public class IsUpdatedByMethod : TestFixture {
		CFunc coroutineUnderTest;

		[SetUp]
		public void Setup() {
			CallInfo ci = new CallInfo();
			coroutineUnderTest = new CFunc(RoutineYield(ci, () => null));
		}

		[Test]
		public void Default_WhenCalledWithNull_ReturnsTrue() {
			Assert.IsTrue(coroutineUnderTest.IsUpdatedBy(null));
		}

		[Test]
		public void WhenCalledWithObjectThatIsNotTheRegisteredUpdater_ReturnsFalse() {
			Assert.IsFalse(coroutineUnderTest.IsUpdatedBy(new object()));
		}

		[Test]
		public void WhenCalledWithNULL_AndHasRegisteredUpdater_ReturnsFalse() {
			object expectedUpdater = new object();
			coroutineUnderTest.RegisterUpdater(expectedUpdater);

			Assert.IsFalse(coroutineUnderTest.IsUpdatedBy(null));
		}

		[Test]
		public void WhenCalledwithRegisteredUpdater_ReturnsTrue() {
			object expectedUpdater = new object();
			coroutineUnderTest.RegisterUpdater(expectedUpdater);

			Assert.IsTrue(coroutineUnderTest.IsUpdatedBy(expectedUpdater));
		}
	}

	public class HasUpdaterProperty : TestFixture {
		CFunc coroutineUnderTest;

		[SetUp]
		public void SetUp() {
			CallInfo ci = new CallInfo();
			coroutineUnderTest = new CFunc(RoutineYield(ci, () => null));
		}

		[Test]
		public void Default_ReturnsFalse() {
			Assert.IsFalse(coroutineUnderTest.HasUpdater);
		}

		[Test]
		public void WhenRegisterOwnerCalled_ReturnsTrue() {
			coroutineUnderTest.RegisterUpdater(new object());

			Assert.IsTrue(coroutineUnderTest.HasUpdater);
		}
	}


	public class TestFixture {

		protected IEnumerator RoutineYield(CallInfo info, Func<object> yieldAction) {
			info.Begin();
			{
				yield return yieldAction();
			}
			info.End();
		}

		public class CallInfo {
			DateTime _startTime;
			DateTime _endTime;

			public bool FirstLineHit { get; private set; }
			public bool LastLineHit { get; private set; }
			public IList<CallInfo> Children = new List<CallInfo>();

			public int TotalSeconds {
				get {
					return (int)(_endTime - _startTime).TotalSeconds;
				}
			}

			public void Begin() {
				_startTime = DateTime.Now;
				FirstLineHit = true;
			}

			public void End() {
				_endTime = DateTime.Now;
				LastLineHit = true;
			}

			public CallInfo NewChild() {
				CallInfo retVal = new CallInfo();
				Children.Add(retVal);
				return retVal;
			}
		}

		protected void CallMoveNextUntilCoroutineCompletes(CFunc coroutine, int timeoutSeconds = 10) {
			DateTime timeoutTime = DateTime.Now.AddSeconds(timeoutSeconds);
			while(coroutine.MoveNext()) {
				if(DateTime.Now >= timeoutTime) {
					Assert.Fail("Timeout reached before coroutine finished.");
					break;
				}
			}
		}

		protected int CallMoveNextUntilCoroutineCompletesOrHitsMaxIterations(CFunc coroutine, int maxIterations = 100) {
			int iter = 1;
			try {
				while(coroutine.MoveNext()) {
					if(++iter > maxIterations) {
						Assert.Fail("Max Iterations Reached Before Corouine Finished.");
						break;
					}
				}
			} catch(Exception e) {
				Debug.LogException(e);
			}
			return iter;
		}

		protected IEnumerator RoutineLoop(CallInfo ci, int loopCount, bool throwException = false) {
			ci.Begin();
			{
				while(--loopCount > 0) {
					yield return null;
				}
				if(throwException) {
					throw new TestException();
				}
			}
			ci.End();
		}

		protected IEnumerator RoutineWithTwoInnerLoops(CallInfo ci, int loopCount) {
			ci.Begin();
			{
				yield return RoutineLoop(ci.NewChild(), loopCount);
				yield return RoutineLoop(ci.NewChild(), loopCount);
			}
			ci.End();
		}

		public class TestException : Exception {
			public static void Expect() {
				LogAssert.Expect(LogType.Exception, new Regex("TestException:.*"));
			}
		}

		public class TestRethrowException : Exception {
			public static void Expect() {
				LogAssert.Expect(LogType.Exception, new Regex("TestRethrowException:.*"));
			}
		}
	}
}
