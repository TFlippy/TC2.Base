
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

					public Crafting.Recipe.Handle selected_recipe;
					public Belt.Flags flags;

					public static Sprite Icon { get; } = new Sprite("ui_icons.wrench", 0, 1, 24, 24, 0, 0);

#if CLIENT
					public static List<(uint index, float rank)> recipe_indices = new List<(uint index, float rank)>(64);

					public void Draw(Entity ent_wrench, ref Wrench.Data wrench)
					{
						ref var player = ref Client.GetPlayer();
						ref var region = ref Client.GetRegion();

						var faction_id = player.faction_id;

						ref readonly var kb = ref Control.GetKeyboard();
						ref readonly var mouse = ref Control.GetMouse();

						var wpos_mouse = mouse.GetInterpolatedPosition();
						var cpos_mouse = GUI.WorldToCanvas(wpos_mouse);

						var scale = GUI.GetWorldToCanvasScale();

						var info_src = new TargetInfo(this.ent_src, true);
						var info_dst = new TargetInfo(this.ent_dst, false);
						var info_new = default(TargetInfo);

						var errors_src = Build.Errors.None;
						var errors_dst = Build.Errors.None;
						var errors_new = Build.Errors.None;

						//if (!info_src.valid || !info_dst.valid)
						{
							Span<OverlapResult> results = stackalloc OverlapResult[16];
							if (region.TryOverlapPointAll(wpos_mouse, 0.125f, ref results, mask: Physics.Layer.Entity))
							{
								foreach (ref var result in results)
								{
									if (result.entity == info_src.entity || result.entity == info_dst.entity) continue;

									info_new = new TargetInfo(result.entity, !info_src.valid);
									if (info_new.valid)
									{
										break;
									}
								}
							}
						}

						if (info_new.valid)
						{
							GUI.SetCursor(App.CursorType.Hand, 10);
						}

						var distance = 0.00f;
						if (info_src.valid)
						{
							if (info_dst.valid)
							{
								distance = Vector2.Distance(info_src.pos, info_dst.pos);
							}
							else if (info_new.valid)
							{
								distance = Vector2.Distance(info_src.pos, info_new.pos);
							}
							else
							{
								distance = Vector2.Distance(info_src.pos, mouse.position);
							}
						}

						var color_src = GUI.font_color_success;
						var color_dst = GUI.font_color_success;
						var color_new = GUI.font_color_yellow;

						{
							ref var recipe = ref this.selected_recipe.GetRecipe();
							if (!recipe.IsNull())
							{
								if (info_src.valid)
								{
									errors_src |= EvaluateBelt(ref region, ref info_src, ref recipe, faction_id: faction_id);
									if (!info_new.valid)
									{
										errors_new.SetFlag(Build.Errors.MaxLength | Build.Errors.OutOfRange, Vector2.Distance(info_src.pos, wpos_mouse) > recipe.placement.Value.length_max);
									}
								}

								if (info_dst.valid)
								{
									errors_dst |= EvaluateBelt(ref region, ref info_dst, ref recipe, faction_id: faction_id);
									if (info_src.valid)
									{
										errors_dst |= Wrench.Mode.Belts.EvaluateBeltConnection(ref region, ref info_src, ref info_dst, ref recipe, out _, player.faction_id);
									}
								}

								if (info_new.valid)
								{
									errors_new |= EvaluateBelt(ref region, ref info_new, ref recipe, faction_id: faction_id);
									if (info_src.valid)
									{
										errors_new |= Wrench.Mode.Belts.EvaluateBeltConnection(ref region, ref info_src, ref info_new, ref recipe, out _, player.faction_id);
									}
								}
							}
						}

						if (errors_src != Build.Errors.None)
						{
							color_src = GUI.font_color_error;
						}

						if (errors_dst != Build.Errors.None)
						{
							color_dst = GUI.font_color_error;
						}

						if (errors_new != Build.Errors.None)
						{
							color_new = GUI.font_color_error;
						}

						if (info_src.valid)
						{
							if (!info_new.valid && !info_dst.valid)
							{
								var dir = (info_src.pos - wpos_mouse).GetNormalizedFast();
								var n = new Vector2(-dir.Y, dir.X);
								var offset_src = n * info_src.radius;
								var offset_mouse = n * info_src.radius * 0.50f;

								GUI.DrawLine2((info_src.pos + offset_src).WorldToCanvas(), (wpos_mouse + offset_mouse).WorldToCanvas(), color_src, color_new.WithAlphaMult(0.00f), 2.00f, 2.00f);
								GUI.DrawLine2((info_src.pos - offset_src).WorldToCanvas(), (wpos_mouse - offset_mouse).WorldToCanvas(), color_src, color_new.WithAlphaMult(0.00f), 2.00f, 2.00f);
							}

							if (info_new.valid)
							{
								var dir = (info_src.pos - info_new.pos).GetNormalizedFast();
								var n = new Vector2(-dir.Y, dir.X);
								var offset_src = n * info_src.radius;
								var offset_new = n * info_new.radius;

								GUI.DrawLine2((info_src.pos + offset_src).WorldToCanvas(), (info_new.pos + offset_new).WorldToCanvas(), color_src, color_new.WithAlphaMult(0.50f), 2.00f, 2.00f);
								GUI.DrawLine2((info_src.pos - offset_src).WorldToCanvas(), (info_new.pos - offset_new).WorldToCanvas(), color_src, color_new.WithAlphaMult(0.50f), 2.00f, 2.00f);
							}

							if (info_dst.valid)
							{
								var dir = (info_src.pos - info_dst.pos).GetNormalizedFast();
								var n = new Vector2(-dir.Y, dir.X);
								var offset_src = n * info_src.radius;
								var offset_dst = n * info_dst.radius;

								GUI.DrawLine2((info_src.pos + offset_src).WorldToCanvas(), (info_dst.pos + offset_dst).WorldToCanvas(), color_src, color_dst, 2.00f, 2.00f);
								GUI.DrawLine2((info_src.pos - offset_src).WorldToCanvas(), (info_dst.pos - offset_dst).WorldToCanvas(), color_src, color_dst, 2.00f, 2.00f);
							}
						}

						if (info_src.valid)
						{
							GUI.DrawCircle(info_src.pos.WorldToCanvas(), info_src.radius * scale, color_src, 2.00f);
						}

						if (info_dst.valid)
						{
							GUI.DrawCircle(info_dst.pos.WorldToCanvas(), info_dst.radius * scale, color_dst, 2.00f);
						}

						if (info_new.valid)
						{
							GUI.DrawCircle(info_new.pos.WorldToCanvas(), info_new.radius * scale, color_new.WithAlphaMult(0.50f), 2.00f);
						}

						if (mouse.GetKeyDown(Mouse.Key.Left))
						{
							if (info_new.valid)
							{
								if (errors_new == Build.Errors.None)
								{
									var rpc = new Wrench.Mode.Belts.SetTargetRPC()
									{
										ent_src = info_new.is_src ? info_new.entity : this.ent_src,
										ent_dst = !info_new.is_src ? info_new.entity : this.ent_dst,
									};
									rpc.Send(ent_wrench);

									Sound.PlayGUI(GUI.sound_select, volume: 0.07f, pitch: info_new.is_src ? 0.80f : 0.95f);
								}
								else
								{
									Sound.PlayGUI(GUI.sound_error, 0.50f);
								}
							}
						}
						else if (mouse.GetKeyDown(Mouse.Key.Right))
						{
							if (info_src.valid || info_dst.valid)
							{
								var rpc = new Wrench.Mode.Belts.SetTargetRPC()
								{
									ent_src = info_dst.valid ? info_src.entity : default,
									ent_dst = default,
								};
								rpc.Send(ent_wrench);

								Sound.PlayGUI(GUI.sound_select, volume: 0.07f, pitch: 0.80f);
							}
						}

						//if (info_src.valid)
						//{
						//	//var pos_mid = (info_src.pos + pos_mouse) * 0.50f;
						//	//GUI.DrawText($"Distance: {distance:0.00}/20.00 m", pos_mid.WorldToCanvas());
						//	GUI.DrawText($"Distance: {distance:0.00}/20.00 m", pos_mouse.WorldToCanvas());
						//}

						using (GUI.Group.New(size: new Vector2(48 + 32 - 5, GUI.GetRemainingHeight())))
						{
							using (var dropdown = GUI.Scrollbox.New("wrench.belts.recipes", GUI.GetRemainingSpace(), padding: new Vector2(4, 4)))
							{
								//if (dropdown.show)
								//{
								GUI.DrawBackground(GUI.tex_window, dropdown.group_frame.GetInnerRect(), padding: new(8));

								var recipes = Shop.GetAllRecipes();
								foreach (ref var recipe in recipes)
								{
									if (recipe.type == Crafting.Recipe.Type.Wrench && recipe.tags.HasAll(Crafting.Recipe.Tags.Belt))
									{
										using (GUI.ID.Push(recipe.id))
										{
											using (GUI.Group.New(size: new Vector2(GUI.GetRemainingWidth(), 48)))
											{
												var frame_size = new Vector2(48, 48);
												var selected = this.selected_recipe.id == recipe.id;
												using (var button = GUI.CustomButton.New(recipe.name, frame_size, sound: GUI.sound_select, sound_volume: 0.10f))
												{
													GUI.Draw9Slice((selected || button.hovered) ? GUI.tex_slot_simple_hover : GUI.tex_slot_simple, new Vector4(4), button.bb);
													GUI.DrawSpriteCentered(recipe.icon, button.bb, scale: 2.00f);

													if (button.pressed)
													{
														var rpc = new Wrench.Mode.Belts.EditRPC
														{
															recipe = new Crafting.Recipe.Handle(recipe.id)
														};
														rpc.Send(ent_wrench);
														//dropdown.Close();
													}
												}
												if (GUI.IsItemHovered())
												{
													using (GUI.Tooltip.New())
													{
														using (GUI.Wrap.Push(256))
														{
															GUI.Title(recipe.name);
															GUI.Text(recipe.desc, color: GUI.font_color_default);
														}
													}
												}
											}
										}
									}
									//}
								}
							}

							{
								//ref var recipe = ref this.selected_recipe.GetRecipe();
								//if (!recipe.IsNull() && recipe.placement.HasValue)
								//{
								//	var placement = recipe.placement.Value;

								//	GUI.LabelShaded("Distance:", distance, $"{{0:0.00}}/{placement.length_max:0.00} m");
								//}
								//GUI.LabelShaded("Distance:", distance, "{0:0.00}m");

								//if (GUI.DrawButton("Confirm", new Vector2(128, 40), enabled: info_src.valid && info_dst.valid, color: GUI.col_button_ok))
								//{
								//	var rpc = new Wrench.Mode.Belts.ConfirmRPC()
								//	{

								//	};
								//	rpc.Send(ent_wrench);
								//}
							}
						}

						GUI.SameLine();

						using (var group = GUI.Group.New(size: new Vector2(GUI.GetRemainingWidth(), GUI.GetRemainingHeight()), padding: new(4)))
						{
							GUI.DrawBackground(GUI.tex_panel, group.GetOuterRect(), padding: new(8));

							ref var recipe = ref this.selected_recipe.GetRecipe();
							if (!recipe.IsNull() && recipe.placement.HasValue)
							{
								using (GUI.Group.New(size: new(GUI.GetRemainingWidth(), 28), padding: new(4, 2)))
								{
									GUI.TitleCentered(recipe.name, size: 24, pivot: new Vector2(0.00f, 0.50f));
								}

								GUI.SeparatorThick();

								using (GUI.Group.New(size: GUI.GetRemainingSpace(), padding: new(4, 6)))
								{
									using (GUI.Wrap.Push(GUI.GetRemainingWidth()))
									{
										GUI.TextShaded(recipe.desc, color: GUI.font_color_desc);

										GUI.NewLine();

										if (recipe.products[0].type == Crafting.Product.Type.Prefab && recipe.products[0].prefab.TryGetPrefab(out var prefab))
										{
											var root = prefab.Root;
											if (root != null)
											{
												if (root.TryGetComponentData<Belt.Data>(out var belt_data, initialized: true))
												{
													GUI.DrawStats(root, priority_min: Statistics.Priority.Low);

													//belt_data.speed_max
												}
											}
										}
									}
								}

								var placement = recipe.placement.Value;

								//GUI.LabelShaded("Distance:", distance, $"{{0:0.00}}/{placement.length_max:0.00} m");
							}
							//GUI.LabelShaded("Distance:", distance, "{0:0.00}m");
						}

						//if (info_src.valid)
						if (info_src.valid && info_dst.valid)
						{
							ref var recipe = ref this.selected_recipe.GetRecipe();
							if (!recipe.IsNull() && recipe.placement.HasValue)
							{
								var placement = recipe.placement.Value;

								//using (var hud = GUI.Window.Standalone("Wrench.HUD", position: (info_src.pos - new Vector2(0.00f, info_src.radius + 0.25f)).WorldToCanvas(), size: new(168, 100), pivot: new(0.50f, 1.00f)))
								//using (var hud = GUI.Window.Standalone("Wrench.HUD", position: (info_dst.pos - new Vector2(0.00f, info_dst.radius + 0.25f)).WorldToCanvas(), size: new(168, 100), pivot: new(0.50f, 1.00f)))
								using (var hud = GUI.Window.Standalone("Wrench.HUD", position: ((info_src.pos + info_dst.pos) * 0.50f).WorldToCanvas(), size: new(168, 100), pivot: new(0.50f, 0.50f)))
								{
									if (hud.show)
									{
										GUI.DrawBackground(GUI.tex_panel, hud.group.GetOuterRect(), padding: new(4));

										using (GUI.Group.New(size: GUI.GetRemainingSpace() - new Vector2(0, 48), padding: new(4)))
										{
											GUI.LabelShaded("Distance:", distance, $"{{0:0.00}}/{placement.length_max:0.00} m");

											if (GUI.Checkbox("Reversed", ref this.flags, Belt.Flags.Crossed, size: new Vector2(GUI.GetRemainingWidth(), 32)))
											{
												var rpc = new Wrench.Mode.Belts.EditRPC
												{
													flags = this.flags
												};
												rpc.Send(ent_wrench);
											}
										}

										using (GUI.Group.Centered(outer_size: GUI.GetRemainingSpace(), inner_size: new(100, 40)))
										{
											if (GUI.DrawButton("Create", new Vector2(100, 40), enabled: info_src.valid && info_dst.valid, error: (errors_src | errors_dst) != Build.Errors.None, color: GUI.col_button_ok))
											{
												var rpc = new Wrench.Mode.Belts.ConfirmRPC()
												{

												};
												rpc.Send(ent_wrench);
											}
											GUI.DrawHoverTooltip("Create a belt connection.");
										}
									}
								}
							}
						}
					}
