
namespace TC2.Base.Components
{
	public static partial class Levitator
	{
		[Serializable]
		public partial struct Node
		{
			[Serializable]
			public partial struct Setting
			{
				public Keyboard.Key? key;
			}

			[Editor.Picker.Direction(true, true)] public Vector2 dir;
			[Editor.Picker.Position(true)] public Vector2 offset;

			public float ratio;
			public Keyboard.Key key;
		}

		[IComponent.Data(Net.SendType.Reliable, region_only: true)]
		public partial struct Data: IComponent
		{
			[Flags]
			public enum Flags: uint
			{
				None = 0,

				Use_Analog = 1u << 0

				//Hold_Shift = 1 << 0
			}

			public FixedArray4<Levitator.Node> nodes;

			public float lerp_rate = 0.30f;
			public float efficiency = 0.60f;

			public Levitator.Data.Flags flags;
			public Levitator.Data.Flags flags_editable;

			public float particle_size = 1.00f;

			[Editor.Picker.Position(true)] public Vector2 offset;

			public Data()
			{

			}
		}

		[IComponent.Data(Net.SendType.Unreliable, region_only: true)]
		public partial struct State: IComponent
		{
			public FixedArray4<float> node_rates;

			public Vector2 current_direction;
			public Vector2 current_offset;
			public float current_force;
			[Save.Ignore, Net.Ignore] public float next_linecast;

			public State()
			{

			}
		}

		public struct EditRPC: Net.IRPC<Levitator.Data>
		{
			public Levitator.Data.Flags? flags;
			public FixedArray4<Levitator.Node.Setting> settings;

#if SERVER
			public void Invoke(ref NetConnection connection, Entity entity, ref Levitator.Data data)
			{
				var sync = false;

				//if (this.power.HasValue)
				//{
				//	//data.essence_available = Maths.Clamp01(this.power.Value);
				//	sync = true;
				//}

				for (var i = 0; i < this.settings.Length; i++)
				{
					ref var node_edit = ref this.settings[i];
					ref var node = ref data.nodes[i];

					if (node.ratio > 0.00f)
					{
						if (node_edit.key.HasValue)
						{
							node.key = node_edit.key.Value;
							sync = true;
						}
					}
				}

				if (this.flags.HasValue)
				{
					data.flags = (data.flags & ~data.flags_editable) | (this.flags.Value & data.flags_editable);
					sync = true;
				}

				if (sync)
				{
					data.Sync(entity);
				}
			}
#endif
		}

