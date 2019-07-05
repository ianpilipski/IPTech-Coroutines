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
using System.Linq;
using NUnit.Framework;
using NSubstitute;
using IPTech.Coroutines;
using IPTech.Coroutines.Insights;
using System.Collections.Generic;

namespace InsightsTests {
	public class TestFixture {

		protected TimelineData timelineDataUnderTest;
		protected TestException ExpectedException;
		protected DateTime testStartTime;

		[SetUp]
		public void SetUp() {
			testStartTime = DateTime.Now;
			ExpectedException = new TestException();
			timelineDataUnderTest = new TimelineData();
		}

		protected ICFunc CreateRoutine(string functionName, ICFunc parent=null) {
			ICFunc retVal = Substitute.For<ICFunc>();
			retVal.FunctionName.Returns(functionName);
			if(parent != null) {
				retVal.HasUpdater.Returns(true);
				retVal.IsUpdatedBy(Arg.Is<object>(parent)).Returns(true);
			}
			return retVal;
		}
	}

	public class StartTimeProperty : TestFixture {
		[Test]
		public void ReturnsTimeCreated() {
			Assert.GreaterOrEqual(timelineDataUnderTest.StartTime, testStartTime);
			Assert.LessOrEqual(timelineDataUnderTest.StartTime, DateTime.Now);
		}
	}

	public class CurrentTimeProperty : TestFixture {
		[Test]
		public void Default_ReturnsStartTime() {
			Assert.AreEqual(timelineDataUnderTest.StartTime, timelineDataUnderTest.CurrentTime);
		}

		[Test]
		public void WhenUpdateIsCalled_ReturnsNow() {
			Assume.That(timelineDataUnderTest.CurrentTime < DateTime.Now);

			DateTime start = DateTime.Now;
			timelineDataUnderTest.Update();

			Assert.GreaterOrEqual(timelineDataUnderTest.CurrentTime, start);
			Assert.LessOrEqual(timelineDataUnderTest.CurrentTime, DateTime.Now);
		}
	}

	public class UpdateMethod : TestFixture {
		[Test]
		public void CurrentTimeIsUpdated() {
			DateTime oldCurrentTime = timelineDataUnderTest.CurrentTime;
			DateTime start = DateTime.Now;

			timelineDataUnderTest.Update();

			Assert.AreNotEqual(oldCurrentTime, timelineDataUnderTest.CurrentTime);
			Assert.GreaterOrEqual(timelineDataUnderTest.CurrentTime, start);
			Assert.LessOrEqual(timelineDataUnderTest.CurrentTime, DateTime.Now);
		}

		[Test]
		public void ParentRelationshipIsUpdated() {
			Assume.That(false);
		}

		[Test]
		public void UpdateIsCalledOnRootEntries() {
			Assume.That(false);
		}
	}

	public class IEnumerableInterface : TestFixture {
		
		[Test]
		public void WhenEntryIsAdded_IEnumeratorContainsExpectedParentChildRelationships(
			[Range(1,4)] int expectedRootEntries,
			[Range(0,3)] int expectedChildEntries,
			[Range(0,2)] int expectedGrandChildEntries
		) {
			Dictionary<ITimelineEntry, Dictionary<ITimelineEntry, List<ITimelineEntry>>> dict = new Dictionary<ITimelineEntry, Dictionary<ITimelineEntry, List<ITimelineEntry>>>();

			for(int i=0; i<expectedRootEntries; i++) {
				ICFunc parent = CreateRoutine("parent"+i);
				var rootEntry = timelineDataUnderTest.Add(parent);
				dict.Add(rootEntry, new Dictionary<ITimelineEntry, List<ITimelineEntry>>());

				for(int j = 0; j < expectedChildEntries; j++) {
					ICFunc child = CreateRoutine("child" + j, parent);
					var childEntry = timelineDataUnderTest.Add(child);
					dict[rootEntry].Add(childEntry, new List<ITimelineEntry>());

					for(int k=0; k < expectedGrandChildEntries; k++) {
						ICFunc grandChild = CreateRoutine("grandChild", child);
						grandChild.HasUpdater.Returns(true);
						grandChild.IsUpdatedBy(Arg.Is<object>(child)).Returns(true);
						var grandChildEntry = timelineDataUnderTest.Add(grandChild);
						dict[rootEntry][childEntry].Add(grandChildEntry);
					}
				}
			}

			timelineDataUnderTest.Update();

			Assert.AreEqual(expectedRootEntries, timelineDataUnderTest.Count());
			int index = 0;
			foreach(var item in timelineDataUnderTest) {
				Assert.Contains(item, ((ICollection)dict.Keys));
				Assert.IsNull(item.Parent);
				Assert.AreEqual(expectedChildEntries, item.Children.Count());

				int childIndex = 0;
				foreach(var child in item.Children) {
					Assert.Contains(child, ((ICollection)dict[item].Keys));
					Assert.AreSame(item, child.Parent);
					Assert.AreEqual(expectedGrandChildEntries, child.Children.Count());

					int grandChildIndex = 0;
					foreach(var grandChild in child.Children) {
						Assert.Contains(grandChild, ((ICollection)dict[item][child]));
						Assert.AreSame(child, grandChild.Parent);
						Assert.AreEqual(0, grandChild.Children.Count());
						grandChildIndex++;
					}
					childIndex++;
				}
				index++;
			}
		}
	}

