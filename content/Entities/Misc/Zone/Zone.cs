using TC2.Base.Components;
using System.Runtime.InteropServices;

namespace TC2.Conquest
{
	public static partial class Zone
	{
		[IComponent.Data(Net.SendType.Reliable, IComponent.Scope.Region)]
		public partial struct Data(): IComponent
		{
			[Flags]
			public enum Flags: uint
			{
				None = 0u,
			}

			public Zone.Data.Flags flags;
			public uint unused_00;
			public uint unused_01;
			public uint unused_02;

			//[Save.Force, Editor.Slider.Clamped(-16, 16, snap: 0.125f, mark_modified: true)]
			//[Editor.Picker.Position(relative: true, mark_modified: true)]
			[Editor.Picker.Box(mark_modified: true, snap: 0.25f)]
			public AABB rect;

		}

		[ISystem.Update.A(ISystem.Mode.Single, ISystem.Scope.Region)]
		public static void OnUpdate(ISystem.Info.Common info, ref Region.Data.Common region, Entity entity,
		[Source.Owned] ref Zone.Data zone, [Source.Owned] ref Transform.Data transform)
		{

		}

#if CLIENT
		public partial struct ZoneGUI: IGUICommand
		{
			public Entity ent_zone;
			public Zone.Data zone;
			public Transform.Data transform;

			public void Draw()
			{
				ref var region_common = ref this.ent_zone.GetRegionCommon();

				//var quad = this.zone.rect.ToQuad();

				var w_size = this.zone.rect.GetSize();

				var w_pos = this.transform.GetInterpolatedPosition();
				//var w

				//var w_quad = this.transform.LocalToWorld(quad);

				//var c_pos = region_common.WorldToCanvas(w_pos);
				//var c_quad = region_common.WorldToCanvas(w_quad);

				//var c_pos = c_quad.c

				//var c_box = 

				var c_box = region_common.WorldToCanvas(this.zone.rect.Translate(w_pos));
				var c_pos = c_box.GetPosition(pivot: new(0.00f, 0.00f));


				using (var window = GUI.Window.Standalone(Utf8String.CreateHex("hud.zone_"u8, this.ent_zone.GetLower()), position: c_pos, pivot: new(0.00f, 0.00f), size: c_box.GetSize(), padding: new(4.00f), flags: GUI.Window.Flags.No_Appear_Focus))
				{
					if (window.show)
					{
						ref var shape = ref this.ent_zone.GetTrait<Zone.Data, Shape.Complex>();
						if (shape.IsNotNull())
						{
							var points_span = shape.GetPointsWorld(out var points_buffer); //.AsSpan(shape.count);
							//points_span.ForEach(ref region_common, GUI.WorldToCanvas);
							foreach (ref var point in points_span)
							{
								point = region_common.WorldToCanvas(point);
							}

							GUI.DrawPolygon(points_span.AsVector2(), color: GUI.col_button_yellow.WithAlpha(16), layer: GUI.Layer.Background);
							foreach (var line in points_span.GetClosedLineEnumerator())
							{
								//GUI.DrawLine(line, color: GUI.font_color_green, 1, layer: GUI.Layer.Background);
								GUI.DrawLineTextured(line.a, line.b, h_texture: GUI.tex_bar_solid, color: GUI.col_button_yellow.WithAlpha(192), thickness: 4, overshoot: 2.00f, layer: GUI.Layer.Background);
							}
						}

						GUI.DrawWindowBackground(texture: GUI.tex_window, padding: 4); // padding: new(8.00f), color: GUI.col_frame);
					
						if (GUI.DrawButton("test"u8, size: new(64, 32)))
						{

						}
					}
				}
			}
		}

		[ISystem.LateGUI(ISystem.Mode.Single, ISystem.Scope.Region)]
		public static void OnGUI(ISystem.Info info, ref Region.Data region, Entity ent_zone,
		[Source.Owned] in Zone.Data zone, [Source.Owned] in Transform.Data transform)
		{
			var gui = new Zone.ZoneGUI()
			{
				ent_zone = ent_zone,
				zone = zone,
				transform = transform
			};
			gui.Submit();
		}
#endif
	}
}