		[ISystem.LateUpdate(ISystem.Mode.Single, ISystem.Scope.Region)]
		public static void OnUpdate(ref Region.Data region, ref XorRandom random, ISystem.Info info, Entity entity,
		[Source.Owned] in Transform.Data transform, [Source.Owned] in Control.Data control, [Source.Owned] ref Body.Data body,
		[Source.Owned] ref Levitator.Data levitator, [Source.Owned] ref Levitator.State levitator_state, [Source.Owned] ref EssenceContainer.Data container, [Source.Owned, Optional(true)] ref Overheat.Data overheat)
		{
			ref readonly var kb = ref control.keyboard;
			ref var essence_data = ref container.h_essence.GetData();

			var force_total = Vector2.Zero;
			var offset = Vector2.Zero;
			var rate_total = 0.00f;

			var is_mirrored = float.IsNegative(transform.scale.GetParity());
			var count = 0;

			const bool draw_debug = false;

			for (var i = 0; i < levitator.nodes.Length; i++)
			{
				ref var node = ref levitator.nodes[i];
				if (node.key != 0)
				{
					var key = node.key;

					if (is_mirrored)
					{
						if (key.HasAny(Keyboard.Key.MoveLeft))
						{
							key.SetFlag(Keyboard.Key.MoveLeft, false);
							key.SetFlag(Keyboard.Key.MoveRight, true);
						}
						else if (key.HasAny(Keyboard.Key.MoveRight))
						{
							key.SetFlag(Keyboard.Key.MoveLeft, true);
							key.SetFlag(Keyboard.Key.MoveRight, false);
						}
					}

					//var force = node.dir * node.ratio * levitator.force_multiplier;
					//force_total += force;

#if SERVER
					if (draw_debug)
					{
						region.DrawDebugDir(transform.LocalToWorld(levitator.offset + node.offset), transform.LocalToWorldDirection(node.dir * node.ratio * 4), Color32BGRA.Yellow, 2.00f);
						region.DrawDebugCircle(transform.LocalToWorld(levitator.offset + node.offset), 0.125f, Color32BGRA.Yellow, 2.00f);
					}
#endif

					ref var node_rate = ref levitator_state.node_rates[i];
					node_rate = Maths.Lerp(node_rate, kb.GetKey(key, false) ? 1.00f : 0.00f, levitator.lerp_rate);
					rate_total += node_rate * node.ratio;

					if (node_rate > 0.01f)
					{
						//force_new += force;


						offset += node.offset;
			
						if (essence_data.IsNotNull())
						{
							//var modifier = Maths.Clamp(force_len * 0.05f, 0.30f, 1.00f);
							//var modifier2 = Maths.Clamp(force_len * 0.05f, 0.10f, 1.00f);


							var node_amount = container.available * node_rate * node.ratio;
							var force_local = node.dir * (node_amount * essence_data.force_emit) * levitator.efficiency;

							force_total += force_local;
							body.AddForceLocal(-force_local, node.offset + levitator.offset);

							var dir_world = transform.LocalToWorldDirection(node.dir.GetNormalized());

#if CLIENT
							var noise_x = MathF.CopySign(random.NextFloatRange(1.50f, 2), random.NextFloat());
							var noise_y = random.NextFloatRange(0.90f, 2) * 1.00f * 2;
							var noise_intensity = random.NextFloatRange(0.10f, 1.00f);

							var color = essence_data.color_emit;
							color = ColorBGRA.Lerp(color, Color32BGRA.White, random.NextFloatRange(0.00f, 0.50f));

							//region.DrawDebugText(transform.LocalToWorld(levitator.offset + node.offset + new Vector2(0, 3)), $"{node_amount:0}\n{node_rate:P2}\n{force_local:0.00}", Color32BGRA.White);

							Particle.Spawn(ref region, new Particle.Data()
							{
								texture = texture_glow,
								lifetime = random.NextFloatRange(0.10f, 0.50f) * (levitator.particle_size * 0.50f),
								pos = transform.LocalToWorld(levitator.offset + node.offset) + random.NextVector2(0.10f) - (dir_world * 2.00f),
								vel = dir_world * random.NextFloatRange(0.90f, 1.00f) * node_amount * 2.00f,
								force = random.NextUnitVector2Range(0, 8),
								//fps = random.NextByteRange(3, 5),
								stretch = new Vector2(noise_x, noise_y),
								rotation = ((MathF.PI * -0.50f) - (dir_world.GetAngleRadiansFast()) + random.NextFloatRange(-0.50f, 0.50f)) * -transform.scale.GetParity(),
								frame_count = 1,
								frame_count_total = 1,
								frame_offset = 0,
								scale = random.NextFloatRange(0.40f, 0.90f) * (MathF.Pow(node_amount, 0.50f) * 0.50f) * node_rate * levitator.particle_size, //random.NextFloatRange(0.10f, 0.90f) * levitator.particle_size,
								angular_velocity = random.NextFloat(4.00f),
								growth = random.NextFloatRange(2.00f, 6.00f) * levitator.particle_size,
								drag = random.NextFloatRange(0.15f, 0.25f),
								color_a = ColorBGRA.Lerp(color, 0xffffffff, random.NextFloatRange(0.00f, 0.40f)) * random.NextFloatRange(0.50f, 2.50f),
								color_b = color.WithColorMult(0.10f),
								glow = 0.01f + (((0.50f + MathF.Abs(noise_intensity)) * 1.00f) * node_rate)
								//face_dir_ratio = 1.00f
							});
#endif
						}

						count++;
					}
				}
			}

			if (count > 0)
			{
				offset /= count;
			}

			offset += levitator.offset;
			//var force_new_dir = force_new.GetNormalized(out var force_new_len);
			var force_total_dir = force_total.GetNormalized(out var force_total_len);

			levitator_state.current_direction = Vector2.Lerp(levitator_state.current_direction, force_total_dir, 0.10f);
			levitator_state.current_offset = Vector2.Lerp(levitator_state.current_offset, offset, 0.20f);
			levitator_state.current_force = Maths.Lerp(levitator_state.current_force, force_total_len, levitator.lerp_rate);
			//levitator.force_multiplier = 0.00f;
			container.rate = Maths.MoveTowards(container.rate, rate_total, levitator.lerp_rate);

			//container.rate = Maths.NormalizeClamp(levitator_state.current_force, force_total_len);

#if SERVER
			if (draw_debug)
			{
				region.DrawDebugDir(transform.LocalToWorld(levitator_state.current_offset), transform.LocalToWorldDirection(levitator_state.current_direction), Color32BGRA.Green, 2.00f);
				region.DrawDebugCircle(transform.LocalToWorld(levitator_state.current_offset), 0.125f, Color32BGRA.Green, 2.00f);
			}
#endif

			if (essence_data.IsNotNull())
			{
				//levitator.force_multiplier = essence_data.force_emit * container.available;

				if (overheat.IsNotNull())
				{
					overheat.AddHeat(essence_data.heat_emit * container.available * container.rate, body.GetMass());
				}

				if (info.WorldTime >= levitator_state.next_linecast)
				{
					var dir = levitator_state.current_direction; //.GetNormalized(out var len);

					var force_len = levitator_state.current_force * 0.001f * 0.10f;
					//App.WriteLine(force_len);

					if (force_len > 4.00f)
					{
						Span<LinecastResult> results = stackalloc LinecastResult[16];

						var max_dist = 8.00f;

						var radius = 4.00f;
						var pos_a = transform.LocalToWorld(levitator_state.current_offset + (dir * radius));
						var pos_b = transform.LocalToWorld(levitator_state.current_offset + (dir * (radius + max_dist)));

						var len = force_len; // * 0.10f;

#if SERVER
						if (draw_debug)
						{
							region.DrawDebugLine(pos_a, pos_b, Color32BGRA.Green, 4);
						}
#endif

						if (region.TryLinecastAll(pos_a, pos_b, radius, ref results, mask: Physics.Layer.Essence | Physics.Layer.Destructible | Physics.Layer.World, exclude: Physics.Layer.Tree | Physics.Layer.Building | Physics.Layer.Ignore_Bullet | Physics.Layer.Ignore_Melee))
						{
							results.SortByDistance();

							foreach (ref var result in results)
							{
								//var dot = Vector2.Dot(dir, result.world_position - pos_a);
								//if (dot > 0.00f || result.alpha <= 0.00f) continue;
								if (result.layer.HasAny(Physics.Layer.Dynamic) && result.alpha <= -0.50f) continue;

								var alpha = Maths.Clamp01(1.50f - result.alpha);

//#if SERVER
//								if (result.material_type == Material.Type.Foliage || result.material_type == Material.Type.Wood || result.material_type == Material.Type.Glass || result.material_type == Material.Type.Flesh)
//								{
//									var damage = force_len * 50.00f * alpha;


//									Damage.Hit(ent_attacker: entity, ent_owner: entity, ent_target: result.entity,
//										position: result.world_position, velocity: dir, normal: -dir,
//										damage_integrity: damage, damage_durability: damage, damage_terrain: damage * 0.20f,
//										target_material_type: result.material_type, damage_type: Damage.Type.Shockwave,
//										yield: 0.00f, size: 4.00f, impulse: force_len * 0.25f);
//									//entity.Hit(entity, result.entity, result.world_position, dir2, -dir2, force_len * 50.00f * alpha, result.material_type, Damage.Type.Shockwave, terrain_damage_multiplier: 0.20f, size: 4.00f, knockback: 15.00f, speed: 30.00f);

//									App.WriteLine(damage);
//									region.DrawDebugCircle(result.world_position, 1.00f, Color32BGRA.Red, 4.00f);
//								}
//#endif

								if (result.layer.HasAny(Physics.Layer.World | Physics.Layer.Essence))
								{
#if CLIENT
									for (var i = 0; i < 4; i++)
									{
										//var dir2 = i == 0 ? Vector2.Reflect(dir, result.normal) : result.normal.GetPerpendicular(random.NextBool(0.50f));
										var dir2 = result.normal.GetPerpendicular(i == 0);

										Particle.Spawn(ref region, new Particle.Data()
										{
											texture = texture_smoke,
											lifetime = random.NextFloatRange(0.50f, 1.00f) * levitator.particle_size,
											pos = result.world_position - (dir * 1),
											vel = dir2.RotateByRad(random.NextFloatRange(-0.10f, 0.10f)) * random.NextFloatRange(0.10f, 1.00f) * Maths.Max(len, 10.00f) * 3.00f,
											force = new Vector2(0.00f, -0.10f) + dir.RotateByRad(random.NextFloatRange(-1.00f, 1.00f)) * random.NextFloatRange(0.90f, 1.00f) * Maths.Max(len, 1.00f),
											fps = random.NextByteRange(3, 5),
											frame_count = 64,
											frame_count_total = 64,
											frame_offset = random.NextByteRange(0, 64),
											scale = random.NextFloatRange(0.70f, 0.90f) * levitator.particle_size,
											rotation = random.NextFloat(10.00f),
											angular_velocity = random.NextFloat(2.00f),
											growth = random.NextFloatRange(1.00f, 1.50f) * levitator.particle_size,
											drag = random.NextFloatRange(0.06f, 0.10f),
											color_a = new Color32BGRA((byte)(random.NextByteRange(100, 120) * alpha), 240, 240, 240),
											color_b = new Color32BGRA(0, 240, 240, 240)
										});
									}
#endif


									break;
								}
							}
						}
					}
					levitator_state.next_linecast = info.WorldTime + random.NextFloatRange(0.10f, 0.15f);
				}
			}
		}

