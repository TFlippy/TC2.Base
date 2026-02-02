namespace TC2.Base.Components
{
	public static partial class Zeppelin
	{
		[IComponent.Data(Net.SendType.Reliable, IComponent.Scope.Region | IComponent.Scope.Global)]
		public partial struct Data(): IComponent
		{
			public float unused_00;
			public float unused_01;
			public float unused_02;
			public float unused_03;
		}

#if CLIENT
		public partial struct ZeppelinGUI: IGUICommand
		{
			public Entity ent_zeppelin;
			public Zeppelin.Data zeppelin;
			public Transform.Data transform;

			public void Draw()
			{
				using (var window = GUI.Window.Interaction("Zeppelin"u8, this.ent_zeppelin))
				{
					this.StoreCurrentWindowTypeID(order: 6);
					if (window.show)
					{

					}
				}
			}
		}

		[ISystem.GUI(ISystem.Mode.Single, ISystem.Scope.Region)]
		public static void OnGUI([Source.Owned] in Interactable.Data interactable, 
		Entity ent_zeppelin, [Source.Owned] in Zeppelin.Data zeppelin, [Source.Owned] in Transform.Data transform)
		{
			if (interactable.IsActive())
			{
				var gui = new ZeppelinGUI()
				{
					ent_zeppelin = ent_zeppelin,
					zeppelin = zeppelin,
					transform = transform,
				};
				gui.Submit();
			}
		}
#endif

		[ISystem.Update.C(ISystem.Mode.Single, ISystem.Scope.Region)]
		public static void OnUpdate(ISystem.Info info, ref Region.Data region, Entity entity,
		[Source.Owned] ref Zeppelin.Data zeppelin, [Source.Owned] ref Transform.Data transform, [Source.Owned] ref Body.Data body)
		{
			body.SetVelocity(new Vector2(0, 0));
		}
	}
}
