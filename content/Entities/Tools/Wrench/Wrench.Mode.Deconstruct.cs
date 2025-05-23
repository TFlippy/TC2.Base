﻿
using System.Diagnostics.CodeAnalysis;

namespace TC2.Base.Components
{
	public static partial class Wrench
	{
		public static partial class Mode
		{
			public static partial class Deconstruct
			{
				[Flags]
				public enum Flags: uint
				{
					None = 0,

					[Asset.Ignore] Active = 1 << 0,
				}

				[Flags]
				public enum Filter: uint
				{
					None = 0,

					[Name("Buildings")]
					Buildings = 1u << 0,

					[Name("Belts")]
					Belts = 1u << 1,

					[Name("Conveyors")]
					Conveyors = 1u << 2,

					[Name("Ladders")]
					Ladders = 1u << 3,

					[Name("Doors")]
					Doors = 1u << 4,
				}

				[IComponent.Data(Net.SendType.Reliable, IComponent.Scope.Region, name: "Wrench (Demolish)", order: 8)]
				public partial struct Data: IComponent, Wrench.IMode, Wrench.ITargeterMode<TargetInfo>
				{
					public static readonly Sound.Handle sound_dismantle_default = "wrench.dismantle.00";

					[Asset.Ignore] public EntRef<Dismantlable.Data> ref_dismantlable;
					public Deconstruct.Flags flags;
					public Deconstruct.Filter filter = Filter.Buildings | Filter.Belts | Filter.Conveyors | Filter.Ladders | Filter.Doors;
					public Sound.Handle sound_dismantle = sound_dismantle_default;

					//[Save.Ignore, Net.Ignore]
					//public Vector2 target_pos;

					public Entity EntTarget => this.ref_dismantlable.entity;

					public TargetInfo CreateTargetInfo(Entity entity)
					{
						return new TargetInfo(entity);
					}

					public static Sprite Icon { get; } = new Sprite("ui_icons.wrench", 24, 24, 2, 0);
					public static string Name { get; } = "Demolish";

					public Recipe.Type RecipeType => Recipe.Type.Undefined;
					public Physics.Layer LayerMask
					{
						get
						{
							var mask = Physics.Layer.None;

							if (this.filter.HasAny(Deconstruct.Filter.Buildings))
							{
								mask |= Physics.Layer.Building;
							}

							if (this.filter.HasAny(Deconstruct.Filter.Belts))
							{
								mask |= Physics.Layer.Belt;
							}

							if (this.filter.HasAny(Deconstruct.Filter.Conveyors))
							{
								mask |= Physics.Layer.Conveyor;
							}

							if (this.filter.HasAny(Deconstruct.Filter.Ladders))
							{
								mask |= Physics.Layer.Climbable;
							}

							if (this.filter.HasAny(Deconstruct.Filter.Doors))
							{
								mask |= Physics.Layer.Door;
							}

							return mask;
						}
					}
					public Physics.Layer LayerRequire => Physics.Layer.Dismantlable;
					public Physics.Layer LayerExclude => Physics.Layer.None;

					public Color32BGRA ColorOk => Color32BGRA.Orange;
					public Color32BGRA ColorError => Color32BGRA.Grey;
					public Color32BGRA ColorNew => Color32BGRA.White;

					public Data()
					{

					}

#if CLIENT
					public void SendSetTargetRPC(Entity ent_wrench, Entity ent_target)
					{
						var rpc = new SetTargetRPC
						{
							ent_target = ent_target
						};
						rpc.Send(ent_wrench);
					}

