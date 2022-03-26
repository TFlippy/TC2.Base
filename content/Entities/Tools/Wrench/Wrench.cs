
namespace TC2.Base.Components
{
	public static partial class Wrench
	{
		[IComponent.Data(Net.SendType.Reliable)]
		public partial struct Data: IComponent
		{
			public int mode_index;
		}

		public interface IMode: IComponent
		{
			
		}

#if CLIENT
		public struct WrenchGUI: IGUICommand
		{
			public Entity ent_wrench;

			public Transform.Data transform;
			public Wrench.Data wrench;

			public void Draw()
			{
				var window_size = new Vector2((48 * 8) + 32, (48 * 7) + 32 + 24);

				using (var window = GUI.Window.Standalone("Wrench", size: window_size, padding: new(8, 8), pivot: new(0.50f, 0.50f)))
				{
					this.StoreCurrentWindowTypeID();
					if (window.show)
					{
						GUI.DrawWindowBackground(GUI.tex_window_menu, new Vector4(8, 8, 8, 8));

						ref var region = ref Client.GetRegion();

						GUI.Text("Derp");
					}
				}
			}
		}

		[ISystem.GUI(ISystem.Mode.Single)]
		public static void OnGUI(ISystem.Info info, Entity entity, [Source.Parent] in Interactor.Data interactor, [Source.Owned] ref Wrench.Data wrench, [Source.Owned] in Transform.Data transform, [Source.Parent] in Player.Data player, [Source.Owned] in Control.Data control)
		{
			if (player.IsLocal())
			{
				var gui = new WrenchGUI()
				{
					ent_wrench = entity,
					transform = transform,
					wrench = wrench
				};

				gui.Submit();
			}
		}
#endif

		[ISystem.Update(ISystem.Mode.Single)]
		public static void Update(ISystem.Info info, Entity entity,
		[Source.Owned] ref Wrench.Data wrench, [Source.Owned] in Transform.Data transform, [Source.Owned] in Control.Data control, [Source.Owned] in Body.Data body)
		{

		}
	}
}
