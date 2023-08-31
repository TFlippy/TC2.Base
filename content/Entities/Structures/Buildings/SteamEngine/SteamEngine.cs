
using System.Text;

namespace TC2.Base.Components
{
	public static partial class SteamEngine
	{
		public static readonly Texture.Handle texture_smoke = "BiggerSmoke_Light";
		public static readonly Sprite sprite_burst_default = new Sprite("steam_engine.burst.00", 32, 24, 1, 0);

		[IComponent.Data(Net.SendType.Reliable, region_only: true)]
		public partial struct Data: IComponent
		{
			[Flags]
			public enum Flags: uint
			{
				None = 0,

				Linear_Piston = 1u << 0
			}

			[Editor.Picker.Position(relative: true, mark_modified: true)] public Vector2 piston_offset;
			[Editor.Picker.Position(relative: true, mark_modified: true)] public Vector2 steam_offset;
			[Editor.Picker.Position(relative: true, mark_modified: true)] public Vector2 exhaust_offset;
			[Editor.Picker.Position(relative: true, mark_modified: true)] public Vector2 burst_offset;
			public Vector2 smoke_direction = new(0, -1);

			public Sprite sprite_burst = SteamEngine.sprite_burst_default;

			public SteamEngine.Data.Flags flags;

			public float steam_size = 1.00f;
			public float steam_interval;
			public float piston_radius;
			public float water_capacity = 10.00f;

			public float speed_max;
			[Asset.Ignore] public float speed_target; // TODO: Move this into SteamEngine.State

			public float force;
			public float efficiency;
			public float burst_threshold = 0.85f;
			public float burst_chance_modifier = 1.00f;
			public float shake_multiplier = 1.00f;

			public float volume_multiplier = 1.00f;
			public float pitch_multiplier = 1.00f;

			public float max_acceleration = 1.00f;

			public Data()
			{
			}
		}

		[IComponent.Data(Net.SendType.Unreliable, region_only: true), Asset.Ignore]
		public partial struct State: IComponent
		{
			public float speed_current;
			public float force_current;
			public float temperature_current;
			public float pressure_current;

			[Save.Ignore, Net.Ignore] public float next_steam;
			[Save.Ignore, Net.Ignore] public float next_exhaust;
			[Save.Ignore, Net.Ignore] public float next_tick;
		}

		[IEvent.Data]
		public partial struct ExplodeEvent: IEvent
		{
			public float power = 0.00f;

			public ExplodeEvent()
			{

			}
		}

		public struct ConfigureRPC: Net.IRPC<SteamEngine.Data>
		{
			public float? speed_target;

#if SERVER
			public void Invoke(ref NetConnection connection, Entity entity, ref SteamEngine.Data data)
			{
				var sync = false;

				if (this.speed_target.HasValue)
				{
					data.speed_target = Maths.Clamp(this.speed_target.Value, 0.00f, data.speed_max);
					sync = true;
				}

				if (sync)
				{
					data.Sync(entity);
				}
			}
#endif
		}

#if SERVER
		[ISystem.Event<EssenceNode.FailureEvent>(ISystem.Mode.Single, ISystem.Scope.Region)]
		public static void OnFailure(ISystem.Info info, Entity entity, ref XorRandom random, ref Region.Data region, ref EssenceNode.FailureEvent data, 
		[Source.Owned] ref SteamEngine.Data steam_engine, [Source.Owned] ref SteamEngine.State steam_engine_state)
		{
			steam_engine.shake_multiplier *= random.NextFloatRange(2.00f, 3.50f);
			steam_engine.speed_target = random.NextFloatRange(0.00f, steam_engine.speed_max);
			steam_engine.Sync(entity);
		}
#endif

		public const float update_interval = 0.20f;

