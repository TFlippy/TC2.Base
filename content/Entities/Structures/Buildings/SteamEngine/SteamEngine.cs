
namespace TC2.Base.Components
{
	public static partial class SteamEngine
	{
		public static readonly Texture.Handle texture_smoke = "BiggerSmoke_Light";

		[IComponent.Data(Net.SendType.Reliable)]
		public partial struct Data: IComponent
		{
			[Editor.Picker.Position(relative: true, mark_modified: true)] public Vector2 piston_offset;
			[Editor.Picker.Position(relative: true, mark_modified: true)] public Vector2 steam_offset;
			[Editor.Picker.Position(relative: true, mark_modified: true)] public Vector2 exhaust_offset;

			public float steam_size = 1.00f;
			public float steam_interval;
			public float piston_radius;

			public float speed_max;
			public float speed_target;

			public float force;
			public float efficiency;
			public float shake_multiplier = 1.00f;

			public float volume_multiplier = 1.00f;
			public float pitch_multiplier = 1.00f;

			public float max_acceleration = 1.00f;

			public Data()
			{
			}
		}

		[IComponent.Data(Net.SendType.Unreliable)]
		public partial struct State: IComponent
		{
			public float speed_current; 		
			public float force_current;

			[Save.Ignore, Net.Ignore] public float next_steam;
			[Save.Ignore, Net.Ignore] public float next_exhaust;
			[Save.Ignore, Net.Ignore] public float next_tick;
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

		public const float update_interval = 0.20f;

		[ISystem.LateUpdate(ISystem.Mode.Single)]
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

