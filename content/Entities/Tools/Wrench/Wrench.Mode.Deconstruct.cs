
using System.Diagnostics.CodeAnalysis;

namespace TC2.Base.Components
{
	public static partial class Wrench
	{
		public static partial class Mode
		{
			public static partial class Deconstruct
			{
				[IComponent.Data(Net.SendType.Reliable)]
				public partial struct Data: IComponent, Wrench.IMode, Wrench.ITargeterMode<TargetInfo>
				{
					[Save.Ignore] public Entity ent_target;

					[UnscopedRef]
					public ref Entity EntTarget => ref this.ent_target;

					public TargetInfo CreateTargetInfo(Entity entity)
					{
						return new TargetInfo(entity);
					}

					public static Sprite Icon { get; } = new Sprite("ui_icons.wrench", 0, 1, 24, 24, 1, 0);
					public Crafting.Recipe.Tags RecipeTags => Crafting.Recipe.Tags.Duct;
					public Physics.Layer LayerMask => Physics.Layer.Duct;

#if CLIENT
					public void SendSetTargetRPC(Entity ent_wrench, Entity ent_target)
					{
						var rpc = new SetTargetRPC
						{
							ent_target = ent_target
						};
						rpc.Send(ent_wrench);
					}

					public void DrawInfo(Entity ent_wrench, ref TargetInfo info)
					{

					}

					public void DrawHUD(Entity ent_wrench, ref TargetInfo info_target)
					{
						using (var hud = GUI.Window.Standalone("Wrench.HUD", position: info_target.pos.WorldToCanvas(), size: new(168, 100), pivot: new(0.50f, 0.50f)))
						{
							if (hud.show)
							{
								GUI.DrawBackground(GUI.tex_panel, hud.group.GetOuterRect(), padding: new(4));

								using (GUI.Group.New(size: GUI.GetRemainingSpace() - new Vector2(0, 48), padding: new(4)))
								{

								}

								using (GUI.Group.Centered(outer_size: GUI.GetRemainingSpace(), inner_size: new(100, 40)))
								{
							
								}
							}
						}
					}
#endif
				}

				public struct SetTargetRPC: Net.IRPC<Wrench.Mode.Deconstruct.Data>
				{
					public Entity ent_target;

#if SERVER
					public void Invoke(ref NetConnection connection, Entity entity, ref Wrench.Mode.Deconstruct.Data data)
					{
						ref var region = ref entity.GetRegion();

						data.ent_target = this.ent_target;
						data.Sync(entity);
					}
#endif
				}

				public struct TargetInfo: ITargetInfo
				{
					public Entity entity;

					public Dismantlable.Data dismantlable;
					public Transform.Data transform;

					public float radius;
					public Vector2 pos;

					public bool alive;
					public bool valid;

					public Entity Entity => this.entity;
					public ulong ComponentID => ECS.GetID<Dismantlable.Data>();
					public Vector2 Position => this.pos;
					public float Radius => this.radius;
					public bool IsSource => true;
					public bool IsAlive => this.alive;
					public bool IsValid => this.valid;

					public TargetInfo(Entity entity)
					{
						this.entity = entity;
						this.alive = this.entity.IsAlive();

						if (this.alive)
						{
							this.valid = true;

							this.valid &= this.entity.GetComponent<Dismantlable.Data>().TryGetValue(out this.dismantlable);
							this.valid &= this.entity.GetComponent<Transform.Data>().TryGetValue(out this.transform);

							if (this.valid)
							{
								this.radius = 1.00f;
								this.pos = this.transform.position;
							}
						}
					}
				}
			}
		}
	}
}