		//[ISystem.AddFirst(ISystem.Mode.Single, ISystem.Scope.Region), HasTag<SteamEngine.Data>("damaged", true, Source.Modifier.Owned)]
		[ISystem.Event<SteamEngine.ExplodeEvent>(ISystem.Mode.Single, ISystem.Scope.Region)]
		public static void OnExplode(ISystem.Info info, ref Region.Data region, ref XorRandom random, Entity entity, ref SteamEngine.ExplodeEvent data,
		[Source.Owned] in Transform.Data transform,
		[Source.Owned] ref SteamEngine.Data steam_engine, [Source.Owned] ref SteamEngine.State steam_engine_state)
		{
			var max_radius = Maths.Clamp(data.power * 10.00f, 10.00f, 100.00f);

			App.WriteLine("steam engine damaged");
#if CLIENT
			Sound.Play("explosion.steam.01", transform.position, volume: 1.50f, size: 1.00f, priority: 0.70f, dist_multiplier: 2.20f);
			Shake.Emit(ref region, transform.position, 1.00f, 1.00f, max_radius * 2.00f);

			var smoke_count = (int)Maths.Clamp(data.power * 10.00f, 10.00f, 50.00f);
			for (var i = 0; i < smoke_count; i++)
			{
				Particle.Spawn(ref region, new Particle.Data()
				{
					texture = texture_smoke,
					lifetime = random.NextFloatRange(12.00f, 30.00f),
					pos = transform.position + random.NextVector2(0.40f),
					vel = random.NextUnitVector2Range(0.00f, 15.00f) + new Vector2(random.NextFloatRange(-40, 40), random.NextFloatRange(-30, 10)),
					force = random.NextUnitVector2Range(0.00f, 2.00f) + new Vector2(random.NextFloatRange(0, 2), -random.NextFloatRange(1, 2)),
					fps = random.NextByteRange(3, 8),
					frame_count = 64,
					frame_count_total = 64,
					frame_offset = random.NextByteRange(0, 64),
					scale = random.NextFloatRange(1.50f, 2.00f),
					//rotation = random.NextFloat(4.00f),
					angular_velocity = random.NextFloat(1.00f),
					growth = random.NextFloatRange(0.05f, 0.20f),
					drag = random.NextFloatRange(0.05f, 0.10f),
					color_a = random.NextColor32Range(new Color32BGRA(200, 255, 255, 255), new Color32BGRA(140, 240, 240, 240)),
					color_b = new Color32BGRA(0, 240, 240, 240),
					stretch = new Vector2(random.NextFloatRange(0.40f, 1.00f), random.NextFloatRange(0.40f, 1.00f)),
					face_dir_ratio = random.NextFloatRange(0.00f, 1.00f),
				});
			}
#endif

#if SERVER
			entity.AddTag<SteamEngine.Data>("damaged");
#endif
		}

#if CLIENT
		[ISystem.Render(ISystem.Mode.Single, ISystem.Scope.Region), HasTag<SteamEngine.Data>("damaged", true, Source.Modifier.Owned)]
		public static void RenderBurst(ISystem.Info info, ref XorRandom random, [Source.Owned] in Transform.Data transform, [Source.Owned] in Animated.Renderer.Data renderer,
		[Source.Owned] ref SteamEngine.Data steam_engine, [Source.Owned] ref SteamEngine.State steam_engine_state)
		{
			Animated.Renderer.Draw(transform, new()
			{
				sprite = steam_engine.sprite_burst,
				z = renderer.z + 0.01f,
				//rotation = -(resizable.flags.HasAny(Resizable.Flags.Orthogonal) ? Maths.Snap(rot, MathF.PI * 0.50f) : rot),
				rotation = 0.00f,
				scale = Vector2.One,
				offset = steam_engine.burst_offset
			});
		}
#endif

#if SERVER
		[ISystem.Event<Health.PostDamageEvent>(ISystem.Mode.Single, ISystem.Scope.Region), HasTag<SteamEngine.Data>("damaged", false, Source.Modifier.Owned)]
		public static void OnPostDamage(ISystem.Info info, Entity entity, ref XorRandom random, ref Health.PostDamageEvent data, 
		[Source.Owned] ref Health.Data health, [Source.Owned] in Transform.Data transform,
		[Source.Owned] ref SteamEngine.Data steam_engine, [Source.Owned] ref SteamEngine.State steam_engine_state)
		{
			var modifier = Maths.NormalizeClamp(steam_engine_state.speed_current, steam_engine.speed_max);
			if (modifier > 0.30f)
			{
				var health_min = MathF.Min(health.integrity, health.durability);
				var chance = (1.00f - (health_min * 1.50f)) * modifier * 0.15f * steam_engine.burst_chance_modifier;

				//App.WriteLine($"boom {chance}");
				if (health_min <= steam_engine.burst_threshold && random.NextBool(chance))
				{
					var power = MathF.Pow(steam_engine.force * modifier * 0.001f, 0.50f);

					var ev = new SteamEngine.ExplodeEvent()
					{
						power = power
					};
					entity.TriggerEventDeferred(ev, sync: true);
				}
			}
		}
#endif

