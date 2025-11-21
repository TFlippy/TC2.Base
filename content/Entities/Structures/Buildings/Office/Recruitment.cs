namespace TC2.Base.Components
{
	public static partial class Recruitment
	{
		[IComponent.Data(Net.SendType.Reliable, IComponent.Scope.Region | IComponent.Scope.Global)]
		public partial struct Data(): IComponent
		{
			[Flags]
			public enum Flags: uint
			{
				None = 0,
			}

			public Recruitment.Data.Flags flags;
		}

#if CLIENT
		[ISystem.Update.B(ISystem.Mode.Single, ISystem.Scope.Region | ISystem.Scope.Global)]
		public static void OnUpdate(ISystem.Info.Common info, ref Region.Data.Common region_common, ref XorRandom random, Entity entity,
		[Source.Owned] in Transform.Data transform,
		[Source.Owned] ref Recruitment.Data recruitment,
		[Source.Owned] in Faction.Data faction)
		{

		}
#endif

		public struct ConfigureRPC: Net.IRPC<Recruitment.Data>
		{

#if SERVER
			public void Invoke(Net.IRPC.Context rpc, ref Recruitment.Data data)
			{
				var sync = false;


				if (sync)
				{
					rpc.Sync(ref data);
				}
			}
#endif
		}

#if CLIENT
		public struct RecruitmentGUI: IGUICommand
		{
			public Entity ent_recruitment;
			public Transform.Data transform;

			public Recruitment.Data recruitment;

			public Faction.Data faction;
			public Company.Data company;
			public Entrance.Linkable.Data entrance_linkable;

			public void Draw()
			{
				using (var window = GUI.Window.Interaction(identifier: "Recruitment"u8, entity: this.ent_recruitment, tooltip_tab: "Manage recruitment of new characters."))
				{
					this.StoreCurrentWindowTypeID(order: -100);
					if (window.show)
					{
						ref var region_common = ref this.ent_recruitment.GetRegionCommon();
						ref var location_data = ref region_common.GetLocation(out var location_asset);
						ref var entrance_data = ref this.entrance_linkable.h_entrance.GetData(out var entrance_asset);

						var w_right = (48 * 4) + 24;

						using (GUI.Group.New(size: new(GUI.RmX - w_right, GUI.RmY)))
						{
							using (GUI.Group.New(size: new(GUI.RmX, 32), padding: new(8)))
							{
								GUI.Title(location_data.IsNotNull() ? location_data.name_short : "<location>"u8, size: 24);
								GUI.FocusableAsset(location_asset);
								GUI.SameLine();
								GUI.Title(" | "u8, size: 24);
								GUI.SameLine();
								GUI.Title((entrance_data.IsNotNull() ? entrance_data.name : "<entrance>"u8), size: 24);
								GUI.FocusableAsset(entrance_asset);
							}

							using (GUI.Group.New(size: GUI.Rm))
							{
								GUI.DrawFillBackground(GUI.tex_panel, new(8));

								//using (GUI.Group.New(size: GUI.Rm, padding: new(12, 12)))
								{

									using (GUI.Group.New(size: GUI.Rm, padding: new(12)))
									{
										if (location_data.IsNotNull())
										{
											var population = location_data.population;
											if (population is not null)
											{
												var newline = false;

												foreach (var (h_species, num) in population)
												{
													ref var species_data = ref h_species.GetData();
													if (species_data.IsNotNull())
													{
														if (newline) GUI.NewLine(4);

														using (var pop_row = GUI.Group.New(size: new(GUI.RmX, 32)))
														{
															pop_row.DrawBackground(GUI.tex_panel);

															using (var group_icon = GUI.Group.New(size: new(GUI.RmY)))
															{
																GUI.DrawSpriteCentered(species_data.sprite_head_male, group_icon.GetOuterRect(), layer: GUI.Layer.Window, scale: 3.00f);
															}

															GUI.SameLine(8);

															GUI.TitleCentered(species_data.name, pivot: new(0.00f, 0.50f), size: 20.00f);
															GUI.TitleCentered(num, format: "'~ '0", pivot: new(1.00f, 0.50f), size: 20.00f, offset: new(-8, 0));

															newline = true;
														}
													}
													GUI.FocusableAsset(h_species);
												}
											}

										}
										//GUI.DrawFillBackground(GUI.tex_panel, new(8, 8, 8, 8), margin: new(-12, 0, -12, -12));


									}
								}
							}
						}

						GUI.SameLine();

						using (GUI.Group.New(size: new(w_right, GUI.RmY)))
						{

						}
					}
				}
			}
		}

		[ISystem.LateGUI(ISystem.Mode.Single, ISystem.Scope.Region | ISystem.Scope.Global)]
		public static void OnGUI(Entity ent_recruitment,
		[Source.Owned] in Interactable.Data interactable,
		[Source.Owned] in Transform.Data transform,
		[Source.Owned] in Recruitment.Data recruitment,
		[Source.Owned, Optional] in Faction.Data faction,
		[Source.Owned, Optional] in Company.Data company,
		[Source.Owned, Optional] in Entrance.Linkable.Data entrance_linkable)
		{
			if (interactable.IsActive())
			{
				var gui = new RecruitmentGUI()
				{
					ent_recruitment = ent_recruitment,

					transform = transform,

					recruitment = recruitment,

					faction = faction,
					company = company,
					entrance_linkable = entrance_linkable
				};
				gui.Submit();
			}
		}
#endif
	}
}
