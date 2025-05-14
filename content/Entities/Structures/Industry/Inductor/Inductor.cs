namespace TC2.Base.Components
{
	public static partial class Inductor
	{
		[IComponent.Data(Net.SendType.Reliable), IComponent.With<Inductor.State>]
		public struct Data(): IComponent
		{
			[Flags]
			public enum Flags: uint
			{
				None = 0,

				Use_Misc = 1 << 0
			}

			public Inductor.Data.Flags flags;

			public IMaterial.Handle h_material_coil;

			public float coil_length;
			public float coil_thickness;
			public byte coil_turns;


			//public float frequency;

			[Save.Ignore, Net.Ignore] public float t_next_update;
		}

		[IComponent.Data(Net.SendType.Unreliable)]
		public struct State(): IComponent
		{
			public float frequency_current;
			public float frequency_target;

			public Volume coil_volume;
			public Mass coil_mass;

			public Power power_current;
		}

		// https://en.wikipedia.org/wiki/Inductance#Inductance_of_a_solenoid

		//[IComponent.Data(Net.SendType.Unreliable)]
		//public struct State: IComponent
		//{
		//	public Temperature temperature_target;
		//	public Temperature temperature_current;

		//	[Save.Ignore, Net.Ignore] public float t_next_update;
		//}

		//		public struct ConfigureRPC: Net.IRPC<Inductor.State>
		//		{
		//#if SERVER
		//			public void Invoke(Net.IRPC.Context rpc, ref Inductor.State data)
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
		public const float update_interval_failed = 3.00f;

		public static readonly Texture.Handle texture_smoke = "BiggerSmoke_Light";

		[ISystem.PostUpdate.C(ISystem.Mode.Single, ISystem.Scope.Region)]
		public static void Update(ISystem.Info info, ref Region.Data region, ref XorRandom random, Entity entity,
		[Source.Owned] in Transform.Data transform, 
		[Source.Owned] ref Body.Data body, [Source.Owned, Pair.Component<Inductor.Data>] ref Shape.Line shape,
		[Source.Owned] ref Essence.Container.Data essence_container,
		[Source.Owned] ref Inductor.Data inductor, [Source.Owned] ref Inductor.State inductor_state)
		{
			var time = info.WorldTime;
			if (time >= inductor.t_next_update)
			{
				//inductor.t_next_update = time + update_interval;

				if (body.HasArbiters())
				{
					var power = essence_container.GetElectricPower(); // essence_container.GetEmittedEssenceAmount();

					foreach (var arbiter in body.GetArbiters())
					{
						if (arbiter.HasShape(shape))
						{
							var ent_arbiter = arbiter.GetEntity();

							// TODO: this is slow
							ref var heat_state = ref ent_arbiter.GetComponent<Heat.State>();
							if (heat_state.IsNotNull())
							{
								heat_state.AddPower(power, info.DeltaTime);
							}
						}
					}	
				}
			}
		}

		//[ISystem.Update.B(ISystem.Mode.Single, ISystem.Scope.Region)]
		//public static void UpdateEssence(ISystem.Info info, ref Region.Data region, ref XorRandom random, Entity entity,
		//[Source.Owned] in Transform.Data transform, [Source.Owned] ref Essence.Container.Data essence_container,
		//[Source.Owned, Pair.Component<Essence.Container.Data>] ref Inductor.Data inductor, IComponent.Handle h_inductor)
		//{
		//	var time = info.WorldTime;
		//	if (time >= inductor.t_next_update)
		//	{
		//		ref var essence_data = ref essence_container.h_essence.GetData();
		//		if (essence_data.IsNotNull())
		//		{
		//			inductor.temperature_target += (essence_data.heat_emit * essence_container.rate * essence_container.available) * info.DeltaTime;
		//		}
		//		//App.WriteLine("a");

		//		//inductor.t_next_update = time + update_interval;
		//	}
		//}

#if CLIENT
		public struct InductorGUI: IGUICommand
		{
			public Entity ent_inductor;

			public Transform.Data transform;

			public Inductor.Data inductor;

			public void Draw()
			{
				using (var window = GUI.Window.Interaction("Inductor"u8, this.ent_inductor))
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
		[Source.Owned] in Inductor.Data inductor)
		{
			if (interactable.IsActive())
			{
				var gui = new InductorGUI()
				{
					ent_inductor = entity,
					transform = transform,
					inductor = inductor,
				};
				gui.Submit();
			}
		}
#endif

		//#if CLIENT
		//		[ISystem.VeryLateUpdate(ISystem.Mode.Single, ISystem.Scope.Region)]
		//		public static void UpdateEffects(ISystem.Info info, ref Region.Data region, ref XorRandom random,
		//		[Source.Owned] in Transform.Data transform,
		//		[Source.Owned] in Inductor.Data inductor, [Source.Owned] ref Inductor.State inductor_state,
		//		[Source.Owned, Pair.Component<Inductor.State>, Optional(true)] ref Sound.Emitter sound_emitter,
		//		[Source.Owned, Pair.Component<Inductor.State>, Optional(true)] ref Light.Data light)
		//		{

		//		}
		//#endif
	}
}