		[ISystem.LateUpdate(ISystem.Mode.Single, ISystem.Scope.Region), HasTag<SteamEngine.Data>("damaged", false, Source.Modifier.Owned)]
		public static void UpdateShake(ISystem.Info info,
		[Source.Owned] ref SteamEngine.Data steam_engine, [Source.Owned] ref SteamEngine.State steam_engine_state,
		[Source.Owned] ref Body.Data body,
		[Source.Owned] ref Axle.Data wheel, [Source.Owned] ref Axle.State wheel_state)
		{
			if (body.type == Body.Type.Dynamic)
			{
				body.AddForceLocal(new Vector2(MathF.Cos(wheel_state.rotation * 2.00f), (MathF.Sin(wheel_state.rotation * 2.00f)) * 1) * wheel_state.angular_velocity * wheel_state.angular_momentum * steam_engine.shake_multiplier, Vector2.Zero);
			}
		}

		[ISystem.Update(ISystem.Mode.Single, ISystem.Scope.Region), HasTag<SteamEngine.Data>("damaged", false, Source.Modifier.Owned)]
		public static void Update(ISystem.Info info, Entity entity, ref XorRandom random,
		[Source.Owned] ref SteamEngine.Data steam_engine, [Source.Owned] ref SteamEngine.State steam_engine_state,
		[Source.Owned] in Burner.Data burner, [Source.Owned] ref Burner.State burner_state,
		[Source.Owned] ref Axle.Data wheel, [Source.Owned] ref Axle.State wheel_state)
		{
			steam_engine_state.temperature_current = Maths.MoveTowards(steam_engine_state.temperature_current, Region.ambient_temperature, 1.00f);

			var torque = steam_engine.force * steam_engine.piston_radius;
			var power = (float)(burner_state.available_power * steam_engine.efficiency);
			var speed = torque > 0.00f ? power / torque : 0.00f;
			//steam_engine_state.speed_current = Maths.MoveTowards(steam_engine_state.speed_current, speed, steam_engine.max_acceleration * App.fixed_update_interval_s);
			steam_engine_state.speed_current = Maths.Lerp(steam_engine_state.speed_current, speed, 0.05f);

			wheel_state.SetAngularVelocity(torque, steam_engine_state.speed_current); // Maths.Lerp(wheel_state.angular_velocity, steam_engine_state.speed_current, 0.10f));

			var m = ((1.00f / steam_engine.speed_max) * (steam_engine.speed_target - wheel_state.angular_velocity));
			burner_state.modifier = (burner_state.modifier + (m * 0.01f)).Clamp01();

			if (info.WorldTime >= steam_engine_state.next_tick)
			{
				steam_engine_state.next_tick = info.WorldTime + update_interval;
			}
		}

		//		[ISystem.Update(ISystem.Mode.Single, ISystem.Scope.Region), HasTag<SteamEngine.Data>("damaged", false, Source.Modifier.Owned)]
		//		public static void UpdateOverloaded(ISystem.Info info, Entity entity, ref XorRandom random,
		//		[Source.Owned] ref SteamEngine.Data steam_engine, [Source.Owned] ref SteamEngine.State steam_engine_state,
		//		[Source.Owned] in Burner.Data burner, [Source.Owned] ref Burner.State burner_state, [Source.Owned] in Transform.Data transform,
		//		[Source.Owned] ref Axle.Data wheel, [Source.Owned] ref Axle.State wheel_state, [Source.Owned] ref Health.Data health)
		//		{
		//#if SERVER
		//			if (wheel_state.flags.HasAny(Axle.State.Flags.Revolved))
		//			{
		//				var health_min = MathF.Min(health.integrity, health.durability);
		//				var modifier = Maths.NormalizeClamp(steam_engine_state.speed_current, steam_engine.speed_max);

		//				if (health_min < steam_engine.burst_threshold * modifier)
		//				{
		//					var chance = (1.00f - (health_min * health_min)) * (modifier * 0.20f * steam_engine.burst_chance_modifier);

