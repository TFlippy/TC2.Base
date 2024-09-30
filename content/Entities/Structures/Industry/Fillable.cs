namespace TC2.Base.Components
{
	public static partial class Fillable
	{
		[IComponent.Data(Net.SendType.Reliable)]
		public struct Data: IComponent
		{
			[Flags]
			public enum Flags: uint
			{
				None = 0,

				Use_Misc = 1 << 0
			}

			public Fillable.Data.Flags flags;
			public Material.Form form_type;

			public float capacity;
			public float amount;

			[Save.Ignore, Net.Ignore] public float t_next_update;

			public Data()
			{

			}
		}

#if SERVER
		[ISystem.Event<Blob.ContactEvent>(ISystem.Mode.Single, ISystem.Scope.Region, order: 50)]
		public static void OnContactBlob(ISystem.Info info, ref Region.Data region, Entity ent_fillable, ref Blob.ContactEvent ev,
		[Source.Owned] ref Heat.Data heat, [Source.Owned] ref Heat.State heat_state,
		[Source.Owned] ref Body.Data body, [Source.Owned] ref Transform.Data transform,
		//[Source.Owned, Pair.Component<Fillable.Data>] ref Inventory4.Data inventory,
		[Source.Owned] ref Fillable.Data fillable)
		{
			App.WriteLine("fillable: on contact blob");
		}
#endif

		[ISystem.PostUpdate.C(ISystem.Mode.Single, ISystem.Scope.Region)]
		public static void Update(ISystem.Info info, ref Region.Data region, ref XorRandom random, Entity entity,
		[Source.Owned] in Transform.Data transform,
		[Source.Owned] ref Fillable.Data fillable)
		{

		}

#if CLIENT
		public struct FillableGUI: IGUICommand
		{
			public Entity ent_fillable;

			public Transform.Data transform;
			public Fillable.Data fillable;

			public void Draw()
			{
				using (var window = GUI.Window.InteractionMisc("Fillable"u8, this.ent_fillable, size: new(24, 96 * 1)))
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
		[Source.Owned] in Fillable.Data fillable)
		{
			if (interactable.IsActive())
			{
				var gui = new FillableGUI()
				{
					ent_fillable = entity,

					transform = transform,
					fillable = fillable
				};
				gui.Submit();
			}
		}
#endif
	}
}