		[ISystem.Update(ISystem.Mode.Single)]
		public static void Update(ISystem.Info info,
		[Source.Owned] ref SteamEngine.Data steam_engine, [Source.Owned] ref SteamEngine.State steam_engine_state,
		[Source.Owned] in Burner.Data burner, [Source.Owned] ref Burner.State burner_state, 
		[Source.Owned] ref Axle.Data wheel, [Source.Owned] ref Axle.State wheel_state)
		{
			var torque = steam_engine.force * steam_engine.piston_radius;
			var power = (float)(burner_state.available_power * steam_engine.efficiency);
			var speed = power / torque;
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
				using (var window = GUI.Window.Interaction("Steam Engine", this.ent_steam_engine))
				{
					this.StoreCurrentWindowTypeID();
					if (window.show)
					{
						using (GUI.Group.New(new(GUI.GetRemainingWidth(), 96)))
						{
							//Boiler.DrawGauge(this.steam_engine_state.speed_current, 0.00f, this.steam_engine.speed_max);
							GUI.DrawGauge(MathF.Abs(this.wheel_state.angular_velocity), 0.00f, this.steam_engine.speed_max);

							GUI.SameLine();

							GUI.DrawTemperatureRange(this.burner_state.current_temperature, this.burner_state.current_temperature, 2000, size: new Vector2(24, GUI.GetRemainingHeight()));
							//GUI.SameLine();
							//GUI.DrawWorkV(0.50f, size: new Vector2(24, GUI.GetRemainingHeight()));

							GUI.SameLine();


							using (GUI.Group.New(new(GUI.GetRemainingWidth(), 96)))
							{
								GUI.DrawInventoryDock(Inventory.Type.Fuel, new(48, 48));


								using (GUI.Group.New(padding: new(4, 4)))
								{
									//GUI.Text($"{MathF.Abs(this.wheel_state.angular_velocity):0.00}/{this.steam_engine.speed_max:0.00} rad/s");
									GUI.Text($"{MathF.Abs(this.wheel_state.angular_velocity):0.00} rad/s");
									GUI.Text($"{(this.wheel_state.old_tmp_torque * 0.010f):0.00} kNm/s");
									GUI.Text($"{(this.wheel_state.old_tmp_torque * MathF.Abs(this.wheel_state.angular_velocity) * 0.001f):0.00} kW");
									//GUI.Text($"{(this.wheel_state.old_tmp_torque * MathF.Abs(this.wheel_state.angular_velocity)):0.00} W");
									//GUI.Text($"{(this.burner_state.available_power):0.00} W");
								}
							}
						}

						//if (GUI.SliderFloat("Target Speed", ref this.steam_engine.speed_target, 0.00f, this.steam_engine.speed_max, size: new Vector2(160, 32)))
						if (GUI.SliderFloat("Target Speed", ref this.steam_engine.speed_target, 0.00f, this.steam_engine.speed_max, size: new Vector2(160, 32)))
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

		[ISystem.EarlyGUI(ISystem.Mode.Single)]
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

		[ISystem.VeryLateUpdate(ISystem.Mode.Single)]
		public static void UpdateRenderer(ISystem.Info info, [Source.Owned] in Transform.Data transform,
		[Source.Owned] in SteamEngine.Data steam_engine, [Source.Owned] ref SteamEngine.State steam_engine_state,
		[Source.Owned] in Axle.Data wheel, [Source.Owned] ref Axle.State wheel_state,
		[Source.Owned, Pair.Of<SteamEngine.Data>] ref Animated.Renderer.Data renderer_piston)
		{
			ref var region = ref info.GetRegion();
			var random = XorRandom.New();

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
				if (delta <= 0.10f)
				{
					Particle.Spawn(ref region, new Particle.Data()
					{
						texture = texture_smoke,
						lifetime = random.NextFloatRange(5.00f, 7.00f) * steam_engine.steam_size,
						pos = transform.LocalToWorldNoRotation(steam_engine.steam_offset) + random.NextVector2(0.20f),
						vel = new Vector2(0.00f, -1.50f),
						force = new Vector2(0.30f, -0.40f),
						fps = random.NextByteRange(8, 10),
						frame_count = 64,
						frame_count_total = 64,
						frame_offset = random.NextByteRange(0, 64),
						scale = random.NextFloatRange(0.20f, 0.40f) * steam_engine.steam_size,
						rotation = random.NextFloat(10.00f),
						angular_velocity = random.NextFloat(0.50f),
						growth = 0.30f * steam_engine.steam_size,
						//drag = random.NextFloatRange(0.01f, 0.02f),
						color_a = new Color32BGRA(200, 240, 240, 240),
						color_b = new Color32BGRA(0, 240, 240, 240)
					});
				}
			}

			var (sin, cos) = MathF.SinCos(-wheel_state.rotation * transform.scale.GetParity());
			renderer_piston.offset = wheel.offset + (new Vector2(cos, sin) * steam_engine.piston_radius);
			renderer_piston.rotation = -(renderer_piston.offset - steam_engine.piston_offset).GetNormalized().GetAngleRadians();
		}

		[ISystem.VeryLateUpdate(ISystem.Mode.Single)]
		public static void UpdateSound(ISystem.Info info, [Source.Owned] in Transform.Data transform,
		[Source.Owned] in SteamEngine.Data steam_engine, [Source.Owned] ref SteamEngine.State steam_engine_state,
		[Source.Owned, Pair.Of<SteamEngine.Data>] ref Sound.Emitter sound_emitter, [Source.Owned] in Axle.Data wheel, [Source.Owned] ref Axle.State wheel_state)
		{
			var speed = MathF.Abs(wheel_state.angular_velocity);

			sound_emitter.volume = Maths.Clamp(speed * 0.50f * steam_engine.volume_multiplier, 0.00f, 1.00f);
			sound_emitter.pitch = 0.60f + Maths.Clamp(speed * 0.10f * steam_engine.pitch_multiplier, 0.00f, 2.00f);
		}

		[ISystem.VeryLateUpdate(ISystem.Mode.Single)]
		public static void UpdateSoundExhaust(ISystem.Info info, [Source.Owned] in Transform.Data transform,
		[Source.Owned] in SteamEngine.Data steam_engine, [Source.Owned] ref SteamEngine.State steam_engine_state,
		[Source.Owned] in Axle.Data wheel, [Source.Owned] ref Axle.State wheel_state,
		[Source.Owned, Pair.Of<SteamEngine.State>] ref Sound.Emitter sound_emitter)
		{
			var random = XorRandom.New();
			ref var region = ref info.GetRegion();
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

			if (delta > 0.10f && info.WorldTime >= steam_engine_state.next_exhaust)
			{
				steam_engine_state.next_exhaust = info.WorldTime + 0.10f;

				Particle.Spawn(ref region, new Particle.Data()
				{
					texture = texture_smoke,
					lifetime = random.NextFloatRange(0.50f, 1.00f) * steam_engine.steam_size,
					pos = transform.LocalToWorldNoRotation(steam_engine.exhaust_offset) + random.NextVector2(0.20f),
					vel = new Vector2((-3.00f - Maths.Clamp(delta, 0.00f, 3.00f)) * transform.scale.GetParity(), -0.50f),
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
