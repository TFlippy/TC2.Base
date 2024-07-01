namespace TC2.Base.Components
{
	public static partial class Fermenter
	{
		[IComponent.Data(Net.SendType.Reliable, region_only: true), IComponent.With<Fermenter.State>]
		public partial struct Data: IComponent
		{
			[Flags]
			public enum Flags: uint
			{
				None = 0,
			}

			public Fermenter.Data.Flags flags;
		}

		[IComponent.Data(Net.SendType.Unreliable, region_only: true)]
		public partial struct State: IComponent
		{
			[Flags]
			public enum Flags: uint
			{
				None = 0,
			}

			public IRecipe.Handle h_recipe_cached;
			public Fermenter.State.Flags flags;
		}

#if CLIENT
		[ISystem.EarlyUpdate(ISystem.Mode.Single, ISystem.Scope.Region)]
		public static void OnUpdate(ISystem.Info info, ref Region.Data region, ref XorRandom random, Entity entity,
		[Source.Owned] in Transform.Data transform, 
		[Source.Owned] ref Fermenter.Data fermenter, [Source.Owned] ref Fermenter.State fermenter_state,
		[Source.Owned] ref Crafter.Data crafter, [Source.Owned] ref Crafter.State crafter_state)
		{

		}
#endif

#if SERVER
		[ISystem.Update.E(ISystem.Mode.Single, ISystem.Scope.Region)]
		public static void Update(ISystem.Info info, Entity entity,
		[Source.Owned] in Transform.Data transform,
		[Source.Owned] ref Fermenter.Data fermenter, [Source.Owned] ref Fermenter.State fermenter_state, 
		[Source.Owned] in Crafter.Data crafter, [Source.Owned] ref Crafter.State crafter_state)
		{
			if (crafter.h_recipe != 0)
			{
				if (fermenter_state.h_recipe_cached.TrySet(crafter.h_recipe))
				{
					ref var recipe = ref crafter.GetCurrentRecipe();
					if (recipe.IsNotNull())
					{
						//var load = 0.00f;

						//foreach (ref var requirement in recipe.requirements.AsSpan())
						//{
						//	if (requirement.type == Crafting.Requirement.Type.Work && requirement.work == crafter.h_work) // && requirement.work == Work.Type.Fermentering)
						//	{
						//		crafter_state.current_work_type
						//		//load += requirement.difficulty;
						//	}
						//}
					}
				}

				ref var work_data = ref crafter_state.current_work_type.GetData();
				if (work_data.IsNotNull())
				{


					//App.WriteLine("work");
				}
			}

			//switch (crafter_state.current_work_type)
			//{
			//	case Work.Type.Fermentering:
			//	{
			//		fermenter.load_multiplier = crafter_state.current_work_difficulty;

			//		fermenter_state.flags.SetFlag(Fermenter.State.Flags.Success, false);

			//		if (axle_state.flags.HasAny(Axle.State.Flags.Revolved))
			//		{
			//			if (MathF.Abs(axle_state.angular_velocity) >= 1.00f)
			//			{
			//				crafter_state.work += 1.00f;
			//				fermenter_state.flags.SetFlag(Fermenter.State.Flags.Success, true);
			//			}
			//			else
			//			{
			//				crafter_state.work = 0.00f;
			//			}

			//			fermenter_state.flags.SetFlag(Fermenter.State.Flags.Smashed, true);

			//			axle_state.Sync(entity, true);
			//			//crafter_state.Sync(entity);
			//			fermenter_state.Sync(entity, true);
			//		}
			//	}
			//	break;
			//}
		}
#endif

#if CLIENT
		public struct FermenterGUI: IGUICommand
		{
			public Entity ent_fermenter;
			public Transform.Data transform;

			public Fermenter.Data fermenter;
			public Fermenter.State fermenter_state;

			public Crafter.Data crafter;
			public Crafter.State crafter_state;

			public void Draw()
			{
				using (var window = GUI.Window.Interaction("Fermenter"u8, this.ent_fermenter))
				{
					this.StoreCurrentWindowTypeID(order: -100);
					if (window.show)
					{
						Crafting.Context.NewFromCurrentCharacter(ent_producer: this.ent_fermenter, out var context);

						var w_right = (48 * 4) + 24;

						using (GUI.Group.New(size: new Vector2(GUI.RmX - w_right, GUI.RmY)))
						{
							using (GUI.Group.New(size: GUI.Rm))
							{
								GUI.DrawFillBackground(GUI.tex_frame, new(8, 8, 8, 8));

								using (GUI.Group.New(size: GUI.Rm, padding: new(12, 12)))
								{
									//CrafterExt.DrawRecipe(ref region, ref this.crafter, ref this.crafter_state);
									CrafterExt.DrawRecipe(ref context, ref this.crafter, ref this.crafter_state);

									//ref var recipe = ref this.crafter.GetCurrentRecipe();
									//if (!recipe.IsNull())
									//{
									//	//ref var inventory_data = ref this.ent_fermenter.GetTrait<Crafter.State, Inventory8.Data>();

									//	//GUI.DrawShopRecipe(ref region, this.crafter.recipe, this.ent_fermenter, player.ent_controlled, this.transform.position, default, default, inventory_data.GetHandle(), draw_button: false, draw_title: false, draw_description: false, search_radius: 0.00f);
									//}

									using (GUI.Group.New(size: GUI.Rm, padding: new(12, 12)))
									{
										GUI.DrawFillBackground(GUI.tex_panel, new(8, 8, 8, 8), margin: new(-12, 0, -12, -12));


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
		public static void OnGUI(Entity ent_fermenter,
		[Source.Owned] in Transform.Data transform,
		[Source.Owned] in Fermenter.Data fermenter, [Source.Owned] in Fermenter.State fermenter_state,
		[Source.Owned] in Crafter.Data crafter, [Source.Owned] in Crafter.State crafter_state,
		[Source.Owned] in Interactable.Data interactable)
		{
			if (interactable.IsActive())
			{
				var gui = new FermenterGUI()
				{
					ent_fermenter = ent_fermenter,

					transform = transform,

					fermenter = fermenter,
					fermenter_state = fermenter_state,

					crafter = crafter,
					crafter_state = crafter_state,
				};
				gui.Submit();
			}
		}
#endif
	}
}
