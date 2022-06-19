
namespace TC2.Base.Components
{
	public static partial class Door
	{
		[IComponent.Data(Net.SendType.Reliable)]
		public partial struct Data: IComponent
		{
			public static readonly Sound.Handle default_sound_lock = "door_lock";
			public static readonly Sound.Handle default_sound_unlock = "door_unlock";
			public static readonly Sound.Handle default_sound_locked = "door_locked";
			public static readonly Sound.Handle default_sound_stuck = "door.stuck.00";

			public Sound.Handle sound_open = default;
			public Sound.Handle sound_close = default;

			public Sound.Handle sound_lock = default_sound_lock;
			public Sound.Handle sound_unlock = default_sound_unlock;
			public Sound.Handle sound_locked = default_sound_locked;
			public Sound.Handle sound_stuck = default_sound_stuck;

			public Vector2 size_open = default;
			public Vector2 size_closed = default;

			public Vector2 offset_open = default;
			public Vector2 offset_closed = default;

			public Door.Direction direction = default;
			public Door.Flags flags = default;

			public Data()
			{

			}
		}

		public enum Direction: uint
		{
			Horizontal,
			Vertical
		}

		[Flags]
		public enum Flags: uint
		{
			None = 0,

			Open = 1 << 0,
			Lockable = 1 << 1,
			Locked = 1 << 2,
			Bidirectional = 1 << 3
		}

#if SERVER
		[ISystem.Event<Interactable.InteractEvent>(ISystem.Mode.Single)]
		public static void OnInteract(ISystem.Info info, Entity entity, [Source.Owned] ref Interactable.InteractEvent data, 
		[Source.Owned] in Transform.Data transform, [Source.Owned] ref Animated.Renderer.Data renderer, [Source.Owned] ref Door.Data door, [Source.Owned] ref Interactable.Data interactable, 
		[Source.Owned] ref Body.Data body, [Source.Owned, Pair.Of<Body.Data>] ref Shape.Box shape, [Source.Shared, Optional] in Faction.Data faction)
		{
			ref var region = ref info.GetRegion();
			var random = XorRandom.New();

			var is_same_faction = faction.id == 0 || (data.faction_id == faction.id);

			if (door.flags.HasAll(Door.Flags.Lockable) && data.control.keyboard.GetKey(Keyboard.Key.LeftShift))
			{
				if (!door.flags.HasAll(Door.Flags.Open) && is_same_faction)
				{
					door.flags ^= Door.Flags.Locked;

					if (door.flags.HasAll(Door.Flags.Locked))
					{
						Sound.Play(ref region, door.sound_lock, transform.position, volume: 0.70f, pitch: random.NextFloatRange(0.95f, 1.05f), priority: 0.70f);
						WorldNotification.Push(ref region, "* Locks *", 0xffffda00, transform.position);
					}
					else
					{
						Sound.Play(ref region, door.sound_unlock, transform.position, volume: 0.70f, pitch: random.NextFloatRange(0.95f, 1.05f), priority: 0.70f);
						WorldNotification.Push(ref region, "* Unlocks *", 0xffffda00, transform.position);
					}

					door.Sync(entity);
				}
			}
			else
			{
				if (door.flags.HasAll(Door.Flags.Lockable | Door.Flags.Locked) && !is_same_faction)
				{
					Sound.Play(ref region, door.sound_locked, transform.position, volume: 0.70f, pitch: random.NextFloatRange(0.95f, 1.05f), priority: 0.40f);
					WorldNotification.Push(ref region, "* Locked *", 0xffff0000, transform.position);
				}
				else
				{
					var stuck = false;

					if (door.flags.HasAll(Door.Flags.Open))
					{
						var mask = shape.GetCombinedMask(); 
						mask.SetFlag(Physics.Layer.Solid, true);

						Span<ShapeOverlapResult> results = stackalloc ShapeOverlapResult[4];
						if (region.TryOverlapShapeAll(ref shape, ref results, mask: mask, exclude: Physics.Layer.World | Physics.Layer.Building | Physics.Layer.Door | Physics.Layer.Static))
						{
							WorldNotification.Push(ref region, "* DOOR STUCK! *", 0xffff0000, transform.position);
							Sound.Play(ref region, door.sound_stuck, transform.position, volume: 1.00f, pitch: random.NextFloatRange(0.95f, 1.05f), priority: 0.40f);
							Shake.Emit(ref region, transform.position, 0.30f, 0.30f, 6.00f);

							foreach (ref var result in results)
							{
								ref var hit_body = ref result.entity.GetComponent<Body.Data>();
								if (!hit_body.IsNull())
								{
									var dir = door.offset_open.GetNormalized();
									var force = Physics.LimitForce(ref hit_body, dir * -10000.00f, new Vector2(4, 4));

									hit_body.AddForceWorld(force, transform.position);
								}
							}

							stuck = true;
						}
					}

					if (!stuck)
					{
						door.flags ^= Door.Flags.Open;

						if (door.flags.HasAll(Door.Flags.Open))
						{
							shape.mask.SetFlag(Physics.Layer.Solid, false);
							shape.layer.SetFlag(Physics.Layer.Solid, false);

							var delta = (data.control.mouse.position - transform.position);
							var sign = 1.00f;

							if (door.flags.HasAll(Door.Flags.Bidirectional))
							{
								sign = (float)((door.direction == Direction.Horizontal ? delta.X : delta.Y) < 0.00f ? -1.00f : 1.00f);
							}

							var scale = door.direction == Direction.Horizontal ? new Vector2(sign, 1.00f) : new Vector2(1.00f, sign);

							shape.size = door.size_open;
							shape.offset = door.offset_open * scale;

							renderer.scale = scale;
							renderer.sprite.frame.X = 4;
							renderer.z = -100;

							Sound.Play(ref region, door.sound_open, transform.position, volume: 1.00f, pitch: random.NextFloatRange(0.95f, 1.05f), priority: 0.40f);
						}
						else
						{
							shape.mask.SetFlag(Physics.Layer.Solid, true);
							shape.layer.SetFlag(Physics.Layer.Solid, true);

							shape.size = door.size_closed;
							shape.offset = door.offset_closed;

							renderer.sprite.frame.X = 0;
							renderer.z = 100.00f;

							Sound.Play(ref region, door.sound_close, transform.position, volume: 1.00f, pitch: random.NextFloatRange(0.95f, 1.05f), priority: 0.40f);
						}

						body.Rebuild();

						door.Sync(entity);
						renderer.Sync(entity);
						shape.Sync<Shape.Box, Body.Data>(entity);
					}
				}
			}
		}
#endif
	}
}
