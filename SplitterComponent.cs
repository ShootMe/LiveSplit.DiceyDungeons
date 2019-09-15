#if !DebugInfo
using LiveSplit.Model;
using LiveSplit.UI;
using LiveSplit.UI.Components;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;
using System.Xml;
namespace LiveSplit.DiceyDungeons {
	public class SplitterComponent : IComponent {
		public string ComponentName { get { return "Dicey Dungeons Autosplitter"; } }
		public TimerModel Model { get; set; }
		public IDictionary<string, Action> ContextMenuControls { get { return null; } }
		private static string LOGFILE = "_DiceyDungeons.txt";
		private Dictionary<LogObject, string> currentValues = new Dictionary<LogObject, string>();
		private SplitterMemory mem;
		private SplitterSettings settings;
		private int currentSplit = -1, lastLogCheck, lastFloor;
		private bool hasLog, lastHasPointer;
		private Thread updateLoop;

		public SplitterComponent(LiveSplitState state) {
			mem = new SplitterMemory();
			settings = new SplitterSettings();
			foreach (LogObject key in Enum.GetValues(typeof(LogObject))) {
				currentValues[key] = "";
			}

			if (state != null) {
				Model = new TimerModel() { CurrentState = state };
				Model.InitializeGameTime();
				Model.CurrentState.IsGameTimePaused = true;
				state.OnReset += OnReset;
				state.OnPause += OnPause;
				state.OnResume += OnResume;
				state.OnStart += OnStart;
				state.OnSplit += OnSplit;
				state.OnUndoSplit += OnUndoSplit;
				state.OnSkipSplit += OnSkipSplit;

				updateLoop = new Thread(UpdateLoop);
				updateLoop.IsBackground = true;
				updateLoop.Start();
			}
		}

		private void UpdateLoop() {
			while (updateLoop != null) {
				try {
					GetValues();
				} catch (Exception ex) {
					WriteLog(ex.ToString());
				}
				Thread.Sleep(8);
			}
		}
		public void GetValues() {
			if (!mem.HookProcess()) { return; }

			if (Model != null) {
				HandleSplits();
			}

			LogValues();
		}
		private void HandleSplits() {
			bool shouldSplit = false;

			if (currentSplit == -1) {
				bool hasPointer = mem.HasPointer();
				shouldSplit = lastHasPointer != hasPointer && DateTime.Now > mem.LastHooked.AddSeconds(5);
				lastHasPointer = hasPointer;
			} else if (Model.CurrentState.CurrentPhase == TimerPhase.Running) {
				int floor = mem.Floor();

				if (currentSplit < Model.CurrentState.Run.Count && currentSplit < settings.Splits.Count) {
					SplitName split = settings.Splits[currentSplit];

					switch (split) {
						case SplitName.Floor1: shouldSplit = lastFloor == 1 && floor == 2; break;
						case SplitName.Floor2: shouldSplit = lastFloor == 2 && floor == 3; break;
						case SplitName.Floor3: shouldSplit = lastFloor == 3 && floor == 4; break;
						case SplitName.Floor4: shouldSplit = lastFloor == 4 && floor == 5; break;
						case SplitName.Floor5: shouldSplit = lastFloor == 5 && floor == 6; break;
						case SplitName.Boss: shouldSplit = floor == 6 && mem.HasPointer(Player.Enemy) && mem.HP(Player.Enemy) <= 0; break;
					}
				} else {
					shouldSplit = floor == 6 && mem.HasPointer(Player.Enemy) && mem.HP(Player.Enemy) <= 0;
				}

				lastFloor = floor;
			}

			HandleSplit(shouldSplit, false);
		}
		private void HandleSplit(bool shouldSplit, bool shouldReset = false) {
			if (shouldReset) {
				if (currentSplit >= 0) {
					Model.Reset();
				}
			} else if (shouldSplit) {
				if (currentSplit == -1) {
					Model.Start();
				} else {
					Model.Split();
				}
			}
		}
		private void LogValues() {
			if (lastLogCheck == 0) {
				hasLog = File.Exists(LOGFILE);
				lastLogCheck = 300;
			}
			lastLogCheck--;

			if (hasLog || !Console.IsOutputRedirected) {
				string prev = string.Empty, curr = string.Empty;
				foreach (LogObject key in Enum.GetValues(typeof(LogObject))) {
					prev = currentValues[key];

					switch (key) {
						case LogObject.CurrentSplit: curr = currentSplit.ToString(); break;
						case LogObject.Pointers: curr = mem.HasPointer().ToString(); break;
						case LogObject.Floor: curr = mem.Floor().ToString(); break;
						case LogObject.HP: curr = mem.HP().ToString(); break;
						case LogObject.Dice: curr = mem.Dice().ToString(); break;
						case LogObject.Kills: curr = mem.Kills().ToString(); break;
						case LogObject.Gold: curr = mem.Gold().ToString(); break;
						case LogObject.Level: curr = mem.Level().ToString(); break;
						case LogObject.XP: curr = mem.PlayerXP().ToString(); break;
						case LogObject.Character: curr = mem.Name(); break;
						case LogObject.Enemy: curr = mem.Name(Player.Enemy); break;
						case LogObject.EnemyHP: curr = mem.HP(Player.Enemy).ToString(); break;
						case LogObject.EnemyLevel: curr = mem.Level(Player.Enemy).ToString(); break;
						case LogObject.EnemyDice: curr = mem.Dice(Player.Enemy).ToString(); break;
						case LogObject.Special: curr = (mem.SpecialValue() - mem.Special() == 0).ToString(); break;
						case LogObject.PlayerTurn: curr = mem.Turn().ToString(); break;
						default: curr = string.Empty; break;
					}

					if (string.IsNullOrEmpty(prev)) { prev = string.Empty; }
					if (string.IsNullOrEmpty(curr)) { curr = string.Empty; }
					if (!prev.Equals(curr)) {
						WriteLog(DateTime.Now.ToString(@"HH\:mm\:ss.fff") + (Model != null ? " | " + Model.CurrentState.CurrentTime.RealTime.Value.ToString("G").Substring(3, 11) : "") + ": " + key.ToString() + ": ".PadRight(16 - key.ToString().Length, ' ') + prev.PadLeft(25, ' ') + " -> " + curr);

						currentValues[key] = curr;
					}
				}
			}
		}

