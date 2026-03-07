namespace TC2.Base.Components
{
	public static class Extinguisher
	{
		[IComponent.Data(Net.SendType.Unreliable, IComponent.Scope.Region)]
		public struct Data(): IComponent
		{
			[Editor.Picker.Position(true)]
			public Vector2 offset;
			public float max_length = 3.00f;
			public float thickness = 1.00f;
			public float damage = 600.00f;
			public float damage_extra = 1100.00f;
			public float fuel_usage = 0.10f;

			public IMaterial.Handle h_material_fuel;
			public Sound.Handle sound_start;
			//public Sound.Handle sound_end;

			[Net.Ignore, Save.Ignore] public float t_last_use;
			[Net.Ignore, Save.Ignore] public float t_last_sound;
			[Net.Ignore, Save.Ignore] public float t_next_use;
		}

		internal static readonly Texture.Handle bigger_smoke_light = "BiggerSmoke_Light";
		internal static readonly Texture.Handle metal_spark = "Metal_Spark";
		internal static readonly Texture.Handle metal_spark_01 = "metal_spark.01";

		[ISystem.Update.A(ISystem.Mode.Single, ISystem.Scope.Region)]
		public static void Update(ISystem.Info info, Entity entity, ref Region.Data region, ref XorRandom random,
		[Source.Owned] in Transform.Data transform, [Source.Owned] ref Body.Data body, [Source.Owned] in Control.Data control, [Source.Owned] ref Extinguisher.Data extinguisher,
		[Source.Owned, Pair.Component<Extinguisher.Data>] ref Inventory1.Data inventory)
		{
			var time = info.WorldTime;
			var lmb = control.mouse.GetKey(Mouse.Key.Left);
			var any = lmb;

			if (any)
			{
				any &= inventory.resource == extinguisher.h_material_fuel;
			}

			if (any)
			{
				var aim_dir = transform.GetDirection();

				var pos_a = transform.LocalToWorld(extinguisher.offset) + (aim_dir * 0.50f);
				var pos_b = pos_a + (aim_dir * extinguisher.max_length);

				var ent_parent = body.GetParent();
				var h_faction = body.GetFaction();

				body.AddForce(aim_dir * -body.GetMass() * 25.00f);

				if (time >= extinguisher.t_next_use)
				{
					extinguisher.t_next_use = time + random.NextFloatExtra(0.14f, 0.07f);

#if SERVER
					inventory.Remove(extinguisher.h_material_fuel, extinguisher.fuel_usage);
#endif

					Span<LinecastResult> results = FixedArray.CreateSpan16NoInit<LinecastResult>(out var results_buffer);

					if (region.TryLinecastAll(world_position_start: pos_a, world_position_end: pos_b, radius: extinguisher.thickness * 0.50f, hits: ref results, layer: Physics.Layer.Fire, mask: Physics.Layer.World | Physics.Layer.Organic | Physics.Layer.Flammable | Physics.Layer.Heatable | Physics.Layer.Destructible,
					exclude: Physics.Layer.Essence | Physics.Layer.Water | Physics.Layer.Fire | Physics.Layer.Stored | Physics.Layer.Bounds))
					{
						results.SortByDistance();

						ref var terrain = ref region.GetTerrain();
						foreach (ref var result in results)
						{
							var area = result.GetShapeArea();
							if (area <= 0.00f) continue;

							var area_inv = (area * 2).RcpFast01();

							ref var block = ref result.GetBlock(out var h_block);
							if (block.IsNotNull())
							{
//								var dirty_flags = Chunk.DirtyFlags.None;
//#if SERVER
//								dirty_flags |= Chunk.DirtyFlags.Collider | Chunk.DirtyFlags.Texture | Chunk.DirtyFlags.Stability | Chunk.DirtyFlags.Colors | Chunk.DirtyFlags.Sync;
//#endif

//								var pos_hit = result.world_position;

//								terrain.IterateSquareLine(
//									world_position_a: pos_hit - (aim_dir * 0.25f),
//									world_position_b: pos_hit + (aim_dir * random.NextFloatRange(0.20f, 2.50f)).RotateByRad(random.NextFloat(0.30f)),
//									thickness: extinguisher.thickness * random.NextFloatRange(0.50f, 1.20f),
//									argument: ref args,
//									func: Func,
//									dirty_flags: dirty_flags);
							}
							else
							{
#if SERVER
								var damage = random.NextFloatExtra(extinguisher.damage, extinguisher.damage_extra) * area_inv;
								var skip_damage = false;

								if (result.layer.HasAny(Physics.Layer.Flammable))
								{
									//skip_damage = true;
									Fire.Ignite(ent_target: result.entity,
										step: 8.00f * area_inv,
										intensity: 100.00f,
										damage: damage,
										damage_type: Damage.Type.Phlogiston,
										temperature: 2700.00f,
										flags: Fire.Flags.None);

									damage *= 0.65f;
								}

								if (result.layer.HasAny(Physics.Layer.Heatable))
								{
									ref var heat_state = ref result.entity.GetComponent<Heat.State>();
									if (heat_state.IsNotNull())
									{
										//skip_damage = true;

										heat_state.AddEnergy(damage * 5.00f);
										heat_state.Sync(result.entity, true);

										damage *= 0.50f;
									}
								}

								if (!skip_damage)
								{
									Damage.Hit(ent_attacker: entity, ent_owner: default, ent_target: result.entity,
									position: result.world_position, velocity: aim_dir * random.NextFloatExtra(2.00f, 8.00f), normal: result.normal,
									damage_integrity: damage, damage_durability: damage * 0.70f, damage_terrain: damage,
									target_material_type: result.material_type, damage_type: Damage.Type.Phlogiston,
									flags: Damage.Flags.No_Loot_Pickup | Damage.Flags.No_Loot_Notification,
									yield: 1.10f, size: 1.20f, impulse: 100.00f, faction_id: h_faction, pain: 1.10f, stun: 0.95f);
								}


								//Damage.Hit(ent_attacker: entity, ent_owner: ent_parent, ent_target: result.entity,
								//position: result.world_position, velocity: aim_dir * random.NextFloatExtra(2.00f, 3.00f), normal: result.normal,
								//damage_integrity: damage, damage_durability: damage, damage_terrain: damage,
								//target_material_type: result.material_type, damage_type: Damage.Type.Phlogiston,
								//flags: Damage.Flags.No_Loot_Pickup,
								//yield: 0.90f, size: 1.20f, impulse: 100.00f, faction_id: h_faction, pain: 1.60f, stun: 0.95f);									
#endif
							}
						}
					}
				}
			}
		}

		public static readonly Texture.Handle texture_smoke = "BiggerSmoke_Light";

		[ISystem.VeryLateUpdate(ISystem.Mode.Single, ISystem.Scope.Region)]
		public static void UpdateEffects(ISystem.Info info, Entity entity, ref Region.Data region, ref XorRandom random,
		[Source.Owned] in Transform.Data transform, [Source.Owned] in Body.Data body, [Source.Owned, Pair.Component<Extinguisher.Data>] ref Inventory1.Data inventory,
		[Source.Owned] in Control.Data control, [Source.Owned] ref Extinguisher.Data extinguisher, [Source.Owned] ref Sound.Mixer sound_mix)
		{
			var lmb = control.mouse.GetKey(Mouse.Key.Left);
			var is_active = false;

			if (lmb)
			{
				is_active = inventory.resource == extinguisher.h_material_fuel;
			}

			sound_mix.modifier = is_active ? 1.00f : 0.00f;

#if CLIENT
			if (is_active && info.WorldTime >= extinguisher.t_last_sound + 0.15f)
			{
				//if (control.mouse.GetKeyUp(Mouse.Key.Left))
				//{
				//	extinguisher.last_sound = info.WorldTime;
				//	Sound.Play(extinguisher.sound_end, transform.position, volume: 1.20f);
				//}
				//else 

				if (control.mouse.GetKeyDown(Mouse.Key.Left))
				{
					extinguisher.t_last_sound = info.WorldTime;
					Sound.Play(extinguisher.sound_start, transform.position, volume: 0.90f);
				}

			}

			if (is_active)
			{
				extinguisher.t_last_sound = info.WorldTime;

				var aim_dir = transform.GetDirection();

				var pos_a = transform.LocalToWorld(extinguisher.offset) + (aim_dir * 0.50f);
				//var pos_b = pos_a + (aim_dir * extinguisher.max_length * 0.1250f);

				for (var i = 0; i < 1; i++)
				{
					Particle.Spawn(ref region, new Particle.Data()
					{
						texture = Geyser.texture_metal_spark,
						lifetime = random.NextFloatRange(0.10f, 0.30f),
						pos = pos_a,
						vel = body.GetVelocity() + (aim_dir * random.NextFloatRange(15.00f, 40.00f)) + random.NextUnitVector2Range(0.10f, 10.50f),
						fps = 16,
						frame_count = 4,
						frame_offset = random.NextByteRange(0, 4),
						frame_count_total = 4,
						scale = random.NextFloatRange(0.50f, 1.20f),
						//rotation = random.NextFloat(MathF.PI),
						//angular_velocity = random.NextFloat(MathF.PI * 10.00f),
						growth = random.NextFloatRange(0.20f, 4.00f),
						stretch = new Vector2(random.NextFloatRange(1.00f, 2.00f), random.NextFloatRange(0.10f, 0.20f)),
						force = random.NextUnitVector2Range(2, 8) + new Vector2(0.00f, random.NextFloatRange(-40, 50)),
						color_a = new Color32BGRA(255, 255, 255, 255),
						color_b = new Color32BGRA(255, 255, 255, 255),
						drag = random.NextFloatRange(0.08f, 0.19f),
						angular_velocity = random.NextFloatRange(-0.20f, 0.40f),
						face_dir_ratio = 1.00f,
						lit = 1.00f
					});
				}

				Particle.Spawn(ref region, new Particle.Data()
				{
					texture = Light.tex_light_fire_00,
					lifetime = random.NextFloatRange(0.12f, 0.20f),
					pos = pos_a,
					//vel = new Vector2(0.70f, -1.10f) * (1.00f + (modifier2 * 0.10f)),
					vel = body.GetVelocity() + (aim_dir * random.NextFloatRange(15.00f, 35.00f)) + random.NextUnitVector2Range(0.10f, 1.50f),
					//force = new Vector2(0, 2.50f) * random.NextFloatRange(0.90f, 1.10f),
					scale = random.NextFloatRange(1.10f, 1.30f),
					//rotation = random.NextFloatRange(-10.00f, 10.00f),
					//angular_velocity = random.NextFloat(1.00f),
					growth = random.NextFloatRange(15.00f, 35.00f),
					drag = random.NextFloatRange(0.07f, 0.10f),
					stretch = new Vector2(random.NextFloatRange(1.55f, 1.74f), random.NextFloatRange(0.30f, 0.40f)),
					color_a = ColorBGRA.RGBA(1.00f, 0.70f, 0.30f, random.NextFloatExtra(5.00f, 10.00f)),
					color_b = ColorBGRA.RGBA(0.50f, 0.10f, 0.00f, 0.00f),
					glow = random.NextFloatRange(8.00f, 20.00f),
					force = random.NextUnitVector2Range(1, 2) + new Vector2(0.00f, random.NextFloatRange(-15, 15)) + (aim_dir * 3.00f),
					face_dir_ratio = 1.00f
				});
			}
#endif
		}
	}
}
