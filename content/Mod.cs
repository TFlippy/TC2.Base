using System.Text;
using TC2.Base.Components;

namespace TC2.Base
{
	public sealed partial class BaseMod: Mod
	{
		[Shitcode]
		protected override void OnRegister(ref ModContext context)
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

			//IMaterial.Database.AddAssetPostProcessor(static (definition, ref data) =>
			//{
			//	if (data.flags.HasAny(Material.Flags.Essence))
			//	{
			//		var identifier = definition.identifier;

			//		var suffix = identifier.SliceAfterLast('.', out _);
			//		if (!suffix.IsEmpty)
			//		{
			//			var h_essence = new IEssence.Handle(suffix);
			//			if (h_essence.IsValid())
			//			{
			//				Essence.material_to_essence[definition.GetHandle()] = h_essence;
			//				Essence.essence_to_material[h_essence] = definition.GetHandle();
			//			}
			//		}
			//	}
			//});
		}

		protected override void OnInitialize(ref ModContext context)
		{

		}

#if SERVER
		[ChatCommand.Region("origin", "", creative: true)]
		public static void OriginCommand(ref ChatCommand.Context context, IOrigin.Handle h_origin, bool force_new = false)
		{
			ref var region = ref context.GetRegionCommon();
			Assert.IsNotNull(ref region);

			//ref var player = ref context.GetPlayerData();
			//Assert.IsNotNull(ref player);

			ref var origin_data = ref h_origin.GetData(out var origin_asset);
			//Assert.IsNotNull(ref origin_data);

			if (origin_data.IsNull())
			{
				var origins_span = IOrigin.Database.GetAssetsSpan();
				var sb = new StringBuilder();

				sb.AppendLine("Invalid origin, use one of these:");
				foreach (var origin in origins_span)
				{
					sb.AppendPrefixed(origin.identifier, "- ");
					sb.AppendLine();
				}

				context.text_out = sb.ToString();
				return;
			}


			ref var player_data = ref context.GetPlayer(out var player_asset);
			Assert.IsNotNull(ref player_data);

			var random = XorRandom.New(true);

			//var h_origin = (IOrigin.Handle)origin;

			//ref var character = ref player.GetControlledCharacter().data;
			if (!force_new) // && character.IsNotNull() && character.ent_controlled.IsAlive())
			{
				ref var current_character_data = ref context.GetCharacter(out var current_character_asset);
				Assert.IsNotNull(ref current_character_data);

				context.text_out = $"Changing current character's origin from \"{current_character_data.origin}\" to \"{h_origin}\"...";

				current_character_data.origin = origin_asset;
				current_character_asset.Sync();

				//throw new NotImplementedException();
				//ref var character_data = ref character.character_id.GetData(out var character_asset);
				//Assert.NotNull(ref character_data);

				//if (Spawner.TryApplyOrigin(ref random, region.GetLocationHandle(), h_origin, ref character_data))
				//{
				//	App.WriteLine("ok");
				//}
			}
			else
			{
				var h_faction = origin_data.faction;
				var h_character = Spawner.CreateCharacter(region: ref region, random: ref random, h_origin: h_origin, scope: Asset.Scope.Region, asset_flags: Asset.Flags.Recycle, h_player: player_asset, h_faction: h_faction);
				context.text_out = $"Created new character \"{h_character}\" with \"{h_origin}\" origin.";

				if (Assert.Check(h_character.IsValid(), Assert.Level.Warn))
				{
					Spawner.TryGenerateKits(ref random, h_character);

					if (!region.IsGlobal())
					{
						Spawner.SpawnCharacter(ref region.AsRegion(), h_character, position: context.GetTargetPosition(), h_player: player_asset, h_faction: h_faction, control: true).ContinueWith((ent) =>
						{
							ref var character = ref h_character.GetData();
							if (character.IsNotNull())
							{
								Loadout.Spawn(ent, character.kits, money: character.money);
							}
						});
					}
				}
			}
		}

		[ChatCommand.Region("control", "", creative: true)]
		public static void ControlCommand(ref ChatCommand.Context context)
		{
			ref var region = ref context.GetRegion();
			Assert.IsNotNull(ref region);

			ref var player = ref context.GetPlayerData();
			Assert.IsNotNull(ref player);

			var ent_target = context.GetTargetEntity();
			Assert.Check(ent_target.IsAlive());

			var ent_target_root = ent_target.GetRoot();
			Assert.Check(ent_target_root.IsAlive());

			ref var character = ref ent_target_root.GetComponentWithOwner<Character.Data>(Relation.Type.Instance, out var ent_character, include_self: true);
			if (character.IsNotNull())
			{
				//throw new NotImplementedException();
				ref var character_data = ref ent_character.GetAssetData<ICharacter.Data>(); // character.character_id.GetData(out var character_asset);
				Assert.IsNotNull(ref character_data);

				player.SetControlledCharacter(ent_character);
			}
			//else
			//{
			//	character = ref player.GetControlledCharacter().data;
			//	if (character.IsNotNull())
			//	{
			//		ent_target_root.SetAssociatedCharacter(character.ent_character);
			//	}
			//}
		}

		[ChatCommand.Region("tp", "", creative: true)]
		public static void TeleportCommand(ref ChatCommand.Context context)
		{
			ref var region = ref context.GetRegion();
			Assert.IsNotNull(ref region);

			ref var player = ref context.GetPlayerData();
			Assert.IsNotNull(ref player);

			//ref var character = ref player.GetControlledCharacter().data;
			//Assert.NotNull(ref character);

			//var ent_root = context.GetTargetEntity(); // character.ent_controlled;
			var ent_root = player.ent_controlled;
			Assert.Check(ent_root.IsAlive());

			var ts = Timestamp.Now();

			//ent_root = ent_root.GetRoot(Relation.Type.Seat).GetRoot(Relation.Type.Instance).GetRoot(Relation.Type.Child);
			//App.WriteLine(ent_root.GetRoot(Relation.Type.Seat).GetRoot(Relation.Type.Child));

			ref var transform_root = ref ent_root.GetComponent<Transform.Data>();
			Assert.IsNotNull(ref transform_root);

			Span<Entity> entities = stackalloc Entity[32];
			ent_root.GetAllChildren(ref entities);
			Assert.Check(entities.Length > 0);

			ent_root.RemoveRelation(Entity.Wildcard, Relation.Type.Seat);
			ent_root.RemoveRelation(Entity.Wildcard, Relation.Type.Child);
			ent_root.RemoveRelation(Entity.Wildcard, Relation.Type.Stored);
			ent_root.RemoveRelation(Entity.Wildcard, Relation.Type.Rope);

			var target_pos = context.GetTargetPosition();
			
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

					transform.SetPosition(target_pos + offset);
					transform.Modified(ent, true);

					ent.MarkModified<Body.Data>(true);

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

		[ChatCommand.Global("move", "", creative: true)]
		public static void MoveCommand(ref ChatCommand.Context context)
		{
			ref var region = ref context.GetRegionCommon();
			Assert.IsNotNull(ref region);

			var ent_target = context.GetTargetEntity();
			Assert.Check(ent_target.IsAlive());

			var pos = context.GetTargetPosition();

			ref var transform = ref ent_target.GetComponent<Transform.Data>();
			if (transform.IsNotNull())
			{
				transform.SetPosition(pos);
				transform.Modified(ent_target, true);

				ent_target.MarkModified<Body.Data>(true);
			}
		}
#endif
	}
}

