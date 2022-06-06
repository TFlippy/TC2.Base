
namespace TC2.Base.Components
{
	public static partial class MovementCooling
	{
		[IComponent.Data(Net.SendType.Reliable, name: "Movement Cooling")]
		public partial struct Data: IComponent
		{
			[Statistics.Info("Multiplier", description: "TODO: Desc", format: "{0:0.00}x", comparison: Statistics.Comparison.Higher, priority: Statistics.Priority.Medium)]
			public float modifier;

			[Save.Ignore, Net.Ignore] public float next_sync;
		}

		public const float sync_interval = 1.00f;

		[ISystem.LateUpdate(ISystem.Mode.Single)]
		public static void Update(ISystem.Info info, Entity entity,
		[Source.Owned] ref Overheat.Data overheat, [Source.Owned] in Body.Data body, [Source.Owned] ref MovementCooling.Data movement_cooling)
		{
			if (overheat.heat_current <= 0.01f || body.IsSleeping()) return;

			var amount = ((body.GetAngularVelocity() * 5.00f) + body.GetVelocity().Length()) * movement_cooling.modifier;
			if (amount > 5.00f)
			{
				overheat.heat_current = Maths.MoveTowards(overheat.heat_current, 30.00f, (amount * info.DeltaTime) / MathF.Max(body.GetMass() * 0.05f, 1.00f));

#if SERVER
				if (info.WorldTime >= movement_cooling.next_sync)
				{
					movement_cooling.next_sync = info.WorldTime + MovementCooling.sync_interval;
					overheat.Sync(entity);
				}
#endif
			}
		}
	}
}
