namespace TC2.Base.Components
{
	[WIP]
	public static partial class Driller
	{
		[IComponent.Data(Net.SendType.Reliable, IComponent.Scope.Region)]
		public struct Data(): IComponent
		{
			[Flags]
			public enum Flags: uint
			{
				None = 0,

				Active = 1 << 0,
				Stuck = 1 << 1,
			}

			public Driller.Data.Flags flags;
			public required float interval = 0.50f;

			[Editor.Picker.Position(relative: true)] public required Vec2f offset;
			[Editor.Picker.Direction(normalize: true)] public required Vec2f direction;

			public required float power;
			public required float yield;
			public required float thickness;
			public required float size;

			public required float speed;
			public required float distance_max;
			public float distance_current;




			[Save.Ignore, Net.Ignore] public float t_next_update;
		}

		[ISystem.Update.B(ISystem.Mode.Single, ISystem.Scope.Region)]
		public static void OnUpdate(ISystem.Info info, ref Region.Data region, ref XorRandom random, Entity entity,
		[Source.Owned] in Transform.Data transform, [Source.Owned] ref Body.Data body, [Source.Owned] ref Health.Data health,
		[Source.Owned] ref Driller.Data driller)
		{
			var time = info.WorldTime;
			if (time >= driller.t_next_update)
			{
	
			}
		}

		[ISystem.Update.C(ISystem.Mode.Single, ISystem.Scope.Region)]
		public static void OnUpdate_Heat(ISystem.Info info, ref Region.Data region, ref XorRandom random, Entity entity,
		[Source.Owned] in Transform.Data transform, [Source.Owned] ref Body.Data body, [Source.Owned] ref Health.Data health,
		[Source.Owned] ref Driller.Data driller, [Source.Owned] ref Heat.Data heat, [Source.Owned] ref Heat.State heat_state)
		{

		}

		[ISystem.PostUpdate.C(ISystem.Mode.Single, ISystem.Scope.Region)]
		public static void OnUpdate_FX(ISystem.Info info, ref Region.Data region, ref XorRandom random, Entity entity,
		[Source.Owned] in Transform.Data transform, [Source.Owned] ref Body.Data body,
		[Source.Owned] ref Driller.Data driller, [Source.Owned, Pair.Component<Driller.Data>] ref Sound.Emitter sound_emitter)
		{
		
		}

#if CLIENT
		public struct DrillerGUI: IGUICommand
		{
			public Entity ent_driller;

			public Transform.Data transform;

			public Driller.Data driller;

			public void Draw()
			{
				using (var window = GUI.Window.Interaction("Driller"u8, this.ent_driller))
				{
					this.StoreCurrentWindowTypeID(order: -100);
					if (window.show)
					{
						using (GUI.Group.New(size: new Vector2(GUI.RmX, GUI.RmY)))
						{
							//GUI.TextShaded("Derpo"u8);
						}
					}
				}
			}
		}

		[ISystem.GUI(ISystem.Mode.Single, ISystem.Scope.Region)]
		public static void OnGUI(Entity entity,
		[Source.Owned] in Interactable.Data interactable, [Source.Owned] in Transform.Data transform,
		[Source.Owned] in Driller.Data driller)
		{
			if (interactable.IsActive())
			{
				var gui = new DrillerGUI()
				{
					ent_driller = entity,
					transform = transform,
					driller = driller,
				};
				gui.Submit();
			}
		}
#endif

		//#if CLIENT
		//		[ISystem.VeryLateUpdate(ISystem.Mode.Single, ISystem.Scope.Region)]
		//		public static void UpdateEffects(ISystem.Info info, ref Region.Data region, ref XorRandom random,
		//		[Source.Owned] in Transform.Data transform,
		//		[Source.Owned] in Driller.Data driller, [Source.Owned] ref Driller.State driller_state,
		//		[Source.Owned, Pair.Component<Driller.State>, Optional(true)] ref Sound.Emitter sound_emitter,
		//		[Source.Owned, Pair.Component<Driller.State>, Optional(true)] ref Light.Data light)
		//		{

		//		}
		//#endif
	}
}
