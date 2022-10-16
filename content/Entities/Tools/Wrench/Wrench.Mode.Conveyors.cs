
using System.Diagnostics.CodeAnalysis;

namespace TC2.Base.Components
{
	public static partial class Wrench
	{
		public static partial class Mode
		{
			public static partial class Conveyors
			{
				[IComponent.Data(Net.SendType.Reliable)]
				public partial struct Data: IComponent, Wrench.IMode, Wrench.ILinkerMode<Conveyors.TargetInfo, Duct.Data>
				{
					[Save.Ignore] public Entity ent_src;
					[Save.Ignore] public Entity ent_dst;

					[Save.Ignore] public ulong inventory_id_src;
					[Save.Ignore] public ulong inventory_id_dst;

					public IRecipe.Handle selected_recipe;
					//public Belt.Flags flags;

					public static Sprite Icon { get; } = new Sprite("ui_icons.wrench", 0, 1, 24, 24, 1, 0);
					public Crafting.Recipe.Tags RecipeTags => Crafting.Recipe.Tags.Conveyor;
					public Physics.Layer LayerMask => Physics.Layer.Conveyor;

					[UnscopedRef] public ref Entity EntitySrc => ref this.ent_src;
					[UnscopedRef] public ref Entity EntityDst => ref this.ent_dst;
					[UnscopedRef] public ref IRecipe.Handle SelectedRecipe => ref this.selected_recipe;

					public TargetInfo CreateTargetInfo(Entity entity, bool is_src)
					{
						return new TargetInfo(entity, is_src);
					}

#if CLIENT
					public void SendSetTargetRPC(Entity ent_wrench, Entity ent_src, Entity ent_dst)
					{
						var rpc = new SetTargetRPC
						{
							ent_src = ent_src,
							ent_dst = ent_dst
						};
						rpc.Send(ent_wrench);
					}

					public void SendSetRecipeRPC(Entity ent_wrench, IRecipe.Handle recipe)
					{
						var rpc = new EditRPC
						{
							recipe = recipe
						};
						rpc.Send(ent_wrench);
					}

