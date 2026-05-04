
namespace TC2.Base.Components
{
	public static partial class Essence
	{
		public static partial class Emitter
		{
			public enum Type: byte
			{
				Undefined,

				//[Name("Contact-Plate", desc: "TODO: Desc")]
				//Contact_Plate,
				Ambient,
				Needle,
				Fork,
				Actuator,

				[Name("Projector", desc: "Experimental emitter array capable of projecting effects of essences onto distant surfaces.")]
				Projector,

				//[Name("Thumper", desc: "")]
				//Thumper,
			}

			public enum Kind: byte
			{
				Undefined,

				[Name("Contact-Plate", desc: "TODO: Desc")]
				Contact_Plate,
				[Name("Electrical", desc: "TODO: Desc")]
				Electrical,
				[Name("Fragmentation", desc: "TODO: Desc")]
				Fragmentation,

				//[Name("Projector", desc: "Experimental emitter array capable of projecting effects of essences onto distant surfaces.")]
				//Projector,

				//[Name("Thumper", desc: "")]
				//Thumper,
			}

			[Flags]
			public enum Flags: ushort
			{
				None = 0,

				Internal = 1 << 0,
				External = 1 << 1,

				Self_Discharging = 1 << 4,

				Enable_Signal_Read = 1 << 7,
				Enable_Pulse_Event = 1 << 8,

				//Show_GUI = 1 << 0,
				//Allow_Edit_Rate = 1 << 1,
				//Allow_Edit_Frequency = 1 << 2
			}

			[Flags]
			public enum StateFlags: ushort
			{
				None = 0,

				Charging = 1 << 0,

				[Asset.Ignore] Discharging = 1 << 4,
				[Asset.Ignore] Discharged = 1 << 5,

				[Asset.Ignore] Pulsing = 1 << 6,
				[Asset.Ignore] Pulsed = 1 << 7,
			}

			[IComponent.Data(Net.SendType.Unreliable, IComponent.Scope.Region)]
			public partial struct Data(): IComponent
			{
				[Net.Segment.A, Save.Force, Editor.Picker.Position(relative: true)] public required Vec2f offset;
				[Net.Segment.A, Save.Force, Editor.Picker.Direction(normalize: true)] public required Vec2f direction = Vec2f.Down;

				[Save.NewLine]
				[Net.Segment.A, Save.Force] public required Essence.Emitter.Type type;
				[Net.Segment.A] private byte unused_a_00;
				[Net.Segment.A, Save.Force] public Essence.Emitter.Flags flags;
				[Net.Segment.A, Save.Force, Editor.Slider.Clamped(0.00f, 10000.00f, snap: 1.00f)] public required float charge_capacity;
				[Net.Segment.A, Save.Force, Editor.Slider.Clamped(0.00f, 1.00f, snap: 0.001f)] public required float charge_loss = 0.03f;
				[Net.Segment.A, Save.Force, Editor.Slider.Clamped(0.00f, 1.00f, snap: 0.001f)] public required float efficiency = 1.00f;

				[Save.NewLine]
				[Net.Segment.B, Save.Force] public required IEssence.Handle h_essence;
				[Net.Segment.B, Save.Force] public required ISoundMix.Handle h_soundmix_pulse;
				[Net.Segment.B] public ISoundMix.Handle h_soundmix_discharge;
				[Net.Segment.B] public ISoundMix.Handle h_soundmix_unused_01;

				[Save.NewLine]
				[Net.Segment.C] public Essence.Emitter.StateFlags state_flags;
				[Net.Segment.C, Asset.Ignore] public IEssence.Handle h_essence_charge;
				[Net.Segment.C] public Signal.Channel channel_emit;
				[Net.Segment.C] private byte unused_c_02;
				[Net.Segment.C] private ushort unused_c_03;

				[Save.NewLine]
				[Net.Segment.D, Asset.Ignore] public float current_charge;
				[Net.Segment.D, Asset.Ignore] public float current_charge_ratio;
				[Net.Segment.D, Asset.Ignore] public float current_emit;
				[Net.Segment.D, Asset.Ignore] public float current_instability;




				//public float frequency;


				//public float rate_speed = 0.10f;
				//public float rate_max;
				//public float rate_target;
				//[Asset.Ignore] public float rate_current;
			}