		//					//App.WriteLine($"{chance}");
		//					if (random.NextBool(chance))
		//					{
		//						entity.Hit(entity, entity, transform.position, random.NextUnitVector2Range(1, 1), random.NextUnitVector2Range(1, 1), random.NextFloatRange(50, steam_engine.force * modifier * 0.10f), Material.Type.Metal, Damage.Type.Blunt);
		//						//App.WriteLine("overloaded");
		//					}
		//				}
		//			}
		//#endif
		//		}

		[ISystem.Update(ISystem.Mode.Single, ISystem.Scope.Region), HasTag<SteamEngine.Data>("damaged", true, Source.Modifier.Owned)]
		public static void UpdateDamaged(ISystem.Info info,
		[Source.Owned] ref SteamEngine.Data steam_engine, [Source.Owned] ref SteamEngine.State steam_engine_state,
		[Source.Owned] in Burner.Data burner, [Source.Owned] ref Burner.State burner_state,
		[Source.Owned] ref Axle.Data wheel, [Source.Owned] ref Axle.State wheel_state)
		{
			var torque = steam_engine.force * steam_engine.piston_radius;
			var speed = 0.00f;
			//steam_engine_state.speed_current = Maths.MoveTowards(steam_engine_state.speed_current, speed, steam_engine.max_acceleration * App.fixed_update_interval_s);
			steam_engine_state.speed_current = Maths.Lerp(steam_engine_state.speed_current, speed, 0.25f);

			wheel_state.SetAngularVelocity(torque, steam_engine_state.speed_current); // Maths.Lerp(wheel_state.angular_velocity, steam_engine_state.speed_current, 0.10f));

			burner_state.modifier = 0.00f;

			if (info.WorldTime >= steam_engine_state.next_tick)
			{
				steam_engine_state.next_tick = info.WorldTime + update_interval;
			}
		}

#if CLIENT
		public partial struct SteamEngineGUI: IGUICommand
		{
			public Entity ent_steam_engine;
			public SteamEngine.Data steam_engine;
			public SteamEngine.State steam_engine_state;
			public Axle.Data wheel;
			public Axle.State wheel_state;
			public Burner.Data burner;
			public Burner.State burner_state;

			public void Draw()
			{
				using (var window = GUI.Window.InteractionMisc("Steam Engine", this.ent_steam_engine, new Vector2(280, 132)))
				{
					//this.StoreCurrentWindowTypeID();
					if (window.show)
					{
						using (GUI.Group.New(new(GUI.RmX, 96)))
						{
							//Boiler.DrawGauge(this.steam_engine_state.speed_current, 0.00f, this.steam_engine.speed_max);
							GUI.DrawGauge(MathF.Abs(this.wheel_state.angular_velocity), 0.00f, this.steam_engine.speed_max);

							//GUI.SameLine();
							//GUI.DrawTemperatureRange(this.steam_engine_state.temperature_current, Maths.CelsiusToKelvin(100), Maths.CelsiusToKelvin(375), size: new Vector2(24, GUI.RmY), color_a: GUI.col_white, color_b: GUI.col_white);

							GUI.SameLine();
							GUI.DrawTemperatureRange(this.burner_state.current_temperature, this.burner_state.current_temperature, 2000, size: new Vector2(24, GUI.RmY));
							//GUI.SameLine();
							//GUI.DrawWorkV(0.50f, size: new Vector2(24, GUI.RmY));

							GUI.SameLine();


							using (GUI.Group.New(new(GUI.RmX, 96)))
							{
								GUI.DrawInventoryDock(Inventory.Type.Fuel, new(48, 48));


								using (GUI.Group.New(padding: new(4, 4)))
								{
									//GUI.Text($"{MathF.Abs(this.wheel_state.angular_velocity):0.00}/{this.steam_engine.speed_max:0.00} rad/s");
									GUI.Text($"{MathF.Abs(this.wheel_state.angular_velocity):0.00} rad/s");
									GUI.Text($"{(this.wheel_state.old_tmp_torque):0.00} Nm/s");
									GUI.Text($"{(this.wheel_state.old_tmp_torque * MathF.Abs(this.wheel_state.angular_velocity) * 0.001f):0.00} kW");
									//GUI.Text($"{(this.wheel_state.old_tmp_torque * MathF.Abs(this.wheel_state.angular_velocity)):0.00} W");
									//GUI.Text($"{(this.burner_state.available_power):0.00} W");
								}
							}
						}

						//if (GUI.SliderFloat("Target Speed", ref this.steam_engine.speed_target, 0.00f, this.steam_engine.speed_max, size: new Vector2(160, 32)))
						if (GUI.SliderFloat("Target Speed", ref this.steam_engine.speed_target, 0.00f, this.steam_engine.speed_max, size: new Vector2(160, 32), snap: 0.100f))
						{
							var rpc = new SteamEngine.ConfigureRPC()
							{
								speed_target = this.steam_engine.speed_target
							};
							rpc.Send(this.ent_steam_engine);
						}
					}
				}
			}
		}

