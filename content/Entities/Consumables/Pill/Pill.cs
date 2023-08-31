
namespace TC2.Base.Components
{
	public static partial class Pill
	{
		[IComponent.Data(Net.SendType.Reliable, region_only: true)]
		public partial struct Data: IComponent
		{
			public float test;
		}
	}
}