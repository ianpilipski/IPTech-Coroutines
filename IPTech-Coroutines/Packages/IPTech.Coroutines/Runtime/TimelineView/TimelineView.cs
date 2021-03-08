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
using System.Linq;
using UnityEngine;

namespace IPTech.Coroutines.Insights {
	public class TimelineView {
		readonly ControlContext _controlContext;
		readonly TimelineStyle _style;

		ITimelineEntry _selectedEntry;
		Vector2 _scrollPos;
		bool _continuousScrolling;
		float _scale = 100.0f;
		string _scaleText = "100";
		Rect splitterRect;
		bool dragging;
		float splitterHeight = 64;

		public TimelineView(GUISkin skin) {
			_controlContext = new ControlContext();
			_style = new TimelineStyle(skin);
		}

		public void Draw(ITimelineData timelineData) {
			if(_continuousScrolling) {
				_scrollPos.x = float.MaxValue;
			}
			_scrollPos = Draw(_scrollPos, timelineData, _scale);
		}

		public Vector2 Draw(Vector2 scrollPos, ITimelineData timelineData, float scale=1.0f) {
			using(new GUIScope(_style.Skin)) {
				using(new GUILayout.VerticalScope(_style.Background)) {
					Rect headerRect = GUILayoutUtility.GetRect(new GUIContent("0"), _style.Tickmarks, GUILayout.ExpandWidth(true));
					using(var ss = new GUILayout.ScrollViewScope(scrollPos, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true))) {
						DrawTimeLine(timelineData, scale);
						scrollPos = ss.scrollPosition;
					}
					DrawHeaderInRect(headerRect, scrollPos, timelineData, scale);
					DrawFooterSplitter();
					DrawFooter(timelineData);
				}
				return scrollPos;
			}
		}

		void DrawHeaderInRect(Rect headerRect, Vector2 scrollPos, ITimelineData timelineData, float scale) {
			DateTime startTime = timelineData.StartTime;
			double seconds = (timelineData.CurrentTime - startTime).TotalSeconds;

			float startSecs = (scrollPos.x / scale);
			int startOffset = (int)((startSecs - Math.Truncate(startSecs)) * scale);
			int totalSecsToDraw = (int)Math.Ceiling((headerRect.width + startOffset) / scale);
			headerRect.x -= startOffset;
			for(int i = 0; i < totalSecsToDraw; i++) {
				Rect r = new Rect(
					headerRect.x + (scale * i),
					headerRect.y,
					scale,
					headerRect.height
				);
				GUI.Label(r, ((int)startSecs + i).ToString(), _style.Tickmarks);
			}
		}

		void DrawFooterSplitter() {
			GUILayout.Box("",
				GUILayout.Height(8),
				GUILayout.MaxHeight(8),
				GUILayout.MinHeight(8),
				GUILayout.ExpandWidth(true)
			);

			if(Application.isPlaying && !dragging) {
				Vector2 mousePos = Input.mousePosition;
				mousePos.y = Screen.height - mousePos.y;
				Vector2 p = GUIUtility.ScreenToGUIPoint(mousePos);
				if(splitterRect.Contains(p)) {
					Debug.Log("In it!");
					Cursor.SetCursor(_style.ResizeUpDown, new Vector2(16, 16), CursorMode.Auto);
				} else {
					Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
				}
			}
#if UNITY_EDITOR
			UnityEditor.EditorGUIUtility.AddCursorRect(splitterRect, UnityEditor.MouseCursor.SplitResizeUpDown);
#endif

			// Splitter events
			if(Event.current != null) {
				switch(Event.current.type) {
					case EventType.Repaint:
						splitterRect = GUILayoutUtility.GetLastRect();
						splitterRect.yMin -= 4;
						splitterRect.height += 4;
						break;
					case EventType.MouseDown:
						if(splitterRect.Contains(Event.current.mousePosition)) {
							dragging = true;
							Event.current.Use();
						}
						break;
					case EventType.MouseDrag:
						if(dragging) {
							splitterHeight -= Event.current.delta.y;
							Event.current.Use();
						}
						break;
					case EventType.MouseUp:
						if(dragging) {
							dragging = false;
							Event.current.Use();
						}
						break;
				}
			}
		}

