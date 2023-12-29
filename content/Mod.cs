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
				var h_character = Spawner.CreateCharacter(ref region, ref random, h_origin, scope: Asset.Scope.Region, asset_flags: Asset.Flags.Recycle, h_player: player.h_player);
				if (Assert.Check(h_character.IsValid(), Assert.Level.Warn))
				{
					Spawner.SpawnCharacter(ref region, h_character, position: player.control.mouse.position, h_player: player.h_player, control: true);
				}
			}
		}

		[ChatCommand.Region("tp", "", creative: true)]
		public static void TeleportCommand(ref ChatCommand.Context context)
		{
			ref var region = ref context.GetRegion();
			Assert.NotNull(ref region);

			ref var player = ref context.GetPlayer();
			Assert.NotNull(ref player);

			ref var character = ref player.GetControlledCharacter().data;
			Assert.NotNull(ref character);

			var ent_root = character.ent_controlled;
			Assert.Check(ent_root.IsAlive());

			var ts = Timestamp.Now();

			//ent_root = ent_root.GetRoot(Relation.Type.Seat).GetRoot(Relation.Type.Instance).GetRoot(Relation.Type.Child);
			//App.WriteLine(ent_root.GetRoot(Relation.Type.Seat).GetRoot(Relation.Type.Child));

			ref var transform_root = ref ent_root.GetComponent<Transform.Data>();
			Assert.NotNull(ref transform_root);

			Span<Entity> entities = stackalloc Entity[32];
			ent_root.GetAllChildren(ref entities);
			Assert.Check(entities.Length > 0);

			ent_root.RemoveRelation(Entity.Wildcard, Relation.Type.Seat);
			ent_root.RemoveRelation(Entity.Wildcard, Relation.Type.Child);
			ent_root.RemoveRelation(Entity.Wildcard, Relation.Type.Stored);
			ent_root.RemoveRelation(Entity.Wildcard, Relation.Type.Rope);

			var mouse_pos = player.control.mouse.position;

			foreach (var ent in entities)
			{
				//var ref_transform = ent.GetComponentWithOwner<Transform.Data>(Relation.Type.Instance);
				//if (ref_transform.entity != ent) continue;

				if (ent.GetComponentOwner<Transform.Data>(Relation.Type.Instance) != ent) continue;

				ref var transform = ref ent.GetComponent<Transform.Data>();
				//ref var transform = ref ref_transform.data;
				if (transform.IsNotNull())
				{
					var offset = transform.position - transform_root.position;

					transform.SetPosition(mouse_pos + offset);
					transform.Sync(ent, true);

					//ref var physics = ref ent.GetComponent<Physics.Data>();
					//if (physics.IsNotNull())
					//{
					//	physics.position = mouse_pos + offset;
					//	physics.elapsed = 0.00f;
					//	physics.Sync(ent, true);
					//}

					//App.WriteLine(ent);
				}
			}

			var ts_elapsed = ts.GetMilliseconds();
			App.WriteLine($"Teleported in {ts_elapsed:0.0000} ms");
		}
#endif
	}
}

