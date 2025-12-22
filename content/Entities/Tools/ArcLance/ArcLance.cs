namespace TC2.Base.Components
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
		public partial struct Data(): IComponent
		{
			public Electrode.Flags flags;

			public ISubstance.Handle h_substance_electrode_a;
			public ISubstance.Handle h_substance_electrode_b;

			public Distance electrode_separation;
			public Electrode.Type type;

			[Editor.Picker.Position(true)]
			public Vector2 offset_a;

			[Editor.Picker.Position(true)]
			public Vector2 offset_b;

			public Energy capacity;
			//public Power discharge_rate = Power.kW(5000.00f);

			//public Chance chance_discharge = new Chance(10);

			public float discharge_chance_mult = 1.00f;

			public Sound.Handle h_sound_on;
			public Sound.Handle h_sound_off;

			public Mouse.Key key_activate = Mouse.Key.Left;
		}

		[IComponent.Data(Net.SendType.Unreliable, region_only: true)]
		public partial struct State(): IComponent
		{
			[Flags]
			public enum Flags: byte
			{
				None = 0,

				Active = 1 << 0,
			}

			public Energy charge;
			public Electrode.State.Flags flags;

			[Save.Ignore, Net.Ignore] public float t_next_update;
			[Save.Ignore, Net.Ignore] public float t_next_discharge;
			[Save.Ignore, Net.Ignore] public float t_next_sync;
		}

		public static readonly Sound.Handle sound_hit_default = "arcane_infuser.fizzle.00";

		public static readonly Texture.Handle tex_blank = "blank";
		public static readonly Texture.Handle tex_light = "light.circle.00";
		public static readonly Texture.Handle tex_smoke = "BiggerSmoke_Light";
		public static readonly Texture.Handle tex_spark = "metal_spark.01";

		public static readonly Sound.Handle[] sounds_zap =
		{
			"essence.discharge.01"
		};

		public static readonly Sound.Handle[] sounds_arc =
		{
			"effect.zap.00"
		};

		[ISystem.Update.B(ISystem.Mode.Single, ISystem.Scope.Region)]
		public static void OnUpdateControls(ISystem.Info info, ref Region.Data region, ref XorRandom random, Entity entity,
		[Source.Owned] in Transform.Data transform, [Source.Owned] ref Body.Data body, [Source.Owned] ref Control.Data control,
		[Source.Owned] ref Electrode.Data electrode, [Source.Owned] ref Electrode.State electrode_state)
		{
#if SERVER
			if (electrode_state.flags.TrySetFlag(Electrode.State.Flags.Active, control.mouse.GetKey(electrode.key_activate)))
			{
				electrode_state.Sync(entity, true);

				Sound.Play(ref region, electrode_state.flags.HasAny(State.Flags.Active) ? electrode.h_sound_on : electrode.h_sound_off, transform.position, volume: 0.50f, pitch: 1.00f);
			}
#endif
		}

		[IEvent.Data]
		public partial struct DischargeEvent(): IEvent
		{
			public Vector2 pos;
			public Vector2 pos_target;

			public Entity ent_target;

			public Energy energy_discharged;
			public float modifier;
			public float radius;
		}

		public static Texture.Handle tex_light_box = "light.box.00";

		[ISystem.Event<Electrode.DischargeEvent>(ISystem.Mode.Single, ISystem.Scope.Region)]
		public static void OnDischarge(ISystem.Info info, Entity entity, ref Region.Data region, ref XorRandom random, ref Electrode.DischargeEvent ev,
		[Source.Owned] in Transform.Data transform,
		[Source.Owned] ref Electrode.Data electrode, [Source.Owned] ref Electrode.State electrode_state)
		{
#if CLIENT
			Sound.Play("essence.discharge.01", ev.pos, volume: random.NextFloatRange(1.20f, 1.80f), pitch: random.NextFloatRange(0.80f, 1.50f), size: 4.00f, dist_multiplier: 1.35f);
			Shake.Emit(ref region, ev.pos, 0.50f, 0.65f, 40.00f);

			var dir = (ev.pos_target - ev.pos).GetNormalized(out var dist);

			for (var i = 0; i < 15; i++)
			{
				Particle.Spawn(ref region, new Particle.Data()
				{
					texture = tex_blank,
					lifetime = random.NextFloatRange(0.10f, 0.80f),
					pos = ev.pos + random.NextUnitVector2Range(0.00f, 0.50f),
					vel = random.NextUnitVector2Range(2.00f, 10.00f) + (region.GetGravity() * random.NextFloatRange(0, 1)),
					fps = 0,
					scale = random.NextFloatRange(0.40f, 1.00f),
					growth = -random.NextFloatRange(0.50f, 1.00f),
					force = random.NextUnitVector2Range(0, 10) + region.GetGravity(),
					drag = random.NextFloatRange(0.00f, 0.20f),
					angular_velocity = random.NextFloatRange(-0.10f, 0.10f),
					stretch = new Vector2(random.NextFloatRange(0.50f, 1.50f), 0.05f),
					color_a = random.NextColor32Range(0xffffffff, 0xffc89eff),
					color_b = random.NextColor32Range(0xffc89eff, 0xffffffff),
					lit = 1.00f,
					face_dir_ratio = 1.00f
				});
			}

			for (var i = 0; i < 1; i++)
			{
				Particle.Spawn(ref region, new Particle.Data()
				{
					texture = tex_light,
					lifetime = random.NextFloatRange(0.05f, 0.10f),
					pos = ev.pos,
					vel = dir.RotateByRad(random.NextFloatRange(-2.00f, 2.00f)) * random.NextFloatRange(10.00f, 40.00f),
					force = dir.RotateByRad(random.NextFloatRange(-0.20f, 0.20f)) * random.NextFloatRange(0, 200),
					scale = random.NextFloatRange(15.00f, 30.00f),
					rotation = random.NextFloatRange(-3.50f, 3.50f),
					angular_velocity = random.NextFloat(0.50f),
					growth = random.NextFloatRange(100.00f, 200.00f),
					drag = random.NextFloatRange(0.07f, 0.25f),
					color_a = random.NextColor32Range(0xffffffff, 0xffc89eff),
					color_b = random.NextColor32Range(0xffc89eff, 0xffffffff),
					glow = random.NextFloatRange(4.00f, 8.00f),
				});
			}

			var spark_count = 10;
			var step = dist / spark_count;

			if (Camera.IsVisible(Camera.CullType.Rect2x, ev.pos))
			{
				for (var i = 0; i < spark_count; i++)
				{
					Particle.Spawn(ref region, new Particle.Data()
					{
						texture = tex_light_box,
						lifetime = random.NextFloatRange(0.05f, 0.20f),
						pos = ev.pos + (dir * step * i) + random.NextUnitVector2Range(0.00f, 0.10f),
						vel = (dir + random.NextUnitVector2Range(0.00f, 0.50f)) * (i * step * 4),
						fps = 0,
						scale = random.NextFloatRange(0.50f, 2.00f),
						growth = random.NextFloatRange(0.50f, 6.00f),
						force = random.NextUnitVector2Range(0, 20) + (dir * random.NextFloatRange(10, 40)),
						drag = 0.05f,
						stretch = new Vector2(random.NextFloatRange(0.50f, 2.00f), random.NextFloatRange(0.10f, 0.30f)),
						color_a = random.NextColor32Range(0xffffffff, 0xffc89eff),
						color_b = random.NextColor32Range(0xffc89eff, 0xff2080ff),
						glow = random.NextFloatRange(30.00f, 60.00f),
						//lit = random.NextFloatRange(0.00f, 1.00f),
						face_dir_ratio = random.NextFloatRange(0.50f, 1.00f)
					});
				}

				for (var i = 0; i < 40; i++)
				{
					Particle.Spawn(ref region, new Particle.Data()
					{
						texture = tex_blank,
						lifetime = random.NextFloatRange(0.10f, 0.50f),
						pos = ev.pos_target,
						vel = dir.RotateByRad(random.NextFloatRange(-0.50f, 0.50f)) * random.NextFloatRange(-5, 20),
						fps = 0,
						scale = random.NextFloatRange(0.80f, 1.90f),
						growth = -random.NextFloatRange(0.50f, 1.00f),
						force = random.NextUnitVector2Range(0, 40),
						drag = random.NextFloatRange(0.00f, 0.40f),
						angular_velocity = random.NextFloatRange(-0.50f, 0.50f),
						stretch = new Vector2(random.NextFloatRange(0.50f, 1.50f), 0.05f),
						color_a = random.NextColor32Range(0xffffffff, 0xffc89eff),
						color_b = random.NextColor32Range(0xffc89eff, 0xffffffff),
						lit = 1.00f,
						face_dir_ratio = 1.00f
					});
				}

				for (var i = 0; i < 4; i++)
				{
					Particle.Spawn(ref region, new Particle.Data()
					{
						texture = tex_light,
						lifetime = random.NextFloatRange(0.05f, 0.10f),
						pos = Vector2.Lerp(ev.pos, ev.pos_target, i * 0.25f),
						vel = dir.RotateByRad(random.NextFloatRange(-2.00f, 2.00f)) * random.NextFloatRange(10.00f, 40.00f),
						force = dir.RotateByRad(random.NextFloatRange(-0.20f, 0.20f)) * random.NextFloatRange(0, 200),
						scale = random.NextFloatRange(15.00f, 30.00f),
						rotation = random.NextFloatRange(-3.50f, 3.50f),
						angular_velocity = random.NextFloat(0.50f),
						growth = random.NextFloatRange(100.00f, 200.00f),
						drag = random.NextFloatRange(0.07f, 0.25f),
						color_a = random.NextColor32Range(0xffffffff, 0xffc89eff),
						color_b = random.NextColor32Range(0xffc89eff, 0xffffffff),
						glow = random.NextFloatRange(4.00f, 8.00f),
					});
				}

				for (var i = 0; i < 4; i++)
				{
					Particle.Spawn(ref region, new Particle.Data()
					{
						texture = tex_smoke,
						lifetime = random.NextFloatRange(3.00f, 5.00f),
						pos = ev.pos + random.NextVector2(1.00f),
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

				for (var i = 0; i < 5; i++)
				{
					Particle.Spawn(ref region, new Particle.Data()
					{
						texture = tex_spark,
						lifetime = random.NextFloatRange(1.00f, 2.00f),
						pos = ev.pos,
						vel = (dir * random.NextFloatRange(0, 20)).RotateByRad(random.NextFloatRange(-2.00f, 2.00f)) + random.NextVector2(15),
						fps = 0,
						frame_count = 1,
						frame_offset = random.NextByteRange(0, 4),
						frame_count_total = 4,
						scale = random.NextFloatRange(0.20f, 0.60f),
						growth = -random.NextFloatRange(0.60f, 1.00f),
						force = new Vector2(0.00f, random.NextFloatRange(10, 40)),
						drag = random.NextFloatRange(0.01f, 0.10f),
						stretch = new Vector2(random.NextFloatRange(1.00f, 2.00f), 0.75f),
						color_a = random.NextColor32Range(0xffffffff, 0xffc89eff),
						color_b = random.NextColor32Range(0xffffc0b0, 0xffffa090),
						lit = 1.00f,
						face_dir_ratio = 1.00f,
					});
				}
			}
#endif

#if SERVER
			//region.SpawnResource("ozone", ev.pos_target, random.NextFloatExtra(10, 150), 1.50f, Resource.SpawnFlags.No_Offset | Resource.SpawnFlags.Merge);
#endif
		}

		public const float sync_interval = 0.50f;

		[ISystem.LateUpdate(ISystem.Mode.Single, ISystem.Scope.Region)]
		public static void OnUpdateEffects(ISystem.Info info, ref Region.Data region, ref XorRandom random, Entity entity,
		[Source.Owned] in Transform.Data transform,
		[Source.Owned] ref Electrode.Data electrode, [Source.Owned] ref Electrode.State electrode_state,
		[Source.Owned, Pair.Component<Electrode.Data>, Optional(true)] ref Light.Data light,
		[Source.Owned, Pair.Component<Electrode.Data>, Optional(true)] ref Sound.Emitter sound_emitter)
		{
#if CLIENT
			var discharge_modifier = CalculateModifier(electrode_state.charge.MJ(), 2.95f, 0.05f, 0.06f);

			if (sound_emitter.IsNotNull())
			{
				sound_emitter.volume_mult.LerpFMARef(discharge_modifier * 1.70f, 0.02f);
				sound_emitter.pitch_mult.LerpFMARef(Maths.Lerp01(0.40f, 2.75f, discharge_modifier * 2.10f), 0.05f);
			}

			if (electrode_state.charge.MJ() > 0.50f && random.NextBool(discharge_modifier * 4.00f))
			{
				//var discharge_modifier = CalculateModifier(electrode_state.charge.MJ(), 2.95f, 0.05f, 0.06f);
				var pos = transform.LocalToWorld(random.NextBool(0.50f) ? electrode.offset_a : electrode.offset_b);
				Shake.Emit(ref region, pos, 0.37f * discharge_modifier, 1.20f * discharge_modifier, 60.00f * discharge_modifier);

				var dir = transform.GetDirection();
				for (var i = 0; i < 2; i++)
				{
					Particle.Spawn(ref region, new Particle.Data()
					{
						texture = Light.tex_light_fire_00,
						lifetime = random.NextFloatRange(0.01f, 0.11f),
						pos = pos + random.NextUnitVector2(0.07f),
						vel = dir.RotateByRad(random.NextFloatRange(-0.20f, 0.20f)) * random.NextFloatRange(0, 10),
						force = dir.RotateByRad(random.NextFloatRange(-0.20f, 0.20f)) * random.NextFloatRange(50, 100),
						scale = random.NextFloatRange(0.10f, 1.20f),
						//rotation = random.NextFloatRange(-0.50f, 0.50f),
						//angular_velocity = random.NextFloat(0.50f),
						face_dir_ratio = 1.00f,
						stretch = new Vector2(random.NextFloatRange(0.50f, 0.70f), 0.85f),
						growth = random.NextFloatRange(100.00f, 500.00f),
						drag = random.NextFloatRange(0.01f, 0.15f),
						color_a = random.NextColor32Range(0xffcfafff, 0x88c89eff),
						color_b = random.NextColor32Range(0x00c89eff, 0x007fcfff),
						glow = random.NextFloatRange(4.00f, 8.00f) * Maths.Mulpo(discharge_modifier, 0.75f),
					});
				}

				for (var i = 0; i < 2; i++)
				{
					Particle.Spawn(ref region, new Particle.Data()
					{
						texture = tex_blank,
						lifetime = random.NextFloatRange(0.01f, 0.07f),
						pos = pos + random.NextUnitVector2Range(0.10f, 0.30f),
						vel = random.NextUnitVector2Range(2.00f, 10.00f) + (region.GetGravity() * random.NextFloatRange(0.10f, 0.30f)),
						fps = 0,
						scale = random.NextFloatRange(0.40f, 1.50f),
						growth = -random.NextFloatRange(0.20f, 1.00f),
						force = random.NextUnitVector2Range(0, 15),
						drag = random.NextFloatRange(0.01f, 0.20f),
						angular_velocity = random.NextFloatRange(-0.10f, 0.10f),
						stretch = new Vector2(random.NextFloatRange(0.50f, 0.70f), 0.05f),
						color_a = random.NextColor32Range(0xffffffff, 0xffc89eff).WithAlphaMult((discharge_modifier * 1.50f).Clamp01()),
						color_b = random.NextColor32Range(0xffc89eff, 0xffffffff),
						lit = 1.00f,
						face_dir_ratio = 1.00f
					});
				}
			}
#endif

		}

		// https://www.desmos.com/calculator/1fedzprrld
		public static float CalculateModifier(float energy_mj, float i, float j, float k)
		{
			return Maths.Clamp01((Maths.Pow(i, energy_mj * j) * k) - k);
		}

		[Shitcode]
		[ISystem.Update.C(ISystem.Mode.Single, ISystem.Scope.Region)]
		public static void OnUpdate(ISystem.Info info, ref Region.Data region, ref XorRandom random, Entity entity,
		[Source.Owned] in Transform.Data transform, [Source.Owned] ref Essence.Container.Data essence_container,
		[Source.Owned] ref Electrode.Data electrode, [Source.Owned] ref Electrode.State electrode_state)
		{
			electrode_state.charge *= 0.999f;
			var time = info.WorldTime;

#if SERVER
			if (electrode_state.flags.HasAny(Electrode.State.Flags.Active))
			{
				var dt = info.DeltaTime;
				var power = essence_container.GetElectricPower();
				if (electrode_state.charge.m_value.TryAddTowards(electrode.capacity.m_value, power.m_value * dt))
				{
					if (time >= electrode_state.t_next_sync || electrode_state.charge >= electrode.capacity)
					{
						electrode_state.t_next_sync = time + sync_interval;
						electrode_state.Sync(entity, true);
					}
				}
			}

			if (electrode_state.charge >= 100.00f && time >= electrode_state.t_next_discharge)
			{
				var discharge_modifier = CalculateModifier(electrode_state.charge.MJ(), 2.95f, 0.05f, 0.06f); // electrode_state.charge.m_value.Cbrt(); //.MJ() * 0.10f;
				var chance = discharge_modifier * electrode.discharge_chance_mult;
				//App.WriteLine(discharge_modifier);

				if (random.NextBool(chance))
				{
					var energy_discharged = electrode_state.charge.m_value.MultSub(random.NextFloatExtra(0.70f, 0.25f));
					var radius = Maths.Clamp(16.00f * discharge_modifier * random.NextFloatExtra(0.50f, 0.35f), 1.00f, 64.00f);
					var pos = transform.LocalToWorld(random.NextBool(0.50f) ? electrode.offset_a : electrode.offset_b);

					var ev = new Electrode.DischargeEvent
					{
						energy_discharged = energy_discharged,
						radius = radius,
						pos = pos,
						pos_target = pos + (transform.GetDirection() * radius) + random.NextUnitVector2(radius * 0.25f),
						modifier = discharge_modifier
					};

					entity.TriggerEventDeferred(ev, sync: true);
					electrode_state.Sync(entity, true);
				}

				electrode_state.t_next_discharge = time + random.NextFloatExtra(0.50f, 1.50f);
			}
#endif
		}

		[ISystem.Update.D(ISystem.Mode.Single, ISystem.Scope.Region)]
		public static void OnUpdateContacts(ISystem.Info info, ref Region.Data region, ref XorRandom random, Entity entity,
		[Source.Owned] in Transform.Data transform, [Source.Owned] ref Body.Data body,
		[Source.Owned] ref Electrode.Data electrode, [Source.Owned] ref Electrode.State electrode_state,
		[Source.Owned, Pair.Index(0)] ref Shape.Circle shape_electrode_a,
		[Source.Owned, Pair.Index(1), Optional(true)] ref Shape.Circle shape_electrode_b)
		{
			if (electrode_state.charge < 10.00f) return;

			if (body.HasArbiters())
			{
				var pairs_span = FixedArray.CreateSpan8<uint>(out var pairs_buffer);

				foreach (var arbiter in body.GetArbiters())
				{
					if (arbiter.HasShape(shape_electrode_a) || (shape_electrode_b.IsNotNull() && arbiter.HasShape(shape_electrode_b)))
					{
						var ent_arbiter = arbiter.GetEntity();
						var state = arbiter.GetState();

						var pos = arbiter.GetContactsPosition();

						if (electrode.type == Electrode.Type.Dual && state == Body.Arbiter.State.Begin)
						{
							var discharged_amount = electrode_state.charge.m_value.MultDiff(0.35f);
							var modifier = 0.50f + (discharged_amount * 0.001f);

							var dir = arbiter.GetNormal();
							//App.WriteLine(modifier);

#if CLIENT
							var h_sound_zap = sounds_zap.GetRandom(ref random);

							if (modifier > 25.00f) h_sound_zap = "essence.blast.00";
							else if (modifier > 15.50f) h_sound_zap = "essence.discharge.02";
							else if (modifier > 7.25f) h_sound_zap = "essence.collapse.electricity.01";
							else if (modifier > 2.25f) h_sound_zap = "essence.discharge.05";
							else h_sound_zap = "zapper.zap.01";

							//Sound.Play(sounds_zap.GetRandom(ref random), pos, volume: random.NextFloatExtra(1.40f, 0.50f) * modifier, pitch: random.NextFloatExtra(0.90f, 1.15f) / Maths.Clamp(0.50f + (modifier * 0.15f), 1.00f, 2.15f), dist_multiplier: 1.25f * modifier);
							Sound.Play(h_sound_zap, pos, volume: random.NextFloatExtra(1.50f, 0.30f) * Maths.Mulpo(modifier, 0.080f), pitch: random.NextFloatExtra(0.90f, 1.75f) * Maths.Lerp01(1.10f, 0.60f, Maths.Mulpo(modifier, 0.10f)), dist_multiplier: 1.25f * Maths.Mulpo(modifier, 0.070f), priority: 0.50f);
							Shake.Emit(ref region, pos, 0.37f * modifier, 0.60f * modifier, 24.00f * modifier);

							{
								var count = (int)(12 * Maths.Mulpo(modifier, 0.15f));
								for (var i = 0; i < count; i++)
								{
									Particle.Spawn(ref region, new Particle.Data()
									{
										texture = tex_blank,
										lifetime = random.NextFloatRange(0.10f, 1.00f) * Maths.Mulpo(modifier, 0.15f),
										pos = pos + random.NextUnitVector2Range(0.10f, 0.20f),
										vel = dir.RotateByRad(random.NextFloatRange(-2.50f, 2.50f)) * random.NextFloatRange(0, 80),
										fps = 0,
										scale = random.NextFloatRange(0.50f, 1.50f) * Maths.Mulpo(modifier, 0.05f),
										growth = -random.NextFloatRange(1.60f, 4.00f),
										force = random.NextUnitVector2Range(0, 80),
										drag = random.NextFloatRange(0.00f, 0.20f),
										stretch = new Vector2(random.NextFloatRange(0.50f, 1.50f), 0.05f),
										color_a = random.NextColor32Range(0xffffffff, 0xffc89eff),
										color_b = random.NextColor32Range(0xffc89eff, 0xffffffff),
										lit = random.NextFloatRange(0.50f, 1.00f),
										face_dir_ratio = 1.00f
									});
								}
							}

							{
								var count = (int)(4 * Maths.Mulpo(modifier, 0.15f));
								for (var i = 0; i < count; i++)
								{
									Particle.Spawn(ref region, new Particle.Data()
									{
										texture = tex_light,
										lifetime = random.NextFloatRange(0.01f, 0.10f) * Maths.Mulpo(modifier, 0.10f),
										pos = pos + random.NextVector2(0.10f),
										vel = dir.RotateByRad(random.NextFloatRange(-2.00f, 2.00f)) * random.NextFloatRange(10.00f, 40.00f) * Maths.Mulpo(modifier, 0.15f),
										force = dir.RotateByRad(random.NextFloatRange(-0.20f, 0.20f)) * random.NextFloatRange(0, 200),
										scale = random.NextFloatRange(5.00f, 30.00f),
										rotation = random.NextFloatRange(-3.50f, 3.50f),
										angular_velocity = random.NextFloat(0.50f),
										growth = random.NextFloatRange(100.00f, 200.00f) * Maths.Mulpo(modifier, 0.20f),
										drag = random.NextFloatRange(0.07f, 0.25f),
										color_a = random.NextColor32Range(0xffffffff, 0xffc89eff),
										color_b = random.NextColor32Range(0x0fc89eff, 0x0fffffff),
										glow = random.NextFloatRange(8.00f, 15.00f) * Maths.Mulpo(modifier, 0.10f),
									});
								}
							}

							{
								var count = (int)(3 * Maths.Mulpo(modifier, 0.35f));
								for (var i = 0; i < count; i++)
								{
									Particle.Spawn(ref region, new Particle.Data()
									{
										texture = tex_smoke,
										lifetime = random.NextFloatRange(1.20f, 3.00f) * Maths.Mulpo(modifier, 0.05f),
										pos = pos + random.NextVector2(0.50f) * Maths.Mulpo(modifier, 0.04f),
										vel = (random.NextUnitVector2Range(0.40f * i, 0.50f * i) + random.NextVector2(3.00f)) * Maths.Mulpo(modifier, 0.07f),
										fps = random.NextByteRange(5, 10),
										frame_count = 64,
										frame_count_total = 64,
										frame_offset = random.NextByteRange(0, 64),
										//scale = random.NextFloatRange(1.50f, 2.50f),
										scale = random.NextFloatRange(0.30f, 0.40f) * Maths.Mulpo(modifier, 0.07f),
										rotation = random.NextFloat(10.00f),
										angular_velocity = random.NextFloat(2.00f),
										growth = random.NextFloatRange(0.15f, 0.25f) * Maths.Mulpo(modifier, 0.10f),
										drag = random.NextFloatRange(0.02f, 0.04f),
										force = new Vector2(0.00f, random.NextFloatRange(0, -2)),
										color_a = random.NextColor32Range(0x80ffffff, 0xa0ffffff),
										color_b = random.NextColor32Range(0x00ffffff, 0x00808080)
									});
								}
							}

							{
								var count = (int)(3 * Maths.Mulpo(modifier, 0.35f));
								for (var i = 0; i < count; i++)
								{
									Particle.Spawn(ref region, new Particle.Data()
									{
										texture = tex_spark,
										lifetime = random.NextFloatRange(1.00f, 2.00f) * Maths.Mulpo(modifier, 0.15f),
										pos = pos + random.NextVector2(0.20f),
										vel = (dir * random.NextFloatRange(0, 40)).RotateByRad(random.NextFloatRange(-1.15f, 1.15f)) + random.NextVector2(15),
										fps = 0,
										frame_count = 1,
										frame_offset = random.NextByteRange(0, 4),
										frame_count_total = 4,
										scale = random.NextFloatRange(0.20f, 0.60f) * Maths.Mulpo(modifier, 0.05f),
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
							}
#endif

#if SERVER
							//var discharged_amount = electrode_state.charge.m_value.MultDiff(0.31f);

							ref var heat_state = ref ent_arbiter.GetComponent<Heat.State>();
							if (heat_state.IsNotNull())
							{
								heat_state.AddEnergy(discharged_amount * 1.30f);
								heat_state.Sync(ent_arbiter, true);
							}

							var damage = discharged_amount * random.NextFloatExtra(0.90f, modifier * 0.30f); // random.NextFloatExtra(450, 550);

							Damage.Hit(ent_attacker: entity, ent_owner: entity, ent_target: ent_arbiter,
							position: pos, velocity: -dir * 8.00f, normal: dir,
							damage_integrity: damage, damage_durability: damage, damage_terrain: damage,
							target_material_type: arbiter.GetMaterial(), damage_type: Damage.Type.Electricity,
							yield: 0.95f, size: 1.50f, impulse: 400.00f * modifier, flags: Damage.Flags.No_Loot_Pickup | Damage.Flags.No_Loot_Notification);

							Damage.Hit(ent_attacker: entity, ent_owner: entity, ent_target: entity,
							position: pos, velocity: dir * 4.00f, normal: -dir,
							damage_integrity: damage * 0.020f, damage_durability: damage * 0.020f, damage_terrain: damage * 0.020f,
							target_material_type: body.GetMaterial(), damage_type: Damage.Type.Electricity,
							yield: 0.95f, size: 1.10f, impulse: 100.00f * modifier, flags: Damage.Flags.No_Loot_Pickup | Damage.Flags.No_Loot_Notification);

							electrode_state.Sync(entity, true);
#endif
						}

						if (electrode.type == Electrode.Type.Single || (pairs_span.TryAddUnique(ent_arbiter.GetShortID(), out var contains) && contains))
						{
							//App.WriteLine($"touch {state}");

#if CLIENT
							Particle.Spawn(ref region, new Particle.Data()
							{
								texture = Light.tex_light_circle_04,
								lifetime = random.NextFloatExtra(0.20f, 0.35f),
								pos = arbiter.GetContactsPosition() - (arbiter.GetNormal() * 0.35f),
								scale = random.NextFloatRange(1.40f, 1.70f),
								growth = random.NextFloatRange(7.90f, 18.50f),
								color_a = ColorBGRA.RGBA(1.00f, 0.70f, 0.40f, 10.00f),
								color_b = ColorBGRA.RGBA(0.20f, 0.00f, 0.00f, 0.00f),
								glow = 1.20f
							});
#endif

							if (state == Body.Arbiter.State.Begin || random.NextBool(0.40f))
							{
								var discharged_amount = electrode_state.charge.m_value.MultDiff(0.65f);
								var modifier = 0.25f + (discharged_amount * 0.001f);

								var dir = random.NextUnitVector2Range(0.50f, 1.50f);

#if CLIENT

								if (state == Body.Arbiter.State.Begin || random.NextBool(0.30f))
								{
									//Sound.Play(sounds_arc.GetRandom(ref random), pos, volume: random.NextFloatExtra(0.50f, 1.00f), pitch: random.NextFloatExtra(0.70f, 1.75f), dist_multiplier: 0.75f, priority: 0.10f);
									Sound.Play(sounds_arc.GetRandom(ref random), pos, volume: random.NextFloatExtra(0.40f, 0.30f) * Maths.Mulpo(modifier, 0.080f), pitch: random.NextFloatExtra(0.70f, 1.45f) * Maths.Lerp01(1.10f, 0.50f, Maths.Mulpo(modifier, 0.10f)), dist_multiplier: 0.55f * Maths.Mulpo(modifier, 0.070f), priority: 0.10f);
								}
								//Shake.Emit(ref region, pos, 0.37f, 0.60f, 24.00f);

								//{   
								//	Particle.Spawn(ref region, new Particle.Data()
								//	{
								//		texture = Light.tex_light_circle_04,
								//		lifetime = random.NextFloatExtra(1.50f, 0.65f),
								//		pos = arbiter.GetContactsPosition() - (arbiter.GetNormal() * 0.35f),
								//		scale = random.NextFloatRange(1.40f, 1.70f),
								//		growth = random.NextFloatRange(3.90f, 4.50f),
								//		color_a = new Vector4(1.00f, 0.70f, 0.40f, 10.00f),
								//		color_b = new Vector4(0.20f, 0.00f, 0.00f, 0.00f),
								//		glow = 1.20f
								//	});
								//}

								{
									var count = (int)(random.NextFloatExtra(0.25f, 1.00f) * Maths.Mulpo(modifier, 0.75f));
									for (var i = 0; i < count; i++)
									{
										Particle.Spawn(ref region, new Particle.Data()
										{
											texture = tex_blank,
											lifetime = random.NextFloatRange(0.10f, 1.00f) * Maths.Mulpo(modifier, 0.15f),
											pos = pos + random.NextUnitVector2Range(0.10f, 0.20f),
											vel = dir.RotateByRad(random.NextFloatRange(-1.50f, 1.50f)) * random.NextFloatRange(0, 40),
											fps = 0,
											scale = random.NextFloatRange(0.50f, 1.50f) * Maths.Mulpo(modifier, 0.05f),
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
								}

								{
									for (var i = 0; i < 1; i++)
									{
										Particle.Spawn(ref region, new Particle.Data()
										{
											texture = tex_light,
											lifetime = random.NextFloatRange(0.05f, 0.15f) * Maths.Mulpo(modifier, 0.17f),
											pos = pos + random.NextVector2(0.80f),
											vel = dir.RotateByRad(random.NextFloatRange(-2.00f, 2.00f)) * random.NextFloatRange(5.00f, 10.00f) * Maths.Mulpo(modifier, 0.07f),
											force = dir.RotateByRad(random.NextFloatRange(-0.20f, 0.20f)) * random.NextFloatRange(0, 200),
											scale = random.NextFloatRange(15.00f, 50.00f),
											rotation = random.NextFloatRange(-3.50f, 3.50f),
											angular_velocity = random.NextFloat(0.50f),
											growth = random.NextFloatRange(250.00f, 400.00f) * Maths.Mulpo(modifier, 0.15f),
											drag = random.NextFloatRange(0.07f, 0.25f),
											color_a = random.NextColor32Range(0xffffffff, 0xffc89eff),
											color_b = random.NextColor32Range(0x0fc89eff, 0x0fffffff),
											glow = random.NextFloatRange(2.00f, 3.00f) * Maths.Mulpo(modifier, 0.10f),
										});
									}
								}

								if (random.NextBool(0.40f * modifier))
								{
									for (var i = 0; i < 1; i++)
									{
										Particle.Spawn(ref region, new Particle.Data()
										{
											texture = tex_smoke,
											lifetime = random.NextFloatRange(1.00f, 1.50f),
											pos = pos + random.NextVector2(0.80f),
											vel = random.NextUnitVector2Range(0.20f * i, 0.50f * i) + random.NextVector2(8.00f),
											fps = random.NextByteRange(5, 10),
											frame_count = 64,
											frame_count_total = 64,
											frame_offset = random.NextByteRange(0, 64),
											//scale = random.NextFloatRange(1.50f, 2.50f),
											scale = random.NextFloatRange(0.40f, 0.60f),
											rotation = random.NextFloat(10.00f),
											angular_velocity = random.NextFloat(2.80f),
											growth = random.NextFloatRange(0.15f, 0.25f),
											drag = random.NextFloatRange(0.01f, 0.03f),
											force = new Vector2(0.00f, random.NextFloatRange(0, -3)),
											color_a = random.NextColor32Range(0x70ffffff, 0xc0ffffff),
											color_b = random.NextColor32Range(0x00ffffff, 0x00808080)
										});
									}
								}

								{
									var count = (int)(random.NextFloatExtra(0.75f, 2.00f) * Maths.Mulpo(modifier, 0.75f));
									for (var i = 0; i < count; i++)
									{
										Particle.Spawn(ref region, new Particle.Data()
										{
											texture = tex_spark,
											lifetime = random.NextFloatRange(1.00f, 2.00f) * Maths.Mulpo(modifier, 0.15f),
											pos = pos + random.NextVector2(0.20f),
											vel = (dir * random.NextFloatRange(0, 20)).RotateByRad(random.NextFloatRange(-1.15f, 1.15f)) + random.NextVector2(15) + new Vector2(0, -random.NextFloatExtra(0.00f, 30.00f)),
											fps = 0,
											frame_count = 1,
											frame_offset = random.NextByteRange(0, 4),
											frame_count_total = 4,
											scale = random.NextFloatRange(0.20f, 0.60f),
											growth = -random.NextFloatRange(0.60f, 1.00f),
											force = new Vector2(0.00f, random.NextFloatRange(20, 80)),
											drag = random.NextFloatRange(0.01f, 0.10f),
											stretch = new Vector2(random.NextFloatRange(1.00f, 2.00f), 0.75f),
											color_a = random.NextColor32Range(0xffffddbb, 0xffc89eff),
											color_b = random.NextColor32Range(0xffffc0b0, 0xffffa090),
											lit = 1.00f,
											face_dir_ratio = 1.00f,
										});
									}
								}
#endif

#if SERVER

								ref var heat_state = ref ent_arbiter.GetComponent<Heat.State>();
								if (heat_state.IsNotNull())
								{
									//heat_state.AddPower(20000.00f, info.DeltaTime);

									heat_state.AddEnergy(discharged_amount * 1.35f);
									heat_state.Sync(ent_arbiter, true);
								}
								else
								{
									var damage = discharged_amount * random.NextFloatExtra(0.80f, 0.20f);

									Damage.Hit(ent_attacker: entity, ent_owner: entity, ent_target: ent_arbiter,
									position: pos, velocity: dir * 8.00f, normal: -dir,
									damage_integrity: damage, damage_durability: damage, damage_terrain: damage,
									target_material_type: arbiter.GetMaterial(), damage_type: Damage.Type.Electricity,
									yield: 0.95f, size: 1.50f, impulse: 0.00f, flags: Damage.Flags.No_Loot_Pickup | Damage.Flags.No_Loot_Notification);
								}

								electrode_state.Sync(entity, true);
#endif
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
		public partial struct Data(): IComponent
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
		//[Source.Owned] ref Essence.Container.Data essence_container,
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

			//var multiplier = 0.50f + (Maths.Pow2(random.NextFloat01()) * 0.50f);
			var multiplier = Maths.FMA(Maths.Pow2(random.NextFloat01()), 0.50f, 0.50f);

			Damage.Hit(ent_attacker: entity, ent_owner: data.ent_owner, ent_target: data.ent_target,
				position: data.world_position, velocity: data.direction * 8.00f, normal: -data.direction,
				damage_integrity: arcer.damage_integrity * multiplier, damage_durability: arcer.damage_durability * multiplier, damage_terrain: arcer.damage_terrain * multiplier,
				target_material_type: data.target_material_type, damage_type: Damage.Type.Electricity,
				yield: 0.95f, size: 1.50f, impulse: 0.00f, flags: Damage.Flags.No_Loot_Pickup | Damage.Flags.No_Loot_Notification);


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
								Arm.Drop(ent_parent: ent_holder_arm, ent_child: data.ent_target, direction: -data.direction);
							}
						}

						Damage.Hit(ent_attacker: entity, ent_owner: data.ent_owner, ent_target: result.entity,
							position: result.world_position, velocity: data.direction * 4.00f, normal: -data.direction,
							damage_integrity: arcer.damage_integrity * multiplier * 0.25f, damage_durability: arcer.damage_durability * multiplier * 0.35f, damage_terrain: arcer.damage_terrain * multiplier * 0.25f,
							target_material_type: result.material_type, damage_type: Damage.Type.Electricity, pain: 1.10f,
							yield: 0.95f, size: 1.00f, impulse: 100.00f * impulse_mult, flags: Damage.Flags.No_Loot_Pickup | Damage.Flags.No_Loot_Notification);
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