#endif
				}

				public ref struct TargetInfo
				{
					public Entity entity;
					public ulong component_id;

					public ref Axle.Data axle;
					public ref Transform.Data transform;

					public float radius;
					public Vector2 pos;

					public bool is_src;
					public bool alive;
					public bool valid;

					public TargetInfo(Entity entity, bool is_src)
					{
						this.entity = entity;
						this.is_src = is_src;
						this.alive = this.entity.IsAlive();
						this.axle = ref Unsafe.NullRef<Axle.Data>();
						this.transform = ref Unsafe.NullRef<Transform.Data>();

						if (this.alive)
						{
							this.axle = ref this.entity.GetComponent<Axle.Data>();
							this.transform = ref this.entity.GetComponent<Transform.Data>();

							this.valid = !this.axle.IsNull() && !this.transform.IsNull();
							if (this.valid)
							{
								this.radius = this.is_src ? this.axle.radius_a : this.axle.radius_b;
								this.pos = this.transform.LocalToWorld(this.axle.offset);
							}
						}
					}
				}

				public static Build.Errors EvaluateBelt(ref Region.Data region, ref TargetInfo info, ref Crafting.Recipe recipe, IFaction.Handle faction_id = default)
				{
					var errors = Build.Errors.None;

					if (!recipe.IsNull() && recipe.type == Crafting.Recipe.Type.Wrench && recipe.tags.HasAny(Crafting.Recipe.Tags.Belt) && recipe.placement.HasValue && info.valid)
					{
						var placement = recipe.placement.Value;

						var claim_ratio = Claim.GetOverlapRatio(ref region, AABB.Circle(info.pos, 1.00f), faction_id: faction_id);
						errors.SetFlag(Build.Errors.Claimed, claim_ratio < placement.min_claim);
					}
					else
					{
						errors |= Build.Errors.Invalid;
					}

					return errors;
				}

				public static Build.Errors EvaluateBeltConnection(ref Region.Data region, ref TargetInfo info_src, ref TargetInfo info_dst, ref Crafting.Recipe recipe, out float distance, IFaction.Handle faction_id = default)
				{
					var errors = Build.Errors.None;
					distance = 0.00f;

					if (!recipe.IsNull() && recipe.type == Crafting.Recipe.Type.Wrench && recipe.tags.HasAny(Crafting.Recipe.Tags.Belt) && recipe.placement.HasValue && info_src.valid && info_dst.valid && info_src.entity != info_dst.entity)
					{
						var placement = recipe.placement.Value;

						var dir = (info_src.pos - info_dst.pos).GetNormalized(out distance);
						errors.SetFlag(Build.Errors.OutOfRange | Build.Errors.MaxLength, distance > placement.length_max);

						var claim_ratio = MathF.Min(Claim.GetOverlapRatio(ref region, AABB.Circle(info_src.pos, 1.00f), faction_id: faction_id), Claim.GetOverlapRatio(ref region, AABB.Circle(info_dst.pos, 1.00f), faction_id: faction_id));
						errors.SetFlag(Build.Errors.Claimed, claim_ratio < placement.min_claim);

						var pos_mid = (info_src.pos + info_dst.pos) * 0.50f;

						Span<OverlapResult> results = stackalloc OverlapResult[32];
						if (region.TryOverlapPointAll(pos_mid, 1.00f, ref results, mask: Physics.Layer.Belt))
						{
							foreach (ref var result in results)
							{
								ref var belt = ref result.entity.GetComponent<Belt.Data>();
								if (!belt.IsNull())
								{
									var ent_belt_src = belt.a.entity;
									var ent_belt_dst = belt.b.entity;

									if ((ent_belt_src == info_src.entity && ent_belt_dst == info_dst.entity) || (ent_belt_src == info_dst.entity && ent_belt_dst == info_src.entity))
									{
										errors.SetFlag(Build.Errors.Obstructed, true);
										break;
									}
								}
							}
						}
					}
					else
					{
						errors |= Build.Errors.Invalid;
					}

					return errors;
				}

				public struct SetTargetRPC: Net.IRPC<Wrench.Mode.Belts.Data>
				{
					public Entity ent_src;
					public Entity ent_dst;

					public ulong component_id_src;
					public ulong component_id_dst;

#if SERVER
					public void Invoke(ref NetConnection connection, Entity entity, ref Wrench.Mode.Belts.Data data)
					{
						App.WriteLine($"{this.ent_src} == {data.ent_src}; {this.ent_dst} == {data.ent_dst}");

						ref var region = ref entity.GetRegion();

						data.ent_src = this.ent_src;
						data.ent_dst = this.ent_dst;
						data.Sync(entity);
					}
#endif
				}

				public struct EditRPC: Net.IRPC<Wrench.Mode.Belts.Data>
				{
					public Crafting.Recipe.Handle? recipe;
					public Belt.Flags? flags;

#if SERVER
					public void Invoke(ref NetConnection connection, Entity entity, ref Wrench.Mode.Belts.Data data)
					{
						if (this.recipe.HasValue)
						{
							data.selected_recipe = this.recipe.Value;
						}

						if (this.flags.HasValue)
						{
							data.flags = this.flags.Value;
						}

						data.Sync(entity);
					}
#endif
				}

				public struct ConfirmRPC: Net.IRPC<Wrench.Mode.Belts.Data>
				{
#if SERVER
					public void Invoke(ref NetConnection connection, Entity entity, ref Wrench.Mode.Belts.Data data)
					{
						ref var region = ref entity.GetRegion();
						ref var player = ref connection.GetPlayer();
						ref var recipe = ref data.selected_recipe.GetRecipe();

						if (!region.IsNull() && !player.IsNull() && !recipe.IsNull())
						{
							var errors = Build.Errors.None;

							var info_src = new TargetInfo(data.ent_src, true);
							var info_dst = new TargetInfo(data.ent_dst, false);

							if (info_src.valid && info_dst.valid)
							{
								var pos_mid = (info_src.pos + info_dst.pos) * 0.50f;
				
								errors |= Wrench.Mode.Belts.EvaluateBeltConnection(ref region, ref info_src, ref info_dst, ref recipe, out _, player.faction_id);
								if (errors == Build.Errors.None)
								{
									var arg = (data.ent_src, data.ent_dst, data.flags);

									region.SpawnPrefab(recipe.products[0].prefab, pos_mid).ContinueWith(ent =>
									{
										ref var belt = ref ent.GetComponent<Belt.Data>();
										if (!belt.IsNull())
										{
											belt.a.Set(arg.ent_src);
											belt.b.Set(arg.ent_dst);

											belt.a_state.Set(arg.ent_src);
											belt.b_state.Set(arg.ent_dst);

											belt.flags = arg.flags;

											ent.MarkModified<Belt.Data>(sync: true);
										}
									});

									data.ent_src = default;
									data.ent_dst = default;
								}
							}

							data.Sync(entity);
						}
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

					public static Sprite Icon { get; } = new Sprite("ui_icons.wrench", 0, 1, 24, 24, 1, 0);

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
				if (GUI.DrawIconButton($"wrench.mode.{info.identifier}", T.Icon, new(64, 64)))
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
				var window_size = new Vector2(350, 400);

				using (var window = GUI.Window.Standalone("Wrench", size: window_size, padding: new(8, 8), pivot: new(0.50f, 0.50f)))
				{
					this.StoreCurrentWindowTypeID();
					if (window.show)
					{
						GUI.DrawWindowBackground(GUI.tex_window_menu, new Vector4(8, 8, 8, 8));

						ref var region = ref Client.GetRegion();

						using (GUI.Group.New(size: new(GUI.GetRemainingWidth(), 32)))
						{
							Wrench.DrawModeButton<Wrench.Mode.Belts.Data>(this.ent_wrench);
							GUI.SameLine();

							Wrench.DrawModeButton<Wrench.Mode.Ducts.Data>(this.ent_wrench);
							GUI.SameLine();
						}

						GUI.SeparatorThick();

						using (GUI.Group.New(size: GUI.GetRemainingSpace()))
						{
							GUI.Dock.New(Wrench.dock_identifier, size: GUI.GetRemainingSpace());
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

				using (var window = GUI.Window.Docked(info.identifier, dock_identifier: Wrench.dock_identifier, padding: new(0, 0)))
				{
					if (window.show)
					{
						//GUI.Title(info.identifier);

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
