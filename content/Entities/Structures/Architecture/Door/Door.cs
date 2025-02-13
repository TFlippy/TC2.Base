
namespace TC2.Base.Components
{
	public static partial class Door
	{
		[IComponent.Data(Net.SendType.Reliable, IComponent.Scope.Region)]
		public partial struct Data(): IComponent
		{
			public static readonly Sound.Handle default_sound_lock = "door_lock";
			public static readonly Sound.Handle default_sound_unlock = "door_unlock";
			public static readonly Sound.Handle default_sound_locked = "door_locked";
			public static readonly Sound.Handle default_sound_stuck = "door.stuck.00";

			public Sound.Handle sound_open;
			public Sound.Handle sound_close;

			public Sound.Handle sound_lock = Door.Data.default_sound_lock;
			public Sound.Handle sound_unlock = Door.Data.default_sound_unlock;
			public Sound.Handle sound_locked = Door.Data.default_sound_locked;
			public Sound.Handle sound_stuck = Door.Data.default_sound_stuck;

			public Vector2 size_open;
			public Vector2 size_closed;

			public Vector2 offset_open;
			public Vector2 offset_closed;

			public Door.Direction direction;
			public Door.Flags flags;

			public uint frame_closed = 0;
			public uint frame_open = 4;

			public float fps_close = 10.00f;
			public float fps_open = 10.00f;

			[Asset.Ignore] public float animation_progress;
			[Asset.Ignore, Net.Ignore, Save.Ignore] public float last_use_time;
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
		[ISystem.Event<Map.ImportEvent>(ISystem.Mode.Single, ISystem.Scope.Region)]
		public static void OnImport(ISystem.Info info, Entity entity, ref Region.Data region, [Source.Owned] ref Map.ImportEvent data,
		[Source.Owned] ref Door.Data door)
		{
			//App.WriteLine($"import door {entity}");
			ref var entry = ref data.entry;

			door.flags.AddFlag(Door.Flags.Locked, door.flags.HasAny(Door.Flags.Lockable) && entry.faction != null);
			//door.Sync(entity, true);
		}
#endif

