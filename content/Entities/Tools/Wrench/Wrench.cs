
using Keg;
using static TC2.Base.Components.Pathfinding;
using static TC2.Base.Components.Wrench;

namespace TC2.Base.Components
{
	public static partial class Wrench
	{
		[IComponent.Data(Net.SendType.Reliable)]
		public partial struct Data: IComponent
		{
			[Save.Ignore]
			public ulong selected_component_id;
		}

		public static partial class Mode
		{
			public static partial class Belts
			{
				[IComponent.Data(Net.SendType.Reliable)]
				public partial struct Data: IComponent, Wrench.IMode
				{
					[Save.Ignore] public Entity ent_src;
					[Save.Ignore] public Entity ent_dst;

					public static Sprite Icon { get; } = new Sprite("ui_icons_builder_categories", 0, 1, 16, 16, 0, 0);

#if CLIENT
					private ref struct HoveredEntityInfo
					{
						public Entity entity;
						public ref Axle.Data axle;
						public ref Transform.Data transform;
					}

					public void Draw(Entity ent_wrench, ref Wrench.Data wrench)
					{
						ref var player = ref Client.GetPlayer();
						ref var region = ref Client.GetRegion();

						ref readonly var kb = ref Control.GetKeyboard();
						ref readonly var mouse = ref Control.GetMouse();

						var ent_src_tmp = this.ent_src;
						var ent_dst_tmp = this.ent_dst;
						var ent_new = default(Entity);

						var c_pos_src = default(Vector2?);
						var c_pos_dst = default(Vector2?);
						var c_pos_new = default(Vector2?);
						var c_pos_mouse = GUI.WorldToCanvas(mouse.GetInterpolatedPosition());

						ref var axle_src = ref Unsafe.NullRef<Axle.Data>();
						ref var axle_dst = ref Unsafe.NullRef<Axle.Data>();
						ref var axle_new = ref Unsafe.NullRef<Axle.Data>();

						ref var transform_src = ref Unsafe.NullRef<Transform.Data>();
						ref var transform_dst = ref Unsafe.NullRef<Transform.Data>();
						ref var transform_new = ref Unsafe.NullRef<Transform.Data>();

						var scale = GUI.GetWorldToCanvasScale();

						Span<OverlapResult> results = stackalloc OverlapResult[16];
						if (region.TryOverlapPointAll(mouse.GetInterpolatedPosition(), 0.125f, ref results, mask: Physics.Layer.Entity))
						{
							foreach (ref var result in results)
							{
								if (result.entity.HasComponent<Axle.Data>())
								{
									ent_new = result.entity;
									break;
								}
							}
						}

						var is_src_alive = this.ent_src.IsAlive();
						var is_dst_alive = this.ent_dst.IsAlive();
						var is_new_alive = ent_new.IsAlive();

						if (is_new_alive)
						{
							if (!is_src_alive)
							{
								ent_src_tmp = ent_new;
							}
							else if (!is_dst_alive)
							{
								ent_dst_tmp = ent_new;
							}

							GUI.SetCursor(App.CursorType.Hand, 10);
						}

						if (is_src_alive)
						{
							axle_src = ref this.ent_src.GetComponent<Axle.Data>();
							transform_src = ref this.ent_src.GetComponent<Transform.Data>();

							if (!axle_src.IsNull() && !transform_src.IsNull())
							{
								c_pos_src = GUI.WorldToCanvas(transform_src.LocalToWorldNoRotation(axle_src.offset));
							}
						}

						if (is_dst_alive)
						{
							axle_dst = ref this.ent_dst.GetComponent<Axle.Data>();
							transform_dst = ref this.ent_dst.GetComponent<Transform.Data>();

							if (!axle_dst.IsNull() && !transform_dst.IsNull())
							{
								c_pos_dst = GUI.WorldToCanvas(transform_dst.LocalToWorldNoRotation(axle_dst.offset));
							}
						}

						var color = new Color32BGRA(0xffffff00);

						var claim_ratio = Claim.GetOverlapRatio(ref region, AABB.Circle(mouse.GetInterpolatedPosition(), 1.00f), player.faction_id);
						var is_allowed = claim_ratio > 0.90f;

						if (!is_allowed)
						{
							color = GUI.font_color_red;
						}

						if (c_pos_src.HasValue)
						{
							if (c_pos_dst.HasValue)
							{
								GUI.DrawLine(c_pos_src.Value, c_pos_dst.Value, color, 4.00f);
							}
							else
							{
								GUI.DrawLine(c_pos_src.Value, c_pos_mouse, color, 4.00f);
							}
						}

						if (c_pos_src.HasValue && !axle_src.IsNull() && !transform_src.IsNull())
						{
							GUI.DrawCircle(c_pos_src.Value, axle_src.radius_a * scale, color);
						}

						if (c_pos_dst.HasValue && !axle_dst.IsNull() && !transform_dst.IsNull())
						{
							GUI.DrawCircle(c_pos_dst.Value, axle_dst.radius_b * scale, color);
						}

						if (mouse.GetKeyDown(Mouse.Key.Left))
						{
							if (is_allowed)
							{
								var rpc = new Wrench.Mode.Belts.SetTargetRPC()
								{
									ent_src = ent_src_tmp,
									ent_dst = ent_dst_tmp,
								};
								rpc.Send(ent_wrench);
							}
							else
							{
								Sound.PlayGUI(GUI.sound_error, 0.50f);
							}
						}
						else if (mouse.GetKeyDown(Mouse.Key.Right))
						{
							var rpc = new Wrench.Mode.Belts.SetTargetRPC()
							{
								ent_src = default,
								ent_dst = default,
							};
							rpc.Send(ent_wrench);
						}
					}
#endif
				}

