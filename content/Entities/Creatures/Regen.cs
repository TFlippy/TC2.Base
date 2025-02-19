
namespace TC2.Base.Components
{
	public static partial class Regen
	{
		[Flags]
		public enum Flags: uint
		{
			None = 0u
		
			
		}

		[IComponent.Data(Net.SendType.Reliable, IComponent.Scope.Global | IComponent.Scope.Region)]
		public partial struct Data(): IComponent, IOverridable
		{
			public float amount;
			public float interval = 3.00f;

			public float min_a = 0.70f;
			public float max_a = 1.00f;
			public float multiplier_a = 0.50f;

			public float min_b = 0.70f;
			public float max_b = 1.00f;
			public float multiplier_b = 1.00f;

			[Save.Ignore, Net.Ignore] public float t_next_regen;
		}

#if SERVER
		[ISystem.Event<Health.DamageEvent>(ISystem.Mode.Single, ISystem.Scope.Global | ISystem.Scope.Region), HasTag("dead", false, Source.Modifier.Owned)]
		public static void OnDamage(ISystem.Info.Common info, ref XorRandom random, ref Health.DamageEvent ev,
		[Source.Owned, Original] ref Regen.Data regen, [Source.Owned] in Health.Data health)
		{
			regen.t_next_regen = Maths.Max(regen.t_next_regen, info.WorldTime + random.NextFloatExtra(2.00f, 8.00f));
		}

		[ISystem.PostUpdate.C(ISystem.Mode.Single, ISystem.Scope.Region, interval: 0.22f), HasTag("dead", false, Source.Modifier.Owned)]
		public static void OnUpdateOrganic(ISystem.Info info, Entity entity, ref XorRandom random,
		[Source.Owned, Original] ref Regen.Data regen, [Source.Owned, Override] in Regen.Data regen_override, [Source.Owned, Override] in Organic.Data organic_override,
		[Source.Owned] in Transform.Data transform, [Source.Owned] ref Health.Data health)
		{
			if (info.WorldTime >= regen.t_next_regen)
			{
				if (health.integrity < 1.00f || health.durability < 1.00f)
				{
					entity.Heal(ent_healer: entity,
						ent_owner: entity,
						world_position: transform.position,
						amount: regen_override.amount * random.NextFloatRange(0.80f, 1.00f),
						min_a: regen_override.min_a,
						max_a: regen_override.max_a,
						min_b: regen_override.min_b,
						max_b: regen_override.max_b,
						multiplier_a: organic_override.regeneration * regen_override.multiplier_a,
						multiplier_b: organic_override.regeneration * regen_override.multiplier_b);

					regen.t_next_regen = info.WorldTime + regen_override.interval;
				}
				else
				{
					regen.t_next_regen = info.WorldTime + 10.00f;
				}
			}
		}

		[ISystem.PostUpdate.C(ISystem.Mode.Single, ISystem.Scope.Region, interval: 0.22f), HasTag("wrecked", false, Source.Modifier.Owned), HasComponent<Organic.Data>(Source.Modifier.Owned, false)]
		public static void OnUpdate(ISystem.Info info, Entity entity, ref XorRandom random,
		[Source.Owned, Original] ref Regen.Data regen, [Source.Owned, Override] in Regen.Data regen_override,
		[Source.Owned] in Transform.Data transform, [Source.Owned] ref Health.Data health)
		{
			if (info.WorldTime >= regen.t_next_regen)
			{
				if (health.integrity < 1.00f || health.durability < 1.00f)
				{
					entity.Heal(ent_healer: entity,
						ent_owner: entity,
						world_position: transform.position,
						amount: regen_override.amount * random.NextFloatRange(0.80f, 1.00f),
						min_a: regen_override.min_a,
						max_a: regen_override.max_a,
						min_b: regen_override.min_b,
						max_b: regen_override.max_b,
						multiplier_a: regen_override.multiplier_a,
						multiplier_b: regen_override.multiplier_b);

					regen.t_next_regen = info.WorldTime + regen_override.interval;
				}
				else
				{
					regen.t_next_regen = info.WorldTime + 10.00f;
				}
			}
		}
#endif
	}
}