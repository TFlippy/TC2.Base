
namespace TC2.Base.Components
{
	public static partial class Climber_DEV
	{
		public static Sound.Handle[] snd_walljump =
		{
			"walljump.00",
			"walljump.01",
			"walljump.02"
		};

		// Crappily exposed Climber.cs for now, since it interacts with physics constraint pointers
		[ISystem.VeryLateUpdate(ISystem.Mode.Single)]
		public static void OnUpdate(ISystem.Info info, Entity entity,
		[Source.Owned] in Transform.Data transform, [Source.Owned, Pair.Of<Climber.Data>] ref Shape.Circle shape,
		[Source.Owned, Override] in Organic.Data organic, [Source.Owned] in Organic.State organic_state,
		[Source.Owned] ref Climber.Data climber, [Source.Owned] in Health.Data health, [Source.Owned] ref Body.Data body, [Source.Owned] ref Control.Data control)
		{
			//var ts = Timestamp.Now();

			ref var region = ref info.GetRegion();
			ref var keyboard = ref control.keyboard;

			climber.MaxForce = 0.00f;
			climber.OffsetA = body.GetWorldCenterOfGravity() + (body.GetVelocity() * App.fixed_update_interval_s);
			climber.cling_entity = default;

			var is_climbing = false;
			var can_move = !keyboard.GetKey(Keyboard.Key.NoMove);
			var can_wallclimb = can_move && keyboard.GetKey(Keyboard.Key.MoveLeft | Keyboard.Key.MoveRight);

			var force = new Vector2(0, float.Epsilon);
			var normal = new Vector2(0, 0);

			if (can_move && body.HasArbiters())
			{
				var climb_force = 0.00f;

				foreach (var arbiter in body.GetArbiters())
				{
					var layer = arbiter.GetLayer();
					if (!layer.HasAny(Physics.Layer.Bounds))
					{
						if (!is_climbing && layer.HasAny(Physics.Layer.Climbable) && !layer.HasAny(Physics.Layer.Tree))
						{
							var ent_arbiter = arbiter.GetEntity();

							var cling_force = climber.cling_force;
							var climb_speed = climber.climb_speed;
							cling_force *= Maths.Cutoff(organic_state.efficiency * organic_state.consciousness_shared, 0.30f, 0.00f) * organic.strength;
							climber.cling_entity = ent_arbiter;
							climber.pos_climbable = arbiter.GetPosition();

							var vel = Vector2.Zero;

							if (keyboard.GetKey(Keyboard.Key.MoveLeft)) vel.X -= 1.50f;
							if (keyboard.GetKey(Keyboard.Key.MoveRight)) vel.X += 1.50f;
							if (keyboard.GetKey(Keyboard.Key.MoveUp)) vel.Y -= 1.00f;
							if (keyboard.GetKey(Keyboard.Key.MoveDown)) vel.Y += 1.50f;

							vel *= climb_speed;

							climber.MaxForce = cling_force;
							climber.OffsetA += (vel * App.fixed_update_interval_s);

							if (vel != Vector2.Zero) body.Activate();

							is_climbing = true;
							//break;
						}
						else if (can_wallclimb && arbiter.GetRigidityDynamic() > 0.90f)
						{
							normal += arbiter.GetNormal();
							//if (normal.Y >= -0.40f)
							{
								var f = MathF.Abs(normal.X) * arbiter.GetFriction() * climber.climb_force;
								climb_force = MathF.Max(climb_force, f);
							}
						}
						else if (!is_climbing && layer.HasAny(Physics.Layer.Water))
						{
							var ent_arbiter = arbiter.GetEntity();

							var swim_force_modifier = climber.climb_force * 0.40f;

							var vel = Vector2.Zero;

							if (keyboard.GetKey(Keyboard.Key.MoveLeft)) vel.X -= 1.20f;
							if (keyboard.GetKey(Keyboard.Key.MoveRight)) vel.X += 1.20f;
							if (keyboard.GetKey(Keyboard.Key.MoveUp)) vel.Y -= 0.50f;
							if (keyboard.GetKey(Keyboard.Key.MoveDown)) vel.Y += 8.00f;

							vel *= 0.10f;

							if (vel.LengthSquared() > 0.01f) body.Activate();

							var swim_force = vel * swim_force_modifier;
							swim_force = Physics.LimitForce(ref body, swim_force, new Vector2(5, 5));

							body.AddForce(swim_force);
						}
					}
				}

				if (can_wallclimb && climb_force > 0.00f)
				{
					climb_force *= climber.climb_force;
					climb_force *= organic_state.efficiency * organic.strength;

					var max_speed = new Vector2(10, 10);

					normal = normal.GetNormalized(out var normal_len);
					if (normal_len > 0.01f && info.WorldTime >= climber.last_walljump + 0.20f)
					{
						var do_walljump = keyboard.GetKeyDown(float.IsPositive(normal.X) ? Keyboard.Key.MoveLeft : Keyboard.Key.MoveRight);
						if (do_walljump)
						{
							//App.WriteLine(do_walljump);

							force.X *= normal.X * 50.00f;
							climb_force *= 50.00f;
							max_speed.X *= 2.00f;
							max_speed.Y *= 2.50f;

							climber.last_walljump = info.WorldTime;

#if CLIENT
							var random = XorRandom.New();
							Sound.Play(snd_walljump.GetRandom(ref random), transform.position, volume: random.NextFloatRange(0.22f, 0.25f), pitch: random.NextFloatRange(0.85f, 0.95f));
#endif
						}
					}

					force.Y += -climb_force;
					force = Physics.LimitForce(ref body, force, max_speed);

					body.AddForce(force);

					climber.last_wallclimb = info.WorldTime;
					climber.last_force = force;
				}
			}

			//App.WriteLine($"climber in {ts.GetMilliseconds():0.0000} ms");
		}
	}

