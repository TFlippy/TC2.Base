using TC2.Base.Components;
using System.Runtime.InteropServices;

namespace TC2.Conquest
{
	public static partial class Stance
	{
		[IComponent.AddTo<Character.Data>]
		[IComponent.Data(Net.SendType.Unreliable, IComponent.Scope.Region | IComponent.Scope.Global)]
		public partial struct Data(): IComponent
		{
			[Flags]
			public enum Flags: ushort
			{
				None = 0,
			}

			public ushort h_stance;
			public Stance.Data.Flags flags;

			public uint unused_00;
			public uint unused_01;
			public uint unused_02;
		}

		[ISystem.Update.A(ISystem.Mode.Single, ISystem.Scope.Region | ISystem.Scope.Global)]
		public static void OnUpdate(ISystem.Info.Common info, ref Region.Data.Common region, Entity entity,
		[Source.Owned] ref Stance.Data stance)
		{

		}

		public partial struct EditRPC: Net.IRPC<Stance.Data>
		{
			public ushort? h_stance;
			public Stance.Data.Flags? flags;
#if SERVER
			public void Invoke(Net.IRPC.Context rpc, ref Data data)
			{
				//Assert.Check(rpc.entity == rpc.connection.can) // TODO: add a check if the sender can control the entity

				var sync = false;

				sync |= data.h_stance.TrySet(this.h_stance);
				sync |= data.flags.TrySetFlagMasked(this.flags, mask: Data.Flags.None);

				if (sync)
				{
					rpc.Sync(ref data, overwrite: true);
				}
			}
#endif
		}

#if CLIENT
		public partial struct StanceGUI: IGUICommand
		{
			public Entity ent_stance;
			public Stance.Data stance;
	
			public void Draw()
			{
				var rect_canvas = GUI.GetCanvasRect();

				using (var window = GUI.Window.Standalone(identifier: "hud.stance"u8,
				position: rect_canvas.GetPosition(pivot: new(0.50f, 1.00f), offset: new(0.00f, -(96.00f + 12.00f))),
				pivot: new(0.50f, 1.00f),
				size: new(0.00f, 64.00f),
				flags: GUI.Window.Flags.No_Appear_Focus))
				{
					var h_stance_selected = this.stance.h_stance;

					static void Inner_DrawButton(Entity ent_stance, Utf8String identifier, Utf8String name, ushort h_stance, ushort h_stance_selected, Sprite sprite)
					{
						var col_default = GUI.col_button.WithAlpha(128);
						var col_selected = GUI.col_white;

						if (GUI.DrawSpriteButton(identifier: identifier, sprite: sprite, size: new(GUI.RmY), scale: 3.00f, color: h_stance_selected == h_stance ? col_selected : col_default, color_pressed: col_selected))
						{
							var rpc = new EditRPC
							{
								h_stance = h_stance
							};
							rpc.Send(ent_stance);
						}
						GUI.DrawHoverTooltip(name);
					}

					Inner_DrawButton(ent_stance: this.ent_stance, identifier: "bt.move"u8, name: "Move"u8, h_stance: 0, h_stance_selected: h_stance_selected, sprite: GUI.spr_icons_widget.WithFrame(1, 6));
					GUI.SameLine();

					Inner_DrawButton(ent_stance: this.ent_stance, identifier: "bt.work"u8, name: "Work"u8, h_stance: 1, h_stance_selected: h_stance_selected, sprite: GUI.spr_icons_widget.WithFrame(2, 6));
					GUI.SameLine();

					Inner_DrawButton(ent_stance: this.ent_stance, identifier: "bt.harvest"u8, name: "Harvest"u8, h_stance: 2, h_stance_selected: h_stance_selected, sprite: GUI.spr_icons_widget.WithFrame(5, 6));
					GUI.SameLine();

					Inner_DrawButton(ent_stance: this.ent_stance, identifier: "bt.combat"u8, name: "Combat"u8, h_stance: 3, h_stance_selected: h_stance_selected, sprite: GUI.spr_icons_widget.WithFrame(3, 6));

					ref readonly var kb = ref Control.GetKeyboard();
					if (kb.GetKeyDown(Keyboard.Key.Tab))
					{
						
					}


					//GUI.DrawWindowBackground();
					//window.group.DrawBackground(GUI.tex_window);


					//GUI.DrawRectFilled(rect_canvas, layer: GUI.Layer.Foreground);

					//using (var group = GUI.Group.New(size: GUI.Av))
					//{
					//	group.DrawBackground(GUI.tex_window);
					//}
				}
			}
		}

		[ISystem.LateGUI(ISystem.Mode.Single, ISystem.Scope.Region | ISystem.Scope.Global), HasTag("local", true, Source.Modifier.Shared)]
		public static void OnGUI(ISystem.Info.Common info, ref Region.Data.Common region, Entity ent_stance,
		[Source.Owned] in Stance.Data stance)
		{
			var gui = new Stance.StanceGUI()
			{
				ent_stance = ent_stance,
				stance = stance
			};
			gui.Submit();
		}
#endif
	}
}