					public void DrawInfo(Entity ent_wrench, ref TargetInfo info)
					{
						var dirty = false;
						var rpc = new ConfigureRPC
						{

						};

						var filter_tmp = this.filter;
						using (GUI.Group.New(size: new Vector2(GUI.RmX, 40), padding: new(0, 0)))
						{
							if (GUI.EnumInput("filter", ref filter_tmp, new Vector2(GUI.RmX - 80, 40), show_label: false))
							{
								rpc.filter = filter_tmp;
								dirty = true;
							}

							GUI.SameLine();

							GUI.TitleCentered("Filter", size: 24, pivot: new(0.50f, 0.50f));
						}

						GUI.SeparatorThick();

						using (GUI.Group.New(size: new Vector2(GUI.RmX, 32), padding: new(8, 4)))
						{
							GUI.TitleCentered(info.alive ? info.entity.GetPrefabName() : "<no target selected>", size: 32, pivot: new(0.00f, 0.50f));
						}

						GUI.SeparatorThick();

						using (var group = GUI.Group.New(size: new Vector2(GUI.RmX, GUI.RmY - 40), padding: new(8, 8)))
						{
							GUI.DrawBackground(GUI.tex_panel, group.GetOuterRect(), new(4));

							GUI.Title("Retrievable resources:", size: 16);
							//GUI.SeparatorThick();

							var items_span = info.dismantlable.items.AsSpan();

							for (var i = 0; i < items_span.Length; i++)
							{
								ref var item = ref items_span[i];
								switch (item.type)
								{
									case Shipment.Item.Type.Resource:
									{
										var resource = new Resource.Data(item.material, item.quantity);
										GUI.DrawResource(ref resource, new Vector2(GUI.RmX, 32), GUI.font_color_green);
									}
									break;
								}
							}
						}

						using (GUI.Group.New(size: GUI.Rm, padding: new(0, 0)))
						{
							using (var group = GUI.Group.New(size: new Vector2(GUI.RmX - 120, GUI.RmY), padding: new(8, 8)))
							{
								GUI.TitleCentered($"{info.dismantlable.current_work:0.00}/{info.dismantlable.required_work:0.00}", size: 16, pivot: new(0.50f, 1.00f));
							}

							GUI.SameLine();

							var active = !this.flags.HasAny(Deconstruct.Flags.Active);
							if (GUI.DrawButton("Dismantle", size: GUI.Rm, enabled: info.IsAlive, color: active ? GUI.col_button_error : GUI.col_button_error.WithColorMult(0.50f)))
							{
								rpc.active = active;
								dirty = true;
							}
						}

						//GUI.TitleCentered("Deconstruct", size: 24, pivot: new Vector2(0.50f, 0.50f));




						//using (GUI.Group.New(size: new(GUI.RmX, 28), padding: new(4, 2)))
						//{
						//	GUI.TitleCentered("Deconstruct", size: 24, pivot: new Vector2(0.00f, 0.50f));
						//}

						//GUI.SeparatorThick();

						//using (GUI.Group.New(size: GUI.Rm, padding: new(4, 6)))
						//{
						//	using (GUI.Wrap.Push(GUI.RmX))
						//	{
						//		//GUI.TextShaded(recipe.desc, color: GUI.font_color_desc);

						//		//GUI.NewLine();

						//		//if (recipe.products[0].type == Crafting.Product.Type.Prefab && recipe.products[0].prefab.TryGetPrefab(out var prefab))
						//		//{
						//		//	var root = prefab.Root;
						//		//	if (root != null)
						//		//	{
						//		//		GUI.DrawStats(root, priority_min: Statistics.Priority.Low);
						//		//	}
						//		//}
						//	}
						//}

						if (dirty)
						{
							rpc.Send(ent_wrench);
						}
					}

