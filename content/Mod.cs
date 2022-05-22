using TC2.Base.Components;

namespace TC2.Base
{
	public sealed partial class ModInstance: Mod
	{
		protected override void OnRegister(ModContext context)
		{
			Modification.OnInitialize += RegisterModifications;
			Modification.OnInitialize += RegisterGunModifications;
			Modification.OnInitialize += RegisterMeleeModifications;
			Modification.OnInitialize += RegisterDrillModifications;
			Modification.OnInitialize += RegisterMedkitModifications;
			Modification.OnInitialize += RegisterBodyModifications;
			Modification.OnInitialize += RegisterExplosiveModifications;
			Modification.OnInitialize += RegisterArmorModifications;

			Material.OnPostInitialize += Essence.Init;

#if SERVER
			// TODO: hack, add a way to handle this with attributes
			Player.OnCreate += OnCreatePlayer;
#endif
		}

		private static void OnCreatePlayer(ref Region.Data region, ref Player.Data player)
		{
			player.ent_player.AddComponent<Selection.Data>();
		}

		protected override void OnInitialize(ModContext context)
		{

		}

		protected override void OnConfigure(ModContext context)
		{

		}
	}
}

