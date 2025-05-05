namespace TC2.Base.Components
{
	public static partial class PowerHammer
	{
		[IComponent.Data(Net.SendType.Reliable)]
		public partial struct Data(): IComponent
		{
			[Flags]
			public enum Flags: uint
			{
				None = 0,
			}

			[Save.Force] public PowerHammer.Data.Flags flags;

			[Save.NewLine]
			[Save.Force, Editor.Picker.Position(true)] public Vec2f slider_offset;
			[Save.Force] public float slider_length;

			[Save.NewLine]
			public float speed;
			public float load_multiplier;
		}

		[ISystem.Update.B(ISystem.Mode.Single, ISystem.Scope.Region)]
		public static void OnUpdate(ISystem.Info info, ref Region.Data region, ref XorRandom random, Entity entity,
		[Source.Owned] ref PowerHammer.Data power_hammer, [Source.Owned] in Transform.Data transform,
		[Source.Owned] ref Crafter.Data crafter, [Source.Owned] ref Crafter.State crafter_state)
		{

		}

		public static readonly Gradient<float> gradient_work = new(0.00f, 0.00f, 0.00f, 0.00f, 0.00f, 1.00f, 1.00f);

		[ISystem.Update.C(ISystem.Mode.Single, ISystem.Scope.Region)]
		public static void UpdateAxle(ISystem.Info info, ref Region.Data region, ref XorRandom random, Entity entity,
		[Source.Owned] ref PowerHammer.Data power_hammer, [Source.Owned] in Transform.Data transform,
		[Source.Owned] ref Axle.Data axle, [Source.Owned] ref Axle.State axle_state)
		{
			//var t = MathF.Pow((MathF.Cos(axle_state.rotation) + 1.00f) * 0.50f, 1.50f);
			//if (MathF.Abs(axle_state.rotation) <= 1.00f)
			//{
			//	t = 1;
			//}

			//axle_state.force_load_new += val * power_hammer.load_multiplier * 200.00f;
		}

#if CLIENT
		public struct PowerHammerGUI: IGUICommand
		{
			public Entity ent_power_hammer;
			public Transform.Data transform;

			public PowerHammer.Data power_hammer;

			public Crafter.Data crafter;
			public Crafter.State crafter_state;

			public Axle.Data axle;
			public Axle.State axle_state;

			public static uint load_graph_index;
			public static readonly float[] load_graph = new float[App.tickrate * 3];

			public void Draw()
			{
				using (var window = GUI.Window.Interaction("Power Hammer"u8, this.ent_power_hammer))
				{
					this.StoreCurrentWindowTypeID(order: -100);
					if (window.show)
					{
						ref var player = ref Client.GetPlayer();
						ref var region = ref Client.GetRegion();

						var w_right = (48 * 4) + 24;

						using (GUI.Group.New(size: new Vector2(GUI.RmX - w_right, GUI.RmY)))
						{
							using (GUI.Group.New(size: GUI.Rm))
							{
								GUI.DrawFillBackground(GUI.tex_frame, new(8, 8, 8, 8));

								using (GUI.Group.New(size: GUI.Rm, padding: new(12, 12)))
								{
									//CrafterExt.DrawRecipe(ref region, ref this.crafter, ref this.crafter_state);
									//CrafterExt.DrawRecipe(ref context, ref this.crafter, ref this.crafter_state);

									//ref var recipe = ref this.crafter.GetCurrentRecipe();
									//if (!recipe.IsNull())
									//{
									//	//ref var inventory_data = ref this.ent_power_hammer.GetTrait<Crafter.State, Inventory8.Data>();

									//	//GUI.DrawShopRecipe(ref region, this.crafter.recipe, this.ent_power_hammer, player.ent_controlled, this.transform.position, default, default, inventory_data.GetHandle(), draw_button: false, draw_title: false, draw_description: false, search_radius: 0.00f);
									//}

									using (GUI.Group.New(size: GUI.Rm, padding: new(12, 12)))
									{
										GUI.DrawFillBackground(GUI.tex_panel, new(8, 8, 8, 8), margin: new(-12, 0, -12, -12));

										//load_graph[load_graph_index] = this.axle_state.force_load_old;
										//GUI.LineGraph("##load", load_graph, ref load_graph_index, size: GUI.Rm, scale_min: 0.00f, scale_max: 10000.00f);
									}
								}
							}
						}

						GUI.SameLine();

						using (GUI.Group.New(size: new Vector2(w_right, GUI.RmY)))
						{

						}
					}
				}
			}
		}

		[ISystem.EarlyGUI(ISystem.Mode.Single, ISystem.Scope.Region)]
		public static void OnGUI(Entity entity,
		[Source.Owned] in PowerHammer.Data power_hammer, [Source.Owned] in Transform.Data transform,
		[Source.Owned] in Crafter.Data crafter, [Source.Owned] in Crafter.State crafter_state,
		[Source.Owned] in Axle.Data axle, [Source.Owned] in Axle.State axle_state,
		[Source.Owned] in Interactable.Data interactable)
		{
			if (interactable.IsActive())
			{
				var gui = new PowerHammerGUI()
				{
					ent_power_hammer = entity,

					transform = transform,

					power_hammer = power_hammer,

					crafter = crafter,
					crafter_state = crafter_state,

					axle = axle,
					axle_state = axle_state
				};
				gui.Submit();
			}
		}

		[ISystem.Update.A(ISystem.Mode.Single, ISystem.Scope.Region)]
		public static void OnUpdate(ISystem.Info info,
		[Source.Owned] ref PowerHammer.Data power_hammer, [Source.Owned] in Transform.Data transform,
		[Source.Owned] ref Axle.Data axle, [Source.Owned] ref Axle.State axle_state,
		[Source.Owned, Pair.Component<PowerHammer.Data>] ref Animated.Renderer.Data renderer_slider)
		{

		}
#endif
	}
}
