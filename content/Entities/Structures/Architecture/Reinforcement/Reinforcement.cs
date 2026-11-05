
namespace TC2.Base.Components
{
	public static partial class Reinforcement
	{
		[IComponent.Data(Net.SendType.Reliable, scope: IComponent.Scope.Region)]
		public struct Data: IComponent
		{
			public float unused_00;
			public float unused_01;
			public float unused_02;
			public float unused_03;
		}
	}
}