namespace TC2.Base.Components
{
	public static partial class SawMill
	{
		[IComponent.Data(Net.SendType.Reliable), IComponent.With<SawMill.State>]
		public struct Data: IComponent
		{
			public float slider_distance = 6.00f;

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

			public State()
			{

			}
		}

		public const float update_interval = 0.20f;

		[ISystem.Update(ISystem.Mode.Single)]
		public static void Update(ISystem.Info info, Entity entity,
		[Source.Owned] in SawMill.Data sawmill, [Source.Owned] ref SawMill.State sawmill_state,
		[Source.Owned] ref Wheel.Data wheel)
		{
			if (info.WorldTime >= sawmill_state.next_update)
			{
				sawmill_state.next_update = info.WorldTime + update_interval;
			}
		}

		[ISystem.LateUpdate(ISystem.Mode.Single)]
		public static void UpdateSlider(ISystem.Info info, Entity entity,
		[Source.Owned] in SawMill.Data sawmill, [Source.Owned] ref SawMill.State sawmill_state,
		[Source.Owned] ref Joint.Slider slider)
		{
			slider.min = sawmill.slider_distance * sawmill_state.slider_ratio;
			slider.max = slider.min;
		}

#if CLIENT
		[ISystem.LateUpdate(ISystem.Mode.Single)]
		public static void UpdateRenderer(ISystem.Info info, Entity entity,
		[Source.Owned] in SawMill.Data sawmill, [Source.Owned] in SawMill.State sawmill_state,
		[Source.Owned] in Wheel.Data wheel, [Source.Owned, Trait.Of<SawMill.State>] ref Animated.Renderer.Data renderer_saw)
		{
			renderer_saw.rotation = (renderer_saw.rotation - (wheel.angular_velocity * info.DeltaTime * sawmill_state.gear_ratio)) % MathF.Tau;
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