		[ISystem.EarlyGUI(ISystem.Mode.Single, ISystem.Scope.Region)]
		public static void OnGUI(Entity entity, [Source.Owned] in SteamEngine.Data steam_engine, [Source.Owned] in SteamEngine.State steam_engine_state,
		[Source.Owned] in Axle.Data wheel, [Source.Owned] in Axle.State wheel_state, [Source.Owned] in Burner.Data burner, [Source.Owned] in Burner.State burner_state,
		[Source.Owned] in Interactable.Data interactable)
		{
			if (interactable.show)
			{
				var gui = new SteamEngineGUI()
				{
					ent_steam_engine = entity,
					steam_engine = steam_engine,
					steam_engine_state = steam_engine_state,
					wheel = wheel,
					wheel_state = wheel_state,
					burner = burner,
					burner_state = burner_state
				};
				gui.Submit();
			}
		}

		[ISystem.VeryLateUpdate(ISystem.Mode.Single, ISystem.Scope.Region)]
		public static void UpdateRenderer(ISystem.Info info, ref Region.Data region, ref XorRandom random, [Source.Owned] in Transform.Data transform,
		[Source.Owned] in SteamEngine.Data steam_engine, [Source.Owned] ref SteamEngine.State steam_engine_state,
		[Source.Owned] in Axle.Data wheel, [Source.Owned] ref Axle.State wheel_state,
		[Source.Owned, Pair.Of<SteamEngine.Data>] ref Animated.Renderer.Data renderer_piston)
		{
			var speed = MathF.Abs(wheel_state.angular_velocity);
			if (speed > 1.00f && info.WorldTime >= steam_engine_state.next_steam)
			{
				steam_engine_state.next_steam = info.WorldTime + (steam_engine.steam_interval * Maths.Clamp(MathF.ReciprocalEstimate(speed * 0.10f), 0.50f, 1.50f));

				var delta = 0.00f;

				if (wheel_state.angular_velocity != 0.00f)
				{
					if (float.IsNegative(wheel_state.angular_velocity))
					{
						delta = steam_engine.speed_target - wheel_state.angular_velocity;
					}
					else
					{
						delta = wheel_state.angular_velocity - steam_engine.speed_target;
					}
				}


				//App.WriteLine(delta);
				if (delta <= 0.10f && Camera.IsVisible(Camera.CullType.Rect2x, transform.position))
				{
					Particle.Spawn(ref region, new Particle.Data()
					{
						texture = texture_smoke,
						lifetime = random.NextFloatRange(3.00f, 6.00f) * steam_engine.steam_size,
						pos = transform.LocalToWorld(steam_engine.steam_offset) + random.NextVector2(0.20f),
						vel = transform.LocalToWorld(steam_engine.smoke_direction * random.NextFloatRange(0.90f, 1.10f), position: false),
						force = new Vector2(random.NextFloatRange(-0.20f, 0.20f), -random.NextFloatRange(0.40f, 2.00f)),
						fps = random.NextByteRange(8, 10),
						frame_count = 64,
						frame_count_total = 64,
						frame_offset = random.NextByteRange(0, 64),
						scale = random.NextFloatRange(0.20f, 0.40f) * steam_engine.steam_size,
						rotation = random.NextFloat(10.00f),
						angular_velocity = random.NextFloat(0.50f),
						growth = 0.30f * steam_engine.steam_size,
						drag = random.NextFloatRange(0.001f, 0.01f),
						color_a = new Color32BGRA(150, 240, 240, 240),
						color_b = new Color32BGRA(0, 240, 240, 240)
					});
				}
			}

			var (sin, cos) = MathF.SinCos(-wheel_state.rotation * transform.scale.GetParity());
			renderer_piston.offset = wheel.offset + (new Vector2(cos, sin) * steam_engine.piston_radius);
			renderer_piston.rotation = -(renderer_piston.offset - steam_engine.piston_offset).GetNormalized().GetAngleRadians();
		}

