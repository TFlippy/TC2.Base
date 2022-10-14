
using System.Diagnostics.CodeAnalysis;

namespace TC2.Base.Components
{
	public static partial class Wrench
	{
		public static partial class Mode
		{
			public static partial class Belts
			{
				[IComponent.Data(Net.SendType.Reliable)]
				public partial struct Data: IComponent, Wrench.IMode, Wrench.ILinkerMode<Belts.TargetInfo, Belt.Data>
				{
					[Save.Ignore] public Entity ent_src;
					[Save.Ignore] public Entity ent_dst;

					public IRecipe.Handle selected_recipe;
					public Belt.Flags flags;

					public static Sprite Icon { get; } = new Sprite("ui_icons.wrench", 0, 1, 24, 24, 0, 0);
					public Crafting.Recipe.Tags RecipeTags => Crafting.Recipe.Tags.Belt;
					public Physics.Layer LayerMask => Physics.Layer.Belt;

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
						var rpc = new Wrench.Mode.Belts.SetTargetRPC()
						{
							ent_src = ent_src,
							ent_dst = ent_dst
						};
						rpc.Send(ent_wrench);
					}

					public void SendSetRecipeRPC(Entity ent_wrench, IRecipe.Handle recipe)
					{
						var rpc = new Wrench.Mode.Belts.EditRPC
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
											if (root.TryGetComponentData<Belt.Data>(out var belt_data, initialized: true))
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

					public Entity Entity => this.entity;
					public ulong ComponentID => ECS.GetID<Belt.Data>();
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
					public IRecipe.Handle? recipe;
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
						ref var recipe = ref data.selected_recipe.GetData();

						if (!region.IsNull() && !player.IsNull() && !recipe.IsNull())
						{
							var errors = Build.Errors.None;

							var info_src = new TargetInfo(data.ent_src, true);
							var info_dst = new TargetInfo(data.ent_dst, false);

							if (info_src.valid && info_dst.valid)
							{
								var pos_mid = (info_src.pos + info_dst.pos) * 0.50f;
				
								errors |= data.EvaluateNodePair<Wrench.Mode.Belts.Data, Wrench.Mode.Belts.TargetInfo, Belt.Data>(ref region, ref info_src, ref info_dst, ref recipe, out _, player.faction_id);
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
		}
	}
}
