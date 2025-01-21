
namespace TC2.Base.Components
{
	public static partial class Repairable
	{
		[Flags]
		public enum Flags: uint
		{
			None = 0,
		}

		[IComponent.Data(Net.SendType.Unreliable, IComponent.Scope.Global | IComponent.Scope.Region)]
		public partial struct Data(): IComponent
		{
			public Repairable.Flags flags;
			public float cost_multiplier = 1.00f;
		}

		// [ISystem.AddFirst(ISystem.Mode.Single, ISystem.Scope.Region)]
		// public static void OnAddBody([Source.Owned] in Repairable.Data repairable, [Source.Owned] ref Body.Data body)
		// {
		// 	body.override_shape_layer |= Physics.Layer.Repairable;
		// 	body.flags &= ~Body.Flags.NonDirty;
		// }

		// [ISystem.RemoveLast(ISystem.Mode.Single, ISystem.Scope.Region)]
		// public static void OnRemoveBody([Source.Owned] in Repairable.Data repairable, [Source.Owned] ref Body.Data body)
		// {
		// 	body.override_shape_layer &= ~Physics.Layer.Repairable;
		// 	body.flags &= ~Body.Flags.NonDirty;
		// }
	}
}