		//#if SERVER
		//		[ISystem.Event<Health.PostDamageEvent>(ISystem.Mode.Single, ISystem.Scope.Region)]
		//		public static void OnPostDamage(ISystem.Info info, Entity entity, ref Health.PostDamageEvent data, [Source.Owned] in Health.Data health, [Source.Owned] ref Levitator.Data levitator, [Source.Owned] ref Levitator.State levitator_state, [Source.Owned] in Transform.Data transform)
		//		{
		//			if (info.WorldTime >= levitator_state.next_boom)
		//			{
		//				var modifier = MathF.Pow(Maths.Clamp01(Maths.Min(1.00f - Maths.Min(health.integrity, health.durability), levitator.EssenceRate)), 2.00f);
		//				//App.WriteLine($"modifier: {modifier}");

		//				if (modifier >= 0.25f)
		//				{
		//					var random = XorRandom.New();
		//					if (random.NextBool(modifier))
		//					{
		//						levitator_state.next_boom = info.WorldTime + random.NextFloatRange(0.50f, 1.00f);

		//						ref var region = ref info.GetRegion();
		//						var prefab = Essence.GetEssencePrefab(Essence.Type.Motion);
		//						if (prefab.id != 0)
		//						{
		//							var amount = levitator.EssenceAvailable * Maths.Max(levitator.EssenceRate, modifier * modifier) * modifier;
		//							if (amount >= 5.00f)
		//							{
		//								region.SpawnPrefab(prefab, transform.position).ContinueWith(x =>
		//								{
		//									ref var essence_node = ref x.GetComponent<EssenceNode.Data>();
		//									if (!essence_node.IsNull())
		//									{
		//										essence_node.type = Essence.Type.Motion;
		//										essence_node.amount = amount;
		//										essence_node.stability = 0.00f;
		//										essence_node.volatility = 1.00f;

