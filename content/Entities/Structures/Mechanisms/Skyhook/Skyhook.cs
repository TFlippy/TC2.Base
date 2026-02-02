namespace TC2.Base.Components
{
	public static partial class Skyhook
	{
		[IComponent.Data(Net.SendType.Reliable, IComponent.Scope.Region)]
		public partial struct Data(): IComponent
		{
			public float unused_00;
			public float unused_01;
			public float unused_02;
			public float unused_03;
		}

#if CLIENT
		public partial struct SkyhookGUI: IGUICommand
		{
			public Entity ent_skyhook;
			public Skyhook.Data skyhook;
			public Transform.Data transform;

			public void Draw()
			{
				using (var window = GUI.Window.Interaction("Skyhook"u8, this.ent_skyhook))
				{
					this.StoreCurrentWindowTypeID(order: 7);
					if (window.show)
					{

					}
				}
			}
		}

		[ISystem.GUI(ISystem.Mode.Single, ISystem.Scope.Region)]
		public static void OnGUI([Source.Owned] in Interactable.Data interactable,
		Entity ent_skyhook, [Source.Owned] in Skyhook.Data skyhook, [Source.Owned] in Transform.Data transform)
		{
			if (interactable.IsActive())
			{
				var gui = new SkyhookGUI()
				{
					ent_skyhook = ent_skyhook,
					skyhook = skyhook,
					transform = transform,
				};
				gui.Submit();
			}
		}
#endif

		[ISystem.Update.C(ISystem.Mode.Single, ISystem.Scope.Region)]
		public static void OnUpdate(ISystem.Info info, ref Region.Data region, Entity entity,
		[Source.Owned] ref Skyhook.Data skyhook, [Source.Owned] ref Transform.Data transform, [Source.Owned] ref Body.Data body)
		{

		}
	}
}