	public static partial class Runner
	{
		[Flags]
		public enum Flags: uint
		{
			None = 0,

			Grounded = 1 << 0,
			Walking = 1 << 1,
			Jumping = 1 << 2,
			Falling = 1 << 3,
			Crouching = 1 << 4,
			Climbing = 1 << 5,
			Sitting = 1 << 6,
			WallClimbing = 1 << 7,
		}

		[IComponent.Data(Net.SendType.Unreliable)]
		public partial struct Data: IComponent, IOverridable
		{
			public float walk_force = default;
			public float jump_force = default;
			public float max_speed = 10.00f;
			public float max_jump_speed = 10.00f;

			public float walk_lerp = 0.15f;
			public float jump_decay = 0.50f;
			public float crouch_speed_modifier = 0.50f;

			public Data()
			{

			}
		}

		[IComponent.Data(Net.SendType.Unreliable)]
		public partial struct State: IComponent
		{
			[Save.Ignore] public float air_modifier_current = default;
			[Save.Ignore] public float walk_modifier_current = default;
			[Save.Ignore] public float uphill_force_current = default;
			[Save.Ignore] public float jump_force_current = default;
			[Save.Ignore] public Runner.Flags flags = default;

			[Save.Ignore, Net.Ignore] public float air_time = default;

			[Save.Ignore, Net.Ignore] public float last_jump = default;
			[Save.Ignore, Net.Ignore] public float last_ground = default;
			[Save.Ignore, Net.Ignore] public float last_climb = default;
			[Save.Ignore, Net.Ignore] public float last_air = default;

			[Save.Ignore, Net.Ignore] public Vector2 last_force = default;
			[Save.Ignore, Net.Ignore] public Vector2 last_wallclimb_force = default;

			public State()
			{

			}
		}

		[ISystem.AddFirst(ISystem.Mode.Single), HasRelation(Source.Modifier.Any, Relation.Type.Seat, true)]
		public static void OnSit(ISystem.Info info, Entity entity, [Source.Owned] ref Runner.State runner_state)
		{
			runner_state.flags.SetFlag(Runner.Flags.Sitting, true);
		}

