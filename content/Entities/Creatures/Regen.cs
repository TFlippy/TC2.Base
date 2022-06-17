
namespace TC2.Base.Components
{
	public static partial class Regen
	{
		[IComponent.Data(Net.SendType.Unreliable)]
		public partial struct Data: IComponent
		{
			public float amount = default;
			public float interval = 3.00f;

			public float min_a = 0.70f;
			public float max_a = 1.00f;
			public float multiplier_a = 0.50f;

			public float min_b = 0.70f;
			public float max_b = 1.00f;
			public float multiplier_b = 1.00f;

			[Save.Ignore, Net.Ignore] public float next_regen = default;

			public Data()
			{

			}
		}

#if SERVER
		[ISystem.Update(ISystem.Mode.Single), HasTag("dead", false, Source.Modifier.Owned)]
		public static void OnUpdate(ISystem.Info info, Entity entity, [Source.Owned, Override] in Organic.Data organic, [Source.Owned] ref Health.Data health, [Source.Owned] ref Regen.Data regen, [Source.Owned] in Transform.Data transform)
		{
			if (info.WorldTime >= regen.next_regen)
			{
				regen.next_regen = info.WorldTime + regen.interval;

				if (health.integrity < 1.00f || health.durability < 1.00f)
				{
					entity.Heal(entity, entity, transform.position, regen.amount, min_a: regen.min_a, max_a: regen.max_a, min_b: regen.min_b, max_b: regen.max_b);
				}
			}
		}
#endif
	}
}