		//										essence_node.Sync(x);
		//									}
		//								});
		//							}
		//						}
		//					}
		//				}
		//			}
		//		}
		//#endif

		//[ISystem.PreUpdate.Reset(ISystem.Mode.Single, ISystem.Scope.Region)]
		//public static void UpdatePower(Entity entity, [Source.Owned, Pair.Of<Levitator.Data>] ref Inventory1.Data inventory, [Source.Owned] ref Levitator.Data levitator)
		//{
		//	//App.WriteLine("inv");
		//	Essence.IPowered.UpdateInventory(ref levitator, ref inventory);
		//	levitator.force_multiplier = Essence.force_per_motion_essence * levitator.EssenceAvailable;
		//}

		//#if SERVER
		//		[ISystem.Event<Health.PostDamageEvent>(ISystem.Mode.Single, ISystem.Scope.Region)]
		//		public static void OnPostDamage(ISystem.Info info, Entity entity, ref Health.PostDamageEvent data, [Source.Owned] in Transform.Data transform, [Source.Owned] ref Health.Data health, [Source.Owned] ref Levitator.Data levitator)
		//		{
		//			ref var region = ref info.GetRegion();

		//			//EssenceLeak.Create(entity, Essence.Type.Motion, transform.WorldToLocal(data.damage.world_position), (data.damage.normal - data.damage.direction) * 0.50f);
		//		}
		//#endif

#if CLIENT
		//[ISystem.GUI(ISystem.Mode.Single, ISystem.Scope.Region)]
		//public static void UpdateDebug(ISystem.Info info, Entity entity,
		//[Source.Owned] in Transform.Data transform, [Source.Owned] in Control.Data control, [Source.Owned] ref Body.Data body,
		//[Source.Owned] ref Levitator.Data levitator, [Source.Owned] ref Levitator.State levitator_state,
		//[Source.Owned, Pair.Of<Levitator.Data>] ref Sound.Emitter sound_emitter)
		//{
		//	return;

