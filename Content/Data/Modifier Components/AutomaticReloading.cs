
namespace TC2.Base.Components
{
	public static partial class AutomaticReloading
	{

		[IComponent.Data(Net.SendType.Reliable)]
		public partial struct Data: IComponent
		{

		}

		[ISystem.EarlyUpdate(ISystem.Mode.Single)]
		public static void PreUpdate(ISystem.Info info, Entity entity,
		[Source.Owned] ref Control.Data control, [Source.Owned] in AutomaticReloading.Data randomActivations, 
		[Source.Owned] ref Gun.State gunState, [Source.Owned] ref Gun.Data gunData, [Source.Owned, Trait.Of<Gun.Data>] ref Inventory1.Data inventory_magazine)
		{
			if (inventory_magazine.resource.quantity < gunData.ammo_per_shot)
			{
				gunState.stage = Gun.Stage.Reloading;
			}
		}
	}
}
