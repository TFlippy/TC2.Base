namespace TC2.Base.Components
{
	public static partial class Compactor
	{
		[IComponent.Data(Net.SendType.Reliable)]
		public struct Data(): IComponent
		{
			[Flags]
			public enum Flags: uint
			{
				None = 0,

				Use_Misc = 1 << 0
			}

			public Compactor.Data.Flags flags;
			[Editor.Picker.Position(true, true)] public Vector2 offset;
			[Save.Ignore, Net.Ignore] public float t_next_update;
		}

		public const float update_interval = 0.20f;

		[ISystem.PostUpdate.C(ISystem.Mode.Single, ISystem.Scope.Region)]
		public static void Update(ISystem.Info info, ref Region.Data region, ref XorRandom random, Entity entity,
		[Source.Owned] ref Transform.Data transform, [Source.Owned] ref Body.Data body,
		[Source.Owned] ref Crafter.Data crafter, [Source.Owned] ref Crafter.State crafter_state, 
		[Source.Owned] ref Compactor.Data compactor)
		{

		}

#if CLIENT
		public struct CompactorGUI: IGUICommand
		{
			public Entity ent_compactor;

			public Transform.Data transform;
			public Crafter.Data crafter;
			public Crafter.State crafter_state;
			public Compactor.Data compactor;

			public void Draw()
			{
				using (var window = GUI.Window.InteractionMisc("Compactor"u8, this.ent_compactor, size: new(24, 96 * 1)))
				{
					this.StoreCurrentWindowTypeID(order: -100);
					if (window.show)
					{
						using (GUI.Group.New(size: new Vector2(GUI.RmX, GUI.RmY)))
						{

						}
					}
				}
			}
		}

		[ISystem.GUI(ISystem.Mode.Single, ISystem.Scope.Region)]
		public static void OnGUI(Entity entity,
		[Source.Owned] in Interactable.Data interactable, [Source.Owned] in Transform.Data transform,
		[Source.Owned] in Crafter.Data crafter, [Source.Owned] in Crafter.State crafter_state, 
		[Source.Owned] in Compactor.Data compactor)
		{
			if (interactable.IsActive())
			{
				var gui = new CompactorGUI()
				{
					ent_compactor = entity,

					transform = transform,
					crafter = crafter,
					crafter_state = crafter_state,
					compactor = compactor
				};
				gui.Submit();
			}
		}
#endif
	}
}
