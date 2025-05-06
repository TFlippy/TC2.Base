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
				
				[Asset.Ignore] Impacted = 1 << 16
			}

			[Net.Segment.A, Save.Force] public PowerHammer.Data.Flags flags;
			[Net.Segment.A] public Sound.Handle h_sound_impact;

			[Save.NewLine]
			[Net.Segment.A, Save.Force, Editor.Picker.Position(true)] public required Vec2f slider_offset;
			[Net.Segment.A, Save.Force] public required float slider_length;

			[Save.NewLine]
			[Net.Segment.B, Save.Force] public float speed = 1.00f;
			[Net.Segment.B, Save.Force] public float gear_ratio = 1.00f;
			[Net.Segment.B] public float load_multiplier = 1.00f;

			[Net.Segment.C, Asset.Ignore, Save.Ignore] public Entity ent_joint;
			[Net.Segment.C, Asset.Ignore, Save.Ignore] public Entity ent_joint_attached;

			[Net.Segment.D, Asset.Ignore] public float current_load;
		}

		[ISystem.PreUpdate.Reset(ISystem.Mode.Single, ISystem.Scope.Region)]
		public static void OnUpdateReset(ISystem.Info info, ref Region.Data region, ref XorRandom random, Entity ent_hammer,
		[Source.Owned] ref PowerHammer.Data hammer, [Source.Owned] in Transform.Data transform, [Source.Owned] ref Body.Data body,
		[Source.Owned] ref Crafter.Data crafter, [Source.Owned] ref Crafter.State crafter_state)
		{
			
		}

		[ISystem.Update.A(ISystem.Mode.Single, ISystem.Scope.Region)]
		public static void OnUpdateJoint(Entity ent_hammer, Entity ent_joint_base,
		[Source.Shared] ref PowerHammer.Data hammer, [Source.Owned] ref Joint.Base joint_base) 
		{
			hammer.ent_joint = ent_joint_base;
			hammer.ent_joint_attached = joint_base.ent_attached;


		}

		[ISystem.Update.C(ISystem.Mode.Single, ISystem.Scope.Region)]
		public static void UpdateAxle(ISystem.Info info, ref Region.Data region, ref XorRandom random, Entity ent_hammer,
		[Source.Owned] ref PowerHammer.Data hammer, [Source.Owned] in Transform.Data transform,
		[Source.Owned] ref Crafter.Data crafter, [Source.Owned] ref Crafter.State crafter_state,
		[Source.Owned] ref Axle.Data axle, [Source.Owned] ref Axle.State axle_state)
		{
			//var t = MathF.Pow((MathF.Cos(axle_state.rotation) + 1.00f) * 0.50f, 1.50f);
			//if (MathF.Abs(axle_state.rotation) <= 1.00f)
			//{
			//	t = 1;
			//}

			//axle_state.force_load_new += val * power_hammer.load_multiplier * 200.00f;
		}

		[ISystem.Update.D(ISystem.Mode.Single, ISystem.Scope.Region)]
		public static void OnUpdate(ISystem.Info info, ref Region.Data region, ref XorRandom random, Entity ent_hammer,
		[Source.Owned] ref PowerHammer.Data hammer, [Source.Owned] in Transform.Data transform, [Source.Owned] ref Body.Data body,
		[Source.Owned] ref Crafter.Data crafter, [Source.Owned] ref Crafter.State crafter_state)
		{

		}

		[ISystem.PostUpdate.D(ISystem.Mode.Single, ISystem.Scope.Region)]
		public static void OnUpdateEffects(ISystem.Info info,
		[Source.Owned] ref PowerHammer.Data hammer, [Source.Owned] in Transform.Data transform,
		[Source.Owned] in Axle.Data axle, [Source.Owned] in Axle.State axle_state,
		[Source.Owned, Pair.Component<PowerHammer.Data>, Optional(true)] ref Animated.Renderer.Data renderer_slider,
		[Source.Owned, Pair.Component<PowerHammer.Data>, Optional(true)] ref Sound.Emitter sound_emitter)
		{
			if (renderer_slider.IsNotNull())
			{
				renderer_slider.offset = hammer.slider_offset - new Vector2(0.00f, renderer_slider.sprite.size.y.Div16x() + (MathF.Pow(Maths.HvCos(axle_state.rotation), hammer.speed) * hammer.slider_length));
			}

			if (sound_emitter.IsNotNull())
			{

			}
		}

		//[ISystem.Update.(ISystem.Mode.Single, ISystem.Scope.Region)]
		//public static void OnUpdate(ISystem.Info info, ref Region.Data region, ref XorRandom random, Entity ent_hammer,
		//[Source.Owned] ref PowerHammer.Data hammer, [Source.Owned] in Transform.Data transform,
		//[Source.Owned] ref Crafter.Data crafter, [Source.Owned] ref Crafter.State crafter_state)
		//{

		//}