		void DrawFooter(ITimelineData timelineData) {
			using(new GUILayout.HorizontalScope(_style.Footer, GUILayout.Height(splitterHeight))) {
				using(new GUILayout.VerticalScope(GUILayout.ExpandHeight(true), GUILayout.ExpandWidth(true))) {
					String msg = "(select entry to see details)";
					if(_selectedEntry != null) {
						msg = string.Format(
							"Start: {0}\nDuration: {1}\n{2}\n{3}",
							_selectedEntry.Start.Subtract(timelineData.StartTime).TotalSeconds.ToString("0.00"),
							(_selectedEntry.End.HasValue ? _selectedEntry.End.Value.Subtract(_selectedEntry.Start).TotalSeconds.ToString("0.00") : ""),
							_selectedEntry.Name,
							_selectedEntry.ExtendedInfo
						);
					} 
					GUILayout.Label(msg, GUILayout.ExpandHeight(true));	
				}
				using(new GUILayout.VerticalScope(GUILayout.ExpandHeight(true), GUILayout.Width(100))) {
					using(new GUILayout.HorizontalScope()) {
						GUILayout.Label("Auto", GUILayout.ExpandWidth(true));
						_continuousScrolling = GUILayout.Toggle(_continuousScrolling, "", GUILayout.Width(16));
					}
					GUILayout.FlexibleSpace();
					using(new GUILayout.HorizontalScope()) {
						if(GUILayout.Button("-", GUILayout.Height(16))) {
							_scale = Mathf.Clamp(_scale * 0.9f, 0.1f, 100000.0f);
							_scaleText = _scale.ToString();
						}
						if(GUILayout.Button("+", GUILayout.Height(16))) {
							_scale = Mathf.Clamp(_scale * 1.1f, 0.1f, 100000.0f);
							_scaleText = _scale.ToString();
						}
					}
					_scaleText = GUILayout.TextField(_scaleText, GUILayout.ExpandWidth(true));
					float newScale;
					if(float.TryParse(_scaleText, out newScale)) {
						if(newScale > 0.1f && newScale != _scale) {
							_scale = newScale;
						}
					}
				}
			}
		}
		
		public void DrawTimeLine(ITimelineData timelineData, float scale=1.0f) {
			foreach(var entry in timelineData) {
				DrawTimeLineEntry(entry, timelineData.StartTime, timelineData.CurrentTime, scale, 0);
			}
		}

		void DrawTimeLineEntry(ITimelineEntry entry, DateTime startTime, DateTime currentTime, float scale, int index) {
			ControlContext.Context context = _controlContext.Get(entry);
			ContextVar<bool> foldOut = context.Get<bool>("foldOut", false);

			float left = (float)(entry.Start - startTime).TotalSeconds * scale;
			float right = (float)(CalculateEndTime(entry, currentTime) - startTime).TotalSeconds * scale;
			float width = Mathf.Max(right - left, 1);

			using(new GUILayout.HorizontalScope(index>0 ? GUIStyle.none : _style.Row, GUILayout.ExpandWidth(true))) {
				GUILayout.Space(left);

				using(new GUILayout.VerticalScope()) {
					using(new GUILayout.HorizontalScope(index == 0 ? _style.GetEntry(entry.State) : _style.GetChildEntry(entry.State), GUILayout.Width(width))) {
						if(entry.Children.Any()) {
							foldOut.Value = GUILayout.Toggle(foldOut.Value, GUIContent.none, _style.EntryToggle);
						} else {
							GUILayout.Space(16);
						}
						if(GUILayout.Button(entry.Name, _style.EntryText, GUILayout.MaxWidth(width - 10))) {
							_selectedEntry = entry;
						}
					}

					if(foldOut.Value) {
						foreach(var child in entry.Children) {
							DrawTimeLineEntry(child, entry.Start, currentTime, scale, ++index);
						}
					}
				}
			}
		}

		DateTime CalculateEndTime(ITimelineEntry entry, DateTime defaultEndTime) {
			return entry.LastUpdated;
			//return entry.End ?? defaultEndTime;
		}
		
		Rect CalculateScaledRect(DateTime startTime, DateTime beginTime, DateTime endTime, float scale) {
			float left = (float)(beginTime - startTime).TotalSeconds * scale;
			float right = (float)(endTime - startTime).TotalSeconds * scale;
			float width = right - left;
			width = Mathf.Max(width, 1);
			return new Rect(left, 0, width, 0);
		}

		class GUIScope : IDisposable {
			GUISkin _skin;
			
			public GUIScope(GUISkin newSkin) {
				_skin = GUI.skin;
				GUI.skin = newSkin;
			}

			public void Dispose() {
				GUI.skin = _skin;
			}
		}

		class ControlContext : Dictionary<string,object> {
			IDictionary<object, Context> _contextDict = new Dictionary<object, Context>();
		
			public Context Get(object obj) {
				Context cc;
				if(!_contextDict.TryGetValue(obj, out cc)) {
					cc = new Context();
					_contextDict.Add(obj, cc);
				}
				return cc;
			}
			
			public class Context {	
				IDictionary<string, object> _values = new Dictionary<string, object>();

				public ContextVar<T> Get<T>(string key, T defaultValue) {
					object val;
					if(!_values.TryGetValue(key, out val)) {
						val = (_values[key]=new ContextVar<T>(defaultValue));
					}
					return (ContextVar<T>)val;
				}
			}
		}
		
		class ContextVar<T> {
			public T Value { get; set; }
			
			public ContextVar(T value) {
				Value = value;
			}	
		}
	}
}
