
namespace TC2.Base.Components
{
	public static partial class MovementCooling
	{
		[IComponent.Data(Net.SendType.Reliable)]
		public partial struct Data: IComponent
		{
			public float modifier;
			[Save.Ignore, Net.Ignore] public float next_sync;
		}

		public const float sync_interval = 1.00f;

		[ISystem.LateUpdate(ISystem.Mode.Single)]
		public static void Update(ISystem.Info info, Entity entity,
		[Source.Owned] ref Overheat.Data overheat, [Source.Owned] in Body.Data body, [Source.Owned] ref MovementCooling.Data movement_cooling)
		{
			if (body.IsSleeping()) return;

			var amount = ((body.GetAngularVelocity() * 2.00f) + body.GetVelocity().Length()) * movement_cooling.modifier;
			if (amount > 5.00f)
			{
				overheat.heat_current -= MathF.Min(overheat.heat_current, amount);

#if SERVER
				if (info.WorldTime >= movement_cooling.next_sync && overheat.heat_current > 0.00f)
				{
					movement_cooling.next_sync = info.WorldTime + MovementCooling.sync_interval;
					overheat.Sync(entity);
				}
#endif
			}
		}
	}
}