	namespace TimelineEntryTests {
		public class TimelineEntryTestFixture : TestFixture {
			protected const string expectedFunctionName = "testFunctionName";
			protected ITimelineEntry entryUnderTest;
			protected ICFunc rootCoroutine;

			[SetUp]
			public void SetUpTimelineEntryTests() {
				rootCoroutine = CreateRoutine(expectedFunctionName);
				entryUnderTest = timelineDataUnderTest.Add(rootCoroutine);
				// return true after registration with Add
				rootCoroutine.IsDone.Returns(true);
				rootCoroutine.LastUpdated.Returns(DateTime.Now);
			}
		}

		public class NameProperty : TimelineEntryTestFixture {
			[Test]
			public void ReturnsFunctionName() {
				Assert.AreEqual(expectedFunctionName, entryUnderTest.Name);
			}
		}

		public class StartProperty : TimelineEntryTestFixture {
			[Test]
			public void ReturnsTimeCreated() {
				Assert.GreaterOrEqual(entryUnderTest.Start, testStartTime);
				Assert.LessOrEqual(entryUnderTest.Start, DateTime.Now);
			}
		}

		public class EndProperty : TimelineEntryTestFixture {
			[Test]
			public void WhenNotCompleted_ReturnsNull() {
				rootCoroutine.IsDone.Returns(false);
				Assert.IsNull(entryUnderTest.End);
			}

			[Test]
			public void WhenCompleted_ReturnsTimeCompleted() {
				Assert.GreaterOrEqual(entryUnderTest.End, testStartTime);
				Assert.LessOrEqual(entryUnderTest.End, DateTime.Now);
			}
		}

		public class ParentProperty : TimelineEntryTestFixture {
			[Test]
			public void RootEntries_ReturnsNull() {
				Assert.IsNull(entryUnderTest.Parent);
			}

			[Test]
			public void ChildEntries_ReturnsParent() {
				ITimelineEntry childEntry = timelineDataUnderTest.Add(CreateRoutine("child", rootCoroutine));

				Assert.AreSame(entryUnderTest, childEntry.Parent);
			}
		}

		public class ChildrenProperty : TimelineEntryTestFixture {
			[Test]
			public void Default_ReturnsEmptyEnumeration() {
				Assert.IsNotNull(entryUnderTest.Children);
				Assert.AreEqual(0, entryUnderTest.Children.Count());
			}

			[Test]
			public void WhenChildrenAreRegistered_ReturnsExpectedChildren([Range(1,3)] int expectedChildren) {
				List<ITimelineEntry> children = new List<ITimelineEntry>(expectedChildren);
				for(int i = 0; i < expectedChildren; i++) {
					children.Add(timelineDataUnderTest.Add(CreateRoutine("child", rootCoroutine)));
				}

				Assert.IsNotNull(entryUnderTest.Children);
				Assert.AreEqual(expectedChildren, entryUnderTest.Children.Count());

				int index = 0;
				foreach(var child in entryUnderTest.Children) {
					Assert.AreSame(children[index], child);
					index++;
				}
			}
		}

	}

	public class TestException : Exception { }
}
