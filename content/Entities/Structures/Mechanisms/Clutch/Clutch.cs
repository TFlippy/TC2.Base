
namespace TC2.Base.Components
{
	public static partial class Clutch
	{
		[IComponent.Data(Net.SendType.Unreliable, region_only: true)]
		public partial struct Data: IComponent
		{
			public Vector2 offset_enabled;
			public Vector2 offset_disabled;

			public int state;
			public float speed = 0.125f;

			public Sound.Handle sound_enable;
			public Sound.Handle sound_disable;

			public float modifier;
			public float modifier_target;

			[Save.Ignore, Net.Ignore] public float t_next_switch;

			public Data()
			{

			}
		}

		public struct ConfigureRPC: Net.IRPC<Clutch.Data>
		{
			public int state;

#if SERVER
			public void Invoke(ref NetConnection connection, Entity entity, ref Clutch.Data data)
			{
				ref var region = ref entity.GetRegion();
				if (region.GetWorldTime() >= data.t_next_switch)
				{
					data.t_next_switch = region.GetWorldTime() + 0.30f;

					var sync = false;

					if (data.state.TrySet(this.state))
					{
						ref var transform = ref entity.GetComponent<Transform.Data>();
						if (!transform.IsNull())
						{
							Sound.Play(ref region, this.state == 0 ? data.sound_disable : data.sound_enable, transform.position);
							Shake.Emit(ref region, transform.position, 0.30f, 0.30f, 8.00f);

							if (this.state == 0)
							{
								WorldNotification.Push(ref region, $"* Off *", Color32BGRA.Red, position: transform.position, velocity: new(0.00f, 4.00f), lifetime: 0.70f);
							}
							else
							{
								WorldNotification.Push(ref region, $"* On *", Color32BGRA.Green, position: transform.position, velocity: new(0.00f, -4.00f), lifetime: 0.70f);
							}
						}

						sync = true;
					}

					if (sync)
					{
						data.Sync(entity);
					}
				}
			}
#endif
		}

		[ISystem.EarlyUpdate(ISystem.Mode.Single, ISystem.Scope.Region)]
		public static void Update(ISystem.Info info, Entity entity,
		[Source.Owned] ref Axle.Data axle, [Source.Owned] ref Axle.State axle_state,
		[Source.Owned] in Transform.Data transform, [Source.Owned] ref Clutch.Data clutch)
		{
			if (clutch.state == 0)
			{
				clutch.modifier_target = 0.00f;
			}
			else
			{
				clutch.modifier_target = 1.00f;
			}

			clutch.modifier = Maths.MoveTowards(clutch.modifier, clutch.modifier_target, clutch.speed);

			axle.modifier = clutch.modifier;
			//axle.offset_inner = clutch.offset_disabled * (1.00f - clutch.modifier);
			axle.offset_inner = Vector2.Lerp(clutch.offset_disabled, clutch.offset_enabled, clutch.modifier);
		}

#if CLIENT
		[ISystem.Update(ISystem.Mode.Single, ISystem.Scope.Region, interval: 0.20f)]
		public static void UpdateEffects(ISystem.Info info, Entity entity,
		[Source.Owned] in Transform.Data transform, [Source.Owned] ref Clutch.Data clutch, [Source.Owned, Pair.Of<Clutch.Data>] ref Animated.Renderer.Data renderer)
		{
			renderer.sprite.frame.X = (uint)clutch.state;
		}

		public partial struct ClutchGUI: IGUICommand
		{
			public Entity ent_clutch;
			public Clutch.Data clutch;
			public Axle.Data axle;
			public Axle.State axle_state;

			public void Draw()
			{
				using (var window = GUI.Window.InteractionMisc("Clutch", this.ent_clutch, new Vector2(48, 96)))
				{
					//this.StoreCurrentWindowTypeID();
					if (window.show)
					{
						if (GUI.SliderInt("State", ref this.clutch.state, 0, 1, size: new Vector2(GUI.RmX, GUI.RmY), snap: 1, vertical: true))
						{
							var rpc = new Clutch.ConfigureRPC()
							{
								state = this.clutch.state
							};
							rpc.Send(this.ent_clutch);
						}
					}
				}
			}
		}

		[ISystem.EarlyGUI(ISystem.Mode.Single, ISystem.Scope.Region)]
		public static void OnGUI(Entity entity, [Source.Owned] in Clutch.Data clutch,
		[Source.Owned] in Axle.Data axle, [Source.Owned] in Axle.State axle_state, [Source.Owned] in Interactable.Data interactable)
		{
			if (interactable.show)
			{
				var gui = new ClutchGUI()
				{
					ent_clutch = entity,
					clutch = clutch,
					axle = axle,
					axle_state = axle_state
				};
				gui.Submit();
			}
		}
#endif
	}
}
