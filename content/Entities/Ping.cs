
namespace TC2.Base.Components
{
	public static partial class Ping
	{
		[Flags]
		public enum Flags: uint
		{
			None = 0u,
		}

		[IComponent.Data(Net.SendType.Unreliable), IComponent.AddTo<Player.Data>()]
		public partial struct Data: IComponent
		{
			[Save.Ignore] public Entity ent_target;
			[Save.Ignore] public Vector2 pos;
			[Save.Ignore] public Color32BGRA color;
			[Save.Ignore] public float duration;
			[Save.Ignore] public float elapsed;
			[Save.Ignore] public FixedString16 text;
			[Save.Ignore, Net.Ignore] public float next_ping;
		}

		public partial struct PingRPC: Net.IRPC<Ping.Data>
		{
			public Entity ent_target;
			public Vector2 pos;
			public Color32BGRA color;
			public FixedString16 text;

#if SERVER
			public void Invoke(ref NetConnection connection, Entity entity, ref Data data)
			{
				ref var region = ref connection.GetRegion();
				ref var player = ref connection.GetPlayer();

				if (region.IsNotNull() && player.IsNotNull() && entity == player.ent_player)
				{
					if (region.GetWorldTime() >= data.next_ping)
					{
						var random = XorRandom.New(true);

						data.pos = this.pos;
						data.color = this.color;
						data.ent_target = this.ent_target;
						data.elapsed = 0.00f;
						data.duration = 10.00f;
						data.text = this.text;
						data.next_ping = region.GetWorldTime() + 0.20f;

						//Sound.Play(ref region, "ui.alert.bwoing.02", data.pos, volume: 1.00f, pitch: 1.00f, size: 0.10f);
						Sound.PlayGUI(ref region, "ui.misc.02", volume: 1.20f, pitch: random.NextFloatRange(0.98f, 1.01f), h_faction: player.faction_id);

						data.Sync(entity, true);
					}
				}
			}
#endif
		}

#if CLIENT
		public partial struct PingGUI: IGUICommand
		{
			public Entity ent_ping;
			public Ping.Data ping;