					public void DrawHUD(Entity ent_wrench, ref TargetInfo info_target)
					{
						if (info_target.IsAlive)
						{
							ref var transform_wrench = ref ent_wrench.GetComponent<Transform.Data>();
							if (transform_wrench.IsNotNull())
							{
								var target_pos = info_target.Position;
								ref var target_body = ref info_target.entity.GetComponent<Body.Data>();
								if (target_body.IsNotNull())
								{
									target_pos = target_body.GetClosestPoint(transform_wrench.GetInterpolatedPosition(), true).world_position;
								}
								else
								{
									ref var target_transform = ref info_target.transform;
									if (target_transform.IsNotNull())
									{
										target_pos = target_transform.GetInterpolatedPosition();
									}
								}

								//var text = "Dismantling...";

								var dist_sq = Vector2.DistanceSquared(transform_wrench.GetInterpolatedPosition(), target_pos);
								var color = dist_sq <= (2.00f * 2.00f) ? Color32BGRA.Green : Color32BGRA.Red;
								var target_pos_c = target_pos.WorldToCanvas();

								GUI.DrawLine(transform_wrench.GetInterpolatedPosition().WorldToCanvas(), target_pos_c, color.WithAlphaMult(0.25f), 4);
								GUI.DrawCircleFilled(target_pos_c, 0.20f * GUI.GetWorldToCanvasScale(), color.WithAlphaMult(0.50f));
								//GUI.DrawTextCentered(text, target_pos_c + new Vector2(0, 20), font: GUI.Font.Superstar, size: 16, color: GUI.font_color_title);
							}
						}

						//using (var hud = GUI.Window.Standalone("Wrench.HUD", position: info_target.pos.WorldToCanvas(), size: new(168, 200), pivot: new(0.50f, 0.50f), force_position: false))
						//{
						//	if (hud.show)
						//	{
						//		GUI.DrawBackground(GUI.tex_panel, hud.group.GetOuterRect(), padding: new(4));

						//		//using (GUI.Group.New(size: GUI.Rm - new Vector2(0, 48), padding: new(4)))
						//		//{

						//		//}

						//		using (GUI.Group.New(size: new Vector2(GUI.RmX, GUI.RmY - 40), padding: new(8, 8)))
						//		{
						//			GUI.Title("Produces:");
						//			GUI.SeparatorThick();

						//			var items_span = info_target.dismantlable.items.AsSpan();

						//			for (var i = 0; i < items_span.Length; i++)
						//			{
						//				ref var item = ref items_span[i];
						//				switch (item.type)
						//				{
						//					case Shipment.Item.Type.Resource:
						//					{
						//						var resource = new Resource.Data(item.material, item.quantity);
						//						GUI.DrawResource(ref resource, new Vector2(GUI.RmX, 32), GUI.font_color_green);
						//					}
						//					break;
						//				}
						//			}

						//			GUI.TitleCentered($"{info_target.dismantlable.current_work:0.00}/{info_target.dismantlable.required_work:0.00}", pivot: new(0.50f, 1.00f));
						//		}

						//		using (GUI.Group.Centered(outer_size: GUI.Rm, inner_size: new(100, 40)))
						//		{
						//			var active = !this.flags.HasAny(Deconstruct.Flags.Active);
						//			if (GUI.DrawButton("Dismantle", size: GUI.Rm, color: active ? GUI.col_button_error : GUI.col_button_error.WithColorMult(0.50f)))
						//			{
						//				var rpc = new ConfigureRPC
						//				{
						//					active = active
						//				};
						//				rpc.Send(ent_wrench);
						//			}
						//		}
						//	}
						//}

					}
#endif
				}

				public struct SetTargetRPC: Net.IRPC<Wrench.Mode.Deconstruct.Data>
				{
					public Entity ent_target;

#if SERVER
					public void Invoke(Net.IRPC.Context rpc, ref Wrench.Mode.Deconstruct.Data data)
					{
						ref var region = ref rpc.entity.GetRegion();

						data.flags.SetFlag(Flags.Active, false);
						data.ref_dismantlable.Set(this.ent_target);
						data.Sync(rpc.entity);
					}
#endif
				}