					public void DrawInfo(Entity ent_wrench, ref TargetInfo info_src, ref TargetInfo info_dst, Build.Errors errors_src, Build.Errors errors_dst, float distance)
					{
						ref var recipe = ref this.selected_recipe.GetData();
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
											if (root.TryGetComponentData<Duct.Data>(out var belt_data, initialized: true))
											{
												GUI.DrawStats(root, priority_min: Statistics.Priority.Low);
											}
										}
									}
								}
							}
						}
					}

					public void DrawHUD(Entity ent_wrench, ref TargetInfo info_src, ref TargetInfo info_dst, Build.Errors errors_src, Build.Errors errors_dst, float distance)
					{
						ref var recipe = ref this.selected_recipe.GetData();
						if (!recipe.IsNull() && recipe.placement.HasValue)
						{
							var placement = recipe.placement.Value;

							//using (var hud = GUI.Window.Standalone("Wrench.HUD", position: (info_src.pos - new Vector2(0.00f, info_src.radius + 0.25f)).WorldToCanvas(), size: new(168, 100), pivot: new(0.50f, 1.00f)))
							//using (var hud = GUI.Window.Standalone("Wrench.HUD", position: (info_dst.pos - new Vector2(0.00f, info_dst.radius + 0.25f)).WorldToCanvas(), size: new(168, 100), pivot: new(0.50f, 1.00f)))
							using (var hud = GUI.Window.Standalone("Wrench.HUD", position: ((info_src.pos + info_dst.pos) * 0.50f).WorldToCanvas(), size: new(300, 200), pivot: new(0.50f, 0.50f)))
							{
								if (hud.show)
								{
									GUI.DrawBackground(GUI.tex_window, hud.group.GetOuterRect(), padding: new(4));

									var inventories_src = info_src.entity.GetInventories();
									var inventories_dst = info_dst.entity.GetInventories();

									var sync = false;

									using (GUI.Group.New(size: GUI.GetRemainingSpace() - new Vector2(0, 48), padding: new(4)))
									{
										//GUI.LabelShaded("Distance:", distance, $"{{0:0.00}}/{placement.length_max:0.00} m");

										var w = GUI.GetRemainingWidth();

										using (GUI.ID.Push(this.ent_src))
										{
											using (GUI.Group.New(size: new Vector2(w * 0.50f, GUI.GetRemainingHeight()), padding: new(4)))
											{
												DrawInventories(ref inventories_src, ref this.inventory_id_src, ref sync);
											}
										}

										GUI.SameLine();

										using (GUI.ID.Push(this.ent_dst))
										{
											using (GUI.Group.New(size: new Vector2(w * 0.50f, GUI.GetRemainingHeight()), padding: new(4)))
											{
												DrawInventories(ref inventories_dst, ref this.inventory_id_dst, ref sync);
											}
										}
									}

									if (sync)
									{
										var rpc = new SetTargetRPC
										{
											ent_src = this.ent_src,
											ent_dst = this.ent_dst,

											component_id_src = this.inventory_id_src,
											component_id_dst = this.inventory_id_dst,
										};
										rpc.Send(ent_wrench);
									}

									using (GUI.Group.Centered(outer_size: GUI.GetRemainingSpace(), inner_size: new(100, 40)))
									{
										if (GUI.DrawButton("Create", new Vector2(100, 40), enabled: info_src.valid && info_dst.valid, error: (errors_src | errors_dst) != Build.Errors.None, color: GUI.col_button_ok))
										{
											var rpc = new Wrench.Mode.Conveyors.ConfirmRPC()
											{

											};
											rpc.Send(ent_wrench);
										}
										GUI.DrawHoverTooltip("Create a conveyor connection.");
									}
								}
							}
						}
					}

					private static void DrawInventories(scoped ref Inventory.Handle.List inventories, scoped ref ulong selected_inventory_id, scoped ref bool sync)
					{
						foreach (var h_inventory in inventories)
						{
							if (h_inventory.Flags.HasAny(Inventory.Flags.Allow_Ducts))
							{
								using (GUI.ID.Push(h_inventory.ID))
								{
									using (var group_row = GUI.Group.New(size: new Vector2(GUI.GetRemainingWidth(), 32), padding: new(4)))
									{
										GUI.DrawBackground(GUI.tex_panel, group_row.GetOuterRect(), new Vector4(4));

										GUI.TitleCentered(h_inventory.Name, pivot: new(0.50f, 0.50f));

										var is_selected = selected_inventory_id == h_inventory.ID;
										if (GUI.Selectable3(h_inventory.Name, group_row.GetOuterRect(), is_selected))
										{
											selected_inventory_id = is_selected ? 0 : h_inventory.ID;
											sync = true;
										}
									}
								}
							}
						}
					}

					void Wrench.ILinkerMode<Conveyors.TargetInfo, Duct.Data>.DrawGizmos(ref Vector2 wpos_mouse, ref TargetInfo info_src, ref TargetInfo info_dst, ref TargetInfo info_new, ref Color32BGRA color_src, ref Color32BGRA color_dst, ref Color32BGRA color_new)
					{
						if (info_src.IsValid)
						{
							if (!info_new.IsValid && !info_dst.IsValid)
							{
								var dir = (info_src.Position - wpos_mouse).GetNormalizedFast();

								GUI.DrawLine2((info_src.Position).WorldToCanvas(), (wpos_mouse).WorldToCanvas(), color_src, color_new.WithAlphaMult(0.00f), 8.00f, 8.00f);
							}

							if (info_new.IsValid)
							{
								var dir = (info_src.Position - info_new.Position).GetNormalizedFast();

								GUI.DrawLine2((info_src.Position).WorldToCanvas(), (info_new.Position).WorldToCanvas(), color_src, color_new.WithAlphaMult(0.50f), 8.00f, 8.00f);
							}

							if (info_dst.IsValid)
							{
								var dir = (info_src.Position - info_dst.Position).GetNormalizedFast();

								GUI.DrawLine2((info_src.Position).WorldToCanvas(), (info_dst.Position).WorldToCanvas(), color_src, color_dst, 8.00f, 8.00f);
							}
						}

						if (info_src.IsValid)
						{
							//GUI.DrawCircle(info_src.Position.WorldToCanvas(), info_src.Radius * GUI.GetWorldToCanvasScale(), color_src, 2.00f);
							GUI.DrawEntity(info_src.Entity, color_src.WithAlphaMult(0.50f));
						}

						if (info_dst.IsValid)
						{
							//GUI.DrawCircle(info_dst.Position.WorldToCanvas(), info_dst.Radius * GUI.GetWorldToCanvasScale(), color_dst, 2.00f);
							GUI.DrawEntity(info_dst.Entity, color_dst.WithAlphaMult(0.50f));
						}

						if (info_new.IsValid)
						{
							//GUI.DrawCircle(info_new.Position.WorldToCanvas(), info_new.Radius * GUI.GetWorldToCanvasScale(), color_new.WithAlphaMult(0.50f), 2.00f);
							GUI.DrawEntity(info_new.Entity, color_new.WithAlphaMult(0.50f));
						}
					}