		[ISystem.RemoveLast(ISystem.Mode.Single), HasRelation(Source.Modifier.Any, Relation.Type.Seat, true)]
		public static void OnUnSit(ISystem.Info info, Entity entity, [Source.Owned] ref Runner.State runner_state)
		{
			runner_state.flags.SetFlag(Runner.Flags.Sitting, false);
		}

		[ISystem.EarlyUpdate(ISystem.Mode.Single)]
		public static void UpdateOrganic(ISystem.Info info, [Source.Owned, Override] ref Runner.Data runner, [Source.Owned, Override] in Organic.Data organic, [Source.Owned] in Organic.State organic_state)
		{
			//App.WriteLine($"{organic.dexterity}");

			runner.walk_force *= Maths.Clamp(organic.strength, 0.50f, 1.80f) * organic.coordination * organic.motorics;
			runner.jump_force *= Maths.Clamp(MathF.Min(organic.strength, organic.dexterity), 0.75f, 1.40f) * organic.coordination * organic.motorics;
			runner.max_speed *= organic_state.efficiency * organic.coordination;
			runner.max_jump_speed *= organic_state.efficiency * organic.consciousness;

			if (organic_state.efficiency < 0.50f)
			{
				runner.max_speed *= 0.50f;
				runner.max_jump_speed *= 0.20f;
			}
		}

		[ISystem.Update(ISystem.Mode.Single)]
		public static void UpdateClimbing(ISystem.Info info, [Source.Owned, Override] in Runner.Data runner, [Source.Owned] ref Runner.State runner_state, [Source.Parent] in Climber.Data climber)
		{
			runner_state.flags.SetFlag(Runner.Flags.Climbing, climber.cling_entity.IsValid());
			runner_state.flags.SetFlag(Runner.Flags.WallClimbing, (info.WorldTime - climber.last_wallclimb) < 0.10f);
			if (runner_state.flags.HasAll(Runner.Flags.Climbing))
			{
				runner_state.last_climb = info.WorldTime;
				runner_state.last_wallclimb_force = climber.last_force;
			}
		}

