﻿namespace TC2.Base.Components
{
	public static partial class Electrode
	{
		public enum Type: byte
		{
			Undefined,

			Single,
			Dual,
			Triple
		}

		[Flags]
		public enum Flags: uint
		{
			None = 0u,

			Insulated = 1u << 0,
			Continuous = 1u << 1,
		}

		[IComponent.Data(Net.SendType.Reliable, region_only: true), IComponent.With<Electrode.State>]
		public partial struct Data: IComponent
		{
			public Electrode.Flags flags;

			public ISubstance.Handle h_substance_electrode_a;
			public ISubstance.Handle h_substance_electrode_b;

			public Distance electrode_separation;
			public Electrode.Type type;

			public float damage_integrity = 400.00f;
			public float damage_durability = 4000.00f;
			public float damage_terrain = 200.00f;

			[Editor.Picker.Position(true)]
			public Vector2 offset_a;

			[Editor.Picker.Position(true)]
			public Vector2 offset_b;

			public Data()
			{

			}
		}

		[IComponent.Data(Net.SendType.Unreliable, region_only: true)]
		public partial struct State: IComponent
		{
			[Flags]
			public enum Flags: byte
			{
				None = 0,

				Active = 1 << 0,
			}

			public float charge;
			public Electrode.State.Flags flags;

			public State()
			{

			}
		}

		public static readonly Sound.Handle sound_hit_default = "arcane_infuser.fizzle.00";

		public static readonly Texture.Handle tex_blank = "blank";
		public static readonly Texture.Handle tex_light = "light.circle.00";
		public static readonly Texture.Handle tex_smoke = "BiggerSmoke_Light";
		public static readonly Texture.Handle tex_spark = "metal_spark.01";

		[ISystem.Update.C(ISystem.Mode.Single, ISystem.Scope.Region)]
		public static void OnUpdate(ISystem.Info info, ref Region.Data region, ref XorRandom random, Entity entity,
		[Source.Owned] in Transform.Data transform, [Source.Owned] ref Body.Data body,
		[Source.Owned] ref Electrode.Data electrode, [Source.Owned] ref Electrode.State electrode_state,
		[Source.Owned, Pair.Index(0)] ref Shape.Circle shape_electrode_a,
		[Source.Owned, Pair.Index(1)] ref Shape.Circle shape_electrode_b)
		{
			var time = info.WorldTime;

			if (body.HasArbiters())
			{
				var pairs_span = FixedArray.CreateSpan8<uint>(out var pairs_buffer);

				foreach (var arbiter in body.GetArbiters())
				{
					if (arbiter.HasShape(shape_electrode_a) || arbiter.HasShape(shape_electrode_b))
					{
						var ent_arbiter = arbiter.GetEntity();
						var state = arbiter.GetState();

						var pos = arbiter.GetContactsPosition();


						if (state == Body.Arbiter.State.Begin)
						{
							var dir = arbiter.GetNormal();

#if CLIENT

							Sound.Play("essence.discharge.01", pos, volume: random.NextFloatExtra(1.40f, 0.50f), pitch: random.NextFloatExtra(0.90f, 1.75f), dist_multiplier: 1.25f);
							Shake.Emit(ref region, pos, 0.37f, 0.60f, 24.00f);

							for (var i = 0; i < 12; i++)
							{
								Particle.Spawn(ref region, new Particle.Data()
								{
									texture = tex_blank,
									lifetime = random.NextFloatRange(0.10f, 1.00f),
									pos = pos + random.NextUnitVector2Range(0.10f, 0.20f),
									vel = dir.RotateByRad(random.NextFloatRange(-2.50f, 2.50f)) * random.NextFloatRange(0, 80),
									fps = 0,
									scale = random.NextFloatRange(0.50f, 1.50f),
									growth = -random.NextFloatRange(1.60f, 4.00f),
									force = random.NextUnitVector2Range(0, 80),
									drag = random.NextFloatRange(0.00f, 0.40f),
									stretch = new Vector2(random.NextFloatRange(0.50f, 1.50f), 0.05f),
									color_a = random.NextColor32Range(0xffffffff, 0xffc89eff),
									color_b = random.NextColor32Range(0xffc89eff, 0xffffffff),
									lit = random.NextFloatRange(0.50f, 1.00f),
									face_dir_ratio = 1.00f
								});
							}

							for (var i = 0; i < 4; i++)
							{
								Particle.Spawn(ref region, new Particle.Data()
								{
									texture = tex_light,
									lifetime = random.NextFloatRange(0.01f, 0.10f),
									pos = pos + random.NextVector2(0.10f),
									vel = dir.RotateByRad(random.NextFloatRange(-2.00f, 2.00f)) * random.NextFloatRange(10.00f, 40.00f),
									force = dir.RotateByRad(random.NextFloatRange(-0.20f, 0.20f)) * random.NextFloatRange(0, 200),
									scale = random.NextFloatRange(2.00f, 30.00f),
									rotation = random.NextFloatRange(-3.50f, 3.50f),
									angular_velocity = random.NextFloat(0.50f),
									growth = random.NextFloatRange(10.00f, 200.00f),
									drag = random.NextFloatRange(0.07f, 0.25f),
									color_a = random.NextColor32Range(0xffffffff, 0xffc89eff),
									color_b = random.NextColor32Range(0xffc89eff, 0xffffffff),
									glow = random.NextFloatRange(2.00f, 8.00f),
								});
							}

							for (var i = 0; i < 2; i++)
							{
								Particle.Spawn(ref region, new Particle.Data()
								{
									texture = tex_smoke,
									lifetime = random.NextFloatRange(2.00f, 3.00f),
									pos = pos + random.NextVector2(0.50f),
									vel = random.NextUnitVector2Range(0.20f * i, 0.50f * i) + random.NextVector2(3.00f),
									fps = random.NextByteRange(5, 10),
									frame_count = 64,
									frame_count_total = 64,
									frame_offset = random.NextByteRange(0, 64),
									//scale = random.NextFloatRange(1.50f, 2.50f),
									scale = random.NextFloatRange(0.30f, 0.40f),
									rotation = random.NextFloat(10.00f),
									angular_velocity = random.NextFloat(2.00f),
									growth = random.NextFloatRange(0.15f, 0.25f),
									drag = random.NextFloatRange(0.02f, 0.04f),
									force = new Vector2(0.00f, random.NextFloatRange(0, -2)),
									color_a = random.NextColor32Range(0x80ffffff, 0xa0ffffff),
									color_b = random.NextColor32Range(0x00ffffff, 0x00808080)
								});
							}

							for (var i = 0; i < 3; i++)
							{
								Particle.Spawn(ref region, new Particle.Data()
								{
									texture = tex_spark,
									lifetime = random.NextFloatRange(1.00f, 2.00f),
									pos = pos + random.NextVector2(0.20f),
									vel = (dir * random.NextFloatRange(0, 40)).RotateByRad(random.NextFloatRange(-1.15f, 1.15f)) + random.NextVector2(15),
									fps = 0,
									frame_count = 1,
									frame_offset = random.NextByteRange(0, 4),
									frame_count_total = 4,
									scale = random.NextFloatRange(0.20f, 0.60f),
									growth = -random.NextFloatRange(0.60f, 1.00f),
									force = new Vector2(0.00f, random.NextFloatRange(20, 80)),
									drag = random.NextFloatRange(0.01f, 0.10f),
									stretch = new Vector2(random.NextFloatRange(1.00f, 2.00f), 0.75f),
									color_a = random.NextColor32Range(0xffffffff, 0xffc89eff),
									color_b = random.NextColor32Range(0xffffc0b0, 0xffffa090),
									lit = 1.00f,
									face_dir_ratio = 1.00f,
								});
							}
#endif

#if SERVER
							ref var heat_state = ref ent_arbiter.GetComponent<Heat.State>();
							if (heat_state.IsNotNull())
							{
								heat_state.AddEnergy(1450.00f);
								heat_state.Sync(ent_arbiter, true);
							}

							var damage = random.NextFloatExtra(450, 550);

							Damage.Hit(ent_attacker: entity, ent_owner: entity, ent_target: ent_arbiter,
							position: pos, velocity: dir * 8.00f, normal: -dir,
							damage_integrity: damage, damage_durability: damage, damage_terrain: damage,
							target_material_type: arbiter.GetMaterial(), damage_type: Damage.Type.Electricity,
							yield: 0.95f, size: 1.50f, impulse: -50.00f, flags: Damage.Flags.No_Loot_Pickup);
#endif
						}

						if (pairs_span.TryAddUnique(ent_arbiter.GetShortID(), out var contains) && contains)
						{
							//App.WriteLine($"touch {state}");

							if (state == Body.Arbiter.State.Begin || random.NextBool(0.40f))
							{
								var dir = random.NextUnitVector2Range(0.50f, 1.50f);


#if CLIENT

								if (state == Body.Arbiter.State.Begin || random.NextBool(0.40f)) Sound.Play("effect.zap.00", pos, volume: random.NextFloatExtra(0.50f, 1.00f), pitch: random.NextFloatExtra(0.70f, 1.75f), dist_multiplier: 0.75f);
								//Shake.Emit(ref region, pos, 0.37f, 0.60f, 24.00f);

								for (var i = 0; i < 6; i++)
								{
									Particle.Spawn(ref region, new Particle.Data()
									{
										texture = tex_blank,
										lifetime = random.NextFloatRange(0.10f, 1.00f),
										pos = pos + random.NextUnitVector2Range(0.10f, 0.20f),
										vel = dir.RotateByRad(random.NextFloatRange(-2.50f, 2.50f)) * random.NextFloatRange(0, 80),
										fps = 0,
										scale = random.NextFloatRange(0.50f, 1.50f),
										growth = -random.NextFloatRange(1.60f, 4.00f),
										force = random.NextUnitVector2Range(0, 80),
										drag = random.NextFloatRange(0.00f, 0.40f),
										stretch = new Vector2(random.NextFloatRange(0.50f, 1.50f), 0.05f),
										color_a = random.NextColor32Range(0xffffffff, 0xffc89eff),
										color_b = random.NextColor32Range(0xffc89eff, 0xffffffff),
										lit = random.NextFloatRange(0.50f, 1.00f),
										face_dir_ratio = 1.00f
									});
								}

								for (var i = 0; i < 1; i++)
								{
									Particle.Spawn(ref region, new Particle.Data()
									{
										texture = tex_light,
										lifetime = random.NextFloatRange(0.01f, 0.10f),
										pos = pos + random.NextVector2(0.10f),
										vel = dir.RotateByRad(random.NextFloatRange(-2.00f, 2.00f)) * random.NextFloatRange(5.00f, 20.00f),
										force = dir.RotateByRad(random.NextFloatRange(-0.20f, 0.20f)) * random.NextFloatRange(0, 200),
										scale = random.NextFloatRange(15.00f, 50.00f),
										rotation = random.NextFloatRange(-3.50f, 3.50f),
										angular_velocity = random.NextFloat(0.50f),
										growth = random.NextFloatRange(10.00f, 300.00f),
										drag = random.NextFloatRange(0.07f, 0.25f),
										color_a = random.NextColor32Range(0xffffffff, 0xffc89eff),
										color_b = random.NextColor32Range(0xffc89eff, 0xffffffff),
										glow = random.NextFloatRange(2.00f, 4.00f),
									});
								}

								// for (var i = 0; i < 1; i++)
								// {
								// 	Particle.Spawn(ref region, new Particle.Data()
								// 	{
								// 		texture = tex_smoke,
								// 		lifetime = random.NextFloatRange(2.00f, 3.00f),
								// 		pos = pos + random.NextVector2(0.50f),
								// 		vel = random.NextUnitVector2Range(0.20f * i, 0.50f * i) + random.NextVector2(3.00f),
								// 		fps = random.NextByteRange(5, 10),
								// 		frame_count = 64,
								// 		frame_count_total = 64,
								// 		frame_offset = random.NextByteRange(0, 64),
								// 		//scale = random.NextFloatRange(1.50f, 2.50f),
								// 		scale = random.NextFloatRange(0.30f, 0.40f),
								// 		rotation = random.NextFloat(10.00f),
								// 		angular_velocity = random.NextFloat(2.00f),
								// 		growth = random.NextFloatRange(0.15f, 0.25f),
								// 		drag = random.NextFloatRange(0.02f, 0.04f),
								// 		force = new Vector2(0.00f, random.NextFloatRange(0, -2)),
								// 		color_a = random.NextColor32Range(0x80ffffff, 0xa0ffffff),
								// 		color_b = random.NextColor32Range(0x00ffffff, 0x00808080)
								// 	});
								// }

								for (var i = 0; i < 3; i++)
								{
									Particle.Spawn(ref region, new Particle.Data()
									{
										texture = tex_spark,
										lifetime = random.NextFloatRange(1.00f, 2.00f),
										pos = pos + random.NextVector2(0.20f),
										vel = (dir * random.NextFloatRange(0, 40)).RotateByRad(random.NextFloatRange(-1.15f, 1.15f)) + random.NextVector2(15),
										fps = 0,
										frame_count = 1,
										frame_offset = random.NextByteRange(0, 4),
										frame_count_total = 4,
										scale = random.NextFloatRange(0.20f, 0.60f),
										growth = -random.NextFloatRange(0.60f, 1.00f),
										force = new Vector2(0.00f, random.NextFloatRange(20, 80)),
										drag = random.NextFloatRange(0.01f, 0.10f),
										stretch = new Vector2(random.NextFloatRange(1.00f, 2.00f), 0.75f),
										color_a = random.NextColor32Range(0xffffffff, 0xffc89eff),
										color_b = random.NextColor32Range(0xffffc0b0, 0xffffa090),
										lit = 1.00f,
										face_dir_ratio = 1.00f,
									});
								}

#endif

								ref var heat_state = ref ent_arbiter.GetComponent<Heat.State>();
								if (heat_state.IsNotNull())
								{
#if SERVER
									//heat_state.AddPower(20000.00f, info.DeltaTime);

									heat_state.AddEnergy(2450.00f);
									heat_state.Sync(ent_arbiter, true);
#endif
								}
								else
								{
#if SERVER

									var damage = random.NextFloatExtra(250, 750);

									Damage.Hit(ent_attacker: entity, ent_owner: entity, ent_target: ent_arbiter,
									position: pos, velocity: dir * 8.00f, normal: -dir,
									damage_integrity: damage, damage_durability: damage, damage_terrain: damage,
									target_material_type: arbiter.GetMaterial(), damage_type: Damage.Type.Electricity,
									yield: 0.95f, size: 1.50f, impulse: -150.00f, flags: Damage.Flags.No_Loot_Pickup);
#endif
								}
							}
						}
					}
				}
			}
		}
	}

	public static partial class Arcer
	{
		public enum Type: byte
		{
			Undefined,

			Single,
			Dual,
			Triple
		}

		[Flags]
		public enum Flags: uint
		{
			None = 0u,

			Insulated = 1u << 0,
			Continuous = 1u << 1,
		}

		[IComponent.Data(Net.SendType.Reliable, region_only: true)]
		public partial struct Data: IComponent
		{
			public Arcer.Flags flags;
			public ISubstance.Handle h_substance_electrode;
			public Distance electrode_separation;

			public Sound.Handle sound_hit = Arcer.sound_hit_default;

			public Arcer.Type type;

			public float damage_integrity = 400.00f;
			public float damage_durability = 4000.00f;
			public float damage_terrain = 200.00f;

			[Editor.Picker.Position(true)]
			public Vector2 offset;

			public Data()
			{

			}
		}

		//[IComponent.Data(Net.SendType.Unreliable, region_only: true)]
		//public partial struct State: IComponent
		//{
		//	[Flags]
		//	public enum Flags: byte
		//	{
		//		None = 0,

		//		Active = 1 << 0,
		//	}

		//	public float charge;
		//	public Arcer.State.Flags flags;

		//	public State()
		//	{

		//	}
		//}

		public static readonly Sound.Handle sound_hit_default = "arcane_infuser.fizzle.00";

		public static readonly Texture.Handle tex_blank = "blank";
		public static readonly Texture.Handle tex_light = "light.circle.00";
		public static readonly Texture.Handle tex_smoke = "BiggerSmoke_Light";
		public static readonly Texture.Handle tex_spark = "metal_spark.01";

		//[ISystem.Update.C(ISystem.Mode.Single, ISystem.Scope.Region)]
		//public static void OnUpdate(ISystem.Info info, ref Region.Data region, ref XorRandom random, Entity entity,
		//[Source.Owned] in Transform.Data transform, [Source.Owned] ref Body.Data body,
		//[Source.Owned] ref EssenceContainer.Data essence_container,
		//[Source.Owned] ref Arcer.Data arcer, [Source.Owned, Pair.Component<Arcer.Data>] ref Shape.Line shape)
		//{
		//	var time = info.WorldTime;
		//}

		[ISystem.Event<Melee.HitEvent>(ISystem.Mode.Single, ISystem.Scope.Region)]
		public static void OnHit(ISystem.Info info, Entity entity, ref Region.Data region, ref XorRandom random,
		ref Melee.HitEvent data, [Source.Owned] ref Arcer.Data arcer)
		{
#if CLIENT
			var dir = data.direction;

			for (var i = 0; i < 40; i++)
			{
				Particle.Spawn(ref region, new Particle.Data()
				{
					texture = tex_blank,
					lifetime = random.NextFloatRange(0.10f, 1.00f),
					pos = data.world_position + random.NextUnitVector2Range(0.10f, 0.20f),
					vel = dir.RotateByRad(random.NextFloatRange(-2.50f, 2.50f)) * random.NextFloatRange(0, 80),
					fps = 0,
					scale = random.NextFloatRange(0.50f, 1.50f),
					growth = -random.NextFloatRange(1.60f, 4.00f),
					force = random.NextUnitVector2Range(0, 80),
					drag = random.NextFloatRange(0.00f, 0.40f),
					stretch = new Vector2(random.NextFloatRange(0.50f, 1.50f), 0.05f),
					color_a = random.NextColor32Range(0xffffffff, 0xffc89eff),
					color_b = random.NextColor32Range(0xffc89eff, 0xffffffff),
					lit = random.NextFloatRange(0.50f, 1.00f),
					face_dir_ratio = 1.00f
				});
			}

			for (var i = 0; i < 4; i++)
			{
				Particle.Spawn(ref region, new Particle.Data()
				{
					texture = tex_light,
					lifetime = random.NextFloatRange(0.01f, 0.10f),
					pos = data.world_position + random.NextVector2(0.10f),
					vel = dir.RotateByRad(random.NextFloatRange(-2.00f, 2.00f)) * random.NextFloatRange(10.00f, 40.00f),
					force = dir.RotateByRad(random.NextFloatRange(-0.20f, 0.20f)) * random.NextFloatRange(0, 200),
					scale = random.NextFloatRange(2.00f, 30.00f),
					rotation = random.NextFloatRange(-3.50f, 3.50f),
					angular_velocity = random.NextFloat(0.50f),
					growth = random.NextFloatRange(10.00f, 200.00f),
					drag = random.NextFloatRange(0.07f, 0.25f),
					color_a = random.NextColor32Range(0xffffffff, 0xffc89eff),
					color_b = random.NextColor32Range(0xffc89eff, 0xffffffff),
					glow = random.NextFloatRange(2.00f, 8.00f),
				});
			}

			for (var i = 0; i < 4; i++)
			{
				Particle.Spawn(ref region, new Particle.Data()
				{
					texture = tex_smoke,
					lifetime = random.NextFloatRange(3.00f, 5.00f),
					pos = data.world_position + random.NextVector2(1.00f),
					vel = random.NextUnitVector2Range(0.50f * i, 1.50f * i) + random.NextVector2(5.00f),
					fps = random.NextByteRange(5, 10),
					frame_count = 64,
					frame_count_total = 64,
					frame_offset = random.NextByteRange(0, 64),
					//scale = random.NextFloatRange(1.50f, 2.50f),
					scale = random.NextFloatRange(0.50f, 0.80f),
					rotation = random.NextFloat(10.00f),
					angular_velocity = random.NextFloat(1.00f),
					growth = random.NextFloatRange(0.15f, 0.20f),
					drag = random.NextFloatRange(0.02f, 0.04f),
					force = new Vector2(0.00f, random.NextFloatRange(0, -2)),
					color_a = random.NextColor32Range(0x80ffffff, 0xa0ffffff),
					color_b = random.NextColor32Range(0x00ffffff, 0x00808080)
				});
			}

			for (var i = 0; i < 20; i++)
			{
				Particle.Spawn(ref region, new Particle.Data()
				{
					texture = tex_spark,
					lifetime = random.NextFloatRange(1.00f, 2.00f),
					pos = data.world_position + random.NextVector2(0.20f),
					vel = (dir * random.NextFloatRange(0, 40)).RotateByRad(random.NextFloatRange(-1.15f, 1.15f)) + random.NextVector2(15),
					fps = 0,
					frame_count = 1,
					frame_offset = random.NextByteRange(0, 4),
					frame_count_total = 4,
					scale = random.NextFloatRange(0.20f, 0.60f),
					growth = -random.NextFloatRange(0.60f, 1.00f),
					force = new Vector2(0.00f, random.NextFloatRange(20, 80)),
					drag = random.NextFloatRange(0.01f, 0.10f),
					stretch = new Vector2(random.NextFloatRange(1.00f, 2.00f), 0.75f),
					color_a = random.NextColor32Range(0xffffffff, 0xffc89eff),
					color_b = random.NextColor32Range(0xffffc0b0, 0xffffa090),
					lit = 1.00f,
					face_dir_ratio = 1.00f,
				});
			}
#endif

#if SERVER
			Sound.Play(ref region, arcer.sound_hit, data.world_position, volume: 1.00f, pitch: random.NextFloatRange(0.70f, 0.85f), size: 1.00f);
			Shake.Emit(ref region, data.world_position, 0.50f, 0.80f, 20.00f);

			var multiplier = 0.50f + (MathF.Pow(random.NextFloatRange(0.00f, 1.00f), 2.00f) * 0.50f);

			Damage.Hit(ent_attacker: entity, ent_owner: data.ent_owner, ent_target: data.ent_target,
				position: data.world_position, velocity: data.direction * 8.00f, normal: -data.direction,
				damage_integrity: arcer.damage_integrity * multiplier, damage_durability: arcer.damage_durability * multiplier, damage_terrain: arcer.damage_terrain * multiplier,
				target_material_type: data.target_material_type, damage_type: Damage.Type.Electricity,
				yield: 0.95f, size: 1.50f, impulse: 0.00f, flags: Damage.Flags.No_Loot_Pickup);


			// Zap the holder too when hitting a metallic object
			if (data.target_material_type == Material.Type.Metal && data.ent_target.IsAlive())
			{
				var ent_holder_arm = data.ent_target.GetParent(Relation.Type.Child);
				if (ent_holder_arm.IsAlive())
				{
					//App.WriteLine($"{ent_holder_arm.GetFullName()}");

					var oc_body = ent_holder_arm.GetComponentWithOwner<Body.Data>(Relation.Type.Instance, false);
					if (oc_body.IsValid())
					{
						ref var body_holder = ref oc_body.data;
						var result = body_holder.GetClosestPoint(data.world_position, allow_inside: true);

						var impulse_mult = 1.00f;
						if (result.material_type == Material.Type.Flesh || result.material_type == Material.Type.Insect)
						{
							impulse_mult *= 6.00f;

							if (random.NextBool(0.70f))
							{
								Arm.Drop(ent_holder_arm, target: data.ent_target, direction: -data.direction);
							}
						}

						Damage.Hit(ent_attacker: entity, ent_owner: data.ent_owner, ent_target: result.entity,
							position: result.world_position, velocity: data.direction * 4.00f, normal: -data.direction,
							damage_integrity: arcer.damage_integrity * multiplier * 0.25f, damage_durability: arcer.damage_durability * multiplier * 0.35f, damage_terrain: arcer.damage_terrain * multiplier * 0.25f,
							target_material_type: result.material_type, damage_type: Damage.Type.Electricity, pain: 1.10f,
							yield: 0.95f, size: 1.00f, impulse: 100.00f * impulse_mult, flags: Damage.Flags.No_Loot_Pickup);
					}
				}
			}


			//if (random.NextBool(0.10f))
			//{
			//	Fire.Ignite(data.ent_target, random.NextFloatRange(1.00f, 4.00f), Fire.Flags.No_Radius_Ignite);
			//}
#endif

		}
	}
}