				public struct SetTargetRPC: Net.IRPC<Wrench.Mode.Belts.Data>
				{
					public Entity ent_src;
					public Entity ent_dst;

#if SERVER
					public void Invoke(ref NetConnection connection, Entity entity, ref Wrench.Mode.Belts.Data data)
					{
					App.WriteLine($"{this.ent_src} == {data.ent_src}; {this.ent_dst} == {data.ent_dst}");

						data.ent_src = this.ent_src;
						data.ent_dst = this.ent_dst;

						data.Sync(entity);
					}
#endif
				}

			}

			public static partial class Ducts
			{
				[IComponent.Data(Net.SendType.Reliable)]
				public partial struct Data: IComponent, Wrench.IMode
				{
					[Save.Ignore] public Entity ent_src;
					[Save.Ignore] public Entity ent_dst;

					public static Sprite Icon { get; } = new Sprite("ui_icons_builder_categories", 0, 1, 16, 16, 1, 0);

#if CLIENT
					public void Draw(Entity ent_wrench, ref Wrench.Data wrench)
					{
						GUI.Text("hi");
					}
#endif
				}
			}
		}

		public interface IMode: IComponent
		{
			public static abstract Sprite Icon { get; }

#if CLIENT
			public void Draw(Entity ent_wrench, ref Wrench.Data wrench);
#endif
		}

		public struct SelectModeRPC: Net.IRPC<Wrench.Data>
		{
			public ulong selected_component_id;

#if SERVER
			public void Invoke(ref NetConnection connection, Entity entity, ref Wrench.Data data)
			{
				if (entity.HasComponent(this.selected_component_id))
				{
					data.selected_component_id = this.selected_component_id;
					data.Sync(entity);
				}
			}
#endif
		}

#if CLIENT
		public static void DrawModeButton<T>(Entity ent_wrench) where T : unmanaged, Wrench.IMode
		{
			ref var info = ref ECS.GetInfo<T>();

			using (GUI.ID.Push(info.id))
			{
				if (GUI.DrawIconButton($"wrench.mode.{info.identifier}", T.Icon, new(40, 40)))
				{
					var rpc = new Wrench.SelectModeRPC()
					{
						selected_component_id = info.id
					};
					rpc.Send(ent_wrench);
				}

				if (GUI.IsItemHovered())
				{
					using (GUI.Tooltip.New())
					{
						GUI.Title(info.identifier);
					}
				}
			}
		}

