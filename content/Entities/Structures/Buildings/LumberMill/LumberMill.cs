namespace TC2.Base.Components
{
	public static partial class LumberMill
	{
		[IComponent.Data(Net.SendType.Reliable), IComponent.With<LumberMill.State>]
		public struct Data: IComponent
		{
			public float test = 1.00f;

			public Data()
			{

			}
		}

		public struct ConfigureRPC: Net.IRPC<LumberMill.State>
		{
			public float gear_ratio;

#if SERVER
			public void Invoke(ref NetConnection connection, Entity entity, ref LumberMill.State data)
			{
				data.gear_ratio_current = this.gear_ratio;

				data.Sync(entity);
			}
#endif
		}

		[IComponent.Data(Net.SendType.Unreliable)]
		public struct State: IComponent
		{
			public float gear_ratio_current;

			[Net.Ignore, Save.Ignore] public float next_update;
		}

		public const float update_interval = 0.20f;

		[ISystem.Update(ISystem.Mode.Single)]
		public static void Update(ISystem.Info info, Entity entity,
		[Source.Owned] in LumberMill.Data lumbermill, [Source.Owned] ref LumberMill.State lumbermill_state,
		[Source.Owned] ref Wheel.Data wheel)
		{
			if (info.WorldTime >= lumbermill_state.next_update)
			{
				lumbermill_state.next_update = info.WorldTime + update_interval;
			}
		}

#if CLIENT
		[ISystem.LateUpdate(ISystem.Mode.Single)]
		public static void UpdateRenderer(ISystem.Info info, Entity entity,
		[Source.Owned] in LumberMill.Data lumbermill, [Source.Owned] in LumberMill.State lumbermill_state,
		[Source.Owned] in Wheel.Data wheel, [Source.Owned, Trait.Of<LumberMill.State>] ref Animated.Renderer.Data renderer_saw)
		{
			renderer_saw.rotation = (renderer_saw.rotation - (wheel.angular_velocity * info.DeltaTime * lumbermill_state.gear_ratio_current)) % MathF.Tau;
		}
#endif

#if CLIENT
		public struct LumberMillGUI: IGUICommand
		{
			public Entity ent_lumbermill;
			public LumberMill.Data lumbermill;
			public LumberMill.State lumbermill_state;

			public void Draw()
			{
				using (var window = GUI.Window.Interaction("Lumber Mill", this.ent_lumbermill))
				{
					this.StoreCurrentWindowTypeID(order: -100);

					ref var player = ref Client.GetPlayer();
					ref var region = ref Client.GetRegion();

					var w_right = (48 * 4) + 24;

					using (GUI.Group.New(size: new Vector2(GUI.GetRemainingWidth() - w_right, GUI.GetRemainingHeight())))
					{
						using (GUI.Group.New(size: new Vector2(GUI.GetRemainingWidth(), GUI.GetRemainingHeight() - 96 - 12)))
						{
							GUI.DrawFillBackground("ui_frame", new(8, 8, 8, 8));

							
						}
					}
				}
			}
		}

		[ISystem.EarlyGUI(ISystem.Mode.Single)]
		public static void OnGUI(Entity entity, [Source.Owned] in LumberMill.Data lumbermill, [Source.Owned] in LumberMill.State lumbermill_state, [Source.Owned] in Interactable.Data interactable)
		{
			if (interactable.show)
			{
				var gui = new LumberMillGUI()
				{
					ent_lumbermill = entity,

					lumbermill = lumbermill,
					lumbermill_state = lumbermill_state,
				};
				gui.Submit();
			}
		}
#endif
	}
}
