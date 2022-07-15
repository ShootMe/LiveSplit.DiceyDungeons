using System.ComponentModel;
namespace LiveSplit.DiceyDungeons {
	public enum LogObject {
		CurrentSplit,
		Pointers,
		Floor,
		Character,
		HP,
		Level,
		Dice,
		Special,
		PlayerTurn,
		Kills,
		Gold,
		XP,
		Enemy,
		EnemyHP,
		EnemyLevel,
		EnemyDice
	}
	public enum SplitName {
		[Description("Manual Split (Not Automatic)"), ToolTip("Does not split automatically. Use this for custom splits not yet defined.")]
		ManualSplit,

		[Description("Floor 1"), ToolTip("Splits when leaving Floor 1")]
		Floor1,
		[Description("Floor 2"), ToolTip("Splits when leaving Floor 2")]
		Floor2,
		[Description("Floor 3"), ToolTip("Splits when leaving Floor 3")]
		Floor3,
		[Description("Floor 4"), ToolTip("Splits when leaving Floor 4")]
		Floor4,
		[Description("Floor 5"), ToolTip("Splits when leaving Floor 5")]
		Floor5,
		[Description("Boss"), ToolTip("Splits when beating the Boss")]
		Boss,
		[Description("Enemy Defeated"), ToolTip("Splits when beating any enemy")]
		EnemyDefeated
	}
}