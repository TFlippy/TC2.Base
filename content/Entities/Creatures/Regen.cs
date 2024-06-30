
namespace TC2.Base.Components
{
	public static partial class Regen
	{
		[Flags]
		public enum Flags: uint
		{
			None = 0u
		
			
		}

		[IComponent.Data(Net.SendType.Reliable, region_only: true), IComponent.With<Regen.State>]
		public partial struct Data: IComponent, IOverridable
		{
			public float amount;
			public float interval = 3.00f;

			public float min_a = 0.70f;
			public float max_a = 1.00f;
			public float multiplier_a = 0.50f;

			public float min_b = 0.70f;
			public float max_b = 1.00f;
			public float multiplier_b = 1.00f;

			public Data()
			{

			}
		}

		[IComponent.Data(Net.SendType.Unreliable, region_only: true)]
		public partial struct State: IComponent
		{
			[Save.Ignore, Net.Ignore] public float next_regen;

			public State()
			{

			}
		}

#if SERVER
		[ISystem.PostUpdate.C(ISystem.Mode.Single, ISystem.Scope.Region, interval: 0.22f), HasTag("dead", false, Source.Modifier.Owned)]
		public static void OnUpdateOrganic(ISystem.Info info, Entity entity, ref XorRandom random,
		[Source.Owned, Override] in Organic.Data organic, [Source.Owned] ref Health.Data health, 
		[Source.Owned, Override] in Regen.Data regen, [Source.Owned] ref Regen.State regen_state, 
		[Source.Owned] in Transform.Data transform)
		{
			if (info.WorldTime >= regen_state.next_regen)
			{
				if (health.integrity < 1.00f || health.durability < 1.00f)
				{
					entity.Heal(ent_healer: entity,
						ent_owner: entity,
						world_position: transform.position,
						amount: regen.amount * random.NextFloatRange(0.80f, 1.00f),
						min_a: regen.min_a,
						max_a: regen.max_a,
						min_b: regen.min_b,
						max_b: regen.max_b,
						multiplier_a: organic.regeneration * regen.multiplier_a,
						multiplier_b: organic.regeneration * regen.multiplier_b);

					regen_state.next_regen = info.WorldTime + regen.interval;
				}
				else
				{
					regen_state.next_regen = info.WorldTime + 10.00f;
				}
			}
		}

		[ISystem.PostUpdate.C(ISystem.Mode.Single, ISystem.Scope.Region, interval: 0.22f), HasTag("wrecked", false, Source.Modifier.Owned), HasComponent<Organic.Data>(Source.Modifier.Owned, false)]
		public static void OnUpdate(ISystem.Info info, Entity entity, ref XorRandom random,
		[Source.Owned] ref Health.Data health,
		[Source.Owned, Override] in Regen.Data regen, [Source.Owned] ref Regen.State regen_state,
		[Source.Owned] in Transform.Data transform)
		{
			if (info.WorldTime >= regen_state.next_regen)
			{
				if (health.integrity < 1.00f || health.durability < 1.00f)
				{
					entity.Heal(ent_healer: entity,
						ent_owner: entity,
						world_position: transform.position,
						amount: regen.amount * random.NextFloatRange(0.80f, 1.00f),
						min_a: regen.min_a,
						max_a: regen.max_a,
						min_b: regen.min_b,
						max_b: regen.max_b,
						multiplier_a: regen.multiplier_a,
						multiplier_b: regen.multiplier_b);

					regen_state.next_regen = info.WorldTime + regen.interval;
				}
				else
				{
					regen_state.next_regen = info.WorldTime + 10.00f;
				}
			}
		}
#endif
	}
}