			public void Draw()
			{
				var pos = this.ping.pos;
				var rect_canvas = GUI.GetCanvasRect().Pad(16, 16, 16, 16);

				var c_pos = pos.WorldToCanvas();
				c_pos = rect_canvas.ClipPoint(c_pos);

				using (var window = GUI.Window.HUD($"ping.{this.ent_ping}", position: c_pos, size: new Vector2(64, 64), pivot: new(0.50f, 0.50f)))
				{
					if (window.show)
					{
						var scale = GUI.GetWorldToCanvasScale();

						var speed = 4.40f;
						var sin = Maths.HvSin(App.GetCurrentTime() * speed);

						var expand_duration = 0.66f;
						var bounce_duration = 1.00f;

						var expand_lerp_t = Maths.NormalizeClamp(this.ping.elapsed, expand_duration);
						var bounce_lerp_t = Maths.NormalizeClamp(this.ping.elapsed, bounce_duration);
						var fade_lerp_t = Maths.InvLerp01(this.ping.duration, this.ping.duration - 1.00f, this.ping.elapsed);

						var fade_alpha = fade_lerp_t;

						var col_text = GUI.font_color_title.LumaBlend(this.ping.color, 0.75f);
						var col_inner = GUI.font_color_default.LumaBlend(this.ping.color, 0.95f);
						var col_lines = GUI.font_color_default.LumaBlend(this.ping.color, 0.75f).WithAlphaMult(Maths.LerpEaseOut(0.00f, 0.75f, expand_lerp_t, Maths.Easing.Quad));
						var col_outer = col_lines.WithAlphaMult(Maths.LerpEaseOut(0.00f, 0.95f, expand_lerp_t, Maths.Easing.Quad));
						var col_expand = this.ping.color;

						if (fade_lerp_t > 0.00f)
						{
							col_text = col_text.WithAlphaMult(fade_alpha);
							col_inner = col_inner.WithAlphaMult(fade_alpha);
							col_lines = col_lines.WithAlphaMult(fade_alpha);
							col_outer = col_outer.WithAlphaMult(fade_alpha);
							col_expand = col_expand.WithAlphaMult(fade_alpha);
						}

						var circle_outer_extra = 0.00f;
						if (bounce_lerp_t < 1.00f) circle_outer_extra = Maths.LerpEaseOut(2.00f, 0.00f, bounce_lerp_t, Maths.Easing.Elastic);

						//if (expand_lerp_t < 1.00f) GUI.DrawCircleFilled(c_pos, Maths.LerpEaseOut(0.00f, 40.00f, expand_lerp_t, Maths.Easing.Quint) * scale, color: col_expand.WithAlphaMult(Maths.LerpEaseInOut(0.05f, 0.00f, expand_lerp_t * 1.20f, Maths.Easing.Quad)), segments: 64);
						if (expand_lerp_t < 1.00f) GUI.DrawCircleFilled(c_pos, Maths.LerpEaseOut(0.00f, 10.00f, expand_lerp_t, Maths.Easing.Circ) * scale, color: col_expand.WithAlphaMult(Maths.LerpEaseOut(0.50f, 0.00f, expand_lerp_t, Maths.Easing.Quint)), segments: 64);

						var line_radius = 2.00f + circle_outer_extra;

						GUI.DrawLine(c_pos - new Vector2(line_radius * scale, 0.00f), c_pos + new Vector2(line_radius * scale, 0.00f), col_lines);
						GUI.DrawLine(c_pos - new Vector2(0.00f, line_radius * scale), c_pos + new Vector2(0.00f, line_radius * scale), col_lines);

						GUI.DrawCircleFilled(c_pos, (0.25f - (circle_outer_extra * 0.70f)) * scale, color: col_inner, segments: 4);
						GUI.DrawCircle(c_pos, (Maths.Lerp(0.80f, 1.20f, sin) + circle_outer_extra) * scale, color: col_outer.WithAlphaMult(Maths.LerpEaseOut(0.00f, 0.40f, expand_lerp_t, Maths.Easing.Circ)), thickness: 2.00f, segments: 20);

						GUI.TitleCentered(this.ping.text, pivot: new(0.50f, 1.00f), color: col_text);
					}
				}
			}
		}

		[ISystem.EarlyGUI(ISystem.Mode.Single)]
		public static void OnGUI(ISystem.Info info, Entity entity, [Source.Owned] in Ping.Data ping)
		{
			//if (faction.IsNull() || faction.id == Client.GetFaction())
			if (ping.elapsed < ping.duration)
			{
				var gui = new PingGUI()
				{
					ent_ping = entity,
					ping = ping
				};
				gui.Submit();
			}
		}

		[ISystem.Update(ISystem.Mode.Single)]
		public static void OnUpdate(ISystem.Info info, Entity entity, [Source.Owned] ref Ping.Data ping, [Source.Owned] in Control.Data control)
		{
			//if (faction.IsNull() || faction.id == Client.GetFaction())
			if (ping.elapsed < ping.duration)
			{
				ping.elapsed += info.DeltaTime;
			}
		}

		[ISystem.Update(ISystem.Mode.Single), HasTag("local", true, Source.Modifier.Owned)]
		public static void OnUpdateLocal(ISystem.Info info, Entity entity, [Source.Owned] ref Ping.Data ping, [Source.Owned] in Control.Data control)
		{
			//if (faction.IsNull() || faction.id == Client.GetFaction())
			if (info.WorldTime >= ping.next_ping)
			{
				if (control.mouse.GetKeyDown(Mouse.Key.Middle))
				{
					var color = GUI.font_color_default;

					ref var faction_data = ref Client.GetFaction().GetData();
					if (faction_data.IsNotNull())
					{
						//color = color.LumaBlend(faction_data.color_a, 0.50f);
						color = faction_data.color_a;
					}

					var rpc = new Ping.PingRPC()
					{
						color = color,
						pos = control.mouse.GetInterpolatedPosition(),
						text = Client.NetConnection.GetName()
					};
					rpc.Send(entity);

					ping.next_ping = info.WorldTime + 0.22f;
				}
			}
		}
#endif
	}
}
