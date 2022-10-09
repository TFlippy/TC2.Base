
namespace TC2.Base.Components
{
	public static partial class Capturable
	{
		[IComponent.Data(Net.SendType.Reliable)]
		public partial struct Data: IComponent
		{
			public Workshop.Order order;

			public IFaction.Handle capturing_faction_id;

			[Net.Ignore, Save.Ignore]
			public float next_check;

			public Data()
			{

			}
		}

		public struct CaptureRPC: Net.IRPC<Capturable.Data>
		{
			public float test;

#if SERVER
			public void Invoke(ref NetConnection connection, Entity entity, ref Capturable.Data data)
			{
				ref var player = ref connection.GetPlayer();
				data.capturing_faction_id = player.faction_id;

				ref var order = ref data.order;
				order.amount_multiplier = 1.00f;
				order.flags = Workshop.Order.Flags.InProgress;

				var work_span = order.work.AsSpan();
				work_span[0] = new Work.Amount(Work.Type.Capturing, 10.00f, 200.00f, 0.00f);

				data.Sync(entity);
			}
#endif
		}

#if SERVER
		[ISystem.Update(ISystem.Mode.Single)]
		public static void UpdateCapturable(ISystem.Info info, Entity entity, [Source.Owned] in Transform.Data transform,
		[Source.Owned] ref Worker.Data worker, [Source.Owned] ref Worker.State worker_state, [Source.Work] ref Capturable.Data capturable, [Source.Work] in Transform.Data transform_capturable)
		{
			if (info.WorldTime >= worker_state.next_work && Vector2.DistanceSquared(transform.position, transform_capturable.position) <= (Worker.max_distance * Worker.max_distance))
			{
				var cooldown = 0.20f;

				ref var order = ref capturable.order;
				if (order.flags.HasAll(Workshop.Order.Flags.InProgress))
				{
					var ent_construction = entity.GetParent(Relation.Type.Work);

					var experience = entity.GetComponentWithOwner<Experience.Data>(Relation.Type.Instance);
					var result = Worker.PerformWork(transform.position, ref info.GetRegion(), ref order, entity, ref worker, ref worker_state, experience, out cooldown, Constants.Experience.crafting_experience_multiplier_npc);

					switch (result)
					{
						case Worker.WorkResult.Progress:
						{
							ent_construction.SyncComponent(ref capturable);
						}
						break;

						case Worker.WorkResult.Done:
						{
							worker.work_index = null;
							entity.SyncComponent(ref worker);
						}
						break;
					}
				}
				else
				{
					worker.order_index = null;
					entity.SyncComponent(ref worker);
				}

				worker_state.next_work = info.WorldTime + (cooldown * 5.00f);
			}
		}

		[ISystem.LateUpdate(ISystem.Mode.Single)]
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void OnUpdate(ISystem.Info info, Entity entity, [Source.Owned] ref Capturable.Data capturable)
		{
			if (info.WorldTime >= capturable.next_check)
			{
				capturable.next_check = info.WorldTime + 1.00f;
				if (capturable.capturing_faction_id.id != 0)
				{
					ref var order = ref capturable.order;

					var sync = false;

					var done = true;
					foreach (ref var work in order.work.AsSpan())
					{
						done &= work.type == Work.Type.Undefined || work.current >= work.required;
					}

					if (done)
					{
						order.flags &= ~Workshop.Order.Flags.InProgress;
						order.flags |= Workshop.Order.Flags.Complete;

						sync = true;
					}

					if (capturable.order.flags.HasAny(Workshop.Order.Flags.Complete))
					{
						entity.SetFaction(capturable.capturing_faction_id);

						capturable.capturing_faction_id = default;
						capturable.order = default;

						sync = true;
					}

					if (sync)
					{
						capturable.Sync(entity);
					}
				}
			}
		}
#endif

#if CLIENT
		public struct CapturableGUI: IGUICommand
		{
			public Entity ent_capturable;

