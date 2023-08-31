
namespace TC2.Base.Components
{
	public static partial class Shield
	{
		[IComponent.Data(Net.SendType.Reliable, region_only: true)]
		public partial struct Data: IComponent
		{

		}

		[ISystem.Add(ISystem.Mode.Single)]
		[ISystem.VeryLateUpdate(ISystem.Mode.Single, interval: 0.50f)]
		public static void UpdateHoldable([Source.Owned] in Shield.Data shield, [Source.Owned] ref Holdable.Data holdable)
		{
			holdable.hints.SetFlag(NPC.ItemHints.Armor | NPC.ItemHints.Defensive | NPC.ItemHints.Melee | NPC.ItemHints.Short_Range, true);
		}
	}
}
