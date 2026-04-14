namespace TC2.Base.Components
{
	public static partial class Guardhouse
	{
		[IComponent.Data(Net.SendType.Reliable, IComponent.Scope.Region)]
		public partial struct Data(): IComponent
		{
			[Flags]
			public enum Flags: uint
			{
				None = 0,
			}

			public Guardhouse.Data.Flags flags;

			public float unused_00;
			public float unused_01;
			public float unused_02;
		}

		public struct EditRPC: Net.IRPC<Guardhouse.Data>
		{

#if SERVER
			public void Invoke(Net.IRPC.Context rpc, ref Guardhouse.Data data)
			{
				var sync = false;

				if (sync)
				{
					rpc.Sync(ref data, true);
				}
			}
#endif
		}

		[ISystem.Update.B(ISystem.Mode.Single, ISystem.Scope.Region)]
		public static void OnUpdate(ISystem.Info info, ref Region.Data region, ref XorRandom random, Entity ent_guardhouse,
		[Source.Owned] ref Personnel.Data personnel, [Source.Owned] ref Guardhouse.Data guardhouse,
		[Source.Owned] ref Body.Data body, [Source.Owned] in Transform.Data transform)
		{

		}

#if CLIENT
		public struct GuardhouseGUI: IGUICommand
		{
			public Entity ent_guardhouse;

			public Guardhouse.Data guardhouse;
			public Personnel.Data personnel;
			public Transform.Data transform;

			public void Draw()
			{
				using (var window = GUI.Window.Interaction(identifier: "Guard Post"u8, entity: this.ent_guardhouse,
				color_tab: 0xff719240, tooltip_tab: "Manage guards and security policies."))
				{
					this.StoreCurrentWindowTypeID(order: -1000);
					if (window.show)
					{
						using (var group_main = GUI.Group.New(size: GUI.Rm))
						{
							group_main.DrawBackground(GUI.tex_window);
						}
					}
				}
			}
		}

		[ISystem.GUI(ISystem.Mode.Single, ISystem.Scope.Region)]
		public static void OnGUI([Source.Owned] in Interactable.Data interactable, Entity ent_guardhouse,
		[Source.Owned] in Guardhouse.Data guardhouse, [Source.Owned] in Personnel.Data personnel, 
		[Source.Owned] in Transform.Data transform)
		{
			if (interactable.IsActive())
			{
				var gui = new GuardhouseGUI()
				{
					ent_guardhouse = ent_guardhouse,

					guardhouse = guardhouse,
					personnel = personnel,
					transform = transform
				};
				gui.Submit();
			}
		}
#endif
	}
}
