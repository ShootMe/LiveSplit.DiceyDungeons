using LiveSplit.Memory;
using System;
using System.Diagnostics;
namespace LiveSplit.DiceyDungeons {
	public enum Player {
		User = 0x1D267E8,
		Enemy = 0x1D267F0
	}
	public partial class SplitterMemory {
		public Process Program { get; set; }
		public bool IsHooked { get; set; } = false;
		public DateTime LastHooked;
		public IntPtr BaseAddress { get; set; }

		public SplitterMemory() {
			LastHooked = DateTime.MinValue;
			BaseAddress = IntPtr.Zero;
		}
		public bool HasPointer(Player player = Player.User) {
			return Program.Read<ulong>(BaseAddress, (int)player) != 0;
		}
		public int HP(Player player = Player.User) {
			return Program.Read<int>(BaseAddress, (int)player, 0x1b0);
		}
		public int MaxHP(Player player = Player.User) {
			return Program.Read<int>(BaseAddress, (int)player, 0x1b4);
		}
		public int Dice(Player player = Player.User) {
			return Program.Read<int>(BaseAddress, (int)player, 0x190);
		}
		public int ActionsThisTurn(Player player = Player.User) {
			return Program.Read<int>(BaseAddress, (int)player, 0x1e0);
		}
		public int Kills(Player player = Player.User) {
			return Program.Read<int>(BaseAddress, (int)player, 0x1c4);
		}
		public int Special(Player player = Player.User) {
			return Program.Read<int>(BaseAddress, (int)player, 0x200);
		}
		public int SpecialValue(Player player = Player.User) {
			return Program.Read<int>(BaseAddress, (int)player, 0x204);
		}
		public int Level(Player player = Player.User) {
			return Program.Read<int>(BaseAddress, (int)player, 0x1c0);
		}
		public string Name(Player player = Player.User) {
			return Program.ReadAscii((IntPtr)Program.Read<ulong>(BaseAddress, (int)player, 0x68));
		}
		public int Gold(Player player = Player.User) {
			return Program.Read<int>(BaseAddress, (int)player, 0x1cc);
		}
		public bool Turn(Player player = Player.User) {
			return !Program.Read<bool>(BaseAddress, (int)player, 0x20);
        }

        /// <summary>
        /// The index of the character selected in the menu.
        /// </summary>
        /// <returns>0: Warrior, 1: Thief, 2: Robot, 3: Inventor, 4: Witch, 5: Jester, 6: Backstage.<br />
		/// If Halloween, 0: Warrior, 1: Inventor, 2: Witch.<br />
		/// If Reunion, 0: Thief, 1: Jester, 2: Warrior, 3: Witch, 4: Robot, 5: Inventor</returns>
        public int CharacterSelectedIndex()
        {
            return Program.Read<int>(BaseAddress, 0x1D1B1D8);
        }

        /// <summary>
        /// The last episode selected in the episode selection menu.
        /// </summary>
        public int EpisodeSelected()
        {
            return Program.Read<int>(BaseAddress, 0x1D1AC14) + 1;
        }

		/// <summary>
		/// Returns the current amount of XP in the EXP bar.
		/// </summary>
		public int PlayerXP() {
			return Program.Read<int>(BaseAddress, 0x1D1A804) - PlayerXPNeeded();
		}
		public int PlayerXPNeeded() {
			return Program.Read<int>(BaseAddress, 0x1D1A800);
		}
		public int Floor() {
			return Program.Read<int>(BaseAddress, 0x1D26760) + 1;
		}

		public bool HookProcess() {
			IsHooked = Program != null && !Program.HasExited;
			if (!IsHooked && DateTime.Now > LastHooked.AddSeconds(1)) {
				LastHooked = DateTime.Now;
				Process[] processes = Process.GetProcessesByName("DiceyDungeons");
				Program = processes != null && processes.Length > 0 ? processes[0] : null;

				if (Program != null && !Program.HasExited) {
					MemoryReader.Update64Bit(Program);
					BaseAddress = Program.MainModule.BaseAddress;
					IsHooked = true;
				}
			}

			return IsHooked;
		}
		public void Dispose() {
			if (Program != null) {
				Program.Dispose();
			}
		}
	}
}