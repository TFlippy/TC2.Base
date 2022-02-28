
namespace TC2.Base.Components
{
	public static partial class AutomaticReload
	{
		[IComponent.Data(Net.SendType.Reliable)]
		public partial struct Data: IComponent
		{

		}

		[ISystem.EarlyUpdate(ISystem.Mode.Single)]
		public static void Update(ISystem.Info info, Entity entity, 
		[Source.Owned] ref Gun.State gun_state, [Source.Owned] ref Gun.Data gun, [Source.Owned] in AutomaticReload.Data automatic_reload, [Source.Owned, Trait.Of<Gun.Data>] ref Inventory1.Data inventory_magazine)
		{
			if (gun_state.stage != Gun.Stage.Reloading && inventory_magazine.resource.quantity < gun.ammo_per_shot)
			{
				gun_state.stage = Gun.Stage.Reloading;
			}
		}
	}
}
