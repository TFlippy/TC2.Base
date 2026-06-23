
namespace TC2.Base.Components
{
	public static partial class Shield
	{
		[Flags]
		public enum Flags: uint
		{
			None = 0u,
		}

		[IComponent.Data(Net.SendType.Unreliable, IComponent.Scope.Region)]
		public partial struct Data: IComponent
		{
			public Shield.Flags flags;
		}

		[Shitcode]
		[ISystem.Add(ISystem.Mode.Single, ISystem.Scope.Region)]
		[ISystem.VeryLateUpdate(ISystem.Mode.Single, ISystem.Scope.Region, interval: 0.50f)]
		public static void UpdateHoldable([Source.Owned] in Shield.Data shield, [Source.Owned] ref Holdable.Data holdable)
		{
			holdable.hints.SetFlag(NPC.ItemHints.Armor | NPC.ItemHints.Defensive | NPC.ItemHints.Melee | NPC.ItemHints.Short_Range, true);
		}
	}
}