		[ISystem.LateUpdate(ISystem.Mode.Single, ISystem.Scope.Region)]
		public static void UpdateAnimation(ISystem.Info info, Entity entity,
		[Source.Owned] in Transform.Data transform, [Source.Owned] ref Animated.Renderer.Data renderer, [Source.Owned] ref Door.Data door)
		{
			if (door.flags.HasAny(Door.Flags.Open))
			{
				if (door.animation_progress <= 1.00f)
				{
					door.animation_progress = Maths.MoveTowards(door.animation_progress, 1.00f, App.fixed_update_interval_s * door.fps_close);
					renderer.sprite.frame.x = (uint)Maths.Lerp(door.frame_closed, door.frame_open, door.animation_progress);
				}
			}
			else
			{
				if (door.animation_progress >= 0.00f)
				{
					door.animation_progress = Maths.MoveTowards(door.animation_progress, 0.00f, App.fixed_update_interval_s * door.fps_close);
					renderer.sprite.frame.x = (uint)Maths.Lerp(door.frame_closed, door.frame_open, door.animation_progress);
				}
			}
		}

#if SERVER
		[ISystem.Event<Interactable.InteractEvent>(ISystem.Mode.Single, ISystem.Scope.Region)]
		public static void OnInteract(ISystem.Info info, Entity entity, ref XorRandom random, ref Region.Data region, [Source.Owned] ref Interactable.InteractEvent data, 
		[Source.Owned] in Transform.Data transform, [Source.Owned] ref Animated.Renderer.Data renderer, [Source.Owned] ref Door.Data door, [Source.Owned] ref Interactable.Data interactable, 
		[Source.Owned] ref Body.Data body, [Source.Owned, Pair.Component<Body.Data>] ref Shape.Box shape, [Source.Owned, Optional] in Faction.Data faction)
		{
			var is_same_faction = faction.id == 0 || (data.faction_id == faction.id);

			if (door.flags.HasAny(Door.Flags.Lockable) && data.control.keyboard.GetKey(Keyboard.Key.LeftShift))
			{
				if (door.flags.HasNone(Door.Flags.Open) && is_same_faction)
				{
					door.flags ^= Door.Flags.Locked;

					if (door.flags.HasAny(Door.Flags.Locked))
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
				if (door.flags.HasAll(Door.Flags.Lockable | Door.Flags.Locked) && (!is_same_faction || faction.id == 0))
				{
					Sound.Play(ref region, door.sound_locked, transform.position, volume: 0.70f, pitch: random.NextFloatRange(0.95f, 1.05f), priority: 0.40f);
					WorldNotification.Push(ref region, "* Locked *", 0xffff0000, transform.position);
				}
				else
				{
					var stuck = false;
					door.last_use_time = info.WorldTime;

					if (door.flags.HasAny(Door.Flags.Open))
					{
						var mask = shape.GetCombinedMask(); 
						mask.SetFlag(Physics.Layer.Solid, true);

						Span<ShapeOverlapResult> results = stackalloc ShapeOverlapResult[8];
						if (region.TryOverlapShapeAll(ref shape, ref results, mask: mask, require: Physics.Layer.Dynamic, exclude: Physics.Layer.World | Physics.Layer.Building | Physics.Layer.Door | Physics.Layer.Gas | Physics.Layer.Essence | Physics.Layer.Fire | Physics.Layer.Water | Physics.Layer.Static))
						{
							WorldNotification.Push(ref region, "* DOOR STUCK! *", 0xffff0000, transform.position);
							Sound.Play(ref region, door.sound_stuck, transform.position, volume: 1.00f, pitch: random.NextFloatRange(0.95f, 1.05f), priority: 0.40f);
							Shake.Emit(ref region, transform.position, 0.30f, 0.30f, 6.00f);

							foreach (ref var result in results)
							{
								ref var hit_body = ref result.GetBody();
								if (!hit_body.IsNull())
								{
									var dir = door.offset_open.GetNormalized();
									dir.Y = 1.000f;
									
									var force = Physics.LimitForce(ref hit_body, dir * -15000.00f, new Vector2(6, 6));

									hit_body.AddForceWorld(force, transform.position);
								}
							}

							stuck = true;
						}
					}

					if (!stuck)
					{
						door.flags ^= Door.Flags.Open;

						if (door.flags.HasAny(Door.Flags.Open))
						{
							shape.mask.RemoveFlag(Physics.Layer.Solid);
							shape.layer.RemoveFlag(Physics.Layer.Solid);

							var delta = (data.control.mouse.position - transform.position);
							var sign = 1.00f;

							if (door.flags.HasAny(Door.Flags.Bidirectional))
							{
								sign = (float)((door.direction == Direction.Horizontal ? delta.X : delta.Y) < Maths.epsilon ? -1.00f : 1.00f);
							}

							var scale = door.direction == Direction.Horizontal ? new Vector2(sign, 1.00f) : new Vector2(1.00f, sign);

							shape.size = door.size_open;
							shape.offset = door.offset_open * scale;

							renderer.scale = scale;
							//renderer.sprite.frame.X = 4;
							renderer.z = -200;

							Sound.Play(ref region, door.sound_open, transform.position, volume: 1.00f, pitch: random.NextFloatRange(0.95f, 1.05f), priority: 0.40f);
						}
						else
						{
							shape.mask.AddFlag(Physics.Layer.Solid);
							shape.layer.AddFlag(Physics.Layer.Solid);

							shape.size = door.size_closed;
							shape.offset = door.offset_closed;

							//renderer.sprite.frame.X = 0;
							renderer.z = 100.00f;

							Sound.Play(ref region, door.sound_close, transform.position, volume: 1.00f, pitch: random.NextFloatRange(0.95f, 1.05f), priority: 0.40f);
						}

						body.MarkDirty();

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
