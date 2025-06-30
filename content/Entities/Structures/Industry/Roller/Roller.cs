namespace TC2.Base.Components
{
	public static partial class Roller
	{
		[IComponent.Data(Net.SendType.Reliable, IComponent.Scope.Region)]
		public partial struct Data(): IComponent
		{
			[Editor.Picker.Position(relative: true)] public Vec2f offset;


		}

#if CLIENT
		private static readonly Texture.Handle bigger_smoke_light = "BiggerSmoke_Light";
		private static readonly Texture.Handle metal_spark_01 = "metal_spark.01";

		[ISystem.EarlyUpdate(ISystem.Mode.Single, ISystem.Scope.Region)]
		public static void OnUpdateEffects(ISystem.Info info, ref Region.Data region, ref XorRandom random, Entity entity,
		[Source.Owned] in Transform.Data transform, 
		[Source.Owned] ref Roller.Data roller, 
		[Source.Owned] ref Crafter.State state)
		{

		}
#endif

#if SERVER
		[ISystem.LateUpdate(ISystem.Mode.Single, ISystem.Scope.Region)]
		public static void Update(ISystem.Info info, ref Region.Data region, ref XorRandom random, Entity ent_roller,
		[Source.Owned] in Transform.Data transform, 
		[Source.Owned] ref Roller.Data roller, 
		[Source.Owned] in Crafter.Data crafter, [Source.Owned] ref Crafter.State crafter_state)
		{
			//if (crafter.recipe.id != 0)
			//{
			//	if (roller.recipe_cached.id != crafter.recipe.id)
			//	{
			//		ref var recipe = ref crafter.GetCurrentRecipe();
			//		if (!recipe.IsNull())
			//		{
			//			var load = 0.00f;

			//			foreach (ref var requirement in recipe.requirements.AsSpan())
			//			{
			//				if (requirement.type == Crafting.Requirement.Type.Work && requirement.work == Work.Type.Rollering)
			//				{
			//					load += requirement.difficulty;
			//				}
			//			}

			//			roller.load_multiplier = load;
			//		}

			//		roller.recipe_cached = crafter.recipe;
			//	}
			//}

			//switch (crafter_state.current_work_type)
			//{
			//	case Work.Type.Rollering:
			//	{
			//		roller.load_multiplier = crafter_state.current_work_difficulty;

			//		roller_state.flags.SetFlag(Roller.State.Flags.Success, false);

			//		if (axle_state.flags.HasAny(Axle.State.Flags.Revolved))
			//		{
			//			if (MathF.Abs(axle_state.angular_velocity) >= 1.00f)
			//			{
			//				crafter_state.work += 1.00f;
			//				roller_state.flags.SetFlag(Roller.State.Flags.Success, true);
			//			}
			//			else
			//			{
			//				crafter_state.work = 0.00f;
			//			}

			//			roller_state.flags.SetFlag(Roller.State.Flags.Smashed, true);

			//			axle_state.Sync(entity, true);
			//			//crafter_state.Sync(entity);
			//			roller_state.Sync(entity, true);
			//		}
			//	}
			//	break;
			//}
		}
#endif

#if CLIENT
		public struct RollerGUI: IGUICommand
		{
			public Entity ent_roller;
			public Transform.Data transform;

			public Roller.Data roller;
	
			public Crafter.Data crafter;
			public Crafter.State crafter_state;

			//public Axle.Data axle;
			//public Axle.State axle_state;

			public static uint load_graph_index;
			public static float[] load_graph = new float[App.tickrate * 3];

			public void Draw()
			{
				using (var window = GUI.Window.InteractionMisc(title: "Roller"u8, entity: this.ent_roller, size: new(48 * 3, 96), show_misc: true))
				{
					this.StoreCurrentWindowTypeID(order: 100);
					if (window.show)
					{
						GUI.DrawFillBackground(GUI.tex_frame, new(8, 8, 8, 8));

						//ref var player = ref Client.GetPlayer();
						//ref var region = ref Client.GetRegion();
						//ref var character = ref Client.GetCharacter(out var character_asset);

						//Crafting.Context.NewFromCharacter(ref region.AsCommon(), character_asset, ent_producer: this.ent_roller, out var context);

						//var w_right = (48 * 4) + 24;

						//using (GUI.Group.New(size: new Vector2(GUI.RmX - w_right, GUI.RmY)))
						//{
						//	using (GUI.Group.New(size: GUI.Rm))
						//	{
						//		GUI.DrawFillBackground(GUI.tex_frame, new(8, 8, 8, 8));

						//		using (GUI.Group.New(size: GUI.Rm, padding: new(12, 12)))
						//		{
						//			//CrafterExt.DrawRecipe(ref region, ref this.crafter, ref this.crafter_state);
						//			//CrafterExt.DrawRecipe(ref context, ref this.crafter, ref this.crafter_state);

						//			//ref var recipe = ref this.crafter.GetCurrentRecipe();
						//			//if (!recipe.IsNull())
						//			//{
						//			//	//ref var inventory_data = ref this.ent_roller.GetTrait<Crafter.State, Inventory8.Data>();

						//			//	//GUI.DrawShopRecipe(ref region, this.crafter.recipe, this.ent_roller, player.ent_controlled, this.transform.position, default, default, inventory_data.GetHandle(), draw_button: false, draw_title: false, draw_description: false, search_radius: 0.00f);
						//			//}

						//			//using (GUI.Group.New(size: GUI.Rm, padding: new(12, 12)))
						//			//{
						//			//	GUI.DrawFillBackground(GUI.tex_panel, new(8, 8, 8, 8), margin: new(-12, 0, -12, -12));

						//			//	load_graph[load_graph_index] = this.axle_state.force_load_old;
						//			//	GUI.LineGraph("##load", load_graph, ref load_graph_index, size: GUI.Rm, scale_min: 0.00f, scale_max: 10000.00f);
						//			//}
						//		}
						//	}
						//}

						//GUI.SameLine();

						//using (GUI.Group.New(size: new Vector2(w_right, GUI.RmY)))
						//{
						//	using (GUI.Group.New(size: new Vector2(48 * 4, 48 * 2) + new Vector2(24, 24), padding: new(12)))
						//	{
						//		GUI.DrawFillBackground(GUI.tex_frame, new(8, 8, 8, 8));

						//		ref var shipment = ref this.ent_roller.GetComponent<Shipment.Data>();
						//		if (shipment.IsNotNull())
						//		{
						//			var item_context = GUI.ItemContext.Begin(false);
						//			GUI.DrawShipment(ref item_context, this.ent_roller, ref shipment, new Vector2(96, 48));
						//		}
						//	}

						//	using (GUI.Group.New(size: new Vector2(48 * 4, 48 * 2) + new Vector2(24, 24)))
						//	{
						//		GUI.DrawFillBackground(GUI.tex_frame, new(8, 8, 8, 8));

						//		using (GUI.Group.New(size: GUI.Rm, padding: new(12, 12)))
						//		{
						//			GUI.DrawInventoryDock(Inventory.Type.Output, size: new(48 * 4, 48 * 2));
						//		}
						//	}

						//	using (GUI.Group.New(size: new Vector2(48 * 4, GUI.RmY) + new Vector2(24, 0)))
						//	{
						//		GUI.DrawFillBackground(GUI.tex_frame, new(8, 8, 8, 8));

						//		using (GUI.Group.New(size: GUI.Rm, padding: new(12, 12)))
						//		{
						//			GUI.DrawInventoryDock(Inventory.Type.Input, size: new(48 * 4, 48 * 2));
						//			//GUI.DrawWorkH(Maths.Normalize(this.crafter_state.work, this.crafter.required_work), size: GUI.Rm with { Y = 32 } - new Vector2(48, 0));
						//		}
						//	}
						//}

					}
				}
			}
		}

		[ISystem.EarlyGUI(ISystem.Mode.Single, ISystem.Scope.Region)]
		public static void OnGUI(Entity ent_roller, [Source.Owned] in Transform.Data transform, 
		[Source.Owned] in Roller.Data roller,
		[Source.Owned] in Crafter.Data crafter, [Source.Owned] in Crafter.State crafter_state,
		[Source.Owned] in Interactable.Data interactable)
		{
			if (interactable.IsActive())
			{
				var gui = new RollerGUI()
				{
					ent_roller = ent_roller,

					transform = transform,

					roller = roller,

					crafter = crafter,
					crafter_state = crafter_state,

					//axle = axle,
					//axle_state = axle_state
				};
				gui.Submit();
			}
		}

		[ISystem.Update.A(ISystem.Mode.Single, ISystem.Scope.Region)]
		public static void OnUpdate(ISystem.Info info,
		[Source.Owned] in Transform.Data transform,
		[Source.Owned] ref Roller.Data roller, // [Source.Owned] ref Axle.Data axle, [Source.Owned] ref Axle.State axle_state,
		[Source.Owned, Pair.Component<Roller.Data>] ref Animated.Renderer.Data renderer_roller)
		{
			//renderer_slider.offset = roller.slider_offset + new Vector2(0.00f, MathF.Pow((MathF.Cos(axle_state.rotation) + 1.00f) * 0.50f, roller.speed) * roller.slider_length);
		}
#endif
	}
}
