﻿
namespace TC2.Base.Components
{
	public static partial class AutomaticReload
	{
		[IComponent.Data(Net.SendType.Reliable, name: "Auto-Reload", region_only: true)]
		public partial struct Data: IComponent
		{
			[Save.Ignore, Net.Ignore]
			public float next_reload;
		}

#if SERVER
		[ISystem.VeryLateUpdate(ISystem.Mode.Single, ISystem.Scope.Region, interval: 0.20f)]
		public static void Update(ISystem.Info info, Entity entity, 
		[Source.Owned] ref Gun.State gun_state, [Source.Owned] ref Gun.Data gun, [Source.Owned] ref AutomaticReload.Data automatic_reload, [Source.Owned, Pair.Component<Gun.Data>] ref Inventory1.Data inventory_magazine)
		{
			if (info.WorldTime >= automatic_reload.next_reload && gun_state.stage != Gun.Stage.Reloading && inventory_magazine.resource.quantity < gun.ammo_per_shot)
			{
				//gun_state.stage = Gun.Stage.Reloading;
				gun_state.hints.AddFlag(Gun.Hints.Wants_Reload);
				gun_state.Sync(entity, true);

				automatic_reload.next_reload = info.WorldTime + 3.00f;
			}
		}
#endif
	}
}
