namespace TC2.Base.Components
{
	public static partial class SawMill
	{
		[IComponent.Data(Net.SendType.Reliable), IComponent.With<SawMill.State>]
		public struct Data: IComponent
		{
			public float slider_distance = 6.00f;
			public Vector2 saw_offset = default;
			public float saw_radius = 1.00f; 

			public Data()
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

		[IComponent.Data(Net.SendType.Unreliable)]
		public struct State: IComponent
		{
			public float gear_ratio = 1.00f;
			public float slider_ratio = default;

			[Net.Ignore, Save.Ignore] public float next_update = default;
			[Net.Ignore, Save.Ignore] public float next_hit = default;

			public State()
			{

			}
		}

		public const float update_interval = 0.20f;
		public const float hit_interval = 0.10f;

		[ISystem.VeryEarlyUpdate(ISystem.Mode.Single)]
		public static void UpdateSlider(ISystem.Info info, Entity entity,
		[Source.Owned] in SawMill.Data sawmill, [Source.Owned] ref SawMill.State sawmill_state,
		[Source.Owned] ref Joint.Distance joint_distance)
		{
			joint_distance.distance = sawmill.slider_distance * sawmill_state.slider_ratio;
		}

		[ISystem.Update(ISystem.Mode.Single)]
		public static void UpdateDamage(ISystem.Info info, Entity entity, Entity ent_health,
		[Source.Parent] in SawMill.Data sawmill, [Source.Parent] ref SawMill.State sawmill_state, [Source.Parent] in Transform.Data transform_parent,
		[Source.Owned] ref Health.Data health, [Source.Owned] in Body.Data body_child)
		{
			if (info.WorldTime >= sawmill_state.next_hit)
			{
				var wpos_saw = transform_parent.LocalToWorld(sawmill.saw_offset);

				var overlap = body_child.GetClosestPoint(wpos_saw);
				var dir = overlap.gradient;

				if (overlap.distance < sawmill.saw_radius)
				{
					sawmill_state.next_hit = info.WorldTime + hit_interval;
#if SERVER
					entity.Hit(entity, ent_health, overlap.world_position, dir, -dir, 100.00f, overlap.material_type, Damage.Type.Saw, yield: 1.00f);
#endif
				}
			}
		}

#if CLIENT
		[ISystem.LateUpdate(ISystem.Mode.Single)]
		public static void UpdateRenderer(ISystem.Info info, Entity entity,
		[Source.Owned] in SawMill.Data sawmill, [Source.Owned] in SawMill.State sawmill_state,
		[Source.Owned] in Wheel.Data wheel, [Source.Owned, Trait.Of<SawMill.Data>] ref Animated.Renderer.Data renderer_saw)
		{
			renderer_saw.rotation = (renderer_saw.rotation - (wheel.angular_velocity * info.DeltaTime * sawmill_state.gear_ratio)) % MathF.Tau;
			renderer_saw.offset = sawmill.saw_offset;
		}

		[ISystem.VeryLateUpdate(ISystem.Mode.Single)]
		public static void UpdateSoundIdle(ISystem.Info info, Entity entity,
		[Source.Owned] in SawMill.Data sawmill, [Source.Owned] ref SawMill.State sawmill_state,
		[Source.Owned] ref Wheel.Data wheel, [Source.Owned, Trait.Of<SawMill.Data>] ref Sound.Emitter sound_emitter)
		{
			var wheel_speed = MathF.Abs(wheel.angular_velocity);
			var random = XorRandom.New();

			sound_emitter.volume = Maths.Lerp2(sound_emitter.volume, Maths.Clamp(wheel_speed * 0.20f, 0.00f, 0.50f) * random.NextFloatRange(0.90f, 1.00f), 0.10f, 0.02f);
			sound_emitter.pitch = Maths.Lerp2(sound_emitter.pitch, 0.30f + (Maths.Clamp(wheel_speed * 0.20f, 0.00f, 0.50f)) * random.NextFloatRange(0.80f, 1.00f), 0.02f, 0.01f);
		}

		[ISystem.VeryLateUpdate(ISystem.Mode.Single)]
		public static void UpdateSoundCutting(ISystem.Info info, Entity entity,
		[Source.Owned] in SawMill.Data sawmill, [Source.Owned] ref SawMill.State sawmill_state,
		[Source.Owned] ref Wheel.Data wheel, [Source.Owned, Trait.Of<SawMill.State>] ref Sound.Emitter sound_emitter)
		{
			var modifier_target = info.WorldTime < sawmill_state.next_hit ? 1.00f : 0.00f;
			var random = XorRandom.New();

			sound_emitter.volume = Maths.Lerp2(sound_emitter.volume, modifier_target * 0.40f, 0.05f, 0.30f);
			sound_emitter.pitch = Maths.Lerp2(sound_emitter.pitch, 0.45f + (Maths.Clamp(modifier_target, 0.00f, 0.25f) * random.NextFloatRange(0.50f, 1.20f)), 0.02f, 0.08f);
		}
#endif

#if CLIENT
		public struct SawMillGUI: IGUICommand
		{
			public Entity ent_sawmill;
			public SawMill.Data sawmill;
			public SawMill.State sawmill_state;

			public void Draw()
			{
				using (var window = GUI.Window.Interaction("Sawmill", this.ent_sawmill))
				{
					this.StoreCurrentWindowTypeID(order: -100);

					ref var player = ref Client.GetPlayer();
					ref var region = ref Client.GetRegion();

					using (var group = GUI.Group.New(size: new Vector2(GUI.GetRemainingWidth() * 0.50f, GUI.GetRemainingHeight()), padding: new Vector2(4, 4)))
					{
						GUI.DrawBackground(GUI.tex_frame, group.GetOuterRect(), new(8));

						var dirty = false;
						if (GUI.SliderFloat("Slider", ref this.sawmill_state.slider_ratio, 1.00f, 0.00f, "%.2f", size: new Vector2(GUI.GetRemainingWidth(), 32)))
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
				}
			}
		}

		[ISystem.EarlyGUI(ISystem.Mode.Single)]
		public static void OnGUI(Entity entity, [Source.Owned] in SawMill.Data sawmill, [Source.Owned] in SawMill.State sawmill_state, [Source.Owned] in Interactable.Data interactable)
		{
			if (interactable.show)
			{
				var gui = new SawMillGUI()
				{
					ent_sawmill = entity,

					sawmill = sawmill,
					sawmill_state = sawmill_state,
				};
				gui.Submit();
			}
		}
#endif
	}
}