		public void Update(IInvalidator invalidator, LiveSplitState lvstate, float width, float height, LayoutMode mode) {
		}
		public void OnReset(object sender, TimerPhase e) {
			currentSplit = -1;
			lastHasPointer = mem.HasPointer();
			WriteLog("---------Reset----------------------------------");
		}
		public void OnResume(object sender, EventArgs e) {
			WriteLog("---------Resumed--------------------------------");
		}
		public void OnPause(object sender, EventArgs e) {
			WriteLog("---------Paused---------------------------------");
		}
		public void OnStart(object sender, EventArgs e) {
			currentSplit = 0;
			Model.CurrentState.SetGameTime(TimeSpan.Zero);
			Model.CurrentState.IsGameTimePaused = true;
			WriteLog("---------New Game " + Assembly.GetExecutingAssembly().GetName().Version.ToString(3) + "-------------------------");
		}
		public void OnUndoSplit(object sender, EventArgs e) {
			currentSplit--;
			WriteLog("---------Undo-----------------------------------");
		}
		public void OnSkipSplit(object sender, EventArgs e) {
			currentSplit++;
			WriteLog("---------Skip-----------------------------------");
		}
		public void OnSplit(object sender, EventArgs e) {
			currentSplit++;
			WriteLog("---------Split-----------------------------------");
		}
		private void WriteLog(string data) {
			if (hasLog || !Console.IsOutputRedirected) {
				if (Console.IsOutputRedirected) {
					using (StreamWriter wr = new StreamWriter(LOGFILE, true)) {
						wr.WriteLine(data);
					}
				} else {
					Console.WriteLine(data);
				}
			}
		}

		public Control GetSettingsControl(LayoutMode mode) { return settings; }
		public void SetSettings(XmlNode document) { settings.SetSettings(document); }
		public XmlNode GetSettings(XmlDocument document) { return settings.UpdateSettings(document); }
		public void DrawHorizontal(Graphics g, LiveSplitState state, float height, Region clipRegion) { }
		public void DrawVertical(Graphics g, LiveSplitState state, float width, Region clipRegion) { }
		public float HorizontalWidth { get { return 0; } }
		public float MinimumHeight { get { return 0; } }
		public float MinimumWidth { get { return 0; } }
		public float PaddingBottom { get { return 0; } }
		public float PaddingLeft { get { return 0; } }
		public float PaddingRight { get { return 0; } }
		public float PaddingTop { get { return 0; } }
		public float VerticalHeight { get { return 0; } }
		public void Dispose() {
			if (updateLoop != null) {
				updateLoop = null;
			}
			if (Model != null) {
				Model.CurrentState.OnReset -= OnReset;
				Model.CurrentState.OnPause -= OnPause;
				Model.CurrentState.OnResume -= OnResume;
				Model.CurrentState.OnStart -= OnStart;
				Model.CurrentState.OnSplit -= OnSplit;
				Model.CurrentState.OnUndoSplit -= OnUndoSplit;
				Model.CurrentState.OnSkipSplit -= OnSkipSplit;
			}
		}
	}
}
#endif