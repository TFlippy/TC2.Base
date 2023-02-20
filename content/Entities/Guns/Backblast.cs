namespace TC2.Base.Components
{
	public static partial class Backblast
	{
		[IComponent.Data(Net.SendType.Reliable), IComponent.With<Gun.State>]
		public partial struct Data: IComponent
		{
			[Editor.Picker.Position(true, true)]
			public Vector2 exhaust_offset = default;

			public Data()
			{

			}
		}

		[ISystem.EarlyUpdate(ISystem.Mode.Single)]
		public static void OnUpdate(ISystem.Info info, Entity entity,
		[Source.Owned] in Transform.Data transform, [Source.Owned] in Control.Data control, [Source.Owned] ref Body.Data body,
		[Source.Owned] ref Gun.Data gun, [Source.Owned] ref Gun.State gun_state,
		[Source.Owned] in Backblast.Data backblast)
		{
			if (gun_state.stage == Gun.Stage.Fired)
			{
				ref var region = ref info.GetRegion();

				var pos = transform.LocalToWorld(backblast.exhaust_offset);
				var dir = -transform.GetDirection();

#if SERVER
				Span<LinecastResult> results = stackalloc LinecastResult[8];
				if (region.TryLinecastAll(pos + (dir * 0.50f), pos + (dir * 2.00f), 0.50f, ref results, mask: Physics.Layer.Destructible | Physics.Layer.World))
				{
					results.SortByDistance();

					var ent_owner = body.GetParent();

					foreach (ref var result in results)
					{
						if (result.layer.HasAny(Physics.Layer.World))
						{


							break;
						}
						else
						{
							switch (result.material_type)
							{
								case Material.Type.Flesh:
								case Material.Type.Insect:
								case Material.Type.Fabric:
								case Material.Type.Paper:
								case Material.Type.Foliage:
								case Material.Type.Mushroom:
								case Material.Type.Rubber:
								case Material.Type.Wood:
								{
									//entity.Hit(ent_owner, result.entity, result.world_position, dir, result.normal, 1000.00f, result.material_type, Damage.Type.Explosion, knockback: 4.00f);

									ref var burning = ref result.entity.GetOrAddComponent<Burning.Data>(sync: true);
									if (!burning.IsNull())
									{
										burning.radius = 1.00f;
										burning.flicker = 0.03f;
									}
								}
								break;
							}
						}
					}
				}
#endif
			}
		}
	}
}
