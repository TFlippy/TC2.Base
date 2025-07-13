
namespace TC2.Base.Components
{
	public static partial class Gyrochronatom
	{
		[Flags]
		public enum Flags: ushort
		{
			None = 0,

			Active = 1 << 0
		}

		[IComponent.Data(Net.SendType.Reliable, IComponent.Scope.Region), IComponent.With<Gyrochronatom.State>]
		public struct Data(): IComponent
		{
			[Save.Force] public required Gyrochronatom.Flags flags;
			[Save.Force] public required Gyrochronatom.Flags flags_editable = Flags.Active;
			//private ushort unused_00;

			[Save.Force] public required ushort interval = 50;
			[Save.Force] public required ushort interval_min = 20;
			[Save.Force] public required ushort interval_max = 500;
			[Save.Force] public required ushort interval_step = 5;

			[Save.Force] public required Signal.Channels channels_pulse;

			public Sound.Handle h_sound_pulse;
			//public ushort duration;


			//public Signal.Channels signals_a;
			//public Signal.Channels signals_b;
			//public Signal.Channels signals_c;
			//public Signal.Channels signals_d;

			public required float signal_strength = 1.00f;
		}

		[IComponent.Data(Net.SendType.Unreliable, IComponent.Scope.Region)]
		public struct State(): IComponent
		{
			public int counter;

			public Signal.Channels channels_pulse;
			public Signal.Channels channels_unused_b;
			public Signal.Channels channels_unused_c;
			public Signal.Channels channels_unused_d;
		}

		[ISystem.PreUpdate.B(ISystem.Mode.Single, ISystem.Scope.Region, flags: ISystem.Flags.Unchecked, order: 40), HasTag("wrecked", false, Source.Modifier.Owned)]
		public static void Update(Entity entity,/* ref Region.Data region, ref XorRandom random,*/
		[Source.Owned] in Transform.Data transform, [Source.Owned] ref Analog.Relay.Data relay,
		[Source.Owned] ref Gyrochronatom.Data gyrochronatom, [Source.Owned] ref Gyrochronatom.State gyrochronatom_state)
		{
			//if (gyrochronatom.channels_pulse == Signal.Channels.None) return;
			if (gyrochronatom.flags.HasNone(Flags.Active)) return;
			if (gyrochronatom.interval == 0) return;

#if SERVER
			if (--gyrochronatom_state.counter < 0)
			{
				gyrochronatom_state.counter = gyrochronatom.interval;
				gyrochronatom_state.channels_pulse = gyrochronatom.channels_pulse;

				if (gyrochronatom.channels_pulse != Signal.Channels.None) gyrochronatom_state.Sync(entity, true);
				//relay.signal_input[Signal.Channel.Red] += 1.00f;
				//relay.Modified(entity, true);
			}
#endif

			var channels_pulse = gyrochronatom_state.channels_pulse;
			if (channels_pulse != Signal.Channels.None)
			{
				var signal_tmp = new Signal(gyrochronatom.signal_strength);
				var signal_tmp_masked = signal_tmp.Select(default, channels_pulse);
				relay.signal_input += signal_tmp_masked;

				gyrochronatom_state.channels_pulse = Signal.Channels.None;

#if CLIENT
				if (gyrochronatom.h_sound_pulse)
				{
					Sound.Play(gyrochronatom.h_sound_pulse, transform.position);
				}
#endif
			}
		}

		public struct ConfigureRPC: Net.IRPC<Gyrochronatom.Data>
		{
			public Gyrochronatom.Flags? flags;
			public ushort? interval;
#if SERVER
			public void Invoke(Net.IRPC.Context rpc, ref Gyrochronatom.Data data)
			{
				var sync = false;

				sync |= data.flags.TrySetFlagMasked(this.flags, data.flags_editable);
				sync |= data.interval.TrySet(this.interval, data.interval_min, data.interval_max);

				if (sync)
				{
					rpc.Sync(ref data);
				}
			}
#endif
		}

#if CLIENT
		public struct GyrochronatomGUI: IGUICommand
		{
			public Entity ent_gyrochronatom;
			public Transform.Data transform;
			public Gyrochronatom.Data gyrochronatom;

			public void Draw()
			{
				using (var window = GUI.Window.InteractionMisc("Gyrochronatom"u8, this.ent_gyrochronatom, size: new(48 * 2, 48)))
				{
					this.StoreCurrentWindowTypeID(order: -100);
					if (window.show)
					{
						using (var group = GUI.Group.New(size: GUI.Rm, padding: new(4)))
						{
							group.DrawBackground(GUI.tex_frame);

							var interval_min = this.gyrochronatom.interval_min;
							var interval_max = this.gyrochronatom.interval_max;
							var interval_cur = this.gyrochronatom.interval;
							var interval_step = this.gyrochronatom.interval_step;

							var interval_min_s = interval_min * App.fixed_update_interval_s_f32;
							var interval_max_s = interval_max * App.fixed_update_interval_s_f32;
							var interval_cur_s = interval_cur * App.fixed_update_interval_s_f32;
							var interval_step_s = interval_step * App.fixed_update_interval_s_f32;

							if (GUI.SliderFloat(label: "Interval"u8,
							value: ref interval_cur_s,
							min: interval_min_s,
							max: interval_max_s,
							size: new(GUI.RmX, 24),
							snap: interval_step_s))
							{
								var rpc = new Gyrochronatom.ConfigureRPC()
								{
									interval = (ushort)(interval_cur_s * App.tickrate_f32).RoundToUInt()
								};
								rpc.Send(this.ent_gyrochronatom);
							}

							//GUI.TextShaded("Derpo"u8);
						}
					}
				}
			}
		}

		[ISystem.GUI(ISystem.Mode.Single, ISystem.Scope.Region)]
		public static void OnGUI(Entity entity,
		[Source.Owned] in Interactable.Data interactable, [Source.Owned] in Transform.Data transform,
		[Source.Owned] in Gyrochronatom.Data gyrochronatom)
		{
			if (interactable.IsActive())
			{
				var gui = new GyrochronatomGUI()
				{
					ent_gyrochronatom = entity,
					transform = transform,
					gyrochronatom = gyrochronatom,
				};
				gui.Submit();
			}
		}
#endif
	}
}
