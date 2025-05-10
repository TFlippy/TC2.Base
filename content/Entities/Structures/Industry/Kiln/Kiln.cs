namespace TC2.Base.Components
{
	public static partial class Kiln
	{
		[IComponent.Data(Net.SendType.Reliable)]
		public partial struct Data(): IComponent
		{
			[Flags]
			public enum Flags: ushort
			{
				None = 0,
			}

			[Net.Segment.A, Save.Force] public Kiln.Data.Flags flags;
		}

		[ISystem.PreUpdate.Reset(ISystem.Mode.Single, ISystem.Scope.Region)]
		public static void OnUpdateReset(ISystem.Info info, ref Region.Data region, ref XorRandom random, Entity ent_kiln,
		[Source.Owned] ref Kiln.Data kiln, [Source.Owned] in Transform.Data transform, [Source.Owned] ref Body.Data body,
		[Source.Owned] ref Crafter.Data crafter, [Source.Owned] ref Crafter.State crafter_state)
		{

		}

		[ISystem.Update.D(ISystem.Mode.Single, ISystem.Scope.Region)]
		public static void OnUpdate(ISystem.Info info, ref Region.Data region, ref XorRandom random, Entity ent_kiln,
		[Source.Owned] ref Kiln.Data kiln, [Source.Owned] in Transform.Data transform, [Source.Owned] ref Body.Data body,
		[Source.Owned] ref Crafter.Data crafter, [Source.Owned] ref Crafter.State crafter_state)
		{

		}

#if CLIENT
		public struct KilnGUI: IGUICommand
		{
			public Entity ent_kiln;
			public Transform.Data transform;

			public Kiln.Data kiln;

			public Crafter.Data crafter;
			public Crafter.State crafter_state;

			public void Draw()
			{
				using (var window = GUI.Window.InteractionMisc("Kiln"u8, this.ent_kiln, size: new(96, 48)))
				{
					this.StoreCurrentWindowTypeID(order: -100);
					if (window.show)
					{
						using (var group = GUI.Group.New(GUI.Rm))
						{
							//GUI.DrawBackground(group, GUI.tex_window);

							ref var player = ref Client.GetPlayer();
							ref var region = ref Client.GetRegion();

							var context = GUI.ItemContext.Begin();
							{
								//GUI.DrawBackground(GUI.tex_frame, group.GetOuterRect(), new(8));

								using (var group_info = GUI.Group.New(GUI.Rm, padding: new(8)))
								{
									GUI.DrawBackground(group_info, GUI.tex_window);
								}
							}
						}
					}
				}
			}
		}

		[ISystem.EarlyGUI(ISystem.Mode.Single, ISystem.Scope.Region)]
		public static void OnGUI(Entity ent_kiln,
		[Source.Owned] in Kiln.Data kiln, [Source.Owned] in Transform.Data transform,
		[Source.Owned] in Crafter.Data crafter, [Source.Owned] in Crafter.State crafter_state,
		[Source.Owned] in Interactable.Data interactable)
		{
			if (interactable.IsActive())
			{
				var gui = new KilnGUI()
				{
					ent_kiln = ent_kiln,

					transform = transform,

					kiln = kiln,

					crafter = crafter,
					crafter_state = crafter_state
				};
				gui.Submit();
			}
		}
#endif
	}
}
