namespace TC2.Base.Components
{
	public static partial class Cauldron
	{
		[IComponent.Data(Net.SendType.Reliable, IComponent.Scope.Region)]
		public struct Data(): IComponent
		{
			[Flags]
			public enum Flags: ushort
			{
				None = 0,

				Use_Misc = 1 << 15
			}

			[Net.Ignore, Save.Ignore] public float t_next_update;
			public Cauldron.Data.Flags flags;
			private ushort unused_00;

			[Editor.Picker.Position(true, true)] public Vec2f offset;
		}

		[ISystem.Update.A(ISystem.Mode.Single, ISystem.Scope.Region)]
		public static void OnUpdate(ISystem.Info info, ref XorRandom random, ref Region.Data region, Entity ent_cauldron,
		[Source.Owned] in Transform.Data transform, [Source.Owned] ref Cauldron.Data cauldron)
		{

		}

		[ISystem.PreUpdate.C(ISystem.Mode.Single, ISystem.Scope.Region)]
		public static void OnPreUpdate(ISystem.Info info, ref XorRandom random, ref Region.Data region, Entity ent_cauldron,
		[Source.Owned] in Transform.Data transform,
		[Source.Owned] ref Cauldron.Data cauldron,
		[Source.Owned] in Crafter.Data crafter, [Source.Owned] ref Crafter.State crafter_state)
		{

		}

#if CLIENT
		public struct CauldronGUI: IGUICommand
		{
			public Entity ent_cauldron;

			public Transform.Data transform;
			public Crafter.Data crafter;
			public Crafter.State crafter_state;
			public Cauldron.Data cauldron;

			public void Draw()
			{
				using (var window = GUI.Window.InteractionMisc("Cauldron"u8, this.ent_cauldron, size: new(48 * 3, 48 * 2)))
				{
					this.StoreCurrentWindowTypeID(order: -90);
					if (window.show)
					{
						using (var group = GUI.Group.New(size: GUI.Rm))
						{
							group.DrawBackground(GUI.tex_frame);
						}
					}
				}
			}
		}

		[ISystem.LateGUI(ISystem.Mode.Single, ISystem.Scope.Region, order: 50)]
		public static void OnGUI(Entity entity,
		[Source.Owned] in Interactable.Data interactable, [Source.Owned] in Transform.Data transform,
		[Source.Owned] in Crafter.Data crafter, [Source.Owned] in Crafter.State crafter_state,
		[Source.Owned] in Cauldron.Data cauldron)
		{
			if (interactable.IsActive())
			{
				var gui = new CauldronGUI()
				{
					ent_cauldron = entity,

					transform = transform,
					crafter = crafter,
					crafter_state = crafter_state,
					cauldron = cauldron
				};
				gui.Submit();
			}
		}
#endif
	}
}
