
namespace TC2.Base.Components
{
	// TODO: refactor this - just moved it from engine code, uses some pretty old stuff
	public static partial class FactionExt
	{
#if CLIENT
		public partial struct FactionHUD: IGUICommand
		{
			public IFaction.Handle faction_id;

			public void Draw()
			{
				using (var widget = Sidebar.Widget.New("faction", "Faction", new Sprite(GUI.tex_icons_widget, 16, 16, 3, 0), size: new Vector2(48 * 6, 48 * 8), order: -20.00f))
				{
					if (widget.state_flags.HasAny(Sidebar.Widget.StateFlags.Show))
					{
						ref var region = ref Client.GetRegion();
						if (this.faction_id.TryGetData(out var ref_faction))
						{
							using (GUI.Group.New(size: GUI.GetAvailableSize()))
							{
								if (this.faction_id != 0)
								{
									//using (var group = GUI.Group.New(new(0, 0), new(4, 4)))
									//{
									//	GUI.Title(ref_faction.value.name, size: 24);
									//}

									//GUI.SeparatorThick();

									using (GUI.Group.New(new(GUI.RmX, 40), new(0, 0)))
									{
										using (GUI.Group.New(new(GUI.RmX - 80, 40), new(8, 0)))
										{
											GUI.TitleCentered($"{ref_faction.value.name}", size: 24, pivot: new(0.00f, 0.50f));
										}

										GUI.SameLine();

										if (Constants.Factions.enable_leave_faction)
										{
											if (GUI.DrawConfirmButton("confirm.leave", "Leave", "Do you want to leave this faction?", new Vector2(80, 40), color: GUI.col_button_error))
											{
												var rpc = new Player.LeaveFactionRPC()
												{

												};
												rpc.Send(Client.GetEntity());
											}
											GUI.DrawHoverTooltip("Leave this faction.");
										}
									}

									GUI.SeparatorThick();

									using (GUI.Group.New(size: new Vector2(GUI.RmX, GUI.RmY), padding: new(8)))
									{
										GUI.Title("Members", size: 20);
										GUI.SeparatorThick();

										using (var scrollable = GUI.Scrollbox.New("Members.Scrollable", size: GUI.Rm, padding: new(4)))
										{
											GUI.DrawBackground(GUI.tex_panel, scrollable.group_frame.GetInnerRect(), padding: new(8));

											using (var table = GUI.Table.New("Members", 4))
											{
												if (table.show)
												{
													table.SetupColumnFlex(1);
													table.SetupColumnFixed(48);
													table.SetupColumnFixed(56);
													table.SetupColumnFixed(56);
													//table.SetupColumnFlex(1);

													using (var row = table.NextRow(16, header: true))
													{
														using (row.Column(0)) GUI.Title("Name");
														using (row.Column(1)) GUI.Title("Money");
														using (row.Column(2)) GUI.Title("Rank");
														using (row.Column(3)) GUI.Title("Status");
														//using (row.Column(2)) GUI.Title("Test");
													}

													var faction_id_tmp = this.faction_id;

													//region.Query<Region.GetPlayersQuery>(Func).Execute(ref this);
													foreach (ref var row in region.IterateQuery<Region.GetPlayersQuery>())
													{
														row.Run((ISystem.Info info, Entity entity, in Player.Data player) =>
														{
															if (player.faction_id == faction_id_tmp && player.faction_id.TryGetData(out var ref_faction))
															{
																using (var row = GUI.Table.Row.New(new Vector2(GUI.GetAvailableWidth(), 24)))
																{
																	using (GUI.ID.Push(entity))
																	{
																		using (row.Column(0))
																		{
																			GUI.TextShaded(player.GetName());
																		}

																		var player_tmp = player; // TODO: hack
																		//ref var money = ref player_tmp.GetMoney().data;
																		//if (!money.IsNull())
																		//{
																		//	using (row.Column(1))
																		//	{
																		//		GUI.TextShaded($"{money.amount.FormatAmount():0.##}");
																		//	}
																		//}

																		using (row.Column(2))
																		{
																			if (ref_faction.value.leader_player_id == player.id)
																			{
																				GUI.TextShaded("Leader");
																			}
																		}

																		using (row.Column(3))
																		{
																			GUI.TextShaded(player.flags.HasAll(Player.Flags.Online) ? "Online" : "Offline");
																		}

																		GUI.SameLine();
																		GUI.Selectable("", false, play_sound: false, size: new Vector2(0, 0), same_line: false);
																	}
																}
															}
														});
													}
												}
											}
										}
									}
								}
							}
						}
					}
				}
			}
		}

		[ISystem.EarlyGUI(ISystem.Mode.Single), HasTag("local", true, Source.Modifier.Owned)]
		public static void OnHUD(ISystem.Info info, Entity entity, [Source.Owned] in Player.Data player)
		{
			if (player.faction_id.id != 0)
			{
				var gui = new FactionHUD()
				{
					faction_id = player.faction_id
				};
				gui.Submit();
			}
		}
#endif
	}
}
