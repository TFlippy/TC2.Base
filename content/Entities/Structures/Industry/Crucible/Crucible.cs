﻿namespace TC2.Base.Components
{
	public static partial class Crucible
	{
		[IComponent.Data(Net.SendType.Reliable)]
		public struct Data: IComponent
		{
			[Flags]
			public enum Flags: uint
			{
				None = 0,

				Use_Misc = 1 << 0
			}

			public Crucible.Data.Flags flags;

			[Save.Ignore, Net.Ignore] public float t_next_update;

			public Data()
			{

			}
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
		//			public void Invoke(ref NetConnection connection, Entity entity, ref Crucible.State data)
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
		[Source.Owned] in Transform.Data transform,
		[Source.Owned] in Heat.Data heat, [Source.Owned] ref Heat.State heat_state, 
		[Source.Owned] ref Crucible.Data crucible)
		{

		}

		//[ISystem.Update.B(ISystem.Mode.Single, ISystem.Scope.Region)]
		//public static void UpdateEssence(ISystem.Info info, ref Region.Data region, ref XorRandom random, Entity entity,
		//[Source.Owned] in Transform.Data transform, [Source.Owned] ref EssenceContainer.Data essence_container,
		//[Source.Owned, Pair.Component<EssenceContainer.Data>] ref Crucible.Data crucible, IComponent.Handle h_crucible)
		//{
		//	var time = info.WorldTime;
		//	if (time >= crucible.t_next_update)
		//	{
		//		ref var essence_data = ref essence_container.h_essence.GetData();
		//		if (essence_data.IsNotNull())
		//		{
		//			crucible.temperature_target += (essence_data.heat_emit * essence_container.rate * essence_container.available) * info.DeltaTime;
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
			public Heat.Data heat;
			public Heat.State heat_state;
			public Crucible.Data crucible;

			public void Draw()
			{
				using (var window = GUI.Window.InteractionMisc("Crucible"u8, this.ent_crucible, size: new(24, 96 * 1)))
				{
					this.StoreCurrentWindowTypeID(order: -100);
					if (window.show)
					{
						using (GUI.Group.New(size: new Vector2(GUI.RmX, GUI.RmY)))
						{

						}
					}
				}
			}
		}

		[ISystem.GUI(ISystem.Mode.Single, ISystem.Scope.Region)]
		public static void OnGUI(Entity entity,
		[Source.Owned] in Interactable.Data interactable, [Source.Owned] in Transform.Data transform,
		[Source.Owned] in Heat.Data heat, [Source.Owned] in Heat.State heat_state, [Source.Owned] in Crucible.Data crucible)
		{
			if (interactable.IsActive())
			{
				var gui = new CrucibleGUI()
				{
					ent_crucible = entity,

					transform = transform,
					heat = heat,
					heat_state = heat_state,
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