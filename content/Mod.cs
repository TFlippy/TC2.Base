using TC2.Base.Components;

namespace TC2.Base
{
	public sealed partial class BaseMod: Mod
	{
		[Shitcode]
		protected override void OnRegister(ModContext context)
		{
			Augment.OnInitialize += RegisterAugments;
			Augment.OnInitialize += RegisterGunAugments;
			Augment.OnInitialize += RegisterMeleeAugments;
			Augment.OnInitialize += RegisterDrillAugments;
			Augment.OnInitialize += RegisterMedkitAugments;
			Augment.OnInitialize += RegisterExplosiveAugments;
			Augment.OnInitialize += RegisterArmorAugments;
			Augment.OnInitialize += RegisterWrenchAugments;
			Augment.OnInitialize += RegisterVehicleAugments;

#if CLIENT
			//HitEffects.Init();
#endif

			IMaterial.Database.AddAssetPostProcessor((IMaterial.Definition definition, ref IMaterial.Data data) =>
			{
				if (data.flags.HasAny(Material.Flags.Essence))
				{
					var identifier = definition.identifier;

					var suffix = identifier.SliceAfterLast('.');
					if (!suffix.IsEmpty)
					{
						var h_essence = new IEssence.Handle(suffix);
						if (h_essence.IsValid())
						{
							Essence.material_to_essence[definition.GetHandle()] = h_essence;
							Essence.essence_to_material[h_essence] = definition.GetHandle();
						}
					}
				}
			});
		}

		protected override void OnInitialize(ModContext context)
		{

		}
	}
}

