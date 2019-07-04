/*
	IPTech.Coroutines is a coroutine and debug visualizer library

    Copyright (C) 2019  Ian Pilipski

    This program is free software: you can redistribute it and/or modify
    it under the terms of the MIT license

    You should have received a copy of the MIT License
    along with this program.  If not, see <https://opensource.org/licenses/MIT>.
*/

// TODO: Make sure timeline shows error coroutines in the flow as red.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace IPTech.Coroutines.Insights {
	public class TimelineData : ITimelineData {
		IList<TimelineEntry> _entries;
		IList<TimelineEntry> _pendingEntries;

		public DateTime StartTime { get; private set; }
		public DateTime CurrentTime { get; private set; }

		public TimelineData() {
			ResetTimeToNow();
			_entries = new List<TimelineEntry>();
		}

		void ResetTimeToNow() {
			StartTime = DateTime.Now;
			CurrentTime = StartTime;
		}

		public ITimelineEntry Add(ICFunc coroutineFunction, object owner = null) {
			TimelineEntry entry = new TimelineEntry(coroutineFunction);
			_entries.Add(entry);
			UpdateEntry(entry);
			return entry;
		}

		public void Clear() {
			_entries.Clear();
			ResetTimeToNow();
		}

		public void Update() {
			UpdateCurrentTime();
			for(int i = _entries.Count - 1; i >= 0; i--) {
				UpdateEntry(_entries[i]);
			}
		}

		void UpdateCurrentTime() {
			CurrentTime = DateTime.Now;
		}

		void UpdateEntry(TimelineEntry entry) {
			if(entry._coroutineFunction.HasUpdater && entry.Parent == null) {
				TimelineEntry parent = Find(tle => entry._coroutineFunction.IsUpdatedBy(tle._coroutineFunction));
				if(parent != null) {
					if(_entries.Contains(entry)) {
						_entries.Remove(entry);
					}
					if(!parent.Children.Contains(entry)) {
						parent.AddChild(entry);
					}
				}
			}
		}

		TimelineEntry Find(Func<TimelineEntry, bool> predicate) {
			Stack<TimelineEntry> stack = new Stack<TimelineEntry>();

			foreach(TimelineEntry entry in _entries) {
				stack.Push(entry);
			}

			while(stack.Count > 0) {
				TimelineEntry entry = stack.Pop();
				if(predicate(entry)) {
					return entry;
				}
				foreach(TimelineEntry child in entry.Children) {
					stack.Push(child);
				}
			}

			return null;
		}

		public IEnumerator<ITimelineEntry> GetEnumerator() {
			return _entries.Cast<ITimelineEntry>().GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator() {
			return _entries.GetEnumerator();
		}

		class TimelineEntry : ITimelineEntry, IEnumerable<TimelineEntry> {
			public readonly ICFunc _coroutineFunction;
			readonly IList<TimelineEntry> _children;
			
			public TimelineEntryState State {
				get {
					if(_coroutineFunction.Error != null) {
						return TimelineEntryState.Attention;
					}
					if(!End.HasValue) {
						return TimelineEntryState.Highlight;
					}
					return TimelineEntryState.Normal;
				}
			}

			public string Name { get; private set; }

			public ITimelineEntry Parent { get; private set; }
			public IEnumerable<ITimelineEntry> Children { get { return _children.Cast<ITimelineEntry>(); } }

			public string ExtendedInfo {
				get {
					string retVal = "Coroutine Still Running";
					if(End.HasValue) {
						return "Coroutine Done";
					}

					if(_coroutineFunction.Error != null) {
						retVal = string.Format("{0}\nException: {0}", retVal, _coroutineFunction.Error);
					}

					return retVal;
				}
			}

			public DateTime  Start       { get; private set; }
			public DateTime  LastUpdated { get { return _coroutineFunction.LastUpdated;	} }
			public DateTime? End {
				get {
					if(_coroutineFunction.IsDone) {
						return _coroutineFunction.LastUpdated;
					}
					return null;
				}
			}

			public TimelineEntry(ICFunc coroutineFunction) {
				Start = DateTime.Now;
				_children = new List<TimelineEntry>();
				_coroutineFunction = coroutineFunction;
				SetFunctionName();
			}

			void SetFunctionName() {
				string name = _coroutineFunction.FunctionName;
				int index = name.LastIndexOf(">c__Iterator", StringComparison.InvariantCulture);
				if(index>0) {
					Name = name.Substring(0, index+1);
				}
			}

			public void AddChild(TimelineEntry entry) {
				entry.Parent = this;
				_children.Add(entry);
			}

			public IEnumerator<TimelineEntry> GetEnumerator() {
				return _children.GetEnumerator();
			}

			IEnumerator IEnumerable.GetEnumerator() {
				return _children.GetEnumerator();
			}
		}
	}
}