		[ISystem.VeryLateUpdate(ISystem.Mode.Single, ISystem.Scope.Region)]
		public static void UpdateSound(ISystem.Info info, [Source.Owned] in Transform.Data transform,
		[Source.Owned] in SteamEngine.Data steam_engine, [Source.Owned] ref SteamEngine.State steam_engine_state,
		[Source.Owned, Pair.Of<SteamEngine.Data>] ref Sound.Emitter sound_emitter, [Source.Owned] in Axle.Data wheel, [Source.Owned] ref Axle.State wheel_state)
		{
			var speed = MathF.Abs(wheel_state.angular_velocity);

			sound_emitter.volume = Maths.Clamp(speed * 0.50f * steam_engine.volume_multiplier, 0.00f, 1.00f);
			sound_emitter.pitch = 0.60f + Maths.Clamp(speed * 0.10f * steam_engine.pitch_multiplier, 0.00f, 2.00f);
		}

		[ISystem.VeryLateUpdate(ISystem.Mode.Single, ISystem.Scope.Region)]
		public static void UpdateSoundExhaust(ISystem.Info info, ref Region.Data region, ref XorRandom random, 
		[Source.Owned] in Transform.Data transform,
		[Source.Owned] in SteamEngine.Data steam_engine, [Source.Owned] ref SteamEngine.State steam_engine_state,
		[Source.Owned] in Axle.Data wheel, [Source.Owned] ref Axle.State wheel_state,
		[Source.Owned, Pair.Of<SteamEngine.State>] ref Sound.Emitter sound_emitter)
		{
			var delta = 0.00f;

			if (wheel_state.angular_velocity != 0.00f)
			{
				if (float.IsNegative(wheel_state.angular_velocity))
				{
					delta = steam_engine.speed_target - wheel_state.angular_velocity;
				}
				else
				{
					delta = wheel_state.angular_velocity - steam_engine.speed_target;
				}
			}

			delta = MathF.Max(0.00f, delta);

			//delta = 0;
			//if (MathF.Abs(delta) < 0.10f) delta = 0.00f;

			//if (delta > -0.10f) delta = 0.00f;
			//App.WriteLine(delta);

			sound_emitter.volume = Maths.Lerp(sound_emitter.volume, Maths.Clamp(delta * 2.00f, 0.00f, 1.00f), 0.10f);
			sound_emitter.pitch = 1.00f; // 0.60f + Maths.Clamp(delta * 0.10f, 0.00f, 0.50f);

			if (delta > 0.10f && info.WorldTime >= steam_engine_state.next_exhaust && Camera.IsVisible(Camera.CullType.Rect2x, transform.position))
			{
				steam_engine_state.next_exhaust = info.WorldTime + 0.10f;

				Particle.Spawn(ref region, new Particle.Data()
				{
					texture = texture_smoke,
					lifetime = random.NextFloatRange(0.50f, 1.00f) * steam_engine.steam_size,
					pos = transform.LocalToWorld(steam_engine.exhaust_offset) + random.NextVector2(0.20f),
					vel = transform.LocalToWorld(new Vector2((-3.00f - Maths.Clamp(delta, 0.00f, 3.00f)) - 0.50f), position: false),
					//vel = steam_engine.smoke_direction * (-3.00f - Maths.Clamp(delta, 0.00f, 3.00f)) * random.NextFloatRange(0.90f, 1.10f),
					force = new Vector2(0.30f, -0.40f),
					fps = random.NextByteRange(10, 15),
					frame_count = 64,
					frame_count_total = 64,
					frame_offset = random.NextByteRange(0, 64),
					scale = random.NextFloatRange(0.20f, 0.40f) * steam_engine.steam_size,
					rotation = random.NextFloat(10.00f),
					angular_velocity = random.NextFloat(3.00f),
					growth = 0.30f * steam_engine.steam_size,
					drag = random.NextFloatRange(0.01f, 0.02f),
					color_a = new Color32BGRA(200, 240, 240, 240),
					color_b = new Color32BGRA(0, 240, 240, 240)
				});
			}
		}
#endif
	}
}
