namespace TC2.Base.Components
{
	public static partial class Crucible
	{
		[IComponent.Data(Net.SendType.Reliable, IComponent.Scope.Region)]
		public struct Data(): IComponent
		{
			[Flags]
			public enum Flags: uint
			{
				None = 0,

				Use_Misc = 1 << 0
			}

			public Crucible.Data.Flags flags;
			public float flow_rate = 10.00f;

			[Editor.Picker.Position(true, true)] public Vector2 offset;

			[Save.Ignore, Net.Ignore] public float t_next_update;
		}

		//[IComponent.Data(Net.SendType.Unreliable)]
		//public struct State: IComponent
		//{
		//	public Temperature temperature_target;
		//	public Temperature temperature_current;

		//	[Save.Ignore, Net.Ignore] public float t_next_update;
		//}

		//		public struct ConfigureRPC: Net.IRPC<Crucible.State>
		//		{
		//#if SERVER
		//			public void Invoke(Net.IRPC.Context rpc, ref Crucible.State data)
		//			{
		//				//ref var region = ref entity.GetRegion();
		//				//if (region.GetWorldTime() >= data.t_next_edit)
		//				//{
		//				//	data.t_next_edit = region.GetWorldTime() + 0.10f;

		//				//	var sync = false;

		//				//	if (this.burner_modifier.TryGetValue(out var v_burner_modifier))
		//				//	{
		//				//		ref var burner_state = ref entity.GetComponent<Burner.State>();
		//				//		if (burner_state.IsNotNull())
		//				//		{
		//				//			if (burner_state.modifier_intake_target.TrySet(v_burner_modifier.Clamp01()))
		//				//			{
		//				//				burner_state.Sync(entity, true);
		//				//			}
		//				//		}
		//				//	}

		//				//	if (sync)
		//				//	{
		//				//		data.Sync(entity, true);
		//				//	}
		//				//}
		//			}
		//#endif
		//		}

		public const float update_interval = 0.20f;

		[ISystem.PostUpdate.C(ISystem.Mode.Single, ISystem.Scope.Region)]
		public static void Update(ISystem.Info info, ref Region.Data region, ref XorRandom random, Entity entity,
		[Source.Owned] ref Transform.Data transform,
		[Source.Owned] ref Crafter.Data crafter, [Source.Owned] ref Crafter.State crafter_state, 
		[Source.Owned] ref Crucible.Data crucible)
		{

		}

#if SERVER
		[ISystem.Event<Blob.PourEvent>(ISystem.Mode.Single, ISystem.Scope.Region, order: 100)]
		public static void OnPourEvent(ISystem.Info info, ref Region.Data region, ref XorRandom random, Entity ent_crucible, [Source.Owned] ref Blob.PourEvent ev,
		[Source.Owned] ref Body.Data body, [Source.Owned] ref Transform.Data transform, 
		[Source.Owned] ref Crucible.Data crucible)
		{
			App.WriteLine($"OnPourEvent {ent_crucible}; {ev.h_substance}; {ev.h_prefab}; {ev.mass}");

			ev.rate = crucible.flow_rate;
			ev.offset = crucible.offset;

			region.SpawnPrefab(ev.h_prefab, ev: ev, position: transform.LocalToWorld(crucible.offset), rotation: transform.rotation);
		}
#endif

		//[ISystem.Update.B(ISystem.Mode.Single, ISystem.Scope.Region)]
		//public static void UpdateEssence(ISystem.Info info, ref Region.Data region, ref XorRandom random, Entity entity,
		//[Source.Owned] in Transform.Data transform, [Source.Owned] ref Essence.Container.Data essence_container,
		//[Source.Owned, Pair.Component<Essence.Container.Data>] ref Crucible.Data crucible, IComponent.Handle h_crucible)
		//{
		//	var time = info.WorldTime;
		//	if (time >= crucible.t_next_update)
		//	{
		//		ref var essence_data = ref essence_container.h_essence.GetData();
		//		if (essence_data.IsNotNull())
		//		{
		//			crucible.temperature_target += (essence_data.crafter_emit * essence_container.rate * essence_container.available) * info.DeltaTime;
		//		}
		//		//App.WriteLine("a");

		//		//crucible.t_next_update = time + update_interval;
		//	}
		//}

#if CLIENT
		public struct CrucibleGUI: IGUICommand
		{
			public Entity ent_crucible;

			public Transform.Data transform;
			public Crafter.Data crafter;
			public Crafter.State crafter_state;
			public Crucible.Data crucible;

			public void Draw()
			{
				//using (var window = GUI.Window.InteractionMisc("Crucible"u8, this.ent_crucible, size: new(24, 96 * 1)))
				//{
				//	this.StoreCurrentWindowTypeID(order: -100);
				//	if (window.show)
				//	{
				//		using (GUI.Group.New(size: new Vector2(GUI.RmX, GUI.RmY)))
				//		{

				//		}
				//	}
				//}
			}
		}

		[ISystem.GUI(ISystem.Mode.Single, ISystem.Scope.Region)]
		public static void OnGUI(Entity entity,
		[Source.Owned] in Interactable.Data interactable, [Source.Owned] in Transform.Data transform,
		[Source.Owned] in Crafter.Data crafter, [Source.Owned] in Crafter.State crafter_state, 
		[Source.Owned] in Crucible.Data crucible)
		{
			if (interactable.IsActive())
			{
				var gui = new CrucibleGUI()
				{
					ent_crucible = entity,

					transform = transform,
					crafter = crafter,
					crafter_state = crafter_state,
					crucible = crucible
				};
				gui.Submit();
			}
		}
#endif

		//#if CLIENT
		//		[ISystem.VeryLateUpdate(ISystem.Mode.Single, ISystem.Scope.Region)]
		//		public static void UpdateEffects(ISystem.Info info, ref Region.Data region, ref XorRandom random,
		//		[Source.Owned] in Transform.Data transform,
		//		[Source.Owned] in Crucible.Data crucible, [Source.Owned] ref Crucible.State crucible_state,
		//		[Source.Owned, Pair.Component<Crucible.State>, Optional(true)] ref Sound.Emitter sound_emitter,
		//		[Source.Owned, Pair.Component<Crucible.State>, Optional(true)] ref Light.Data light)
		//		{

		//		}
		//#endif
	}
}