#if CLIENT
		public struct PowerHammerGUI: IGUICommand
		{
			public Entity ent_hammer;
			public Transform.Data transform;

			public PowerHammer.Data hammer;

			public Crafter.Data crafter;
			public Crafter.State crafter_state;

			public Axle.Data axle;
			public Axle.State axle_state;

			public static uint load_graph_index;
			public static readonly float[] load_graph = new float[App.tickrate * 3];

			public void Draw()
			{
				using (var window = GUI.Window.InteractionMisc("Power Hammer"u8, this.ent_hammer, size: new(96, 168)))
				{
					this.StoreCurrentWindowTypeID(order: -100);
					if (window.show)
					{
						using (var group = GUI.Group.New(GUI.Rm))
						{
							GUI.DrawBackground(group, GUI.tex_window);

							ref var player = ref Client.GetPlayer();
							ref var region = ref Client.GetRegion();

							var context = GUI.ItemContext.Begin();
							{
								//GUI.DrawBackground(GUI.tex_frame, group.GetOuterRect(), new(8));

								using (var slot = GUI.EntitySlot.New(ref context, "slot.power_hammer"u8, "Item"u8, this.hammer.ent_joint_attached, new Vector2(GUI.RmX, 64)))
								{
									if (slot.pressed)
									{
										switch (slot.action)
										{
											case GUI.ItemContext.Action.Add:
											{
												var rpc = new Holdable.AttachRPC
												{
													ent_joint = this.hammer.ent_joint
												};
												rpc.Send(context.ent_pickup_target);
											}
											break;

											case GUI.ItemContext.Action.Remove:
											{
												var rpc = new Holdable.DetachRPC
												{

												};
												rpc.Send(this.hammer.ent_joint_attached);
											}
											break;

											case GUI.ItemContext.Action.Swap:
											{
												var rpc = new Holdable.AttachRPC
												{
													ent_joint = this.hammer.ent_joint
												};
												rpc.Send(context.ent_pickup_target);
											}
											break;
										}
									}
								}

								//var dirty = false;
								//if (GUI.SliderFloat("Slider"u8, ref this.sawmill_state.slider_ratio, 1.00f, 0.00f, size: new Vector2(GUI.RmX, slider_h)))
								//{
								//	dirty = true;
								//}

								//if (dirty)
								//{
								//	var rpc = new SawMill.ConfigureRPC
								//	{
								//		gear_ratio = this.sawmill_state.gear_ratio,
								//		slider_ratio = this.sawmill_state.slider_ratio
								//	};
								//	rpc.Send(this.ent_sawmill);
								//}
							}




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
							//			//	//ref var inventory_data = ref this.ent_power_hammer.GetTrait<Crafter.State, Inventory8.Data>();

							//			//	//GUI.DrawShopRecipe(ref region, this.crafter.recipe, this.ent_power_hammer, player.ent_controlled, this.transform.position, default, default, inventory_data.GetHandle(), draw_button: false, draw_title: false, draw_description: false, search_radius: 0.00f);
							//			//}

							//			using (GUI.Group.New(size: GUI.Rm, padding: new(12, 12)))
							//			{
							//				GUI.DrawFillBackground(GUI.tex_panel, new(8, 8, 8, 8), margin: new(-12, 0, -12, -12));

							//				//load_graph[load_graph_index] = this.axle_state.force_load_old;
							//				//GUI.LineGraph("##load", load_graph, ref load_graph_index, size: GUI.Rm, scale_min: 0.00f, scale_max: 10000.00f);
							//			}
							//		}
							//	}
							//}

							//GUI.SameLine();

							//using (GUI.Group.New(size: new Vector2(w_right, GUI.RmY)))
							//{

							//}
						}
					}
				}
			}
		}

		[ISystem.EarlyGUI(ISystem.Mode.Single, ISystem.Scope.Region)]
		public static void OnGUI(Entity ent_hammer,
		[Source.Owned] in PowerHammer.Data hammer, [Source.Owned] in Transform.Data transform,
		[Source.Owned] in Crafter.Data crafter, [Source.Owned] in Crafter.State crafter_state,
		[Source.Owned] in Axle.Data axle, [Source.Owned] in Axle.State axle_state,
		[Source.Owned] in Interactable.Data interactable)
		{
			if (interactable.IsActive())
			{
				var gui = new PowerHammerGUI()
				{
					ent_hammer = ent_hammer,

					transform = transform,

					hammer = hammer,

					crafter = crafter,
					crafter_state = crafter_state,

					axle = axle,
					axle_state = axle_state
				};
				gui.Submit();
			}
		}
#endif
	}
}
