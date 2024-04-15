
namespace TC2.Base.Components
{
	public static partial class Cable
	{
		[IComponent.Data(Net.SendType.Unreliable, region_only: true)]
		public partial struct Data: IComponent
		{
			public float stability;

			public Data()
			{

			}
		}
	}
}
