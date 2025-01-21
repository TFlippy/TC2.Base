namespace TC2.Base.Components
{
	public static partial class Electrolyzer
	{
		[IComponent.Data(Net.SendType.Reliable, IComponent.Scope.Region), IComponent.With<Electrolyzer.State>]
		public struct Data: IComponent
		{
			[Flags]
			public enum Flags: uint
			{
				None = 0,
				Use_Misc = 1 << 0
			}

			public Electrolyzer.Data.Flags flags;

			[Save.Ignore, Net.Ignore] public float t_next_update;

			public Data()
			{

			}
		}

		[IComponent.Data(Net.SendType.Unreliable, IComponent.Scope.Region)]
		public struct State: IComponent
		{

			[Save.Ignore, Net.Ignore] public float t_next_update;
		}

		public struct ConfigureRPC: Net.IRPC<Electrolyzer.Data>
		{
#if SERVER
			public void Invoke(Net.IRPC.Context rpc, ref Electrolyzer.Data data)
			{
				//ref var region = ref entity.GetRegion();
				//if (region.GetWorldTime() >= data.t_next_edit)
				//{
				//	data.t_next_edit = region.GetWorldTime() + 0.10f;

				//	var sync = false;

				//	if (this.burner_modifier.TryGetValue(out var v_burner_modifier))
				//	{
				//		ref var burner_state = ref entity.GetComponent<Burner.State>();
				//		if (burner_state.IsNotNull())
				//		{
				//			if (burner_state.modifier_intake_target.TrySet(v_burner_modifier.Clamp01()))
				//			{
				//				burner_state.Sync(entity, true);
				//			}
				//		}
				//	}

				//	if (sync)
				//	{
				//		data.Sync(entity, true);
				//	}
				//}
			}
#endif
		}

		public const float update_interval = 0.20f;
		public const float update_interval_failed = 3.00f;

		public static readonly Texture.Handle texture_smoke = "BiggerSmoke_Light";

		[ISystem.PostUpdate.C(ISystem.Mode.Single, ISystem.Scope.Region)]
		public static void Update(ISystem.Info info, ref Region.Data region, ref XorRandom random, Entity entity,
		[Source.Owned] in Transform.Data transform, [Source.Owned] ref Body.Data body,
		[Source.Owned] ref Electrolyzer.Data electrolyzer, [Source.Owned] ref Electrolyzer.State electrolyzer_state)
		{
			var time = info.WorldTime;
			if (time >= electrolyzer.t_next_update)
			{
				electrolyzer.t_next_update = time + update_interval;

			}
		}

#if CLIENT
		public struct ElectrolyzerGUI: IGUICommand
		{
			public Entity ent_electrolyzer;

			public Transform.Data transform;

			public Electrolyzer.Data electrolyzer;
			public Electrolyzer.State electrolyzer_state;

			public void Draw()
			{
				using (var window = GUI.Window.Interaction("Electrolyzer"u8, this.ent_electrolyzer))
				{
					this.StoreCurrentWindowTypeID(order: -100);
					if (window.show)
					{
						using (GUI.Group.New(size: new Vector2(GUI.RmX, GUI.RmY)))
						{
							//GUI.TextShaded("Derpo"u8);
						}
					}
				}
			}
		}

		[ISystem.GUI(ISystem.Mode.Single, ISystem.Scope.Region)]
		public static void OnGUI(Entity entity,
		[Source.Owned] in Interactable.Data interactable, [Source.Owned] in Transform.Data transform,
		[Source.Owned] in Electrolyzer.Data electrolyzer, [Source.Owned] in Electrolyzer.State electrolyzer_state)
		{
			if (interactable.IsActive())
			{
				var gui = new ElectrolyzerGUI()
				{
					ent_electrolyzer = entity,
					transform = transform,
					electrolyzer = electrolyzer,
					electrolyzer_state = electrolyzer_state,
				};
				gui.Submit();
			}
		}
#endif

		//#if CLIENT
		//		[ISystem.VeryLateUpdate(ISystem.Mode.Single, ISystem.Scope.Region)]
		//		public static void UpdateEffects(ISystem.Info info, ref Region.Data region, ref XorRandom random,
		//		[Source.Owned] in Transform.Data transform,
		//		[Source.Owned] in Electrolyzer.Data electrolyzer, [Source.Owned] ref Electrolyzer.State electrolyzer_state,
		//		[Source.Owned, Pair.Component<Electrolyzer.State>, Optional(true)] ref Sound.Emitter sound_emitter,
		//		[Source.Owned, Pair.Component<Electrolyzer.State>, Optional(true)] ref Light.Data light)
		//		{

		//		}
		//#endif
	}
}
