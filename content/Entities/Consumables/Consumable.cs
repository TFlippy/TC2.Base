
using System.Numerics;

namespace TC2.Base.Components
{
	public static partial class Consumable
	{
		[Flags]
		public enum Flags: uint
		{
			None = 0,

			Enable_Use_On_Self = 1u << 0,
			Enable_Use_On_Others = 1u << 1,

			Separate_Uses = 1u << 2,
			Consume_On_Interact = 1u << 3,

			Enable_Sprite_Frames = 1u << 4,
			Hide_Message = 1u << 5
		}

		public enum Action: uint
		{
			Undefined = 0,

			Eat,
			Drink,
			Inject,
			Inhale,
			Smoke,
			Open
		}

		[IComponent.Data(Net.SendType.Reliable, region_only: true)]
		public partial struct Data: IComponent
		{
			public Consumable.Action action;
			public Consumable.Flags flags;

			public Sound.Handle sound_use;

			public int uses_max = 1;
			public int uses;

			[Statistics.Info("Release Rate", description: "TODO: Desc", format: "{0:0.##} ml/s", comparison: Statistics.Comparison.None, priority: Statistics.Priority.Medium)]
			public float release_rate;

			[Statistics.Info("Release Step", description: "TODO: Desc", format: "{0:0.##} ml/s", comparison: Statistics.Comparison.None, priority: Statistics.Priority.Medium)]
			public float release_step;

			public Data()
			{

			}
		}

		[IEvent.Data]
		public partial struct ConsumeEvent: IEvent
		{
			public Entity ent_organic;
			public Entity ent_holder;
			public Entity ent_consumable;
			public Entity ent_target;
			public Vector2 world_position;

			public float amount_modifier = 1.00f;

			public ConsumeEvent()
			{

			}
		}

		public static uint GetFrame(int uses, int uses_max, uint frame_count)
		{
			return (uint)Maths.Clamp(MathF.Ceiling(MathF.Log2((1.00f - ((float)uses / Maths.Max((float)uses_max, 1.00f))) * frame_count) + 1), 0.00f, (int)frame_count - 1);
		}

		[ISystem.Modified.Component<Consumable.Data>(ISystem.Mode.Single, ISystem.Scope.Region)]
		public static void UpdateSprite([Source.Owned] in Consumable.Data consumable, [Source.Owned] ref Animated.Renderer.Data renderer)
		{
			if (consumable.flags.HasAny(Consumable.Flags.Enable_Sprite_Frames))
			{
				var x = GetFrame(consumable.uses, consumable.uses_max, renderer.sprite.count);
				renderer.sprite.frame.X = x;
			}
		}

#if SERVER
		[ISystem.Event<Interactable.InteractEvent>(ISystem.Mode.Single, ISystem.Scope.Region)]
		public static void OnInteract(ISystem.Info info, Entity entity, ref XorRandom random, ref Region.Data region, [Source.Owned] ref Interactable.InteractEvent data,
		[Source.Owned] ref Interactable.Data interactable, [Source.Owned] ref Consumable.Data consumable, [Source.Owned] ref Transform.Data transform)
		{
			if (consumable.flags.HasAny(Consumable.Flags.Consume_On_Interact))
			{
				Consumable.Use(ref region, ent_consumable: entity, ent_holder: data.ent_interactor, ent_target: data.ent_interactor, ref consumable, transform.position);
			}
		}

