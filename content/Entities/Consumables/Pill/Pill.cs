
namespace TC2.Base.Components
{
	public static partial class Pill
	{
		[IComponent.Data(Net.SendType.Reliable)]
		public partial struct Data: IComponent
		{
			public float test;
		}
	}
}