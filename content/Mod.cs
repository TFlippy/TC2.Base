
namespace TC2.Base
{
	public sealed partial class ModInstance: Mod
	{
		protected override void OnRegister(ModContext context)
		{
			Modification.OnInitialize += RegisterGunModifications;
			Modification.OnInitialize += RegisterHealthModifications;
			Modification.OnInitialize += RegisterBodyModifications;
			Modification.OnInitialize += RegisterFuseModifications;
			Modification.OnInitialize += RegisterExplosiveModifications;
			Modification.OnInitialize += RegisterMeleeModifications;
			Modification.OnInitialize += RegisterDrillModifications;
			Modification.OnInitialize += RegisterMedkitModifications;
			Modification.OnInitialize += RegisterOverheatModifications;
			Modification.OnInitialize += RegisterModifications;
		}

		protected override void OnInitialize(ModContext context)
		{
			
		}

		protected override void OnConfigure(ModContext context)
		{

		}
	}
}