		public const string dock_identifier = "wrench.mode.dock";

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

						GUI.Text($"Derp: {this.wrench.selected_component_id}");

						using (GUI.Group.New(size: new(GUI.GetRemainingWidth(), 32)))
						{
							Wrench.DrawModeButton<Wrench.Mode.Belts.Data>(this.ent_wrench);
							GUI.SameLine();

							Wrench.DrawModeButton<Wrench.Mode.Ducts.Data>(this.ent_wrench);
							GUI.SameLine();

							//var count = 4;
							//for (var i = 0; i < count; i++)
							//{
							//	using (GUI.ID.Push(i + 1))
							//	{
							//		DrawModeButton<>

							//		//if (GUI.DrawIconButton($"wrench.mode.{i}", new Sprite("ui_icons_builder_categories", 0, 1, 16, 16, (uint)i, 0), new(40, 40)))
							//		//{

							//		//}

							//		//if (GUI.IsItemHovered())
							//		//{
							//		//	using (GUI.Tooltip.New())
							//		//	{
							//		//		//GUI.Title(category_names[i]);
							//		//	}
							//		//}

							//		if (i < count - 1) GUI.SameLine();
							//	}
							//}
						}

						using (GUI.Group.New(size: GUI.GetRemainingSpace()))
						{
							GUI.Dock.New(Wrench.dock_identifier, size: GUI.GetRemainingSpace());
							//var v = new WrenchModeGUI<Wrench.Mode.Belts.Data>();
							//v.Draw();
						}
					}
				}
			}
		}

		public struct WrenchModeGUI<T>: IGUICommand where T : unmanaged, Wrench.IMode
		{
			public Entity ent_wrench;

			public Transform.Data transform;
			public Wrench.Data wrench;
			public T mode;

			public void Draw()
			{
				ref var info = ref ECS.GetInfo<T>();

				using (var window = GUI.Window.Docked(info.identifier, dock_identifier: Wrench.dock_identifier, padding: new(8, 8)))
				{
					if (window.show)
					{
						GUI.Title(info.identifier);

						this.mode.Draw(this.ent_wrench, ref this.wrench);
					}
				}
			}
		}

		[ISystem.GUI(ISystem.Mode.Single)]
		public static void OnGUI(ISystem.Info info, Entity entity,
		[Source.Parent] in Interactor.Data interactor, [Source.Owned] ref Wrench.Data wrench,
		[Source.Owned] in Transform.Data transform, [Source.Parent] in Player.Data player, [Source.Owned] in Control.Data control)
		{
			if (player.IsLocal())
			{
				//App.WriteLine("ye");

				var gui = new WrenchGUI()
				{
					ent_wrench = entity,
					transform = transform,
					wrench = wrench
				};
				gui.Submit();
			}
		}

		[ISystem.LateGUI(ISystem.Mode.Single)]
		public static void OnGUIMode<T>(ISystem.Info info, Entity entity,
		[Source.Owned] in T mode, [Source.Parent] in Interactor.Data interactor, [Source.Owned] ref Wrench.Data wrench,
		[Source.Owned] in Transform.Data transform, [Source.Parent] in Player.Data player, [Source.Owned] in Control.Data control) where T : unmanaged, Wrench.IMode
		{
			if (player.IsLocal() && wrench.selected_component_id == ECS.GetID<T>())
			{
				var gui = new WrenchModeGUI<T>()
				{
					ent_wrench = entity,
					transform = transform,
					wrench = wrench,
					mode = mode
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
