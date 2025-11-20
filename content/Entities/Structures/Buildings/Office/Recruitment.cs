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
						ref var entrance_data = ref this.entrance_linkable.h_entrance.GetData();

						var w_right = (48 * 4) + 24;

						using (GUI.Group.New(size: new Vector2(GUI.RmX - w_right, GUI.RmY)))
						{
							using (GUI.Group.New(size: GUI.Rm))
							{
								GUI.DrawFillBackground(GUI.tex_frame, new(8, 8, 8, 8));

								using (GUI.Group.New(size: GUI.Rm, padding: new(12, 12)))
								{
						
									using (GUI.Group.New(size: GUI.Rm, padding: new(12, 12)))
									{
										GUI.DrawFillBackground(GUI.tex_panel, new(8, 8, 8, 8), margin: new(-12, 0, -12, -12));


									}
								}
							}
						}

						GUI.SameLine();

						using (GUI.Group.New(size: new Vector2(w_right, GUI.RmY)))
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