		//	ref readonly var kb = ref control.keyboard;

		//	var force = Vector2.Zero;

		//	var is_mirrored = float.IsNegative(transform.scale.GetParity());

		//	GUI.DrawLine(GUI.WorldToCanvas(transform.LocalToWorld(levitator_state.current_offset)), GUI.WorldToCanvas(transform.LocalToWorld(levitator_state.current_offset + levitator_state.current_direction)), 0xff00ff00, 2.00f);

		//	for (var i = 0; i < levitator.nodes.Length; i++)
		//	{
		//		ref var node = ref levitator.nodes[i];
		//		if (node.key != 0)
		//		{
		//			var key = node.key;
		//			if (is_mirrored)
		//			{
		//				if (key == Keyboard.Key.MoveLeft) key = Keyboard.Key.MoveRight;
		//				else if (key == Keyboard.Key.MoveRight) key = Keyboard.Key.MoveLeft;
		//			}

		//			if (kb.GetKey(key))
		//			{
		//				var dir = node.dir;

		//				var pos_a = transform.LocalToWorld(levitator.offset + node.offset);
		//				var pos_b = transform.LocalToWorld(levitator.offset + node.offset + (dir * 4.00f));

		//				GUI.DrawLine(GUI.WorldToCanvas(pos_a), GUI.WorldToCanvas(pos_b), 0xffff0000, 2.00f);
		//			}
		//		}
		//	}

		//	//GUI.DrawLine(GUI.WorldToCanvas(transform.LocalToWorld(new Vector2(0, 1.50f))), GUI.WorldToCanvas(transform.LocalToWorld(new Vector2(0, 1.50f)) + transform.LocalToWorldDirection(levitator_state.current_direction)), 0xffff0000, 2.00f, false);
		//}

		public static readonly Texture.Handle texture_smoke = "BiggerSmoke_Light";
		public static readonly Texture.Handle texture_spark = "essence.spark";
		public static readonly Texture.Handle texture_glow = "essence.glow";

		[ISystem.VeryLateUpdate(ISystem.Mode.Single, ISystem.Scope.Region)]
		public static void UpdateSprite(ref XorRandom random,
		[Source.Owned] in Levitator.Data levitator,
		[Source.Owned, Pair.Of<Levitator.Data>] ref Animated.Renderer.Data renderer)
		{
			renderer.offset = Vector2.Lerp(renderer.offset, levitator.offset + random.NextUnitVector2Range(0.03f, 0.05f), 0.50f);
		}