			[ISystem.PreUpdate.D(ISystem.Mode.Single, ISystem.Scope.Region)]
			public static void OnUpdate_Signal(ISystem.Info info, ref XorRandom random, Entity entity, Entity ent_essence_emitter,
			[Source.Owned] in Transform.Data transform, [Source.Owned] ref Analog.Relay.Data analog_relay,
			[Source.Owned] ref Essence.Emitter.Data essence_emitter)
			{
				//return;
				if (essence_emitter.flags.HasNone(Flags.Enable_Signal_Read)) return;
#if SERVER

				var signal_value = analog_relay.signal_current[essence_emitter.channel_emit];
				//essence_emitter.flags.SetFlag(Essence.Emitter.Flags.Pulsed, signal_value > 0.10f);

				if (essence_emitter.state_flags.TryAddFlag(Essence.Emitter.StateFlags.Pulsing, signal_value > 0.10f))
				{
					//var rec = entity.GetRecord();



					//static void Func(ref PulseEvent ev)
					//{
					//	ev.amount *= Maths.Atan2Fast(ev.dir.y, ev.dir.x); // (ev.amount); // -2, 2, ev.amount); // MathF.IEEERemainder(ev.amount, ev.dir.x);
					//}

					//var ts = Timestamp.Now();
					////Func(ref ev);
					//var ev = new PulseEvent()
					//{
					//	h_essence = essence_emitter.h_essence,
					//	emitter_type = essence_emitter.type,
					//	amount = signal_value,
					//	pos = transform.position,
					//	dir = essence_emitter.direction
					//};
					//ev.Trigger(entity);
					////ev.Trigger(rec, IComponent.Handle.FromComponent<Piston.Data>());
					////ev.TriggerDeferred(entity);
					//var ts_elapsed = ts.GetMilliseconds();

					//App.WriteLine($"{ts_elapsed:0.0000} ms");
				}
#endif

			}

			[ISystem.PostUpdate.B(ISystem.Mode.Single, ISystem.Scope.Region)]
			public static void OnUpdate_Essence(ISystem.Info info, ref Region.Data region, ref XorRandom random, Entity ent_piston,
			[Source.Owned] in Transform.Data transform, /*[Source.Owned] ref Control.Data control,*/
			[Source.Owned] ref Piston.Data piston, [Source.Owned] ref Essence.Emitter.Data essence_emitter)
			{
				//if (control.mouse.GetKey(Mouse.Key.Left))

				if (essence_emitter.state_flags.HasAny(Essence.Emitter.StateFlags.Pulsed))
				{
					var power = (essence_emitter.current_emit * Essence.GetKineticPower(essence_emitter.h_essence_charge).m_value); //.Abs();
					var speed_add = Maths.SqrtSigned((power + power) / piston.mass);

					//App.WriteLine($"press pressed {power}; {speed_add}; {essence_emitter.current_emit}", color: App.Color.Magenta);


#if SERVER
					//var speed_add = Energy.GetVelocity(power, piston.mass);

					//piston.current_speed += Energy.GetVelocity(Essence.GetForce()
					piston.current_velocity += speed_add;
					//var energy_impact = Energy.GetKineticEnergy(piston.mass, piston.current_speed);
					//App.WriteValue(energy_impact);

					//App.WriteValue(speed_add);
					piston.Sync(ent_piston, true);

#endif

#if CLIENT
					//App.WriteValue(essence_emitter.current_charge_ratio);
					var h_essence = essence_emitter.h_essence_charge;
					if (h_essence)
					{
						var intensity = essence_emitter.current_charge_ratio;
						if (intensity > 0.10f)
						{
							//App.WriteValue(intensity);

							// new IEssence.Handle("motion");
							ref var essence_data = ref h_essence.GetData();
							if (essence_data.IsNotNull())
							{
								var pos = transform.LocalToWorld(essence_emitter.offset);
								var dir = transform.LocalToWorldDirection(essence_emitter.direction);

								var color_a = ColorBGRA.Lerp(essence_data.color_emit, ColorBGRA.White, 0.50f);
								var color_b = essence_data.color_emit.WithColorMult(0.20f).WithAlphaMult(0.00f);

								//App.WriteLine("essence");

								//Sound.Play(region: ref region, sound: essence_emitter.h_sound_emit, world_position: pos, volume: 1.00f, pitch: 1.00f, size: 0.35f, dist_multiplier: 0.65f);
								Sound.Play(region: ref region, h_soundmix: essence_emitter.h_soundmix_pulse, random: ref random, pos: pos,
									volume: Maths.Lerp01(0.50f, 1.50f, intensity),
									pitch: Maths.Lerp01(0.72f, 1.05f, intensity)); //, volume: 1.00f, pitch: 1.00f, size: 0.35f, dist_multiplier: 0.65f);

								//Sound.Play(region: ref region, world_position: pos, sound: essence_data.sound_impulse,
								//	volume: Maths.Lerp01(0.50f, 1.50f, intensity),
								//	pitch: Maths.Lerp01(1.18f, 0.95f, intensity)); //, volume: 1.00f, pitch: 1.00f, size: 0.35f, dist_multiplier: 0.65f);
								if (intensity > 0.30f) Shake.Emit(region: ref region, world_position: pos, trauma: 0.45f * intensity, max: 0.50f, radius: 10.00f);

								Particle.Spawn(ref region, new Particle.Data()
								{
									texture = Light.tex_light_circle_00,
									lifetime = 0.20f,
									pos = pos - dir,
									vel = dir * speed_add,
									drag = 0.20f,
									frame_count = 1,
									frame_count_total = 1,
									frame_offset = 0,
									scale = 1.00f,
									stretch = new Vector2(1.00f, 0.50f),
									face_dir_ratio = 1.00f,
									growth = 50.00f,
									color_a = color_a,
									color_b = color_b,
									glow = 20.00f * intensity
								});

								//Particle.Spawn(ref region, new Particle.Data()
								//{
								//	texture = Light.tex_light_circle_04,
								//	lifetime = random.NextFloatRange(1.00f, 1.25f),
								//	pos = data.world_position + (dir * 0.50f),
								//	scale = random.NextFloatRange(1.00f, 1.50f),
								//	growth = random.NextFloatRange(1.50f, 2.00f),
								//	stretch = new Vector2(0.90f, 0.60f),
								//	rotation = dir.GetAngleRadiansFast(),
								//	face_dir_ratio = 1.00f,
								//	color_a = new Vector4(1.00f, 0.70f, 0.40f, 30.00f),
								//	color_b = new Vector4(0.20f, 0.00f, 0.00f, 0.00f),
								//	glow = 1.00f
								//});

							}
						}
					}
#endif
				}
			}

