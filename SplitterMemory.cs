using LiveSplit.Memory;
using System;
using System.Diagnostics;
namespace LiveSplit.DiceyDungeons {
	public enum Player {
		User = 0x1b25f00,
		Enemy = 0x1b25f08
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
		public int PlayerXP() {
			return PlayerXPNeeded() - Program.Read<int>(BaseAddress, 0x1b1a908);
		}
		public int PlayerXPNeeded() {
			return Program.Read<int>(BaseAddress, 0x1b1a90c);
		}
		public int Floor() {
			return Program.Read<int>(BaseAddress, 0x1b25ecc) + 1;
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