		[ISystem.VeryLateUpdate(ISystem.Mode.Single, ISystem.Scope.Region)]
		public static void UpdateEffects(ref Region.Data region, ref XorRandom random, Entity entity,
		[Source.Owned] in Transform.Data transform, [Source.Owned] ref EssenceContainer.Data container,
		[Source.Owned] in Levitator.Data levitator, [Source.Owned] ref Levitator.State levitator_state)
		//[Source.Owned, Pair.Of<Levitator.Data>] ref Light.Data light)
		{
			ref var essence_data = ref container.h_essence.GetData();
			if (essence_data.IsNotNull())
			{
				var dir = levitator_state.current_direction;
				var dir_w = transform.LocalToWorldDirection(levitator_state.current_direction).GetNormalized();

				//var force_len = levitator_state.current_force * 0.001f * 0.10f;
				var force_len = container.available * container.rate * 0.50f;
				if (force_len > 0.50f)
				{
					var modifier = Maths.Clamp(force_len * 0.05f, 0.30f, 1.00f);
					var modifier2 = Maths.Clamp(force_len * 0.05f, 0.10f, 1.00f);

					var noise_x = MathF.CopySign(random.NextFloatRange(1.50f, 3), random.NextFloat());
					var noise_y = random.NextFloatRange(0.10f, 2) * modifier * 2;
					var noise_intensity = random.NextFloatRange(0.10f, 1.00f);

					var color = essence_data.color_emit;
					color = ColorBGRA.Lerp(color, 0xffffffff, random.NextFloatRange(0.00f, 0.50f));
					var color2 = essence_data.color_emit;



					//light.color = color;
					//light.intensity = ((0.50f + MathF.Abs(noise_intensity)) * 10.00f) * modifier;
					//light.scale = Vector2.Lerp(light.scale, new Vector2(2 + (4 * noise_x), 2 + (4 * noise_y)) * 0.50f * modifier, 1);
					//light.offset = levitator_state.current_offset + new Vector2(random.NextFloatRange(-0.50f, 0.50f), random.NextFloatRange(-0.50f, 0.50f));
					//light.rotation = ((MathF.PI * 0.50f) + (dir.GetAngleRadians()) + random.NextFloatRange(-0.20f, 0.20f)) * -transform.scale.GetParity();

					{
						Particle.Spawn(ref region, new Particle.Data()
						{
							texture = texture_smoke,
							lifetime = random.NextFloatRange(0.50f, 1.00f) * levitator.particle_size * modifier2,
							pos = transform.LocalToWorld(levitator_state.current_offset + (dir * Maths.Clamp(force_len, 2.00f, 3))) + random.NextVector2(0.40f),
							vel = dir_w.RotateByRad(random.NextFloatRange(-0.60f, 0.60f)) * random.NextFloatRange(0.90f, 1.00f) * Maths.Max(force_len, 10.00f) * 3.00f,
							force = new Vector2(0.00f, -0.10f) + dir_w.RotateByRad(random.NextFloatRange(-1.00f, 1.00f)) * random.NextFloatRange(0.90f, 1.00f) * Maths.Max(force_len, 1.00f),
							fps = random.NextByteRange(3, 5),
							frame_count = 64,
							frame_count_total = 64,
							frame_offset = random.NextByteRange(0, 64),
							scale = random.NextFloatRange(0.80f, 1.00f) * levitator.particle_size,
							rotation = random.NextFloat(10.00f),
							angular_velocity = random.NextFloat(2.00f),
							growth = random.NextFloatRange(1.00f, 1.50f) * levitator.particle_size,
							drag = random.NextFloatRange(0.09f, 0.15f),
							color_a = ColorBGRA.Lerp(new Color32BGRA(random.NextByteRange(50, 100), 240, 240, 240), color2, random.NextFloatRange(0.00f, 0.00f)),
							color_b = new Color32BGRA(0, 240, 240, 240)
						});
					}

					{
						Particle.Spawn(ref region, new Particle.Data()
						{
							texture = texture_spark,
							lifetime = random.NextFloatRange(0.10f, 0.20f) * levitator.particle_size * modifier2,
							pos = transform.LocalToWorld(levitator_state.current_offset) + random.NextVector2(0.70f),
							vel = dir_w.RotateByRad(random.NextFloatRange(-0.60f, 0.60f)) * random.NextFloatRange(0.90f, 1.00f) * Maths.Max(force_len, 10.00f) * 9.00f,
							force = new Vector2(0.00f, -0.10f) + dir_w.RotateByRad(random.NextFloatRange(-1.00f, 1.00f)) * random.NextFloatRange(0.90f, 1.00f) * Maths.Max(force_len, 1.00f),
							//fps = random.NextByteRange(3, 5),
							//stretch = new Vector2(0.1f,0.4f),
							frame_count = 1,
							frame_count_total = 1,
							frame_offset = 0,
							scale = random.NextFloatRange(0.10f, 0.90f) * levitator.particle_size,
							rotation = random.NextFloat(10.00f),
							angular_velocity = random.NextFloat(2.00f),
							growth = -0.50f,
							drag = random.NextFloatRange(0.07f, 0.15f),
							color_a = ColorBGRA.Lerp(color2, 0xffffffff, random.NextFloatRange(0.00f, 0.40f)) * random.NextFloatRange(0.50f, 2.50f),
							color_b = color2.WithColorMult(0.10f),
							glow = 1.00f
						});
					}

					Shake.Emit(ref region, transform.position, random.NextFloatRange(0.00f, 0.20f) * Maths.Clamp((force_len * 0.05f) - 0.01f, 0.00f, 1.00f), 0.25f, 50.00f);
				}
				//else
				//{
				//	light.intensity = 0.00f;
				//}
			}
			//else
			//{
			//	light.intensity = 0.00f;
			//}
		}

