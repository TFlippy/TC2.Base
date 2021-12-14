
namespace TC2.Base.Components
{
	public static partial class MovementCooling
	{

		[IComponent.Data(Net.SendType.Reliable)]
		public partial struct Data: IComponent
		{
			public float mod;
		}

		[ISystem.LateUpdate(ISystem.Mode.Single)]
		public static void Update(ISystem.Info info, Entity entity,
		[Source.Owned] ref Overheat.Data overheat, [Source.Owned] ref Physics.Data physics,
		[Source.Owned] in MovementCooling.Data movementCooling)
		{
			float amount = physics.angular_velocity + physics.velocity.Length();
			if (amount > 5.00f)
			{
				overheat.heat_current -= MathF.Min(overheat.heat_current, amount * movementCooling.mod);
			}
		}
	}
}