			// TODO: move to piston
			[ISystem.PostUpdate.C(ISystem.Mode.Single, ISystem.Scope.Region)]
			public static void OnPostUpdate_Piston(ISystem.Info info, ref XorRandom random, Entity ent_essence_emitter,
			[Source.Owned] in Transform.Data transform, [Source.Owned] ref Essence.Container.Data essence_container,
			[Source.Owned] ref Body.Data body, [Source.Owned] ref Essence.Emitter.Data essence_emitter)
			{
				if (essence_emitter.state_flags.HasAny(StateFlags.Pulsing | StateFlags.Pulsed))
				{
					if (essence_emitter.state_flags.HasAny(StateFlags.Pulsing))
					{
						essence_emitter.state_flags.AddRemFlags(StateFlags.Pulsed, Essence.Emitter.StateFlags.Pulsing);


#if SERVER
						var amount_taken = essence_emitter.current_charge.MultSub(0.98f);
						essence_emitter.current_emit += amount_taken;
						essence_emitter.Sync(ent_essence_emitter);
#endif
					}
					else
					{
						//App.WriteValue(info.()[0]);

						essence_emitter.state_flags.RemoveFlag(StateFlags.Pulsed);
						essence_emitter.current_emit = 0.00f;
					}
				}
#if SERVER
				else if (essence_emitter.flags.HasAny(Essence.Emitter.Flags.Self_Discharging))
				{
					//if (essence_emitter.current_charge_ratio >= 1.00f)

					if (random.NextBool(essence_emitter.current_instability))
					{
						//var amount_taken = essence_emitter.current_charge.MultSub(0.98f);
						//if (random.NextBool(essence_emitter.current_charge_ratio * essence_container.rate_current * 0.20f))
						//if (random.NextBool(essence_emitter.current_charge_ratio - 1.00f))
						{

							//essence_emitter.current_emit += amount_taken;
							if (essence_emitter.state_flags.TryAddFlag(Essence.Emitter.StateFlags.Pulsing))
							{
								//App.WriteLine(h_essence_emitter);
								//essence_emitter.Sync(ent_essence_emitter, IComponent.Handle.FromComponentPair<Piston.Data, Essence.Emitter.Data>());
							}
						}
					}

					//else
					//{
					//	essence_emitter.current_emit = 0.00f;
					//	essence_emitter.state_flags.RemoveFlag(Essence.Emitter.StateFlags.Pulse);
					//}
				}
#endif
			}

			[ISystem.PostUpdate.D(ISystem.Mode.Single, ISystem.Scope.Region)]
			public static void OnPostUpdate_Charge([Source.Owned] ref Essence.Container.Data essence_container,
			[Source.Owned] ref Essence.Emitter.Data essence_emitter)
			{
				essence_emitter.h_essence_charge = essence_container.h_essence;
				essence_emitter.current_charge_ratio = Maths.NormalizeFast(essence_emitter.current_charge, essence_emitter.charge_capacity);
				var rate = essence_container.rate_current * essence_container.h_essence.GetDischargeMult();

				essence_emitter.current_instability = Maths.Max((essence_emitter.current_charge_ratio) -
					(essence_container.stability * essence_container.h_essence.GetStabilityMult()), 0.00f).Pow2();

				var current_charge = essence_emitter.current_charge;
				essence_emitter.current_charge -= essence_emitter.current_charge * essence_emitter.charge_loss;
				essence_emitter.current_charge += (rate * essence_container.available) * App.fixed_update_interval_s_f32;
			}
		}









	}
}
