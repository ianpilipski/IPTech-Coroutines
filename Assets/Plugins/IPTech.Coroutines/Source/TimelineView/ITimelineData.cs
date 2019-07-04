/*
	IPTech.Coroutines is a coroutine and debug visualizer library

    Copyright (C) 2019  Ian Pilipski

    This program is free software: you can redistribute it and/or modify
    it under the terms of the MIT license

    You should have received a copy of the MIT License
    along with this program.  If not, see <https://opensource.org/licenses/MIT>.
*/

using System;
using System.Collections.Generic;

namespace IPTech.Coroutines.Insights {
	public class EntryAddedEventArgs : EventArgs {
		public readonly ITimelineEntry TimelineEntry;
		public EntryAddedEventArgs(ITimelineEntry entry) {
			TimelineEntry = entry;
		}
	}
	
	public interface ITimelineData : IEnumerable<ITimelineEntry> {
		DateTime StartTime { get; }
		DateTime CurrentTime { get; }
	}
}
