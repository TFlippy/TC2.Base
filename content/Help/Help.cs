using TC2.Base.Components;
using System.Runtime.InteropServices;

namespace TC2.Conquest
{
	public static partial class Help
	{
		[IComponent.Data(Net.SendType.Unreliable, IComponent.Scope.Region | IComponent.Scope.Global)]
		public partial struct Data(): IComponent
		{
			[Flags]
			public enum Flags: ushort
			{
				None = 0,
			}

			public ushort h_help;
			public Help.Data.Flags flags;

			public uint unused_00;
			public uint unused_01;
			public uint unused_02;
		}

		//[ISystem.Update.A(ISystem.Mode.Single, ISystem.Scope.Region | ISystem.Scope.Global)]
		//public static void OnUpdate(ISystem.Info.Common info, ref Region.Data.Common region, Entity entity,
		//[Source.Owned] ref Help.Data help)
		//{

		//}

#if CLIENT
		public partial struct HelpGUI: IGUICommand
		{
			public Entity ent_help;
			public Help.Data help;
	
			public void Draw()
			{
				var rect_canvas = GUI.GetCanvasRect();

				using (var window = GUI.Window.Standalone(identifier: "hud.help"u8,
				position: rect_canvas.GetPosition(pivot: new(0.50f, 1.00f), offset: new(0.00f, -(96.00f + 12.00f))),
				pivot: new(0.50f, 1.00f),
				size: new(0.00f, 64.00f),
				flags: GUI.Window.Flags.No_Appear_Focus))
				{

				}
			}
		}

		[ISystem.LateGUI(ISystem.Mode.Single, ISystem.Scope.Region | ISystem.Scope.Global)]
		public static void OnGUI(ISystem.Info.Common info, ref Region.Data.Common region, Entity ent_help,
		[Source.Owned] in Help.Data help)
		{
			var gui = new Help.HelpGUI()
			{
				ent_help = ent_help,
				help = help
			};
			gui.Submit();
		}
#endif
	}
}

