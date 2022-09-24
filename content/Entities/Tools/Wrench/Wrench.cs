
using System.Diagnostics.CodeAnalysis;

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
				public partial struct Data: IComponent, Wrench.IConnectable<Belts.TargetInfo, Belt.Data>
				{
					[Save.Ignore] public Entity ent_src;
					[Save.Ignore] public Entity ent_dst;

					public Crafting.Recipe.Handle selected_recipe;
					public Belt.Flags flags;

					public static Sprite Icon { get; } = new Sprite("ui_icons.wrench", 0, 1, 24, 24, 0, 0);
					public Crafting.Recipe.Tags RecipeTags => Crafting.Recipe.Tags.Belt;
					public Physics.Layer LayerMask => Physics.Layer.Belt;

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
						var rpc = new Wrench.Mode.Belts.SetTargetRPC()
						{
							ent_src = ent_src,
							ent_dst = ent_dst
						};
						rpc.Send(ent_wrench);
					}

					public void SendSetRecipeRPC(Entity ent_wrench, Crafting.Recipe.Handle recipe)
					{
						var rpc = new Wrench.Mode.Belts.EditRPC
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

			public static partial class Ducts
			{
				[IComponent.Data(Net.SendType.Reliable)]
				public partial struct Data: IComponent, Wrench.IMode
				{
					[Save.Ignore] public Entity ent_src;
					[Save.Ignore] public Entity ent_dst;

					public static Sprite Icon { get; } = new Sprite("ui_icons.wrench", 0, 1, 24, 24, 1, 0);
					public Crafting.Recipe.Tags RecipeTags => Crafting.Recipe.Tags.Duct;

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
			public Crafting.Recipe.Tags RecipeTags { get; }

			public bool IsRecipeValid(ref Region.Data region, ref Crafting.Recipe recipe)
			{
				return !recipe.IsNull() && recipe.type == Crafting.Recipe.Type.Wrench && recipe.tags.HasAny(this.RecipeTags) && recipe.placement.HasValue;
			}

#if CLIENT
			public void Draw(Entity ent_wrench, ref Wrench.Data wrench);
#endif
		}

		public interface ITargetInfo
		{
			public Entity Entity { get; }
			public Vector2 Position { get; }
			public float Radius { get; }

			public bool IsSource { get; }
			public bool IsAlive { get; }
			public bool IsValid { get; }
		}

		// TODO: shithack, can't access interface methods on structs without boxing
		public static Build.Errors EvaluateNode<T, TInfo, TLink>(ref this T self, ref Region.Data region, ref TInfo info, ref Crafting.Recipe recipe, IFaction.Handle faction_id = default) where T : unmanaged, IConnectable<TInfo, TLink> where TInfo : unmanaged, ITargetInfo where TLink : unmanaged, IComponent, ILink
		{
			return self.EvaluateNode(ref region, ref info, ref recipe, faction_id: faction_id);
		}

		// TODO: shithack, can't access interface methods on structs without boxing
		public static Build.Errors EvaluateNodePair<T, TInfo, TLink>(ref this T self, ref Region.Data region, ref TInfo info_src, ref TInfo info_dst, ref Crafting.Recipe recipe, out float distance, IFaction.Handle faction_id = default) where T: unmanaged, IConnectable<TInfo, TLink> where TInfo : unmanaged, ITargetInfo where TLink : unmanaged, IComponent, ILink
		{
			return self.EvaluateNodePair(ref region, ref info_src, ref info_dst, ref recipe, out distance, faction_id: faction_id);
		}

		public interface IConnectable<TInfo, TLink>: IMode where TInfo : unmanaged, ITargetInfo where TLink : unmanaged, IComponent, ILink
		{
			public ref Entity EntitySrc { get; }
			public ref Entity EntityDst { get; }
			public ref Crafting.Recipe.Handle SelectedRecipe { get; }

			public Physics.Layer LayerMask { get; }
			public TInfo CreateTargetInfo(Entity entity, bool is_src);

#if CLIENT
			public void SendSetTargetRPC(Entity ent_wrench, Entity ent_src, Entity ent_dst);
			public void SendSetRecipeRPC(Entity ent_wrench, Crafting.Recipe.Handle recipe);

			public void DrawInfo(Entity ent_wrench, ref TInfo info_src, ref TInfo info_dst, Build.Errors errors_src, Build.Errors errors_dst, float distance);
			public void DrawHUD(Entity ent_wrench, ref TInfo info_src, ref TInfo info_dst, Build.Errors errors_src, Build.Errors errors_dst, float distance);
#endif
			public Build.Errors EvaluateNode(ref Region.Data region, ref TInfo info, ref Crafting.Recipe recipe, IFaction.Handle faction_id = default)
			{
				var errors = Build.Errors.None;

				if (this.IsRecipeValid(ref region, ref recipe) && info.IsValid)
				{
					var placement = recipe.placement.Value;

					var claim_ratio = Claim.GetOverlapRatio(ref region, AABB.Circle(info.Position, 1.00f), faction_id: faction_id);
					errors.SetFlag(Build.Errors.Claimed, claim_ratio < placement.min_claim);
				}
				else
				{
					errors |= Build.Errors.Invalid;
				}

				return errors;
			}

			[MethodImpl(MethodImplOptions.NoInlining)]
			public Build.Errors EvaluateNodePair(ref Region.Data region, ref TInfo info_src, ref TInfo info_dst, ref Crafting.Recipe recipe, out float distance, IFaction.Handle faction_id = default)
			{
				var errors = Build.Errors.None;
				distance = 0.00f;

				if (this.IsRecipeValid(ref region, ref recipe) && info_src.IsValid && info_dst.IsValid && info_src.Entity != info_dst.Entity)
				{
					var placement = recipe.placement.Value;

					var dir = (info_src.Position - info_dst.Position).GetNormalized(out distance);
					errors.SetFlag(Build.Errors.OutOfRange | Build.Errors.MaxLength, distance > placement.length_max);

					var claim_ratio = MathF.Min(Claim.GetOverlapRatio(ref region, AABB.Circle(info_src.Position, 1.00f), faction_id: faction_id), Claim.GetOverlapRatio(ref region, AABB.Circle(info_dst.Position, 1.00f), faction_id: faction_id));
					errors.SetFlag(Build.Errors.Claimed, claim_ratio < placement.min_claim);

					var pos_mid = (info_src.Position + info_dst.Position) * 0.50f;

					Span<OverlapResult> results = stackalloc OverlapResult[32];
					if (region.TryOverlapPointAll(pos_mid, 1.00f, ref results, mask: this.LayerMask))
					{
						foreach (ref var result in results)
						{
							ref var link = ref result.entity.GetComponent<TLink>();
							if (!link.IsNull())
							{
								var ent_link_src = link.EntityA;
								var ent_link_dst = link.EntityB;

								if ((ent_link_src == info_src.Entity && ent_link_dst == info_dst.Entity) || (ent_link_src == info_dst.Entity && ent_link_dst == info_src.Entity))
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

#if SERVER
			//public static void CreateConnection(ref Region.Data region, Entity entity)
			//{
			//	ref var region = ref entity.GetRegion();
			//	ref var player = ref connection.GetPlayer();
			//	ref var recipe = ref data.selected_recipe.GetRecipe();

			//	if (!region.IsNull() && !player.IsNull() && !recipe.IsNull())
			//	{
			//		var errors = Build.Errors.None;

			//		var info_src = new TargetInfo(data.ent_src, true);
			//		var info_dst = new TargetInfo(data.ent_dst, false);

			//		if (info_src.valid && info_dst.valid)
			//		{
			//			var pos_mid = (info_src.pos + info_dst.pos) * 0.50f;

			//			errors |= data.EvaluateNodePair(ref region, ref info_src, ref info_dst, ref recipe, out _, player.faction_id);
			//			if (errors == Build.Errors.None)
			//			{
			//				var arg = (data.ent_src, data.ent_dst, data.flags);

			//				region.SpawnPrefab(recipe.products[0].prefab, pos_mid).ContinueWith(ent =>
			//				{
			//					ref var belt = ref ent.GetComponent<Belt.Data>();
			//					if (!belt.IsNull())
			//					{
			//						belt.a.Set(arg.ent_src);
			//						belt.b.Set(arg.ent_dst);

			//						belt.a_state.Set(arg.ent_src);
			//						belt.b_state.Set(arg.ent_dst);

			//						belt.flags = arg.flags;

			//						ent.MarkModified<Belt.Data>(sync: true);
			//					}
			//				});

			//				data.ent_src = default;
			//				data.ent_dst = default;
			//			}
			//		}

			//		data.Sync(entity);
			//	}
			//}
#endif

#if CLIENT
			void IMode.Draw(Entity ent_wrench, ref Wrench.Data wrench)
			{
				ref var player = ref Client.GetPlayer();
				ref var region = ref Client.GetRegion();

				var faction_id = player.faction_id;

				ref readonly var kb = ref Control.GetKeyboard();
				ref readonly var mouse = ref Control.GetMouse();

				var wpos_mouse = mouse.GetInterpolatedPosition();
				var cpos_mouse = GUI.WorldToCanvas(wpos_mouse);

				var scale = GUI.GetWorldToCanvasScale();

				var info_src = this.CreateTargetInfo(this.EntitySrc, true);
				var info_dst = this.CreateTargetInfo(this.EntityDst, false);
				var info_new = default(TInfo);

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
							if (result.entity == info_src.Entity || result.entity == info_dst.Entity) continue;

							info_new = this.CreateTargetInfo(result.entity, !info_src.IsValid);
							if (info_new.IsValid)
							{
								break;
							}
						}
					}
				}

				if (info_new.IsValid)
				{
					GUI.SetCursor(App.CursorType.Hand, 10);
				}

				var distance = 0.00f;
				if (info_src.IsValid)
				{
					if (info_dst.IsValid)
					{
						distance = Vector2.Distance(info_src.Position, info_dst.Position);
					}
					else if (info_new.IsValid)
					{
						distance = Vector2.Distance(info_src.Position, info_new.Position);
					}
					else
					{
						distance = Vector2.Distance(info_src.Position, mouse.position);
					}
				}

				var color_src = GUI.font_color_success;
				var color_dst = GUI.font_color_success;
				var color_new = GUI.font_color_yellow;

				{
					ref var recipe = ref this.SelectedRecipe.GetRecipe();
					if (!recipe.IsNull())
					{
						if (info_src.IsValid)
						{
							errors_src |= this.EvaluateNode(ref region, ref info_src, ref recipe, faction_id: faction_id);
							if (!info_new.IsValid)
							{
								errors_new.SetFlag(Build.Errors.MaxLength | Build.Errors.OutOfRange, Vector2.Distance(info_src.Position, wpos_mouse) > recipe.placement.Value.length_max);
							}
						}

						if (info_dst.IsValid)
						{
							errors_dst |= this.EvaluateNode(ref region, ref info_dst, ref recipe, faction_id: faction_id);
							if (info_src.IsValid)
							{
								errors_dst |= this.EvaluateNodePair(ref region, ref info_src, ref info_dst, ref recipe, out _, player.faction_id);
							}
						}

						if (info_new.IsValid)
						{
							errors_new |= this.EvaluateNode(ref region, ref info_new, ref recipe, faction_id: faction_id);
							if (info_src.IsValid)
							{
								errors_new |= this.EvaluateNodePair(ref region, ref info_src, ref info_new, ref recipe, out _, player.faction_id);
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

				if (info_src.IsValid)
				{
					if (!info_new.IsValid && !info_dst.IsValid)
					{
						var dir = (info_src.Position - wpos_mouse).GetNormalizedFast();
						var n = new Vector2(-dir.Y, dir.X);
						var offset_src = n * info_src.Radius;
						var offset_mouse = n * info_src.Radius * 0.50f;

						GUI.DrawLine2((info_src.Position + offset_src).WorldToCanvas(), (wpos_mouse + offset_mouse).WorldToCanvas(), color_src, color_new.WithAlphaMult(0.00f), 2.00f, 2.00f);
						GUI.DrawLine2((info_src.Position - offset_src).WorldToCanvas(), (wpos_mouse - offset_mouse).WorldToCanvas(), color_src, color_new.WithAlphaMult(0.00f), 2.00f, 2.00f);
					}

					if (info_new.IsValid)
					{
						var dir = (info_src.Position - info_new.Position).GetNormalizedFast();
						var n = new Vector2(-dir.Y, dir.X);
						var offset_src = n * info_src.Radius;
						var offset_new = n * info_new.Radius;

						GUI.DrawLine2((info_src.Position + offset_src).WorldToCanvas(), (info_new.Position + offset_new).WorldToCanvas(), color_src, color_new.WithAlphaMult(0.50f), 2.00f, 2.00f);
						GUI.DrawLine2((info_src.Position - offset_src).WorldToCanvas(), (info_new.Position - offset_new).WorldToCanvas(), color_src, color_new.WithAlphaMult(0.50f), 2.00f, 2.00f);
					}

					if (info_dst.IsValid)
					{
						var dir = (info_src.Position - info_dst.Position).GetNormalizedFast();
						var n = new Vector2(-dir.Y, dir.X);
						var offset_src = n * info_src.Radius;
						var offset_dst = n * info_dst.Radius;

						GUI.DrawLine2((info_src.Position + offset_src).WorldToCanvas(), (info_dst.Position + offset_dst).WorldToCanvas(), color_src, color_dst, 2.00f, 2.00f);
						GUI.DrawLine2((info_src.Position - offset_src).WorldToCanvas(), (info_dst.Position - offset_dst).WorldToCanvas(), color_src, color_dst, 2.00f, 2.00f);
					}
				}

				if (info_src.IsValid)
				{
					GUI.DrawCircle(info_src.Position.WorldToCanvas(), info_src.Radius * scale, color_src, 2.00f);
				}

				if (info_dst.IsValid)
				{
					GUI.DrawCircle(info_dst.Position.WorldToCanvas(), info_dst.Radius * scale, color_dst, 2.00f);
				}

				if (info_new.IsValid)
				{
					GUI.DrawCircle(info_new.Position.WorldToCanvas(), info_new.Radius * scale, color_new.WithAlphaMult(0.50f), 2.00f);
				}

				if (mouse.GetKeyDown(Mouse.Key.Left))
				{
					if (info_new.IsValid)
					{
						if (errors_new == Build.Errors.None)
						{
							this.SendSetTargetRPC(ent_wrench, ent_src: info_new.IsSource ? info_new.Entity : this.EntitySrc, ent_dst: !info_new.IsSource ? info_new.Entity : this.EntityDst);
							Sound.PlayGUI(GUI.sound_select, volume: 0.07f, pitch: info_new.IsSource ? 0.80f : 0.95f);
						}
						else
						{
							Sound.PlayGUI(GUI.sound_error, 0.50f);
						}
					}
				}
				else if (mouse.GetKeyDown(Mouse.Key.Right))
				{
					if (info_src.IsValid || info_dst.IsValid)
					{
						this.SendSetTargetRPC(ent_wrench, ent_src: info_dst.IsValid ? info_src.Entity : default, ent_dst: default);
						Sound.PlayGUI(GUI.sound_select, volume: 0.07f, pitch: 0.80f);
					}
				}

				using (GUI.Group.New(size: new Vector2(48 + 32 - 5, GUI.GetRemainingHeight())))
				{
					using (var scrollbox = GUI.Scrollbox.New("wrench.recipes", GUI.GetRemainingSpace(), padding: new Vector2(4, 4)))
					{
						GUI.DrawBackground(GUI.tex_window, scrollbox.group_frame.GetInnerRect(), padding: new(8));

						var recipes = Shop.GetAllRecipes();
						foreach (ref var recipe in recipes)
						{
							if (recipe.type == Crafting.Recipe.Type.Wrench && recipe.tags.HasAll(this.RecipeTags))
							{
								using (GUI.ID.Push(recipe.id))
								{
									using (GUI.Group.New(size: new Vector2(GUI.GetRemainingWidth(), 48)))
									{
										var frame_size = new Vector2(48, 48);
										var selected = this.SelectedRecipe.id == recipe.id;
										using (var button = GUI.CustomButton.New(recipe.name, frame_size, sound: GUI.sound_select, sound_volume: 0.10f))
										{
											GUI.Draw9Slice((selected || button.hovered) ? GUI.tex_slot_simple_hover : GUI.tex_slot_simple, new Vector4(4), button.bb);
											GUI.DrawSpriteCentered(recipe.icon, button.bb, scale: 2.00f);

											if (button.pressed)
											{
												this.SendSetRecipeRPC(ent_wrench, new Crafting.Recipe.Handle(recipe.id));
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
						}
					}
				}

				GUI.SameLine();

				using (var group = GUI.Group.New(size: new Vector2(GUI.GetRemainingWidth(), GUI.GetRemainingHeight()), padding: new(4)))
				{
					GUI.DrawBackground(GUI.tex_panel, group.GetOuterRect(), padding: new(8));
					this.DrawInfo(ent_wrench, ref info_src, ref info_dst, errors_src, errors_dst, distance);
				}

				if (info_src.IsValid && info_dst.IsValid)
				{
					this.DrawHUD(ent_wrench, ref info_src, ref info_dst, errors_src, errors_dst, distance);
				}
			}
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