		[ISystem.VeryLateUpdate(ISystem.Mode.Single, ISystem.Scope.Region), HasTag("dead", false, Source.Modifier.Parent)]
		public static void Update(ISystem.Info info, Entity entity, ref Region.Data region,
		[Source.Owned] in Transform.Data transform, [Source.Parent] in Transform.Data transform_parent, [Source.Owned] ref Consumable.Data consumable, [Source.Parent] in Control.Data control, [Source.Parent, Override] in Organic.Data organic, [Source.Parent] in Arm.Data arm)
		{
			if (consumable.flags.HasAny(Consumable.Flags.Enable_Use_On_Others) && control.mouse.GetKeyDown(Mouse.Key.Left))
			{
				if (Vector2.DistanceSquared(transform.position, control.mouse.position) < (3 * 3))
				{
					var ent_holder = entity.GetParent(Relation.Type.Child);
					if (ent_holder.IsAlive())
					{
						Span<OverlapResult> results = stackalloc OverlapResult[8];
						if (region.TryOverlapPointAll(control.mouse.position, 0.01f, ref results, mask: Physics.Layer.Organic | Physics.Layer.Creature))
						{
							ref var result_nearest = ref results.GetNearest();
							if (region.IsInLineOfSight(transform.position, result_nearest.world_position, 0.125f, mask: Physics.Layer.World | Physics.Layer.Solid, exclude: Physics.Layer.Ignore_Bullet | Physics.Layer.Ignore_Melee | Physics.Layer.Dynamic, query_flags: Physics.QueryFlag.Static))
							{
								var ent_target = result_nearest.entity;

								Consumable.Use(ref region, ent_consumable: entity, ent_holder: ent_holder, ent_target: ent_target, consumable: ref consumable, world_position: result_nearest.world_position);

								//var oc_organic = ent_target.GetComponentWithOwner<Organic.Data>(Relation.Type.Instance);
								//if (oc_organic.IsValid())
								//{
								//	Sound.Play(ref region, consumable.sound_use, result_nearest.world_position);

								//	consumable.uses++;

								//	var data = new Consumable.ConsumeEvent();
								//	data.ent_organic = oc_organic.entity;
								//	data.ent_holder = ent_holder;
								//	data.ent_consumable = entity;
								//	data.world_position = result_nearest.world_position;

								//	if (!consumable.flags.HasAny(Consumable.Flags.Separate_Uses))
								//	{
								//		data.amount_modifier /= consumable.uses_max;
								//	}

								//	var message = string.Empty;
								//	switch (consumable.action)
								//	{
								//		case Action.Eat: message = $" * Ate {entity.GetName()} *"; break;
								//		case Action.Drink: message = $"* Drank {entity.GetName()} *"; break;
								//		case Action.Inject: message = $"* Injected {entity.GetName()} *"; break;
								//		case Action.Inhale: message = $"* Inhaled {entity.GetName()} *"; break;
								//		case Action.Smoke: message = $"* Smoked {entity.GetName()} *"; break;
								//		default: message = $"* Used {entity.GetName()} *"; break;
								//	}

								//	WorldNotification.Push(ref region, message, Color32BGRA.Yellow, data.world_position, lifetime: 1.00f, send_type: Net.SendType.Unreliable);

								//	entity.Notify(ref data);

								//	if (consumable.uses >= consumable.uses_max)
								//	{
								//		entity.Delete();
								//	}
								//}
							}
						}
					}
				}
			}
			else if (consumable.flags.HasAny(Consumable.Flags.Enable_Use_On_Self) && control.mouse.GetKeyDown(Mouse.Key.Right))
			{
				var ent_holder = entity.GetParent(Relation.Type.Child);
				if (ent_holder.IsValid())
				{
					Consumable.Use(ref region, ent_consumable: entity, ent_holder: ent_holder, ent_target: ent_holder, consumable: ref consumable, world_position: transform_parent.position);
				}
			}
		}

		public static void Use(ref Region.Data region, Entity ent_consumable, Entity ent_holder, Entity ent_target, ref Consumable.Data consumable, Vector2 world_position)
		{
			if (ent_holder.IsAlive() && ent_target.IsAlive())
			{
				var oc_organic = ent_target.GetComponentWithOwner<Organic.Data>(Relation.Type.Instance);
				if (oc_organic.IsValid())
				{
					Sound.Play(ref region, consumable.sound_use, world_position);

					consumable.uses++;

					var data = new Consumable.ConsumeEvent();
					data.ent_organic = oc_organic.entity;
					data.ent_holder = ent_holder;
					data.ent_consumable = ent_consumable;
					data.ent_target = ent_target;
					data.world_position = world_position;

					if (!consumable.flags.HasAny(Consumable.Flags.Separate_Uses) && consumable.uses_max > 0)
					{
						data.amount_modifier /= consumable.uses_max;
					}

					if (!consumable.flags.HasAny(Consumable.Flags.Hide_Message))
					{
						var message = string.Empty;
						switch (consumable.action)
						{
							case Action.Eat: message = $" * Eats {ent_consumable.GetName()} *"; break;
							case Action.Drink: message = $"* Drinks {ent_consumable.GetName()} *"; break;
							case Action.Inject: message = $"* Injects {ent_consumable.GetName()} *"; break;
							case Action.Inhale: message = $"* Inhales {ent_consumable.GetName()} *"; break;
							case Action.Smoke: message = $"* Smokes {ent_consumable.GetName()} *"; break;
							case Action.Open: message = $"* Opens {ent_consumable.GetName()} *"; break;
							default: message = $"* Uses {ent_consumable.GetName()} *"; break;
						}

						WorldNotification.Push(ref region, message, Color32BGRA.Yellow, data.world_position, lifetime: 1.00f, send_type: Net.SendType.Unreliable);
					}

					ent_consumable.TriggerEvent(ref data);

					if (consumable.uses >= consumable.uses_max)
					{
						ent_consumable.Delete();
					}
				}
			}
		}
#endif
	}
}