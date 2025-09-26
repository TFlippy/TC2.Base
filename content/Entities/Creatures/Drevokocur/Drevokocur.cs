namespace TC2.Base.Components
{
	public static class Drevokocur
	{
		[ISystem.Update.A(ISystem.Mode.Single, ISystem.Scope.Region), HasTag("dead", false, Source.Modifier.Owned), HasTag("drevokocur", true, Source.Modifier.Owned)]
		public static void OnUpdate(ISystem.Info info, ref XorRandom random,
		[Source.Owned] ref NPC.Data npc, [Source.Owned, Override] ref Animal.Data animal_override,
		[Source.Owned, Override] ref Organic.Data organic_override)
		{

		}
	}
}
