
using System.Diagnostics.CodeAnalysis;

namespace TC2.Base.Components
{
	public static partial class Wrench
	{
		public static partial class Mode
		{
			public static partial class Ducts
			{
				[IComponent.Data(Net.SendType.Reliable)]
				public partial struct Data: IComponent, Wrench.IConnectable<Ducts.TargetInfo, Duct.Data>
				{
					[Save.Ignore] public Entity ent_src;
					[Save.Ignore] public Entity ent_dst;

					public Crafting.Recipe.Handle selected_recipe;
					//public Belt.Flags flags;

					public static Sprite Icon { get; } = new Sprite("ui_icons.wrench", 0, 1, 24, 24, 1, 0);
					public Crafting.Recipe.Tags RecipeTags => Crafting.Recipe.Tags.Duct;
					public Physics.Layer LayerMask => Physics.Layer.Duct;

					[UnscopedRef] public ref Entity EntitySrc => ref this.ent_src;
					[UnscopedRef] public ref Entity EntityDst => ref this.ent_dst;
					[UnscopedRef] public ref Crafting.Recipe.Handle SelectedRecipe => ref this.selected_recipe;

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

					public void SendSetRecipeRPC(Entity ent_wrench, Crafting.Recipe.Handle recipe)
					{
						var rpc = new EditRPC
						{
							recipe = recipe
						};
						rpc.Send(ent_wrench);
					}

					public void DrawInfo(Entity ent_wrench, ref TargetInfo info_src, ref TargetInfo info_dst, Build.Errors errors_src, Build.Errors errors_dst, float distance)
					{
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
									}

									using (GUI.Group.Centered(outer_size: GUI.GetRemainingSpace(), inner_size: new(100, 40)))
									{
										if (GUI.DrawButton("Create", new Vector2(100, 40), enabled: info_src.valid && info_dst.valid, error: (errors_src | errors_dst) != Build.Errors.None, color: GUI.col_button_ok))
										{
											var rpc = new Wrench.Mode.Ducts.ConfirmRPC()
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
#endif
				}

				public struct TargetInfo: ITargetInfo
				{
					public Entity entity;

					public Axle.Data axle;
					public Transform.Data transform;

					public float radius;
					public Vector2 pos;

					public bool is_src;
					public bool alive;
					public bool valid;

					Entity ITargetInfo.Entity => this.entity;
					Vector2 ITargetInfo.Position => this.pos;
					float ITargetInfo.Radius => this.radius;

					bool ITargetInfo.IsSource => this.is_src;
					bool ITargetInfo.IsAlive => this.alive;
					bool ITargetInfo.IsValid => this.valid;

					public TargetInfo(Entity entity, bool is_src)
					{
						this.entity = entity;
						this.is_src = is_src;
						this.alive = this.entity.IsAlive();

						if (this.alive)
						{
							this.valid = true;

							this.valid &= this.entity.GetComponent<Axle.Data>().TryGetValue(out this.axle);
							this.valid &= this.entity.GetComponent<Transform.Data>().TryGetValue(out this.transform);

							if (this.valid)
							{
								this.radius = this.is_src ? this.axle.radius_a : this.axle.radius_b;
								this.pos = this.transform.LocalToWorld(this.axle.offset);
							}
						}
					}
				}

				public struct SetTargetRPC: Net.IRPC<Wrench.Mode.Ducts.Data>
				{
					public Entity ent_src;
					public Entity ent_dst;

					public ulong component_id_src;
					public ulong component_id_dst;

#if SERVER
					public void Invoke(ref NetConnection connection, Entity entity, ref Wrench.Mode.Ducts.Data data)
					{
						App.WriteLine($"{this.ent_src} == {data.ent_src}; {this.ent_dst} == {data.ent_dst}");

						ref var region = ref entity.GetRegion();

						data.ent_src = this.ent_src;
						data.ent_dst = this.ent_dst;
						data.Sync(entity);
					}
#endif
				}

				public struct EditRPC: Net.IRPC<Wrench.Mode.Ducts.Data>
				{
					public Crafting.Recipe.Handle? recipe;

					public float? throughput_min;
					public float? throughput_max;
					public float? interval;

					public Material.Flags? filter_flags;
					public Material.Type? filter_type;
					public Material.Handle? filter_material;


#if SERVER
					public void Invoke(ref NetConnection connection, Entity entity, ref Wrench.Mode.Ducts.Data data)
					{
						if (this.recipe.HasValue)
						{
							data.selected_recipe = this.recipe.Value;
						}

						data.Sync(entity);
					}
#endif
				}

				public struct ConfirmRPC: Net.IRPC<Wrench.Mode.Ducts.Data>
				{
#if SERVER
					public void Invoke(ref NetConnection connection, Entity entity, ref Wrench.Mode.Ducts.Data data)
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
				
								errors |= data.EvaluateNodePair<Wrench.Mode.Ducts.Data, Wrench.Mode.Ducts.TargetInfo, Duct.Data>(ref region, ref info_src, ref info_dst, ref recipe, out _, player.faction_id);
								if (errors == Build.Errors.None)
								{
									var arg = (data.ent_src, data.ent_dst);

									region.SpawnPrefab(recipe.products[0].prefab, pos_mid).ContinueWith(ent =>
									{
										ref var duct = ref ent.GetComponent<Duct.Data>();
										if (!duct.IsNull())
										{
											//belt.a.Set(arg.ent_src);
											//belt.b.Set(arg.ent_dst);

											//belt.a_state.Set(arg.ent_src);
											//belt.b_state.Set(arg.ent_dst);

											//belt.flags = arg.flags;

											//ent.MarkModified<Duct.Data>(sync: true);
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
		}
	}
}