		[ISystem.VeryLateUpdate(ISystem.Mode.Single, ISystem.Scope.Region)]
		public static void UpdateSound(ISystem.Info info, Entity entity,
		[Source.Owned] in Levitator.Data levitator, [Source.Owned] ref Levitator.State levitator_state, [Source.Owned] ref EssenceContainer.Data container,
		[Source.Owned, Pair.Of<EssenceContainer.Data>] ref Sound.Emitter sound_emitter)
		{
			var dir = levitator_state.current_direction;
			//var force_len = levitator_state.current_force * 0.001f * 0.10f;
			var modifier = container.available * container.rate * 0.30f;
			//App.WriteLine(modifier);

			sound_emitter.volume = Maths.Lerp2(sound_emitter.volume, Maths.Clamp(modifier * 0.08f, 0.00f, 1.50f), 0.02f, 0.05f);
			sound_emitter.pitch = Maths.Lerp2(sound_emitter.pitch, 0.30f + Maths.Clamp(modifier * 0.10f, 0.40f, 1.00f) * 0.40f, 0.01f, 0.04f);
		}

		public partial struct LevitatorGUI: IGUICommand
		{
			public Entity ent_levitator;
			public Levitator.Data levitator;
			public Levitator.State levitator_state;

			public void Draw()
			{
				using (var window = GUI.Window.Interaction("Levitator"u8, this.ent_levitator))
				{
					this.StoreCurrentWindowTypeID();
					if (window.show)
					{
						var rpc = new Levitator.EditRPC();
						var sync = false;

						//Boiler.DrawGauge(this.levitator.EssenceAvailable * this.levitator.EssenceRate, 0.00f, this.levitator.EssenceAvailable);

						//GUI.SameLine();

						GUI.DrawInventoryDock(Inventory.Type.Essence, new Vector2(48, 48));

						GUI.SameLine();

						using (var group = GUI.Group.New(size: new Vector2(GUI.RmX, 48), padding: new Vector2(4, 4)))
						{
							GUI.DrawBackground(GUI.tex_panel, group.GetOuterRect(), new(4));

							//if (this.levitator.flags_editable.HasAny(Levitator.Data.Flags.Hold_Shift))
							//{
							//	if (GUI.Checkbox("Hold Shift", ref this.levitator.flags, Levitator.Data.Flags.Hold_Shift, new Vector2(96, 24)))
							//	{
							//		rpc.flags = this.levitator.flags;
							//		sync = true;
							//	}
							//	GUI.DrawHoverTooltip("Require holding shift to control the levitator.");
							//}
						}

						//GUI.Text($"{(this.levitator.force_multiplier * 0.001f):0.00} kN");
						//GUI.Text($"{(this.levitator.EssenceAvailable):0.00}");


						for (var i = 0; i < this.levitator.nodes.Length; i++)
						{
							ref var node = ref this.levitator.nodes[i];
							if (node.ratio > 0.00f)
							{
								using (GUI.ID.Push(100 + i))
								{
									using (var group = GUI.Group.New(size: new Vector2(GUI.RmX, 48), padding: new Vector2(4, 4)))
									{
										GUI.DrawBackground(GUI.tex_panel, group.GetOuterRect(), new(4));

										if (GUI.EnumInput($"key.{i}", ref node.key, new Vector2(GUI.RmX, 40), show_label: false))
										{
											rpc.settings[i].key = node.key;
											sync = true;
										}
									}
								}
							}
						}

						if (sync)
						{
							rpc.Send(this.ent_levitator);
						}

						//if (GUI.SliderFloat("Power", ref this.levitator.essence_available, 0.00f, 1.00f, size: new Vector2(160, 32)))
						//{
						//	var rpc = new Levitator.ConfigureRPC()
						//	{
						//		power = this.levitator.essence_available
						//	};
						//	rpc.Send(this.ent_levitator);
						//}


					}
				}
			}
		}

		[ISystem.EarlyGUI(ISystem.Mode.Single, ISystem.Scope.Region)]
		public static void OnGUI(Entity entity, [Source.Owned] in Levitator.Data levitator, [Source.Owned] in Levitator.State levitator_state,
		[Source.Owned] in Interactable.Data interactable)
		{
			if (interactable.show)
			{
				var gui = new LevitatorGUI()
				{
					ent_levitator = entity,
					levitator = levitator,
					levitator_state = levitator_state,
				};
				gui.Submit();
			}
		}
#endif
	}
}
