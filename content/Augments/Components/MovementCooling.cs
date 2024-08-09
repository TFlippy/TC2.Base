
namespace TC2.Base.Components
{
	[Obsolete("This is now part of Heat component's base functionality.")]
	public static partial class MovementCooling
	{
		[IComponent.Data(Net.SendType.Reliable, name: "Movement Cooling", region_only: true)]
		public partial struct Data: IComponent
		{
			[Statistics.Info("Multiplier", description: "TODO: Desc", format: "{0:0.00}x", comparison: Statistics.Comparison.Higher, priority: Statistics.Priority.Medium)]
			public float modifier;

			[Save.Ignore, Net.Ignore] public float next_sync;
		}

		public const float sync_interval = 1.00f;

//		[ISystem.LateUpdate(ISystem.Mode.Single, ISystem.Scope.Region, interval: 0.10f)]
//		public static void Update(ISystem.Info info, Entity entity,
//		[Source.Owned] ref Heat.State heat_state, [Source.Owned] in Body.Data body, [Source.Owned] ref MovementCooling.Data movement_cooling)
//		{
//			if (heat_state.temperature_current < 400.00f || body.IsSleeping()) return;

//			var amount = ((body.GetAngularVelocity() * 5.00f) + body.GetVelocity().Length()) * movement_cooling.modifier;
//			if (amount > 5.00f)
//			{
//				heat_state.temperature_current = Maths.MoveTowards(heat_state.temperature_current, 30.00f, (amount * info.DeltaTime) / Maths.Max(body.GetMass() * 0.05f, 1.00f));

//#if SERVER
//				if (info.WorldTime >= movement_cooling.next_sync)
//				{
//					movement_cooling.next_sync = info.WorldTime + MovementCooling.sync_interval;
//					heat_state.Sync(entity, true);
//				}
//#endif
//			}
//		}
	}
}
