
namespace TC2.Base.Components
{
	public static partial class Cable
	{
		[IComponent.Data(Net.SendType.Unreliable, IComponent.Scope.Region)]
		public partial struct Data(): IComponent
		{
			public float stability;
		}
	}
}
