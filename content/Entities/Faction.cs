
namespace TC2.Base.Components
{
	// TODO: refactor this - just moved it from engine code, uses some pretty old stuff
	public static partial class FactionExt
	{
#if CLIENT
		public partial struct FactionHUD: IGUICommand
		{
			public IFaction.Handle h_faction;

			public void Draw()
			{
				using (var widget = Sidebar.Widget.New("faction", "Faction", new Sprite(GUI.tex_icons_widget, 16, 16, 3, 0), size: new Vector2(48 * 6, 48 * 8), order: -20.00f))
				{
					if (widget.state_flags.HasAny(Sidebar.Widget.StateFlags.Show))
					{
						//ref var region = ref Client.GetRegion();
						ref var faction_data = ref this.h_faction.GetData(out var faction_asset);
						if (faction_data.IsNotNull())
						{
							using (GUI.Group.New(size: GUI.GetAvailableSize()))
							{
								//if (this.h_faction != 0)
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
											GUI.TitleCentered(faction_data.name, size: 24, pivot: new(0.00f, 0.50f));
										}

										GUI.SameLine();

										if (Constants.Factions.enable_leave_faction)
										{
											if (GUI.DrawConfirmButton("confirm.leave", "Leave", "Do you want to leave this faction?", new Vector2(80, 40), color: GUI.col_button_error))
											{
												var rpc = new Player.LeaveFactionRPC()
												{

												};
												rpc.Send();
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

													var faction_id_tmp = this.h_faction;

													//region.Query<Region.GetPlayersQuery>(Func).Execute(ref this);
													foreach (var s_player in IPlayer.Database.GetAssetsSpan())
													{
														ref var member_player_data = ref s_player.GetData();
														if (member_player_data.h_faction == this.h_faction)
														{
															using (var row = GUI.Table.Row.New(new Vector2(GUI.RmX, 20)))
															{
																var h_player_member = s_player.GetHandle();
																{
																	using (GUI.ID.Push(h_player_member))
																	{
																		using (row.Column(0))
																		{
																			GUI.TextShaded(member_player_data.name);
																		}

																		using (row.Column(2))
																		{
																			if (faction_data.IsLeader(h_player_member))
																			{
																				GUI.TextShaded("Leader"u8);
																			}
																		}

																		using (row.Column(3))
																		{
																			//GUI.TextShaded(player.flags.HasAll(Player.Flags.Online) ? "Online" : "Offline");
																		}

																		GUI.SameLine();
																		GUI.Selectable("", false, play_sound: false, size: new Vector2(0, 0), same_line: false);
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
					}
				}
			}
		}

		[ISystem.EarlyGUI(ISystem.Mode.Single, ISystem.Scope.Global)]
		public static void OnHUD(ISystem.Info.Global info, ref Region.Data.Global region, [Source.Owned] ref World.Global world)
		{
			var h_faction = Client.GetFactionHandle();
			if (h_faction.IsValid())
			{
				var gui = new FactionHUD()
				{
					h_faction = h_faction
				};
				gui.Submit();
			}
		}
#endif
	}
}
