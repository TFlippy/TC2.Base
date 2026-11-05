namespace TC2.Base.Components
{
	public static partial class Pulverizer
	{
		[IComponent.Data(Net.SendType.Reliable, IComponent.Scope.Region)]
		public struct Data: IComponent
		{
			[Flags]
			public enum Flags: ushort
			{
				None = 0,

				Use_Misc = 1 << 15
			}

			[Net.Ignore, Save.Ignore] public float t_next_update;
			public Pulverizer.Data.Flags flags;
			private ushort unused_00;

			[Editor.Picker.Position(true, true)] public Vec2f offset;
			[Editor.Picker.Box(true)] public AABB rect;

		}

		[ISystem.PreUpdate.C(ISystem.Mode.Single, ISystem.Scope.Region)]
		public static void OnPreUpdate_C(ISystem.Info info, ref XorRandom random, ref Region.Data region, Entity ent_pulverizer,
		[Source.Owned] in Transform.Data transform,
		[Source.Owned] ref Pulverizer.Data pulverizer,
		[Source.Owned] ref Body.Data body,
		[Source.Owned] in Crafter.Data crafter, [Source.Owned] ref Crafter.State crafter_state)
		{

		}

		[ISystem.Update.C(ISystem.Mode.Single, ISystem.Scope.Region)]
		public static void OnUpdate_C(ISystem.Info info, ref XorRandom random, ref Region.Data region, Entity ent_pulverizer,
		[Source.Owned] in Transform.Data transform,
		[Source.Owned] ref Pulverizer.Data pulverizer,
		[Source.Owned] ref Body.Data body,
		[Source.Owned] in Crafter.Data crafter, [Source.Owned] ref Crafter.State crafter_state,
		[Source.Owned, Pair.Component<Pulverizer.Data>] ref Inventory1.Data inventory_input,
		[Source.Owned] ref Storage.Data storage)
		{
#if SERVER
			//var ts = Timestamp.Now();
			if (storage.inv_storage.TryGetHandle(out var h_inventory_output))
			{

			}
			//var ts_elapsed = ts.GetMilliseconds();
			//region.DrawDebugText(transform.position.AddY(App.IsClient ? 1 : 0), $"{ts_elapsed:0.00000} ms", Color32BGRA.White);
#endif
		}

		[ISystem.Update.D(ISystem.Mode.Single, ISystem.Scope.Region)]
		public static void OnUpdate_D(ISystem.Info info, ref XorRandom random, ref Region.Data region, Entity ent_pulverizer,
		[Source.Owned] in Transform.Data transform, [Source.Owned] ref Pulverizer.Data pulverizer)
		{
			//region.DrawDebugText(transform.position, "D", Color32BGRA.White);
		}


#if CLIENT
		public struct PulverizerGUI: IGUICommand
		{
			public Entity ent_pulverizer;

			public Transform.Data transform;
			public Crafter.Data crafter;
			public Crafter.State crafter_state;
			public Pulverizer.Data pulverizer;

			public void Draw()
			{
				using (var window = GUI.Window.InteractionMisc("Pulverizer"u8, this.ent_pulverizer, size: new(48 * 3, 96 * 1)))
				{
					this.StoreCurrentWindowTypeID(order: -100);
					if (window.show)
					{
						using (GUI.Group.New(size: GUI.Rm))
						{

						}
					}
				}
			}
		}

		[ISystem.LateGUI(ISystem.Mode.Single, ISystem.Scope.Region)]
		public static void OnGUI(Entity ent_pulverizer,
		[Source.Owned] in Interactable.Data interactable, [Source.Owned] in Transform.Data transform,
		[Source.Owned] in Crafter.Data crafter, [Source.Owned] in Crafter.State crafter_state,
		[Source.Owned] in Pulverizer.Data pulverizer)
		{
			if (interactable.IsActive())
			{
				var gui = new PulverizerGUI()
				{
					ent_pulverizer = ent_pulverizer,

					transform = transform,
					crafter = crafter,
					crafter_state = crafter_state,
					pulverizer = pulverizer
				};
				gui.Submit();
			}
		}
#endif
	}
}
