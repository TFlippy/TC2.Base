using TC2.Base.Components;

namespace TC2.Base
{
	public sealed partial class BaseMod: Mod
	{
		protected override void OnRegister(ModContext context)
		{
			Augment.OnInitialize += RegisterAugments;
			Augment.OnInitialize += RegisterGunAugments;
			Augment.OnInitialize += RegisterMeleeAugments;
			Augment.OnInitialize += RegisterDrillAugments;
			Augment.OnInitialize += RegisterMedkitAugments;
			Augment.OnInitialize += RegisterExplosiveAugments;
			Augment.OnInitialize += RegisterArmorAugments;

			Material.OnPostInitialize += Essence.Init;

//#if SERVER
//			// TODO: hack, add a way to handle this with attributes
//			Player.OnCreate += OnCreatePlayer;
//#endif
		}

		//private static void OnCreatePlayer(ref Region.Data region, ref Player.Data player)
		//{
		//	player.ent_player.AddComponent<Selection.Data>();
		//}

		protected override void OnInitialize(ModContext context)
		{

		}

		protected override void OnConfigure(ModContext context)
		{

		}
	}
}

