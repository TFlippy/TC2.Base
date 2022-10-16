namespace TC2.Base.Components
{
	public static partial class SawMill
	{
		[IComponent.Data(Net.SendType.Reliable), IComponent.With<SawMill.State>]
		public struct Data: IComponent
		{
			public float slider_distance = 5.00f;
			public Vector2 saw_offset = default;
			public float saw_radius = 1.00f;

			public Data()
			{

			}
		}

		[IComponent.Data(Net.SendType.Unreliable)]
		public struct State: IComponent
		{
			public float gear_ratio = 1.00f;
			public float slider_ratio = default;

			[Net.Ignore, Save.Ignore] public float next_update = default;
			[Net.Ignore, Save.Ignore] public float last_hit = default;

			public State()
			{

			}
		}

		public struct ConfigureRPC: Net.IRPC<SawMill.State>
		{
			public float gear_ratio;
			public float slider_ratio;

#if SERVER
			public void Invoke(ref NetConnection connection, Entity entity, ref SawMill.State data)
			{
				data.gear_ratio = this.gear_ratio;
				data.slider_ratio = this.slider_ratio;

				data.Sync(entity);
			}
#endif
		}

		public const float update_interval = 0.10f;

		[ISystem.VeryEarlyUpdate(ISystem.Mode.Single)]
		public static void UpdateSlider(ISystem.Info info, Entity entity,
		[Source.Owned] in SawMill.Data sawmill, [Source.Owned] ref SawMill.State sawmill_state,
		[Source.Owned, Original] ref Joint.Distance joint_distance)
		{
			joint_distance.distance = sawmill.slider_distance * sawmill_state.slider_ratio;
		}

		[ISystem.Update(ISystem.Mode.Single)]
		public static void UpdateDamage(ISystem.Info info, Entity entity, Entity ent_health,
		[Source.Parent] ref Axle.Data wheel, [Source.Parent] ref Axle.State wheel_state, [Source.Parent] in SawMill.Data sawmill, [Source.Parent] ref SawMill.State sawmill_state, [Source.Parent] in Transform.Data transform_parent,
		[Source.Owned] ref Health.Data health, [Source.Owned] in Body.Data body_child)
		{
			if (info.WorldTime >= sawmill_state.next_update)
			{
				var wheel_speed = MathF.Max(MathF.Abs(wheel_state.angular_velocity) - 2.00f, 0.00f);
				//var modifier = MathF.Min(wheel_speed * 0.08f, 1.00f);

				if (wheel_speed > 2.00f)
				{
					var wpos_saw = transform_parent.LocalToWorld(sawmill.saw_offset);

					var overlap = body_child.GetClosestPoint(wpos_saw);
					var dir = overlap.gradient;

					if (overlap.distance < sawmill.saw_radius)
					{
#if SERVER
						var damage = wheel_state.old_tmp_torque * 0.15f;
						entity.Hit(entity, ent_health, overlap.world_position, dir, -dir, damage, overlap.material_type, Damage.Type.Saw, yield: 1.00f, speed: wheel_speed);
#endif

						sawmill_state.last_hit = info.WorldTime;
					}
				}

				sawmill_state.next_update = info.WorldTime + update_interval;
			}
		}

#if CLIENT
		[ISystem.VeryLateUpdate(ISystem.Mode.Single)]
		public static void UpdateRenderer(ISystem.Info info, Entity entity,
		[Source.Owned] in SawMill.Data sawmill, [Source.Owned] in SawMill.State sawmill_state,
		[Source.Owned] in Axle.Data wheel, [Source.Owned] ref Axle.State wheel_state, [Source.Owned, Pair.Of<SawMill.Data>] ref Animated.Renderer.Data renderer_saw)
		{
			renderer_saw.rotation = (renderer_saw.rotation - (wheel_state.angular_velocity * info.DeltaTime * sawmill_state.gear_ratio)) % MathF.Tau;
			renderer_saw.offset = sawmill.saw_offset;
		}

		[ISystem.VeryLateUpdate(ISystem.Mode.Single)]
		public static void UpdateSoundIdle(ISystem.Info info, Entity entity,
		[Source.Owned] in SawMill.Data sawmill, [Source.Owned] ref SawMill.State sawmill_state,
		[Source.Owned] ref Axle.Data wheel, [Source.Owned] ref Axle.State wheel_state, [Source.Owned, Pair.Of<SawMill.Data>] ref Sound.Emitter sound_emitter)
		{
			var wheel_speed = MathF.Max(MathF.Abs(wheel_state.angular_velocity) - 2.00f, 0.00f);
			var random = XorRandom.New();

			sound_emitter.volume = Maths.Lerp2(sound_emitter.volume, Maths.Clamp(wheel_speed * 0.08f, 0.00f, 0.50f) * random.NextFloatRange(0.90f, 1.00f), 0.10f, 0.02f);
			sound_emitter.pitch = Maths.Lerp2(sound_emitter.pitch, 0.30f + (Maths.Clamp(wheel_speed * 0.11f, 0.00f, 0.50f)) * random.NextFloatRange(0.80f, 1.00f), 0.02f, 0.01f);
		}

		[ISystem.VeryLateUpdate(ISystem.Mode.Single)]
		public static void UpdateSoundCutting(ISystem.Info info, Entity entity,
		[Source.Owned] in SawMill.Data sawmill, [Source.Owned] ref SawMill.State sawmill_state,
		[Source.Owned] ref Axle.Data wheel, [Source.Owned] ref Axle.State wheel_state, [Source.Owned, Pair.Of<SawMill.State>] ref Sound.Emitter sound_emitter)
		{
			var wheel_speed = MathF.Max(MathF.Abs(wheel_state.angular_velocity) - 2.00f, 0.00f);
			var modifier = (info.WorldTime - sawmill_state.last_hit) < 0.25 ? MathF.Min(wheel_speed * 0.08f, 1.00f) : 0.00f;
			var random = XorRandom.New();

			sound_emitter.volume = Maths.Lerp2(sound_emitter.volume, modifier * 0.60f, 0.05f, 0.30f);
			sound_emitter.pitch = Maths.Lerp2(sound_emitter.pitch, 0.45f + (MathF.Min(modifier, 0.25f) * random.NextFloatRange(0.50f, 1.20f)), 0.02f, 0.08f);
		}
#endif

#if CLIENT
		public struct SawMillGUI: IGUICommand
		{
			public Entity ent_sawmill;

			public SawMill.Data sawmill;
			public SawMill.State sawmill_state;

			public Axle.Data wheel;
			public Axle.State wheel_state;

			public void Draw()
			{
				using (var window = GUI.Window.Interaction("Sawmill", this.ent_sawmill))
				{
					this.StoreCurrentWindowTypeID(order: -100);

					ref var player = ref Client.GetPlayer();
					ref var region = ref Client.GetRegion();

					const float slider_h = 32;

					var frame_size = Inventory.GetFrameSize(4, 2);

					using (GUI.Group.New(size: new Vector2(GUI.GetRemainingWidth(), GUI.GetRemainingHeight())))
					{
						using (var group = GUI.Group.New(size: new Vector2(GUI.GetRemainingWidth() - frame_size.X - 32, GUI.GetRemainingHeight()), padding: new Vector2(8, 8)))
						{
							GUI.DrawBackground(GUI.tex_frame, group.GetOuterRect(), new(8));

							using (GUI.Group.New(size: new Vector2(GUI.GetRemainingWidth(), GUI.GetRemainingHeight() - slider_h)))
							{
								GUI.Label("Angular Velocity:", this.wheel_state.angular_velocity, "{0:0.00} rad/s");
								GUI.Label("Torque:", this.wheel_state.old_tmp_torque, "{0:0.00} Nm/s");
							}

							var dirty = false;
							if (GUI.SliderFloat("Slider", ref this.sawmill_state.slider_ratio, 1.00f, 0.00f, "%.2f", size: new Vector2(GUI.GetRemainingWidth(), slider_h)))
							{
								dirty = true;
							}

							if (dirty)
							{
								var rpc = new SawMill.ConfigureRPC
								{
									gear_ratio = this.sawmill_state.gear_ratio,
									slider_ratio = this.sawmill_state.slider_ratio
								};
								rpc.Send(this.ent_sawmill);
							}
						}

						GUI.SameLine();

						using (var group = GUI.Group.Centered(GUI.GetRemainingSpace(), frame_size))
						{
							GUI.DrawBackground(GUI.tex_frame, group.GetOuterRect(), new(8));

							GUI.DrawInventoryDock(Inventory.Type.Output, size: frame_size);
						}
					}
				}
			}
		}

		[ISystem.EarlyGUI(ISystem.Mode.Single)]
		public static void OnGUI(Entity entity, [Source.Owned] in SawMill.Data sawmill, [Source.Owned] in SawMill.State sawmill_state, [Source.Owned] in Axle.Data wheel, [Source.Owned] ref Axle.State wheel_state, [Source.Owned] in Interactable.Data interactable)
		{
			if (interactable.show)
			{
				var gui = new SawMillGUI()
				{
					ent_sawmill = entity,

					sawmill = sawmill,
					sawmill_state = sawmill_state,
					
					wheel = wheel,
					wheel_state = wheel_state
				};
				gui.Submit();
			}
		}
#endif
	}
}
