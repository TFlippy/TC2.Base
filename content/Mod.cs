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

					var suffix = identifier.SliceAfterLast('.', out _);
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

#if SERVER
		[ChatCommand.Region("origin", "", creative: true)]
		public static void OriginCommand(ref ChatCommand.Context context, IOrigin.Handle h_origin, bool force_new = false)
		{
			ref var region = ref context.GetRegion();
			Assert.NotNull(ref region);

			ref var player = ref context.GetPlayer();
			Assert.NotNull(ref player);

			var random = XorRandom.New(true);

			//var h_origin = (IOrigin.Handle)origin;

			ref var character = ref player.GetControlledCharacter().data;
			if (!force_new && character.IsNotNull() && character.ent_controlled.IsAlive())
			{
				ref var character_data = ref character.character_id.GetData(out var character_asset);
				Assert.NotNull(ref character_data);

				if (Spawner.TryApplyOrigin(ref region, ref random, h_origin, ref character_data))
				{
					App.WriteLine("ok");
				}
			}
			else
			{
				var h_character = Spawner.CreateCharacter(ref region, ref random, h_origin);
				if (Assert.Check(h_character.IsValid(), Assert.Level.Warn))
				{
					Spawner.SpawnCharacter(ref region, h_character, position: player.control.mouse.position, h_player: player.h_player, control: true);
				}
			}
		}
#endif
	}
}

