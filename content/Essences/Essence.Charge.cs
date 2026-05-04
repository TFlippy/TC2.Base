
namespace TC2.Base.Components
{
	public static partial class Essence
	{
		public static partial class Charge
		{
			[Flags]
			public enum Flags: byte
			{
				None = 0,


			}

			public enum Type: byte
			{
				Undefined = 0,


			}

			[ITrait.Data(Net.SendType.Unreliable, IComponent.Scope.Global | IComponent.Scope.Region)]
			public partial struct Data(): ITrait
			{
				public IEssence.Handle h_essence;
				public Essence.Charge.Flags flags;
				public Essence.Charge.Type type;

				[Save.NewLine]
				public float unused_01;
				public float amount;
				[Editor.Slider.Clamped(min: 0.00f, max: 16.00f, snap: 0.001f)]
				public float radius;
			}

			[ISystem.Update.A(ISystem.Mode.Single, ISystem.Scope.Region)]
			public static void OnUpdate_A(ISystem.Info info, ref XorRandom random, ref Region.Data region, Entity entity,
			[Source.Owned] ref Transform.Data transform,
			[Source.Owned, Pair.Wildcard] ref Essence.Charge.Data charge)
			{

			}

#if CLIENT
			[ISystem.PostUpdate.A(ISystem.Mode.Single, ISystem.Scope.Region)]
			public static void OnPostUpdate_A(ISystem.Info info, ref XorRandom random, ref Region.Data region, Entity entity,
			[Source.Owned] in Transform.Data transform,
			[Source.Owned, Pair.Wildcard] ref Essence.Charge.Data charge)
			{

			}
#endif
		}









	}
}