		[ISystem.LateUpdate(ISystem.Mode.Single)]
		public static void UpdateMovement(ISystem.Info info, [Source.Owned, Override] in Runner.Data runner, [Source.Owned] ref Runner.State runner_state, [Source.Owned] ref Body.Data body, [Source.Owned] in Control.Data control)
		{
			ref readonly var keyboard = ref control.keyboard;

			var force = new Vector2(0, 0);
			var max_speed = new Vector2(runner.max_speed, runner.max_jump_speed);
			var velocity = body.GetVelocity();
			var mass = body.GetMass();

			var can_move = !keyboard.GetKey(Keyboard.Key.NoMove) && !runner_state.flags.HasAny(Runner.Flags.Sitting);
			var is_walking = can_move && keyboard.GetKey(Keyboard.Key.MoveLeft | Keyboard.Key.MoveRight);
			var any = can_move && keyboard.GetKey(Keyboard.Key.MoveLeft | Keyboard.Key.MoveRight | Keyboard.Key.MoveUp | Keyboard.Key.MoveDown);

			if (any)
			{
				body.Activate();
			}

			runner_state.flags.SetFlag(Runner.Flags.Walking, is_walking);

			if (is_walking)
			{
				runner_state.walk_modifier_current = Maths.Lerp(runner_state.walk_modifier_current, 1.00f, runner.walk_lerp);

				if (velocity.X > -runner.max_speed && keyboard.GetKey(Keyboard.Key.MoveLeft)) force.X -= runner.walk_force * runner_state.walk_modifier_current;
				if (velocity.X < +runner.max_speed && keyboard.GetKey(Keyboard.Key.MoveRight)) force.X += runner.walk_force * runner_state.walk_modifier_current;
			}
			else
			{
				runner_state.walk_modifier_current = 0.00f;
			}

			var normal = default(Vector2);
			var layers = default(Physics.Layer);
			var friction = 0.00f;

			var arbiter_count = 0;
			foreach (var arbiter in body.GetArbiters())
			{
				if (arbiter.GetRigidityDynamic() > 0.90f)
				{
					normal += arbiter.GetNormal();
					friction += arbiter.GetFriction();
					layers |= arbiter.GetLayer();

					arbiter_count++;
				}
			}

			if (arbiter_count > 0)
			{
				normal /= arbiter_count;
				friction /= arbiter_count;
			}

			normal = normal.GetNormalized();

			var is_grounded = false;

			if (arbiter_count > 0) // && !layers.HasAll(Physics.Layer.Bounds))
			{
				is_grounded = MathF.Abs(normal.X) < 0.95f;

				var dot = MathF.Abs(Vector2.Dot(normal, new Vector2(MathF.Sign(normal.X), 0)));

				//runner_state.uphill_force_current = -MathF.Abs(dot * force.Length()) * friction * 0.50f;
				runner_state.uphill_force_current = -MathF.Abs(dot * force.X);

				force.Y = runner_state.uphill_force_current;
				force.X *= (1.00f - dot);
			}
			else
			{
				force.X *= (runner_state.flags.HasAny(Runner.Flags.WallClimbing)) ? 0.50f : 0.75f;
				force.Y *= 0.00f;

				force.Y -= runner_state.uphill_force_current * 0.75f;
				runner_state.uphill_force_current *= 0.50f;
			}

			if (is_grounded)
			{
				runner_state.flags.SetFlag(Runner.Flags.Grounded, true);
				runner_state.last_ground = info.WorldTime;
			}
			else
			{
				runner_state.flags.SetFlag(Runner.Flags.Grounded, false);
				runner_state.last_air = info.WorldTime;
			}

			if (can_move && keyboard.GetKey(Keyboard.Key.MoveUp) && (info.WorldTime - runner_state.last_jump) > 0.40f && (((info.WorldTime - runner_state.last_ground) < 0.20f) || (((info.WorldTime - runner_state.last_climb) > 0.00f) && (info.WorldTime - runner_state.last_climb) < 0.20f)))
			{
				runner_state.jump_force_current = runner.jump_force;
				runner_state.last_jump = info.WorldTime;
			}

			force.Y -= runner_state.jump_force_current;
			runner_state.jump_force_current *= runner.jump_decay;

			runner_state.flags.SetFlag(Runner.Flags.Crouching, can_move && control.keyboard.GetKey(Keyboard.Key.MoveDown) && (info.WorldTime - runner_state.last_ground) <= 0.25f);
			if (runner_state.flags.HasAll(Runner.Flags.Crouching))
			{
				runner_state.jump_force_current *= 0.50f;
				max_speed.X *= runner.crouch_speed_modifier;
			}

			if (!is_walking)
			{
				var required_force_dir = (velocity * mass * App.tickrate) - force;
				required_force_dir = required_force_dir.GetNormalized(out var required_force_magnitude);
				required_force_dir *= Maths.Clamp(runner.walk_force * 0.50f, -required_force_magnitude, required_force_magnitude);

				if (!runner_state.flags.HasAll(Runner.Flags.Grounded))
				{
					required_force_dir.X *= 0.02f;
					required_force_dir.Y *= 0.00f;
				}

				force -= required_force_dir;
			}

			runner_state.air_time = info.WorldTime - MathF.Max(runner_state.last_climb, runner_state.last_ground);
			runner_state.air_modifier_current = Maths.Lerp(runner_state.air_modifier_current, 1.00f - Maths.Clamp(runner_state.air_time - 0.75f, 0.00f, 1.00f), 0.10f);

			//max_speed *= runner_state.speed_modifier;
			//force *= runner_state.force_modifier;
			force *= runner_state.air_modifier_current;

			force = Physics.LimitForce(ref body, force, max_speed);

			runner_state.last_force = force;
			body.AddForce(force);
		}
	}
}