				public struct ConfigureRPC: Net.IRPC<Wrench.Mode.Deconstruct.Data>
				{
					public Deconstruct.Filter? filter;
					public bool? active;

#if SERVER
					public void Invoke(Net.IRPC.Context rpc, ref Wrench.Mode.Deconstruct.Data data)
					{
						ref var region = ref rpc.entity.GetRegion();

						var sync = false;

						if (this.filter.TryGetValue(out var v_filter))
						{
							data.filter = v_filter;
							sync = true;
						}

						if (this.active.TryGetValue(out var v_active) && data.flags.TrySetFlag(Deconstruct.Flags.Active, v_active))
						{
							sync = true;
						}

						if (sync)
						{
							data.Sync(rpc.entity);
						}
					}
#endif
				}

#if SERVER
				[ISystem.Update(ISystem.Mode.Single, ISystem.Scope.Region, interval: 0.60f), HasTag("dead", false, Source.Modifier.Parent), HasRelation(Source.Modifier.Owned, Relation.Type.Stored, false)]
				public static void Update(ISystem.Info info, Entity entity, ref XorRandom random, ref Region.Data region, 
				[Source.Owned] in Transform.Data transform, [Source.Parent] in Interactor.Data interactor,
				[Source.Owned] ref Wrench.Data wrench, [Source.Owned] ref Wrench.Mode.Deconstruct.Data deconstruct)
				{
					if (deconstruct.flags.HasAny(Deconstruct.Flags.Active) && deconstruct.ref_dismantlable.TryGetHandle(out var h_dismantlable))
					{
						var target_pos = transform.position;
						ref var target_body = ref deconstruct.ref_dismantlable.entity.GetComponent<Body.Data>();
						if (target_body.IsNotNull())
						{
							target_pos = target_body.GetClosestPoint(transform.position, true).world_position;
						}
						else
						{
							ref var target_transform = ref deconstruct.ref_dismantlable.entity.GetComponent<Transform.Data>();
							if (target_transform.IsNotNull())
							{
								target_pos = target_transform.position;
							}
						}

						//ref var target_transform = ref deconstruct.ref_dismantlable.entity.GetComponent<Transform.Data>();
						//if (target_transform.IsNotNull())
						{
							var dir = (target_pos - transform.position).GetNormalized(out var dist);
							if (dist <= 2.00f && region.IsInLineOfSight(transform.position, target_pos, 0.125f, mask: Physics.Layer.World, exclude: Physics.Layer.Essence))
							{
								h_dismantlable.data.current_work += 1.00f * info.DeltaTime;
								if (h_dismantlable.data.current_work >= h_dismantlable.data.required_work)
								{
									h_dismantlable.data.flags.SetFlag(Dismantlable.Flags.Active, true);
									h_dismantlable.data.yield = Maths.Clamp(h_dismantlable.data.yield, 0.00f, 1.00f);
									h_dismantlable.Sync();
									h_dismantlable.entity.Delete();

									deconstruct.ref_dismantlable.Reset();
									deconstruct.flags.SetFlag(Deconstruct.Flags.Active, false);
									deconstruct.Sync(entity, true);
								}
								else
								{
									Sound.Play(ref region, deconstruct.sound_dismantle, transform.position, volume: random.NextFloatRange(0.90f, 1.10f), pitch: random.NextFloatRange(0.95f, 1.05f));

									h_dismantlable.Sync();
								}
							}
						}
					}
				}
#endif

				public struct TargetInfo: ITargetInfo
				{
					public Entity entity;

					public Dismantlable.Data dismantlable;
					public Transform.Data transform;

					public float radius;
					public Vector2 pos;

					public bool alive;
					public bool valid;
					public bool selectable;

					public readonly Entity Entity => this.entity;
					public readonly IComponent.Handle ComponentID => ECS.GetID<Dismantlable.Data>();
					public readonly Vector2 Position => this.pos;
					public readonly float Radius => this.radius;
					public readonly bool IsSource => true;
					public readonly bool IsSelectable => this.selectable;
					public readonly bool IsAlive => this.alive;
					public readonly bool IsValid => this.valid;

					public TargetInfo(Entity entity)
					{
						this.entity = entity;
						this.alive = this.entity.IsAlive();

						if (this.alive)
						{
							this.valid = true;

							this.valid &= this.entity.GetComponent<Dismantlable.Data>().TryGetValueFromRef(out this.dismantlable);
							this.valid &= this.entity.GetComponent<Transform.Data>().TryGetValueFromRef(out this.transform);

							if (this.valid)
							{
								this.radius = 1.00f;
								this.pos = this.transform.position;
							}
						}
					}
				}
			}
		}
	}
}