#endif
				}

				public struct TargetInfo: ITargetInfo
				{
					public Entity entity;
					public ulong inventory_id;

					public Transform.Data transform;

					public float radius;
					public Vector2 pos;

					public bool is_src;
					public bool alive;
					public bool valid;

					public Entity Entity => this.entity;
					public ulong ComponentID => this.inventory_id;
					public Vector2 Position => this.pos;
					public float Radius => this.radius;
					public bool IsSource => this.is_src;
					public bool IsAlive => this.alive;
					public bool IsValid => this.valid;

					public TargetInfo(Entity entity, bool is_src)
					{
						this.entity = entity;
						this.is_src = is_src;
						this.alive = this.entity.IsAlive();

						if (this.alive)
						{
							this.valid = true;

							this.valid &= this.entity.GetComponent<Transform.Data>().TryGetValue(out this.transform);

							var has_inventory = false;

							var inventories = this.entity.GetInventories();
							foreach (var h_inventory in inventories)
							{
								if (h_inventory.Flags.HasAny(Inventory.Flags.Allow_Ducts))
								{
									has_inventory = true;
									break;
								}
							}

							valid &= has_inventory;

							if (this.valid)
							{
								this.radius = 1.00f;
								this.pos = this.transform.position;
							}
						}
					}
				}

				public struct SetTargetRPC: Net.IRPC<Wrench.Mode.Conveyors.Data>
				{
					public Entity ent_src;
					public Entity ent_dst;

					public ulong component_id_src;
					public ulong component_id_dst;

#if SERVER
					public void Invoke(ref NetConnection connection, Entity entity, ref Wrench.Mode.Conveyors.Data data)
					{
						App.WriteLine($"{this.ent_src} == {data.ent_src}; {this.ent_dst} == {data.ent_dst}");

						ref var region = ref entity.GetRegion();

						data.ent_src = this.ent_src;
						data.ent_dst = this.ent_dst;

						data.inventory_id_src = this.component_id_src;
						data.inventory_id_dst = this.component_id_dst;

						data.Sync(entity);
					}
#endif
				}

				public struct EditRPC: Net.IRPC<Wrench.Mode.Conveyors.Data>
				{
					public IRecipe.Handle? recipe;

					public float? throughput_min;
					public float? throughput_max;
					public float? interval;

					public Material.Flags? filter_flags;
					public Material.Type? filter_type;
					public Material.Handle? filter_material;


#if SERVER
					public void Invoke(ref NetConnection connection, Entity entity, ref Wrench.Mode.Conveyors.Data data)
					{
						if (this.recipe.HasValue)
						{
							data.selected_recipe = this.recipe.Value;
						}

						data.Sync(entity);
					}
#endif
				}

				public struct ConfirmRPC: Net.IRPC<Wrench.Mode.Conveyors.Data>
				{
#if SERVER
					public void Invoke(ref NetConnection connection, Entity entity, ref Wrench.Mode.Conveyors.Data data)
					{
						ref var region = ref entity.GetRegion();
						ref var player = ref connection.GetPlayer();
						ref var recipe = ref data.selected_recipe.GetData();

						if (!region.IsNull() && !player.IsNull() && !recipe.IsNull())
						{
							var errors = Build.Errors.None;

							var info_src = new TargetInfo(data.ent_src, true);
							var info_dst = new TargetInfo(data.ent_dst, false);

							if (info_src.valid && info_dst.valid)
							{
								var pos_mid = (info_src.pos + info_dst.pos) * 0.50f;
				
								errors |= data.EvaluateNodePair<Wrench.Mode.Conveyors.Data, Wrench.Mode.Conveyors.TargetInfo, Duct.Data>(ref region, ref info_src, ref info_dst, ref recipe, out _, player.faction_id);
								if (errors == Build.Errors.None)
								{
									var arg = (data.ent_src, data.ent_dst, data.inventory_id_src, data.inventory_id_dst);

									region.SpawnPrefab(recipe.products[0].prefab, pos_mid).ContinueWith(ent =>
									{
										ref var duct = ref ent.GetComponent<Duct.Data>();
										if (!duct.IsNull())
										{
											duct.a.Set(arg.ent_src, arg.inventory_id_src);
											duct.b.Set(arg.ent_dst, arg.inventory_id_dst);

											ent.MarkModified<Duct.Data>(sync: true);

											//belt.a.Set(arg.ent_src);
											//belt.b.Set(arg.ent_dst);

											//belt.a_state.Set(arg.ent_src);
											//belt.b_state.Set(arg.ent_dst);

											//belt.flags = arg.flags;

											//ent.MarkModified<Duct.Data>(sync: true);
										}

										//ent.AddRel2<Duct.Link>(arg.ent_src, default, false, false, false);
										//ent.AddRel2<Duct.Link>(arg.ent_dst, default, false, false, false);
									});

									data.ent_src = default;
									data.ent_dst = default;

									data.inventory_id_src = default;
									data.inventory_id_dst = default;
								}
							}

							data.Sync(entity);
						}
					}
#endif
				}
			}
		}
	}
}
