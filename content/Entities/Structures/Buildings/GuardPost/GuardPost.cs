namespace TC2.Base.Components
{
	[Asset.Hjson(prefix: "policy.", capacity_world: 256, capacity_region: 128, capacity_local: 32, flags_world: Asset.Flags.None, flags_local: Asset.Flags.Recycle)]
	public interface IPolicy: IAsset2<IPolicy, IPolicy.Data>
	{
		[Flags]
		public enum Flags: uint
		{
			None = 0,
		}

		[Flags]
		public enum Tags: uint
		{
			None = 0,
		}

		[Net.MsgPack]
		public struct Result
		{
			public enum Type: byte
			{
				Undefined = 0,

				Allow,
				Deny,

				Detain,
				Destroy,
				Confiscate,
				Incapacitate,
			}

			[Flags]
			public enum Flags: ushort
			{
				Undefined = 0,

				Alarm = 1 << 0,
			}

			[Net.Key(00), Save.Force] public required IPolicy.Result.Type type;
			[Net.Key(01)] public byte unused_00;
			[Net.Key(02), Save.Force] public IPolicy.Result.Flags flags;
		}

		public enum ScoreType: byte
		{
			Undefined = 0,

			Hazard,
			Wealth,
		}

		[Net.MsgPack, Save.Inline]
		public struct FlagFilter<T> where T: unmanaged, Enum
		{
			[Net.Key(00), Save.Force] public float multiplier;
			[Net.Key(01)] private uint unused_00;
			[Net.Key(02), Save.Force] public T mask;
		}

		[Net.MsgPack, Save.Inline]
		public struct FlagFilter2<T> where T : unmanaged, Enum
		{
			[Net.Key(00), Save.Force] public required IPolicy.Result.Type type;
			[Net.Key(01)] public byte unused_00;
			[Net.Key(02), Save.Force] public IPolicy.Result.Flags flags;
			[Net.Key(03), Save.Force] public float multiplier;
			[Net.Key(04), Save.Force] public T mask;
		}

		//[Net.MsgPack, Save.Inline]
		//public struct HandleFilter<T> where T : unmanaged, Enum
		//{
		//	[Save.Force, Net.Key(00)] public float multiplier;
		//	[Save.Force, Net.Key(01)] private uint unused_00;
		//	[Save.Force, Net.Key(02)] public T mask;
		//}

		[Net.MsgPack]
		public struct Data(): IName
		{
			[Net.Key(00), Save.Force] public required string name;

			[Save.NewLine]
			[Net.Key(03), Save.Force] public required IPolicy.Flags flags;
			[Net.Key(04), Save.Force] public required IPolicy.Tags tags;

			[Save.NewLine]
			[Net.Key(08)] public HashSet<ISpecies.Handle> hs_species;
			[Net.Key(09)] public HashSet<IFaction.Handle> hs_faction;

			[Save.NewLine]
			[Net.Key(16)] public IPolicy.FlagFilter<NPC.SelfHints> filter_hints;

			readonly ReadOnlySpan<char> IName.GetName() => this.name;
			readonly ReadOnlySpan<char> IName.GetShortName() => this.name;
		}
	}

	public static partial class GuardPost
	{
		//public enum Polic

		public struct MaterialPolicy()
		{
			public float modifier = 1.00f;

			public Material.Flags filter_tags;
			public NPC.ItemHints filter_hints;
			//public NPC.Connotations filter_connotations;
		}

		public struct ItemPolicy
		{
			public Material.Flags filter_flags;
			public NPC.ItemHints filter_hints;
			public NPC.Connotations filter_connotations;
		}

		//public struct Policy
		//{
		//	public Material.Flags filter_flags;
		//	public NPC.ItemHints filter_hints;
		//	public NPC.Connotations filter_connotations;
		//}

		[IComponent.Data(Net.SendType.Reliable, IComponent.Scope.Region)]
		public partial struct Data(): IComponent
		{
			[Flags]
			public enum Flags: ushort
			{
				None = 0,

				Search_Inventories = 1 << 8,
				Search_Shipments = 1 << 9,
				Search_Attached = 1 << 10,
				Search_Toolbelt = 1 << 11,
			}

			public required GuardPost.Data.Flags flags;
			public ushort unused_00;
			public uint unused_01;
			public required Tickstamp.Interval check_interval;
			//[Net.Ignore, Save.Ignore] public Tickstamp t_next_check;

			[Save.NewLine]
			public IPolicy.Handle h_policy_test_a;
			public IPolicy.Handle h_policy_test_b;

			public float unused_02;
			public float unused_03;
			public float unused_04;
			
			[Save.NewLine]
			public NPC.ItemHints item_hints_whitelist;
			public NPC.ItemHints item_hints_blacklist;

			public MaterialPolicy policy_material;
			public ItemPolicy policy_item;

			[Save.NewLine]
			[Save.Ignore, Asset.Ignore] public Entity ent_target_prev;
			[Save.Ignore, Asset.Ignore] public Entity ent_target_current;

			//[Save.NewLine]
			//public NPC.SelfHints self_hints_whitelist;
			//public NPC.SelfHints self_hints_blacklist;

			[Save.NewLine]
			public required Filter.Mask2x<Physics.Layer> filter_search;
			[Editor.Picker.Box()] public required AABB search_rect;
		}

		public struct EditRPC: Net.IRPC<GuardPost.Data>
		{
			public MaterialPolicy? policy_material;
			public ItemPolicy? policy_item;
			public NPC.ItemHints? item_hints_whitelist;
			public NPC.ItemHints? item_hints_blacklist;

#if SERVER
			public void Invoke(Net.IRPC.Context rpc, ref GuardPost.Data data)
			{
				var sync = false;

				ref var policy_material_new = ref this.policy_material.GetRefOrNull();
				if (policy_material_new.IsNotNull())
				{
					sync |= data.policy_material.filter_tags.TryChange(policy_material_new.filter_tags);
					sync |= data.policy_material.filter_hints.TryChange(policy_material_new.filter_hints);
					//sync |= data.policy_material.filter_connotations.TryChange(policy_material_new.filter_connotations);
				}

				ref var policy_item_new = ref this.policy_item.GetRefOrNull();
				if (policy_item_new.IsNotNull())
				{
					sync |= data.policy_item.filter_flags.TryChange(policy_item_new.filter_flags);
					sync |= data.policy_item.filter_hints.TryChange(policy_item_new.filter_hints);
					sync |= data.policy_item.filter_connotations.TryChange(policy_item_new.filter_connotations);
				}

				if (sync)
				{
					rpc.Sync(ref data, true);
				}
			}
#endif
		}

		[ISystem.Update.B(ISystem.Mode.Single, ISystem.Scope.Region)]
		public static void OnUpdate(ISystem.Info info, ref Region.Data region, ref XorRandom random, Entity ent_guardpost,
		[Source.Owned] ref Personnel.Data personnel, [Source.Owned] ref GuardPost.Data guardpost,
		[Source.Owned] ref Body.Data body, [Source.Owned] in Transform.Data transform)
		{
			if (region.GetCurrentTickstamp().CheckInterval(guardpost.check_interval))
			{
				//var ts = Timestamp.Now();
				var rect_world = guardpost.search_rect.Translate(transform.position); // transform.LocalToWorld(guardpost.search_rect);

				var results_span = FixedArray.CreateSpan32NoInit<OverlapBBResult>(out var results_buffer);
				if (region.TryOverlapBBAll(rect: rect_world, hits: ref results_span,
				require: guardpost.filter_search.require, exclude: guardpost.filter_search.exclude,
				mask: Physics.Layer.Storage | Physics.Layer.Item | Physics.Layer.Shipment | Physics.Layer.Resource | Physics.Layer.Harvestable | Physics.Layer.Crate,
				query_flags: Physics.QueryFlag.Dynamic))
				{

				}

//				var ts_elapsed = ts.GetMilliseconds();
//#if SERVER
//				WorldNotification.Push(region: ref region, message: $"{results_span.Length}\n{ts_elapsed:0.000} ms", 
//					color: results_span.IsEmpty ? Color32BGRA.Yellow : Color32BGRA.Green, position: transform.position);
//#endif
			}
		}

#if CLIENT
		public struct GuardPostGUI: IGUICommand
		{
			public Entity ent_guardpost;

			public GuardPost.Data guardpost;
			public Personnel.Data personnel;
			public Transform.Data transform;

			public void Draw()
			{
				using (var window = GUI.Window.Interaction(identifier: "Guard Post"u8, entity: this.ent_guardpost,
				color_tab: 0xff719240, tooltip_tab: "Manage guards and security policies."))
				{
					this.StoreCurrentWindowTypeID(order: -1000);
					if (window.show)
					{
						using (var group_policies = GUI.Group.New(size: new(340, GUI.RmY), padding: new(6)))
						{
							group_policies.DrawBackground(GUI.tex_window);

							const Material.Flags mask_material_flags_default = ~(Material.Flags.RESERVED_13 | Material.Flags.RESERVED_62 | Material.Flags.Primary | Material.Flags.Ammo_Musket);
							if (GUI.EnumInput("material.flags"u8, ref this.guardpost.policy_material.filter_tags, size: new(GUI.RmX, 40), show_none: true,
							mask: mask_material_flags_default, max_flags: 8, show_label: false, columns: 3, tooltip: "Blacklist (Material Types)"u8))
							{
								var rpc = new GuardPost.EditRPC
								{
									policy_material = this.guardpost.policy_material
								};
								rpc.Send(this.ent_guardpost);
							}

							const NPC.ItemHints mask_hints_default = ~(NPC.ItemHints.None);
							if (GUI.EnumInput("material.hints"u8, ref this.guardpost.policy_material.filter_hints, size: new(GUI.RmX, 40), show_none: true,
							mask: mask_hints_default, max_flags: 12, show_label: false, columns: 3, tooltip: "Blacklist (Material Properties)"u8))
							{
								var rpc = new GuardPost.EditRPC
								{
									policy_material = this.guardpost.policy_material
								};
								rpc.Send(this.ent_guardpost);
							}

							//const NPC.Connotations mask_connotations_default = ~(NPC.Connotations.None);
							//if (GUI.EnumInput("material.connotations"u8, ref this.guardpost.policy_material.filter_connotations, size: new(GUI.RmX, 40), show_none: true,
							//mask: mask_connotations_default, max_flags: 12, show_label: false, columns: 3, tooltip: "Blacklist (Material Connotations)"u8))
							//{
							//	var rpc = new GuardPost.EditRPC
							//	{
							//		policy_material = this.guardpost.policy_material
							//	};
							//	rpc.Send(this.ent_guardpost);
							//}
						}
					}
				}
			}
		}

		[ISystem.GUI(ISystem.Mode.Single, ISystem.Scope.Region)]
		public static void OnGUI([Source.Owned] in Interactable.Data interactable, Entity ent_guardpost,
		[Source.Owned] in GuardPost.Data guardpost, [Source.Owned] in Personnel.Data personnel,
		[Source.Owned] in Transform.Data transform)
		{
			if (interactable.IsActive())
			{
				var gui = new GuardPostGUI()
				{
					ent_guardpost = ent_guardpost,

					guardpost = guardpost,
					personnel = personnel,
					transform = transform
				};
				gui.Submit();
			}
		}
#endif
	}
}