			public Capturable.Data capturable;

			public void Draw()
			{
				using (var window = GUI.Window.Interaction("Capturable", this.ent_capturable))
				{
					this.StoreCurrentWindowTypeID(order: -100);

					if (window.show)
					{
						ref var player = ref Client.GetPlayer();
						ref var region = ref Client.GetRegion();
						ref var experience = ref player.GetExperience().data;
						var ref_worker = player.ent_controlled.GetComponentWithOwner<Worker.Data>(Relation.Type.Instance);
						ref var worker = ref ref_worker.data;

						ref var order = ref this.capturable.order;
						ref var recipe = ref order.recipe.GetRecipe();

						var has_resources = true;
						var has_worker = false;

						var faction_id = player.faction_id;
						ref var faction = ref faction_id.GetValue();
						//var frame_size = Inventory.GetFrameSize(4, 2);

						//using (GUI.Group.New(size: new Vector2(GUI.GetRemainingWidth(), GUI.GetRemainingHeight())))
						//{
						//	using (var group = GUI.Group.New(size: new Vector2(GUI.GetRemainingWidth() - frame_size.X - 32, GUI.GetRemainingHeight()), padding: new Vector2(8, 8)))
						//	{
						//		GUI.DrawBackground(GUI.tex_frame, group.GetOuterRect(), new(8));


						//	}
						//}

						using (GUI.Group.New(size: GUI.GetRemainingSpace()))
						{
							for (var i = 0; i < 1; i++)
							{
								ref var work = ref order.work[i];
								using (GUI.ID.Push(i))
								{
									using (GUI.Group.New(size: new Vector2(GUI.GetRemainingWidth(), 48)))
									{
										var button_size = new Vector2(24 * 4, 48);

										GUI.DrawWork(ref experience, work, new(GUI.GetRemainingWidth() - button_size.X, GUI.GetRemainingHeight()), Color32BGRA.Lerp(GUI.col_button, faction.color_a, 0.50f));
										GUI.OffsetLine(GUI.GetRemainingWidth() - button_size.X);

										if (work.type != Work.Type.Undefined)
										{
											var button_color = has_resources ? GUI.col_button_yellow : GUI.font_color_default_dark;

											if (work.current >= work.required)
											{
												if (GUI.DrawButton("", button_size, enabled: false))
												{

												}
											}
											else if (worker.work_index != i)
											{
												if (GUI.DrawButton("Capture", button_size, enabled: work.current < work.required && has_resources, color: button_color))
												{
													var rpc = new Worker.SetTargetRPC
													{
														ent_target = this.ent_capturable,
														order_index = (ushort)0,
														work_index = (ushort)i,
													};
													rpc.Send(ref_worker.entity);
												}
											}
											else
											{
												if (GUI.DrawButton("Stop", button_size, enabled: work.current < work.required && has_resources, color: button_color))
												{
													var rpc = new Worker.SetTargetRPC
													{
														ent_target = default
													};
													rpc.Send(ref_worker.entity);
												}
											}
										}
										else
										{
											if (GUI.DrawButton("", button_size, enabled: false))
											{

											}
										}
									}
								}
							}

							if (GUI.DrawButton("DEV: Reset", new Vector2(120, 48)))
							{
								var rpc = new Capturable.CaptureRPC()
								{

								};
								rpc.Send(this.ent_capturable);
							}

							//GUI.SameLine();

							//GUI.DrawWorkH(0.50f, size: new Vector2(GUI.GetRemainingWidth(), 48));
						}
					}
				}
			}
		}

		[ISystem.EarlyGUI(ISystem.Mode.Single)]
		public static void OnGUI(Entity entity, [Source.Owned] in Capturable.Data capturable, [Source.Owned] in Interactable.Data interactable)
		{
			if (interactable.show)
			{
				var gui = new CapturableGUI()
				{
					ent_capturable = entity,

					capturable = capturable,
				};
				gui.Submit();
			}
		}
#endif
	}
}
